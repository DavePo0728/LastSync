using ActionTree.Editor;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.MessageBox;
public enum DragMode
{
	None,
	Move,
	MakeChild
}
public class GraphInteractionManager
{
	//====================
	// Drag Node
	//====================

	public bool IsDraggingNode { get; private set; }

	public BaseNodeView DragNode { get; private set; }

	public Vector2 DragStartMouse { get; private set; }

	public Vector2 DragStartNode { get; private set; }


	//====================
	// Hover Parent
	//====================
	public Vector2 CurrentMousePosition { get; private set; }

	public void UpdateMouse(Vector2 mouse)
	{
		CurrentMousePosition = mouse;
	}
	public BaseNodeView HoverParent { get; private set; }
	public void SetHoverParent(BaseNodeView node)
	{
		HoverParent = node;
	}

	//====================
	// Box Selection
	//====================

	public bool IsBoxSelecting { get; private set; }

	public Vector2 BoxStart { get; private set; }

	public Vector2 BoxEnd { get; private set; }

	//====================
	// Pan
	//====================

	public bool IsPanning { get; private set; }
	private Vector2 lastMousePosition;
	public void BeginDrag(BaseNodeView node, Vector2 mousePosition)
	{
		DragNode = node;

		DragStartMouse = mousePosition;

		DragStartNode = node.Node.Position;

		IsDraggingNode = true;
	}

	public void EndDrag()
	{
		DragNode = null;

		IsDraggingNode = false;
	}
	

	//public void BeginConnection(BaseNodeView node)
	//{
	//	IsCreatingConnection = true;

	//	ConnectionStart = node;
	//}

	//public void UpdateConnection(Vector2 mouse)
	//{
	//	ConnectionMouse = mouse;
	//}

	//public void EndConnection()
	//{
	//	IsCreatingConnection = false;

	//	ConnectionStart = null;
	//}

	public void SetHoverNode(BaseNodeView node)
	{
		HoverParent = node;
	}
	public void BeginPan(Vector2 mousePosition)
	{
		IsPanning = true;
		lastMousePosition = mousePosition;
	}

	public Vector2 UpdatePan(Vector2 mousePosition)
	{
		Vector2 delta = mousePosition - lastMousePosition;

		lastMousePosition = mousePosition;

		return delta;
	}

	public void EndPan()
	{
		IsPanning = false;
	}
	public void BeginBoxSelection(Vector2 mousePosition)
	{
		IsBoxSelecting = true;

		BoxStart = mousePosition;

		BoxEnd = mousePosition;
	}

	public void UpdateBoxSelection(Vector2 mousePosition)
	{
		BoxEnd = mousePosition;
	}

	public void EndBoxSelection()
	{
		IsBoxSelecting = false;
	}
}
