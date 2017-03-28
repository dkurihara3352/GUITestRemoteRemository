
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MyUtility;

namespace InventorySystem{
	[System.SerializableAttribute]
	public class InventoryItemEditorWindow : EditorWindow {
		
		/* fields
		*/
			
			public InventoryItemEditorWindowData m_windowData;
			SerializedObject m_windowDataSO;
			SerializedProperty m_windowDataProp;
			SerializedObject m_itemListSO;
			SerializedProperty m_itemListProp;
			string m_scriptableObjectsPath = "Assets/InventorySystem/ScriptableObjects";


			int m_selectedInvListIndex = 0;

			int m_selectedWindDataIndex = 0;

			string m_skinPath = "Assets/InventorySystem/Skins";

			ListId m_selListId;

			enum ListId{
				All,
				BowList,
				WearList,
				ShieldList,
				MeleeWeaponList,
				QuiverList,
				PackList,
				CraftItemList

			}
			SerializedProperty m_listPropToEdit = null;
			
			Vector2 m_scrollPos = Vector2.zero;
			string m_newItemName = null;
			string m_newAssetPath = null;

			


			

		[MenuItem("Window/Inventory Item Editor Window %#w")]
		static void Init(){
			InventoryItemEditorWindow invSysWindow = (InventoryItemEditorWindow)EditorWindow.GetWindow<InventoryItemEditorWindow>(true);
			invSysWindow.Show();
		}	

		void OnEnable(){
			string[] searchPath = new string[]{m_scriptableObjectsPath};
			string[] guids = AssetDatabase.FindAssets("t: InventoryItemEditorWindowData", searchPath);
			if(guids.Length == 0){

				if(EditorPrefs.HasKey("windowDataPath"))
					EditorPrefs.DeleteKey("windowDataPath");

			}else if(EditorPrefs.HasKey("windowDataPath")){

				string path = EditorPrefs.GetString("windowDataPath");
				InventoryItemEditorWindowData windowData = (InventoryItemEditorWindowData)AssetDatabase.LoadAssetAtPath(path,typeof(InventoryItemEditorWindowData));
				m_windowData = windowData;
				m_windowDataSO = new SerializedObject(m_windowData);

				if(m_windowData.invList != null){
					
					m_itemListProp = m_windowDataSO.FindProperty("invList");
					m_itemListSO = new SerializedObject((InventoryItemList)m_itemListProp.objectReferenceValue);
				}
			}
		}

		void OnGUI(){
				EditorGUILayout.LabelField("pos.width: " + position.width + ", position height: "+ position.height);
				GUI.skin = (GUISkin)AssetDatabase.LoadAssetAtPath(m_skinPath + "/CustomEditorSkin.guiskin", typeof(GUISkin));
			GUILayout.BeginHorizontal();
				GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(150f));//Left
					
					DrawTopLeftSection(/*topLeftContRect*/);
					DrawButtomLeftSection();
				GUILayout.EndVertical();

				GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(300f));//middle
					DrawTopMiddleSection();
					DrawButtomMiddleSection();
				GUILayout.EndVertical();

				GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(600f));
					DrawTopRightSection();
					DrawButtomRightSection();
				GUILayout.EndVertical();


			GUILayout.EndHorizontal();
			
		}
		/*	Left
		*/
			/* Top Left Section
			*/
				void DrawTopLeftSection(){
					
					DrawWindowData();		
				}
				
				void DrawWindowData(){
					
						GUILayout.BeginVertical(GUI.skin.box);
							
							string dispStr;
							if(m_windowData == null)
								dispStr = "null";
							else
								dispStr = m_windowData.name.ToString();
							EditorGUILayout.LabelField("windowData: " +  dispStr);

							if(m_windowData == null){
								
								string[] searchPath = new string[]{m_scriptableObjectsPath};
								string[] guids = AssetDatabase.FindAssets("t: InventoryItemEditorWindowData", searchPath);

								if(guids.Length <1){//if there's no window data asset 
									if(GUILayout.Button("Create New WindowData")){
										
										CreateNewInventoryItemEditorWindowData();
									}
								}else{// if there's some, but not assigned
									GUILayout.BeginHorizontal();

										List<string> names = new List<string>();
										for (int i = 0; i < guids.Length; i++)
										{
											string path = AssetDatabase.GUIDToAssetPath(guids[i]);
											InventoryItemEditorWindowData windowData = (InventoryItemEditorWindowData)AssetDatabase.LoadAssetAtPath(path, typeof(InventoryItemEditorWindowData));
											names.Add(windowData.name.ToString());
										}
										string[] namesArray = names.ToArray();
										
										m_selectedWindDataIndex = EditorGUILayout.Popup(m_selectedWindDataIndex, namesArray);
										string pathOfTBA = AssetDatabase.GUIDToAssetPath(guids[m_selectedWindDataIndex]);
										InventoryItemEditorWindowData windowDataToAssign = (InventoryItemEditorWindowData)AssetDatabase.LoadAssetAtPath(pathOfTBA, typeof(InventoryItemEditorWindowData));
										if(GUILayout.Button("Assign", GUILayout.Width(100f))){
											m_windowData = windowDataToAssign;
											m_windowDataSO = new SerializedObject(m_windowData);
											EditorPrefs.SetString("windowDataPath", pathOfTBA);
											
											
										}
									GUILayout.EndHorizontal();
								}
							}else{//if already assigned

								GUILayout.BeginVertical();

									if(GUILayout.Button("Deassign")){
										m_windowData = null;
										m_windowDataSO = null;
										if(EditorPrefs.HasKey("windowDataPath"))
											EditorPrefs.DeleteKey("windowDataPath");
									}

									GUILayout.BeginHorizontal();
										if(GUILayout.Button("Delete Asset")){
											DeleteWindowDataAsset();
										}
									GUILayout.EndHorizontal();

								GUILayout.EndVertical();
							}
							
						GUILayout.EndVertical();

				}
				void DeleteWindowDataAsset(){
					if(m_itemListProp != null)
						DeleteInventoryListAsset();
					string path = AssetDatabase.GetAssetPath(m_windowData);
					m_windowData = null;
					m_windowDataSO = null;
					AssetDatabase.DeleteAsset(path);
					if(EditorPrefs.HasKey("windowDataPath"))
						EditorPrefs.DeleteKey("windowDataPath");
				}
			/* Buttom Left
			*/
				void DrawButtomLeftSection(){
					GUILayout.BeginVertical(GUI.skin.box);
						
						DrawInvListAssetManager();
			
						DrawInventoryListContent();

					GUILayout.EndVertical();
				
				}

				void DrawInvListAssetManager(){
					GUILayout.BeginVertical(GUI.skin.box);
						if(m_windowData == null || m_windowDataSO == null){
							EditorGUILayout.HelpBox("There's no InventoryItemEditorWindowData assigned. \n Create and/or assign it first.", MessageType.Warning);
						}else{

							m_windowDataSO.Update();

							
							GUILayout.BeginVertical();

								string dispStr = null;
								if(m_windowData.invList == null)
									dispStr = "null";
								else dispStr = m_windowData.invList.name.ToString();
								EditorGUILayout.LabelField("invList: " +  dispStr);

								if(m_windowData.invList == null){
									string[] searchPath = new string[]{m_scriptableObjectsPath};
									string[] guids = AssetDatabase.FindAssets("t: InventoryItemList", searchPath);
									if(guids.Length <1){//if none in the asset
										if(GUILayout.Button("Create New")){
											CreateInventoryItemList();
										}
									}else{//there is, but not assigned

										GUILayout.BeginHorizontal();
											
											List<string> names = new List<string>();
											for (int i = 0; i < guids.Length; i++)
											{
												string path = AssetDatabase.GUIDToAssetPath(guids[i]);
												InventoryItemList invList = (InventoryItemList)AssetDatabase.LoadAssetAtPath(path, typeof(InventoryItemList));
												names.Add(invList.name.ToString());
											}
											string[] namesArray = names.ToArray();

											m_selectedInvListIndex = EditorGUILayout.Popup(m_selectedInvListIndex, namesArray);
											string pathOfTBA = AssetDatabase.GUIDToAssetPath(guids[m_selectedInvListIndex]);
											InventoryItemList invListToAssign = (InventoryItemList)AssetDatabase.LoadAssetAtPath(pathOfTBA, typeof(InventoryItemList));

											if(GUILayout.Button("Assign")){

												m_windowDataSO.FindProperty("invList").objectReferenceValue = invListToAssign;
												m_itemListProp = m_windowDataSO.FindProperty("invList");
												m_itemListSO = new SerializedObject((InventoryItemList)m_itemListProp.objectReferenceValue);

											}

										GUILayout.EndHorizontal();
									}
								}else{
									GUILayout.BeginVertical();
										
										if(GUILayout.Button("Deassign")){
											m_windowDataSO.FindProperty("invList").objectReferenceValue = null;
											m_itemListProp = null;
											m_itemListSO = null;
										}

										if(GUILayout.Button("Delete Asset")){
											DeleteInventoryListAsset();
										}
									GUILayout.EndVertical();
								}
								m_windowDataSO.ApplyModifiedProperties();
							GUILayout.EndVertical();
						}
					GUILayout.EndVertical();
				}
				
				int deleteIndex = -1;
				InventoryItem itemToEdit;
				
				void DrawInventoryListContent(){
		
					GUILayout.BeginVertical(GUI.skin.box);
						
						DrawListSelector();

						if(m_listPropToEdit != null){
							SerializedObject listPropToEditSO = m_listPropToEdit.serializedObject;
							
							listPropToEditSO.Update();
							if(m_windowData != null){
								if(m_windowData.invList != null){
									if(m_listPropToEdit != null){
										
										DrawItemCreator();

										DrawListElements();

									}else{
										EditorGUILayout.HelpBox("listPropToEdit not assigned properly", MessageType.Error);
									}
								}else{
									EditorGUILayout.HelpBox("Create or assign InventoryItemList first", MessageType.Warning);
								}
							}else{
								EditorGUILayout.HelpBox("Create or assign InventoryItemEditorWindowData first", MessageType.Warning);
							}
							
							listPropToEditSO.ApplyModifiedProperties();
							
						}else{
							EditorGUILayout.HelpBox("select inventory list to edit", MessageType.Warning);	
						}	
					GUILayout.EndVertical();		
				}

				void DrawListSelector(){
					if(m_windowData == null || m_windowDataSO ==null){
						EditorGUILayout.HelpBox("Create and/or asssign windowData first", MessageType.Warning);
					}else{
						if(m_windowData.invList == null || m_itemListSO == null){
							EditorGUILayout.HelpBox("Create and/or assign inventoryList first", MessageType.Warning);
						}else{
							
							m_windowDataSO.Update();
							
							if(m_windowData.invList == null){
								EditorGUILayout.HelpBox("Create or Assign InventoryItemList first.", MessageType.Warning);
							}else{
								m_selListId = (ListId)EditorGUILayout.EnumPopup(m_selListId);
							
								
								switch(m_selListId){
									case ListId.All:
										m_listPropToEdit = m_itemListSO.FindProperty("allItemsList");
										break;
									case ListId.BowList:
										m_listPropToEdit = m_itemListSO.FindProperty("bowList");
										break;
									case ListId.WearList:
										m_listPropToEdit = m_itemListSO.FindProperty("wearList");
										break;
									case ListId.ShieldList:
										m_listPropToEdit = m_itemListSO.FindProperty("shieldList");
										break;
									case ListId.MeleeWeaponList:
										m_listPropToEdit = m_itemListSO.FindProperty("meleeWeaponList");
										break;
									case ListId.QuiverList:
										m_listPropToEdit = m_itemListSO.FindProperty("quiverList");
										break;
									case ListId.PackList:
										m_listPropToEdit = m_itemListSO.FindProperty("packList");
										break;
									case ListId.CraftItemList:
										m_listPropToEdit = m_itemListSO.FindProperty("craftItemList");
										break;

								}
							}
						}
					}
				}

				void DrawItemCreator(){
					GUILayout.BeginVertical();
						GUILayout.BeginHorizontal();

							EditorGUILayout.LabelField("new item name", GUILayout.Width(90f));
							m_newItemName = EditorGUILayout.TextField(m_newItemName, GUILayout.Width(150f));

						GUILayout.EndHorizontal();
						
						GUI.enabled = !(m_selListId == ListId.All);

						if(GUILayout.Button("Create and Add")){
							m_newAssetPath = CreatedAssetPath();//from newItemName and selListId create a new asset and return path to the asset
							if(m_newAssetPath == null || m_newAssetPath == string.Empty){
								EditorGUILayout.HelpBox("couldn't create a new asset, might be invalid name", MessageType.Error);
							}else{
								m_listPropToEdit.InsertArrayElementAtIndex(m_listPropToEdit.arraySize);
								
								InventoryItem invItem = (InventoryItem)AssetDatabase.LoadAssetAtPath(m_newAssetPath, typeof(InventoryItem));

								itemToEdit = invItem;
								
								m_listPropToEdit.GetArrayElementAtIndex(m_listPropToEdit.arraySize -1).objectReferenceValue = invItem;
								UpdateAllItemsList();
								UpdateItemIds(m_selListId);
								ReorderList(ref m_listPropToEdit);
							}
						}
						GUI.enabled = true;
					GUILayout.EndVertical();
				}

				void DrawListElements(){
					if(m_listPropToEdit.arraySize == 0)
						GUILayout.Box("array size 0", GUILayout.Height(200f));
					else{
						m_scrollPos = GUILayout.BeginScrollView(m_scrollPos);
							GUILayout.BeginVertical(GUI.skin.box);
								for (int i = 0; i < m_listPropToEdit.arraySize; i++)
								{
									SerializedProperty invItemProp = m_listPropToEdit.GetArrayElementAtIndex(i);
									if(invItemProp.objectReferenceValue != null){

										SerializedObject invItemSO = new SerializedObject(invItemProp.objectReferenceValue);
										
										GUILayout.BeginHorizontal(GUI.skin.textField);
											
											Sprite sprite = (Sprite)invItemSO.FindProperty("itemSprite").objectReferenceValue;
											if(sprite != null)
												MyEditorWindowUtility.DrawOnGUISprite(sprite, .05f);

											string itemNameStr = invItemSO.FindProperty("itemName").stringValue;
											EditorGUILayout.LabelField(itemNameStr, GUILayout.Width(80f));
											
											GUILayout.FlexibleSpace();

											
											if(GUILayout.Button("Edit", m_selListId != ListId.All?GUILayout.Width(50f): GUILayout.Width(100f))){
											
												if(invItemProp.objectReferenceValue is Bow){
													itemToEdit = (Bow)invItemProp.objectReferenceValue;
												}
												else if(invItemProp.objectReferenceValue is Wear){
													itemToEdit = (Wear)invItemProp.objectReferenceValue;
												}
												else if(invItemProp.objectReferenceValue is Shield){
													itemToEdit = (Shield)invItemProp.objectReferenceValue;
												}
												else if(invItemProp.objectReferenceValue is MeleeWeapon){
													itemToEdit = (MeleeWeapon)invItemProp.objectReferenceValue;
												}
												else if(invItemProp.objectReferenceValue is Quiver){
													itemToEdit = (Quiver)invItemProp.objectReferenceValue;
												}
												else if(invItemProp.objectReferenceValue is Pack){
													itemToEdit = (Pack)invItemProp.objectReferenceValue;
												}
												else if(invItemProp.objectReferenceValue is CraftItem){
													itemToEdit = (CraftItem)invItemProp.objectReferenceValue;
												}
											}
											if(m_selListId != ListId.All){
												if(GUILayout.Button("Delete", GUILayout.Width(50f))){
													deleteIndex = i;		
												}
											}

										GUILayout.EndHorizontal();
										
									}
								}
								if(deleteIndex != -1){
									if(m_selListId != ListId.All){
										string path = AssetDatabase.GetAssetPath(m_listPropToEdit.GetArrayElementAtIndex(deleteIndex).objectReferenceValue);
										InventoryItem invItemToDelete = (InventoryItem)m_listPropToEdit.GetArrayElementAtIndex(deleteIndex).objectReferenceValue;
										m_listPropToEdit.GetArrayElementAtIndex(deleteIndex).objectReferenceValue = null;
										m_listPropToEdit.DeleteArrayElementAtIndex(deleteIndex);
										AssetDatabase.DeleteAsset(path);
										
										if(invItemToDelete is Bow)
											UpdateItemIds(ListId.BowList);
										else if(invItemToDelete is Wear)
											UpdateItemIds(ListId.WearList);
										else if(invItemToDelete is Shield)
											UpdateItemIds(ListId.ShieldList);
										else if(invItemToDelete is MeleeWeapon)
											UpdateItemIds(ListId.MeleeWeaponList);
										else if(invItemToDelete is Quiver)
											UpdateItemIds(ListId.QuiverList);
										else if(invItemToDelete is Pack)
											UpdateItemIds(ListId.PackList);
										else if(invItemToDelete is CraftItem)
											UpdateItemIds(ListId.CraftItemList);
										
										invItemToDelete = null;

										deleteIndex = -1;
										itemToEdit = null;
										UpdateAllItemsList();
										ReorderList(ref m_listPropToEdit);
									}
								}
							GUILayout.EndVertical();
						GUILayout.EndScrollView();
					}
				}
				
				void ReorderList(ref SerializedProperty listProp){
					
					int origSize = listProp.arraySize;
					List<InventoryItem> temp = new List<InventoryItem>();
					
					int idToDelete = -1;
					while(temp.Count < origSize){
						InventoryItem itemWithMinId = null;
						for (int i = 0; i < listProp.arraySize; i++)
						{
							InventoryItem invItem = (InventoryItem)listProp.GetArrayElementAtIndex(i).objectReferenceValue;

							if(itemWithMinId == null || invItem.itemId < itemWithMinId.itemId){
								itemWithMinId = invItem;
								idToDelete = i;
							}
						}
						temp.Add(itemWithMinId);
						listProp.GetArrayElementAtIndex(idToDelete).objectReferenceValue = null;
						listProp.DeleteArrayElementAtIndex(idToDelete);
					}

					while(listProp.arraySize> 0){
						listProp.DeleteArrayElementAtIndex(0);
					}
					for (int i = 0; i < temp.Count; i++)
					{
						listProp.InsertArrayElementAtIndex(i);
						listProp.GetArrayElementAtIndex(i).objectReferenceValue = temp[i];
					}

				}
				

			
		
		/*	Middle
		*/
			/*	TopMiddle
			*/
				void DrawTopMiddleSection(){
					DrawItemEditor();
				}
				
				
				Vector2 m_dismIngScrollPos;
				int m_dismSelIndex;
				bool m_showDism = true;

				
				Vector2 m_crafIngScrollPos;
				
				int m_crafSelIndex;
				bool m_showCraf = true;
				
				bool m_editDismantleToNames;
				bool m_editCraftedFromNames;
				bool m_isIdEditable = false;

				ItemTier m_tierToAssign;

				void DrawItemEditor(){
					if(itemToEdit != null){
						SerializedObject itemSO = new SerializedObject(itemToEdit);
						SerializedProperty itemNameProp = itemSO.FindProperty("itemName");
						SerializedProperty itemSpriteProp = itemSO.FindProperty("itemSprite");
						SerializedProperty itemIdProp = itemSO.FindProperty("itemId");
						SerializedProperty isUnlockedProp = itemSO.FindProperty("isUnlocked");
						
						SerializedProperty tierProp = itemSO.FindProperty("tier");
						SerializedProperty isStackableProp = itemSO.FindProperty("isStackable");
						
						itemSO.Update();
						
						GUILayout.BeginVertical(GUI.skin.box);

							EditorGUILayout.LabelField("Edit Item: " + itemNameProp.stringValue, EditorStyles.boldLabel);
							GUILayout.BeginHorizontal();
							EditorGUILayout.LabelField("item name", GUILayout.Width(70f));
							itemNameProp.stringValue = EditorGUILayout.TextField(itemNameProp.stringValue);
							GUILayout.EndHorizontal();
							
							GUILayout.BeginHorizontal();
								GUILayout.BeginVertical();

									GUILayout.BeginHorizontal();
										EditorGUILayout.LabelField("item sprite", GUILayout.Width(70f));
										itemSpriteProp.objectReferenceValue = (Sprite)EditorGUILayout.ObjectField(itemSpriteProp.objectReferenceValue, typeof(Sprite), false);
									GUILayout.EndHorizontal();
									
									GUILayout.BeginHorizontal();
										
										if(!m_isIdEditable){
											GUILayout.BeginHorizontal();
											EditorGUILayout.LabelField("item index", GUILayout.Width(80f));
											
											EditorGUILayout.LabelField(itemIdProp.intValue.ToString(), GUILayout.Width(50f));
											GUILayout.EndHorizontal();
										}
											
										else{
											GUILayout.BeginHorizontal();
												EditorGUILayout.LabelField("item index", GUILayout.Width(80f));
												
												itemIdProp.intValue = EditorGUILayout.IntField(itemIdProp.intValue, GUILayout.Width(50f));
											GUILayout.EndHorizontal();
											
										}

										GUILayout.FlexibleSpace();
										
										if(GUILayout.Button(m_isIdEditable? "Done": "Edit", GUILayout.Width(50f))){
											m_isIdEditable = !m_isIdEditable;
										}
									GUILayout.EndHorizontal();

									EditorGUILayout.LabelField("isUnlocked: " + isUnlockedProp.boolValue.ToString());

									GUILayout.BeginHorizontal();
										m_tierToAssign = (ItemTier)EditorGUILayout.EnumPopup(m_tierToAssign);
										if(GUILayout.Button("Assign", GUILayout.Width(70f)))
											tierProp.enumValueIndex = (int)m_tierToAssign;
									GUILayout.EndHorizontal();
									
									EditorGUILayout.LabelField("tier", tierProp.enumDisplayNames[tierProp.enumValueIndex]);
									isStackableProp.boolValue = EditorGUILayout.Toggle("is stackable", isStackableProp.boolValue);
									

								GUILayout.EndVertical();
								
								
								if(itemSpriteProp.objectReferenceValue != null)
								MyEditorWindowUtility.DrawOnGUISprite((Sprite)itemSpriteProp.objectReferenceValue, .3f);
							
							GUILayout.EndHorizontal();
							
							if(itemToEdit is EquipableGear){
								GUILayout.BeginVertical(GUI.skin.box);
									
									// SerializedProperty isFavoriteProp = itemSO.FindProperty("isFavorite");
									// SerializedProperty gearLevelProp = itemSO.FindProperty("gearLevel");
									// SerializedProperty isEquippedProp = itemSO.FindProperty("isEquipped");
									SerializedProperty maxLevelProp = itemSO.FindProperty("maxLevel");
									
									// EditorGUILayout.LabelField("is equipped: " + isEquippedProp.boolValue.ToString());
									// EditorGUILayout.LabelField("is favorite: " + isFavoriteProp.boolValue.ToString());
									
									maxLevelProp.intValue = EditorGUILayout.IntField("max level", maxLevelProp.intValue);
									// gearLevelProp.intValue = EditorGUILayout.IntField("gear level", gearLevelProp.intValue);

									GUILayout.BeginVertical(GUI.skin.box);
									
										SerializedProperty dismantleToProp = itemSO.FindProperty("dismantleTo");
										SerializedProperty craftedFromProp = itemSO.FindProperty("craftedFrom");
										DrawDismantleTo(dismantleToProp, ref m_editDismantleToNames);	
										DrawCraftedFrom(craftedFromProp, ref m_editCraftedFromNames);
									
									GUILayout.EndVertical();

								
								GUILayout.EndVertical();	
							}		

						GUILayout.EndVertical();

						itemSO.ApplyModifiedProperties();
					}else{
						EditorGUILayout.HelpBox("Select item to edit first. It is currently set to null", MessageType.Info);
					}
				}

				void DrawDismantleTo(SerializedProperty dismantleToProp, ref bool edit){
					GUILayout.BeginVertical(GUI.skin.box);	
						m_showDism = EditorGUILayout.Foldout(m_showDism, "Dismantle To", true);

						if(m_showDism){
							
							if(GUILayout.Button(edit?"Done Edit":"Edit Name")){
								edit = !edit;
							}
							SerializedProperty dismIngNameProp = dismantleToProp.FindPropertyRelative("ingredientsName");
							
							if(edit){
								GUILayout.BeginHorizontal();
									EditorGUILayout.LabelField("ing name: ", GUILayout.Width(80f));
									GUILayout.FlexibleSpace();
									dismIngNameProp.stringValue = EditorGUILayout.TextField(dismIngNameProp.stringValue, GUILayout.Width(150f));
								GUILayout.EndHorizontal();
							}
							else	
								
								EditorGUILayout.LabelField(dismIngNameProp.stringValue);
								
							

							SerializedProperty elementsProp = dismantleToProp.FindPropertyRelative("elements");
							EditorGUILayout.LabelField("arraySize: " + elementsProp.arraySize.ToString());
							GUILayout.BeginHorizontal();
							
								List<string> namesList = new List<string>();
								SerializedProperty allItemsListProp = m_itemListSO.FindProperty("allItemsList");
								for (int j = 0; j < allItemsListProp.arraySize; j++)
								{
									InventoryItem invItem = (InventoryItem)allItemsListProp.GetArrayElementAtIndex(j).objectReferenceValue;
									namesList.Add(invItem.itemName);
								}
								string[] namesArray = namesList.ToArray();
								m_dismSelIndex = EditorGUILayout.Popup(m_dismSelIndex, namesArray);
								InventoryItem invItemToAdd = (InventoryItem)allItemsListProp.GetArrayElementAtIndex(m_dismSelIndex).objectReferenceValue;
							
								if(GUILayout.Button("Add")){
									
									elementsProp.InsertArrayElementAtIndex(elementsProp.arraySize);
									elementsProp.GetArrayElementAtIndex(elementsProp.arraySize -1).FindPropertyRelative("inventoryItem").objectReferenceValue = invItemToAdd;
								}
							
							GUILayout.EndHorizontal();

							m_dismIngScrollPos = GUILayout.BeginScrollView(m_dismIngScrollPos);
								for (int i = 0; i < elementsProp.arraySize; i++)
								{
									
									GUILayout.BeginHorizontal(GUI.skin.textField);
									
										SerializedObject invItemSO = new SerializedObject((InventoryItem)elementsProp.GetArrayElementAtIndex(i).FindPropertyRelative("inventoryItem").objectReferenceValue);
										invItemSO.Update();
										
										Sprite sprite = (Sprite)invItemSO.FindProperty("itemSprite").objectReferenceValue;
										if(sprite != null)
											MyEditorWindowUtility.DrawOnGUISprite(sprite, .05f);
										
										SerializedProperty itemNameProp = invItemSO.FindProperty("itemName");
										EditorGUILayout.LabelField(itemNameProp.stringValue, GUILayout.Width(100f));
									
										GUILayout.FlexibleSpace();
										
										EditorGUILayout.LabelField("quantity", GUILayout.Width(50f));
										SerializedProperty quantProp = elementsProp.GetArrayElementAtIndex(i).FindPropertyRelative("quantity");
										quantProp.intValue = EditorGUILayout.IntField(quantProp.intValue, GUILayout.Width(30f));
										
										if(GUILayout.Button("Deassign", GUILayout.Width(60f))){
											elementsProp.DeleteArrayElementAtIndex(i);
										}
										invItemSO.ApplyModifiedProperties();

									GUILayout.EndHorizontal();
									
								}
							GUILayout.EndScrollView();
						}
							
					GUILayout.EndVertical();
				}
				void DrawCraftedFrom(SerializedProperty craftedFromProp, ref bool edit){
					GUILayout.BeginVertical(GUI.skin.box);	
						m_showCraf = EditorGUILayout.Foldout(m_showCraf, "Crafted From", true);

						if(m_showCraf){

							if(GUILayout.Button(edit? "Done Edit": "Edit Name")){
								edit = !edit;
							}

							SerializedProperty crafIngNameProp = craftedFromProp.FindPropertyRelative("ingredientsName");
							
						
							if(edit){
								GUILayout.BeginHorizontal();
									EditorGUILayout.LabelField("ing name: ", GUILayout.Width(80f));
									GUILayout.FlexibleSpace();
									crafIngNameProp.stringValue = EditorGUILayout.TextField(crafIngNameProp.stringValue, GUILayout.Width(150f));
								GUILayout.EndHorizontal();
							}
							else

								EditorGUILayout.LabelField(crafIngNameProp.stringValue);
								
							

							SerializedProperty elementsProp = craftedFromProp.FindPropertyRelative("elements");

							EditorGUILayout.LabelField("arraySize: " + elementsProp.arraySize.ToString());
							GUILayout.BeginHorizontal();
							
								List<string> namesList = new List<string>();
								SerializedProperty allItemsListProp = m_itemListSO.FindProperty("allItemsList");
								for (int j = 0; j < allItemsListProp.arraySize; j++)
								{
									InventoryItem invItem = (InventoryItem)allItemsListProp.GetArrayElementAtIndex(j).objectReferenceValue;
									namesList.Add(invItem.itemName);
								}
								string[] namesArray = namesList.ToArray();
								m_crafSelIndex = EditorGUILayout.Popup(m_crafSelIndex, namesArray);
								InventoryItem invItemToAdd = (InventoryItem)allItemsListProp.GetArrayElementAtIndex(m_crafSelIndex).objectReferenceValue;
							
								if(GUILayout.Button("Add")){
									
									elementsProp.InsertArrayElementAtIndex(elementsProp.arraySize);
									elementsProp.GetArrayElementAtIndex(elementsProp.arraySize -1).FindPropertyRelative("inventoryItem").objectReferenceValue = invItemToAdd;
								}
							
							GUILayout.EndHorizontal();

							m_dismIngScrollPos = GUILayout.BeginScrollView(m_dismIngScrollPos);
								for (int i = 0; i < elementsProp.arraySize; i++)
								{
									
									GUILayout.BeginHorizontal(GUI.skin.textField);
									
										SerializedObject invItemSO = new SerializedObject((InventoryItem)elementsProp.GetArrayElementAtIndex(i).FindPropertyRelative("inventoryItem").objectReferenceValue);
										invItemSO.Update();
										
										Sprite sprite = (Sprite)invItemSO.FindProperty("itemSprite").objectReferenceValue;
										if(sprite != null)
											MyEditorWindowUtility.DrawOnGUISprite(sprite, .05f);

										SerializedProperty itemNameProp = invItemSO.FindProperty("itemName");
										EditorGUILayout.LabelField(itemNameProp.stringValue, GUILayout.Width(100f));
									
										GUILayout.FlexibleSpace();
										
										EditorGUILayout.LabelField("quantity", GUILayout.Width(50f));
										SerializedProperty quantProp = elementsProp.GetArrayElementAtIndex(i).FindPropertyRelative("quantity");
										quantProp.intValue = EditorGUILayout.IntField(quantProp.intValue, GUILayout.Width(30f));
										
										if(GUILayout.Button("Deassign", GUILayout.Width(60f))){
											elementsProp.DeleteArrayElementAtIndex(i);
										}
										invItemSO.ApplyModifiedProperties();

									GUILayout.EndHorizontal();
									
								}
							GUILayout.EndScrollView();
						}
					GUILayout.EndVertical();
				}
				

			/*	ButtomMiddle
			*/
				void DrawButtomMiddleSection(){
					// GUILayout.Box(GUIContent.none, GUILayout.Width(300f), GUILayout.Height(300f));
					
				}
		
		/*	Right
		*/
		
			/*	TopRight
			*/
				void DrawTopRightSection(){
					if(itemToEdit != null){

						SerializedObject itemSO = new SerializedObject(itemToEdit);
							itemSO.Update();
						if(itemToEdit is Bow){
							DrawBowEditor(itemSO);
						}
						else if(itemToEdit is Wear){
							DrawWearEditor(itemSO);
						}
						else if(itemToEdit is Shield){
							DrawShieldEditor(itemSO);
						}
						else if(itemToEdit is MeleeWeapon){
							DrawMeleeWeaponEditor(itemSO);
						}
						else if(itemToEdit is Quiver){
							DrawQuiverEditor(itemSO);
						}
						else if(itemToEdit is Pack){
							DrawPackEditor(itemSO);
						}
						else if(itemToEdit is CraftItem){
							DrawCraftItemEditor(itemSO);
						}
						
						itemSO.ApplyModifiedProperties();
					}else{
						EditorGUILayout.HelpBox("item to edit is set to null", MessageType.Info);
					}
				}
				/*	bowEditor fields
				*/
					bool showDrawProfile = true;
					bool showDrawStrength = true;
					bool showHandling = true;
					bool showSpecialEffect = true;
					bool editDrawProfile;
					bool editDrawStrength;
					bool editHandling;
					bool editSpecialEffect;
				void DrawBowEditor(SerializedObject itemSO){
					Bow bow = (Bow)itemSO.targetObject;
					SerializedProperty drawProfileProp = itemSO.FindProperty("drawProfile");
					AttributeCurve drawProfileAC = bow.drawProfile;
					SerializedProperty drawStrengthProp = itemSO.FindProperty("drawStrength");
					AttributeCurve drawStrengthAC = bow.drawStrength;
					SerializedProperty handlingProp = itemSO.FindProperty("handling");
					AttributeCurve handlingAC = bow.handling;
					SerializedProperty specialEffectProp = itemSO.FindProperty("specialEffect");
					AttributeCurve specialEffectAC = bow.specialEffect;
					GUILayout.BeginVertical(GUI.skin.box);
						EditorGUILayout.LabelField("Bow Attribute Curves");

						GUILayout.BeginHorizontal();
							GUILayout.BeginVertical(GUILayout.Width(300f));
								DrawAttributeCurveEditor(drawProfileProp, ref showDrawProfile, drawProfileAC, ref editDrawProfile);
								DrawAttributeCurveEditor(drawStrengthProp, ref showDrawStrength, drawStrengthAC, ref editDrawStrength);
							GUILayout.EndVertical();

							GUILayout.BeginVertical(GUILayout.Width(300f));
								DrawAttributeCurveEditor(handlingProp, ref showHandling, handlingAC, ref editHandling);
								DrawAttributeCurveEditor(specialEffectProp, ref showSpecialEffect, specialEffectAC, ref editSpecialEffect);
							GUILayout.EndVertical();
						GUILayout.EndHorizontal();
					GUILayout.EndVertical();
				}
				/*	wearEditor fields
				*/
					bool showArmour = true;
					bool showSwiftness = true;
					bool showCarriedGearEfficacy = true;
					bool editArmour;
					bool editSwiftness;
					bool editCarriedGearEfficacy;
				void DrawWearEditor(SerializedObject itemSO){
					Wear wear = (Wear)itemSO.targetObject;
					SerializedProperty armourProp = itemSO.FindProperty("armour");
					AttributeCurve armourAC = wear.armour;
					SerializedProperty swiftnessProp = itemSO.FindProperty("swiftness");
					AttributeCurve swiftnessAC = wear.swiftness;
					SerializedProperty carriedGearEfficacyProp = itemSO.FindProperty("carriedGearEfficacy");
					AttributeCurve carriedGearEfficacyAC = wear.carriedGearEfficacy;
					
					GUILayout.BeginVertical(GUI.skin.box);
						EditorGUILayout.LabelField("Wear Attribute Curves");

						GUILayout.BeginHorizontal();
							GUILayout.BeginVertical(GUILayout.Width(300f));
								DrawAttributeCurveEditor(armourProp, ref showArmour, armourAC, ref editArmour);
								DrawAttributeCurveEditor(swiftnessProp, ref showSwiftness, swiftnessAC, ref editSwiftness);
							GUILayout.EndVertical();

							GUILayout.BeginVertical(GUILayout.Width(300f));
								DrawAttributeCurveEditor(carriedGearEfficacyProp, ref showCarriedGearEfficacy, carriedGearEfficacyAC, ref editCarriedGearEfficacy);
								// DrawAttributeCurveEditor(specialEffectProp, ref showSpecialEffect, specialEffectAC, ref editSpecialEffect);
							GUILayout.EndVertical();
						GUILayout.EndHorizontal();
					GUILayout.EndVertical();
				}
				/*	shield field
				*/
					bool showLongevity = true;
					bool showSturdiness = true;
					bool showDeflection = true;
					bool editLongevity;
					bool editSturdiness;
					bool editDeflection;
				void DrawShieldEditor(SerializedObject itemSO){
					Shield shield = (Shield)itemSO.targetObject;
					SerializedProperty longevityProp = itemSO.FindProperty("longevity");
					AttributeCurve longevityAC = shield.longevity;
					SerializedProperty sturdinessProp = itemSO.FindProperty("sturdiness");
					AttributeCurve sturdinessAC = shield.sturdiness;
					SerializedProperty deflectionProp = itemSO.FindProperty("deflection");
					AttributeCurve deflectionAC = shield.deflection;

					GUILayout.BeginVertical(GUI.skin.box);
						EditorGUILayout.LabelField("Wear Attribute Curves");

						GUILayout.BeginHorizontal();
							GUILayout.BeginVertical(GUILayout.Width(300f));
								DrawAttributeCurveEditor(longevityProp, ref showLongevity, longevityAC, ref editLongevity);
								DrawAttributeCurveEditor(sturdinessProp, ref showSturdiness, sturdinessAC, ref editSturdiness);
							GUILayout.EndVertical();

							GUILayout.BeginVertical(GUILayout.Width(300f));
								DrawAttributeCurveEditor(deflectionProp, ref showDeflection, deflectionAC, ref editDeflection);
								
							GUILayout.EndVertical();
						GUILayout.EndHorizontal();
					GUILayout.EndVertical();
					
				}

				/*	melee weapon fields
				*/
					bool showMWLongevity = true;
					bool showKnockPower = true;
					bool showFireRate = true;
					bool editMWLongevity;
					bool editKnockPower;
					bool editFireRate;
				void DrawMeleeWeaponEditor(SerializedObject itemSO){
					
					MeleeWeapon mWeapon = (MeleeWeapon)itemSO.targetObject;
					SerializedProperty longevityProp = itemSO.FindProperty("longevity");
					AttributeCurve longevityAC = mWeapon.longevity;
					SerializedProperty knockPowerProp = itemSO.FindProperty("knockPower");
					AttributeCurve knockPowerAC = mWeapon.knockPower;
					SerializedProperty fireRateProp = itemSO.FindProperty("fireRate");
					AttributeCurve fireRateAC = mWeapon.fireRate;
					
					GUILayout.BeginVertical(GUI.skin.box);
						EditorGUILayout.LabelField("Melee Weapon Attribute Curves");

						GUILayout.BeginHorizontal();
							GUILayout.BeginVertical(GUILayout.Width(300f));
								DrawAttributeCurveEditor(longevityProp, ref showMWLongevity, longevityAC, ref editMWLongevity);
								DrawAttributeCurveEditor(knockPowerProp, ref showKnockPower, knockPowerAC, ref editKnockPower);
							GUILayout.EndVertical();

							GUILayout.BeginVertical(GUILayout.Width(300f));
								DrawAttributeCurveEditor(fireRateProp, ref showFireRate, fireRateAC, ref editFireRate);
								
							GUILayout.EndVertical();
						GUILayout.EndHorizontal();
					GUILayout.EndVertical();

				}

				bool showEffectsEfficacy = true;
				bool showRounds = true;
				bool showAddedEffects = true;
				bool editEffectsEfficacy;
				bool editRounds;
				bool editAddedEffects;
				void DrawQuiverEditor(SerializedObject itemSO){
					Quiver quiver = (Quiver)itemSO.targetObject;
					SerializedProperty addedEffectsProp = itemSO.FindProperty("addedEffects");
					SerializedProperty effectsEfficacyProp = itemSO.FindProperty("effectsEfficacy");
					AttributeCurve effectsEfficacyAC = quiver.effectsEfficacy;
					SerializedProperty roundsProp = itemSO.FindProperty("rounds");
					AttributeCurve roundsAC = quiver.rounds;

					GUILayout.BeginVertical(GUI.skin.box);
						EditorGUILayout.LabelField("Quiver Attribute Curves");

						GUILayout.BeginHorizontal();
							GUILayout.BeginVertical(GUILayout.Width(300f));
								DrawAttributeCurveEditor(effectsEfficacyProp, ref showEffectsEfficacy, effectsEfficacyAC, ref editEffectsEfficacy);
								DrawAttributeCurveEditor(roundsProp, ref showRounds, roundsAC, ref editRounds);
							GUILayout.EndVertical();

							GUILayout.BeginVertical(GUILayout.Width(300f));
								DrawAddedEffectsEditor(quiver, addedEffectsProp);
								
							GUILayout.EndVertical();
						GUILayout.EndHorizontal();
					GUILayout.EndVertical();

				}
				ShotEffect shotEffectToAdd;
				void DrawAddedEffectsEditor(Quiver quiver, SerializedProperty addedEffectsProp){
					GUILayout.BeginVertical();
						EditorGUILayout.LabelField("Added Effects");

						GUILayout.BeginHorizontal();
							shotEffectToAdd = (ShotEffect)EditorGUILayout.EnumPopup(shotEffectToAdd);
							int index = (int)shotEffectToAdd;
							if(GUILayout.Button("Add")){
								addedEffectsProp.InsertArrayElementAtIndex(addedEffectsProp.arraySize);
								addedEffectsProp.GetArrayElementAtIndex(addedEffectsProp.arraySize-1).enumValueIndex = index;
								
								
							}

						GUILayout.EndHorizontal();
							GUILayout.BeginVertical(GUI.skin.box);
								for (int i = 0; i < addedEffectsProp.arraySize; i++)
								{
									GUILayout.BeginHorizontal(GUI.skin.textField);
										int propIndex = addedEffectsProp.GetArrayElementAtIndex(i).enumValueIndex;
										string shotEffectStr = addedEffectsProp.enumDisplayNames[propIndex];
										EditorGUILayout.LabelField(shotEffectStr);
										if(GUILayout.Button("Delete")){
											addedEffectsProp.DeleteArrayElementAtIndex(i);
										}
										
									GUILayout.EndHorizontal();	
								}
								EditorGUILayout.LabelField("addedEffectsListCount: " + addedEffectsProp.arraySize.ToString());
							GUILayout.EndVertical();
					GUILayout.EndVertical();
				}

				/*	pack fields
				*/
					bool showEfficacy = true;
					bool editEfficacy = true;
				void DrawPackEditor(SerializedObject itemSO){
					Pack pack = (Pack)itemSO.targetObject;
					SerializedProperty lootBonusProp = itemSO.FindProperty("lootBonus");
					SerializedProperty bonusTriggerProp = itemSO.FindProperty("bonusTrigger");
					SerializedProperty efficacyProp = itemSO.FindProperty("efficacy");
					AttributeCurve efficacyAC = pack.efficacy;

					GUILayout.BeginVertical(GUI.skin.box);
						EditorGUILayout.LabelField("Quiver Attribute Curves");

						GUILayout.BeginHorizontal();
							GUILayout.BeginVertical(GUILayout.Width(300f));
								DrawAttributeCurveEditor(efficacyProp, ref showEfficacy, efficacyAC, ref editEfficacy);
								
							GUILayout.EndVertical();

							GUILayout.BeginVertical(GUILayout.Width(300f));
								DrawLootBonusEditor(lootBonusProp);
								DrawBonusTriggerEditor(bonusTriggerProp);
								
							GUILayout.EndVertical();
						GUILayout.EndHorizontal();
					GUILayout.EndVertical();

				}
				LootBonus lootBonusToAdd;
				void DrawLootBonusEditor(SerializedProperty lootBonusProp){
					GUILayout.BeginVertical(GUI.skin.box);
						EditorGUILayout.LabelField("Loot Bonus");
						GUILayout.BeginHorizontal();
							lootBonusToAdd = (LootBonus)EditorGUILayout.EnumPopup(lootBonusToAdd);
							if(GUILayout.Button("Add")){
								lootBonusProp.InsertArrayElementAtIndex(lootBonusProp.arraySize);
								lootBonusProp.GetArrayElementAtIndex(lootBonusProp.arraySize -1).enumValueIndex = (int)lootBonusToAdd;
							}
						GUILayout.EndHorizontal();
						GUILayout.BeginVertical(GUI.skin.box);
							for (int i = 0; i < lootBonusProp.arraySize; i++)
							{
								GUILayout.BeginHorizontal(GUI.skin.textField);
									int lootBonusIndex = lootBonusProp.GetArrayElementAtIndex(i).enumValueIndex;
									string lootBonusString = lootBonusProp.GetArrayElementAtIndex(i).enumDisplayNames[lootBonusIndex];
									EditorGUILayout.LabelField(lootBonusString);
									if(GUILayout.Button("Delete")){
										lootBonusProp.DeleteArrayElementAtIndex(i);
									}
								GUILayout.EndHorizontal();
							}
						GUILayout.EndVertical();
					GUILayout.EndVertical();
				}
				BonusTrigger bonusTriggerToAssign;
				void DrawBonusTriggerEditor(SerializedProperty bonusTriggerProp){
					GUILayout.BeginVertical(GUI.skin.box);
						EditorGUILayout.LabelField("Bonus Trigger");
						GUILayout.BeginHorizontal();
							bonusTriggerToAssign = (BonusTrigger)EditorGUILayout.EnumPopup(bonusTriggerToAssign);
							if(GUILayout.Button("Assign")){
								bonusTriggerProp.enumValueIndex = (int)bonusTriggerToAssign;
							}
						GUILayout.EndHorizontal();
						EditorGUILayout.LabelField("Bonus Trigger: " + bonusTriggerProp.enumDisplayNames[bonusTriggerProp.enumValueIndex]);
						
					GUILayout.EndVertical();
				}
				void DrawCraftItemEditor(SerializedObject itemSO){
					CraftItem craftItem = (CraftItem)itemSO.targetObject;
					SerializedProperty attBonusListProp = itemSO.FindProperty("attributeBonusList");
					
					GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(300f));
						EditorGUILayout.LabelField("Craft Item Attribute Bonus");
						GUILayout.BeginVertical(GUI.skin.box);
							for (int i = 0; i < attBonusListProp.arraySize; i++)
							{
								GUILayout.BeginHorizontal();
									SerializedProperty attCurveIdProp = attBonusListProp.GetArrayElementAtIndex(i).FindPropertyRelative("attCurveId");
									string curveName = attCurveIdProp.enumNames[attCurveIdProp.enumValueIndex];
									SerializedProperty addedBonusProp = attBonusListProp.GetArrayElementAtIndex(i).FindPropertyRelative("addedBonus");
									EditorGUILayout.LabelField(curveName, GUILayout.Width(150f));
									GUILayout.FlexibleSpace();
									addedBonusProp.floatValue = EditorGUILayout.FloatField(addedBonusProp.floatValue, GUILayout.Width(70f));
								GUILayout.EndHorizontal();			
							}
						GUILayout.EndVertical();
					GUILayout.EndVertical();
				}
				
				void DrawAttributeCurveEditor(SerializedProperty attCurveProp, ref bool show, AttributeCurve ac, ref bool edit){
					
					SerializedProperty curveProp = attCurveProp.FindPropertyRelative("curve");
					SerializedProperty curveNameProp = attCurveProp.FindPropertyRelative("curveName");
					SerializedProperty inputNameProp = attCurveProp.FindPropertyRelative("inputName");
					SerializedProperty outputNameProp = attCurveProp.FindPropertyRelative("outputName");
					SerializedProperty outputMinProp = attCurveProp.FindPropertyRelative("outputMin");
					SerializedProperty outputMaxProp = attCurveProp.FindPropertyRelative("outputMax");
					SerializedProperty inputMinProp = attCurveProp.FindPropertyRelative("inputMin");
					SerializedProperty inputMaxProp = attCurveProp.FindPropertyRelative("inputMax");
					SerializedProperty previewOutputProp = attCurveProp.FindPropertyRelative("previewOutput");
					SerializedProperty previewInputProp = attCurveProp.FindPropertyRelative("previewInput");

					attCurveProp.serializedObject.Update();
					curveProp.serializedObject.Update();

					GUILayout.BeginVertical(GUI.skin.box);
						
						show = EditorGUILayout.Foldout(show, curveNameProp.stringValue, true);

						if(show){

								
							if(GUILayout.Button(edit?"Done Editing": "Edit Names", GUILayout.Width(100f))){
								edit = !edit;
							}

							if(edit)
								curveNameProp.stringValue = EditorGUILayout.TextField("Curve Name", curveNameProp.stringValue);

							GUILayout.BeginHorizontal(GUILayout.Height(100f));
								/*	output boxes
								*/
									GUILayout.BeginVertical(GUI.skin.box);
										if(edit){
											EditorGUILayout.LabelField("output: ", GUILayout.Width(50f));
											outputNameProp.stringValue = EditorGUILayout.TextField(outputNameProp.stringValue, GUILayout.Width(80f));
										}
										else
											EditorGUILayout.LabelField(outputNameProp.stringValue, GUILayout.Width(80f));

										GUILayout.BeginHorizontal();
										EditorGUILayout.LabelField("max", GUILayout.Width(30f));
										outputMaxProp.floatValue = EditorGUILayout.FloatField(outputMaxProp.floatValue, GUILayout.Width(30f));
										GUILayout.EndHorizontal();
										
										GUILayout.FlexibleSpace();

										GUILayout.BeginHorizontal();
										EditorGUILayout.LabelField("min", GUILayout.Width(30f));
										outputMinProp.floatValue = EditorGUILayout.FloatField(outputMinProp.floatValue, GUILayout.Width(30f));
										GUILayout.EndHorizontal();
										

									GUILayout.EndVertical();
								/*	curve box
								*/
									curveProp.animationCurveValue = EditorGUILayout.CurveField(curveProp.animationCurveValue/*, GUILayout.Width(100f)*/, GUILayout.Height(100f));

							GUILayout.EndHorizontal();
							
							/*	input boxes
							*/
								GUILayout.BeginHorizontal(GUI.skin.box);
									if(edit){
										EditorGUILayout.LabelField("input:", GUILayout.Width(40f));
										inputNameProp.stringValue = EditorGUILayout.TextField(inputNameProp.stringValue, GUILayout.Width(80f));
									}
									else
										EditorGUILayout.LabelField(inputNameProp.stringValue, GUILayout.Width(80f));
									EditorGUILayout.LabelField("min", GUILayout.Width(30f));
									inputMinProp.floatValue = EditorGUILayout.FloatField(inputMinProp.floatValue, GUILayout.Width(30f));

									GUILayout.FlexibleSpace();

									EditorGUILayout.LabelField("max", GUILayout.Width(30f));
									inputMaxProp.floatValue = EditorGUILayout.FloatField(inputMaxProp.floatValue, GUILayout.Width(30f));

								GUILayout.EndHorizontal();

							GUILayout.BeginHorizontal();
							if(GUILayout.Button("UpdateCurve")){
								attCurveProp.serializedObject.Update();
								curveProp.serializedObject.ApplyModifiedProperties();
								ac.UpdateCurve();
								curveProp.serializedObject.Update();
								attCurveProp.serializedObject.ApplyModifiedProperties();

							}
							if(GUILayout.Button("Reset")){
								attCurveProp.serializedObject.Update();
								curveProp.serializedObject.ApplyModifiedProperties();
								ac.ResetCurve();
								curveProp.serializedObject.Update();
								attCurveProp.serializedObject.ApplyModifiedProperties();
							}
							GUILayout.EndHorizontal();

							GUILayout.BeginVertical(GUI.skin.box);
								EditorGUILayout.LabelField("Preview");
								GUILayout.BeginHorizontal();
									EditorGUILayout.LabelField(inputNameProp.stringValue, GUILayout.Width(100f));
									GUILayout.FlexibleSpace();
									previewInputProp.floatValue = EditorGUILayout.Slider(previewInputProp.floatValue, inputMinProp.floatValue, inputMaxProp.floatValue/*, GUILayout.Width(50f)*/);
									
								GUILayout.EndHorizontal();
								
								GUILayout.BeginHorizontal();
									EditorGUILayout.LabelField(outputNameProp.stringValue, GUILayout.Width(100f));
									
									previewOutputProp.floatValue = ac.curve.Evaluate(previewInputProp.floatValue);
									GUILayout.FlexibleSpace();
									EditorGUILayout.LabelField(previewOutputProp.floatValue.ToString()/*, GUILayout.Width(50f)*/);
								GUILayout.EndHorizontal();
							GUILayout.EndVertical();

						}
					GUILayout.EndVertical();
					
					curveProp.serializedObject.ApplyModifiedProperties();
					attCurveProp.serializedObject.ApplyModifiedProperties();
					
				}
			/*	ButtomRight
			*/
				void DrawButtomRightSection(){
					// GUILayout.Box(GUIContent.none, GUILayout.Width(300f), GUILayout.Height(200f));
				}
		/*	Helper Methods
		*/
			string CreatedAssetPath(){
				
				string newPath = null;
				// string[] searchPath;
				// string[] guids;
				// int countPreadd;
				
				switch(m_selListId){
					case ListId.All:

					break;

					case ListId.BowList:
						newPath = m_scriptableObjectsPath + "/InventoryItems/Bows/" + m_newItemName + ".asset";
						Bow bowWithTheName = (Bow)AssetDatabase.LoadAssetAtPath(newPath, typeof(Bow));
						if(bowWithTheName != null)
							return null;

						// UpdateItemIds(ListId.BowList);
						// searchPath = new string[]{scriptableObjectsPath + "/InventoryItems/Bows"};
						// guids = AssetDatabase.FindAssets("t: Bow", searchPath);
						// countPreadd = guids.Length;

						Bow newBow = (Bow)ScriptableObject.CreateInstance<Bow>();
						newBow.itemName = m_newItemName;
						// newBow.itemId = countPreadd;

						newBow.dismantleTo = new Ingredients();
						newBow.dismantleTo.ingredientsName = m_newItemName + " Dismantle Components";
						newBow.dismantleTo.elements = new List<IngredientEntry>();
						
						newBow.craftedFrom = new Ingredients();
						newBow.craftedFrom.ingredientsName = m_newItemName + " Craft Components";
						newBow.craftedFrom.elements = new List<IngredientEntry>();

						CreateNewAttCurve(m_newItemName, ref newBow.drawProfile, "Draw Profile Curve", "Time", "Shot Power");
						CreateNewAttCurve(m_newItemName, ref newBow.drawStrength, "Draw Strength Curve", "Gear Level", "Draw Strength");
						CreateNewAttCurve(m_newItemName, ref newBow.handling, "Handling Curve", "Gear Level", "Handling");	
						CreateNewAttCurve(m_newItemName, ref newBow.specialEffect, "Special Effect Curve", "Gear Level", "Efficacy");

						AssetDatabase.CreateAsset(newBow, newPath);
						AssetDatabase.SaveAssets();

					break;

					case ListId.WearList:
						newPath = m_scriptableObjectsPath + "/InventoryItems/Wears/" + m_newItemName + ".asset";
						Wear wearWithTheName = (Wear)AssetDatabase.LoadAssetAtPath(newPath, typeof(Wear));
						if(wearWithTheName != null)
							return null;
						
						// UpdateItemIds(ListId.WearList);
						// searchPath = new string[]{m_scriptableObjectsPath + "/InventoryItems/Wears"};
						// guids = AssetDatabase.FindAssets("t: Wear", searchPath);
						// countPreadd = guids.Length;

						Wear newWear = (Wear)ScriptableObject.CreateInstance<Wear>();
						newWear.itemName = m_newItemName;
						// newWear.itemId = countPreadd + 100;

						newWear.dismantleTo = new Ingredients();
						newWear.dismantleTo.ingredientsName = m_newItemName + " Dismantle Components";
						newWear.dismantleTo.elements = new List<IngredientEntry>();
						
						newWear.craftedFrom = new Ingredients();
						newWear.craftedFrom.ingredientsName = m_newItemName + " Craft Components";
						newWear.craftedFrom.elements = new List<IngredientEntry>();

						
						CreateNewAttCurve(m_newItemName, ref newWear.armour, "Armour Curve", "Gear Level", "Armour");
						CreateNewAttCurve(m_newItemName, ref newWear.swiftness, "Swiftness Curve", "Gear Level", "Swiftness");
						CreateNewAttCurve(m_newItemName, ref newWear.carriedGearEfficacy, "Carried Efficacy Curve", "Gear Level", "Efficacy");

						AssetDatabase.CreateAsset(newWear, newPath);
						AssetDatabase.SaveAssets();
					break;
					
					case ListId.ShieldList:
						newPath = m_scriptableObjectsPath + "/InventoryItems/Shields/" + m_newItemName + ".asset";
						Shield shieldWithTheName = (Shield)AssetDatabase.LoadAssetAtPath(newPath, typeof(Shield));
						if(shieldWithTheName != null)
							return null;
						
						// UpdateItemIds(ListId.ShieldList);
						// searchPath = new string[]{m_scriptableObjectsPath + "/InventoryItems/Shields"};
						// guids = AssetDatabase.FindAssets("t: Shield", searchPath);
						// countPreadd = guids.Length;

						Shield newShield = (Shield)ScriptableObject.CreateInstance<Shield>();
						newShield.itemName = m_newItemName;
						// newShield.itemId = countPreadd + 200;

						newShield.dismantleTo = new Ingredients();
						newShield.dismantleTo.ingredientsName = m_newItemName + " Dismantle Components";
						newShield.dismantleTo.elements = new List<IngredientEntry>();
						
						newShield.craftedFrom = new Ingredients();
						newShield.craftedFrom.ingredientsName = m_newItemName + " Craft Components";
						newShield.craftedFrom.elements = new List<IngredientEntry>();

						CreateNewAttCurve(m_newItemName, ref newShield.longevity, "Longevity Curve", "Gear Level", "Longevity");
						CreateNewAttCurve(m_newItemName, ref newShield.sturdiness, "Sturdiness Curve", "Gear Level", "Sturdiness");
						CreateNewAttCurve(m_newItemName, ref newShield.deflection, "Deflection Curve", "Gear Level", "Deflection");

						AssetDatabase.CreateAsset(newShield, newPath);
						AssetDatabase.SaveAssets();
					break;

					case ListId.MeleeWeaponList:
						newPath = m_scriptableObjectsPath + "/InventoryItems/MeleeWeapons/" + m_newItemName + ".asset";
						MeleeWeapon meleeWeaponWithTheName = (MeleeWeapon)AssetDatabase.LoadAssetAtPath(newPath, typeof(MeleeWeapon));
						if(meleeWeaponWithTheName != null)
							return null;
						
						// UpdateItemIds(ListId.MeleeWeaponList);
						// searchPath = new string[]{m_scriptableObjectsPath + "/InventoryItems/MeleeWeapons"};
						// guids = AssetDatabase.FindAssets("t: MeleeWeapon", searchPath);
						// countPreadd = guids.Length;

						MeleeWeapon newMeleeWeapon = (MeleeWeapon)ScriptableObject.CreateInstance<MeleeWeapon>();
						newMeleeWeapon.itemName = m_newItemName;
						// newMeleeWeapon.itemId = countPreadd + 300;

						newMeleeWeapon.dismantleTo = new Ingredients();
						newMeleeWeapon.dismantleTo.ingredientsName = m_newItemName + " Dismantle Components";
						newMeleeWeapon.dismantleTo.elements = new List<IngredientEntry>();
						
						newMeleeWeapon.craftedFrom = new Ingredients();
						newMeleeWeapon.craftedFrom.ingredientsName = m_newItemName + " Craft Components";
						newMeleeWeapon.craftedFrom.elements = new List<IngredientEntry>();

						CreateNewAttCurve(m_newItemName, ref newMeleeWeapon.longevity, "Longevity Curve", "Gear Level", "Longevity");
						CreateNewAttCurve(m_newItemName, ref newMeleeWeapon.knockPower, "Knock Power Curve", "Gear Level", "Knock Power");
						CreateNewAttCurve(m_newItemName, ref newMeleeWeapon.fireRate, "Fire Rate Curve", "Gear Level", "Fire Rate");


						AssetDatabase.CreateAsset(newMeleeWeapon, newPath);
						AssetDatabase.SaveAssets();
					break;
					
					case ListId.QuiverList:
						newPath = m_scriptableObjectsPath + "/InventoryItems/Quivers/" + m_newItemName + ".asset";
						Quiver quiverWithTheName = (Quiver)AssetDatabase.LoadAssetAtPath(newPath, typeof(Quiver));
						if(quiverWithTheName != null)
							return null;
						
						// UpdateItemIds(ListId.QuiverList);
						// searchPath = new string[]{m_scriptableObjectsPath + "/InventoryItems/Quivers"};
						// guids = AssetDatabase.FindAssets("t: Quiver", searchPath);
						// countPreadd = guids.Length;

						Quiver newQuiver = (Quiver)ScriptableObject.CreateInstance<Quiver>();
						newQuiver.itemName = m_newItemName;
						// newQuiver.itemId = countPreadd + 400;

						newQuiver.dismantleTo = new Ingredients();
						newQuiver.dismantleTo.ingredientsName = m_newItemName + " Dismantle Components";
						newQuiver.dismantleTo.elements = new List<IngredientEntry>();
						
						newQuiver.craftedFrom = new Ingredients();
						newQuiver.craftedFrom.ingredientsName = m_newItemName + " Craft Components";
						newQuiver.craftedFrom.elements = new List<IngredientEntry>();

						newQuiver.addedEffects = new List<ShotEffect>();

						CreateNewAttCurve(m_newItemName, ref newQuiver.effectsEfficacy, "Effects Efficacy Curve", "Gear Level", "Efficacy");
						CreateNewAttCurve(m_newItemName, ref newQuiver.rounds, "Rounds Curve", "Gear Level", "Rounds");

						AssetDatabase.CreateAsset(newQuiver, newPath);
						AssetDatabase.SaveAssets();
					break;

					case ListId.PackList:
						newPath = m_scriptableObjectsPath + "/InventoryItems/Packs/" + m_newItemName + ".asset";
						Pack packWithTheName = (Pack)AssetDatabase.LoadAssetAtPath(newPath, typeof(Pack));
						if(packWithTheName != null)
							return null;
						
						// UpdateItemIds(ListId.PackList);
						// searchPath = new string[]{m_scriptableObjectsPath + "/InventoryItems/Packs"};
						// guids = AssetDatabase.FindAssets("t: Pack", searchPath);
						// countPreadd = guids.Length;

						Pack newPack = (Pack)ScriptableObject.CreateInstance<Pack>();
						newPack.itemName = m_newItemName;
						// newPack.itemId = countPreadd + 500;

						newPack.dismantleTo = new Ingredients();
						newPack.dismantleTo.ingredientsName = m_newItemName + " Dismantle Components";
						newPack.dismantleTo.elements = new List<IngredientEntry>();
						
						newPack.craftedFrom = new Ingredients();
						newPack.craftedFrom.ingredientsName = m_newItemName + " Craft Components";
						newPack.craftedFrom.elements = new List<IngredientEntry>();

						newPack.lootBonus = new List<LootBonus>();
						newPack.bonusTrigger = BonusTrigger.None;

						CreateNewAttCurve(m_newItemName, ref newPack.efficacy, "Efficacy Curve", "Gear Level", "Efficacy");


						AssetDatabase.CreateAsset(newPack, newPath);
						AssetDatabase.SaveAssets();
					break;

					case ListId.CraftItemList:
						newPath = m_scriptableObjectsPath + "/InventoryItems/CraftItems/" + m_newItemName + ".asset";
						CraftItem craftItemWithTheName = (CraftItem)AssetDatabase.LoadAssetAtPath(newPath, typeof(CraftItem));
						if(craftItemWithTheName != null)
							return null;
						
						// UpdateItemIds(ListId.CraftItemList);
						// searchPath = new string[]{m_scriptableObjectsPath + "/InventoryItems/CraftItems"};
						// guids = AssetDatabase.FindAssets("t: CraftItem", searchPath);
						// countPreadd = guids.Length;

						CraftItem newCraftItem = (CraftItem)ScriptableObject.CreateInstance<CraftItem>();
						newCraftItem.itemName = m_newItemName;

						newCraftItem.attributeBonusList = new List<AttributeBonus>();
						
						AttributeBonus bowDrawStrengthBonus = new AttributeBonus();
						bowDrawStrengthBonus.attCurveId = AttCurveId.BowDrawStrength;
						bowDrawStrengthBonus.addedBonus = 0f;
						newCraftItem.attributeBonusList.Add(bowDrawStrengthBonus);
						
						AttributeBonus bowHandlingBonus = new AttributeBonus();
						bowHandlingBonus.attCurveId = AttCurveId.BowHandling;
						bowHandlingBonus.addedBonus = 0f;
						newCraftItem.attributeBonusList.Add(bowHandlingBonus);

						AttributeBonus bowSpecialEffectBonus = new AttributeBonus();
						bowSpecialEffectBonus.attCurveId = AttCurveId.BowSpecialEffect;
						bowSpecialEffectBonus.addedBonus = 0f;
						newCraftItem.attributeBonusList.Add(bowSpecialEffectBonus);

						AttributeBonus wearArmourBonus = new AttributeBonus();
						wearArmourBonus.attCurveId = AttCurveId.WearArmour;
						wearArmourBonus.addedBonus = 0f;
						newCraftItem.attributeBonusList.Add(wearArmourBonus);

						AttributeBonus WearSwiftnessBonus = new AttributeBonus();
						WearSwiftnessBonus.attCurveId = AttCurveId.WearSwiftness;
						WearSwiftnessBonus.addedBonus = 0f;
						newCraftItem.attributeBonusList.Add(WearSwiftnessBonus);

						AttributeBonus WearCarriedGearEfficacyBonus = new AttributeBonus();
						WearCarriedGearEfficacyBonus.attCurveId = AttCurveId.WearCarriedGearEfficacy;
						WearCarriedGearEfficacyBonus.addedBonus = 0f;
						newCraftItem.attributeBonusList.Add(WearCarriedGearEfficacyBonus);

						AttributeBonus shieldLongevityBonus = new AttributeBonus();
						shieldLongevityBonus.attCurveId = AttCurveId.ShieldLongevity;
						shieldLongevityBonus.addedBonus = 0f;
						newCraftItem.attributeBonusList.Add(shieldLongevityBonus);

						AttributeBonus shieldSturdinessBonus = new AttributeBonus();
						shieldSturdinessBonus.attCurveId = AttCurveId.ShieldSturdiness;
						shieldSturdinessBonus.addedBonus = 0f;
						newCraftItem.attributeBonusList.Add(shieldSturdinessBonus);

						AttributeBonus shieldDeflectionBonus = new AttributeBonus();
						shieldDeflectionBonus.attCurveId = AttCurveId.ShieldDeflection;
						shieldDeflectionBonus.addedBonus = 0f;
						newCraftItem.attributeBonusList.Add(shieldDeflectionBonus);

						AttributeBonus meleeWeaponLongevityBonus = new AttributeBonus();
						meleeWeaponLongevityBonus.attCurveId = AttCurveId.MeleeWeaponLongevity;
						meleeWeaponLongevityBonus.addedBonus = 0f;
						newCraftItem.attributeBonusList.Add(meleeWeaponLongevityBonus);

						AttributeBonus meleeWeaponKnockPowerBonus = new AttributeBonus();
						meleeWeaponKnockPowerBonus.attCurveId = AttCurveId.MeleeWeaponKnockPower;
						meleeWeaponKnockPowerBonus.addedBonus = 0f;
						newCraftItem.attributeBonusList.Add(meleeWeaponKnockPowerBonus);

						AttributeBonus MeleeWeaponFireRateBonus = new AttributeBonus();
						MeleeWeaponFireRateBonus.attCurveId = AttCurveId.MeleeWeaponFireRate;
						MeleeWeaponFireRateBonus.addedBonus = 0f;
						newCraftItem.attributeBonusList.Add(MeleeWeaponFireRateBonus);

						AttributeBonus quiverEffectEfficacyBonus = new AttributeBonus();
						quiverEffectEfficacyBonus.attCurveId = AttCurveId.QuiverEffectsEfficacy;
						quiverEffectEfficacyBonus.addedBonus = 0f;
						newCraftItem.attributeBonusList.Add(quiverEffectEfficacyBonus);

						AttributeBonus quiverRoundsBonus = new AttributeBonus();
						quiverRoundsBonus.attCurveId = AttCurveId.QuiverRounds;
						quiverRoundsBonus.addedBonus = 0f;
						newCraftItem.attributeBonusList.Add(quiverRoundsBonus);

						AttributeBonus packEfficacyBonus = new AttributeBonus();
						packEfficacyBonus.attCurveId = AttCurveId.PackEfficacy;
						packEfficacyBonus.addedBonus = 0f;
						newCraftItem.attributeBonusList.Add(packEfficacyBonus);


						AssetDatabase.CreateAsset(newCraftItem, newPath);
						AssetDatabase.SaveAssets();
					break;

					default: return null;
				}
				
				return newPath;
				
			}

			void UpdateItemIds(ListId listId){
					string[] searchPath;
					string[] guids;
				switch(listId){
					case ListId.BowList:
						searchPath = new string[]{m_scriptableObjectsPath + "/InventoryItems/Bows"};
						guids = AssetDatabase.FindAssets("t: Bow", searchPath);
						for (int i = 0; i < guids.Length; i++)
						{
							string path = AssetDatabase.GUIDToAssetPath(guids[i]);
							Bow bow = (Bow)AssetDatabase.LoadAssetAtPath(path, typeof(Bow));
							SerializedObject bowSO = new SerializedObject(bow);
							bowSO.Update();
							bowSO.FindProperty("itemId").intValue = i;
							// bow.itemId = i;
							bowSO.ApplyModifiedProperties();
						}
						AssetDatabase.SaveAssets();
					break;

					case ListId.WearList:
						searchPath = new string[]{m_scriptableObjectsPath + "/InventoryItems/Wears"};
						guids = AssetDatabase.FindAssets("t: Wear", searchPath);
						for (int i = 0; i < guids.Length; i++)
						{
							string path = AssetDatabase.GUIDToAssetPath(guids[i]);
							Wear wear = (Wear)AssetDatabase.LoadAssetAtPath(path, typeof(Wear));
							SerializedObject wearSO = new SerializedObject(wear);
							wearSO.Update();
							wearSO.FindProperty("itemId").intValue = 100 + i;
							wearSO.ApplyModifiedProperties();
							
						}
						AssetDatabase.SaveAssets();
					break;

					case ListId.ShieldList:
						searchPath = new string[]{m_scriptableObjectsPath + "/InventoryItems/Shields"};
						guids = AssetDatabase.FindAssets("t: Shield", searchPath);
						for (int i = 0; i < guids.Length; i++)
						{
							string path = AssetDatabase.GUIDToAssetPath(guids[i]);
							Shield shield = (Shield)AssetDatabase.LoadAssetAtPath(path, typeof(Shield));
							SerializedObject shieldSO = new SerializedObject(shield);
							shieldSO.Update();
							shieldSO.FindProperty("itemId").intValue = 200 + i;
							shieldSO.ApplyModifiedProperties();
						}
						AssetDatabase.SaveAssets();
					break;

					case ListId.MeleeWeaponList:
						searchPath = new string[]{m_scriptableObjectsPath + "/InventoryItems/MeleeWeapons"};
						guids = AssetDatabase.FindAssets("t: MeleeWeapon", searchPath);
						for (int i = 0; i < guids.Length; i++)
						{
							string path = AssetDatabase.GUIDToAssetPath(guids[i]);
							MeleeWeapon meleeWeapon = (MeleeWeapon)AssetDatabase.LoadAssetAtPath(path, typeof(MeleeWeapon));
							SerializedObject meleeWeaponSO = new SerializedObject(meleeWeapon);
							meleeWeaponSO.Update();
							meleeWeaponSO.FindProperty("itemId").intValue = 300 + i;
							meleeWeaponSO.ApplyModifiedProperties();
						}
						AssetDatabase.SaveAssets();
					
					break;

					case ListId.QuiverList:
						searchPath = new string[]{m_scriptableObjectsPath + "/InventoryItems/Quivers"};
						guids = AssetDatabase.FindAssets("t: Quiver", searchPath);
						for (int i = 0; i < guids.Length; i++)
						{
							string path = AssetDatabase.GUIDToAssetPath(guids[i]);
							Quiver quiver = (Quiver)AssetDatabase.LoadAssetAtPath(path, typeof(Quiver));
							SerializedObject quiverSO = new SerializedObject(quiver);
							quiverSO.Update();
							quiverSO.FindProperty("itemId").intValue = 400 + i;
							quiverSO.ApplyModifiedProperties();
						}
						AssetDatabase.SaveAssets();
					break;

					case ListId.PackList:
						searchPath = new string[]{m_scriptableObjectsPath + "/InventoryItems/Packs"};
						guids = AssetDatabase.FindAssets("t: Pack", searchPath);
						for (int i = 0; i < guids.Length; i++)
						{
							string path = AssetDatabase.GUIDToAssetPath(guids[i]);
							Pack pack = (Pack)AssetDatabase.LoadAssetAtPath(path, typeof(Pack));
							SerializedObject packSO = new SerializedObject(pack);
							packSO.Update();
							packSO.FindProperty("itemId").intValue = 500 + i;
							packSO.ApplyModifiedProperties();
						}
						AssetDatabase.SaveAssets();
					break;

					case ListId.CraftItemList:
						searchPath = new string[]{m_scriptableObjectsPath + "/InventoryItems/CraftItems"};
						guids = AssetDatabase.FindAssets("t: CraftItem", searchPath);
						for (int i = 0; i < guids.Length; i++)
						{
							string path = AssetDatabase.GUIDToAssetPath(guids[i]);
							CraftItem craftItem = (CraftItem)AssetDatabase.LoadAssetAtPath(path, typeof(CraftItem));
							SerializedObject craftItemSO = new SerializedObject(craftItem);
							craftItemSO.Update();
							craftItemSO.FindProperty("itemId").intValue = 600 + i;
							craftItemSO.ApplyModifiedProperties();
						}
						AssetDatabase.SaveAssets();
					break;
				}
			}

			void CreateNewAttCurve(string newItemName, ref AttributeCurve ac, string curveName, string inputName, string outputName){
				ac = new AttributeCurve();
				ac.curveName = newItemName + " " + curveName;
				ac.inputName = inputName;
				ac.outputName = outputName;
				ac.curve = new AnimationCurve();
			}
			void UpdateAllItemsList(){
				
				SerializedProperty allItemsListProp = m_itemListSO.FindProperty("allItemsList");
				SerializedProperty bowListProp = m_itemListSO.FindProperty("bowList");
				SerializedProperty wearListProp = m_itemListSO.FindProperty("wearList");
				SerializedProperty shieldListProp = m_itemListSO.FindProperty("shieldList");
				SerializedProperty meleeWeaponListProp = m_itemListSO.FindProperty("meleeWeaponList");
				SerializedProperty quiverListProp = m_itemListSO.FindProperty("quiverList");
				SerializedProperty packListProp = m_itemListSO.FindProperty("packList");
				SerializedProperty craftItemListProp = m_itemListSO.FindProperty("craftItemList");

				SerializedObject allItemsListSO = allItemsListProp.serializedObject;
				
				allItemsListProp.ClearArray();

				List<SerializedProperty> listPropsList = new List<SerializedProperty>();
				listPropsList.Add(bowListProp);
				listPropsList.Add(wearListProp);
				listPropsList.Add(shieldListProp);
				listPropsList.Add(meleeWeaponListProp);
				listPropsList.Add(quiverListProp);
				listPropsList.Add(packListProp);
				listPropsList.Add(craftItemListProp);

				for (int i = 0; i < listPropsList.Count; i++)
				{
					for (int j = 0; j < listPropsList[i].arraySize; j++)
					{
						allItemsListProp.InsertArrayElementAtIndex(allItemsListProp.arraySize);
						allItemsListProp.GetArrayElementAtIndex(allItemsListProp.arraySize - 1).objectReferenceValue = 
						(InventoryItem)listPropsList[i].GetArrayElementAtIndex(j).objectReferenceValue;
					}
				}
			
				
				allItemsListSO.ApplyModifiedProperties();
				// itemListSO.ApplyModifiedProperties();
				// windowDataSO.ApplyModifiedProperties();
			}
			void DeleteInventoryListAsset(){
				string path = AssetDatabase.GetAssetPath(m_windowData.invList);
				// Debug.Log(path);
				// SerializedObject windowDataSO = new SerializedObject(invSysWindowData);
				// SerializedProperty invListProp = windowDataSO.FindProperty("invList");
				// windowDataSO.Update();
				m_windowDataSO.FindProperty("invList").objectReferenceValue = null;
				m_itemListProp = null;
				m_itemListSO = null;
				// invListProp.objectReferenceValue = null;
				// windowDataSO.ApplyModifiedProperties();
				AssetDatabase.DeleteAsset(path);
			}


			void CreateNewInventoryItemEditorWindowData(){
				InventoryItemEditorWindowData windowData = (InventoryItemEditorWindowData)ScriptableObject.CreateInstance<InventoryItemEditorWindowData>();
			

				string path = m_scriptableObjectsPath + "/InventoryItemEditorWindowData.asset";
				AssetDatabase.CreateAsset(windowData, path);
				AssetDatabase.SaveAssets();
			}


			void CheckAndCreateInventoryItemList(ref Rect buttonRect){
				/*	create a new InventoryItemList asset and save in the assets, and store it the local field
					Make a warning before create if there's already one
					Force me to confirm before proceed to overwrite
				*/
				
				string [] searchPath = new string[]{m_scriptableObjectsPath};
				string[] guids = AssetDatabase.FindAssets("t: InventoryItemList", searchPath);
				if(guids.Length<1){
					//there's none so create new
					CreateInventoryItemList();
				}else{
					if(guids.Length>1){
						Debug.LogError("Theere's more than 1 assets of type InventoryItemList in the assets");
						return;
					}
					/*	There's already one and only one
						implment the overrite warning systems here
					*/
					
					PopupWindow.Show(buttonRect, new ItemListWarningWindow());
					if (Event.current.type == EventType.Repaint) 
					buttonRect = GUILayoutUtility.GetLastRect();
				}
			}
			public void CreateInventoryItemList(){
				InventoryItemList newList = (InventoryItemList)ScriptableObject.CreateInstance<InventoryItemList>();
				string path = m_scriptableObjectsPath  + "/InventoryItemList.asset";
				
				newList.allItemsList = new List<InventoryItem>();
				newList.bowList = new List<Bow>();
				newList.wearList = new List<Wear>();
				newList.shieldList = new List<Shield>();
				newList.meleeWeaponList = new List<MeleeWeapon>();
				newList.quiverList = new List<Quiver>();
				newList.packList = new List<Pack>();
				newList.craftItemList = new List<CraftItem>();
				
				AssetDatabase.CreateAsset(newList, path);
				AssetDatabase.SaveAssets();

			}
					
	}
}
