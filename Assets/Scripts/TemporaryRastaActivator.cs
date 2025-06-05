using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TemporaryRastaActivator : MonoBehaviour
{
    public string triggerTag = "GreenLeaf";

    public List<GameObject> objectsToActivate = new List<GameObject>();
    public List<ParticleSystem> particleSystemsToControl = new List<ParticleSystem>();
    public Image uiImageToShow;

    public float activationDuration = 10.0f;

    public AudioSource audioSource;
    public AudioClip activationClip;

    private Coroutine deactivationCoroutine;

    void Start()
    {
        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
        foreach (ParticleSystem ps in particleSystemsToControl)
        {
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
        if (uiImageToShow != null)
        {
            uiImageToShow.enabled = false;
        }

        if (audioSource == null)
        {
            Debug.LogWarning("AudioSource not assigned in TemporaryRastaActivator.");
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                 Debug.LogError("AudioSource component not found on GameObject and not assigned manually.");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(triggerTag))
        {
            Debug.Log($"Collision with trigger '{triggerTag}'. Activating objects, starting particle systems and showing UI Image.");

            if (deactivationCoroutine != null)
            {
                StopCoroutine(deactivationCoroutine);
            }

            foreach (GameObject obj in objectsToActivate)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }

            foreach (ParticleSystem ps in particleSystemsToControl)
            {
                if (ps != null)
                {
                    ps.Play();
                }
            }

            if (uiImageToShow != null)
            {
                uiImageToShow.enabled = true;
            }

            if (audioSource != null && activationClip != null)
            {
                audioSource.clip = activationClip;
                audioSource.Play();
                Debug.Log("Starting track playback.");
            }

            deactivationCoroutine = StartCoroutine(DeactivateAfterDelay());
        }
    }

    private IEnumerator DeactivateAfterDelay()
    {
        yield return new WaitForSeconds(activationDuration);

        Debug.Log($"Passed {activationDuration} seconds. Deactivating objects, stopping particle systems and hiding UI Image.");

        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }

        foreach (ParticleSystem ps in particleSystemsToControl)
        {
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        if (uiImageToShow != null)
        {
            uiImageToShow.enabled = false;
        }

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log("Stopping track playback.");
        }

        deactivationCoroutine = null;
    }
}
