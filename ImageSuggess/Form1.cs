using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using xNet;

namespace ImageSuggess
{
    public partial class Form1 : Form
    {
        public static String PATH_SETTING_PATH = "setting.option";
        public static String PATH_TOKEN_FILE = "setting.ini";
        public static String PATH_DATABASE = "data.db";
        public static String PARAMESTER_GETIMAGE = "?pageSize=100";
        #region Khai báo biến
        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, string pvParam, uint fWinIni);
        const uint SPI_SETDESKWALLPAPER = 0x14;
        const uint SPIF_UPDATEINIFILE = 0x01;
        WMPLib.WindowsMediaPlayer wplayer = new WMPLib.WindowsMediaPlayer();
        public delegate void StopTimer();
        String URL_IMG = "https://photoslibrary.googleapis.com/v1/mediaItems";
        List<PhotoInfor.Mediaitem> mediaitems;
        int position = 0;
        #endregion


        public Form1()
        {
            InitializeComponent();
            mediaitems = new List<PhotoInfor.Mediaitem>();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            DiChuyen runD = new DiChuyen(this, this.pictureBox1);
            Setting set = new Setting();
            if(set.ShowDialog() == DialogResult.OK)
            {
                if (isCheckFile())
                {
                    mediaitems = JsonConvert.DeserializeObject<List<PhotoInfor.Mediaitem>>(File.ReadAllText(PATH_DATABASE));
                    SetImageToPictureBox();
                    timer1.Start();
                }
                else
                {
                    GET(URL_IMG + PARAMESTER_GETIMAGE, () => {
                        MessageBox.Show("Đã tải thành công");
                        PhotoInfor.Mediaitem itemImage = mediaitems.ElementAt(new Random().Next(mediaitems.Count));
                        SetImage(itemImage);
                        File.WriteAllText(PATH_DATABASE, JsonConvert.SerializeObject(mediaitems));
                        SetImageToPictureBox();
                        timer1.Start();
                        String path = File.ReadAllText(PATH_SETTING_PATH);
                        playListMusic(path);
                    },
                    () => {
                        MessageBox.Show("Đã tải xong " + mediaitems.Count);
                        PhotoInfor.Mediaitem itemImage = mediaitems.ElementAt(new Random().Next(mediaitems.Count));
                        SetImage(itemImage);
                        File.WriteAllText(PATH_DATABASE, JsonConvert.SerializeObject(mediaitems));
                        SetImageToPictureBox();
                        timer1.Start();
                        String path = File.ReadAllText(PATH_SETTING_PATH);
                        playListMusic(path);
                    });
                }
                
            }
            
            
        }

