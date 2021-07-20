using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject gameOverText;                                 // 게임종료 텍스트 담는 변수
    public bool isGameOver;                                         // 게임종료

    void Start()
    {
        isGameOver = false;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void EndGame()
    {
        isGameOver = true;
        gameOverText.SetActive(true);
    }
}
