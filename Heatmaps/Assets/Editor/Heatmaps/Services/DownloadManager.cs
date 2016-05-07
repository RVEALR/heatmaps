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
/// 
/// RAW DATA API
/// export_job (POST)
/// Requests a new export job. The job may take seconds or minutes to process.
///     params:
///         key: the secret key for the project.
///         startDate: an ISO-8601 datetime representing the start date of desired data.
///         endDate: an ISO-8601 datetime representing the end date of desired data.
///                  If omitted, job will generate through to the present.
///         previousJobId: if provided, replaces startDate, using the endDate of the previous job.
///         dataSet: A type of event to download:
///             - appRunning: sent periodically as the game runs
///             - appStart: sent once each time the game starts
///             - custom: your custom events, including heatmap events, are part of these
///             - deviceInfo: sent once, detailing the type of device on which the game is running
///             - transaction: sent whenever an in-app purchase is recorded
///             - userInfo: used with attribution info
///         format: Supported return formats. Currently includes:
///             - json
///             - tsv
///     returns:
///         A JSON string reporting the jobId of the created job, or an error.
///         {
///             "error":"any error content",
///             "jobId":"An Id to be used to check status and download the result"
///         }
///     example:
///         https://analytics.cloud.unity3d.com/api/v2/exportJob
///         POST with params:
///             appId: "your-app-id"
///             key: "your-secret-key"
///             startDate: 2017-01-31
///             endDate: 2017-02-05
///             dataSet: "custom"
///             format: "tsv"
///
/// export_job (GET)
/// Checks on the status of a job.
///     params:
///         appId: The application Id.
///         jobId: The unique identifier for this job.
///     returns:
///         A JSON string reporting the job status.
///         {
///             "error":"any error content",
///             "jobId":"The provided jobId",
///             "status":"created,submitted,finished,error"
///             "data":"A download URL for the completed job"
///         }
///     example:
///         https://analytics.cloud.unity3d.com/api/v2/exportJob?appId=your-app-id&jobId=your-job-id
///
/// download (GET)
/// Fetches the result data from a job
///     params:
///         jobId: The unique identifier for this job.
///         secureHash: A key to the job sent from the server via export_job (GET).
///     returns:
///         A JSON string listing all known historical jobs.
///         {
///             "all_jobs": [
///                {
///                     "status": "finished",
///                     "jobId": "unique-job-id",
///                     "dataSet": "custom",
///                     "dateRange": "04/01/2016 - 04/02/2016",
///                     "created": "04/30/2016 18:32:56",
///                     "jobDuration": "00:00:22",
///                     "jobLink": "link-to-data-download",
///                     "downloadFormat": "tsv",
///                     "secureHash": "the-secure-hash"
///                },
///                ...
///             ],
///             "data_sets": [
///                 "deviceInfo",
///                 "custom",
///                 ...
///             ]
///         }
///     example:
///         https://analytics.cloud.unity3d.com/api/v2/download/your-job-id/secure-hash
/// 
/// 
/// export_jobs (GET)
/// Returns the historical list of jobs for this app.
///     params
///         appId: The application Id.
///     example:
///         https://analytics.cloud.unity3d.com/api/v2/exportJobs?appId=your-app-id


using System;
using System.Collections.Generic;
using System.Net;
using System.Collections.Specialized;
using System.Text;
using UnityEngine;

namespace UnityAnalytics
{
    interface IDownloadManager
    {
        string m_AppId{get;set;}
        string m_DataKey{get;set;}
        JobRequest CreateJob(DateTime startDate, DateTime endDate);
        JobRequest CreateJob(DateTime startDate);
        JobRequest CreateJob(JobRequest previousJob);
        JobRequest CreateJob(JobRequest previousJob, DateTime endDate);
        List<JobRequest> GetJobs();
        void Download(JobRequest job);
    }

    public class DownloadManager : IDownloadManager
    {
        private const string BasePath = "http://localhost:3000/api/";
        public const string CreateJobPath = BasePath + "exportJob";
        public const string JobStatusPath = BasePath + "exportJob";
        public const string GetJobsPath = BasePath + "exportJobs";
        public const string DownloadPath = BasePath + "download";


        protected string _appId;
        public string m_AppId
        {
            get
            {
                return _appId;
            }
            set
            {
                _appId = value;
            }
        }
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

        public DownloadManager()
        {
            m_Requests = new List<JobRequest>();
        }

        public JobRequest CreateJob(JobRequest priorJob)
        {
            
            return CreateJob(priorJob, DateTime.UtcNow);
        }

        public JobRequest CreateJob(DateTime startDate)
        {
            return CreateJob(startDate, DateTime.UtcNow);
        }

        public JobRequest CreateJob(JobRequest priorJob, DateTime endDate)
        {
            JobRequest job = new JobRequest();
            job.m_PreviousRequest = priorJob;
            job.m_StartDate = priorJob.m_EndDate;
            job.m_EndDate   = endDate;
            job.m_DataKey = m_DataKey;
            job.m_AppId = m_AppId;
            job.Create();
            m_Requests.Add(job);
            return job;
        }

        public JobRequest CreateJob(DateTime startDate, DateTime endDate)
        {
            JobRequest job = new JobRequest();
            job.m_StartDate = startDate;
            job.m_EndDate   = endDate;
            job.m_DataKey = m_DataKey;
            job.m_AppId = m_AppId;
            job.Create();
            m_Requests.Add(job);
            return job;
        }

