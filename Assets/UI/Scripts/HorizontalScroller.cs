
using UnityEngine.EventSystems;
public class HorizontalScroller : AxisScroller, IHorizontalDragHandler {

	// Use this for initialization
	protected override void Start () {
		base.SetAxis(0);
		base.Start();
	}

	public void OnHorizontalDrag(PointerEventData eventData){
		OnAxisDrag(eventData);
	}
	
}
