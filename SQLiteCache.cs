using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Data.SQLite.Linq;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace GeoTool
{


    static class SQLiteCache
    {
        private static string dbBasicPath;
        private static string dbFolderName;
        private static string dbFileName;
        private static string dbPath;

        private static SQLiteConnectionStringBuilder connBuilder = new SQLiteConnectionStringBuilder();

        static SQLiteCache()
        {
            dbBasicPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            dbFolderName = "GeoToolLib";
            dbFileName = "db.sqlite";
            
            if (!Directory.Exists(Path.Combine(dbBasicPath, dbFolderName)))
            {
                Directory.CreateDirectory(Path.Combine(dbBasicPath, dbFolderName));
            }

            dbPath = Path.Combine(dbBasicPath, dbFolderName, dbFileName);
            connBuilder.DataSource = dbPath;
            connBuilder.Version = 3;
            connBuilder.DefaultTimeout = 400;

            if (!File.Exists(dbPath))
            {
                initDb();
            }
        }

        private static void initDb()
        {
            SQLiteConnection.CreateFile(dbPath);

            using (SQLiteConnection connection = new SQLiteConnection(connBuilder.ToString()))
            {
                connection.Open();

                string query = @"CREATE TABLE [subnets] (
                    [subnet] INTEGER NOT NULL UNIQUE DEFAULT 1, 
                    [country] VARCHAR NOT NULL DEFAULT None, 
                    [city] VARCHAR NOT NULL DEFAULT None, 
                    [carrier] VARCHAR NOT NULL DEFAULT None, 
                    [org] VARCHAR NOT NULL DEFAULT None, 
                    [ccode] VARCHAR NOT NULL DEFAULT None, 
                    [state] VARCHAR NOT NULL DEFAULT None, 
                    [sld] VARCHAR NOT NULL DEFAULT None,  
                    [idx] INTEGER)";

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = query;
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void Add(GeoData ipData)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connBuilder.ToString()))
            {
                connection.Open();

                long longIP = IpUtilities.IpToUint(ipData.IpAddress);
                long subnet24 = longIP - (longIP % 256);
                long idx = longIP - (longIP % 65536);

                string query = string.Format(@"INSERT INTO [subnets] ([subnet], [country], [city], [carrier], [org], [ccode], [state], [sld], [idx]) 
                VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}' )",
                    subnet24,
                    ipData.Country,
                    ipData.City,
                    ipData.Carrier,
                    ipData.Organisation,
                    ipData.CountryCode,
                    ipData.State,
                    ipData.Sld,
                    idx);

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = query;
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
            }

        }

        public static GeoData Get(IPAddress ip)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connBuilder.ToString()))
            {
                connection.Open();

                long longIP = IpUtilities.IpToUint(ip);
                long subnet24 = longIP - (longIP % 256);

                string query = string.Format(@"SELECT * FROM [subnets] WHERE idx = ({0} - ({0} % 65536)) AND subnet = '{0}'", subnet24);

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = query;
                    command.CommandType = CommandType.Text;
                    SQLiteDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        GeoData ipData = new GeoData();
                        ipData.IpAddress = ip;
                        ipData.Country = dt.Rows[0].Field<string>("country");
                        ipData.City = dt.Rows[0].Field<string>("city");
                        ipData.Carrier = dt.Rows[0].Field<string>("carrier");
                        ipData.Organisation = dt.Rows[0].Field<string>("org");
                        ipData.CountryCode = dt.Rows[0].Field<string>("ccode");
                        ipData.State = dt.Rows[0].Field<string>("state");
                        ipData.Sld = dt.Rows[0].Field<string>("sld");
                        return ipData;
                    }
                }
            }
            return null;
        }
    }
}
