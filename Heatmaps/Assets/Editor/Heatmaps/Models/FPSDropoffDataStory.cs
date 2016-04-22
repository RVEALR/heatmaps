using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;

namespace UnityAnalyticsHeatmap
{
    public class FPSDropoffDataStory : MazeDataStory
    {
        public FPSDropoffDataStory()
        {
            name = "Maze 2: FPS Dropoff";
            genre = "2D Maze Game";
            description = "In the second half of our maze game exploration, your question is: where in my game does my framerate drop? ";
            description += "We're sending the same PlayerPosition event, but this time " ;
            description += "we're sending 'fps' as a parameter. We can chart it and get an idea of where the game might be slowing down.";
            whatToTry = "Generate this data. In the Heatmapper, Process the data. In the 'Render' section, check that your shape is 'Cube' or 'Square'. ";
            whatToTry += "Look at it once. It's like other heatmaps you've seen it before. Now check 'Remap color to field'. In the textfield ";
            whatToTry += "that appears, enter the value 'fps'. Now click the Process button.";
            whatToTry += "Observe how color now represents places with higher and lower FPS. ";
            whatToTry += "Remember that color on the right side of the gradient represents HIGHER density, and since you've re-mapped fps ";
            whatToTry += "the color on the right will display HIGH fps.";
            sampleCode = "using UnityAnalyticsHeatmap;\n";
            sampleCode += "using System.Collections.Generic;\n\n";
            sampleCode += "// The fps and gameTurn variables are examples.\n";
            sampleCode += "// You'll need to calculate fps and place the result in the dictionary.\n";
            sampleCode += "// gameTurn points out that the 'time' variable can reflect any numerical value you want.\n";
            sampleCode += "HeatmapEvent.Send(\"PlayerPosition\",transform.position,gameTurn,new Dictionary<string,object>(){{\"fps\", fps}});";
        }

        override protected Dictionary<double, string> Play()
        {
            Prefill();
            Carve(m_Width/2, m_Height/2, m_Directions["N"], null);

            var retv = new Dictionary<double, string>();
            m_CurrentFileLines = 0;
            double firstDate = 0d;
            DateTime now = DateTime.UtcNow;
            string data = "";
            int[] position = new int[2]{m_Width/2, m_Height/2};
            int[] lastPosition = new int[2]{m_Width/2, m_Height/2};

            for (int a = 0; a < m_PlayThroughs; a++)
            {
                m_Route.Add(position);
                for (int b = 0; b < m_EventCount; b++)
                {
                    string evt = "";
                    // Date
                    DateTime dt = now.Subtract(new TimeSpan((TimeSpan.TicksPerSecond * (m_EventCount - b)) + (TimeSpan.TicksPerSecond*(m_EventCount-a))));
                    string dts = dt.ToString("yyyy-MM-dd hh:mm:ss.ms");
                    evt += dts + "\t";
                    if (m_CurrentFileLines == 0) {
                        firstDate = Math.Round((dt - epoch).TotalSeconds);
                    }

                    if (b == 0) {
                        position = new int[2]{m_Width/2, m_Height/2};
                        lastPosition = new int[2]{m_Width/2, m_Height/2};
                    }

                    // Device ID & name
                    evt += "d" + a + "-XXXX-XXXX\t";
                    evt += m_EventName + "\t";

                    // Build the JSON
                    int[] previousPosition = position.Clone() as int[];
                    position = Move(position, lastPosition);
                    m_Route.Add(position);
                    lastPosition = previousPosition;

                    evt += "{";
                    evt += "\"x\":\"" + position[0] + "\",";
                    evt += "\"y\":\"" + position[1] + "\",";

                    evt += "\"t\":\"" + b + "\",";

                    // simulating an fps drop on the right side of the map
                    float fps = 90f;
                    fps -= Mathf.Abs(10-position[0]) * Mathf.Abs(10-position[1]);

                    evt += "\"fps\":\"" + fps + "\",";
                    evt += "\"unity.name\":" + "\"" + m_EventName + "\"" + "}\n";

                    data += evt;
                    m_CurrentFileLines ++;
                    if (m_CurrentFileLines >= m_LinesPerFile || b == m_EventCount-1) {
                        retv.Add(firstDate, data);
                        m_CurrentFileLines = 0;
                        data = "";
                    }
                }
            }
            return retv;
        }
    }
}

