
using System;
using UnityEngine;
using UnityEditor;
using UnityAnalyticsHeatmap;
using System.Collections.Generic;
using System.Collections;
using UnityAnalytics;

public class RawDataInspector : EditorWindow
{
    private static string k_FetchKey = "UnityAnalyticsRawDataGenFetchKey";
    private static string k_Installed = "UnityAnalyticsRawDataGenInstallKey";
    private static string k_DataPathKey = "UnityAnalyticsRawDataGenDataPath";
    private static string k_DeviceCountKey = "UnityAnalyticsRawDataGenDeviceCount";
    private static string k_SessionCountKey = "UnityAnalyticsRawDataGenSessionCount";
    private static string k_EventNamesKey = "UnityAnalyticsRawDataGenEventNames";
    private static string k_CustomEventsKey = "UnityAnalyticsRawDataGenCustomEvents";
    private static string k_EventCountKey = "UnityAnalyticsRawDataGenEventCount";

    private static string k_IncludeTimeKey = "UnityAnalyticsRawDataGenIncludeTime";

    private static string k_IncludeLevelKey = "UnityAnalyticsRawDataGenIncludeLevel";
    private static string k_MinLevel = "UnityAnalyticsRawDataGenMinLevel";
    private static string k_MaxLevel = "UnityAnalyticsRawDataGenMaxLevel";

    private static string k_IncludeFPSKey = "UnityAnalyticsRawDataGenIncludeFPS";
    private static string k_MinFPS = "UnityAnalyticsRawDataGenMinFPS";
    private static string k_MaxFPS = "UnityAnalyticsRawDataGenMaxFPS";

    private static string k_IncludeXKey = "UnityAnalyticsRawDataGenIncludeX";
    private static string k_MinX = "UnityAnalyticsRawDataGenMinX";
    private static string k_MaxX = "UnityAnalyticsRawDataGenMaxX";

    private static string k_IncludeYKey = "UnityAnalyticsRawDataGenIncludeY";
    private static string k_MinY = "UnityAnalyticsRawDataGenMinY";
    private static string k_MaxY = "UnityAnalyticsRawDataGenMaxY";

    private static string k_IncludeZKey = "UnityAnalyticsRawDataGenIncludeZ";
    private static string k_MinZ = "UnityAnalyticsRawDataGenMinZ";
    private static string k_MaxZ = "UnityAnalyticsRawDataGenMaxZ";

    private static string k_RotationKey = "UnityAnalyticsRawDataGenRotation";
    private static string k_MinRX = "UnityAnalyticsRawDataGenMinRX";
    private static string k_MaxRX = "UnityAnalyticsRawDataGenMaxRX";
    private static string k_MinRY = "UnityAnalyticsRawDataGenMinRY";
    private static string k_MaxRY = "UnityAnalyticsRawDataGenMaxRY";
    private static string k_MinRZ = "UnityAnalyticsRawDataGenMinRZ";
    private static string k_MaxRZ = "UnityAnalyticsRawDataGenMaxRZ";

    private static string k_MinDX = "UnityAnalyticsRawDataGenMinDX";
    private static string k_MaxDX = "UnityAnalyticsRawDataGenMaxDX";
    private static string k_MinDY = "UnityAnalyticsRawDataGenMinDY";
    private static string k_MaxDY = "UnityAnalyticsRawDataGenMaxDY";
    private static string k_MinDZ = "UnityAnalyticsRawDataGenMinDZ";
    private static string k_MaxDZ = "UnityAnalyticsRawDataGenMaxDZ";

    private static int defaultEventCount = 100;
    private static int defaultDeviceCount = 5;
    private static int defaultSessionCount = 10;
    private static float defaultMinAngle = 0f;
    private static float defaultMaxAngle = 360f;
    private static float defaultMinSpace = -100f;
    private static float defaultMaxSpace = 100f;
    private static int defaultRotational = 0;
    private static int defaultMinLevel = 1;
    private static int defaultMaxLevel = 99;
    private static float defaultMinFPS = 1f;
    private static float defaultMaxFPS = 99f;

    public const string headers = "ts\tappid\ttype\tuserid\tsessionid\tremote_ip\tplatform\tsdk_ver\tdebug_device\tuser_agent\tsubmit_time\tname\tcustom_params\n";
    private static DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
    
    [MenuItem("Window/Unity Analytics/Raw Data #%r")]
    static void RawDataInspectorMenuOption()
    {
        EditorWindow.GetWindow(typeof(RawDataInspector));
    }

    string m_DataPath = "";

    int m_EventCount = defaultEventCount;
    int m_DeviceCount = defaultDeviceCount;
    int m_SessionCount = defaultSessionCount;

    bool m_SendIos = true;
    bool m_SendAndroid = true;
    bool m_SendWeb = true;


    int m_DataStoryIndex = 0;
    GUIContent[] m_DataStoryList = new GUIContent[]{ 
        new GUIContent("Basic Functionality"),
        new GUIContent("Really Big Game"),
        new GUIContent("Maze 1: Multilevel Game"),
        new GUIContent("Maze 2: FPS Dropoff"), 
        new GUIContent("VR Lookat"),
        new GUIContent("Speed Racer")
    };
    DataStory[] m_DataStories = new DataStory[]{
        new BasicDataStory(),
        new ReallyBigDataStory(),
        new MultiLevelDataStory(),
        new FPSDropoffDataStory(),
        new VRLookAtDataStory(),
        new SpeedRacerDataStory()
    };

