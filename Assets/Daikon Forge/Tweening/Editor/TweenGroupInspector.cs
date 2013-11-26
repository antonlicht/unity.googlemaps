/* Copyright 2013 Daikon Forge */
using UnityEngine;
using UnityEditor;

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

[CustomEditor( typeof( dfTweenGroup ) )]
public class TweenGroupInspector : Editor
{

	public override void OnInspectorGUI()
	{

		var group = target as dfTweenGroup;

		dfEditorUtil.ComponentCopyButton( target );

		EditorGUIUtility.LookLikeControls( 100f );
		EditorGUI.indentLevel += 1;

		GUILayout.Label( "General", "HeaderLabel" );
		{

			var name = EditorGUILayout.TextField( "Name", group.TweenName );
			if( name != group.TweenName )
			{
				dfEditorUtil.MarkUndo( group, "Change Tween Group Name" );
				group.TweenName = name;
			}

			var mode = (dfTweenGroup.TweenGroupMode)EditorGUILayout.EnumPopup( "Mode", group.Mode );
			if( mode != group.Mode )
			{
				dfEditorUtil.MarkUndo( group, "Change Tween Group Mode" );
				group.Mode = mode;
			}

		}

		GUILayout.Label( "Tweens", "HeaderLabel" );
		{

			var tweens = group.Tweens;

			EditorGUI.indentLevel += 1;

			for( int i = 0; i < tweens.Count; i++ )
			{
				GUILayout.BeginHorizontal();
				{

					var component = dfEditorUtil.ComponentField( "Item " + ( i + 1 ), tweens[ i ], typeof( dfTweenPlayableBase ) ) as dfTweenPlayableBase;
					if( component != tweens[ i ] )
					{
						dfEditorUtil.MarkUndo( group, "Add/Remove Tween" );
						tweens[ i ] = component;
					}

					if( GUILayout.Button( "-", "minibutton", GUILayout.Width( 15 ) ) )
					{
						dfEditorUtil.MarkUndo( group, "Add/Remove Tween" );
						tweens.RemoveAt( i );
						break;
					}

				}
				GUILayout.EndHorizontal();
			}

			EditorGUI.indentLevel -= 1;

			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Space( dfEditorUtil.LabelWidth + 5 );
				if( GUILayout.Button( "Add", "minibutton" ) )
				{
					tweens.Add( null );
				}
			}
			EditorGUILayout.EndHorizontal();

		}

		// Show "Play" button when application is playing
		showDebugPlayButton( group );

	}

	private static void showDebugPlayButton( dfTweenPlayableBase tween )
	{

		if( !Application.isPlaying )
			return;

		GUILayout.Label( "Debug", "HeaderLabel" );
		EditorGUILayout.BeginHorizontal();
		{
			GUILayout.Space( dfEditorUtil.LabelWidth + 5 );
			if( GUILayout.Button( "Play", "minibutton" ) )
			{
				tween.Play();
			}
			if( GUILayout.Button( "Stop", "minibutton" ) )
			{
				tween.Stop();
			}
			if( GUILayout.Button( "Reset", "minibutton" ) )
			{
				tween.Reset();
			}
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Separator();

	}

}
