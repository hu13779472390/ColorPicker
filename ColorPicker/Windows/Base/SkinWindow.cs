﻿using ColorPicker.Native;
using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

namespace ColorPicker.Windows.Base
{
    public partial class SkinWindow : ResizeWindow
    {
        class Box
        {
            public Rectangle Bounds { get; set; }
            public Bitmap Image { get; }
            public Action<object> Event;
            public bool Visible { get; set; } = true;

            public Box(Rectangle area, Bitmap image)
            {
                this.Bounds = area;
                this.Image = image;
            }
        }
        
        private Color _edgeColor = Color.FromArgb(41, 128, 185);
        public Color EdgeColor
        {
            get
            {
                return _edgeColor;
            }
            set
            {
                _edgeColor = value;
                this.Invalidate();
            }
        }

        private Color _titleColor = Color.Black;
        public Color TitleColor
        {
            get
            {
                return _titleColor;
            }
            set
            {
                _titleColor = value;
                this.Invalidate();
            }
        }

        public int CaptionHeight { get; set; } = 30;

        private bool _iconVisible = true;
        public bool IconVisible
        {
            get
            {
                return _iconVisible;
            }
            set
            {
                _iconVisible = value;
                this.Invalidate();
            }
        }
        
        public bool SettingBox
        {
            get
            {
                return mcBoxSetting.Visible;
            }
            set
            {
                mcBoxSetting.Visible = value;
                this.Invalidate();
            }
        }

        public new bool MinimizeBox
        {
            get
            {
                return mcBoxMinimize.Visible;
            }
            set
            {
                mcBoxMinimize.Visible = value;
                this.Invalidate();
            }
        }

        private bool _activated;
        public bool IsActivated
        {
            get
            {
                return _activated;
            }
        }
        
        private Box mcBoxExit = new Box(new Rectangle(0, 0, 20, 20), Properties.Resources.exit);
        private Box mcBoxMinimize = new Box(new Rectangle(0, 0, 20, 20), Properties.Resources.minimize);
        private Box mcBoxSetting = new Box(new Rectangle(0, 0, 20, 20), Properties.Resources.setting);

        public SkinWindow()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);

