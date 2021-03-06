# Introduction
Heatmaps are an incredibly useful visualization of spatial events. While not new, heatmaps have been used a lot in the last 20 years, primarily to make complex data sets more actionable. With the rise of web analytics, heatmaps have been invaluable to map areas where the user spend the most time on a particular page or website. In the context of interactive content, such as games and VR experiences heatmaps can answer a myriad of questions such as

1. Where do players die? 
2. Where do they score kills? 
3. Where do they get stuck? 

By collecting the data from hundreds or thousands of plays, you begin to assemble a large-scale picture of how your 
users experience your game. The patterns that emerge can help you tune the game and improve things like game economy, difficulty curve, and even level design.

![Example Heatmap](https://bytebucket.org/Unity-Technologies/heatmaps/raw/3f60ffb1750ed62c7e2273a8cc7e94ac42db9298/heatmap.png)

The heatmap visualization and data generation system allow the recording, reading and rendering of spatial data via a three-step process.

1. Track data using an analytics package.
2. Fetch and process raw event data using Raw Data Export and the Raw Data Inspector.
3. Render the heatmap with the Heatmapper Inspector. 

## Note
Originally developed by [Unity](https://bitbucket.org/Unity-Technologies/heatmaps), this package is intended to be used with any analytics software as long as the the data can be exported in the format that the heatmap renderer can visualize. Since the code is open source, we encourage other developers write the wrappers for data transformation.

### Quick start, local test data generation
1. Download the installer.
2. Create a new, blank project.
3. Double-click the installer and follow instructions to install this plug-in.
4. Open Window > Unity Analytics > Raw Data.
5. In the Raw Data Inspector, select 'Generate Test Data'. Click the 'Generate' button.
6. Open Window > Unity Analytics > Heatmapper.
7. Open both the 'Data' and 'Render' subpanels.
8. Click 'Process' to see a test heatmap.
9. Explore the other test data options within the Raw Data Inspector. We recommend you click 'Purge' before moving from example to example.

#### Using Unity Analytics as the analytics provider

1. Make sure your project is [signed up](http://response.unity3d.com/analytics-early-access-sign-up) for Heatmaps.
2. Activate Unity Analytics in your project.
3. Install the plugin into your project.
4. Find your Project ID (UPID) and API Key on your Analytics dashboard 'Config' page. Paste these values into the correct fields in the Raw Data Inspector. (There is a button at the bottom of the Raw Data Inspector that will take you to your project's Configuration page.)
5. Set up some Heatmap events in the general form:
`UnityAnalyticsHeatmap.HeatmapEvent.Send(transform, Time.time);    `
6. Test your game using a development build (e.g., the Editor), making sure you send some heatmap events, this can be confirmed by checking the validator window on your project's integration page from the Analytics dashboard. 
7. In the Raw Data Inspector, under 'Fetch Data', create a job.
     * Under 'New Job', pick 'custom' from the pulldown.
     * Select some dates, in the form YYYY-MM-DD. Obviously, these dates should include the day you sent the heatmap events.
     * Hit the 'Create Job' button.
     * Wait for the job to finish processing (may take a few minutes).
     * Click 'Download'.
8. Open Window > Unity Analytics > Heatmapper.
9. Open both the 'Data' and 'Render' subpanels.
10. Click 'Process' to see your heatmap.
11. If you don't see a heatmap, you may need to play with the various settings, including Color, Particle Size, Start Time and End Time. For example, one reason for no data appearing is that the particles are so small that you simply can't make them out.

Please refer to the complete [documentation](https://bitbucket.org/Unity-Technologies/heatmaps/wiki/browse/) for a comprehensive explanation of all features.

Unity Heatmaps is covered under the [MIT/X11 license](https://bitbucket.org/Unity-Technologies/heatmaps/src/d2ca4fd043ad9b3d005423a5ecba81772e0ce9d1/license.txt?at=master&fileviewer=file-view-default).
