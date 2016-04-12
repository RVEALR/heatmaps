using System;
using System.Collections.Generic;

namespace UnityAnalyticsHeatmap
{
    public class MultiLevelDataStory : DataStory
    {
        public MultiLevelDataStory()
        {
            name = "Multilevel Game";
            genre = "2D Platformer";
            description = "This demo shows how you can separate events by level. In fact, you can separate by ";
            description += "pretty much ANYTHING.";
            whatToTry = "Generate this data, which shows coin collection in a 2D platformer. Open the Heatmapper and click the Process button. ";
            whatToTry += "Now, there's a LOT of data here and it looks very messy. That's because what you're seeing is actually ";
            whatToTry += "data from MANY levels on top of each other. Click the 'Separate on Field' button and replace the words ";
            whatToTry += "'Field Name' with 'level' (case matters!). Now Process again. Hey presto! You can now see the clear shape ";
            whatToTry += "of a single level. Did you see the 'Option' dropdown appear? Click that dropdown. ";
            whatToTry += "You can now see each level's worth of data individually.\n\n";
            whatToTry += "Under 'Aggregate', uncheck 'Unique Devices' and click Process. Open the 'Option' list to see how it has changed. ";
            whatToTry += "Not only are the levels separated, so are the individual devices. You can use this to see how individual ";
            whatToTry += "players play.";
        }

        #region implemented abstract members of DataStory
        public override Dictionary<double, string> Generate()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

