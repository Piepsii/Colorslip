using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            Transform checkpoint = other.transform.GetChild(0);
            GameManager.instance.currentLevel.levelCheckpoint = checkpoint;
            GameManager.instance.NextLevel();
            GameManager.instance.currentLevel.isCurrent = false;
            checkpoint.GetComponentInParent<Level>().isCurrent = true;
        }
        else if (other.CompareTag("Death"))
        {
            GameManager.instance.NextLevel();
            Transform checkpoint = GameManager.instance.currentLevel.levelCheckpoint;
            transform.SetPositionAndRotation(checkpoint.position, checkpoint.rotation);
        }
        else if (other.CompareTag("Goal"))
        {
            GameManager.instance.RemoveLevel();
            GameManager.instance.NextLevel();
            Transform checkpoint = GameManager.instance.currentLevel.levelCheckpoint;
            transform.SetPositionAndRotation(checkpoint.position, checkpoint.rotation);
        }

    }
}
