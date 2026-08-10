using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ActionTree.Data;
using ActionTree.Runtime;
namespace ActionTree.Editor
{
	public class ActionTreeWindow : EditorWindow
	{
		private ActionTreeAsset currentAsset;
		private GameObject currentOwner;
		public GameObject Owner => currentOwner;

		private readonly ActionTreeEditorContext context = new();
		public ActionTreeEditorContext Context => context;

		public static ActionTreeWindow Instance { get; private set; }

		private ActionTreeCanvas Canvas;

		private GraphInspector Inspector;

		private void OnEnable()
		{
			Instance = this;
		}

		private void OnDisable()
		{
			if (Instance == this)
				Instance = null;
		}
		[MenuItem("Tools/Action Tree")]


		public static void Open(ActionTreeAsset asset, GameObject owner)
		{
			ActionTreeWindow window = GetWindow<ActionTreeWindow>();

			window.titleContent = new GUIContent("Action Tree");

			window.Load(asset, owner);
		}
		public void Load(ActionTreeAsset asset, GameObject owner)
		{
			currentAsset = asset;
			currentOwner = owner;

			context.Runner = owner.GetComponent<BehaviorTreeRunner>();
			context.Refresh();

			if (Canvas != null)
				Canvas.Load(asset);
		}


		public void RefreshContext()
		{
			context.Refresh();
		}


		private void CreateGUI()
		{
			rootVisualElement.Clear();

			Inspector = new GraphInspector(Context);
			Canvas = new ActionTreeCanvas(Inspector);

			TwoPaneSplitView splitView = new TwoPaneSplitView(
				0,      // 左邊是第一個 Pane
				1000,   // 左邊初始寬度
				TwoPaneSplitViewOrientation.Horizontal);

			splitView.Add(Canvas);
			splitView.Add(Inspector);

			rootVisualElement.Add(splitView);

			if (currentAsset != null)
				Canvas.Load(currentAsset);
		}
	}
}