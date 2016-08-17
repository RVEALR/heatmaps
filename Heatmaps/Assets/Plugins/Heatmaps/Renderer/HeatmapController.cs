﻿/// <summary>
/// Heat map controller.
/// </summary>
/// This is an exceedingly simple example of a runtime controller
/// for the HeatmapMeshRenderer. It's not exceptionally flexible,
/// but it teaches you everything you need to know if you want to
/// use the HeatmapMeshRenderer as a runtime component. To use:
/// 
/// 1. If you don’t have a Resources folder, create one.
/// 2. In the Resources folder, place a JSON file generated by aggregation.
/// 3. In your game, create an empty GameObject. Name it ‘MyRuntimeHeatmap’.
/// 4. Add the HeatmapController MonoBehaviour to ‘MyRuntimeHeatmap’
/// 5. Look at the Inspector for ‘MyRuntimeHeatmap’. Under Heat Map Controller, find the Data Path field.
/// 6. Type in the name of your JSON file.
/// 7. Hit Play! 


using System;
using System.Collections.Generic;
using UnityAnalyticsHeatmap;
using UnityEngine;


[RequireComponent(typeof(HeatmapMeshRenderer))]
public class HeatmapController : MonoBehaviour
{
    public string dataPath = "";
    public string[] options;
    public int optionIndex;
    private int oldOptionIndex = -1;

    public float pointSize = 10;
    private float oldPointSize = -99;

    HeatmapDataParser m_DataParser = new HeatmapDataParser();
    Dictionary<string, HeatPoint[]> m_Data;

    private  Gradient gradient;

    float m_MaxDensity = 0;
    float m_MaxTime = 0;
    Vector3 m_LowSpace = Vector3.zero;
    Vector3 m_HighSpace = Vector3.zero;

    void Start()
    {
        gradient = GetComponent<GradientContainer>().ColorGradient;

        // If there's a path, load data
        if (!String.IsNullOrEmpty(dataPath))
        {
            LoadData();
        }
    }

    /// <summary>
    /// Load data from a resource in the Resources folder
    /// </summary>
    void LoadData()
    {
        // Use the parser to load data
        m_DataParser.LoadData(dataPath, parseHandler, HeatmapDataParser.k_AsResource);
    }

    /// <summary>
    /// Once loaded, returns all the important info.
    /// </summary>
    /// <param name="heatData">A dictionary of all the heat data.</param>
    /// <param name="maxDensity">The maximum data density.</param>
    /// <param name="maxTime">The maximum time from the data.</param>
    /// <param name="options">The list of possible options (usually event names).</param>
    void parseHandler(Dictionary<string, HeatPoint[]> heatData, string[] options)
    {
        m_Data = heatData;
        this.options = options;
        Render();
    }

    /// <summary>
    /// Renders the heatmap
    /// </summary>
    void Render()
    {
        if (m_Data == null)
            return;

        SetLimits(m_Data[options[optionIndex]]);

        var r = gameObject.GetComponent<IHeatmapRenderer>();
        r.allowRender = true;
        r.pointSize = pointSize;
        r.UpdateGradient(gradient);
        r.UpdateTimeLimits(0, m_MaxTime);
        r.UpdateRenderMask(m_LowSpace.x, m_HighSpace.x, m_LowSpace.y, m_HighSpace.y, m_LowSpace.z, m_HighSpace.z);
        r.UpdateRenderStyle(RenderShape.Triangle, RenderDirection.YZ);
        r.UpdatePointData(m_Data[options[optionIndex]], m_MaxDensity);

        r.RenderHeatmap();
    }

    /// <summary>
    /// We can adjust the optionIndex or pointSize at runtime
    /// </summary>
    void Update()
    {
        if (optionIndex != oldOptionIndex || pointSize != oldPointSize)
        {
            if (options == null)
            {
                optionIndex = 0;
            }
            else
            {
                optionIndex = Math.Max(0, optionIndex);
                optionIndex = Math.Min(optionIndex, options.Length-1);
                Render();
                oldOptionIndex = optionIndex;
            }
            oldPointSize = pointSize;
        }

        // Uncomment this if you want to see output of current/total points
        //        if (m_Data != null)
        //        {
        //            var r = gameObject.GetComponent<IHeatmapRenderer>();
        //            Debug.Log(r.currentPoints + "/" + r.totalPoints);
        //        }
    }

    /// <summary>
    /// Sets time and space variables for the renderer based on the data
    /// </summary>
    /// <param name="points">The heatmap data that will be fed to the renderer.</param>
    void SetLimits(HeatPoint[] points)
    {
        float maxDensity = 0;
        m_MaxTime = 0;
        m_LowSpace = new Vector3();
        m_HighSpace = new Vector3();

        for (int a = 0; a < points.Length; a++)
        {
            maxDensity = Mathf.Max(maxDensity, points[a].density);
            m_MaxTime = Mathf.Max(m_MaxTime, points[a].time);
            m_LowSpace = Vector3.Min(m_LowSpace, points[a].position);
            m_HighSpace = Vector3.Max(m_HighSpace, points[a].position);
        }
    }
}
