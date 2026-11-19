using System;
using UnityEngine;

[Serializable]
public sealed class WeaponShotAudio
{
    [SerializeField] private AudioClip clip;
    [SerializeField] private Transform origin;
    [SerializeField, Range(0f, 1f)] private float volume = 0.7f;
    [SerializeField, Min(0.1f)] private float minDistance = 5f;
    [SerializeField, Min(0.1f)] private float maxDistance = 40f;

    private AudioSource source;

    public void Play(Transform weapon)
    {
        if (clip == null) return;

        if (source == null)
        {
            Transform audioOrigin = origin != null ? origin : weapon;
            source = audioOrigin.gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.dopplerLevel = 0f;
        }

        source.minDistance = Mathf.Max(0.1f, minDistance);
        source.maxDistance = Mathf.Max(source.minDistance + 0.1f, maxDistance);
        source.PlayOneShot(clip, Mathf.Clamp01(volume));
    }
}
