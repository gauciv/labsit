using System;
using System.Collections.ObjectModel;
using LaboratorySitInSystem.DataAccess;
using LaboratorySitInSystem.Helpers;
using LaboratorySitInSystem.Models;

namespace LaboratorySitInSystem.ViewModels
{
    public class ScheduleManagementViewModel : ViewModelBase
    {
        private readonly IScheduleRepository _scheduleRepo;
        private readonly IStudentRepository _studentRepo;

        private ObservableCollection<Student> _students;
        private Student _selectedStudent;
        private ObservableCollection<ClassSchedule> _schedules;
        private ClassSchedule _selectedSchedule;
        private string _editSubjectName;
        private DayOfWeek _editDayOfWeek;
        private TimeSpan _editStartTime;
        private TimeSpan _editEndTime;
        private string _statusMessage;

        public ObservableCollection<Student> Students
        {
            get => _students;
            set => SetProperty(ref _students, value);
        }

        public bool IsStudentsEmpty => Students?.Count == 0;

        public Student SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                if (SetProperty(ref _selectedStudent, value))
                {
                    LoadSchedulesForSelectedStudent();
                }
            }
        }

        public ObservableCollection<ClassSchedule> Schedules
        {
            get => _schedules;
            set => SetProperty(ref _schedules, value);
        }

        public bool IsSchedulesEmpty => Schedules?.Count == 0;

        public ClassSchedule SelectedSchedule
        {
            get => _selectedSchedule;
            set
            {
                if (SetProperty(ref _selectedSchedule, value))
                {
                    PopulateFormFromSelectedSchedule();
                }
            }
        }

        public string EditSubjectName
        {
            get => _editSubjectName;
            set => SetProperty(ref _editSubjectName, value);
        }

        public DayOfWeek EditDayOfWeek
        {
            get => _editDayOfWeek;
            set => SetProperty(ref _editDayOfWeek, value);
        }

        public TimeSpan EditStartTime
        {
            get => _editStartTime;
            set => SetProperty(ref _editStartTime, value);
        }

        public TimeSpan EditEndTime
        {
            get => _editEndTime;
            set => SetProperty(ref _editEndTime, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public RelayCommand AddCommand { get; }
        public RelayCommand UpdateCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand ClearFormCommand { get; }

        public ScheduleManagementViewModel(IScheduleRepository scheduleRepo, IStudentRepository studentRepo)
        {
            _scheduleRepo = scheduleRepo ?? throw new ArgumentNullException(nameof(scheduleRepo));
            _studentRepo = studentRepo ?? throw new ArgumentNullException(nameof(studentRepo));

            Students = new ObservableCollection<Student>();
            Schedules = new ObservableCollection<ClassSchedule>();

            AddCommand = new RelayCommand(ExecuteAdd);
            UpdateCommand = new RelayCommand(ExecuteUpdate);
            DeleteCommand = new RelayCommand(ExecuteDelete);
            ClearFormCommand = new RelayCommand(ExecuteClearForm);

            LoadStudents();
        }

        private void LoadStudents()
        {
            Students.Clear();
            foreach (var student in _studentRepo.GetAll())
            {
                Students.Add(student);
            }
            OnPropertyChanged(nameof(IsStudentsEmpty));
        }

        private void LoadSchedulesForSelectedStudent()
        {
            Schedules.Clear();
            if (SelectedStudent != null)
            {
                foreach (var schedule in _scheduleRepo.GetByStudentId(SelectedStudent.StudentId))
                {
                    Schedules.Add(schedule);
                }
            }
            OnPropertyChanged(nameof(IsSchedulesEmpty));
        }

        private void ExecuteAdd(object parameter)
        {
            if (SelectedStudent == null)
            {
                StatusMessage = "Please select a student first.";
                return;
            }

            if (string.IsNullOrWhiteSpace(EditSubjectName))
            {
                StatusMessage = "Please fill in all required fields.";
                return;
            }

            if (EditStartTime >= EditEndTime)
            {
                StatusMessage = "Start time must be before end time.";
                return;
            }

            if (_scheduleRepo.HasOverlap(SelectedStudent.StudentId, EditDayOfWeek, EditStartTime, EditEndTime))
            {
                StatusMessage = "Schedule overlaps with an existing schedule.";
                return;
            }

            var schedule = new ClassSchedule
            {
                StudentId = SelectedStudent.StudentId,
                SubjectName = EditSubjectName,
                DayOfWeek = EditDayOfWeek,
                StartTime = EditStartTime,
                EndTime = EditEndTime
            };

            _scheduleRepo.Add(schedule);
            LoadSchedulesForSelectedStudent();
            ClearForm();
            StatusMessage = "Schedule added successfully.";
        }

        private void ExecuteUpdate(object parameter)
        {
            if (SelectedSchedule == null)
            {
                StatusMessage = "Please select a schedule to update.";
                return;
            }

            if (string.IsNullOrWhiteSpace(EditSubjectName))
            {
                StatusMessage = "Please fill in all required fields.";
                return;
            }

            if (EditStartTime >= EditEndTime)
            {
                StatusMessage = "Start time must be before end time.";
                return;
            }

            if (_scheduleRepo.HasOverlap(SelectedSchedule.StudentId, EditDayOfWeek, EditStartTime, EditEndTime, SelectedSchedule.ScheduleId))
            {
                StatusMessage = "Schedule overlaps with an existing schedule.";
                return;
            }

            SelectedSchedule.SubjectName = EditSubjectName;
            SelectedSchedule.DayOfWeek = EditDayOfWeek;
            SelectedSchedule.StartTime = EditStartTime;
            SelectedSchedule.EndTime = EditEndTime;

            _scheduleRepo.Update(SelectedSchedule);
            LoadSchedulesForSelectedStudent();
            StatusMessage = "Schedule updated successfully.";
        }

        private void ExecuteDelete(object parameter)
        {
            if (SelectedSchedule == null)
            {
                StatusMessage = "Please select a schedule to delete.";
                return;
            }

            _scheduleRepo.Delete(SelectedSchedule.ScheduleId);
            LoadSchedulesForSelectedStudent();
            ClearForm();
            StatusMessage = "Schedule deleted successfully.";
        }

        private void ExecuteClearForm(object parameter)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            EditSubjectName = string.Empty;
            EditDayOfWeek = DayOfWeek.Monday;
            EditStartTime = TimeSpan.Zero;
            EditEndTime = TimeSpan.Zero;
            SelectedSchedule = null;
            StatusMessage = string.Empty;
        }

        private void PopulateFormFromSelectedSchedule()
        {
            if (SelectedSchedule != null)
            {
                EditSubjectName = SelectedSchedule.SubjectName;
                EditDayOfWeek = SelectedSchedule.DayOfWeek;
                EditStartTime = SelectedSchedule.StartTime;
                EditEndTime = SelectedSchedule.EndTime;
            }
        }
    }
}
