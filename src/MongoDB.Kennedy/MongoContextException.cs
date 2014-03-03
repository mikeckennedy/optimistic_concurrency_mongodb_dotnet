using System;
using System.Linq;
using System.Runtime.Serialization;
using MongoDB.Driver;

namespace MongoDB.Kennedy
{
	public class MongoContextException : MongoException 
	{
		public MongoContextException(string message) : base(message)
		{
		}

		public MongoContextException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public MongoContextException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
