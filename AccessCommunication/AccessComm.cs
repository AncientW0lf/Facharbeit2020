using BoxLib.Scripts;
using System;
using System.Data.Common;
using System.Data.OleDb;
using System.IO;
using System.Security;
using System.Threading.Tasks;

namespace AccessCommunication
{
	/// <summary>
	/// This communicator opens a Microsoft Access database and can execute queries in it to manipulate or fetch data.
	/// </summary>
	public class AccessComm : IDisposable
	{
		/// <summary>
		/// Checks if this object has been disposed.
		/// </summary>
		public bool IsDisposed { get; private set; }

		/// <summary>
		/// The connection object to the database.
		/// </summary>
		private OleDbConnection _connection;

		/// <summary>
		/// Initializes a new <see cref="AccessComm"/> and opens the specified database.
		/// </summary>
		/// <param name="dbPath">The full path to the database to open.</param>
		/// <param name="password">The optional password to open the database.</param>
		/// <exception cref="FileNotFoundException">Throws when the specified database file cannot be found.</exception>
		/// <exception cref="OleDbException">Contains database specific errors.</exception>
		public AccessComm(string dbPath, SecureString password)
		{
			if(!File.Exists(dbPath))
				throw new FileNotFoundException("Could not find specified database!", dbPath);

			password.Handle(pass => _connection = new OleDbConnection(
				"Provider=Microsoft.ACE.OLEDB.12.0;" +
				$"Data Source={dbPath};" +
				"Persist Security Info=False;" +
				$"Jet OLEDB:Database Password={pass};"));
			
			_connection.Open();
		}

		/// <summary>
		/// Executes a query inside the database and returns a <see cref="QueryResult"/> object.
		/// </summary>
		/// <param name="query">The full query to execute.</param>
		/// <returns>Detailed info about the executed query.</returns>
		public async Task<QueryResult> ExecuteQuery(string query)
		{
			//Throws an exception if the object is already disposed
			if(IsDisposed) throw new InvalidOperationException("Connection is closed.");

			//Creates the command object which holds the query
			using var command = new OleDbCommand(query, _connection);

			//Tries to execute the query and save its info
			DbDataReader reader = null;
			QueryResult res;
			try
			{
				reader = await command.ExecuteReaderAsync();

				res = new QueryResult(query, reader);
			}
			finally
			{
				//Disposes the used objects
				reader?.Close();
				command.Dispose();
			}

			return res;
		}

		/// <inheritdoc />
		public void Dispose()
		{
			if(IsDisposed) return;

			_connection.Close();
			_connection.Dispose();
			IsDisposed = true;
		}
	}
}
