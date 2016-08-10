using System;
using UnityEditor;
using UnityEngine;

namespace UnityAnalyticsHeatmap
{

    public static class AnalyticsDatePicker {

        public static string DatePicker(Rect controlRect, string value)
        {
            int controlID = GUIUtility.GetControlID (FocusType.Keyboard);

            switch (Event.current.GetTypeForControl(controlID))
            {
            case EventType.Repaint:
            case EventType.Layout:
                value = EditorGUILayout.TextField(value);
                break;
            case EventType.MouseDown:
                if (controlRect.Contains(Event.current.mousePosition) && Event.current.button == 0)
                {
                    GUIUtility.hotControl = controlID;
                    Event.current.Use();
                }
                break;

            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlID)
                {
                    GUIUtility.hotControl = 0;

                    Debug.Log(value);
                    //                GUIContent buttonText = new GUIContent("...");
                    //                Rect rt = GUILayoutUtility.GetRect(buttonText, GUI.skin.button);
                    //                if (GUI.Button(rt,buttonText,GUI.skin.button))
                    //                {
                    //                    DatePicker window = ScriptableObject.CreateInstance<DatePicker>();
                    //                    window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
                    //                    window.ShowAsDropDown(rt, new Vector2(300, 200));
                    //                }
                    Event.current.Use();
                }
                break;
            }

            return value;
        }
    }

    public class DatePicker : EditorWindow {

        public string value;

        void OnGUI()
        {
            value = EditorGUILayout.TextField(value);

            if (GUILayout.Button("Close"))
            {
                Close();
            }
        }
    }

    public class DatePickerModel
    {
        private int _year;
        public int m_Year
        {
            get
            {
                return _year;
            }
            set
            {
                _year = value;
                Calculate();
            }
        }
        private int _month;
        public int m_Month
        {
            get
            {
                return _month;
            }
            set
            {
                _month = value;
                Calculate();
            }
        }
        private int _date;
        public int m_Date
        {
            get
            {
                return _date;
            }
            set
            {
                _date = value;
                Calculate();
            }
        }

        string[] m_DaysOfTheWeek = new string[7]
            {"Sunday", "Monday", "Tuesday",
                "Wednesday", "Thursday", "Friday", "Saturday"};
        string[] m_MonthsOfTheYear = new string[12]
            {"January", "February", "March", "April",
            "May", "June", "July", "August",
            "September", "October", "November", "December"
            };
        int[] m_DaysInMonths = new int[12]{31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31};
        string[] m_MonthDays;

        private void Calculate()
        {
            var date = new DateTime(m_Year, m_Month, m_Date);
            int firstDay = FirstDayOfWeek(date);
            int daysInMonth = m_DaysInMonths[m_Month];

            m_MonthDays = new string[daysInMonth + firstDay];
            for(int a = 0; a < daysInMonth + firstDay; a++)
            {
                m_MonthDays[a] = (a < firstDay) ? " " : (a - firstDay).ToString();
            }

        }

        // Employ Zeller's congruence to compute the month's calendar
        public static int FirstDayOfWeek (DateTime date)
        {
            int day = date.Day;
            int month = ((date.Month + 9) % 12) + 1;

            int yearsInCentury = month > 10 ?
                (date.Year - 1) % 100 :
                date.Year % 100;

            int century = yearsInCentury == 99 && month > 10 ?
                (date.Year - 100) / 100 :
                date.Year / 100;

            int dayOfWeek = ((((int)((2.6 * (float)month) - 0.2)
                + day + yearsInCentury + (yearsInCentury / 4)
                + (century / 4) - (2 * century))) % 7);

            return dayOfWeek;
        }
    }
}

