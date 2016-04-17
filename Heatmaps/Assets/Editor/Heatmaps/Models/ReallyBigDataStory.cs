using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityAnalyticsHeatmap
{
    public class ReallyBigDataStory : DataStory
    {
        public ReallyBigDataStory()
        {
            name = "Really Big Game";
            genre = "3D Flight Combat Sim";
            description = "This demonstrates some important ideas about scale, direction, and time.";
            whatToTry = "Generate this data. Open the Heatmapper and click the Process button, which first shows you combat kills. ";
            whatToTry += "Or does it? You might not see much right away for two reasons. First this demo shows data in a very large ";
            whatToTry += "area and you're probably zoomed in. Second, check the Particle size. Try setting size to around 25,  ";
            whatToTry += "then zoom out so you can see all the points. ";
            whatToTry += "Notice that this data is a bit sparse because of the scale of the map. ";
            whatToTry += "Under 'Aggregate', change the value of 'Space Smooth' to 500 and re-process. ";
            whatToTry += "Adjust the particle size to 250. Now you can see the general areas where ";
            whatToTry += "kills have occurred and the map becomes more useful.\n\n";

            whatToTry += "Under Render, Find the 'Option' dropdown. If you click it, you'll see in addition to 'CombatKills' ";
            whatToTry += "there's an option for 'PlayerPosition'. Choose that and instead of seeing kills, you'll see where in this sim ";
            whatToTry += "your players have gone. While space smooth of 500 was good for kills, it's likely too coarse for player position. ";
            whatToTry += "Re-adjust space smoothing and particle size to 10. ";
            whatToTry += "Uncheck the 'Direction' checkbox and Process again. ";
            whatToTry += "Now, under Particle 'Shape' pick 'Arrow'. What you're now seeing is not simply WHERE the player went, ";
            whatToTry += "but what direction they flew.\n\n";

            whatToTry += "Under 'Aggregate', uncheck 'Time', then Process again. ";
            whatToTry += "You might try bringing the particle size up to around 25. In the Render section ";
            whatToTry += "under 'Time' note the start and end values. Change the end value to 1, change 'Play Speed' to 0.1 and ";
            whatToTry += "press the 'Play' button to watch the airplanes fly! With a little practice, you can even scrub the timeline.";
        }

        #region implemented abstract members of DataStory
        public override Dictionary<double, string> Generate()
        {
            // Set a seed so set is consistently generated
            UnityEngine.Random.seed = 42;

            var retv = new Dictionary<double, string>();

            int playThroughs = 5;
            int eventCount = 500;
            int linesPerFile = 500;
            int currentFileLines = 0;

            float randomRange = .25f;
            float radius = 1000f;

            double firstDate = 0d;
            DateTime now = DateTime.UtcNow;

            string data = "";
            string[] eventNames = new string[]{ "Heatmap.CombatKills", "Heatmap.PlayerPosition" };

            Vector3 position = Vector3.zero, rotation = Vector3.zero, pointOnCircle = Vector3.zero;
            for (int a = 0; a < playThroughs; a++)
            {

                for (int b = 0; b < eventCount; b++)
                {

                    string evt = "";

                    // Date
                    DateTime dt = now.Subtract(new TimeSpan((TimeSpan.TicksPerSecond * (eventCount - b)) + (TimeSpan.TicksPerSecond*(eventCount-a))));
                    string dts = dt.ToString("yyyy-MM-dd hh:mm:ss.ms");
                    evt += dts + "\t";
                    if (currentFileLines == 0) {
                        firstDate = Math.Round((dt - epoch).TotalSeconds);
                    }

                    if (b == 0) {
                        position = Vector3.zero;
                        rotation = Vector3.zero;
                        pointOnCircle = new Vector3(UnityEngine.Random.Range(-radius, radius),
                            UnityEngine.Random.Range(-radius, radius),
                            UnityEngine.Random.Range(-radius, radius));
                    }

                    // Device ID & name
                    evt += "d" + a + "-XXXX-XXXX\t";
                    string eventName = (b % 100 == 0) ? eventNames[0] : eventNames[1];
                    evt += eventName + "\t";

                    // Build the JSON
                    Vector3 lastPosition = new Vector3(position.x,position.y,position.z);

                    position = UpdatePosition(ref position, ref pointOnCircle, radius, randomRange);

                    //create the rotation to look at the target
                    Vector3 dir = (lastPosition-position).normalized;
                    rotation = Quaternion.LookRotation(dir).eulerAngles;

                    evt += "{";
                    evt += "\"x\":\"" + position.x + "\",";
                    evt += "\"y\":\"" + position.y + "\",";
                    evt += "\"z\":\"" + position.z + "\",";

                    evt += "\"t\":\"" + (2f*b) + "\",";

                    evt += "\"rx\":\"" + rotation.x + "\",";
                    evt += "\"ry\":\"" + rotation.y + "\",";
                    evt += "\"rz\":\"" + rotation.z + "\",";

                    evt += "\"unity.name\":" + "\"" + eventName + "\"" + "}\n";

                    data += evt;
                    currentFileLines ++;
                    if (currentFileLines >= linesPerFile || b == eventCount-1) {
                        retv.Add(firstDate, data);
                        currentFileLines = 0;
                        data = "";
                    }
                }
            }
            return retv;
        }
        #endregion
    }
}
