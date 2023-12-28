using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;

public class LanMobsMelee : NetworkBehaviour
{

    public NetworkVariable<float> baseHealth = new(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> currentHealth = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> finalHealth = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> hasHit = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> remainingQuestions = new(5, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public float attackRange = .5f, walkSpeed = 1f, walkTime = .5f, idleTime = 0f, baseDamage = 25, finalDamage,
     attackSpeed = 1, attackCooldown = 0, elapsedTime, distance, deathTimer = 45f;

    Rigidbody2D rb;
    Vector2 currentTarget;
    public bool isWalking = false, isAttacking, isIdle = true, isDead, hasAttacked;
    Collider2D enemyCollider;
    Slider slider;
    public Transform damagePool, textBox;
    TextMeshProUGUI textBoxText;
    public Collider2D target;
    [SerializeField] LanGameManager gmScript;
    [SerializeField] LanInteractionManager interactionManager;
    GameObject npc;
    Vector3 originalPos;
    Animator anim;
    LanPlayer[] players;
    LanPlayer targetScript;
    [SerializeField] bool isBoss, isEmmanuel, isRespawnable;
    [SerializeField] string[] dialogues;
    [SerializeField] Transform wilson, ending;
    [SerializeField] LanMobsMelee emmanuel;
    bool dialogue1Done, dialogue2Done;
    //sounds
    AudioSource hitAudioSource, dieAudioSource, dialogueAudioSource;
    Transform bloodEffectParent;
    public Collider2D attackersTarget;
    [SerializeField] AudioClip[] dialogueClips;
    public override void OnNetworkSpawn()
    {
        //initialize variables
        damagePool = GameObject.FindWithTag("DamagePool").transform;
        originalPos = transform.position;
        //animators component
        anim = transform.GetChild(3).GetComponent<Animator>();
        transform.GetChild(3).GetComponent<ClientNetworkAnimator>().Animator = anim;

        hitAudioSource = transform.GetChild(5).GetChild(0).GetComponent<AudioSource>();

        slider = transform.GetChild(1).GetComponent<Slider>();
        rb = GetComponent<Rigidbody2D>();
        enemyCollider = GetComponent<BoxCollider2D>();
        bloodEffectParent = GameObject.FindWithTag("BloodEffects").transform;

        if (isBoss)
        {
            textBox = transform.GetChild(6);
            textBoxText = textBox.GetChild(0).GetComponent<TextMeshProUGUI>();
            dieAudioSource = transform.GetChild(5).GetChild(1).GetComponent<AudioSource>();
        }
        //else { //randomize enemy design
        //    int drawDesign = Random.Range(0, gmScript.MobsDesign.Length);
        //    Instantiate(gmScript.MobsDesign[drawDesign].transform.GetChild(0), transform.GetChild(3));
        //    Destroy(transform.GetChild(3).GetChild(0).gameObject);
        //}


        UpdateStats();

        currentHealth.OnValueChanged += (float oldValue, float newValue) =>
        {
            UpdateHealthBar(oldValue, newValue);
        };




        //////////////////////////////////////////////////////////OPTIMIZATIONS//////////////////////////////////////////////////////////////
        transform.GetChild(3).GetComponent<ClientNetworkAnimator>().enabled = false;
        transform.GetChild(3).GetComponent<ClientNetworkTransform>().enabled = false;
        transform.GetChild(1).gameObject.SetActive(false); //disable slider
        transform.GetChild(4).gameObject.SetActive(false); //disable
        if (isRespawnable)
        {
            GetComponent<LanMobsMelee>().enabled = false;
            anim.enabled = false;
            //transform.GetChild(3).GetChild(0).gameObject.SetActive(false);
        }
        else if (!isBoss && PlayerPrefs.GetInt("finishIntro") == 1)
        {
            gameObject.SetActive(false);
        }
        //END OPTIMIZATIONS


        if (isBoss)
        {
            //initialization for boss
            dieAudioSource = transform.GetChild(5).GetChild(1).GetComponent<AudioSource>();

            if (isEmmanuel)
            {
                dialogueAudioSource = transform.GetChild(5).GetChild(2).GetComponent<AudioSource>();
            }

            //DisableBoss();
        }
    }

    private void DisableBoss()
    {
        if (gmScript.difficulty < 3)
        {
            print("less true");
            gameObject.SetActive(false);
        }
    }

    public void UpdateStats()
    {
        if (!IsOwner) return;
        finalHealth.Value = baseHealth.Value * (gmScript.enemyStatsModifier / 100f);
        currentHealth.Value = finalHealth.Value;
        finalDamage = baseDamage * (gmScript.enemyStatsModifier / 100f);

        //set slider values
        slider.maxValue = finalHealth.Value;
        slider.value = currentHealth.Value;
    }


    private void FixedUpdate()
    {
        if (!IsHost || !IsClient) return;
        attackCooldown -= Time.deltaTime;
        idleTime += Time.deltaTime;

        if (isIdle && !isAttacking && IsOwner)
        {        //if idle and not attacking
            if (idleTime! >= 3)
            {
                if (!isWalking)
                {
                    Vector2 randomDirection = new Vector2(Random.Range(-.5f, .5f), Random.Range(-.5f, .5f));
                    currentTarget = transform.position + (Vector3)randomDirection;

                    isWalking = true;
                    elapsedTime = 0f; // Reset elapsed time
                    idleTime = Random.Range(2f, 5f);
                }
                if (elapsedTime < walkTime)
                { //start moving
                    anim.SetBool("isMoving", true);
                    rb.velocity = (currentTarget - (Vector2)transform.position) * walkSpeed;
                    //flip object
                    if (!isBoss)
                    {
                        if (rb.velocity.x < 0)
                        {
                            transform.GetChild(3).localScale = new Vector3(-.6f, .6f, 1);
                        }
                        else
                        {
                            transform.GetChild(3).localScale = new Vector3(.6f, .6f, 1);
                        }
                    }
                    else //is a boss
                    {
                        if (rb.velocity.x < 0)
                        {
                            transform.GetChild(3).localScale = new Vector3(-.1f, .1f, 1);
                        }
                        else
                        {
                            transform.GetChild(3).localScale = new Vector3(.1f, .1f, 1);
                        }
                    }
                    elapsedTime += Time.deltaTime;
                }
                else
                { //stop moving
                    anim.SetBool("isMoving", false);
                    rb.velocity = Vector2.zero;
                    isWalking = false;
                    idleTime = 0f;
                }
            }
        }
        else if (isAttacking && target != null)
        {           //if attacking and the player that attack this object is not dead
            distance = Vector2.Distance(transform.position, new Vector2(target.transform.position.x, target.transform.position.y));

            if (target != null)
            {
                Vector3 direction = target.transform.position - transform.position;

                //flip object
                if (!isBoss)
                {
                    if (direction.x < 0)
                    {
                        transform.GetChild(3).localScale = new Vector3(-.6f, .6f, 1);
                    }
                    else
                    {
                        transform.GetChild(3).localScale = new Vector3(.6f, .6f, 1);
                    }
                }
                else //is a boss
                {
                    if (direction.x < 0)
                    {
                        transform.GetChild(3).localScale = new Vector3(-.1f, .1f, 1);
                    }
                    else
                    {
                        transform.GetChild(3).localScale = new Vector3(.1f, .1f, 1);
                    }
                }
            }


            if (distance <= attackRange)
            {         //if distance of the player is less or equal attackRange, attack
                anim.SetBool("isMoving", false);
                rb.velocity = Vector2.zero;
                if (target != null && IsOwnedByServer)
                { //if health is greater than 0, attack
                    if (attackCooldown <= 0)
                    {
                        anim.Play("attack");
                        attackCooldown = 1 / attackSpeed;

                        if (target.CompareTag("Player"))
                        {
                            AttackServerRpc(finalDamage, targetScript.NetworkObjectId);
                        }
                        else //else is a knight
                        {
                            AttackServerRpc(finalDamage, target.GetComponent<LanKnights>().NetworkObjectId);
                        }
                    }

                }
            }
            else if (distance > attackRange && distance <= 5)
            { //start chasing enemy
                rb.velocity = target.transform.position - transform.position * walkSpeed;
                anim.SetBool("isMoving", true);
            }
            else
            {
                anim.SetBool("isMoving", false);
                rb.velocity = Vector2.zero;
            }

        }
        else if (isDead)
        {
            rb.velocity = Vector2.zero;
        }
        else
        {
            isAttacking = false;
            isIdle = true;
        }
    }




    [ServerRpc(RequireOwnership = false)]
    private void AttackServerRpc(float finalDamage, ulong playerID)
    {
        if (target.CompareTag("Knight"))
        {
            target.GetComponent<LanKnights>().Attacked(finalDamage); //attack knights
        }
        else
        {
            AttackServerClientRpc(finalDamage, playerID); //attack player
        }
    }

    [ClientRpc]
    void AttackServerClientRpc(float finalDamage, ulong playerID)
    {
        foreach (var item in gmScript.players)
        {
            if (item.NetworkObjectId == playerID && !item.isDead)
            {
                item.Attacked(finalDamage, playerID);
                break;
            }
        }
    }

    [ClientRpc]
    public void AttackedClientRpc(float damage, ulong networkId, int isKnight)
    {
        if (IsOwner && hasHit.Value)
        {
            SubtracthealthServerRpc(damage, networkId); // call server to subtract network variable health using ServerRpc
        }

        ulong tempId = 987;
        if (tempId != networkId)
        { //optimization - if networkID has change, call getComponent;
            tempId = networkId;
            if (isKnight == 1)
            {
                foreach (var item in gmScript.knights)
                {
                    if (item.NetworkObjectId == networkId)
                    {
                        target = item.GetComponent<Collider2D>();
                        break;
                    }
                }
            }
            else
            { //is a player
                if (!hasHit.Value)
                {
                    attackCooldown = 100;
                }
                foreach (var p in gmScript.players)
                {
                    if (p.NetworkObjectId == networkId)
                    {
                        target = p.GetComponent<Collider2D>();
                        targetScript = p.GetComponent<LanPlayer>();
                        break;
                    }
                }
            }
        }


        isIdle = false;
        isAttacking = true;


        //spawn damage pops
        Transform temp;
        temp = damagePool.GetChild(0);
        temp.GetComponent<TextMeshProUGUI>().color = Color.red;
        temp.GetComponent<TextMeshProUGUI>().SetText(((int)damage).ToString());
        temp.SetParent(transform);
        temp.gameObject.SetActive(true);


        if (currentHealth.Value <= 0 && !isBoss) //MOBS
        {
            isDead = true;
            enemyCollider.enabled = false;
            isAttacking = false;
            isIdle = false;
            gmScript.player.GiveRewardsServerRpc(25f, false);
            if (networkId == gmScript.player.NetworkObjectId)
            {
                gmScript.player.npc = gameObject;
                //interactionManager.gameObject.SetActive(true);
            }
            StartCoroutine(DisableEnemy());
            anim.Play("die");
            //gameObject.SetActive(false);
        }
        else if (currentHealth.Value <= 0 && isBoss) //BOSS
        {
            isDead = true;
            enemyCollider.enabled = false;
            isAttacking = false;
            anim.SetBool("isMoving", false);
            anim.Play("die");

            if (isEmmanuel)
            {
                dieAudioSource.Play();
            }
        }
        else
        {
            //play hit sound
            hitAudioSource.Play();
        }

    }

    IEnumerator DisableEnemy()
    {
        isAttacking = false;
        isIdle = false;
        isDead = true;
        yield return new WaitForSeconds(.5f);
        gameObject.SetActive(false);
    }
    private void OnDisable()
    {
        attackCooldown = 1 / attackSpeed;
        SetHasHitServerRpc(false);
        //hasHit = false;
        target = null;
        if (isRespawnable)
        { //if false, dont respawn
            Invoke(nameof(RespawnWait), deathTimer);
        }

    }

    void RespawnWait()
    {
        isDead = false;
        gameObject.SetActive(true);
        enemyCollider.enabled = true;
        target = null;


        if (!IsOwner) return;
        //reset stats
        currentHealth.Value = finalHealth.Value; //server only
        transform.position = originalPos; //server only
    }


    public void UpdateHealthBar(float oldValue, float newValue)
    {
        slider.maxValue = finalHealth.Value;
        slider.value = newValue;

        if (newValue < oldValue)
        {
            Transform bloodEffectTemp = bloodEffectParent.GetChild(0);
            bloodEffectTemp.SetParent(transform);
            bloodEffectTemp.gameObject.SetActive(true);

        }



        if (isBoss && hasHit.Value == true)
        {
            if (hasAttacked == false && isEmmanuel)
            {
                dialogueAudioSource.clip = dialogueClips[0];
                dialogueAudioSource.Play();
                StartCoroutine(PlayEmmanuelEvilDialogue());
                hasAttacked = true;
                //attackCooldown = 1 / attackSpeed;
            }
            else if (newValue < (finalHealth.Value * .85) && newValue > (finalHealth.Value * .55) && isEmmanuel && !dialogue1Done) //boss 65-70% 
            {
                dialogueAudioSource.clip = dialogueClips[1];
                dialogueAudioSource.Play();
                StartCoroutine(PlayEmmanuelEvilDialogue1());
                dialogue1Done = true;
            }
            else if (newValue < (finalHealth.Value * .50) && newValue > (finalHealth.Value * .35) && isEmmanuel && !dialogue2Done) //trigger on 45-50% hp //teleport wilson here
            {
                dialogueAudioSource.clip = dialogueClips[2];
                dialogueAudioSource.Play();


                wilson.transform.position = new(transform.position.x + .5f, transform.position.y);
                var wilsonScript = wilson.GetComponent<LanMobsMelee>();
                wilsonScript.hasAttacked = true;
                //wilsonScript.SetHasHitServerRpc(true);
                wilsonScript.SetTarget(target);
                wilsonScript.isAttacking = true;


                StartCoroutine(PlayEmmanuelEvilDialogue2());
                dialogue2Done = true;
            }
            else if (newValue <= 0 && isEmmanuel)
            {
                StartCoroutine(PlayEmmanuelEvilDialogue3());
            }
        }
    }


    //call server to subtract
    [ServerRpc(RequireOwnership = false)]
    public void SubtracthealthServerRpc(float damage, ulong playerID)
    {
        currentHealth.Value -= damage;  //currentHealth.Value = currentHealth.Value - damage
        //play animation
        anim.Play("Hit");
    }




    //blood effect
    /*
    [ServerRpc(RequireOwnership = false)] public void SpawnBloodEffectServerRpc() {
        gmScript.player.SpawnBloodEffectServerRpc(NetworkObjectId);
    } */

    [ServerRpc(RequireOwnership = false)]
    public void HealServerRpc(float healAmount)
    {
        currentHealth.Value += healAmount;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetHasHitServerRpc(bool state)
    {
        if (!IsOwner) return;
        hasHit.Value = state;
    }

    //////////////////DIALOGUE//////////////////////////////////////////////////////////////
    IEnumerator PlayEmmanuelEvilDialogue()
    {
        textBox.gameObject.SetActive(true);
        string dialogue0 = "Thus, you've come to deal with me, " + gmScript.player.username + "?" + " What a shame that your efforts were useless.";
        foreach (var item in dialogue0) //
        {
            textBoxText.text += item;
            yield return new WaitForSeconds(0.025f);
        }
        yield return new WaitForSeconds(4f);
        textBox.gameObject.SetActive(false);
        textBoxText.text = null;
    }

    IEnumerator PlayEmmanuelEvilDialogue1()
    {
        textBox.gameObject.SetActive(true);
        foreach (var item in dialogues[1])
        {
            textBoxText.text += item;
            yield return new WaitForSeconds(0.025f);
        }
        yield return new WaitForSeconds(4f);
        textBox.gameObject.SetActive(false);
        textBoxText.text = null;
    }

    IEnumerator PlayEmmanuelEvilDialogue2()
    { //wilson the evil teleported
        textBox.gameObject.SetActive(true);
        foreach (var item in dialogues[2])
        {
            textBoxText.text += item;
            yield return new WaitForSeconds(0.025f);
        }
        yield return new WaitForSeconds(3f);
        textBox.gameObject.SetActive(false);
        textBoxText.text = null;
    }

    IEnumerator PlayEmmanuelEvilDialogue3()  //You have demonstrated
    {
        yield return new WaitForSeconds(1f);
        dialogueAudioSource.clip = dialogueClips[3];
        dialogueAudioSource.Play();

        textBox.gameObject.SetActive(true);
        foreach (var item in dialogues[3])
        {
            textBoxText.text += item;
            yield return new WaitForSeconds(0.025f);
        }
        yield return new WaitForSeconds(2f);
        textBox.gameObject.SetActive(false);
        textBoxText.text = null;
        StartCoroutine(PlayEmmanuelEvilDialogue4());
    }

    IEnumerator PlayEmmanuelEvilDialogue4()
    { //you have won
        string dialogeTemp = "You, " + gmScript.player.username + ", have won.";
        yield return new WaitForSeconds(1f);

        textBox.gameObject.SetActive(true);
        foreach (var item in dialogeTemp)
        {
            textBoxText.text += item;
            yield return new WaitForSeconds(0.025f);
        }
        yield return new WaitForSeconds(1.5f);
        textBox.gameObject.SetActive(false);
        textBoxText.text = null;

        //START ENDING
        gmScript.player.StartEndingServerRpc();
    }

    public void AttackOnce(float damage)
    {
        anim.Play("attack");
        AttackServerRpc(damage, targetScript.NetworkObjectId);
    }


    public bool GetIsBoss()
    {
        return isBoss;
    }

    public bool GetIsEmmanuel()
    {
        return isEmmanuel;
    }

    public void SetTarget(Collider2D target)
    {
        targetScript = target.GetComponent<LanPlayer>();
        this.target = target;
    }

    public void SubtractRemainingQuestions()
    {
        remainingQuestions.Value--;

        if (remainingQuestions.Value == 0)
        {
            attackCooldown = 1 / attackSpeed;
            hasHit.Value = true;
        }
    }



}

