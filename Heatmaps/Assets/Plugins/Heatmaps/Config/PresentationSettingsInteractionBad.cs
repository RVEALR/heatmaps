using System;
using System.Collections.Generic;

namespace UnityAnalyticsHeatmap
{
    public class PresentationSettingsInteractionBad : HeatmapSettings
    {
        public override void OnEnable ()
        {
            base.OnEnable();

            smoothSpaceOption = 0;
            smoothSpace = 1f;
            smoothRotationOption  = 0;
            smoothRotation = 90f;
            smoothTimeOption = 2;

            separateUsers = true;

            heatmapInFront = true;
            heatmapOptions = new List<int>{0, 0};

            particleSize = .1f;
            particleShape = 2;
            particleProjection = 1;

            maskFollowType = 1;
            maskType = 0;
        }
    }
}

