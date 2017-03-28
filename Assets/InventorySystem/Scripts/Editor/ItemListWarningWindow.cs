using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace InventorySystem{
	public class ItemListWarningWindow : OverwriteWarningWindow {

		protected override string Message(){
			return "There's already an ItemList asset in Asset folder. \n Do you wish to overwrite?";
		}

		protected override void CallEditorWindowMethod(){
			base.invSysWindow.CreateInventoryItemList();
		}

	}

}
