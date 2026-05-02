using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using LaboratorySitInSystem.Helpers;

namespace LaboratorySitInSystem.ViewModels
{
    public class TeamMember : ViewModelBase
    {
        private bool _isSelected;

        public string Name { get; set; }
        public string Role { get; set; }
        public string Motto { get; set; }
        public string ImageFileName { get; set; }

        public string ImagePath
        {
            get
            {
                // Try multiple extensions
                var basePath = Path.Combine(Directory.GetCurrentDirectory(), "Images", "Members");
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(ImageFileName);
                
                // Check for common image extensions
                string[] extensions = { ".jpg", ".jpeg", ".png", ".bmp" };
                
                foreach (var ext in extensions)
                {
                    var path = Path.Combine(basePath, fileNameWithoutExt + ext);
                    if (File.Exists(path))
                        return path;
                }
                
                return null;
            }
        }

        public bool HasImage => !string.IsNullOrEmpty(ImagePath);

        public string Initials
        {
            get
            {
                if (string.IsNullOrEmpty(Name)) return "?";
                var parts = Name.Split(' ');
                if (parts.Length >= 2)
                    return $"{parts[0][0]}{parts[1][0]}".ToUpper();
                return Name.Substring(0, 1).ToUpper();
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }

    public class AboutViewModel : ViewModelBase
    {
        private TeamMember _currentMember;
        private int _currentIndex;

        public ObservableCollection<TeamMember> TeamMembers { get; }

        public TeamMember CurrentMember
        {
            get => _currentMember;
            set => SetProperty(ref _currentMember, value);
        }

        public ICommand NextMemberCommand { get; }
        public ICommand PreviousMemberCommand { get; }
        public ICommand SelectMemberCommand { get; }

        public AboutViewModel()
        {
            TeamMembers = new ObservableCollection<TeamMember>
            {
                new TeamMember
                {
                    Name = "Dane Lloyd Aying",
                    Role = "Developer",
                    Motto = "God will provide",
                    ImageFileName = "dane.jpg"
                },
                new TeamMember
                {
                    Name = "Romel Jasper Namocatcat",
                    Role = "Developer",
                    Motto = "Trust the timing of your life",
                    ImageFileName = "romel.jpg"
                },
                new TeamMember
                {
                    Name = "Elihu France S. Caalim",
                    Role = "Developer",
                    Motto = "Life's too hard not to enjoy every moment.",
                    ImageFileName = "elihu.jpg"
                },
                new TeamMember
                {
                    Name = "Justin",
                    Role = "Developer",
                    Motto = "One Team One Goal",
                    ImageFileName = "justin.jpg"
                },
                new TeamMember
                {
                    Name = "Ashley",
                    Role = "Developer",
                    Motto = "mo graduate",
                    ImageFileName = "ashley.jpg"
                },
                new TeamMember
                {
                    Name = "Vincent",
                    Role = "Developer",
                    Motto = "toy",
                    ImageFileName = "vincent.jpg"
                }
            };

            NextMemberCommand = new RelayCommand(_ => NextMember());
            PreviousMemberCommand = new RelayCommand(_ => PreviousMember());
            SelectMemberCommand = new RelayCommand(member => SelectMember(member as TeamMember));

            _currentIndex = 0;
            UpdateCurrentMember();
        }

        private void NextMember()
        {
            _currentIndex = (_currentIndex + 1) % TeamMembers.Count;
            UpdateCurrentMember();
        }

        private void PreviousMember()
        {
            _currentIndex = (_currentIndex - 1 + TeamMembers.Count) % TeamMembers.Count;
            UpdateCurrentMember();
        }

        private void SelectMember(TeamMember member)
        {
            if (member == null) return;
            _currentIndex = TeamMembers.IndexOf(member);
            UpdateCurrentMember();
        }

        private void UpdateCurrentMember()
        {
            // Update selection state
            foreach (var member in TeamMembers)
            {
                member.IsSelected = false;
            }

            TeamMembers[_currentIndex].IsSelected = true;
            CurrentMember = TeamMembers[_currentIndex];
        }
    }
}
