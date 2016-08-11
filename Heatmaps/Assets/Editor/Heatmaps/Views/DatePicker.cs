using System;
using UnityEditor;
using UnityEngine;

namespace UnityAnalyticsHeatmap
{

    public static class AnalyticsDatePicker {

        private static GUIStyle fieldStyle;
        private static Vector2 fieldOffset = new Vector2(0, -2f);

        private static Vector2 winSize = new Vector2(300f, 175f);

        public static string s_CachedValue;
        private static int s_CachedControlId;
        private static bool s_DoDropDown = false;

        public static string DatePicker(string value)
        {
            fieldStyle = new GUIStyle("box");
            int controlID = GUIUtility.GetControlID (FocusType.Keyboard);
            Rect controlRect = EditorGUILayout.GetControlRect(false);


            fieldStyle.contentOffset = fieldOffset;
            fieldStyle.clipping = TextClipping.Overflow;
            EditorGUI.LabelField(controlRect, value, fieldStyle);

            switch (Event.current.GetTypeForControl(controlID))
            {
            case EventType.MouseDown:
                if (controlRect.Contains(Event.current.mousePosition) && Event.current.button == 0)
                {
                    GUIUtility.hotControl = controlID;
                    Event.current.Use();
                }
                break;
            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlID && Event.current.button == 0 
                    && controlRect.Contains(Event.current.mousePosition))
                {
                    s_DoDropDown = true;
                    Event.current.Use();
                }
                break;

            case EventType.Repaint:
                if (GUIUtility.hotControl == controlID && s_DoDropDown)
                {
                    GUIUtility.hotControl = 0;
                    s_DoDropDown = false;
                    DatePicker window = ScriptableObject.CreateInstance<DatePicker>();
                    Vector2 p = EditorGUIUtility.GUIToScreenPoint(new Vector2(controlRect.x, controlRect.y+controlRect.height));
                    window.ShowPopup(new Rect(p.x, p.y-winSize.y, winSize.x, winSize.y), winSize, value);
                    s_CachedControlId = controlID;
                }
                break;
            }

            if (s_CachedControlId == controlID && s_CachedValue != null)
            {
                value = s_CachedValue;
                s_CachedControlId = 0;
                s_CachedValue = null;
            }

            return value;
        }
    }

    public class DatePicker : EditorWindow
    {
        public string value;
        private DatePickerModel model = new DatePickerModel();
        private bool doCommit = false;
        private GUILayoutOption itemWidth = GUILayout.Width(35f);

        private static GUIStyle fieldStyle;
        private static GUIStyle dateStyle;
        private static RectOffset dateFieldRectOffset = new RectOffset(0, 0, 2, 0);

        private static string[] m_MonthsOfTheYear = new string[12]
        {"January", "February", "March", "April",
            "May", "June", "July", "August",
            "September", "October", "November", "December"};

        public void ShowAsDropDown(Rect buttonRect, Vector2 windowSize, string dateValue)
        {
            base.ShowAsDropDown(buttonRect, windowSize);
            value = dateValue;
        }

        void OnGUI()
        {
            if (fieldStyle == null)
            {
                fieldStyle = new GUIStyle();
                fieldStyle.alignment = TextAnchor.MiddleCenter;

                dateStyle = new GUIStyle("box");
                dateStyle.alignment = TextAnchor.MiddleCenter;
                dateStyle.margin = dateFieldRectOffset;
                dateStyle.stretchWidth = true;
                dateStyle.clipping = TextClipping.Overflow;
            }

            value = EditorGUILayout.TextField(value);
            if (string.IsNullOrEmpty(value))
                return;

            model.Update(value);
            using(new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("<"))
                {
                    model.decrementMonth();
                    model.Calculate();
                }

                int monthIndex = Mathf.Clamp(model.m_Month - 1, 1, 12);
                EditorGUILayout.LabelField(model.m_Year + " " + m_MonthsOfTheYear[monthIndex], dateStyle);
                if (GUILayout.Button(">"))
                {
                    model.incrementMonth();
                    model.Calculate();
                }
            }

            using (new EditorGUILayout.VerticalScope("box"))
            {
                for (int a = 0; a < 6; a++)
                {
                    using(new EditorGUILayout.HorizontalScope())
                    {
                        for (int b = a*7; b < (a*7)+7; b++)
                        {
                            var dt = model.m_MonthDays[b];
                            if (dt == 0)
                            {
                                EditorGUILayout.LabelField(" ", itemWidth);
                            }
                            else if (dt == model.m_Date)
                            {
                                
                                EditorGUILayout.LabelField(dt.ToString(), fieldStyle, itemWidth);
                            }
                            else if (GUILayout.Button(dt.ToString(), itemWidth))
                            {
                                //TODO: Commit the new date
                                model.m_Date = dt;
                                doCommit = true;
                            }
                        }
                    }
                }
            }
            value = model.dateString;

            if (doCommit || Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                Commit();
            }
        }


        int count = 0;

        void Commit()
        {
            Debug.Log("COMMIT");
            count ++;
            AnalyticsDatePicker.s_CachedValue = value;
            if (count > 5 || Event.current.type == EventType.MouseUp || Event.current.type == EventType.KeyUp) {
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
                dateString = _year + "-" + MinFormat(m_Month) + "-" + MinFormat(m_Date);
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
                dateString = m_Year + "-" + MinFormat(_month) + "-" + MinFormat(m_Date);
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
                dateString = m_Year + "-" + MinFormat(m_Month) + "-" + MinFormat(_date);
            }
        }

        private string _dateString;
        public string dateString
        {
            get
            {
                return _dateString;
            }
            private set {
                _dateString = value;
            }
        }

        public void decrementMonth()
        {
            if (m_Month - 1 < 1)
            {
                m_Year --;
                m_Month = 12;
            }
            else
            {
                m_Month --;
            }
        }

        public void incrementMonth()
        {
            if (m_Month + 1 > 12)
            {
                m_Year ++;
                m_Month = 1;
            }
            else
            {
                m_Month ++;
            }
        }


        int[] m_DaysInMonths = new int[12]{31, 28, 31, 30, 31, 30, 31, 30, 30, 31, 30, 31};
        public int[] m_MonthDays;

        public void Update(string date)
        {
            dateString = date;
            string[] elements = date.Split('-');
            try {
                m_Year = int.Parse(elements[0]);
                m_Month = int.Parse(elements[1]);
                m_Date = int.Parse(elements[2]);
            } catch {
                //No-op
            }
            Calculate();
        }

        public void Calculate()
        {
            if (!CheckDateValidity())
            {
                return;
            }
            var date = new DateTime(m_Year, m_Month, 1);
            int firstDay = (int)date.DayOfWeek;
            int daysInMonth = GetDaysInMonth();

            m_MonthDays = new int[42];
            for(int a = 0; a < 42; a++)
            {
                if (a >= firstDay && a < daysInMonth+firstDay)
                {
                    m_MonthDays[a] =  a + 1 - firstDay;
                }
            }
        }

        int GetDaysInMonth()
        {
            int days = m_DaysInMonths[m_Month-1];
            // leap year calculation
            if (m_Month == 2 && m_Year % 4 == 0 && (m_Year % 100 != 0 || m_Year % 400 == 0))
            {
                days ++;
            }
            return days;
        }

        bool CheckDateValidity()
        {
            try
            {
                new DateTime(m_Year, m_Month, m_Date);
                return true;
            }
            catch
            {
                return false;
            }
        }

        string MinFormat(int number)
        {
            return number < 10 ? "0" + number : number.ToString();
        }
    }
}

