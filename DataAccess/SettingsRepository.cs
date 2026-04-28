using LaboratorySitInSystem.Models;
using MySql.Data.MySqlClient;

namespace LaboratorySitInSystem.DataAccess
{
    public class SettingsRepository : ISettingsRepository
    {
        public SystemSettings GetSettings()
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();
            using var command = new MySqlCommand(
                "SELECT settings_id, alarm_threshold FROM system_settings WHERE settings_id = 1",
                connection);
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new SystemSettings
                {
                    SettingsId = reader.GetInt32("settings_id"),
                    AlarmThreshold = reader.GetInt32("alarm_threshold")
                };
            }
            return null;
        }

        public void UpdateAlarmThreshold(int threshold)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();
            using var command = new MySqlCommand(
                "UPDATE system_settings SET alarm_threshold = @threshold WHERE settings_id = 1",
                connection);
            command.Parameters.AddWithValue("@threshold", threshold);
            command.ExecuteNonQuery();
        }
    }
}
