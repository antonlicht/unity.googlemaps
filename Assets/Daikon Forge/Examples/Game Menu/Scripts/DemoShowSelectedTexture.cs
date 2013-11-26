using UnityEngine;
using System.Collections;

public class DemoShowSelectedTexture : MonoBehaviour
{

	public dfTextureSprite Sprite;

	void OnSelectedItemChanged( DemoListField field )
	{
		if( Sprite != null )
		{
			Sprite.Texture = Resources.Load( field.ItemData[ field.SelectedIndex ] ) as Texture2D;
		}
	}

}
