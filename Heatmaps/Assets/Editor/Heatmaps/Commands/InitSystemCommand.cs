using System;
using strange.extensions.command.impl;
using UnityEngine;

namespace UnityAnalyticsHeatmap
{
    public class InitSystemCommand : Command
    {
        public override void Execute()
        {
            Debug.Log("InitSystemCommand");
        }
    }
}

