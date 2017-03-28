using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
using MyUtility;

public class AxisScroller : UIBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler{

	
	Image image;
	public Color defCol = new Color(1f, 1f, 1f, .5f);
	Color red = new Color(1f, 0f, 0f, .5f);
	Color blue = new Color(0f, 0f, 1f, .5f);


	public List<RectTransform> m_elements;
	List<RectTransform> m_indexedElements;
	public bool m_loop;
	public bool m_autoGen;
	public float m_elementDimension;
	public bool m_isContinuous;
	public int m_initiallyFocusedIndex = 1;
	public GUISkin guiSkin;

	[System.SerializableAttribute]
	public class AxisDragFloatArgEvent: UnityEvent<float>{

	}
	[System.Serializable]
	public class AxisDragIntArgEvent: UnityEvent<int>{

	}

	public AxisDragFloatArgEvent onValueChanged = new AxisDragFloatArgEvent();
	public AxisDragIntArgEvent onFocus = new AxisDragIntArgEvent();

	public AnimationCurve focusCurve;

	public int m_axis = 0;
	
	RectTransform m_rectTrans;
	public float focusTime = .2f;

	[RangeAttribute(0f, 1f)]
	public float normalizedCursorPos = 0.5f;
	float cursorPosOnRect{
		get{
			if(m_rectTrans != null)
				return (normalizedCursorPos - .5f) * m_rectTrans.sizeDelta[m_axis];
			else return 0f;
		}
	}
	[RangeAttribute(0f, 1f)]
	public float normalizedPosOnRect = 0.5f;

	


	void SmoothFocus(RectTransform rt, float normalizedPosOnTargetRect ,float focusInitVel){
		
		float offset = (normalizedPosOnTargetRect - .5f) * rt.sizeDelta[m_axis];
		float targetPos = - rt.anchoredPosition[m_axis] + cursorPosOnRect - offset;
		
		StartCoroutine(MoveElements(targetPos, focusTime, focusInitVel));
	}
	void InstantFocus(RectTransform rt, float normalizedPosOnTargetRect){
		
		float offset = (normalizedPosOnTargetRect - .5f) * rt.sizeDelta[m_axis];
		float displacement = -rt.anchoredPosition[m_axis] + cursorPosOnRect - offset;
		if(m_elements != null){
			Vector2 newPos = m_elements[0].anchoredPosition;
			newPos[m_axis] += displacement;
			// Vector2 newPos = new Vector2(m_elements[0].anchoredPosition[m_axis] + displacement, m_elements[0].anchoredPosition.y);
			m_elements[0].anchoredPosition = newPos;
			AlignElements();
		}
	}

	float m_totalContentLength;
	protected override void Start(){
		
		base.Start();
		InitializeElements();
		AlignElements();
		image = gameObject.GetComponent<Image>();
		image.color = defCol;
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
		
		focusCurve = new AnimationCurve();
		Keyframe key0 = new Keyframe(0f, 0f, 0f, 0f);
		Keyframe key1 = new Keyframe(1f, 1f, 0f, 0f);
		focusCurve.AddKey(0f, 0f);
		focusCurve.AddKey(1f, 1f);
		focusCurve.MoveKey(0, key0);
		focusCurve.MoveKey(1, key1);
	}

	float GetTotalContentLength(){
		float result = 0f;
		for (int i = 0; i < m_elements.Count; i++)
		{
			result += m_elements[i].sizeDelta[m_axis];
		}
		return result;
	}
	void InitializeElements(){
		if(m_elements == null){
			m_elements = new List<RectTransform>();
		}else{
			m_elements.Clear();
		}
		if(!m_autoGen){
			for (int i = 0; i < transform.childCount; i++)
			{
				Transform child = transform.GetChild(i);
				if(child.GetComponent<RectTransform>())
					m_elements.Add(child.GetComponent<RectTransform>());
			}
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
			float alignPoint = m_elements[0].anchoredPosition[m_axis];
			for (int i = 0; i < m_elements.Count; i++)
			{
				RectTransform rt = m_elements[i];
				Vector2 newPos = rt.anchoredPosition;
				newPos[m_axis] = alignPoint;
				// Vector2 newPos = new Vector2(alignPoint, rt.anchoredPosition.y);
				rt.anchoredPosition = newPos;
				alignPoint = rt.anchoredPosition[m_axis] + rt.sizeDelta[m_axis];
			}
		}
	}

