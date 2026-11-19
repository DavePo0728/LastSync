using System.Collections;
using UnityEngine;

public class AttackRangeRect : MonoBehaviour
{
	[SerializeField]
	private float attackRange = 5f;

	[SerializeField]
	private float width = 1.5f;

	[SerializeField]
	private float duration = 1f;

	[SerializeField]
	private float yOffset = 0.03f;

	[SerializeField]
	private Material telegraphMaterial;

	[SerializeField]
	private Material hitMaterial;

	private GameObject telegraphArea;
	private GameObject hitArea;
	private Coroutine routine;

	private void Awake()
	{
		EnsureAreaObjects();
		Hide();
	}

	public void Play()
	{
		if (routine != null)
			StopCoroutine(routine);

		routine = StartCoroutine(PlayRoutine());
	}

	public void Play(float range, float castDuration)
	{
		attackRange = range;
		duration = castDuration;
		Play();
	}

	public void Hide()
	{
		if (telegraphArea != null)
			telegraphArea.SetActive(false);

		if (hitArea != null)
			hitArea.SetActive(false);
	}

	// Use the same local footprint as the telegraph. Height is ignored because
	// this is a ground-area attack; the target position is projected onto it.
	public bool ContainsWorldPoint(Vector3 worldPoint, float range)
	{
		Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
		Vector3 size = GetAreaSize(range);
		return Mathf.Abs(localPoint.x) <= size.x * 0.5f &&
			localPoint.z >= 0f && localPoint.z <= size.z;
	}

	private Vector3 GetAreaSize(float length)
	{
		return new Vector3(Mathf.Max(0.001f, width), 0.02f, Mathf.Max(0.001f, length));
	}

	private IEnumerator PlayRoutine()
	{
		EnsureAreaObjects();
		SetAreaSize(telegraphArea, attackRange);
		SetAreaSize(hitArea, 0.001f);

		telegraphArea.SetActive(true);
		hitArea.SetActive(true);

		float timer = 0f;

		while (timer < duration)
		{
			timer += Time.deltaTime;

			float progress = duration <= 0f
				? 1f
				: Mathf.Clamp01(timer / duration);

			SetAreaSize(hitArea, Mathf.Lerp(0.001f, attackRange, progress));
			yield return null;
		}

		yield return new WaitForSeconds(0.1f);

		Hide();
		routine = null;
	}

	private void EnsureAreaObjects()
	{
		if (telegraphArea == null)
			telegraphArea = CreateArea("TelegraphArea", telegraphMaterial);

		if (hitArea == null)
			hitArea = CreateArea("HitArea", hitMaterial);
	}

	private GameObject CreateArea(string objectName, Material material)
	{
		GameObject area = GameObject.CreatePrimitive(PrimitiveType.Cube);
		area.name = objectName;
		area.transform.SetParent(transform, false);
		area.transform.localPosition = new Vector3(0f, yOffset, attackRange * 0.5f);
		area.transform.localRotation = Quaternion.identity;

		if (area.TryGetComponent(out Collider areaCollider))
			Destroy(areaCollider);

		if (material != null && area.TryGetComponent(out Renderer renderer))
			renderer.sharedMaterial = material;

		SetAreaSize(area, attackRange);
		return area;
	}

	private void SetAreaSize(GameObject area, float length)
	{
		if (area == null)
			return;

		Vector3 size = GetAreaSize(length);
		area.transform.localPosition = new Vector3(0f, yOffset, size.z * 0.5f);
		area.transform.localScale = size;
	}
}
