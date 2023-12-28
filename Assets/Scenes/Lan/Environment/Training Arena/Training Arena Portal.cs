using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TrainingArenaPortal : MonoBehaviour
{
    [SerializeField] Transform popPool;
    [SerializeField] LanGameManager gmScript;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<LanPlayer>();
            if (player.isDoneTraining == true && player.IsLocalPlayer)
            {
                PlayerPrefs.SetInt("hasFinishTraining", 1);
                player.StartIntroduction();
                StartCoroutine(TeleportDelay(player));
            }

        }
    }

    IEnumerator TeleportDelay(LanPlayer player)
    {
        gmScript.EnableMinimap(false);
        gmScript.SetMissionPanelState(false);
        yield return new WaitForSeconds(1.5f);
        player.transform.position = new(0, 0);
    }
}
