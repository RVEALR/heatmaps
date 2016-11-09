﻿/// <summary>
/// Adapter API for sending Heatmap analytics events
/// </summary>
/// This is <i>simply</i> an adapter. As such, you could choose not to
/// use it at all, but by passing your events through this API you gain type
/// safety and ensure that you're conforming to the data that the aggregator
/// and Heatmapper expect to receive.
/// 
/// The script is designed to work in Unity 4.6 > 5.x

using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 ||  UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0
using analyticsResultNamespace = UnityEngine.Cloud.Analytics;
using analyticsEventNamespace = UnityEngine.Cloud.Analytics.UnityAnalytics;


#else
using analyticsResultNamespace = UnityEngine.Analytics;
using analyticsEventNamespace = UnityEngine.Analytics.Analytics;
#endif


namespace UnityAnalyticsHeatmap
{
    public class HeatmapEvent
    {
        private static Dictionary<string, object> s_Dictionary = new Dictionary<string, object>();

        private static bool s_SaveToLocal = false;
        private static string s_LocalSavePath;

        private static string s_SessionId;

        /// <summary>
        /// When set to true, HeatmapEvents are saved to a local file, instead of sent to the server.
        /// </summary>
        /// <param name="value">If set to <c>true</c> save to a local file.</param>
        public static bool saveToLocal
        {
            get
            {
                return s_SaveToLocal;
            }
            set
            {
                s_SaveToLocal = value;
            }
        }

        /// <summary>
        /// Sets the local save path.
        /// </summary>
        /// <value>The path on the local drive where HeatmapEvents will be saved.</param>
        public static string localSavePath
        {
            get
            {
                return s_LocalSavePath;
            }
            set
            {
                s_LocalSavePath = value;
            }
        }

        /// <summary>
        /// Send the event with position and an optional dictionary.
        /// </summary>
        /// Note that Vector2 will implicitly convert to Vector3
        public static analyticsResultNamespace.AnalyticsResult Send(string eventName, Vector3 v, Dictionary<string, object> options = null)
        {
            AddXY(v.x, v.y);
            AddZ(v.z);
            AddOptions(options);
            return Commit(eventName);
        }

        /// <summary>
        /// Send the event with position, time and an optional dictionary.
        /// </summary>
        /// Note that Vector2 will implicitly convert to Vector3
        public static analyticsResultNamespace.AnalyticsResult Send(string eventName, Vector3 v, float time, Dictionary<string, object> options = null)
        {
            AddXY(v.x, v.y);
            AddZ(v.z);
            AddTime(time);
            AddOptions(options);
            return Commit(eventName);
        }

        /// <summary>
        /// Send the event with position, time, rotation (as a float) and an optional dictionary.
        /// </summary>
        /// Note that Vector2 will implicitly convert to Vector3.
        /// Note also that this variation is particularly suited to 2D environments
        public static analyticsResultNamespace.AnalyticsResult Send(string eventName, Vector3 v, float time, float rotation, Dictionary<string, object> options = null)
        {
            AddXY(v.x, v.y);
            AddZ(v.z);
            s_Dictionary["rx"] = rotation;
            AddTime(time);
            AddOptions(options);
            return Commit(eventName);
        }

        /// <summary>
        /// Send the event with position, rotation and an optional dictionary.
        /// </summary>
        public static analyticsResultNamespace.AnalyticsResult Send(string eventName, Transform trans, Dictionary<string, object> options = null)
        {
            AddXY(trans.position.x, trans.position.y);
            AddZ(trans.position.z);
            AddRotation(trans.rotation.eulerAngles);
            AddOptions(options);
            return Commit(eventName);
        }

        /// <summary>
        /// Send the event with position, rotation, time and an optional dictionary.
        /// </summary>
        public static analyticsResultNamespace.AnalyticsResult Send(string eventName, Transform trans, float time, Dictionary<string, object> options = null)
        {
            AddXY(trans.position.x, trans.position.y);
            AddZ(trans.position.z);
            AddRotation(trans.rotation.eulerAngles);
            AddTime(time);
            AddOptions(options);
            return Commit(eventName);
        }

        /// <summary>
        /// Send the event with position, rotation and an optional dictionary.
        /// </summary>
        public static analyticsResultNamespace.AnalyticsResult Send(string eventName, Vector3 position, Quaternion q, Dictionary<string, object> options = null)
        {
            AddXY(position.x, position.y);
            AddZ(position.z);
            AddRotation(q.eulerAngles);
            AddOptions(options);
            return Commit(eventName);
        }

        /// <summary>
        /// Send the event with position, rotation, time and an optional dictionary.
        /// </summary>
        public static analyticsResultNamespace.AnalyticsResult Send(string eventName, Vector3 position, Quaternion q, float time, Dictionary<string, object> options = null)
        {
            AddXY(position.x, position.y);
            AddZ(position.z);
            AddRotation(q.eulerAngles);
            AddTime(time);
            AddOptions(options);
            return Commit(eventName);
        }

