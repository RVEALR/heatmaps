/// <summary>
/// Gets raw events from the Unity Analytics server.
/// </summary>
/// This is a port of get_raw_events.py, because not everyone likes to use
/// python.

using System;
using UnityEngine;
using System.Collections;
using System.Net;
using System.IO;
using MiniJSON;
using System.Collections.Generic;

namespace UnityAnalytics
{
	public class RawEventClient
	{
		string versionNum = "0.0.1";

		public string version {
			get {
				return versionNum;
			}
		}

		public void Fetch(string path, UnityAnalyticsEventType[] events, DateTime startDate, DateTime endDate) {
			if (startDate > endDate) {
				throw new Exception ("End date must be before start date.");
			}

			//Load the Data Export Manifest
			object manifest = FetchData (path);
			List<object> data = manifest as List<object>;

			if (data != null) {

				//We have the manifest, look for batches within the time frame
				int foundItems = 0;

				foreach (Dictionary<string, object> manifestItem in data) {
					if (manifestItem.ContainsKey ("generated_at") && manifestItem.ContainsKey("url")) {
						DateTime generatedAt = DateTime.Parse (manifestItem ["generated_at"] as string);

						//Ignore if outside date range
						if (generatedAt >= startDate && generatedAt <= endDate) {
							foundItems++;
							Dictionary<string, object> batch = FetchData (manifestItem ["url"] as string) as Dictionary<string, object>;
							List<object> batchData = batch["data"] as List<object>;
							string batchID = batch ["batchid"] as string;

							if (batchData != null) {
								foreach (Dictionary<string, object> batchItem in batchData) {
									string bUrl = batchItem ["url"] as string;

									//Trim so we d/l only requested event types
									foreach (UnityAnalyticsEventType evt in events) {
										if (bUrl.IndexOf(evt.ToString()) > -1) {
											DownloadFile (bUrl, batchID, evt.ToString());
										}
									}
								}
							}
						}
					}
				}

				if (foundItems == 0) {
					Debug.LogWarning ("No data found within specified dates.");
				}
			}
		}

		protected object FetchData(string path) {
			// Handle any problems that might arise when reading the text
			try
			{
				WebRequest www = WebRequest.Create(path);
				Stream stream = www.GetResponse().GetResponseStream();
				StreamReader reader = new StreamReader(stream);

				using (reader)
				{
					string text = reader.ReadToEnd();
					return Json.Deserialize(text);
				}
			}
			// If anything broke in the try block, we throw an exception with information
			// on what didn't work
			catch (Exception e)
			{
				Debug.Log(e.Message);
			}
			return null;
		}

		protected void DownloadFile(string path, string batchID, string eventType) {
			var client = new WebClient ();

			string savePath = System.IO.Path.Combine (Application.dataPath, "HeatmapData");
			if (!System.IO.Directory.Exists (savePath)) {
				System.IO.Directory.CreateDirectory (savePath);
			}

			string fileName = batchID + "_" + eventType + ".txt";
			client.DownloadFile(path, savePath + Path.DirectorySeparatorChar + fileName);
			Debug.Log ("Downloaded " + fileName);
		}
	}
}