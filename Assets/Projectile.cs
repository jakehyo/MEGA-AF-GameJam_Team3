using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if this hit something other than the player
        if (!collision.gameObject.CompareTag("Player"))
        {
            OnTerrainCollide();
        }
    }
    private void OnTerrainCollide()
    {
        Destroy(gameObject);
    }

}
