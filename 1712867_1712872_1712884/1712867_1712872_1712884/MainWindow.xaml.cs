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
using System.Diagnostics;

namespace _1712867_1712872_1712884
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string PlayListHistoryFileName = "PlayListHistory.txt";
        //private MediaPlayer _mediaPlayer = new MediaPlayer();
        private BindingList<FileInfo> _playList = new BindingList<FileInfo>();
        private DispatcherTimer _timer = new DispatcherTimer();
        private int currentPlayerIndex = -1;
        private bool _isPlaying = false;
        private bool _isDragging = false;
        private bool _isShuffling = false;
        private bool _isShuffled = true;
        private bool _isLoop = false;
        private bool _isLoop1 = false;
        private List<int> _shuffleArr = new List<int>();
        private static Random _random = new Random();
        private NameConverter _nameConverter = new NameConverter();
        private List<int> _deletedPlayListArr = new List<int>();
        private bool _isDeleting = false;
        //private bool _isMediaEnded = false;
        private bool _isTicking = false;
        private IKeyboardMouseEvents _hook;
        private bool _isExcuted = false;


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
            if (e.Control && e.Shift && (e.KeyCode == Keys.P))
            {
                if (!this._isPlaying)
                {
                    this.playButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                }
            }

            //Pause
            if (e.Control && e.Shift && (e.KeyCode == Keys.S))
            {
                if (this._isPlaying)
                {
                    this.playButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                }
            }

            //Next
            if (e.Control && e.Shift && (e.KeyCode == Keys.N))
            {
                this.nextButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
            }

            //Previous
            if (e.Control && e.Shift && (e.KeyCode == Keys.Z))
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

            //Nếu chương trình có Shuffle
            if (this._isShuffling == false)
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
            else  //Nếu không có Shuffle
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
            //Dừng chương trình khi chạy hết _playList mà không có Loop
            this._isDragging = true;    //
            progressPlayer.Value = 0;   //
            mediaPlayer.Position = TimeSpan.FromSeconds(progressPlayer.Value);  //
            this._isDragging = false;    //
            this.playButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent)); //
        }

        /// <summary>
        /// GenerateRandom
        /// </summary>
        /// <param name="count"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
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
                this._isExcuted = true; //
                playerTime.Text = String.Format($"{mediaPlayer.Position.ToString(@"mm\:ss")} / {mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss")}");

                progressPlayer.Minimum = 0;
                progressPlayer.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                progressPlayer.Value = mediaPlayer.Position.TotalSeconds;

                var _currentAudioOrVideoname = _nameConverter.Convert(_playList[this.currentPlayerIndex].Name.ToString(), null, null, null).ToString();
                currentAudioOrVideoName1.Text = _currentAudioOrVideoname;
                currentAudioOrVideoName2.Text = _currentAudioOrVideoname;

                //Thực hiện shuffle nếu suffleButton được chọn mà chưa thực thi
                if (this._isShuffled == false)
                {
                    _shuffleArr.Add(this.currentPlayerIndex);
                    randomPlayList();
                    this._isShuffled = true;
                }


                //Hiển thị ảnh minh họa audio/video cho File mp3 hay mp4
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

                //Thực hiện việc tick vào CheckBox của những elements trong DataTemplate đã được tick trong lần gần nhất trước khi tắt chương trình
                if (this._isTicking == true)
                {
                    if (this._deletedPlayListArr.Count > 0)
                    {
                        for (int i = 0; i < this._deletedPlayListArr.Count; i++)
                        {
                            System.Windows.Controls.ListViewItem myListViewItem = (System.Windows.Controls.ListViewItem)(playListListView.ItemContainerGenerator.ContainerFromIndex(this._deletedPlayListArr[i]));

                            ContentPresenter myContentPresenter = FindVisualChild<ContentPresenter>(myListViewItem);

                            DataTemplate myDataTemplate = myContentPresenter.ContentTemplate;

                            //System.Windows.Controls.CheckBox target = (System.Windows.Controls.CheckBox)myDataTemplate.FindName("", myContentPresenter);

                            //target.IsChecked = true;

                            System.Windows.Controls.StackPanel myStackPanel = (System.Windows.Controls.StackPanel)myDataTemplate.FindName("playListStackPanel", myContentPresenter);

                            System.Windows.Controls.CheckBox target = (System.Windows.Controls.CheckBox)myStackPanel.Children[0];

                            target.IsChecked = true;

                            this._deletedPlayListArr.RemoveAt(this._deletedPlayListArr.Count - 1);
                        }
                    }
                    this._isTicking = false;
                }
            }
        }

        private void uploadButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == true)
            {
                this.resetPlayList();   //Reset lại PlayList trước khi chọn những audio/video mới
                foreach (var fileName in openFileDialog.FileNames)
                {
                    var player = new FileInfo(fileName);
                    this._playList.Add(player);
                }

                //
                this.currentPlayerIndex = 0;
                runMediaPlayer();   //Run media player tại vị trí currentPlayerIndex
                if (this._isShuffling == true)
                {
                    this._isShuffled = false;
                }
            }
        }

        private void resetPlayList()
        {
            if(this._isPlaying == true)
            {
                this.playButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
            }
            this.currentPlayerIndex = -1;
            this._isDeleting = false;
            this._playList.Clear();
            this._shuffleArr.Clear();
            this._deletedPlayListArr.Clear();


            //Thiết lập lại đồng hồ
            progressPlayer.Value = 0;
            progressPlayer.Maximum = 0.01;
            playerTime.Text = TimeSpan.FromSeconds(progressPlayer.Value).ToString(@"mm\:ss");
            playerTime.Text += " / ";
            playerTime.Text += TimeSpan.FromSeconds(progressPlayer.Maximum).ToString(@"mm\:ss");
        }

        private void playListListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var mediaPlayer = sender as System.Windows.Controls.ListView;
            if(mediaPlayer.SelectedIndex == -1)
            {
                return;
            }
            this.currentPlayerIndex = mediaPlayer.SelectedIndex;
            runMediaPlayer();

        }

        /*Run media player tại vị trí currentPlayerIndex*/
        private void runMediaPlayer()
        {
            this._isPlaying = true;
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
            //_isDragging = true để tiếp tục chạy sự kiện _timer_Tick mà không bị xung đột
            this._isDragging = true;
        }

        private void progressPlayer_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            //_isDragging = false để không bị xung đột khi chạy chương trình (xung đột trong sự kiện _timer_Tick
            this._isDragging = false;
            mediaPlayer.Position = TimeSpan.FromSeconds(progressPlayer.Value);
        }

        private void progressPlayer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            playerTime.Text = TimeSpan.FromSeconds(progressPlayer.Value).ToString(@"mm\:ss");
            playerTime.Text += " / ";
            playerTime.Text += TimeSpan.FromSeconds(progressPlayer.Maximum).ToString(@"mm\:ss");
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            if (this._isPlaying == false)
            {
                return;
            }

            //Nếu không có _isShuffling
            if (!this._isShuffling)
            {
                this.currentPlayerIndex++;

                //Nếu currentPlayerIndex >= this._playList.Count thì chạy chương trình tại vị trí currentPlayerIndex = 0 trong _playList
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
            else  //Nếu có _isShuffling
            {
                //Chạy tại vị trí sau vị trị hiện tại trong mảng _shuffleArr
                for (int i = 0; i < _shuffleArr.Count - 1; i++)
                {
                    if (this._shuffleArr[i] == this.currentPlayerIndex)
                    {
                        this.currentPlayerIndex = this._shuffleArr[i + 1];
                        mediaPlayer.Source = new Uri(this._playList[this.currentPlayerIndex].ToString(), UriKind.Absolute);
                        return;
                    }
                }

                //Chạy tại vị trí đầu của mảng _shuffleArr
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
                //Nếu không có _isShuffling
                if (!this._isShuffling)
                {
                    this.currentPlayerIndex--;

                    //Nếu this.currentPlayerIndex < 0 thì chạy tại vị trí cuối của _playList
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
                else  //Nếu có _isShuffling
                {
                    //Chạy tại vị trí trước vị trị hiện tại trong mảng _shuffleArr
                    for (int i = 1; i < _shuffleArr.Count; i++)
                    {
                        if (this._shuffleArr[i] == this.currentPlayerIndex)
                        {
                            this.currentPlayerIndex = this._shuffleArr[i - 1];
                            mediaPlayer.Source = new Uri(this._playList[this.currentPlayerIndex].ToString(), UriKind.Absolute);
                            return;
                        }
                    }

                    //Chạy tại vị trí cuối của mảng _shuffleArr
                    this.currentPlayerIndex = this._shuffleArr[this._shuffleArr.Count - 1];
                    mediaPlayer.Source = new Uri(this._playList[this.currentPlayerIndex].ToString(), UriKind.Absolute);

                }
            }
        }

        private void shuffleButton_Click(object sender, RoutedEventArgs e)
        {
            this._isShuffling = !this._isShuffling;

            if (this._isShuffling)
            {
                shuffleButton.Background = Brushes.Red;
                //_shuffleArr.Add(this.currentPlayerIndex);
                //randomPlayList();
                this._isShuffled = false;
            }
            else
            {
                shuffleButton.Background = Brushes.White;
                _shuffleArr.Clear();
                this._isShuffled = true;    //
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

            //Nếu có phần trong mảng _deletedPlayListArr thì hiện deleteButton
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

            //Nếu không có phần trong mảng _deletedPlayListArr thì ẩn deleteButton
            if (this._deletedPlayListArr.Count == 0)
            {
                deleteButton.Visibility = Visibility.Collapsed;
            }
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            this._isDeleting = true;    //Bật _isDeleting = true để không bị xung đột khi chương trình đang chạy (xung đột trong sự kiện _timer_Tick
            bool isDeletedPosition = false;
            int count = 0;

            //Sắp xếp lại mảng để xóa _deletedPlayListArr, _shuffleArr và _playList cho dễ
            sortDeletedPlayListArray(this._deletedPlayListArr);

            //Xóa _playList
            for (int i = this._deletedPlayListArr.Count - 1; i >= 0; i--)
            {
                if(this.currentPlayerIndex == this._deletedPlayListArr[i])
                {
                    isDeletedPosition = true;

                    continue;
                }

                this._playList.RemoveAt(this._deletedPlayListArr[i]);

                if (this._deletedPlayListArr[i] < this.currentPlayerIndex)
                {
                    this.currentPlayerIndex--;
                    count++;
                }
            }

            //Xóa _shuffleArr
            for (int i = this._deletedPlayListArr.Count - 1; i >= 0; i--)
            {
                if (this.currentPlayerIndex + count == this._deletedPlayListArr[i])
                {
                    isDeletedPosition = true;
                    continue;
                }

                for (int j = 0; j < this._shuffleArr.Count; j++)
                {
                    if (this._deletedPlayListArr[i] == this._shuffleArr[j])
                    {
                        var temp = this._shuffleArr[j];
                        this._shuffleArr.RemoveAt(j);

                        for (int k = 0; k < this._shuffleArr.Count; k++)
                        {
                            if (this._shuffleArr[k] > temp)
                            {
                                this._shuffleArr[k] = this._shuffleArr[k] - 1;
                            }
                        }
                        break;
                    }
                }
            }

            //Xóa _deletedPlayListArr
            this._deletedPlayListArr.Clear();

            //Thêm vị trí currentPlayerIndex vào _deletedPlayListArr để không thể xóa khi chương trình đang chạy
            if (isDeletedPosition == true)
            {
                this._deletedPlayListArr.Add(this.currentPlayerIndex);
            }

            //Ẩn deleteButton khi không có phần tử nào trong mảng _deletedPlayListArr cần xóa
            if (this._deletedPlayListArr.Count == 0)
            {
                deleteButton.Visibility = Visibility.Collapsed;
            }

            this._isDeleting = false;
        }

        /*Sắp xếp lại mảng deletedPlayListArr*/
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loadPlayListHistory();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            this._hook.KeyUp -= KeyUp_Hook;
            this._hook.Dispose();

            //Nếu chương trình đã thực thi
            if (this._isExcuted == true)
            {
                savePlayListHistory();
            }
        }

        /*Load data khi chương trình vừa chạy lên*/
        private void loadPlayListHistory()
        {
            var reader = new StreamReader(PlayListHistoryFileName);
            string str = "";

            //Load _playList
            str = reader.ReadLine();
            if(str == "PlayList:")
            {
                str = reader.ReadLine();
                while (str != "ShuffleArr:")
                {
                    this._playList.Add(new FileInfo(str));
                    str = reader.ReadLine();
                }
            }
            else
            {
                //Chưa Save
                return;
            }

            //Load _shuffleArr
            if (str == "ShuffleArr:")
            {
                str = reader.ReadLine();
                while (str != "DeletedPlayListArr:")
                {
                    this._shuffleArr.Add(int.Parse(str));
                    str = reader.ReadLine();
                }
            }

            //Load _deletedPlayListArr
            if (str == "DeletedPlayListArr:")
            {
                str = reader.ReadLine();
                while (str != "CurrentPlayerIndex:")
                {
                    this._deletedPlayListArr.Add(int.Parse(str));
                    str = reader.ReadLine();
                }
            }

            //Load currentPlayerIndex
            if (str == "CurrentPlayerIndex:")
            {
                str = reader.ReadLine();
                while (str != "IsPlaying:")
                {
                    this.currentPlayerIndex = int.Parse(str);
                    str = reader.ReadLine();
                }
            }

            //Load _isPlaying
            if (str == "IsPlaying:")
            {
                str = reader.ReadLine();
                while (str != "IsDragging:")
                {
                    this._isPlaying = bool.Parse(str);
                    str = reader.ReadLine();
                }
            }

            //Load _isDragging
            if (str == "IsDragging:")
            {
                str = reader.ReadLine();
                while (str != "IsShuffling:")
                {
                    this._isDragging = bool.Parse(str);
                    str = reader.ReadLine();
                }
            }

            //Load _isShuffling
            if (str == "IsShuffling:")
            {
                str = reader.ReadLine();
                while (str != "IsShuffled:")
                {
                    this._isShuffling = bool.Parse(str);
                    str = reader.ReadLine();
                }
            }

            //Load _isShuffled
            if (str == "IsShuffled:")
            {
                str = reader.ReadLine();
                while (str != "IsLoop:")
                {
                    this._isShuffled = bool.Parse(str);
                    str = reader.ReadLine();
                }
            }

            //Load _isLoop
            if (str == "IsLoop:")
            {
                str = reader.ReadLine();
                while (str != "IsLoop1:")
                {
                    this._isLoop = bool.Parse(str);
                    str = reader.ReadLine();
                }
            }

            //Load _isLoop1
            if (str == "IsLoop1:")
            {
                str = reader.ReadLine();
                while (str != "IsDeleting:")
                {
                    this._isLoop1 = bool.Parse(str);
                    str = reader.ReadLine();
                }
            }

            //Load _isDeleting
            if (str == "IsDeleting:")
            {
                str = reader.ReadLine();
                while (str != "State:")
                {
                    this._isDeleting = bool.Parse(str);
                    str = reader.ReadLine();
                }
            }

            //Load State
            if (str == "State:")
            {
                str = reader.ReadLine();
                progressPlayer.Value = double.Parse(str);
                str = reader.ReadLine();
                progressPlayer.Minimum = double.Parse(str);
                str = reader.ReadLine();
                progressPlayer.Maximum = double.Parse(str);
            }

            //Bật shuffleButton (nếu có)
            if (this._isShuffling == true)
            {
                shuffleButton.Background = Brushes.Red;
            }

            //Bật loopButton lần 1 (nếu có)
            if (this._isLoop == true)
            {
                loopButton.Background = Brushes.Red;
            }

            //Bật loopButton lần 2 (nếu có)
            if (this._isLoop1 == true)
            {
                loopButton.Background = Brushes.Red;
                loopImage.Source = new BitmapImage(new Uri("/Images/loop-1.png", UriKind.Relative));
            }

            if (this._deletedPlayListArr.Count > 0)
            {
                deleteButton.Visibility = Visibility.Visible;
                this._isTicking = true;
            }

            this.mediaPlayer.Source = new Uri(_playList[this.currentPlayerIndex].ToString(), UriKind.Absolute);
            this.mediaPlayer.Position = TimeSpan.FromSeconds(progressPlayer.Value);

            //Chạy lại chương trình lần gần nhất ta chạy trước khi tắt
            if (this._isPlaying == true)
            {
                this.mediaPlayer.Pause();
                this.mediaPlayer.Play();
                playImage.Source = new BitmapImage(new Uri("/Images/pause.png", UriKind.Relative));
                this.ResumeButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));

                //System.Threading.Thread.Sleep(1000);
                _timer.Start();

            }
            else
            {
                //this.mediaPlayer.Pause();
                playImage.Source = new BitmapImage(new Uri("/Images/play.png", UriKind.Relative));
                this.PauseButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));

                //System.Threading.Thread.Sleep(1000);
                _timer.Start();
            }

            reader.Close();
        }

        /*Save data trước khi chương trình tắt*/
        private void savePlayListHistory()
        {
            var writter = new StreamWriter(PlayListHistoryFileName);

            //Save _playList
            writter.WriteLine("PlayList:");
            for (int i = 0; i < this._playList.Count; i++)
            {
                writter.WriteLine(this._playList[i]);
            }

            //Save _shuffleArr
            writter.WriteLine("ShuffleArr:");
            for (int i = 0; i < this._shuffleArr.Count; i++)
            {
                writter.WriteLine(this._shuffleArr[i]);
            }

            //Save _deletedPlayListArr
            writter.WriteLine("DeletedPlayListArr:");
            for (int i = 0; i < this._deletedPlayListArr.Count; i++)
            {
                writter.WriteLine(this._deletedPlayListArr[i]);
            }

            //Save currentPlayerIndex
            writter.WriteLine("CurrentPlayerIndex:");
            writter.WriteLine(this.currentPlayerIndex);

            //Save _isPlaying
            writter.WriteLine("IsPlaying:");
            writter.WriteLine(this._isPlaying);

            //Save _isDragging
            writter.WriteLine("IsDragging:");
            writter.WriteLine(this._isDragging);

            //Save _isShuffling
            writter.WriteLine("IsShuffling:");
            writter.WriteLine(this._isShuffling);

            //Save _isShuffled
            writter.WriteLine("IsShuffled:");
            writter.WriteLine(this._isShuffled);

            //Save _isLoop
            writter.WriteLine("IsLoop:");
            writter.WriteLine(this._isLoop);

            //Save _isLoop1
            writter.WriteLine("IsLoop1:");
            writter.WriteLine(this._isLoop1);

            //Save _isDeleting
            writter.WriteLine("IsDeleting:");
            writter.WriteLine(this._isDeleting);

            //Save State
            writter.WriteLine("State:");
            writter.WriteLine(progressPlayer.Value);
            writter.WriteLine(progressPlayer.Minimum);
            writter.WriteLine(progressPlayer.Maximum);

            writter.Close();
        }

        /// <summary>
        ///         /*How to: Find DataTemplate-Generated Elements*/
        /// </summary>
        /// <typeparam name="childItem"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        private childItem FindVisualChild<childItem>(DependencyObject obj)
            where childItem : DependencyObject
        {
            for(int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                {
                    return (childItem)child;
                }
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }
            return null;
        }

