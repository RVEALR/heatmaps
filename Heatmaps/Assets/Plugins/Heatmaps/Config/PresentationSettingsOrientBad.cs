using System;
using System.Collections.Generic;

namespace UnityAnalyticsHeatmap
{
    public class PresentationSettingsOrientBad : HeatmapSettings
    {
        public override void OnEnable ()
        {
            base.OnEnable();

            smoothSpaceOption = 0;
            smoothSpace = 1f;

            smoothRotationOption  = 0;
            smoothRotation = 90f;

            smoothTimeOption = 2;
            smoothTime = 0f;

            separateUsers = true;

            heatmapInFront = false;
            heatmapOptions = new List<int>{0, 1};

            particleSize = .22f;
            particleShape = 1;
            particleProjection = 1;

            maskFollowType = 1;
            maskType = 0;
        }

        public override void PostProcess()
        {

        }
    }
}

