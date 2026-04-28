using LaboratorySitInSystem.Models;
using MySql.Data.MySqlClient;

namespace LaboratorySitInSystem.DataAccess
{
    public class AdminRepository : IAdminRepository
    {
        public AdminUser Authenticate(string username, string password)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();
            using var command = new MySqlCommand(
                "SELECT admin_id, username, password_hash FROM admin_users WHERE username = @username AND password_hash = SHA2(@password, 256)",
                connection);
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", password);
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new AdminUser
                {
                    AdminId = reader.GetInt32("admin_id"),
                    Username = reader.GetString("username"),
                    PasswordHash = reader.GetString("password_hash")
                };
            }
            return null;
        }

        public void UpdatePassword(string username, string newPasswordHash)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();
            using var command = new MySqlCommand(
                "UPDATE admin_users SET password_hash = SHA2(@newPassword, 256) WHERE username = @username",
                connection);
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@newPassword", newPasswordHash);
            command.ExecuteNonQuery();
        }
    }
}
