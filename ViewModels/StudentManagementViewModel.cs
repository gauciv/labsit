using System;
using System.Collections.ObjectModel;
using LaboratorySitInSystem.DataAccess;
using LaboratorySitInSystem.Helpers;
using LaboratorySitInSystem.Models;

namespace LaboratorySitInSystem.ViewModels
{
    public class StudentManagementViewModel : ViewModelBase
    {
        private readonly IStudentRepository _studentRepo;

        private ObservableCollection<Student> _students;
        private Student _selectedStudent;
        private string _searchQuery;
        private string _editStudentId;
        private string _editFirstName;
        private string _editLastName;
        private string _editCourse;
        private int _editYearLevel;
        private string _statusMessage;

        public ObservableCollection<Student> Students
        {
            get => _students;
            set => SetProperty(ref _students, value);
        }

        public bool IsEmpty => Students.Count == 0;

        public Student SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                if (SetProperty(ref _selectedStudent, value))
                {
                    PopulateFormFromSelectedStudent();
                }
            }
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set => SetProperty(ref _searchQuery, value);
        }

        public string EditStudentId
        {
            get => _editStudentId;
            set => SetProperty(ref _editStudentId, value);
        }

        public string EditFirstName
        {
            get => _editFirstName;
            set => SetProperty(ref _editFirstName, value);
        }

        public string EditLastName
        {
            get => _editLastName;
            set => SetProperty(ref _editLastName, value);
        }

        public string EditCourse
        {
            get => _editCourse;
            set => SetProperty(ref _editCourse, value);
        }

        public int EditYearLevel
        {
            get => _editYearLevel;
            set => SetProperty(ref _editYearLevel, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public RelayCommand AddCommand { get; }
        public RelayCommand UpdateCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand SearchCommand { get; }
        public RelayCommand ClearFormCommand { get; }

        public StudentManagementViewModel(IStudentRepository studentRepo)
        {
            _studentRepo = studentRepo ?? throw new ArgumentNullException(nameof(studentRepo));

            Students = new ObservableCollection<Student>();

            AddCommand = new RelayCommand(ExecuteAdd);
            UpdateCommand = new RelayCommand(ExecuteUpdate);
            DeleteCommand = new RelayCommand(ExecuteDelete);
            SearchCommand = new RelayCommand(ExecuteSearch);
            ClearFormCommand = new RelayCommand(ExecuteClearForm);

            LoadAllStudents();
        }

        private void LoadAllStudents()
        {
            Students.Clear();
            foreach (var student in _studentRepo.GetAll())
            {
                Students.Add(student);
            }
            OnPropertyChanged(nameof(IsEmpty));
        }

        private void ExecuteAdd(object parameter)
        {
            if (string.IsNullOrWhiteSpace(EditStudentId) ||
                string.IsNullOrWhiteSpace(EditFirstName) ||
                string.IsNullOrWhiteSpace(EditLastName) ||
                string.IsNullOrWhiteSpace(EditCourse))
            {
                StatusMessage = "Please fill in all required fields.";
                return;
            }

            if (EditYearLevel <= 0)
            {
                StatusMessage = "Year level must be greater than zero.";
                return;
            }

            var student = new Student
            {
                StudentId = EditStudentId,
                FirstName = EditFirstName,
                LastName = EditLastName,
                Course = EditCourse,
                YearLevel = EditYearLevel
            };

            _studentRepo.Add(student);
            LoadAllStudents();
            ClearForm();
            StatusMessage = "Student added successfully.";
        }

        private void ExecuteUpdate(object parameter)
        {
            if (SelectedStudent == null)
            {
                StatusMessage = "Please select a student to update.";
                return;
            }

            if (EditYearLevel <= 0)
            {
                StatusMessage = "Year level must be greater than zero.";
                return;
            }

            SelectedStudent.FirstName = EditFirstName;
            SelectedStudent.LastName = EditLastName;
            SelectedStudent.Course = EditCourse;
            SelectedStudent.YearLevel = EditYearLevel;

            _studentRepo.Update(SelectedStudent);
            LoadAllStudents();
            StatusMessage = "Student updated successfully.";
        }

        private void ExecuteDelete(object parameter)
        {
            if (SelectedStudent == null)
            {
                StatusMessage = "Please select a student to delete.";
                return;
            }

            _studentRepo.Delete(SelectedStudent.StudentId);
            LoadAllStudents();
            ClearForm();
            StatusMessage = "Student deleted successfully.";
        }

        private void ExecuteSearch(object parameter)
        {
            Students.Clear();

            var results = string.IsNullOrWhiteSpace(SearchQuery)
                ? _studentRepo.GetAll()
                : _studentRepo.Search(SearchQuery);

            foreach (var student in results)
            {
                Students.Add(student);
            }
            OnPropertyChanged(nameof(IsEmpty));
        }

        private void ExecuteClearForm(object parameter)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            EditStudentId = string.Empty;
            EditFirstName = string.Empty;
            EditLastName = string.Empty;
            EditCourse = string.Empty;
            EditYearLevel = 0;
            SelectedStudent = null;
            StatusMessage = string.Empty;
        }

        private void PopulateFormFromSelectedStudent()
        {
            if (SelectedStudent != null)
            {
                EditStudentId = SelectedStudent.StudentId;
                EditFirstName = SelectedStudent.FirstName;
                EditLastName = SelectedStudent.LastName;
                EditCourse = SelectedStudent.Course;
                EditYearLevel = SelectedStudent.YearLevel;
            }
        }
    }
}
