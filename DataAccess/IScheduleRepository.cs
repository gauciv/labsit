using System;
using System.Collections.Generic;
using LaboratorySitInSystem.Models;

namespace LaboratorySitInSystem.DataAccess
{
    public interface IScheduleRepository
    {
        List<ClassSchedule> GetByStudentId(string studentId);
        ClassSchedule GetActiveSchedule(string studentId, DayOfWeek day, TimeSpan currentTime);
        void Add(ClassSchedule schedule);
        void Update(ClassSchedule schedule);
        void Delete(int scheduleId);
        bool HasOverlap(string studentId, DayOfWeek day, TimeSpan start, TimeSpan end, int? excludeId = null);
        List<ClassSchedule> GetTodaySchedules(string studentId, DayOfWeek today);
    }
}
