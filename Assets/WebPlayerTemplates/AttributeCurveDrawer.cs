using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace InventorySystem{
	
	[CustomPropertyDrawer(typeof(AttributeCurve))]
	public class AttributeCurveDrawer : PropertyDrawer {

		float curveBoxDimension = 50f;
		float space = 5f;
		float bigSpace = 10f;
		float defHeight = EditorGUIUtility.singleLineHeight;
		Color greyCol = new Color(.7f, .7f, .7f);

		public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label){
			// if(prop.objectReferenceValue == null) return;
			SerializedObject attCurveSO = new SerializedObject((AttributeCurve)prop.objectReferenceValue);
			SerializedProperty curveProp = attCurveSO.FindProperty("curve");
			SerializedProperty nameProp = attCurveSO.FindProperty("curveName");
			SerializedProperty outMinProp = attCurveSO.FindProperty("outputMin");
			SerializedProperty outMaxProp = attCurveSO.FindProperty("outputMax");
			SerializedProperty inMinProp = attCurveSO.FindProperty("inputMin");
			SerializedProperty inMaxProp = attCurveSO.FindProperty("inputMax");
			SerializedProperty previewOutProp = attCurveSO.FindProperty("previewOutput");
			SerializedProperty previewInProp = attCurveSO.FindProperty("previewInput");
			SerializedProperty showProp = attCurveSO.FindProperty("showInInspector");
			SerializedProperty inputNameProp = attCurveSO.FindProperty("inputName");
			SerializedProperty outputNameProp = attCurveSO.FindProperty("outputName");
			
			
			Rect titleRect = new Rect(pos);
			titleRect.height = defHeight;
			titleRect.y = pos.y + bigSpace;
			
			Rect contentRect = new Rect(pos);
			contentRect.height = pos.height - bigSpace - defHeight -space;
			contentRect.y = pos.y + bigSpace + defHeight + space;

			Rect curveRow = new Rect(contentRect);
			curveRow.height = curveBoxDimension + space + defHeight;

				float floatBoxWidth = defHeight * 2;

				Rect curveRect = new Rect(curveRow);
				curveRect.height = curveBoxDimension;
				curveRect.width = curveRow.width - space - defHeight;
				curveRect.x = curveRow.x + floatBoxWidth + space;

				Rect outMaxRect = new Rect(curveRow);
				outMaxRect.height = defHeight;
				outMaxRect.width = floatBoxWidth;

				Rect outMinRect = new Rect(outMaxRect);
				outMinRect.y = curveRow.y + curveRow.height - defHeight - space - defHeight;

				Rect inMinRect = new Rect(outMinRect);
				inMinRect.x = curveRect.x;
				inMinRect.y = curveRect.y + curveRect.height + space;

				Rect inMaxRect = new Rect(inMinRect);
				inMaxRect.x = curveRow.x + curveRow.width - floatBoxWidth;
			
			// Rect nameRow = new Rect(contentRect);
			// nameRow.height = defHeight;
			// nameRow.y = contentRect.y + curveRow.height/* + space + defHeight*/ + space;
			
			Rect sliderRow = new Rect(contentRect);
			sliderRow.height = defHeight;
			sliderRow.y = contentRect.y + curveRow.height + space/* + defHeight + space*/ /*+ nameRow.height + space*/;
			
			float labelRatio = .3f;
			
			Rect sliderLabelRect = new Rect(sliderRow);
			sliderLabelRect.width = (sliderRow.width - space) * labelRatio;
			
			Rect sliderFieldRect = new Rect(sliderRow);
			sliderFieldRect.width = sliderRow.width - sliderLabelRect.width - space;
			sliderFieldRect.x = sliderRow.x + sliderLabelRect.width + space;

			Rect previewRow = new Rect(contentRect);
			previewRow.height = defHeight;
			previewRow.y = contentRect.y + curveRow.height + space + defHeight + space /*+ nameRow.height + space*//* + sliderRow.height + space*/;

			Rect previewLabelRect = new Rect(previewRow);
			previewLabelRect.width = (previewRow.width - space) * labelRatio;
			Rect previewFieldRect = new Rect(previewRow);
			previewFieldRect.width = previewRow.width - previewLabelRect.width - space;
			previewFieldRect.x = previewRow.x + previewLabelRect.width + space;



			SerializedObject curveSO = curveProp.serializedObject;
			attCurveSO.Update();
			curveSO.Update();

			int indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			showProp.boolValue = EditorGUI.Foldout(titleRect, showProp.boolValue, nameProp.stringValue);
			if(showProp.boolValue){
				EditorGUI.DrawRect(contentRect, greyCol);
				
				curveProp.animationCurveValue = EditorGUI.CurveField(curveRect, curveProp.animationCurveValue);
				
				outMaxProp.floatValue = EditorGUI.FloatField(outMaxRect, outMaxProp.floatValue);
				outMinProp.floatValue = EditorGUI.FloatField(outMinRect, outMinProp.floatValue);
				inMaxProp.floatValue = EditorGUI.FloatField(inMaxRect, inMaxProp.floatValue);
				inMinProp.floatValue = EditorGUI.FloatField(inMinRect, inMinProp.floatValue);


				AttributeCurve attCurveScr = (AttributeCurve)attCurveSO.targetObject;

				attCurveScr.UpdateCurve();
				
				curveSO.ApplyModifiedProperties();
				
			
				
				EditorGUI.LabelField(sliderLabelRect, inputNameProp.stringValue.ToString());
				previewInProp.floatValue = EditorGUI.Slider(sliderFieldRect, previewInProp.floatValue, inMinProp.floatValue, inMaxProp.floatValue);
				
				previewOutProp.floatValue = curveProp.animationCurveValue.Evaluate(previewInProp.floatValue);
				
				EditorGUI.LabelField(previewLabelRect, outputNameProp.stringValue);
				EditorGUI.LabelField(previewFieldRect, previewOutProp.floatValue.ToString());
			}

			EditorGUI.indentLevel = indent;
			attCurveSO.ApplyModifiedProperties();
			
		}

		public override float GetPropertyHeight(SerializedProperty prop, GUIContent label){
			// if(prop.objectReferenceValue == null)
				// return 0f;
			SerializedObject attCurveSO = new SerializedObject((AttributeCurve)prop.objectReferenceValue);
			SerializedProperty showProp = attCurveSO.FindProperty("showInInspector");
			
			float result;
			if(showProp.boolValue)
				result = bigSpace + defHeight+ space + curveBoxDimension + space + defHeight /*+ space + defHeight*/ + space + defHeight +space + defHeight; 
			else
				result = bigSpace + defHeight;

			return result;
			
		}
	}

}
