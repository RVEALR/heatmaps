using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;

namespace UnityAnalyticsHeatmap
{
    public class FPSDropoffDataStory : DataStory
    {

        public List<List<MazeMapPoint>> m_Map;
        public List<int[]> m_Route;


        int m_Width = 25;
        int m_Height = 25;
        Dictionary<string, MazeDirection> m_Directions = new Dictionary<string, MazeDirection> {
            { "N", new MazeDirection("N", 0, 1, "S") },
            { "S", new MazeDirection("S", 0, -1, "N") },
            { "E", new MazeDirection("E", 1, 0, "W") },
            { "W", new MazeDirection("W", -1, 0, "E") }
        };

        int m_PlayThroughs = 5;
        int m_LinesPerFile = 200;
        int m_CurrentFileLines = 0;
        int m_EventCount = 750;
        string m_EventName = "Heatmap.PlayerPosition";


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
        }

        #region implemented abstract members of DataStory
        public override Dictionary<double, string> Generate()
        {
            Prefill();
            Carve(m_Width/2, m_Height/2, m_Directions["N"], null);
            return Play();
        }
        #endregion

        Dictionary<double, string> Play()
        {
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

        int[] Move(int[] position, int[] lastPosition)
        {
            int[] newPosition = position.Clone() as int[];
            if (position[0] <= 1 || position[0] >= m_Width-1 || position[1] <= 0 || position[1] >= m_Height-1)
            {
                return lastPosition;
            }
            var possibles = new List<string>(){ "N", "S", "E", "W" };
            Shuffle(possibles);
            for (var a = 0; a < possibles.Count; a++) {
                MazeDirection direction = m_Directions[possibles[a]];
                MazeMapPoint pt0 = m_Map[position[0]][position[1]];
                int newX = position[0] + direction.dx;
                int newY = position[1] + direction.dy;
                MazeMapPoint pt1 = m_Map[newX][newY];

                if (lastPosition[0] == pt1.x && lastPosition[1] == pt1.y)
                {
                    continue;
                }
                else if (pt0.exit == direction.direction || pt0.entrance == direction.direction)
                {
                    newPosition[0] = pt1.x;
                    newPosition[1] = pt1.y;
                    break;
                }
            }
            return newPosition;
        }

        void Prefill()
        {
            m_Map =  new List<List<MazeMapPoint>>();
            m_Route = new List<int[]>();
            for (int x = 0; x < m_Width; x++)
            {
                m_Map.Add(new List<MazeMapPoint>());
                for (int y = 0; y < m_Height; y++)
                {
                    m_Map[x].Add(new MazeMapPoint(x, y));
                }
            }
        }

        void Carve(int x0, int y0, MazeDirection direction, MazeDirection lastDirection)
        {
            int x1 = x0 + direction.dx;
            int y1 = y0 + direction.dy;
            if (x1 == 0 || x1 == m_Width || y1 == 0 || y1 == m_Height)
            {
                return;
            }
            if ( m_Map[x1][y1].seen )
            {
                return;
            }
            m_Map[x0][y0].exit = direction.direction;
            m_Map[x1][y1].entrance = direction.opposite;
            m_Map[x1][y1].seen = true;

            var possibles = new List<string>(){ "N", "S", "E", "W" };
            Shuffle(possibles);

            if (lastDirection != null)
            {
                possibles.Remove(lastDirection.direction);
                possibles.Add(lastDirection.direction);
            }

            for (var i = 0; i < possibles.Count; i++) {
                Carve(x1, y1, m_Directions[possibles[i]], direction);
            }
        }

        static System.Random rnd = new System.Random(42);
        public static void Shuffle<T>(IList<T> list) {
            int n = list.Count;
            while (n > 1) {
                int k = (rnd.Next(0, n) % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }

    public class MazeMapPoint
    {
        public bool seen = false;
        public int x = 0;
        public int y = 0;
        public string exit;
        public string entrance;

        public MazeMapPoint(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    class MazeDirection
    {
        public string direction;
        public int dx;
        public int dy;
        public string opposite;

        public MazeDirection(string direction, int dx, int dy, string opposite)
        {
            this.direction = direction;
            this.dx = dx;
            this.dy = dy;
            this.opposite = opposite;
        }
    }
}

