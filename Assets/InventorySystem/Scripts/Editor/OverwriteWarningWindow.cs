
using UnityEngine;
using UnityEditor;

namespace InventorySystem{
	public abstract class OverwriteWarningWindow : PopupWindowContent {

		protected InventoryItemEditorWindow invSysWindow;
		GUIStyle newStyle;
		

		public override void OnOpen(){
			// invSysWindow = (InventoryItemEditorWindow)editorWindow;
			invSysWindow = (InventoryItemEditorWindow)EditorWindow.GetWindow(typeof(InventoryItemEditorWindow));
			newStyle = new GUIStyle();
			newStyle.richText = true;
			newStyle.wordWrap = true;
			newStyle.alignment = TextAnchor.MiddleCenter;
		}

		public override void OnGUI(Rect pos){

			float margin = 10f;
			Rect contentRect = new Rect(pos);
			contentRect.x = pos.x + margin;
			contentRect.y = pos.y + margin;
			contentRect.width = pos.width - margin *2;
			contentRect.height = pos.height - margin *2;
			Rect textFieldRect = new Rect(contentRect);
			textFieldRect.height = (contentRect.height - margin) *.7f;
			Rect buttonRect = new Rect(contentRect);
			buttonRect.height = contentRect.height - margin - textFieldRect.height;
			buttonRect.y = contentRect.y + textFieldRect.height + margin;
			string text = Message();
			EditorGUI.LabelField(textFieldRect, text, newStyle);
			if(GUI.Button(buttonRect, "Yes")){
				// invSysWindow.CreateInventoryItemList();
				CallEditorWindowMethod();
			}
		}

		protected virtual string Message(){
			return null /*"There's already an asset of type InventoryItemList in the assets. \n Do you wish to overwrite?"*/;
		}
		
		public override Vector2 GetWindowSize(){
			Vector2 windowSize = new Vector2(300f, 100f);
			return windowSize;
		}

		protected virtual void CallEditorWindowMethod(){
			// invSysWindow.CreateInventoryItemList();
		}
	}
}
