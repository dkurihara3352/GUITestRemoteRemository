using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyUtility{
	public class MyEditorWindowUtility{

		public static void DrawOnGUISprite(Sprite aSprite, float shrinkRate)
		{
			Rect spriteRect = aSprite.rect;
			float spriteW = spriteRect.width;
			float spriteH = spriteRect.height;
			Rect rect = GUILayoutUtility.GetRect(spriteW * shrinkRate, spriteH *shrinkRate);
			if (Event.current.type == EventType.Repaint)
			{
				var tex = aSprite.texture;
				
				spriteRect.xMin /= tex.width;
				spriteRect.xMax /= tex.width;
				spriteRect.yMin /= tex.height;
				spriteRect.yMax /= tex.height;

				GUI.DrawTextureWithTexCoords(rect, tex, spriteRect);
			}
		}
	}

}
