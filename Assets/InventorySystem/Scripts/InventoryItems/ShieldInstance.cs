using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyUtility;

namespace InventorySystem{
	public class ShieldInstance : EquipableItemInstance {

		public void Initialize(Shield shield){
			this.m_item = shield;
		}

		protected override int m_GearLevel{
			get{
				int total = m_LongevityLevel + m_SturdinessLevel + m_DeflectionLevel;
				return total;
			}
		}
		[SerializeField]
		private int m_longevityLevel;
		public int m_LongevityLevel{
			get{
				return m_longevityLevel;
			}
			set{
				if(value == m_longevityLevel) return;
				else{
					if(value > m_longevityLevel){
						if(value - m_longevityLevel <= AvailablePowerUpSteps())
							m_longevityLevel = value;
						else{
							DebugUtility.PrintRed(m_item.itemName +"'s longevity is tried to get modified exceeding the max level. " + (value - m_longevityLevel - AvailablePowerUpSteps()).ToString() + " levels are cut off");
							m_longevityLevel += AvailablePowerUpSteps();
						}
					}else{
						m_longevityLevel = value;
					}
				}
			}
		}
		[SerializeField]
		private int m_sturdinessLevel;
		public int m_SturdinessLevel{
			get{
				return m_sturdinessLevel;
			}
			set{
				if(value == m_sturdinessLevel) return;
				else{
					if(value > m_sturdinessLevel){
						if(value - m_sturdinessLevel <= AvailablePowerUpSteps())
							m_sturdinessLevel = value;
						else{
							DebugUtility.PrintRed(m_item.itemName +"'s sturdiness is tried to get modified exceeding the max level. " + (value - m_sturdinessLevel - AvailablePowerUpSteps()).ToString() + " levels are cut off");
							m_sturdinessLevel += AvailablePowerUpSteps();
						}
					}else{
						m_sturdinessLevel = value;
					}
				}
			}
		}
		[SerializeField]
		private int m_deflectionLevel;
		public int m_DeflectionLevel{
			get{
				return m_deflectionLevel;
			}
			set{
				if(value == m_deflectionLevel) return;
				else{
					if(value > m_deflectionLevel){
						if(value - m_deflectionLevel <= AvailablePowerUpSteps())
							m_deflectionLevel = value;
						else{
							DebugUtility.PrintRed(m_item.itemName +"'s deflection is tried to get modified exceeding the max level. " + (value - m_deflectionLevel - AvailablePowerUpSteps()).ToString() + " levels are cut off");
							m_deflectionLevel += AvailablePowerUpSteps();
						}
					}else{
						m_deflectionLevel = value;
					}
				}
			}
		}

		public float GetLongevityValue(float level){
			Shield shield = (Shield)this.m_item;
			AttributeCurve longevityCurve = shield.longevity;
			return longevityCurve.curve.Evaluate(level);
		}
		public float GetSturdinessValue(float level){
			Shield shield = (Shield)this.m_item;
			AttributeCurve sturdinessValue = shield.sturdiness;
			return sturdinessValue.curve.Evaluate(level);
		}
		public float GetDeflectionValue(float level){
			Shield shield = (Shield)this.m_item;
			AttributeCurve deflectionCurve = shield.deflection;
			return deflectionCurve.curve.Evaluate(level);
		}

		public void UpdateLongevityLevel(int newLevel){
			m_LongevityLevel = newLevel;
			UpdateGearLevel();
		}
		public void UpdateSturdinessLevel(int newLevel){
			m_SturdinessLevel = newLevel;
			UpdateGearLevel();
		}
		public void UpdateDeflectionLevel(int newLevel){
			m_DeflectionLevel = newLevel;
			UpdateGearLevel();
		}

		private void UpdateGearLevel(){
			this.gearLevel = m_LongevityLevel + m_SturdinessLevel + m_DeflectionLevel;
		}


	}

}
