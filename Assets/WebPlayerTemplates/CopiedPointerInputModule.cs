using System;
using System.Collections.Generic;
using System.Text;

namespace UnityEngine.EventSystems
{
	public abstract class PointerInputModule : BaseInputModule
	{
		/*	the next three classes are used only in mouse control (not touch)
		*/
		protected class ButtonState
		/*	a pair of InputButton and MouseButtonEventState(a pair of PointerEventData and FramePressState)
		*/
		{
			private PointerEventData.InputButton m_Button = PointerEventData.InputButton.Left;

			private PointerInputModule.MouseButtonEventData m_EventData;

			public PointerInputModule.MouseButtonEventData eventData
			{
				get
				{
					return this.m_EventData;
				}
				set
				{
					this.m_EventData = value;
				}
			}

			public PointerEventData.InputButton button
			{
				get
				{
					return this.m_Button;
				}
				set
				{
					this.m_Button = value;
				}
			}
		}

		protected class MouseState
		/*	stores all the ButtonStates
			has ability to find if any is pressed or released
			has ability to get or set ButtonState from InputButton
		*/
		{
			private List<PointerInputModule.ButtonState> m_TrackedButtons = new List<PointerInputModule.ButtonState>();

			public bool AnyPressesThisFrame()
			{
				bool result;
				for (int i = 0; i < this.m_TrackedButtons.Count; i++)
				{
					if (this.m_TrackedButtons[i].eventData.PressedThisFrame())
					{
						result = true;
						return result;
					}
				}
				result = false;
				return result;
			}

			public bool AnyReleasesThisFrame()
			{
				bool result;
				for (int i = 0; i < this.m_TrackedButtons.Count; i++)
				{
					if (this.m_TrackedButtons[i].eventData.ReleasedThisFrame())
					{
						result = true;
						return result;
					}
				}
				result = false;
				return result;
			}

			public PointerInputModule.ButtonState GetButtonState(PointerEventData.InputButton button)
			{
				PointerInputModule.ButtonState buttonState = null;
				for (int i = 0; i < this.m_TrackedButtons.Count; i++)
				{
					if (this.m_TrackedButtons[i].button == button)
					{
						buttonState = this.m_TrackedButtons[i];
						break;
					}
				}
				if (buttonState == null)
				{
					buttonState = new PointerInputModule.ButtonState
					{
						button = button,
						eventData = new PointerInputModule.MouseButtonEventData()
					};
					this.m_TrackedButtons.Add(buttonState);
				}
				return buttonState;
			}

			public void SetButtonState(PointerEventData.InputButton button, PointerEventData.FramePressState stateForMouseButton, PointerEventData data)
			{
				PointerInputModule.ButtonState buttonState = this.GetButtonState(button);
				buttonState.eventData.buttonState = stateForMouseButton;
				buttonState.eventData.buttonData = data;
			}
		}

		public class MouseButtonEventData
		/*	a pair of PointerEventData and FramePressState
			has ability to inform if its pressed or released this frame
		*/
		{
			public PointerEventData.FramePressState buttonState;

			public PointerEventData buttonData;

			public bool PressedThisFrame()
			{
				return this.buttonState == PointerEventData.FramePressState.Pressed || this.buttonState == PointerEventData.FramePressState.PressedAndReleased;
			}

			public bool ReleasedThisFrame()
			{
				return this.buttonState == PointerEventData.FramePressState.Released || this.buttonState == PointerEventData.FramePressState.PressedAndReleased;
			}
		}

		public const int kMouseLeftId = -1;

		public const int kMouseRightId = -2;

		public const int kMouseMiddleId = -3;

		public const int kFakeTouchesId = -4;

		protected Dictionary<int, PointerEventData> m_PointerData = new Dictionary<int, PointerEventData>();
		/*	a collection of PointerEventData paired with its pointerID
		*/

		private readonly PointerInputModule.MouseState m_MouseState = new PointerInputModule.MouseState();
		/*	kind of like a static member
		*/

