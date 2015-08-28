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
			#if UNITY_EDITOR_WIN
			GUIStyle s = new GUIStyle(GUI.skin.label);
			s.wordWrap = true;
			s.font = EditorStyles.boldFont;
			EditorGUILayout.LabelField ("Warning: this subpanel may fail to work on Windows machines. Use the python script instead.", s);
			GUILayout.Space(10f);
			#endif

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