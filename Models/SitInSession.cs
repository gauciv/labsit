using System;

namespace LaboratorySitInSystem.Models
{
    public class SitInSession
    {
        public int SessionId { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string SubjectName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsScheduled { get; set; }
        public bool EarlyEnded { get; set; }
        public bool RequiresApproval { get; set; }
        public bool IsApproved { get; set; }
        public TimeSpan Duration => (EndTime ?? DateTime.Now) - StartTime;
    }
}