        public List<JobRequest> GetJobs()
        {
            WebClient client = new WebClient();
            Authorization(client);
            //client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            string responsebody =  client.DownloadString(DownloadManager.GetJobsPath + "/" + m_AppId);
            //string responsebody = Encoding.UTF8.GetString(responsebytes);


            Debug.Log(responsebody);

            var report = MiniJSON.Json.Deserialize(responsebody) as Dictionary<string, object>;


            List<JobRequest> list = new List<JobRequest>();
            if (report.ContainsKey("all_jobs"))
            {
                List<object> items;
                items = report["all_jobs"] as List<object>;
                foreach(Dictionary<string, object> item in items)
                {
                    JobRequest job = new JobRequest();
                    job.m_AppId = m_AppId;

                    object created = null;
                    item.TryGetValue("created", out created);
                    job.m_Created = created == null ? "unknown" : created.ToString();

                    object dateRange = null;
                    item.TryGetValue("dateRange", out dateRange);
                    job.m_DateRange = dateRange == null ? "unknown" : dateRange.ToString();

                    object status = null;
                    item.TryGetValue("status", out status);
                    job.m_Status = (JobRequestStatus)Enum.Parse(typeof(JobRequestStatus), status.ToString());
                    list.Add(job);
                }
            }
            return list;
        }
        public void Download(JobRequest job)
        {
        }

        protected void Authorization(WebClient client)
        {
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(m_AppId + ":" + m_DataKey));
            client.Headers[HttpRequestHeader.Authorization] = string.Format("Basic ", credentials);
        }
    }

    /// <summary>
    /// Represents a request to RDE for a new Job
    /// </summary>
    public class JobRequest
    {
        // Request variables
        public JobRequest m_PreviousRequest;   // Note: null if m_StartDate is set.
        public DateTime m_StartDate;           // Note: null if m_PreviousRequest is set.
        public DateTime m_EndDate;             // Note: null if open-ended query.
        public string m_AppId;
        public string m_DataKey;
        public UnityAnalyticsEventType m_DataSet;

        // Result variables
        public JobRequestStatus m_Status;
        public string m_ErrorMessage = "";
        public string m_JobId;
        public string m_JobLink;
        public string m_SecureHash;
        public string m_Format = "tsv";

        // Useful for debugging and UI
        public string m_Created;
        public string m_DateRange;
        public string m_Duration;

        WebClient client = new WebClient();
        bool m_RequestPending = false;

        public delegate void CompletionHandler(bool success, string reason = "");
        public CompletionHandler downloadCompletionHandler;

        public JobRequest Create()
        {
            if (!m_RequestPending && !string.IsNullOrEmpty(m_AppId))
            {
                DateTime useStartDate = (m_PreviousRequest != null) ? m_PreviousRequest.m_EndDate : m_StartDate;
                string start = useStartDate.ToString("yyyy-MM-dd");
                string end = m_EndDate.ToString("yyyy-MM-dd");

                var payload = new NameValueCollection();
                payload.Add("appId", m_AppId);
                payload.Add("key", m_DataKey);
                payload.Add("startDate", start);
                payload.Add("endDate", end);
                payload.Add("format", "tsv");
                payload.Add("dataset", "custom");

                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                var responsebytes =  client.UploadValues(DownloadManager.CreateJobPath, "POST", payload);
                string responsebody = Encoding.UTF8.GetString(responsebytes);
                var report = MiniJSON.Json.Deserialize(responsebody) as Dictionary<string, string>;

                if (report.ContainsKey("error"))
                {
                    //Do error stuff
                    m_Status = JobRequestStatus.error;
                    m_ErrorMessage = report["error"];
                }
                else if (report.ContainsKey("jobId"))
                {
                    m_JobId = report["jobId"];
                    m_Status = JobRequestStatus.created;
                }
                return this;
            }
            return null;
        }

        public JobRequest GetStatus()
        {
            if (!m_RequestPending && !string.IsNullOrEmpty(m_AppId) && !string.IsNullOrEmpty(m_JobId))
            {
                string data = "appId = " + m_AppId + " jobId = " + m_JobId;
                var result = client.UploadString(DownloadManager.JobStatusPath, "GET", data);
                var report = MiniJSON.Json.Deserialize(result) as Dictionary<string, string>;

                if (report.ContainsKey("error"))
                {
                    //Do error stuff
                    m_Status = JobRequestStatus.error;
                    m_ErrorMessage = report["error"];
                }
                else if (report.ContainsKey("status"))
                {
                    m_Status = (JobRequestStatus)Enum.Parse(typeof(JobRequestStatus), report["status"]);
                    if (m_Status == JobRequestStatus.finished && report.ContainsKey("data"))
                    {
                        m_JobLink = report["data"];
                    }
                }
                return this;
            }
            return null;
        }

        public void Download(string url, string filePath, CompletionHandler handler)
        {
            if (!m_RequestPending)
            {
                m_RequestPending = true;
                this.downloadCompletionHandler = handler;
                client.DownloadFileAsync(new Uri(url), filePath);
            }
        }

        protected void OnDownloadFileCompleted(System.ComponentModel.AsyncCompletedEventArgs e)
        {
            //            base.OnDownloadFileCompleted(e);
            //            downloadCompletionHandler(true, "Download complete");
        }
    }

    /// <summary>
    /// JobRequest status.
    /// </summary>
    /// Mirrors the statuses returned from the server, adding 'Local'
    /// which indicates that the data was found to be local on the user's drive.
    public enum JobRequestStatus
    {
        created,
        submitted,
        finished,
        error,
        local
    }
}