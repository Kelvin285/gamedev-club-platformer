using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelHit : MonoBehaviour
{
    public GameObject boom;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        var player = collision.gameObject.GetComponent<Player>();
        if (player != null)
        {
            if (player.jump || player.dash > 0 || !player.on_ground && player.motion.y > 0)
            {
                boom.SetActive(true);
                Destroy(gameObject);
            }
        }
    }
    private void OnCollisionStay(Collision collision)
    {
        var player = collision.gameObject.GetComponent<Player>();
        if (player != null)
        {
            if (player.jump || player.dash > 0 || !player.on_ground && player.motion.y > 0)
            {
                boom.SetActive(true);
                Destroy(gameObject);
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        var player = collision.gameObject.GetComponent<Player>();
        if (player != null)
        {
            if (player.jump || player.dash > 0 || !player.on_ground && player.motion.y > 0)
            {
                boom.SetActive(true);
                Destroy(gameObject);
            }
        }
    }
}
