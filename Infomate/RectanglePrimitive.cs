using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Infomate {
    class RectanglePrimitive {
        public Rectangle prim=new Rectangle();
        public Color col=new Color();
        public RectanglePrimitive() {
            prim = Rectangle.FromLTRB(0,0,0,0);
            col = Color.FromArgb(255, 255, 255);
        }
        public RectanglePrimitive(Rectangle primp,Color colp) {
            prim = primp;
            col = colp;
        }
    }
}
