/// <summary>
/// The portion of the Heatmapper that controls downloading data from the Unity Analytics Raw Event API.
/// </summary>

using System;
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace UnityAnalyticsHeatmap
{
	
	public class RawEventInspector
	{
		private const string URL_KEY = "UnityAnalyticsHeatmapDataExportUrlKey";

		private string path = "";
		private string startDate = "";
		private string endDate = "";

		private RawEventClient client = new RawEventClient ();

		public RawEventInspector() {
			path = EditorPrefs.GetString (URL_KEY);
			endDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now);
			startDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.Subtract(new TimeSpan(5, 0, 0, 0)));
		}

		public static RawEventInspector Init()
		{
			return new RawEventInspector ();
		}

		public void OnGUI()
		{
			path = EditorGUILayout.TextField (new GUIContent("Data Export URL", "Copy the URL from the 'Editing Project' page of your project dashboard"), path);

			startDate = EditorGUILayout.TextField (new GUIContent("Start Date (YYYY-MM-DD)",  "Start date as ISO-8601 datetime"), startDate);
			endDate = EditorGUILayout.TextField (new GUIContent("End Date (YYYY-MM-DD)",  "End date as ISO-8601 datetime"), endDate);

			if (GUILayout.Button (new GUIContent("Download", "Download raw data in the specified range")) && !string.IsNullOrEmpty(path)) {
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