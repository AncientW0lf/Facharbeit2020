using BoxLib.Scripts;
using System;
using System.Data.OleDb;
using System.Security;

namespace AccessCommunication
{
	public class AccessComm : IDisposable
	{
		public bool IsDisposed { get; private set; }

		private OleDbConnection _connection;

		public AccessComm(string dbPath, SecureString password)
		{
			password.Handle(pass => _connection = new OleDbConnection(
				"Provider=Microsoft.ACE.OLEDB.12.0;" +
				$"Data Source={dbPath};" +
				"Persist Security Info=False;" +
				$"Jet OLEDB:Database Password={pass};"));
			
			_connection.Open();
		}

		public QueryResult ExecuteQuery(string query)
		{
			if(IsDisposed) throw new InvalidOperationException("Connection is closed.");

			// Create the Command object.
			using var command = new OleDbCommand(query, _connection);

			// Open the connection in a try/catch block. 
			// Create and execute the DataReader, writing the result
			// set to the console window.
			OleDbDataReader reader = null;
			QueryResult res;
			try
			{
				reader = command.ExecuteReader();

				res = new QueryResult(query, reader);
			}
			finally
			{
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