        #region các hàm thực thi
        public void playListMusic(String path)
        {
            try
            {
                WMPLib.IWMPPlaylist playlist = wplayer.playlistCollection.newPlaylist("GirlPlay2");
                DirectoryInfo dic = new DirectoryInfo(path);
                List<FileInfo> files = new List<FileInfo>();
                files.AddRange(dic.EnumerateFiles());
                foreach (FileInfo item in files)
                {
                    WMPLib.IWMPMedia media;
                    media = wplayer.newMedia(item.FullName);
                    playlist.appendItem(media);
                }
                wplayer.currentPlaylist = playlist;
                wplayer.controls.play();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error play music: ");
            }
            
        }
        public bool isCheckFile()
        {
            if (!File.Exists(PATH_DATABASE) || String.IsNullOrEmpty(File.ReadAllText(PATH_DATABASE)) || File.ReadAllText(PATH_DATABASE).Length <=10)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public void SetImageToPictureBox()
        {
            Thread newThread = new Thread(() =>
            {
                PhotoInfor.Mediaitem item = mediaitems.ElementAt(new Random().Next(mediaitems.Count));
                try
                {
                    HttpRequest http = new HttpRequest();
                    http.Get(item.baseUrl);
                }catch(Exception ex)
                {
                    mediaitems.Clear();
                    GET(URL_IMG + PARAMESTER_GETIMAGE, () => {
                        MessageBox.Show("Đã tải thành công");
                        PhotoInfor.Mediaitem itemImage = mediaitems.ElementAt(new Random().Next(mediaitems.Count));
                        SetImage(itemImage);
                        File.WriteAllText(PATH_DATABASE, JsonConvert.SerializeObject(mediaitems));
                        timer1.Start();
                    },
                    ()=> {
                        MessageBox.Show("Đã tải lỗi. Vui lòng cập nhật lại token");
                        AddToken add = new AddToken(
                        ()=> {
                            MessageBox.Show("Cập nhật token thành công!");
                            Thread th = new Thread(()=> {
                                GET(URL_IMG + PARAMESTER_GETIMAGE, () =>
                                {
                                    MessageBox.Show("Đã tải thành công danh sách ảnh");
                                    PhotoInfor.Mediaitem itemImage = mediaitems.ElementAt(new Random().Next(mediaitems.Count));
                                    SetImage(itemImage);
                                    File.WriteAllText(PATH_DATABASE, JsonConvert.SerializeObject(mediaitems));
                                    timer1.Start();
                                }, () =>
                                {
                                    MessageBox.Show("Đã tải lỗi. ứng dụng sẽ được tắt vui lòng mở lại và thửu lại");
                                    Application.Exit();
                                });
                            });
                            th.IsBackground = true;
                            th.Start();
                        },
                        ()=> {

                        });
                        add.ShowDialog();
                    });
                    
                }
            });
            newThread.IsBackground = true;
            newThread.Start();
        }
        public delegate void Reponsive();
        public delegate void Error();
        public void GET(String url, Reponsive reponsive, Error err)
        {
            String token = File.ReadAllText(PATH_TOKEN_FILE);
            HttpRequest http = getRequest(token);
            try
            {
                String output = http.Get(url).ToString();
                PhotoInfor.Rootobject root = JsonConvert.DeserializeObject<PhotoInfor.Rootobject>(output);
                String nextPageToken = root.nextPageToken;
                mediaitems.AddRange(root.mediaItems);
                if (String.IsNullOrEmpty(nextPageToken))
                {
                    MessageBox.Show("Đã tải dữ liệu ảnh Google Photos thành công");
                    reponsive();
                }
                else
                {
                    GET(url + "&pageToken=" + nextPageToken, reponsive, err);
                }

            }
            catch (HttpException ex)
            {
                if (ex.Message.Contains("413"))
                {
                    reponsive();
                }
                else
                {
                    err();
                }
                
            }
        }
        public HttpRequest getRequest(String token)
        {
            HttpRequest http = new HttpRequest();
            http.AddHeader("Authorization", "Bearer " + token);
            return http;
        }
        public void SetImage(PhotoInfor.Mediaitem item)
        {
            Image img = null;
            try
            {
                img = Image.FromStream(new HttpRequest().Get(item.baseUrl + "=w700-h350").ToMemoryStream());
            }
            catch(Exception ex)
            {
                MessageBox.Show("Lỗi tải ảnh");
                timer1.Stop();
                SetImageToPictureBox();
                return;
            }
            
            if (img == null)
                return;
            using (Graphics g = Graphics.FromImage(img))
            {
                SolidBrush drawBrush = new SolidBrush(Color.White);
                SolidBrush draw = new SolidBrush(Color.FromArgb(100,0,0,0));
                SolidBrush draw2 = new SolidBrush(Color.FromArgb(170, 0, 0, 0));
                Font drawFont = new Font("Arial", 9);
                g.FillRectangle(draw, 0, 0, img.Width, 60);
                g.DrawString("ID: "+ convertString(item.id,img.Width, drawFont), drawFont, drawBrush, 10, 10);
                g.DrawString("Width: "+  img.Width + "Height: "+img.Height, drawFont, drawBrush, 10, 25);
                g.DrawString("FileName: "+ convertString(item.filename, img.Width, drawFont), drawFont, drawBrush, 10, 40);
                g.FillRectangle(draw2, 0, img.Height - 30, 30, 30);
                g.FillRectangle(draw2, img.Width-30, img.Height-30 , 30, 30);

            }
            int width = img.Width;
            int height = img.Height;
            Point lo = new Point();
            this.Invoke(new MethodInvoker(() =>
            {
                this.Width = width;
                this.Height = height;
                lo = this.Location;

            }));
            pictureBox1.Invoke(new MethodInvoker(() =>
            {
                pictureBox1.Location = new Point(0, 0);
                pictureBox1.Width = width;
                pictureBox1.Height = height;
                pictureBox1.Image = img;
            }));
            this.Invoke(new MethodInvoker(() =>
            {
                this.Location = lo;

            }));
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (mediaitems.Count <= 0)
                return;
            Thread t = new Thread(() =>
            {
                position = new Random().Next(mediaitems.Count);
                PhotoInfor.Mediaitem item = mediaitems.ElementAt(position);
                SetImage(item);
            });
            t.IsBackground = true;
            t.Start();
        }
        public String convertString(String s, int w, Font font)
        {
            if (String.IsNullOrEmpty(s)) 
                return "";
            if (s.Length < 10)
                return s;
            int count = ((2*s.Length) / 3) ;
            return s.Substring(0, 10) + " ...";
        }
        private void setBackground()
        {
            try
            {
                Thread t = new Thread(() =>
                {
                    string path = @"C:\ImageGirl";
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                    PhotoInfor.Mediaitem item = mediaitems.ElementAt(position);
                    HttpRequest http = new HttpRequest();
                    http.Get(item.baseUrl + "=d").ToFile(path + "\\temb.png");
                    SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path + "\\temb.png", SPIF_UPDATEINIFILE);
                });
                t.IsBackground = true;
                t.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        #endregion

        #region Delegate sự kiện
        private void tảiẢnhToolStripMenuItem_Click(object sender, EventArgs e)
        {

            int pos = position;  
            SaveFileDialog file = new SaveFileDialog();
            file.Filter = "PNG files (*.png)|*.png|JPEG files (*.*)|*.jpg|All files (*.*)|*.*";
            file.FileName = "Girl.png";
            if (file.ShowDialog() == DialogResult.OK)
            {
                Thread t = new Thread(() =>
               {
                   try
                   {
                       PhotoInfor.Mediaitem item = mediaitems.ElementAt(pos);
                       HttpRequest http = new HttpRequest();
                       http.Get(item.baseUrl + "=d").ToFile(file.FileName);
                   }
                   catch (Exception ex)
                   {
                       MessageBox.Show("Error: " + ex.Message);
                   }
               });
                t.IsBackground = true;
                t.Start();
                
            }
        }
        private void setBackgroundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            setBackground();
        }
        
        private void onTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(this.TopMost)
                this.TopMost = false;
            else
                this.TopMost = true;
        }
        
        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if((e.X > 0 && e.X < 30) && (e.Y>pictureBox1.Height-30))
            {
                Thread t = new Thread(() =>
                {
                    position --;
                    PhotoInfor.Mediaitem item = mediaitems.ElementAt(position);
                    SetImage(item);

                });
                t.IsBackground = true;
                t.Start();
            }
            else
            if((e.X>pictureBox1.Width-30) && (e.Y > pictureBox1.Height-30))
            {
                Thread t = new Thread(() =>
                {
                    position ++;
                    PhotoInfor.Mediaitem item = mediaitems.ElementAt(position);
                    SetImage(item);

                });
                t.IsBackground = true;
                t.Start();
            }
        }
        private void pictureBox1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if(e.KeyCode == Keys.Right)
            {
                Thread t = new Thread(() =>
                {
                    position++;
                    PhotoInfor.Mediaitem item = mediaitems.ElementAt(position);
                    SetImage(item);

                });
                t.IsBackground = true;
                t.Start();
            }
            else
            if(e.KeyCode == Keys.Left)
            {
                Thread t = new Thread(() =>
                {
                    position--;
                    PhotoInfor.Mediaitem item = mediaitems.ElementAt(position);
                    SetImage(item);

                });
                t.IsBackground = true;
                t.Start();
            }
        }
        

        private void setFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            Setting set = new Setting(()=> {
                timer1.Start();
            });
            set.setIsReset(true);
            set.ShowDialog();
            String path = File.ReadAllText(PATH_SETTING_PATH);
            playListMusic(path);
        }
        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            wplayer.controls.stop();
        }
        #endregion
        
    }
}



public class PhotoInfor
{

    public class Rootobject
    {
        public Mediaitem[] mediaItems { get; set; }
        public string nextPageToken { get; set; }
    }

    public class Mediaitem
    {
        public string id { get; set; }
        public string description { get; set; }
        public string productUrl { get; set; }
        public string baseUrl { get; set; }
        public string mimeType { get; set; }
        public Mediametadata mediaMetadata { get; set; }
        public string filename { get; set; }
    }

    public class Mediametadata
    {
        public DateTime creationTime { get; set; }
        public string width { get; set; }
        public string height { get; set; }
        public Photo photo { get; set; }
    }

    public class Photo
    {
    }

}