<<<<<<< HEAD:1712867_1712872_1712884-MiniProject-MultimediaPlayer/1712867_1712872_1712884/1712867_1712872_1712884/MainWindow.xaml.cs
        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            var SaveWindowScreen = new SaveWindow();
            if (SaveWindowScreen.ShowDialog() == true)
            {
                var playlistName = SaveWindowScreen.PlaylistName;
                StreamWriter fileOut = new StreamWriter($"{playlistName}.txt");
                foreach (var song in _playList)
                {
                    Debug.WriteLine(song);
                    fileOut.WriteLine(song);
                }
                fileOut.Close();
            }
        }


        private void folderButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                var songs = File.ReadAllLines(openFileDialog.FileName);
                resetPlayList();
                foreach (var song in songs)
                {
                    _playList.Add(new FileInfo(song));
                }
                //playListListView.ItemsSource = _playList;

                //
                this.currentPlayerIndex = 0;
                runMediaPlayer();   //Run media player tại vị trí currentPlayerIndex
                if (this._isShuffling == true)
                {
                    this._isShuffled = false;
                }
=======
        private void FolderButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openFileDialog.ShowDialog()==true)
            {
                var songs = File.ReadAllLines(openFileDialog.FileName);
                resetPlayList();
                foreach(var song in songs)
                {
                    _playList.Add(new FileInfo(song));
                }
                playListListView.ItemsSource = _playList;
            }

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Window1 window = new Window1();
            if (window.ShowDialog() == true)
            {
                var playlistName = window.PlaylistName;
                StreamWriter fileOut = new StreamWriter($"{playlistName}.txt");
                foreach (var song in _playList)
                {
                    Debug.WriteLine(song);
                    fileOut.WriteLine(song);
                }
                fileOut.Close();
>>>>>>> aa3074c16e0be1d2e4e99d3924f4b13a1630b57c:1712867_1712872_1712884/1712867_1712872_1712884/MainWindow.xaml.cs
            }
        }
    }
}
