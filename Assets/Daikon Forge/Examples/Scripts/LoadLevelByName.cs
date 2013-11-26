using System;

using UnityEngine;

[Serializable]
public class LoadLevelByName : MonoBehaviour
{

	public string LevelName;

	void OnClick()
	{
		if( !string.IsNullOrEmpty( LevelName ) )
		{
			Application.LoadLevel( LevelName );
		}
	}

}
