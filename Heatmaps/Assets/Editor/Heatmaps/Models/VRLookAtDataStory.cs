using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityAnalyticsHeatmap
{
    public class VRLookAtDataStory : DataStory
    {
        public VRLookAtDataStory()
        {
            name = "VR Look At";
            genre = "VR Adventure";
            description = "Imagine this data as part of a VR game. Your question: as they move through my game, ";
            description += "are users looking where I want them to look?";
            whatToTry = "Generate this data, then click Process in the Heatmapper. In the render setting under 'Shape', ";
            whatToTry += "pick 'Point to Point'. Observe how you can see not just where the user was in the virtual world, ";
            whatToTry += "but also what they were looking at.\n\n";
            whatToTry += "This is done by sending two Vector3s. The first is the position of the player. The second ";
            whatToTry += "is the position of a collider to which we raycast. The same technique could be used in a first-person ";
            whatToTry += "shooter to find the things the player shot.\n\n";
            whatToTry += "Now look at the 'Masking' subsection. This allows you to trim away data based on its position.";
            whatToTry += "Note how the Y axis has no handles. This is because all the Y data in this demo is at ";
            whatToTry += "the same ordinate. Try tweaking the X and Z values to isolate out and inspect a single source position.";
        }

        DateTime now;

        #region implemented abstract members of DataStory
        public override Dictionary<double, string> Generate()
        {
            var retv = new Dictionary<double, string>();

            int linesPerFile = 500;
            int currentFileLines = 0;
            int positions = 30;
            int lookThisManyPlaces = 3;
            int lookThisManyTimesMin = 5;
            int lookThisManyTimesMax = 20;
            string eventName = "Heatmap.LookAt";
            int eventIndex = 0;

            float randomRange = .25f;
            float radius = 1000f;

            double firstDate = 0d;
            now = DateTime.UtcNow;

            string data = "";


            Vector3 position = Vector3.zero, destination = Vector3.zero, pointOnCircle = Vector3.zero;
            int a=0, b=0;
            while (a < positions)
            {
                position = Vector3.zero;
                destination = Vector3.zero;
                pointOnCircle = new Vector3(UnityEngine.Random.Range(-radius, radius),
                    0f,
                    UnityEngine.Random.Range(-radius, radius));
                position = UpdatePosition(ref position, ref pointOnCircle, radius, randomRange);
                position.y = 0f;

                while (b < lookThisManyPlaces)
                {
                    int numTimesToLook = UnityEngine.Random.Range(lookThisManyTimesMin, lookThisManyTimesMax);
                    float xAddition = UnityEngine.Random.Range(-radius, radius);
                    float yAddition = UnityEngine.Random.Range(0, radius/2f);
                    float zAddition = UnityEngine.Random.Range(-radius, radius);

                    for (int c = 0; c < numTimesToLook; c++)
                    {
                        string evt = "";

                        // Date
                        var ta = new TimeSpan(TimeSpan.TicksPerSecond * eventIndex);
                        DateTime dt = now.Subtract(ta);
                        string dts = dt.ToString("yyyy-MM-dd hh:mm:ss.ms");
                        evt += dts + "\t";
                        if (currentFileLines == 0) {
                            firstDate = Math.Round((dt - epoch).TotalSeconds);
                        }

                        // Device ID & name
                        evt += "d0-XXXX-XXXX\t";
                        evt += eventName + "\t";

                        // Position and time
                        evt += "{";
                        evt += "\"x\":\"" + position.x + "\",";
                        evt += "\"y\":\"" + position.y + "\",";
                        evt += "\"z\":\"" + position.z + "\",";
                        evt += "\"t\":\"" + b + "\",";

                        // destination
                        destination = new Vector3(position.x + xAddition, position.y + yAddition, position.z + zAddition);
                        evt += "\"dx\":\"" + destination.x + "\",";
                        evt += "\"dy\":\"" + destination.y + "\",";
                        evt += "\"dz\":\"" + destination.z + "\",";

                        evt += "\"unity.name\":" + "\"" + eventName + "\"" + "}\n";

                        data += evt;
                        currentFileLines ++;
                        eventIndex++;
                        if (currentFileLines >= linesPerFile || (a == positions-1 && b == lookThisManyPlaces-1 && c == numTimesToLook-1)) {
                            retv.Add(firstDate, data);
                            currentFileLines = 0;
                            data = "";
                        }
                    }

                    if (UnityEngine.Random.Range(0f, 1f) > .5f) {
                        b++;
                    }
                }
                b=0;
                if (UnityEngine.Random.Range(0f, 1f) > .5f) {
                    a++;
                }
            }
            return retv;
        }
        #endregion
    }
}

