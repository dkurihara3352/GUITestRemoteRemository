using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using InventorySystem;
using MyUtility;
public class Slot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler {


	Image image;
	Text text;
	public Slottable slottablePrefab;
	Slottable newSlottable;
	public Transform slotZoneTrans;
	InventoryItem item;
	int quantity;
	void Awake(){
		image = GetComponent<Image>();
		
	}
	void Start(){

	}

	
	public Slottable droppedSlottable = null;
	public void OnDrop(PointerEventData eventData){
		
		Slottable draggedSlottable = eventData.pointerDrag.GetComponent<Slottable>();
	
		if(draggedSlottable != null && draggedSlottable.m_isPickedUp){
			InventoryItem droppedItem = draggedSlottable.m_Item;
			
			StartCoroutine(draggedSlottable.MoveToSlot(this));
			Debug.Log(droppedItem.itemName.ToString() + " is dropped on " + this.name);

		}
	}
	

	public void SlotIn(Slottable sltbl){
		
		DebugUtility.PrintRed("Slot in called");
		
		newSlottable = (Slottable)GameObject.Instantiate(slottablePrefab,Vector3.zero, Quaternion.identity, slotZoneTrans);
		newSlottable.transform.SetAsLastSibling();
		newSlottable.Initialize(null, sltbl.m_itemInstance, sltbl.m_Item, sltbl.m_PickAmount);
		
		RectTransform newRT = newSlottable.GetComponent<RectTransform>();
		newRT.anchorMin = new Vector2(0f, 1f);
		newRT.anchorMax = new Vector2(0f, 1f);
		newRT.anchoredPosition = new Vector2(.5f, .5f);
		newRT.sizeDelta = new Vector2(80f, 80f);
	}

	public void OnPointerUp(PointerEventData eventData){
		print(this.name+"'s pointer up called");
	}

	public void OnPointerEnter(PointerEventData eventData){
		print(this.name + "'s PointerEnter called");
		if(eventData.pointerDrag != null && eventData.pointerDrag.GetComponent<Slottable>() != null){
			
			Slottable travellingSlottable = eventData.pointerDrag.GetComponent<Slottable>();
			travellingSlottable.m_isOnSlot = true;
			travellingSlottable.m_hoverSlot = this;
			DebugUtility.PrintRed("is on slot: " + travellingSlottable.m_isOnSlot + ", hoverSlot: " + travellingSlottable.m_hoverSlot.gameObject.name);
		}
	}
	public void OnPointerExit(PointerEventData eventData){
		print(this.name + "'s PointerExit called");
		if(eventData.pointerDrag != null && eventData.pointerDrag.GetComponent<Slottable>() != null){
			
			Slottable travellingSlottable = eventData.pointerDrag.GetComponent<Slottable>();
			travellingSlottable.m_isOnSlot = false;
			// travellingSlottable.m_hoverSlot = null;
			
			string str;
			if(travellingSlottable.m_hoverSlot == null){
				str = "null";
			}else{
				str = travellingSlottable.m_hoverSlot.gameObject.name;
			}
			DebugUtility.PrintRed("is on slot: " + travellingSlottable.m_isOnSlot + ", hoverSlot: " + str);
		}

	}
}
