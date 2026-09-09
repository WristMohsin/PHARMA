using System;
using System.Configuration;
using System.Data;
using System.Data.Odbc;

namespace PHARMA.DataAccess
{
    /// <summary>
    /// Creates ODBC connections for SQL Server.
    /// Compatible with Windows 7+ (use SQL Server ODBC Driver or Native Client).
    /// </summary>
    public static class DbConnectionFactory
    {
        private static string _connectionString;

        public static string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    var cs = ConfigurationManager.ConnectionStrings[PHARMA.Common.Constants.ConnectionStringName];
                    if (cs == null || string.IsNullOrEmpty(cs.ConnectionString))
                        throw new InvalidOperationException("Connection string 'PHARMA' not found in App.config. Use ODBC format.");
                    _connectionString = cs.ConnectionString;
                }
                return _connectionString;
            }
            set { _connectionString = value; } // for testing
        }

        public static IDbConnection CreateConnection()
        {
            var conn = new OdbcConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        public static IDbConnection CreateConnection(string connectionString)
        {
            var conn = new OdbcConnection(connectionString);
            conn.Open();
            return conn;
        }
    }
}