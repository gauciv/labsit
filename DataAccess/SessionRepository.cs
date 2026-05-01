using System;
using System.Collections.Generic;
using LaboratorySitInSystem.Models;
using MySql.Data.MySqlClient;

namespace LaboratorySitInSystem.DataAccess
{
    public class SessionRepository : ISessionRepository
    {
        public List<SitInSession> GetActiveSessions()
        {
            var sessions = new List<SitInSession>();
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();
            using var command = new MySqlCommand(
                "SELECT s.session_id, s.student_id, CONCAT(st.first_name, ' ', st.last_name) AS student_name, " +
                "s.subject_name, s.start_time, s.end_time, s.is_scheduled, s.early_ended " +
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
                "s.subject_name, s.start_time, s.end_time, s.is_scheduled, s.early_ended " +
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
                      "s.subject_name, s.start_time, s.end_time, s.is_scheduled, s.early_ended " +
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
                "INSERT INTO sitin_sessions (student_id, subject_name, start_time, is_scheduled) " +
                "VALUES (@studentId, @subjectName, @startTime, @isScheduled)",
                connection);
            command.Parameters.AddWithValue("@studentId", session.StudentId);
            command.Parameters.AddWithValue("@subjectName", session.SubjectName);
            command.Parameters.AddWithValue("@startTime", session.StartTime);
            command.Parameters.AddWithValue("@isScheduled", session.IsScheduled);
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
                "s.subject_name, s.start_time, s.end_time, s.is_scheduled, s.early_ended " +
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
                EarlyEnded = !reader.IsDBNull(reader.GetOrdinal("early_ended")) && reader.GetBoolean("early_ended")
            };
        }
    }
}
