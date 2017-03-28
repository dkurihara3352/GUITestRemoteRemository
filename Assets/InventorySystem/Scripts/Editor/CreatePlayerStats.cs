using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using InventorySystem;

public class CreatePlayerStats : Editor {

	[MenuItem("Assets/Create/Custom/PlayerStats")]
	public static void CreatePlayerStatsData(){
		PlayerStats newPlayerStats = ScriptableObject.CreateInstance<PlayerStats>();
		newPlayerStats.Init(null, null, 0);
		string path = "Assets/InventorySystem/ScriptableObjects/newPlayerStats.asset";
		AssetDatabase.CreateAsset(newPlayerStats, path);
		AssetDatabase.SaveAssets();
	}
}
