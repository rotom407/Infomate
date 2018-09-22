using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Infomate {

    public partial class FrmMain : Form {

        public FrmMain() {
            InitializeComponent();
            ContextMenu = MnuOptions;
        }

        private bool Fshrink = false;
        private int Fwidth = 0;
        private int Fheight = 0;
        private List<BarGraphElement> bargraphlist = new List<BarGraphElement>();
        private void Form1_Load(object sender, EventArgs e) {
            Rectangle rect = Screen.PrimaryScreen.Bounds;
            ShowInTaskbar = false;
            Width = Fwidth;
            Height = Fheight;
            Opacity = 0.80;
            Left = (int)(0.8 * rect.Width - Width);
            Top = (int)(0.8 * rect.Height - Height);
            TopMost = true;
            
            bargraphlist.Add(new BatteryBarGraph());
            bargraphlist.Add(new CPUMemoryBarGraph());
            int i = 0;
            foreach (BarGraphElement bge in bargraphlist) {
                bge.Initialize();
                bge.Boundary = new Rectangle(0, i*24, 200, 24);
                i++;
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e) {
            foreach(BarGraphElement bge in bargraphlist) {
                List<RectanglePrimitive> rectanglePrimitives = bge.GetPrimitives();
                foreach(RectanglePrimitive rp in rectanglePrimitives) {
                    SolidBrush br = new SolidBrush(rp.col);
                    e.Graphics.FillRectangle(br, rp.prim);
                }
                e.Graphics.DrawString(bge.GetString(), Font, Brushes.White, bge.BoundaryDisp.Left + 5, bge.BoundaryDisp.Top + 3);
                e.Graphics.DrawString(bge.GetString(), Font, Brushes.Black, bge.BoundaryDisp.Left + 5, bge.BoundaryDisp.Top + 2);
            }
        }

        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;

        private void Form1_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                dragging = true;
                dragCursorPoint = Cursor.Position;
                dragFormPoint = this.Location;
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e) {
            if (dragging) {
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(dif));
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e) {
            dragging = false;
            if (e.Button == MouseButtons.Right) {

            }
        }

        private void Form1_Doubleclick(object sender, MouseEventArgs e) {
            Fshrink = !Fshrink;
            foreach(BarGraphElement bge in bargraphlist) {
                bge.Shrink = !bge.Shrink;
            }
            TmrUpdateData_Timer(null, null);
        }

        private void TmrAnimation_Timer(object sender, EventArgs e) {
            RectangleF boundaryrf=new RectangleF(0,0,0,0);
            foreach(BarGraphElement bge in bargraphlist) {
                bge.UpdateAnimation(false);
                boundaryrf = RectangleF.Union(boundaryrf, bge.BoundaryDisp);
            }
            if ((int)boundaryrf.Width != Fwidth || (int)boundaryrf.Height != Fheight) {
                Fwidth = (int)boundaryrf.Width;
                Fheight = (int)boundaryrf.Height;
                Width = Fwidth;
                Height = Fheight;
            }
            Invalidate();
        }

        private void MenuItem1_Click(object sender, EventArgs e) {
            Application.Exit();
        }

        private void MenuItem2_Click(object sender, EventArgs e) {
            FrmConfig form2 = new FrmConfig();
            switch (form2.ShowDialog()) {
                case DialogResult.OK:

                    MessageBox.Show(form2.textBox1.Text);
                    break;
                case DialogResult.Cancel:
                    break;
            }
        }

        private void TmrUpdateData_Timer(object sender, EventArgs e) {
            foreach (BarGraphElement bge in bargraphlist) {
                bge.UpdateValues();
            }
        }
    }
}
