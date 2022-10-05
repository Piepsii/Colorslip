using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            Transform spawnpoint = other.transform.GetChild(0);
            GameManager.instance.currentLevel.checkpoint = spawnpoint;
            var touchedLevel = spawnpoint.GetComponentInParent<Level>();
            if (touchedLevel.isCurrent == false)
            {
                GameManager.instance.Iterate();
                GameManager.instance.currentLevel = touchedLevel;
                touchedLevel.isCurrent = true;
            }
        }
        else if (other.CompareTag("Death"))
        {
            GameManager.instance.NextLevel();
            Transform checkpoint = GameManager.instance.currentLevel.checkpoint;
            transform.SetPositionAndRotation(checkpoint.position, checkpoint.rotation);
        }
        else if (other.CompareTag("Goal"))
        {
            GameManager.instance.RemoveLevel();
            GameManager.instance.NextLevel();
            Transform checkpoint = GameManager.instance.currentLevel.checkpoint;
            transform.SetPositionAndRotation(checkpoint.position, checkpoint.rotation);
        }

    }
}
