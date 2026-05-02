using System;

namespace LaboratorySitInSystem.ViewModels
{
    /// <summary>
    /// Event arguments for session change notifications
    /// </summary>
    public class SessionChangedEventArgs : EventArgs
    {
        public string StudentId { get; set; }
        public bool IsSessionStarted { get; set; }
    }

    /// <summary>
    /// Static event hub for session change notifications
    /// </summary>
    public static class SessionEventHub
    {
        public static event EventHandler<SessionChangedEventArgs> SessionChanged;

        public static void NotifySessionStarted(string studentId)
        {
            SessionChanged?.Invoke(null, new SessionChangedEventArgs 
            { 
                StudentId = studentId, 
                IsSessionStarted = true 
            });
        }

        public static void NotifySessionEnded(string studentId)
        {
            SessionChanged?.Invoke(null, new SessionChangedEventArgs 
            { 
                StudentId = studentId, 
                IsSessionStarted = false 
            });
        }
    }
}
