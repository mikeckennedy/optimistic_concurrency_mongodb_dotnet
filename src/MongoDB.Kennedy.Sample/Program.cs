using System;
using System.Linq;

namespace MongoDB.Kennedy.Sample
{
	internal class Program
	{
		private static void Main()
		{
			Console.WriteLine("Sample app for concurrent mongodb context by @mkennedy");

			var ctx = new DataContext("SampleDriverDb");
			if (!ctx.Books.Any())
			{
				Console.WriteLine("Adding sample data.");
				LoadData();
			}

			Book book = ctx.Books.OrderBy(b => b._id).First();

			Console.WriteLine("Let's edit this book: " + book.Name);
			Console.Write("Enter a new title for the first book: ");
			book.Name = Console.ReadLine();

			Console.Write("Do you want to simulate an edit in between? [Y/N]  ");
			string edit = (Console.ReadLine() ?? "n").Trim().ToLower();
			if (edit == "y")
			{
				// imagine this ran in another app or page request in parallel.
				Book bookEdited = ctx.Books.OrderBy(b => b._id).First();
				bookEdited.PageCount++;
				ctx.Save(bookEdited);
			}

			var saveMode = ConcurrentSaveOptions.ProtectServerChanges;
			if (edit == "y")
			{
				Console.Write("Do you want to save in 'protect server changes mode' or 'overwrite mode'? [P/O]  ");
				string writeMode = (Console.ReadLine() ?? "p").Trim().ToLower();
				saveMode = writeMode == "o"
					? ConcurrentSaveOptions.OverwriteServerChanges
					: ConcurrentSaveOptions.ProtectServerChanges;
			}

			Console.WriteLine("Saving your edits...");
			Console.WriteLine("Saving with mode: " + saveMode);
			try
			{
				// simple save - protects server changes: ctx.Save(book);
				ctx.Save(book, saveMode); // pass the save mode so users can test both outcomes.
				Console.WriteLine("Save successful.");
			}
			catch (Exception x)
			{
				Console.WriteLine("Save error: " + x);
			}
		}

		public static void LoadData()
		{
			var ctx = new DataContext("SampleDriverDb");

			Book[] books =
			{
				new Book {Name = "Book 1", PageCount = 100},
				new Book {Name = "Book 2", PageCount = 200},
				new Book {Name = "Book 3", PageCount = 300},
				new Book {Name = "Book 4", PageCount = 400}
			};

			foreach (Book book in books)
			{
				ctx.Save(book);
			}
		}
	}
}