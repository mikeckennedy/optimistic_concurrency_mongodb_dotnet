using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Kennedy.Tests.TestData;

namespace MongoDB.Kennedy.Tests
{
	[TestClass]
	public class MongoDbDataContextTests
	{
		[TestMethod]
		public void db_creation_success_test()
		{
			TestDbContext mongo = BuildTestDb();

			Assert.AreEqual(1, mongo.Owners.Count());
			Assert.AreEqual(2, mongo.Pets.Count());
		}

		[TestMethod]
		public void db_collection_clear_test()
		{
			TestDbContext mongo = BuildTestDb();

			mongo.Clear<Pet>();

			Assert.AreEqual(0, mongo.Pets.Count());
		}

		[TestMethod]
		public void db_delete_entity_no_interface_required_test()
		{
			TestDbContext mongo = BuildTestDb();

			Pet p = mongo.Pets.First();
			Pet p2 = mongo.Pets.Last();

			mongo.Delete<Pet>(p.Id);

			Assert.AreNotEqual(p.Id, p2.Id);

			Assert.AreEqual(1, mongo.Pets.Count());
		}

		[TestMethod]
		public void db_delete_entity_with_interface_required_test()
		{
			TestDbContext mongo = BuildTestDb();

			Owner o = mongo.Owners.First();

			mongo.Delete(o);

			Assert.AreEqual(0, mongo.Owners.Count());
		}

		[TestMethod]
		public void db_update_entity_without_interface_required_test()
		{
			TestDbContext mongo = BuildTestDb();

			Pet p = mongo.Pets.First();

			string name = p.Name;
			p.Name = "Tiger";

			mongo.Save(p);

			Pet p2 = mongo.Pets.Single(c => c.Id == p.Id);

			Assert.AreNotEqual(name, p.Name);
			Assert.AreEqual(p.Name, p2.Name);
		}

		[TestMethod]
		public void db_update_entity_with_interface_required_test()
		{
			TestDbContext mongo = BuildTestDb();

			Owner o = mongo.Owners.First();

			int count = o.NumberOfPets;
			o.NumberOfPets++;

			mongo.Save(o);

			Owner o2 = mongo.Owners.Single(ow => ow._id == o._id);

			Assert.AreNotEqual(count, o.NumberOfPets);
			Assert.AreEqual(o.NumberOfPets, o2.NumberOfPets);
		}

		private TestDbContext BuildTestDb()
		{
			var mongo = new TestDbContext("test_mongoctx");

			mongo.Clear<Pet>();
			mongo.Clear<Owner>();

			// Add some pets:
			var p = new Pet();
			p.Age = 5;
			p.Name = "Fluffy";
			p.Type = "Cat";

			mongo.Save(p);

			p = new Pet();
			p.Age = 4;
			p.Name = "Spot";
			p.Type = "Dog";

			mongo.Save(p);

			// Add some owners:
			var o = new Owner();
			o.Name = "Ralph";
			o.NumberOfPets = 50;

			mongo.Save(o);

			return mongo;
		}
	}
}