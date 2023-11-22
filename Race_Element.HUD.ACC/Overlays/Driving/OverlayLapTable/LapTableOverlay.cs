﻿using RaceElement.Data.ACC.Database.LapDataDB;
using RaceElement.Data.ACC.Tracker.Laps;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.OverlayUtil.Drawing;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace RaceElement.HUD.ACC.Overlays.OverlayLapTimeTable
{
    [Overlay(Name = "Lap Table", Description = "A table showing time for each lap and optionally sectors.", OverlayType = OverlayType.Release, Version = 1.00,
        OverlayCategory = OverlayCategory.Lap)]
    internal sealed class LapTableOverlay : AbstractOverlay
    {
        private readonly LapTimeTableConfiguration _config = new LapTimeTableConfiguration();
        private GraphicsGrid _graphicsGrid;
        private Font _font;
        private CachedBitmap[] _columnBackgroundsValid;
        private CachedBitmap[] _columnBackgroundsInvalid;
        private CachedBitmap[] _columnBackgroundsGreen;
        private CachedBitmap[] _columnBackgroundsPurple;

        private List<KeyValuePair<int, DbLapData>> _storedLaps;
        private bool _dataIsPreview = false;

        public LapTableOverlay(Rectangle rectangle) : base(rectangle, "Lap Table")
        {
            this.RefreshRateHz = 2;
            _storedLaps = new List<KeyValuePair<int, DbLapData>>();
        }

        public override void SetupPreviewData()
        {
            _dataIsPreview = true;

            Dictionary<int, DbLapData> Laps = new Dictionary<int, DbLapData>();
            Random rand = new Random();
            int maxSectorDeviation = 10000;
            int s1 = 28525 + rand.Next(-maxSectorDeviation, maxSectorDeviation);
            int s2 = 38842 + rand.Next(-maxSectorDeviation, maxSectorDeviation);
            int s3 = 36840 + rand.Next(-maxSectorDeviation, maxSectorDeviation);
            int startLapIndex = 100 + rand.Next(-50, 800);

            for (int i = startLapIndex; i < _config.Table.Rows + startLapIndex; i++)
            {
                DbLapData randomData = new DbLapData()
                {
                    Index = i,
                    LapType = Broadcast.LapType.Regular,
                    IsValid = rand.Next(0, 10) > 3,
                    Sector1 = s1 + rand.Next(-200, 200),
                    Sector2 = s2 + rand.Next(-300, 300),
                    Sector3 = s3 + rand.Next(-300, 500),
                };
                randomData.Time = randomData.Sector1 + randomData.Sector2 + randomData.Sector3;
                Laps.Add(i, randomData);
            }

            _storedLaps = Laps.OrderByDescending(x => x.Key).ToList();
        }

        public override void BeforeStart()
        {
            float scale = this.Scale;
            if (_dataIsPreview) scale = 1f;

            _font = FontUtil.FontSegoeMono(12 * scale);

            int rows = 1;
            rows += _config.Table.Rows;
            int columns = 2;
            if (_config.Table.ShowSectors) columns += 3;

            _graphicsGrid = new GraphicsGrid(rows, columns);

            float fontHeight = (int)(_font.GetHeight(120));
            int columnHeight = (int)(Math.Ceiling(fontHeight) + 1 * scale);
            int[] columnWidths = new int[] { (int)(45f * scale), (int)(95f * scale), (int)(78f * scale), (int)(78f * scale), (int)(78f * scale) };
            int totalWidth = columnWidths[0] + columnWidths[1];

            // set up backgrounds and invalid ones
            Color colorDefault = Color.FromArgb(190, Color.Black);
            Color colorRed = Color.FromArgb(150, Color.Red);
            Color colorGreen = Color.FromArgb(150, Color.LimeGreen);
            Color colorPurple = Color.FromArgb(150, Color.MediumPurple);
            using HatchBrush columnBrushDefault = new HatchBrush(HatchStyle.LightUpwardDiagonal, colorDefault, Color.FromArgb(colorDefault.A - 25, colorDefault));
            using HatchBrush columnBrushRed = new HatchBrush(HatchStyle.LightUpwardDiagonal, colorDefault, Color.FromArgb(colorRed.A - 75, colorRed));
            using HatchBrush columnBrushGreen = new HatchBrush(HatchStyle.LightUpwardDiagonal, colorDefault, Color.FromArgb(colorGreen.A - 25, colorGreen));
            using HatchBrush columnBrushPurple = new HatchBrush(HatchStyle.LightUpwardDiagonal, colorDefault, Color.FromArgb(colorPurple.A - 25, colorPurple));
            _columnBackgroundsValid = new CachedBitmap[columns];
            _columnBackgroundsInvalid = new CachedBitmap[columns];
            _columnBackgroundsGreen = new CachedBitmap[columns];
            _columnBackgroundsPurple = new CachedBitmap[columns];
            for (int i = 0; i < columns; i++)
            {
                _columnBackgroundsValid[i] = new CachedBitmap(columnWidths[i], columnHeight, g =>
                {
                    using LinearGradientBrush brush = new LinearGradientBrush(new PointF(columnWidths[i], columnHeight), new PointF(0, 0), Color.FromArgb(0, 0, 0, 0), Color.FromArgb(colorDefault.A, 10, 10, 10));
                    g.FillRoundedRectangle(brush, new Rectangle(0, 0, columnWidths[i], columnHeight), (int)(_config.Table.Roundness * scale));
                    g.FillRoundedRectangle(columnBrushDefault, new Rectangle(0, 0, columnWidths[i], columnHeight), (int)(_config.Table.Roundness * scale));
                });
                _columnBackgroundsInvalid[i] = new CachedBitmap(columnWidths[i], columnHeight, g =>
                {
                    using LinearGradientBrush brush = new LinearGradientBrush(new PointF(0, 0), new PointF(columnWidths[i], columnHeight), Color.FromArgb(0, 0, 0, 0), Color.FromArgb(colorRed.A, colorRed.R, 10, 10));
                    g.FillRoundedRectangle(brush, new Rectangle(0, 0, columnWidths[i], columnHeight), (int)(_config.Table.Roundness * scale));
                    g.FillRoundedRectangle(columnBrushRed, new Rectangle(0, 0, columnWidths[i], columnHeight), (int)(_config.Table.Roundness * scale));
                });
                _columnBackgroundsGreen[i] = new CachedBitmap(columnWidths[i], columnHeight, g =>
                {
                    using LinearGradientBrush brush = new LinearGradientBrush(new PointF(0, 0), new PointF(columnWidths[i], columnHeight), Color.FromArgb(0, 0, 0, 0), Color.FromArgb(colorGreen.A, colorGreen.R, 10, 10));
                    g.FillRoundedRectangle(brush, new Rectangle(0, 0, columnWidths[i], columnHeight), (int)(_config.Table.Roundness * scale));
                    g.FillRoundedRectangle(columnBrushGreen, new Rectangle(0, 0, columnWidths[i], columnHeight), (int)(_config.Table.Roundness * scale));
                });
                _columnBackgroundsPurple[i] = new CachedBitmap(columnWidths[i], columnHeight, g =>
                {
                    using LinearGradientBrush brush = new LinearGradientBrush(new PointF(0, 0), new PointF(columnWidths[i], columnHeight), Color.FromArgb(0, 0, 0, 0), Color.FromArgb(colorPurple.A, colorPurple.R, 10, 10));
                    g.FillRoundedRectangle(brush, new Rectangle(0, 0, columnWidths[i], columnHeight), (int)(_config.Table.Roundness * scale));
                    g.FillRoundedRectangle(columnBrushPurple, new Rectangle(0, 0, columnWidths[i], columnHeight), (int)(_config.Table.Roundness * scale));
                });
            }

            // add header row, base columns
            DrawableTextCell col0 = new DrawableTextCell(new Rectangle(0, 0, columnWidths[0], columnHeight), _font);
            _graphicsGrid.Grid[0][0] = col0;

            DrawableTextCell col1 = new DrawableTextCell(new RectangleF(col0.Rectangle.Width, 0, columnWidths[1], columnHeight), _font);
            col1.CachedBackground = _columnBackgroundsValid[1];
            col1.UpdateText("Time");
            _graphicsGrid.Grid[0][1] = col1;

            // add header columns for sectors
            if (_config.Table.ShowSectors)
            {
                totalWidth += columnWidths[2] + columnWidths[3] + columnWidths[4];

                DrawableTextCell col2 = new DrawableTextCell(new RectangleF(col1.Rectangle.X + columnWidths[1], 0, columnWidths[2], columnHeight), _font);
                col2.CachedBackground = _columnBackgroundsValid[2];
                col2.UpdateText("S1");
                DrawableTextCell col3 = new DrawableTextCell(new RectangleF(col2.Rectangle.X + columnWidths[2], 0, columnWidths[3], columnHeight), _font);
                col3.CachedBackground = _columnBackgroundsValid[3];
                col3.UpdateText("S2");
                DrawableTextCell col4 = new DrawableTextCell(new RectangleF(col3.Rectangle.X + columnWidths[3], 0, columnWidths[4], columnHeight), _font);
                col4.CachedBackground = _columnBackgroundsValid[4];
                col4.UpdateText("S3");
                _graphicsGrid.Grid[0][2] = col2;
                _graphicsGrid.Grid[0][3] = col3;
                _graphicsGrid.Grid[0][4] = col4;
            }

            this.Width = totalWidth;
            this.Height = columnHeight * (_config.Table.Rows + 1); // +1 for header

            // config data rows
            for (int row = 1; row <= _config.Table.Rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    int x = columnWidths.Take(column).Sum();
                    int y = row * columnHeight;
                    int width = columnWidths[column];
                    RectangleF rect = new RectangleF(x, y, width, columnHeight);
                    DrawableTextCell cell = new DrawableTextCell(rect, _font);
                    cell.CachedBackground = _columnBackgroundsValid[column];
                    _graphicsGrid.Grid[row][column] = cell;
                }
            }
        }

        public override void BeforeStop()
        {
            _graphicsGrid?.Dispose();
            _font?.Dispose();
            for (int i = 0; i < _columnBackgroundsValid.Length; i++)
                _columnBackgroundsValid[i].Dispose();
        }

        public override void Render(Graphics g)
        {
            if (!_dataIsPreview)
                _storedLaps = LapTracker.Instance.Laps.OrderByDescending(x => x.Key).Take(_config.Table.Rows).ToList();

            int fastestLapIndex = LapTracker.Instance.Laps.GetFastestLapIndex();
            DbLapData bestLap = null;
            if (fastestLapIndex != -1)
                bestLap = LapTracker.Instance.Laps[fastestLapIndex];

            int fastestSector1 = LapTracker.Instance.Laps.GetFastestSector(1);
            int fastestSector2 = LapTracker.Instance.Laps.GetFastestSector(2);
            int fastestSector3 = LapTracker.Instance.Laps.GetFastestSector(3);

            if (_dataIsPreview)
            {
                Random rand = new Random();
                int maxSectorDeviation = 10000;
                fastestSector1 = 23525 + rand.Next(-maxSectorDeviation, maxSectorDeviation);
                fastestSector2 = 37842 + rand.Next(-maxSectorDeviation, maxSectorDeviation);
                fastestSector3 = 35840 + rand.Next(-maxSectorDeviation, maxSectorDeviation);
                bestLap = new DbLapData { Sector1 = fastestSector1 - 250, Sector2 = fastestSector2 + 50, Sector3 = fastestSector3 + 30 };
            }

            int row = 1;
            foreach (var lap in _storedLaps)
            {
                DrawableTextCell lapCell = (DrawableTextCell)_graphicsGrid.Grid[row][0];
                lapCell.UpdateText($"{lap.Key}");

                string lapTimeValue = $"--:--.---";
                if (lap.Value.Time != -1)
                {
                    TimeSpan best = TimeSpan.FromMilliseconds(lap.Value.Time);
                    lapTimeValue = $"{best:mm\\:ss\\:fff}";

                }

                DrawableTextCell lapTimeCell = (DrawableTextCell)_graphicsGrid.Grid[row][1];
                if (bestLap != null)
                {
                    if (lap.Value.Time == bestLap.Sector1 + bestLap.Sector2 + bestLap.Sector3)
                        lapTimeCell.CachedBackground = _columnBackgroundsGreen[1];
                }
                if (!lap.Value.IsValid)
                    lapTimeCell.CachedBackground = _columnBackgroundsInvalid[1];
                lapTimeCell.UpdateText($"{lapTimeValue}");


                if (_config.Table.ShowSectors)
                {
                    int sector1 = lap.Value.Sector1;
                    int sector2 = lap.Value.Sector2;
                    int sector3 = lap.Value.Sector3;

                    DrawableTextCell sector1Cell = (DrawableTextCell)_graphicsGrid.Grid[row][2];
                    sector1Cell.UpdateText($"{sector1 / 1000d:F3}");
                    if (sector1 <= fastestSector1 && lap.Value.IsValid)
                        sector1Cell.CachedBackground = _columnBackgroundsPurple[2];
                    else if (bestLap != null)
                    {
                        if (sector1 == bestLap.Sector1 && sector1Cell.CachedBackground != _columnBackgroundsValid[2])
                            sector1Cell.CachedBackground = _columnBackgroundsGreen[2];
                    }
                    else
                        sector1Cell.CachedBackground = _columnBackgroundsValid[2];


                    DrawableTextCell sector2Cell = (DrawableTextCell)_graphicsGrid.Grid[row][3];
                    sector2Cell.UpdateText($"{sector2 / 1000d:F3}");
                    if (sector2 <= fastestSector2 && lap.Value.IsValid)
                        sector2Cell.CachedBackground = _columnBackgroundsPurple[3];
                    else if (bestLap != null)
                    {
                        if (sector2 == bestLap.Sector2 && sector2Cell.CachedBackground != _columnBackgroundsValid[3])
                            sector2Cell.CachedBackground = _columnBackgroundsGreen[3];
                    }
                    else
                        sector2Cell.CachedBackground = _columnBackgroundsValid[3];


                    DrawableTextCell sector3Cell = (DrawableTextCell)_graphicsGrid.Grid[row][4];
                    sector3Cell.UpdateText($"{sector3 / 1000d:F3}");
                    if (sector3 <= fastestSector3 && lap.Value.IsValid)
                        sector3Cell.CachedBackground = _columnBackgroundsPurple[4];
                    else if (bestLap != null)
                    {
                        if (sector3 == bestLap.Sector3 && sector3Cell.CachedBackground != _columnBackgroundsValid[4])
                            sector3Cell.CachedBackground = _columnBackgroundsGreen[4];
                    }
                    else
                        sector3Cell.CachedBackground = _columnBackgroundsValid[4];
                }

                row++;
            }

            _graphicsGrid?.Draw(g);
        }
    }
}
