using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using LaboratorySitInSystem.ViewModels;

namespace LaboratorySitInSystem.Views
{
    public partial class LoadingView : UserControl
    {
        private DispatcherTimer _timer;
        private Action _onLoadingComplete;

        public LoadingView()
        {
            InitializeComponent();
            Loaded += LoadingView_Loaded;
        }

        private void LoadingView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoadingViewModel viewModel)
            {
                _onLoadingComplete = viewModel.OnLoadingComplete;
            }

            try
            {
                // Try multiple ways to load the video
                string videoPath = null;

                // Method 1: Relative to executable
                string exePath = AppDomain.CurrentDomain.BaseDirectory;
                string path1 = Path.Combine(exePath, "LoadingVideo", "LabSit.mp4");
                
                // Method 2: Pack URI
                string path2 = "pack://application:,,,/LoadingVideo/LabSit.mp4";
                
                // Method 3: Relative path
                string path3 = Path.GetFullPath(Path.Combine(exePath, "..", "..", "..", "LoadingVideo", "LabSit.mp4"));

                if (File.Exists(path1))
                {
                    videoPath = path1;
                    System.Diagnostics.Debug.WriteLine($"[LOADING] Video found at: {path1}");
                }
                else if (File.Exists(path3))
                {
                    videoPath = path3;
                    System.Diagnostics.Debug.WriteLine($"[LOADING] Video found at: {path3}");
                }
                else
                {
                    // Try pack URI
                    LoadingVideo.Source = new Uri(path2, UriKind.Absolute);
                    System.Diagnostics.Debug.WriteLine($"[LOADING] Trying pack URI: {path2}");
                }

                if (!string.IsNullOrEmpty(videoPath))
                {
                    LoadingVideo.Source = new Uri(videoPath, UriKind.Absolute);
                }

                // Start playing the video
                LoadingVideo.Play();

                // Set up timer for 3 seconds
                _timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(3)
                };
                _timer.Tick += Timer_Tick;
                _timer.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LOADING ERROR] {ex.Message}");
                // If video fails, still proceed after 3 seconds
                _timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(3)
                };
                _timer.Tick += Timer_Tick;
                _timer.Start();
            }
        }

        private void LoadingVideo_MediaOpened(object sender, RoutedEventArgs e)
        {
            // Video is ready to play
            System.Diagnostics.Debug.WriteLine("[LOADING] Video opened successfully");
            LoadingVideo.Play();
        }

        private void LoadingVideo_MediaEnded(object sender, RoutedEventArgs e)
        {
            // Loop the video if it ends before 3 seconds
            System.Diagnostics.Debug.WriteLine("[LOADING] Video ended, looping...");
            LoadingVideo.Position = TimeSpan.Zero;
            LoadingVideo.Play();
        }

        private void LoadingVideo_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[LOADING] Video failed to load: {e.ErrorException?.Message}");
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            
            try
            {
                LoadingVideo.Stop();
            }
            catch { }
            
            // Navigate to the target view
            _onLoadingComplete?.Invoke();
        }
    }
}
