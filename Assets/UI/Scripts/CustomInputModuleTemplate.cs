using System;
using UnityEngine.Serialization;
using System.Collections.Generic;

namespace UnityEngine.EventSystems
{
	[AddComponentMenu("Event/Custom Input Module")]
	public class CustomInputModuleTemplate : PointerInputModule
	{
		[Obsolete("Mode is no longer needed on input module as it handles both mouse and keyboard simultaneously.", false)]
		public enum InputMode
		{
			Mouse,
			Buttons
		}
		private List<GameObject> m_popupStack = new List<GameObject>();
		private GameObject m_topPopup{
			get{
				if(m_popupStack.Count == 0) 
					return null;
				else
					return m_popupStack[m_popupStack.Count - 1];
			}
		}

		public void AddPopup(GameObject popupHandler){
			Touch touch = new Touch();
			bool pressed;
			bool released;
			PointerEventData touchPointerEventData = base.GetTouchPointerEventData(touch, out pressed, out released);

			if(m_topPopup != null){
				ExecuteEvents.Execute<IPopupDefocusHandler>(m_topPopup, touchPointerEventData, CustomEvents.popupDefocusHandler);
			}
			m_popupStack.Add(popupHandler);
			
			ExecuteEvents.Execute<IPopupFocusHandler>(m_topPopup, touchPointerEventData, CustomEvents.popupFocusHandler);
			
		}
		private float m_PrevActionTime;

		private Vector2 m_LastMoveVector;

		private int m_ConsecutiveMoveCount = 0;

		private Vector2 m_LastMousePosition;

		private Vector2 m_MousePosition;

		[SerializeField]
		private string m_HorizontalAxis = "Horizontal";

		[SerializeField]
		private string m_VerticalAxis = "Vertical";

		[SerializeField]
		private string m_SubmitButton = "Submit";

		[SerializeField]
		private string m_CancelButton = "Cancel";

		[SerializeField]
		private float m_InputActionsPerSecond = 10f;

		[SerializeField]
		private float m_RepeatDelay = 0.5f;

		[FormerlySerializedAs("m_AllowActivationOnMobileDevice"), SerializeField]
		private bool m_ForceModuleActive;
		[SerializeField]
		private bool supportMultiTouch;

		[Obsolete("Mode is no longer needed on input module as it handles both mouse and keyboard simultaneously.", false)]
		public CustomInputModuleTemplate.InputMode inputMode
		{
			get
			{
				return CustomInputModuleTemplate.InputMode.Mouse;
			}
		}

		[Obsolete("allowActivationOnMobileDevice has been deprecated. Use forceModuleActive instead (UnityUpgradable) -> forceModuleActive")]
		public bool allowActivationOnMobileDevice
		{
			get
			{
				return this.m_ForceModuleActive;
			}
			set
			{
				this.m_ForceModuleActive = value;
			}
		}

		public bool forceModuleActive
		{
			get
			{
				return this.m_ForceModuleActive;
			}
			set
			{
				this.m_ForceModuleActive = value;
			}
		}

		public float inputActionsPerSecond
		{
			get
			{
				return this.m_InputActionsPerSecond;
			}
			set
			{
				this.m_InputActionsPerSecond = value;
			}
		}

		public float repeatDelay
		{
			get
			{
				return this.m_RepeatDelay;
			}
			set
			{
				this.m_RepeatDelay = value;
			}
		}

		public string horizontalAxis
		{
			get
			{
				return this.m_HorizontalAxis;
			}
			set
			{
				this.m_HorizontalAxis = value;
			}
		}

		public string verticalAxis
		{
			get
			{
				return this.m_VerticalAxis;
			}
			set
			{
				this.m_VerticalAxis = value;
			}
		}

		public string submitButton
		{
			get
			{
				return this.m_SubmitButton;
			}
			set
			{
				this.m_SubmitButton = value;
			}
		}

		public string cancelButton
		{
			get
			{
				return this.m_CancelButton;
			}
			set
			{
				this.m_CancelButton = value;
			}
		}

		protected CustomInputModuleTemplate()
		{
		}

		public override void UpdateModule()
		{
			this.m_LastMousePosition = this.m_MousePosition;
			this.m_MousePosition = base.input.mousePosition;
		}

