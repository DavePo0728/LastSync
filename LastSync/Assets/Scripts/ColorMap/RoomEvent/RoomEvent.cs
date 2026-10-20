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
	/// ����ƥ�
	/// </summary>
	public abstract void Execute();

	/// <summary>
	/// �I�s�U�@�Өƥ�
	/// </summary>
	protected void Finish()
	{
		if (Next != null)
			Next.Execute();
	}
}

