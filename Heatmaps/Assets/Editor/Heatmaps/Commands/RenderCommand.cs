using System;
using strange.extensions.command.impl;
using UnityEngine;

namespace UnityAnalyticsHeatmap
{
    public class RenderCommand : Command
    {
        [Inject]
        public IRendererSettings settings { get; set; }

        [Inject]
        public IHeatmapRenderer renderer { get; set; }
        
        [Inject]
        public IRenderInfo renderInfo { get; set; }
        
        [Inject]
        public IRenderData renderData { get; set; }

        public override void Execute()
        {
            if (renderer != null)
            {
                renderer.UpdateColors(new Color[]{ settings.lowDensityColor, settings.mediumDensityColor, settings.highDensityColor });
                renderer.UpdateThresholds(new float[]{ settings.lowThreshold, settings.highThreshold });
                renderer.pointSize = settings.particleSize;
                renderer.UpdateRenderStyle(settings.particleShapeIds[settings.particleShapeIndex], 
                                           settings.particleDirectionIds[settings.particleDirectionIndex]);
                renderer.UpdateTimeLimits(settings.startTime, settings.endTime);
                renderer.UpdatePointData(renderData.currentPoints, renderData.maxDensity);

                renderer.RenderHeatmap();

                renderInfo.currentPoints = renderer.currentPoints;
                renderInfo.totalPoints = renderer.totalPoints;
            }
        }
    }
}

