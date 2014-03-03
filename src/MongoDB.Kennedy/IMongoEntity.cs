using System;
using System.Linq;
using MongoDB.Bson;

namespace MongoDB.Kennedy
{
	public interface IMongoEntity 
	{
		ObjectId _id { get; }
		string _accessId { get; set; }	
	}
}
