
namespace UnityEngine.EventSystems{
	public static class MyCustomEventsExample
	{
		// call that does the mapping
		private static void Execute(ICustomEventHandler handler, BaseEventData eventData)
		{
			// The ValidateEventData makes sure the passed event data is of the correct type
			handler.OnCustomEvent (ExecuteEvents.ValidateEventData<PointerEventData> (eventData));
		}

		// helper to return the functor that should be invoked
		public static ExecuteEvents.EventFunction<ICustomEventHandler> customEventHandler
		{
			get { return Execute; }
		}
	}

	// public static class MyCustomEvents{

	// 	private static void Execute(IHorizontalScrollHandler handler)
	// }

}