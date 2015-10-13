using System;
using strange.extensions.command.impl;

namespace UnityAnalyticsHeatmap
{
    public class PurgeMetadataCommand : Command
    {
        [Inject]
        public RawEventClient client{ get; set; }


        public override void Execute()
        {
            client.PurgeData();
        }
    }
}

