using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace KellTerminal.SQL
{
    public static class HelpConnection
    {
        static string connString = ConfigurationManager.ConnectionStrings["connString"].ConnectionString;
        public static string ConnectionString
        {
            get
            {
                return connString;
            }
            set
            {
                connString = value;
            }
        }

        public static SqlConnection GetConnection()
        {
            SqlConnection sqlcon = new SqlConnection(ConnectionString);
            return sqlcon;
        }

        public static bool TestConnect(string connString)
        {
            SqlConnection conn = new SqlConnection(connString);
            try
            {
                conn.Open();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
        }
    }
}
