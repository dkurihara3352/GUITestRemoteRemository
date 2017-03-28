
namespace UnityEngine.EventSystems{

	public static class CustomEvents{

		private static void Execute(IHorizontalDragHandler handler, BaseEventData eventData){
			handler.OnHorizontalDrag(ExecuteEvents.ValidateEventData<PointerEventData>(eventData));
		}

		public static ExecuteEvents.EventFunction<IHorizontalDragHandler> horizontalDragHandler{
			get{return Execute;}
		}
		private static void Execute(IVerticalDragHandler handler, BaseEventData eventData){
			handler.OnVerticalDrag(ExecuteEvents.ValidateEventData<PointerEventData>(eventData));
		}

		public static ExecuteEvents.EventFunction<IVerticalDragHandler> verticalDragHandler{
			get{return Execute;}
		}
		private static void Execute(IPopupHideHandler handler, BaseEventData eventData){
			handler.OnPopupHide(ExecuteEvents.ValidateEventData<PointerEventData>(eventData));
		}

		public static ExecuteEvents.EventFunction<IPopupHideHandler> popupHideHandler{
			get{return Execute;}
		}
		private static void Execute(IPopupFocusHandler handler, BaseEventData eventData){
			handler.OnPopupFocus(ExecuteEvents.ValidateEventData<PointerEventData>(eventData));
		}

		public static ExecuteEvents.EventFunction<IPopupFocusHandler> popupFocusHandler{
			get{return Execute;}
		}
		private static void Execute(IPopupDefocusHandler handler, BaseEventData eventData){
			handler.OnPopupDefocus(ExecuteEvents.ValidateEventData<PointerEventData>(eventData));
		}

		public static ExecuteEvents.EventFunction<IPopupDefocusHandler> popupDefocusHandler{
			get{return Execute;}
		}
	}
}