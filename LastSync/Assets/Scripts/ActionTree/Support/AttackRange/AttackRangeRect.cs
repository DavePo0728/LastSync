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

		float safeLength = Mathf.Max(0.001f, length);
		area.transform.localPosition = new Vector3(0f, yOffset, safeLength * 0.5f);
		area.transform.localScale = new Vector3(width, 0.02f, safeLength);
	}
}
