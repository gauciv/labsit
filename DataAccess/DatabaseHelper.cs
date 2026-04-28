using MySql.Data.MySqlClient;

namespace LaboratorySitInSystem.DataAccess
{
    public static class DatabaseHelper
    {
        private static string _connectionString;

        public static void Initialize(string connectionString)
        {
            _connectionString = connectionString;
        }

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }
    }
}
