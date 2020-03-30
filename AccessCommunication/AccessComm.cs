using BoxLib.Scripts;
using System.Data.OleDb;
using System.Security;

namespace AccessCommunication
{
    internal static class AccessComm
    {
        public static QueryResult ExecuteQuery(string filepath, SecureString password, string query)
        {
            var res = new QueryResult();

            password.Handle(pass =>
            {
                // Create and open the connection in a using block. This
                // ensures that all resources will be closed and disposed
                // when the code exits.
                using var connection = new OleDbConnection(
                    "Provider=Microsoft.ACE.OLEDB.12.0;" +
                    $"Data Source={filepath};" +
                    "Persist Security Info=False;" +
                    $"Jet OLEDB:Database Password={pass};");
			
                // Create the Command object.
                using var command = new OleDbCommand(query, connection);

                // Open the connection in a try/catch block. 
                // Create and execute the DataReader, writing the result
                // set to the console window.
                OleDbDataReader reader = null;
                try
                {
                    connection.Open();
                    reader = command.ExecuteReader();

                    res = new QueryResult(query, reader);
                }
                finally
                {
                    reader?.Close();
                    command.Dispose();
                }
            });

            return res;
        }
    }
}
