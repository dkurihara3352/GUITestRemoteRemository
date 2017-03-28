using System;

namespace UnityEngine.EventSystems{

	public interface IHorizontalDragHandler: IDragHandler{
		void OnHorizontalDrag(PointerEventData eventData);
	}

	public interface IVerticalDragHandler: IDragHandler{
		void OnVerticalDrag(PointerEventData eventData);
	}

	public interface IPopupHideHandler: IEventSystemHandler{
		void OnPopupHide(PointerEventData eventData);
	}

	public interface IPopupFocusHandler: IEventSystemHandler{
		void OnPopupFocus(PointerEventData eventData);
	}
	public interface IPopupDefocusHandler: IEventSystemHandler{
		void OnPopupDefocus(PointerEventData eventData);
	}
}
