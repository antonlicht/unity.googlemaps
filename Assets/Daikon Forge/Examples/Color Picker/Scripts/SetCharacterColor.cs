using UnityEngine;
using System.Collections;

public class SetCharacterColor : MonoBehaviour
{

	public SkinnedMeshRenderer CharacterRenderer;

	public Color BeltColor
	{
		get { return CharacterRenderer.material.GetColor( "_TeamColor" ); }
		set { CharacterRenderer.material.SetColor( "_TeamColor", value ); }
	}

}
