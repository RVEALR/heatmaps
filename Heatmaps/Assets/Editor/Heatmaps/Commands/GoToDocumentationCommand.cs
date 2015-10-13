using System;
using strange.extensions.command.impl;
using UnityEngine;

namespace UnityAnalyticsHeatmap
{
    public class GoToDocumentationCommand : Command
    {
        public override void Execute()
        {
            Application.OpenURL("https://bitbucket.org/Unity-Technologies/heatmaps/wiki/Home");
        }
    }
}

