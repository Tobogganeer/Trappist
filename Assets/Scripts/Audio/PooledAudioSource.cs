using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PooledAudioSource : MonoBehaviour
{
    private Transform originalParent;

    private void Awake()
    {
        originalParent = transform.parent;
        gameObject.SetActive(false);
    }

    public void DisableAfterTime(float seconds)
    {
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(false);
            transform.SetParent(originalParent);
            return;
        }

        StopAllCoroutines();
        StartCoroutine(DisableAfterSeconds(seconds));
    }

    private IEnumerator DisableAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        gameObject.SetActive(false);
        transform.SetParent(originalParent);
    }

    private void OnDestroy()
    {
        AudioMaster.OnAudioSourceDestroyed(this);
    }
}
