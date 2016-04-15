using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityAnalyticsHeatmap
{
    public class BasicDataStory : DataStory
    {
        public BasicDataStory()
        {
            name = "Basic Functionality";
            genre = "Any";
            description = "This first demo shows off a few key ideas, such as particle shapes, sizes, and colors.";
            whatToTry = "Generate this data, then open the Heatmapper, ensure that 'Local Only' is checked, and click the Process button. ";
            whatToTry += "First, notice the numbers at the bottom of the Heatmapper ('Points in current set' and ";
            whatToTry += "'Points currently displayed'). These give you an idea of how much data you should expect to see displayed.\n\n";
            whatToTry += "Notice that the generated heatmap has three colors. Play with the color thresholds to 'tune' the ";
            whatToTry += "separation between high-, medium- and low-frequency events. Click on a ";
            whatToTry += "color swatch and play with that color; see how that changes the look of the heatmap. You can even change the alpha.\n\n";
            whatToTry += "Now, under 'Particle', change the size and shape settings and again observe how this affects the display.";
        }

        #region implemented abstract members of DataStory
        public override Dictionary<double, string> Generate()
        {
            var retv = new Dictionary<double, string>();

            int eventCount = 2000;
            int linesPerFile = 500;
            int currentFileLines = 0;
            int deviceCount = 2;
            float minRadius = 5f;
            float radius = 10f;

            double firstDate = 0d;
            DateTime now = DateTime.UtcNow;
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            string data = "";
            string eventName = "Heatmap.ShotWeapon";

            for (int a = 0; a < eventCount; a++)
            {
                string evt = "";

                // Date
                DateTime dt = now.Subtract(new TimeSpan(TimeSpan.TicksPerSecond * (eventCount - a)));
                string dts = dt.ToString("yyyy-MM-dd hh:mm:ss.ms");
                evt += dts + "\t";
                if (currentFileLines == 0) {
                    firstDate = Math.Round((dt - epoch).TotalSeconds);
                }

                // Device ID & name
                evt += "d" + UnityEngine.Random.Range(0, deviceCount) + "-XXXX-XXXX\t";
                evt += eventName + "\t";

                // Build the JSON

                float mult = UnityEngine.Random.Range(0f, 1f) > .5f ? -1f : 1f;
                float distance = UnityEngine.Random.Range(minRadius, radius) * mult;
                Vector3 rot = new Vector3(UnityEngine.Random.Range(-1f,1f), UnityEngine.Random.Range(-1f,1f), UnityEngine.Random.Range(-1f,1f));
                Vector3 position = rot.normalized * distance;

                evt += "{";
                evt += "\"x\":\"" + position.x + "\",";
                evt += "\"y\":\"" + position.y + "\",";
                evt += "\"z\":\"" + position.z + "\",";

                evt += "\"unity.name\":" + "\"" + eventName + "\"" + "}\n";

                data += evt;
                currentFileLines ++;
                if (currentFileLines >= linesPerFile || a == eventCount-1) {
                    retv.Add(firstDate, data);
                    currentFileLines = 0;
                    data = "";
                }
            }
            return retv;
        }
        #endregion
    }
}
