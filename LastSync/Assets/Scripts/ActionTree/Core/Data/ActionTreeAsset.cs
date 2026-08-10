using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace ActionTree.Data
{
	[CreateAssetMenu(
		fileName = "New ActionTree",
		menuName = "Action Tree/Action Tree")]
	[MovedFrom(false, null, null, "ActionTreeAsset")]
	public class ActionTreeAsset : ScriptableObject, ISerializationCallbackReceiver
	{
		private const int CurrentStableFormatVersion = 1;

		[SerializeReference]
		public RootNodeData Root;

		[SerializeReference]
		public List<BaseNodeData> Nodes = new();

		[SerializeField]
		private int stableFormatVersion = CurrentStableFormatVersion;

		[SerializeField]
		private string stableRootGuid;

		[SerializeField]
		private List<ActionTreeNodeRecord> stableNodes = new();

		public void Create()
		{
			EnsureRoot();
			Repair();
		}

		public void OnBeforeSerialize()
		{
			Repair();
			SyncStableRecords();
		}

		public void OnAfterDeserialize()
		{
			if (ShouldRebuildFromStableRecords())
				RebuildFromStableRecords();

			Repair();
		}

		public void Repair()
		{
			if (Nodes == null)
				Nodes = new List<BaseNodeData>();

			RemoveNullAndDuplicateNodes();
			EnsureRoot();
			CollectChildrenIntoNodes();
			RepairNodeState();
			RepairParentLinks();
		}

		public void SaveStableBackup()
		{
			Repair();
			SyncStableRecords();
		}

		private bool ShouldRebuildFromStableRecords()
		{
			if (stableNodes == null || stableNodes.Count == 0)
				return false;

			if (stableFormatVersion > CurrentStableFormatVersion)
			{
				Debug.LogWarning(
					$"{name} was saved with newer ActionTree stable format version {stableFormatVersion}.");
				return false;
			}

			if (Root == null || Nodes == null || Nodes.Count == 0)
				return true;

			int liveCount = 0;

			foreach (BaseNodeData node in Nodes)
			{
				if (node != null)
					liveCount++;
			}

			return liveCount < stableNodes.Count;
		}

		private void SyncStableRecords()
		{
			stableFormatVersion = CurrentStableFormatVersion;
			stableRootGuid = Root != null ? Root.Guid : string.Empty;

			stableNodes ??= new List<ActionTreeNodeRecord>();
			stableNodes.Clear();

			if (Nodes == null)
				return;

			foreach (BaseNodeData node in Nodes)
			{
				if (node == null)
					continue;

				stableNodes.Add(ActionTreeNodeRecord.FromNode(node));
			}
		}

		private void RebuildFromStableRecords()
		{
			Dictionary<string, BaseNodeData> nodesByGuid = new();
			List<BaseNodeData> rebuiltNodes = new();

			foreach (ActionTreeNodeRecord record in stableNodes)
			{
				BaseNodeData node = CreateNode(record);

				if (node == null)
					continue;

				node.RestoreSerializedState(
					record.Guid,
					record.Rect,
					record.Title,
					record.Comment,
					record.IsExpanded);

				RestoreNodePayload(node, record);

				nodesByGuid[node.Guid] = node;
				rebuiltNodes.Add(node);
			}

			foreach (ActionTreeNodeRecord record in stableNodes)
			{
				if (!nodesByGuid.TryGetValue(record.Guid, out BaseNodeData parent))
					continue;

				foreach (string childGuid in record.ChildGuids)
				{
					if (nodesByGuid.TryGetValue(childGuid, out BaseNodeData child))
						parent.AddChild(child);
				}
			}

			Nodes = rebuiltNodes;
			Root = FindRootByGuid(stableRootGuid) ?? FindRoot();
		}

		private static BaseNodeData CreateNode(ActionTreeNodeRecord record)
		{
			Vector2 position = record.Rect.position;

			switch (record.TypeId)
			{
				case ActionTreeNodeTypeIds.Root:
					return new RootNodeData(position);

				case ActionTreeNodeTypeIds.Sequence:
					return new SequenceNodeData(position);

				case ActionTreeNodeTypeIds.Selector:
					return new SelectorNodeData(position);

				case ActionTreeNodeTypeIds.Action:
					return new ActionNodeData(position);

				case ActionTreeNodeTypeIds.Condition:
					return new ConditionNodeData(position);

				case ActionTreeNodeTypeIds.Method:
					return new MethodNodeData(position);

				default:
					Debug.LogWarning($"Unsupported ActionTree node type id: {record.TypeId}");
					return null;
			}
		}

		private static void RestoreNodePayload(BaseNodeData node, ActionTreeNodeRecord record)
		{
			if (record.Action == null)
				return;

			switch (node)
			{
				case ActionNodeData actionNode:
					actionNode.Action.CopyFrom(record.Action);
					break;

				case ConditionNodeData conditionNode:
					conditionNode.Condition.CopyFrom(record.Action);
					break;

				case MethodNodeData methodNode:
					methodNode.Action.CopyFrom(record.Action);
					break;
			}
		}

		private void EnsureRoot()
		{
			if (Root == null)
				Root = FindRoot();

			if (Root == null)
				Root = new RootNodeData(new Vector2(300, 200));

			if (!Nodes.Contains(Root))
				Nodes.Insert(0, Root);
		}

		private RootNodeData FindRoot()
		{
			foreach (BaseNodeData node in Nodes)
			{
				if (node is RootNodeData root)
					return root;
			}

			return null;
		}

		private RootNodeData FindRootByGuid(string guid)
		{
			if (string.IsNullOrEmpty(guid))
				return null;

			foreach (BaseNodeData node in Nodes)
			{
				if (node is RootNodeData root && root.Guid == guid)
					return root;
			}

			return null;
		}

		private void RemoveNullAndDuplicateNodes()
		{
			HashSet<BaseNodeData> seen = new();

			for (int i = Nodes.Count - 1; i >= 0; i--)
			{
				BaseNodeData node = Nodes[i];

				if (node == null || !seen.Add(node))
					Nodes.RemoveAt(i);
			}
		}

		private void CollectChildrenIntoNodes()
		{
			for (int i = 0; i < Nodes.Count; i++)
			{
				BaseNodeData node = Nodes[i];

				if (node == null)
					continue;

				node.RemoveInvalidChildren();

				foreach (BaseNodeData child in node.Children)
				{
					if (child != null && !Nodes.Contains(child))
						Nodes.Add(child);
				}
			}
		}

		private void RepairNodeState()
		{
			foreach (BaseNodeData node in Nodes)
			{
				node?.EnsureSerializedState();
				node?.RemoveInvalidChildren();
			}
		}

		private void RepairParentLinks()
		{
			foreach (BaseNodeData node in Nodes)
			{
				node?.SetParentFromAsset(null);
			}

			foreach (BaseNodeData parent in Nodes)
			{
				if (parent == null)
					continue;

				for (int i = parent.Children.Count - 1; i >= 0; i--)
				{
					BaseNodeData child = parent.Children[i];

					if (child == null || child == parent || child.IsAncestorOf(parent))
					{
						parent.Children.RemoveAt(i);
						continue;
					}

					if (child.Parent != null && child.Parent != parent)
					{
						parent.Children.RemoveAt(i);
						continue;
					}

					child.SetParentFromAsset(parent);
				}
			}

			Root.SetParentFromAsset(null);
		}

		[Serializable]
		private sealed class ActionTreeNodeRecord
		{
			[SerializeField]
			private string typeId;

			[SerializeField]
			private string guid;

			[SerializeField]
			private Rect rect;

			[SerializeField]
			private string title;

			[SerializeField]
			private string comment;

			[SerializeField]
			private bool isExpanded;

			[SerializeField]
			private List<string> childGuids = new();

			[SerializeField]
			private ActionData action;

			public string TypeId => typeId;
			public string Guid => guid;
			public Rect Rect => rect;
			public string Title => title;
			public string Comment => comment;
			public bool IsExpanded => isExpanded;
			public List<string> ChildGuids => childGuids;
			public ActionData Action => action;

			public static ActionTreeNodeRecord FromNode(BaseNodeData node)
			{
				ActionTreeNodeRecord record = new()
				{
					typeId = node.TypeId,
					guid = node.Guid,
					rect = node.Rect,
					title = node.Title,
					comment = node.Comment,
					isExpanded = node.IsExpanded,
					childGuids = new List<string>()
				};

				foreach (BaseNodeData child in node.Children)
				{
					if (child != null)
						record.childGuids.Add(child.Guid);
				}

				switch (node)
				{
					case ActionNodeData actionNode:
						record.action = actionNode.Action.Clone();
						break;

					case ConditionNodeData conditionNode:
						record.action = conditionNode.Condition.Clone();
						break;

					case MethodNodeData methodNode:
						record.action = methodNode.Action.Clone();
						break;
				}

				return record;
			}
		}
	}
}