    string m_AppId = "";
    string m_DataKey = "";
    string m_FetchKey = "";
    string m_StartDate = "";
    string m_EndDate = "";

    List<JobRequest> m_Jobs = null;

    int m_DataSource = 0;
    static int FETCH = 0;
    static int GENERATE = 1;
    int m_GenerateType = 0;
    static int HEATMAP_RANDOM = 0;
    static int FREEFORM_RANDOM = 1;
    static int DEMO = 2;

    List<string> m_EventNames = new List<string>{ };
    List<TestCustomEvent> m_CustomEvents = new List<TestCustomEvent>();

    bool m_IncludeTime = true;

    bool m_IncludeX = true;
    float m_MinX = defaultMinSpace;
    float m_MaxX = defaultMaxSpace;

    bool m_IncludeY = true;
    float m_MinY = defaultMinSpace;
    float m_MaxY = defaultMaxSpace;

    bool m_IncludeZ = true;
    float m_MinZ = defaultMinSpace;
    float m_MaxZ = defaultMaxSpace;

    // Flag for rotation vs destination
    int m_Rotational = defaultRotational;
    static int ROTATION = 1;
    static int DESTINATION = 2;

    float m_MinRX = defaultMinAngle;
    float m_MaxRX = defaultMaxAngle;
    float m_MinRY = defaultMinAngle;
    float m_MaxRY = defaultMaxAngle;
    float m_MinRZ = defaultMinAngle;
    float m_MaxRZ = defaultMaxAngle;

    float m_MinDX = defaultMinSpace;
    float m_MaxDX = defaultMaxSpace;
    float m_MinDY = defaultMinSpace;
    float m_MaxDY = defaultMaxSpace;
    float m_MinDZ = defaultMinSpace;
    float m_MaxDZ = defaultMaxSpace;

    bool m_IncludeLevel = false;
    int m_MinLevel = defaultMinLevel;
    int m_MaxLevel = defaultMaxLevel;

    bool m_IncludeFPS = false;
    float m_MinFPS = defaultMinFPS;
    float m_MaxFPS = defaultMaxFPS;

    Vector2 m_ScrollPosition;

    DownloadManager downloadManager = new DownloadManager();

    public RawDataInspector()
    {
        titleContent = new GUIContent("Raw Data");
    }

    void OnFocus()
    {
        if (EditorPrefs.GetBool(k_Installed))
        {
            RestoreValues();
        }
        else
        {
            SetInitValues();
        }
    }

