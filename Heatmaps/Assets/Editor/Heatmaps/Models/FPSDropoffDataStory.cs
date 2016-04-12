using System;
using System.Collections.Generic;

namespace UnityAnalyticsHeatmap
{
    public class FPSDropoffDataStory : DataStory
    {
        public FPSDropoffDataStory()
        {
            name = "FPS Dropoff";
            genre = "3D Maze Game";
            description = "Imagine this data as part of a maze game. Your question: where in my game does my framerate drop? ";
            description += "We're sending an event called PlayerPosition, about once per second." ;
            description += "By sending 'fps' as a parameter, we can chart it and get an idea of slower areas.";
            whatToTry = "Generate this data. In the Heatmapper, first check 'Remap color to field'. In the textfield ";
            whatToTry += "that appears, enter the value 'fps'. In the 'Render' section, check that your shape is 'Cube' ";
            whatToTry += "or 'Square'. Now click the Process button.";
            whatToTry += "Observe how color now represents the places with higher and lower FPS.";
        }

        #region implemented abstract members of DataStory
        public override Dictionary<double, string> Generate()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

