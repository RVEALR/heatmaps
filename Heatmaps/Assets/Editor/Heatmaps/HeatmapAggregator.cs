using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using System.Globalization;

namespace UnityAnalyticsHeatmap
{
	public class HeatmapAggregator
	{

		public HeatmapAggregator ()
		{
		}

		public void Process(List<string> inputFiles, DateTime startDate, DateTime endDate, 
							float space, float time, bool disaggregateTime, 
							List<string> events)
		{
			string outputFileName = System.IO.Path.GetFileName (inputFiles [0]).Replace(".txt", ".json");
			foreach (string file in inputFiles) {
				LoadStream (file, startDate, endDate, space, time, disaggregateTime, events, outputFileName);
			}
		}

		protected void LoadStream(string path, 
									DateTime startDate, DateTime endDate, 
									float space, float time, bool disaggregateTime, 
									List<string> events, string outputFileName)
		{
			Dictionary<Tuplish, Dictionary<string, float>> pointDict = new Dictionary<Tuplish, Dictionary<string, float>>();
			Dictionary<string, List<Dictionary<string, float>>> outputData = new Dictionary<string, List<Dictionary<string, float>>> ();

			StreamReader reader = new StreamReader(path);
			using (reader)
			{
				string tsv = reader.ReadToEnd();
				string[] rows = tsv.Split('\n');

				for (int a = 0; a < rows.Length; a++) {
					string[] rowData = rows [a].Split ('\t');

					if (string.IsNullOrEmpty(rowData [0]) || string.IsNullOrEmpty(rowData [2]) || string.IsNullOrEmpty(rowData [3])) {
						Debug.Log ("Empty Line...skipping");
						continue;
					}

					DateTime rowDate = DateTime.Parse (rowData [0]);

					// Pass on rows outside any date trimming
					if (rowDate < startDate || rowDate > endDate) {
						continue;
					}

					string eventName = rowData [2];
					Dictionary<string, object> datum = MiniJSON.Json.Deserialize (rowData [3]) as Dictionary<string, object>;
					
					// If we're filtering events, pass if not in list
					if (events.Count > 0 && events.IndexOf (eventName) == -1) {
						continue;
					}

					// If no x/y, this isn't a Heatmap Event. Pass.
					if (!datum.ContainsKey ("x") || !datum.ContainsKey ("y")) {
						Debug.Log ("Unable to find x/y in: " + datum.ToString () + ". Skipping...");
						continue;
					}

					float x = float.Parse ((string)datum ["x"]);
					float y = float.Parse ((string)datum ["y"]);
						
					// z is optional
					float z = datum.ContainsKey ("z") ? float.Parse ((string)datum ["z"]) : 0;

					// Round
					x = Divide (x, space);
					y = Divide (y, space);
					z = Divide (z, space);

					// t is optional and always 0 if we're not disaggregating
					float t = !datum.ContainsKey ("t") || !disaggregateTime ? 0 : float.Parse ((string)datum ["t"]);
					t = Divide (t, time);

					// Tuple-like key to determine if this point is unique, or needs to be merged with another
					Tuplish tuple = new Tuplish (new object[]{eventName, x, y, z, t});

					Dictionary<string, float> point;
					if (pointDict.ContainsKey (tuple)) {
						// Use existing point if it exists
						point = pointDict [tuple];
						point ["d"] = point ["d"] + 1;
					} else {
						// Create point if it doesn't exist
						point = new Dictionary<string, float> ();
						point ["x"] = x;
						point ["y"] = y;
						point ["z"] = z;
						point ["t"] = t;
						point ["d"] = 1;
						pointDict [tuple] = point;

						// Create the event list if it doesn't exist
						if (!outputData.ContainsKey(eventName)) {
							outputData.Add (eventName, new List<Dictionary<string, float>> ());
						}
						// Add the new point to the list
						outputData [eventName].Add (point);
					}
				}
			}


			// Test if any data was generated
			bool hasData = false;
			List<int> reportList = new List<int>{};
			foreach (var generated in outputData) {
				hasData = generated.Value.Count > 0;
				reportList.Add (generated.Value.Count);
				if (!hasData) {
					break;
				}
			}
			if (hasData) {
				var report = reportList.Select(x => x.ToString()).ToArray();
				Debug.Log ("The aggregation process yielded " + reportList.Count + " groups with the following point counts [" + string.Join(",", report) + "]");
				SaveFile (outputFileName, outputData);
			} else {
				Debug.LogWarning ("The aggregation process yielded no results.");
			}
		}



		protected void SaveFile(string outputFileName, Dictionary<string, List<Dictionary<string, float>>> outputData) {
			string savePath = System.IO.Path.Combine (Application.dataPath, "HeatmapData");
			if (!System.IO.Directory.Exists (savePath)) {
				System.IO.Directory.CreateDirectory (savePath);
			}

			var json = MiniJSON.Json.Serialize (outputData);
			System.IO.File.WriteAllText (savePath + Path.DirectorySeparatorChar + outputFileName, json);
		}

		protected float Divide(float value, float divisor) {
			float mod = value % divisor;
			float rounded = Mathf.Round(value/divisor) * divisor;
			if (mod > divisor/2) {
				rounded -= divisor / 2;
			} else {
				rounded += divisor / 2;
			}
			return rounded;
		}
	}

	// Unity doesn't support Tuple, so here's a Tuple-like standin
	internal class Tuplish : IEquatable<Tuplish>
	{

		private List<object> objects;

		internal Tuplish(params object[] args) {
			this.objects = new List<object> (args);
		}

		#region IEquatable implementation

		public bool Equals (Tuplish other)
		{
			return objects.SequenceEqual (other.objects);
		}

		#endregion

		public override int GetHashCode()
		{
			int hash = 17;
			foreach(object o in this.objects){
				hash = hash * 23 + (o == null ? 0 : o.GetHashCode ());
			}
			return hash;
		}
	}
}

