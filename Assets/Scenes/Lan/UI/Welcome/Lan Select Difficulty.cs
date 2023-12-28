using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LanSelectDifficulty : MonoBehaviour
{
    [SerializeField] LanGameManager gmScript;
    [SerializeField] GameObject welcome;
    public Transform enemyManager;
    Transform skillEffectsParent, startButton;
    int playerLevel;
    TextMeshProUGUI difficultyText;

    private void Awake()
    {
        playerLevel = PlayerPrefs.GetInt("level");
        welcome = GameObject.FindWithTag("UI").transform.GetChild(7).gameObject;
        enemyManager = GameObject.FindWithTag("EnemyManager").transform.GetChild(0);
        skillEffectsParent = GameObject.FindWithTag("SkillEffects").transform;
        startButton = transform.GetChild(2);
        difficultyText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
    }
    public void onValueChange(int var)
    {
        switch (var)
        {
            case 0:
                gmScript.enemyStatsModifier = 100;
                gmScript.difficulty = 0;

                startButton.gameObject.SetActive(true);
                difficultyText.color = Color.white;
                difficultyText.SetText("Select Difficulty: ");
                break;

            case 1:
                gmScript.enemyStatsModifier = 200;
                gmScript.difficulty = 1;


                // if (playerLevel < 6)
                // {
                //     startButton.gameObject.SetActive(false);
                //     difficultyText.color = Color.red;
                //     difficultyText.SetText("Level 6 or higher required");
                // }
                break;

            case 2:
                gmScript.enemyStatsModifier = 350;
                gmScript.difficulty = 2;


                // if (playerLevel < 16)
                // {
                //     startButton.gameObject.SetActive(false);
                //     difficultyText.color = Color.red;
                //     difficultyText.SetText("Level 16 or higher required");
                // }
                break;

            case 3:
                gmScript.enemyStatsModifier = 500;
                gmScript.difficulty = 3;


                // if (playerLevel < 26)
                // {
                //     startButton.gameObject.SetActive(false);
                //     difficultyText.color = Color.red;
                //     difficultyText.SetText("Level 26 or higher required");
                // }
                break;
        }

    }

    public void ButtonPressed()
    {
        for (int i = 0; i < enemyManager.childCount; i++)
        {
            enemyManager.GetChild(i).GetComponent<LanMobsMelee>().UpdateStats();
        }
        welcome.SetActive(false);
    }
}
