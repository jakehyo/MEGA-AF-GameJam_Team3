using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    static GameObject CurrentRopeInstance = null;

    private void Awake()
    {
        if (CurrentRopeInstance != null)
        {
            Destroy(CurrentRopeInstance);
        }

        CurrentRopeInstance = gameObject;
    }

    public void EnableCollider()
    {
        GetComponent<CapsuleCollider2D>().enabled = true;
    }
}
