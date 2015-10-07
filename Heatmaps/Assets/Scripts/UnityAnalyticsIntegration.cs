
using System.Collections;
using UnityEngine;

#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 ||  UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0
using UnityEngine.Cloud.Analytics;

#else
using UnityEngine.Analytics;
#endif

public class UnityAnalyticsIntegration : MonoBehaviour
{

    #if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 ||  UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0
    void Start () {
        const string projectId = "Place your project ID here";
        UnityEngine.Cloud.Analytics.UnityAnalytics.StartSDK (projectId);
    }
    #endif
}
