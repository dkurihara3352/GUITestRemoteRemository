using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Text.RegularExpressions;

namespace MyUtility{
	public class MyTypeUtility{

		public static string GetPropertyType(SerializedProperty property)
		{
			var type = property.type;
			var match = Regex.Match(type, @"PPtr<\$(.*?)>");
			if (match.Success)
				type = match.Groups[1].Value;
			return type;
		}
 
		public static Type GetPropertyObjectType(SerializedProperty property)
		{
			return typeof(UnityEngine.Object).Assembly.GetType("UnityEngine." + GetPropertyType(property));
		}
	}

}
