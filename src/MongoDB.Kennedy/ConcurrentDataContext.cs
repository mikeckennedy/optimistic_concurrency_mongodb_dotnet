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
		/// The name of the collection will be the name of the type.
		/// The save mode is ConcurrentSaveOptions.ProtectServerChanges.
		/// </summary>
		/// <typeparam name="T">Type of entity</typeparam>
		/// <param name="entity">The entity to be saved</param>
		public override void Save<T>(T entity)
		{
			Save(entity, GetCollectionName<T>(), ConcurrentSaveOptions.ProtectServerChanges);
		}

		/// <summary>
		/// Save changes to entity back to MongoDB with concurrency protection 
		/// specified by saveOptions. If you want to *overwrite* changes made 
		/// on the server use ConcurrentSaveOptions.OverwriteServerChanges.
		/// The save mode is ConcurrentSaveOptions.ProtectServerChanges.
		/// </summary>
		/// <typeparam name="T">Type of entity</typeparam>
		/// <param name="entity">The entity to be saved</param>
		/// <param name="collectionName">The name of the collection in the MongoDB database</param>
		public override void Save<T>(T entity, string collectionName)
		{
			this.Save(entity, collectionName, ConcurrentSaveOptions.ProtectServerChanges);
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
		public void Save<T>(T entity, ConcurrentSaveOptions saveOptions) where T: class
		{
			this.Save(entity, GetCollectionName<T>(), saveOptions);
		}		
		
		public void Save<T>(T entity, string connectionName, ConcurrentSaveOptions saveOptions) where T : class
		{
			if (entity == null)
				throw new ArgumentNullException("entity");

			IMongoEntity mongoEntity = entity as IMongoEntity;
			if (mongoEntity== null)
				throw new InvalidOperationException("Cannot save entity in ConcurrentDataContext. " + typeof(T).Name + " does not implemented IMongoEntity.");

			if (mongoEntity._id == ObjectId.Empty)
				InternalInsert(entity, connectionName);
			else
				InternalUpdate(entity, connectionName, saveOptions);
		}

		private void InternalInsert<T>(T entity, string name)// where T : IMongoEntity
		{
			IMongoEntity mongoEntity = (IMongoEntity)entity;

			var coll = Db.GetCollection<T>(name);
			mongoEntity._accessId = BuildAccessId();

			WriteConcernResult concern = coll.Insert(entity);
			if (!concern.Ok)
				throw new MongoQueryException("Cannot insert entity: " + concern.LastErrorMessage);
		}

		private void InternalUpdate<T>(T entity, string name, ConcurrentSaveOptions saveOptions)// where T : IMongoEntity
		{
			IMongoEntity mongoEntity = (IMongoEntity)entity;

			var coll = Db.GetCollection<T>(name);

			IMongoQuery find;
			if (saveOptions == ConcurrentSaveOptions.ProtectServerChanges)
				find = Query.And(Query.EQ("_id", mongoEntity._id), Query.EQ("_accessId", mongoEntity._accessId));
			else
				find = Query.EQ("_id", mongoEntity._id);

			string originalAccessId = mongoEntity._accessId;
			mongoEntity._accessId = BuildAccessId();

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

				bool isConcurrencyError = coll.Find(Query.EQ("_id", mongoEntity._id)).Any();//coll.AsQueryable().Any(e => e._id == mongoEntity._id);
				if (isConcurrencyError)
				{
					throw new MongoConcurrencyException("Entity modified by other writer since being retreived from db: id = " + mongoEntity._id);
				}
				else
				{
					throw new MongoException("Cannot update entity (no entity with ID " + mongoEntity._id + " exists in the db.");
				}
			}
			catch
			{
				mongoEntity._accessId = originalAccessId;
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
