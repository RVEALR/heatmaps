# heatmaps
Heatmaps are an incredibly useful (and rather beautiful) visualization of spatial events. Where do players die? Where do they score kills? Where do they get stuck? By collecting the data from hundreds or thousands of plays, you begin to assemble a large-scale picture of how your users experience your game. The patterns that emerge can help you tune the game and improve things like game economy, difficulty curve, and even good ol' fashioned fun.

The Unity Analytics heatmap system is built on top of existing Analytics technologies. The heatmap is not itself a product so much as an exploration and demonstration of something that can be built by leveraging Custom Events and the new Data Export feature.

Heatmaps allow the recording, reading and rendering of spatial data via a four-step process.

- Track data using UnityAnalytics.HeatmapEvent.Send
- Retrieve raw event data using Data Export and either the get_raw_events.py script or the Fetch Raw Custom Events section of the Heatmapper inspector
- Aggregate the event data with either the heatmap_aggr.py or the Aggregate Events section of the Heatmapper inspector
- Render the point cloud with the Render section of the Heatmapper inspector

Complete details on using this repo are available here: 
https://docs.google.com/document/d/1ZTTS_GZE7VaQK_y8dgsACX1JTFiWHcQUT-ro3g2DGAk

## Quick start
1. Download or clone this repo.
2. Copy the directories Heatmaps/Assets/Editor and Heatmaps/Editor/Plugins to your project (obviously take care not to overwrite anything already there).
3. Copy your raw data url. You'll find this on your project's settings page in the Analytics dashboard.
4. Download your raw data:
  - On Mac:
    - In Unity, Select Window > Heatmapper
    - Open the "Fetch Raw Custom Events" subpanel.
    - Paste the URL you copied in step 3.
    - Click "Download".
  - On Windows:
    - Ensure you have Python installed.
    - On a command-line interface, navigate to the file Processing/get_raw_events.py in the downloaded repo.
    - Issue the command `python get_raw_events.py 'URL_YOU_JUST_COPIED'` (note the use of single quotes)
5. Aggregate your raw data:
  - Back in Unity (either platform), open the Heatmapper and select the "Aggregate Events" subpanel.
  - Click "Add File" and navigate to one of the files you just downloaded.
  - Use the "Space Smooth" and "Time Smooth" values to round out the data (these values are divisors).
  - Click "Process". This creates a new JSON file. By default it will be placed in Assets/HeatmapData.
6. Render:
  - Open the "Render" subpanel.
  - Click "Find File" and navigate to the JSON file you created in step 5.
  - Click "Load".
  - You should see a rendering of the heatmap.
  - If you don't see a heatmap, you may need to play with the various settings, including Color, Particle Size, Start Time and End Time. For example, one reason for no data appearing is Particle Size == 0.

Please refer to the complete documentation for a comprehensive explanation of all features.
