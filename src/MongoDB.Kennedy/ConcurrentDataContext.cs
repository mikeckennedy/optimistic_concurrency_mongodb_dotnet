using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;

namespace MongoDB.Kennedy
{
	public class ConcurrentDataContext : MongoDbDataContext
	{
		protected ConcurrentDataContext(string databaseName,
			string serverName = "localhost",
            int port = 27017,
			bool safeMode = true) : base(databaseName, serverName, port, safeMode)
		{
		}

		/// <summary>
		/// Save changes to entity back to MongoDB *with* concurrency protection.
		/// </summary>
		/// <typeparam name="T">Type of entity</typeparam>
		/// <param name="entity">The entity to be saved</param>
		public override void Save<T>(T entity)
		{
			Save(entity, ConcurrentSaveOptions.ProtectServerChanges);
		}

		/// <summary>
		/// Save changes to entity back to MongoDB with concurrency protection 
		/// specified by saveOptions. If you want to *overwrite* changes made 
		/// on the server use ConcurrentSaveOptions.OverwriteServerChanges.
		/// </summary>
		/// <typeparam name="T">Type of entity</typeparam>
		/// <param name="entity">The entity to be saved</param>
		/// <param name="saveOptions">
		/// Enables or disables concurrency projected, 
		/// ConcurrentSaveOptions.ProtectServerChanges is recommended.
		/// </param>
		public void Save<T>(T entity, ConcurrentSaveOptions saveOptions) where T : IMongoEntity
		{
			if (entity._id == ObjectId.Empty)
				InternalInsert(entity);
			else
				InternalUpdate(entity, saveOptions);
		}

		private void InternalInsert<T>(T entity) where T : IMongoEntity
		{
			var name = GetCollectionName<T>();
			var coll = Db.GetCollection<T>(name);
			entity._accessId = BuildAccessId();

			WriteConcernResult concern = coll.Insert(entity);
			if (!concern.Ok)
				throw new MongoQueryException("Cannot insert entity: " + concern.LastErrorMessage);
		}

		private void InternalUpdate<T>(T entity, ConcurrentSaveOptions saveOptions) where T : IMongoEntity
		{
			var name = GetCollectionName<T>();
			var coll = Db.GetCollection<T>(name);

			IMongoQuery find;
			if (saveOptions == ConcurrentSaveOptions.ProtectServerChanges)
				find = Query.And(Query.EQ("_id", entity._id), Query.EQ("_accessId", entity._accessId));
			else
				find = Query.EQ("_id", entity._id);

			string originalAccessId = entity._accessId;
			entity._accessId = BuildAccessId();

			try
			{
				var wrap = new BsonDocumentWrapper(entity);
				var update = new UpdateDocument(wrap.ToBsonDocument());
				WriteConcernResult res = coll.Update(find, update);

				if (res.DocumentsAffected == 1 && res.Ok)
				{
					// success
					return;
				}

				if (!res.Ok)
				{
					throw new MongoConcurrencyException(res.ErrorMessage);
				}
				
				// problem. is there just no document or is there a concurrency problem.
				// let's do a little work to no be overly agressive on the errors.
				bool isConcurrencyError = coll.AsQueryable().Any(e => e._id == entity._id);
				if (isConcurrencyError)
				{
					throw new MongoConcurrencyException("Entity modified by other writer since being retreived from db: id = " + entity._id);
				}
				else
				{
					throw new MongoException("Cannot update entity (no entity with ID " + entity._id + " exists in the db.");
				}
			}
			catch
			{
				entity._accessId = originalAccessId;
				throw;
			}
		}

		public override void Delete<T>(T entity)
		{
			var name = GetCollectionName<T>();

			var find = Query.And(
				Query.EQ("_id", entity._id),
				Query.EQ("_accessId", entity._accessId));

			string originalAccessId = entity._accessId;
			entity._accessId = BuildAccessId();
			try
			{
				var coll = Db.GetCollection<T>(name);

				WriteConcernResult res = coll.Remove(find);
				if (res.DocumentsAffected == 1 && res.Ok)
				{
					// success
					return;
				}

				if (!res.Ok)
				{
					throw new MongoQueryException(res.ErrorMessage);
				}
				
				// problem. is there just no document or is there a concurrency problem.
				// let's do a little work to no be overly agressive on the errors.
				bool isTrueError = coll.AsQueryable().Any(e => e._id == entity._id);
				if (isTrueError)
				{
					throw new MongoCommandException(
						"The following entity has been modified by another request between the time you requested and then deleted it: " +
						entity._id);
				}
			}
			catch
			{
				entity._accessId = originalAccessId;
				throw;
			}
		}

		private static string BuildAccessId()
		{
			return Guid.NewGuid().ToString()
				.Replace("-", "")
				.Replace("{", "")
				.Replace("}", "")
				.Substring(0, 6);
		}
	}
}
