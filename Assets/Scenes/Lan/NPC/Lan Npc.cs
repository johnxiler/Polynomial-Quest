using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanNpc : MonoBehaviour
{
    public GameObject interactButton, attackButton;
    public LanGameManager gmScript;
    public LanPlayer player;
    public bool isNpc, isOnTrainingGround, isTrainingExample, isLibrarian, isBookShelf, isTrainingGroundStatue, isAI, isDone, isFillinTheBlanks, isTrueOrFalse, hasAudio;
    Collider2D trigger;
    Transform questionair, dungeon;
    //public LanQuestions questionairScript;
    [SerializeField] int sequence;

    public string[] dialogue;
    public AudioClip[] clip;
    public void Initialize()
    {

        trigger = GetComponent<Collider2D>();
        attackButton = GameObject.FindWithTag("Controls").transform.GetChild(1).gameObject;
        interactButton = GameObject.FindWithTag("Controls").transform.GetChild(2).gameObject;
        questionair = GameObject.FindWithTag("Questionair").transform;
        //GetComponent<LanPlayer>().enabled = true;

        StartCoroutine(HasSetDifficultyCheck());
    }

    IEnumerator HasSetDifficultyCheck()
    {
        while (!gmScript.player.IsOwnedByServer && !gmScript.hasSetDifficultyForClient)
        {
            yield return null;
        }
        //check if is a statue
        if (!isAI && !isNpc)
        {
            dungeon = transform.parent.parent.parent;
            LoadStatue();
        }
        else if (isOnTrainingGround)
        {
            BySequenceCheck();
            LoadStatue();
        }
    }

    public void LoadStatue()
    {
        if (PlayerPrefs.GetInt(transform.GetSiblingIndex().ToString() + transform.parent.GetSiblingIndex().ToString() + transform.parent.parent.GetSiblingIndex().ToString() + gmScript.difficulty) == 1 && !isAI && !isNpc)
        {
            IsDone();
        }
        else if (isOnTrainingGround && PlayerPrefs.GetInt(transform.GetSiblingIndex().ToString() + transform.parent.GetSiblingIndex().ToString() + " isOnTrainingground") == 1)
        {
            IsDone();

        }
        else if (isTrainingExample && PlayerPrefs.GetInt(transform.GetSiblingIndex().ToString() + transform.parent.GetSiblingIndex().ToString() + " isOnTrainingground") == 1)
        {
            IsDone();
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && trigger.isTrigger)
        {
            player = other.GetComponent<LanPlayer>();
            if (!player.IsLocalPlayer) return;
            interactButton.SetActive(true);
            attackButton.SetActive(false);
            gmScript.player.npc = gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && trigger.isTrigger)
        {
            player = null;
            attackButton.SetActive(true);
            interactButton.SetActive(false);
            gmScript.player.npc = null;
        }
    }

    public void IsDone()
    {
        if (!isAI && !isNpc)
        {
            if (isTrainingExample)
            {
                if (gmScript.exmapleStatues < 2)
                {
                    gmScript.exmapleStatues++;
                    SaveTrainingStatue();
                }
            }
            else  //is a statue
            {
                gmScript.dungeonStatues++;
                PlayerPrefs.SetInt(transform.GetSiblingIndex().ToString() + transform.parent.GetSiblingIndex().ToString() + transform.parent.parent.GetSiblingIndex().ToString() + gmScript.difficulty, 1); //save
            }
            gameObject.SetActive(false);
            gmScript.UpdateMission();

        }
        else if (isOnTrainingGround)
        {
            if (isLibrarian)
            {
                gmScript.librarian++;
            }
            else if (isBookShelf)
            {
                if (gmScript.topicsDiscovered < 9)
                {
                    gmScript.topicsDiscovered++;
                }
            }
            else if (isTrainingGroundStatue)
            {
                if (gmScript.interactedStatues < 9)
                {
                    gmScript.interactedStatues++;
                    gmScript.BySequenceCheck();
                }
            }
            gmScript.UpdateMission();

            if (isBookShelf) //isTrainingGroundStatue || isBookShelf
            {
                gameObject.SetActive(false);
            }
            else
            {
                transform.GetChild(0).gameObject.SetActive(false);
            }

            SaveTrainingStatue();
        }
        else //enemy ai
        {
            gmScript.player.DisableEnemyServerRpc(transform.GetSiblingIndex());
        }


    }


    private void OnDisable()
    {
        //DrawQuestions();
    }

    void SaveTrainingStatue()
    {
        //save
        PlayerPrefs.SetInt(transform.GetSiblingIndex().ToString() + transform.parent.GetSiblingIndex().ToString() + " isOnTrainingground", 1);
    }


    public void BySequenceCheck()
    {
        if (!isAI && isTrainingGroundStatue && transform.GetSiblingIndex() == gmScript.interactedStatues)
        {
            //Debug.Log("true " + transform.GetSiblingIndex() + gmScript.interactedStatues);
            GetComponent<CircleCollider2D>().enabled = true;
            transform.GetChild(0).gameObject.SetActive(true);
            transform.GetChild(1).gameObject.SetActive(true);
            transform.GetChild(3).gameObject.SetActive(true);
        }
        else if (!isAI && isTrainingGroundStatue && isOnTrainingGround)
        {
            //Debug.Log("false " + transform.GetSiblingIndex() + gmScript.interactedStatues);
            GetComponent<CircleCollider2D>().enabled = false;
            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(1).gameObject.SetActive(false);
            transform.GetChild(3).gameObject.SetActive(false);
        }
    }
}
