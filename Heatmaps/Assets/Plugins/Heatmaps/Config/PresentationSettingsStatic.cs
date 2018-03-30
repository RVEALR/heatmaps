using System;
using System.Collections.Generic;

namespace RVEALR.Heatmaps
{
    public class PresentationSettingsStatic : HeatmapSettings
    {
        public override void OnEnable ()
        {
            base.OnEnable();

            smoothSpaceOption = 0;
            smoothSpace = 1f;

            smoothRotationOption  = 2;
            smoothRotation = 0f;

            smoothTimeOption = 2;
            smoothTime = 0;

            separateUsers = false;

            heatmapInFront = false;
            heatmapOptions = new List<int>{0};

            particleSize = 1f;
            particleShape = 3;
            particleDirection = 0;

            maskFollowType = 1;
            maskType = 0;
        }
    }
}

