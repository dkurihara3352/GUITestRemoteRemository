
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using InventorySystem;
using MyUtility;

[RequireComponent(typeof(Image))]
public class Slottable : MonoBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerUpHandler, IPointerDownHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler
{


	InventoryItem m_item;
	public ItemInstance m_itemInstance;
	public InventoryItem m_Item{
		get{return m_item;}
		set{m_item = value;}
	}
	SlotGroup m_origSG;
	public SlotGroup m_OrigSG{
		get{return m_origSG;}
		set{
			m_origSG = value;
		}
	}
	
	GameObject newObj;



	GameObject m_draggedIcon;
	
	RectTransform m_draggedPlane;
	
	bool m_isPickupTimerOn = false;
	public RectTransform m_rectTrans;
	Image m_image;

	
	float m_pickupTimer = 0f;
	float m_pickupTime = .3f;
	AxisScroller m_axisScroller;
	public Text m_quantText;
	Text m_pickedQuantText;
	EventSystem m_eventSystem;
	Color m_emptyColor = new Color(.7f, .7f, .7f);
	
	[UnityEngine.SerializeField]
	float m_pickUpDistThreshold = 5f;
	
	
	Color m_pickUpTargetCol = Color.blue;
	private int m_quantity;
	public int m_Quantity{
		get{
			return m_quantity;
		}
		set{
			if(value == m_quantity) return;
			
			if(value < 0) 
				value = 0;
			
			m_quantity = value;
			
			if(m_Item.isStackable){
				if(m_quantText == null)
					m_quantText = transform.GetChild(0).GetComponent<Text>();
				m_quantText.text = m_quantity.ToString();
			}
			if(m_quantity == 0){
				m_image.color = m_emptyColor;
			}else{
				m_image.color = m_initCol;
			}
		}
	}

	private int m_pickAmount = 0;
	public int m_PickAmount{
		get{return m_pickAmount;}
		set{
			if(m_pickAmount == value) return;
			
			int sum = m_Quantity+ m_pickAmount;
			
			if(value > sum) 
				value = sum;

			m_Quantity = sum - value;
			m_pickAmount = value;

			if(m_Item.isStackable){

				if(m_pickedQuantText!= null)
					m_pickedQuantText.text = value.ToString();
			}
		}
	}

	float m_offsetMult = .02f;
	bool m_isPointerMoved = false;
	bool m_isResetTimerOn = false;
	bool m_isWFRDone = true;
	Color m_initCol;
	bool m_isTouchedInside = false;
	bool m_isTouchedOutside = false;
	bool m_isMovingToSlot = false;
	
	public bool m_isOnSlot = false;
	public Slot m_hoverSlot = null;
	public bool m_isPickedUp = false;
	LayoutElement m_layoutElement;
	Canvas m_canvas;
	SlotGroupManager m_slotGroupManager;

	public void InitHierarchyDependents(){
		this.m_axisScroller = FindInParents<AxisScroller>(gameObject);
		this.m_canvas = FindInParents<Canvas>(gameObject);
	}
	public void Initialize(SlotGroup slotGroup, ItemInstance itemInstance, InventoryItem item, int quantity){
		
		this.m_OrigSG = slotGroup;
		this.m_Item = item;
		this.m_itemInstance = itemInstance;
		this.m_image = GetComponent<Image>();
		this.m_image.sprite = m_Item.itemSprite;

		
		this.m_slotGroupManager = FindObjectOfType<SlotGroupManager>();
		this.m_eventSystem = FindObjectOfType<EventSystem>();
		this.m_quantText = transform.GetChild(0).GetComponent<Text>();
		this.m_rectTrans = GetComponent<RectTransform>();
		this.m_layoutElement = GetComponent<LayoutElement>();
		m_layoutElement.ignoreLayout = true;

		this.m_initCol = m_image.color;
		
		if(!this.m_Item.isStackable){
			// this.m_Quantity = 1;
			this.m_quantText.enabled = false;
		// }else{

		}
		this.m_Quantity = quantity;
		
		this.m_defaultColor = Color.white;
		this.m_hoveredColor = Color.cyan;
		this.m_equippedColor = new Color(.3f, .3f ,.3f);
		this.m_deactiveColor = new Color(.2f, .2f, 1f);
	}
	
