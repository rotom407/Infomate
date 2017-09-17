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

namespace Infomate
{

    public partial class Form1 : Form
    {
        Guid GUID_DEVICE_BATTERY = new Guid(0x72631E54, 0x78A4, 0x11D0, 0xBC, 0xF7, 0x00, 0xAA, 0x00, 0xB7, 0xB3, 0x2A);

        [StructLayout(LayoutKind.Sequential)]
        struct SP_DEVICE_INTERFACE_DATA
        {
            public Int32 cbSize;
            public Guid interfaceClassGuid;
            public Int32 flags;
            private UIntPtr reserved;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            public int cbSize;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string DevicePath;
        }
        /*[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 1)]
        struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            public int cbSize;
            public char devicePath;
        }*/
        
        [StructLayout(LayoutKind.Sequential)]
        struct SP_DEVINFO_DATA
        {
            public uint cbSize;
            public Guid classGuid;
            public uint devInst;
            public IntPtr reserved;
        }
        [StructLayout(LayoutKind.Sequential)]
        struct BATTERY_WAIT_STATUS
        {
            public int BatteryTag;
            public int Timeout;
            public int PowerState;
            public int LowCapacity;
            public int HighCapacity;
        }
        [StructLayout(LayoutKind.Sequential)]
        struct BATTERY_STATUS
        {
            public int PowerState;
            public int Capacity;
            public int Voltage;
            public int Rate;
        }


        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SetupDiGetClassDevs(
                                              ref Guid ClassGuid,
                                              [MarshalAs(UnmanagedType.LPTStr)] string Enumerator,
                                              IntPtr hwndParent,
                                              uint Flags
                                             );
        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern Boolean SetupDiEnumDeviceInterfaces(
           IntPtr hDevInfo,
           IntPtr devInfo,
           ref Guid interfaceClassGuid,
           UInt32 memberIndex,
           ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData
        );
        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern Boolean SetupDiGetDeviceInterfaceDetail(
           IntPtr hDevInfo,
           ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
           ref SP_DEVICE_INTERFACE_DETAIL_DATA deviceInterfaceDetailData,
           //ref byte[] deviceInterfaceDetailData,
           UInt32 deviceInterfaceDetailDataSize,
           ref UInt32 requiredSize,
           //ref SP_DEVINFO_DATA deviceInfoData
           IntPtr deviceInfoData
        );
        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern Boolean SetupDiGetDeviceInterfaceDetail(
           IntPtr hDevInfo,
           ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
           IntPtr deviceInterfaceDetailData,
           UInt32 deviceInterfaceDetailDataSize,
           ref UInt32 requiredSize,
           IntPtr deviceInfoData
        );
        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = true)]
        static extern void MoveMemory(IntPtr dest, IntPtr src, int size);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall,
            SetLastError = true)]
                public static extern SafeFileHandle CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr SecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile
        );
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            static extern bool DeviceIoControl(
            Microsoft.Win32.SafeHandles.SafeFileHandle hDevice,
            uint IoControlCode,
            ref int InBuffer,
            int nInBufferSize,
            ref int OutBuffer,
            int nOutBufferSize,
            ref int pBytesReturned,
            IntPtr Overlapped
        );
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool DeviceIoControl(
            Microsoft.Win32.SafeHandles.SafeFileHandle hDevice,
            uint IoControlCode,
            ref BATTERY_WAIT_STATUS InBuffer,
            int nInBufferSize,
            ref BATTERY_STATUS OutBuffer,
            int nOutBufferSize,
            ref int pBytesReturned,
            IntPtr Overlapped
        );
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();


        public Form1()
        {
            InitializeComponent();
            ContextMenu = contextMenu1;
        }

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        private int STACKHEIGHT = 24;
        private int STACKWIDTH = 200;
        private double Fwidthtarget = 200;
        private double Fwidthnow = 0;
        private double meterA = 0.0;
        private double meterB = 0.0;
        private double meterAnow = 0.0;
        private double meterBnow = 0.0;
        private String metertext = "";
        private byte meterBR = 0, meterBG = 0, meterBB = 0;
        private bool Fshrink = false;
        private BATTERY_STATUS batterystats;

        private void Form1_Load(object sender, EventArgs e)
        {
            Rectangle rect=Screen.PrimaryScreen.Bounds;
            ShowInTaskbar = false;
            Width = (int)Fwidthnow;
            Height = STACKHEIGHT;
            Opacity = 0.80;
            Left = (int)(0.8 * rect.Width - Width);
            Top = (int)(0.8 * rect.Height-Height);
            batterystats = getbatterystatus();
            TopMost = true;
        }

        /*protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            using (Pen selPen = new Pen(Color.Blue))
            {
                g.DrawRectangle(selPen, 10, 10, 50, 50);
            }
        }*/

        private void Form1_Paint(object sender,PaintEventArgs e)
        {
            SolidBrush backbr = new SolidBrush(Color.FromArgb(255, meterBR, meterBG, meterBB));
            SolidBrush frontbkbr = new SolidBrush(Color.FromArgb(255, 32, 32, 32));
            SolidBrush frontbr = new SolidBrush(frontcolor(batterystats.Capacity));
            Rectangle back = new Rectangle(0, 0, (int)(Width * meterBnow), Height);
            Rectangle frontbk = new Rectangle(2, 2, Width - 4, Height - 4);
            Rectangle front = new Rectangle(3, 3, (int)(meterAnow * (Width - 6)), Height - 6);
            //e.Graphics.Clear(Color.Black);
            e.Graphics.FillRectangle(backbr, back);
            e.Graphics.FillRectangle(frontbkbr, frontbk);
            e.Graphics.FillRectangle(frontbr, front);
            e.Graphics.DrawString(metertext, Font, Brushes.White, 5, 3);
            e.Graphics.DrawString(metertext, Font, Brushes.Black, 5, 2);
        }

        /*private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }*/
        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                dragging = true;
                dragCursorPoint = Cursor.Position;
                dragFormPoint = this.Location;
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(dif));
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
            if (e.Button == MouseButtons.Right)
            {
                
            }
        }

        private void Form1_Doubleclick(object sender, MouseEventArgs e)
        {

            if (Fshrink)
            {
                Fwidthtarget = STACKWIDTH;
            }
            else
            {
                Fwidthtarget = STACKWIDTH/4;
            }
            Fshrink = !Fshrink;
            timer2_Timer(null, null);
        }

        private Color frontcolor(int refvalueA)
        {
            return Color.FromArgb(255, 255, 255, 255);
        }
        private Color backcolor(int refvalueA)
        {
            return Color.FromArgb(255, 255, 255, 255);
        }
        private double clamp(double x)
        {
            if (x > 1.0) return 1.0;
            if (x < 0.0) return 0.0;
            return x;
        }
        private double ratewidthcol(int batrate,ref byte R,ref byte G,ref byte B)
        {
            double tmp;
            if (batrate > 0)
            {
                tmp = clamp((batrate - 29000) / 4000.0);
                R = 0; G = 178; B = 148;
            }
            else if (batrate == 0)
            {
                tmp=0.0;
                R = 0; G = 0; B = 0;
            }
            else
            {
                tmp = clamp((-batrate - 2000) / 10000.0);
                R = (byte)(tmp*255); G = 178; B = 148;
            }
            return tmp;
        }

        private void timer1_Timer(object sender, EventArgs e)
        {
            Fwidthnow = (2.0 * Fwidthnow + Fwidthtarget) / 3.0;
            Width = (int)Fwidthnow;
            meterAnow = meterA;
            meterBnow = (4.0 * meterBnow + meterB) / 5.0;
            Invalidate();
        }

        private void menuItem1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private BATTERY_STATUS getbatterystatus()
        {
            IntPtr hdev;
            SP_DEVICE_INTERFACE_DATA did=new SP_DEVICE_INTERFACE_DATA();
            SP_DEVICE_INTERFACE_DETAIL_DATA didd = new SP_DEVICE_INTERFACE_DETAIL_DATA();
            BATTERY_WAIT_STATUS bws;
            BATTERY_STATUS res= new BATTERY_STATUS();
            SafeFileHandle hBattery;
            uint cbReq = 0;
            int err, outLen=0;
            int wait = 1000, s = 0;
            string path;
            did.cbSize = Marshal.SizeOf(did);
            didd.cbSize = /*Marshal.SizeOf(didd)*/4 + Marshal.SystemDefaultCharSize;
            hdev = SetupDiGetClassDevs(ref GUID_DEVICE_BATTERY,"",IntPtr.Zero,0x2|0x10);
            err = Marshal.GetLastWin32Error();
            SetupDiEnumDeviceInterfaces(hdev, IntPtr.Zero,ref GUID_DEVICE_BATTERY, 1,ref did);
            err = Marshal.GetLastWin32Error();
            SetupDiGetDeviceInterfaceDetail(hdev, ref did, IntPtr.Zero, 0, ref cbReq, IntPtr.Zero);
            err = Marshal.GetLastWin32Error();
            SetupDiGetDeviceInterfaceDetail(hdev, ref did,ref didd, cbReq, ref cbReq, IntPtr.Zero);
            err = Marshal.GetLastWin32Error();
            path = didd.DevicePath.Substring(0);
            hBattery = CreateFile(path, 0x80000000 | 0x40000000, 0x1 | 0x2, IntPtr.Zero, 3, 0x80, IntPtr.Zero);
            err = Marshal.GetLastWin32Error();
            DeviceIoControl(hBattery, 2703424, ref wait, Marshal.SizeOf(wait), ref s, Marshal.SizeOf(s), ref outLen, IntPtr.Zero);
            err = Marshal.GetLastWin32Error();
            bws.BatteryTag = s;
            bws.Timeout = 10000;
            bws.PowerState = 0x1 | 0x2 | 0x4 | 0x8;
            bws.LowCapacity = 0;
            bws.HighCapacity = 10000;
            DeviceIoControl(hBattery, 2703436, ref bws, Marshal.SizeOf(bws), ref res, Marshal.SizeOf(res), ref outLen, IntPtr.Zero);
            err = Marshal.GetLastWin32Error();
            //MessageBox.Show(res.Capacity.ToString());
            return res;
        }

        private void timer2_Timer(object sender, EventArgs e)
        {
            batterystats = getbatterystatus();
            meterA = batterystats.Capacity/42400.0;
            meterB = ratewidthcol(batterystats.Rate, ref meterBR, ref meterBG, ref meterBB);
            if (Fshrink)
            {
                metertext = String.Format("{0:F2}", batterystats.Capacity / 424.0);
            }
            else
            {
                if (batterystats.Rate < 0)
                {
                    metertext = String.Format("{0:F2}% T={1:F1}h P={2:F1}W", batterystats.Capacity / 424.0, -(double)batterystats.Capacity / batterystats.Rate, Math.Abs(batterystats.Rate) / 1000.0);
                }
                else if (batterystats.Rate == 0)
                {
                    metertext = String.Format("{0:F2}% P=0W", batterystats.Capacity / 424.0);
                }
                else
                {
                    metertext = String.Format("{0:F2}% T={1:F1}h P={2:F1}W", batterystats.Capacity / 424.0, (42400.0-(double)batterystats.Capacity) / batterystats.Rate, Math.Abs(batterystats.Rate) / 1000.0);
                }
            }
                /*
            System.Management.ObjectQuery query = new ObjectQuery("Select * FROM Win32_Battery");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

            ManagementObjectCollection collection = searcher.Get();

            foreach (ManagementObject mo in collection)
            {
                foreach (PropertyData property in mo.Properties)
                {
                    //if(property.Name=="")
                    Debug.Print("Property {0}: Value is {1}", property.Name, property.Value);
                }
            }
            */
        }


    }
}
