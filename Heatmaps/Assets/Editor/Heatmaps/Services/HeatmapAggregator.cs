/// <summary>
/// Handles aggregation of raw data into heatmap data.
/// </summary>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

namespace UnityAnalyticsHeatmap
{
    public class HeatmapAggregator
    {

        int m_ReportFiles = 0;
        int m_ReportRows = 0;
        int m_ReportLegalPoints = 0;

        Dictionary<Tuplish, Dictionary<string, float>> m_PointDict;
        string m_DataPath = "";

        public delegate void CompletionHandler(string jsonPath);

        CompletionHandler m_CompletionHandler;


        public HeatmapAggregator(string dataPath)
        {
            SetDataPath(dataPath);
        }

        /// <summary>
        /// Sets the data path.
        /// </summary>
        /// <param name="dataPath">The location on the host machine from which to retrieve data.</param>
        public void SetDataPath(string dataPath)
        {
            m_DataPath = dataPath;
        }

        /// <summary>
        /// Process the specified inputFiles, using the other specified parameters.
        /// </summary>
        /// <param name="inputFiles">A list of one or more raw data text files.</param>
        /// <param name="startDate">Any timestamp prior to this ISO 8601 date will be trimmed.</param>
        /// <param name="endDate">Any timestamp after to this ISO 8601 date will be trimmed.</param>
        /// <param name="aggregateOn">A list of properties on which to specify point uniqueness.</param>
        /// <param name="smoothOn">A dictionary of properties that are smoothable, along with their smoothing values. <b>Must be a subset of aggregateOn.</b></param>
        /// <param name="groupOn">A list of properties on which to group resulting lists (supports arbitrary data, plus 'eventName' and 'deviceID').</param>
        /// <param name="events">A list of events to explicitly include.</param>
        public void Process(CompletionHandler completionHandler,
            List<string> inputFiles, DateTime startDate, DateTime endDate,
            List<string> aggregateOn,
            Dictionary<string, float> smoothOn,
            List<string> groupOn,
            List<string> events = null)
        {
            m_CompletionHandler = completionHandler;

            string outputFileName = System.IO.Path.GetFileName(inputFiles[0]).Replace(".txt", ".json");
            var outputData = new Dictionary<Tuplish, List<Dictionary<string, float>>>();

            m_ReportFiles = 0;
            m_ReportLegalPoints = 0;
            m_ReportRows = 0;
            m_PointDict = new Dictionary<Tuplish, Dictionary<string, float>>();

            foreach (string file in inputFiles)
            {
                m_ReportFiles++;
                LoadStream(outputData, file, startDate, endDate,
                    aggregateOn, smoothOn, groupOn,
                    events, outputFileName);
            }

            // Test if any data was generated
            bool hasData = false;
            var reportList = new List<int>{ };
            foreach (var generated in outputData)
            {
                hasData = generated.Value.Count > 0;
                reportList.Add(generated.Value.Count);
                if (!hasData)
                {
                    break;
                }
            }
            if (hasData)
            {
                var reportArray = reportList.Select(x => x.ToString()).ToArray();

                //Output what happened
                string report = "Report of " + m_ReportFiles + " files:\n";
                report += "Total of " + reportList.Count + " groups numbering [" + string.Join(",", reportArray) + "]\n";
                report += "Total rows: " + m_ReportRows + "\n";
                report += "Total points analyzed: " + m_ReportLegalPoints;
                Debug.Log(report);

                SaveFile(outputFileName, outputData);
            }
            else
            {
                Debug.LogWarning("The aggregation process yielded no results.");
            }
        }

