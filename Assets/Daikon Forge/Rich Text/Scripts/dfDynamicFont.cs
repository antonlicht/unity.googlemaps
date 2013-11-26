/* Copyright 2013 Daikon Forge */

/****************************************************************************
 * PLEASE NOTE: The code in this file is under extremely active development
 * and is likely to change quite frequently. It is not recommended to modify
 * the code in this file, as your changes are likely to be overwritten by
 * the next product update when it is published.
 * **************************************************************************/

using UnityEngine;

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using UnityObject = UnityEngine.Object;
using UnityMaterial = UnityEngine.Material;
using UnityFont = UnityEngine.Font;

[ExecuteInEditMode]
[Serializable]
[AddComponentMenu( "Daikon Forge/User Interface/Dynamic Font" )]
public class dfDynamicFont : dfFontBase
{

	#region Static variables 

	private static List<dfDynamicFont> loadedFonts = new List<dfDynamicFont>();
	private static CharacterInfo[] glyphBuffer = new CharacterInfo[ 1024 ];

	#endregion

	#region Serialized private fields

	[SerializeField]
	private UnityFont baseFont;

	[SerializeField]
	private UnityMaterial material;

	[SerializeField]
	private int baseFontSize = -1;

	[SerializeField]
	private int baseline = -1;

	[SerializeField]
	private int lineHeight = 0;

	#endregion

	#region Private runtime variables 

	private bool invalidatingDependentControls = false;
	private bool wasFontAtlasRebuilt = false;

	#endregion

	#region dfFontBase overrides 

	/// <summary>
	/// Gets or sets the Material that will be used to render text
	/// </summary>
	public override UnityMaterial Material
	{
		get
		{
			this.material.mainTexture = this.baseFont.material.mainTexture;
			return this.material;
		}
		set
		{
			if( value != this.material )
			{
				this.material = value;
				dfGUIManager.RefreshAll();
			}
		}
	}

	/// <summary>
	/// Returns a reference to the texture which contains the
	/// glyph images that will be used to render text
	/// </summary>
	public override Texture Texture
	{
		get { return this.baseFont.material.mainTexture; }
	}

	/// <summary>
	/// Returns a value indicating whether the font configuration is valid
	/// </summary>
	public override bool IsValid
	{
		get
		{
			return
				this.baseFont != null &&
				this.Material != null &&
				this.Texture != null;
		}
	}

	/// <summary>
	/// Gets or sets the base font size that the Font is set to. This value
	/// *must* match the [Font Size] value of the Font, as it is used during
	/// rendering to calculate the vertical offset of each character.
	/// </summary>
	public override int FontSize
	{
		get { return this.baseFontSize; }
		set
		{
			if( value != this.baseFontSize )
			{
				this.baseFontSize = value;
				dfGUIManager.RefreshAll();
			}
		}
	}

	/// <summary>
	/// Gets or sets the default minimum height of each line of rendered text
	/// </summary>
	public override int LineHeight
	{
		get { return this.lineHeight; }
		set
		{
			if( value != this.lineHeight )
			{
				this.lineHeight = value;
				dfGUIManager.RefreshAll();
			}
		}
	}

	public override dfFontRendererBase ObtainRenderer()
	{
		throw new NotImplementedException();
		//return DynamicFontRenderer.Obtain();
	}

	public override int GetKerning( char previousChar, char currentChar )
	{
		// TODO: See if kerning data can be extracted from TTF font files during dynamic font creation
		return 0;
	}

	public override dfFont.GlyphDefinition GetGlyph( char character )
	{

		var glyph = RequestCharacters( character.ToString(), this.FontSize, FontStyle.Normal )[ 0 ];
		var vert = glyph.vert;

		return new dfFont.GlyphDefinition()
		{
			X = 0,
			Y = 0,
			id = character,
			XOffset = (int)vert.x,
			YOffset = (int)Mathf.Abs( vert.y ),
			XAdvance = (int)( vert.x + vert.width ),
			Height = (int)vert.height,
			Width = (int)vert.width,
		};

	}

	#endregion

	#region Public properties

	/// <summary>
	/// Gets or sets the TrueType or OpenType baseFont that will be used to render
	/// any text using this dfDynamicFont
	/// </summary>
	public UnityFont BaseFont
	{
		get { return this.baseFont; }
		set
		{
			if( value != this.baseFont )
			{
				this.baseFont = value;
				dfGUIManager.RefreshAll();
			}
		}
	}

	public int Baseline
	{
		get { return this.baseline; }
		set
		{
			if( value != this.baseline )
			{
				this.baseline = value;
				dfGUIManager.RefreshAll();
			}
		}
	}

	public int Descent
	{
		get { return this.LineHeight - this.baseline; }
	}

	#endregion

	#region Static methods

	public static dfDynamicFont FindByName( string name )
	{

		for( int i = 0; i < loadedFonts.Count; i++ )
		{
			if( string.Compare( loadedFonts[ i ].name, name, true ) == 0 )
				return loadedFonts[ i ];
		}

		var asset = Resources.Load( name ) as GameObject;
		if( asset == null )
		{
			return null;
		}

		var newFont = asset.GetComponent<dfDynamicFont>();
		if( newFont == null )
		{
			return null;
		}

		loadedFonts.Add( newFont );

		return newFont; 

	}

