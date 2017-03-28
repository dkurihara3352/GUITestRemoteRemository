using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem{
	
	
	
	[System.Serializable]
	public class IngredientEntry{
		public InventoryItem inventoryItem;
		public int quantity;

	}

	public enum ShotEffect{
		Pepper, Blast, Marker, Incinerate
	}
	public enum LootBonus{
		Parts, Gears
	}

	public enum BonusTrigger{
		None, Brutal, Assassin, Survival, Basic
	}

	public enum ItemTier{
		Makeshift, Crafted, Forged, Masterpiece 
	}

	[System.SerializableAttribute]
	public class AttributeBonus{
		public AttCurveId attCurveId;
		public float addedBonus;
	}

	public enum AttCurveId{
		BowDrawStrength,
		BowHandling,
		BowSpecialEffect,
		WearArmour,
		WearSwiftness,
		WearCarriedGearEfficacy,
		ShieldLongevity,
		ShieldSturdiness,
		ShieldDeflection,
		MeleeWeaponLongevity,
		MeleeWeaponKnockPower,
		MeleeWeaponFireRate,
		QuiverEffectsEfficacy,
		QuiverRounds,
		PackEfficacy

	}
	[System.SerializableAttribute]
	public class Slot{
		public Slottable slottable;
		public RectTransform slotRect;
	}

	public enum SlotGroupType{
		Pool, Bow, Wear, CarriedGear, CraftItems
	}

}
