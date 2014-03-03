using System;
using System.Linq;
using MongoDB.Driver;

namespace MongoDB.Kennedy.Tests.TestData
{
	internal class TestConcurrentDbContext : ConcurrentDataContext
	{
		public TestConcurrentDbContext(string databaseName, string serverName = "localhost", int port = 27017,
			bool safeMode = true) :
				base(databaseName, serverName, port, safeMode)
		{
		}

		public MongoCollection<Lion> LionCollection
		{
			get { return GetMongoCollection<Lion>(); }
		}

		public IQueryable<Lion> Lions
		{
			get { return GetCollection<Lion>(); }
		}

		public IQueryable<Pet> Pets
		{
			get { return GetCollection<Pet>(); }
		}

		public MongoCollection<Owner> OwnersCollection
		{
			get { return GetMongoCollection<Owner>(); }
		}

		public IQueryable<Owner> Owners
		{
			get { return GetCollection<Owner>(); }
		}
	}
}