	void Start(){
		
	}

	
	void StopPickUpTimer(){
		if(m_isPickupTimerOn)
		m_isPickupTimerOn = false;
		m_pickupTimer = 0f;
	
	}

	IEnumerator WaitAndPickUp(PointerEventData eventData){
		//make the timer count up only while the eventData.delta stuff is under certain threshold.
		//while counting up, if delta gets higher than a threshold(finger is moved), Reset.
		//Reset breaks early, reset the timer == 0f,
		m_isPickupTimerOn = true;
		m_image.color = m_initCol;
		
		
		while(m_isPickupTimerOn){	

			if(eventData.delta.sqrMagnitude >= m_pickUpDistThreshold * m_pickUpDistThreshold){
				
				m_isPointerMoved = true;
				StopPickUpTimer();
				m_image.color = m_initCol;
				
				yield break;
			}

			m_pickupTimer += Time.unscaledDeltaTime;

			if(m_pickupTimer >= m_pickupTime){
				
				
				PickUp(eventData);
				yield break;
			}

			if(m_eventSystem.enabled == false){
				StopPickUpTimer();
				
				m_image.color = m_initCol;
		
				yield break;
			}

			m_image.color = Color.Lerp(m_initCol, m_pickUpTargetCol, m_pickupTimer/m_pickupTime);
			yield return null;
		}
	
	}
	public void CheckQuantityColor(){
		if(m_Quantity <= 0){
			m_image.color = m_emptyColor;
		}
	}
	IEnumerator ChangeColor(Color from, Color to){
		float t = 0f;
		m_image.color = to;
		while(true){
			if(t > 1f){
				m_image.color = to;
				
				CheckQuantityColor();
				yield break;
			}
			m_image.color = Color.Lerp(from, to, t);

			t += Time.unscaledDeltaTime/ .5f;
			yield return null;
		}
	}
	public void OnPointerClick(PointerEventData eventData){
	}

	public void OnInitializePotentialDrag(PointerEventData eventData){
		if(!m_isWFRDone) eventData.pointerDrag = this.gameObject;
		else if(m_axisScroller != null && !m_isPickedUp){

			eventData.pointerDrag = m_axisScroller.gameObject;
			m_axisScroller.OnInitializePotentialDrag(eventData);
			
		}
	}
	
	public void OnBeginDrag(PointerEventData eventData)
	{

	}


	public void OnNonEventDrop(PointerEventData eventData){
		// StartCoroutine(ChangeColor(Color.cyan, Color.white));
		SetHoverState(false);

	}

	GameObject CreateAndSetupDraggedIcon(Canvas canvas, ref Text pickedText){
		// DebugUtility.PrintGreen(this.m_itemInstance.name + "'s CreateAndSetupDraggedIcon is entered");
		GameObject draggedIcon = new GameObject("draggedIcon");
		draggedIcon.transform.SetParent(canvas.transform, false);
		draggedIcon.transform.SetAsLastSibling();

		Image addedImage = draggedIcon.AddComponent<Image>();
		CanvasGroup addedCanvasGroup = draggedIcon.AddComponent<CanvasGroup>();
		addedCanvasGroup.blocksRaycasts = false;

		addedImage.sprite = GetComponent<Image>().sprite;
		draggedIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(80f, 80f);
		if(m_Item.isStackable){

			GameObject textObj = new GameObject();
			
			textObj.transform.parent = draggedIcon.transform;
			Text text = textObj.AddComponent<Text>();
			text.transform.localPosition = m_quantText.transform.localPosition;
			text.alignment = TextAnchor.MiddleCenter;
			text.rectTransform.anchorMax = Vector2.one;
			text.rectTransform.anchorMin = Vector2.zero;
			text.rectTransform.sizeDelta = Vector2.one;
			text.color = m_quantText.color;
			text.fontSize = m_quantText.fontSize;
			text.font = m_quantText.font;
			pickedText = text;
		}else{
			pickedText = null;
		}
		m_slotGroupManager.draggedIcon = draggedIcon;
		string str;
		if(m_draggedIcon == null)
			str = "null";
		else
			str = m_draggedIcon.gameObject.name;
		// DebugUtility.PrintGreen(this.m_itemInstance.name + "'s CreateAndSetupDraggedIcon is left with m_draggedIcon: " + str);
		
		
		return draggedIcon;
		
	}
	void PickUp(PointerEventData eventData){
		StopPickUpTimer();
		
		if(eventData.pointerDrag != null){
			AxisScroller axisScroller = eventData.pointerDrag.GetComponent<AxisScroller>();
			if(axisScroller == null) DebugUtility.PrintPink("axisScroller that is being drag is not present at pickup, because pointerDrag is " + eventData.pointerDrag.name);
			else{
				axisScroller.OnEndDrag(eventData);
				axisScroller.StopMovement();
			}
		}
		eventData.pointerDrag = this.gameObject;

		if (m_canvas == null)
			return;
		m_draggedIcon = CreateAndSetupDraggedIcon(m_canvas, ref m_pickedQuantText);
			
		m_draggedPlane = m_canvas.transform as RectTransform;
		
		m_isPickedUp = true;
		m_PickAmount = 1;
		
		m_slotGroupManager.FilterDestination(this);
			/*	for all SGs in the scene, check to see if it can hold the currenly picked slottable
					and if so, activate
			
				for all the destination candidate SGs' slottables
				check to see if origSG can hold the slottable
					and if so, make it activate
			*/
		
		SetDraggedPosition(eventData);
		Probe(eventData);
		
	}
	public void OnDrag(PointerEventData eventData)
	{
		if(!m_isPickedUp){
			if(m_axisScroller != null){

				m_axisScroller.OnAxisDrag(eventData);
			}
		}else{
			if(m_draggedIcon != null)
				SetDraggedPosition(eventData);
		}
		Probe(eventData);
		
	}

