/// <summary>
/// Visual component for managing a list of popup lists.
/// </summary>

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RVEALR.Heatmaps
{
    public class AnalyticsListGroup
    {

        public delegate void ChangeHandler(List<int> value);


        public static List<int> ListGroup(List<int>value, List<List<string>> lists, ChangeHandler change)
        {
            if (lists == null || value == null)
            {
                return value;
            }
            EditorGUI.BeginChangeCheck();
            for(int a = 0; a < value.Count; a++)
            {
                var listArray = lists[a].ToArray();
                value[a] = EditorGUILayout.Popup(value[a], listArray);
            }
            if (EditorGUI.EndChangeCheck())
            {
                change(value);
            }
            return value;
        }
    }
}

