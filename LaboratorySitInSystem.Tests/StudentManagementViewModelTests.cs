using System.Collections.Generic;
using LaboratorySitInSystem.DataAccess;
using LaboratorySitInSystem.Models;
using LaboratorySitInSystem.ViewModels;
using Moq;
using Xunit;

namespace LaboratorySitInSystem.Tests
{
    public class StudentManagementViewModelTests
    {
        private readonly Mock<IStudentRepository> _mockStudentRepo;

        public StudentManagementViewModelTests()
        {
            _mockStudentRepo = new Mock<IStudentRepository>();
            _mockStudentRepo.Setup(r => r.GetAll()).Returns(new List<Student>());
        }

        private StudentManagementViewModel CreateVm(List<Student> initialStudents = null)
        {
            if (initialStudents != null)
            {
                _mockStudentRepo.Setup(r => r.GetAll()).Returns(initialStudents);
            }
            return new StudentManagementViewModel(_mockStudentRepo.Object);
        }

        [Fact]
        public void Constructor_LoadsAllStudents()
        {
            var students = new List<Student>
            {
                new Student { StudentId = "S001", FirstName = "John", LastName = "Doe", Course = "BSCS", YearLevel = 2 },
                new Student { StudentId = "S002", FirstName = "Jane", LastName = "Smith", Course = "BSIT", YearLevel = 1 }
            };

            var vm = CreateVm(students);

            Assert.Equal(2, vm.Students.Count);
        }

        [Fact]
        public void AddCommand_WithValidFields_AddsStudentAndRefreshes()
        {
            var vm = CreateVm();
            _mockStudentRepo.Setup(r => r.GetAll()).Returns(new List<Student>
            {
                new Student { StudentId = "S001", FirstName = "John", LastName = "Doe", Course = "BSCS", YearLevel = 2 }
            });

            vm.EditStudentId = "S001";
            vm.EditFirstName = "John";
            vm.EditLastName = "Doe";
            vm.EditCourse = "BSCS";
            vm.EditYearLevel = 2;

            vm.AddCommand.Execute(null);

            _mockStudentRepo.Verify(r => r.Add(It.Is<Student>(s =>
                s.StudentId == "S001" &&
                s.FirstName == "John" &&
                s.LastName == "Doe" &&
                s.Course == "BSCS" &&
                s.YearLevel == 2
            )), Times.Once);
            Assert.Equal(1, vm.Students.Count);
            Assert.Contains("added successfully", vm.StatusMessage);
        }

        [Fact]
        public void AddCommand_WithMissingFields_ShowsError()
        {
            var vm = CreateVm();

            vm.EditStudentId = "S001";
            vm.EditFirstName = "";
            vm.EditLastName = "Doe";
            vm.EditCourse = "BSCS";

            vm.AddCommand.Execute(null);

            _mockStudentRepo.Verify(r => r.Add(It.IsAny<Student>()), Times.Never);
            Assert.Contains("fill in all required fields", vm.StatusMessage);
        }

        [Fact]
        public void UpdateCommand_WithSelectedStudent_UpdatesAndRefreshes()
        {
            var student = new Student { StudentId = "S001", FirstName = "John", LastName = "Doe", Course = "BSCS", YearLevel = 2 };
            var vm = CreateVm(new List<Student> { student });

            vm.SelectedStudent = student;
            vm.EditFirstName = "Johnny";
            vm.EditLastName = "Doe";
            vm.EditCourse = "BSIT";
            vm.EditYearLevel = 3;

            vm.UpdateCommand.Execute(null);

            _mockStudentRepo.Verify(r => r.Update(It.Is<Student>(s =>
                s.StudentId == "S001" &&
                s.FirstName == "Johnny" &&
                s.Course == "BSIT" &&
                s.YearLevel == 3
            )), Times.Once);
            Assert.Contains("updated successfully", vm.StatusMessage);
        }

        [Fact]
        public void UpdateCommand_WithNoSelection_ShowsError()
        {
            var vm = CreateVm();

            vm.UpdateCommand.Execute(null);

            _mockStudentRepo.Verify(r => r.Update(It.IsAny<Student>()), Times.Never);
            Assert.Contains("select a student to update", vm.StatusMessage);
        }

