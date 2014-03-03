using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Kennedy.Tests.TestData;

namespace MongoDB.Kennedy.Tests
{
	[TestClass]
	public class ConcurrentDataContextTests
	{
		[TestMethod]
		public void db_creation_success_test()
		{
			TestConcurrentDbContext mongo = BuildTestDb();

			Assert.AreEqual(1, mongo.Owners.Count());
			Assert.AreEqual(2, mongo.Lions.Count());
		}

		[TestMethod]
		public void db_collection_clear_test()
		{
			TestConcurrentDbContext mongo = BuildTestDb();

			mongo.Clear<Lion>();

			Assert.AreEqual(0, mongo.Lions.Count());
		}


		[ExpectedException(typeof (MongoConcurrencyException))]
		[TestMethod]
		public void db_delete_entity_interface_required_test()
		{
			TestConcurrentDbContext mongo = BuildTestDb();

			Owner o = mongo.Owners.First();

			Owner o2 = mongo.Owners.First();
			mongo.Save(o2);

			mongo.Delete(o);
		}

		// Doesn't even compile:  as intended
		//[ExpectedException(typeof(MongoConcurrencyException))]
		//[TestMethod]
		//public void db_delete_entity_without_interface_is_failure_test()
		//{
		//	TestConcurrentDbContext mongo = BuildTestDb();

		//	Pet p = mongo.Pets.First();

		//	mongo.Delete(p);

		//	Assert.AreEqual(0, mongo.Owners.Count());
		//}

		[ExpectedException(typeof (InvalidOperationException))]
		[TestMethod]
		public void db_save_entity_without_interface_is_error_test()
		{
			TestConcurrentDbContext mongo = BuildTestDb();

			var p = new Pet();
			mongo.Save(p);
		}

		[TestMethod]
		public void db_update_entity_without_interface_required_test()
		{
			TestConcurrentDbContext mongo = BuildTestDb();

			Lion p = mongo.Lions.First();

			string name = p.Name;
			p.Name = "Tiger";

			mongo.Save(p);

			Lion p2 = mongo.Lions.Single(c => c._id == p._id);

			Assert.AreNotEqual(name, p.Name);
			Assert.AreEqual(p.Name, p2.Name);
		}

		[ExpectedException(typeof (InvalidOperationException))]
		[TestMethod]
		public void db_update_entity_without_interface_is_failure_test()
		{
			TestConcurrentDbContext mongo = BuildTestDb();

			Pet p = mongo.Pets.First();

			mongo.Save(p);
		}

		[TestMethod]
		public void db_update_entity_with_interface_required_test()
		{
			TestConcurrentDbContext mongo = BuildTestDb();

			Owner o = mongo.Owners.First();

			int count = o.NumberOfPets;
			o.NumberOfPets++;

			mongo.Save(o);

			Owner o2 = mongo.Owners.Single(ow => ow._id == o._id);

			Assert.AreNotEqual(count, o.NumberOfPets);
			Assert.AreEqual(o.NumberOfPets, o2.NumberOfPets);
		}

		[TestMethod]
		public void db_update_entity_changes_accessId_test()
		{
			TestConcurrentDbContext mongo = BuildTestDb();

			Owner o = mongo.Owners.First();

			o.NumberOfPets++;
			string accessId = o._accessId;

			mongo.Save(o);

			Owner o2 = mongo.Owners.Single(ow => ow._id == o._id);

			// AccessID changes on save
			Assert.AreNotEqual(accessId, o._accessId);

			// Is persisted to DB.
			Assert.AreEqual(o2._accessId, o._accessId);
		}

		[TestMethod]
		public void db_update_applied_on_overwrite_test()
		{
			TestConcurrentDbContext mongo = BuildTestDb();

			Owner o = mongo.Owners.First();

			// Simulate other user updating db.
			Owner o2 = mongo.Owners.First();
			o2.NumberOfPets = 10;
			mongo.Save(o2);

			o.NumberOfPets = 1000;

			mongo.Save(o, ConcurrentSaveOptions.OverwriteServerChanges);

			Owner dbOwner = mongo.Owners.First();
			Assert.AreEqual(o.NumberOfPets, dbOwner.NumberOfPets);
		}


		[TestMethod]
		public void db_update_rolled_back_on_edit_test()
		{
			TestConcurrentDbContext mongo = BuildTestDb();

			Owner o = mongo.Owners.First();

			// Simulate other user updating db.
			Owner o2 = mongo.Owners.First();
			o2.NumberOfPets = 10;
			mongo.Save(o2);

			o.NumberOfPets = 1000;

			try
			{
				mongo.Save(o);
			}
			catch
			{
			}

			Owner dbOwner = mongo.Owners.First();
			Assert.AreEqual(o2.NumberOfPets, dbOwner.NumberOfPets);
		}

		[ExpectedException(typeof (MongoConcurrencyException))]
		[TestMethod]
		public void db_update_fails_on_edit_test()
		{
			TestConcurrentDbContext mongo = BuildTestDb();

			Owner o = mongo.Owners.First();

			// Simulate other user updating db.
			Owner o2 = mongo.Owners.First();
			o2.NumberOfPets++;
			mongo.Save(o2);

			mongo.Save(o);
		}


		private TestConcurrentDbContext BuildTestDb()
		{
			var mongo = new TestConcurrentDbContext("test_mongoctx");

			mongo.Clear<Lion>();
			mongo.Clear<Owner>();

			// Add some pets:
			var p = new Lion();
			p.Age = 5;
			p.Name = "Fluffy";
			p.Type = "Cat";

			mongo.Save(p);

			p = new Lion();
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