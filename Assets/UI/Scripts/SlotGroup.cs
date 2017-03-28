
using UnityEngine;
using System.Collections;

using InventorySystem;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MyUtility;


public class SlotGroup : MonoBehaviour {

	public PlayerInventory inventory; 
	public Slottable slottablePrefab;
	public Transform panel;

	[SerializeField]
	public List<Slot> m_slots;
	[SerializeField]
	public List<Slot> m_tempSlots;
	
	public SlotGroupType m_slotGroupType;
	public RectTransform m_dropZoneRect;
	public bool isExpandable;
	public GameObject m_slotPrefab;
	public bool isSubtractable;
	public bool m_isActiveForSelection = false;

	
	public Slot this[int index]{
		get{
			return m_slots[index];
		}
	}

	public int SlotCount{
		get{return m_slots.Count;}
	}
	



	public bool HasVacancy{
		get{
			bool result = false;
			for (int i = 0; i < SlotCount; i++)
			{
				if(this[i].slottable == null){
					result = true;
				}
			}
			return result;
		}
	}

	public int GetSlotIndex(int itemId){
		int result = -1;
		for (int i = 0; i < SlotCount; i++)
		{
			if(this[i].slottable != null){

				int itemIndex = this[i].slottable.m_Item.itemId;
				if(itemIndex == itemId)
					result = i;
			}
		}
		return result;
	}
	public void CleanUpData(){
		for (int i = 0; i < inventory.entries.Count; i++)
		{
			if(inventory.entries[i].quantity ==0 && this.m_slotGroupType!= SlotGroupType.Pool){
				inventory.entries.RemoveAt(i);
			}
		}	
	}
	public void SortData(){
		List<InventoryItemEntry> tempList = new List<InventoryItemEntry>();
		List<InventoryItemEntry> copy = new List<InventoryItemEntry>();
		copy.AddRange(inventory.entries);
		while(tempList.Count < inventory.entries.Count){

			int prevId = -1;
			int idAtMin = -1;
			for (int i = 0; i < copy.Count; i++)
			{
				if(copy[i].itemInstance.m_item.itemId < prevId|| prevId == -1){
					prevId = copy[i].itemInstance.m_item.itemId;
					idAtMin = i;
				}
			}
			tempList.Add(copy[idAtMin]);
			copy.RemoveAt(idAtMin);
		}
		inventory.entries = tempList;

	}
	