	void Probe(PointerEventData eventData){
		m_slotGroupManager.GetSGandSlottableUnderCursor(ref m_slottableUnderCursor, ref m_SGUnderCursor, eventData);
	}
	private void SetDraggedPosition(PointerEventData eventData)
	{

		RectTransform draggedIconRT = m_draggedIcon.GetComponent<RectTransform>();
		Vector3 globalMousePos;

		Vector2 screenPoint = new Vector2(eventData.position.x - Screen.width *m_offsetMult, eventData.position.y + Screen.width * m_offsetMult);
		if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_draggedPlane,screenPoint, eventData.pressEventCamera, out globalMousePos))
		{
			
			draggedIconRT.position = globalMousePos;
			draggedIconRT.rotation = m_draggedPlane.rotation;
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		
	}
	
	public void OnPointerDown(PointerEventData eventData){
		
		m_isPointerMoved = false;
		if(m_Quantity > 0){
			if(!m_isPickedUp)
				StartCoroutine(WaitAndPickUp(eventData));
			
			if(EventSystem.current != null && !EventSystem.current.alreadySelecting)
				EventSystem.current.SetSelectedGameObject(gameObject, eventData);
			m_isTouchedInside = true;

		}
	}

	
	public void OnPointerUp(PointerEventData eventData){
		Transact(eventData);
		
	}

	SlotGroup m_SGUnderCursor;
	Slottable m_slottableUnderCursor;

	
	
	void Transact(PointerEventData eventData){
		StopPickUpTimer();
		if(m_isPickedUp){

			SlotGroup targetSG = null;
			bool swapped = false;
			bool added = false;
			Slottable swappedSlottable = null;



			if(m_slottableUnderCursor != null){// on some slottable
				if(m_slottableUnderCursor != this){// the sb is not the orig
					m_slottableUnderCursor.OnNonEventDrop(eventData);
					targetSG = m_SGUnderCursor;
					if(targetSG != null && targetSG != m_OrigSG){//it's in some other SG

						if(this.m_Item.isStackable){
							// if(targetSG.GetSlotIndex(this.m_Item.itemId) != -1){
							if(m_slottableUnderCursor.m_Item.itemId == this.m_Item.itemId){
								/*	this is stackable and SBUC is same item
								*/
								if(m_OrigSG.isSubtractable){

									Add(targetSG);
									added = true;
								}
								
							}else{
								/*	this is stackable but SBUC has different item
								*/
								Swap(m_slottableUnderCursor);
								swapped = true;
								swappedSlottable = m_slottableUnderCursor;
							}
						}else{
							/*	this is not stackable
							*/
							if(m_slottableUnderCursor.m_itemInstance != this.m_itemInstance){

								Swap(m_slottableUnderCursor);
								swapped = true;
								swappedSlottable = m_slottableUnderCursor;
							}else{
								if(m_OrigSG.isSubtractable){

									Add(targetSG);
									added = true;
								}
							}
						}
					}else{//targetSG == null || targetSG == m_OrigSG
						StartCoroutine(Revert(eventData));
						return;
					}
				}else{//m_slottableUnderCursor == this
					
					OnNonEventDrop(eventData);
					m_isTouchedInside = false;
					m_isTouchedOutside = false;
					StartCoroutine(WaitAndReset(eventData));
					return;
				}

			}else{//slottableUnderCursor == null
				if(m_SGUnderCursor != null){
					if(m_SGUnderCursor != m_OrigSG){// in some other SG
						
						targetSG = m_SGUnderCursor;
						if(m_SGUnderCursor.SlotCount == 1){
							if(m_SGUnderCursor[0].slottable != null){
								if(m_SGUnderCursor[0].slottable.m_Item.itemId == this.m_Item.itemId && this.m_Item.isStackable){
									/*	this is stackable and the only Slottable the SG has has the same item
									*/
									if(m_OrigSG.isSubtractable){

										Add(m_SGUnderCursor);
										added = true;
									}
								}else{
									/*	this is not stackable, nor the only slottable has different item
									*/
									Swap(m_SGUnderCursor[0].slottable);
									swapped = true;
									swappedSlottable = m_SGUnderCursor[0].slottable;
								}
							}else{
								/*	the only slot doesn't have a slottable so fill it
								*/
								if(m_OrigSG.isSubtractable){

									Add(m_SGUnderCursor);
									added = true;
								}
							}
						}else{//more than one slot
							if(m_SGUnderCursor.HasVacancy){
								//filled
								if(m_OrigSG.isSubtractable){

									Add(m_SGUnderCursor);
									added = true;
								}
							}else{//no vacancy
								if(m_SGUnderCursor.isExpandable){
									if(m_OrigSG.isSubtractable){

										Add(m_SGUnderCursor);
										added = true;
									}
								}
								else if(m_SGUnderCursor.GetSlotIndex(m_Item.itemId) != -1){//the SG has same item
									if(this.m_Item.isStackable){
										/*	same and stackable
										*/
										if(m_OrigSG.isSubtractable){

											Add(m_SGUnderCursor);
											added = true;
										}
									}
									// else{
									// 	/* this is not stackable 
									// 	*/
									// 	StartCoroutine(Revert(eventData));
									// 	return;
									// }
								}
								// else{
								// 	/*	no same item in the sg
								// 	*/
								// 	StartCoroutine(Revert(eventData));
								// 	return;
								// }
							}
						}

					}else{//SGUC is the orig SG, and there's no SBUC
						StartCoroutine(Revert(eventData));
						return;
					}
				}else{//there's no SGUC nor SBUC
					StartCoroutine(Revert(eventData));
					return;
				}
			}
			if(!added && !swapped){
				StartCoroutine(Revert(eventData));
			}else{

				targetSG.CleanUpData();
				this.m_OrigSG.CleanUpData();
				targetSG.SortData();
				this.m_OrigSG.SortData();
				targetSG.UpdateSlots(eventData);
				this.m_OrigSG.UpdateSlots(eventData);
				
				this.StartCoroutine(this.MoveToSlot(targetSG, targetSG.GetNewSlotRect(this), eventData));
				if(swapped){
					
					swappedSlottable.m_draggedIcon = swappedSlottable.CreateAndSetupDraggedIcon(m_canvas, ref swappedSlottable.m_pickedQuantText);
					swappedSlottable.m_draggedIcon.GetComponent<RectTransform>().anchoredPosition = 
					ConvertRectPosToCanvasPos(swappedSlottable.m_OrigSG.GetSlotRect(swappedSlottable), eventData);
					swappedSlottable.StartCoroutine(swappedSlottable.MoveToSlot(this.m_OrigSG, this.m_OrigSG.GetNewSlotRect(swappedSlottable), eventData));
				}
			}
		}else{
			m_isTouchedInside = false;
			m_isTouchedOutside = false;
			StartCoroutine(WaitAndReset(eventData));
		}
	}

	Vector2 ConvertRectPosToCanvasPos(RectTransform rectTrans, PointerEventData eventData){
		Vector2 worldPos = rectTrans.position;
		Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, worldPos);
		RectTransform canvasRect = m_canvas.GetComponent<RectTransform>();
		Vector3 canvasWorldPos;
		RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, screenPos, eventData.pressEventCamera, out canvasWorldPos);
		Vector2 canvasPos = new Vector2(canvasWorldPos.x - canvasRect.rect.width *.5f, canvasWorldPos.y - canvasRect.rect.height *.5f);

		return canvasPos;
	}
	
	void Swap(Slottable targetSlottable){
		SlotGroup targetSG = targetSlottable.m_OrigSG;
		targetSG.RemoveItemFromData(targetSlottable, targetSlottable.m_Quantity);
		this.m_OrigSG.RemoveItemFromData(this, this.m_PickAmount);

		targetSG.AddItemToData(this, this.m_PickAmount);
		this.m_OrigSG.AddItemToData(targetSlottable, targetSlottable.m_Quantity);
	}

	void Add(SlotGroup sg){
		sg.AddItemToData(this, this.m_PickAmount);
		this.m_OrigSG.RemoveItemFromData(this, this.m_PickAmount);
	}

	
	void OnNonEventTap(PointerEventData eventData){
		
		StartCoroutine(ChangeColor(Color.magenta, Color.white));
	}

	
	IEnumerator WaitAndReset(PointerEventData eventData){
		
		float m_resetTimer = 0f;
		m_isWFRDone = false;
		m_isResetTimerOn = true;
		while(m_isResetTimerOn){

			if(m_resetTimer > m_pickupTime){
		
				
				if(m_isPickedUp){
					DestroyDraggedIcon();
					m_PickAmount = 0;
				}
				else{
					if(!m_isPointerMoved)
						OnNonEventTap(eventData);
					
				}
				
				m_isResetTimerOn = false;
				m_isWFRDone = true;
				m_resetTimer = 0f;
				m_slotGroupManager.SetActiveStateAll(true);
				yield break;
			}

			if(!m_isResetTimerOn){
				
				m_isWFRDone = true;
				m_resetTimer = 0f;
				m_slotGroupManager.SetActiveStateAll(true);
				
				yield break;
			}

			if(m_isTouchedOutside){
				m_isTouchedOutside = false;

				
				if(m_isPickedUp)
					StartCoroutine(Revert(eventData));
				
				m_isResetTimerOn = false;
				m_isWFRDone = true;
				m_resetTimer = 0f;
				m_slotGroupManager.SetActiveStateAll(true);
				yield break;
			}

			if(m_isTouchedInside){
				
				m_isTouchedInside = false;

				if(!m_isPickedUp)
					PickUp(eventData);
				else if(m_Item.isStackable)
					m_PickAmount ++;
				
				if(eventData.pointerDrag != null){
				
					AxisScroller axisScroller = eventData.pointerDrag.GetComponent<AxisScroller>();
					if(axisScroller != null){
						axisScroller.OnEndDrag(eventData);
						axisScroller.StopMovement();
					}
						
				}
		
				eventData.pointerDrag = this.gameObject;
				print("comes as far as here");
				
				m_isWFRDone = true;
				m_isResetTimerOn = false;
				m_resetTimer = 0f;
				// m_slotGroupManager.SetActiveStateAll(true);
				yield break;
			}

			m_resetTimer += Time.deltaTime;

			
			
			yield return null;
		}
		
	}
	void DestroyDraggedIcon(){
		if (m_draggedIcon != null)
		Destroy(m_draggedIcon);

		m_draggedIcon = null;
		
		m_isPickedUp = false;
		
	}

	

	static public T FindInParents<T>(GameObject go) where T : Component
	{
		if (go == null) return null;
		var comp = go.GetComponent<T>();

		if (comp != null)
			return comp;
		
		var t = go.transform.parent;
		while (t != null && comp == null)
		{
			comp = t.gameObject.GetComponent<T>();
			t = t.parent;
		}
		return comp;
	}


	
	public void OnSelect(BaseEventData eventData){
		
	}
	public void OnDeselect(BaseEventData eventData){
		/*	called when touching something that is not currently selected,
			or, drag is released
		*/	

		m_isTouchedOutside = true;
		
	}

	
	public bool m_isMovable = false;
	public bool m_isDoneMoving = true;
	
	public AnimationCurve m_moveCurve;
	public float m_travelTime = .5f;

	public IEnumerator Move(RectTransform targetRT, Vector2 initPos, Vector2 targetPos, float decRate){
		
		float dist = (targetPos - initPos).magnitude;
		
		float t = 0f;
		
		m_isMovable = true;
		m_isDoneMoving = false;
		
		m_eventSystem.enabled = false;
		
		while(m_isMovable){

			if(!m_isMovable){
				
				m_isDoneMoving = true;
				m_eventSystem.enabled = true;
				yield break;
			}

			if(t > 1f){
				targetRT.anchoredPosition = targetPos;
				m_isMovable = false;
				m_isDoneMoving = true;
				m_eventSystem.enabled = true;
				yield break;
			}

			float value = m_moveCurve.Evaluate(t);

			Vector2 targetThisFrame = Vector2.Lerp(initPos, targetPos, value);

			targetRT.anchoredPosition = targetThisFrame;
			

			t += Time.unscaledDeltaTime/ m_travelTime;

			yield return null;
		}
	}

	public IEnumerator MoveWithinSG(RectTransform targetSlot, float decRate, PointerEventData eventData){
		
		/*	gotta move remained childrened to the slotRect
		*/

		// Vector2 initPosSS = m_rectTrans.position;
		
		Detach(eventData);
		RectTransform panelRect = (RectTransform)m_OrigSG.panel;
		Vector2 targetPos;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRect, targetSlot.position, eventData.pressEventCamera, out targetPos);

		targetPos.x += (panelRect.pivot.x - .5f) * panelRect.rect.width;
		targetPos.y += (panelRect.pivot.y - .5f) * panelRect.rect.height;

		Vector2 initPos = m_rectTrans.anchoredPosition;
		// RectTransform thisSlotRect = m_OrigSG.GetSlotRect(this);
				
		// Vector2 targetPos;
		
		// RectTransformUtility.ScreenPointToLocalPointInRectangle(thisSlotRect, targetSlot.position, eventData.pressEventCamera, out targetPos);

		DebugUtility.PrintBlue(this.m_itemInstance.name + "'s MWSG: targetSlot.position: " + targetSlot.position.ToString() + ", targetPos: " + targetPos.ToString() + ", m_rectTrans.anchPos: " + m_rectTrans.anchoredPosition.ToString());
		
		yield return StartCoroutine(Move(this.m_rectTrans, initPos, /*targetPos*/targetPos, decRate));
		Attach(targetSlot);
	}

	IEnumerator Revert(PointerEventData eventData){
		/*	first need to convert the point in the canvas into a corresponding point in the orig slot rect
			and set that as the destination
		*/
		RectTransform canvasRect = m_canvas.GetComponent<RectTransform>();
		RectTransform origSlot = m_OrigSG.GetSlotRect(this);
		// Vector2 origSlotPosScreenSpace = RectTransformUtility.WorldToScreenPoint(null, origSlot.position);
		Vector2 targetPos;
		RectTransformUtility.ScreenPointToLocalPointInRectangle
		(canvasRect, origSlot.position, eventData.pressEventCamera, out targetPos);

		// Vector2 targetPos = new Vector2(posOnCanvas.x - canvasRect.rect.width * .5f, posOnCanvas.y - canvasRect.rect.height * .5f);
		
		RectTransform iconRT = m_draggedIcon.GetComponent<RectTransform>();

		Vector2 iconPos = iconRT.anchoredPosition;
		// Vector2 iconPosScreenSpace = RectTransformUtility.WorldToScreenPoint(null, iconRT.position);



		Vector2 origPos = m_rectTrans.anchoredPosition;
		
		
		// DebugUtility.PrintBlue(this.m_itemInstance.name + "'s reverted: origSlotWorPos: " + origSlot.position.ToString() + ", origScreenPos: " + origSlotPosScreenSpace.ToString() + ", targetPos: " + targetPos.ToString());
		yield return StartCoroutine(Move(iconRT, iconPos,/*origPos*/targetPos, .5f));

		DestroyDraggedIcon();
		
		m_PickAmount = 0;
		m_slotGroupManager.SetActiveStateAll(true);
	}
	
	

	public IEnumerator MoveToSlot(SlotGroup slotGroup, RectTransform slot, PointerEventData eventData){
		// Vector2 newPos = slotGroup.GetNewPos(this);
		
		/*	if there's no dragged icon obj create it here
		*/
		// Transform iconTrans = m_draggedIcon.transform;
		RectTransform iconRT = m_draggedIcon.GetComponent<RectTransform>();
		// Vector2 curPos = new Vector2(iconTrans.position.x, iconTrans.position.y);
		Vector2 curPos = iconRT.anchoredPosition;

		RectTransform canvasRect = m_canvas.GetComponent<RectTransform>();
		// Vector2 newSlotPosScreenSpace = RectTransformUtility.WorldToScreenPoint(null, slot.position);
		Vector3 newSlotPosOnCanvas = Vector3.zero;
		RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, /*newSlotPosScreenSpace*/slot.position, eventData.pressEventCamera, out newSlotPosOnCanvas);

		Vector2 newPos = new Vector2(newSlotPosOnCanvas.x - canvasRect.rect.width * .5f, newSlotPosOnCanvas.y - canvasRect.rect.height * .5f);


		yield return StartCoroutine(Move(iconRT, curPos, newPos, .5f));
		
		DestroyDraggedIcon();
		// m_PickAmount = 0;
		m_pickAmount = 0;
		// Attach(slot);
		/*	call slotGroup.CompleteSlotsUpdate()
			Destroy Self
		*/

		slotGroup.CompleteSlotsUpdate();
		m_slotGroupManager.SetActiveStateAll(true);
		if(this.m_OrigSG.m_slotGroupType != SlotGroupType.Pool)
		Destroy(this.gameObject);
		// DebugUtility.PrintBlue(this.m_itemInstance.name + "'s MoveToSlot's end is reached ");
	}
	Color m_equippedColor;
	
	bool m_isHovered = false;
	Color m_hoveredColor;
	Color m_defaultColor;
	public void SetHoverState(bool hovered){
		if(hovered){
			m_isHovered = true;
			m_image.color = m_hoveredColor;
		}else{
			m_isHovered = false;
			m_image.color = m_defaultColor;
			CheckQuantityColor();
		}
	}

	public bool m_isActiveForSelection = true;
	Color m_deactiveColor;

	public void SetActiveForSelectionState(bool active){
		if(active){
			m_isActiveForSelection = true;
			m_image.color = m_defaultColor;
			CheckQuantityColor();
		}else{
			m_isActiveForSelection = false;
			m_image.color = m_deactiveColor;
		}
	}

	public void Attach(RectTransform slot){
		transform.SetParent(slot);
		m_rectTrans.anchorMax = Vector2.one;
		m_rectTrans.anchorMin = Vector2.zero;
		m_rectTrans.sizeDelta = Vector2.zero;
		m_rectTrans.anchoredPosition = Vector2.zero;
		
	}

	public void Detach(PointerEventData eventData){
		RectTransform prevPar = (RectTransform)transform.parent;
		RectTransform panelRect = (RectTransform)m_OrigSG.panel;
		Vector2 prevPosSS = prevPar.position;
		Vector2 newPosOnPanel;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRect, prevPosSS, eventData.pressEventCamera, out newPosOnPanel);
		/*	m_OrigSG.panel
		*/
		newPosOnPanel.x += (panelRect.pivot.x - .5f) * panelRect.rect.width;
		newPosOnPanel.y += (panelRect.pivot.y - .5f) *panelRect.rect.height;

		transform.SetParent(m_OrigSG.panel);
		transform.SetAsLastSibling();
		m_rectTrans.anchorMax = new Vector2(.5f, .5f);
		m_rectTrans.anchorMin = new Vector2(.5f, .5f);
		// m_rectTrans.anchoredPosition = m_OrigSG.GetSlotRect(this).anchoredPosition;
		m_rectTrans.anchoredPosition = /*prevPar.anchoredPosition*/newPosOnPanel;
		m_rectTrans.sizeDelta = new Vector2(80f, 80f);
		

	}

	public bool ChildIsPointerEnter(PointerEventData eventData){
		bool result = false;
		for (int i = 0; i < transform.childCount; i++)
		{
			if(transform.GetChild(i).gameObject == eventData.pointerEnter)
				result = true;
		}
		return result;
	}
}
