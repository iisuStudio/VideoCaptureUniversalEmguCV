using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.Util;
using Windows.Storage;
using Windows.Storage.Pickers;

// 空白頁項目範本已記錄在 http://go.microsoft.com/fwlink/?LinkId=234238

namespace VideoCaptureUniversalDemo
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class MainPage : Page
    {

        
        private Capture _capture = null;
        Image<Bgr, Byte> frame;
             
        int cam = 0;
        double webcam_frm_cnt = 0;
        double FrameRate = 0;
        double TotalFrames = 0;
        double Framesno = 0;
        double codec_double = 0;


        public MainPage()
        {
            this.InitializeComponent();
        }

        private void ReleaseData()
        {
            if (_capture != null)
                _capture.Dispose();
        }

        private async void ProcessFrame(object sender, Windows.ApplicationModel.SuspendingEventArgs arg)
        {
            try
            {
                Framesno = _capture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_POS_FRAMES);
                
                frame = _capture.QueryFrame();

                if (frame != null)
                {
                    
                    pictureBox1.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(frame.ToBitmap());
                    if (cam == 0)
                    {
                        Video_seek.Value = (int)(Framesno);
                        
                        double time_index = _capture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_POS_MSEC);
                        //Time_Label.Text = "Time: " + TimeSpan.FromMilliseconds(time_index).ToString().Substring(0, 8);

                        double framenumber = _capture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_POS_FRAMES);
                        //Frame_lbl.Text = "Frame: " + framenumber.ToString();

                        //Thread.Sleep((int)(1000.0 / FrameRate)); http://stackoverflow.com/questions/12641223/thread-sleep-replacement-in-net-for-windows-store
                        await Task.Delay(TimeSpan.FromSeconds((int)(10.0 / FrameRate)));
                    }

                    if (cam == 1)
                    {
                        //Frame_lbl.Text = "Frame: " + (webcam_frm_cnt++).ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                
                //MessageBox.Show(ex.Message.ToString());
            }
        }

        private void Video_seek_Scroll(object sender, EventArgs e)
        {
            if (_capture != null)
            {
                if (_capture.GrabProcessState == System.Threading.Tasks.TaskStatus.Running) //System.Threading.ThreadState.Running
                {
                    _capture.Pause();
                    while (_capture.GrabProcessState == System.Threading.Tasks.TaskStatus.Running) ;//do nothing wait for stop
                    _capture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_POS_FRAMES, Video_seek.Value);
                    _capture.Start();
                }
                else
                {
                    _capture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_POS_FRAMES, Video_seek.Value);
                    ProcessFrame(null, null);
                }
            }
        }

        private async void button1_Click_1(object sender, RoutedEventArgs e)
        {
            if (comboBox1.SelectedItem.ToString() == "")
            {
                //MessageBox.Show("Select Capture Method");
            }
            else
                if (button1.Content.ToString() == "Play")
                {
                    #region cameracapture
                    if (comboBox1.SelectedItem.ToString() == "Capture From Camera")
                    {
                        try
                        {
                            _capture = null;
                            _capture = new Capture(0);
                            _capture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FPS, 30);
                            _capture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT, 240);
                            _capture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_WIDTH, 320);

                            //Time_Label.Text = "Time: ";
                            //Codec_lbl.Text = "Codec: ";
                            //Frame_lbl.Text = "Frame: ";

                            webcam_frm_cnt = 0;
                            cam = 1;
                            Video_seek.Value = 0;

                            Application.Current.Suspending += new SuspendingEventHandler(ProcessFrame); //https://docs.microsoft.com/en-us/windows/uwp/launch-resume/suspend-an-app
                            button1.Content = "Stop";
                            comboBox1.IsEnabled = false;
                        }
                        catch (NullReferenceException excpt)
                        {
                            //MessageBox.Show(excpt.Message);
                        }
                    }
                    #endregion cameracapture

                    #region filecapture
                    if (comboBox1.SelectedItem.ToString() == "Capture From File")
                    {
                        

                        //openFileDialog1.Filter = "MP4|*.mp4"; https://msdn.microsoft.com/library/windows/apps/br207847?cs-save-lang=1&cs-lang=csharp#code-snippet-2
                        //openFileDialog1.FileName = "";

                        FileOpenPicker picker = new FileOpenPicker();
                        picker.ViewMode = PickerViewMode.Thumbnail;
                        picker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
                        picker.FileTypeFilter.Add("MP4|*.mp4");

                        //if (openFileDialog1.ShowDialog() == DialogResult.OK)
                        StorageFile file = await picker.PickSingleFileAsync();
                        if(file != null)
                        {
                            try
                            {
                                _capture = null;
                                _capture = new Capture(file.Name);
                                _capture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT, 240);
                                _capture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_WIDTH, 320);
                                FrameRate = _capture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FPS);
                                TotalFrames = _capture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_COUNT);
                                codec_double = _capture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FOURCC);
                                byte[] b = BitConverter.GetBytes(Convert.ToUInt32(codec_double));
                                string s = new string(System.Text.Encoding.UTF8.GetString(b, 0, b.Length).ToCharArray());
                                //Codec_lbl.Text = "Codec: " + s;
                                cam = 0;
                                Video_seek.Minimum = 0;
                                Video_seek.Maximum = (int)TotalFrames - 1;
                                Application.Current.Suspending += new SuspendingEventHandler(ProcessFrame); //https://docs.microsoft.com/en-us/windows/uwp/launch-resume/suspend-an-app
                                button1.Content = "Stop";
                                comboBox1.IsEnabled = false;
                            }
                            catch (NullReferenceException excpt)
                            {
                                //MessageBox.Show(excpt.Message);
                            }
                        }
                    }
                    #endregion filecapture
                }
                else
                    #region stopcapture
                    if (button1.Content.ToString() == "Stop")
                    {
                        _capture.Stop();

                        Application.Current.Suspending -= ProcessFrame;
                        ReleaseData();
                        button1.Content = "Play";
                        comboBox1.IsEnabled = true;
                        pictureBox1.Source = null; //將物件設為空白(blank)
                        if (cam == 1)
                        {
                            _capture.Dispose();
                            cam = 0;
                        }
                    }
                    #endregion stopcapture
        }
    }
}
