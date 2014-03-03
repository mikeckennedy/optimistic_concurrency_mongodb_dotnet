using System;
using System.Linq;
using MongoDB.Driver;

namespace MongoDB.Kennedy.Tests.TestData
{
	internal class TestDbContext : MongoDbDataContext
	{
		public TestDbContext(string databaseName, string serverName = "localhost", int port = 27017, bool safeMode = true) :
			base(databaseName, serverName, port, safeMode)
		{
		}

		public MongoCollection<Pet> PetsCollection
		{
			get { return GetMongoCollection<Pet>(); }
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