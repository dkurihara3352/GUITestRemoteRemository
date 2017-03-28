using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyUtility;

namespace InventorySystem{
	
	[System.Serializable]
	public class BowInstance : EquipableItemInstance{

		[SerializeField]
		
		protected override int m_GearLevel{
			get{
				int total = m_DrawStrengthLevel + m_HandlingLevel + m_SpecialEffectLevel;
				return total;
			}
		}
		[SerializeField]
		private int m_drawStrengthLevel;
		public int m_DrawStrengthLevel{
			get{
				return m_drawStrengthLevel;}
			set{
				if(value == m_drawStrengthLevel) return;
				else{
					if(value > m_drawStrengthLevel){
						if(value - m_drawStrengthLevel <= AvailablePowerUpSteps())
							m_drawStrengthLevel = value;
						else{
							DebugUtility.PrintRed(m_item.itemName +"'s drawStrengthLevel is tried to get modified exceeding the max level. " + (value - m_drawStrengthLevel - AvailablePowerUpSteps()).ToString() + " levels are cut off");
							m_drawStrengthLevel += AvailablePowerUpSteps();
						}
					}else{
						m_drawStrengthLevel = value;
					}
				}
			}
		}
		[SerializeField]
		private int m_handlingLevel;
		public int m_HandlingLevel{
			get{
				return m_handlingLevel;}
			set{
				if(value == m_handlingLevel) return;
				else{
					if(value > m_handlingLevel){
						if(value - m_handlingLevel <= AvailablePowerUpSteps())
							m_handlingLevel = value;
						else{
							DebugUtility.PrintRed(m_item.itemName +"'s handlingLevel is tried to get modified exceeding the max level. " + (value - m_handlingLevel - AvailablePowerUpSteps()).ToString() + " levels are cut off");
							m_handlingLevel += AvailablePowerUpSteps();
						}
					}else{
						m_handlingLevel = value;
					}
				}
			}
		}
		[SerializeField]
		private int m_specialEffectLevel;
		public int m_SpecialEffectLevel{
			get{
				return m_specialEffectLevel;}
			set{
				if(value == m_specialEffectLevel) return;
				else{
					if(value > m_specialEffectLevel){
						if(value - m_specialEffectLevel <= AvailablePowerUpSteps())
							m_specialEffectLevel = value;
						else{
							DebugUtility.PrintRed(m_item.itemName +"'s specialEffectLevel is tried to get modified exceeding the max level. " + (value - m_specialEffectLevel - AvailablePowerUpSteps()).ToString() + " levels are cut off");
							m_specialEffectLevel += AvailablePowerUpSteps();
						}
					}else{
						m_specialEffectLevel = value;
					}
				}
			}
		}

		public float GetShotPowerValue(float drawTime){
			Bow bow = (Bow)this.m_item;
			AttributeCurve drawProfileCurve = bow.drawProfile;
			return drawProfileCurve.curve.Evaluate(drawTime);
		}
		public float GetDrawStrengthValue(float level){
			Bow bow = (Bow)this.m_item;
			AttributeCurve drawStrengthCurve = bow.drawStrength;
			return drawStrengthCurve.curve.Evaluate(level);
		}
		public float GetHandlingValue(float level){
			Bow bow = (Bow)this.m_item;
			AttributeCurve handlingCurve = bow.handling;
			return handlingCurve.curve.Evaluate(level);
		}
		public float GetSpecialEffectValue(float level){
			Bow bow = (Bow)this.m_item;
			AttributeCurve specialEffectCurve = bow.specialEffect;
			return specialEffectCurve.curve.Evaluate(level);
		}

		public void Initialize(Bow bow){
			this.m_item = bow;
		}
		public void UpdateDrawStrengthLevel(int newLevel){
			m_DrawStrengthLevel = newLevel;
			UpdateGearLevel();
		}
		public void UpdateHandlingLevel(int newLevel){
			m_HandlingLevel= newLevel;
			UpdateGearLevel();
		}
		public void UpdateSpecialEffectLevel(int newLevel){
			m_SpecialEffectLevel= newLevel;
			UpdateGearLevel();
		}
		private void UpdateGearLevel(){
			this.gearLevel = m_DrawStrengthLevel + m_HandlingLevel + m_SpecialEffectLevel;
		}

	}

}
