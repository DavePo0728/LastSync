using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ActionTree.Runtime;

namespace ActionTree.Editor
{
	public abstract class RuntimeNodeInspector : VisualElement
	{
		protected Button typeButton;

		protected VisualElement fieldContainer;

		protected RuntimeNode currentRuntime;

		public event Action Changed;

		protected RuntimeNodeInspector(string title)
		{
		}

		protected void NotifyChanged()
		{
			Changed?.Invoke();
		}


		protected abstract void SetRuntime(RuntimeNode runtime);
	}
}