using System;

namespace LaboratorySitInSystem.Models
{
    public class ClassSchedule
    {
        public int ScheduleId { get; set; }
        public string StudentId { get; set; }
        public string SubjectName { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
