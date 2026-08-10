using UnityEngine;

public class DoorController : MonoBehaviour
{
	[SerializeField]
	private Animator animator;

	private static readonly int OpenHash =
		Animator.StringToHash("open");


	public void Open()
	{
		animator.SetBool(OpenHash, true);
	}

	public void Close()
	{
		animator.SetBool(OpenHash, false);
	}
}