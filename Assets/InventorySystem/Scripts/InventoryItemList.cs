
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem{
	[System.SerializableAttribute]
	public class InventoryItemList: ScriptableObject{
		public List<InventoryItem> allItemsList;
		public List<Bow> bowList;
		public List<Wear> wearList;
		public List<Shield> shieldList;
		public List<MeleeWeapon> meleeWeaponList;
		public List<Quiver> quiverList;
		public List<Pack> packList;
		public List<CraftItem> craftItemList;
	}

}
