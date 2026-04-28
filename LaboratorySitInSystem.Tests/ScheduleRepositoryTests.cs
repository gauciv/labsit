using LaboratorySitInSystem.DataAccess;
using LaboratorySitInSystem.Models;
using Moq;
using Xunit;

namespace LaboratorySitInSystem.Tests
{
    public class ScheduleRepositoryTests
    {
        private readonly Mock<IScheduleRepository> _mockRepo;

        public ScheduleRepositoryTests()
        {
            _mockRepo = new Mock<IScheduleRepository>();
        }

        [Fact]
        public void HasOverlap_OverlappingTimeRanges_ReturnsTrue()
        {
            // Existing schedule: 08:00 - 10:00 on Monday
            // New schedule:      09:00 - 11:00 on Monday (overlaps)
            _mockRepo.Setup(r => r.HasOverlap(
                "S001",
                DayOfWeek.Monday,
                new TimeSpan(9, 0, 0),
                new TimeSpan(11, 0, 0),
                null
            )).Returns(true);

            var result = _mockRepo.Object.HasOverlap("S001", DayOfWeek.Monday, new TimeSpan(9, 0, 0), new TimeSpan(11, 0, 0));

            Assert.True(result);
        }

        [Fact]
        public void HasOverlap_NonOverlappingTimeRanges_ReturnsFalse()
        {
            // Existing schedule: 08:00 - 10:00 on Monday
            // New schedule:      10:00 - 12:00 on Monday (no overlap, adjacent)
            _mockRepo.Setup(r => r.HasOverlap(
                "S001",
                DayOfWeek.Monday,
                new TimeSpan(10, 0, 0),
                new TimeSpan(12, 0, 0),
                null
            )).Returns(false);

            var result = _mockRepo.Object.HasOverlap("S001", DayOfWeek.Monday, new TimeSpan(10, 0, 0), new TimeSpan(12, 0, 0));

            Assert.False(result);
        }

        [Fact]
        public void HasOverlap_CompletelyContainedRange_ReturnsTrue()
        {
            // Existing schedule: 08:00 - 12:00 on Tuesday
            // New schedule:      09:00 - 11:00 on Tuesday (fully inside)
            _mockRepo.Setup(r => r.HasOverlap(
                "S001",
                DayOfWeek.Tuesday,
                new TimeSpan(9, 0, 0),
                new TimeSpan(11, 0, 0),
                null
            )).Returns(true);

            var result = _mockRepo.Object.HasOverlap("S001", DayOfWeek.Tuesday, new TimeSpan(9, 0, 0), new TimeSpan(11, 0, 0));

            Assert.True(result);
        }

        [Fact]
        public void HasOverlap_DifferentDay_ReturnsFalse()
        {
            // Existing schedule: 08:00 - 10:00 on Monday
            // New schedule:      08:00 - 10:00 on Tuesday (different day)
            _mockRepo.Setup(r => r.HasOverlap(
                "S001",
                DayOfWeek.Tuesday,
                new TimeSpan(8, 0, 0),
                new TimeSpan(10, 0, 0),
                null
            )).Returns(false);

            var result = _mockRepo.Object.HasOverlap("S001", DayOfWeek.Tuesday, new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0));

            Assert.False(result);
        }

        [Fact]
        public void HasOverlap_WithExcludeId_ExcludesSpecifiedSchedule()
        {
            // When updating schedule ID 5, exclude it from overlap check
            _mockRepo.Setup(r => r.HasOverlap(
                "S001",
                DayOfWeek.Monday,
                new TimeSpan(8, 0, 0),
                new TimeSpan(10, 0, 0),
                5
            )).Returns(false);

            var result = _mockRepo.Object.HasOverlap("S001", DayOfWeek.Monday, new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0), 5);

            Assert.False(result);
        }

        [Fact]
        public void GetByStudentId_ReturnsSchedulesForStudent()
        {
            var schedules = new List<ClassSchedule>
            {
                new ClassSchedule { ScheduleId = 1, StudentId = "S001", SubjectName = "Math", DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(10, 0, 0) },
                new ClassSchedule { ScheduleId = 2, StudentId = "S001", SubjectName = "Science", DayOfWeek = DayOfWeek.Wednesday, StartTime = new TimeSpan(13, 0, 0), EndTime = new TimeSpan(15, 0, 0) }
            };
            _mockRepo.Setup(r => r.GetByStudentId("S001")).Returns(schedules);

            var result = _mockRepo.Object.GetByStudentId("S001");

            Assert.Equal(2, result.Count);
            Assert.All(result, s => Assert.Equal("S001", s.StudentId));
        }
    }
}