        [Fact]
        public void DeleteCommand_WithSelectedStudent_DeletesAndRefreshes()
        {
            var student = new Student { StudentId = "S001", FirstName = "John", LastName = "Doe", Course = "BSCS", YearLevel = 2 };
            var vm = CreateVm(new List<Student> { student });

            vm.SelectedStudent = student;
            vm.DeleteCommand.Execute(null);

            _mockStudentRepo.Verify(r => r.Delete("S001"), Times.Once);
            Assert.Contains("deleted successfully", vm.StatusMessage);
        }

        [Fact]
        public void DeleteCommand_WithNoSelection_ShowsError()
        {
            var vm = CreateVm();

            vm.DeleteCommand.Execute(null);

            _mockStudentRepo.Verify(r => r.Delete(It.IsAny<string>()), Times.Never);
            Assert.Contains("select a student to delete", vm.StatusMessage);
        }

        [Fact]
        public void SearchCommand_WithQuery_CallsSearchAndPopulatesList()
        {
            var vm = CreateVm();
            var results = new List<Student>
            {
                new Student { StudentId = "S001", FirstName = "John", LastName = "Doe", Course = "BSCS", YearLevel = 2 }
            };
            _mockStudentRepo.Setup(r => r.Search("John")).Returns(results);

            vm.SearchQuery = "John";
            vm.SearchCommand.Execute(null);

            Assert.Single(vm.Students);
            Assert.Equal("S001", vm.Students[0].StudentId);
        }

        [Fact]
        public void SearchCommand_WithEmptyQuery_LoadsAllStudents()
        {
            var allStudents = new List<Student>
            {
                new Student { StudentId = "S001", FirstName = "John", LastName = "Doe", Course = "BSCS", YearLevel = 2 },
                new Student { StudentId = "S002", FirstName = "Jane", LastName = "Smith", Course = "BSIT", YearLevel = 1 }
            };
            var vm = CreateVm();
            _mockStudentRepo.Setup(r => r.GetAll()).Returns(allStudents);

            vm.SearchQuery = "";
            vm.SearchCommand.Execute(null);

            Assert.Equal(2, vm.Students.Count);
        }

        [Fact]
        public void SelectedStudent_PopulatesFormFields()
        {
            var student = new Student { StudentId = "S001", FirstName = "John", LastName = "Doe", Course = "BSCS", YearLevel = 2 };
            var vm = CreateVm(new List<Student> { student });

            vm.SelectedStudent = student;

            Assert.Equal("S001", vm.EditStudentId);
            Assert.Equal("John", vm.EditFirstName);
            Assert.Equal("Doe", vm.EditLastName);
            Assert.Equal("BSCS", vm.EditCourse);
            Assert.Equal(2, vm.EditYearLevel);
        }

        [Fact]
        public void ClearFormCommand_ClearsAllFormFields()
        {
            var student = new Student { StudentId = "S001", FirstName = "John", LastName = "Doe", Course = "BSCS", YearLevel = 2 };
            var vm = CreateVm(new List<Student> { student });

            vm.SelectedStudent = student;
            vm.ClearFormCommand.Execute(null);

            Assert.Equal(string.Empty, vm.EditStudentId);
            Assert.Equal(string.Empty, vm.EditFirstName);
            Assert.Equal(string.Empty, vm.EditLastName);
            Assert.Equal(string.Empty, vm.EditCourse);
            Assert.Equal(0, vm.EditYearLevel);
            Assert.Null(vm.SelectedStudent);
            Assert.Equal(string.Empty, vm.StatusMessage);
        }

        [Fact]
        public void AddCommand_InvokesCallbackWhenProvided()
        {
            var callbackInvoked = false;
            var vm = new StudentManagementViewModel(_mockStudentRepo.Object, () => callbackInvoked = true);

            vm.EditStudentId = "S001";
            vm.EditFirstName = "John";
            vm.EditLastName = "Doe";
            vm.EditCourse = "BSCS";
            vm.EditYearLevel = 2;

            vm.AddCommand.Execute(null);

            Assert.True(callbackInvoked);
        }

        [Fact]
        public void DeleteCommand_InvokesCallbackWhenProvided()
        {
            var callbackInvoked = false;
            var student = new Student { StudentId = "S001", FirstName = "John", LastName = "Doe", Course = "BSCS", YearLevel = 2 };
            var vm = new StudentManagementViewModel(_mockStudentRepo.Object, () => callbackInvoked = true);
            _mockStudentRepo.Setup(r => r.GetAll()).Returns(new List<Student> { student });
            vm.LoadAllStudents();

            vm.SelectedStudent = student;
            vm.DeleteCommand.Execute(null);

            Assert.True(callbackInvoked);
        }
    }
}
