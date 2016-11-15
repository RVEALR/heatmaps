using System;
using System.Collections.Generic;
using UnityEngine;

public class EventWriter
{
    /// <summary>
    /// Stringify an event for output as TSV
    /// </summary>
    /// <returns>The event.</returns>
    /// <param name="eventName">Event name.</param>
    /// <param name="parameters">A dictionary of parameters.</param>
    /// <param name="deviceId">A theoretically unique device identifier.</param>
    /// <param name="sessionId">A theoretically unique session identifier.</param>
    /// <param name="platform">Platform.</param>
    /// <param name="isDebug">If set to <c>true</c> is a debug device.</param>
    public static string WriteEvent(string eventName, Dictionary<string, object> parameters, string deviceId, string sessionId, string platform, double currentMilliseconds, bool isDebug = false)
    {
        string evt = "";
        evt += currentMilliseconds + "\t";

        // AppID
        #if UNITY_5
        evt += (string.IsNullOrEmpty(Application.cloudProjectId)) ? "1234-abcd-5678-efgh" : Application.cloudProjectId;
        #else
        evt += "1234-abcd-5678-efgh";
        #endif
        evt += "\t";

        // Event Type
        evt += "custom\t";

        // User ID, Session ID
        evt += deviceId + "\t";
        evt += sessionId + "\t";

        // Remote IP
        evt += "1.1.1.1\t";

        // Platform
        evt += platform + "\t";

        // SDK Version
        evt += "5.3.4\t";

        // IsDebug
        evt += isDebug + "\t";

        // User agent
        evt += "Corridor%20Z/3 CFNetwork/758.2.8 Darwin/15.0.0\t";

        // Submit time
        evt += currentMilliseconds + "\t";

        // Event Name
        evt += eventName + "\t";

        evt += WriteParams(eventName, parameters);

        return evt;
    }

    static string WriteParams(string eventName, Dictionary<string, object> parameters)
    {
        string json = "{";

        foreach(KeyValuePair<string, object> kv in parameters)
        {
            json += Quotify(kv.Key) + ":" + Quotify(kv.Value.ToString());
            json += ",";
        }
        json += Quotify("unity.name") + ":" + Quotify(eventName) + "}";
        return json;
    }

    static string Quotify(string value)
    {
        return "\"" + value + "\"";
    }
}

