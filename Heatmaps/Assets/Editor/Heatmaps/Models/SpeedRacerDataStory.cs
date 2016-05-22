using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityAnalyticsHeatmap
{
    public class SpeedRacerDataStory : DataStory
    {
        public SpeedRacerDataStory()
        {
            name = "Speed Racer";
            genre = "2D Car Racing Game";
            description = "Imagine that this data comes from a racing game with a classic oval track. ";
            description += "Your questions: how fast do players take the corners? Where do they crash?";
            whatToTry = "Generate this data, then click Process in the Heatmapper. In the render setting under 'Shape', ";
            whatToTry += "pick 'Point to Point' and a particle size around 2. Observe how you can see not ";
            whatToTry += "just where the user was in the virtual world, ";
            whatToTry += "but also how fast they were going. You can see that users have a lot more trouble on one side ";
            whatToTry += "of the track.\n\n";
            whatToTry += "This demo uses the same basic technique as VR Look At (sending two Vector3s), but instead of ";
            whatToTry += "using a collider to determine the 'destination' position, we simply calculate based on velocity.";

            sampleCode = "using UnityAnalyticsHeatmap;\n\n";
            sampleCode += "// projectionVector3 in this case is a calculation based on current direction\n";
            sampleCode += "// and velocity.\n";
            sampleCode += "HeatmapEvent.Send(\"PlayerPosition\",transform.position,projectionVector3,Time.timeSinceLevelLoad);\n";
            sampleCode += "// Send this one on crash to figure out where it all went wrong.\n";
            sampleCode += "HeatmapEvent.Send(\"Crash\",transform.position,projectionVector3,Time.timeSinceLevelLoad);";
        }

        float m_Radius = 100f;
        float m_RadiusFlattener = .5f;
        
        private void UpdateOval(ref Vector3 position, float p)
        {
            position.x = m_Radius * Mathf.Cos(p * Mathf.PI/180f);
            position.y = m_Radius * Mathf.Sin(p * Mathf.PI/180f) * m_RadiusFlattener;
            position.z = 0f;
        }

//        #region implemented abstract members of DataStory
//        public override Dictionary<double, string> Generate()
//        {
//            // Set a seed so set is consistently generated
//            UnityEngine.Random.seed = 42;
//
//            var retv = new Dictionary<double, string>();
//            
//            int linesPerFile = 500;
//            int currentFileLines = 0;
//            int playThroughs = 5;
//            int eventCount = 200;
//            float baseSpeed = 2f;
//            float speed = 0f;
//
//            string eventName1 = "Heatmap.PlayerPosition";
//            string eventName2 = "Heatmap.Crash";
//            int eventIndex = 0;
//
//            double firstDate = 0d;
//            DateTime now = DateTime.UtcNow;
//
//            string data = "";
//
//            for (int a  = 0; a < playThroughs; a++)
//            {
//                Vector3 position = Vector3.zero, destination = Vector3.zero;
//                float theta = 1f;
//                UpdateOval(ref position, theta);
//                for (int b = 0; b < eventCount; b++)
//                {
//                    speed = (Mathf.Abs(m_Radius - position.x)/m_Radius) * baseSpeed + UnityEngine.Random.Range(0.1f,5f);
//                    theta += speed;
//                    string eventName = eventName1;
//                    if (speed > baseSpeed * 4f && UnityEngine.Random.Range(0f,1f) > .25f) {
//                        eventName = eventName2;
//                    }
//
//                    Vector3 lastPosition = position;
//                    UpdateOval(ref position, theta);
//                    string evt = "";
//
//                    // Date
//                    var ta = new TimeSpan(TimeSpan.TicksPerSecond * eventIndex);
//                    DateTime dt = now.Subtract(ta);
//                    string dts = dt.ToString("yyyy-MM-dd hh:mm:ss.ms");
//                    evt += dts + "\t";
//                    if (currentFileLines == 0) {
//                        firstDate = Math.Round((dt - epoch).TotalSeconds);
//                    }
//
//                    // Device ID & name
//                    evt += "d" + a + "-XXXX-XXXX\t";
//                    evt += eventName + "\t";
//
//                    // Position and time
//                    evt += "{";
//                    evt += "\"x\":\"" + position.x + "\",";
//                    evt += "\"y\":\"" + position.y + "\",";
//                    evt += "\"z\":\"" + position.z + "\",";
//                    evt += "\"t\":\"" + b + "\",";
//
//                    // destination
//                    Vector3 diff = position-lastPosition;
//                    destination = Vector3.MoveTowards(position, position + (diff*speed), 10000f);
//                    evt += "\"dx\":\"" + destination.x + "\",";
//                    evt += "\"dy\":\"" + destination.y + "\",";
//                    evt += "\"dz\":\"" + destination.z + "\",";
//
//                    evt += "\"unity.name\":" + "\"" + eventName + "\"" + "}\n";
//
//                    data += evt;
//                    currentFileLines ++;
//                    eventIndex++;
//                    if (eventName == eventName2)
//                    {
//                        speed = 0f;
//                    }
//                    if (currentFileLines >= linesPerFile || eventIndex == eventCount-1)
//                    {
//                        retv.Add(firstDate, data);
//                        currentFileLines = 0;
//                        data = "";
//                    }
//                }
//            }
//            return retv;
//        }
//        #endregion





        #region implemented abstract members of DataStory
        public override Dictionary<double, string> Generate()
        {
            // Set a seed so set is consistently generated
            UnityEngine.Random.seed = 42;

            List<string> eventNames = new List<string>(){"Heatmap.PlayerPosition","Heatmap.Crash"};

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
                var dx = new TestEventParam("dx", TestEventParam.Str, "");
                customEvent.Add(dx);
                var dy = new TestEventParam("dy", TestEventParam.Str, "");
                customEvent.Add(dy);
                var dz = new TestEventParam("dz", TestEventParam.Str, "");
                customEvent.Add(dz);
                events.Add(customEvent);
            }

            var retv = new Dictionary<double, string>();

            string data = RawDataInspector.headers;
            int fileCount = 0;
            int eventCount = 300;
            int deviceCount = 2;
            int sessionCount = 2;

            // Custom to this lesson
            float baseSpeed = 2f;
            float crashMod = 3.5f;
            float speed = 0f;

            DateTime now = DateTime.UtcNow;
            int totalSeconds = deviceCount * eventCount * sessionCount;
            double endSeconds = Math.Round((now - epoch).TotalSeconds);
            double startSeconds = endSeconds - totalSeconds;
            double currentSeconds = startSeconds;
            double firstDate = currentSeconds;

            Vector3 position = Vector3.zero, destination = Vector3.zero;

            for (int a = 0; a < deviceCount; a++)
            {
                string platform = "ios";
                for (int b = 0; b < sessionCount; b++)
                {
                    float theta = 1f;
                    UpdateOval(ref position, theta);
                    for (int c = 0; c < eventCount; c++)
                    {
                        
                        speed = (Mathf.Abs(m_Radius - position.x)/m_Radius) * baseSpeed + UnityEngine.Random.Range(0.1f,5f);
                        theta += speed;

                        Vector3 lastPosition = position;
                        UpdateOval(ref position, theta);


                        currentSeconds ++;
                        TestCustomEvent customEvent = (speed > baseSpeed * crashMod && UnityEngine.Random.Range(0f,1f) > .25f) ? events[1] : events[0];
                        customEvent.SetParam("t", c.ToString());

                        // destination
                        Vector3 diff = position-lastPosition;
                        destination = Vector3.MoveTowards(position, position + (diff*speed), 10000f);

                        customEvent.SetParam("x", position.x.ToString());
                        customEvent.SetParam("y", position.y.ToString());
                        customEvent.SetParam("z", position.z.ToString());
                        customEvent.SetParam("dx", destination.x.ToString());
                        customEvent.SetParam("dy", destination.y.ToString());
                        customEvent.SetParam("dz", destination.z.ToString());

                        string evt = customEvent.WriteEvent(a, b, currentSeconds, platform);
                        data += evt;

                    }
                }
            }
            retv.Add(firstDate, data);
            fileCount++;
            return retv;
        }
        #endregion
    }
}
