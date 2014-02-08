using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;

namespace MongoDB.Kennedy.Sample
{
	public class Book : IMongoEntity
	{
		public ObjectId _id { get; set; }
		public string Name { get; set; }
		public int PageCount { get; set; }
		public string _accessId { get; set; }
	}
}
