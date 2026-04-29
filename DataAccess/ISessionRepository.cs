using System;
using System.Collections.Generic;
using LaboratorySitInSystem.Models;

namespace LaboratorySitInSystem.DataAccess
{
    public interface ISessionRepository
    {
        List<SitInSession> GetActiveSessions();
        SitInSession GetActiveSessionByStudent(string studentId);
        List<SitInSession> GetHistory(DateTime? from, DateTime? to, string studentId, string subject);
        void StartSession(SitInSession session);
        void EndSession(int sessionId, DateTime endTime);
        int GetActiveSessionCount();
        int GetStudentSitInCount(string studentId);
        List<SitInSession> GetStudentRecentHistory(string studentId, int limit);
        void EndSessionEarly(int sessionId, DateTime endTime);
    }
}
