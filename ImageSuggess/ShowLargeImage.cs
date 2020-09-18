using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageSuggess
{
    public partial class ShowLargeImage : Form
    {
        public delegate void StopTimer();
        public StopTimer stopTime;
        String url;
        public ShowLargeImage()
        {
            InitializeComponent();
        }
        public ShowLargeImage(String url, StopTimer stop)
        {
            this.url = url;
            this.stopTime = stop;
            InitializeComponent();
        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void ShowLargeImage_Load(object sender, EventArgs e)
        {
            pictureBox1.MouseWheel += new MouseEventHandler(pic_1_mouse_wheel);
            pictureBox1.ImageLocation = url;
            DiChuyen dc = new DiChuyen(this, pictureBox1);

            this.Width = Screen.PrimaryScreen.Bounds.Width;
            this.Height = Screen.PrimaryScreen.Bounds.Height;
            pictureBox1.Width = Screen.PrimaryScreen.Bounds.Width;
            pictureBox1.Height = Screen.PrimaryScreen.Bounds.Height;

        }

        public void pic_1_mouse_wheel(object obj, MouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                
                    Image img = pictureBox1.Image;
                    using (Graphics g = Graphics.FromImage(img))
                    {
                        double tile = img.Width / img.Height;
                        ResizeImage(img, img.Width * 2, img.Height * 2);
                    }
            }
        }
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private void ShowLargeImage_FormClosing(object sender, FormClosingEventArgs e)
        {
            stopTime();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            
        }

        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            this.Close();
        }
    }
}
