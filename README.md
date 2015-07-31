# heatmaps
Heat maps are an incredibly useful (and rather beautiful) visualization of spatial events. Where do players die? Where do they score kills? Where do they get stuck? By collecting the data from hundreds or thousands of plays, you begin to assemble a large-scale picture of how your users experience your game. The patterns that emerge can help you tune the game and improve things like game economy, difficulty curve, and even good ol’ fashioned fun.

The Unity Analytics heat map system is built on top of existing Analytics technologies. The heat map is not itself a product so much as an exploration and demonstration of something that can be built by leveraging Custom Events and the new Data Export feature.

Heat maps allow the recording, reading and rendering of spatial data via a four-step process.

- Track data using UnityAnalytics.HeatMapEvent.Send
- Retrieve raw event data using Data Export and the get_raw_events.py script
- Aggregate the event data with heat_map_aggr.py
- Render the point cloud with HeatMapInspector

Complete details on using this repo are available here: 
https://docs.google.com/document/d/1ZTTS_GZE7VaQK_y8dgsACX1JTFiWHcQUT-ro3g2DGAk
