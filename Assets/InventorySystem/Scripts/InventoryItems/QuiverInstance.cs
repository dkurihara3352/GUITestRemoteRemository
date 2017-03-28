using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyUtility;

namespace InventorySystem{
	public class QuiverInstance : EquipableItemInstance {

		public void Initialize(Quiver quiver){
			this.m_item = quiver;
		}
		
		protected override int m_GearLevel{
			get{
				int total = m_EffectsEfficacyLevel + m_RoundsLevel;
				return total;
			}
		}
		[SerializeField]
		private int m_effectsEfficacyLevel;
		public int m_EffectsEfficacyLevel{
			get{
				return m_effectsEfficacyLevel;
			}
			set{
				if(value == m_effectsEfficacyLevel) return;
				else{
					if(value > m_effectsEfficacyLevel){
						if(value - m_effectsEfficacyLevel <= AvailablePowerUpSteps())
							m_effectsEfficacyLevel = value;
						else{
							DebugUtility.PrintRed(m_item.itemName +"'s effectsEfficacy is tried to get modified exceeding the max level. " + (value - m_effectsEfficacyLevel - AvailablePowerUpSteps()).ToString() + " levels are cut off");
							m_effectsEfficacyLevel += AvailablePowerUpSteps();
						}
					}else{
						m_effectsEfficacyLevel = value;
					}
				}
			}
		}
		[SerializeField]
		private int m_roundsLevel;
		public int m_RoundsLevel{
			get{
				return m_roundsLevel;
			}
			set{
				if(value == m_roundsLevel) return;
				else{
					if(value > m_roundsLevel){
						if(value - m_roundsLevel <= AvailablePowerUpSteps())
							m_roundsLevel = value;
						else{
							DebugUtility.PrintRed(m_item.itemName +"'s rounds is tried to get modified exceeding the max level. " + (value - m_roundsLevel - AvailablePowerUpSteps()).ToString() + " levels are cut off");
							m_roundsLevel += AvailablePowerUpSteps();
						}
					}else{
						m_roundsLevel = value;
					}
				}
			}
		}

		public float GetEffectsEfficacyValue(float level){
			Quiver quiver = (Quiver)this.m_item;
			AttributeCurve effectsEfficacyCurve = quiver.effectsEfficacy;
			return effectsEfficacyCurve.curve.Evaluate(level);
		}
		public float GetRoundsValue(float level){
			Quiver quiver = (Quiver)this.m_item;
			AttributeCurve roundsCurve = quiver.rounds;
			return roundsCurve.curve.Evaluate(level);
		}

		public void UpdateEffectsEfficacyLevel(int newLevel){
			this.m_EffectsEfficacyLevel = newLevel;
			UpdateGearLevel();
		}
		public void UpdateRoundsLevel(int newLevel){
			this.m_RoundsLevel = newLevel;
			UpdateGearLevel();
		}

		private void UpdateGearLevel(){
			this.gearLevel = m_EffectsEfficacyLevel + m_RoundsLevel;
		}
	}

}
