using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;

namespace MongoDB.Kennedy
{
	public interface IMongoEntity 
	{
		ObjectId _id { get; }
		string _accessId { get; set; }	
	}
}