	IEnumerator MoveElements(float targetPos, float travelTime, float initVel){
		while(!m_isDoneMoving){
			m_isMovable = false;
			yield return null;
		}
		float t = 0f;
	
		m_contentStartPos = m_elements[0].anchoredPosition[m_axis];		
		
		float tangentRad = (initVel == 0f || targetPos == 0f)? 0f: initVel/targetPos;
		Keyframe newKey = focusCurve[0];
			newKey.outTangent = tangentRad;
		
		focusCurve.MoveKey(0, newKey);
		
		m_isMovable = true;
		m_isDoneMoving = false;
		while(!m_isDoneMoving){
			if(!m_isMovable){
				m_isDoneMoving = true;
				
				// DebugUtility.PrintRed(gameObject.name + " has broken out from a coroutine MoveElements");
				yield break;
			}
			if(t>1f){
				Vector2 settledPos = m_elements[0].anchoredPosition;
				settledPos[m_axis] = m_contentStartPos + targetPos;
				// m_elements[0].anchoredPosition = new Vector2(m_contentStartPos + targetPos, m_elements[0].anchoredPosition.y);
				// m_elements[0].anchoredPosition = 
				// AlignElements();
				
				SetContentAnchoredPosition(settledPos[m_axis]);
				m_isDoneMoving = true;
				onFocus.Invoke(GetIndex(GetCurrentElementUnderCursor()));
				yield break;
			}
			
			RectTransform rt = m_elements[0];
				
			float target = m_contentStartPos + targetPos;
			float value = focusCurve.Evaluate(t);
			float targetThisFrame = m_contentStartPos *(1f - value) + target *(value);

			// Vector2 newPos = new Vector2(targetThisFrame, rt.anchoredPosition.y);
			Vector2 newPos = rt.anchoredPosition;
			newPos[m_axis] = targetThisFrame;

			t += Time.unscaledDeltaTime / travelTime;
			// rt.anchoredPosition = newPos;
			
			SetContentAnchoredPosition(newPos[m_axis]);
			
			yield return null;
		}
	}


	/*	GUI
	*/
		public bool showGUI = false;
		void OnGUI(){
			if(showGUI){

				GUI.skin = guiSkin;
				
				Rect guiRect = new Rect(10f, 10f, 300f, 700f);
				GUILayout.BeginArea(guiRect, GUI.skin.box);

					DrawElementsInfo();
					DrawSmoothFocus();
					GUILayout.Label("tangent: " + Mathf.Rad2Deg * Mathf.Atan(focusCurve.keys[0].outTangent));
					GUILayout.Label("releaseInitVel: " + m_releaseInitVel);
					GUILayout.Label("offset: " + GetOffset(0f).ToString());

					GUILayout.Label("pointerLocRecPos: " + m_pointerLocRecPos);
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
						SmoothFocus(m_indexedElements[m_ChosenIndex], normalizedPosOnRect, 0f);
				}
			GUILayout.EndHorizontal();
		}


	
	
	
	public void OnInitializePotentialDrag(PointerEventData eventData){
		
		// print(gameObject.name + "'s OnInitializePotentialDrag is called");
		m_isMovable = false;
		
	}

	float m_pointerStartLocalPos;
	float m_contentStartPos;
	
	public void OnBeginDrag(PointerEventData eventData){

		// print(gameObject.name + "'s OnBeginDrag is called");
		Vector2 pointerStartPosV2 = Vector2.zero;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(m_rectTrans, eventData.position, eventData.pressEventCamera, out pointerStartPosV2);
		m_pointerStartLocalPos = pointerStartPosV2[m_axis];
		m_contentStartPos = m_elements[0].anchoredPosition[m_axis];
		m_RTUnderCursorAtTouch = GetCurrentElementUnderCursor();
	}
	Vector2 m_pointerLocRecPos = Vector2.zero;
	float m_pointerDeltaPos;
	float m_contDeltaPos;
	public void OnAxisDrag(PointerEventData eventData){
		
		// print(gameObject.name + "'s OnAxisDrag called");
		image.color = red;
		
		RectTransformUtility.ScreenPointToLocalPointInRectangle(m_rectTrans, eventData.position, eventData.pressEventCamera, out m_pointerLocRecPos);
			
		m_pointerDeltaPos = m_pointerLocRecPos[m_axis] - m_pointerStartLocalPos;
		
		m_contDeltaPos = m_contentStartPos + m_pointerDeltaPos;

		if(!m_loop){

		float offset = GetOffset(m_contDeltaPos - m_elements[0].anchoredPosition[m_axis]);
		m_contDeltaPos += offset;
	
		if(offset != 0f)
			m_contDeltaPos -= RubberDelta(offset, m_rectTrans.sizeDelta[m_axis]);
		}
		
		SetContentAnchoredPosition(m_contDeltaPos);
		
		// print(gameObject.name + "'s OnAsixDrag is ended");
	}