            mcBoxExit.Event = new Action<object>(Exit_Click);
            mcBoxMinimize.Event = new Action<object>(Minimize_Click);
            mcBoxSetting.Event = new Action<object>(OnSettingOpen);
        }

        protected virtual void OnSettingOpen(object obj)
        {
        }

        private void Minimize_Click(object obj)
        {
            NativeMethods.SendMessage(this.Handle, (uint)NativeEnums.WM.SYSCOMMAND, new IntPtr((int)NativeEnums.SCType.SC_MINIMIZE), IntPtr.Zero);
        }

        private void Exit_Click(object sender)
        {
            this.Close();
        }

        protected virtual void OnMinimize()
        {
            WindowState = FormWindowState.Minimized;
        }

        protected virtual void OnRestore()
        {
            WindowState = FormWindowState.Normal;
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            DrawTitle(g);
            DrawControlBox(g);
            DrawEdge(g);

            base.OnPaint(e);
        }

        #region [ Draw ]

        private void DrawTitle(Graphics g)
        {
            Point iconPt = Point.Empty;

            if (IconVisible && this.Icon != null)
            {
                iconPt = new Point(6, CaptionHeight / 2 - 8);
                
                g.DrawIcon(this.Icon, new Rectangle(iconPt, new Size(16, 16)));
            }

            using (Font f = new Font("맑은고딕", 10))
            {
                Color c = TitleColor;

                if (!_activated)
                {
                    c = Color.FromArgb(150, 150, 150);
                }

                SizeF size = g.MeasureString(this.Text, f);
                PointF pt = new PointF(iconPt.X + (IconVisible ? 16 : 4) + 3, CaptionHeight / 2 - size.Height / 2);

                using (SolidBrush sb = new SolidBrush(c))
                {
                    g.DrawString(this.Text, f, sb, pt);
                }
            }
        }

        private void DrawControlBox(Graphics g)
        {
            DrawBox(g, mcBoxExit);
            if (MinimizeBox) DrawBox(g, mcBoxMinimize);
            if (SettingBox) DrawBox(g, mcBoxSetting);
        }

        private void DrawBox(Graphics g, Box box)
        {
            g.DrawImage(box.Image, new Point(box.Bounds.X + box.Bounds.Width / 2 - box.Image.Width / 2,
                                       box.Bounds.Y + box.Bounds.Height / 2 - box.Image.Height / 2));
        }

        private void DrawEdge(Graphics g)
        {
            Color c = EdgeColor;

            if (!_activated)
            {
                c = Color.FromArgb(190, 190, 190);
            }

            using (SolidBrush sb = new SolidBrush(c))
            {
                using (Pen p = new Pen(sb))
                {
                    g.DrawRectangle(p, new Rectangle(0, 0, this.Width - 1, this.Height - 1));
                }
            }
        }

        #endregion

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            this.Invalidate();
        }

        protected override void OnResizing(ResizeDirection direction, MouseEventArgs e)
        {
            if (direction == ResizeDirection.Center && e.Y > CaptionHeight) return;

            Box[] boxs = { mcBoxExit, mcBoxMinimize, mcBoxSetting };
            foreach (Box box in boxs)
            {
                if (IntersectWith(box.Bounds, new Point(e.X, e.Y))) return;
            }

            base.OnResizing(direction, e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
        }

        protected override void OnClick(EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;

            Box[] boxs = { mcBoxExit, mcBoxMinimize };

            foreach (Box box in boxs)
            {
                if (box.Visible && IntersectWith(box.Bounds, new Point(me.X, me.Y)))
                {
                    box.Event?.Invoke(box);
                    return;
                }
            }

            base.OnClick(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (mcBoxSetting.Visible && IntersectWith(mcBoxSetting.Bounds, new Point(e.X, e.Y)))
            {
                mcBoxSetting.Event?.Invoke(mcBoxSetting);
            }

            base.OnMouseDown(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            Point ePt = new Point(this.Width - mcBoxExit.Bounds.Width - 5, CaptionHeight / 2 - mcBoxExit.Bounds.Height / 2);
            mcBoxExit.Bounds = new Rectangle(ePt, mcBoxExit.Bounds.Size);

            Point lPt = ePt;

            if (MinimizeBox)
            {
                Point mPt = new Point(lPt.X - mcBoxMinimize.Bounds.Width - 5, CaptionHeight / 2 - mcBoxMinimize.Bounds.Height / 2);
                mcBoxMinimize.Bounds = new Rectangle(mPt, mcBoxMinimize.Bounds.Size);

                lPt = mPt;
            }
            
            if (SettingBox)
            {
                Point sPt = new Point(lPt.X - mcBoxSetting.Bounds.Width - 5, CaptionHeight / 2 - mcBoxSetting.Bounds.Height / 2);
                mcBoxSetting.Bounds = new Rectangle(sPt, mcBoxSetting.Bounds.Size);
            }

            this.Invalidate();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            
            _activated = true;
            this.Invalidate();
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);

            _activated = false;
            this.Invalidate();
        }

        internal static bool IntersectWith(Rectangle area, Point pt)
        {
            return (pt.X >= area.X && pt.X <= area.Right) && (pt.Y >= area.Y && pt.Y <= area.Bottom);
        }

        protected override void WndProc(ref Message m)
        {
            NativeEnums.WM msg = (NativeEnums.WM)m.Msg;

            if (msg == NativeEnums.WM.SYSCOMMAND)
            {
                NativeEnums.SCType sc = (NativeEnums.SCType)m.WParam.ToInt32();

                if (sc == NativeEnums.SCType.SC_MINIMIZE)
                {
                    OnMinimize();
                }
                else if (sc == NativeEnums.SCType.SC_RESTORE)
                {
                    OnRestore();
                }
            }

            base.WndProc(ref m);
        }
    }
}
