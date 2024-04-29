using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyCube : MonoBehaviour
{
    [SerializeField]
    private float delay = 10f;
    private void Start()
    {
        StartCoroutine(DestroyDelay());
    }
    private IEnumerator DestroyDelay()
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

}
