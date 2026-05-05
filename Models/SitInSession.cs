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
        public string Status { get; set; } // "pending", "approved", "rejected"
        public TimeSpan Duration => (EndTime ?? DateTime.Now) - StartTime;

        public bool IsPending => Status == "pending";
        public bool IsApproved => Status == "approved";
    }
}
