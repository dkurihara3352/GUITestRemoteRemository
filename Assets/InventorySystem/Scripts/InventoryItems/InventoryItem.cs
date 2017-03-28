
using UnityEngine;

namespace InventorySystem{

	[System.Serializable]
	public abstract class InventoryItem : ScriptableObject {
		public int itemId;
		public string itemName;
		public Sprite itemSprite;
		public Texture2D icon;
		public bool isUnlocked;
		public ItemTier tier;
		// public bool isFavorite;
		public bool isStackable;
		

	}

	[System.Serializable]
	public abstract class EquipableGear: InventoryItem{
		// public bool isEquipped;
		public int maxLevel;
		// public int gearLevel;
		public Ingredients dismantleTo;
		public Ingredients craftedFrom;
	}

	[System.SerializableAttribute]
	public abstract class MainGear: EquipableGear{

	}
	[System.SerializableAttribute]
	public abstract class CarriedGear: EquipableGear{

	}

}
