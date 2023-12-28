using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using TMPro;

public class Ranking : MonoBehaviour
{
    [SerializeField] Transform byScore, byLevel;
    [SerializeField] TextMeshProUGUI title;
    [SerializeField] LanGameManager gmScript;
    int arrayLength;
    LanPlayer[] players;
    LanPlayer temp;

    private void OnEnable()
    {
        players = gmScript.players;
        arrayLength = players.Length;

        ByScore();
    }

    public void ByScore()
    {
        var hasFirstLoopDone = false;
        for (int i = 0; i < arrayLength; i++)
        {
            for (int j = 0; j < arrayLength - i - 1; j++)
            {
                if (players[j].score.Value > players[j + 1].score.Value)
                {
                    temp = players[j];
                    players[j] = players[j + 1];
                    players[j + 1] = temp;
                }
            }
        }

        foreach (var item in players.Reverse())
        {
            if (!hasFirstLoopDone)
            {
                byScore.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().SetText(item.playerName.Value.ToString());
                byScore.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().SetText(item.score.Value.ToString());
                hasFirstLoopDone = true;
            }
            else
            {
                if (byScore.childCount > 1)
                {
                    for (int i = 1; i < byScore.childCount; i++)
                    {
                        Destroy(byScore.GetChild(i).gameObject);
                    }
                }
                var firstItem = Instantiate(byScore.GetChild(0).transform, byScore);
                firstItem.GetChild(0).GetComponent<TextMeshProUGUI>().SetText(item.playerName.Value.ToString());
                firstItem.GetChild(1).GetComponent<TextMeshProUGUI>().SetText(item.score.Value.ToString());
            }
        }

        title.SetText("Score");
        byLevel.parent.gameObject.SetActive(false);
        byScore.parent.gameObject.SetActive(true);
    }

    public void ByLevel()
    {
        var hasFirstLoopDone = false;
        for (int i = 0; i < arrayLength; i++)
        {
            for (int j = 0; j < arrayLength - i - 1; j++)
            {
                if (players[j].level.Value > players[j + 1].level.Value)
                {
                    temp = players[j];
                    players[j] = players[j + 1];
                    players[j + 1] = temp;
                }
            }
        }
        foreach (var item in players.Reverse())
        {
            if (!hasFirstLoopDone)
            {
                byLevel.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().SetText(item.playerName.Value.ToString());
                byLevel.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().SetText(item.level.Value.ToString());
                hasFirstLoopDone = true;
            }
            else
            {
                if (byLevel.childCount > 1)
                {
                    for (int i = 0; i < byLevel.childCount; i++)
                    {
                        Destroy(byLevel.GetChild(i).gameObject);
                    }
                }
                var firstItem = Instantiate(byScore.GetChild(0).transform, byLevel);
                firstItem.GetChild(0).GetComponent<TextMeshProUGUI>().SetText(item.playerName.Value.ToString());
                firstItem.GetChild(1).GetComponent<TextMeshProUGUI>().SetText(item.level.Value.ToString());
            }
        }

        title.SetText("Level");
        byLevel.parent.gameObject.SetActive(true);
        byScore.parent.gameObject.SetActive(false);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
