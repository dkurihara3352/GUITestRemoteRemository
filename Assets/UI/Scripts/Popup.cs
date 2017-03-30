﻿using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image), typeof(CanvasGroup))]
public class Popup : UIBehaviour, IPopupFocusHandler, IPopupDefocusHandler, IPopupHideHandler {

	public CustomInputModuleTemplate customInputModule;
	public Canvas canvas;
	Image image;
	Color defocusedCol = new Color(.7f, .7f, .7f);
	Color focusCol = Color.white;
	
	RaycastBlocker m_raycastBlocker;
	public GameObject m_rayBlockerPrefab;
	CanvasGroup m_canvasGroup{
		get{
			if(this.gameObject.GetComponent<CanvasGroup>() == null){
				return this.gameObject.AddComponent<CanvasGroup>();
			}else{
				return this.gameObject.GetComponent<CanvasGroup>();
			}
		}
	}
	RectTransform m_rectTrans{
		get{
			return this.gameObject.GetComponent<RectTransform>();
		}
	}
	protected override void OnEnable(){
		base.OnEnable();
	}

	protected override void Start(){
		base.Start();
		image = GetComponent<Image>();
		image.color = focusCol;
		DeactivateCanvasGroup();
	}

	void DeactivateCanvasGroup(){
		if(m_canvasGroup != null){
			m_canvasGroup.alpha = 0f;
			m_canvasGroup.blocksRaycasts = false;
			m_canvasGroup.interactable = false;
		}
	}
	void ActivateCanvasGroup(){
		if(m_canvasGroup != null){
			m_canvasGroup.alpha = 1f;
			m_canvasGroup.blocksRaycasts = true;
			m_canvasGroup.interactable = true;
		}
	}

	

	public void OnPopupFocus(PointerEventData eventData){
		print(gameObject.name + "'s OnPopupFocus is called");
		image.color = focusCol;
		ActivateCanvasGroup();
		/*
			if the raycastBlocker is being deactivated, stop the coroutine first
			Activate and SetInHierarchy raycastBlocker
			try to find one in the scene
			create and store reference if not found
		*/
		if(m_raycastBlocker == null){
			FindOrCreateRaycastBlocker();
		}
		if(m_raycastBlocker.IsBeingDeactivated()){
			m_raycastBlocker.StopDeactivation();
		}
		transform.SetParent(canvas.transform, true);
		
		m_raycastBlocker.Activate(this.transform);
		
	}

	public void OnPopupDefocus(PointerEventData eventData){
		print(gameObject.name + "'s OnPopupDefocus is called");
		image.color = defocusedCol;
		DeactivateCanvasGroup();
		m_canvasGroup.alpha = 1f;
		
	}

	public void OnPopupHide(PointerEventData eventData){
		print(gameObject.name + "'s OnpopupHide is called");
		StartCoroutine(Fade(false));
		
		m_raycastBlocker.Deactivate();
	}

	public void OpenPopup(){/*explicitly called from unity event*/
		print(gameObject.name + "'s OpenPopup is called");
		customInputModule.AddPopup(this.gameObject);

		StartCoroutine(Fade(true));

	}
	
	bool isDoneFading = true;
	public float fadeTime = .2f;
	bool isFadable = false;
	IEnumerator Fade(bool fadeIn){
		float t = 0f;
		isDoneFading = false;
		isFadable = true;
		
		float curAlpha = fadeIn? 0f: 1f;
		float targetAlpha = fadeIn? 1f: 0f;
			
		while(!isDoneFading){
			if(!isFadable){
				isDoneFading = true;
				yield break;
			}

			if(t >= fadeTime){
				isDoneFading = true;
				m_canvasGroup.alpha = targetAlpha;
				if(fadeIn)
					ActivateCanvasGroup();
				else
					DeactivateCanvasGroup();
				yield break;
			}

			m_canvasGroup.alpha = Mathf.Lerp(curAlpha, targetAlpha, t == 0f? 0f: t/fadeTime);

			t+= Time.unscaledDeltaTime;
			
			yield return null;
		}
	}

	void FindOrCreateRaycastBlocker(){
		RaycastBlocker raycastBlocker = FindObjectOfType<RaycastBlocker>();
		if(raycastBlocker == null){
			raycastBlocker = Instantiate(m_rayBlockerPrefab, Vector3.zero, Quaternion.identity).GetComponent<RaycastBlocker>();
		}
		raycastBlocker.Initialize(fadeTime);
		m_raycastBlocker = raycastBlocker;
	}
}
