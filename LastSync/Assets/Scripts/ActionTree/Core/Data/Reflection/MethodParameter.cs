using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace ActionTree
{

    [Serializable]
	[MovedFrom(false, null, null, "MethodParameter")]
    public class MethodParameter
    {
		public MethodParameter()
		{
		}

		public MethodParameter(MethodParameter source)
		{
			if (source == null)
				return;

			parameterName = source.parameterName;
			parameterType = source.parameterType;
			intValue = source.intValue;
			floatValue = source.floatValue;
			boolValue = source.boolValue;
			stringValue = source.stringValue;
			vector2Value = source.vector2Value;
			vector3Value = source.vector3Value;
			vector4Value = source.vector4Value;
			colorValue = source.colorValue;
			objectValue = source.objectValue;
		}

    	public string parameterName;

    	// AssemblyQualifiedName
    	public string parameterType;

    	public int intValue;
    	public float floatValue;
    	public bool boolValue;
    	public string stringValue;

    	public Vector2 vector2Value;
    	public Vector3 vector3Value;
    	public Vector4 vector4Value;

    	public Color colorValue;

    	public UnityEngine.Object objectValue;
    }
}
