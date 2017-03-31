﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(CanvasGroup), typeof(Image))]
public class RaycastBlocker : MonoBehaviour {

	public Transform m_canvasTrans;
	RectTransform m_rectTrans;
	CanvasGroup m_canvasGroup;
	float m_fadeTime;
	bool m_initialized = false;
	public void Initialize(float fadeTime){
		if(!m_initialized){
			m_initialized = true;
			transform.SetParent(m_canvasTrans);
			
			m_rectTrans = GetComponent<RectTransform>();
			m_rectTrans.anchorMax = Vector2.one;
			m_rectTrans.anchorMin = Vector2.zero;
			m_rectTrans.pivot = new Vector2(.5f, .5f);
			m_rectTrans.sizeDelta = Vector2.one;

			m_canvasGroup = GetComponent<CanvasGroup>();
			m_canvasGroup.alpha = 0f;
			m_canvasGroup.interactable = false;
			m_canvasGroup.blocksRaycasts = false;
			// m_canvasGroup.ignoreParentGroups = true;
			m_fadeTime = fadeTime;


			m_isActivated = false;
			m_isBeingDeactivated = false;
			m_isDeactivatable = true;
		}
	}
	
	void Start () {
		
	}
	
	void Update () {
		
	}

	public void Deactivate(){
		// m_isActivated = false;
		StartCoroutine(InternalDeactivate());
	}
	public bool IsBeingDeactivated(){
		return m_isBeingDeactivated;
	}
	bool m_isActivated = false;
	public void Activate(Transform popup){
		/*
			if already activated, just set hierarchy
		*/
			m_canvasGroup.blocksRaycasts = true;
			StartCoroutine(InternalActivate(popup));
		// if(!m_isActivated){

			
		// }
		
		SetHierarchy(popup);

	}
	void SetHierarchy(Transform popup){
		transform.SetAsLastSibling();
		if(popup.parent != m_canvasTrans)
			popup.SetParent(m_canvasTrans, true);
		popup.SetAsLastSibling();
	}
	IEnumerator InternalActivate(Transform popup){
		float timer = 0f;
		while(true){
			if(timer > m_fadeTime){
				if(!m_isActivated)
					CompleteActivation(popup);
				yield break;
			}
			if(!m_isActivated)
				m_canvasGroup.alpha = Mathf.Lerp(0f, m_activatedAlpha, timer/m_fadeTime);

			timer += Time.unscaledDeltaTime;
			yield return null;
		}
	}

	void CompleteActivation(Transform popup){
		m_isActivated = true;
		m_canvasGroup.alpha = m_activatedAlpha;
	}

	public void StopDeactivation(){
		m_isDeactivatable = false;
	}
	bool m_isBeingDeactivated = false;
	bool m_isDeactivatable = true;
	
	float m_activatedAlpha = .5f;
	IEnumerator InternalDeactivate(){
		m_isBeingDeactivated = true;
		m_isDeactivatable = true;
		float timer = 0f;
		while(m_isDeactivatable){

			if(!m_isDeactivatable){
				m_isBeingDeactivated = false;
				m_isDeactivatable = true;
				m_canvasGroup.alpha = m_activatedAlpha;
				yield break;
			}

			if(timer > m_fadeTime){
				m_isBeingDeactivated = false;
				m_isDeactivatable = true;
				CompleteDeactivation();
				yield break;
			}

			m_canvasGroup.alpha = Mathf.Lerp(m_activatedAlpha, 0f, timer/m_fadeTime);

			timer += Time.unscaledDeltaTime;
			yield return null;
		}
	}
	void CompleteDeactivation(){
		m_canvasGroup.blocksRaycasts = false;
		m_canvasGroup.alpha = 0f;
		m_isActivated = false;
	}
}
