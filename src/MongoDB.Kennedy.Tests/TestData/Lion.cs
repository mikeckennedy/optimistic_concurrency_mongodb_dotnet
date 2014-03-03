using System;
using MongoDB.Bson;

namespace MongoDB.Kennedy.Tests.TestData
{
	public class Lion : IMongoEntity
	{
		public string Name { get; set; }
		public string Type { get; set; }
		public int Age { get; set; }
		public string _accessId { get; set; }
		public ObjectId _id { get; set; }
	}
}