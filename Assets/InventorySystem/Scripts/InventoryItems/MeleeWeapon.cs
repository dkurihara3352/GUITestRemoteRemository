namespace InventorySystem{
	[System.SerializableAttribute]
	public class MeleeWeapon: CarriedGear{
		public AttributeCurve longevity;
		public AttributeCurve knockPower;
		public AttributeCurve fireRate;
	}
}