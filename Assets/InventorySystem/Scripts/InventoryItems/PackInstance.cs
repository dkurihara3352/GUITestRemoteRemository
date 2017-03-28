using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyUtility;

namespace InventorySystem{
	public class PackInstance : EquipableItemInstance {

		public void Initialize(Pack pack){
			this.m_item = pack;
		}

		protected override int m_GearLevel{
			get{
				return m_EfficacyLevel;
			}
		}
		[SerializeField]
		private int m_efficacyLevel;
		public int m_EfficacyLevel{
			get{
				return m_efficacyLevel;
			}
			set{
				if(value == m_efficacyLevel) return;
				else{
					if(value > m_efficacyLevel){
						if(value - m_efficacyLevel <= AvailablePowerUpSteps())
							m_efficacyLevel = value;
						else{
							DebugUtility.PrintRed(m_item.itemName +"'s efficacy is tried to get modified exceeding the max level. " + (value - m_efficacyLevel - AvailablePowerUpSteps()).ToString() + " levels are cut off");
							m_efficacyLevel += AvailablePowerUpSteps();
						}
					}else{
						m_efficacyLevel = value;
					}
				}
			}
		}

		public float GetEfficacyValue(float level){
			Pack pack = (Pack)this.m_item;
			AttributeCurve efficacyCurve = pack.efficacy;
			return efficacyCurve.curve.Evaluate(level);
		}

		public void UpdateEfficacyLevel(int newLevel){
			this.m_EfficacyLevel = newLevel;
			UpdateGearLevel();
		}

		private void UpdateGearLevel(){
			this.gearLevel = m_EfficacyLevel;
		}
	}

}
