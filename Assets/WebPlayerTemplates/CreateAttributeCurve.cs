
using UnityEngine;
using UnityEditor;
using InventorySystem;

public class CreateAttributeCurve{

	[MenuItem("Assets/Create/AttributeCurve")]
	public static void Create(){
		AttributeCurve newAsset = (AttributeCurve)ScriptableObject.CreateInstance<AttributeCurve>();
		newAsset.curve = new AnimationCurve();
		newAsset.InitCurve();
		newAsset.curveName = "my Curve Name";
		newAsset.inputName = "my Input";
		newAsset.outputName = "my Output";
		AssetDatabase.CreateAsset(newAsset, "Assets/AttributeCurve.asset");
		AssetDatabase.SaveAssets();
	}
	public static void Create(string curveName, string inputName, string outputName){
		AttributeCurve newAsset = (AttributeCurve)ScriptableObject.CreateInstance<AttributeCurve>();
		newAsset.curve = new AnimationCurve();
		newAsset.InitCurve();
		newAsset.curveName = curveName;
		newAsset.inputName = inputName;
		newAsset.outputName = outputName;
		AssetDatabase.CreateAsset(newAsset, "Assets/AttributeCurve.asset");
		AssetDatabase.SaveAssets();
	}
}
