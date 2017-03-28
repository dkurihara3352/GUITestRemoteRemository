using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MyUtility;


namespace InventorySystem{
	public class PlayerInventoryEditorWindow : EditorWindow {

		[MenuItem("Window/Inventory System Window %#r")]
		static void Init(){
			PlayerInventoryEditorWindow inventoryEditorWindow = (PlayerInventoryEditorWindow)EditorWindow.GetWindow<PlayerInventoryEditorWindow>(true);
			inventoryEditorWindow.Show();
		}

		void OnEnable(){
			minSize = new Vector2(750f, 200f);
		}

		void OnGUI(){
			// EditorGUILayout.LabelField("window.w: " + position.width + ", window.h: " + position.height);
			GUI.skin = (GUISkin)AssetDatabase.LoadAssetAtPath("Assets/InventorySystem/Skins/CustomEditorSkin.guiskin", typeof(GUISkin));
			GUILayout.BeginHorizontal();

				GUILayout.BeginVertical(GUILayout.Width(400f));
					DrawPlayerInventoryAssetManager();
					DrawPlayerInventoryEditor();
				GUILayout.EndVertical();

				GUILayout.BeginVertical(GUILayout.Width(350f));
					DrawItemInstanceEditor();
				GUILayout.EndVertical();

			GUILayout.EndHorizontal();
		}

		PlayerInventory m_inventoryToEdit;
		string m_playerInventoryPath = "Assets/InventorySystem/ScriptableObjects/PlayerInventory";
		string m_newInventoryName = "PlayerInventory";
		int selectedInventoryIndex;
		void DrawPlayerInventoryAssetManager(){
			GUILayout.BeginVertical(GUI.skin.box);

				string[] searchPath = new string[]{m_playerInventoryPath};
				string[] guids = AssetDatabase.FindAssets("t: PlayerInventory", searchPath);
				List<string> names = new List<string>();
				for (int i = 0; i < guids.Length; i++)
				{
					string path = AssetDatabase.GUIDToAssetPath(guids[i]);
					PlayerInventory inventoryAtPath = (PlayerInventory)AssetDatabase.LoadAssetAtPath(path, typeof(PlayerInventory));
					names.Add(inventoryAtPath.name.ToString());
				}
				string[] namesArray = names.ToArray();

				EditorGUILayout.LabelField("Create new player inventory");
				GUILayout.BeginHorizontal();
				m_newInventoryName = EditorGUILayout.TextField(m_newInventoryName);
				if(GUILayout.Button("Create")){
					PlayerInventory newInventory = (PlayerInventory)ScriptableObject.CreateInstance<PlayerInventory>();
					newInventory.entries = new List<InventoryItemEntry>();
					string path = m_playerInventoryPath + "/" + m_newInventoryName + ".asset";
					AssetDatabase.CreateAsset(newInventory, path);
					AssetDatabase.SaveAssets();
				}
				GUILayout.EndHorizontal();
				
				EditorGUILayout.LabelField("Create new player inventory");
				GUILayout.BeginHorizontal();
					selectedInventoryIndex = EditorGUILayout.Popup(selectedInventoryIndex, namesArray);
					if(GUILayout.Button("Edit", GUILayout.Width(50f))){

						m_inventoryToEdit = (PlayerInventory)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[selectedInventoryIndex]), typeof(PlayerInventory));
					}
				GUILayout.EndHorizontal();


				// if(namesArray.Length == 0){//if there's none
				// 	EditorGUILayout.LabelField("Create new player inventory");
				// 	GUILayout.BeginHorizontal();
				// 	m_newInventoryName = EditorGUILayout.TextField(m_newInventoryName);
				// 	if(GUILayout.Button("Create")){
				// 		PlayerInventory newInventory = (PlayerInventory)ScriptableObject.CreateInstance<PlayerInventory>();
				// 		newInventory.entries = new List<InventoryItemEntry>();
				// 		string path = m_playerInventoryPath + "/" + m_newInventoryName + ".asset";
				// 		AssetDatabase.CreateAsset(newInventory, path);
				// 		AssetDatabase.SaveAssets();
				// 	}
				// 	GUILayout.EndHorizontal();
				// }else{//if there's some
				// 	EditorGUILayout.LabelField("Select Player Inventory to Edit");
				// 	GUILayout.BeginVertical(GUI.skin.box);
				// 		for (int i = 0; i < namesArray.Length; i++)
				// 		{
				// 			GUILayout.BeginHorizontal(GUI.skin.textField);
				// 				EditorGUILayout.LabelField(namesArray[i]);
				// 				if(GUILayout.Button("Edit")){
				// 					string path = AssetDatabase.GUIDToAssetPath(guids[i]);
				// 					m_inventoryToEdit = (PlayerInventory)AssetDatabase.LoadAssetAtPath(path, typeof(PlayerInventory));
				// 				}
				// 			GUILayout.EndHorizontal();
				// 		}
				// 	GUILayout.EndVertical();

