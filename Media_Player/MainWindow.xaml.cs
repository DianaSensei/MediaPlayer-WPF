using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Xml;
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
        public bool IsLoop = false;//Loop Mode

        int stt = 0;
        int queue = 0;
        double volume_value;
        string total_duration;
        double process;//Process Bar
        string currentMediaDetail;//About
        string processString;
        static string[] videoExt =
        {
            ".FLV",".AVI",".WMV",".MP4",".MPG",".MPEG",".M4V"
        };
        static string[] audioExt =
        {
            ".MP3",".WAV",".WAVE",".WMA"
        };

        Playlist currentPlaylist = new Playlist();//Current Playlist
        Media currentMedia;//Media ready
        //Player use to play media
        MediaPlayer mediaPlayer = new MediaPlayer();
        //Player use to get duration of media
        MediaPlayer mediaDuration = new MediaPlayer();

        BindingList<Playlist> playlists = new BindingList<Playlist>(); //List Playlists
        public string CurrentMediaDetail
        {
            get => currentMediaDetail; set
            {
                currentMediaDetail = value;


                RaiseChangeEvent();
            }
        }

        public double Process
        {
            get => process; set
            {
                process = value;
                RaiseChangeEvent();
            }
        }

        public string Total_duration
        {
            get => total_duration; set
            {
                total_duration = value;
                RaiseChangeEvent();
            }
        }

        public string ProcessString
        {
            get
            {
                return processString;
            }
            set
            {
                processString = value;
                RaiseChangeEvent();
            }
        }

        void RaiseChangeEvent([CallerMemberName]string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public event PropertyChangedEventHandler PropertyChanged;
        DispatcherTimer processTimer = new DispatcherTimer();
        public MainWindow()
        {
            InitializeComponent();

            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Playlists"))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Playlists");
            }
            list.ItemsSource = currentPlaylist.mediaList;
            this.DataContext = this;

            //Dispatcher run Process bar
            Process = 0;
            processTimer.Interval = TimeSpan.FromMilliseconds(100);
            processTimer.Tick += ProcessTimer_Tick;
            processTimer.Start();
            //Continue play next media when current media is finished
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;

            mediaPlayer.Volume = 100;
            Volume_Slider.Value = 100;

            Combobox_playlist.ItemsSource = playlists;
            PreLoadPlayList();
        }

        private void MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            if (queue + 1 >= currentPlaylist.mediaList.Count)
            {
                if (IsLoop) queue = -1;
                else
                {
                    Btn_Play.IsChecked = false;
                    IsPlaying = false;
                    IsPaused = false;
                    IsStopped = true;
                    queue = 0;
                    currentMedia = currentPlaylist.mediaList[queue];
                    return;
                }
            }
            Next_Btn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, Next_Btn));
        }

        private void ProcessTimer_Tick(object sender, EventArgs e)
        {

            if (currentMedia == null || currentMedia.Duration_length == 0)
            {
                ProcessString = TimeSpan.FromSeconds(0).ToString("mm':'ss") + "/" + TimeSpan.FromSeconds(0).ToString("mm':'ss");
                return;
            }
            CurrentMediaDetail = Path.GetFileNameWithoutExtension(currentMedia.File_Path);
            Process = mediaPlayer.Position.TotalSeconds * 100 / currentMedia.Duration_length;
            ProcessString = mediaPlayer.Position.ToString("mm':'ss") + "/" + TimeSpan.FromSeconds(currentMedia.Duration_length).ToString("mm':'ss");
        }
        //Get duration of media
        int getDuration(string path)
        {
            int duration = 0;
            mediaDuration.Open(new Uri(path));
            while (!mediaDuration.NaturalDuration.HasTimeSpan) ;
            duration = (int)mediaDuration.NaturalDuration.TimeSpan.TotalSeconds;
            return duration;
        }

        bool IsAudioFile(string path)
        {
            return -1 != Array.IndexOf(audioExt, Path.GetExtension(path).ToUpperInvariant());
        }
        bool IsVideoFile(string path)
        {
            return -1 != Array.IndexOf(videoExt, Path.GetExtension(path).ToUpperInvariant());
        }
        //Add new Media to current playlist
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
                        AddNewMedia(filepaths[i]);
                    }

                }
                UpdateTotalString();
            }
        }
        void AddNewMedia(string str)
        {
            currentPlaylist.mediaList.Add(new Media()
            {
                Order = stt += 1,
                File_Path = str,
                Duration_length = getDuration(str)
            }); ;
        }
        private void Play_Button_Click(object sender, RoutedEventArgs e)
        {

            if (!IsPlaying)
            {
                if (!IsPaused)
                {
                    if (currentPlaylist.mediaList.Count > 0)
                    {
                        if (currentMedia == null)
                        {
                            currentMedia = currentPlaylist.mediaList[0];
                        }
                        mediaPlayer.Open(new Uri(currentMedia.File_Path));
                    }
                }
                IsStopped = false;
                IsPaused = false;
                IsPlaying = true;
                mediaPlayer.Play();
                ((Storyboard)Resources["Storyboard"]).Begin();
            }
            else
            {
                mediaPlayer.Pause();
                IsStopped = false;
                IsPlaying = false;
                IsPaused = true;
                ((Storyboard)Resources["Storyboard"]).Pause();
            }
        }
        private void Next_Button_Click(object sender, RoutedEventArgs e)
        {
            if (queue + 1 >= currentPlaylist.mediaList.Count)
            {
                if (!IsLoop) return;
                queue = -1;
            }
            queue += 1;
            currentMedia = currentPlaylist.mediaList[queue];
            if (!IsPlaying) return;
            if (!File.Exists(currentMedia.File_Path))
            {
                MessageBox.Show("Media not Exists!");
                (sender as Button).RaiseEvent(new RoutedEventArgs(Button.ClickEvent, sender));
                return;
            }
            mediaPlayer.Stop();
            mediaPlayer.Open(new Uri(currentMedia.File_Path));
            mediaPlayer.Play();
        }
        private void Prev_Button_Click(object sender, RoutedEventArgs e)
        {
            if (queue - 1 < 0)
                return;
            queue -= 1;
            currentMedia = currentPlaylist.mediaList[queue];
            if (!IsPlaying) return;
            if (!File.Exists(currentMedia.File_Path))
            {
                MessageBox.Show("Media not Exists!");
                (sender as Button).RaiseEvent(new RoutedEventArgs(Button.ClickEvent, sender));
                return;
            }
            mediaPlayer.Stop();
            mediaPlayer.Open(new Uri(currentMedia.File_Path));
            mediaPlayer.Play();
        }
        //Loop Mode...
        private void Replay_Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as ToggleButton;
            if (btn.IsChecked == true)
            {
                IsLoop = true;
            }
            else IsLoop = false;
        }
        private void PreLoadPlayList()
        {
            var Path = AppDomain.CurrentDomain.BaseDirectory + "PlayLists\\";
            string[] file = Directory.GetFiles(Path, "*.pl");

            playlists.Clear();
            foreach (var i in file)
                playlists.Add(CreatePlaylist(i));
        }
        private void ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            PreLoadPlayList();
        }
        private void Combobox_playlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            var path = (cb.SelectedItem as Playlist).m_Path;
            LoadPlaylist(path);
            list.ItemsSource = currentPlaylist.mediaList;
        }
        private Playlist CreatePlaylist(string i)
        {
            Playlist pl = new Playlist();
            pl.m_Path = i;
            return pl;
        }
        private void Btn_NewPlaylist(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
            Process_Slider.Value = 0;

            Btn_Play.IsChecked = false;
            currentMedia = null;
            IsPlaying = false;
            IsPaused = false;
            IsStopped = true;

            queue = 0;
            stt = 0;
            currentPlaylist = new Playlist();
            list.ItemsSource = currentPlaylist.mediaList;
            CurrentMediaDetail = "";
            UpdateTotalString();
        }
        private void Btn_SavePlaylist(object sender, RoutedEventArgs e)
        {
            if (currentPlaylist == null) return;
            if (currentPlaylist.mediaList.Count == 0) return;
            if (currentMedia == null) currentMedia = currentPlaylist.mediaList[0];
            var Path = currentPlaylist.m_Path;
            SavePlaylist(Path);
        }
        void LoadPlaylist(string Path)
        {
            var doc = new XmlDocument();
            doc.Load(Path);
            Btn_Play.IsChecked = false;
            var root = doc.DocumentElement;
            #region Player
            IsPlaying = bool.Parse(root.Attributes["IsPlaying"].Value);
            IsPaused = bool.Parse(root.Attributes["IsPaused"].Value);
            IsStopped = bool.Parse(root.Attributes["IsStopped"].Value);
            IsLoop = bool.Parse(root.Attributes["IsLoop"].Value);
            stt = int.Parse(root.Attributes["Stt"].Value);
            queue = int.Parse(root.Attributes["Queue"].Value);
            Volume_Slider.Value = Double.Parse(root.Attributes["Volume"].Value);
            #endregion
            #region Current Media
            var currentMediaNode = root.ChildNodes[0];
            currentMedia = new Media();
            currentMedia.Order = int.Parse(currentMediaNode.Attributes["Order"].Value);
            currentMedia.File_Path = currentMediaNode.Attributes["Path"].Value;
            currentMedia.Duration_length = int.Parse(currentMediaNode.Attributes["Duration"].Value);
            #endregion
            #region Media List
            var m_stt = 0;
            var MediaList = root.ChildNodes[1];
            var m_Path = MediaList.Attributes["PlayListPath"].Value;
            currentPlaylist = CreatePlaylist(m_Path);
            foreach (XmlNode i in MediaList)
            {
                m_stt++;
                var media = new Media();
                media.Order = m_stt;
                media.File_Path = i.Attributes["Path"].Value;
                media.Duration_length = int.Parse(i.Attributes["Duration"].Value);
                currentPlaylist.mediaList.Add(media);
            }
            #endregion
            mediaPlayer.Open(new Uri(currentMedia.File_Path));
            Process_Slider.Value = Double.Parse(root.Attributes["Process"].Value);
            UpdateTotalString();
        }


        void SavePlaylist(string Path)
        {
            XmlDocument doc = new XmlDocument();
            #region Player
            var root = doc.CreateElement("Player");
            root.SetAttribute("IsPlaying", bool.FalseString);
            root.SetAttribute("IsStopped", IsStopped.ToString());
            root.SetAttribute("IsPaused", bool.TrueString);
            root.SetAttribute("IsLoop", IsLoop.ToString());
            root.SetAttribute("Stt", stt.ToString());
            root.SetAttribute("Queue", queue.ToString());
            root.SetAttribute("Volume", Volume_Slider.Value.ToString());
            root.SetAttribute("Process", process.ToString());
            doc.AppendChild(root);
            #endregion
            #region Current Media
            var currentMediaNode = doc.CreateElement("CurrentMedia");
            currentMediaNode.SetAttribute("Order", currentMedia.Order.ToString());
            currentMediaNode.SetAttribute("Path", currentMedia.File_Path);
            currentMediaNode.SetAttribute("Duration", currentMedia.Duration_length.ToString());
            currentMediaNode.SetAttribute("Position", Process_Slider.Value.ToString());
            root.AppendChild(currentMediaNode);
            #endregion
            #region Media List
            var mediaList = doc.CreateElement("MediaList");
            mediaList.SetAttribute("PlayListPath", currentPlaylist.m_Path);
            root.AppendChild(mediaList);
            foreach (var i in currentPlaylist.mediaList)
            {
                var mediaNode = doc.CreateElement("Media");
                mediaNode.SetAttribute("Order", i.Order.ToString());
                mediaNode.SetAttribute("Path", i.File_Path);
                mediaNode.SetAttribute("Duration", i.Duration_length.ToString());
                mediaList.AppendChild(mediaNode);
            }
            #endregion
            doc.Save(Path);
        }
        void UpdateTotalString()
        {
            int total = 0;
            foreach (var i in currentPlaylist.mediaList)
            {
                total += i.Duration_length;
            }
            Total_duration = TimeSpan.FromSeconds(total).ToString("hh':'mm':'ss");
        }
        //Shuffle Mode
        private void Shuffle_Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as ToggleButton;
            if (btn.IsChecked == false)
            {
                currentPlaylist.ShuffleOff();
                return;
            }
            currentPlaylist.Shuffle();
        }

        private void Volume_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double value = (sender as Slider).Value;
            mediaPlayer.Volume = value;
            if (value == 0)
            {
                Volumn_Mode_Btn.IsChecked = true;
            }
            else
            {
                Volumn_Mode_Btn.IsChecked = false;
            }
        }
        private void Volumn_Mode_Btn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as ToggleButton;
            if (btn.IsChecked == true)
            {
                volume_value = Volume_Slider.Value;
                Volume_Slider.Value = 0;
            }
            else
            {
                Volume_Slider.Value = volume_value;
            }
        }

        private void Process_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = (sender as Slider).Value;
            var value = slider * currentMedia.Duration_length / 100;

            mediaPlayer.Position = TimeSpan.FromSeconds(value);
        }

        private void Delete_Btn_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).DataContext;
            int index = list.Items.IndexOf(item);
            currentPlaylist.mediaList.RemoveAt(index);
            UpdateTotalString();
        }

    }
}
