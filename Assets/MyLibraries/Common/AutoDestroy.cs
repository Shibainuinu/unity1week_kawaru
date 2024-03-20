using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [SerializeField] private float destroySecond = 10.0f;

    private float elasticTime = 0;

    private void Update()
    {
        elasticTime += Time.deltaTime;
        if (elasticTime >= destroySecond) 
        {
            Destroy(gameObject);
        }
    }
}
