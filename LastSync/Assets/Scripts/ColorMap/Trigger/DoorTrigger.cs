using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
	[SerializeField]
	private DoorController door;

	private void OnTriggerEnter(Collider other)
	{
		//if (!other.CompareTag("Player"))
		//	return;
		////Debug.Log("1");
		//door.Open();
	}

	private void OnTriggerExit(Collider other)
	{
		//if (!other.CompareTag("Player"))
		//	return;
		////Debug.Log("2");
		//door.Close();
	}
}