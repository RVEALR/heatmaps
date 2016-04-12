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
            whatToTry += "pick 'Point to Point'. Observe how you can see not just where the user was in the virtual world, ";
            whatToTry += "but also how fast they were going. You can see that users have a lot more trouble on one side ";
            whatToTry += "of the track.\n\n";
            whatToTry += "This demo uses the same basic technique as VR Look At (sending two Vector3s), but instead of ";
            whatToTry += "using a collider to determine the 'destination' position, we simply calculate based on velocity.";
        }

        float radius = 100f;
        float radiusFlattener = .5f;
        
        private void UpdateOval(ref Vector3 position, float p)
        {
            position.x = radius * Mathf.Cos(p * Mathf.PI/180f);
            position.y = radius * Mathf.Sin(p * Mathf.PI/180f) * radiusFlattener;
            position.z = 0f;
        }

        #region implemented abstract members of DataStory
        public override Dictionary<double, string> Generate()
        {
            var retv = new Dictionary<double, string>();
            
            int linesPerFile = 500;
            int currentFileLines = 0;
            int playThroughs = 5;
            int eventCount = 200;
            float baseSpeed = 2f;
            float speed = 0f;

            string eventName1 = "Heatmap.PlayerPosition";
            string eventName2 = "Heatmap.Crash";
            int eventIndex = 0;
            
            double firstDate = 0d;
            DateTime now = DateTime.UtcNow;
            
            string data = "";
            
            
            Vector3 position = Vector3.zero, destination = Vector3.zero;
            float theta = 1f;
            UpdateOval(ref position, theta);

            for (int a  = 0; a < playThroughs; a++)
            {
                theta = 1f;
                for (int b = 0; b < eventCount; b++)
                {
                    //
                    speed = (Mathf.Abs(radius - position.x)/radius) * baseSpeed + UnityEngine.Random.Range(0.1f,5f);
                    theta = theta + speed;
                    string eventName = eventName1;
                    if (speed > baseSpeed * 4f && UnityEngine.Random.Range(0f,1f) > .25f) {
                        eventName = eventName2;
                    }

                    Vector3 lastPosition = position;
                    UpdateOval(ref position, theta);
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
                    evt += "d" + a + "-XXXX-XXXX\t";
                    evt += eventName + "\t";
                    
                    // Position and time
                    evt += "{";
                    evt += "\"x\":\"" + position.x + "\",";
                    evt += "\"y\":\"" + position.y + "\",";
                    evt += "\"z\":\"" + position.z + "\",";
                    evt += "\"t\":\"" + b + "\",";
                    
                    // destination
                    Vector3 diff = position-lastPosition;
                    destination = Vector3.MoveTowards(position, position + (diff*speed), 10000f);
                    evt += "\"dx\":\"" + destination.x + "\",";
                    evt += "\"dy\":\"" + destination.y + "\",";
                    evt += "\"dz\":\"" + destination.z + "\",";
                    
                    evt += "\"unity.name\":" + "\"" + eventName + "\"" + "}\n";
                    
                    data += evt;
                    currentFileLines ++;
                    eventIndex++;
                    if (eventName == eventName2)
                    {
                        speed = 0f;
                    }
                    if (currentFileLines >= linesPerFile || eventIndex == eventCount-1)
                    {
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
