/// <summary>
/// Attaching this component to a GameObject allows the Heatmap to use it as an alternate origin.
/// </summary>
///
/// By default, all heatmap points are calculated according to world (global) space. If a
/// point is sent with the optional "originID" set, the Heatmapper will look for a GameObject
/// with a HeatmapOrigin component and a matching originID tag and calculate relative to that
/// GameObject's local transform space.
/// 
/// To use:
/// If you want your heatmap to use global space: do nothing. Go away!
/// If you want your heatmap to center on something other than global space:
/// 1. Attach this component to the GameObject you want to act as a center.
/// 2. Decide on labelling options for that GameObject's HeatmapOrigin (see ID Labelling Options, below).
/// 3. When sending a HeatmapEvent, attach the optional `originID` property, citing the appropriate
/// originId.
/// 
/// <example>
/// public class PositionTracker : MonoBehaviour
/// {
/// [Range(0.1f, 60.0f)]
/// public float trackIntervalInSeconds = 30.0f;
/// // Assign the origin, so its originID can be included with the event information
/// public HeatmapOrigin myOrigin;
/// void Start()
/// {
///     StartCoroutine(TrackingTick());
/// }
/// IEnumerator TrackingTick()
/// {
///     yield return new WaitForSeconds(trackIntervalInSeconds);
///     // Note how when we send the event, we tie it back to the origin
///     HeatmapEvent.Send ("PlayerPosition", transform.localPosition, null, myOrigin.m_OriginID);
///     StartCoroutine(TrackingTick());
/// }
/// </example>
/// 
/// ID Labelling Options
/// The `HeatmapOrigin.m_OriginID` tells the heatmap where to originate a set of points.
/// The origin is generated from a combination of three controls:
/// 1. `Label Stem` is a human-readable name. If empty, the stem will be an empty string.
/// 2. If `Label Is Dynamic` is checked, a GUID will be generated.
/// 3. If `Label Is Dynamic At Runtime` is checked, a new GUID will be generated at runtime.
///
/// Examples
/// Stem Only:
/// "BasicSoldier", "AdvancedSoldier", "BigBoss"
/// We may spawn many instances of these classes, and we want to establish a cloud of points around an example instance.
///
/// Dynamic Only:
/// "e64016f5-7ecd-4f04-8489-f37d4e31e26e", "50445e1a-814c-4c41-bfbd-3fe2b623bc1b"
/// Not very human-readable, but very useful if you don't want to have to enter values every
/// time you add a HeatmapOrigin.
/// 
/// Stem + Dynamic:
/// "Chicken-e64016f5-7ecd-4f04-8489-f37d4e31e26e", "Chicken-50445e1a-814c-4c41-bfbd-3fe2b623bc1b"
/// This is helpful if you're spawning lots of instances -- and you want something readable -- but each needs a unique ID.

using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEditor;

namespace UnityAnalyticsHeatmap
{
    [ExecuteInEditMode]
    public class HeatmapOrigin : MonoBehaviour
    {
        public bool m_LabelIsDynamic;
        public bool m_LabelIsDynamicAtRuntime;
        public string m_LabelStem = "";

        [ReadOnly(hidden=true)] public string m_Guid;
        [ReadOnly] public string m_OriginID;

        bool m_OldLabelIsDynamic;
        string m_OldLabelStem;
        string m_OldGuid;

        void Awake()
        {
            if (m_LabelIsDynamicAtRuntime) {
                ClearGuid ();
            }
            GenerateID();
        }

        void Update()
        {
            GenerateID();
            ForceDynamic ();
        }

        void GenerateID()
        {
            GenerateGuid();
            if (m_OldLabelIsDynamic != m_LabelIsDynamic) {
                if (!m_LabelIsDynamic) {
                    ClearGuid ();
                }
                GenerateGuid ();
                m_OldLabelIsDynamic = m_LabelIsDynamic;
            }
            if (m_Guid != m_OldGuid || m_LabelStem != m_OldLabelStem) {
                if (m_LabelIsDynamic && !string.IsNullOrEmpty (m_LabelStem)) {
                    m_OriginID = m_LabelStem + "-" + m_Guid;
                } else if (m_LabelIsDynamic) {
                    m_OriginID = m_Guid;
                } else {
                    m_OriginID = m_LabelStem;
                }
                m_OldGuid = m_Guid;
                m_OldLabelStem = m_LabelStem;
            }
        }

        void GenerateGuid()
        {
            if (m_LabelIsDynamic && string.IsNullOrEmpty(m_Guid))
            {
                m_Guid = System.Guid.NewGuid().ToString();
            }
        }

        void ClearGuid()
        {
            m_Guid = "";
        }

        void ForceDynamic()
        {
            if (!m_LabelIsDynamic && m_LabelIsDynamicAtRuntime) {
                m_LabelIsDynamic = true;
            }
        }
    }

    public class ReadOnlyAttribute : PropertyAttribute {
        public bool hidden;
        public ReadOnlyAttribute(){}
        public ReadOnlyAttribute(bool hidden)
        {
            this.hidden = hidden;
        }
    }

    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    /** ReadOnlyAttributeDrawer - A class to make Read-Only inspector properties. **/
    public class ReadOnlyAttributeDrawer : PropertyDrawer
    {
        // Necessary since some properties tend to collapse smaller than their content
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ReadOnlyAttribute readOnly = attribute as ReadOnlyAttribute;
            return readOnly.hidden ? 0f : EditorGUI.GetPropertyHeight(property, label, true);
        }

        // Draw a disabled property field
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ReadOnlyAttribute readOnly = attribute as ReadOnlyAttribute;
            if (!readOnly.hidden)
            {
                GUI.enabled = false; // Disable fields
                EditorGUI.PropertyField(position, property, label, true);
                GUI.enabled = true; // Enable fields
            }
        }
    }
}

