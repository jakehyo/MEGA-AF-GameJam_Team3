using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenBlock : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GetComponent<BoxCollider2D>().isTrigger = false;
            GetComponent<SpriteRenderer>().enabled = true;
            tag = "Terrain";
        }
    }
}
