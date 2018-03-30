using System;
using System.Collections.Generic;

namespace RVEALR.Heatmaps
{
    public class PresentationSettingsInteractionBad : HeatmapSettings
    {
        public override void OnEnable ()
        {
            base.OnEnable();

            smoothSpaceOption = 1;
            smoothSpace = 0f;

            smoothRotationOption  = 2;
            smoothRotation = 0f;

            smoothTimeOption = 2;
            smoothTime = 0f;

            separateUsers = true;

            heatmapInFront = false;
            heatmapOptions = new List<int>{1, 1};

            particleSize = .1f;
            particleShape = 2;
            particleProjection = 1;

            maskFollowType = 1;
            maskType = 0;
        }
    }
}

