using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Kennedy.Sample
{
	public class DataContext : ConcurrentDataContext
	{
		public DataContext(string dbName) : base(dbName)
		{
		}

		public IQueryable<Book> Books
		{
			get { return base.GetCollection<Book>(); }
		}
	}
}
