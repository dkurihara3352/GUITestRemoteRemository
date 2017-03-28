
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem{
	[System.SerializableAttribute]
	public class PlayerInventory : ScriptableObject {

		public List<InventoryItemEntry> entries;

		public InventoryItemEntry GetEntry(int itemId){
			InventoryItemEntry result = null;
			for (int i = 0; i < entries.Count; i++)
			{
				if(entries[i].itemInstance.m_item.itemId == itemId)
					result = entries[i];

			}
			return result;
		}
	}

	[System.Serializable]
	public class InventoryItemEntry{

		public ItemInstance itemInstance;
		public int quantity;
		// public void MarkEquipped(){

		// 	if(this.itemInstance is EquipableItemInstance){
		// 		EquipableItemInstance equiInst = (EquipableItemInstance)this.itemInstance;
		// 		equiInst.SetEquippedState(true);
		// 		this.quantity = 0;
		// 	}else{// this is crafted item
		// 		//do nothing
		// 	}
		// }
	}

}
