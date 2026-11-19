using ActionTree.Data;
using ActionTree.Runtime;
using UnityEngine;

[DisallowMultipleComponent]
public class BehaviorTreeRunner : MonoBehaviour
{
	[SerializeField]
	private ActionTreeAsset tree;

	public ActionTreeAsset Tree => tree;

	private BehaviorTree behaviorTree;
	private AIContext context;

	private AIStatus status;
	bool isDead = false;
    private void Awake()
	{
		RebuildTree();
        status = GetComponent<AIStatus>();
    }
	void OnEnable()
    {
        if (status != null)
        {
            status.Died += StopTree;
        }
    }
	void OnDisable()
    {
        if (status != null)
        {
			status.Died -= StopTree;
        }
    }
    private void OnValidate()
	{
		if (Application.isPlaying)
			RebuildTree();
	}

	public void RebuildTree()
	{
		if (tree == null)
		{
			Debug.LogWarning($"{name} has no ActionTreeAsset assigned.", this);
			behaviorTree = null;
			return;
		}

		context = new AIContext(gameObject);
		behaviorTree = BehaviorTreeBuilder.Build(tree);
	}

	private void Update()
	{
		if (behaviorTree == null|| isDead||status == null ||status.IsDead())
			return;

		behaviorTree.Tick(context);
	}
	private void StopTree()
	{

        isDead = true;
		behaviorTree?.Reset();
    }
}
