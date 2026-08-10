using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace ActionTree.Data
{
	public static class ActionTreeNodeTypeIds
	{
		public const string Root = "root";
		public const string Sequence = "sequence";
		public const string Selector = "selector";
		public const string Action = "action";
		public const string Condition = "condition";
		public const string Method = "method";
	}

	[System.Serializable]
	[MovedFrom(false, null, null, "BaseNodeData")]
	public abstract class BaseNodeData
	{
		[SerializeField]
		private string guid;

		[SerializeField]
		private Rect rect;

		[SerializeField]
		private string title;

		[SerializeField]
		private string comment = "";

		[SerializeField]
		private bool isSelected;

		[SerializeField]
		private bool isExpanded;

		[System.NonSerialized]
		private BaseNodeData parent;

		[SerializeReference]
		private List<BaseNodeData> children = new();

		public virtual float NodeWidth => 220f;

		public virtual float NodeHeight => 50f;

		public virtual float ChildSpacingX => 40f;

		public virtual float ChildSpacingY => 120f;

		public virtual bool CanDelete => true;
		public virtual bool CanDuplicate => true;
		public virtual bool CanRename => true;
		public virtual bool CanMove => true;
		public virtual string TypeId => GetType().Name;

		public virtual bool CanAddChild(BaseNodeData child) => false;
		public virtual bool CanAcceptParent(BaseNodeData parent) => true;
		public string Guid => guid;

		public Rect Rect
		{
			get => rect;
			protected set => rect = value;
		}
		public void SetHeight(float height)
		{
			Rect = new Rect(
				Rect.position,
				new Vector2(Rect.width, height));
		}
		public string Title
		{
			get => title;
			protected set => title = value;
		}

		public string Comment
		{
			get => comment;
			set => comment = value;
		}

		public bool IsSelected
		{
			get => isSelected;
			set => isSelected = value;
		}

		public bool IsExpanded
		{
			get => isExpanded;
			set => isExpanded = value;
		}

		public BaseNodeData Parent
		{
			get => parent;
			set => parent = value;
		}

		public List<BaseNodeData> Children => children;



		protected BaseNodeData(Vector2 position, string title)
		{
			guid = System.Guid.NewGuid().ToString("N");

			rect = new Rect(position, new Vector2(NodeWidth, NodeHeight));

			this.title = title;
		}

		internal void EnsureSerializedState()
		{
			if (string.IsNullOrEmpty(guid))
				guid = System.Guid.NewGuid().ToString("N");

			if (string.IsNullOrEmpty(title))
				title = GetType().Name.Replace("NodeData", "");

			comment ??= "";

			if (children == null)
				children = new List<BaseNodeData>();

			if (rect.width <= 0f || rect.height <= 0f)
				ResetSize();
		}

		internal void RestoreSerializedState(
			string savedGuid,
			Rect savedRect,
			string savedTitle,
			string savedComment,
			bool savedIsExpanded)
		{
			guid = string.IsNullOrEmpty(savedGuid)
				? System.Guid.NewGuid().ToString("N")
				: savedGuid;

			rect = savedRect.width > 0f && savedRect.height > 0f
				? savedRect
				: new Rect(savedRect.position, new Vector2(NodeWidth, NodeHeight));

			title = string.IsNullOrEmpty(savedTitle)
				? GetType().Name.Replace("NodeData", "")
				: savedTitle;

			comment = savedComment ?? "";
			isExpanded = savedIsExpanded;
			isSelected = false;

			children ??= new List<BaseNodeData>();
			children.Clear();
		}

		internal void SetParentFromAsset(BaseNodeData value)
		{
			parent = value;
		}

		internal void RemoveInvalidChildren()
		{
			if (children == null)
			{
				children = new List<BaseNodeData>();
				return;
			}

			HashSet<BaseNodeData> seen = new();

			for (int i = children.Count - 1; i >= 0; i--)
			{
				BaseNodeData child = children[i];

				if (child == null || child == this || !seen.Add(child))
					children.RemoveAt(i);
			}
		}
		public void ResetSize()
		{
			Rect = new Rect(
				Rect.position,
				new Vector2(NodeWidth, NodeHeight));
		}
		public Vector2 Position
		{
			get => Rect.position;
			set => Rect = new Rect(value, Rect.size);
		}

		public Vector2 Size => Rect.size;

		public float Width => Rect.width;

		public float Height => Rect.height;

		public void SetPosition(Vector2 position)
		{
			Rect = new Rect(position, Rect.size);
		}
		public float SubtreeWidth { get; private set; }

		public void SetSubtreeWidth(float width)
		{
			SubtreeWidth = width;
		}
		public bool IsLeaf => Children.Count == 0;
		public int ChildCount => Children.Count;
		public virtual Vector2 InputPort => TopPort;

		public virtual Vector2 OutputPort => BottomPort;



		public bool AddChild(BaseNodeData child)
		{
			if (child == null || child == this)
				return false;

			if (child.IsAncestorOf(this))
				return false;

			if (!CanAddChild(child))
				return false;

			if (!child.CanAcceptParent(this))
				return false;

			if (Children.Contains(child))
				return false;

			if (child.Parent != null)
			{
				child.Parent.RemoveChild(child);
			}

			child.Parent = this;

			Children.Add(child);

			return true;
		}

		public bool RemoveChild(BaseNodeData child)
		{
			if (child == null)
				return false;

			if (!Children.Remove(child))
				return false;

			child.Parent = null;

			return true;
		}

		public void ClearChildren()
		{
			foreach (BaseNodeData child in Children)
			{
				child.Parent = null;
			}

			Children.Clear();
		}

		public bool Contains(BaseNodeData child)
		{
			return Children.Contains(child);
		}

		public bool IsAncestorOf(BaseNodeData node)
		{
			if (node == null)
				return false;

			BaseNodeData current = node.Parent;

			while (current != null)
			{
				if (current == this)
					return true;

				current = current.Parent;
			}

			return false;
		}

		public Vector2 LeftPort => new(Position.x, Position.y + Height * 0.5f);
		public Vector2 RightPort => new(Position.x + Width, Position.y + Height * 0.5f);
		public Vector2 TopPort => new(Position.x + Width * 0.5f, Position.y);
		public Vector2 BottomPort => new(Position.x + Width * 0.5f, Position.y + Height);

		public virtual string Subtitle => "";

		public Vector2 GetOutputPort(Vector2 targetPosition)
		{
			Vector2 delta = targetPosition - Position;

			if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
			{
				return delta.x >= 0
					? RightPort
					: LeftPort;
			}

			return delta.y >= 0
				? BottomPort
				: TopPort;
		}
		public Vector2 GetInputPort(Vector2 targetPosition)
		{
			Vector2 delta = targetPosition - Position;

			if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
			{
				return delta.x >= 0
					? RightPort
					: LeftPort;
			}

			return delta.y >= 0
				? BottomPort
				: TopPort;
		}

	}
}