	public void PrintTempSlots(){
		for (int i = 0; i < m_tempSlots.Count; i++)
		{
			if(m_tempSlots[i].slottable != null){

				DebugUtility.PrintPurple(this.gameObject.name + "'s m_tempSlots[" + i.ToString() + "].slottable.m_itemInstance.name: " + m_tempSlots[i].slottable.m_itemInstance.name.ToString());
			}else{
				DebugUtility.PrintPurple(this.gameObject.name + "'s m_tempSlots[" + i.ToString() + "].slottable is null");
				
			}
				
		}
	}
	public void UpdateSlots(PointerEventData eventData){
		/*	create new slots part
				search in the current slots for slottable reference
				update quantit
				if not found
					create new and update its position and hierarchy
					disable its image component
				if the quantity is 0 and not pool
					do not include it in the new slots

			Transit part
				for each slottable in the current slots
					try to spot the new slot referring to the newSlots
						if found, start to move toward it
						if not, detach it and disable image and leave it there until it is deleted upon completion of transaction
		*/
		/*	Create part
		*/
			m_tempSlots = new List<Slot>();
			for (int i = 0; i < m_slots.Count; i++)
			{
				Slot newSlot = new Slot();
				newSlot.slottable = null;
				newSlot.slotRect = m_slots[i].slotRect;
				m_tempSlots.Add(newSlot);
			}
			int addedSlotsCount = inventory.entries.Count - m_slots.Count;
			if(addedSlotsCount > 0 ){
				for (int i = 0; i < addedSlotsCount; i++)
				{
					Slot newSlot = new Slot();
					newSlot.slottable = null;
					newSlot.slotRect = Instantiate(m_slotPrefab, Vector3.zero, Quaternion.identity).GetComponent<RectTransform>();
					newSlot.slotRect.SetParent(panel);
					newSlot.slotRect.SetAsLastSibling();
					// LayoutGroup panelLayoutGroup = panel.GetComponent<LayoutGroup>();
					// if(panelLayoutGroup != null){
					// 	panelLayoutGroup.CalculateLayoutInputHorizontal();
					// 	panelLayoutGroup.CalculateLayoutInputVertical();
					// 	panelLayoutGroup.SetLayoutHorizontal();
					// 	panelLayoutGroup.SetLayoutVertical();
					// }
					m_tempSlots.Add(newSlot);
				}
			}
			/*	at this point the m_tempSlots has at least as many slots with slotRect as inventory entries
				slotRects are oredered hierarchically
				slottables are empty
			*/
			for (int i = 0; i < inventory.entries.Count; i++)
			{	
				bool found = false;
				for (int j = 0; j < m_slots.Count; j++)
				{
					if(m_slots[j].slottable != null){

						if(m_slots[j].slottable.m_itemInstance ==inventory.entries[i].itemInstance){
							found = true;
							m_tempSlots[i].slottable = m_slots[j].slottable;
							m_tempSlots[i].slottable.m_Quantity = inventory.entries[i].quantity;
						}	
					}
				}
				if(!found){
					Slottable newSlottable = Instantiate(slottablePrefab, Vector3.zero, Quaternion.identity);
					newSlottable.Initialize(this, inventory.entries[i].itemInstance, inventory.entries[i].itemInstance.m_item, inventory.entries[i].quantity);
					newSlottable.Attach(m_tempSlots[i].slotRect);
					newSlottable.InitHierarchyDependents();
					newSlottable.GetComponent<Image>().enabled = false;
					m_tempSlots[i].slottable = newSlottable;
				}
				if(m_tempSlots[i].slottable.m_Quantity == 0 && m_slotGroupType != SlotGroupType.Pool){
					m_tempSlots[i].slottable = null;
				}	
			}

			for (int i = 0; i < m_tempSlots.Count; i++)
			{
				if(m_tempSlots[i].slottable == null && isExpandable){
					m_tempSlots.RemoveAt(i);
				}
			}	
			/*	at this point all the itemInstance in the inventory is represented as slottable and stored in m_tempSlots
				if not pool the zero quantity slottables are omitted
			*/
			
			
		/*	Transit part
		*/
		
			for (int i = 0; i < m_slots.Count; i++)
			{
				bool found = false;
				for (int j = 0; j < m_tempSlots.Count; j++)
				{
					if(m_slots[i].slottable != null && m_tempSlots[j].slottable != null){

						if(m_slots[i].slottable == m_tempSlots[j].slottable){
							found = true;
		
							m_slots[i].slottable.StartCoroutine(m_slots[i].slottable.MoveWithinSG(m_tempSlots[j].slotRect, .5f, eventData));
							continue;
						}
					}
				}
				if(!found){

					if(m_slots[i].slottable != null){

						m_slots[i].slottable.Detach(eventData);
						m_slots[i].slottable.GetComponent<Image>().enabled = false;
					}
				}
			}
	}

	public void CompleteSlotsUpdate(){
		m_slots = m_tempSlots;
		for (int i = 0; i < SlotCount; i++)
		{
			if(this[i].slottable != null){

				Image image = this[i].slottable.GetComponent<Image>();
				if(image.enabled == false)
					image.enabled = true;
			}
		}
	}
	

	public void AddItemToData(Slottable slottable, int quantity){
		InventoryItem item = slottable.m_Item;
		bool found = false;
		
		for (int i = 0; i < inventory.entries.Count; i++)
		{
			if(item.isStackable){

				if(inventory.entries[i].itemInstance.m_item.itemId == item.itemId){

					inventory.entries[i].quantity += quantity;
					found = true;
				}
			}else{
				if(inventory.entries[i].itemInstance == slottable.m_itemInstance){
					inventory.entries[i].quantity += quantity;
					found = true;
				}
			}
		}
		

		if(!found){
			InventoryItemEntry newEntry = new InventoryItemEntry();
			newEntry.itemInstance = slottable.m_itemInstance;
			newEntry.quantity = quantity;
			inventory.entries.Add(newEntry);
		}
	}


