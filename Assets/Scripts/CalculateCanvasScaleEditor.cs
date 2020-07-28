#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CalculateCanvasScale))]

public class CalculateCanvasScaleEditor : Editor
{

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		CalculateCanvasScale t = (CalculateCanvasScale)target;
		GUILayout.Label("Расчет масштаба холста:", EditorStyles.boldLabel);
		if (GUILayout.Button("Calculate")) t.Calculate();
	}
}
#endif