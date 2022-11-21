﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.HUD.Overlay.OverlayUtil.ProgressBars
{
    public class HorizontalProgressBar
    {
        // dimension
        private int _width;
        private int _height;
        public float Scale { private get; set; } = 1f;

        // values
        public double Min { private get; set; } = 0;
        public double Max { private get; set; } = 1;
        public double Value { private get; set; } = 0;

        // style
        public bool Rounded { private get; set; }
        public float Rounding { private get; set; } = 3;
        public Brush OutlineBrush { private get; set; } = Brushes.White;
        public Brush FillBrush { private get; set; } = Brushes.OrangeRed;

        private CachedBitmap _cachedOutline;

        public HorizontalProgressBar(int width, int height)
        {
            _width = width;
            _height = height;
        }

        public void Draw(Graphics g, int x, int y)
        {
            if (_cachedOutline == null)
                RenderCachedOutline();

            double percent = Value / Max;

            int scaledHeight = (int)(_height * Scale);
            int scaledWidth = (int)(_width * Scale);

            CachedBitmap barBitmap = new CachedBitmap(scaledWidth + 1, scaledHeight + 1, bg =>
            {
                if (Rounded)
                {
                    if (percent >= 0.035f)
                    {
                        int width = (int)(scaledWidth * percent);
                        bg.FillRoundedRectangle(FillBrush, new Rectangle(0, 0, scaledWidth - width, scaledHeight), (int)(Rounding * Scale));
                    }
                }
                else
                    bg.FillRectangle(FillBrush, new Rectangle(0, 0 + scaledHeight, scaledWidth - (int)(scaledWidth * percent), (int)(scaledHeight)));
            });

            barBitmap?.Draw(g, x, y, _width, _height);
            _cachedOutline?.Draw(g, x, y, _width, _height);
        }

        private void RenderCachedOutline()
        {
            int scaledWidth = (int)(_width * Scale);
            int scaledHeight = (int)(_height * Scale);
            if (Rounded)
                _cachedOutline = new CachedBitmap(scaledWidth + 1, scaledHeight + 1, g => g.DrawRoundedRectangle(new Pen(OutlineBrush, 1 * Scale), new Rectangle(0, 0, scaledWidth, scaledHeight), (int)(Rounding * Scale)));
            else
                _cachedOutline = new CachedBitmap(scaledWidth + 1, scaledHeight + 1, g => g.DrawRectangle(new Pen(OutlineBrush, 1 * Scale), new Rectangle(0, 0, scaledWidth, scaledHeight)));
        }
    }
}