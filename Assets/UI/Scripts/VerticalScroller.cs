using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class VerticalScroller : AxisScroller, IVerticalDragHandler {

	// Use this for initialization
	protected override void Start () {
		print(gameObject.name + "'s Vertical Scroller Start called");
		m_axis = 1;
		base.Start();
	}

	public void OnVerticalDrag(PointerEventData eventData){
		base.OnAxisDrag(eventData);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
