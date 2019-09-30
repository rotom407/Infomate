using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.Threading;

namespace Infomate {
    class NetworkBarGraph : BarGraphElement {
        private int pingdivisiormax = 2;
        private int pingdivisor = 0;
        private int[] lastping1 = new int[10];
        private int lastping1ptr = 0;
        readonly object ping1lock = new object();
        private int[] lastping2 = new int[10];
        private int lastping2ptr = 0;
        readonly object ping2lock = new object();
        private int uploadspd = 0;
        private int downloadspd = 0;
        private int ping1avg = 0, ping1suc = 0, ping2avg = 0, ping2suc = 0;
        private double ping1scr = 1.0, ping2scr = 1.0;
        bool networkenabled = true;

        Ping pingsender1 = new Ping();
        Ping pingsender2 = new Ping();
        string pinghost1 = "www.bing.com";
        string pinghost2 = "www.google.com";
        string pingdata = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        byte[] pingbuffer;

        PerformanceCounter[] pcsent;
        PerformanceCounter[] pcreceived;

        public NetworkBarGraph() {
            BackgroundColor = Color.FromArgb(0, 0, 0);
            ForegroundBackgroundColor = Color.FromArgb(32, 32, 32);
            BackgroundMeter.ColorBegin = Color.FromArgb(0, 178, 148);
            BackgroundMeter.AnimationSpeed = 0.01;
            BackgroundMeter.ColorEnd = Color.FromArgb(255, 178, 148);
            ForegroundMeter.ColorBegin = Color.FromArgb(255, 255, 255);
            ForegroundMeter.ColorEnd = Color.FromArgb(255, 255, 255);
            ForegroundMeter.AnimationSpeed = 0.01;
            HighlightMeter.AlwaysInstant = true;

            pingbuffer = Encoding.ASCII.GetBytes(pingdata);
            pingsender1.PingCompleted += new PingCompletedEventHandler(Ping1CompletedCallback);
            pingsender2.PingCompleted += new PingCompletedEventHandler(Ping2CompletedCallback);

            PerformanceCounterCategory pcg = new PerformanceCounterCategory("Network Interface");
            string[] instances = pcg.GetInstanceNames();
            List<string> instancesmon = new List<string>();
            foreach(string inst in instances) {
                if (inst.Contains("VM")) {

                } else {
                    instancesmon.Add(inst);
                }
            }
            pcsent = new PerformanceCounter[instancesmon.Count];
            pcreceived = new PerformanceCounter[instancesmon.Count];
            for(int i = 0; i < instancesmon.Count; i++) {
                string instance = instancesmon[i];
                pcsent[i] = new PerformanceCounter("Network Interface", "Bytes Sent/sec", instance);
                pcreceived[i] = new PerformanceCounter("Network Interface", "Bytes Received/sec", instance);
            }
        }

        private void Ping1CompletedCallback(object sender, PingCompletedEventArgs e) {
            lock (ping1lock) {
                ping1scr = 0.95 * ping1scr;
                if (e.Cancelled || (e.Error != null)) {
                    lastping1[lastping1ptr] = -1;
                } else {
                    if (e.Reply.Status.ToString() != "Success") {
                        lastping1[lastping1ptr] = -1;
                    } else {
                        ping1scr += 0.05;
                        lastping1[lastping1ptr] = (int)e.Reply.RoundtripTime;
                    }
                }

                //Console.Write("ping1:");
                //Console.WriteLine(lastping1[lastping1ptr]);
                lastping1ptr = (lastping1ptr + 1) % 10;
            }
            
        }

        private void Ping2CompletedCallback(object sender, PingCompletedEventArgs e) {
            lock (ping2lock) {
                ping2scr = 0.95 * ping2scr;
                if (e.Cancelled || (e.Error != null)) {
                    lastping2[lastping2ptr] = -1;
                } else {
                    if (e.Reply.Status.ToString() != "Success") {
                        lastping2[lastping2ptr] = -1;
                    } else {
                        ping2scr += 0.05;
                        lastping2[lastping2ptr] = (int)e.Reply.RoundtripTime;
                    }
                }
                //Console.Write("ping2:");
                //Console.WriteLine(lastping2[lastping2ptr]);
                lastping2ptr = (lastping2ptr + 1) % 10;
            }
        }


