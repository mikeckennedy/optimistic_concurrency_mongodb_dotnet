using System;
using MongoDB.Bson;

namespace MongoDB.Kennedy.Tests.TestData
{
	public class Owner : IMongoEntity
	{
		public string Name { get; set; }
		public int NumberOfPets { get; set; }
		public ObjectId _id { get; set; }

		public string _accessId { get; set; }
	}
}