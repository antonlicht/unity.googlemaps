/* Copyright 2013 Daikon Forge */
using UnityEngine;
using UnityEditor;

using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using Object = UnityEngine.Object;

public class dfEditorUtil
{

	public static Component clipboard;

	private static Queue<Action> actionQueue = null;

	public static void DelayedInvoke( Action callback )
	{

		if( actionQueue == null )
		{

			actionQueue = new Queue<Action>();

			EditorApplication.update += () =>
			{
				while( actionQueue.Count > 0 )
				{
					var action = actionQueue.Dequeue();
					action();
				}
			};

		}

		actionQueue.Enqueue( callback );

	}

	public static float LabelWidth
	{
		get
		{

			var members = typeof( EditorGUIUtility ).GetMember( "labelWidth", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static );
			if( members == null || members.Length != 1 )
				return 75f;

			var member = members[ 0 ];

			if( member is FieldInfo )
			{
				return (float)( (FieldInfo)member ).GetValue( null );
			}

			if( member is PropertyInfo )
			{
				return (float)( (PropertyInfo)member ).GetValue( null, null );
			}

			return 75f;

		}
		set
		{

			var members = typeof( EditorGUIUtility ).GetMember( "labelWidth", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static );
			if( members == null || members.Length != 1 )
				return;

			var member = members[ 0 ];

			if( member is FieldInfo )
			{
				( (FieldInfo)member ).SetValue( null, value );
			}

			if( member is PropertyInfo )
			{
				( (PropertyInfo)member ).SetValue( null, value, null );
			}

		}
	}

	public static void MarkUndo( Object target, string UndoMessage )
	{
		//Debug.Log( UndoMessage );
		Undo.RegisterSceneUndo( UndoMessage );
		EditorUtility.SetDirty( target );
		//SceneView.RepaintAll();
	}

	public static void DrawHorzLine( int height = 2, int padding = 4 )
	{

		GUILayout.Space( padding );

		var savedColor = GUI.color;
		GUI.color = EditorGUIUtility.isProSkin ? new Color( 0.157f, 0.157f, 0.157f ) : new Color( 0.75f, 0.75f, 0.75f );
		GUILayout.Box( "", GUILayout.ExpandWidth( true ), GUILayout.Height( height ) );
		GUI.color = savedColor;

		GUILayout.Space( padding );

	}

	public static void DrawSeparator()
	{

		GUILayout.Space( 12f );
			 
		if( Event.current.type == EventType.Repaint )
		{

			Texture2D tex = EditorGUIUtility.whiteTexture;

			Rect rect = GUILayoutUtility.GetLastRect();

			var savedColor = GUI.color;
			GUI.color = new Color( 0f, 0f, 0f, 0.25f );
			
			GUI.DrawTexture( new Rect( 0f, rect.yMin + 6f, Screen.width, 4f ), tex );
			GUI.DrawTexture( new Rect( 0f, rect.yMin + 6f, Screen.width, 1f ), tex );
			GUI.DrawTexture( new Rect( 0f, rect.yMin + 9f, Screen.width, 1f ), tex );

			GUI.color = savedColor;

		}

	}

	public static void ComponentCopyButton( Object target )
	{

		EditorGUILayout.BeginHorizontal();
		{
			GUILayout.FlexibleSpace();
			if( GUILayout.Button( "Copy", "minibutton" ) )
			{
				dfEditorUtil.clipboard = target as Component;
			}
		}
		EditorGUILayout.EndHorizontal();

	}

	public static Component ComponentField( string label, Component value, Type componentType = null )
	{

		componentType = componentType ?? typeof( MonoBehaviour );

		EditorGUILayout.BeginHorizontal();
		{

			EditorGUILayout.LabelField( label, "", GUILayout.Width( dfEditorUtil.LabelWidth - 10 ) );

			GUILayout.Space( 5 );

			var displayText = value == null ? "[none]" : value.ToString();
			GUILayout.Label( displayText, "TextField", GUILayout.ExpandWidth( true ), GUILayout.MinWidth( 100 ) );

			var evt = Event.current;
			if( evt != null )
			{
				var textRect = GUILayoutUtility.GetLastRect();
				if( evt.type == EventType.mouseDown && evt.clickCount == 2 )
				{
					if( textRect.Contains( evt.mousePosition ) )
					{
						if( GUI.enabled && value != null )
						{
							Selection.activeObject = value;
							EditorGUIUtility.PingObject( value );
							GUIUtility.hotControl = value.GetInstanceID();
						}
					}
				}
				else if( evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform )
				{
					if( textRect.Contains( evt.mousePosition ) )
					{

						var reference = DragAndDrop.objectReferences.First();
						var draggedComponent = (Component)null;
						if( reference is Transform )
						{
							draggedComponent = (Transform)reference;
						}
						else if( reference is GameObject )
						{
							draggedComponent =
								( (GameObject)reference )
								.GetComponents( componentType )
								.FirstOrDefault();
						}
						else if( reference is Component )
						{
							draggedComponent = reference as Component;
							if( draggedComponent == null )
							{
								draggedComponent =
									( (Component)reference )
									.GetComponents( componentType )
									.FirstOrDefault();
							}
						}

						DragAndDrop.visualMode = ( draggedComponent == null ) ? DragAndDropVisualMode.None : DragAndDropVisualMode.Copy;

						if( evt.type == EventType.DragPerform )
						{
							value = draggedComponent;
						}

						evt.Use();

					}
				}
			}

			GUI.enabled = ( clipboard != null );
			{
				var tooltip = ( clipboard != null ) ? string.Format( "Paste {0} ({1})", clipboard.name, clipboard.GetType().Name ) : "";
				var content = new GUIContent( "Paste", tooltip );
				if( GUILayout.Button( content, "minibutton", GUILayout.Width( 50 ) ) )
				{
					value = clipboard;
				}
			}
			GUI.enabled = true;

		}
		EditorGUILayout.EndHorizontal();

		GUILayout.Space( 3 );

		return value;

	}

	public static void DrawTexture( Rect rect, Texture2D texture )
	{

		if( texture == null )
			return;

		var size = new Vector2( texture.width, texture.height );
		var destRect = rect;

		if( destRect.width < size.x || destRect.height < size.y )
		{

			var newHeight = size.y * rect.width / size.x;
			if( newHeight <= rect.height )
				destRect.height = newHeight;
			else
				destRect.width = size.x * rect.height / size.y;

		}
		else
		{
			destRect.width = size.x;
			destRect.height = size.y;
		}

		if( destRect.width < rect.width ) destRect.x = rect.x + ( rect.width - destRect.width ) * 0.5f;
		if( destRect.height < rect.height ) destRect.y = rect.y + ( rect.height - destRect.height ) * 0.5f;

		GUI.DrawTexture( destRect, texture );

	}

	internal static void DrawSprite( Rect rect, dfAtlas atlas, string sprite )
	{
		var spriteInfo = atlas[ sprite ];
		GUI.DrawTextureWithTexCoords( rect, atlas.Material.mainTexture, spriteInfo.region, true );
	}

}
