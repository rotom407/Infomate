using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;
using Microsoft.VisualBasic.Devices;

namespace Infomate {
    class CPUMemoryBarGraph : BarGraphElement {
        List<PerformanceCounter> cpuCounters;
        PerformanceCounter diskCounter;
        PerformanceCounter ramCounter;
        int cpucores = 4;
        float cputotalpercent = 0.0f;
        float cpumaxpercent = 0.0f;
        float disktotalpercent = 0.0f;
        float ramfreenum = 0.0f;
        float ramtotalpercent = 0.0f;
        ulong totalramsize;
        public CPUMemoryBarGraph() {
            BackgroundColor = Color.FromArgb(0, 0, 0);
            ForegroundBackgroundColor = Color.FromArgb(32, 32, 32);
            BackgroundMeter.ColorBegin = Color.FromArgb(0, 178, 148);
            BackgroundMeter.AnimationSpeed = 0.01;
            BackgroundMeter.ColorEnd = Color.FromArgb(255, 178, 148);
            ForegroundMeter.ColorBegin = Color.FromArgb(255, 255, 255);
            ForegroundMeter.ColorEnd = Color.FromArgb(255, 255, 255);
            HighlightMeter.AlwaysInstant = true;
            ramCounter = new PerformanceCounter("Memory", "Available Bytes");
            diskCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
        }

        public override string GetString() {
            if (Shrink) {
                return String.Format("{0:F1}", ramtotalpercent);
            } else {
                return String.Format("M:{0:F2}% C:{1:F0}+{2:F0}% D:{3:F2}%", ramtotalpercent,cputotalpercent,cpumaxpercent-cputotalpercent,disktotalpercent);
            }
        }

        public override void Initialize() {
            foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get()) {
                cpucores = int.Parse(item["NumberOfLogicalProcessors"].ToString());
            }
            cpuCounters = new List<PerformanceCounter>();
            for (int i = 0; i < cpucores; i++) {
                try {
                    cpuCounters.Add(new PerformanceCounter("Processor", "% Processor Time", i.ToString()));
                }catch(Exception e) {
                    break;
                }
            }
            totalramsize = new ComputerInfo().TotalPhysicalMemory;
            initialized = true;
        }

        private double Clamp(double x) {
            if (x > 1.0) return 1.0;
            if (x < 0.0) return 0.0;
            return x;
        }

        public override void UpdateValues() {
            float newcputotalpercent = 0.0f;
            cpumaxpercent = 0.0f;
            foreach(var item in cpuCounters) {
                float nv = item.NextValue();
                newcputotalpercent+=nv;
                cpumaxpercent = Math.Max(cpumaxpercent, nv);
            }
            newcputotalpercent /= cpucores;
            cputotalpercent = newcputotalpercent;
            ramfreenum = ramCounter.NextValue();
            disktotalpercent = diskCounter.NextValue();
            ramtotalpercent = 100.0f*(1.0f - ramfreenum / totalramsize);
            BackgroundMeter.Percent = Clamp(cputotalpercent / 100.0);
            ForegroundMeter.Percent = Clamp(1.0-ramtotalpercent / 100.0);
            /*
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
            HighlightMeter.Percent = 0.0;*/
            //Debug.WriteLine(ramtotalpercent);
        }
    }
}
