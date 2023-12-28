using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MageSkill3 : MonoBehaviour
{
    [SerializeField] LanGameManager gmScript;
    public ulong playerID;
    private void OnEnable() {
        if(gmScript.player.NetworkObjectId == playerID) {
            gmScript.player.isManaShieldOn = true;
        }
        StartCoroutine(SkillDuration());
    }

    IEnumerator SkillDuration() {
        yield return new  WaitForSeconds(3);
        if(gmScript.player.NetworkObjectId == playerID) { //remove buff if true
            gmScript.player.isManaShieldOn = false;
        }
        Destroy(gameObject);
    }
}
