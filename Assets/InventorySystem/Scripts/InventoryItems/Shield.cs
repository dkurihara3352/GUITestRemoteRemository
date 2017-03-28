namespace InventorySystem{
	[System.SerializableAttribute]
	public class Shield: CarriedGear{
		public AttributeCurve longevity;
		public AttributeCurve sturdiness;
		public AttributeCurve deflection;
	}
}