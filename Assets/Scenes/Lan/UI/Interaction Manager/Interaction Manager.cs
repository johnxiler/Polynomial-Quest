using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LanInteractionManager : MonoBehaviour
{
    public int index, attemps;
    public LanPlayer player;
    TextMeshProUGUI textBox, attempsText, continueButtonText, hintsText;
    [SerializeField] LanGameManager gmScript;
    public LanNpc npcScript;
    [SerializeField] GameObject continueButton, answerSelection, attempsLabel, hintLabel, hintsAttemptsBG;
    AudioSource questionairSoundEffect;
    [SerializeField] Toggle toggle;
    bool dontShowAgain;
    [SerializeField] LanCameraController playerCamera;
    LanQuestions questionair;
    [SerializeField] Transform questionairParent;
    [SerializeField] GameObject correctText, wrongText, trueOrFalse, fillInTheBlanks, withChoices;
    LanMobsMelee enemy;
    Collider2D enemyCollider;
    [SerializeField] AudioSource DialogueSource;
    int draw;


    private void Awake()
    {
        textBox = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        attempsText = transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        questionairSoundEffect = GetComponent<AudioSource>();
        continueButtonText = continueButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        hintsText = hintLabel.GetComponent<TextMeshProUGUI>();
        player = gmScript.player;
    }



    private void OnEnable()
    {
        gmScript.SetMissionPanelState(false);
        playerCamera.setCameraOffset(-0.5f);
        npcScript = player.npc.GetComponent<LanNpc>();
        continueButton.SetActive(true);

        textBox.SetText("Follow the direction to solve the questions. ");
        continueButtonText.SetText("Continue");

        if (npcScript.isNpc)
        {
            attempsLabel.SetActive(false);
            hintLabel.SetActive(false);
            hintsAttemptsBG.SetActive(false);
        }
        else if (npcScript.isOnTrainingGround && !npcScript.isTrainingExample)
        {

        }
        else //is enemy
        {
            if (npcScript.isAI)
            {
                enemy = npcScript.GetComponent<LanMobsMelee>();
                enemyCollider = enemy.GetComponent<Collider2D>();
                //enemy.target = gmScript.player.GetComponent<Collider2D>();
            }
            DrawQuestions();

            questionairSoundEffect.Play();
            attempsLabel.SetActive(true);
            hintLabel.SetActive(true);
            hintsAttemptsBG.SetActive(true);
        }



        if (dontShowAgain) //if true
        {
            continueButton.SetActive(true);
            toggle.gameObject.SetActive(false);
            Continue();
            // if (npcScript.isOnTrainingGround)
            // {
            //     textBox.SetText(npcScript.dialogue[index]); //after clikcing COntinue button, show the questions
            // }
            // else
            // {
            //     textBox.SetText(questionair.question[index]); //after clikcing COntinue button, show the questions
            // }
        }
        else
        {
            toggle.gameObject.SetActive(true);
            //toggle.isOn = false;
        }
        UpdateUI();
        gmScript.SetMissionPanelState(false);
    }

    public void Continue()
    {
        PlayDialogue();

        toggle.gameObject.SetActive(false);
        if (!npcScript.isNpc && (questionair.question.Length - 1) == index) //if index is equal to the length of text array of the npc, show the question //and object is not npc
        {
            continueButton.SetActive(false);
            answerSelection.SetActive(true);
            textBox.SetText(questionair.question[index]); //after clikcing COntinue button, show the questions
        }
        // else if ((npcScript.dialogue.Length - 1) == index && npcScript.isNpc)
        // {
        //     Debug.Log("show dialogue");
        //     textBox.SetText(npcScript.dialogue[index]);
        // }
        else if (npcScript.isNpc && npcScript.dialogue.Length == index) //last dialogue/question is shown then close interaction
        {
            Debug.Log("exit ss");
            gameObject.SetActive(false);
            index = 0;
        }
        else //EXIT BUTTON
        {
            Debug.Log("exit");
            if (!npcScript.isNpc && (questionair.question.Length - 1) == index)
            {
                continueButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().SetText("Exit");

                // //if npc is on training ground, disable gameobject to hide 
                // if (npcScript.isOnTrainingGround)
                // {
                //     npcScript.transform.GetChild(0).gameObject.SetActive(false);
                // }
            }
            else if (npcScript.isNpc && (npcScript.dialogue.Length - 1) == index)
            {
                continueButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().SetText("Exit");
                // npcScript.transform.GetChild(0).gameObject.SetActive(false);
                // npcScript.GetComponent<CircleCollider2D>().enabled = false;
                npcScript.IsDone();

            }

            if (npcScript.isOnTrainingGround || npcScript.isNpc)
            {
                textBox.SetText(npcScript.dialogue[index]);
                //npcScript.IsDone();
            }
            else
            {
                textBox.SetText(questionair.question[index]);
            }
            index += 1;
        }
    }

    public void UpdateUI()
    {
        attempsText.SetText("attempts: " + answerSelection.GetComponent<LanAnswerSelection>().attempts.ToString());
        hintsText.SetText("hints: " + player.hint.ToString());
    }

    public void ToggleOnValueChange(bool isCheck)
    {
        dontShowAgain = isCheck;
    }

    public void DrawQuestions()
    {
        if (npcScript.isTrainingExample && npcScript.isTrueOrFalse)
        {
            Debug.Log("get true or false attempt");
            do
            {
                draw = Random.Range(0, questionairParent.GetChild(gmScript.difficulty).transform.childCount);
                questionair = questionairParent.GetChild(gmScript.difficulty).GetChild(draw).GetComponent<LanQuestions>();
            }
            while (!questionair.isTrueOrFalse || questionair.isDone);
        }
        else
        {
            if (npcScript.isAI && enemy.GetIsBoss() && enemy.GetIsEmmanuel()) //is a boss
            {
                do
                {
                    draw = Random.Range(0, questionairParent.GetChild(4).transform.childCount);
                    questionair = questionairParent.GetChild(4).GetChild(draw).GetComponent<LanQuestions>();
                }
                while (questionair.isDone);
            }
            else if (npcScript.isAI && enemy.GetIsBoss()) //is a boss
            {
                do
                {
                    draw = Random.Range(0, questionairParent.GetChild(5).transform.childCount);
                    questionair = questionairParent.GetChild(5).GetChild(draw).GetComponent<LanQuestions>();
                }
                while (questionair.isDone);
            }
            else //is a mob
            {
                Debug.Log("get anyt");
                draw = Random.Range(0, questionairParent.GetChild(gmScript.difficulty).transform.childCount);
                questionair = questionairParent.GetChild(gmScript.difficulty).GetChild(draw).GetComponent<LanQuestions>();
            }
            //}
            //while (questionair.isDone);
        }
        answerSelection.GetComponent<LanAnswerSelection>().SelectAnswerType();
        //answerSelection.SetActive(true);
    }




    public LanQuestions GetQuestionair()
    {
        return questionair;
    }
    public void CorrectAnswer()
    {
        correctText.SetActive(true);
        CloseAnswerButtons();
        gmScript.correctSound.Play();
        if (npcScript.isAI && !enemy.GetIsBoss())
        {
            //Destroy(questionairParent.GetChild(gmScript.difficulty).GetChild(draw).GetComponent<LanQuestions>());
            Destroy(questionair.gameObject);
        }
        else
        {
            GetQuestionair().isDone = true;
        }

        if (!npcScript.isAI)//is a statue
        {
            npcScript.IsDone(); //method to disable gameobjects
            player.GiveRewardsServerRpc(100f, false);
        }
        else if (npcScript.isTrainingExample)
        {
            npcScript.IsDone(); //method to disable gameobjects
        }
        else
        {

            if (!enemy.GetIsBoss()) //if not a boss
            {
                enemy.attackCooldown = 1 / enemy.attackSpeed;
                enemy.SetHasHitServerRpc(true);
                enemy.SubtracthealthServerRpc(enemy.finalHealth.Value * .5f, player.NetworkObjectId);
            }
            else //is a boss
            {
                //enemy.SetHasHitServerRpc(true);
                enemy.SubtracthealthServerRpc(100, player.NetworkObjectId);
            }

        }
        gameObject.SetActive(false);
    }
    public void WrongAnswer()
    {

        questionair.attempts -= 1; //subtract attemps
        gmScript.wrongSound.Play();
        wrongText.SetActive(true);
        if (player.score.Value > 0)
        {
            player.score.Value -= 100;
        }
        gmScript.UpdateUI();

        if (questionair.attempts == 1 && npcScript.isAI)
        {
            var damage = player.currentHealth.Value * .40f;
            enemy.AttackOnce(damage);
        }
        else if (questionair.attempts <= 0 && npcScript.isAI) //if wrong and attemp is zero and its an enemy
        {
            var damage = player.finalHealth.Value * .60f;
            enemy.AttackOnce(damage);

            enemyCollider.enabled = true;
            enemy.isDead = false;
            //enemy.target = null;
            enemy.attackCooldown = 1 / enemy.attackSpeed;
            GetQuestionair().attempts = 2;

            //heal enemy
            if (enemy.currentHealth.Value <= 0)
            { //if dead and health is negative
                enemy.HealServerRpc((enemy.currentHealth.Value * -1f) + (enemy.finalHealth.Value * .50f));
            }
            else
            {
                enemy.HealServerRpc(enemy.finalHealth.Value * .50f);
            }
            CloseAnswerButtons();
            gameObject.SetActive(false); //close interaction
        }
        else if (questionair.attempts == 1)
        {
            var damage = player.finalHealth.Value * .40f;
            player.Attacked(damage, player.NetworkObjectId);
        }
        else if (questionair.attempts <= 0) //if wrong and attemp is zero and is statue
        {
            var damage = player.finalHealth.Value * .60f;
            player.Attacked(damage, player.NetworkObjectId);
            gameObject.SetActive(false); //close interaction
            CloseAnswerButtons();

        }

    }

    void CloseAnswerButtons()
    {
        if (!questionair.isFillinTheBlanks && !questionair.isTrueOrFalse)
        { //is with choices
            withChoices.SetActive(false);
        }
        else if (questionair.isTrueOrFalse) //is true or false
        {
            trueOrFalse.SetActive(false);
        }
        else
        { //is a fill in the blanks
            fillInTheBlanks.SetActive(false);
        }
    }

    void PlayDialogue()
    {
        if (npcScript.hasAudio && index < npcScript.clip.Length && npcScript.clip[index] != null)
        {
            DialogueSource.clip = npcScript.clip[index];
            DialogueSource.Play();
        }
    }


    private void OnDisable()
    {
        gmScript.SetMissionPanelState(true);
        playerCamera.setCameraOffset(0);
        answerSelection.SetActive(false);

        //show questionair again for the boss
        if (npcScript.isAI && enemy.GetIsBoss() && enemy.remainingQuestions.Value > 0)
        {
            enemy.SubtractRemainingQuestions();
            Invoke(nameof(OpenQuestionair), .25f);
        }
    }

    void OpenQuestionair()
    {
        if (enemy.remainingQuestions.Value == 0) return;
        gameObject.SetActive(true); //open questionair
    }


}
