using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace Version4
{
    class dbtest
    {
        public static void Test()
        {
            SqliteConnectionStringBuilder cstring =
                new SqliteConnectionStringBuilder()
                {
                    Cache = SqliteCacheMode.Private,
                    DataSource = @".\maps.sql3",
                    Mode = SqliteOpenMode.ReadWriteCreate
                };
            Console.WriteLine(cstring.ConnectionString);
            SqliteConnection conn = new SqliteConnection(cstring.ConnectionString);
            SqliteCommand C = new SqliteCommand("CREATE TABLE wilma (name)", conn);
            conn.Open();
            C.ExecuteNonQuery();
        }
    }
}
