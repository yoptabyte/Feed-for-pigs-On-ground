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
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(triggerTag))
        {
            Debug.Log($"Столкновение с триггером '{triggerTag}'. Активирую объекты, запускаю системы частиц и показываю UI Image.");

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

            deactivationCoroutine = StartCoroutine(DeactivateAfterDelay());
        }
    }

    private IEnumerator DeactivateAfterDelay()
    {
        yield return new WaitForSeconds(activationDuration);

        Debug.Log($"Прошло {activationDuration} секунд. Деактивирую объекты, останавливаю системы частиц и скрываю UI Image.");

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

        deactivationCoroutine = null;
    }
}