using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyUtility{
	public class MyRectUtility{

		public static Rect RectWithMargin(Rect origRect, float margin){
			Rect newRect = new Rect(origRect);
			
			newRect.width = origRect.width - margin *2;
			newRect.height = origRect.height - margin *2;
			newRect.x = origRect.x + margin;
			newRect.y = origRect.y + margin;
			return newRect;
		}

		public static void SplitRect(Rect origRect, out Rect firstRect, out Rect secondRect, float ratio, float space, bool isHorizontal){
			Rect resultA = new Rect(origRect);
			if(isHorizontal){
				resultA.width = origRect.width * ratio;
			}else{
				resultA.height = origRect.height * ratio;
			}

			Rect resultB = new Rect(origRect);
			if(isHorizontal){
				resultB.width = origRect.width - space - resultA.width;
				resultB.x = origRect.x + resultA.width + space;
			}else{
				resultB.height = origRect.height - space - resultA.height;
				resultB.y = origRect.y + resultA.height + space;
			}

			firstRect = resultA;
			secondRect = resultB;
		}
		
	}

}
