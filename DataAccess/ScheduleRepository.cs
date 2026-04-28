using System;
using System.Collections.Generic;
using LaboratorySitInSystem.Models;
using MySql.Data.MySqlClient;

namespace LaboratorySitInSystem.DataAccess
{
    public class ScheduleRepository : IScheduleRepository
    {
        public List<ClassSchedule> GetByStudentId(string studentId)
        {
            var schedules = new List<ClassSchedule>();
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();
            using var command = new MySqlCommand(
                "SELECT schedule_id, student_id, subject_name, day_of_week, start_time, end_time FROM class_schedules WHERE student_id = @studentId",
                connection);
            command.Parameters.AddWithValue("@studentId", studentId);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                schedules.Add(ReadSchedule(reader));
            }
            return schedules;
        }

        public ClassSchedule GetActiveSchedule(string studentId, DayOfWeek day, TimeSpan currentTime)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();
            using var command = new MySqlCommand(
                "SELECT schedule_id, student_id, subject_name, day_of_week, start_time, end_time FROM class_schedules " +
                "WHERE student_id = @studentId AND day_of_week = @day AND start_time <= @currentTime AND end_time > @currentTime",
                connection);
            command.Parameters.AddWithValue("@studentId", studentId);
            command.Parameters.AddWithValue("@day", (int)day);
            command.Parameters.AddWithValue("@currentTime", currentTime);
            using var reader = command.ExecuteReader();
            return reader.Read() ? ReadSchedule(reader) : null;
        }

        public void Add(ClassSchedule schedule)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();
            using var command = new MySqlCommand(
                "INSERT INTO class_schedules (student_id, subject_name, day_of_week, start_time, end_time) VALUES (@studentId, @subjectName, @dayOfWeek, @startTime, @endTime)",
                connection);
            command.Parameters.AddWithValue("@studentId", schedule.StudentId);
            command.Parameters.AddWithValue("@subjectName", schedule.SubjectName);
            command.Parameters.AddWithValue("@dayOfWeek", (int)schedule.DayOfWeek);
            command.Parameters.AddWithValue("@startTime", schedule.StartTime);
            command.Parameters.AddWithValue("@endTime", schedule.EndTime);
            command.ExecuteNonQuery();
        }

        public void Update(ClassSchedule schedule)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();
            using var command = new MySqlCommand(
                "UPDATE class_schedules SET student_id = @studentId, subject_name = @subjectName, day_of_week = @dayOfWeek, start_time = @startTime, end_time = @endTime WHERE schedule_id = @scheduleId",
                connection);
            command.Parameters.AddWithValue("@scheduleId", schedule.ScheduleId);
            command.Parameters.AddWithValue("@studentId", schedule.StudentId);
            command.Parameters.AddWithValue("@subjectName", schedule.SubjectName);
            command.Parameters.AddWithValue("@dayOfWeek", (int)schedule.DayOfWeek);
            command.Parameters.AddWithValue("@startTime", schedule.StartTime);
            command.Parameters.AddWithValue("@endTime", schedule.EndTime);
            command.ExecuteNonQuery();
        }

        public void Delete(int scheduleId)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();
            using var command = new MySqlCommand("DELETE FROM class_schedules WHERE schedule_id = @scheduleId", connection);
            command.Parameters.AddWithValue("@scheduleId", scheduleId);
            command.ExecuteNonQuery();
        }

        public bool HasOverlap(string studentId, DayOfWeek day, TimeSpan start, TimeSpan end, int? excludeId = null)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();
            var sql = "SELECT COUNT(*) FROM class_schedules WHERE student_id = @studentId AND day_of_week = @day AND start_time < @end AND end_time > @start";
            if (excludeId.HasValue)
            {
                sql += " AND schedule_id != @excludeId";
            }
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@studentId", studentId);
            command.Parameters.AddWithValue("@day", (int)day);
            command.Parameters.AddWithValue("@start", start);
            command.Parameters.AddWithValue("@end", end);
            if (excludeId.HasValue)
            {
                command.Parameters.AddWithValue("@excludeId", excludeId.Value);
            }
            var count = Convert.ToInt32(command.ExecuteScalar());
            return count > 0;
        }

        private static ClassSchedule ReadSchedule(MySqlDataReader reader)
        {
            return new ClassSchedule
            {
                ScheduleId = reader.GetInt32("schedule_id"),
                StudentId = reader.GetString("student_id"),
                SubjectName = reader.GetString("subject_name"),
                DayOfWeek = (DayOfWeek)reader.GetInt32("day_of_week"),
                StartTime = reader.GetTimeSpan("start_time"),
                EndTime = reader.GetTimeSpan("end_time")
            };
        }
    }
}
