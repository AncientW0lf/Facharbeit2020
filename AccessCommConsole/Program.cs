using System;
using System.Security;
using AccessCommunication;

namespace AccessCommConsole
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			Console.WriteLine(
@"  __  __ _                     __ _       _                                            
 |  \/  (_)__ _ _ ___ ___ ___ / _| |_    /_\  __ __ ___ ______                         
 | |\/| | / _| '_/ _ (_-</ _ \  _|  _|  / _ \/ _/ _/ -_|_-<_-<                         
 |_|__|_|_\__|_| \___/__/\___/_|_ \__| /_/_\_\__\__\___/__/__/_                  _     
  / __|___ _ __  _ __ _  _ _ _ (_)__ __ _| |_(_)___ _ _    / __|___ _ _  ___ ___| |___ 
 | (__/ _ \ '  \| '  \ || | ' \| / _/ _` |  _| / _ \ ' \  | (__/ _ \ ' \(_-</ _ \ / -_)
  \___\___/_|_|_|_|_|_\_,_|_||_|_\__\__,_|\__|_\___/_||_|  \___\___/_||_/__/\___/_\___|
                                                                                       ");
			Console.WriteLine("Please enter a command:");

			string input = null;
			AccessComm comm = null;
			while(input != "exit")
			{
				Console.Write("> ");
				input = Console.ReadLine();

				switch(input)
				{
					case "help":
						ShowHelp();
						break;

					case "open":
						OpenDB(ref comm);
						break;

					case "close":
						CloseDB(ref comm);
						break;

					case "query":
						QueryDB(ref comm);
						break;
				}
			}
		}

		private static void ShowHelp()
		{
			Console.WriteLine("COMMANDS: help, open, close, query");
		}

		private static void OpenDB(ref AccessComm communicator)
		{
			if(communicator?.IsDisposed == false)
			{
				Console.WriteLine("A database has already been opened!");
				return;
			}

			Console.Write("Enter the path to a database (MS Access 2007-2016): ");
			string dbPath = Console.ReadLine();
			Console.Write("Enter the database password (if available): ");
			var dbPass = new SecureString();
			while(true)
			{
				ConsoleKeyInfo keystroke = Console.ReadKey(true);

				if(keystroke.KeyChar == (char)ConsoleKey.Enter)
					break;

				if(keystroke.KeyChar == (char) ConsoleKey.Backspace)
				{
					if(dbPass.Length <= 0) continue;

					dbPass.RemoveAt(dbPass.Length - 1);

					Console.CursorLeft--;
					Console.Write(" ");
					Console.CursorLeft--;
				}
				else
				{
					dbPass.AppendChar(keystroke.KeyChar);
					Console.Write("*");
				}
			}
			Console.WriteLine();

			try
			{
				communicator = new AccessComm(dbPath, dbPass);
			}
			catch(Exception e)
			{
				Console.WriteLine($"Could not open database! Exception: {e.Message}");

				if(communicator?.IsDisposed == false)
					communicator.Dispose();

				return;
			}
			Console.WriteLine("Opened database.");
		}

		private static void CloseDB(ref AccessComm communicator)
		{
			if(communicator?.IsDisposed == false)
			{
				communicator.Dispose();
				Console.WriteLine("Closed database.");
			}
			else
			{
				Console.WriteLine("There is no database to close.");
			}
		}

		private static void QueryDB(ref AccessComm communicator)
		{
			if(communicator?.IsDisposed == null || communicator?.IsDisposed == true)
			{
				Console.WriteLine("Please open a database first to execute queries.");
			}
			else
			{
				Console.Write("Query: ");
				string query = Console.ReadLine();
				Console.WriteLine("Executing query...");
				var res = new QueryResult();
				try
				{
					res = communicator.ExecuteQuery(query);
				}
				catch(Exception e)
				{
					Console.WriteLine(e.Message);
				}
				Console.WriteLine($"Query executed. Status: {(res.Success ? "Success" : "Failed")}\n" +
				                  $"Records affected: {res.RecordsAffected}");

				if(res.ReturnedRows?.Count <= 0 || res.ReturnedRows == null) return;

				Console.WriteLine();
				Console.WriteLine("Query returned data:");
				for(int y = 0; y < res.ReturnedRows.Count; y++)
				{
						Console.WriteLine(string.Join(", ", res.ReturnedRows[y]));
				}
			}
		}
	}
}
