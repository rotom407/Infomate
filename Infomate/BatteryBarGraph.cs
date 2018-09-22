using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Win32.SafeHandles;

namespace Infomate {
    class BatteryBarGraph : BarGraphElement {

        Guid GUID_DEVICE_BATTERY = new Guid(0x72631E54, 0x78A4, 0x11D0, 0xBC, 0xF7, 0x00, 0xAA, 0x00, 0xB7, 0xB3, 0x2A);

        [StructLayout(LayoutKind.Sequential)]
        struct SP_DEVICE_INTERFACE_DATA {
            public Int32 cbSize;
            public Guid interfaceClassGuid;
            public Int32 flags;
            private readonly UIntPtr reserved;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct SP_DEVICE_INTERFACE_DETAIL_DATA {
            public int cbSize;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string DevicePath;
        }
        [StructLayout(LayoutKind.Sequential)]
        struct SP_DEVINFO_DATA {
            public uint cbSize;
            public Guid classGuid;
            public uint devInst;
            public IntPtr reserved;
        }
        [StructLayout(LayoutKind.Sequential)]
        struct BATTERY_WAIT_STATUS {
            public int BatteryTag;
            public int Timeout;
            public int PowerState;
            public int LowCapacity;
            public int HighCapacity;
        }
        [StructLayout(LayoutKind.Sequential)]
        struct BATTERY_STATUS {
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
        private double Clamp(double x) {
            if (x > 1.0) return 1.0;
            if (x < 0.0) return 0.0;
            return x;
        }

        private string batterypath = "";
        private BATTERY_STATUS batterystats=new BATTERY_STATUS();

        private string Getbatterypath() {
            IntPtr hdev;
            SP_DEVICE_INTERFACE_DATA did = new SP_DEVICE_INTERFACE_DATA();
            SP_DEVICE_INTERFACE_DETAIL_DATA didd = new SP_DEVICE_INTERFACE_DETAIL_DATA();
            uint cbReq = 0;
            int err = 0;
            did.cbSize = Marshal.SizeOf(did);
            didd.cbSize = /*Marshal.SizeOf(didd)*/4 + Marshal.SystemDefaultCharSize;
            hdev = SetupDiGetClassDevs(ref GUID_DEVICE_BATTERY, "", IntPtr.Zero, 0x2 | 0x10);
            err = Marshal.GetLastWin32Error();
            SetupDiEnumDeviceInterfaces(hdev, IntPtr.Zero, ref GUID_DEVICE_BATTERY, 1, ref did);
            err = Marshal.GetLastWin32Error();
            SetupDiGetDeviceInterfaceDetail(hdev, ref did, IntPtr.Zero, 0, ref cbReq, IntPtr.Zero);
            err = Marshal.GetLastWin32Error();
            SetupDiGetDeviceInterfaceDetail(hdev, ref did, ref didd, cbReq, ref cbReq, IntPtr.Zero);
            err = Marshal.GetLastWin32Error();
            return didd.DevicePath.Substring(0);
        }

        private BATTERY_STATUS Getbatterystatus(string path) {
            BATTERY_WAIT_STATUS bws;
            BATTERY_STATUS res = new BATTERY_STATUS();
            SafeFileHandle hBattery;
            int err = 0, outLen = 0;
            int wait = 1000, s = 0;
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
            hBattery.Close();
            return res;
        }

        public BatteryBarGraph() {
            BackgroundColor = Color.FromArgb(0, 0, 0);
            ForegroundBackgroundColor = Color.FromArgb(32, 32, 32);
            BackgroundMeter.ColorBegin = Color.FromArgb(0, 178, 148);
            BackgroundMeter.ColorEnd = Color.FromArgb(255, 178, 148);
            ForegroundMeter.ColorBegin = Color.FromArgb(255, 255, 255);
            ForegroundMeter.ColorEnd = Color.FromArgb(255, 255, 255);
            ForegroundMeter.AlwaysInstant = true;
            HighlightMeter.AlwaysInstant = true;
        }

        public override string GetString() {
            if (Shrink) {
                return String.Format("{0:F2}", batterystats.Capacity / 424.0);
            } else {
                if (batterystats.Rate < 0) {
                    return String.Format("{0:F2}% T={1:F1}h P={2:F1}W", batterystats.Capacity / 424.0, -(double)batterystats.Capacity / batterystats.Rate, Math.Abs(batterystats.Rate) / 1000.0);
                } else if (batterystats.Rate == 0) {
                    return String.Format("{0:F2}% P=0W", batterystats.Capacity / 424.0);
                } else {
                    return String.Format("{0:F2}% T={1:F1}h P={2:F1}W", batterystats.Capacity / 424.0, (42400.0 - (double)batterystats.Capacity) / batterystats.Rate, Math.Abs(batterystats.Rate) / 1000.0);
                }
            }
        }

        public override void Initialize() {
            batterypath = Getbatterypath();
            initialized = true;
        }

        public override void UpdateValues() {
            batterystats=Getbatterystatus(batterypath);
            ForegroundMeter.Percent = batterystats.Capacity / 42400.0;
            if (batterystats.Rate > 0) {
                BackgroundMeter.Percent = Clamp((batterystats.Rate - 29000) / 4000.0);
                BackgroundMeter.ColorEnd = Color.FromArgb(0, 178, 148);
            } else if (batterystats.Rate == 0) {
                BackgroundMeter.Percent = 0.0;
            } else {
                BackgroundMeter.Percent = Clamp((-batterystats.Rate - 2000) / 10000.0);
                BackgroundMeter.ColorEnd = Color.FromArgb(255, 178, 148);
            }
            HighlightMeter.Percent = 0.0;
            //Debug.WriteLine(batterystats.Voltage);
        }
    }
}
