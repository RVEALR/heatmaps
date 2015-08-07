/// <summary>
/// Bot death tracker.
/// </summary>
/// Example of a HeatMap event that tracks the death of a Bot in the AngryBots game
/// by firing OnDestroy. Note how we call analyticsEnabled = false when the app
/// quits. This suppresses false positives when the user quits the game.

using UnityEngine;
using UnityAnalytics;

public class BotDeathTracker : MonoBehaviour, IAnalyticsDispatcher
{

    private bool analyticsEnabled = true;

    void OnDestroy()
    {
        if (analyticsEnabled)
        {
			HeatMapEvent.Send("BotKill", transform.position, Time.timeSinceLevelLoad);
        }
    }

    void OnApplicationQuit()
    {
        analyticsEnabled = false;
    }

    public void DisableAnalytics()
    {
        analyticsEnabled = false;
    }

    public void EnableAnalytics()
    {
        analyticsEnabled = true;
    }
}