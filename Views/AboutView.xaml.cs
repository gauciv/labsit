using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using LaboratorySitInSystem.Helpers;

namespace LaboratorySitInSystem.Views
{
    public partial class AboutView : UserControl
    {
        private bool _scrollChangedHandled = false;

        public AboutView()
        {
            InitializeComponent();
            DataContext = new AboutViewViewModel(this);
            Loaded += AboutView_Loaded;
            SizeChanged += AboutView_SizeChanged;
        }

        private void AboutView_Loaded(object sender, RoutedEventArgs e)
        {
            // Set spacer widths first
            UpdateSpacerWidths();
            
            // Initialize carousel position after spacers are set
            if (DataContext is AboutViewViewModel vm)
            {
                vm.InitializeCarousel();
            }
            
            // Delay scroll to ensure layout is complete
            Dispatcher.BeginInvoke(new Action(() => 
            {
                if (DataContext is AboutViewViewModel vm)
                {
                    vm.ScrollToCurrentCard();
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
            
            // Hook up ScrollChanged event
            var scrollViewer = FindName("CarouselScrollViewer") as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.ScrollChanged += CarouselScrollViewer_ScrollChanged;
            }
        }

        private void CarouselScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // Only handle once to reposition after initial layout
            if (!_scrollChangedHandled && e.ExtentWidthChange != 0)
            {
                _scrollChangedHandled = true;
                if (DataContext is AboutViewViewModel vm)
                {
                    vm.ScrollToCurrentCard();
                }
            }
        }

        private void AboutView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Update spacer widths on size change
            UpdateSpacerWidths();
            
            // Reposition to current card after size change
            if (DataContext is AboutViewViewModel vm)
            {
                vm.ScrollToCurrentCard();
            }
        }

        private void UpdateSpacerWidths()
        {
            var scrollViewer = FindName("CarouselScrollViewer") as ScrollViewer;
            var leftSpacer = FindName("LeftSpacer") as Border;
            var rightSpacer = FindName("RightSpacer") as Border;

            if (scrollViewer != null && leftSpacer != null && rightSpacer != null)
            {
                // Wait for actual width to be available
                if (scrollViewer.ActualWidth > 0)
                {
                    double viewportWidth = scrollViewer.ActualWidth;
                    double spacerWidth = (viewportWidth - 360) / 2;
                    if (spacerWidth > 0)
                    {
                        leftSpacer.Width = spacerWidth;
                        rightSpacer.Width = spacerWidth;
                    }
                }
                else
                {
                    // Fallback to 800px viewport
                    leftSpacer.Width = 220;
                    rightSpacer.Width = 220;
                }
            }
        }
    }

    public class TeamMember
    {
        public string Name { get; set; }
        public string Role { get; set; }
        public string Motto { get; set; }
        public string ImageFileName { get; set; }

        public string FirstName
        {
            get
            {
                if (string.IsNullOrEmpty(Name)) return "";
                var parts = Name.Split(' ');
                return parts[0];
            }
        }

        public string RoleUpperCase
        {
            get => Role?.ToUpper() ?? "";
        }

        public string MottoWithPrefix
        {
            get => $"Motto: {Motto}";
        }

        public string ImagePath
        {
            get
            {
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(ImageFileName);
                string[] extensions = { ".jpg", ".jpeg", ".png", ".bmp" };
                
                // Try multiple base paths
                string[] basePaths = 
                {
                    Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Members"),
                    Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Assets", "Members")
                };
                
                foreach (var basePath in basePaths)
                {
                    foreach (var ext in extensions)
                    {
                        var path = Path.Combine(basePath, fileNameWithoutExt + ext);
                        if (File.Exists(path))
                            return path;
                    }
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
    }

    public class AboutViewViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        private int _currentIndex;
        private AboutView _view;
        private bool _isAnimating = false;
        private const double CARD_WIDTH = 360;
        private const double CARD_MARGIN = 16; // 8 on each side
        private const double VIEWPORT_WIDTH = 800;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }

        public ObservableCollection<TeamMember> TeamMembers { get; }

        public int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                _currentIndex = value;
                OnPropertyChanged(nameof(CurrentIndex));
            }
        }

        public ICommand NextMemberCommand { get; }
        public ICommand PreviousMemberCommand { get; }
        public ICommand SelectMemberByIndexCommand { get; }

        public AboutViewViewModel(AboutView view)
        {
            _view = view;
            
            TeamMembers = new ObservableCollection<TeamMember>
            {
                new TeamMember
                {
                    Name = "Vincent",
                    Role = "Lead Developer",
                    Motto = "toy",
                    ImageFileName = "vincent.jpg"
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
                    Name = "Dane",
                    Role = "Developer",
                    Motto = "God will provide",
                    ImageFileName = "dane.jpg"
                },
                new TeamMember
                {
                    Name = "Elihu",
                    Role = "Developer",
                    Motto = "Life's too hard not to enjoy every moment.",
                    ImageFileName = "elihu.jpg"
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
                    Name = "Romel",
                    Role = "Developer",
                    Motto = "Trust the timing of your life",
                    ImageFileName = "romel.jpg"
                }
            };

            NextMemberCommand = new RelayCommand(_ => GoToNextMember());
            PreviousMemberCommand = new RelayCommand(_ => GoToPreviousMember());
            SelectMemberByIndexCommand = new RelayCommand(param => SelectMemberByIndex(param));

            CurrentIndex = 0;
        }

