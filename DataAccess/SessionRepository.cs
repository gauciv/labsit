using System;
using System.Collections.Generic;
using LaboratorySitInSystem.Models;
using MySql.Data.MySqlClient;

namespace LaboratorySitInSystem.DataAccess
{
    public class SessionRepository : ISessionRepository
    {
        // Static constructor to run migrations once when the class is first used
        static SessionRepository()
        {
            EnsureApprovalColumnsExist();
        }

        /// <summary>
        /// Checks if the approval columns exist in the sitin_sessions table and adds them if missing.
        /// This runs automatically on first use of SessionRepository.
        /// </summary>
        private static void EnsureApprovalColumnsExist()
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                connection.Open();

                // Check if requires_approval column exists
                bool requiresApprovalExists = ColumnExists(connection, "sitin_sessions", "requires_approval");
                bool isApprovedExists = ColumnExists(connection, "sitin_sessions", "is_approved");

                // Add requires_approval column if it doesn't exist
                if (!requiresApprovalExists)
                {
                    using var cmd = new MySqlCommand(
                        "ALTER TABLE sitin_sessions ADD COLUMN requires_approval BOOLEAN NOT NULL DEFAULT FALSE",
                        connection);
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("Added 'requires_approval' column to sitin_sessions table.");
                }

                // Add is_approved column if it doesn't exist
                if (!isApprovedExists)
                {
                    using var cmd = new MySqlCommand(
                        "ALTER TABLE sitin_sessions ADD COLUMN is_approved BOOLEAN NOT NULL DEFAULT FALSE",
                        connection);
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("Added 'is_approved' column to sitin_sessions table.");
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't crash the application
                Console.WriteLine($"Error ensuring approval columns exist: {ex.Message}");
            }
        }

        /// <summary>
        /// Helper method to check if a column exists in a table.
        /// </summary>
        private static bool ColumnExists(MySqlConnection connection, string tableName, string columnName)
        {
            using var cmd = new MySqlCommand(
                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS " +
                "WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @tableName AND COLUMN_NAME = @columnName",
                connection);
            cmd.Parameters.AddWithValue("@tableName", tableName);
            cmd.Parameters.AddWithValue("@columnName", columnName);
            
            var result = cmd.ExecuteScalar();
            return Convert.ToInt32(result) > 0;
        }

        public List<SitInSession> GetActiveSessions()
        {
            var sessions = new List<SitInSession>();
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();
            using var command = new MySqlCommand(
                "SELECT s.session_id, s.student_id, CONCAT(st.first_name, ' ', st.last_name) AS student_name, " +
                "s.subject_name, s.start_time, s.end_time, s.is_scheduled, s.early_ended, " +
                "s.requires_approval, s.is_approved " +
                "FROM sitin_sessions s " +
                "JOIN students st ON s.student_id = st.student_id " +
                "WHERE s.end_time IS NULL",
                connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                sessions.Add(ReadSession(reader));
            }
            return sessions;
        }

