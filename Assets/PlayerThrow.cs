using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerThrow : MonoBehaviour
{
    public float baseThrowSpeed = 1.0f;
    public float chargeThrowMultiplier = 10.0f;
    public float maxChargeTime = 4.0f;

    private float chargeTime = 0.0f;
    [SerializeField] private GameObject projectilePrefab;
    PlayerMovement playerMoveScript;
    AudioSource audioSource;
    public AudioClip throwClip;

    private void Start()
    {
        playerMoveScript = GetComponent<PlayerMovement>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        //if throw button (e) is pressed, charge the throw timer
        if (Input.GetKey(KeyCode.E))
        {
            chargeTime = Mathf.Clamp(chargeTime + Time.deltaTime, 0.0f, maxChargeTime);
        }
        else if (Input.GetKeyUp(KeyCode.E)) //when throw button is released, throw a projectile
        {
            //sfx
            audioSource.clip = throwClip;
            audioSource.Play();
            
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            //swap direction if player is not facing right
            if (playerMoveScript.facingRight)
            {
                projectile.GetComponent<Rigidbody2D>().velocity = new Vector2(baseThrowSpeed, baseThrowSpeed) * (chargeThrowMultiplier * chargeTime);
            }
            else
            {
                projectile.GetComponent<Rigidbody2D>().velocity = new Vector2(-baseThrowSpeed, baseThrowSpeed) * (chargeThrowMultiplier * chargeTime);
            }
            chargeTime = 0.0f;
        }
        else if (!Input.GetKey(KeyCode.E))
        {
            chargeTime = 0.0f;
        }
    }
}
