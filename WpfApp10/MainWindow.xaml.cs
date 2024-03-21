using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using YoutubeExplode;

namespace WpfApp10
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string folderPath = "";
        private TimeSpan _currentPosition;
        private bool isPlaying;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void downloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (UrlTextBox.Text == "" || UrlTextBox.Text == "Enter URL:")
            {
                ResultTextBlock.Text = "Enter video URL first";
                return;
            }

            string videoUrl = UrlTextBox.Text.Trim();
            string selectedQuality = ((ComboBoxItem)QualityComboBox.SelectedItem).Content.ToString();

            try
            {
                var youtube = new YoutubeClient();
                var video = await youtube.Videos.GetAsync(videoUrl);

                var streamInfoSet = await youtube.Videos.Streams.GetManifestAsync(video.Id);
                var streamInfo = streamInfoSet.GetMuxedStreams().FirstOrDefault(s => s.VideoQuality.ToString() == selectedQuality);

                if (streamInfo == null)
                {
                    ResultTextBlock.Text = "The selected quality is not available for this video";
                    return;
                }

                string outputFilePath = Path.Combine(folderPath, $"{video.Id}.{streamInfo.Container}");
                if (File.Exists(outputFilePath))
                {
                    int i = 1;
                    do {
                        outputFilePath = Path.Combine(folderPath, $"{video.Id}({i++}).{streamInfo.Container}");
                    } while (File.Exists(outputFilePath));
                }

                ResultTextBlock.Text = "Download started";
                await youtube.Videos.Streams.DownloadAsync(streamInfo, outputFilePath);

                ResultTextBlock.Text = "Video downloaded successfully: " + outputFilePath;

                VideoPlayer.Source = new Uri(outputFilePath);
                isPlaying = true;
                playButton_Click(sender, e);
            }
            catch (Exception ex)
            {
                ResultTextBlock.Text = "Error downloading video: " + ex.Message;
            }
        }

        private void selectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.Title = "Select folder";

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                folderPath = dialog.FileName;
            }

            if (folderPath != "") downloadButton.IsEnabled = true;
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isPlaying)
            {
                VideoPlayer.Position = _currentPosition;
                VideoPlayer.Play();
                playButton.Content = "Pause";
                isPlaying = true;
            }
            else
            {
                _currentPosition = VideoPlayer.Position;

                VideoPlayer.Pause();
                playButton.Content = "Play";
                isPlaying = false;
            }
        }

        private void againButton_Click(object sender, RoutedEventArgs e)
        {
            VideoPlayer.Stop();
            _currentPosition = new TimeSpan(0);
            isPlaying = false;
            playButton_Click(sender, e);
        }

        private void VideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            playButton.Visibility = Visibility.Visible;
            againButton.Visibility = Visibility.Visible;

            this.Width = 700;
            this.Height = 620;
        }

        private void UrlTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (UrlTextBox.Text == "Enter URL:") UrlTextBox.Text = "";
        }

        private void UrlTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (UrlTextBox.Text == "") UrlTextBox.Text = "Enter URL:";
        }
    }
}