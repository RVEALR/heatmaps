using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityAnalyticsHeatmap
{
    public class MultiLevelDataStory : MazeDataStory
    {
        int m_Levels = 25;

        public MultiLevelDataStory()
        {
            name = "Maze 1: Multilevel Game";
            genre = "2D Maze Game";
            description = "This demo shows how you can separate events by level. In fact, you can separate by ";
            description += "pretty much ANYTHING.";
            whatToTry = "Generate this data, which shows player position in a 2D maze game. Open the Heatmapper. ";
            whatToTry += "Ensure your space smoothing is 1 and particle size is .75. Uncheck 'time' and click the Process button. ";
            whatToTry += "Under 'Time' set the end time and 'Play Speed' to 1. Click 'Play'.\n\n";
            whatToTry += "Now, there's a LOT of data here and the action looks very messy. That's because what you're seeing is actually ";
            whatToTry += "data from MANY levels on top of each other. Click the 'Separate on Field' button and replace the words ";
            whatToTry += "'Field Name' with 'level' (case matters!). Now Process again. ";
            whatToTry += "Again, set the end time to 1 and press 'Play'. Hey presto! You can see the clear shape ";
            whatToTry += "of players navigating a single maze level. Did you see the 'Option' dropdown appear? Click that dropdown. ";
            whatToTry += "You can now choose to view each level's worth of data individually.\n\n";
            whatToTry += "Under 'Aggregate', uncheck 'Unique Devices' and click Process. Open the 'Option' list to see how it has changed. ";
            whatToTry += "Not only are the levels separated, so are the individual devices. You can use this to see how individual ";
            whatToTry += "players play.";
            sampleCode = "using UnityAnalyticsHeatmap;\n";
            sampleCode += "using System.Collections.Generic;\n\n";
            sampleCode += "// The level and gameTurn variables are examples.\n";
            sampleCode += "// You'll need to maintain a level variable and place the result in the dictionary.\n";
            sampleCode += "// gameTurn points out that the 'time' variable can reflect any numerical value you want.\n";
            sampleCode += "HeatmapEvent.Send(\"PlayerPosition\",transform.position,gameTurn,new Dictionary<string,object>(){{\"level\", level}});";
        }

        override protected Dictionary<double, string> Play()
        {
            var retv = new Dictionary<double, string>();
            m_CurrentFileLines = 0;
            double firstDate = 0d;
            DateTime now = DateTime.UtcNow;
            string data = "";
            int[] position = new int[2]{m_Width/2, m_Height/2};
            int[] lastPosition = new int[2]{m_Width/2, m_Height/2};


            for (int a = 0; a < m_Levels; a++)
            {
                long aSeconds = TimeSpan.TicksPerSecond * m_PlayThroughs * m_EventCount * a;
                Prefill();
                Carve(m_Width/2, m_Height/2, m_Directions["N"], null);

                for (int b = 0; b < m_PlayThroughs; b++)
                {
                    long bSeconds = TimeSpan.TicksPerSecond * b * m_EventCount;
                    m_Route.Add(position);
                    for (int c = 0; c < m_EventCount; c++)
                    {
                        long cSeconds = TimeSpan.TicksPerSecond * c;
                        string evt = "";
                        // Date
                        DateTime dt = now.Subtract(new TimeSpan(aSeconds + bSeconds + cSeconds));
                        string dts = dt.ToString("yyyy-MM-dd hh:mm:ss.ms");
                        evt += dts + "\t";
                        if (m_CurrentFileLines == 0) {
                            firstDate = Math.Round((dt - epoch).TotalSeconds);
                        }

                        if (c == 0) {
                            position = new int[2]{m_Width/2, m_Height/2};
                            lastPosition = new int[2]{m_Width/2, m_Height/2};
                        }

                        // Device ID & name
                        evt += "d" + b + "-XXXX-XXXX\t";
                        evt += m_EventName + "\t";

                        // Build the JSON
                        int[] previousPosition = position.Clone() as int[];
                        position = Move(position, lastPosition);
                        m_Route.Add(position);
                        lastPosition = previousPosition;

                        evt += "{";
                        evt += "\"x\":\"" + position[0] + "\",";
                        evt += "\"y\":\"" + position[1] + "\",";

                        evt += "\"t\":\"" + c + "\",";

                        // simulating an fps drop on the right side of the map
                        float fps = 90f;
                        fps -= Mathf.Abs(10-position[0]) * Mathf.Abs(10-position[1]);

                        evt += "\"fps\":\"" + fps + "\",";
                        evt += "\"level\":\"" + a + "\",";
                        evt += "\"unity.name\":" + "\"" + m_EventName + "\"" + "}\n";

                        data += evt;
                        m_CurrentFileLines ++;
                        if (m_CurrentFileLines >= m_LinesPerFile || c == m_EventCount-1) {
                            retv.Add(firstDate, data);
                            m_CurrentFileLines = 0;
                            data = "";
                        }
                    }
                }
            }
            return retv;
        }
    }
}

