using System;
using System.Collections.ObjectModel;
using LaboratorySitInSystem.DataAccess;
using LaboratorySitInSystem.Helpers;
using LaboratorySitInSystem.Models;

namespace LaboratorySitInSystem.ViewModels
{
    public class SitInHistoryViewModel : ViewModelBase
    {
        private readonly ISessionRepository _sessionRepo;

        private DateTime? _fromDate;
        private DateTime? _toDate;
        private string _studentIdFilter;
        private string _subjectFilter;
        private string _statusMessage;

        public ObservableCollection<SitInSession> Sessions { get; }

        public DateTime? FromDate
        {
            get => _fromDate;
            set => SetProperty(ref _fromDate, value);
        }

        public DateTime? ToDate
        {
            get => _toDate;
            set => SetProperty(ref _toDate, value);
        }

        public string StudentIdFilter
        {
            get => _studentIdFilter;
            set => SetProperty(ref _studentIdFilter, value);
        }

        public string SubjectFilter
        {
            get => _subjectFilter;
            set => SetProperty(ref _subjectFilter, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public RelayCommand SearchCommand { get; }
        public RelayCommand ClearFiltersCommand { get; }

        public SitInHistoryViewModel(ISessionRepository sessionRepo)
        {
            _sessionRepo = sessionRepo ?? throw new ArgumentNullException(nameof(sessionRepo));

            Sessions = new ObservableCollection<SitInSession>();
            SearchCommand = new RelayCommand(ExecuteSearch);
            ClearFiltersCommand = new RelayCommand(ExecuteClearFilters);

            LoadHistory();
        }

        private void ExecuteSearch(object parameter)
        {
            LoadHistory();
        }

        private void ExecuteClearFilters(object parameter)
        {
            FromDate = null;
            ToDate = null;
            StudentIdFilter = null;
            SubjectFilter = null;
            LoadHistory();
        }

        private void LoadHistory()
        {
            var history = _sessionRepo.GetHistory(FromDate, ToDate, StudentIdFilter, SubjectFilter);
            Sessions.Clear();
            foreach (var session in history)
            {
                Sessions.Add(session);
            }
            StatusMessage = $"{Sessions.Count} record(s) found.";
        }
    }
}
