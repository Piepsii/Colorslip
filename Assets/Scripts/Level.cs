using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public Transform levelCheckpoint;
    public bool isCurrent;

    private void Update()
    {
        if (!isCurrent)
        {
            Transform player = GameObject.FindWithTag("Player").transform;
            transform.position = new Vector3(player.position.x, transform.position.y, player.position.z);

        }
    }

}
