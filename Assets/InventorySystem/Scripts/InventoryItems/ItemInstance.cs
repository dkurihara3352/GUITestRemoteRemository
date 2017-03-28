using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InventorySystem{

	[System.Serializable]
	public abstract class ItemInstance : ScriptableObject {

		public InventoryItem m_item;
		// [SerializeField]
		// public InventoryItem m_Item{
		// 	get{return m_item;}
		// }
	}


	[System.Serializable]
	public abstract class EquipableItemInstance: ItemInstance{
		// bool m_isEquipped;
		bool m_isFavorite;
		
		public int gearLevel;
		protected abstract int m_GearLevel{
			get;
		}

		public int AvailablePowerUpSteps(){
			EquipableGear equipableGear = (EquipableGear)this.m_item;
			return equipableGear.maxLevel - m_GearLevel;
		}

		// public void SetEquippedState(bool equipped){
		// 	m_isEquipped = equipped;
		// }
		public void SetFaveState(bool fave){
			m_isFavorite = fave;
		}


	}

}

