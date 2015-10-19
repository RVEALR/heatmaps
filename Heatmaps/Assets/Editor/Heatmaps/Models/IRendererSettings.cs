
using System;
using UnityEngine;


namespace UnityAnalyticsHeatmap
{
    public interface IRendererSettings
    {
        Color lowDensityColor { get; set; }
        Color mediumDensityColor { get; set; }
        Color highDensityColor { get; set; }

        float highThreshold { get; set; }
        float lowThreshold { get; set; }

        float startTime { get; set; }
        float endTime { get; set; }
        float maxTime { get; set; }

        float particleSize { get; set; }
        int particleShapeIndex { get; set; }

        int particleDirectionIndex { get; set; }

        float playSpeed { get; set; }
        bool isPlaying { get; set; }

        RenderShape[] particleShapeIds { get; }
        RenderDirection[] particleDirectionIds { get; }
    }
}

