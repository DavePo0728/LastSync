using System.Collections.Generic;
using UnityEngine;

public class RoomEventManager : MonoBehaviour
{
	[SerializeField]
	private List<EventEntry> events = new();
	private readonly HashSet<EventTrigger> triggeredEvents = new();

	public Transform DoorsRoot { get; private set; }

	public RoomInstance Room { get; private set; }
	private void Awake()
	{
		DoorsRoot = transform.Find("Doors");
	}

	public void Initialize(RoomInstance room)
	{
		Room = room;
	}
	/// <summary>
	/// 觸發事件
	/// </summary>
	public void Trigger(EventTrigger trigger)
	{
		if (triggeredEvents.Contains(trigger))
		{
			return;
		}

		foreach (EventEntry entry in events)
		{
			if (entry.Trigger != trigger)
				continue;

			triggeredEvents.Add(trigger);
			entry.RootEvent?.Execute();
			return;
		}
	}

	/// <summary>
	/// 新增事件鏈
	/// </summary>
	public void AddEvent(EventTrigger trigger, RoomEvent rootEvent)
	{
		events.Add(new EventEntry
		{
			Trigger = trigger,
			RootEvent = rootEvent
		});
	}

	/// <summary>
	/// 取得事件
	/// </summary>
	public RoomEvent GetEvent(EventTrigger trigger)
	{
		foreach (EventEntry entry in events)
		{
			if (entry.Trigger == trigger)
				return entry.RootEvent;
		}

		return null;
	}

	public void Clear()
	{
		events.Clear();
		triggeredEvents.Clear();
	}
}
