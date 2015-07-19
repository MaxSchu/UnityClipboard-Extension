using UnityEngine;
using UnityEditor;
using System.Collections;

public class MyWindow : EditorWindow
	
{
	string myString = "Hello World";
	bool groupEnabled;
	bool myBool = true;
	float myFloat = 1.23f;

	[MenuItem ("Window/My Window")]
	
	public static void  ShowWindow () {
		EditorWindow.GetWindow(typeof(MyWindow));
	}

	void OnGUI () {
		GUILayout.Label ("Base Settings", EditorStyles.boldLabel);
		myString = EditorGUILayout.TextField ("Text Field", myString);
		
		groupEnabled = EditorGUILayout.BeginToggleGroup ("Optional Settings", groupEnabled);
		myBool = EditorGUILayout.Toggle ("Toggle", myBool);
		myFloat = EditorGUILayout.Slider ("Slider", myFloat, -3, 3);
		EditorGUILayout.EndToggleGroup ();
	}
}