    void OnGUI()
    {
        m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
        EditorPrefs.SetString(k_DataPathKey, m_DataPath);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset"))
        {
            if (EditorUtility.DisplayDialog("Resetting to factory defaults", "Are you sure?", "Reset", "Cancel"))
            {
                SetInitValues();
            }
        }
        if (GUILayout.Button("Open Folder"))
        {
            EditorUtility.RevealInFinder(m_DataPath);
        }
        GUILayout.EndHorizontal();

        //output path
        EditorGUILayout.LabelField("Output path", EditorStyles.boldLabel);
        m_DataPath = EditorGUILayout.TextField(m_DataPath);
        if (m_DataPath == "") {
            m_DataPath = Application.persistentDataPath;
        }

        m_DataSource = GUILayout.Toolbar(m_DataSource, new string[] {"Fetch Data", "Generate Test Data"});

        if (m_DataSource == FETCH)
        {
            FetchView();
        }
        else if (m_DataSource == GENERATE) 
        {
            m_GenerateType = EditorGUILayout.Popup(m_GenerateType, new string[] { "Heatmap Random", "Freeform Random" , "Demo Data"});
            if (m_GenerateType == HEATMAP_RANDOM)
            {
                HeatmapRandomDataView();
                CreateCode();
            }
            else if (m_GenerateType == FREEFORM_RANDOM)
            {
                FreeformRandomDataView();
                CreateCode();
            }
            else if (m_GenerateType == DEMO)
            {
                CreateDemoData();
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Purge"))
            {
                if (EditorUtility.DisplayDialog("Destroy local data?", "You are about to delete your local heatmaps data cache, meaning you'll have to reload from the server (or regenerate from this tool). Are you sure?", "Purge", "Cancel"))
                {
                    PurgeData();
                }
            }
            if (GUILayout.Button("Generate"))
            {
                GenerateData();
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }

    void FetchView()
    {
        string oldPath = m_FetchKey;
        m_AppId = EditorGUILayout.TextField(new GUIContent("App ID", "Copy the AppID from the 'Editing Project' page of your project dashboard"), m_AppId);
        m_FetchKey = EditorGUILayout.TextField(new GUIContent("Secret Key", "Copy the key from the 'Editing Project' page of your project dashboard"), m_FetchKey);

        downloadManager.m_AppId = m_AppId;
        downloadManager.m_DataKey = m_DataKey;


        if (oldPath != m_FetchKey && !string.IsNullOrEmpty(m_FetchKey))
        {
            EditorPrefs.SetString(k_FetchKey, m_FetchKey);
        }
        m_StartDate = EditorGUILayout.TextField(new GUIContent("Start Date (YYYY-MM-DD)", "Start date as ISO-8601 datetime"), m_StartDate);
        m_EndDate = EditorGUILayout.TextField(new GUIContent("End Date (YYYY-MM-DD)", "End date as ISO-8601 datetime"), m_EndDate);

        if (GUILayout.Button("Create New Job"))
        {

        }
        GUILayout.Space(20f);

        if (GUILayout.Button("Get Jobs"))
        {
            m_Jobs = downloadManager.GetJobs();
        }
        if (m_Jobs != null)
        {
            for (int a = 0; a < m_Jobs.Count; a++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(m_Jobs[a].m_DateRange);
                GUILayout.Label(m_Jobs[a].m_Status.ToString());
                GUILayout.EndHorizontal();
            }
        }
    }

    void FreeformRandomDataView()
    {

        var preCustomEventsString = TestCustomEvent.StringifyList(m_CustomEvents);

        int iterator = 0;
        foreach(TestCustomEvent evt in m_CustomEvents)
        {
            // Display an individual custom event definition

            GUILayout.Label("Event " + ++iterator + ": " + evt.name, EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            evt.name = EditorGUILayout.TextField(evt.name);
            if (GUILayout.Button(new GUIContent("- event", "Delete this event")))
            {
                m_CustomEvents.Remove(evt);
                break;
            }
            if (GUILayout.Button(new GUIContent("+ param", "Add a parameter to this event")))
            {
                evt.Add(new TestEventParam());
            }
            EditorGUILayout.EndHorizontal();
            for(int a = 0; a < evt.Count; a++)
            {
                EditorGUILayout.BeginVertical();
                var param = evt[a];
                EditorGUILayout.BeginHorizontal();
                param.name = EditorGUILayout.TextField(new GUIContent("Parameter " + (a+1)), param.name);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                switch(param.type)
                {
                    case TestEventParam.Bool:
                        EditorGUILayout.LabelField("Value True or False");
                        break;
                    case TestEventParam.Str:
                        param.strValue = EditorGUILayout.TextField(new GUIContent("Value"), param.strValue);
                        break;
                    case TestEventParam.Num:
                        param.min = EditorGUILayout.FloatField(new GUIContent("Range"), param.min);
                        param.max = EditorGUILayout.FloatField(param.max);
                        break;
                }
                param.type = GUILayout.Toolbar(param.type, new GUIContent[] {
                    new GUIContent("S", "string"), 
                    new GUIContent("#", "float or int"), 
                    new GUIContent("B", "boolean")
                });
                if (GUILayout.Button(new GUIContent("- param", "Delete this parameter")))
                {
                    evt.Remove(param);
                    break;
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
        }
        EditorGUILayout.Space();
        if (GUILayout.Button(new GUIContent("+ Event", "Events to be randomly added into the created data.")))
        {
            m_CustomEvents.Add(new TestCustomEvent());
        }
        string postCustomEvents = TestCustomEvent.StringifyList(m_CustomEvents);
        if (preCustomEventsString != postCustomEvents)
        {
            EditorPrefs.SetString(k_CustomEventsKey, postCustomEvents);
        }
        EditorGUILayout.Space();
        CommonEventView();
    }

    void HeatmapRandomDataView()
    {
        ViewEventNames();

        //time
        IncludeSet(ref m_IncludeTime, "time", k_IncludeTimeKey);
        
        //x
        GUILayout.BeginHorizontal();
        if (IncludeSet(ref m_IncludeX, "x", k_IncludeXKey, true)) {
            DrawFloatRange(ref m_MinX, ref m_MaxX, k_MinX, k_MaxX);
        }
        GUILayout.EndHorizontal();
        //y
        GUILayout.BeginHorizontal();
        if (IncludeSet(ref m_IncludeY, "y", k_IncludeYKey, true)) {
            DrawFloatRange(ref m_MinY, ref m_MaxY, k_MinY, k_MaxY);
        }
        GUILayout.EndHorizontal();
        //z
        GUILayout.BeginHorizontal();
        if (IncludeSet(ref m_IncludeZ, "z", k_IncludeZKey)) {
            DrawFloatRange(ref m_MinZ, ref m_MaxZ, k_MinZ, k_MaxZ);
        }
        GUILayout.EndHorizontal();
        
        m_Rotational = GUILayout.SelectionGrid(m_Rotational, new string[] {"None", "Rotation", "Destination"}, 3);
        EditorPrefs.SetInt(k_RotationKey, m_Rotational);
        
        if (m_Rotational == ROTATION) {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("rx");
            DrawFloatRange(ref m_MinRX, ref m_MaxRX, k_MinRX, k_MaxRX);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ry");
            DrawFloatRange(ref m_MinRY, ref m_MaxRY, k_MinRY, k_MaxRY);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("rz");
            DrawFloatRange(ref m_MinRZ, ref m_MaxRZ, k_MinRZ, k_MaxRZ);
            GUILayout.EndHorizontal();
        } else if (m_Rotational == DESTINATION) {
            //destination
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("dx");
            DrawFloatRange(ref m_MinDX, ref m_MaxDX, k_MinDX, k_MaxDX);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("dy");
            DrawFloatRange(ref m_MinDY, ref m_MaxDY, k_MinDY, k_MaxDY);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("dz");
            DrawFloatRange(ref m_MinDZ, ref m_MaxDZ, k_MinDZ, k_MaxDZ);
            GUILayout.EndHorizontal();
        }
        
        //level
        GUILayout.BeginHorizontal();
        if (IncludeSet(ref m_IncludeLevel, "level", k_IncludeLevelKey)) {
            DrawIntRange(ref m_MinLevel, ref m_MaxLevel, k_MinLevel, k_MaxLevel);
        }
        GUILayout.EndHorizontal();
        
        //fps
        GUILayout.BeginHorizontal();
        if (IncludeSet(ref m_IncludeFPS, "fps", k_IncludeFPSKey)) {
            DrawFloatRange(ref m_MinFPS, ref m_MaxFPS, k_MinFPS, k_MaxFPS);
        }
        GUILayout.EndHorizontal();

        CommonEventView();
    }

    void CommonEventView()
    {
        GUILayout.Space(20f);

        m_DeviceCount = EditorGUILayout.IntField(new GUIContent("Device count", "The number of unique devices you want to simulate"), m_DeviceCount);
        EditorPrefs.SetInt(k_DeviceCountKey, m_DeviceCount);

        m_SessionCount = EditorGUILayout.IntField(new GUIContent("Session count", "The number of sessions you want to simulate per device"), m_SessionCount);
        EditorPrefs.SetInt(k_SessionCountKey, m_SessionCount);

        m_EventCount = EditorGUILayout.IntField(new GUIContent("Event count", "The total number of events you want to simulate per session"), m_EventCount);
        EditorPrefs.SetInt(k_EventCountKey, m_EventCount);

        GUILayout.BeginVertical("box");
        GUILayout.Label("Platforms");
        m_SendIos = EditorGUILayout.Toggle(new GUIContent("iOS", "Send as if from iOS"), m_SendIos);
        m_SendAndroid = EditorGUILayout.Toggle(new GUIContent("Android", "Send as if from Android"), m_SendAndroid);
        m_SendWeb = EditorGUILayout.Toggle(new GUIContent("WebGL", "Send as if from WebGL"), m_SendWeb);
        GUILayout.EndVertical();
    }

    void CreateDemoData()
    {
        m_DataStoryIndex = EditorGUILayout.Popup(new GUIContent("Demo", "Pick a story for some demo data."), m_DataStoryIndex, m_DataStoryList);

        var story = m_DataStories[m_DataStoryIndex];
        EditorGUILayout.LabelField("Genre", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(story.genre);
        EditorGUILayout.LabelField("Description", EditorStyles.boldLabel);
        EditorGUILayout.TextArea(story.description, EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("What to try", EditorStyles.boldLabel);
        EditorGUILayout.TextArea(story.whatToTry, EditorStyles.wordWrappedLabel);


        var g = new GUIStyle(GUI.skin.textArea);
        g.wordWrap = true;
        EditorGUILayout.TextArea(story.sampleCode, g);

        // These commented-out lines allow us to actually build a physical maze in the scene.
        // I created this during debugging and thought it was worth keeping.
        // If you ever want to see this work, uncomment the two lines below
        // and generate data using the FPSDropoffDataStory.
//        BuildTarget(story as FPSDropoffDataStory);
//        BuildRoute(story as FPSDropoffDataStory);
    }

    void BuildRoute(FPSDropoffDataStory story)
    {
        if (story == null || story.m_Map == null)
            return;

        var route = story.m_Route;

        var go = GameObject.Find("UnityAnalytics__Route");
        if (go == null)
        {
            go = new GameObject("UnityAnalytics__Route");
        }
        Transform[] children = go.transform.GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            if (child != null && child.gameObject != go)
            {
                child.parent = null;
                DestroyImmediate(child.gameObject);
            }
        }

        for (int a = 0; a < route.Count; a++)
        {
            var tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tile.name = "route_" + a + "___" + route[a][0] + "_" + route[a][1];
            tile.transform.parent = go.transform;
            tile.transform.position = new Vector3(route[a][0], route[a][1], 0);
            tile.transform.localScale = Vector3.one * .1f;

        }
    }

    void BuildTarget(FPSDropoffDataStory story)
    {
        if (story == null || story.m_Map == null)
            return;

        var map = story.m_Map;

        var go = GameObject.Find("UnityAnalytics__Maze");
        if (go == null)
        {
            go = new GameObject("UnityAnalytics__Maze");
        }

        Transform[] children = go.transform.GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            if (child != null && child.gameObject != go)
            {
                child.parent = null;
                DestroyImmediate(child.gameObject);
            }
        }

        for (int x = 0; x < map.Count; x++)
        {
            for (int y = 0; y < map[x].Count; y++)
            {
                MazeMapPoint pt = map[x][y];
                var tile = new GameObject("tile" + pt.x + "_" + pt.y);

                tile.transform.parent = go.transform;
                tile.transform.position = new Vector3(x, y, 0);

                if (pt.entrance != null && pt.exit != null)
                {
                    GameObject wall1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    wall1.transform.parent = tile.transform;
                    GameObject wall2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    wall2.transform.parent = tile.transform;

                    wall1.transform.localScale = new Vector3(1f, .05f, .1f);
                    wall2.transform.localScale = new Vector3(1f, .05f, .1f);

                    var possibles = new List<string>(){ "N", "S", "E", "W" };

                    int exitIdx = possibles.IndexOf(pt.entrance);
                    if (exitIdx > -1)
                        possibles.RemoveAt(exitIdx);

                    int entranceIdx = possibles.IndexOf(pt.exit);
                    if (entranceIdx > -1)
                        possibles.RemoveAt(entranceIdx);

                    Vector3 p1 = MazeDataStory.GetVecPos(possibles[0]);
                    Vector3 p2 = MazeDataStory.GetVecPos(possibles[1]);
                    Quaternion q1 = MazeDataStory.GetQ(possibles[0]);
                    Quaternion q2 = MazeDataStory.GetQ(possibles[1]);
                    wall1.transform.localPosition = p1;
                    wall1.transform.rotation = q1;
                    wall2.transform.localPosition = p2;
                    wall2.transform.rotation = q2;
                }
            }
        }
    }

    void CreateCode()
    {
        EditorGUILayout.LabelField("Example code", EditorStyles.boldLabel);

        string code = "using UnityAnalyticsHeatmap;\n";
        bool needDict = false;

        if (m_IncludeFPS || m_IncludeLevel)
        {
            needDict = true;
            code += "using System.Collections.Generic;\n\n";
            code += "// Dictionary variables are examples. You must create your own!\n";
        }

        if (!m_IncludeX || !m_IncludeY)
        {
            code = "All events must include at a minimum x and y.";
        }
        else
        {
            code += "HeatmapEvent.Send(";
            string testEventName = "someEvent";
            if (m_EventNames != null && m_EventNames.Count > 0) {
                testEventName = m_EventNames[0];
            }
            string eventName = "\"" + testEventName + "\"";
            string transformText = "";
            string time = "";
            string dict = "";

            if (m_Rotational == ROTATION)
            {
                transformText += "transform";
            }
            else if (m_IncludeZ)
            {
                transformText += "transform.position";
            }
            else
            {
                transformText += "new Vector2(transform.position.x, transform.position.y)";
            }
            if (m_Rotational == DESTINATION)
            {
                transformText += ",otherGameObject.transform.position";
            }

            if (m_IncludeTime)
            {
                time = ",Time.timesinceLevelLoad";
            }
            if (needDict)
            {
                dict = ",new Dictionary<string,object>(){";
                if (m_IncludeLevel)
                {
                    dict += "{\"level\", levelId}";
                }
                if (m_IncludeLevel && m_IncludeFPS)
                {
                    dict += ",";
                }
                if (m_IncludeFPS)
                {
                    dict += "{\"fps\", fps}";
                }
                dict += "}";
            }
            code += eventName + "," + transformText + time + dict + ");";
        }
        var g = new GUIStyle(GUI.skin.textArea);
        g.wordWrap = true;
        EditorGUILayout.TextArea(code, g);
    }

    void GenerateData()
    {
        if (m_GenerateType == FREEFORM_RANDOM)
        {
            GenerateFreeformData();
        }
        else if (m_GenerateType == HEATMAP_RANDOM)
        {
            GenerateHeatmapData();
        }
        else
        {
            GenerateStoryData();
        }
    }

    void GenerateFreeformData()
    {
        
        List<string> platforms = new List<string>();
        if (m_SendIos)
            platforms.Add("ios");
        if (m_SendAndroid)
            platforms.Add("android");
        if (m_SendWeb)
            platforms.Add("webgl");

        List<string> problems = new List<string>();

        if (m_DeviceCount < 1) problems.Add("device");
        if (m_SessionCount < 1) problems.Add("session");
        if (m_CustomEvents.Count < 1) problems.Add("event type");
        if (m_EventCount < 1) problems.Add("event");
        if (platforms.Count < 1) problems.Add("platform");

        // If we can't generate, report problems
        if (problems.Count > 0)
        {
            string missing = "";
            for(int p = 0; p < problems.Count; p++)
            {
                missing += problems[p];
                if (problems.Count > 1 && p != problems.Count - 1)
                {
                    missing += ", ";
                    if (p == problems.Count - 2)
                    {
                        missing += "and ";
                    }
                }
            }
            Debug.LogWarningFormat("You must have at least one {0} to generate data.", missing);
            return;
        }

        int linesPerFile = 100;
        int currentFileLines = 0;
        DateTime now = DateTime.UtcNow;

        string data = headers;
        int fileCount = 0;

        int totalSeconds = m_DeviceCount * m_EventCount * m_SessionCount;
        double endSeconds = Math.Round((now - epoch).TotalSeconds);
        double startSeconds = endSeconds - totalSeconds;
        double currentSeconds = startSeconds;
        double firstDate = currentSeconds;

        for (int a = 0; a < m_DeviceCount; a++)
        {
            string platform = platforms[UnityEngine.Random.Range(0, platforms.Count)];
            for (int b = 0; b < m_SessionCount; b++)
            {
                for (int c = 0; c < m_EventCount; c++)
                {
                    TestCustomEvent customEvent = m_CustomEvents[UnityEngine.Random.Range(0, m_CustomEvents.Count)];
                    currentSeconds ++;
                    string evt = customEvent.WriteEvent(a, b, currentSeconds, platform);
                    data += evt;
                    currentFileLines ++;

                    if (currentFileLines >= linesPerFile || currentSeconds == endSeconds) {
                        SaveFile(data, firstDate);
                        firstDate = currentSeconds;
                        currentFileLines = 0;
                        data = headers;
                        fileCount++;
                    }
                }
            }
        }
        string files = (fileCount == 1) ? " file." : " files.";
        Debug.Log("Generated random data: " + totalSeconds + " events " + " in " + fileCount + files);
    }

    void GenerateHeatmapData()
    {
        List<string> platforms = new List<string>();
        if (m_SendIos)
            platforms.Add("ios");
        if (m_SendAndroid)
            platforms.Add("android");
        if (m_SendWeb)
            platforms.Add("webgl");

        List<string> problems = new List<string>();

        if (m_DeviceCount < 1) problems.Add("device");
        if (m_SessionCount < 1) problems.Add("session");
        if (m_CustomEvents.Count < 1) problems.Add("event type");
        if (m_EventNames.Count < 1) problems.Add("event name");
        if (platforms.Count < 1) problems.Add("platform");

        // If we can't generate, report problems
        if (problems.Count > 0)
        {
            string missing = "";
            for(int p = 0; p < problems.Count; p++)
            {
                missing += problems[p];
                if (problems.Count > 1 && p != problems.Count - 1)
                {
                    missing += ", ";
                    if (p == problems.Count - 2)
                    {
                        missing += "and ";
                    }
                }
            }
            Debug.LogWarningFormat("You must have at least one {0} to generate data.", missing);
            return;
        }

        List<TestCustomEvent> events = new List<TestCustomEvent>();
        for (int a = 0; a < m_EventNames.Count; a++)
        {
            TestCustomEvent customEvent = new TestCustomEvent();
            customEvent.name = m_EventNames[a];
            var x = new TestEventParam("x", TestEventParam.Num, m_MinX, m_MaxX);
            customEvent.Add(x);
            var y = new TestEventParam("y", TestEventParam.Num, m_MinY, m_MaxY);
            customEvent.Add(y);
            if (m_IncludeZ)
            {
                var z = new TestEventParam("z", TestEventParam.Num, m_MinZ, m_MaxZ);
                customEvent.Add(z);
            }
            if (m_IncludeLevel)
            {
                var level = new TestEventParam("level", TestEventParam.Num, m_MinLevel, m_MaxLevel);
                customEvent.Add(level);
            }
            if (m_IncludeFPS)
            {
                var fps = new TestEventParam("fps", TestEventParam.Num, m_MinFPS, m_MaxFPS);
                customEvent.Add(fps);
            }
            if (m_IncludeTime)
            {
                // Time needs special-case
                var time = new TestEventParam("t", TestEventParam.Str, "");
                customEvent.Add(time);
            }
            if (m_Rotational == ROTATION)
            {
                var rx = new TestEventParam("rx", TestEventParam.Num, m_MinRX, m_MaxRX);
                customEvent.Add(rx);
                var ry = new TestEventParam("ry", TestEventParam.Num, m_MinRY, m_MaxRZ);
                customEvent.Add(ry);
                var rz = new TestEventParam("rz", TestEventParam.Num, m_MinRZ, m_MaxRZ);
                customEvent.Add(rz);
            }
            else if (m_Rotational == DESTINATION)
            {
                var dx = new TestEventParam("dx", TestEventParam.Num, m_MinDX, m_MaxDX);
                customEvent.Add(dx);
                var dy = new TestEventParam("dy", TestEventParam.Num, m_MinDY, m_MaxDZ);
                customEvent.Add(dy);
                var dz = new TestEventParam("dz", TestEventParam.Num, m_MinDZ, m_MaxDZ);
                customEvent.Add(dz);
            }
            events.Add(customEvent);
        }

        int linesPerFile = 100;
        int currentFileLines = 0;
        DateTime now = DateTime.UtcNow;

        string data = headers;
        int fileCount = 0;

        int totalSeconds = m_DeviceCount * m_EventCount * m_SessionCount;
        double endSeconds = Math.Round((now - epoch).TotalSeconds);
        double startSeconds = endSeconds - totalSeconds;
        double currentSeconds = startSeconds;
        double firstDate = currentSeconds;

        for (int a = 0; a < m_DeviceCount; a++)
        {
            string platform = platforms[UnityEngine.Random.Range(0, platforms.Count)];
            for (int b = 0; b < m_SessionCount; b++)
            {
                for (int c = 0; c < m_EventCount; c++)
                {
                    currentSeconds ++;
                    TestCustomEvent customEvent = events[UnityEngine.Random.Range(0, events.Count)];
                    customEvent.SetParam("t", currentSeconds - startSeconds);
                    if (m_IncludeLevel)
                    {
                        int level = (int)(UnityEngine.Random.Range(m_MinLevel, m_MaxLevel));
                        customEvent.SetParam("level", (float)level, (float)level);
                    }
                    string evt = customEvent.WriteEvent(a, b, currentSeconds, platform);
                    data += evt;
                    currentFileLines ++;

                    if (currentFileLines >= linesPerFile || currentSeconds == endSeconds) {
                        SaveFile(data, firstDate);
                        firstDate = currentSeconds;
                        currentFileLines = 0;
                        data = headers;
                        fileCount++;
                    }
                }
            }
        }
        string files = (fileCount == 1) ? " file." : " files.";
        Debug.Log("Generated heatmap data: " + totalSeconds + " events " + " in " + fileCount + files);
    }

    void GenerateStoryData()
    {
        DataStory story = m_DataStories[m_DataStoryIndex];
        if (story != null)
        {
            Dictionary<double, string> data = story.Generate();
            int fileCount = 0;
            foreach(KeyValuePair<double, string> item in data)
            {
                SaveFile(item.Value, item.Key);
                fileCount ++;
            }
            string files = (fileCount == 1) ? " file." : " files.";
            Debug.Log("Generated data for " + story.name + " in " + fileCount + files);
        }
    }

    bool IncludeSet(ref bool value, string label, string key, bool force=false) {
        string tooltip = force ? label + " must be included" : null;
        var content = new GUIContent(label, tooltip);
        EditorGUI.BeginDisabledGroup(force);
        value = EditorGUILayout.Toggle(content, value);
        EditorGUI.EndDisabledGroup();
        EditorPrefs.SetBool(key, value);
        return value;
    }

    void DrawFloatRange(ref float min, ref float max, string minKey, string maxKey)
    {
        float oldMin = min;
        min = EditorGUILayout.FloatField(min);
        if (oldMin != min)
        {
            EditorPrefs.SetFloat(minKey, min);
        }
        float oldMax = max;
        max = EditorGUILayout.FloatField(max);
        if (oldMax != max)
        {
            EditorPrefs.SetFloat(maxKey, max);
        }
    }

    void DrawIntRange(ref int min, ref int max, string minKey, string maxKey)
    {
        int oldMin = min;
        min = EditorGUILayout.IntField(min);
        if (oldMin != min)
        {
            EditorPrefs.SetInt(minKey, min);
        }
        int oldMax = max;
        max = EditorGUILayout.IntField(max);
        if (oldMax != max)
        {
            EditorPrefs.SetInt(maxKey, max);
        }
    }

    void SaveFile(string data, double firstDate)
    {
        string savePath = System.IO.Path.Combine(GetSavePath(), "RawData");
        // Create the save path if necessary
        if (!System.IO.Directory.Exists(savePath))
        {
            System.IO.Directory.CreateDirectory(savePath);
        }
        string outputFileName = firstDate + "_custom.txt";
        string path = System.IO.Path.Combine(savePath, outputFileName);
        System.IO.File.WriteAllText(path, data);
    }

    string GetSavePath()
    {
        return m_DataPath;
    }
    
    public void PurgeData()
    {
        string savePath = System.IO.Path.Combine(GetSavePath(), "RawData");
        if (System.IO.Directory.Exists(savePath))
        {
            System.IO.Directory.Delete(savePath, true);
        }
    }

    protected void SetInitValues()
    {
        m_DataPath = "";
        m_IncludeTime = true;
        m_IncludeX = m_IncludeY = m_IncludeZ = true;
        m_IncludeLevel = m_IncludeFPS = false;
        m_Rotational = defaultRotational;
        m_MinX = m_MinY = m_MinZ = m_MinDX = m_MinDY = m_MinDZ = defaultMinSpace;
        m_MaxX = m_MaxY = m_MaxZ = defaultMaxSpace;
        m_MinRX = m_MinRY = m_MinRZ = defaultMinAngle;
        m_MaxRX = m_MaxRY = m_MaxRZ = defaultMaxAngle;
        m_MinLevel = defaultMinLevel;
        m_MaxLevel = defaultMaxLevel;
        m_MinFPS = defaultMinFPS;
        m_MaxFPS = defaultMaxFPS;
        string[] eventsList = new string[]{ "PlayerPosition" };
        m_EventNames = new List<string>(eventsList);
        m_CustomEvents = new List<TestCustomEvent>();

        EditorPrefs.SetFloat(k_MinX, m_MinX);
        EditorPrefs.SetFloat(k_MinY, m_MinY);
        EditorPrefs.SetFloat(k_MinZ, m_MinZ);
        EditorPrefs.SetFloat(k_MinDX, m_MinDX);
        EditorPrefs.SetFloat(k_MinDY, m_MinDY);
        EditorPrefs.SetFloat(k_MinDZ, m_MinDZ);
        EditorPrefs.SetFloat(k_MaxX, m_MaxX);
        EditorPrefs.SetFloat(k_MaxY, m_MaxY);
        EditorPrefs.SetFloat(k_MaxZ, m_MaxZ);
        EditorPrefs.SetFloat(k_MaxDX, m_MaxDX);
        EditorPrefs.SetFloat(k_MaxDY, m_MaxDY);
        EditorPrefs.SetFloat(k_MaxDZ, m_MaxDZ);

        EditorPrefs.SetFloat(k_MinRX, m_MinRX);
        EditorPrefs.SetFloat(k_MinRY, m_MinRY);
        EditorPrefs.SetFloat(k_MinRZ, m_MinRZ);
        EditorPrefs.SetFloat(k_MaxRX, m_MaxRX);
        EditorPrefs.SetFloat(k_MaxRY, m_MaxRY);
        EditorPrefs.SetFloat(k_MaxRZ, m_MaxRZ);

        EditorPrefs.SetInt(k_MinLevel, m_MinLevel);
        EditorPrefs.SetInt(k_MaxLevel, m_MaxLevel);
        EditorPrefs.SetFloat(k_MinFPS, m_MinFPS);
        EditorPrefs.SetFloat(k_MaxFPS, m_MaxFPS);
        EditorPrefs.SetString(k_EventNamesKey, eventsList[0]);
        EditorPrefs.SetString(k_CustomEventsKey, eventsList[0]);
        EditorPrefs.SetInt(k_DeviceCountKey, m_DeviceCount);
        EditorPrefs.SetInt(k_SessionCountKey, m_SessionCount);
        EditorPrefs.SetBool(k_Installed, true);
    }

    protected void RestoreValues()
    {
        m_DataPath = EditorPrefs.GetString(k_DataPathKey, m_DataPath);
        m_IncludeTime = EditorPrefs.GetBool(k_IncludeTimeKey, m_IncludeTime);
        m_IncludeX = EditorPrefs.GetBool(k_IncludeXKey, m_IncludeX);
        m_MinX = EditorPrefs.GetFloat(k_MinX, m_MinX);
        m_MaxX = EditorPrefs.GetFloat(k_MaxX, m_MaxX);
        m_IncludeY = EditorPrefs.GetBool(k_IncludeYKey, m_IncludeY);
        m_MinY = EditorPrefs.GetFloat(k_MinY, m_MinY);
        m_MaxY = EditorPrefs.GetFloat(k_MaxY, m_MaxY);
        m_IncludeZ = EditorPrefs.GetBool(k_IncludeZKey, m_IncludeZ);
        m_MinZ = EditorPrefs.GetFloat(k_MinZ, m_MinZ);
        m_MaxZ = EditorPrefs.GetFloat(k_MaxZ, m_MaxZ);

        m_Rotational = EditorPrefs.GetInt(k_RotationKey, m_Rotational);
        m_MinRX = EditorPrefs.GetFloat(k_MinRX, m_MinRX);
        m_MaxRX = EditorPrefs.GetFloat(k_MaxRX, m_MaxRX);
        m_MinRY = EditorPrefs.GetFloat(k_MinRY, m_MinRY);
        m_MaxRY = EditorPrefs.GetFloat(k_MaxRY, m_MaxRY);
        m_MinRZ = EditorPrefs.GetFloat(k_MinRZ, m_MinRZ);
        m_MaxRZ = EditorPrefs.GetFloat(k_MaxRZ, m_MaxRZ);

        m_MinDX = EditorPrefs.GetFloat(k_MinDX, m_MinDX);
        m_MaxDX = EditorPrefs.GetFloat(k_MaxDX, m_MaxDX);
        m_MinDY = EditorPrefs.GetFloat(k_MinDY, m_MinDY);
        m_MaxDY = EditorPrefs.GetFloat(k_MaxDY, m_MaxDY);
        m_MinDZ = EditorPrefs.GetFloat(k_MinDZ, m_MinDZ);
        m_MaxDZ = EditorPrefs.GetFloat(k_MaxDZ, m_MaxDZ);

        m_IncludeLevel = EditorPrefs.GetBool(k_IncludeLevelKey, m_IncludeLevel);
        m_MinLevel = EditorPrefs.GetInt(k_MinLevel, m_MinLevel);
        m_MaxLevel = EditorPrefs.GetInt(k_MaxLevel, m_MaxLevel);

        m_IncludeFPS = EditorPrefs.GetBool(k_IncludeFPSKey, m_IncludeFPS);
        m_MinFPS = EditorPrefs.GetFloat(k_MinFPS, m_MinFPS);
        m_MaxFPS = EditorPrefs.GetFloat(k_MaxFPS, m_MaxFPS);

        m_EventCount = EditorPrefs.GetInt(k_EventCountKey, m_EventCount);
        string loadedEventNames = EditorPrefs.GetString(k_EventNamesKey);
        string[] eventNamesList;
        if (string.IsNullOrEmpty(loadedEventNames))
        {
            eventNamesList = new string[]{ };
        }
        else
        {
            eventNamesList = loadedEventNames.Split('|');
        }
        m_EventNames = new List<string>(eventNamesList);

        string loadedCustomEvents = EditorPrefs.GetString(k_CustomEventsKey);
        m_CustomEvents = new List<TestCustomEvent>();

        string[] customEventsList;
        customEventsList = loadedCustomEvents.Split('\n');
        for (int a = 0; a < customEventsList.Length; a++)
        {
            if (string.IsNullOrEmpty(customEventsList[a]) == false)
            {
                var evt = TestCustomEvent.Parse(customEventsList[a]);
                m_CustomEvents.Add(evt);
            }
        }
    }

    void ViewEventNames()
    {
        string oldEventsString = string.Join("|", m_EventNames.ToArray());
        if (GUILayout.Button(new GUIContent("+ Event Name", "Events to be randomly added into the created data.")))
        {
            m_EventNames.Add("Event name");
        }
        for (var a = 0; a < m_EventNames.Count; a++)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("-", GUILayout.MaxWidth(20f)))
            {
                m_EventNames.RemoveAt(a);
                break;
            }
            m_EventNames[a] = EditorGUILayout.TextField(m_EventNames[a]);
            GUILayout.EndHorizontal();
        }
        string currentEventsString = string.Join("|", m_EventNames.ToArray());

        if (oldEventsString != currentEventsString)
        {
            EditorPrefs.SetString(k_EventNamesKey, currentEventsString);
        }
    }
}
