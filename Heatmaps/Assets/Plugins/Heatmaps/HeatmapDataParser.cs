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

		protected void LoadStream(string path) {
			StreamReader reader = new StreamReader(path);
			using (reader)
			{
				ConsumeHeatmapData (reader.ReadToEnd());
			}
		}

		protected void LoadResource(string path) {
			TextAsset ta = Resources.Load (path) as TextAsset;
			ConsumeHeatmapData (ta.text);
		}

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

