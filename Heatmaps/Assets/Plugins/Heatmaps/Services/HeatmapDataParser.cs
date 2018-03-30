/// <summary>
/// Parses heatmap JSON data for the purpose of loading into the renderer.
/// </summary>
/// This code assumes that data is in the form:
/// {
/// 	"EventName": [
/// 		{"y": XX, "x": XX, "z": -XX, "t": XX, "d": XX},
/// 		...
/// 	],
/// 	"AnotherEventName": [
/// 		...
/// 	],
/// 	...
/// }

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MiniJSON;
using UnityEngine;

namespace RVEALR.Heatmaps
{
    public class HeatmapDataParser
    {
        public const int k_AsResource = 0;
        public const int k_AsStream = 1;
        public const int k_AsData = 2;


        public delegate void ParseHandler(Dictionary<string, HeatPoint[]> heatData, string[] options);

        ParseHandler m_ParseHandler;
        string m_RemapLabel = "";

        public HeatmapDataParser()
        {
        }

        /// <summary>
        /// Loads the data.
        /// </summary>
        /// <param name="path">A location from which to load the data.</param>
        /// <param name="handler">A method handler to which we return the data.</param>
        /// <param name="asResource">If set to <c>true</c> the path is assumed to be a Resource location rather than a URI.</param>
        public void LoadData(string path, ParseHandler handler, int method = k_AsData, string remapLabel = "")
        {
            m_ParseHandler = handler;
            m_RemapLabel = remapLabel;
            if (!string.IsNullOrEmpty(path))
            {
                switch (method)
                {
                    case k_AsData:
                        ConsumeHeatmapData(path);
                        break;
                    case k_AsResource:
                        LoadResource(path);
                        break;
                    case k_AsStream:
                        LoadStream(path);
                        break;
                }
            }
        }

        /// <summary>
        /// Load data from a URI
        /// </summary>
        /// <param name="path">A location from which to load the data.</param>
        protected void LoadStream(string path)
        {
            var reader = new StreamReader(path);
            using (reader)
            {
                ConsumeHeatmapData(reader.ReadToEnd());
            }
        }

        /// <summary>
        /// Load data from a Resource location (suitable for runtime use)
        /// </summary>
        /// <param name="path">A location from which to load the data.</param>
        protected void LoadResource(string path)
        {
            if (File.Exists(path))
            {
                using(var stream = new StreamReader(path))
                {
                    string text = stream.ReadToEnd();
                    ConsumeHeatmapData(text);
                }
            }
        }

        /// <summary>
        /// Read the JSON data and convert into Lists of HeatPoint structs.
        /// </summary>
        /// <param name="text">The loaded data.</param>
        public void ConsumeHeatmapData(string text)
        {
            var heatData = new Dictionary<string, HeatPoint[]>();
            var keys = new ArrayList();
            float maxDensity = 0;
            float maxTime = 0;
            Vector3 lowSpace = Vector3.zero;
            Vector3 highSpace = Vector3.zero;

            Dictionary<string, object> data = Json.Deserialize(text) as Dictionary<string, object>;
            foreach (KeyValuePair<string, object> kv in data)
            {
                keys.Add(kv.Key);

                var pointList = kv.Value as List<object>;
                var array = new HeatPoint[pointList.Count];
                for (int a = 0, aa = pointList.Count; a < aa; a++)
                {
                    array[a] = new HeatPoint();
                    float x = 0, y = 0, z = 0, t = 0, rx = 0, ry = 0, rz = 0, dx = 0, dy = 0, dz = 0;
                    float d = 0;
                    var pt = pointList[a] as Dictionary<string, object>;

                    foreach (KeyValuePair<string,object> pointKV in pt)
                    {
                        var value = (float)Convert.ToDouble(pointKV.Value);
                        switch (pointKV.Key)
                        {
                            case "x":
                                x = value;
                                break;
                            case "y":
                                y = value;
                                break;
                            case "z":
                                z = value;
                                break;
                            case "t":
                                t = value;
                                break;
                            case "d":
                                d = value;
                                break;
                            case "rx":
                                rx = value;
                                break;
                            case "ry":
                                ry = value;
                                break;
                            case "rz":
                                rz = value;
                                break;
                            case "dx":
                                dx = value;
                                break;
                            case "dy":
                                dy = value;
                                break;
                            case "dz":
                                dz = value;
                                break;
                        }
                    }
                    array[a].position = new Vector3(x, y, z);
                    array[a].rotation = new Vector3(rx, ry, rz);
                    array[a].destination = new Vector3(dx, dy, dz);
                    array[a].density = d;
                    if (!String.IsNullOrEmpty(m_RemapLabel))
                    {
                        array[a].densityLabel = m_RemapLabel;
                    }
                    array[a].time = t;
                    maxDensity = Mathf.Max(d, maxDensity);
                    maxTime = Mathf.Max(array[a].time, maxTime);
                    if (a == 0)
                    {
                        lowSpace = highSpace = array[a].position;
                    }
                    else
                    {
                        lowSpace = Vector3.Min(array[a].position, lowSpace);
                        highSpace = Vector3.Max(array[a].position, highSpace);
                    }

                }
                heatData[kv.Key] = array;
            }

            if (m_ParseHandler != null)
            {
                m_ParseHandler(heatData, keys.ToArray(typeof(string)) as string[]);
            }
        }
    }
}
