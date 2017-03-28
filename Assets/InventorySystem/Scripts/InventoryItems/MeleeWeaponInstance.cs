using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyUtility;

namespace InventorySystem{

	public class MeleeWeaponInstance : EquipableItemInstance {

		public void Initialize(MeleeWeapon meleeWeapon){
			this.m_item = meleeWeapon;
		}

		protected override int m_GearLevel{
			get{
				int total = m_LongevityLevel + m_KnockPowerLevel + m_FireRateLevel;
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
		private int m_knockPowerLevel;
		public int m_KnockPowerLevel{
			get{
				return m_knockPowerLevel;
			}
			set{
				if(value == m_knockPowerLevel) return;
				else{
					if(value > m_knockPowerLevel){
						if(value - m_knockPowerLevel <= AvailablePowerUpSteps())
							m_knockPowerLevel = value;
						else{
							DebugUtility.PrintRed(m_item.itemName +"'s knockPower is tried to get modified exceeding the max level. " + (value - m_knockPowerLevel - AvailablePowerUpSteps()).ToString() + " levels are cut off");
							m_knockPowerLevel += AvailablePowerUpSteps();
						}
					}else{
						m_knockPowerLevel = value;
					}
				}
			}
		}
		[SerializeField]
		private int m_fireRateLevel;
		public int m_FireRateLevel{
			get{
				return m_fireRateLevel;
			}
			set{
				if(value == m_fireRateLevel) return;
				else{
					if(value > m_fireRateLevel){
						if(value - m_fireRateLevel <= AvailablePowerUpSteps())
							m_fireRateLevel = value;
						else{
							DebugUtility.PrintRed(m_item.itemName +"'s fireRate is tried to get modified exceeding the max level. " + (value - m_fireRateLevel - AvailablePowerUpSteps()).ToString() + " levels are cut off");
							m_fireRateLevel += AvailablePowerUpSteps();
						}
					}else{
						m_fireRateLevel = value;
					}
				}
			}
		}

		public float GetLongevityValue(float level){
			MeleeWeapon meleeWeapon = (MeleeWeapon)this.m_item;
			AttributeCurve longevityCurve = meleeWeapon.longevity;
			return longevityCurve.curve.Evaluate(level);
		}
		public float GetKnockPowerValue(float level){
			MeleeWeapon meleeWeapon = (MeleeWeapon)this.m_item;
			AttributeCurve knockPowerCurve = meleeWeapon.knockPower;
			return knockPowerCurve.curve.Evaluate(level);
		}
		public float GetFireRateValue(float level){
			MeleeWeapon meleeWeapon = (MeleeWeapon)this.m_item;
			AttributeCurve fireRateCurve = meleeWeapon.fireRate;
			return fireRateCurve.curve.Evaluate(level);
		}

		public void UpdateLongevityLevel(int newLevel){
			this.m_LongevityLevel = newLevel;
			UpdateGearLevel();
		}
		public void UpdateKnockPowerLevel(int newLevel){
			this.m_KnockPowerLevel = newLevel;
			UpdateGearLevel();
		}
		public void UpdateFireRateLevel(int newLevel){
			this.m_FireRateLevel = newLevel;
			UpdateGearLevel();
		}

		private void UpdateGearLevel(){
			this.gearLevel = m_LongevityLevel + m_KnockPowerLevel + m_FireRateLevel;
		}
	}
}
