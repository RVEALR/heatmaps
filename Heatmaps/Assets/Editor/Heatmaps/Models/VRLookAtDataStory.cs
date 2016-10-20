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
            whatToTry += "pick 'Point to Point' (you may also want to set particle size to around 10). Observe how you can see not just where the user was in the virtual world, ";
            whatToTry += "but also what they were looking at.\n\n";
            whatToTry += "This is done by sending two Vector3s. The first is the position of the player. The second ";
            whatToTry += "is the position of a collider to which we raycast. The same technique could be used in a first-person ";
            whatToTry += "shooter to find the things the player shot.\n\n";
            whatToTry += "This map is pretty busy. Look at the 'Masking' controls. These allow you to trim away data based on its position. ";
            whatToTry += "Note how the Y axis has no handles. This is because all the Y data in this demo is at ";
            whatToTry += "the same ordinate. Try tweaking the X and Z values to isolate out and inspect a single source position.";

            sampleCode = "using UnityAnalyticsHeatmap;\n\n";
            sampleCode += "// The otherGameObject in this case is a GameObject represented by a collider.\n";
            sampleCode += "// By raycasting from where the player is standing, we can see what they saw.\n";
            sampleCode += "HeatmapEvent.Send(\"LookAt\",transform.position,otherGameObject.transform.position,Time.timeSinceLevelLoad);";
        }

        #region implemented abstract members of DataStory
        public override Dictionary<double, string> Generate()
        {
            SetRandomSeed();
            List<string> eventNames = new List<string>(){"Heatmap.LookAt"};

            List<TestCustomEvent> events = new List<TestCustomEvent>();
            for (int a = 0; a < eventNames.Count; a++)
            {
                TestCustomEvent customEvent = new TestCustomEvent();
                customEvent.name = eventNames[a];
                var x = new TestEventParam("x", TestEventParam.Str, "");
                customEvent.Add(x);
                var y = new TestEventParam("y", TestEventParam.Str, "");
                customEvent.Add(y);
                var z = new TestEventParam("z", TestEventParam.Str, "");
                customEvent.Add(z);
                var t = new TestEventParam("t", TestEventParam.Str, "");
                customEvent.Add(t);
                var dx = new TestEventParam("rx", TestEventParam.Str, "");
                customEvent.Add(dx);
                var dy = new TestEventParam("ry", TestEventParam.Str, "");
                customEvent.Add(dy);
                var dz = new TestEventParam("rz", TestEventParam.Str, "");
                customEvent.Add(dz);
                events.Add(customEvent);
            }

            var retv = new Dictionary<double, string>();

            string data = "";
            int fileCount = 0;
            int waypointsPerPlay = 25;
            int deviceCount = 5;
            int sessionCount = 1;
            string platform = "ios";

            // Custom to this lesson
            int lookThisManyPlaces = 15;
            int lookThisManyTimesMin = 1;
            int lookThisManyTimesMax = 5;
//            float randomRange = .25f;
            float radius = 50f;
//            float halfRadius = 25f;

            DateTime now = DateTime.UtcNow;
            int totalSeconds = deviceCount * waypointsPerPlay * sessionCount;
            double endSeconds = Math.Round((now - UnityAnalytics.DateTimeUtils.s_Epoch).TotalSeconds);
            double startSeconds = endSeconds - totalSeconds;
            double currentSeconds = startSeconds;
            double firstDate = currentSeconds;

            int currentPoint = 0;
            int currentPlace = 0;
            int totalPoints = 0;

            float shortestUser = 1.5f;
            float tallestUser = 2.5f;

            int totalPlaces = deviceCount * sessionCount * waypointsPerPlay * lookThisManyPlaces;
            int[] numTimesToLookList = new int[totalPlaces];
            for (int d = 0; d < totalPlaces; d++)
            {
                numTimesToLookList[d] = UnityEngine.Random.Range(lookThisManyTimesMin, lookThisManyTimesMax);
                totalPoints += numTimesToLookList[d];
            }

            Vector3 position = Vector3.zero, destination = Vector3.zero;

            for (int a = 0; a < deviceCount; a++)
            {
                for (int b = 0; b < sessionCount; b++)
                {
                    for (int c = 0; c < waypointsPerPlay; c++)
                    {
                        position = new Vector3(UnityEngine.Random.Range(-radius, radius), UnityEngine.Random.Range(shortestUser, tallestUser), UnityEngine.Random.Range(-radius, radius));
                        for (int e = 0; e < lookThisManyPlaces; e++)
                        {
                            Vector3 rotation = new Vector3(UnityEngine.Random.Range(0, 90f), UnityEngine.Random.Range(-180f, 180f), 0f);

                            while (numTimesToLookList[currentPlace] > 0)
                            {
                                Progress(currentPoint, totalPoints);
                                currentPoint ++;
                                numTimesToLookList[currentPlace] --;
                                currentSeconds ++;
                                TestCustomEvent customEvent = events[0];
                                customEvent.SetParam("t", c.ToString());

                                customEvent.SetParam("x", position.x.ToString());
                                customEvent.SetParam("y", position.y.ToString());
                                customEvent.SetParam("z", position.z.ToString());
                                customEvent.SetParam("rx", rotation.x.ToString());
                                customEvent.SetParam("ry", rotation.y.ToString());
                                customEvent.SetParam("rz", rotation.z.ToString());

                                string evt = customEvent.WriteEvent(a, b, currentSeconds, platform);
                                data += evt;
                            }
                            currentPlace ++;
                        }

                    }
                }
            }
            EndProgress();
            retv.Add(firstDate, data);
            fileCount++;
            return retv;
        }
        #endregion
    }
}

