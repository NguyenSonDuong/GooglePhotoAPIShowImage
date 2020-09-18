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
    public partial class AddToken : Form
    {
        public delegate void FinishUpdate();
        public delegate void ErrorUpdate();
        public FinishUpdate fis;
        public ErrorUpdate err;
        public AddToken(FinishUpdate fis,ErrorUpdate err)
        {
            this.err = err;
            this.fis = fis;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
                return;
            try
            {
                File.WriteAllText(Form1.PATH_TOKEN_FILE, textBox1.Text);
                fis();
                this.Close();
            }
            catch(Exception ex)
            {
                err();
                this.Close();
            }
            
        }
    }
}