        public SitInSession GetActiveSessionByStudent(string studentId)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();
            using var command = new MySqlCommand(
                "SELECT s.session_id, s.student_id, CONCAT(st.first_name, ' ', st.last_name) AS student_name, " +
                "s.subject_name, s.start_time, s.end_time, s.is_scheduled, s.early_ended, " +
                "s.requires_approval, s.is_approved " +
                "FROM sitin_sessions s " +
                "JOIN students st ON s.student_id = st.student_id " +
                "WHERE s.end_time IS NULL AND s.student_id = @studentId",
                connection);
            command.Parameters.AddWithValue("@studentId", studentId);
            using var reader = command.ExecuteReader();
            return reader.Read() ? ReadSession(reader) : null;
        }

        public List<SitInSession> GetHistory(DateTime? from, DateTime? to, string studentId, string subject)
        {
            var sessions = new List<SitInSession>();
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();

            var sql = "SELECT s.session_id, s.student_id, CONCAT(st.first_name, ' ', st.last_name) AS student_name, " +
                      "s.subject_name, s.start_time, s.end_time, s.is_scheduled, s.early_ended, " +
                      "s.requires_approval, s.is_approved " +
                      "FROM sitin_sessions s " +
                      "JOIN students st ON s.student_id = st.student_id " +
                      "WHERE s.end_time IS NOT NULL";

            var parameters = new List<MySqlParameter>();

            if (from.HasValue)
            {
                sql += " AND s.start_time >= @from";
                parameters.Add(new MySqlParameter("@from", from.Value));
            }
            if (to.HasValue)
            {
                sql += " AND s.start_time <= @to";
                parameters.Add(new MySqlParameter("@to", to.Value));
            }
            if (!string.IsNullOrEmpty(studentId))
            {
                sql += " AND s.student_id LIKE @studentId";
                parameters.Add(new MySqlParameter("@studentId", $"%{studentId}%"));
            }
            if (!string.IsNullOrEmpty(subject))
            {
                sql += " AND s.subject_name LIKE @subject";
                parameters.Add(new MySqlParameter("@subject", $"%{subject}%"));
            }

            sql += " ORDER BY s.start_time DESC";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddRange(parameters.ToArray());
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                sessions.Add(ReadSession(reader));
            }
            return sessions;
        }

        public void StartSession(SitInSession session)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();
            using var command = new MySqlCommand(
                "INSERT INTO sitin_sessions (student_id, subject_name, start_time, is_scheduled, requires_approval, is_approved) " +
                "VALUES (@studentId, @subjectName, @startTime, @isScheduled, @requiresApproval, @isApproved)",
                connection);
            command.Parameters.AddWithValue("@studentId", session.StudentId);
            command.Parameters.AddWithValue("@subjectName", session.SubjectName);
            command.Parameters.AddWithValue("@startTime", session.StartTime);
            command.Parameters.AddWithValue("@isScheduled", session.IsScheduled);
            command.Parameters.AddWithValue("@requiresApproval", session.RequiresApproval);
            command.Parameters.AddWithValue("@isApproved", session.IsApproved);
            command.ExecuteNonQuery();
        }

        public void EndSession(int sessionId, DateTime endTime)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();
            using var command = new MySqlCommand(
                "UPDATE sitin_sessions SET end_time = @endTime WHERE session_id = @sessionId",
                connection);
            command.Parameters.AddWithValue("@endTime", endTime);
            command.Parameters.AddWithValue("@sessionId", sessionId);
            command.ExecuteNonQuery();
        }

        public int GetActiveSessionCount()
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();
            using var command = new MySqlCommand(
                "SELECT COUNT(*) FROM sitin_sessions WHERE end_time IS NULL",
                connection);
            return Convert.ToInt32(command.ExecuteScalar());
        }

        public int GetStudentSitInCount(string studentId)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();
            using var command = new MySqlCommand(
                "SELECT COUNT(*) FROM sitin_sessions WHERE student_id = @studentId",
                connection);
            command.Parameters.AddWithValue("@studentId", studentId);
            return Convert.ToInt32(command.ExecuteScalar());
        }

        public List<SitInSession> GetStudentRecentHistory(string studentId, int limit)
        {
            var sessions = new List<SitInSession>();
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();
            using var command = new MySqlCommand(
                "SELECT s.session_id, s.student_id, CONCAT(st.first_name, ' ', st.last_name) AS student_name, " +
                "s.subject_name, s.start_time, s.end_time, s.is_scheduled, s.early_ended, " +
                "s.requires_approval, s.is_approved " +
                "FROM sitin_sessions s " +
                "JOIN students st ON s.student_id = st.student_id " +
                "WHERE s.student_id = @studentId " +
                "ORDER BY s.start_time DESC LIMIT @limit",
                connection);
            command.Parameters.AddWithValue("@studentId", studentId);
            command.Parameters.AddWithValue("@limit", limit);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                sessions.Add(ReadSession(reader));
            }
            return sessions;
        }

        public void EndSessionEarly(int sessionId, DateTime endTime)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();
            using var command = new MySqlCommand(
                "UPDATE sitin_sessions SET end_time = @endTime, early_ended = TRUE WHERE session_id = @sessionId",
                connection);
            command.Parameters.AddWithValue("@endTime", endTime);
            command.Parameters.AddWithValue("@sessionId", sessionId);
            command.ExecuteNonQuery();
        }

        public void ApproveSession(int sessionId)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();
            using var command = new MySqlCommand(
                "UPDATE sitin_sessions SET is_approved = TRUE, requires_approval = FALSE WHERE session_id = @sessionId",
                connection);
            command.Parameters.AddWithValue("@sessionId", sessionId);
            command.ExecuteNonQuery();
        }

        private static SitInSession ReadSession(MySqlDataReader reader)
        {
            return new SitInSession
            {
                SessionId = reader.GetInt32("session_id"),
                StudentId = reader.GetString("student_id"),
                StudentName = reader.GetString("student_name"),
                SubjectName = reader.IsDBNull(reader.GetOrdinal("subject_name")) ? null : reader.GetString("subject_name"),
                StartTime = reader.GetDateTime("start_time"),
                EndTime = reader.IsDBNull(reader.GetOrdinal("end_time")) ? null : reader.GetDateTime("end_time"),
                IsScheduled = reader.GetBoolean("is_scheduled"),
                EarlyEnded = !reader.IsDBNull(reader.GetOrdinal("early_ended")) && reader.GetBoolean("early_ended"),
                RequiresApproval = !reader.IsDBNull(reader.GetOrdinal("requires_approval")) && reader.GetBoolean("requires_approval"),
                IsApproved = !reader.IsDBNull(reader.GetOrdinal("is_approved")) && reader.GetBoolean("is_approved")
            };
        }
    }
}
