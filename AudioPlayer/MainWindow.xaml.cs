using MaterialDesignThemes.Wpf;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TagLib;
using System.Text;
using System.Threading;
using System.Drawing;
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


namespace AudioPlayer
{

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private TimeSpan position;
        private TimeSpan Time;
        private TimeSpan _remainingTime;



        public TimeSpan time
        {
            get { return Time; }
            set { Time = value; OnPropertyChanged("time"); }
        }

        public TimeSpan RemainingTime
        {
            get { return _remainingTime; }
            set { _remainingTime = value; OnPropertyChanged("RemainingTime"); }
        }



        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
        }

        private List<string> historyListNames = new List<string>();

      
        static int volumeLevel = 0;
        static int songindex = 0;
        static bool mix = false;
        static bool restart = false;
        static bool playOrStop;
        static string folderPath = "";
        static string path = @"C:\music\loved";
        List<string> fileNames = new List<string>();
        List<string> songsListNames = new List<string>();
        List<string> songsListNamesCopy = new List<string>();
        List<string> fileNamesCopy = new List<string>();
        string[] files;
        System.Timers.Timer timer = new System.Timers.Timer(1000);
        Random rng = new Random();

        private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.NaturalDuration.HasTimeSpan) MediaSlider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.Ticks;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            // Add the current song to the history list
            historyListNames.Add(NameSong.Text);
            HistoryList.ItemsSource = historyListNames;
        }

        private void MediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            NextSong();
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                MediaSlider.Value = mediaPlayer.Position.Ticks;
                if (mediaPlayer.NaturalDuration.HasTimeSpan)
                {
                    RemainingTime = mediaPlayer.NaturalDuration.TimeSpan - mediaPlayer.Position;
                }
                time = mediaPlayer.Position;
                if (mediaPlayer.NaturalDuration.HasTimeSpan)
                {
                    if (time == mediaPlayer.NaturalDuration.TimeSpan) 
                        {
                            if (restart == false) 
                            {
                                NextSong();
                            }
                            else
                            {
                                RestartSong();
                            }
                        }
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Unwrap_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowState = WindowState.Normal;
            }
        }

        private void RollUp_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void PlayOrPause_Click(object sender, RoutedEventArgs e)
        {
            if (playOrStop == true)
            {
                Pause();
            }
            else
            {
                Play();
            }
        }


        private void Play()
        {
            playOrStop = true;
            mediaPlayer.Position = position;
            mediaPlayer.Play();
            var icon = new PackIcon { Kind = PackIconKind.Pause };
            PlayOrPause.Content = icon;
            timer.Enabled = true;
        }

        private void Pause()
        {
            playOrStop = false;
            position = mediaPlayer.Position;
            mediaPlayer.Stop();
            var icon = new PackIcon { Kind = PackIconKind.Play };
            PlayOrPause.Content = icon;
            timer.Enabled = false;
        }

        private void SoundKill_Click(object sender, RoutedEventArgs e)
        {
            NameSong.Text = "";
            mediaPlayer.Source = null;
            mediaPlayer.Stop();
        }

        private void SoundRestart_Click(object sender, RoutedEventArgs e)
        {
            if (restart == false) { restart = true; SoundRestart.Foreground = new SolidColorBrush(Colors.PaleVioletRed); } else { restart = false; SoundRestart.Foreground = new SolidColorBrush(Colors.Black); }
        }

        private void RestartSong()
        {
            MediaSlider.Value = 0; 
            mediaPlayer.Position = new TimeSpan(Convert.ToInt64(MediaSlider.Value));
        }

        private void Mix_Click(object sender, RoutedEventArgs e)
        {
            Mixer();
        }

        private void Mixer()
        {
            if (mix == false)
            {
                Mix.Foreground = new SolidColorBrush(Colors.PaleVioletRed);
                mix = true;
                foreach (var name in fileNames)
                {
                    fileNamesCopy.Add(name);
                }

                foreach (var name in songsListNames)
                {
                    songsListNamesCopy.Add(name);
                }

                int n = fileNamesCopy.Count;
                while (n > 1)
                {
                    n--;
                    int k = rng.Next(n + 1);
                    string value = fileNames[k];
                    fileNames[k] = fileNames[n];
                    fileNames[n] = value;
                    value = songsListNames[k];
                    songsListNames[k] = songsListNames[n];
                    songsListNames[n] = value;
                }
                SongsList.Items.Clear();
                foreach (var name in songsListNames)
                {
                    SongsList.Items.Add(name);
                }
            }
            else
            {
                Mix.Foreground = new SolidColorBrush(Colors.Black);
                mix = false;
                fileNames.Clear();
                foreach (var name in fileNamesCopy)
                {
                    fileNames.Add(name);
                }
                SongsList.Items.Clear();
                songsListNames.Clear();
                foreach (var name in songsListNamesCopy)
                {
                    songsListNames.Add(name);
                    SongsList.Items.Add(name);
                }
                songsListNamesCopy.Clear();
                fileNamesCopy.Clear();
            }
        }

      

       

       


       

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            NextSong();
        }
        private void NextSong()
        {
            if (songindex != fileNames.Count - 1)
            {
                songindex++;
                SongsList.SelectedIndex = songindex;
                mediaPlayer.Source = new Uri(folderPath + "/" + fileNames[songindex]);
                mediaPlayer.Play();
            }
            else
            {
                songindex = fileNames.Count - fileNames.Count;
                SongsList.SelectedIndex = songindex;
                mediaPlayer.Source = new Uri(folderPath + "/" + fileNames[songindex]);
                mediaPlayer.Play();
            }
        }

        private void VolumeChange_Click(object sender, RoutedEventArgs e)
        {
            switch (volumeLevel)
            {
                case 0:
                    volumeLevel = 1;
                    VolumeSlider.Value = 0;
                    mediaPlayer.Volume = VolumeSlider.Value / 100;
                    break; 
                case 1:
                    volumeLevel = 2;
                    VolumeSlider.Value = 25;
                    mediaPlayer.Volume = VolumeSlider.Value / 100;
                    break; 
                case 2:
                    volumeLevel = 3;
                    VolumeSlider.Value = 50;
                    mediaPlayer.Volume = VolumeSlider.Value / 100;
                    break; 
                case 3:
                    volumeLevel = 4;
                    VolumeSlider.Value = 75;
                    mediaPlayer.Volume = VolumeSlider.Value / 100;
                    break; 
                case 4:
                    volumeLevel = 0;
                    VolumeSlider.Value = 100;
                    mediaPlayer.Volume = VolumeSlider.Value / 100;
                    break;
            }
        }

        private void ChoiceSong_Click(object sender, RoutedEventArgs e)
        {
            ChoiceFolder();
        }

        private void ChoiceFolder()
        {
            string extensionFilter = "*.mp3|*.m4a|*.flac|*.wav";
            clearInfo();
            CommonOpenFileDialog dialog = new CommonOpenFileDialog { IsFolderPicker = true };
            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                folderPath = dialog.FileName;
                files = Directory.GetFiles(dialog.FileName).Where(f => extensionFilter.Contains(System.IO.Path.GetExtension(f).ToLower())).ToArray();
                foreach (string file in files)
                {
                    string name = System.IO.Path.GetFileNameWithoutExtension(file);
                    string fullName = System.IO.Path.GetFileName(file);
                    songsListNames.Add(name);
                    fileNames.Add(fullName);
                    SongsList.Items.Add(name);
                }
                Play(0);
            }
        }


        private void clearInfo()
        {
            folderPath = "";
            fileNames.Clear();
            SongsList.Items.Clear();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            BackSong();
        }
        private void BackSong()
        {
            if (songindex != 0)
            {
                songindex--;
                SongsList.SelectedIndex = songindex;
                mediaPlayer.Source = new Uri(folderPath + "/" + fileNames[songindex]);
                mediaPlayer.Play();
            }
            else
            {
                songindex = fileNames.Count - 1;
                SongsList.SelectedIndex = songindex;
                mediaPlayer.Source = new Uri(folderPath + "/" + fileNames[songindex]);
                mediaPlayer.Play();
            }
        }


        private void SongsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SongsList.Items.Count !=0)
            {
                songindex = SongsList.SelectedIndex;
                
              
                Play(songindex);
            }
            
        }
        private void Play(int songindex)
        {
            try
            {
                var icon = new PackIcon { Kind = PackIconKind.Pause };
                PlayOrPause.Content = icon;
                playOrStop = true;
                mediaPlayer.Source = new Uri(folderPath+ "/" + fileNames[songindex]);
                NameSong.Text = $"{SongsList.Items[songindex]}";
                mediaPlayer.Play();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void MediaSlider_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            position = new TimeSpan(Convert.ToInt64(MediaSlider.Value));
            mediaPlayer.Position = new TimeSpan(Convert.ToInt64(MediaSlider.Value));
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Volume = VolumeSlider.Value/100;
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            // Создать новое окно с историей прослушивания
            HistoryWindow historyWindow = new HistoryWindow(historyListNames);

            // Отобразить окно
            historyWindow.ShowDialog();
        }

    }

}