	void LogElementsPosition(){
		string result = string.Empty;
		for (int i = 0; i < m_elements.Count; i++)
		{
			string str = " element " + i.ToString() + " pos.x = " + m_elements[i].anchoredPosition.x + " pos.y = " + m_elements[i].anchoredPosition.y;
			result += str;
		}
		print(result);
	}
	void CheckAndSwapElements(){
		// print("CAS entered");
		// LogElementsPosition();
		if(m_loop){
			int curIndex = GetCurrentElementIndexUnderCursor();
			
			for (int i = 0; i < m_elements.Count +1; i++){
				// LogElementsPosition();
				if(curIndex == m_initiallyFocusedIndex)
					break;

				
				RectTransform minRT = m_elements[0];
				RectTransform maxRT = m_elements[m_elements.Count -1];
				RectTransform RTToMove = null;
				Vector2 newPos = Vector2.zero;

				if(curIndex < m_initiallyFocusedIndex){//scroll toward right
					newPos = maxRT.anchoredPosition;
					newPos[m_axis] = minRT.anchoredPosition[m_axis] - minRT.sizeDelta[m_axis] * .5f - maxRT.sizeDelta[m_axis] * .5f;
					RTToMove = maxRT;
				}else{
				
					newPos = minRT.anchoredPosition;
					newPos[m_axis] = maxRT.anchoredPosition[m_axis] + maxRT.sizeDelta[m_axis] * .5f + minRT.sizeDelta[m_axis] * .5f;
					RTToMove = minRT;
				}

				float delta = m_elements[0].anchoredPosition[m_axis] - m_contentStartPos;
				

				if(RTToMove == minRT){
					m_elements.RemoveAt(0);
					m_elements.Add(minRT);
				}else{
					m_elements.RemoveAt(m_elements.Count -1);
					m_elements.Insert(0, maxRT);
				}

				RTToMove.anchoredPosition = newPos;
				
				delta = m_elements[0].anchoredPosition[m_axis] - delta;
				m_contentStartPos = delta;
				
				curIndex = GetCurrentElementIndexUnderCursor();
				
			}

		}else
			return;
		// print("CAS ended");
	}
	

