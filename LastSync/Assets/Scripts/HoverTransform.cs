using UnityEngine;
using UnityEngine.InputSystem;

public class HoverTransform : MonoBehaviour
{
	[SerializeField] private float aimHeight = 0f;

	private Camera mainCam;
	private Plane aimPlane;

	private void Start()
	{
		mainCam = Camera.main;
		aimPlane = new Plane(Vector3.up, new Vector3(0f, aimHeight, 0f));
	}

	private void Update()
	{
		Ray mouseRay = mainCam.ScreenPointToRay(
			Mouse.current.position.ReadValue());

		if (aimPlane.Raycast(mouseRay, out float distance))
		{
			transform.position = mouseRay.GetPoint(distance);
		}
	}
}