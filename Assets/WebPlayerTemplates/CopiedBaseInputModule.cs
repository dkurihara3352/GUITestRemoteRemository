using System;
using System.Collections.Generic;

namespace UnityEngine.EventSystems
{
	[RequireComponent(typeof(EventSystem))]
	public abstract class BaseInputModule : UIBehaviour
	{
		[NonSerialized]
		protected List<RaycastResult> m_RaycastResultCache = new List<RaycastResult>();
		/*	used to store RaycastResults for EventSystem.RaycastAll
			cleared after use
		*/

		private AxisEventData m_AxisEventData;

		private EventSystem m_EventSystem;

		private BaseEventData m_BaseEventData;

		protected BaseInput m_InputOverride;

		private BaseInput m_DefaultInput;

		public BaseInput input
		{/*	if there's m_InputOverride, return it
			if not
				if there's any BaseInput attached, return it
				else create a new BaseInput, then attach and return it
		*/
			get
			{
				BaseInput result;
				if (this.m_InputOverride != null)
				{
					result = this.m_InputOverride;
				}
				else
				{
					if (this.m_DefaultInput == null)
					{
						BaseInput[] components = base.GetComponents<BaseInput>();/*if non assign a zero-array, not null*/
						BaseInput[] array = components;
						for (int i = 0; i < array.Length; i++)
						{
							BaseInput baseInput = array[i];
							if (baseInput != null && baseInput.GetType() == typeof(BaseInput))
							{
								this.m_DefaultInput = baseInput;
								break;
							}
						}
						if (this.m_DefaultInput == null)
						{
							this.m_DefaultInput = base.gameObject.AddComponent<BaseInput>();
						}
					}
					result = this.m_DefaultInput;
				}
				return result;
			}
		}

		protected EventSystem eventSystem
		{
			get
			{
				return this.m_EventSystem;
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			this.m_EventSystem = base.GetComponent<EventSystem>();
			this.m_EventSystem.UpdateModules();
		}

		protected override void OnDisable()
		{
			this.m_EventSystem.UpdateModules();
			base.OnDisable();
		}

		public abstract void Process();
		/*	overrided in StandaloneInputModule
		*/

		protected static RaycastResult FindFirstRaycast(List<RaycastResult> candidates)
		{/*	search through candidates and return the first occurence
			return null if not
		*/
			RaycastResult result;
			for (int i = 0; i < candidates.Count; i++)
			{
				if (!(candidates[i].gameObject == null))
				{
					result = candidates[i];
					return result;
				}
			}
			result = default(RaycastResult);
			return result;
		}

		protected static MoveDirection DetermineMoveDirection(float x, float y)
		{
			return BaseInputModule.DetermineMoveDirection(x, y, 0.6f);
		}

		protected static MoveDirection DetermineMoveDirection(float x, float y, float deadZone)
		{
			Vector2 vector = new Vector2(x, y);
			MoveDirection result;
			if (vector.sqrMagnitude < deadZone * deadZone)
			{
				result = MoveDirection.None;
			}
			else if (Mathf.Abs(x) > Mathf.Abs(y))
			{
				if (x > 0f)
				{
					result = MoveDirection.Right;
				}
				else
				{
					result = MoveDirection.Left;
				}
			}
			else if (y > 0f)
			{
				result = MoveDirection.Up;
			}
			else
			{
				result = MoveDirection.Down;
			}
			return result;
		}

		protected static GameObject FindCommonRoot(GameObject g1, GameObject g2)
		{
			GameObject result;
			if (g1 == null || g2 == null)
			{
				result = null;
			}
			else
			{
				Transform transform = g1.transform;
				while (transform != null)
				{
					Transform transform2 = g2.transform;
					while (transform2 != null)
					{
						if (transform == transform2)
						{
							result = transform.gameObject;
							return result;
						}
						transform2 = transform2.parent;
					}
					transform = transform.parent;
				}
				result = null;
			}
			return result;
		}

		protected void HandlePointerExitAndEnter(PointerEventData currentPointerData, GameObject newEnterTarget)
		{/*	called in
			StandaloneInputModule
				ProcessTouchPress
				ProcessMousePress
			PointerInputModule
				ProcessMove
				ClearSelection
			sets PointerEventData.pointerEnter
			call OnPointerEnter and/or OnPointerExit depending on what is under the pointer
			OnPointerEnter is called on only the first one that catches raycast
			OnPointerExit is called either on the fist or all of in the stack
			update hovered stack
		*/
			if (newEnterTarget == null || currentPointerData.pointerEnter == null)
			{
				for (int i = 0; i < currentPointerData.hovered.Count; i++)
				{
					ExecuteEvents.Execute<IPointerExitHandler>(currentPointerData.hovered[i], currentPointerData, ExecuteEvents.pointerExitHandler);
				}
				currentPointerData.hovered.Clear();
				if (newEnterTarget == null)
				{
					currentPointerData.pointerEnter = newEnterTarget;
					return;
				}
			}
			/*newEnterTarget != null*/
			if (!(currentPointerData.pointerEnter == newEnterTarget) || !newEnterTarget)
			/*pointerEnter is something other than newEnter target, or null*/
			{
				GameObject gameObject = BaseInputModule.FindCommonRoot(currentPointerData.pointerEnter, newEnterTarget);
				if (currentPointerData.pointerEnter != null)
				{
					Transform transform = currentPointerData.pointerEnter.transform;
					while (transform != null)
					{
						if (gameObject != null && gameObject.transform == transform)
						{
							break;
						}
						ExecuteEvents.Execute<IPointerExitHandler>(transform.gameObject, currentPointerData, ExecuteEvents.pointerExitHandler);
						currentPointerData.hovered.Remove(transform.gameObject);
						transform = transform.parent;
					}
				}
				currentPointerData.pointerEnter = newEnterTarget;
				if (newEnterTarget != null)
				{
					Transform transform2 = newEnterTarget.transform;
					while (transform2 != null && transform2.gameObject != gameObject)
					{
						ExecuteEvents.Execute<IPointerEnterHandler>(transform2.gameObject, currentPointerData, ExecuteEvents.pointerEnterHandler);
						currentPointerData.hovered.Add(transform2.gameObject);
						transform2 = transform2.parent;
					}
				}
			}
		}

		protected virtual AxisEventData GetAxisEventData(float x, float y, float moveDeadZone)
		{/*	used in StandaloneInputModule.SendMoveEventToSelectedObject()
			create new AxisEventData and assign m_AxisEventData to it
			update its fields
		*/
			if (this.m_AxisEventData == null)
			{
				this.m_AxisEventData = new AxisEventData(this.eventSystem);
			}
			this.m_AxisEventData.Reset();
			this.m_AxisEventData.moveVector = new Vector2(x, y);
			this.m_AxisEventData.moveDir = BaseInputModule.DetermineMoveDirection(x, y, moveDeadZone);
			return this.m_AxisEventData;
		}

		protected virtual BaseEventData GetBaseEventData()
		{
			if (this.m_BaseEventData == null)
			{
				this.m_BaseEventData = new BaseEventData(this.eventSystem);
			}
			this.m_BaseEventData.Reset();
			return this.m_BaseEventData;
		}

		public virtual bool IsPointerOverGameObject(int pointerId)
		{
			return false;
		}

		public virtual bool ShouldActivateModule()
		{
			return base.enabled && base.gameObject.activeInHierarchy;
		}

		public virtual void DeactivateModule()
		{
		}

		public virtual void ActivateModule()
		{
		}

		public virtual void UpdateModule()
		{
		}

		public virtual bool IsModuleSupported()
		{
			return true;
		}
	}
}