	#endregion

	#region Public methods

	/// <summary>
	/// Returns a Vector2 structure containing the width and height that would be
	/// required to render a single line of text at the given style with the given style
	/// </summary>
	/// <param name="text">The single line of text to measure</param>
	/// <param name="size">The desired font size</param>
	/// <param name="style">The desired text style</param>
	/// <returns></returns>
	public Vector2 MeasureText( string text, int size, FontStyle style )
	{

		var glyphs = RequestCharacters( text, size, style );
		var styleSize = this.FontSize;

		var measuredSize = new Vector2( 0, styleSize * 1.1f );

		for( int i = 0; i < text.Length; i++ )
		{

			var glyph = glyphs[ i ];
			var width = glyph.vert.x + glyph.vert.width;

			if( text[ i ] == ' ' )
			{
				width = Mathf.Max( width, styleSize * 0.25f );
			}
			else if( text[ i ] == '\t' )
			{
				width += styleSize * 4;
			}

			measuredSize.x += width;

		}

		return measuredSize;

	}

	/// <summary>
	/// Ensures that the requested characters are available in the Unity Font's
	/// texture atlas at the given size and style. 
	/// <para><b>IMPORTANT:</b>See notes in source code comments for 
	/// important warnings about the use of this function.</para>
	/// </summary>
	/// <param name="text">The set of characters that will be rendered</param>
	/// <param name="size">The maximum height, in pixels, of the text to be rendered</param>
	/// <param name="style">The style of text that will be rendered</param>
	public CharacterInfo[] RequestCharacters( string text, int size, FontStyle style )
	{

		if( this.baseFont == null )
			throw new NullReferenceException( "Base Font not assigned: " + this.name );

		// NOTES: This function always returns a reference to a pre-existing 
		// static buffer of character data, to reduce memory thrashing and 
		// minimize memory allocations and GC.Collect() occurances. It is 
		// extremely important that calling code only make use of the characters
		// it requested using the text length as the upper array index, rather 
		// than the length of the returned array. This is already done in code
		// that is included with this library; this warning is for third-party
		// developers who wish to use this class.

		ensureGlyphBufferCapacity( size );

		// It's well known that Unity doesn't call the Start() method of a prefab when
		// the game starts, and that prefabs used in the Editor don't have their local
		// instance variable reset when the game starts, but Unity does reset the 
		// static variables of a prefab. This fact can be (ab)used to detect when the 
		// game is started and perform startup initialization. In this case, we want
		// to ensure that we only subscribe to the textureRebuildCallback event once.
		// Note that this fact also works in reverse: Going from play mode back to 
		// edit mode also resets the static variables
		if( !loadedFonts.Contains( this ) )
		{
			this.baseFont.textureRebuildCallback += OnFontChanged;
			loadedFonts.Add( this );
		}

		// Request the characters and retrieve the glyph data
		this.baseFont.RequestCharactersInTexture( text, size, style );
		getGlyphData( glyphBuffer, text, size, style );

		return glyphBuffer;

	}

	#endregion

	#region Private utility methods

	private void onFontAtlasRebuilt()
	{
		wasFontAtlasRebuilt = true;
		OnFontChanged();
	}

	private void OnFontChanged()
	{

		Profiler.BeginSample( "Dynamic font rebuilding..." );
		try
		{

			if( invalidatingDependentControls )
				return;

			dfGUIManager.RenderCallback callback = null;

			callback = ( manager ) =>
			{

				dfGUIManager.AfterRender -= callback;
				invalidatingDependentControls = true;

				try
				{

					Profiler.BeginSample( "Invalidating dynamic font consumers" );

					if( wasFontAtlasRebuilt )
					{
						this.baseFont.characterInfo = null;
					}

					var labels = FindObjectsOfType( typeof( dfControl ) )
						.Where( x => x is dfRichTextLabel )
						.Cast<dfControl>()
						.ToList();

					for( int i = 0; i < labels.Count; i++ )
					{
						labels[ i ].Invalidate();
					}

					if( wasFontAtlasRebuilt )
					{
						manager.Render();
					}

				}
				finally
				{
					wasFontAtlasRebuilt = false;
					invalidatingDependentControls = false;
					Profiler.EndSample();
				}

			};

			dfGUIManager.AfterRender += callback;

		}
		finally
		{
			Profiler.EndSample();
		}

	}

	private void ensureGlyphBufferCapacity( int size )
	{

		var bufferLen = glyphBuffer.Length;

		if( size < bufferLen )
			return;

		while( bufferLen < size )
		{
			bufferLen += 1024;
		}

		glyphBuffer = new CharacterInfo[ bufferLen ];

	}

	private void getGlyphData( CharacterInfo[] result, string text, int size, FontStyle style )
	{

		for( int i = 0; i < text.Length; i++ )
		{
			if( !baseFont.GetCharacterInfo( text[ i ], out result[ i ], size, style ) )
			{
				result[ i ] = new CharacterInfo()
				{
					flipped = false,
					index = -1,
					size = size,
					style = style,
					width = size * 0.25f
				};
			}
		}

	}

	#endregion

	#region Nested classes 


	#endregion

}
