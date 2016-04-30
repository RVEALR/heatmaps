/// <summary>
/// The suite of classes serving as the Raw Data Export client
/// </summary>
/// 
/// Raw Data Export (RDE) allows near-realtime fetching of raw data from the Analytics server,
/// but the API is more complicated than the 'Takeout' system that it replaces. Here's how it
/// works.
/// 
/// RDE instantiates 'Jobs' that roll up raw data into files based on an app key and a date range.
/// Calling DownloadManager.GetData() with an appropriate date range returns a JobRequest
/// with an appropriate m_JobId that uniquely identifies the Job. But this does NOT mean
/// that you have data. The server asynchronously collates the requested data, a process
/// which may take seconds to minutes to complete.
/// 
/// The DownloadManager polls the server, asking if the Job is complete. Once it is, the
/// manager's CompletionHandler fires, providing the requester with the desired data.
/// 
/// This process is human-slow (as mentioned, it could take minutes). So it is suggested
/// that a human-intervention step be inserted when the data is "baked", rather than
/// automatically pushing the data into an end-use service like heatmaps. In other words,
/// once baked, the kitchen timer should go "ding!"


using System;
using System.Collections.Generic;
using System.Net;

namespace UnityAnalytics
{
    interface IDownloadManager
    {
        string m_DataKey{get;set;}
        JobRequest GetData(DateTime startDate, DateTime endDate);
        JobRequest GetData(DateTime startDate);
        JobRequest GetData(JobRequest since);
        JobRequest GetData(JobRequest since, DateTime endDate);
    }

    /// <summary>
    /// Represents a request to RDE for a new Job
    /// </summary>
    class JobRequest : WebClient
    {
        public JobRequest m_PreviousRequest;   // Note: null if m_StartDate is set.
        public DateTime m_StartDate;           // Note: null if m_PreviousRequest is set.
        public DateTime m_EndDate;             // Note: null if open-ended query.

        static DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <returns>The data.</returns>
        string Fetch(string uri, CompletionHandler handler)
        {
            downloadCompletionHandler = handler;
            double start  = (m_StartDate - epoch).TotalSeconds;
            double end  = (m_EndDate - epoch).TotalSeconds;
            string data = "start = " + start + " end = " + end;
            return this.UploadString(uri, data);
        }

        public delegate void CompletionHandler(bool success, string reason = "");
        public CompletionHandler downloadCompletionHandler;

        public void Download(string url, string filePath, CompletionHandler handler)
        {
            this.downloadCompletionHandler = handler;
            base.DownloadFileAsync(new Uri(url), filePath);
        }

        protected override void OnDownloadFileCompleted(System.ComponentModel.AsyncCompletedEventArgs e)
        {
            base.OnDownloadFileCompleted(e);
            downloadCompletionHandler(true, "Download complete");
        }
    }

    /// <summary>
    /// Represents a downloaded Job from RDE
    /// </summary>
    public class RawDataJob
    {
        public string m_PreviousJobId;    // Note: null if m_StartDate is set.
        public DateTime m_StartDate;      // Note: null if m_PreviousJobId is set.
        public DateTime m_EndDate;
        public string m_JobId;
        public string m_Filename;         // Note: null if data has not been downloaded yet.
    }

    class DownloadManager : IDownloadManager
    {
        protected string _key;
        public string m_DataKey
        {
            get
            {
                return _key;
            }
            set
            {
                _key = value;
            }
        }

        public delegate void CompletionHandler(bool success, string reason = "");
        CompletionHandler downloadCompletionHandler;


        protected List<JobRequest> m_Requests;
        protected List<RawDataJob> m_Data;


        public DownloadManager()
        {
            m_Requests = new List<JobRequest>();
            m_Data = new List<RawDataJob>();
        }

        public JobRequest GetData(JobRequest priorJob)
        {
            
            return GetData(priorJob, DateTime.UtcNow);
        }

        public JobRequest GetData(DateTime startDate)
        {
            return GetData(startDate, DateTime.UtcNow);
        }

        public JobRequest GetData(JobRequest priorJob, DateTime endDate)
        {
            JobRequest job = new JobRequest();
            job.m_PreviousRequest = priorJob;
            job.m_StartDate = priorJob.m_EndDate;
            job.m_EndDate   = endDate;
            m_Requests.Add(job);
            return job;
        }

        public JobRequest GetData(DateTime startDate, DateTime endDate)
        {
            JobRequest job = new JobRequest();
            job.m_StartDate = startDate;
            job.m_EndDate   = endDate;
            m_Requests.Add(job);
            return job;
        }
    }
}