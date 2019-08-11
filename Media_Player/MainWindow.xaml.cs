using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using WMPLib;

namespace Media_Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        public bool IsStopped = false;
        public bool IsPaused = false;
        public bool IsPlaying = false;
        private double contentHeight;
        private double contentWidth;
        private string currentMedia;
        double cur = 0.0;
        int stt = 0;
        static string[] videoExt =
        {
            ".FLV",".AVI",".WMV",".MP4",".MPG",".MPEG",".M4V"
        };
        static string[] audioExt =
        {
            ".MP3",".WAV",".WAVE",".WMA"
        };

        BindingList<Media> currentPlaylist = new BindingList<Media>();
        BindingList<Playlist> playlists = new BindingList<Playlist>();
        WindowsMediaPlayer Player = new WindowsMediaPlayer();
        WindowsMediaPlayer ReadDurationPlayer = new WindowsMediaPlayer();
        IWMPPlaylist iWMPPlaylist;

        public string CurrentMedia
        {
            get => currentMedia; set
            {
                currentMedia = value;
                RaiseChangeEvent();
            }
        }

        public double ContentHeight
        {
            get => contentHeight; set
            {
                contentHeight = value;
                RaiseChangeEvent();
            }
        }

        public double ContentWidth
        {
            get => contentWidth; set
            {
                contentWidth = value;
                RaiseChangeEvent();
            }
        }

        void RaiseChangeEvent([CallerMemberName]string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            list.ItemsSource = currentPlaylist;
            this.DataContext = this;
            ContentHeight = mainwindow.Height - 100;
            contentWidth = mainwindow.Width - 230;
            Combobox_playlist.ItemsSource = playlists;
            list.ItemsSource = currentPlaylist;
            iWMPPlaylist = Player.playlistCollection.newPlaylist("currentPlaylist");
        }

        void CreateMediaElement(string path)
        {
            MediaElement mediaElement = new MediaElement();
            Canvas_content.Children.Add(mediaElement);
            mediaElement.Width = mainwindow.Width - 230;
            mediaElement.Height = mainwindow.Height - 100;
            mediaElement.Source = new Uri(path);
        }
        int getDuration(string path)
        {
            int duration = 0;
            ReadDurationPlayer.URL = path;
            ReadDurationPlayer.controls.play();
            duration = (int)ReadDurationPlayer.currentMedia.duration;
            ReadDurationPlayer.controls.stop();
            return duration;
        }
        string getDurationString(int duration)
        {
            string durationStr;
            TimeSpan result = TimeSpan.FromSeconds(duration);
            if (duration >= 3600)
                durationStr = result.ToString("hh':'mm':'ss");
            else durationStr = result.ToString("mm':'ss");
            return durationStr;
        }
        bool IsAudioFile(string path)
        {
            return -1 != Array.IndexOf(audioExt, Path.GetExtension(path).ToUpperInvariant());
        }
        bool IsVideoFile(string path)
        {
            return -1 != Array.IndexOf(videoExt, Path.GetExtension(path).ToUpperInvariant());
        }

        private void Add_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog
            {
                Filter = "(mp3, wav, mp4, wmv, mpg, avi, flv)| *.mp3; *.wav; *.mp4; *.avi; *.flv; *.wmv; *.mpg | all files | *.* ",
                Multiselect = true
            };
            if (open.ShowDialog() == true)
            {
                string[] filenames = open.SafeFileNames;
                string[] filepaths = open.FileNames;

                for (int i = 0; i < filenames.Length; i++)
                {
                    if (IsAudioFile(filenames[i]) || IsVideoFile(filenames[i]))
                    {
                        stt += 1;
                        string title = stt + ". " + filenames[i];
                        AddNewMedia(filepaths[i]);
                        IWMPMedia3 media = Player.newMedia(filepaths[i]) as IWMPMedia3;
                        iWMPPlaylist.appendItem(media);

                    }

                }
            }
        }
        void AddNewMedia(string str)
        {
            currentPlaylist.Add(new Media()
            {
                File_Path = str,
                Duration_length = getDuration(str)
            });
        }
        private void Play_Button_Click(object sender, RoutedEventArgs e)
        {

            if (!IsPlaying)
            {
                if (IsPaused)
                {
                    Player.controls.currentPosition = cur;
                }
                else
                {
                    Player.currentPlaylist = iWMPPlaylist;
                }
                IsStopped = false;
                IsPaused = false;
                IsPlaying = true;
                Player.controls.play();
            }
            else
            {
                cur = Player.controls.currentPosition;
                Player.controls.pause();
                IsStopped = false;
                IsPlaying = false;
                IsPaused = true;
            }
        }

        private void Next10_Button_Click(object sender, RoutedEventArgs e)
        {
            Player.controls.next();
        }

        private void Prev10_Button_Click(object sender, RoutedEventArgs e)
        {
            Player.controls.previous();
        }

        private void Replay_Button_Click(object sender, RoutedEventArgs e)
        {
            Player.controls.currentPosition = 0;
        }

        private void ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            string[] file = Directory.GetFiles("C:/Users/Systemcall/OneDrive/Máy tính","*.pl");
            playlists.Clear();
            foreach (var i in file)
                playlists.Add(CreatePlaylist(i));
        }
        private Playlist CreatePlaylist(string i)
        {
            Playlist pl = new Playlist();
            pl.m_Path = i;
            return pl;
        }
        private void Btn_SavePlaylist(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_NewPlaylist(object sender, RoutedEventArgs e)
        {
            playlists.Clear();
        }

        private void Combobox_playlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Playlist_Detail(object sender, RoutedEventArgs e)
        {

        }

        private void Mainwindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ContentHeight = mainwindow.Height - 100;
            contentWidth = mainwindow.Width - 230;
        }
    }
}
