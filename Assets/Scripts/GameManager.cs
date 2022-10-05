using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System;

public class GameManager : MonoBehaviour
{
    public List<GameObject> levels;

    public List<GameObject> levelInstances;
    public Level currentLevel;
    public static GameManager instance;
    public float yOffset;
    private float nextY = 0f;

    public GameObject gameOverUI, gameUI;
    public TextMeshProUGUI timeValue, gameTimeValue;
    public List<Image> levelUI;

    private int levelIndex = 0;
    private bool gameOver = false;
    private float time;

    void Start()
    {
        instance = this;
        levelInstances = new List<GameObject>();
        for (int i = 0; i < levels.Count; i++)
        {
            levelInstances.Add(Instantiate(levels[i]));
            levelInstances[i].transform.position += new Vector3(0f, nextY, 0f);
            nextY += yOffset;
        }
        currentLevel = levelInstances[0].GetComponent<Level>();
        currentLevel.isCurrent = true;
        foreach (Image image in levelUI)
        {
            SetAlpha(image, 0.2f);
        }
        SetAlpha(levelUI[levelIndex], 1f);
    }

    private void Update()
    {
        if (!gameOver)
        {
            time += Time.deltaTime;
            TimeSpan ts = TimeSpan.FromSeconds(time);
            gameTimeValue.text = string.Format("{0:00}:{1:00}:{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds);
        }
    }

    public void NextLevel()
    {
        if(levelInstances.Count == 0)
        {
            return;
        }
        if(levelIndex < levelInstances.Count - 1)
        {
            levelIndex++;
        }
        else
        {
            levelIndex = 0;
        }
        currentLevel.gameObject.SetActive(false);
        levelInstances[levelIndex].SetActive(true);
        foreach(Image image in levelUI)
        {
            SetAlpha(image, 0.2f);
        }
        SetAlpha(levelUI[levelIndex], 1f);

        currentLevel = levelInstances[levelIndex].GetComponent<Level>();
        currentLevel.isCurrent = true;
    }

    private void SetAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }

    public void RemoveLevel()
    {
        GameObject temp = levelUI[levelIndex].gameObject;
        levelUI.RemoveAt(levelIndex);
        Destroy(temp);
        levelInstances.RemoveAt(levelIndex);
        if(levelInstances.Count == 0)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        GameObject.FindWithTag("Player").GetComponent<Rigidbody>().isKinematic = true;
        gameOver = true;
        gameOverUI.SetActive(true);
        gameUI.SetActive(false);
        TimeSpan ts = TimeSpan.FromSeconds(time);
        timeValue.text = string.Format("{0:00}:{1:00}:{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