		protected bool GetPointerData(int id, out PointerEventData data, bool create)
		{/*	search in m_PointerData for PointerEventData that matches with id and data is assigned to it, return false
			if not found && create, a new pair is created and stored in m_PointerData, returns true
			if !create, returns false
		*/
			bool result;
			if (!this.m_PointerData.TryGetValue(id, out data) && create)
			{
				data = new PointerEventData(base.eventSystem)
				{
					pointerId = id
				};
				this.m_PointerData.Add(id, data);
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}

		protected void RemovePointerData(PointerEventData data)
		{
			this.m_PointerData.Remove(data.pointerId);
		}

		protected PointerEventData GetTouchPointerEventData(Touch input, out bool pressed, out bool released)
		{/*	called from in StandaloneInputModule.ProcessTouchEvents()
			creates a new PointerEventData with input.fingerId if there isn't one already
			pressed is assigned true if newly created or TouchPhase.Began
			released is assigned true if TouchPhase.Canceled or Ended
			and rest of PointerEventData fields are initialized
		*/
			PointerEventData pointerEventData;
			bool pointerData = this.GetPointerData(input.fingerId, out pointerEventData, true);
			pointerEventData.Reset();
			pressed = (pointerData/*newly created*/ || input.phase == TouchPhase.Began);
			released = (input.phase == TouchPhase.Canceled || input.phase == TouchPhase.Ended);
			if (pointerData/*newly created*/)
			{
				pointerEventData.position = input.position;
			}
			if (pressed)
			{
				pointerEventData.delta = Vector2.zero;
			}
			else
			{
				pointerEventData.delta = input.position - pointerEventData.position;
			}
			pointerEventData.position = input.position;
			pointerEventData.button = PointerEventData.InputButton.Left;
			base.eventSystem.RaycastAll(pointerEventData, this.m_RaycastResultCache);
			RaycastResult pointerCurrentRaycast = BaseInputModule.FindFirstRaycast(this.m_RaycastResultCache);
			pointerEventData.pointerCurrentRaycast = pointerCurrentRaycast;
			this.m_RaycastResultCache.Clear();
			return pointerEventData;
		}

		protected void CopyFromTo(PointerEventData from, PointerEventData to)
		{
			to.position = from.position;
			to.delta = from.delta;
			to.scrollDelta = from.scrollDelta;
			to.pointerCurrentRaycast = from.pointerCurrentRaycast;
			to.pointerEnter = from.pointerEnter;
		}

		protected PointerEventData.FramePressState StateForMouseButton(int buttonId)
		{/*	returns PointerEventData.FramePressState for buttonId,
			checking if mouse button is up or down this frame
		*/
			bool mouseButtonDown = base.input.GetMouseButtonDown(buttonId);
			bool mouseButtonUp = base.input.GetMouseButtonUp(buttonId);
			PointerEventData.FramePressState result;
			if (mouseButtonDown && mouseButtonUp)
			{
				result = PointerEventData.FramePressState.PressedAndReleased;
			}
			else if (mouseButtonDown)
			{
				result = PointerEventData.FramePressState.Pressed;
			}
			else if (mouseButtonUp)
			{
				result = PointerEventData.FramePressState.Released;
			}
			else
			{
				result = PointerEventData.FramePressState.NotChanged;
			}
			return result;
		}

		protected virtual PointerInputModule.MouseState GetMousePointerEventData()
		{
			return this.GetMousePointerEventData(0);
		}

		protected virtual PointerInputModule.MouseState GetMousePointerEventData(int id)
		{/*	
			* id is never used!
			used in StandaloneInputModule.ProcessMouseEvents(int id)
			creates a new PointerEventData with -1 for pointerId, if there isn't one already
			initialize the rest of PointerEventData fields
			creates or gets PointerEventData with -2 and -3, values copied from -1
			assign button fields of each of three PointerEventData with appropriate InputButton(Left,Right,Middle)
			Get StateForMouseButton for each PointerEvent, make 3 pairs and store them in m_MouseState and return it
		*/
			PointerEventData pointerEventData;
			bool pointerData = this.GetPointerData(-1, out pointerEventData, true);
			pointerEventData.Reset();
			if (pointerData/*newly created*/)
			{
				pointerEventData.position = base.input.mousePosition;
			}
			Vector2 mousePosition = base.input.mousePosition;
			if (Cursor.lockState == CursorLockMode.Locked)
			{
				pointerEventData.position = new Vector2(-1f, -1f);
				pointerEventData.delta = Vector2.zero;
			}
			else
			{
				pointerEventData.delta = mousePosition - pointerEventData.position;
				pointerEventData.position = mousePosition;
			}
			pointerEventData.scrollDelta = base.input.mouseScrollDelta;
			pointerEventData.button = PointerEventData.InputButton.Left;
			base.eventSystem.RaycastAll(pointerEventData, this.m_RaycastResultCache);
			RaycastResult pointerCurrentRaycast = BaseInputModule.FindFirstRaycast(this.m_RaycastResultCache);
			pointerEventData.pointerCurrentRaycast = pointerCurrentRaycast;
			this.m_RaycastResultCache.Clear();
			PointerEventData pointerEventData2;
			this.GetPointerData(-2, out pointerEventData2, true);
			this.CopyFromTo(pointerEventData, pointerEventData2);
			pointerEventData2.button = PointerEventData.InputButton.Right;
			PointerEventData pointerEventData3;
			this.GetPointerData(-3, out pointerEventData3, true);
			this.CopyFromTo(pointerEventData, pointerEventData3);
			pointerEventData3.button = PointerEventData.InputButton.Middle;
			this.m_MouseState.SetButtonState(PointerEventData.InputButton.Left, this.StateForMouseButton(0), pointerEventData);
			this.m_MouseState.SetButtonState(PointerEventData.InputButton.Right, this.StateForMouseButton(1), pointerEventData2);
			this.m_MouseState.SetButtonState(PointerEventData.InputButton.Middle, this.StateForMouseButton(2), pointerEventData3);
			return this.m_MouseState;
		}

		protected PointerEventData GetLastPointerEventData(int id)
		{/*	search in m_PointerData for PointerEventData that matches with id if there's one already there and return it, return null if not
		*/
			PointerEventData result;
			this.GetPointerData(id, out result, false);
			return result;
		}

		private static bool ShouldStartDrag(Vector2 pressPos, Vector2 currentPos, float threshold, bool useDragThreshold)
		{
			return !useDragThreshold || (pressPos - currentPos).sqrMagnitude >= threshold * threshold;
		}

		protected virtual void ProcessMove(PointerEventData pointerEvent)
		{/*	used in StandaloneInputModule.ProcessToucheEvents() and StandaloneInputModule.ProcessMouseEvent(int id)
			call BaseInputModule.HandlePointerExitAndEnter if the cursor is not locked
				=> OnPointerExit and OnPointerEnter is called in there
		*/
			GameObject newEnterTarget = (Cursor.lockState != CursorLockMode.Locked) ? pointerEvent.pointerCurrentRaycast.gameObject : null;
			base.HandlePointerExitAndEnter(pointerEvent, newEnterTarget);
		}

		protected virtual void ProcessDrag(PointerEventData pointerEvent)
		{/*	used in StandaloneInputModule.ProcessToucheEvents() and StandaloneInputModule.ProcessMouseEvent(int id)
			call OnBeginDrag
			call OnPointerUp, if for some reason pointerPress != pointerDrag
			call OnDrag
		*/
			if (pointerEvent.IsPointerMoving() && Cursor.lockState != CursorLockMode.Locked && !(pointerEvent.pointerDrag == null))
			{
				if (!pointerEvent.dragging && PointerInputModule.ShouldStartDrag(pointerEvent.pressPosition, pointerEvent.position, (float)base.eventSystem.pixelDragThreshold, pointerEvent.useDragThreshold))
				{
					ExecuteEvents.Execute<IBeginDragHandler>(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.beginDragHandler);
					pointerEvent.dragging = true;
				}
				if (pointerEvent.dragging)
				{
					if (pointerEvent.pointerPress != pointerEvent.pointerDrag)
					{
						ExecuteEvents.Execute<IPointerUpHandler>(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);
						pointerEvent.eligibleForClick = false;
						pointerEvent.pointerPress = null;
						pointerEvent.rawPointerPress = null;
					}
					ExecuteEvents.Execute<IDragHandler>(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.dragHandler);
				}
			}
		}

		public override bool IsPointerOverGameObject(int pointerId)
		{
			PointerEventData lastPointerEventData = this.GetLastPointerEventData(pointerId);
			return lastPointerEventData != null && lastPointerEventData.pointerEnter != null;
		}

		protected void ClearSelection()
		{/*	called in StandaloneInputModule.DeactivateModule()
			on every PointerEventData in m_PointerData, OnPointerExit is called on every obj under pointer and the hover stack is cleared
			OnDeselect is called on them
			eventSystem.m_CurrentSelected is set with null
		*/
			BaseEventData baseEventData = this.GetBaseEventData();
			foreach (PointerEventData current in this.m_PointerData.Values)
			{
				base.HandlePointerExitAndEnter(current, null);
			}
			this.m_PointerData.Clear();
			base.eventSystem.SetSelectedGameObject(null, baseEventData);
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder("<b>Pointer Input Module of type: </b>" + base.GetType());
			stringBuilder.AppendLine();
			foreach (KeyValuePair<int, PointerEventData> current in this.m_PointerData)
			{
				if (current.Value != null)
				{
					stringBuilder.AppendLine("<B>Pointer:</b> " + current.Key);
					stringBuilder.AppendLine(current.Value.ToString());
				}
			}
			return stringBuilder.ToString();
		}

		protected void DeselectIfSelectionChanged(GameObject currentOverGo, BaseEventData pointerEvent)
		{/*	called in StandaloneInputModule.ProcessTouchPress and StandaloneInputModule.ProcessMousePress
			call OnSelect to the newly selected object and OnDeslect on the prev one
		*/
			GameObject eventHandler = ExecuteEvents.GetEventHandler<ISelectHandler>(currentOverGo);/*search UP the hierarchy starting from currentOverGo for the one that can handle*/
			if (eventHandler != base.eventSystem.currentSelectedGameObject)
			{
				base.eventSystem.SetSelectedGameObject(null, pointerEvent);
			}
		}
	}
}