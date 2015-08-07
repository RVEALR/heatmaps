using System;
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace UnityAnalytics
{
	
	public class RawEventInspector
	{
		private const string URL_KEY = "UnityAnalyticsHeatmapDataExportUrlKey";

		public delegate void DataHandler (string[] paths);


		private DataHandler handler;

		private string path = "";
		private string startDate = "";
		private string endDate = "";



		private RawEventClient client = new RawEventClient ();


		public RawEventInspector(DataHandler handler) {
			path = EditorPrefs.GetString (URL_KEY);
			endDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now);
			startDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.Subtract(new TimeSpan(5, 0, 0, 0)));
			this.handler = handler;
		}

		public static RawEventInspector Init(DataHandler handler)
		{
			return new RawEventInspector (handler);
		}

		public void OnGUI()
		{
			path = EditorGUILayout.TextField ("Data Export URL", path);

			startDate = EditorGUILayout.TextField ("Start Date (YYYY-MM-DD)", startDate);
			endDate = EditorGUILayout.TextField ("End Date (YYYY-MM-DD)", endDate);

			if (GUILayout.Button ("Download")) {
				EditorPrefs.SetString (URL_KEY, path);
				DateTime start, end;
				try {
					start = DateTime.Parse(startDate);
				}
				catch {
					throw new Exception("The start date is not properly formatted. Correct format is YYYY-MM-DD.");
				}
				try {
					end = DateTime.Parse(endDate);
				}
				catch {
					throw new Exception("The end date is not properly formatted. Correct format is YYYY-MM-DD.");
				}

				client.Fetch (path, new UnityAnalyticsEventType[]{UnityAnalyticsEventType.custom}, start, end);
			}
		}
	}
}