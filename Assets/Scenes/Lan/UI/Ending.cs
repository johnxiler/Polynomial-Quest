using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Ending : MonoBehaviour
{
    [SerializeField]
    Transform WilsonTransform, emmanuelTransform, controls, playerInforBar, minimap, missionPanel, mapParent, effect104, ultimateWeapon,
    ultimateWeaponSS, itemPool, endingFade, returnButton, welcomePanel;
    [SerializeField] TextMeshProUGUI wilsonText, emmanuelText, endingFadeText;
    [SerializeField] AudioClip[] wilsonSound, emmanuelSound;
    [SerializeField] AudioSource endingAudioSource;
    Vector2 wilsonPosition;
    [SerializeField] LanGameManager gmScript;
    [SerializeField] Lan lan;
    LanPlayer player;
    [SerializeField] EndingRpc endingRpc;
    Animator emmanuelAnim;
    Rigidbody2D emmanuelRb;
    string dialogue1;
    float elapseTime, moveDuration = 1;
    private void Start()
    {
        wilsonPosition = new(0.588f, -0.072f);
        dialogue1 = "Thank you!! Brave one. The darkness among us has been extinguished by your bravery, determination, and mastery of polynomials.";

        emmanuelAnim = emmanuelTransform.GetComponent<Animator>();
        emmanuelRb = emmanuelTransform.GetComponent<Rigidbody2D>();

        gmScript.ChangeBackgroundMusic(3); //3 for victory bg music
        player = gmScript.player;

    }

    void TeleportPlayersEvent()
    {

        //change map
        mapParent.GetChild(1).gameObject.SetActive(false); //disablel extrreme
        mapParent.GetChild(2).gameObject.SetActive(true); //enable forest


        if (player.IsOwnedByServer)
        {
            player.SetPlayerPositionZeroServerRpc();
        }


        //after teleporting the players, set the lighting to normal
        gmScript.difficulty = 0;
        gmScript.SetLighting();

        //stop weather just in case
        gmScript.StopAcid();
        gmScript.isInsideDungeon = true; //to prevent weather debuffs

        WilsonTransform.position = wilsonPosition;
        wilsonText.transform.parent.gameObject.SetActive(true);

        controls.gameObject.SetActive(false);
        playerInforBar.gameObject.SetActive(false);
        minimap.gameObject.SetActive(false);
        missionPanel.gameObject.SetActive(false);

        // StartCoroutine(WilsonDialogue1());
        if (player.IsOwnedByServer)
        {
            endingRpc.StartDialogue1ServerRpc();
        }
    }

    public IEnumerator WilsonDialogue1()
    {
        //play audio
        endingAudioSource.clip = wilsonSound[0];
        endingAudioSource.Play();


        wilsonText.text = null;
        foreach (var item in dialogue1)
        {
            wilsonText.text += item;
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(1.5f);
        wilsonText.text = null;
        wilsonText.transform.parent.gameObject.SetActive(false); //disable textbox


        // StartCoroutine(EmmanuelDialogue1());
        if (player.IsOwnedByServer)
        {
            endingRpc.StartDialogue2ServerRpc();
        }
    }

    public IEnumerator EmmanuelDialogue1()
    {
        Vector3 position = new(0.205f, 0.143f); //0-1
        Vector2 direction = (position - emmanuelRb.transform.position).normalized * .5f;

        while (elapseTime <= moveDuration)
        { //0   //1
            emmanuelAnim.SetBool("isMoving", true);  //running
            elapseTime += Time.fixedDeltaTime;
            emmanuelRb.MovePosition(emmanuelRb.position + direction * Time.deltaTime);
            yield return new WaitForFixedUpdate();
        }
        emmanuelAnim.SetBool("isMoving", false);  //running
        emmanuelText.transform.parent.gameObject.SetActive(true);


        //play audio
        endingAudioSource.clip = emmanuelSound[0];
        endingAudioSource.Play();

        //start text effect
        string emmanuelDialogue1 = "Witness, " + gmScript.player.username + ", the outcome of your bravery.";
        string emmanuelDialogue2 = "You have rebuilt our sanctuary, brought back the lost artifacts, and given these walls new life through your strong commitment.";
        foreach (var item in emmanuelDialogue1)
        {
            emmanuelText.text += item;
            yield return new WaitForSeconds(0.05f);
        }
        yield return new WaitForSeconds(3f);



        //text 2
        emmanuelText.text = null;
        foreach (var item in emmanuelDialogue2)
        {
            emmanuelText.text += item;
            yield return new WaitForSeconds(0.05f);
        }
        yield return new WaitForSeconds(1.5f);



        emmanuelText.text = null;
        emmanuelText.transform.parent.gameObject.SetActive(false);

        // StartCoroutine(WilsonDialogue2());
        if (player.IsOwnedByServer)
        {
            endingRpc.StartDialogue3ServerRpc();
        }
    }

    public IEnumerator WilsonDialogue2()
    {
        //play audio
        endingAudioSource.clip = wilsonSound[1];
        endingAudioSource.Play();


        wilsonText.transform.parent.gameObject.SetActive(true); //enable textbox

        string WilsonDialogue2 = "As evidence of your incredible journey, the castle is still standing. We want to express our thanks by giving you the best gifts possible.";
        foreach (var item in WilsonDialogue2)
        {
            wilsonText.text += item;
            yield return new WaitForSeconds(0.05f);
        }
        yield return new WaitForSeconds(1.5f);
        wilsonText.text = null;

        string WilsonDialogue3 = "equipment with outstanding power.";
        foreach (var item in WilsonDialogue3)
        {
            wilsonText.text += item;
            yield return new WaitForSeconds(0.05f);
        }
        yield return new WaitForSeconds(1.5f);
        wilsonText.text = null;
        wilsonText.transform.parent.gameObject.SetActive(false); //disable textbox

        //SHOW ITEMM!!!!!!!!
        effect104.gameObject.SetActive(true);
        ultimateWeapon.gameObject.SetActive(true);

        yield return new WaitForSeconds(1.5f);
        gmScript.player.inventory[29] = "0"; //delete the item at index 29 just incase the inventory is full

        //host only
        if (player.IsHost)
        {
            player.GiveRewardsServerRpc(500f, true);
        }
        //equip item client and host
        player.EquipItemServerRpc(36, gmScript.player.NetworkObjectId, true);

        // StartCoroutine(EmmanuelDialogue2());
        if (player.IsOwnedByServer)
        {
            endingRpc.StartDialogue4ServerRpc();
        }
    }

    public IEnumerator EmmanuelDialogue2()
    {
        //play audio
        endingAudioSource.clip = emmanuelSound[1];
        endingAudioSource.Play();


        emmanuelText.transform.parent.gameObject.SetActive(true);
        string emmanuelDialogue4 = "With the help of these treasures, you will continue to promote knowledge and overcome obstacles";
        string emmanuelDialogue5 = "Your name will always be remembered in the Castle of Wisdom. " + gmScript.player.username;

        //text 4 start
        foreach (var item in emmanuelDialogue4)
        {
            emmanuelText.text += item;
            yield return new WaitForSeconds(0.05f);
        }
        yield return new WaitForSeconds(1.5f);
        emmanuelText.text = null;


        //play audio
        endingAudioSource.clip = emmanuelSound[2];
        endingAudioSource.Play();
        //text 5 start
        foreach (var item in emmanuelDialogue5)
        {
            emmanuelText.text += item;
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(3f);
        emmanuelText.text = null;
        emmanuelText.transform.parent.gameObject.SetActive(false);

        // StartCoroutine(WilsonDialogue3());
        if (player.IsOwnedByServer)
        {
            endingRpc.StartDialogue5ServerRpc();
        }
    }

    public IEnumerator WilsonDialogue3()
    {
        //play audio
        endingAudioSource.clip = wilsonSound[2];
        endingAudioSource.Play();


        wilsonText.transform.parent.gameObject.SetActive(true); //enable textbox
        string WilsonDialogue4 = "Goodbye, legendary hero. Through the years, your legacy will always be appreciated.";
        foreach (var item in WilsonDialogue4)
        {
            wilsonText.text += item;
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(1.5f);
        wilsonText.text = null;
        wilsonText.transform.parent.gameObject.SetActive(false);
        endingFade.gameObject.SetActive(true);


        //ending text
        string endingText = "Congratulations, " + gmScript.player.username + ", on completing the Polynomial Quest, the castle has been fully restored! Math Genius!";
        foreach (var item in endingText)
        {
            endingFadeText.text += item;
            yield return new WaitForSeconds(0.05f);
        }

        //show return button
        returnButton.gameObject.SetActive(true);



    }

    public void ReturnMenu()
    {
        lan.BackToSelection();

        transform.parent.GetChild(1).gameObject.SetActive(false); //disable last message
        transform.parent.gameObject.SetActive(false); //disable ending object
    }

}