        /// <summary>
        /// Send the event with position, destination and an optional dictionary.
        /// </summary>
        public static analyticsResultNamespace.AnalyticsResult Send(string eventName, Vector3 v, Vector3 v1, Dictionary<string, object> options = null)
        {
            AddXY(v.x, v.y);
            AddZ(v.z);
            AddDestination(v1);
            AddOptions(options);
            return Commit(eventName);
        }

        /// <summary>
        /// Send the event with position, destination, time and an optional dictionary.
        /// </summary>
        public static analyticsResultNamespace.AnalyticsResult Send(string eventName, Vector3 v, Vector3 v1, float time, Dictionary<string, object> options = null)
        {
            AddXY(v.x, v.y);
            AddZ(v.z);
            AddDestination(v1);
            AddTime(time);
            AddOptions(options);
            return Commit(eventName);
        }

        /// <summary>
        /// Transmit the event
        /// </summary>
        protected static analyticsResultNamespace.AnalyticsResult Commit(string eventName)
        {
            analyticsResultNamespace.AnalyticsResult result;
            if (s_SaveToLocal)
            {
                string path = String.IsNullOrEmpty(s_LocalSavePath) ? System.IO.Path.Combine(Application.dataPath, "RawData") : s_LocalSavePath;
                result = analyticsResultNamespace.AnalyticsResult.Ok;
                using (var writer = new StreamWriter(path, true))
                {
                    s_SessionId = (String.IsNullOrEmpty(s_SessionId)) ? System.Guid.NewGuid().ToString() : s_SessionId;
                    string evt = WriteEvent("Heatmap." + eventName, s_Dictionary, "TestDevice", s_SessionId, Application.platform.ToString(), Debug.isDebugBuild);
                    writer.WriteLine(evt);
                }
            }
            else
            {
                result = analyticsEventNamespace.CustomEvent("Heatmap." + eventName, s_Dictionary);
            }
            s_Dictionary.Clear();
            return result;
        }

        /// <summary>
        /// Convenience method for adding X/Y to dict
        /// </summary>
        protected static void AddXY(float x, float y)
        {
            s_Dictionary["x"] = x;
            s_Dictionary["y"] = y;
        }

        /// <summary>
        /// Convenience method for adding Z to dict
        /// </summary>
        protected static void AddZ(float z)
        {
            s_Dictionary["z"] = z;
        }

        /// <summary>
        /// Convenience method for adding time to dict
        /// </summary>
        protected static void AddTime(float time)
        {
            s_Dictionary["t"] = time;
        }

        /// <summary>
        /// Convenience method for adding rotation
        /// </summary>
        protected static void AddRotation(Vector3 r)
        {
            s_Dictionary["rx"] = r.x;
            s_Dictionary["ry"] = r.y;
            s_Dictionary["rz"] = r.z;
        }

        protected static void AddDestination(Vector3 v)
        {
            s_Dictionary["dx"] = v.x;
            s_Dictionary["dy"] = v.y;
            s_Dictionary["dz"] = v.z;
        }

        /// <summary>
        /// Convenience method for adding options to dict
        /// </summary>
        protected static void AddOptions(Dictionary<string, object> options)
        {
            if (options != null)
            {
                foreach (KeyValuePair<string, object> entry in options)
                {
                    s_Dictionary[entry.Key] = entry.Value;
                }
            }
        }

        public static string WriteEvent(string eventName, Dictionary<string, object> parameters, string deviceId, string sessionId, string platform, bool isDebug = false)
        {
            double currentMilliseconds = Math.Floor((DateTime.UtcNow - UnityAnalytics.DateTimeUtils.s_Epoch).TotalMilliseconds);

            string evt = "";
            evt += currentMilliseconds + "\t";

            // AppID
            #if UNITY_5
            evt += (string.IsNullOrEmpty(Application.cloudProjectId)) ? "1234-abcd-5678-efgh" : Application.cloudProjectId;
            #else
            evt += "1234-abcd-5678-efgh";
            #endif
            evt += "\t";

            // Event Type
            evt += "custom\t";

            // User ID, Session ID
            evt += deviceId + "\t";
            evt += sessionId + "\t";

            // Remote IP
            evt += "1.1.1.1\t";

            // Platform
            evt += platform + "\t";

            // SDK Version
            evt += "5.3.4\t";

            // IsDebug
            evt += isDebug + "\t";

            // User agent
            evt += "Corridor%20Z/3 CFNetwork/758.2.8 Darwin/15.0.0\t";

            // Submit time
            evt += currentMilliseconds + "\t";

            // Event Name
            evt += eventName + "\t";

            evt += WriteParams(eventName, parameters);

            return evt;
        }

        static string WriteParams(string eventName, Dictionary<string, object> parameters)
        {
            string json = "{";

            foreach(KeyValuePair<string, object> kv in parameters)
            {
                json += Quotify(kv.Key) + ":" + Quotify(kv.Value.ToString());
                json += ",";
            }
            json += Quotify("unity.name") + ":" + Quotify(eventName) + "}";
            return json;
        }

        static string Quotify(string value)
        {
            return "\"" + value + "\"";
        }

    }
}
