using System.Collections.Generic;
using Npgsql;

namespace DBSizeChecker.DB
{

    /// <summary>
    /// Perfoms query's to db
    /// </summary>
    public class DBClient
    {
        private readonly NpgsqlConnection _connection;
        public DBClient(string connectionSettings) { _connection = new NpgsqlConnection(connectionSettings); }

        public List<DBSizeModel> GetInfo()
        {
            _connection.Open();
            var sizes = new List<DBSizeModel>();
            foreach(var dbName in TakeDBNames())
                {
                    var dbSize = new DBSizeModel(TakeDBSize(dbName), dbName);
                    sizes.Add(dbSize);
                }

            _connection.Close();
            return sizes;
        }

        private List<string> TakeDBNames()
        {
            var query = "SELECT datname FROM  pg_database;";
            var names = new List<string>();
            using(var cmd = new NpgsqlCommand(query, _connection))
                {
                    using(var reader = cmd.ExecuteReader())
                        {
                            while(reader.Read())
                                names.Add(reader.GetString(0));
                        }
                }

            return names;
        }

        private long TakeDBSize(string dbName)
        {
            var query = $"SELECT pg_database_size('{dbName}');";
            using(var cmd = new NpgsqlCommand(query, _connection))
                {
                    using(var reader = cmd.ExecuteReader())
                        {
                            if(reader.Read())
                                return reader.GetInt64(0);
                            return 0;
                        }
                }
        }
    }
}
