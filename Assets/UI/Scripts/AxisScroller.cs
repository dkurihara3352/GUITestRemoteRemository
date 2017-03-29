using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
using MyUtility;

public class AxisScroller : UIBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler{

	
	Image m_image;
	Color m_defCol = new Color(1f, 1f, 1f, .5f);
	Color m_red = new Color(1f, 0f, 0f, .5f);
	Color m_blue = new Color(0f, 0f, 1f, .5f);


	public List<RectTransform> m_elements;
	List<RectTransform> m_indexedElements;
	public bool m_loop;
	// public bool m_autoGen;
	// public float m_elementDimension;
	public bool m_isContinuous;
	public int m_initiallyFocusedIndex = 1;
	public GUISkin m_guiSkin;

	[System.SerializableAttribute]
	public class AxisDragFloatArgEvent: UnityEvent<float>{

	}
	[System.Serializable]
	public class AxisDragIntArgEvent: UnityEvent<int>{

	}

	public AxisDragFloatArgEvent onValueChanged = new AxisDragFloatArgEvent();
	public AxisDragIntArgEvent onFocus = new AxisDragIntArgEvent();

	public AnimationCurve m_focusCurve;

	private int m_axis = 0;
	
	RectTransform m_rectTrans;
	public float m_focusTime = .2f;

	[RangeAttribute(0f, 1f)]
	public float normalizedCursorPos = 0.5f;
	float m_cursorPosOnRect{
		get{
			if(m_rectTrans != null)
				return (normalizedCursorPos - .5f) * m_rectTrans.sizeDelta[m_axis];
			else return 0f;
		}
	}
	[RangeAttribute(0f, 1f)]
	public float m_normalizedPosOnRect = 0.5f;

	
	protected void SetAxis(int axis){
		m_axis = axis;
	}
	

	void SmoothFocus(RectTransform rt, float normalizedPosOnTargetRect ,float focusInitVel){
		
		
		float offset = (normalizedPosOnTargetRect - .5f) * ContentLength(rt);
		float targetPos = - ContentPointOnAxis(rt) + m_cursorPosOnRect - offset;
		
		StartCoroutine(MoveElements(targetPos, m_focusTime, focusInitVel));
	}
	void InstantFocus(RectTransform rt, float normalizedPosOnTargetRect){
		
		float offset = (normalizedPosOnTargetRect - .5f) *ContentLength(rt);

		float displacement = - ContentPointOnAxis(rt) + m_cursorPosOnRect - offset;
		if(m_elements != null){

			float newPoint = ContentPointOnAxis(m_elements[0]);
			newPoint += displacement;
			SetPosition(m_elements[0], newPoint);
			AlignElements();
		}
	}

	float m_totalContentLength;
	protected override void Start(){
		
		base.Start();
		InitializeElements();
		AlignElements();
		m_image = gameObject.GetComponent<Image>();
		m_image.color = m_defCol;
		m_rectTrans = GetComponent<RectTransform>();
		m_totalContentLength = GetTotalContentLength();
		InitializeCurve();
		
		InstantFocus(m_elements[m_initiallyFocusedIndex], .5f);
		
	}
	public void FocusEnds(bool top){
		if(!m_loop){
			if(m_isContinuous){
				if(top)
					SmoothFocus(m_axis == 0? m_elements[0]: m_elements[m_elements.Count - 1], GetMaxFocusTargetNormalizedPos(), 0f);
				else
					SmoothFocus(m_axis == 0? m_elements[m_elements.Count - 1]: m_elements[0], GetMinFocusTargetNormalizedPos(), 0f);
			}else{
				if(top)
					SmoothFocus(m_axis == 0? m_elements[0]: m_elements[m_elements.Count - 1], .5f, 0f);
				else
					SmoothFocus(m_axis == 0? m_elements[m_elements.Count - 1]: m_elements[0], .5f, 0f);	
			}
		}
	}
	void InitializeCurve(){
		
		m_focusCurve = new AnimationCurve();
		Keyframe key0 = new Keyframe(0f, 0f, 0f, 0f);
		Keyframe key1 = new Keyframe(1f, 1f, 0f, 0f);
		m_focusCurve.AddKey(0f, 0f);
		m_focusCurve.AddKey(1f, 1f);
		m_focusCurve.MoveKey(0, key0);
		m_focusCurve.MoveKey(1, key1);
	}

