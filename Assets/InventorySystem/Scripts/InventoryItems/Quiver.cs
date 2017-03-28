using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem{
	[System.SerializableAttribute]
	public class Quiver: CarriedGear{
		public List<ShotEffect> addedEffects;
		public AttributeCurve effectsEfficacy;
		public AttributeCurve rounds;

		// public void Add(ShotEffect shotEffect){
		// 	if(addedEffects!= null){
		// 		Debug.Log("addedEffects.Count before addition: " + addedEffects.Count.ToString());
		// 		addedEffects.Add(shotEffect);
		// 		Debug.Log("addedEffects.Count after addition: " + addedEffects.Count.ToString());
		// 	}else{
		// 		Debug.Log("addedEffects is null");
		// 	}
		// }

	}
	
}
