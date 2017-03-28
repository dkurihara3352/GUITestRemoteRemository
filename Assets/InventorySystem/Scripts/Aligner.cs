using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem{
	public abstract class Aligner : MonoBehaviour {

		public abstract void Align(SlotGroup sg);
	}

}
