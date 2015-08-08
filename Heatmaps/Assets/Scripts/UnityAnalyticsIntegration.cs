
using UnityEngine;
using System.Collections;

#if UNITY_5_0
using UnityEngine.Cloud.Analytics;
#elif UNITY_5_1 || UNITY_5_2
using UnityEngine.Analytics;
#endif

public class UnityAnalyticsIntegration : MonoBehaviour {


	void Start () {
		#if UNITY_5_0
		const string projectId = "5ac1db07-fa32-4355-9d15-3089fb0b1d0f";
		UnityEngine.Cloud.Analytics.UnityAnalytics.StartSDK (projectId);
		#endif
		StartCoroutine (ForceEvents ());
	}

	IEnumerator ForceEvents() {
		yield return new WaitForSeconds(30f);

		#if UNITY_5_0
		UnityEngine.Cloud.Analytics.UnityAnalytics.Transaction("12345abcde", 0.99m, "USD", null, null);
		#elif UNITY_5_1 || UNITY_5_2
		Analytics.Transaction("12345abcde", 0.99m, "USD", null, null);
		#endif
		StartCoroutine (ForceEvents ());
	}

}

