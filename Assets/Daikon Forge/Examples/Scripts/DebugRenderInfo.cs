using System;
using System.Collections;

using UnityEngine;

public class DebugRenderInfo : MonoBehaviour
{

	public float updateInterval = 0.1F;

	private dfLabel info;
	private dfGUIManager view;

	private float lastUpdate = 0f;
	private int frameCount = 0;

	void Start()
	{

		info = GetComponent<dfLabel>();
		if( info == null )
		{
			this.enabled = false;
			throw new InvalidProgramException( "No Label component found" );
		}

		view = info.GetManager();

	}

	void Update()
	{

		frameCount += 1;

		var elapsed = Time.realtimeSinceStartup - lastUpdate;
		if( elapsed < updateInterval )
			return;

		lastUpdate = Time.realtimeSinceStartup;

		float fps = 1f / ( elapsed / (float)frameCount );

#if UNITY_EDITOR
		var screenSize = view.GetScreenSize();
		var screenSizeFormat = string.Format( "{0}x{1}", (int)screenSize.x, (int)screenSize.y );
#else
		var screenSize = new Vector2( Screen.width, Screen.height );
		var screenSizeFormat = string.Format( "{0}x{1}", (int)screenSize.x, (int)screenSize.y );
#endif

		//var totalControls =
		//    view.GetComponentsInChildren<dfControl>()
		//    .Length;

		var statusFormat = @"Screen : {0}, DrawCalls: {1}, Triangles: {2}, Mem: {3:F0}MB, FPS: {4:F0}";

		var status = string.Format(
			statusFormat,
			screenSizeFormat,
			view.TotalDrawCalls,
			view.TotalTriangles,
			GC.GetTotalMemory( false ) / 1048576f,
			fps
		);

		info.Text = status.Trim();

		frameCount = 0;

	}

}
