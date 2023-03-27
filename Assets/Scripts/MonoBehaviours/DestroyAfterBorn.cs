using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterBorn : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(dying());
    }

    IEnumerator dying()
    {
        yield return new WaitForSeconds(5);
        Destroy(gameObject);
    }
}
