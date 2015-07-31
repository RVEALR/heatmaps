/// <summary>
/// Adapter API for sending HeatMap analytics events
/// </summary>
/// This is <i>simply</i> an adapter. As such, you could choose not to
/// use it at all, but by passing your events through this API you gain type
/// safety and ensure that you're conforming to the data that the aggregator
/// and Heat Mapper expect to receive.
/// 
/// The script is designed to work in Unity 4.6 > 5.x

using System;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_4_6 || UNITY_5_0
using analyticsResultNamespace = UnityEngine.Cloud.Analytics;
using analyticsEventNamespace = UnityEngine.Cloud.Analytics.UnityAnalytics;
#else
using analyticsResultNamespace = UnityEngine.Analytics;
using analyticsEventNamespace = UnityEngine.Analytics.Analytics;
#endif


namespace UnityAnalytics
{
	public class HeatMapEvent
	{
		private static Dictionary<string, object> dict = new Dictionary<string, object> ();

		public static analyticsResultNamespace.AnalyticsResult Send (string eventName, Vector3 v)
		{
			dict ["x"] = v.x;
			dict ["y"] = v.y;
			dict ["z"] = v.z;
			return analyticsEventNamespace.CustomEvent ("HeatMap" + eventName, dict);
		}

		public static analyticsResultNamespace.AnalyticsResult Send (string eventName, Vector2 v)
		{
			dict ["x"] = v.x;
			dict ["y"] = v.y;
			return analyticsEventNamespace.CustomEvent ("HeatMap" + eventName, dict);
		}

		public static analyticsResultNamespace.AnalyticsResult Send (string eventName, Vector3 v, float time)
		{
			dict ["x"] = v.x;
			dict ["y"] = v.y;
			dict ["z"] = v.z;
			dict ["t"] = time;
			return analyticsEventNamespace.CustomEvent ("HeatMap" + eventName, dict);
		}

		public static analyticsResultNamespace.AnalyticsResult Send (string eventName, Vector2 v, float time)
		{
			dict ["x"] = v.x;
			dict ["y"] = v.y;
			dict ["t"] = time;
			return analyticsEventNamespace.CustomEvent ("HeatMap" + eventName, dict);
		}
	}
}

