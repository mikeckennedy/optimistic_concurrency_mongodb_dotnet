using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;

namespace MongoDB.Kennedy
{
	public abstract class MongoDbDataContext
	{
		private readonly MongoDatabase _db;

		protected MongoDbDataContext(string databaseName,
			string serverName = "localhost",
			int port = 27017,
			bool safeMode = true)
		{
			DatabaseName = databaseName;
			if (string.IsNullOrWhiteSpace(DatabaseName))
			{
				throw new InvalidOperationException("You must set the database name.");
			}

			if (serverName == null)
				throw new ArgumentNullException("serverName");

			string connStr = string.Format(
				"mongodb://{0}:{1}/{2}",
				serverName,
				port,
				safeMode ? "?safe=true" : "");

			Client = new MongoClient(connStr);
			Server = Client.GetServer();
			_db = Server.GetDatabase(DatabaseName);
		}

		public MongoClient Client { get; private set; }
		public MongoServer Server { get; private set; }

		public string DatabaseName { get; private set; }

		public MongoDatabase Db
		{
			get { return _db; }
		}
		
		protected MongoCollection<T> GetMongoCollection<T>()
		{
			return Db.GetCollection<T>(GetCollectionName<T>());
		}

		protected IQueryable<T> GetCollection<T>()
		{
			return this.GetMongoCollection<T>().AsQueryable();
		}

		public virtual void Delete<T>(ObjectId entityId)
		{
			string name = GetCollectionName<T>();
			Delete(entityId, name);
		}

		public virtual void Delete(ObjectId entityId, string collectionName)
		{
			IMongoQuery query = Query.EQ("_id", entityId);
			Db.GetCollection<object>(collectionName).Remove(query);
		}

		public virtual void Delete<T>(T entity) where T : IMongoEntity
		{
			string name = GetCollectionName<T>();
			Delete(entity, name);
		}

		public virtual void Delete<T>(T entity, string collectionName) where T : IMongoEntity
		{
			this.Delete(entity._id, collectionName);
		}
		
		public virtual void Save<T>(T entity) where T : class
		{
			string name = GetCollectionName<T>();
			Save(entity, name);
		}

		public virtual void Save<T>(T entity, string collectionName) where T : class
		{
			MongoCollection<T> collection = Db.GetCollection<T>(collectionName);
			WriteConcernResult writeConcern = collection.Save(entity);
			if (!writeConcern.Ok)
				throw new MongoContextException(writeConcern.ErrorMessage);
		}

		protected static string GetCollectionName<T>()
		{
			return typeof(T).Name;
		}

		public void Clear<T>()
		{
			string collectionName = GetCollectionName<T>();
			MongoCollection<T> collection = Db.GetCollection<T>(collectionName);
			CommandResult result = collection.RemoveAll();
			if (!result.Ok)
				throw new MongoContextException(result.ErrorMessage);
		}
	}
}