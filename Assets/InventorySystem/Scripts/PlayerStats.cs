using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem;

[System.SerializableAttribute]
public class PlayerStats : ScriptableObject {

	public Bow equippedBow;
	public Wear equippedWear;
	public int unlockedCarriedGearSlotCount;
	public List<CarriedGear> equippedCarriedGears;

	public void Init(Bow bow, Wear wear, int slots){
		this.equippedBow = bow;
		this.equippedWear = wear;
		this.unlockedCarriedGearSlotCount = slots;
		this.equippedCarriedGears = new List<CarriedGear>();
	}


}
