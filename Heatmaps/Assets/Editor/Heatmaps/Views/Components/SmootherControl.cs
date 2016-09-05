using System;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace UnityAnalyticsHeatmap
{
    public class AnalyticsSmootherControl
    {
        private static GUIContent[] s_SmootherOptionsContent;


        private static Texture2D darkSkinUnionIcon = EditorGUIUtility.Load("Assets/Editor/Heatmaps/Textures/union_dark.png") as Texture2D;
        private static Texture2D darkSkinNumberIcon = EditorGUIUtility.Load("Assets/Editor/Heatmaps/Textures/number_dark.png") as Texture2D;
        private static Texture2D darkSkinNoneIcon = EditorGUIUtility.Load("Assets/Editor/Heatmaps/Textures/none_dark.png") as Texture2D;

        private static Texture2D lightSkinUnionIcon = EditorGUIUtility.Load("Assets/Editor/Heatmaps/Textures/union_light.png") as Texture2D;
        private static Texture2D lightSkinNumberIcon = EditorGUIUtility.Load("Assets/Editor/Heatmaps/Textures/number_light.png") as Texture2D;
        private static Texture2D lightSkinNoneIcon = EditorGUIUtility.Load("Assets/Editor/Heatmaps/Textures/none_light.png") as Texture2D;

        public delegate void ChangeHandler(int toggler, float value);

        public static void SmootherControl (ref int toggler, ref float value, 
            string label, string tooltip,  ChangeHandler change,
            int endIndex = -1)
        {
            if (s_SmootherOptionsContent == null)
            {
                var unionIcon = lightSkinUnionIcon;
                var smoothIcon = lightSkinNumberIcon;
                var noneIcon = lightSkinNoneIcon;
                if (EditorGUIUtility.isProSkin)
                {
                    unionIcon = darkSkinUnionIcon;
                    smoothIcon = darkSkinNumberIcon;
                    noneIcon = darkSkinNoneIcon;
                }

                s_SmootherOptionsContent = new GUIContent[] {
                    new GUIContent(smoothIcon, "Smooth to value"),
                    new GUIContent(noneIcon, "No smoothing"),
                    new GUIContent(unionIcon, "Unify all")
                };
            }

            using (new EditorGUILayout.VerticalScope())
            {
                var options = endIndex == -1 ? s_SmootherOptionsContent : 
                    s_SmootherOptionsContent.Take(endIndex).ToArray();

                EditorGUI.BeginChangeCheck();
                toggler = GUILayout.Toolbar(
                    toggler, options, GUILayout.MaxWidth(100));

                float lw = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 50;
                float fw = EditorGUIUtility.fieldWidth;
                EditorGUIUtility.fieldWidth = 20;
                EditorGUI.BeginDisabledGroup(toggler != HeatmapDataProcessor.SMOOTH_VALUE);
                value = EditorGUILayout.FloatField(new GUIContent(label, tooltip), value);
                value = Mathf.Max(0, value);
                EditorGUI.EndDisabledGroup();
                EditorGUIUtility.labelWidth = lw;
                EditorGUIUtility.fieldWidth = fw;

                if (EditorGUI.EndChangeCheck())
                {
                    change(toggler, value);
                }
            }
        }
    }
}

