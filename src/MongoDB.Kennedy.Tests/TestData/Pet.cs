using System;
using System.Linq;
using MongoDB.Bson;

namespace MongoDB.Kennedy.Tests.TestData
{
	public class Pet
	{
		public ObjectId Id { get; set; }
		public string Name { get; set; }
		public string Type { get; set; }
		public int Age { get; set; }
	}
}