using System;
using System.Collections.Generic;

namespace UnityAnalyticsHeatmap
{
    public class PresentationSettingsVRBad : HeatmapSettings
    {
        public override void OnEnable ()
        {
            base.OnEnable();

            smoothSpaceOption = 0;
            smoothSpace = 5f;

            smoothRotationOption  = 0;
            smoothRotation = 10f;

            smoothTimeOption = 2;
            smoothTime = 1f;

            separateUsers = true;

            heatmapInFront = false;
            heatmapOptions = new List<int>{0, 1};

            particleSize = 1.5f;
            particleShape = 2;
            particleProjection = 0;

            maskFollowType = 2;
            maskType = 1;
        }
    }
}

