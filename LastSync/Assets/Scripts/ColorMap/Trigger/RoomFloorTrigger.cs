using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RoomFloorTrigger : MonoBehaviour
{
	public RoomEventManager roomEventManager;


	private void Start()
	{
		//Debug.Log(transform.parent.transform.parent.gameObject.name);
		roomEventManager = transform.parent.transform.parent.GetComponentInParent<RoomEventManager>();
	}
	private void OnTriggerEnter(Collider other)
	{
		if (!other.CompareTag("Player"))
		{

			return;
		}
			

		if (roomEventManager == null)
		{

			return;
		}
			
		roomEventManager.Trigger(EventTrigger.EnterRoom);
	}
}