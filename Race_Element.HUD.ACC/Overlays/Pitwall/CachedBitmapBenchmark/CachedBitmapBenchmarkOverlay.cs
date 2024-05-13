﻿using RaceElement.Core.Jobs.LoopJob;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.CachedBitmapBenchmark;

[Overlay(Name = "CB Benchmark",
Description = "Shows info about the car in front and behind.",
OverlayType = OverlayType.Pitwall,
Version = 1.00,
Authors = ["Reinier Klarenberg"])]
internal sealed class CachedBitmapBenchmarkOverlay : AbstractOverlay
{
    private readonly CachedBitmapBenchmarkConfiguration _config = new();

    private const int InitialWidth = 300, InitialHeight = 250;

    private BenchmarkJob _benchmarkJob;

    private InfoPanel _panel;

    public CachedBitmapBenchmarkOverlay(Rectangle rectangle) : base(rectangle, "CB Benchmark")
    {
        this.RefreshRateHz = 1 / 2f;
    }

    public override void BeforeStart()
    {
        Width = 450;
        Height = 150;
        _panel = new(10, 450);
        if (IsPreviewing) return;

        _benchmarkJob = new(_config.Bench.ComplexityIterations, _config.Bench.SmoothingMode, _config.Bench.CompositingQuality)
        {
            IntervalMillis = 1000 / _config.Bench.IterationsPerSecond
        };
        _benchmarkJob.Run();
    }

    public override void BeforeStop()
    {
        if (IsPreviewing) return;

        _benchmarkJob?.CancelJoin();
    }

    public override bool ShouldRender() => true;

    public sealed override void Render(Graphics g)
    {
        if (IsPreviewing) return;

        if (_benchmarkJob._cached.Count > 2)
        {
            _panel.AddLine("", $"S: {_config.Bench.SmoothingMode}, Q: {_config.Bench.CompositingQuality}, P/S: {_config.Bench.IterationsPerSecond}");
            _panel.AddLine("", $"Iterations: {_benchmarkJob._notCached.Count} - Complexity {_config.Bench.ComplexityIterations}");
            _panel.AddLine("Raw", GetStats(_benchmarkJob._notCached));
            _panel.AddLine("Cached", GetStats(_benchmarkJob._cached));
            _panel.Draw(g);
        }
    }

    private static string GetStats(List<double> data)
    {
        StringBuilder sb = new();
        sb.Append($"Min: {data.Min():F0}");
        sb.Append($", Avg: {data.Average():F0}");
        sb.Append($", Max: {data.Max():F0}");
        return sb.ToString();
    }


}
