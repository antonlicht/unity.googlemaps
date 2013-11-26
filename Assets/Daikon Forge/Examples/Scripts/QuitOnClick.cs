using UnityEngine;
using System.Collections;

public class QuitOnClick : MonoBehaviour
{

	void OnClick()
	{
#if !UNITY_EDITOR
		Application.Quit();
#endif
	}

}
