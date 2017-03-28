using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem;
using UnityEngine.EventSystems;
using System.Linq;
using MyUtility;
public class SlotGroupManager : MonoBehaviour {

	public List<SlotGroup> m_slotGroups;

	void Start(){
		if(!m_slotGroups.Any()){
			DebugUtility.PrintRed("The slotGroups list is empty");
		}
		cam = FindObjectOfType<Camera>();
		InitInventoryEntries();
		InitSlotGroups();
	}
	
	public void InitInventoryEntries(){
		/*	Get itemInstance that is currently in the entries of each inventory
			identify the same itemInstance (there's gotta be one) in the eantry of Pool inventory
			mark each as Equipped and decrease the quantity.
			Do keep in mind this method is going to get extended to be used when Equipment Sets are swapped
		*/
		PlayerInventory bowInventory = GetCurrentlyFocusedInventory(SlotGroupType.Bow);
		if(bowInventory.entries.Count > 0){

			BowInstance equippedBow = (BowInstance)bowInventory.entries[0].itemInstance;
			PlayerInventory poolInventory = GetCurrentlyFocusedInventory(SlotGroupType.Pool);
			for (int i = 0; i < poolInventory.entries.Count; i++)
			{
				if(poolInventory.entries[i].itemInstance == equippedBow){
					
					if(poolInventory.entries[i].quantity >0)
					poolInventory.entries[i].quantity -= bowInventory.entries[0].quantity;
				}

			}
		}
		
		PlayerInventory wearInventory = GetCurrentlyFocusedInventory(SlotGroupType.Wear);
		if(wearInventory.entries.Count > 0){

			WearInstance equippedWear = (WearInstance)wearInventory.entries[0].itemInstance;
			PlayerInventory poolInventory = GetCurrentlyFocusedInventory(SlotGroupType.Pool);
			for (int i = 0; i < poolInventory.entries.Count; i++)
			{
				if(poolInventory.entries[i].itemInstance == equippedWear){
					
					if(poolInventory.entries[i].quantity >0)
					poolInventory.entries[i].quantity -= wearInventory.entries[0].quantity;
				}

			}
		}

	}

	PlayerInventory GetCurrentlyFocusedInventory(SlotGroupType slotGroupType){
		for (int i = 0; i < m_slotGroups.Count; i++)
		{
			
			switch(slotGroupType){
				case SlotGroupType.Bow:
					if(m_slotGroups[i].m_slotGroupType == SlotGroupType.Bow)
						return m_slotGroups[i].inventory;	
				break;
				
				case SlotGroupType.Wear:
					if(m_slotGroups[i].m_slotGroupType == SlotGroupType.Wear)
						return m_slotGroups[i].inventory;	
				break;

				case SlotGroupType.Pool:
					if(m_slotGroups[i].m_slotGroupType == SlotGroupType.Pool)
						return m_slotGroups[i].inventory;
				break;
			}
		}
		return null;
	}

	void InitSlotGroups(){
		for (int i = 0; i < m_slotGroups.Count; i++)
		{
			m_slotGroups[i].InitSlots();
			m_slotGroups[i].Initialize();
		}
	}

	public void FilterDestination(Slottable slottable){
		SetActiveStateAll(false);
		InventoryItem invItem = slottable.m_Item;
		if(invItem is Bow){
			ActivateSlotGroup(SlotGroupType.Bow);
		}else if(invItem is Wear){
			ActivateSlotGroup(SlotGroupType.Wear);
		}else if(invItem is CarriedGear){
			ActivateSlotGroup(SlotGroupType.CarriedGear);
		}
		slottable.m_OrigSG.SetActiveForSelectionState(true);
		ActivateSlotGroup(SlotGroupType.Pool);
		FilterPoolSGSlottables(slottable);
	}

	void FilterPoolSGSlottables(Slottable slottable){
		for (int i = 0; i < m_slotGroups.Count; i++)
		{
			if(m_slotGroups[i].m_slotGroupType == SlotGroupType.Pool){
				m_slotGroups[i].FilterSlottables(slottable);
			}
		}
	}

