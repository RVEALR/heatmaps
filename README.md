# heatmaps
Heatmaps are an incredibly useful (and rather beautiful) visualization of spatial events. Where do players die? Where do they score kills? Where do they get stuck? By collecting the data from hundreds or thousands of plays, you begin to assemble a large-scale picture of how your users experience your game. The patterns that emerge can help you tune the game and improve things like game economy, difficulty curve, and even good ol’ fashioned fun.

The Unity Analytics heatmap system is built on top of existing Analytics technologies. The heatmap is not itself a product so much as an exploration and demonstration of something that can be built by leveraging Custom Events and the new Data Export feature.

Heatmaps allow the recording, reading and rendering of spatial data via a four-step process.

- Track data using UnityAnalytics.HeatmapEvent.Send
- Retrieve raw event data using Data Export and either the get_raw_events.py script or the Fetch Raw Custom Events section of the Heatmapper inspector
- Aggregate the event data with either the heatmap_aggr.py or the Aggregate Events section of the Heatmapper inspector
- Render the point cloud with the Render section of the Heatmapper inspector

Complete details on using this repo are available here: 
https://docs.google.com/document/d/1ZTTS_GZE7VaQK_y8dgsACX1JTFiWHcQUT-ro3g2DGAk
