
using UnityEngine.EventSystems;
public class VerticalScroller : AxisScroller, IVerticalDragHandler {

	// Use this for initialization
	protected override void Start () {
		
		base.SetAxis(1);
		base.Start();
	}

	public void OnVerticalDrag(PointerEventData eventData){
		base.OnAxisDrag(eventData);
	}
	
}
