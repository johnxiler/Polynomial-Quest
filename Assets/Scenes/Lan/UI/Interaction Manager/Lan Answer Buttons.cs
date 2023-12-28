using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LanAnswerButtons : MonoBehaviour
{
    public LanAnswerSelection answerSelection;
    [SerializeField] LanInteractionManager interactionManager;
    [SerializeField] LanGameManager gmScript;
    [SerializeField] Transform rewardsLabelPool;

    TextMeshProUGUI textmesh;


    private void Awake()
    {
        textmesh = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {

        //set size
        if (textmesh.text.Length >= 20)
        {
            textmesh.fontSize = 23;
        }
        else
        {
            textmesh.fontSize = 45;
        }
    }


    public void ButtonPressed()
    {
        if (answerSelection.answerIndex == transform.GetSiblingIndex())
        { //0 //
            interactionManager.CorrectAnswer();
        }
        else
        { //if wrong
            interactionManager.WrongAnswer();
        }
    }
}
