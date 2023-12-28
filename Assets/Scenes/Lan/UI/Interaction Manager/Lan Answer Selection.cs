using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LanAnswerSelection : MonoBehaviour
{
    public GameObject[] buttons; //gameobjects
    [SerializeField] Transform wrongText, correctText, itemsPoolWS, rewardsLabelPool;
    Transform reward;
    [SerializeField] LanGameManager gmScript;
    [SerializeField] LanInteractionManager interactionManager;
    LanPlayer player;
    public string[] choices;
    public int answerIndex, attempts, rewardIndexRange, rewardIndex, potionOrItem;
    LanMobsMelee enemy;
    Collider2D enemyCollider;
    int buttonsCount = 4;


    private void Start()
    {
        player = gmScript.player; //get player from Game Manager
    }
    private void OnEnable()
    {
        //npc = player.npc.GetComponent<LanNpc>();

        choices = interactionManager.GetQuestionair().choices; //answer choices array put in answer variable
        answerIndex = interactionManager.GetQuestionair().answerIndex;
        attempts = interactionManager.GetQuestionair().attempts;

        if (interactionManager.npcScript.isAI)
        {
            enemy = interactionManager.npcScript.GetComponent<LanMobsMelee>();
            enemyCollider = interactionManager.npcScript.GetComponent<Collider2D>();
        }


    }

    public void UseHint()
    {
        if (gmScript.player.hint > 0 && buttonsCount > 1)
        {
            gmScript.player.hint--;
            buttonsCount--;
            int randomValue;
            do
            {
                randomValue = Random.Range(0, 4); // draw 0-3 while random value
            } while (randomValue == answerIndex || !transform.GetChild(0).GetChild(randomValue).gameObject.activeSelf);


            transform.GetChild(0).GetChild(randomValue).gameObject.SetActive(false);
            interactionManager.UpdateUI();

        }
    }

    public void MediumConfirmButton()
    {
        string temp = transform.GetChild(1).GetChild(0).GetComponent<TMP_InputField>().text;
        temp = temp.Replace(" ", "");
        choices[0] = choices[0].Replace(" ", "");
        Debug.Log("Answer is " + choices[0]);


        if (temp.Equals(choices[0], System.StringComparison.OrdinalIgnoreCase))
        { //Ignore Upper and lower case
            correctText.gameObject.SetActive(true);
            //transform.parent.gameObject.SetActive(false);
            interactionManager.gameObject.SetActive(false);
            gmScript.correctSound.Play(); //play sound
            interactionManager.npcScript.IsDone();
            gmScript.player.GiveRewardsServerRpc(25f, false);

        }
        else
        { //if wrong
            attempts -= 1;
            wrongText.gameObject.SetActive(true);
            interactionManager.UpdateUI();

            if (attempts <= 0 && interactionManager.npcScript.isAI)
            { //if wrong and attemp is zero
                enemyCollider.enabled = true;
                enemy.isDead = false;
                interactionManager.GetQuestionair().attempts = 2;

                //heal enemy
                if (enemy.currentHealth.Value <= 0)
                { //if dead and health is negative
                    enemy.HealServerRpc((enemy.currentHealth.Value * -1f) + (enemy.finalHealth.Value * .10f));
                }
                else
                {
                    enemy.HealServerRpc(enemy.finalHealth.Value * .10f);
                }

                transform.parent.gameObject.SetActive(false);
                interactionManager.gameObject.SetActive(false);
            }
            else if (attempts <= 0)
            { //if wrong and attemp is zero and is station
                transform.parent.gameObject.SetActive(false);
                interactionManager.gameObject.SetActive(false);
            }
        }
    }


    //for true or false
    public void True()
    {
        if (answerIndex == 1)
        {
            interactionManager.CorrectAnswer();
        }
        else
        {
            interactionManager.WrongAnswer();
        }
    }

    public void False()
    {
        if (answerIndex == 0)
        {
            interactionManager.CorrectAnswer();
        }
        else
        {
            interactionManager.WrongAnswer();
        }
    }


    public void SelectAnswerType()
    {
        if (!interactionManager.GetQuestionair().isTrueOrFalse) //isfill in the blanks false, if true, it has choices
        {
            buttons[0].transform.GetChild(0).GetComponent<TextMeshProUGUI>().SetText(interactionManager.GetQuestionair().choices[0]);
            buttons[1].transform.GetChild(0).GetComponent<TextMeshProUGUI>().SetText(interactionManager.GetQuestionair().choices[1]);
            buttons[2].transform.GetChild(0).GetComponent<TextMeshProUGUI>().SetText(interactionManager.GetQuestionair().choices[2]);
            buttons[3].transform.GetChild(0).GetComponent<TextMeshProUGUI>().SetText(interactionManager.GetQuestionair().choices[3]);

            transform.GetChild(0).gameObject.SetActive(true); //enable with choices object
            transform.GetChild(2).gameObject.SetActive(false); //disable true or false
            //transform.GetChild(1).gameObject.SetActive(false); //disable fill in the blanks

            foreach (var item in buttons)
            {
                item.SetActive(true);
            }
        }
        else if (interactionManager.GetQuestionair().isTrueOrFalse)
        {
            transform.GetChild(2).gameObject.SetActive(true); //enable true or false
            transform.GetChild(0).gameObject.SetActive(false);
            //transform.GetChild(1).gameObject.SetActive(false); //disable fill in the blanks
        }
    }

    private void OnDisable()
    {
        buttonsCount = 4;
    }

}
