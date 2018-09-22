using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Infomate {
    class MeterData {
        public double Percent=0.0;
        public Color ColorBegin = Color.FromArgb(255, 255, 255);
        public Color ColorEnd = Color.FromArgb(255, 255, 255);
        public double PercentDisp = 0.0;
        public double AnimationSpeed = 0.2;
        public bool EnableAnimation = true;
        public bool AlwaysInstant = false;
        public void UpdateAnimation(bool instant) {
            if (EnableAnimation&&!instant&&!AlwaysInstant) {
                PercentDisp = (1.0 - AnimationSpeed) * PercentDisp + AnimationSpeed * Percent;
            } else {
                PercentDisp = Percent;
            }
        }
    }
}
