using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;

namespace MongoDB.Kennedy
{
	public abstract class MongoDbDataContext
	{
		private readonly MongoDatabase _db;
		public MongoClient Client { get; set; }
		public MongoServer Server { get; set; }

		public string DatabaseName { get; set; }

		protected MongoDbDataContext(string databaseName,
			string serverName = "localhost", 
            int port = 27017,
			bool safeMode = true)
		{
			this.DatabaseName = databaseName;
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

		public MongoDatabase Db
		{
			get { return _db; }
		}

		
		protected IQueryable<T> GetCollection<T>()
		{
			{
				return Db.GetCollection<T>(GetCollectionName<T>()).AsQueryable<T>();
			}
		}

		public virtual void Delete<T>(T entity) where T : IMongoEntity
		{
			var name = GetCollectionName<T>();
			Delete(entity, name);
		}

		public virtual void Delete<T>(T entity, string collectionName) where T : IMongoEntity
		{
			var query = Query.EQ("_id", entity._id);
			Db.GetCollection<T>(collectionName).Remove(query);
		}

		public virtual void Save<T>(T entity) where T : IMongoEntity
        {
            var name = GetCollectionName<T>();
			Save(entity, name);
        }

		public virtual void Save<T>(T entity, string collectionName) where T : IMongoEntity
		{
			Db.GetCollection<T>(collectionName).Save(entity);
		}

		protected static string GetCollectionName<T>()
		{
			return typeof(T).Name;
		}

		public void Clear<T>()
		{
			var collectionName = GetCollectionName<T>();
			var result = Db.DropCollection(collectionName);
			if (!result.Ok)
				throw new ApplicationException(result.ErrorMessage);

		}
	}
}