
using System.Collections.Generic;
namespace InventorySystem{
	[System.SerializableAttribute]
	public class Pack: CarriedGear{
		public List<LootBonus> lootBonus;
		public BonusTrigger bonusTrigger;
		public AttributeCurve efficacy;

	}
}