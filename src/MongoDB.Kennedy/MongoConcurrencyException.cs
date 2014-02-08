using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;

namespace MongoDB.Kennedy
{
	[Serializable]
	public class MongoConcurrencyException : MongoException
	{
		public MongoConcurrencyException(string msg) : base(msg)
		{
		}
	}
}
