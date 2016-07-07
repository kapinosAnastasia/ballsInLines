using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace np_hw_1
{
    public enum BallColors
    {
        none,
        first,
        second
    }
    class RoundButton : Button
    {
        protected override void OnResize(EventArgs e)
        {
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(new Rectangle(5, 5, this.Width - 10, this.Height - 10));
                this.Region = new Region(path);
                this.BackColor = Color.FromArgb(240,240,240);
            }
            base.OnResize(e);
        }

        private BallColors _colorBall;
        public BallColors ColorBall
        {
            get { return this._colorBall;   }
            set { this._colorBall = value;  }
        }
    }
}
