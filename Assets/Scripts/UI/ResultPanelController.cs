using deVoid.UIFramework;
using deVoid.Utils;
using DevoidUI.Core;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultPanelProperties : PanelProperties
{
    public bool isWin;
    public int score;

    public ResultPanelProperties(bool isWin, int score)
    {
        this.isWin = isWin;
        this.score = score;
    }
}

public class ResultPanelController : APanelController<ResultPanelProperties>
{
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text bestScoreText;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button mainMenuButton;

    protected override void OnPropertiesSet()
    {
        base.OnPropertiesSet();
        resultText.text = Properties.isWin ? "Nhiệm vụ hoàn thành" : "Nhiệm vụ thất bại";
        scoreText.text = $"Score: {Properties.score}";
        if (Properties.isWin)
        {
            int bestScore = 0;
            bestScoreText.gameObject.SetActive(true);
            nextLevelButton.gameObject.SetActive(true);
            if (PlayerPrefs.HasKey("bestScore"))
            {
                bestScore = PlayerPrefs.GetInt("bestScore");
                if (Properties.score > bestScore)
                {
                    PlayerPrefs.SetInt("bestScore", Properties.score);
                    bestScore = Properties.score;
                }
            }
            else
            {
                PlayerPrefs.SetInt("bestScore", Properties.score);
                bestScore = Properties.score;
            }

            bestScoreText.text = $"Best Score: {bestScore}";
        }
        else
        {
            bestScoreText.gameObject.SetActive(false);
            nextLevelButton.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        playAgainButton.onClick.AddListener(() =>
        {
            Signals.Get<GameManagerRestartGameSignal>().Dispatch();
            UIFrameManager.Instance.CloseExactPanel(ScreenIds.ResultPanel);
        });
        nextLevelButton.onClick.AddListener(() =>
        {
            Signals.Get<GameManagerNextLevelSignal>().Dispatch();
            UIFrameManager.Instance.CloseExactPanel(ScreenIds.ResultPanel);
        });
        mainMenuButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene(0);
        });
    }
}
