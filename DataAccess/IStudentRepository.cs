using System;
using System.Collections.Generic;
using LaboratorySitInSystem.Models;

namespace LaboratorySitInSystem.DataAccess
{
    public interface IStudentRepository
    {
        List<Student> GetAll();
        Student GetById(string studentId);
        List<Student> Search(string query);
        void Add(Student student);
        void Update(Student student);
        void Delete(string studentId);
    }
}