	float GetOffset(float delta){
		float result = 0f;
		RectTransform minRT = m_elements[0];
		RectTransform maxRT = m_elements[m_elements.Count - 1];
		float curMinContentPoint = minRT.anchoredPosition[m_axis] - minRT.sizeDelta[m_axis] * .5f + delta;
		float curMaxContentPoint = maxRT.anchoredPosition[m_axis] + maxRT.sizeDelta[m_axis] * .5f + delta;
		
		
		float viewRectMin = /*m_rectTrans.anchoredPosition[m_axis]*/ - m_rectTrans.sizeDelta[m_axis] *.5f;
		float viewRectMax = /*m_rectTrans.anchoredPosition[m_axis]*/  m_rectTrans.sizeDelta[m_axis] *.5f;
		float minMargin = cursorPosOnRect + m_rectTrans.sizeDelta[m_axis] * .5f - minRT.sizeDelta[m_axis] * .5f - (normalizedPosOnRect - .5f) * minRT.sizeDelta[m_axis];
		float contentMin = minMargin> 0f? curMinContentPoint - minMargin: curMinContentPoint;
		float maxMargin = cursorPosOnRect + m_rectTrans.sizeDelta[m_axis] * .5f - maxRT.sizeDelta[m_axis] * .5f - (normalizedPosOnRect - .5f) * minRT.sizeDelta[m_axis];
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
		Vector2 curPos = rt.anchoredPosition;
		float totalDelta = newPos - curPos[m_axis];
		
		if(m_loop){
			
			m_correctedDelta = totalDelta % Mathf.Abs(m_totalContentLength);
			
			curPos[m_axis] += m_correctedDelta;
			
		}else
			curPos[m_axis] += totalDelta;

		rt.anchoredPosition = curPos;
		
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
		
		float result = index + (cursorPosOnRect - rt.anchoredPosition[m_axis]- (rt.sizeDelta[m_axis] * .5f))/ rt.sizeDelta[m_axis] ;
		return result;
	}
	public void OnDrag(PointerEventData eventData){
		
		image.color = blue;
		// print(this.gameObject.name.ToString() + "'s OnDrag is called");
		
	}
	public float m_offsetVelMult = .1f;
	public void OnEndDrag(PointerEventData eventData){
		image.color = defCol;
		// print(gameObject.name + "'s OnEndDrag is called");
		float offset = GetOffset(0f);
		float offsetVel = offset * m_offsetVelMult;
		if(offset > 0){
			DebugUtility.PrintBlue("offset > 0, smooth focusing to " + GetMaxFocusTargetNormalizedPos().ToString());
			SmoothFocus(m_elements[m_elements.Count -1], GetMaxFocusTargetNormalizedPos(), offsetVel);
		}else if(offset < 0){
			
			DebugUtility.PrintBlue("offset < 0, smooth focusing to " + GetMinFocusTargetNormalizedPos().ToString());
			SmoothFocus(m_elements[0], GetMinFocusTargetNormalizedPos(), offsetVel);
		}else{
			
			InertialTranslate(eventData);
		}
		
	}
	public float endPadding = 20f;
	float GetMaxFocusTargetNormalizedPos(){
		float result = -1f;
		float viewMax = m_rectTrans.sizeDelta[m_axis] *.5f;
		float contentMaxAtRest = cursorPosOnRect + m_rectTrans.sizeDelta[m_axis] * .5f + m_elements[m_elements.Count -1].sizeDelta[m_axis] * .5f + (normalizedPosOnRect - .5f) * m_elements[m_elements.Count -1].sizeDelta[m_axis];
		float maxMargin = 0f;
		
		if(contentMaxAtRest >= viewMax){
			maxMargin = m_rectTrans.sizeDelta[m_axis] - (cursorPosOnRect + m_rectTrans.sizeDelta[m_axis] * .5f)/* - endPadding*/;
			result = (m_elements[m_elements.Count - 1].sizeDelta[m_axis] - maxMargin) / m_elements[m_elements.Count - 1].sizeDelta[m_axis];
		
		}else{
			
			result = .5f;
		}
		
		return result;
	}
	float GetMinFocusTargetNormalizedPos(){
		float result = -1f;
		float viewMin = 0f;
		float contentMinAtRest = cursorPosOnRect + m_rectTrans.sizeDelta[m_axis] * .5f - m_elements[0].sizeDelta[m_axis] * .5f + (normalizedPosOnRect - .5f) * m_elements[0].sizeDelta[m_axis];
		float minMargin = 0f;
		
		if(contentMinAtRest <= viewMin){
			minMargin = cursorPosOnRect + m_rectTrans.sizeDelta[m_axis] * .5f/* - endPadding*/;
			result = minMargin / m_elements[0].sizeDelta[m_axis];
		}else{

			result = .5f;
		}
		// DebugUtility.PrintBlue("cursorPosOnRect: " + cursorPosOnRect + ", m_rectTrans.sizeDelta[m_axis] * .5f: " + m_rectTrans.sizeDelta[m_axis] * .5f + ", m_elements[0].sizeDelta[m_axis] * .5f: " + m_elements[0].sizeDelta[m_axis] * .5f);
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
		// print("dec called");
		while(!m_isDoneMoving){
			// print("<color=#ff00ffff>waiting for moving is done in dec</color>");
			m_isMovable = false;
			yield return null;
		}

		float vel = initVel;
		m_isMovable = true;
		m_isDoneMoving = false;
		while(!m_isDoneMoving){
			// print("<color=#ff00ffff>main loop of dec entered</color>");
			if(!m_isMovable){
				m_isDoneMoving = true;
				// print("<color=#ff00ffff>broken out from a coroutine Decelerate</color>");
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
					// print("<color=#ff00ffff>broken out from a coroutine Decelerate: offset != 0</color>");
					yield break;
				}
			}
			
			if(m_isContinuous){
				if(Mathf.Abs(vel) < m_stopThresh){
					
					m_isDoneMoving = true;
					// print("<color=#ff00ffff>broken out from a coroutine Decelerate: under stop thresh</color>");
					yield break;
				}
			}else{// not Continuous
				
				if(Mathf.Abs(vel)< m_searchThresh){
					
					Snap(vel);
					
					m_isDoneMoving = true;
					// print("<color=#ff00ffff>broken out from a coroutine Decelerate: under searchThresh</color>");
					yield break;
				}
			}
			
			
			float delta = vel * Time.unscaledDeltaTime;
			float targetPos = m_elements[0].anchoredPosition[m_axis] + delta;
			
			vel *=  Mathf.Pow(m_deceleration,Time.unscaledDeltaTime);
			
			
			SetContentAnchoredPosition(targetPos);
			// print("<color=#ff00ffff> the end of dec reached</color>");
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
			if(eleRT.anchoredPosition[m_axis] - eleRT.sizeDelta[m_axis] * .5f <= cursorPosOnRect){
				if(eleRT.anchoredPosition[m_axis] + eleRT.sizeDelta[m_axis] * .5f >= cursorPosOnRect)
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
			float min = eleRT.anchoredPosition[m_axis] - eleRT.sizeDelta[m_axis] * .5f;
			float max = eleRT.anchoredPosition[m_axis] + eleRT.sizeDelta[m_axis] * .5f;
			if(min <= cursorPosOnRect){
				if(max >= cursorPosOnRect)
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
	
	// public virtual void LayoutComplete(){

	// }
	// public virtual void GraphicUpdateComplete(){

	// }

	// public virtual void Rebuild(CanvasUpdate executing){

	// }

	
}
