using Gma.System.MouseKeyHook;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Forms;

namespace _1712867_1712872_1712884
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private MediaPlayer _mediaPlayer = new MediaPlayer();
        private BindingList<FileInfo> _playList = new BindingList<FileInfo>();
        private DispatcherTimer _timer = new DispatcherTimer();
        private int currentPlayerIndex = -1;
        private bool _isPlaying = false;
        private bool _isDragging = false;
        private bool _isShuffle = false;
        private bool _isLoop = false;
        private bool _isLoop1 = false;
        private List<int> _shuffleArr = new List<int>();
        private static Random _random = new Random();
        private NameConverter _nameConverter = new NameConverter();
        private List<int> _deletedPlayListArr = new List<int>();
        private bool _isDeleting = false;
        //private bool _isMediaEnded = false;
        private IKeyboardMouseEvents _hook;


        public MainWindow()
        {
            InitializeComponent();
            this.BeginButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
            this.PauseButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));

            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            playListListView.ItemsSource = _playList;

            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += _timer_Tick;

            //Đăng ký sự kiện Hook
            this._hook = Hook.GlobalEvents();
            this._hook.KeyUp += KeyUp_Hook;
        }

        private void KeyUp_Hook(object sender, System.Windows.Forms.KeyEventArgs e)
        {

            //Play
            if(e.Control && e.Shift && (e.KeyCode == Keys.P))
            {
                if (!this._isPlaying)
                {
                    this.playButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                }
            }

            //Pause
            if(e.Control && e.Shift && (e.KeyCode == Keys.S))
            {
                if (this._isPlaying)
                {
                    this.playButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                }
            }

            //Next
            if(e.Control && e.Shift && (e.KeyCode == Keys.N))
            {
                this.nextButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
            }

            //Previous
            if(e.Control && e.Shift && (e.KeyCode == Keys.Z))
            {
                this.backButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
            }
        }

        private void MediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            //_isMediaEnded = true;
            if (this._isLoop1 == true)
            {
                mediaPlayer.Source = new Uri(_playList[this.currentPlayerIndex].ToString(), UriKind.Absolute);
                return;
            }

            if (this._isShuffle == false)
            {
                this.currentPlayerIndex++;

                if (this._isLoop == false)
                {
                    if (this.currentPlayerIndex >= _playList.Count)
                    {
                        this.currentPlayerIndex--;

                        //
                        this.PauseButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                        endedMediaWithoutLoop();

                        return;
                    }
                    else
                    {
                        mediaPlayer.Source = new Uri(_playList[this.currentPlayerIndex].ToString(), UriKind.Absolute);
                    }
                }
                else
                {
                    if (this.currentPlayerIndex >= _playList.Count)
                    {
                        this.currentPlayerIndex = 0;
                    }
                    mediaPlayer.Source = new Uri(_playList[this.currentPlayerIndex].ToString(), UriKind.Absolute);
                }             
            }
            else
            {
                if (this._isLoop == false)
                {
                    for (int i = 0; i < _shuffleArr.Count - 1; i++)
                    {
                        if (this._shuffleArr[i] == this.currentPlayerIndex)
                        {
                            this.currentPlayerIndex = this._shuffleArr[i + 1];
                            mediaPlayer.Source = new Uri(this._playList[this.currentPlayerIndex].ToString(), UriKind.Absolute);
                            return;
                        }
                    }

                    //this.currentPlayerIndex = -1;   //

                    //
                    this.PauseButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                    endedMediaWithoutLoop();
                }
                else
                {
                    for (int i = 0; i < _shuffleArr.Count - 1; i++)
                    {
                        if (this._shuffleArr[i] == this.currentPlayerIndex)
                        {
                            this.currentPlayerIndex = this._shuffleArr[i + 1];
                            mediaPlayer.Source = new Uri(this._playList[this.currentPlayerIndex].ToString(), UriKind.Absolute);
                            return;
                        }
                    }

                    this.currentPlayerIndex = this._shuffleArr[0];
                    mediaPlayer.Source = new Uri(this._playList[this.currentPlayerIndex].ToString(), UriKind.Absolute);
                }
            }
        }

        private void endedMediaWithoutLoop()
        {
            this._isDragging = true;    //
            progressPlayer.Value = 0;   //
            mediaPlayer.Position = TimeSpan.FromSeconds(progressPlayer.Value);  //
            this._isDragging = false;    //
            this.playButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent)); //
        }

        public static List<int> GenerateRandom(int count, int min, int max)
        {

            //  initialize set S to empty
            //  for J := N-M + 1 to N do
            //    T := RandInt(1, J)
            //    if T is not in S then
            //      insert T in S
            //    else
            //      insert J in S
            //
            // adapted for C# which does not have an inclusive Next(..)
            // and to make it from configurable range not just 1.

            if (max <= min || count < 0 ||
                    // max - min > 0 required to avoid overflow
                    (count > max - min && max - min > 0))
            {
                // need to use 64-bit to support big ranges (negative min, positive max)
                throw new ArgumentOutOfRangeException("Range " + min + " to " + max +
                        " (" + ((Int64)max - (Int64)min) + " values), or count " + count + " is illegal");
            }

            // generate count random values.
            HashSet<int> candidates = new HashSet<int>();

            // start count values before max, and end at max
            for (int top = max - count; top < max; top++)
            {
                // May strike a duplicate.
                // Need to add +1 to make inclusive generator
                // +1 is safe even for MaxVal max value because top < max
                if (!candidates.Add(_random.Next(min, top + 1)))
                {
                    // collision, add inclusive max.
                    // which could not possibly have been added before.
                    candidates.Add(top);
                }
            }

            // load them in to a list, to sort
            List<int> result = candidates.ToList();

            // shuffle the results because HashSet has messed
            // with the order, and the algorithm does not produce
            // random-ordered results (e.g. max-1 will never be the first value)
            for (int i = result.Count - 1; i > 0; i--)
            {
                int k = _random.Next(i + 1);
                int tmp = result[k];
                result[k] = result[i];
                result[i] = tmp;
            }
            return result;
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            if ((this.mediaPlayer.Source != null) && (this.mediaPlayer.NaturalDuration.HasTimeSpan) && (!_isDragging) && (this.currentPlayerIndex != -1) && (!this._isDeleting) && (this._isPlaying))
            {
                playerTime.Text = String.Format($"{mediaPlayer.Position.ToString(@"mm\:ss")} / {mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss")}");

                progressPlayer.Minimum = 0;
                progressPlayer.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                progressPlayer.Value = mediaPlayer.Position.TotalSeconds;

                var _currentAudioOrVideoname = _nameConverter.Convert(_playList[this.currentPlayerIndex].Name.ToString(), null, null, null).ToString();
                currentAudioOrVideoName1.Text = _currentAudioOrVideoname;
                currentAudioOrVideoName2.Text = _currentAudioOrVideoname;


                if (!_playList[this.currentPlayerIndex].Name.ToString().Contains("mp3"))
                {
                    mediaPlayer.Visibility = Visibility.Visible;
                    rotatePictureStackPanel.Visibility = Visibility.Collapsed;
                    displayingScreenMode.Visibility = Visibility.Visible;
                    currentAudioOrVideoNameStackPanel.Margin = new Thickness(0, 0, 0, 0);
                }
                else
                {
                    mediaPlayer.Visibility = Visibility.Collapsed;
                    rotatePictureStackPanel.Visibility = Visibility.Visible;
                    displayingScreenMode.Visibility = Visibility.Collapsed;
                    currentAudioOrVideoNameStackPanel.Margin = new Thickness(0, 15, 0, 0);
                }
            }
        }

        private void uploadButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == true)
            {
                //var player = new FileInfo(openFileDialog.FileName);

                //this._playList.Add(player);

                foreach (var fileName in openFileDialog.FileNames)
                {
                    var player = new FileInfo(fileName);
                    this._playList.Add(player);
                }
            }
        }


        private void playListListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this._isPlaying = true;
            var mediaPlayer = sender as System.Windows.Controls.ListView;
            this.currentPlayerIndex = mediaPlayer.SelectedIndex;

            this.mediaPlayer.Source = new Uri(_playList[currentPlayerIndex].ToString(), UriKind.Absolute);

            playImage.Source = new BitmapImage(new Uri("/Images/pause.png", UriKind.Relative));
            this.mediaPlayer.Play();

            //
            this.ResumeButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));

            System.Threading.Thread.Sleep(1000);
            _timer.Start();
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.Source == null)
            {
                //MessageBox.Show("No player selected...");
                return;
            }

            if (this._isPlaying == true)
            {
                this.mediaPlayer.Pause();
                playImage.Source = new BitmapImage(new Uri("/Images/play.png", UriKind.Relative));

                this.PauseButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
            }
            else
            {
                this.mediaPlayer.Play();
                playImage.Source = new BitmapImage(new Uri("/Images/pause.png", UriKind.Relative));

                this.ResumeButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
            }

            this._isPlaying = !this._isPlaying;
        }

        private void progressPlayer_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            this._isDragging = true;
        }

        private void progressPlayer_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            this._isDragging = false;
            mediaPlayer.Position = TimeSpan.FromSeconds(progressPlayer.Value);
        }

        private void progressPlayer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            playerTime.Text = TimeSpan.FromSeconds(progressPlayer.Value).ToString(@"mm\:ss");
            playerTime.Text += " /";
            playerTime.Text += TimeSpan.FromSeconds(progressPlayer.Maximum).ToString(@"mm\:ss");
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            if (this._isPlaying == false)
            {
                return;
            }

            if (!this._isShuffle)
            {
                this.currentPlayerIndex++;

                if (this.currentPlayerIndex >= this._playList.Count)
                {
                    this.currentPlayerIndex = 0;
                    mediaPlayer.Source = new Uri(_playList[this.currentPlayerIndex].ToString(), UriKind.Absolute);
                }
                else
                {
                    mediaPlayer.Source = new Uri(_playList[this.currentPlayerIndex].ToString(), UriKind.Absolute);
                }
            }
            else
            {
                for (int i = 0; i < _shuffleArr.Count - 1; i++)
                {
                    if (this._shuffleArr[i] == this.currentPlayerIndex)
                    {
                        this.currentPlayerIndex = this._shuffleArr[i + 1];
                        mediaPlayer.Source = new Uri(this._playList[this.currentPlayerIndex].ToString(), UriKind.Absolute);
                        return;
                    }
                }

                this.currentPlayerIndex = this._shuffleArr[0];
                mediaPlayer.Source = new Uri(this._playList[this.currentPlayerIndex].ToString(), UriKind.Absolute);
            }

        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            if (this._isPlaying == false)
            {
                return;
            }

            if (progressPlayer.Value >= 5)
            {
                mediaPlayer.Source = new Uri(_playList[this.currentPlayerIndex].ToString(), UriKind.Absolute);
            }
            else
            {
                if (!this._isShuffle)
                {
                    this.currentPlayerIndex--;

                    if (this.currentPlayerIndex < 0)
                    {
                        this.currentPlayerIndex = _playList.Count - 1;
                        mediaPlayer.Source = new Uri(_playList[this.currentPlayerIndex].ToString(), UriKind.Absolute);
                    }
                    else
                    {
                        mediaPlayer.Source = new Uri(_playList[this.currentPlayerIndex].ToString(), UriKind.Absolute);
                    }
                }
                else
                {
                    for (int i = 1; i < _shuffleArr.Count; i++)
                    {
                        if (this._shuffleArr[i] == this.currentPlayerIndex)
                        {
                            this.currentPlayerIndex = this._shuffleArr[i - 1];
                            mediaPlayer.Source = new Uri(this._playList[this.currentPlayerIndex].ToString(), UriKind.Absolute);
                            return;
                        }
                    }

                    this.currentPlayerIndex = this._shuffleArr[this._shuffleArr.Count - 1];
                    mediaPlayer.Source = new Uri(this._playList[this.currentPlayerIndex].ToString(), UriKind.Absolute);

                }

            }
        }

        private void shuffleButton_Click(object sender, RoutedEventArgs e)
        {
            if (!this._isPlaying)
            {
                System.Windows.MessageBox.Show("Media not played yet", "Note");
                return;
            }

            this._isShuffle = !this._isShuffle;

            if (this._isShuffle)
            {
                shuffleButton.Background = Brushes.Red;
                _shuffleArr.Add(this.currentPlayerIndex);
                randomPlayList();
            }
            else
            {
                shuffleButton.Background = Brushes.White;
                _shuffleArr.Clear();
            }
        }

        private void randomPlayList()
        {
            List<int> rng = GenerateRandom(this._playList.Count, 0, this._playList.Count);


            for (int i = 0; i < rng.Count; i++)
            {
                if (rng[i] == this._playList.Count)
                {
                    rng.RemoveAt(i);
                    return;
                }
            }

            for (int i = 0; i < rng.Count; i++)
            {
                if (rng[i] != this.currentPlayerIndex)
                {
                    _shuffleArr.Add(rng[i]);
                }
            }
        }

        private void loopButton_Click(object sender, RoutedEventArgs e)
        {
            if (this._isLoop == false)
            {
                this._isLoop = true;
                loopButton.Background = Brushes.Red;
            }
            else
            {
                if (this._isLoop1 == false)
                {
                    this._isLoop1 = true;
                    loopImage.Source = new BitmapImage(new Uri("/Images/loop-1.png", UriKind.Relative));
                }
                else
                {
                    this._isLoop = this._isLoop1 = false;
                    loopButton.Background = Brushes.White;
                    loopImage.Source = new BitmapImage(new Uri("/Images/loop.png", UriKind.Relative));
                }

            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkedPlayer = sender as System.Windows.Controls.CheckBox;

            
            for(int i = 0; i < this._playList.Count; i++)
            {
                if(this._playList[i].FullName.ToString().Contains(checkedPlayer.DataContext.ToString()))
                {
                    this._deletedPlayListArr.Add(i);
                    break;
                }
            }

            if (this._deletedPlayListArr.Count > 0)
            {
                deleteButton.Visibility = Visibility.Visible;
            }

        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var uncheckedPlayer = sender as System.Windows.Controls.CheckBox;

            for (int i = 0; i < this._deletedPlayListArr.Count; i++)
            {
                if (this._playList[this._deletedPlayListArr[i]].FullName.ToString().Contains(uncheckedPlayer.DataContext.ToString()))
                {
                    this._deletedPlayListArr.RemoveAt(i);
                    break;
                }
            }

            if (this._deletedPlayListArr.Count == 0)
            {
                deleteButton.Visibility = Visibility.Collapsed;
            }
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            this._isDeleting = true;
            bool flag = false;
            int count = 0;
            sortDeletedPlayListArray(this._deletedPlayListArr);

            for (int i = this._deletedPlayListArr.Count - 1; i >= 0; i--)
            {
                if(this.currentPlayerIndex == this._deletedPlayListArr[i])
                {
                    flag = true;
                    continue;
                }

                this._playList.RemoveAt(this._deletedPlayListArr[i]);

                if (this._deletedPlayListArr[i] < this.currentPlayerIndex)
                {
                    this.currentPlayerIndex--;
                    count++;
                }

            }


            for (int i = this._deletedPlayListArr.Count - 1; i >= 0; i--)
            {
                if (this.currentPlayerIndex + count == this._deletedPlayListArr[i])
                {
                    flag = true;
                    continue;
                }

                for(int j = 0; j < this._shuffleArr.Count; j++)
                {
                    if (this._deletedPlayListArr[i] == this._shuffleArr[j])
                    {
                        this._shuffleArr.RemoveAt(j);
                    }
                }
            }

            this._deletedPlayListArr.Clear();
            if (flag == true)
            {
                this._deletedPlayListArr.Add(this.currentPlayerIndex);
            }

            if (this._deletedPlayListArr.Count == 0)
            {
                deleteButton.Visibility = Visibility.Collapsed;
            }

            this._isDeleting = false;
        }

        private void sortDeletedPlayListArray(List<int> deletedPlayListArr)
        {
            for (int i = 0; i < deletedPlayListArr.Count - 1; i++)
            {
                for (int j = i + 1; j < deletedPlayListArr.Count; j++)
                {
                    if(deletedPlayListArr[i] > deletedPlayListArr[j])
                    {
                        var temp = deletedPlayListArr[i];
                        deletedPlayListArr[i] = deletedPlayListArr[j];
                        deletedPlayListArr[j] = temp;
                    }
                }
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            this._hook.KeyUp -= KeyUp_Hook;
            this._hook.Dispose();
        }
    }
}
