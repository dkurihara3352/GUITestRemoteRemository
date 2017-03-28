using UIExample;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(InventoryExample))]
public class InventoryExampleDrawer : PropertyDrawer {

	float verSpace = 5f;
	float horSpace = 5f;
	float labelRatio = .4f;
	float defHeight = EditorGUIUtility.singleLineHeight;
	public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label){
		SerializedObject serializedObj = new SerializedObject((InventoryExample)prop.objectReferenceValue);
		SerializedProperty entriesProp = serializedObj.FindProperty("entries");
		int entriesCount = entriesProp.arraySize;
		
		int indent = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;
			serializedObj.Update();

			if(entriesCount > 0)
			for (int i = 0; i < entriesCount; i++)
			{
				SerializedProperty elementProp = entriesProp.GetArrayElementAtIndex(i);
				DrawEntryElement(pos, entriesCount -1, ref serializedObj, elementProp);
			}
			
			serializedObj.ApplyModifiedProperties();
		EditorGUI.indentLevel = indent;


	}

	public override float GetPropertyHeight(SerializedProperty prop, GUIContent label){

		SerializedObject serializedObj = new SerializedObject((InventoryExample)prop.objectReferenceValue);
		SerializedProperty entriesProp = serializedObj.FindProperty("entries");
		int entriesCount = entriesProp.arraySize;

		if(entriesCount > 1)
			return EditorGUIUtility.singleLineHeight * (entriesCount) + verSpace * (entriesCount -1);
		else 
			return EditorGUIUtility.singleLineHeight;
	}

	void DrawEntryElement(Rect pos, int index, ref SerializedObject so, SerializedProperty sp){
		Rect entryRect = new Rect(pos);
		entryRect.height = defHeight;
		entryRect.y = pos.y + (defHeight + verSpace) *index;
		Rect entryLabelRect = new Rect(entryRect);
		entryLabelRect.width = (entryRect.width - horSpace)* labelRatio;
		Rect entryFieldRect = new Rect(entryRect);
		entryFieldRect.x = pos.x + entryLabelRect.width + horSpace;
		entryFieldRect.width = entryRect.width - entryLabelRect.width;

		so.Update();
		
		SerializedObject elementObj = sp.serializedObject;
		SerializedProperty itemProp = sp.FindPropertyRelative("item");
		SerializedProperty quantityProp = sp.FindPropertyRelative("quantity");
		SerializedObject itemObj = new SerializedObject((InventoryItemExample)itemProp.objectReferenceValue);
		SerializedProperty itemNameProp = itemObj.FindProperty("itemName");
		elementObj.Update();
		itemObj.Update();
		
		EditorGUI.LabelField(entryLabelRect, itemNameProp.stringValue);
		quantityProp.intValue = EditorGUI.IntField(entryFieldRect, quantityProp.intValue);

		itemObj.ApplyModifiedProperties();
		elementObj.ApplyModifiedProperties();
		so.ApplyModifiedProperties();
	}
}
