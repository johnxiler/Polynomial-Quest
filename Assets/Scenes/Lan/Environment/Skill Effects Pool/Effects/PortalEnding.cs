using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalEnding : MonoBehaviour
{
    [SerializeField] LanGameManager gmScript;
    [SerializeField] bool isExit, isEntrance;
    [SerializeField] GameObject transition;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // original: if (other.CompareTag("Player") && isEntrance && gmScript.dungeonStatues == 21){}
        if (other.CompareTag("Player") && isEntrance)
        {
            transition.SetActive(true);
            gmScript.isPortalFound = true;
            gmScript.UpdateMission();


            gmScript.player.transform.position = transform.parent.GetChild(2).position; //teleport
        }
        else if (other.CompareTag("Player") && isExit)
        {
            transition.SetActive(true);
            gmScript.player.transform.position = transform.parent.GetChild(3).position;
        }
    }
}