	void ActivateSlotGroup(SlotGroupType slotGroupType){
		for (int i = 0; i < m_slotGroups.Count; i++)
		{
			switch(slotGroupType){
				
				case SlotGroupType.Bow:
					if(m_slotGroups[i].m_slotGroupType == SlotGroupType.Bow)
						m_slotGroups[i].SetActiveForSelectionState(true);
				break;
				
				case SlotGroupType.Wear:
					if(m_slotGroups[i].m_slotGroupType == SlotGroupType.Wear)
						m_slotGroups[i].SetActiveForSelectionState(true);
				break;
				
				case SlotGroupType.CarriedGear:
					if(m_slotGroups[i].m_slotGroupType == SlotGroupType.CarriedGear)
						m_slotGroups[i].SetActiveForSelectionState(true);
				break;
				
				case SlotGroupType.CraftItems:
				break;

				case SlotGroupType.Pool:
					if(m_slotGroups[i].m_slotGroupType == SlotGroupType.Pool)
						m_slotGroups[i].SetActiveForSelectionState(true);
				break;

				default:
				break;
			}
		}
	}
	public void SetActiveStateAll(bool active){
		for (int i = 0; i < m_slotGroups.Count; i++)
		{
			m_slotGroups[i].SetActiveForSelectionState(active); 
		}
	}

	Slottable m_sbUnderCursor;
	SlotGroup m_sgUnderCursor;
	public bool m_displayGUI = true;
	public GUISkin m_guiSkin;
	public GameObject draggedIcon;
	Camera cam;
	void OnGUI(){
		if(m_displayGUI){
			GUI.skin = m_guiSkin;
			Rect guiRect = new Rect(Screen.width * .05f, Screen.height * .05f, Screen.width * .2f, Screen.height * .5f);
			GUILayout.BeginArea(guiRect, GUI.skin.box);
				GUILayout.BeginVertical();
					GUILayout.BeginHorizontal();
						GUILayout.Label("SG under cursor", GUILayout.Width(70f));
						GUILayout.FlexibleSpace();
						string sgName;
						if(m_sgUnderCursor == null)
							sgName = "null";
						else
							sgName = m_sgUnderCursor.gameObject.name;
						GUILayout.Label(sgName, GUILayout.Width(150f));
					GUILayout.EndHorizontal();
					
					GUILayout.BeginHorizontal();
						GUILayout.Label("SB under cursor", GUILayout.Width(70f));
						GUILayout.FlexibleSpace();
						string sbName;
						if(m_sbUnderCursor == null)
							sbName = "null";
						else
							sbName = m_sbUnderCursor.gameObject.name;
						GUILayout.Label(sbName, GUILayout.Width(150f));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
						if(draggedIcon != null){
							// Vector3 screenPosV3 = cam.WorldToScreenPoint(draggedIcon.transform.position);
							// Vector2 screenPos = new Vector2(screenPosV3.x, screenPosV3.y);
							Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, draggedIcon.transform.position);
							GUILayout.Label(screenPos.ToString());
						}
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
						GUILayout.Label("pixelW: " + cam.pixelWidth + " ,pixelH: " + cam.pixelHeight);
					GUILayout.EndHorizontal();

				GUILayout.EndVertical();
			GUILayout.EndArea();

		}
	}

	public void GetSGandSlottableUnderCursor(ref Slottable sbUnderCursor, ref SlotGroup sgUnderCursor, PointerEventData eventData){
		// sgUnderCursor = null;
		// sbUnderCursor = null;
		if(sgUnderCursor != null && !RectTransformUtility.RectangleContainsScreenPoint(sgUnderCursor.m_dropZoneRect, eventData.position)){
			sgUnderCursor.SetHoverState(false);
			sgUnderCursor = null;
			m_sgUnderCursor = null;
		}
		// if(sbUnderCursor != null && !RectTransformUtility.RectangleContainsScreenPoint(sbUnderCursor.m_rectTrans, eventData.position)){
		// 	sbUnderCursor = null;
		// 	sbUnderCursor
		// 	m_sgUnderCursor = null;
		// }
		for (int i = 0; i < m_slotGroups.Count; i++)
		{
			if(m_slotGroups[i].m_isActiveForSelection){
				if(RectTransformUtility.RectangleContainsScreenPoint(m_slotGroups[i].m_dropZoneRect, eventData.position)){

					if(sgUnderCursor == m_slotGroups[i] /*&& sgUnderCursor != null*/){
						//do nothing
					}else{
						if(sgUnderCursor != null)
							sgUnderCursor.SetHoverState(false);
						m_slotGroups[i].SetHoverState(true);
						sgUnderCursor = m_slotGroups[i];
						m_sgUnderCursor = sgUnderCursor;
					}
				}
				
				Slottable prevSlottableUnderCursor = sbUnderCursor;
				if(m_slotGroups[i].CheckAllSlottablesForCursorHover(eventData, ref sbUnderCursor)){
					if(sbUnderCursor == prevSlottableUnderCursor){
						//do nothing
					}else{
						if(prevSlottableUnderCursor != null)
							prevSlottableUnderCursor.SetHoverState(false);
						sbUnderCursor.SetHoverState(true);
						m_sbUnderCursor = sbUnderCursor;
					}
					return;
				}
			}
		}
	}	
}
