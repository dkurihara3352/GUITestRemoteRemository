using System.Collections.Generic;

namespace InventorySystem{
	[System.SerializableAttribute]
	public class CraftItem: InventoryItem{
		
		public List<AttributeBonus> attributeBonusList;
	}
}
