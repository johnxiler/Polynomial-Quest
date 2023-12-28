using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanDungeonExit : MonoBehaviour
{
    public Transform player;
    public GameObject transition;
    [SerializeField] LanGameManager gmScript;

    private void Start()
    {
        transition = GameObject.FindWithTag("UI").transform.GetChild(5).GetChild(0).gameObject;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        print("Trigger exit");
        transition.gameObject.SetActive(true);
        gmScript.player.transform.position = transform.parent.GetChild(3).position;
        gmScript.ShowWeather();
        gmScript.isInsideDungeon = false;
    }
}
