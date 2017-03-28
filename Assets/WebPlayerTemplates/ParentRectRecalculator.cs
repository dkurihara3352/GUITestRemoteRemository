
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ParentRectRecalculator : UIBehaviour {

	DrivenRectTransformTracker tracker;

	public int verticalMargin = 0;
	public int horizontalMargin = 0;
	RectTransform rectTrans;
	RectTransform targetRt;
	

	protected override void Awake(){
		base.Awake();
		rectTrans = GetComponent<RectTransform>();
		targetRt = transform.parent.GetComponent<RectTransform>();
		
	}

	// void Update(){
	// 	if(Input.GetKeyDown(KeyCode.Space)){
	// 		OnREct
	// 	}
	// }
	protected override void OnRectTransformDimensionsChange(){
		
		print("event triggered");
		if(targetRt!= null){
			tracker.Clear();
			tracker.Add(this, targetRt, DrivenTransformProperties.Anchors);
		}
		
		Rect parentRect = targetRt.transform.parent.GetComponent<RectTransform>().rect;
		float parentRectWidth = parentRect.width;
		float parentRectHeight = parentRect.height;

		float newAnchorMinX;
		float newAnchorMaxX;
		if((rectTrans.rect.x - targetRt.rect.x) < horizontalMargin){
			newAnchorMinX = (rectTrans.rect.x - horizontalMargin - parentRect.x)/parentRectWidth;
		}else{
			newAnchorMinX = rectTrans.anchorMin.x;
		}
		if((targetRt.rect.x + targetRt.rect.width) < (rectTrans.rect.x + rectTrans.rect.width + horizontalMargin)){
			newAnchorMaxX = (rectTrans.rect.x + rectTrans.rect.width + horizontalMargin - parentRect.x)/parentRectWidth;
		}else{
			newAnchorMaxX = rectTrans.anchorMax.x;
		}

		float newAnchorMinY;
		float newAnchorMaxY;
		if((rectTrans.rect.y - targetRt.rect.y) < verticalMargin){
			newAnchorMinY = (rectTrans.rect.y - verticalMargin - parentRect.y)/parentRectHeight;
		}else{
			newAnchorMinY = rectTrans.anchorMin.y;
		}
		if((targetRt.rect.y + targetRt.rect.height) < (rectTrans.rect.y + rectTrans.rect.height + verticalMargin)){
			newAnchorMaxY = (rectTrans.rect.y + rectTrans.rect.height + verticalMargin - parentRect.y)/parentRectHeight;
		}else{
			newAnchorMaxY = rectTrans.anchorMax.x;
		}
		Vector2 newAnchorMax = new Vector2(newAnchorMaxX, newAnchorMaxY);
		Vector2 newAnchorMin = new Vector2(newAnchorMinX, newAnchorMinY);

		targetRt.anchorMax = newAnchorMax;
		targetRt.anchorMin = newAnchorMin;
	}
}
