/// <summary>
/// Abstract class for setting up demo data in the heatmap Raw Data Generator.
/// </summary>

using System;
using System.Collections.Generic;
using UnityEngine;


namespace UnityAnalyticsHeatmap
{
    public abstract class DataStory
    {
        public DataStory()
        {
        }
        public static DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);

        public string name = "";
        public string genre = "";
        public string description = "";
        public string whatToTry = "";
        public string sampleCode = "";

        public virtual Dictionary<double, string> Generate()
        {
            // Set a seed so set is consistently generated
            #if UNITY_5_4_OR_NEWER
            UnityEngine.Random.InitState(42);
            #elif
            UnityEngine.Random.seed = 42;
            #endif
            return null;
        }

        public override string ToString()
        {
            string str = "Genre: " + genre + "\n";
            str += "Description: " + description + "\n";
            str += "What to try: " + whatToTry;
            return str;
        }

        protected Vector3 UpdatePosition(ref Vector3 position, ref Vector3 pointOnCircle, float radius, float range)
        {
            pointOnCircle.x = pointOnCircle.x + UnityEngine.Random.Range(0, range);
            float xr = Mathf.Sin(pointOnCircle.x) * radius;
            position.x = xr;

            pointOnCircle.y += UnityEngine.Random.Range(0, range * 2f);
            float yr = Mathf.Sin(pointOnCircle.y) * radius;
            position.y = yr;

            pointOnCircle.z += UnityEngine.Random.Range(0, range * 4f);
            float zr = Mathf.Sin(pointOnCircle.z) * radius;
            position.z = zr;

            return position;
        }
    }
}

