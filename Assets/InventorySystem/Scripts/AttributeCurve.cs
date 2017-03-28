using UnityEngine;

namespace InventorySystem{

	[System.SerializableAttribute]
	public class AttributeCurve{
		[SerializeField]
		public AnimationCurve curve;

		public string curveName;
		public string inputName;
		public string outputName;
		public float outputMin;
		public float outputMax;
		public float inputMin;
		public float inputMax;
		public float previewOutput;
		public float previewInput;

		public bool showInInspector;
		public void UpdateCurve(){
			Keyframe lastKey = curve.keys[curve.length -1];
			Keyframe firstKey = curve.keys[0];
			lastKey.value = outputMax;
			lastKey.time = inputMax;
			firstKey.value = outputMin;
			firstKey.time = inputMin;
			curve.MoveKey(curve.length-1, lastKey);
			curve.MoveKey(0, firstKey);
		}

		public void InitCurve(){
			// curve = new AnimationCurve();
			curve.AddKey(0f, 0f);
			curve.AddKey(1f, 1f);
		}

		public void ResetCurve(){
			
			for (int i = 0; i < curve.length; i++)
			{
				curve.RemoveKey(i);
			}
			curve.AddKey(0f, 0f);
			curve.AddKey(1f, 1f);
		}
		
	}
}
