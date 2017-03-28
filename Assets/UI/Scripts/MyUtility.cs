using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyUtility{
	public class BezierUtility{

		public static Vector3 QuadraticBezier3D(Vector3 p0, Vector3 p1, float t){
			Vector3 result = Vector3.zero;
			float oneMinusT = 1f - t;
			result = (oneMinusT * oneMinusT * p0) + (2f *t * oneMinusT *p1) + (t *t *p1);
			return result;
		}

		public static Vector3 CubicBezier3D(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t){
			Vector3 result = Vector3.zero;
			float oneMinusT = 1f - t;
			result = (oneMinusT *oneMinusT *oneMinusT *p0) + 
					(3f *t *oneMinusT * oneMinusT *p1) +
					(3f *t *t *oneMinusT *p2) +
					(t *t *t *p3);
			return result;
		}

		public static Vector2 QuadraticBezier2D(Vector2 p0, Vector2 p1, float t){
			Vector2 result = Vector2.zero;
			float oneMinusT = 1f - t;
			result = (oneMinusT * oneMinusT * p0) + (2f *t * oneMinusT *p1) + (t *t *p1);
			
			return result;
		}

		public static Vector2 CubicBezier2D(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t){
			Vector2 result = Vector2.zero;
			float oneMinusT = 1f - t;
			result = (oneMinusT *oneMinusT *oneMinusT *p0) + 
					(3f *t *oneMinusT * oneMinusT *p1) +
					(3f *t *t *oneMinusT *p2) +
					(t *t *t *p3);
			return result;
		}

		public static float QuadraticBezier1D(float p0, float p1, float t){
			float result = 0;
			float oneMinusT = 1f - t;
			result = (oneMinusT * oneMinusT * p0) + (2f *t * oneMinusT *p1) + (t *t *p1);
			return result;
		}

		public static float CubicBezier1D(float p0, float p1, float p2, float p3, float t){
			float result = 0;
			float oneMinusT = 1f - t;
			result = (oneMinusT *oneMinusT *oneMinusT *p0) + 
					(3f *t *oneMinusT * oneMinusT *p1) +
					(3f *t *t *oneMinusT *p2) +
					(t *t *t *p3);
			return result;
		}

	}

	public enum BezierMode{
		Quadratic, Cubic
	}

	public class DebugUtility{
		public static void PrintRed(string str){
			Debug.Log("<color=#d80000ff>" + str + "</color>");
		}
		public static void PrintOrange(string str){
			Debug.Log("<color=#db7900ff>" + str + "</color>");
		}
		public static void PrintYellow(string str){
			Debug.Log("<color=#b29800ff>" + str + "</color>");
		}
		public static void PrintGreen(string str){
			Debug.Log("<color=#007814ff>" + str + "</color>");
		}
		public static void PrintCyan(string str){
			Debug.Log("<color=#00a38eff>" + str + "</color>");
		}
		public static void PrintBlue(string str){
			Debug.Log("<color=#007cd8ff>" + str + "</color>");
		}
		public static void PrintPurple(string str){
			Debug.Log("<color=#8e60ffff>" + str + "</color>");
		}
		public static void PrintPink(string str){
			Debug.Log("<color=#ff60f0ff>" + str + "</color>");
		}
	}

}