using LaboratorySitInSystem.DataAccess;
using LaboratorySitInSystem.Models;
using Moq;
using Xunit;

namespace LaboratorySitInSystem.Tests
{
    public class StudentRepositoryTests
    {
        private readonly Mock<IStudentRepository> _mockRepo;

        public StudentRepositoryTests()
        {
            _mockRepo = new Mock<IStudentRepository>();
        }

        [Fact]
        public void GetAll_ReturnsListOfStudents()
        {
            var students = new List<Student>
            {
                new Student { StudentId = "S001", FirstName = "John", LastName = "Doe", Course = "BSCS", YearLevel = 2 },
                new Student { StudentId = "S002", FirstName = "Jane", LastName = "Smith", Course = "BSIT", YearLevel = 3 }
            };
            _mockRepo.Setup(r => r.GetAll()).Returns(students);

            var result = _mockRepo.Object.GetAll();

            Assert.Equal(2, result.Count);
            Assert.Equal("S001", result[0].StudentId);
            Assert.Equal("S002", result[1].StudentId);
        }

        [Fact]
        public void GetAll_WhenEmpty_ReturnsEmptyList()
        {
            _mockRepo.Setup(r => r.GetAll()).Returns(new List<Student>());

            var result = _mockRepo.Object.GetAll();

            Assert.Empty(result);
        }

        [Fact]
        public void GetById_ExistingStudent_ReturnsStudent()
        {
            var student = new Student { StudentId = "S001", FirstName = "John", LastName = "Doe", Course = "BSCS", YearLevel = 2 };
            _mockRepo.Setup(r => r.GetById("S001")).Returns(student);

            var result = _mockRepo.Object.GetById("S001");

            Assert.NotNull(result);
            Assert.Equal("S001", result.StudentId);
            Assert.Equal("John", result.FirstName);
        }

        [Fact]
        public void GetById_NonExistingStudent_ReturnsNull()
        {
            _mockRepo.Setup(r => r.GetById("INVALID")).Returns((Student?)null!);

            var result = _mockRepo.Object.GetById("INVALID");

            Assert.Null(result);
        }

        [Fact]
        public void Search_MatchingQuery_ReturnsFilteredStudents()
        {
            var allStudents = new List<Student>
            {
                new Student { StudentId = "S001", FirstName = "John", LastName = "Doe", Course = "BSCS", YearLevel = 2 },
                new Student { StudentId = "S002", FirstName = "Jane", LastName = "Smith", Course = "BSIT", YearLevel = 3 }
            };
            _mockRepo.Setup(r => r.Search("John")).Returns(allStudents.Where(s => s.FirstName.Contains("John")).ToList());

            var result = _mockRepo.Object.Search("John");

            Assert.Single(result);
            Assert.Equal("S001", result[0].StudentId);
        }

        [Fact]
        public void Search_NoMatch_ReturnsEmptyList()
        {
            _mockRepo.Setup(r => r.Search("NonExistent")).Returns(new List<Student>());

            var result = _mockRepo.Object.Search("NonExistent");

            Assert.Empty(result);
        }

        [Fact]
        public void Add_CallsRepositoryWithStudent()
        {
            var student = new Student { StudentId = "S003", FirstName = "Alice", LastName = "Brown", Course = "BSCS", YearLevel = 1 };

            _mockRepo.Object.Add(student);

            _mockRepo.Verify(r => r.Add(It.Is<Student>(s => s.StudentId == "S003" && s.FirstName == "Alice")), Times.Once);
        }

        [Fact]
        public void Update_CallsRepositoryWithStudent()
        {
            var student = new Student { StudentId = "S001", FirstName = "John", LastName = "Updated", Course = "BSCS", YearLevel = 3 };

            _mockRepo.Object.Update(student);

            _mockRepo.Verify(r => r.Update(It.Is<Student>(s => s.StudentId == "S001" && s.LastName == "Updated")), Times.Once);
        }

        [Fact]
        public void Delete_CallsRepositoryWithStudentId()
        {
            _mockRepo.Object.Delete("S001");

            _mockRepo.Verify(r => r.Delete("S001"), Times.Once);
        }
    }
}
