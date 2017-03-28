using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AttCurveHolder))]
public class AttCurveHolderEditor : Editor {

	SerializedProperty attCurveProp;
	void OnEnable(){

	}
	public override void OnInspectorGUI(){
		attCurveProp = serializedObject.FindProperty("attCurve");
		serializedObject.Update();
		if(attCurveProp.objectReferenceValue == null)
			EditorGUILayout.ObjectField(attCurveProp);
		else{
			EditorGUILayout.ObjectField(attCurveProp);
			EditorGUILayout.PropertyField(attCurveProp);
		}

		serializedObject.ApplyModifiedProperties();
	}
}