				// }
			GUILayout.EndVertical();
		}
		List<InventoryItem> m_listOfAllItems;
		int m_selectedIndexOfItem;
		int m_addedQuant;
		Vector2 m_entriesScrollPos;
		SerializedObject inventorySO;
		ItemInstance m_itemInstanceToEdit;
		void DrawPlayerInventoryEditor(){
			if(m_inventoryToEdit != null){

				inventorySO = new SerializedObject(m_inventoryToEdit);
				inventorySO.Update();
				SerializedProperty entriesProp = inventorySO.FindProperty("entries");

				GUILayout.BeginVertical(GUI.skin.box);
					EditorGUILayout.LabelField(m_inventoryToEdit.name.ToString() + " Editor");
					GUILayout.BeginVertical(GUI.skin.box);

						GUILayout.BeginHorizontal();

							EditorGUILayout.LabelField("Item to Add", GUILayout.Width(70f));
							m_listOfAllItems = ListOfAll();
							List<string> namesList = new List<string>();
							for (int i = 0; i < m_listOfAllItems.Count; i++)
							{
								namesList.Add(m_listOfAllItems[i].name.ToString());
							}
							string[] namesArray = namesList.ToArray();
							m_selectedIndexOfItem = EditorGUILayout.Popup(m_selectedIndexOfItem, namesArray, GUILayout.Width(100f));
							
							InventoryItem itemToAdd = m_listOfAllItems[m_selectedIndexOfItem];
							// ItemInstance itemInstanceToAdd = GetItemInstanceToAdd(itemToAdd);

							if(itemToAdd.isStackable){
								EditorGUILayout.LabelField("quant", GUILayout.Width(50f));
								m_addedQuant = EditorGUILayout.IntField(m_addedQuant, GUILayout.Width(50f));
							}else{
								m_addedQuant = 1;
							}


							GUILayout.FlexibleSpace();

							GUI.enabled = (m_addedQuant > 0);
							if(GUILayout.Button("Create and Add", GUILayout.Width(100f))){
								int indexAtItemToAdd;
								if(IsAlreadyAdded(itemToAdd, entriesProp, out indexAtItemToAdd) && itemToAdd.isStackable){

									entriesProp.GetArrayElementAtIndex(indexAtItemToAdd).FindPropertyRelative("quantity").intValue += m_addedQuant;

								}else{

									entriesProp.InsertArrayElementAtIndex(entriesProp.arraySize);
									ItemInstance newItemInstance = CreateNewItemInstance(itemToAdd);
									// ItemInstance itemInstance = (ItemInstance)entriesProp.GetArrayElementAtIndex(entriesProp.arraySize -1).FindPropertyRelative("itemInstance").objectReferenceValue;
									// new SerializedObject(itemInstance).FindProperty("m_item").objectReferenceValue = itemToAdd;
									// entriesProp.GetArrayElementAtIndex(entriesProp.arraySize - 1).FindPropertyRelative("item").objectReferenceValue = itemToAdd;
									entriesProp.GetArrayElementAtIndex(entriesProp.arraySize - 1).FindPropertyRelative("itemInstance").objectReferenceValue = newItemInstance;
									entriesProp.GetArrayElementAtIndex(entriesProp.arraySize - 1).FindPropertyRelative("quantity").intValue = m_addedQuant;
								}
								// ReorderEntries(ref entriesProp);
								ReorderEntriesRevised(ref entriesProp);
							}
							GUI.enabled = true;

						
						GUILayout.EndHorizontal();
						/*	entries
						*/
						m_entriesScrollPos = GUILayout.BeginScrollView(m_entriesScrollPos);
							GUILayout.BeginVertical(GUI.skin.box);
								
								for (int i = 0; i < entriesProp.arraySize; i++)
								{
									GUILayout.BeginHorizontal(GUI.skin.textField);
										ItemInstance itemInstance = (ItemInstance)entriesProp.GetArrayElementAtIndex(i).FindPropertyRelative("itemInstance").objectReferenceValue;
										SerializedObject itemInstanceSO = new SerializedObject(itemInstance);
									
										InventoryItem itemToDisplay = (InventoryItem)itemInstanceSO.FindProperty("m_item").objectReferenceValue;

										Sprite itemSprite = itemToDisplay.itemSprite;
										MyEditorWindowUtility.DrawOnGUISprite(itemSprite, .05f);
										EditorGUILayout.LabelField(itemInstance.name.ToString(), GUILayout.Width(100f));

										GUILayout.FlexibleSpace();

										if(itemToDisplay is EquipableGear){
											EquipableGear equiGear = (EquipableGear)itemToDisplay;
											int maxLevel = equiGear.maxLevel;
											int gearLevel = itemInstanceSO.FindProperty("gearLevel").intValue;
											string label = gearLevel.ToString() + " / " + maxLevel.ToString();

											EditorGUI.ProgressBar(GUILayoutUtility.GetRect(100f, EditorGUIUtility.singleLineHeight), itemInstanceSO.FindProperty("gearLevel").intValue/(float)(equiGear.maxLevel), label);
										}
										

										if(itemToDisplay.isStackable)
										entriesProp.GetArrayElementAtIndex(i).FindPropertyRelative("quantity").intValue = EditorGUILayout.IntField(entriesProp.GetArrayElementAtIndex(i).FindPropertyRelative("quantity").intValue, GUILayout.Width(50f));

										if(GUILayout.Button("Edit" ,GUILayout.Width(50f))){
											m_itemInstanceToEdit = itemInstance;
										}
										
										if(GUILayout.Button("Delete", GUILayout.Width(50f))){
											entriesProp.DeleteArrayElementAtIndex(i);
											AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(itemInstance));
											// ReorderEntries(ref entriesProp);
											ReorderEntriesRevised(ref entriesProp);
										}
									GUILayout.EndHorizontal();	
									
								}
							GUILayout.EndVertical();
						GUILayout.EndScrollView();

					GUILayout.EndVertical();
				GUILayout.EndVertical();
				inventorySO.ApplyModifiedProperties();
			}else{
				EditorGUILayout.HelpBox("create and/or select inventory to edit", MessageType.Info);
			}
		}
		string inventoryItemsPath = "Assets/InventorySystem/ScriptableObjects/InventoryItems";
		List<InventoryItem> ListOfAll(){
			List<InventoryItem> result = new List<InventoryItem>();
			string[] searchPath = new string[]{inventoryItemsPath};
			string[] bowGuids = AssetDatabase.FindAssets("t: Bow", searchPath);
			for (int i = 0; i < bowGuids.Length; i++)
			{
				string path = AssetDatabase.GUIDToAssetPath(bowGuids[i]);
				InventoryItem itemToAdd = (InventoryItem)AssetDatabase.LoadAssetAtPath(path, typeof(InventoryItem));
				result.Add(itemToAdd);
			}
			string[] wearGuids = AssetDatabase.FindAssets("t: Wear", searchPath);
			for (int i = 0; i < wearGuids.Length; i++)
			{
				string path = AssetDatabase.GUIDToAssetPath(wearGuids[i]);
				InventoryItem itemToAdd = (InventoryItem)AssetDatabase.LoadAssetAtPath(path, typeof(InventoryItem));
				result.Add(itemToAdd);
			}
			string[] shieldGuids = AssetDatabase.FindAssets("t: Shield", searchPath);
			for (int i = 0; i < shieldGuids.Length; i++)
			{
				string path = AssetDatabase.GUIDToAssetPath(shieldGuids[i]);
				InventoryItem itemToAdd = (InventoryItem)AssetDatabase.LoadAssetAtPath(path, typeof(InventoryItem));
				result.Add(itemToAdd);
			}
			string[] meleeWeaponGuids = AssetDatabase.FindAssets("t: MeleeWeapon", searchPath);
			for (int i = 0; i < meleeWeaponGuids.Length; i++)
			{
				string path = AssetDatabase.GUIDToAssetPath(meleeWeaponGuids[i]);
				InventoryItem itemToAdd = (InventoryItem)AssetDatabase.LoadAssetAtPath(path, typeof(InventoryItem));
				result.Add(itemToAdd);
			}
			string[] quiverGuids = AssetDatabase.FindAssets("t: Quiver", searchPath);
			for (int i = 0; i < quiverGuids.Length; i++)
			{
				string path = AssetDatabase.GUIDToAssetPath(quiverGuids[i]);
				InventoryItem itemToAdd = (InventoryItem)AssetDatabase.LoadAssetAtPath(path, typeof(InventoryItem));
				result.Add(itemToAdd);
			}
			string[] packGuids = AssetDatabase.FindAssets("t: Pack", searchPath);
			for (int i = 0; i < packGuids.Length; i++)
			{
				string path = AssetDatabase.GUIDToAssetPath(packGuids[i]);
				InventoryItem itemToAdd = (InventoryItem)AssetDatabase.LoadAssetAtPath(path, typeof(InventoryItem));
				result.Add(itemToAdd);
			}
			string[] craftItemGuids = AssetDatabase.FindAssets("t: CraftItem", searchPath);
			for (int i = 0; i < craftItemGuids.Length; i++)
			{
				string path = AssetDatabase.GUIDToAssetPath(craftItemGuids[i]);
				InventoryItem itemToAdd = (InventoryItem)AssetDatabase.LoadAssetAtPath(path, typeof(InventoryItem));
				result.Add(itemToAdd);
			}
			return result;
		}

		bool IsAlreadyAdded(InventoryItem item, SerializedProperty entriesProp, out int indexFound){
			bool found = false;
			indexFound = -1;
			for (int i = 0; i < entriesProp.arraySize; i++)
			{
				ItemInstance entryItemInstance = (ItemInstance)entriesProp.GetArrayElementAtIndex(i).FindPropertyRelative("itemInstance").objectReferenceValue;
				SerializedObject itemInstanceSO = new SerializedObject(entryItemInstance);
				InventoryItem entryItem = (InventoryItem)itemInstanceSO.FindProperty("m_item").objectReferenceValue;
				
				if(item.itemId == entryItem.itemId){
					indexFound = i;
					found = true;
				}
			}
			return found;
		}

		void DrawItemInstanceEditor(){
			if(m_itemInstanceToEdit != null){
				SerializedObject itemInstanceSO = new SerializedObject(m_itemInstanceToEdit);
				InventoryItem item = (InventoryItem)itemInstanceSO.FindProperty("m_item").objectReferenceValue;
				if(item is Bow) DrawBowInstanceEditor(itemInstanceSO, (Bow)item);
				else if (item is Wear) DrawWearInstanceEditor(itemInstanceSO, (Wear)item);
				else if (item is Shield) DrawShieldInstanceEditor(itemInstanceSO, (Shield)item);
				else if (item is MeleeWeapon) DrawMeleeWeaponInstanceEditor(itemInstanceSO, (MeleeWeapon)item);
				else if (item is Quiver) DrawQuiverInstanceEditor(itemInstanceSO, (Quiver)item);
				else if (item is Pack) DrawPackInstanceEditor(itemInstanceSO, (Pack)item);
				else if (item is CraftItem) DrawCraftItemInstanceEditor(itemInstanceSO, (CraftItem)item);

			}else{
				EditorGUILayout.HelpBox("Select an item instance to edit", MessageType.Info);
			}
		}
		
		int drawStrengthLevel;
		int handlingLevel;
		int specialEffectLevel;
		void DrawBowInstanceEditor(SerializedObject bowInstSO, Bow bow){
			
			GUILayout.BeginVertical(GUI.skin.box);
				EditorGUILayout.LabelField(bowInstSO.targetObject.name);
				GUILayout.BeginVertical(GUI.skin.box);
					
					EditorGUILayout.LabelField("gear level", bowInstSO.FindProperty("gearLevel").intValue.ToString() + " / " + bow.maxLevel.ToString());

					BowInstance bowInst = (BowInstance)bowInstSO.targetObject;
					/*	DrawStrength
					*/
						GUILayout.BeginVertical(GUI.skin.box);
						bowInstSO.Update();
							EditorGUILayout.LabelField("DrawStrength");
						
								drawStrengthLevel = bowInstSO.FindProperty("m_drawStrengthLevel").intValue;
							if(bowInst.AvailablePowerUpSteps()> 0)
								drawStrengthLevel = EditorGUILayout.IntSlider(drawStrengthLevel, (int)bow.drawStrength.inputMin, (int)bow.drawStrength.inputMax);
							else
								drawStrengthLevel = EditorGUILayout.IntSlider(drawStrengthLevel + bowInst.AvailablePowerUpSteps(), (int)bow.drawStrength.inputMin, (int)bow.drawStrength.inputMax);

							
							bowInst.UpdateDrawStrengthLevel(drawStrengthLevel);
							
							
						EditorUtility.SetDirty(bowInst);
						EditorGUILayout.LabelField("draw strength value", bowInst.GetDrawStrengthValue((float)drawStrengthLevel).ToString());
						GUILayout.EndVertical();
					/*	Handling
					*/
						GUILayout.BeginVertical(GUI.skin.box);
						bowInstSO.Update();
							EditorGUILayout.LabelField("Handling");
						
								handlingLevel = bowInstSO.FindProperty("m_handlingLevel").intValue;
							if(bowInst.AvailablePowerUpSteps()> 0)
								handlingLevel = EditorGUILayout.IntSlider(handlingLevel, (int)bow.handling.inputMin, (int)bow.handling.inputMax);
							else
								handlingLevel = EditorGUILayout.IntSlider(handlingLevel + bowInst.AvailablePowerUpSteps(), (int)bow.handling.inputMin, (int)bow.handling.inputMax);

							
							bowInst.UpdateHandlingLevel(handlingLevel);
							
							
						EditorUtility.SetDirty(bowInst);
						EditorGUILayout.LabelField("handling value", bowInst.GetHandlingValue((float)handlingLevel).ToString());
						GUILayout.EndVertical();
					/*	specialEffect
					*/
						GUILayout.BeginVertical(GUI.skin.box);
						bowInstSO.Update();
							EditorGUILayout.LabelField("SpecialEffect");
						
								specialEffectLevel = bowInstSO.FindProperty("m_specialEffectLevel").intValue;
							if(bowInst.AvailablePowerUpSteps()> 0)
								specialEffectLevel = EditorGUILayout.IntSlider(specialEffectLevel, (int)bow.specialEffect.inputMin, (int)bow.specialEffect.inputMax);
							else
								specialEffectLevel = EditorGUILayout.IntSlider(specialEffectLevel + bowInst.AvailablePowerUpSteps(), (int)bow.specialEffect.inputMin, (int)bow.specialEffect.inputMax);

							
							bowInst.UpdateSpecialEffectLevel(specialEffectLevel);
							
							
						EditorUtility.SetDirty(bowInst);
						EditorGUILayout.LabelField("special effect value", bowInst.GetSpecialEffectValue((float)specialEffectLevel).ToString());
						GUILayout.EndVertical();




				GUILayout.EndVertical();
			GUILayout.EndVertical();
		}
		int armourLevel;
		int swiftnessLevel;
		int carriedGearEfficacyLevel;
		void DrawWearInstanceEditor(SerializedObject wearInstSO, Wear wear){
			
			GUILayout.BeginVertical(GUI.skin.box);
				EditorGUILayout.LabelField(wearInstSO.targetObject.name);
				GUILayout.BeginVertical(GUI.skin.box);
					
					EditorGUILayout.LabelField("gear level", wearInstSO.FindProperty("gearLevel").intValue.ToString() + " / " + wear.maxLevel.ToString());

					WearInstance wearInst = (WearInstance)wearInstSO.targetObject;
					/*	Armour
					*/
						GUILayout.BeginVertical(GUI.skin.box);
						wearInstSO.Update();
							EditorGUILayout.LabelField("Armour");
						
								armourLevel = wearInstSO.FindProperty("m_armourLevel").intValue;
							if(wearInst.AvailablePowerUpSteps()> 0)
								armourLevel = EditorGUILayout.IntSlider(armourLevel, (int)wear.armour.inputMin, (int)wear.armour.inputMax);
							else
								armourLevel = EditorGUILayout.IntSlider(armourLevel + wearInst.AvailablePowerUpSteps(), (int)wear.armour.inputMin, (int)wear.armour.inputMax);

							
							wearInst.UpdateArmourLevel(armourLevel);
							
							
						EditorUtility.SetDirty(wearInst);
						EditorGUILayout.LabelField("armour value", wearInst.GetArmourValue((float)armourLevel).ToString());
						GUILayout.EndVertical();
					/*	Swiftness
					*/
						GUILayout.BeginVertical(GUI.skin.box);
						wearInstSO.Update();
							EditorGUILayout.LabelField("Swiftness");
						
								swiftnessLevel = wearInstSO.FindProperty("m_swiftnessLevel").intValue;
							if(wearInst.AvailablePowerUpSteps()> 0)
								swiftnessLevel = EditorGUILayout.IntSlider(swiftnessLevel, (int)wear.swiftness.inputMin, (int)wear.swiftness.inputMax);
							else
								swiftnessLevel = EditorGUILayout.IntSlider(swiftnessLevel + wearInst.AvailablePowerUpSteps(), (int)wear.swiftness.inputMin, (int)wear.swiftness.inputMax);

							
							wearInst.UpdateSwiftnessLevel(swiftnessLevel);
							
							
						EditorUtility.SetDirty(wearInst);
						EditorGUILayout.LabelField("swiftness value", wearInst.GetSwiftnessValue((float)swiftnessLevel).ToString());
						GUILayout.EndVertical();
					/*	carriedGearEfficacy
					*/
						GUILayout.BeginVertical(GUI.skin.box);
						wearInstSO.Update();
							EditorGUILayout.LabelField("CarriedGearEfficacy");
						
								carriedGearEfficacyLevel = wearInstSO.FindProperty("m_carriedGearEfficacyLevel").intValue;
							if(wearInst.AvailablePowerUpSteps()> 0)
								carriedGearEfficacyLevel = EditorGUILayout.IntSlider(carriedGearEfficacyLevel, (int)wear.carriedGearEfficacy.inputMin, (int)wear.carriedGearEfficacy.inputMax);
							else
								carriedGearEfficacyLevel = EditorGUILayout.IntSlider(carriedGearEfficacyLevel + wearInst.AvailablePowerUpSteps(), (int)wear.carriedGearEfficacy.inputMin, (int)wear.carriedGearEfficacy.inputMax);

							
							wearInst.UpdateCarriedGearEfficacyLevel(carriedGearEfficacyLevel);
							
							
						EditorUtility.SetDirty(wearInst);
						EditorGUILayout.LabelField("carried gear efficacy value", wearInst.GetCarriedGearEfficacyValue((float)carriedGearEfficacyLevel).ToString());
						GUILayout.EndVertical();


				GUILayout.EndVertical();
			GUILayout.EndVertical();
		}
		int shieldLongevityLevel;
		int sturdinessLevel;
		int deflectionLevel;
		void DrawShieldInstanceEditor(SerializedObject shieldInstSO, Shield shield){
			
			GUILayout.BeginVertical(GUI.skin.box);
				EditorGUILayout.LabelField(shieldInstSO.targetObject.name);
				GUILayout.BeginVertical(GUI.skin.box);
					
					EditorGUILayout.LabelField("gear level", shieldInstSO.FindProperty("gearLevel").intValue.ToString() + " / " + shield.maxLevel.ToString());

					ShieldInstance shieldInst = (ShieldInstance)shieldInstSO.targetObject;
					/*	Longevity
					*/
						GUILayout.BeginVertical(GUI.skin.box);
						shieldInstSO.Update();
							EditorGUILayout.LabelField("Longevity");
						
								shieldLongevityLevel = shieldInstSO.FindProperty("m_longevityLevel").intValue;
							if(shieldInst.AvailablePowerUpSteps()> 0)
								shieldLongevityLevel = EditorGUILayout.IntSlider(shieldLongevityLevel, (int)shield.longevity.inputMin, (int)shield.longevity.inputMax);
							else
								shieldLongevityLevel = EditorGUILayout.IntSlider(shieldLongevityLevel + shieldInst.AvailablePowerUpSteps(), (int)shield.longevity.inputMin, (int)shield.longevity.inputMax);

							
							shieldInst.UpdateLongevityLevel(shieldLongevityLevel);
							
							
						EditorUtility.SetDirty(shieldInst);
						EditorGUILayout.LabelField("longevity value", shieldInst.GetLongevityValue((float)shieldLongevityLevel).ToString());
						GUILayout.EndVertical();
					/*	Sturdiness
					*/
						GUILayout.BeginVertical(GUI.skin.box);
						shieldInstSO.Update();
							EditorGUILayout.LabelField("Sturdiness");
						
								sturdinessLevel = shieldInstSO.FindProperty("m_sturdinessLevel").intValue;
							if(shieldInst.AvailablePowerUpSteps()> 0)
								sturdinessLevel = EditorGUILayout.IntSlider(sturdinessLevel, (int)shield.sturdiness.inputMin, (int)shield.sturdiness.inputMax);
							else
								sturdinessLevel = EditorGUILayout.IntSlider(sturdinessLevel + shieldInst.AvailablePowerUpSteps(), (int)shield.sturdiness.inputMin, (int)shield.sturdiness.inputMax);

							
							shieldInst.UpdateSturdinessLevel(sturdinessLevel);
							
							
						EditorUtility.SetDirty(shieldInst);
						EditorGUILayout.LabelField("sturdiness value", shieldInst.GetSturdinessValue((float)sturdinessLevel).ToString());
						GUILayout.EndVertical();
					/*	deflection
					*/
						GUILayout.BeginVertical(GUI.skin.box);
						shieldInstSO.Update();
							EditorGUILayout.LabelField("Deflection");
						
								deflectionLevel = shieldInstSO.FindProperty("m_deflectionLevel").intValue;
							if(shieldInst.AvailablePowerUpSteps()> 0)
								deflectionLevel = EditorGUILayout.IntSlider(deflectionLevel, (int)shield.deflection.inputMin, (int)shield.deflection.inputMax);
							else
								deflectionLevel = EditorGUILayout.IntSlider(deflectionLevel + shieldInst.AvailablePowerUpSteps(), (int)shield.deflection.inputMin, (int)shield.deflection.inputMax);

							
							shieldInst.UpdateDeflectionLevel(deflectionLevel);
							
							
						EditorUtility.SetDirty(shieldInst);
						EditorGUILayout.LabelField("deflection value", shieldInst.GetDeflectionValue((float)deflectionLevel).ToString());
						GUILayout.EndVertical();

				GUILayout.EndVertical();
			GUILayout.EndVertical();
		}
		int meleeLongevityLevel;
		int knockPowerLevel;
		int fireRateLevel;
		void DrawMeleeWeaponInstanceEditor(SerializedObject meleeWeaponInstSO, MeleeWeapon meleeWeapon){
			
			GUILayout.BeginVertical(GUI.skin.box);
				EditorGUILayout.LabelField(meleeWeaponInstSO.targetObject.name);
				GUILayout.BeginVertical(GUI.skin.box);
					
					EditorGUILayout.LabelField("gear level", meleeWeaponInstSO.FindProperty("gearLevel").intValue.ToString() + " / " + meleeWeapon.maxLevel.ToString());

					MeleeWeaponInstance meleeWeaponInst = (MeleeWeaponInstance)meleeWeaponInstSO.targetObject;
					/*	meleeLongevity
					*/
						GUILayout.BeginVertical(GUI.skin.box);
						meleeWeaponInstSO.Update();
							EditorGUILayout.LabelField("meleeLongevity");
						
								meleeLongevityLevel = meleeWeaponInstSO.FindProperty("m_longevityLevel").intValue;
							if(meleeWeaponInst.AvailablePowerUpSteps()> 0)
								meleeLongevityLevel = EditorGUILayout.IntSlider(meleeLongevityLevel, (int)meleeWeapon.longevity.inputMin, (int)meleeWeapon.longevity.inputMax);
							else
								meleeLongevityLevel = EditorGUILayout.IntSlider(meleeLongevityLevel + meleeWeaponInst.AvailablePowerUpSteps(), (int)meleeWeapon.longevity.inputMin, (int)meleeWeapon.longevity.inputMax);

							
							meleeWeaponInst.UpdateLongevityLevel(meleeLongevityLevel);
							
							
						EditorUtility.SetDirty(meleeWeaponInst);
						EditorGUILayout.LabelField("longevity value", meleeWeaponInst.GetLongevityValue((float)meleeLongevityLevel).ToString());
						GUILayout.EndVertical();
					/*	KnockPower
					*/
						GUILayout.BeginVertical(GUI.skin.box);
						meleeWeaponInstSO.Update();
							EditorGUILayout.LabelField("KnockPower");
						
								knockPowerLevel = meleeWeaponInstSO.FindProperty("m_knockPowerLevel").intValue;
							if(meleeWeaponInst.AvailablePowerUpSteps()> 0)
								knockPowerLevel = EditorGUILayout.IntSlider(knockPowerLevel, (int)meleeWeapon.knockPower.inputMin, (int)meleeWeapon.knockPower.inputMax);
							else
								knockPowerLevel = EditorGUILayout.IntSlider(knockPowerLevel + meleeWeaponInst.AvailablePowerUpSteps(), (int)meleeWeapon.knockPower.inputMin, (int)meleeWeapon.knockPower.inputMax);

							
							meleeWeaponInst.UpdateKnockPowerLevel(knockPowerLevel);
							
							
						EditorUtility.SetDirty(meleeWeaponInst);
						EditorGUILayout.LabelField("knockPower value", meleeWeaponInst.GetKnockPowerValue((float)knockPowerLevel).ToString());
						GUILayout.EndVertical();
					/*	fireRate
					*/
						GUILayout.BeginVertical(GUI.skin.box);
						meleeWeaponInstSO.Update();
							EditorGUILayout.LabelField("FireRate");
						
								fireRateLevel = meleeWeaponInstSO.FindProperty("m_fireRateLevel").intValue;
							if(meleeWeaponInst.AvailablePowerUpSteps()> 0)
								fireRateLevel = EditorGUILayout.IntSlider(fireRateLevel, (int)meleeWeapon.fireRate.inputMin, (int)meleeWeapon.fireRate.inputMax);
							else
								fireRateLevel = EditorGUILayout.IntSlider(fireRateLevel + meleeWeaponInst.AvailablePowerUpSteps(), (int)meleeWeapon.fireRate.inputMin, (int)meleeWeapon.fireRate.inputMax);

							
							meleeWeaponInst.UpdateFireRateLevel(fireRateLevel);
							
							
						EditorUtility.SetDirty(meleeWeaponInst);
						EditorGUILayout.LabelField("fireRate value", meleeWeaponInst.GetFireRateValue((float)fireRateLevel).ToString());
						GUILayout.EndVertical();

				GUILayout.EndVertical();
			GUILayout.EndVertical();
		}
		int effectsEfficacyLevel;
		int roundsLevel;
		void DrawQuiverInstanceEditor(SerializedObject quiverInstSO, Quiver quiver){
			
			GUILayout.BeginVertical(GUI.skin.box);
				EditorGUILayout.LabelField(quiverInstSO.targetObject.name);
				GUILayout.BeginVertical(GUI.skin.box);
					
					EditorGUILayout.LabelField("gear level", quiverInstSO.FindProperty("gearLevel").intValue.ToString() + " / " + quiver.maxLevel.ToString());

					QuiverInstance quiverInst = (QuiverInstance)quiverInstSO.targetObject;
					/*	quiver
					*/
						GUILayout.BeginVertical(GUI.skin.box);
							EditorGUILayout.LabelField("Added Effects", GUILayout.Width(100f));
							for (int i = 0; i < quiver.addedEffects.Count; i++)
							{
								GUILayout.BeginHorizontal(GUI.skin.textField);
									EditorGUILayout.LabelField(quiver.addedEffects[i].ToString());
								GUILayout.EndHorizontal();
							}
						GUILayout.EndVertical();
					/*	effectsEfficacy
					*/
						GUILayout.BeginVertical(GUI.skin.box);
						quiverInstSO.Update();
							EditorGUILayout.LabelField("effectsEfficacy");
						
								effectsEfficacyLevel = quiverInstSO.FindProperty("m_effectsEfficacyLevel").intValue;
							if(quiverInst.AvailablePowerUpSteps()> 0)
								effectsEfficacyLevel = EditorGUILayout.IntSlider(effectsEfficacyLevel, (int)quiver.effectsEfficacy.inputMin, (int)quiver.effectsEfficacy.inputMax);
							else
								effectsEfficacyLevel = EditorGUILayout.IntSlider(effectsEfficacyLevel + quiverInst.AvailablePowerUpSteps(), (int)quiver.effectsEfficacy.inputMin, (int)quiver.effectsEfficacy.inputMax);

							
							quiverInst.UpdateEffectsEfficacyLevel(effectsEfficacyLevel);
							
							
						EditorUtility.SetDirty(quiverInst);
						EditorGUILayout.LabelField("effectsEfficacy value", quiverInst.GetEffectsEfficacyValue((float)effectsEfficacyLevel).ToString());
						GUILayout.EndVertical();
					/*	Rounds
					*/
						GUILayout.BeginVertical(GUI.skin.box);
						quiverInstSO.Update();
							EditorGUILayout.LabelField("Rounds");
						
								roundsLevel = quiverInstSO.FindProperty("m_roundsLevel").intValue;
							if(quiverInst.AvailablePowerUpSteps()> 0)
								roundsLevel = EditorGUILayout.IntSlider(roundsLevel, (int)quiver.rounds.inputMin, (int)quiver.rounds.inputMax);
							else
								roundsLevel = EditorGUILayout.IntSlider(roundsLevel + quiverInst.AvailablePowerUpSteps(), (int)quiver.rounds.inputMin, (int)quiver.rounds.inputMax);

							
							quiverInst.UpdateRoundsLevel(roundsLevel);
							
							
						EditorUtility.SetDirty(quiverInst);
						EditorGUILayout.LabelField("rounds value", quiverInst.GetRoundsValue((float)roundsLevel).ToString());
						GUILayout.EndVertical();
					

				GUILayout.EndVertical();
			GUILayout.EndVertical();
		}
		int efficacyLevel;
		void DrawPackInstanceEditor(SerializedObject packInstSO, Pack pack){
			
			GUILayout.BeginVertical(GUI.skin.box);
				EditorGUILayout.LabelField(packInstSO.targetObject.name);
				GUILayout.BeginVertical(GUI.skin.box);
					
					EditorGUILayout.LabelField("gear level", packInstSO.FindProperty("gearLevel").intValue.ToString() + " / " + pack.maxLevel.ToString());

					PackInstance packInst = (PackInstance)packInstSO.targetObject;
					/*	Loot Bonus
					*/
						GUILayout.BeginVertical(GUI.skin.box);
							EditorGUILayout.LabelField("Loot Bonus", GUILayout.Width(100f));
							for (int i = 0; i < pack.lootBonus.Count; i++)
							{
								GUILayout.BeginHorizontal(GUI.skin.textField);
								EditorGUILayout.LabelField(pack.lootBonus[i].ToString(), GUILayout.Width(100f));
								GUILayout.EndHorizontal();
							}
						GUILayout.EndVertical();
					/*	Bonus Trigger
					*/
						GUILayout.BeginHorizontal(GUI.skin.box);
							EditorGUILayout.LabelField("Bonus Trigger", GUILayout.Width(100f));
							GUILayout.FlexibleSpace();
							EditorGUILayout.LabelField(pack.bonusTrigger.ToString(), GUILayout.Width(100f));
						GUILayout.EndHorizontal();

					/*	efficacy
					*/
						GUILayout.BeginVertical(GUI.skin.box);
						packInstSO.Update();
							EditorGUILayout.LabelField("Efficacy");
						
								efficacyLevel = packInstSO.FindProperty("m_efficacyLevel").intValue;
							if(packInst.AvailablePowerUpSteps()> 0)
								efficacyLevel = EditorGUILayout.IntSlider(efficacyLevel, (int)pack.efficacy.inputMin, (int)pack.efficacy.inputMax);
							else
								efficacyLevel = EditorGUILayout.IntSlider(efficacyLevel + packInst.AvailablePowerUpSteps(), (int)pack.efficacy.inputMin, (int)pack.efficacy.inputMax);

							
							packInst.UpdateEfficacyLevel(efficacyLevel);
							
							
						EditorUtility.SetDirty(packInst);
						EditorGUILayout.LabelField("efficacy value", packInst.GetEfficacyValue((float)efficacyLevel).ToString());
						GUILayout.EndVertical();
					
				GUILayout.EndVertical();
			GUILayout.EndVertical();
		}
		void DrawCraftItemInstanceEditor(SerializedObject craftItemInstSO, CraftItem craftItem){
			GUILayout.BeginVertical(GUI.skin.box);
				EditorGUILayout.LabelField(craftItemInstSO.targetObject.name);
				GUILayout.BeginVertical(GUI.skin.box);
					for (int i = 0; i < craftItem.attributeBonusList.Count; i++)
					{
						GUILayout.BeginHorizontal(GUI.skin.textField);
							EditorGUILayout.LabelField(craftItem.attributeBonusList[i].attCurveId.ToString(), GUILayout.Width(100f));
							GUILayout.FlexibleSpace();
							EditorGUILayout.LabelField(craftItem.attributeBonusList[i].addedBonus.ToString(), GUILayout.Width(50f));
						GUILayout.EndHorizontal();
					}
				GUILayout.EndVertical();
			GUILayout.EndVertical();

		}

		void ReorderEntries(ref SerializedProperty entriesProp){
			
			int origCount = entriesProp.arraySize;
			List<InventoryItemEntry> temp = new List<InventoryItemEntry>();
			
			while(temp.Count < origCount){
				
				int idAtMin = -1;	
				// InventoryItem itemWithMinId = null;
				ItemInstance itemWithMinId = null;
				int quantAtMin = -1; 
				for (int i = 0; i < entriesProp.arraySize; i++)
				{	
					
					ItemInstance itemInstance = (ItemInstance)entriesProp.GetArrayElementAtIndex(i).FindPropertyRelative("itemInstance").objectReferenceValue;
					SerializedObject itemInstanceSO = new SerializedObject(itemInstance);
					
					
					InventoryItem item = (InventoryItem)itemInstanceSO.FindProperty("m_item").objectReferenceValue;
					// InventoryItem item = (InventoryItem)entriesProp.GetArrayElementAtIndex(i).FindPropertyRelative("item").objectReferenceValue;
					
					if(item == null)
						continue;
					int quant = entriesProp.GetArrayElementAtIndex(i).FindPropertyRelative("quantity").intValue;
					if(itemWithMinId == null || itemWithMinId.m_item.itemId > item.itemId){
						idAtMin = i;
						itemWithMinId = itemInstance;
						quantAtMin = quant;
						itemInstanceSO.FindProperty("m_item").objectReferenceValue = null;
					}
				
				}
				
				InventoryItemEntry minEntry = new InventoryItemEntry();
				minEntry.itemInstance = itemWithMinId;
				minEntry.quantity = quantAtMin;
				temp.Add(minEntry);
			}

			while(entriesProp.arraySize > 0){
				entriesProp.DeleteArrayElementAtIndex(0);
			}

			for (int i = 0; i < temp.Count; i++)
			{
				entriesProp.InsertArrayElementAtIndex(i);
				entriesProp.GetArrayElementAtIndex(i).FindPropertyRelative("itemInstance").objectReferenceValue = temp[i].itemInstance;
				entriesProp.GetArrayElementAtIndex(i).FindPropertyRelative("quantity").intValue = temp[i].quantity;
			}

		}

		void ReorderEntriesRevised(ref SerializedProperty entriesProp){
			
			List<InventoryItemEntry> bowEntryList = new List<InventoryItemEntry>();
			string[] searchPath = new string[]{m_itemInstancesPath};
			string[] guis = AssetDatabase.FindAssets("t: BowInstance", searchPath);
			for (int i = 0; i < guis.Length; i++)
			{
				BowInstance inst = (BowInstance)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guis[i]), typeof(BowInstance));
				int quant = -1;
				for (int j = 0; j < entriesProp.arraySize; j++)
				{
					if(entriesProp.GetArrayElementAtIndex(j).FindPropertyRelative("itemInstance").objectReferenceValue == inst)
						quant = entriesProp.GetArrayElementAtIndex(j).FindPropertyRelative("quantity").intValue;

				}
				if(quant != -1){

				InventoryItemEntry newEntry = new InventoryItemEntry();
				newEntry.itemInstance = inst;
				newEntry.quantity = quant;

				bowEntryList.Add(newEntry);
				}
			}

			
			List<InventoryItemEntry> wearEntryList = new List<InventoryItemEntry>();
			searchPath = new string[]{m_itemInstancesPath};
			guis = AssetDatabase.FindAssets("t: WearInstance", searchPath);
			for (int i = 0; i < guis.Length; i++)
			{
				WearInstance inst = (WearInstance)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guis[i]), typeof(WearInstance));
				int quant = -1;
				for (int j = 0; j < entriesProp.arraySize; j++)
				{
					if(entriesProp.GetArrayElementAtIndex(j).FindPropertyRelative("itemInstance").objectReferenceValue == inst)
						quant = entriesProp.GetArrayElementAtIndex(j).FindPropertyRelative("quantity").intValue;

				}
				if(quant != -1){

				InventoryItemEntry newEntry = new InventoryItemEntry();
				newEntry.itemInstance = inst;
				newEntry.quantity = quant;

				wearEntryList.Add(newEntry);
				}
			}

			List<InventoryItemEntry> shieldEntryList = new List<InventoryItemEntry>();
			searchPath = new string[]{m_itemInstancesPath};
			guis = AssetDatabase.FindAssets("t: ShieldInstance", searchPath);
			for (int i = 0; i < guis.Length; i++)
			{
				ShieldInstance inst = (ShieldInstance)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guis[i]), typeof(ShieldInstance));
				int quant = -1;
				for (int j = 0; j < entriesProp.arraySize; j++)
				{
					if(entriesProp.GetArrayElementAtIndex(j).FindPropertyRelative("itemInstance").objectReferenceValue == inst)
						quant = entriesProp.GetArrayElementAtIndex(j).FindPropertyRelative("quantity").intValue;

				}
				if(quant != -1){

				InventoryItemEntry newEntry = new InventoryItemEntry();
				newEntry.itemInstance = inst;
				newEntry.quantity = quant;

				shieldEntryList.Add(newEntry);
				}
			}


			List<InventoryItemEntry> meleeWeaponEntryList = new List<InventoryItemEntry>();
			searchPath = new string[]{m_itemInstancesPath};
			guis = AssetDatabase.FindAssets("t: MeleeWeaponInstance", searchPath);
			for (int i = 0; i < guis.Length; i++)
			{
				MeleeWeaponInstance inst = (MeleeWeaponInstance)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guis[i]), typeof(MeleeWeaponInstance));
				int quant = -1;
				for (int j = 0; j < entriesProp.arraySize; j++)
				{
					if(entriesProp.GetArrayElementAtIndex(j).FindPropertyRelative("itemInstance").objectReferenceValue == inst)
						quant = entriesProp.GetArrayElementAtIndex(j).FindPropertyRelative("quantity").intValue;

				}
				if(quant != -1){

				InventoryItemEntry newEntry = new InventoryItemEntry();
				newEntry.itemInstance = inst;
				newEntry.quantity = quant;

				meleeWeaponEntryList.Add(newEntry);
				}
				
			}

			List<InventoryItemEntry> quiverEntryList = new List<InventoryItemEntry>();
			searchPath = new string[]{m_itemInstancesPath};
			guis = AssetDatabase.FindAssets("t: QuiverInstance", searchPath);
			for (int i = 0; i < guis.Length; i++)
			{
				QuiverInstance inst = (QuiverInstance)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guis[i]), typeof(QuiverInstance));
				int quant = -1;
				for (int j = 0; j < entriesProp.arraySize; j++)
				{
					if(entriesProp.GetArrayElementAtIndex(j).FindPropertyRelative("itemInstance").objectReferenceValue == inst)
						quant = entriesProp.GetArrayElementAtIndex(j).FindPropertyRelative("quantity").intValue;

				}
				
				if(quant != -1){

				InventoryItemEntry newEntry = new InventoryItemEntry();
				newEntry.itemInstance = inst;
				newEntry.quantity = quant;

				quiverEntryList.Add(newEntry);
				}
			}

			List<InventoryItemEntry> packEntryList = new List<InventoryItemEntry>();
			searchPath = new string[]{m_itemInstancesPath};
			guis = AssetDatabase.FindAssets("t: PackInstance", searchPath);
			for (int i = 0; i < guis.Length; i++)
			{
				PackInstance inst = (PackInstance)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guis[i]), typeof(PackInstance));
				int quant = -1;
				for (int j = 0; j < entriesProp.arraySize; j++)
				{
					if(entriesProp.GetArrayElementAtIndex(j).FindPropertyRelative("itemInstance").objectReferenceValue == inst)
						quant = entriesProp.GetArrayElementAtIndex(j).FindPropertyRelative("quantity").intValue;

				}
				if(quant != -1){

				InventoryItemEntry newEntry = new InventoryItemEntry();
				newEntry.itemInstance = inst;
				newEntry.quantity = quant;

				packEntryList.Add(newEntry);
				}
			}

			List<InventoryItemEntry> craftItemEntryList = new List<InventoryItemEntry>();
			searchPath = new string[]{m_itemInstancesPath};
			guis = AssetDatabase.FindAssets("t: CraftItemInstance", searchPath);
			for (int i = 0; i < guis.Length; i++)
			{
				CraftItemInstance inst = (CraftItemInstance)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guis[i]), typeof(CraftItemInstance));
				int quant = -1;
				for (int j = 0; j < entriesProp.arraySize; j++)
				{
					if(entriesProp.GetArrayElementAtIndex(j).FindPropertyRelative("itemInstance").objectReferenceValue == inst)
						quant = entriesProp.GetArrayElementAtIndex(j).FindPropertyRelative("quantity").intValue;

				}
				if(quant != -1){

				InventoryItemEntry newEntry = new InventoryItemEntry();
				newEntry.itemInstance = inst;
				newEntry.quantity = quant;

				craftItemEntryList.Add(newEntry);
				}
			}
			
			List<InventoryItemEntry> entryList = new List<InventoryItemEntry>();
			entryList.AddRange(bowEntryList);
			entryList.AddRange(wearEntryList);
			entryList.AddRange(shieldEntryList);
			entryList.AddRange(meleeWeaponEntryList);
			entryList.AddRange(quiverEntryList);
			entryList.AddRange(packEntryList);
			entryList.AddRange(craftItemEntryList);

			// List<InventoryItemEntry> newEntriesList = new List<InventoryItemEntry>();

			// int count = 0;
			// while(count < entriesProp.arraySize){

			// 	for (int i = 0; i < entryList.Count; i++)
			// 	{
			// 		if(entryList[i].itemInstance == (ItemInstance)entriesProp.GetArrayElementAtIndex(count).FindPropertyRelative("itemInstance").objectReferenceValue){
			// 			newEntriesList.Add(entryList[i]);
			// 			count++;
			// 		}
			// 	}
			// }
			
			while(entriesProp.arraySize > 0){
				entriesProp.DeleteArrayElementAtIndex(0);
			}
			for (int i = 0; i < entryList.Count; i++)
			{
				entriesProp.InsertArrayElementAtIndex(i);
				entriesProp.GetArrayElementAtIndex(i).FindPropertyRelative("itemInstance").objectReferenceValue = entryList/*newEntriesList*/[i].itemInstance;
				entriesProp.GetArrayElementAtIndex(i).FindPropertyRelative("quantity").intValue = entryList/*newEntriesList*/[i].quantity;
			}
		}

		// void ReorderEntriesRevisedAgain(ref SerializedProperty entriesProp){
		// 	for(int i =0; i < entriesProp.arraySize; i++){

		// 	}
		// }

		string m_itemInstancesPath = "Assets/InventorySystem/ScriptableObjects/ItemInstances";
		ItemInstance CreateNewItemInstance(InventoryItem item){
			ItemInstance result = null;
			if(item is Bow){
				BowInstance bowInstance = ScriptableObject.CreateInstance<BowInstance>();
				bowInstance.Initialize((Bow)item);
				
				int count = 0;
				string newPathWithSuffix = null;
				while(true){
					string suffix = count ==0? "": count.ToString();
					string newPath =  m_itemInstancesPath + "/Bow/" + item.itemName + "_instance" + suffix +".asset";

					BowInstance sameName = (BowInstance)AssetDatabase.LoadAssetAtPath(newPath, typeof(BowInstance));
					if(sameName == null){
						newPathWithSuffix = newPath;
						break;
					}
					count++;
				}

				AssetDatabase.CreateAsset(bowInstance,newPathWithSuffix);
				AssetDatabase.SaveAssets();
				result = bowInstance;
			}
			else if(item is Wear){
				WearInstance wearInstance = ScriptableObject.CreateInstance<WearInstance>();
				wearInstance.Initialize((Wear)item);
				
				int count = 0;
				string newPathWithSuffix = null;
				while(true){
					string suffix = count ==0? "": count.ToString();
					string newPath =  m_itemInstancesPath + "/Wear/" + item.itemName + "_instance" + suffix +".asset";

					WearInstance sameName = (WearInstance)AssetDatabase.LoadAssetAtPath(newPath, typeof(WearInstance));
					if(sameName == null){
						newPathWithSuffix = newPath;
						break;
					}
					count++;
				}

				AssetDatabase.CreateAsset(wearInstance,newPathWithSuffix);
				AssetDatabase.SaveAssets();
				result = wearInstance;
			}
			else if(item is Shield){
				ShieldInstance shieldInstance = ScriptableObject.CreateInstance<ShieldInstance>();
				shieldInstance.Initialize((Shield)item);
				
				int count = 0;
				string newPathWithSuffix = null;
				while(true){
					string suffix = count ==0? "": count.ToString();
					string newPath =  m_itemInstancesPath + "/Shield/" + item.itemName + "_instance" + suffix +".asset";

					ShieldInstance sameName = (ShieldInstance)AssetDatabase.LoadAssetAtPath(newPath, typeof(ShieldInstance));
					if(sameName == null){
						newPathWithSuffix = newPath;
						break;
					}
					count++;
				}

				AssetDatabase.CreateAsset(shieldInstance,newPathWithSuffix);
				AssetDatabase.SaveAssets();
				result = shieldInstance;
			}
			else if(item is MeleeWeapon){
				MeleeWeaponInstance meleeWeaponInstance = ScriptableObject.CreateInstance<MeleeWeaponInstance>();
				meleeWeaponInstance.Initialize((MeleeWeapon)item);
				
				int count = 0;
				string newPathWithSuffix = null;
				while(true){
					string suffix = count ==0? "": count.ToString();
					string newPath =  m_itemInstancesPath + "/MeleeWeapon/" + item.itemName + "_instance" + suffix +".asset";

					MeleeWeaponInstance sameName = (MeleeWeaponInstance)AssetDatabase.LoadAssetAtPath(newPath, typeof(MeleeWeaponInstance));
					if(sameName == null){
						newPathWithSuffix = newPath;
						break;
					}
					count++;
				}

				AssetDatabase.CreateAsset(meleeWeaponInstance,newPathWithSuffix);
				AssetDatabase.SaveAssets();
				result = meleeWeaponInstance;
			}
			else if(item is Quiver){
				QuiverInstance quiverInstance = ScriptableObject.CreateInstance<QuiverInstance>();
				quiverInstance.Initialize((Quiver)item);
				
				int count = 0;
				string newPathWithSuffix = null;
				while(true){
					string suffix = count ==0? "": count.ToString();
					string newPath =  m_itemInstancesPath + "/Quiver/" + item.itemName + "_instance" + suffix +".asset";

					QuiverInstance sameName = (QuiverInstance)AssetDatabase.LoadAssetAtPath(newPath, typeof(QuiverInstance));
					if(sameName == null){
						newPathWithSuffix = newPath;
						break;
					}
					count++;
				}

				AssetDatabase.CreateAsset(quiverInstance,newPathWithSuffix);
				AssetDatabase.SaveAssets();
				result = quiverInstance;
			}
			else if(item is Pack){
				PackInstance packInstance = ScriptableObject.CreateInstance<PackInstance>();
				packInstance.Initialize((Pack)item);
				
				int count = 0;
				string newPathWithSuffix = null;
				while(true){
					string suffix = count ==0? "": count.ToString();
					string newPath =  m_itemInstancesPath + "/Pack/" + item.itemName + "_instance" + suffix +".asset";

					PackInstance sameName = (PackInstance)AssetDatabase.LoadAssetAtPath(newPath, typeof(PackInstance));
					if(sameName == null){
						newPathWithSuffix = newPath;
						break;
					}
					count++;
				}

				AssetDatabase.CreateAsset(packInstance,newPathWithSuffix);
				AssetDatabase.SaveAssets();
				result = packInstance;
			}
			else if(item is CraftItem){
				CraftItemInstance craftItemInstance = ScriptableObject.CreateInstance<CraftItemInstance>();
				craftItemInstance.Initialize((CraftItem)item);
				
				int count = 0;
				string newPathWithSuffix = null;
				while(true){
					string suffix = count ==0? "": count.ToString();
					string newPath =  m_itemInstancesPath + "/CraftItem/" + item.itemName + "_instance" + suffix +".asset";

					CraftItemInstance sameName = (CraftItemInstance)AssetDatabase.LoadAssetAtPath(newPath, typeof(CraftItemInstance));
					if(sameName == null){
						newPathWithSuffix = newPath;
						break;
					}
					count++;
				}

				AssetDatabase.CreateAsset(craftItemInstance,newPathWithSuffix);
				AssetDatabase.SaveAssets();
				result = craftItemInstance;
			}
			return result;
		}

		
	}

}
