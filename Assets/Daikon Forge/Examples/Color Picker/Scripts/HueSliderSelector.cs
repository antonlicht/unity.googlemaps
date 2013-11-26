using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HueSliderSelector : MonoBehaviour 
{

	private dfSlider slider;

	public Color Hue
	{
		get { return this.hue; }
		set
		{
			if( !Color.Equals( value, hue ) )
			{
				this.hue = value;
				if( slider != null )
				{
					slider.Value = ( 1f - HSBColor.FromColor( value ).h );
				}
			}
		}
	}

	private Color hue;

	public void Start()
	{
		this.slider = GetComponent<dfSlider>();
	}

	public void OnValueChanged( dfControl control, float value )
	{
		this.hue = new HSBColor( 1f - value, 1, 1, 1 ).ToColor();
	}

}