		public override bool IsModuleSupported()
		{
			return this.m_ForceModuleActive || base.input.mousePresent || base.input.touchSupported;
		}

		public override bool ShouldActivateModule()
		{/*	returns true if
			(UIBehaviour.enabled && UIBehaviour.gameObject.activeInHierarchy)
			and 
			if there's any raw input there
		*/
			bool result;
			if (!base.ShouldActivateModule())/*!(UIBehaviour.enabled && UIBehaviour.gameObject.activeInHierarchy)*/
			{
				result = false;
			}
			else
			{
				bool flag = this.m_ForceModuleActive;
				flag |= base.input.GetButtonDown(this.m_SubmitButton);
				flag |= base.input.GetButtonDown(this.m_CancelButton);
				flag |= !Mathf.Approximately(base.input.GetAxisRaw(this.m_HorizontalAxis), 0f);
				flag |= !Mathf.Approximately(base.input.GetAxisRaw(this.m_VerticalAxis), 0f);
				flag |= ((this.m_MousePosition - this.m_LastMousePosition).sqrMagnitude > 0f);
				flag |= base.input.GetMouseButtonDown(0);
				if (base.input.touchCount > 0)
				{
					flag = true;
				}
				result = flag;
			}
			return result;
		}

		public override void ActivateModule()
		{/*	initialization
			if eventSystem.firstSelectedGameObject is set with any value it is selected
		*/
			base.ActivateModule();/*nothing in there*/
			this.m_MousePosition = base.input.mousePosition;
			this.m_LastMousePosition = base.input.mousePosition;
			GameObject gameObject = base.eventSystem.currentSelectedGameObject;
			if (gameObject == null)
			{
				gameObject = base.eventSystem.firstSelectedGameObject;
			}
			base.eventSystem.SetSelectedGameObject(gameObject, this.GetBaseEventData());
		}

		public override void DeactivateModule()
		{
			base.DeactivateModule();/*nothing in there*/
			base.ClearSelection();
		}

		public override void Process()
		{/*	hub
			called in EventSystem.Update()
			
			call SendUpdateEventToSelectedObject
			if the above returns false, call SendMoveEventToSelectedObject
			if the above returns false, call SendSubmitEventToSelectedObject
			
			call ProcessTouchEvents
			if the above returns false and input.mousePresent(if there's no touch), call ProcessMouseEvent
		*/
			bool flag = this.SendUpdateEventToSelectedObject();
			if (base.eventSystem.sendNavigationEvents)/*true by default*/
			{
				if (!flag)
				{
					flag |= this.SendMoveEventToSelectedObject();
				}
				if (!flag)
				{
					this.SendSubmitEventToSelectedObject();
				}
			}
			if (!this.ProcessTouchEvents() && base.input.mousePresent)
			{
				this.ProcessMouseEvent();
			}
		}

		private bool ProcessTouchEvents()
		{/*	call PointerInputModule.ProcessMove and PointerInputModule.ProcessDrag for every touches, if the touch is not canceled or ended this frame
			call ProcessTouchPress
			returns true if there's any touches registered
		*/
			for (int i = 0; i < base.input.touchCount; i++)
			{
				Touch touch = base.input.GetTouch(i);
				if (touch.type != TouchType.Indirect)
				{
					bool pressed;
					bool flag;/*true if released*/
					PointerEventData touchPointerEventData = base.GetTouchPointerEventData(touch, out pressed, out flag);
					
					this.ProcessTouchPress(touchPointerEventData, pressed, flag);
					if (!flag)
					{
						this.ProcessMove(touchPointerEventData);
						//ProcessDelayedPress
						this.ProcessDrag(touchPointerEventData);
					}
					else
					{
						base.RemovePointerData(touchPointerEventData);
					}
				}
				if(!supportMultiTouch){
					if(base.input.touchCount>0) return true;
					
					return false;
				}
			}
			return base.input.touchCount > 0;
		}
		Vector2 prevDelta;
		

