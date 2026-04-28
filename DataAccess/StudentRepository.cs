using System;
using System.Collections.Generic;
using LaboratorySitInSystem.Models;
using MySql.Data.MySqlClient;

namespace LaboratorySitInSystem.DataAccess
{
    public class StudentRepository : IStudentRepository
    {
        public List<Student> GetAll()
        {
            var students = new List<Student>();
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();
            using var command = new MySqlCommand("SELECT student_id, first_name, last_name, course, year_level FROM students", connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                students.Add(ReadStudent(reader));
            }
            return students;
        }

        public Student GetById(string studentId)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();
            using var command = new MySqlCommand("SELECT student_id, first_name, last_name, course, year_level FROM students WHERE student_id = @studentId", connection);
            command.Parameters.AddWithValue("@studentId", studentId);
            using var reader = command.ExecuteReader();
            return reader.Read() ? ReadStudent(reader) : null;
        }

        public List<Student> Search(string query)
        {
            var students = new List<Student>();
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();
            using var command = new MySqlCommand(
                "SELECT student_id, first_name, last_name, course, year_level FROM students " +
                "WHERE student_id LIKE @query OR first_name LIKE @query OR last_name LIKE @query OR course LIKE @query",
                connection);
            command.Parameters.AddWithValue("@query", $"%{query}%");
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                students.Add(ReadStudent(reader));
            }
            return students;
        }

        public void Add(Student student)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();
            using var command = new MySqlCommand(
                "INSERT INTO students (student_id, first_name, last_name, course, year_level) VALUES (@studentId, @firstName, @lastName, @course, @yearLevel)",
                connection);
            command.Parameters.AddWithValue("@studentId", student.StudentId);
            command.Parameters.AddWithValue("@firstName", student.FirstName);
            command.Parameters.AddWithValue("@lastName", student.LastName);
            command.Parameters.AddWithValue("@course", student.Course);
            command.Parameters.AddWithValue("@yearLevel", student.YearLevel);
            command.ExecuteNonQuery();
        }

        public void Update(Student student)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();
            using var command = new MySqlCommand(
                "UPDATE students SET first_name = @firstName, last_name = @lastName, course = @course, year_level = @yearLevel WHERE student_id = @studentId",
                connection);
            command.Parameters.AddWithValue("@studentId", student.StudentId);
            command.Parameters.AddWithValue("@firstName", student.FirstName);
            command.Parameters.AddWithValue("@lastName", student.LastName);
            command.Parameters.AddWithValue("@course", student.Course);
            command.Parameters.AddWithValue("@yearLevel", student.YearLevel);
            command.ExecuteNonQuery();
        }

        public void Delete(string studentId)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();
            using var command = new MySqlCommand("DELETE FROM students WHERE student_id = @studentId", connection);
            command.Parameters.AddWithValue("@studentId", studentId);
            command.ExecuteNonQuery();
        }

        private static Student ReadStudent(MySqlDataReader reader)
        {
            return new Student
            {
                StudentId = reader.GetString("student_id"),
                FirstName = reader.GetString("first_name"),
                LastName = reader.GetString("last_name"),
                Course = reader.GetString("course"),
                YearLevel = reader.GetInt32("year_level")
            };
        }
    }
}
