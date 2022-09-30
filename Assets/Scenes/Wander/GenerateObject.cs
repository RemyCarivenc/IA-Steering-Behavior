using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateObject: MonoBehaviour
{
    public GameObject wanderPrefab;
    public int amount = 100;

    private void Start() {
        for(int i =0; i< amount; i++)
        {
            Instantiate(wanderPrefab, transform.position, Quaternion.identity);
        }
    }
}
