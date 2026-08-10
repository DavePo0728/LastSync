using UnityEngine;

public abstract class RoomEvent : MonoBehaviour
{
	[Header("Next Event")]
	public RoomEvent Next;

	protected RoomEventManager Manager;

	protected virtual void Awake()
	{
		Manager = GetComponent<RoomEventManager>();
	}
	/// <summary>
	/// 執行事件
	/// </summary>
	public abstract void Execute();

	/// <summary>
	/// 呼叫下一個事件
	/// </summary>
	protected void Finish()
	{
		if (Next != null)
			Next.Execute();
	}
}

