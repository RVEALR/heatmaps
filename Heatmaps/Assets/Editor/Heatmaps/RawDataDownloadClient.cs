using System;
using System.Net;
using UnityEngine;

namespace UnityAnalyticsHeatmap
{
	public class RawDataDownloadClient : WebClient
	{
		public delegate void CompletionHandler (bool success, string reason = "");
		public CompletionHandler completionHandler;


		public RawDataDownloadClient ()
		{
		}

		public void DownloadFileAsync(string url, string filePath, CompletionHandler handler) {
			this.completionHandler = handler;
			base.DownloadFileAsync (new Uri (url), filePath);
		}

		protected override void OnDownloadFileCompleted (System.ComponentModel.AsyncCompletedEventArgs e)
		{
			base.OnDownloadFileCompleted (e);
			completionHandler (true, "Download complete");
		}
	}
}

