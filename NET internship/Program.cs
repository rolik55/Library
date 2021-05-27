using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using Newtonsoft.Json.Converters;
using System.Threading;

namespace NET_internship
{
	public class Book
	{
		public string Name { get; set; }
		public string Author { get; set; }
		public string Category { get; set; }
		public string Language { get; set; }
		public DateTime publication_date { get; set; }
		public string ISBN { get; set; }
		public bool IsTaken { get; set; }
		public int PeriodTaken { get; set; }
		public DateTime DateTaken { get; set; }
		public string TakenBy { get; set; }
		public Book(string nam, string auth, string categ, string lang, string date, string number)	// constructor
		{
			Name = nam;
			Author = auth;
			Category = categ;
			Language = lang;
			publication_date = Convert.ToDateTime(date);
			ISBN = number;
		}
		public static void SetTaken(Library Added_Books, int index, int period, string person) // sets values when a book is being taken
		{
			Added_Books.books[index].IsTaken = true;
			Added_Books.books[index].TakenBy = person;
			Added_Books.books[index].PeriodTaken = period;
			Added_Books.books[index].DateTaken = DateTime.Now;
		}
		public static void SetReturned(Library Added_Books, int index) // sets values back to default when a book is returned
		{
			DateTime Deadline = Added_Books.books[index].DateTaken.AddDays(Added_Books.books[index].PeriodTaken);
			int compare = DateTime.Compare(Deadline, Added_Books.books[index].DateTaken);
			if (compare > 0)	// checks if return is too late or not
				Console.WriteLine("Book was returned too late. Displaying a funny message :)");
			else Console.WriteLine("Book was returned on time.");
			Thread.Sleep(2000);
			Added_Books.books[index].IsTaken = false;
			Added_Books.Readers[Added_Books.books[index].TakenBy]--;
			Added_Books.books[index].TakenBy = "";
			Added_Books.books[index].PeriodTaken = 0;
			Added_Books.books[index].DateTaken = DateTime.MinValue;
			
		}
		public static bool isAlreadyTaken(int index, Library Added_Books)	// checks if book is taken
		{
			if (Added_Books.books[index].IsTaken == true)
				return true;
			return false;
		}
		public static int FindISBN(string number, Library Added_Books)	// checks if ISBN exists  
		{
			for (int i = 0; i < Added_Books.books.Count; i++)
			{
				if (Added_Books.books[i].ISBN == number)
				{
					return i;
				}
			}
			return -1;
		}
		public static void TakeBook(Library Added_Books) // collects information required for taking a book
		{
			Console.WriteLine("Who is taking a book?");
			string person = Console.ReadLine();
			try
			{
				Added_Books.Readers[person]++;
			}
			catch (KeyNotFoundException)
			{
				Added_Books.Readers.Add(person, 1);
			}
			if (Added_Books.Readers[person] > 3)	// does not allow to take more than 3 books
			{
				while (Added_Books.Readers[person] > 3)
				{
					Added_Books.Readers[person]--;
					Console.WriteLine("This person cannot take any more books. Name another person who is taking a book.");
					person = Console.ReadLine();
					try
					{
						Added_Books.Readers[person]++;
					}
					catch (KeyNotFoundException)
					{
						Added_Books.Readers.Add(person, 1);
					}
				}
			}
			Console.WriteLine("ISBN of the book that is being taken:");
			string number = Console.ReadLine();
			int index = FindISBN(number, Added_Books);
			while (1==1)	// checks if ISBN exists AND the book is not yet taken
			{
				if (index == -1)
				{
					Console.WriteLine("Specified ISBN does not exist. Try inputing another ISBN.");
					number = Console.ReadLine();
					index = FindISBN(number, Added_Books);
				}
				else if (isAlreadyTaken(index, Added_Books))
				{
					Console.WriteLine("This book is already taken. Try inputing another ISBN.");
					number = Console.ReadLine();
					index = FindISBN(number, Added_Books);
				}
				else break;
			}
			Console.WriteLine("For how long is the book being taken in days? Maximum 60 days.");
			string input = Console.ReadLine();
			while (Program.isNumber(input) == false || (Program.isNumber(input) == true && (Convert.ToInt32(input) <= 0 || Convert.ToInt32(input) > 60))) // checks if the period is allowed
			{
				Console.WriteLine("Input is invalid, please try again.");
				input = Console.ReadLine();
			}
			int period = Convert.ToInt32(input);
			SetTaken(Added_Books, index, period, person);
		}
		public static void ListBook(Library Added_Books, int index)	// prints out book information
        {
			Console.WriteLine();
			Console.Write("Name: ");
			Console.WriteLine(Added_Books.books[index].Name);
			Console.Write("Author: ");
			Console.WriteLine(Added_Books.books[index].Author);
			Console.Write("Category: ");
			Console.WriteLine(Added_Books.books[index].Category);
			Console.Write("Language: ");
			Console.WriteLine(Added_Books.books[index].Language);
			Console.Write("Publication date: ");
			Console.WriteLine(Added_Books.books[index].publication_date);
			Console.Write("ISBN: ");
			Console.WriteLine(Added_Books.books[index].ISBN);
		}
	}
	public class Library
	{
		public List<Book> books { get; set; }
		public Dictionary<string, int> Readers = new Dictionary<string, int>();
	}
	class Program
	{
		static Library Added_Books;
		static bool loop = true;
		static int selection;
		public static void Commands() // prints out the main menu interface
		{
			Console.WriteLine("1) Add a new book");
			Console.WriteLine("2) Take a book");
			Console.WriteLine("3) Return a book");
			Console.WriteLine("4) Filter books");
			Console.WriteLine("5) Delete a book");
			Console.WriteLine("6) Exit");
		}
		public static void LoadJson() // deserializes json file to class 
		{
			using (StreamReader r = File.OpenText("books.json"))
			{
				string json = r.ReadToEnd();
				Added_Books = JsonConvert.DeserializeObject<Library>(json);
			}
		}
		public static void WriteJson(string output) // updates the json file
		{
			using (StreamWriter writer = new StreamWriter("books.json"))
			{
				writer.WriteLine(output);
			}
		}
		public static bool isValidDate(string date)	// checks if the input is a valid date
		{
			var dateFormats = new[] { "yyyy.MM.dd", "yyyy-M-dd", "yyyy/MM/dd", "yyyy-MM-dd", "yyyy-M-d" };
			DateTime datetime;
			bool validDate = DateTime.TryParseExact(
				date,
				dateFormats,
				DateTimeFormatInfo.InvariantInfo,
				DateTimeStyles.None,
				out datetime);
			if (validDate)
				return true;
			else
				return false;
		}
		public static bool isNumber(string input)	// checks if the input is a number
		{
			bool b = Microsoft.VisualBasic.Information.IsNumeric(input);
			if (b == false)
				return false;
			return true;
		}
		public static void Results(int count)	// prints out result count after filtering
		{
			Console.WriteLine();
			if (count == 0)
			{
				Console.WriteLine("No matching results were found.");
			}
			else if (count == 1)
			{
				Console.Write(count);
				Console.WriteLine(" result was found.");
			}
			else
			{
				Console.Write(count);
				Console.WriteLine(" results were found.");
			}
			Console.WriteLine();
			Console.WriteLine("Press any key to continue.");
			Console.ReadKey();
		}
		static void Main(string[] args)
		{
			while (loop)
			{
				Console.Clear();
				Commands();
				string input = Console.ReadLine();
				while (isNumber(input) == false || (isNumber(input) == true && (Convert.ToInt32(input) < 1 || Convert.ToInt32(input) > 6)))	// checks if input is valid
				{
					Console.WriteLine("Input is invalid, please try again.");
					input = Console.ReadLine();
				}
				selection = Convert.ToInt32(input);
				if (selection == 1)		// add a new book
				{
					string nam, auth, categ, lang, number, date;
					Console.WriteLine("Name:");
					nam = Console.ReadLine();
					Console.WriteLine("Author:");
					auth = Console.ReadLine();
					Console.WriteLine("Category:");
					categ = Console.ReadLine();
					Console.WriteLine("Language:");
					lang = Console.ReadLine();
					Console.WriteLine("Publication date (YYYY-MM-DD):");
					date = Console.ReadLine();
					while (isValidDate(date) == false)
					{
						Console.WriteLine("Input date is invalid. Try inputing another date.");
						date = Console.ReadLine();
					}
					Console.WriteLine("Number:");
					number = Console.ReadLine();
					Book newBook = new Book(nam, auth, categ, lang, date, number);
					LoadJson();
					Added_Books.books.Add(newBook);
					string output = JsonConvert.SerializeObject(Added_Books);
					WriteJson(output);
					Console.WriteLine("The book was succesfully added to the library.");
					Thread.Sleep(2000);
				}
				else if (selection == 2)	// take a book
				{
					LoadJson();
					Book.TakeBook(Added_Books);
					string output = JsonConvert.SerializeObject(Added_Books);
					WriteJson(output);
					Console.WriteLine("The book was succesfully taken.");
					Thread.Sleep(2000);
				}
				else if (selection == 3)	// return a book
				{
					LoadJson();
					Console.WriteLine("ISBN of the book that is being returned:");
					input = Console.ReadLine();
					int index = Book.FindISBN(input, Added_Books);
					while (1 == 1)	// checks if ISBN exists AND the book is taken
					{
						if (index == -1)
						{
							Console.WriteLine("Specified ISBN does not exist. Try inputing another ISBN.");
							input = Console.ReadLine();
							index = Book.FindISBN(input, Added_Books);
						}
						else if (Book.isAlreadyTaken(index, Added_Books) == false)
						{
							Console.WriteLine("This book is not yet taken. Try inputing another ISBN.");
							input = Console.ReadLine();
							index = Book.FindISBN(input, Added_Books);
						}
						else break;
					}
					Book.SetReturned(Added_Books, index);
					string output = JsonConvert.SerializeObject(Added_Books);
					WriteJson(output);
				}
				else if (selection == 4)	// filter books
                {
					LoadJson();
					Console.Clear();
					Console.WriteLine("1) Filter by author");
					Console.WriteLine("2) Filter by category");
					Console.WriteLine("3) Filter by language");
					Console.WriteLine("4) Filter by ISBN");
					Console.WriteLine("5) Filter by name");
					Console.WriteLine("6) Filter by taken");
					Console.WriteLine("7) Filter by available");
					input = Console.ReadLine();
					while (isNumber(input) == false || (isNumber(input) == true && (Convert.ToInt32(input) < 1 || Convert.ToInt32(input) > 7))) // checks if input is valid
					{
						Console.WriteLine("Input is invalid, please try again.");
						input = Console.ReadLine();
					}
					selection = Convert.ToInt32(input);
					int index, count = 0;
					if (selection == 1)		// filter by author
					{
						Console.WriteLine("Author name to filter by:");
						input = Console.ReadLine();
						for (int i = 0; i < Added_Books.books.Count; i++)
						{
							if (Added_Books.books[i].Author == input)
							{
								index = i;
								Book.ListBook(Added_Books, index);
								count++;
							}
						}
						Results(count);
					}
					else if (selection == 2)	// filter by category
					{
						Console.WriteLine("Category to filter by:");
						input = Console.ReadLine();
						for (int i = 0; i < Added_Books.books.Count; i++)
						{
							if (Added_Books.books[i].Category == input)
							{
								index = i;
								Book.ListBook(Added_Books, index);
								count++;
							}
						}
						Results(count);
					}
					else if (selection == 3)	// filter by language
					{
						Console.WriteLine("Language to filter by:");
						input = Console.ReadLine();
						for (int i = 0; i < Added_Books.books.Count; i++)
						{
							if (Added_Books.books[i].Language == input)
							{
								index = i;
								Book.ListBook(Added_Books, index);
								count++;
							}
						}
						Results(count);
					}
					else if (selection == 4)	// filter by ISBN
                    {
						Console.WriteLine("ISBN to filter by:");
						input = Console.ReadLine();
						for (int i = 0; i < Added_Books.books.Count; i++)
						{
							if (Added_Books.books[i].ISBN == input)
							{
								index = i;
								Book.ListBook(Added_Books, index);
								count++;
							}
						}
						Results(count);
					}
					else if (selection == 5)	// filter by book name
					{
						Console.WriteLine("Book name to filter by:");
						input = Console.ReadLine();
						for (int i = 0; i < Added_Books.books.Count; i++)
						{
							if (Added_Books.books[i].Name == input)
							{
								index = i;
								Book.ListBook(Added_Books, index);
								count++;
							}
						}
						Results(count);
					}
					else if (selection == 6)	// filter by taken
					{
						for (int i = 0; i < Added_Books.books.Count; i++)
						{
							if (Added_Books.books[i].IsTaken == true)
							{
								index = i;
								Book.ListBook(Added_Books, index);
								count++;
							}
						}
						Results(count);
					}
					else if (selection == 7)	// filter by available
					{
						for (int i = 0; i < Added_Books.books.Count; i++)
						{
							if (Added_Books.books[i].IsTaken == false)
							{
								index = i;
								Book.ListBook(Added_Books, index);
								count++;
							}
						}
						Results(count);
					}
				}
				else if (selection == 5)	// delete a book
                {
					LoadJson();
					Console.WriteLine("ISBN of the book that is being deleted:");
					input = Console.ReadLine();
					int index = Book.FindISBN(input, Added_Books);
					while (index == -1)	// checks if book exists
                    {
						Console.WriteLine("Specified book does not exist. Input another ISBN:");
						input = Console.ReadLine();
						index = Book.FindISBN(input, Added_Books);
                    }
					Added_Books.books.RemoveAt(index);
					string output = JsonConvert.SerializeObject(Added_Books);
					WriteJson(output);
					Console.WriteLine("The book was succesfully deleted.");
					Thread.Sleep(2000);
				}
				else if (selection == 6)	// exit the program
				{
					Environment.Exit(0);
				}
			}
		}
	}
}
