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
using System.Text.RegularExpressions;
using VideoLibrary;
using System.Net.Http;
using System.IO;
using Microsoft.Win32;

namespace Final
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HttpClient httpClient = new HttpClient();
        private YouTube youTube = new YouTube();
        
        private string videoQuality = "";
        private string mediaFormat = "";
        private string pathFile = "";
        private string viewPathFile = "";
       

        public MainWindow()
        {
            InitializeComponent();
        }


        private bool checkURL()
        {
            Regex ytRegex = new Regex("([https:\\/\\/www.youtube.com\\/watch].*)");
            string URL = txtURL.Text;
            if (ytRegex.IsMatch(URL)) { return true; }
            else {
                MessageBox.Show("You Entered a URL format that is not accepted");
                return false;
            }
        }


        async Task Downloading(YouTubeVideo video)
        {
            //var video = youTube.GetVideo(URL);
            HttpClient http = new HttpClient();
            long? totalByte = 0;
            //File.WriteAllBytes(pathFile, await video.GetBytesAsync());

            using (Stream output = File.OpenWrite(pathFile))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Head, video.Uri))
                {
                    totalByte = http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result.Content.Headers.ContentLength;
                }
                using (var input = await http.GetStreamAsync(video.Uri))
                {
                    byte[] buffer = new byte[16 * 1024];
                    int read;
                    int totalRead = 0;
                    lblDownloadTxt.Content = "Download Started";
                    while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        output.Write(buffer, 0, read);
                        totalRead += read;
                        pgbar.Value += read;
                        lblDownloadTxt.Content = "Downloading" + (totalRead/totalByte) + "...";
                    }
                    lblDownloadTxt.Content= "Download Complete";
                }
            }




        }



        async Task DownloadSpecific(string URL)
        {
            var videoData = youTube.GetAllVideos(URL);
            var Video = videoData.Where(yt => yt.AdaptiveKind == AdaptiveKind.Video && yt.Format == VideoFormat.Mp4 && yt.Resolution.ToString() == videoQuality).Select(yt => yt);

            var audio = videoData.First(yt => yt.AudioFormat == AudioFormat.Aac);
            //var audio = videoData.Where(yt => yt.AdaptiveKind == AdaptiveKind.Audio).Select(yt => yt.AudioBitrate);
            
            switch (mediaFormat)
            {
                case "MP3":
                    Task doMP3Download = Downloading(audio);

                    break;
                case "MP4":
                    Task doMP4Download = Downloading(Video.ToList()[0]);
                    break;
            
            }



        }


        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            string URL = txtURL.Text;

            if (txtURL.Text == null)
            {
                throw new ApplicationException("Enter a URL to download");
                MessageBox.Show("Invalid URL or entry");
            }

            if (checkURL() == true)
            {
                DownloadSpecific(URL);
                MessageBox.Show("Download Complete");
            }
  
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog showDialog = new SaveFileDialog();
            showDialog.Filter = "MP4 File (*.mp4)|*.mp4|MP3 File|*.mp3";
            //showDialog.Title = "Save an MP4 File";
            Nullable<bool> show = showDialog.ShowDialog();
            

            if (show == true)
            {
                pathFile = showDialog.FileName;
                txtDirectory.Text = pathFile;
            }


        }

        private void VideoQuality_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            videoQuality = ((ComboBoxItem)(((ComboBox)sender).SelectedItem)).Content.ToString();
        }

        private void MediaFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mediaFormat = ((ComboBoxItem)(((ComboBox)sender).SelectedItem)).Content.ToString();
        }

        private void btnVideo_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            Nullable<bool> open = openFile.ShowDialog();

            openFile.Title= "Select a video File";
            openFile.Filter = "Media Files|*.mpg;*.avi;*.wma;*.mov;*.wav;*.mp2;*.mp3|All Files|*.*";

            if (open == true)
            {
                viewPathFile = openFile.FileName;
            }

            VideoDisplay.Source = new Uri(viewPathFile);

        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            VideoDisplay.LoadedBehavior = MediaState.Manual;
            VideoDisplay.UnloadedBehavior = MediaState.Play;
            VideoDisplay.Play();
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            VideoDisplay.LoadedBehavior = MediaState.Manual;
            VideoDisplay.UnloadedBehavior = MediaState.Pause;
            VideoDisplay.Pause();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            VideoDisplay.LoadedBehavior = MediaState.Manual;
            VideoDisplay.UnloadedBehavior = MediaState.Stop;
            VideoDisplay.Stop();
        }
    }





}

      