		protected override void ProcessDrag(PointerEventData pointerEvent)
		{/*	used in StandaloneInputModule.ProcessToucheEvents() and StandaloneInputModule.ProcessMouseEvent(int id)
			call OnBeginDrag
			call OnPointerUp, if for some reason pointerPress != pointerDrag
			call OnDrag

			pointerDrag could be DragHandler, HorizontalDragHandler, or VerticalDragHandler
		*/
			if (pointerEvent.IsPointerMoving() && Cursor.lockState != CursorLockMode.Locked && !(pointerEvent.pointerDrag == null))
			{
				if (!pointerEvent.dragging && ShouldStartDrag(pointerEvent.pressPosition, pointerEvent.position, (float)base.eventSystem.pixelDragThreshold, pointerEvent.useDragThreshold))
				{
					ExecuteEvents.Execute<IBeginDragHandler>(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.beginDragHandler);
					pointerEvent.dragging = true;
				}
				if (pointerEvent.dragging)
				{
					// if (pointerEvent.pointerPress != pointerEvent.pointerDrag)
					// {
					// 	ExecuteEvents.Execute<IPointerUpHandler>(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);
					// 	pointerEvent.pointerPress = null;
					// 	pointerEvent.eligibleForClick = false;
					// 	pointerEvent.rawPointerPress = null;
					// }
					print("pointerDrag is " + pointerEvent.pointerDrag.gameObject.name);
					if(!ExecuteEvents.CanHandleEvent<IHorizontalDragHandler>(pointerEvent.pointerDrag) && !ExecuteEvents.CanHandleEvent<IVerticalDragHandler>(pointerEvent.pointerDrag) && ExecuteEvents.CanHandleEvent<IDragHandler>(pointerEvent.pointerDrag)){
						ExecuteEvents.Execute<IDragHandler>(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.dragHandler);
					}else{

						float thresh = (float)base.eventSystem.pixelDragThreshold;

						if(pointerEvent.delta.sqrMagnitude > (thresh * thresh)){
										
							if(Mathf.Abs(Vector2.Dot(Vector2.right, pointerEvent.delta.normalized))> Mathf.Cos(45f * Mathf.Deg2Rad)){
								if(ExecuteEvents.CanHandleEvent<IHorizontalDragHandler>(pointerEvent.pointerDrag)){
									ExecuteEvents.Execute<IHorizontalDragHandler>(pointerEvent.pointerDrag, pointerEvent, CustomEvents.horizontalDragHandler);
								}else{
									GameObject prevPointerDrag = pointerEvent.pointerDrag;
									pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IHorizontalDragHandler>(pointerEvent.pointerCurrentRaycast.gameObject);
									if(pointerEvent.pointerDrag != null){
										// print("switch to h");

										ExecuteEvents.Execute<IEndDragHandler>(prevPointerDrag, pointerEvent, ExecuteEvents.endDragHandler);
										
										ExecuteEvents.Execute<IInitializePotentialDragHandler>(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
										ExecuteEvents.Execute<IBeginDragHandler>(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.beginDragHandler);
										ExecuteEvents.Execute<IHorizontalDragHandler>(pointerEvent.pointerDrag, pointerEvent, CustomEvents.horizontalDragHandler);
										
										
									}else{
										// print("no handler for h");
										pointerEvent.pointerDrag = prevPointerDrag;
										ExecuteEvents.Execute<IDragHandler>(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.dragHandler);
									}
								}
							}else{
								if(ExecuteEvents.CanHandleEvent<IVerticalDragHandler>(pointerEvent.pointerDrag)){
									ExecuteEvents.Execute<IVerticalDragHandler>(pointerEvent.pointerDrag, pointerEvent, CustomEvents.verticalDragHandler);
								}else{
									GameObject prevPointerDrag = pointerEvent.pointerDrag;
									pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IVerticalDragHandler>(pointerEvent.pointerCurrentRaycast.gameObject);
									if(pointerEvent.pointerDrag != null){
										// print("switch to v");
										ExecuteEvents.Execute<IEndDragHandler>(prevPointerDrag, pointerEvent, ExecuteEvents.endDragHandler);

										ExecuteEvents.Execute<IInitializePotentialDragHandler>(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
										ExecuteEvents.Execute<IBeginDragHandler>(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.beginDragHandler);
										ExecuteEvents.Execute<IVerticalDragHandler>(pointerEvent.pointerDrag, pointerEvent, CustomEvents.verticalDragHandler);
										
										
									}else{
										// print("no handler for v");
										pointerEvent.pointerDrag = prevPointerDrag;
										ExecuteEvents.Execute<IDragHandler>(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.dragHandler);
									}

								}
							}
								
						}else{
								// print("under thresh");

								if(Mathf.Abs(Vector2.Dot(Vector2.right, pointerEvent.delta.normalized))> Mathf.Cos(45f * Mathf.Deg2Rad)){
									if(ExecuteEvents.CanHandleEvent<IHorizontalDragHandler>(pointerEvent.pointerDrag))
										ExecuteEvents.Execute<IHorizontalDragHandler>(pointerEvent.pointerDrag, pointerEvent, CustomEvents.horizontalDragHandler);
									else
										ExecuteEvents.Execute<IDragHandler>(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.dragHandler);

								}else{
									
									if(ExecuteEvents.CanHandleEvent<IVerticalDragHandler>(pointerEvent.pointerDrag))
										ExecuteEvents.Execute<IVerticalDragHandler>(pointerEvent.pointerDrag, pointerEvent, CustomEvents.verticalDragHandler);
									else
										ExecuteEvents.Execute<IDragHandler>(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.dragHandler);
								}
						}
					}
					prevDelta = pointerEvent.delta;
					
				}
			}
		}
		private static bool ShouldStartDrag(Vector2 pressPos, Vector2 currentPos, float threshold, bool useDragThreshold)
		{
			return !useDragThreshold || (pressPos - currentPos).sqrMagnitude >= threshold * threshold;
		}

		/*	protected void ProcessDelayedPress(CustomEventData eventData){
				//if pointerPressed remains the same && eventData.eligibleForDelayedPress , increment timer
				//if timer expires
					//call OnDelayedPointerDown
					//make sure this does not called again until new touch
						//timer is reset
						//eventData.eligibleForDelayedPress = false
							//eligibleForDelayedPress is reset to true when a new touch is detected
			}
		*/

		protected void ProcessTouchPress(PointerEventData pointerEvent, bool pressed, bool released)
		{/*	called in ProcessTouchEvents
			sets PointerEventData.pointerEnter
			sets PointerEventData.pointerPress
			sets PointerEventData.rawPointerPress
			sets PointerEventData.pointerDrag

			on pressed this frame
				calls OnPointerDown
				calls OnInitializePotentialDrag

			on released this frame
				call OnPointerUp
				call OnPointerClick
				call OnDrop
				call OnEndDrag
				call OnPointerExit
		*/
			GameObject gameObject = pointerEvent.pointerCurrentRaycast.gameObject;
			if (pressed)/*this frame, new touch*/
			{
				pointerEvent.eligibleForClick = true;
				pointerEvent.delta = Vector2.zero;
				pointerEvent.dragging = false;
				pointerEvent.useDragThreshold = true;
				pointerEvent.pressPosition = pointerEvent.position;
				pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;
				base.DeselectIfSelectionChanged(gameObject, pointerEvent);
				if (pointerEvent.pointerEnter != gameObject)
				{
					base.HandlePointerExitAndEnter(pointerEvent, gameObject/*newEnterTarget*/);
					pointerEvent.pointerEnter = gameObject;
				}
				/*pointerEnter == gameobject*/
				GameObject gameObject2 = ExecuteEvents.ExecuteHierarchy<IPointerDownHandler>(gameObject, pointerEvent, ExecuteEvents.pointerDownHandler);/*gameObject2 is the one int the hierarchy that handles the event. gameObject is the starting obj*/
				if (gameObject2 == null)
				{/*	if none of gameObjects in the hierarchy can handle OnPointerDown, then assigne gameObject2 with the one in the hierarchy that handles OnPointerClick...????
				*/
					gameObject2 = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
				}
				float unscaledTime = Time.unscaledTime;
				if (gameObject2 == pointerEvent.lastPress)
				{/*	if gameObject2 is the lastly pressed object
					and if the pressed is registered within .3 secs after the prev one, increment clickCount
					else clickCount = 1
				*/
					float num = unscaledTime - pointerEvent.clickTime;
					if (num < 0.3f)
					{
						pointerEvent.clickCount++;
					}
					else
					{
						pointerEvent.clickCount = 1;
					}
					pointerEvent.clickTime = unscaledTime;
				}
				else/*gameObject2 == null || != pointerEvent.lastPress*/
				{
					pointerEvent.clickCount = 1;
				}
				

				pointerEvent.pointerPress = gameObject2;/*could be null*/
				pointerEvent.rawPointerPress = gameObject;/*could be null...*/
				pointerEvent.clickTime = unscaledTime;
				pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(gameObject);
				
				if (pointerEvent.pointerDrag != null)
				{
					ExecuteEvents.Execute<IInitializePotentialDragHandler>(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
				}

				if(m_topPopup != null){
					RectTransform rt= m_topPopup.GetComponent<RectTransform>();
					if(rt != null){
						if(!RectTransformUtility.RectangleContainsScreenPoint(rt, pointerEvent.position)){
							// print("touched outside");
							ExecuteEvents.Execute<IPopupHideHandler>(m_topPopup, pointerEvent, CustomEvents.popupHideHandler);
							m_popupStack.Remove(m_topPopup);
							if(m_topPopup != null){
								ExecuteEvents.Execute<IPopupFocusHandler>(m_topPopup, pointerEvent, CustomEvents.popupFocusHandler);
							}
						}
					}
				}


			}
			if (released)/*this frame*/
			{
				ExecuteEvents.Execute<IPointerUpHandler>(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);
				GameObject eventHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
				if (pointerEvent.pointerPress == eventHandler && pointerEvent.eligibleForClick)
				{
					ExecuteEvents.Execute<IPointerClickHandler>(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
				}
				else if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
				{
					ExecuteEvents.ExecuteHierarchy<IDropHandler>(gameObject, pointerEvent, ExecuteEvents.dropHandler);
				}
				pointerEvent.eligibleForClick = false;
				pointerEvent.pointerPress = null;
				pointerEvent.rawPointerPress = null;
				if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
				{
					ExecuteEvents.Execute<IEndDragHandler>(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);
					prevDelta = Vector2.zero;
				}
				pointerEvent.dragging = false;
				pointerEvent.pointerDrag = null;
				if (pointerEvent.pointerDrag != null)/*??*/
				{
					ExecuteEvents.Execute<IEndDragHandler>(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);
					prevDelta = Vector2.zero;
				}
				pointerEvent.pointerDrag = null;
				ExecuteEvents.ExecuteHierarchy<IPointerExitHandler>(pointerEvent.pointerEnter, pointerEvent, ExecuteEvents.pointerExitHandler);
				pointerEvent.pointerEnter = null;
			}
		}

		protected bool SendSubmitEventToSelectedObject()
		{/*	call OnSubmit and OnCancel
			return true is event is used
			returns false if
				there's no currentSelectedGameObject
				the event is not used
		*/
			bool result;
			if (base.eventSystem.currentSelectedGameObject == null)
			{
				result = false;
			}
			else
			{
				BaseEventData baseEventData = this.GetBaseEventData();
				if (base.input.GetButtonDown(this.m_SubmitButton))
				{
					ExecuteEvents.Execute<ISubmitHandler>(base.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.submitHandler);
				}
				if (base.input.GetButtonDown(this.m_CancelButton))
				{
					ExecuteEvents.Execute<ICancelHandler>(base.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.cancelHandler);
				}
				result = baseEventData.used;
			}
			return result;
		}

		private Vector2 GetRawMoveVector()
		{/*	make the values 1 or -1
		*/
			Vector2 zero = Vector2.zero;
			zero.x = base.input.GetAxisRaw(this.m_HorizontalAxis);
			zero.y = base.input.GetAxisRaw(this.m_VerticalAxis);
			if (base.input.GetButtonDown(this.m_HorizontalAxis))
			{
				if (zero.x < 0f)
				{
					zero.x = -1f;
				}
				if (zero.x > 0f)
				{
					zero.x = 1f;
				}
			}
			if (base.input.GetButtonDown(this.m_VerticalAxis))
			{
				if (zero.y < 0f)
				{
					zero.y = -1f;
				}
				if (zero.y > 0f)
				{
					zero.y = 1f;
				}
			}
			return zero;
		}

		protected bool SendMoveEventToSelectedObject()
		{/*	calls OnMove if certain condition is met
			returns true if the event is used
			returns false if 
				there's no input
				enough time has not passed
				event not used
		*/
			float unscaledTime = Time.unscaledTime;
			Vector2 rawMoveVector = this.GetRawMoveVector();
			bool result;
			if (Mathf.Approximately(rawMoveVector.x, 0f) && Mathf.Approximately(rawMoveVector.y, 0f))
			{
				this.m_ConsecutiveMoveCount = 0;
				result = false;
			}
			else
			{
				bool flag = base.input.GetButtonDown(this.m_HorizontalAxis) || base.input.GetButtonDown(this.m_VerticalAxis);
				bool flag2 = Vector2.Dot(rawMoveVector, this.m_LastMoveVector) > 0f;/*true if direction is SAME (lastMoveVector and rawMoveVector are NOT perpendicular)*/
				if (!flag)/*no axis input*/
				{
					if (flag2 && this.m_ConsecutiveMoveCount == 1)
					{
						flag = (unscaledTime > this.m_PrevActionTime + this.m_RepeatDelay);/*true if enough time has passed*/
					}
					else
					{
						flag = (unscaledTime > this.m_PrevActionTime + 1f / this.m_InputActionsPerSecond);/*true if enough time has passed*/
					}
				}
				if (!flag)
				{
					result = false;
				}
				else/*flag is true*/
				{
					AxisEventData axisEventData = this.GetAxisEventData(rawMoveVector.x, rawMoveVector.y, 0.6f);
					if (axisEventData.moveDir != MoveDirection.None)
					{
						ExecuteEvents.Execute<IMoveHandler>(base.eventSystem.currentSelectedGameObject, axisEventData, ExecuteEvents.moveHandler);
						if (!flag2)/*direction is DIFFERENT*/
						{
							this.m_ConsecutiveMoveCount = 0;
						}
						this.m_ConsecutiveMoveCount++;
						this.m_PrevActionTime = unscaledTime;
						this.m_LastMoveVector = rawMoveVector;
					}
					else
					{
						this.m_ConsecutiveMoveCount = 0;
					}
					result = axisEventData.used;
				}
			}
			return result;
		}

		protected void ProcessMouseEvent()
		{
			this.ProcessMouseEvent(0);
		}

		protected virtual bool ForceAutoSelect()
		{
			return false;
		}

		protected void ProcessMouseEvent(int id)
		{/*	called from Process, but the argument is always 0
			process for all the three buttons
			
			call OnScroll
		*/
			PointerInputModule.MouseState mousePointerEventData = this.GetMousePointerEventData(id);/*id is irrelevant*/
			PointerInputModule.MouseButtonEventData eventData = mousePointerEventData.GetButtonState(PointerEventData.InputButton.Left).eventData;
			if (this.ForceAutoSelect())/*set to false as default*/
			{
				base.eventSystem.SetSelectedGameObject(eventData.buttonData.pointerCurrentRaycast.gameObject, eventData.buttonData);
			}
			this.ProcessMousePress(eventData);
			this.ProcessMove(eventData.buttonData);
			this.ProcessDrag(eventData.buttonData);
			this.ProcessMousePress(mousePointerEventData.GetButtonState(PointerEventData.InputButton.Right).eventData);
			this.ProcessDrag(mousePointerEventData.GetButtonState(PointerEventData.InputButton.Right).eventData.buttonData);
			this.ProcessMousePress(mousePointerEventData.GetButtonState(PointerEventData.InputButton.Middle).eventData);
			this.ProcessDrag(mousePointerEventData.GetButtonState(PointerEventData.InputButton.Middle).eventData.buttonData);
			if (!Mathf.Approximately(eventData.buttonData.scrollDelta.sqrMagnitude, 0f))
			{
				GameObject eventHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(eventData.buttonData.pointerCurrentRaycast.gameObject);
				ExecuteEvents.ExecuteHierarchy<IScrollHandler>(eventHandler, eventData.buttonData, ExecuteEvents.scrollHandler);
			}
		}

		protected bool SendUpdateEventToSelectedObject()
		{/*	if currentSelectedGameObject is null, return false
			call OnUpdateSelected on currentSelectedGameObject
			return whether the event is used or not
		*/
			bool result;
			if (base.eventSystem.currentSelectedGameObject == null)
			{
				result = false;
			}
			else
			{
				BaseEventData baseEventData = this.GetBaseEventData();
				ExecuteEvents.Execute<IUpdateSelectedHandler>(base.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.updateSelectedHandler);
				result = baseEventData.used;
			}
			return result;
		}

		protected void ProcessMousePress(PointerInputModule.MouseButtonEventData data)
		{/*	called in ProcessMouseEvent

			sets pointerPress
				pointerDrag
				rawPointerPress

			on press this frame
				call OnPointerDown
				call OnInitializePotentialDrag
			on release this frame
				call OnPointerUp
				call OnPointerClick
				call OnDrop
				call OnEndDrag
		*/
			PointerEventData buttonData = data.buttonData;
			GameObject gameObject = buttonData.pointerCurrentRaycast.gameObject;/*hit object. selected*/
			if (data.PressedThisFrame())
			{
				buttonData.eligibleForClick = true;
				buttonData.delta = Vector2.zero;
				buttonData.dragging = false;
				buttonData.useDragThreshold = true;
				buttonData.pressPosition = buttonData.position;
				buttonData.pointerPressRaycast = buttonData.pointerCurrentRaycast;
				base.DeselectIfSelectionChanged(gameObject, buttonData);
				GameObject gameObject2 = ExecuteEvents.ExecuteHierarchy<IPointerDownHandler>(gameObject, buttonData, ExecuteEvents.pointerDownHandler);/*pressed obj*/
				if (gameObject2 == null)/*if none turns out to handle OnPointerDown*/
				{
					gameObject2 = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
				}
				float unscaledTime = Time.unscaledTime;
				if (gameObject2 == buttonData.lastPress)
				{/*	if gameObject2 is the lastly pressed object
					and if the pressed is registered within .3 secs after the prev one, increment clickCount
					else clickCount = 1
				*/
					float num = unscaledTime - buttonData.clickTime;
					if (num < 0.3f)
					{
						buttonData.clickCount++;
					}
					else
					{
						buttonData.clickCount = 1;
					}
					buttonData.clickTime = unscaledTime;
				}
				else
				{
					buttonData.clickCount = 1;
				}
				buttonData.pointerPress = gameObject2;
				buttonData.rawPointerPress = gameObject;
				buttonData.clickTime = unscaledTime;
				buttonData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(gameObject);
				if (buttonData.pointerDrag != null)
				{
					ExecuteEvents.Execute<IInitializePotentialDragHandler>(buttonData.pointerDrag, buttonData, ExecuteEvents.initializePotentialDrag);
				}
				if(m_topPopup != null){
					RectTransform rt= m_topPopup.GetComponent<RectTransform>();
					if(rt != null){
						if(!RectTransformUtility.RectangleContainsScreenPoint(rt, buttonData.position)){
							print("touched outside");
							ExecuteEvents.Execute<IPopupHideHandler>(m_topPopup, buttonData, CustomEvents.popupHideHandler);
							m_popupStack.Remove(m_topPopup);
							if(m_topPopup != null){
								ExecuteEvents.Execute<IPopupFocusHandler>(m_topPopup, buttonData, CustomEvents.popupFocusHandler);
							}
						}
					}
				}
			}
			if (data.ReleasedThisFrame())
			{
				ExecuteEvents.Execute<IPointerUpHandler>(buttonData.pointerPress, buttonData, ExecuteEvents.pointerUpHandler);
				GameObject eventHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
				if (buttonData.pointerPress == eventHandler && buttonData.eligibleForClick)
				{
					ExecuteEvents.Execute<IPointerClickHandler>(buttonData.pointerPress, buttonData, ExecuteEvents.pointerClickHandler);
				}
				else if (buttonData.pointerDrag != null && buttonData.dragging)
				{
					ExecuteEvents.ExecuteHierarchy<IDropHandler>(gameObject, buttonData, ExecuteEvents.dropHandler);
				}
				buttonData.eligibleForClick = false;
				buttonData.pointerPress = null;
				buttonData.rawPointerPress = null;
				if (buttonData.pointerDrag != null && buttonData.dragging)
				{
					ExecuteEvents.Execute<IEndDragHandler>(buttonData.pointerDrag, buttonData, ExecuteEvents.endDragHandler);
				}
				buttonData.dragging = false;
				buttonData.pointerDrag = null;
				if (gameObject != buttonData.pointerEnter)
				{
					base.HandlePointerExitAndEnter(buttonData, null);
					base.HandlePointerExitAndEnter(buttonData, gameObject);
				}
			}
		}
	}
}