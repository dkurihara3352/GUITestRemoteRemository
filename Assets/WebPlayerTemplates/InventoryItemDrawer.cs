using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace InventorySystem{
	[CustomPropertyDrawer(typeof(InventoryItem))]
	
	public class InventoryItemDrawer : PropertyDrawer {
		protected float defHeight = EditorGUIUtility.singleLineHeight;
		protected float space = 5f;
		protected float bigSpace = 10f;
		protected Color greyCol = new Color(.7f, .7f, .7f);
		// float contentHeight = 

		public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label){

		}

		public override float GetPropertyHeight(SerializedProperty prop, GUIContent label){
			float result = 0;
			SerializedObject invItemSO = new SerializedObject((InventoryItem)prop.objectReferenceValue);
			SerializedProperty showProp = invItemSO.FindProperty("showProp");
			if(!showProp.boolValue){
				result = defHeight;
			}else{
				// result = 
			}
			return result;
		}
	}
}
