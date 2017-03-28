using UnityEngine;
using System.Collections.Generic;

namespace InventorySystem{

	[System.SerializableAttribute]
	public class Ingredients{
		public string ingredientsName;
		public List<IngredientEntry> elements;
	}
}