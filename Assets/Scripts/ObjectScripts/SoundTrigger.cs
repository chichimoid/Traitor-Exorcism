using UnityEngine;
using System.Collections;

public class SoundTrigger : MonoBehaviour
{
    [Header("Sound Settings")]
    public AudioSource audioSource;
    public AudioClip[] clips;
    public bool playOnce = true;

    [Header("Cooldown Settings")]
    [SerializeField] private float cooldownDuration = 13f;

    private bool isReady = true;
    private bool hasPlayed = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && isReady)
        {
            if (playOnce && hasPlayed) return;

            PlayRandomSound();
            StartCoroutine(CooldownTimer());

            if (playOnce) hasPlayed = true;
        }
    }

    IEnumerator CooldownTimer()
    {
        isReady = false;
        yield return new WaitForSeconds(cooldownDuration);
        isReady = true;
    }

    void PlayRandomSound()
    {
        if (clips.Length == 0) return;

        int randomIndex = Random.Range(0, clips.Length);
        audioSource.PlayOneShot(clips[randomIndex]);
    }
}