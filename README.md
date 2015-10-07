# Sign up
http://unity3d.com/services/analytics/heatmaps

# Introduction
Heatmaps are an incredibly useful (and rather beautiful) visualization of spatial events. Where do players die? Where do they score kills? 
Where do they get stuck? By collecting the data from hundreds or thousands of plays, you begin to assemble a large-scale picture of how your 
users experience your game. The patterns that emerge can help you tune the game and improve things like game economy, difficulty curve, and 
even good ol' fashioned fun.

![Example Heatmap](https://bytebucket.org/Unity-Technologies/heatmaps/raw/3f60ffb1750ed62c7e2273a8cc7e94ac42db9298/heatmap.png)

The Unity Analytics heatmap system is built on top of existing Analytics technologies. The heatmap is not itself a product so much as an 
exploration and demonstration of something that can be built by leveraging Custom Events and the new Data Export feature.

Heatmaps allow the recording, reading and rendering of spatial data via a three-step process.

1. Track data using `UnityAnalyticsHeatmap.HeatmapEvent.Send()`
2. Fetch and process raw event data using Data Export and the Aggregate Events section of the Heatmapper inspector.
3. Render the heatmap with the Render section of the Heatmapper inspector.

Steps 2 and 3 occur inside the Heatmapper inspector in Unity.

## 'Quick' start
'Quick' is in quotes, because even at the fastest, you'll need to take some time to generate some Heatmap events.

1. Download or clone this repo.
2. Double-click the installer and follow instructions to install this plug-in.
3. Set up some Heatmap events in the general form:
    
    UnityAnalyticsHeatmap.HeatmapEvent.Send(transform, Time.time);
    
4. Test your game, making sure you send some heatmap events, and WAIT AWHILE (it might take a few hours for the data to start flowing).
5. Copy your raw data url. You'll find this on your project's settings page in the Analytics dashboard.
6. Download, aggregate and render your data:
    * Open the Heatmapper (Window/Heatmapper)
    * Paste your raw data url (from the previous step) into the field "Data Export URL".
    * Click "Fetch and Process".
7. Render:
    * If you don't see a heatmap, you may need to play with the various settings, including Color, Particle Size, Start Time and End Time. For example, one reason for no data appearing is Particle Size == 0.

Please refer to the complete [documentation](https://bitbucket.org/Unity-Technologies/heatmaps/wiki/browse/) for a comprehensive explanation of all features.

Unity Heatmaps is covered under the [MIT/X11 license](https://bitbucket.org/Unity-Technologies/heatmaps/src/d2ca4fd043ad9b3d005423a5ecba81772e0ce9d1/license.txt?at=master&fileviewer=file-view-default).