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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media;
using Microsoft.Win32;
using System.Media;
using System.Windows.Controls.Primitives;
using System.Threading;

namespace MusicPlayer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        List<AudioFile> AudioPlaylist = new List<AudioFile>();
        MediaPlayer Player = new MediaPlayer();
        Timer SliderSyncTimer;
        bool IsPlayingAudio = false;

        int NextAudioIndex {
            get {
                int currentIndex = ComboBox_AudioPlaylist.SelectedIndex;
                if (currentIndex == ComboBox_AudioPlaylist.Items.Count - 1) { // if last
                    return 0;
                }

                return currentIndex + 1;
            }
        }

        int PreviousAuidioIndex {
            get {
                int currentIndex = ComboBox_AudioPlaylist.SelectedIndex;
                if (currentIndex == 0) { // if first
                    return ComboBox_AudioPlaylist.Items.Count - 1;
                }
                return currentIndex - 1;
            }
        }

        int RandomAudioIndex {
            get {
                Random rnd = new Random();
                int currentIndex = ComboBox_AudioPlaylist.SelectedIndex;
                if (ComboBox_AudioPlaylist.Items.Count < 2) { return currentIndex; }

                while (true) {
                    int randomIndex = rnd.Next(0, ComboBox_AudioPlaylist.Items.Count);
                    if (randomIndex != currentIndex) {
                        return randomIndex;
                    }
                }
                

            }
        }

        
        public MainWindow() {
            InitializeComponent();
            Player.MediaOpened += Player_MediaOpened;
            Player.MediaEnded += Player_Ended;
        }

        class AudioFile {
            public string Path;

            public AudioFile(string path) {
                Path = path;
            }

            public string GetFileNameWithoutPath() {
                string[] splited = Path.Split('\\');
                return splited[splited.Length - 1];
            }
        }
        
        void PlaySelectionInPlaylist() {
            Player.Open(new Uri(AudioPlaylist[ComboBox_AudioPlaylist.SelectedIndex].Path, UriKind.Relative));
            StartGuiPlaing();
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
                Slider_Playback.Value = Player.Position.TotalSeconds;

                TimeSpan sliderSecondsTS = new TimeSpan(0,0, (int)Slider_Playback.Value);
                Label_CurrentTime.Content = GetTimeToDisplayFromTimespan(sliderSecondsTS);


            }));
        }

        void StopGuiPlaing() {
            Player.Stop();
            if (SliderSyncTimer != null) { SliderSyncTimer.Dispose(); }
            Slider_Playback.Value = 0;
            IsPlayingAudio = false;
            Label_CurrentTime.Content = GetTimeToDisplayFromTimespan(new TimeSpan(0));
        }

        void StartGuiPlaing() {
            Player.Play();
            IsPlayingAudio = true;
            SliderSyncTimer = new Timer(SyncAppWithPlayerPosition, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(1000));
        }

        private void Btn_AddAudio_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == false) {
                return;
            }

            AudioPlaylist.Add(new AudioFile(ofd.FileName.ToString()));
            ComboBox_AudioPlaylist.Items.Add(AudioPlaylist[AudioPlaylist.Count-1].GetFileNameWithoutPath());
            ComboBox_AudioPlaylist.SelectedIndex = ComboBox_AudioPlaylist.Items.Count - 1;

            Btn_RemoveAudio.IsEnabled = true;
        }

        private void Btn_PlayPause_Click(object sender, RoutedEventArgs e) {
            if (IsPlayingAudio) {
                Player.Pause();
                IsPlayingAudio = false;
                if (SliderSyncTimer != null) {
                    SliderSyncTimer.Dispose();
                }
            } else {
                StartGuiPlaing();
            }
        }

        private void ComboBox_AudioPlaylist_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (ComboBox_AudioPlaylist.SelectedIndex == -1) {
                return;
            }
            int comboBoxIndex = ComboBox_AudioPlaylist.SelectedIndex;
            Player.Open(new Uri(AudioPlaylist[comboBoxIndex].Path, UriKind.Relative));

            IsPlayingAudio = false;
            StopGuiPlaing();

        }
        private void Slider_Volume_ValueChanged(object sender, DragCompletedEventArgs e) {
            int sliderVolumeValue = (int)Slider_Volume.Value;
            
            Player.Volume = (double)sliderVolumeValue/100;
        }
        
        private void Slider_Playback_ValueChanged(object sender, DragCompletedEventArgs e) {
            int PlaybackValue = (int)Slider_Playback.Value;
            TimeSpan playbackTS = new TimeSpan(0, 0, PlaybackValue);
            Player.Position = playbackTS;

            Label_CurrentTime.Content = GetTimeToDisplayFromTimespan(playbackTS);

        }

        private void Player_MediaOpened(object sender, EventArgs e) {
            Duration opennedMediaDuration = Player.NaturalDuration;

            Slider_Playback.Maximum = Convert.ToDouble(opennedMediaDuration.TimeSpan.TotalSeconds);
            Slider_Playback.Value = 0;

            Label_MaximumTime.Content = GetTimeToDisplayFromTimespan(opennedMediaDuration.TimeSpan);
            
        }
        private void Player_Ended(object sender, EventArgs e) {
            StopGuiPlaing();
            if (CheckBox_RandomAudio.IsChecked == true) {
                ComboBox_AudioPlaylist.SelectedIndex = RandomAudioIndex;
            } else {
                ComboBox_AudioPlaylist.SelectedIndex = NextAudioIndex;
            }
            PlaySelectionInPlaylist();
        }
        private void Btn_Stop_Click(object sender, RoutedEventArgs e) {
            StopGuiPlaing();
        }

        private void Btn_PlayNext_Click(object sender, RoutedEventArgs e) {

            StopGuiPlaing();
            if (CheckBox_RandomAudio.IsChecked == true) {
                ComboBox_AudioPlaylist.SelectedIndex = RandomAudioIndex;
            } else {
                ComboBox_AudioPlaylist.SelectedIndex = NextAudioIndex;
            }
            PlaySelectionInPlaylist();
        }

        private void Btn_PlayPrevios_Click(object sender, RoutedEventArgs e) {
            StopGuiPlaing();
            ComboBox_AudioPlaylist.SelectedIndex = PreviousAuidioIndex;
            PlaySelectionInPlaylist();
        }

        private void Btn_RemoveAudio_Click(object sender, RoutedEventArgs e) {
            StopGuiPlaing();
            if (ComboBox_AudioPlaylist.Items.Count > 0) {
                
                AudioPlaylist.RemoveAt(ComboBox_AudioPlaylist.SelectedIndex);
                ComboBox_AudioPlaylist.Items.RemoveAt(ComboBox_AudioPlaylist.SelectedIndex);
            }
            if (ComboBox_AudioPlaylist.Items.Count < 1) {
                Btn_RemoveAudio.IsEnabled = false;
            }

        }
    }
}