        private string tomagstr(int num) {
            if (num < 0) {
                return num.ToString();
            } else {
                if (num < 1000) {
                    return num.ToString();
                } else if (num < 1000000) {
                    return (num / 1000).ToString() + "k";
                } else {
                    return (num / 1000000).ToString() + "M";
                }
            }
        }
        public override string GetString() {
            if (Shrink) {
                if (!networkenabled) {
                    return "N/A";
                } else {
                    return "570";
                }
            } else {
                if (!networkenabled) {
                    return "Network Unavailable";
                } else {
                    return string.Format("P:{0}/{1},{2}/{3} D:{4} U:{5}",ping1avg,(int)(ping1scr*99),ping2avg, (int)(ping2scr * 99), tomagstr(downloadspd),tomagstr(uploadspd));
                }
            }
        }

        public override void Initialize() {
            initialized = true;
        }

        public override void UpdateValues() {

            pingdivisor++;
            if (pingdivisor >= pingdivisiormax) {
                pingdivisor = 0;

                try {
                    pingsender1.SendAsync(pinghost1, 1000, pingbuffer, new PingOptions(127, true));
                    pingsender2.SendAsync(pinghost2, 1000, pingbuffer, new PingOptions(127, true));
                } catch (InvalidOperationException e){
                    
                }
                lock (ping1lock) {
                    ping1suc = 0;
                    ping1avg = 0;
                    for (int i = 0; i < 10; i++) {
                        if (lastping1[i] > 0) {
                            ping1suc++;
                            ping1avg += lastping1[i];
                        }
                    }
                    if (ping1suc > 0)
                        ping1avg = ping1avg / ping1suc;
                    else
                        ping1avg = 999;
                }
                lock (ping2lock) {
                    ping2suc = 0;
                    ping2avg = 0;
                    for (int i = 0; i < 10; i++) {
                        if (lastping2[i] > 0) {
                            ping2suc++;
                            ping2avg += lastping2[i];
                        }
                    }
                    if (ping2suc > 0)
                        ping2avg = ping2avg / ping2suc;
                    else
                        ping2avg = 999;
                }
                double ping1loss = 0.0;
                ping1loss = (ping1avg - 5.0)/100.0;
                ping1loss += (1.0 - ping1scr);
                if (ping1loss > 1.0) ping1loss = 1.0;
                if (ping1loss < 0.0) ping1loss = 0.0;
                double ping2loss = 0.0;
                ping2loss = (ping2avg - 100.0)/800.0;
                ping2loss += (1.0 - ping2scr);
                if (ping2loss > 1.0) ping2loss = 1.0;
                if (ping2loss < 0.0) ping2loss = 0.0;
                
                double pingloss = 0.3 * ping1loss + 0.7 * ping2loss;
                ForegroundMeter.Percent = (1.0 - pingloss);

                int sentbytes = 0;
                int recvbytes = 0;

                foreach (PerformanceCounter pc in pcsent) {
                    sentbytes += (int)pc.NextValue();
                }
                foreach (PerformanceCounter pc in pcreceived) {
                    recvbytes += (int)pc.NextValue();
                }
                uploadspd = sentbytes;
                downloadspd = recvbytes;
                //Console.WriteLine("Bytes Sent: {0}", sentbytes);
                //Console.WriteLine("Bytes Received: {0}", recvbytes);

                double speedmeterperc = ((double)uploadspd + (double)downloadspd)/1000000.0;
                if (speedmeterperc > 1.0) speedmeterperc = 1.0;
                BackgroundMeter.Percent = speedmeterperc;
            }
        }
    }
}
