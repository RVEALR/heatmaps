/// <summary>
/// Representation of an individual line item of custom raw data.
/// </summary>

using System;
using System.Collections.Generic;

namespace RVEALR.Heatmaps
{
    public struct CustomRawDatum
    {
        DateTime timestamp;
        string appId;
        string userId;
        string sessionId;
        string remoteIp;
        string platform;
        string sdkVersion;
        bool isDebugDevice;
        string userAgent;
        DateTime submitTime;
        string name;
        Dictionary<string, object> customParams;
    }
}

