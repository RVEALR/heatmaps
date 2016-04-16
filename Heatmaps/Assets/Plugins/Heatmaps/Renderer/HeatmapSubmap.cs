using System;
using UnityEngine;
using System.Collections.Generic;
using UnityAnalyticsHeatmap;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class HeatmapSubmap : MonoBehaviour
{

    public List<HeatPoint> m_PointData;

    void Start()
    {
        GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        GetComponent<MeshRenderer>().receiveShadows = false;
        GetComponent<MeshRenderer>().useLightProbes = false;
        GetComponent<MeshRenderer>().reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
    }
}