	float ContentLength(RectTransform rt){
		float result = -1f;
		result = m_axis == 0? rt.rect.width: rt.rect.height;
		return result;
	}

	float ContentPointOnAxis(RectTransform rt){
		
		Vector2 pivotOffset = new Vector2((rt.pivot.x - 0.5f) * rt.rect.width, (rt.pivot.y - 0.5f) * rt.rect.height);
		Vector2 correctedAnchPos = rt.anchoredPosition - pivotOffset;
		return m_axis ==0? correctedAnchPos.x: correctedAnchPos.y;
	}

	void SetPosition(RectTransform rt, Vector2 posOnRect){
		Vector2 pivotOffset = new Vector2((rt.pivot.x - 0.5f) * rt.rect.width, (rt.pivot.y - 0.5f) * rt.rect.height);
		Vector2 targetPos = posOnRect + pivotOffset;
		rt.anchoredPosition  = targetPos;
	}

	void SetPosition(RectTransform rt, float pointOnRect){
		Vector2 pivotOffset = new Vector2((rt.pivot.x - 0.5f) * rt.rect.width, (rt.pivot.y - 0.5f) * rt.rect.height);
		Vector2 targetPos = Vector2.zero;
		if(m_axis == 0){
			targetPos = new Vector2(pointOnRect + pivotOffset.x, rt.anchoredPosition.y);
		}else{
			targetPos = new Vector2(rt.anchoredPosition.x, pointOnRect + pivotOffset.y);
		}
		rt.anchoredPosition = targetPos;
	}


	float GetTotalContentLength(){
		float result = 0f;
		for (int i = 0; i < m_elements.Count; i++)
		{
			/*result += m_elements[i].sizeDelta[m_axis];*/
			result += ContentLength(m_elements[i]);
		}
		return result;
	}
	void InitializeElements(){
		if(m_elements == null){
			m_elements = new List<RectTransform>();
		}else{
			m_elements.Clear();
		}
		
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			if(child.GetComponent<RectTransform>())
				m_elements.Add(child.GetComponent<RectTransform>());
		}
		
