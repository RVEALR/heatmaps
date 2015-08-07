/// <summary>
/// Heat point.
/// </summary>
/// This struct is used by the rendering system to define each individual
/// heat map datum.

using System;
using UnityEngine;

namespace UnityAnalytics
{
	public struct HeatPoint
	{
		public Vector3 position;
		public float density;
		public float time;
	}
}

