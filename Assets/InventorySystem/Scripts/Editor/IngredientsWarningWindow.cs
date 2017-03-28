using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem{
	public class IngredientsWarningWindow : OverwriteWarningWindow{

		protected override string Message(){
			return "There's already an asset with the typed name. \n Do you wish to overwrite?";
		}

		protected override void CallEditorWindowMethod(){
			// base.invSysWindow.CreateNewIngredients();
		}
	}

}