	public void RemoveItemFromData(Slottable slottable, int quantity){
		for (int i = 0; i < inventory.entries.Count; i++)
		{
			if(inventory.entries[i].itemInstance == slottable.m_itemInstance)
				inventory.entries[i].quantity -= quantity;
		}
	}
	

	public RectTransform GetSlotRect(Slottable slottable){
		RectTransform result = null;
		for (int i = 0; i < SlotCount; i++)
		{
			if(this[i].slottable != null){

				if(this[i].slottable.m_itemInstance == slottable.m_itemInstance)
				result = this[i].slotRect;
			}
		}
		return result;
	}

	public RectTransform GetNewSlotRect(Slottable slottable){
		
		RectTransform result = null;
		for (int i = 0; i < m_tempSlots.Count; i++)
		{
			if(m_tempSlots[i].slottable != null){

				if(m_tempSlots[i].slottable.m_itemInstance == slottable.m_itemInstance)
				result = m_tempSlots[i].slotRect;
			}
		}
		return result;
	}

	void Awake(){
		
	}
	void Start(){
		
	}

	public void InitSlots(){
		/*	if isExpandable as many newSlotRects as needed needs to be created to accomodate slottables.
			populate the list with pairs of newly created slottables and slotRects
			children all the slottables to slotRects
		*/
		
		/*	create slottables
			initialize and stuck them in a temp list
			sort in the order of itemId
		*/
		List<Slottable> slottableTempList = new List<Slottable>();
		for(int i = 0; i< inventory.entries.Count; i++){
			// if(inventory.entries[i].itemInstance != null){

				if((m_slotGroupType != SlotGroupType.Pool && inventory.entries[i].quantity > 0)|| m_slotGroupType == SlotGroupType.Pool){
					Slottable newSlottable = (Slottable)Instantiate(slottablePrefab, Vector3.zero, Quaternion.identity);
					newSlottable.Initialize(this, inventory.entries[i].itemInstance, inventory.entries[i].itemInstance.m_item, inventory.entries[i].quantity);
					slottableTempList.Add(newSlottable);
				}
			// }
		}
		
		List<Slottable> orderedTemp = new List<Slottable>();
		while(slottableTempList.Count > 0){

			int prevId = -1;
			int idAtMin = -1;
			for (int i = 0; i < slottableTempList.Count; i++)
			{
				if(slottableTempList[i].m_Item.itemId< prevId || prevId == -1){
					prevId = slottableTempList[i].m_Item.itemId;
					idAtMin = i;
				}
			}
			orderedTemp.Add(slottableTempList[idAtMin]);
			slottableTempList.RemoveAt(idAtMin);
		}
		

		/*	populate a temp list of slotRect in the sibling order
			create if isExpandable
			start with a clean slate in that case
		*/

		List<RectTransform> slotRTTempList = new List<RectTransform>();
		if(isExpandable){
			int numOfSlotsToCreate = inventory.entries.Count;
			
			if(panel.childCount != 0){
				for (int i = 0; i < panel.childCount; i++)
				{
					Destroy(panel.GetChild(i));
				}
			}
			/*	create slots and add them in the temp list
			*/
			for (int i = 0; i < numOfSlotsToCreate; i++)
			{
				RectTransform newSlotRect = Instantiate(m_slotPrefab, Vector3.zero, Quaternion.identity, panel).GetComponent<RectTransform>();
				slotRTTempList.Add(newSlotRect);
			}

		}else{
			for (int i = 0; i < panel.childCount; i++)
			{
				RectTransform slotRect = panel.GetChild(i).GetComponent<RectTransform>();
				slotRTTempList.Add(slotRect);
			}
		}
		/*	make pairs of slottable and slotRect from two temp lists,
			and populate m_slots list
			do keep in mind that there is possibly empty slots (one without matching slottables)
		*/
		if(m_slots != null)
			m_slots.Clear();
		else
			m_slots = new List<Slot>();

		for (int i = 0; i < slotRTTempList.Count; i++)
		{
			Slot newSlot = new Slot();
			if(/*orderedTemp[i] == null*/i >= orderedTemp.Count)
				newSlot.slottable = null;
			else
				newSlot.slottable = orderedTemp[i];
			newSlot.slotRect = slotRTTempList[i];
			// newSlot.slottable.m_rectTrans.anchoredPosition = newSlot.slotRect.anchoredPosition;
			// newSlot.slottable.transform.SetParent(newSlot.slotRect);
			if(newSlot.slottable != null){
				newSlot.slottable.Attach(newSlot.slotRect);
				newSlot.slottable.InitHierarchyDependents();
				newSlot.slottable.CheckQuantityColor();
			}
			m_slots.Add(newSlot);
		}
	}

