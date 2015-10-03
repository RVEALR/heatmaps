/// <summary>
/// Gets raw events from the Unity Analytics server.
/// </summary>
/// This is a port of get_raw_events.py, because not everyone likes to use
/// python.

using MiniJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR_WIN
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
#endif

namespace UnityAnalyticsHeatmap
{
	public class RawEventClient
	{

		public delegate void CompletionHandler (List<string> fileList);
		private CompletionHandler completionHandler;

		private List<string> urlsToFetch;
		private List<string> filesToFetch;
		private int downloadedCount;

		// 1) Check for data
		// 2) Download what we don't have
		// 3) Aggregate

		public void Fetch(string path, UnityAnalyticsEventType[] events, DateTime startDate, DateTime endDate, CompletionHandler handler) {
			urlsToFetch = new List<string> ();
			filesToFetch = new List<string> ();
			downloadedCount = 0;

			if (startDate > endDate) {
				throw new Exception ("End date must be before start date.");
			}

			completionHandler = handler;

			//Load the Data Export Manifest
			object manifest = FetchData (path);
			List<object> data = manifest as List<object>;

			if (data != null) {

				//We have the manifest, look for batches within the time frame
				int foundItems = 0;

				foreach (Dictionary<string, object> manifestItem in data) {
					if (manifestItem.ContainsKey ("generated_at") && manifestItem.ContainsKey ("url")) {
						DateTime generatedAt = DateTime.Parse (manifestItem ["generated_at"] as string);

						//Ignore if outside date range
						if (generatedAt >= startDate && generatedAt <= endDate) {
							foundItems++;
							Dictionary<string, object> batch = FetchData (manifestItem ["url"] as string) as Dictionary<string, object>;
							List<object> batchData = batch ["data"] as List<object>;
							string batchID = batch ["batchid"] as string;

							if (batchData != null) {
								foreach (Dictionary<string, object> batchItem in batchData) {
									string bUrl = batchItem ["url"] as string;

									//Trim so we d/l only requested event types
									foreach (UnityAnalyticsEventType evt in events) {
										if (bUrl.IndexOf (evt.ToString ()) > -1) {
											urlsToFetch.Add (bUrl);
											filesToFetch.Add (ConstructFileName (batchID, evt.ToString ()));
										}
									}
								}
							}
						}
					}
				}

				if (foundItems == 0) {
					Debug.LogWarning ("No data found within specified dates.");
				} else {
					for (var a = 0; a < filesToFetch.Count; a++) {
						string url = urlsToFetch [a];
						string filePath = filesToFetch [a];
						DownloadFile (url, filePath);
					}
				}
			} else {
				//No internet connection. Return local files
				string savePath = GetSavePath ();
				if (System.IO.Directory.Exists (savePath)) {
					filesToFetch = new List<string> (Directory.GetFiles (savePath, "*.txt"));
				}
				completionHandler (filesToFetch);
			}
		}

		public void PurgeData() {
			string savePath = GetSavePath ();

			if (System.IO.Directory.Exists (savePath)) {
				System.IO.Directory.Delete (savePath, true);
			}
		}

		protected object FetchData(string path) {
			#if UNITY_EDITOR_WIN
			// Bypassing SSL security in Windows to work around a CURL bug.
			// This is insecure and should be fixed when the Engine supports SSL.
			ServicePointManager.ServerCertificateValidationCallback = delegate(System.Object obj, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) {
				return true;
			};
			#endif

			WebRequest www = WebRequest.Create(path);
			try {
				Stream stream = www.GetResponse().GetResponseStream();
				StreamReader reader = new StreamReader(stream);
				using (reader)
				{
					string text = reader.ReadToEnd();
					return Json.Deserialize(text);
				}
			}
			catch(WebException ex) {
				Debug.LogWarning ("No web connection. Will proceed with local data if possible.\n" + ex.ToString());
				return null;
			}
		}

		protected string ConstructFileName (string batchID, string eventType) {
			string savePath = GetSavePath ();
			return savePath + Path.DirectorySeparatorChar + batchID + "_" + eventType + ".txt";

		}

		protected void DownloadFile(string path, string filePath) {
			string savePath = GetSavePath ();

			//Create the save path if necessary
			if (!System.IO.Directory.Exists (savePath)) {
				System.IO.Directory.CreateDirectory (savePath);
			}

			if (File.Exists (filePath)) {
				OnDownload (true, "Already downloaded");
			} else {
				var client = new RawDataDownloadClient ();
				client.DownloadFileAsync (path, filePath, OnDownload);
			}
		}

		private void OnDownload(bool success, string reason = "") {
			downloadedCount++;

			string report = "Downloaded " + downloadedCount + "/" + filesToFetch.Count + ". Note: " + reason;
			Debug.Log (report);

			if (downloadedCount == filesToFetch.Count) {
				completionHandler (filesToFetch);
			}
		}

		private string GetSavePath() {
			string savePath = System.IO.Path.Combine (Application.persistentDataPath, "HeatmapData");
			return savePath;
		}
	}
}