        public void InitializeCarousel()
        {
            UpdateCardAppearances();
            ScrollToCard(0, false);
        }

        public void ScrollToCurrentCard()
        {
            ScrollToCard(CurrentIndex, false);
        }

        private void GoToNextMember()
        {
            if (_isAnimating) return;
            
            int nextIndex = (CurrentIndex + 1) % TeamMembers.Count;
            NavigateToCard(nextIndex);
        }

        private void GoToPreviousMember()
        {
            if (_isAnimating) return;
            
            int prevIndex = (CurrentIndex - 1 + TeamMembers.Count) % TeamMembers.Count;
            NavigateToCard(prevIndex);
        }

        private void SelectMemberByIndex(object param)
        {
            if (param == null || _isAnimating) return;
            
            if (int.TryParse(param.ToString(), out int index))
            {
                if (index >= 0 && index < TeamMembers.Count && index != CurrentIndex)
                {
                    NavigateToCard(index);
                }
            }
        }

        private void NavigateToCard(int newIndex)
        {
            if (_isAnimating) return;
            
            CurrentIndex = newIndex;
            UpdateCardAppearances();
            ScrollToCard(newIndex, true);
        }

        private void ScrollToCard(int index, bool animate)
        {
            var scrollViewer = _view.FindName("CarouselScrollViewer") as ScrollViewer;
            var leftSpacer = _view.FindName("LeftSpacer") as Border;
            if (scrollViewer == null) return;

            // Calculate target offset to center the card
            // Each card is at: spacerWidth + (index * (cardWidth + margin))
            // We want the card centered in the viewport
            // So we scroll to: cardPosition - (viewportWidth / 2) + (cardWidth / 2)
            double spacerWidth = leftSpacer?.Width ?? 220;
            double viewportWidth = scrollViewer.ActualWidth > 0 ? scrollViewer.ActualWidth : 800;
            double cardPosition = spacerWidth + (index * (CARD_WIDTH + CARD_MARGIN));
            double targetOffset = cardPosition - (viewportWidth / 2) + (CARD_WIDTH / 2);
            
            if (animate)
            {
                _isAnimating = true;
                SmoothScrollTo(scrollViewer, targetOffset);
            }
            else
            {
                scrollViewer.ScrollToHorizontalOffset(targetOffset);
            }
        }

        private void SmoothScrollTo(ScrollViewer scrollViewer, double targetOffset)
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            double startOffset = scrollViewer.HorizontalOffset;
            double distance = targetOffset - startOffset;
            double elapsed = 0;
            double duration = 250;

            timer.Tick += (s, e) =>
            {
                elapsed += 16;
                double progress = Math.Min(elapsed / duration, 1.0);
                double eased = 1 - Math.Pow(1 - progress, 3); // Cubic ease-out
                scrollViewer.ScrollToHorizontalOffset(startOffset + distance * eased);
                
                if (progress >= 1.0)
                {
                    timer.Stop();
                    _isAnimating = false;
                }
            };
            
            timer.Start();
        }

        private void UpdateCardAppearances()
        {
            for (int i = 0; i < TeamMembers.Count; i++)
            {
                var card = _view.FindName($"Card{i}") as Border;
                var scale = _view.FindName($"Scale{i}") as ScaleTransform;
                
                if (card != null && scale != null)
                {
                    bool isCurrent = (i == CurrentIndex);
                    
                    // Animate opacity
                    var opacityAnimation = new DoubleAnimation
                    {
                        To = isCurrent ? 1.0 : 0.55,
                        Duration = TimeSpan.FromMilliseconds(250),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
                    };
                    card.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
                    
                    // Animate scale
                    var scaleAnimation = new DoubleAnimation
                    {
                        To = isCurrent ? 1.0 : 0.85,
                        Duration = TimeSpan.FromMilliseconds(250),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
                    };
                    scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
                    scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
                    
                    // Update background and shadow
                    if (isCurrent)
                    {
                        card.Background = Application.Current.Resources["CardBgBrush"] as Brush;
                        card.Effect = new DropShadowEffect
                        {
                            Color = Colors.Black,
                            Opacity = 0.12,
                            BlurRadius = 20,
                            ShadowDepth = 5,
                            Direction = 315
                        };
                    }
                    else
                    {
                        card.Background = Application.Current.Resources["CardBgSoftBrush"] as Brush;
                        card.Effect = null;
                    }
                }
            }
        }
    }
}