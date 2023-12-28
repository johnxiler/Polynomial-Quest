using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssassinSkill4 : MonoBehaviour
{
    [SerializeField] LanGameManager gmScript;
    public ulong playerID;
    private void OnEnable() {
        if(gmScript.player.NetworkObjectId == playerID) {
            gmScript.player.moveSpeed += 1f;
            gmScript.player.attackSpeed += 1;
        }
        StartCoroutine(SkillDuration());
    }

    IEnumerator SkillDuration() {
        yield return new  WaitForSeconds(10);
        if(gmScript.player.NetworkObjectId == playerID) { //remove buff if true
            gmScript.player.moveSpeed -= 1f;
            gmScript.player.attackSpeed -= 1;
        }
        Destroy(gameObject);
    }


}
