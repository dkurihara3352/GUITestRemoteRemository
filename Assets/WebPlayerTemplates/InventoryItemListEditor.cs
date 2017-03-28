using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace InventorySystem{

	[CustomEditor(typeof(InventoryItemList))]
	public class InventoryItemListEditor : Editor {

		SerializedProperty bowListProp;
		SerializedProperty wearListProp;
		SerializedProperty shieldListProp;
		SerializedProperty meleeWeaponListProp;
		SerializedProperty quiverListProp;
		SerializedProperty packListProp;
		SerializedProperty craftItemListProp;
		
		void OnEnable(){
			bowListProp = serializedObject.FindProperty("bowList");
			wearListProp = serializedObject.FindProperty("wearList");
			shieldListProp = serializedObject.FindProperty("shieldList");
			meleeWeaponListProp = serializedObject.FindProperty("meleeWeaponList");
			quiverListProp = serializedObject.FindProperty("quiverList");
			packListProp = serializedObject.FindProperty("packList");
			craftItemListProp = serializedObject.FindProperty("craftItemList");
		}

		public override void OnInspectorGUI(){
			DrawListProps(bowListProp);
			DrawListProps(wearListProp);
			DrawListProps(shieldListProp);
			DrawListProps(meleeWeaponListProp);
			DrawListProps(quiverListProp);
			DrawListProps(packListProp);
			DrawListProps(craftItemListProp);
		}

		void Expand(SerializedProperty listProp){
			int listCount = listProp.arraySize;
			for (int i = 0; i < listCount; i++)
			{
				SerializedProperty invItemSP = listProp.GetArrayElementAtIndex(i);
				SerializedObject invItemSO = new SerializedObject((InventoryItem)invItemSP.objectReferenceValue);
				SerializedProperty showProp = invItemSO.FindProperty("showInInspector");
				
				invItemSO.Update();
				showProp.boolValue = true;
				invItemSO.ApplyModifiedProperties();
			}
		}

		void Collapse(SerializedProperty listProp){
			int listCount = listProp.arraySize;
			for (int i = 0; i < listCount; i++)
			{
				SerializedProperty invItemSP = listProp.GetArrayElementAtIndex(i);
				SerializedObject invItemSO = new SerializedObject((InventoryItem)invItemSP.objectReferenceValue);
				SerializedProperty showProp = invItemSO.FindProperty("showInInspector");
				
				invItemSO.Update();
				showProp.boolValue = false;
				invItemSO.ApplyModifiedProperties();
			}
		}

		void DrawListProps(SerializedProperty listProp){
			if(listProp.arraySize == 0){
				EditorGUILayout.LabelField("empty");
			}else{
				InventoryItem invItem = (InventoryItem)listProp.GetArrayElementAtIndex(0).objectReferenceValue;
				string typeStr = null;
				if(invItem is Bow) typeStr = "Bow";
				if(invItem is Wear) typeStr = "Wear";
				if(invItem is Shield) typeStr = "Shield";
				if(invItem is MeleeWeapon) typeStr = "MeleeWeapon";
				if(invItem is Quiver) typeStr = "Quiver";
				if(invItem is Pack) typeStr = "Pack";
				if(invItem is CraftItem) typeStr = "CraftItem";
				
				EditorGUILayout.LabelField(typeStr);

			}
		}
	}
}
