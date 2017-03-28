

namespace InventorySystem{

	[System.SerializableAttribute]
	public class Bow: MainGear{
		[UnityEngine.SerializeField]
		public AttributeCurve drawProfile;
		
		[UnityEngine.SerializeField]
		public AttributeCurve drawStrength;
		
		[UnityEngine.SerializeField]
		public AttributeCurve handling;
		
		[UnityEngine.SerializeField]
		public AttributeCurve specialEffect;
	}
}
