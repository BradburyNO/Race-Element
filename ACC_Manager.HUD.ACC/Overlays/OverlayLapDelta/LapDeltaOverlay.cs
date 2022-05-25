﻿using ACCManager.HUD.ACC.Data.Tracker;
using ACCManager.HUD.ACC.Data.Tracker.Laps;
using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.OverlayUtil;
using ACCManager.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCManager.ACCSharedMemory;

namespace ACCManager.HUD.ACC.Overlays.OverlayLapDelta
{
    internal sealed class LapDeltaOverlay : AbstractOverlay
    {
        private readonly LapDeltaConfig config = new LapDeltaConfig();
        private class LapDeltaConfig : OverlayConfiguration
        {
            [ToolTip("Displays the time for each sector, green colored sectors are personal best.")]
            public bool ShowSectors { get; set; } = true;

            [ToolTip("Sets the maximum range in seconds for the delta bar.")]
            [IntRange(1, 5, 1)]
            public int MaxDelta { get; set; } = 2;

            public LapDeltaConfig() : base()
            {
                this.AllowRescale = true;
            }
        }

        private const int overlayWidth = 200;

        private LapData lastLap = null;

        private readonly InfoTable _table;

        public LapDeltaOverlay(Rectangle rectangle) : base(rectangle, "Lap Delta Overlay")
        {
            _table = new InfoTable(10, new int[] { 60, 113 }) { Y = 17 };
            this.Width = overlayWidth + 1;
            this.Height = _table.FontHeight * 5 + 2 + 4;
            RefreshRateHz = 10;
        }

        public sealed override void BeforeStart()
        {
            if (!this.config.ShowSectors)
                this.Height -= this._table.FontHeight * 3;

            LapTracker.Instance.LapFinished += Collector_LapFinished;
        }

        public sealed override void BeforeStop()
        {
            LapTracker.Instance.LapFinished -= Collector_LapFinished;
        }

        private void Collector_LapFinished(object sender, LapData newLap)
        {
            if (newLap.Sector1 != -1 && newLap.Sector2 != -1 && newLap.Sector3 != -1)
                lastLap = newLap;
        }

        public sealed override void Render(Graphics g)
        {
            double delta = (double)pageGraphics.DeltaLapTimeMillis / 1000;
            DeltaBar deltaBar = new DeltaBar(-this.config.MaxDelta, this.config.MaxDelta, delta) { DrawBackground = true };
            deltaBar.Draw(g, 0, 0, overlayWidth, _table.FontHeight);

            TextRenderingHint previousHint = g.TextRenderingHint;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.TextContrast = 1;
            string deltaText = $"{delta:F3}";
            SizeF textWidth = g.MeasureString(deltaText, _table.Font);
            g.DrawString(deltaText, _table.Font, new SolidBrush(Color.FromArgb(60, Color.Black)), new PointF(overlayWidth / 2 - textWidth.Width + textWidth.Width / 2 + 0.75f, _table.FontHeight / 6 + 0.75f));
            g.DrawString(deltaText, _table.Font, Brushes.White, new PointF(overlayWidth / 2 - textWidth.Width + textWidth.Width / 2, _table.FontHeight / 6));
            g.TextRenderingHint = previousHint;


            if (this.config.ShowSectors)
                AddSectorLines();

            _table.Draw(g);
        }

        private void AddSectorLines()
        {
            LapData lap = LapTracker.Instance.CurrentLap;

            if (lastLap != null && pageGraphics.NormalizedCarPosition < 0.08 && lap.Index != lastLap.Index && lastLap.Sector3 != -1)
                lap = lastLap;

            int fastestSector1 = LapTracker.Instance.Laps.GetFastestSector(1);
            int fastestSector2 = LapTracker.Instance.Laps.GetFastestSector(2);
            int fastestSector3 = LapTracker.Instance.Laps.GetFastestSector(3);

            string[] rowSector1 = new string[2];
            string[] rowSector2 = new string[2];
            string[] rowSector3 = new string[2];
            rowSector1[0] = "-";
            rowSector2[0] = "-";
            rowSector3[0] = "-";

            if (LapTracker.Instance.CurrentLap.Sector1 > -1)
            {
                rowSector1[0] = $"{lap.GetSector1():F3}";
                if (lap.Sector1 > fastestSector1)
                    rowSector1[1] = $"+{(float)(lap.Sector1 - fastestSector1) / 1000:F3}";
            }
            else if (pageGraphics.CurrentSectorIndex == 0)
                rowSector1[0] = $"{((float)pageGraphics.CurrentTimeMs / 1000):F3}";


            if (lap.Sector2 > -1)
            {
                rowSector2[0] = $"{lap.GetSector2():F3}";
                if (lap.Sector2 > fastestSector2)
                    rowSector2[1] = $"+{(float)(lap.Sector2 - fastestSector2) / 1000:F3}";
            }
            else if (lap.Sector1 > -1)
            {
                rowSector2[0] = $"{(((float)pageGraphics.CurrentTimeMs - lap.Sector1) / 1000):F3}";
            }

            if (lap.Sector3 > -1)
            {
                rowSector3[0] = $"{lap.GetSector3():F3}";
                if (lap.Sector3 > fastestSector3)
                    rowSector3[1] = $"+{(float)(lap.Sector3 - fastestSector3) / 1000:F3}";
            }
            else if (lap.Sector2 > -1 && pageGraphics.CurrentSectorIndex == 2)
            {
                rowSector3[0] = $"{(((float)pageGraphics.CurrentTimeMs - lap.Sector2 - lap.Sector1) / 1000):F3}";
            }


            if (pageGraphics.CurrentSectorIndex != 0 && lap.Sector1 != -1 && lap.IsValid)
                _table.AddRow("S1  ", rowSector1, new Color[] { LapTracker.Instance.Laps.IsSectorFastest(1, lap.Sector1) ? Color.LimeGreen : Color.White, Color.Orange });
            else
                _table.AddRow("S1  ", rowSector1, new Color[] { Color.White });

            if (pageGraphics.CurrentSectorIndex != 1 && lap.Sector2 != -1 && lap.IsValid)
                _table.AddRow("S2  ", rowSector2, new Color[] { LapTracker.Instance.Laps.IsSectorFastest(2, lap.Sector2) ? Color.LimeGreen : Color.White, Color.Orange });
            else
                _table.AddRow("S2  ", rowSector2, new Color[] { Color.White });

            if (pageGraphics.CurrentSectorIndex != 2 && lap.Sector3 != -1 && lap.IsValid)
                _table.AddRow("S3  ", rowSector3, new Color[] { LapTracker.Instance.Laps.IsSectorFastest(3, lap.Sector3) ? Color.LimeGreen : Color.White, Color.Orange });
            else
                _table.AddRow("S3  ", rowSector3, new Color[] { Color.White });
        }

        public sealed override bool ShouldRender()
        {
#if DEBUG
            return true;
#endif
            bool shouldRender = true;
            if (pageGraphics.Status == AcStatus.AC_OFF || pageGraphics.Status == AcStatus.AC_PAUSE || (pageGraphics.IsInPitLane == true && !pagePhysics.IgnitionOn))
                shouldRender = false;

            return shouldRender;
        }
    }
}
