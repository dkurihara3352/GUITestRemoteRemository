using System;

namespace UnityEngine.EventSystems
{
	public interface ICustomEventHandler : IEventSystemHandler
	{
		void OnCustomEvent(PointerEventData eventData);
	}

}