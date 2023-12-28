using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class WarriorSkill1 : MonoBehaviour
{
    Collider2D[] targetList;
    [SerializeField] LanGameManager gmScript;
    AudioSource audioSource;
    public float finalDamage, additionalDamagePercentage = .5f, ownerID;


    private void DetectEnemy() { //.2 seconds 
        if(ownerID != gmScript.player.NetworkObjectId) return;    //0   0
        targetList = Physics2D.OverlapCircleAll(transform.position, 0.35f, 1 << 7);

        if(targetList.Length > 0) { //check if there is enemy detected
            foreach (var item in targetList)
            {
                gmScript.player.AttackServerRpc(item.transform.GetSiblingIndex(), finalDamage, gmScript.player.NetworkObjectId);
            }
        }
    }

    private void OnEnable() {
        StartCoroutine(SKillDuration()); //start counting skill duration
        audioSource = GetComponent<AudioSource>();
        finalDamage = (gmScript.player.finalDamage * additionalDamagePercentage) + 10f;

        StartCoroutine(DetectEnemyWait()); 
        transform.localPosition = new Vector3(0,0,0);
        audioSource.Play();

    }

    IEnumerator DetectEnemyWait() {   
        while (true) {
            DetectEnemy();   
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator SKillDuration() {
        yield return new WaitForSeconds(2);
        Destroy(gameObject);
    }
    
}
