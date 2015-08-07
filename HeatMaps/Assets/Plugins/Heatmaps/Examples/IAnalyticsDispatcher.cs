/// <summary>
/// A possible interface for any Analytics dispatcher
/// </summary>

namespace UnityAnalytics
{
	public interface IAnalyticsDispatcher
	{

		void DisableAnalytics();

		void EnableAnalytics();
	}
}
