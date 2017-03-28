using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class HorizontalScroller : AxisScroller, IHorizontalDragHandler {

	// Use this for initialization
	protected override void Start () {
		m_axis = 0;
		base.Start();
	}

	public void OnHorizontalDrag(PointerEventData eventData){
		OnAxisDrag(eventData);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