        internal void LoadStream(Dictionary<Tuplish, List<Dictionary<string, float>>> outputData,
            string path, 
            DateTime startDate, DateTime endDate,
            List<string> aggregateOn,
            Dictionary<string, float> smoothOn,
            List<string> groupOn,
            List<string> eventsWhitelist, string outputFileName)
        {
            // Every point contains at least x/y and potentially these others
            var pointProperties = new string[]{ "x", "y", "z", "t", "rx", "ry", "rz", "dx", "dy", "dz" };

            var reader = new StreamReader(path);
            using (reader)
            {
                string tsv = reader.ReadToEnd();
                string[] rows = tsv.Split('\n');
                m_ReportRows += rows.Length;

                for (int a = 0; a < rows.Length; a++)
                {
                    string[] rowData = rows[a].Split('\t');

                    if (string.IsNullOrEmpty(rowData[0]) || string.IsNullOrEmpty(rowData[2]) || string.IsNullOrEmpty(rowData[3]))
                    {
                        // Re-enable this log if you want to see empty lines
                        //Debug.Log ("Empty Line...skipping");
                        continue;
                    }

                    DateTime rowDate = DateTime.Parse(rowData[0]);
                    string deviceID = rowData[1];
                    string eventName = rowData[2];

                    // Pass on rows outside any date trimming
                    if (rowDate < startDate || rowDate > endDate)
                    {
                        continue;
                    }

                    // If we're filtering events, pass if not in list
                    if (eventsWhitelist.Count > 0 && eventsWhitelist.IndexOf(eventName) == -1)
                    {
                        continue;
                    }

                    Dictionary<string, object> datum = MiniJSON.Json.Deserialize(rowData[3]) as Dictionary<string, object>;
                    // If no x/y, this isn't a Heatmap Event. Pass.
                    if (!datum.ContainsKey("x") || !datum.ContainsKey("y"))
                    {
                        // Re-enable this log line if you want to be see events that aren't valid for heatmapping
                        //Debug.Log ("Unable to find x/y in: " + datum.ToString () + ". Skipping...");
                        continue;
                    }

                    // Passed all checks. Consider as legal point
                    m_ReportLegalPoints++;

                    // Construct both the list of elements that signify a unique item...
                    var tupleList = new List<object>{ eventName };
                    // ...and the point that represents that item
                    var point = new Dictionary<string, float>();
                    foreach (var ag in aggregateOn)
                    {
                        float floatValue = 0f;
                        object arbitraryValue = 0f;
                        // Special case for DeviceIDs, which aren't in the JSON
                        if (ag == "deviceID")
                        {
                            arbitraryValue = deviceID;
                        }
                        else if (datum.ContainsKey(ag))
                        {
                            // parse and divide all in smoothing list
                            float.TryParse((string)datum[ag], out floatValue);
                            if (smoothOn.ContainsKey(ag))
                            {
                                floatValue = Divide(floatValue, smoothOn[ag]);
                            }
                            else
                            {
                                floatValue = 0;
                            }
                            arbitraryValue = floatValue;
                        }

                        tupleList.Add(arbitraryValue);
                        if (pointProperties.Contains(ag))
                        {
                            Debug.Log(ag);
                            point[ag] = floatValue;
                        }
                    }

                    // Tuple-like key to determine if this point is unique, or needs to be merged with another
                    var tuple = new Tuplish(tupleList.ToArray());
                    if (m_PointDict.ContainsKey(tuple))
                    {
                        // Use existing point if it exists
                        point = m_PointDict[tuple];
                        // TODO
                        // This is where we need to look to remap density
                        point["d"] = point["d"] + 1;
                    }
                    else
                    {
                        point["d"] = 1;
                        m_PointDict[tuple] = point;

                        // Group
                        var listTupleKeyList = new List<object>();
                        foreach (var field in groupOn)
                        {
                            // Special case for eventName
                            if (field == "eventName")
                            {
                                listTupleKeyList.Add(eventName);
                            }
                            // Special case for deviceID
                            else if (field == "deviceID")
                            {
                                listTupleKeyList.Add("device:" + deviceID);
                            }
                            // Everything else just added to key
                            else if (datum.ContainsKey(field))
                            {
                                listTupleKeyList.Add(field + ":" + datum[field]);
                            }
                        }
                        var listTupleKey = new Tuplish(listTupleKeyList.ToArray());

                        // Create the event list if the key doesn't exist
                        if (!outputData.ContainsKey(listTupleKey))
                        {
                            outputData.Add(listTupleKey, new List<Dictionary<string, float>>());
                        }
                        // Add the new point to the list
                        outputData[listTupleKey].Add(point);
                    }
                }
            }
        }

        internal void SaveFile(string outputFileName, Dictionary<Tuplish, 
            List<Dictionary<string, float>>> outputData)
        {
            string savePath = System.IO.Path.Combine(m_DataPath, "HeatmapData");
            if (!System.IO.Directory.Exists(savePath))
            {
                System.IO.Directory.CreateDirectory(savePath);
            }

            var json = MiniJSON.Json.Serialize(outputData);
            string jsonPath = savePath + Path.DirectorySeparatorChar + outputFileName;
            System.IO.File.WriteAllText(jsonPath, json);

            m_CompletionHandler(jsonPath);
        }

        protected float Divide(float value, float divisor)
        {
            float mod = value % divisor;
            float rounded = Mathf.Round(value / divisor) * divisor;
            if (mod > divisor / 2f)
            {
                rounded -= divisor / 2f;
            }
            else
            {
                rounded += divisor / 2f;
            }
            return rounded;
        }
    }

    // Unity doesn't support Tuple, so here's a Tuple-like standin
    internal class Tuplish
    {

        List<object> objects;

        internal Tuplish(params object[] args)
        {
            objects = new List<object>(args);
        }

        public override bool Equals(object other)
        {
            return objects.SequenceEqual((other as Tuplish).objects);
        }

        public override int GetHashCode()
        {
            int hash = 17;
            foreach (object o in objects)
            {
                hash = hash * 23 + (o == null ? 0 : o.GetHashCode());
            }
            return hash;
        }

        public override string ToString()
        {
            string s = "";
            foreach (var o in objects)
            {
                s += o.ToString() + ":";
            }
            if (s.LastIndexOf(':') == s.Length - 1)
            {
                s = s.Substring(0, s.Length - 1);
            }
            return s;
        }
    }
}