	public void Initialize(){
		m_image = transform.GetComponent<Image>();
		m_deactiveColor = new Color(.7f, .7f, .7f);
		m_hoveredColor = new Color(1f, 1f, .7f);
		m_defaultColor = Color.white;
	}
	Image m_image;
	Color m_deactiveColor;
	Color m_hoveredColor;
	Color m_defaultColor;
	bool m_isHovered = false;
	public void SetActiveForSelectionState(bool active){
		if(active){
			m_isActiveForSelection = true;
			StartCoroutine(Flash(Color.red));
			/*	make all the slottables this holds ActiveForSelection
			*/
			

			for (int i = 0; i < SlotCount; i++)
			{
				if(this[i].slottable != null)
				this[i].slottable.SetActiveForSelectionState(true);
			}
			

		}else{
			m_isActiveForSelection = false;
			m_image.color = m_deactiveColor;
			/*	maek all the slottables this holds DeactiveForSelection
			*/
			for (int i = 0; i < SlotCount; i++)
			{
				if(this[i].slottable != null)
				this[i].slottable.SetActiveForSelectionState(false);
			}
		}
	}

	public void FilterSlottables(Slottable slottable){
		InventoryItem invItem = slottable.m_Item;
		for (int i = 0; i < SlotCount; i++)
		{
			if(this[i].slottable != null){
				this[i].slottable.SetActiveForSelectionState(false);
				if(invItem is Bow){
					if(this[i].slottable.m_Item is Bow)
						this[i].slottable.SetActiveForSelectionState(true);
				}else if(invItem is Wear){
					if(this[i].slottable.m_Item is Wear)
						this[i].slottable.SetActiveForSelectionState(true);

				}else if(invItem is CarriedGear){
					if(this[i].slottable.m_Item is CarriedGear)
						this[i].slottable.SetActiveForSelectionState(true);

				}else if(invItem is CraftItem){
					if(this[i].slottable.m_Item is CraftItem)
						this[i].slottable.SetActiveForSelectionState(true);

				}
			}
		}
	}

	IEnumerator Flash(Color color){
		float t = 0f;
		while(t< .5f){

			m_image.color = Color.Lerp(color, m_defaultColor, t/.5f);

			t += Time.unscaledDeltaTime;
			yield return null;
		}
		m_image.color = m_defaultColor;
	}



	public void SetHoverState(bool hovered){
		if(hovered){
			m_isHovered = true;
			m_image.color = m_hoveredColor;
		}else{
			m_isHovered = false;
			m_image.color = m_defaultColor;
		}
	}

	public bool CheckAllSlottablesForCursorHover(PointerEventData eventData, ref Slottable sbUnderCursor){
		Slottable prevSlottable = sbUnderCursor;
		sbUnderCursor = null;
		for (int i = 0; i < SlotCount; i++)
		{
			if(this[i].slottable != null){
				if(this[i].slottable.m_isActiveForSelection){
					if(this[i].slottable.gameObject == eventData.pointerEnter || this[i].slottable.ChildIsPointerEnter(eventData)){
						
						sbUnderCursor = this[i].slottable;
						return true;
					}
				}
			}
		}
		if(prevSlottable != null)
			prevSlottable.SetHoverState(false);
		return false;
	}
}




