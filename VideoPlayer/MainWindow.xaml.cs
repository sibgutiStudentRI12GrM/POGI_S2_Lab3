using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Media;
using System.Windows.Controls.Primitives;
using System.Threading;

namespace VP {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        bool IsMediaPlaying = false;
        Timer SliderSyncTimer;
        public MainWindow() {
            InitializeComponent();
            MPE.MediaOpened += MPE_MediaOpened;

        }
        string GetTimeToDisplayFromTimespan(TimeSpan ts) {
            string result = "";
            int daysInMedia = Convert.ToInt32(ts.Days);
            int hoursInMedia = Convert.ToInt32(ts.Hours);
            if (daysInMedia > 0) { result += $"{daysInMedia}:"; }
            if (hoursInMedia > 0) { result += $"{hoursInMedia}:"; }
            int minutesInMedia = Convert.ToInt32(ts.Minutes);
            int secondsInMedia = Convert.ToInt32(ts.Seconds);
            result += $"{minutesInMedia}:{secondsInMedia.ToString("D2")}";
            return result;
        }
        void SyncAppWithPlayerPosition(object state) {
            Dispatcher.Invoke((Action)(() => {
                Slider_Playback.Value = MPE.Position.TotalSeconds;
                TimeSpan sliderSecondsTS = new TimeSpan(0, 0, (int)MPE.Position.TotalSeconds);
                Label_CurrentPlayTime.Content = GetTimeToDisplayFromTimespan(sliderSecondsTS);
            }));
        }
        void StartGuiPlaing() {
            MPE.Play();
            IsMediaPlaying = true;
            SliderSyncTimer = new Timer(SyncAppWithPlayerPosition, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(1000));
        }

        private void MPE_MediaOpened(object sender, EventArgs e) {
            Duration mediaDuration = MPE.NaturalDuration;
            Slider_Playback.Maximum = mediaDuration.TimeSpan.TotalSeconds;
            Label_MediaDuration.Content = GetTimeToDisplayFromTimespan(mediaDuration.TimeSpan);
            
        }

        private void Btn_Open_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == false) {
                return;
            }

            MPE.Source = new Uri(dlg.FileName, UriKind.Relative);
            MPE.Volume = 0.5;
            StartGuiPlaing();

        }

        private void Slider_Volume_ValueChanged(object sender, DragCompletedEventArgs e) {
            MPE.Volume = (double)Slider_Volume.Value / 100;
        }
        void Slider_Playback_ValueChanged(object sender, DragCompletedEventArgs e) {
            int sliderTime = (int)Slider_Playback.Value;
            TimeSpan sliderTS = new TimeSpan(0, 0, sliderTime);
            MPE.Position = sliderTS;
            Label_CurrentPlayTime.Content = GetTimeToDisplayFromTimespan(sliderTS);
        }
        private void Btn_PlayPause_Click(object sender, RoutedEventArgs e) {
            if (IsMediaPlaying) {
                MPE.Pause();
                SliderSyncTimer.Dispose();
                IsMediaPlaying = false;
            } else {
                StartGuiPlaing();
            }
        }

        private void Btn_Stop_Click(object sender, RoutedEventArgs e) {
            MPE.Stop();
            MPE.Source = null;
            IsMediaPlaying = false;
        }
    }
}
