using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageSuggess
{
    public partial class Setting : Form
    {
        public delegate void startTime();
        public startTime start;
        public bool isReset = false;
        public void setIsReset(bool i)
        {
            isReset = i;
        }
        public Setting(startTime start)
        {
            this.start = start; 
            InitializeComponent();
        }
        public Setting()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog();
            if (folder.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = folder.SelectedPath;
                File.WriteAllText("setting.option", folder.SelectedPath);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                richTextBox1.Enabled = false;
                button2.Enabled = false;
            }
            else
            {
                richTextBox1.Enabled = true;
                button2.Enabled = true;
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Setting_Load(object sender, EventArgs e)
        {
            DiChuyen dc = new DiChuyen(this, panel1);
            if(!isReset)
                if (File.Exists("setting.option"))
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            
        }

        private void label3_Click(object sender, EventArgs e)
        {
            
            if(File.Exists("setting.option"))
                this.DialogResult = DialogResult.OK;
            else
                this.DialogResult = DialogResult.No;
            this.Close();
        }

        private void Setting_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(start != null)
            start();
        }
    }
}
