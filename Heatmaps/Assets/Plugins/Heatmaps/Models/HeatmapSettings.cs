using System;
using UnityEditor;
using System.Collections.Generic;
using UnityAnalyticsHeatmap;

public class HeatmapSettings : EventArgs
{
    public virtual void OnEnable()
    {
        heatmapInFront = EditorPrefs.GetBool(HeatmapsInspectorSettingsKeys.k_HeatmapInFrontKey);

        heatmapOptionIndex = 0;
        heatmapOptions = new List<int>();

        maskFollowType = EditorPrefs.GetInt(HeatmapsInspectorSettingsKeys.k_MaskFollowType);
        maskRadius = EditorPrefs.GetFloat(HeatmapsInspectorSettingsKeys.k_MaskRadiusKey, 1.0f);
        maskType = EditorPrefs.GetInt(HeatmapsInspectorSettingsKeys.k_MaskTypeKey);

        particleSize = EditorPrefs.GetFloat(HeatmapsInspectorSettingsKeys.k_ParticleSizeKey);
        particleShape = EditorPrefs.GetInt(HeatmapsInspectorSettingsKeys.k_ParticleShapeKey);
        particleDirection = EditorPrefs.GetInt(HeatmapsInspectorSettingsKeys.k_ParticleSizeKey);
        particleProjection = EditorPrefs.GetInt(HeatmapsInspectorSettingsKeys.k_ParticleProjectionKey);

        remapDensity = EditorPrefs.GetBool(HeatmapsInspectorSettingsKeys.k_RemapColorKey);
        remapColorField = EditorPrefs.GetString(HeatmapsInspectorSettingsKeys.k_RemapColorFieldKey);
        remapOptionIndex = EditorPrefs.GetInt(HeatmapsInspectorSettingsKeys.k_RemapOptionIndexKey);
        remapPercentile = EditorPrefs.GetFloat(HeatmapsInspectorSettingsKeys.k_PercentileKey);

        separateUsers = EditorPrefs.GetBool(HeatmapsInspectorSettingsKeys.k_SeparateUsersKey);


        smoothSpaceOption = EditorPrefs.GetInt(HeatmapsInspectorSettingsKeys.k_SmoothSpaceOptionKey);
        smoothSpace = EditorPrefs.GetInt(HeatmapsInspectorSettingsKeys.k_SmoothSpaceKey);

        smoothRotationOption = EditorPrefs.GetInt(HeatmapsInspectorSettingsKeys.k_SmoothRotationOptionKey);
        smoothRotation = EditorPrefs.GetInt(HeatmapsInspectorSettingsKeys.k_SmoothRotationKey);

        smoothTimeOption = EditorPrefs.GetInt(HeatmapsInspectorSettingsKeys.k_SmoothTimeOptionKey);
        smoothTime = EditorPrefs.GetInt(HeatmapsInspectorSettingsKeys.k_SmoothTimeKey);
    }

    public virtual void PostProcess()
    {
    }

    public bool heatmapInFront;

    public int heatmapOptionIndex;
    public List<int> heatmapOptions;

    public int maskFollowType;
    public float maskRadius;
    public int maskType;

    public float particleSize;
    public int particleShape;
    public int particleDirection;
    public int particleProjection;

    public bool remapDensity;
    public string remapColorField;
    public int remapOptionIndex;
    public float remapPercentile;

    public bool separateUsers;

    public int smoothSpaceOption;
    public float smoothSpace;

    public int smoothRotationOption;
    public float smoothRotation;

    public int smoothTimeOption;
    public float smoothTime;
}