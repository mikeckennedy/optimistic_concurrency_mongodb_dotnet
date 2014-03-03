using System;
using System.Linq;
using System.Runtime.Serialization;

namespace MongoDB.Kennedy
{
	[Serializable]
	public class MongoConcurrencyException : MongoContextException 
	{
		public MongoConcurrencyException(string message) : base(message)
		{
		}

		public MongoConcurrencyException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public MongoConcurrencyException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
