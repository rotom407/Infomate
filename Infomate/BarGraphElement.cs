﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Infomate {
    abstract class BarGraphElement {
        public Color BackgroundColor=new Color();
        public Color ForegroundBackgroundColor=new Color();
        public MeterData BackgroundMeter=new MeterData();
        public MeterData ForegroundMeter=new MeterData();
        public MeterData HighlightMeter=new MeterData();
        private Rectangle boundary = new Rectangle();
        public RectangleF BoundaryDisp=new RectangleF();
        public bool initialized = false;
        public double AnimationSpeed = 0.2;
        public bool Shrink=false;

        public Rectangle Boundary {
            get => boundary; set {
                boundary = value;
                BoundaryDisp = value;
            }
        }

        public void UpdateAnimation(bool instant) {
            BackgroundMeter.UpdateAnimation(instant);
            ForegroundMeter.UpdateAnimation(instant);
            HighlightMeter.UpdateAnimation(instant);
            BoundaryDisp.Width = (float)((1 - AnimationSpeed) * BoundaryDisp.Width + AnimationSpeed * (Boundary.Width * (Shrink?0.2:1.0)));
            BoundaryDisp.Height = Boundary.Height;
        }
        private Color BlendColor(Color cola,Color colb,double percent) {
            return Color.FromArgb((int)(cola.R * (1 - percent) + colb.R * percent), (int)(cola.G * (1 - percent) + colb.G * percent), (int)(cola.B * (1 - percent) + colb.B * percent));
        }
        public List<RectanglePrimitive> GetPrimitives() {
            List<RectanglePrimitive> primlist=new List<RectanglePrimitive>();
            int bdleft = (int)BoundaryDisp.Left, bdtop = (int)BoundaryDisp.Top, bdwidth = (int)BoundaryDisp.Width, bdheight = (int)BoundaryDisp.Height;
            //Background Color
            primlist.Add(new RectanglePrimitive(Rectangle.FromLTRB(bdleft, bdtop, bdleft + bdwidth, bdtop + bdheight), BackgroundColor));
            //Background Bar
            primlist.Add(new RectanglePrimitive(Rectangle.FromLTRB(bdleft, bdtop, Convert.ToInt32(bdleft + bdwidth * BackgroundMeter.PercentDisp), bdtop + bdheight), BlendColor(BackgroundMeter.ColorBegin,BackgroundMeter.ColorEnd,BackgroundMeter.PercentDisp)));
            //Foreground Background
            primlist.Add(new RectanglePrimitive(Rectangle.FromLTRB(bdleft + 2, bdtop + 2, bdleft + bdwidth - 2, bdtop + bdheight - 2), ForegroundBackgroundColor));
            //Foreground Bar
            primlist.Add(new RectanglePrimitive(Rectangle.FromLTRB(bdleft + 3, bdtop + 3, Convert.ToInt32(bdleft +3+ (bdwidth - 6) * ForegroundMeter.PercentDisp), bdtop + bdheight - 3), BlendColor(ForegroundMeter.ColorBegin, ForegroundMeter.ColorEnd, ForegroundMeter.PercentDisp)));
            //Highlight Bar
            primlist.Add(new RectanglePrimitive(Rectangle.FromLTRB(bdleft + 3, bdtop + 3, Convert.ToInt32(bdleft +3+ (bdwidth - 6) * HighlightMeter.PercentDisp), bdtop + bdheight - 3), BlendColor(HighlightMeter.ColorBegin, HighlightMeter.ColorEnd, HighlightMeter.PercentDisp)));
            return primlist;
        }
        public abstract string GetString();
        public abstract void UpdateValues();
        public abstract void Initialize();
    }
}
