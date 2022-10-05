using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public Transform checkpoint;
    public bool isCurrent;
    private Transform player;

    private void Start()
    {
         player = GameObject.FindWithTag("Player").transform;
    }

    private void Update()
    {
        if (!isCurrent)
        {
            SetYPosition(transform.position.y);
        }
    }

    public void SetYPosition(float yPosition)
    {
        transform.position = new Vector3(player.position.x, yPosition, player.position.z);
    }

}