		m_indexedElements = new List<RectTransform>();
		for (int i = 0; i < m_elements.Count; i++)
		{
			m_indexedElements.Add(m_elements[i]);
		}
		
	}
	public void StopMovement(){
		m_isMovable = false;
	}
	
	void AlignElements(){
		if(m_elements != null){
			float alignPoint = ContentPointOnAxis(m_elements[0]) - ContentLength(m_elements[0]) * .5f;
			for (int i = 0; i < m_elements.Count; i++)
			{
				float newPoint = alignPoint + ContentLength(m_elements[i]) * 0.5f;
				SetPosition(m_elements[i], newPoint);
				alignPoint = ContentPointOnAxis(m_elements[i]) + ContentLength(m_elements[i]) * .5f;
			}
		}
	}

	
	IEnumerator MoveElements(float targetPos, float travelTime, float initVel){
		while(!m_isDoneMoving){
			m_isMovable = false;
			yield return null;
		}
		float t = 0f;
	
		/*m_contentStartPos = m_elements[0].anchoredPosition[m_axis];*/		
		m_contentStartPos = ContentPointOnAxis(m_elements[0]);
		
		float tangentRad = (initVel == 0f || targetPos == 0f)? 0f: initVel/targetPos;
		Keyframe newKey = m_focusCurve[0];
			newKey.outTangent = tangentRad;
		
		m_focusCurve.MoveKey(0, newKey);
		
		m_isMovable = true;
		m_isDoneMoving = false;
		while(!m_isDoneMoving){
			if(!m_isMovable){
				m_isDoneMoving = true;
				
				yield break;
			}
			if(t>1f){
				
				float settledPoint = m_contentStartPos + targetPos;
				
				SetContentAnchoredPosition(settledPoint);
				m_isDoneMoving = true;
				onFocus.Invoke(GetIndex(GetCurrentElementUnderCursor()));
				yield break;
			}
			
			RectTransform rt = m_elements[0];
				
			float target = m_contentStartPos + targetPos;
			float value = m_focusCurve.Evaluate(t);
			float targetThisFrame = m_contentStartPos *(1f - value) + target *(value);

			t += Time.unscaledDeltaTime / travelTime;
			
			SetContentAnchoredPosition(targetThisFrame);
			
			yield return null;
		}
	}


	/*	GUI
	*/
		public bool m_showGUI = false;
		void OnGUI(){
			if(m_showGUI){

				GUI.skin = m_guiSkin;
				
				Rect guiRect = new Rect(10f, 10f, 300f, 700f);
				GUILayout.BeginArea(guiRect, GUI.skin.box);

					DrawElementsInfo();
					DrawSmoothFocus();
					GUILayout.Label("tangent: " + Mathf.Rad2Deg * Mathf.Atan(m_focusCurve.keys[0].outTangent));
					GUILayout.Label("releaseInitVel: " + m_releaseInitVel);
					GUILayout.Label("offset: " + GetOffset(0f).ToString());

					GUILayout.Label("pointerLocRecPos: " + m_pointerPosOnRect);
					GUILayout.Label("pointerDelta: " + m_pointerDeltaPos);
					GUILayout.Label("contentDelta: " + m_contDeltaPos);
					GUILayout.Label("current cursor value: " + GetCurrentCursorValue().ToString());
					// GUILayout.Label("current element index under cursor: " + GetCurrentElementIndexUnderCursor());
					GUILayout.Label("total content width: " + m_totalContentLength);
					GUILayout.Label("m_correctedDelta: " + m_correctedDelta);
				

				GUILayout.EndArea();
			}
		}
		void DrawElementsInfo(){
			if(m_elements != null)
				GUILayout.Label("elementsCount: " + m_elements.Count);
			else
				GUILayout.Label("elementsCount: null");
			GUILayout.BeginVertical();
			if(m_elements.Count >0)
			for (int i = 0; i < m_elements.Count; i++)
			{
				GUILayout.Label("elemet " + i + " pos.x:" + m_elements[i].anchoredPosition[0] + " pos.y:" + m_elements[i].anchoredPosition[1] + " width:" + m_elements[i].sizeDelta[0] + " height:" + m_elements[i].sizeDelta[1]);
			}
			
			GUILayout.EndVertical();
		}
		int m_chosenIndex;
		int m_ChosenIndex{
			get{return m_chosenIndex;}
			set{
				if(m_indexedElements!= null){
					if(value > m_indexedElements.Count -1)
						value = 0;
					else if (value < 0)
						value = m_indexedElements.Count -1;
				}
				m_chosenIndex = value;
			}
		}
		void DrawSmoothFocus(){
			GUILayout.BeginHorizontal();
				GUILayout.Label("chosenIndex: " + m_ChosenIndex.ToString());
				if(GUILayout.Button("+")){
					m_ChosenIndex ++;
				}
				if(GUILayout.Button("-")){
					m_ChosenIndex --;
				}
				if(GUILayout.Button("Move")){
					if(m_elements != null)
						SmoothFocus(m_indexedElements[m_ChosenIndex], m_normalizedPosOnRect, 0f);
				}
			GUILayout.EndHorizontal();
		}


	
	
	
	public void OnInitializePotentialDrag(PointerEventData eventData){
		
		m_isMovable = false;
		
	}

	float m_pointerStartPos;
	float m_contentStartPos;
	
	public void OnBeginDrag(PointerEventData eventData){


		Vector2 pointerStartPosV2 = Vector2.zero;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(m_rectTrans, eventData.position, eventData.pressEventCamera, out pointerStartPosV2);
		m_pointerStartPos = pointerStartPosV2[m_axis];
		// m_contentStartPos = m_elements[0].anchoredPosition[m_axis];
		m_contentStartPos = ContentPointOnAxis(m_elements[0]);
		m_RTUnderCursorAtTouch = GetCurrentElementUnderCursor();
	}
	Vector2 m_pointerPosOnRect = Vector2.zero;
	float m_pointerDeltaPos;
	float m_contDeltaPos;
	public void OnAxisDrag(PointerEventData eventData){
		
		
		m_image.color = m_red;
		
		RectTransformUtility.ScreenPointToLocalPointInRectangle(m_rectTrans, eventData.position, eventData.pressEventCamera, out m_pointerPosOnRect);
			
		m_pointerDeltaPos = m_pointerPosOnRect[m_axis] - m_pointerStartPos;
		
		m_contDeltaPos = m_contentStartPos + m_pointerDeltaPos;

		if(!m_loop){

			// float offset = GetOffset(m_contDeltaPos - m_elements[0].anchoredPosition[m_axis]);
			float offset = GetOffset(m_contDeltaPos - ContentPointOnAxis(m_elements[0]));
			m_contDeltaPos += offset;
		
			if(offset != 0f)
				// m_contDeltaPos -= RubberDelta(offset, m_rectTrans.sizeDelta[m_axis]);
				m_contDeltaPos -= RubberDelta(offset, ContentLength(m_rectTrans));
		}
		
		SetContentAnchoredPosition(m_contDeltaPos);
		
	}


	void CheckAndSwapElements(){
		
		if(m_loop){
			int curIndex = GetCurrentElementIndexUnderCursor();
			
			for (int i = 0; i < m_elements.Count +1; i++){
				
				if(curIndex == m_initiallyFocusedIndex)
					break;

				
				RectTransform minRT = m_elements[0];
				RectTransform maxRT = m_elements[m_elements.Count -1];
				RectTransform RTToMove = null;
				Vector2 newPos = Vector2.zero;
				float newPoint = -1f;

				if(curIndex < m_initiallyFocusedIndex){//scroll toward right
					
					newPoint = ContentPointOnAxis(minRT) - ContentLength(minRT) * .5f - ContentLength(maxRT) * .5f;
					RTToMove = maxRT;
				}else{
				
					newPoint = ContentPointOnAxis(maxRT) + ContentLength(maxRT) * .5f + ContentLength(minRT) * .5f;
					RTToMove = minRT;
				}

				float delta = ContentPointOnAxis(m_elements[0]) - m_contentStartPos;
				

				if(RTToMove == minRT){
					m_elements.RemoveAt(0);
					m_elements.Add(minRT);
				}else{
					m_elements.RemoveAt(m_elements.Count -1);
					m_elements.Insert(0, maxRT);
				}

				SetPosition(RTToMove, newPoint);				
				
				delta = ContentPointOnAxis(m_elements[0]) -delta;
				m_contentStartPos = delta;
				
				curIndex = GetCurrentElementIndexUnderCursor();
				
			}

		}else
			return;
		
	}
	

	float GetOffset(float delta){
		float result = 0f;
		RectTransform minRT = m_elements[0];
		RectTransform maxRT = m_elements[m_elements.Count - 1];
		
		float curMinContentPoint = ContentPointOnAxis(minRT) - ContentLength(minRT) * .5f + delta;
		float curMaxContentPoint = ContentPointOnAxis(maxRT) + ContentLength(maxRT) * .5f + delta;
		
		float viewRectMin = - ContentLength(m_rectTrans) *.5f;
		float viewRectMax = ContentLength(m_rectTrans) *.5f;

		float minMargin = m_cursorPosOnRect + ContentLength(m_rectTrans) *.5f - ContentLength(minRT) *.5f - (m_normalizedPosOnRect - .5f) * ContentLength(minRT);
		float contentMin = minMargin> 0f? curMinContentPoint - minMargin: curMinContentPoint;
		
		float maxMargin = m_cursorPosOnRect + ContentLength(m_rectTrans) *.5f - ContentLength(maxRT) *.5f - (m_normalizedPosOnRect - .5f) * ContentLength(minRT);
		float contentMax = maxMargin> 0f? curMaxContentPoint + maxMargin: curMaxContentPoint;


		if(viewRectMin - contentMin < 0)
			result = viewRectMin - contentMin;
		if(viewRectMax - contentMax > 0)
			result = viewRectMax - contentMax;

		return result; 
	}
	float m_correctedDelta;
	void SetContentAnchoredPosition(float newPos){
	
		RectTransform rt = m_elements[0];
		
		float curPoint = ContentPointOnAxis(rt);
		float totalDelta = newPos - curPoint;
		
		if(m_loop){
			
			m_correctedDelta = totalDelta % Mathf.Abs(m_totalContentLength);
			
			curPoint += m_correctedDelta;
			
		}else
			
			curPoint += totalDelta;

		SetPosition(rt, curPoint);
		
		AlignElements();
		if(m_loop)
			CheckAndSwapElements();
		onValueChanged.Invoke(GetCurrentCursorValue());
	}
	int GetIndex(RectTransform rt){
		int index = -1;
		for (int i = 0; i < m_indexedElements.Count; i++)
		{
			RectTransform indexedElement = m_indexedElements[i];
			if(indexedElement == rt)
				index = i;
		}
		return index;
	}

	float GetCurrentCursorValue(){
		RectTransform rt = GetCurrentElementUnderCursor();
		if(rt == null){
			return -2f;
		}
		int index = GetIndex(rt);
		
		float result = index + (m_cursorPosOnRect - ContentPointOnAxis(rt) - (ContentLength(rt) * .5f))/ContentLength(rt);

		return result;
	}
	public void OnDrag(PointerEventData eventData){
		
		m_image.color = m_blue;
		
		
	}
	public float m_offsetVelMult = .1f;
	public void OnEndDrag(PointerEventData eventData){
		m_image.color = m_defCol;
		
		float offset = GetOffset(0f);
		float offsetVel = offset * m_offsetVelMult;
		if(offset > 0){
			
			SmoothFocus(m_elements[m_elements.Count -1], GetMaxFocusTargetNormalizedPos(), offsetVel);
		}else if(offset < 0){
			
			SmoothFocus(m_elements[0], GetMinFocusTargetNormalizedPos(), offsetVel);
		}else{
			
			InertialTranslate(eventData);
		}
		
	}
	
	float GetMaxFocusTargetNormalizedPos(){
		RectTransform lastRect = m_elements[m_elements.Count -1];
		float result = -1f;
		
		float normalizedPosOffset = (m_normalizedPosOnRect - .5f) *ContentLength(lastRect);
		float contentMaxAtRect = m_cursorPosOnRect + ContentLength(m_rectTrans) * .5f + ContentLength(lastRect) *.5f + normalizedPosOffset;
		
		
		if(m_isContinuous && (m_axis == 0? m_rectTrans.rect.width: m_rectTrans.rect.height) < m_totalContentLength ){
			
			float maxMargin = ContentLength(m_rectTrans) - (m_cursorPosOnRect + ContentLength(m_rectTrans) * .5f);
			
			result = (ContentLength(lastRect) - maxMargin) / ContentLength(lastRect);
		
		}else{
			result = m_normalizedPosOnRect;
		}
		
		return result;
	}
	float GetMinFocusTargetNormalizedPos(){
		RectTransform firstRect = m_elements[0];
		float result = -1f;
		
		float normalizedPosOffset = (m_normalizedPosOnRect - .5f) * ContentLength(firstRect);
		
		float contentMinAtRect = m_cursorPosOnRect + ContentLength(m_rectTrans) * .5f - ContentLength(firstRect) *.5f + normalizedPosOffset;
		
		if(m_isContinuous && (m_axis == 0? m_rectTrans.rect.width: m_rectTrans.rect.height) < m_totalContentLength){
		
			float minMargin = m_cursorPosOnRect + ContentLength(m_rectTrans) * .5f;
			
			result = minMargin / ContentLength(firstRect);
		}else{
			result = m_normalizedPosOnRect;
			
		}
		
		return result;
	}
	public float m_scrollThresh = 4000f;
	public float m_flickThresh = 300f;
	public float m_maxReleaseVel = 5000f;
	void InertialTranslate(PointerEventData eventData){
		m_releaseInitVel = eventData.delta[m_axis] / Time.unscaledDeltaTime;
		if(Mathf.Abs(m_releaseInitVel) > m_maxReleaseVel){
			if(m_releaseInitVel > 0)
				m_releaseInitVel = m_maxReleaseVel;
			else
				m_releaseInitVel = - m_maxReleaseVel;
		}
		
		if(m_isContinuous){
			StartCoroutine(Decelerate(m_releaseInitVel));

		}else{
			if(Mathf.Abs(m_releaseInitVel) <= m_scrollThresh && Mathf.Abs(m_releaseInitVel)>= m_flickThresh){
				
				IncreOrDecre(m_releaseInitVel);
			}else{
				StartCoroutine(Decelerate(m_releaseInitVel));
			}
		}
	}
	
	RectTransform m_RTUnderCursorAtTouch;
	bool m_useInitVelOnFlick = false;
	void IncreOrDecre(float initVel){
		
		int refIndex = m_elements.IndexOf(m_RTUnderCursorAtTouch);
		RectTransform targetRT = null;
		int targetIndex = -1;
		if(initVel< 0){//swiping left or down
			if(!m_loop){
				if(refIndex == m_elements.Count -1)
					targetIndex = m_elements.Count -1;
				else
					targetIndex = refIndex + 1;
			}else{
				if(refIndex == m_elements.Count -1)
					targetIndex = 0;
				else
					targetIndex = refIndex + 1;
			}

		}else{
			if(!m_loop){
				if(refIndex == 0)
					targetIndex = 0;
				else
					targetIndex = refIndex - 1;
			}else{
				if(refIndex == 0)
					targetIndex = m_elements.Count - 1;
				else
					targetIndex = refIndex - 1;
			}
			
		}
		targetRT = m_elements[targetIndex];
		SmoothFocus(targetRT, .5f, m_useInitVelOnFlick? initVel: 0f);
		
	}
	public float m_deceleration = .001f;
	public float m_stopThresh = 1f;
	float m_releaseInitVel;
	bool m_isMovable;
	public float m_searchThresh = 200f;
	bool m_isDoneMoving = true;
	IEnumerator Decelerate(float initVel){
		
		while(!m_isDoneMoving){
			
			m_isMovable = false;
			yield return null;
		}

		float vel = initVel;
		m_isMovable = true;
		m_isDoneMoving = false;
		while(!m_isDoneMoving){
		
			if(!m_isMovable){
				m_isDoneMoving = true;
		
				yield break;
			}
			if(!m_loop){
				float offset = GetOffset(0f);
				if(offset != 0f){
					
					float offsetVel = offset * m_offsetVelMult;
					if(offset > 0){
						
						SmoothFocus(m_elements[m_elements.Count -1], GetMaxFocusTargetNormalizedPos(), offsetVel);
						
					}else{
						
						SmoothFocus(m_elements[0], GetMinFocusTargetNormalizedPos(), offsetVel);
					}	
					m_isDoneMoving = true;
					
					yield break;
				}
			}
			
			if(m_isContinuous){
				if(Mathf.Abs(vel) < m_stopThresh){
					
					m_isDoneMoving = true;
					
					yield break;
				}
			}else{// not Continuous
				
				if(Mathf.Abs(vel)< m_searchThresh){
					
					Snap(vel);
					
					m_isDoneMoving = true;
					
					yield break;
				}
			}
			
			
			float delta = vel * Time.unscaledDeltaTime;
			float targetPos = ContentPointOnAxis(m_elements[0]) + delta;
			
			vel *=  Mathf.Pow(m_deceleration,Time.unscaledDeltaTime);
			
			
			SetContentAnchoredPosition(targetPos);
			
			yield return null;
		}
	}
	void Snap(float snapInitVel){
		
		RectTransform snapTarget = GetCurrentElementUnderCursor();
		SmoothFocus(snapTarget, 0.5f, snapInitVel);
	}

	RectTransform GetCurrentElementUnderCursor(){
		RectTransform result = null;
		
		for (int i = 0; i < m_elements.Count; i++)
		{
			RectTransform eleRT = m_elements[i];
			
			if(ContentPointOnAxis(eleRT) - ContentLength(eleRT) *.5f <= m_cursorPosOnRect){
				if(ContentPointOnAxis(eleRT) + ContentLength(eleRT) *.5f >= m_cursorPosOnRect)
					result = eleRT;
			}
		}
		return result;
	}
	int GetCurrentElementIndexUnderCursor(){
		int result = -1;
		
		for (int i = 0; i < m_elements.Count; i++)
		{
			RectTransform eleRT = m_elements[i];
			
			float min = ContentPointOnAxis(eleRT) - ContentLength(eleRT) *.5f;
			float max = ContentPointOnAxis(eleRT) + ContentLength(eleRT) *.5f;
			if(min <= m_cursorPosOnRect){
				if(max >= m_cursorPosOnRect)
					result = i;
			}
			
		}
		if(result == -1){
			if(GetOffset(0f) > 0f)
				result = m_elements.Count;
			
		}
		return result;
	}
	public float m_rubberValue = 0.55f;
	/*	this makes it harder to drag content as it goes farther away from its designated pos
	*/
	private float RubberDelta(float overStretching, float viewSize){
		return (1f - 1f / (Mathf.Abs(overStretching) * /*0.55f*/m_rubberValue / viewSize + 1f)) * viewSize * Mathf.Sign(overStretching);
	}
	
	
	
}
