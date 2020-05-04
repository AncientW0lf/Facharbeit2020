using System;
using System.Security;
using AccessCommunication;

namespace AccessCommConsole
{
	/// <summary>
	/// Main entry point of application.
	/// </summary>
	internal class Program
	{
		/// <summary>
		/// Singleton of this application.
		/// </summary>
		private static Program _instance;

		/// <summary>
		/// Main entry point of this application.
		/// </summary>
		private static void Main(string[] args)
		{
			_instance = new Program();
		}

		/// <summary>
		/// Initializes the singleton of this application. Is a loop to ask for user commands.
		/// </summary>
		private Program()
		{
			//Console splash screen
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

			//Enters loop to ask for commands until the command "exit" is entered
			string input = null;
			AccessComm comm = null;
			while(input != "exit")
			{
				//Reads the next user input
				Console.Write("> ");
				input = Console.ReadLine();

				//Checks if the command is valid and executes linked method
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

					default:
						Console.WriteLine($"\"{input}\" is not a valid command. Please enter \"help\" for a list of commands.");
						break;
				}
			}
		}

		/// <summary>
		/// Prints a simple documentation of all available commands to the console.
		/// </summary>
		private void ShowHelp()
		{
			Console.WriteLine("COMMANDS: help, open, close, query");
		}

		/// <summary>
		/// Tries to open a database by asking the user for a path and password.
		/// </summary>
		/// <param name="communicator">The <see cref="AccessComm"/> to actually open the database.</param>
		private void OpenDB(ref AccessComm communicator)
		{
			//Checks if a database is already open
			if(communicator?.IsDisposed == false)
			{
				Console.WriteLine("A database has already been opened!");
				return;
			}

			//Asks the user for a db path
			Console.Write("Enter the path to a database (MS Access 2007-2016): ");
			string dbPath = Console.ReadLine();

			//Asks the user for the db password, saving it in a secure string
			Console.Write("Enter the database password (if available): ");
			var dbPass = new SecureString();
			while(true)
			{
				//Reads the next key but doesn't display it in console
				ConsoleKeyInfo keystroke = Console.ReadKey(true);

				//Checks if the user pressed enter to finish writing the password
				if(keystroke.KeyChar == (char)ConsoleKey.Enter)
					break;

				//Checks if the user wants to delete a character
				if(keystroke.KeyChar == (char) ConsoleKey.Backspace)
				{
					//Skips deleting íf character length is already 0
					if(dbPass.Length <= 0) continue;

					//Removes the last character
					dbPass.RemoveAt(dbPass.Length - 1);

					//Removes the last character from console
					Console.CursorLeft--;
					Console.Write(" ");
					Console.CursorLeft--;
				}
				else
				{
					//Writes the character both to the secure string and the console (censored)
					dbPass.AppendChar(keystroke.KeyChar);
					Console.Write("*");
				}
			}
			Console.WriteLine();

			//Tries to open the database with the specified data
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

		/// <summary>
		/// Closes the currently opened database.
		/// </summary>
		/// <param name="communicator">The <see cref="AccessComm"/> which holds the opened database.</param>
		private void CloseDB(ref AccessComm communicator)
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

		/// <summary>
		/// Executes a database query in the opened database.
		/// </summary>
		/// <param name="communicator">The <see cref="AccessComm"/> to execute the query in.</param>
		private void QueryDB(ref AccessComm communicator)
		{
			//Informs the user that a database has to be opened for this command to work
			if(communicator?.IsDisposed == null || communicator?.IsDisposed == true)
			{
				Console.WriteLine("Please open a database first to execute queries.");
				return;
			}

			//Reads the user query
			Console.Write("Query: ");
			string query = Console.ReadLine();

			//Tries to execute the query
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

			//Outputs if the query was successful and how many database records were affected
			Console.WriteLine($"Query executed. Status: {(res.Success ? "Success" : "Failed")}\n" +
			                  $"Records affected: {res.RecordsAffected}");

			//Checks if the query returned some data
			if(res.ReturnedRows?.Count <= 0 || res.ReturnedRows == null) return;

			//Displays the returned data
			Console.WriteLine();
			Console.WriteLine("Query returned data:");
			for(int y = 0; y < res.ReturnedRows.Count; y++)
			{
				Console.WriteLine(string.Join(", ", res.ReturnedRows[y]));
			}
		}
	}
}
