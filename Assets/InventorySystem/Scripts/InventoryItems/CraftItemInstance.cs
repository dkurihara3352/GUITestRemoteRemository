using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyUtility;

namespace InventorySystem{
	public class CraftItemInstance : ItemInstance {

		public void Initialize(CraftItem craftItem){
			this.m_item = craftItem;
		}
	}

}
