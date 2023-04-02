using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private GameObject ropePrefab;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if this hit something other than the player
        if (!collision.gameObject.CompareTag("Player") && !collision.gameObject.CompareTag("Rope") && !collision.gameObject.CompareTag("PlayerProjectile"))
        {
            OnTerrainCollide();
        }
    }
    private void OnTerrainCollide()
    {
        Instantiate(ropePrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

}
