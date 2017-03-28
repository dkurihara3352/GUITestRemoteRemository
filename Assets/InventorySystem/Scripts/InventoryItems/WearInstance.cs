using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyUtility;

namespace InventorySystem{
	[System.Serializable]
	public class WearInstance : EquipableItemInstance {

		public void Initialize(Wear wear){
			this.m_item = wear;
		}
		protected override int m_GearLevel{
			get{
				int total = m_ArmourLevel + m_SwiftnessLevel + m_CarriedGearEfficacyLevel;
				return total;
			}
		}
		[SerializeField]
		private int m_armourLevel;
		public int m_ArmourLevel{
			get{
				return m_armourLevel;}
			set{
				if(value == m_armourLevel) return;
				else{
					if(value > m_armourLevel){
						if(value - m_armourLevel <= AvailablePowerUpSteps())
							m_armourLevel = value;
						else{
							DebugUtility.PrintRed(m_item.itemName +"'s armourLevel is tried to get modified exceeding the max level. " + (value - m_armourLevel - AvailablePowerUpSteps()).ToString() + " levels are cut off");
							m_armourLevel += AvailablePowerUpSteps();
						}
					}else{
						m_armourLevel = value;
					}
				}
			}
		}
		[SerializeField]
		private int m_swiftnessLevel;
		public int m_SwiftnessLevel{
			get{
				return m_swiftnessLevel;}
			set{
				if(value == m_swiftnessLevel) return;
				else{
					if(value > m_swiftnessLevel){
						if(value - m_swiftnessLevel <= AvailablePowerUpSteps())
							m_swiftnessLevel = value;
						else{
							DebugUtility.PrintRed(m_item.itemName +"'s swiftnessLevel is tried to get modified exceeding the max level. " + (value - m_swiftnessLevel - AvailablePowerUpSteps()).ToString() + " levels are cut off");
							m_swiftnessLevel += AvailablePowerUpSteps();
						}
					}else{
						m_swiftnessLevel = value;
					}
				}
			}
		}

		[SerializeField]
		private int m_carriedGearEfficacyLevel;
		public int m_CarriedGearEfficacyLevel{
			get{
				return m_carriedGearEfficacyLevel;}
			set{
				if(value == m_carriedGearEfficacyLevel) return;
				else{
					if(value > m_carriedGearEfficacyLevel){
						if(value - m_carriedGearEfficacyLevel <= AvailablePowerUpSteps())
							m_carriedGearEfficacyLevel = value;
						else{
							DebugUtility.PrintRed(m_item.itemName +"'s carriedGearEfficacyLevel is tried to get modified exceeding the max level. " + (value - m_carriedGearEfficacyLevel - AvailablePowerUpSteps()).ToString() + " levels are cut off");
							m_carriedGearEfficacyLevel += AvailablePowerUpSteps();
						}
					}else{
						m_carriedGearEfficacyLevel = value;
					}
				}
			}
		}

		public float GetArmourValue(float level){
			Wear wear = (Wear)this.m_item;
			AttributeCurve armourCurve = wear.armour;
			return armourCurve.curve.Evaluate(level);
		}
		public float GetSwiftnessValue(float level){
			Wear wear = (Wear)this.m_item;
			AttributeCurve swiftnessCurve = wear.swiftness;
			return swiftnessCurve.curve.Evaluate(level);
		}
		public float GetCarriedGearEfficacyValue(float level){
			Wear wear = (Wear)this.m_item;
			AttributeCurve carriedGearEfficacyCurve = wear.carriedGearEfficacy;
			return carriedGearEfficacyCurve.curve.Evaluate(level);
		}

		public void UpdateArmourLevel(int newLevel){
			m_ArmourLevel = newLevel;
			UpdateGearLevel();
		}
		public void UpdateSwiftnessLevel(int newLevel){
			m_SwiftnessLevel = newLevel;
			UpdateGearLevel();
		}
		public void UpdateCarriedGearEfficacyLevel(int newLevel){
			m_CarriedGearEfficacyLevel = newLevel;
			UpdateGearLevel();
		}

		private void UpdateGearLevel(){
			this.gearLevel = m_ArmourLevel + m_SwiftnessLevel + m_CarriedGearEfficacyLevel;
		}
	}

}
