using System;
using System.Collections.Generic;

namespace RVEALR.Heatmaps
{
    public class PresentationSettingsHeading : HeatmapSettings
    {
        public override void OnEnable ()
        {
            base.OnEnable();

            smoothSpaceOption = 1;
            smoothSpace = 1f;
            smoothRotationOption  = 1;
            smoothRotation = 1f;
            smoothTimeOption = 1;

            separateUsers = true;

            heatmapInFront = true;
            heatmapOptions = new List<int>{0, 0};

            particleSize = .29f;
            particleShape = 1;
            particleProjection = 1;

            maskFollowType = 1;
            maskType = 0;
        }
    }
}

