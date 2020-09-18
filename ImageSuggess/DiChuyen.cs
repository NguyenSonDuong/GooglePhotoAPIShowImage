using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageSuggess
{
    public class DiChuyen
    {
        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;
        public Form form;
        public Point formLocationAnd;
        public Control control;

        public DiChuyen(Form form, Control control)
        {
            this.form = form;
            this.control = control;
            control.MouseDown += new MouseEventHandler(FormMain_MouseDown);
            control.MouseMove += new MouseEventHandler(FormMain_MouseMove);
            control.MouseUp += new MouseEventHandler(FormMain_MouseUp);
        }
        private void FormMain_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = form.Location;
        }

        private void FormMain_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                form.Location = Point.Add(dragFormPoint, new Size(dif));
            }
        }

        private void FormMain_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
            formLocationAnd = form.Location;
        }
    }
}
