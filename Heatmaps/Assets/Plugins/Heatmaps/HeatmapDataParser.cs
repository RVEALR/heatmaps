﻿/// <summary>
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
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using MiniJSON;

namespace UnityAnalyticsHeatmap
{
	public class HeatmapDataParser
	{
		public delegate void ParseHandler (Dictionary<string, HeatPoint[]> heatData, float maxDensity, float maxTime, string[] options);
		private ParseHandler handler;

		public HeatmapDataParser ()
		{
		}

		/// <summary>
		/// Loads the data.
		/// </summary>
		/// <param name="path">A location from which to load the data.</param>
		/// <param name="handler">A method handler to which we return the data.</param>
		/// <param name="asResource">If set to <c>true</c> the path is assumed to be a Resource location rather than a URI.</param>
		public void LoadData(string path, ParseHandler handler, bool asResource=false) {
			this.handler = handler;
			if (!string.IsNullOrEmpty (path)) {
				if (asResource) {
					LoadResource (path);
				} else {
					LoadStream (path);
				}
			}
		}

		/// <summary>
		/// Load data from a URI
		/// </summary>
		/// <param name="path">A location from which to load the data.</param>
		protected void LoadStream(string path) {
			StreamReader reader = new StreamReader(path);
			using (reader)
			{
				ConsumeHeatmapData (reader.ReadToEnd());
			}
		}

		/// <summary>
		/// Load data from a Resource location (suitable for runtime use)
		/// </summary>
		/// <param name="path">A location from which to load the data.</param>
		protected void LoadResource(string path) {
			TextAsset ta = Resources.Load (path) as TextAsset;
			ConsumeHeatmapData (ta.text);
		}

		/// <summary>
		/// Read the JSON data and convert into Lists of HeatPoint structs.
		/// </summary>
		/// <param name="text">The loaded data.</param>
		protected void ConsumeHeatmapData(string text) 
		{
			Dictionary<string, HeatPoint[]> heatData = new Dictionary<string, HeatPoint[]> ();
			ArrayList keys = new ArrayList();
			float maxDensity = 0;
			float maxTime = 0;

			Dictionary<string, object> data = Json.Deserialize(text) as Dictionary<string, object>;
			foreach(KeyValuePair<string, object> kv in data)
			{
				keys.Add(kv.Key);

				var pointList = kv.Value as List<object>;
				HeatPoint[] array = new HeatPoint[pointList.Count];
				for (int a = 0, aa = pointList.Count; a < aa; a++)
				{
					array[a] = new HeatPoint();
					float x = 0, y = 0, z = 0, t = 0;
					float d = 0;
					Dictionary<string, object> pt = pointList [a] as Dictionary<string, object>;

					foreach (KeyValuePair<string,object> pointKV in pt) {
						var value = (float)Convert.ToDouble (pointKV.Value);
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
						}
					}
					array[a].position = new Vector3(x,y,z);
					array[a].density = d;
					array [a].time = t;
					maxDensity = Mathf.Max (d, maxDensity);
					maxTime = Mathf.Max (array[a].time, maxTime);
				}
				heatData[kv.Key] = array;
			}

			if (handler != null) {
				handler (heatData, maxDensity, maxTime, keys.ToArray (typeof(string)) as string[]);
			}
		}
	}
}

