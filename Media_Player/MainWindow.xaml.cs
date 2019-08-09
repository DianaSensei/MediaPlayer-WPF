using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;
using WMPLib;

namespace Media_Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public bool IsStopped = false;
        public bool IsPaused = false;
        public bool IsPlaying = false;
        List<Media> _players = new List<Media>();
        public MainWindow()
        {
            InitializeComponent();
     
        }

        public class Media : INotifyPropertyChanged
        {
            private string _filename;
            private string _filepath;

            public string filename
            {
                get => _filename; set
                {
                    _filename = value;
                    RaiseChangeEvent();
                }
            }

            public string filepath
            {
                get => _filepath; set
                {
                    _filepath = value;
                    RaiseChangeEvent();
                }
            }

            void RaiseChangeEvent([CallerMemberName]string name = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(filename));
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        private void Add_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                Filter = "(mp3, wav, mp4, mov, wmv, mpg, avi, 3gp, flv)| *.mp3; *.wav; *.mp4; *.3gp; *.avi; *.mov; *.flv; *.wmv; *.mpg | all files | *.* ",
                Multiselect = true
            };
            if (openFileDialog1.ShowDialog() == true)
            {
                string[] filesname = openFileDialog1.SafeFileNames;
                string[] filespath = openFileDialog1.FileNames;
                
                for (int i = 0; i < filesname.Length; i++)
                {
                    string lvi = i + 1 + ". " + filesname[i];
                    list.Items.Add(lvi);

                }
                for (int i = 0; i < filesname.Length; i++)
                {
                    _players.Add(new Media { filename = filesname[i], filepath = filespath[i] });
                }
            }


        }
        double cur = 0.0;
        private void Play_Button_Click(object sender, RoutedEventArgs e)
        {
            if (IsPlaying == false)
            {
                foreach (var _player in _players)
                {

                    Player.PlayStateChange += new WMPLib._WMPOCXEvents_PlayStateChangeEventHandler(Player_PlayStateChange);
                    Player.MediaError += new WMPLib._WMPOCXEvents_MediaErrorEventHandler(Player_MediaError);
                    Player.URL = _player.filepath;
                }
                IsPlaying = true;
            }
            else
            {
                if (IsStopped == false)
                {
                    Player.controls.currentPosition = cur;
                    Player.controls.play();
                    IsStopped = true;
                }
                else
                {
                    cur = Player.controls.currentPosition;
                    Player.controls.pause();
                    IsStopped = false;
                    IsPlaying = false;
                }
            }
        }
        private void Player_PlayStateChange(int NewState)
        {
            if ((WMPLib.WMPPlayState)NewState == WMPLib.WMPPlayState.wmppsStopped)
            {
                this.Close();
            }
        }

        private void List_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            WMPLib.WindowsMediaPlayer Player;
            foreach ( var _player in _players)
            {
                Player = new WMPLib.WindowsMediaPlayer();
                Player.PlayStateChange += new WMPLib._WMPOCXEvents_PlayStateChangeEventHandler(Player_PlayStateChange);
                Player.MediaError += new WMPLib._WMPOCXEvents_MediaErrorEventHandler(Player_MediaError);
                Player.URL = _player.filepath;
                Player.controls.play();
            }
        }

        private void Next10_Button_Click(object sender, RoutedEventArgs e)
        {
            Player.controls.currentPosition += 10;
        }

        private void Prev10_Button_Click(object sender, RoutedEventArgs e)
        {
            Player.controls.currentPosition -= 10;
        }

        private void Replay_Button_Click(object sender, RoutedEventArgs e)
        {
            Player.controls.currentPosition = 0;
        }


        private void Player_MediaError(object pMediaObject)
        {
            MessageBox.Show("Cannot play media file.");
            this.Close();
        }

        WMPLib.WindowsMediaPlayer Player = new WMPLib.WindowsMediaPlayer();
    }
}
