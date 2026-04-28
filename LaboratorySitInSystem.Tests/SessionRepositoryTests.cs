using LaboratorySitInSystem.DataAccess;
using LaboratorySitInSystem.Models;
using Moq;
using Xunit;

namespace LaboratorySitInSystem.Tests
{
    public class SessionRepositoryTests
    {
        private readonly Mock<ISessionRepository> _mockRepo;

        public SessionRepositoryTests()
        {
            _mockRepo = new Mock<ISessionRepository>();
        }

        [Fact]
        public void GetActiveSessions_ReturnsOnlySessionsWithNullEndTime()
        {
            var activeSessions = new List<SitInSession>
            {
                new SitInSession { SessionId = 1, StudentId = "S001", StudentName = "John Doe", SubjectName = "Math", StartTime = DateTime.Now.AddHours(-1), EndTime = null, IsScheduled = true },
                new SitInSession { SessionId = 2, StudentId = "S002", StudentName = "Jane Smith", SubjectName = "Science", StartTime = DateTime.Now.AddMinutes(-30), EndTime = null, IsScheduled = false }
            };
            _mockRepo.Setup(r => r.GetActiveSessions()).Returns(activeSessions);

            var result = _mockRepo.Object.GetActiveSessions();

            Assert.Equal(2, result.Count);
            Assert.All(result, s => Assert.Null(s.EndTime));
        }

        [Fact]
        public void GetActiveSessions_WhenNoActiveSessions_ReturnsEmptyList()
        {
            _mockRepo.Setup(r => r.GetActiveSessions()).Returns(new List<SitInSession>());

            var result = _mockRepo.Object.GetActiveSessions();

            Assert.Empty(result);
        }

        [Fact]
        public void GetActiveSessions_DoesNotReturnEndedSessions()
        {
            // Setup: only active sessions (null EndTime) are returned
            var activeSessions = new List<SitInSession>
            {
                new SitInSession { SessionId = 1, StudentId = "S001", StudentName = "John Doe", SubjectName = "Math", StartTime = DateTime.Now.AddHours(-1), EndTime = null, IsScheduled = true }
            };
            _mockRepo.Setup(r => r.GetActiveSessions()).Returns(activeSessions);

            var result = _mockRepo.Object.GetActiveSessions();

            // Ended session (SessionId=2) should not appear
            Assert.Single(result);
            Assert.Equal(1, result[0].SessionId);
            Assert.Null(result[0].EndTime);
        }

        [Fact]
        public void GetActiveSessionByStudent_ActiveSession_ReturnsSession()
        {
            var session = new SitInSession { SessionId = 1, StudentId = "S001", StudentName = "John Doe", SubjectName = "Math", StartTime = DateTime.Now.AddHours(-1), EndTime = null, IsScheduled = true };
            _mockRepo.Setup(r => r.GetActiveSessionByStudent("S001")).Returns(session);

            var result = _mockRepo.Object.GetActiveSessionByStudent("S001");

            Assert.NotNull(result);
            Assert.Equal("S001", result.StudentId);
            Assert.Null(result.EndTime);
        }

        [Fact]
        public void GetActiveSessionByStudent_NoActiveSession_ReturnsNull()
        {
            _mockRepo.Setup(r => r.GetActiveSessionByStudent("S001")).Returns((SitInSession?)null!);

            var result = _mockRepo.Object.GetActiveSessionByStudent("S001");

            Assert.Null(result);
        }

        [Fact]
        public void StartSession_CallsRepositoryWithSession()
        {
            var session = new SitInSession { StudentId = "S001", SubjectName = "Math", StartTime = DateTime.Now, IsScheduled = true };

            _mockRepo.Object.StartSession(session);

            _mockRepo.Verify(r => r.StartSession(It.Is<SitInSession>(s => s.StudentId == "S001" && s.SubjectName == "Math")), Times.Once);
        }

        [Fact]
        public void EndSession_CallsRepositoryWithCorrectParameters()
        {
            var endTime = DateTime.Now;

            _mockRepo.Object.EndSession(1, endTime);

            _mockRepo.Verify(r => r.EndSession(1, endTime), Times.Once);
        }

        [Fact]
        public void GetActiveSessionCount_ReturnsCorrectCount()
        {
            _mockRepo.Setup(r => r.GetActiveSessionCount()).Returns(5);

            var result = _mockRepo.Object.GetActiveSessionCount();

            Assert.Equal(5, result);
        }
    }
}
