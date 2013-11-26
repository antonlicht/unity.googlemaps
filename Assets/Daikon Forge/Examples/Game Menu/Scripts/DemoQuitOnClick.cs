using UnityEngine;
using System.Collections;

public class DemoQuitOnClick : MonoBehaviour
{

	void OnClick( dfControl sender, dfMouseEventArgs args )
	{
		Application.Quit();
	}

}
