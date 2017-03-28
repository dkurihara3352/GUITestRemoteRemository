
using UnityEngine.EventSystems;
public class VerticalScroller : AxisScroller, IVerticalDragHandler {

	// Use this for initialization
	protected override void Start () {
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
