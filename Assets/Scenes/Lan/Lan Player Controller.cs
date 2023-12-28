using System.Collections;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using UnityEngine.UI;
using System.Linq;
using Unity.Collections;

public class LanPlayer : NetworkBehaviour
{
    public FixedJoystick joystick;
    public Animator anim;
    public GameObject npc, deathPanel, mobsParent;
    public LanGameManager gmScript;
    public Transform damagePool, dungeonParent;
    public Transform bloodEffectsParent, skillEffectsParent, maps, ui;
    Transform itemsPool, inventoryPanel, reward, itemsPoolWS, rewardsLabelPool;

    //player customizations network variables

    public NetworkVariable<int> belt = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> boots = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> elbow = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> face = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> hood = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> legs = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> shoulder = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> torso = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> wrist = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public NetworkVariable<float> baseHealth = new(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> finalHealth = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> currentHealth = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> score = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> level = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<FixedString128Bytes> playerName = new("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public string[] inventory = { "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0" };

    public float moveSpeed = 1f, attackSpeed = 1, attackCooldown = 0, baseDamage = 25, finalDamage, currentExp,
    baseRequiredExp = 75, finalRequiredExp, currentMana, baseMana = 75, finalMana,
    potion, weaponDmg, baseArmor = 5, finalArmor, itemArmor, equipedWeaponIndex, equipedArmorIndex, deathTimer = 15f,
    damageReduction, attackRange, hint = 10, finishIntro, hasStatsInitialized, weaponIndexAtInventory, armorIndexAtInventory;
    public string username, playerClass;
    public Collider2D[] targetList;
    public Slider sliderHealthWS;
    Rigidbody2D rb;
    Collider2D playerCollider;
    Vector2 targetDirection;
    TextMeshProUGUI deathTimerText;
    LanAttackButton attackButton;
    Image cooldownImage;
    public bool isDead, isUsingSkill, isManaShieldOn, isDoneTraining;
    AudioSource audioSource, hitAudioSource, dieAudioSource;
    [SerializeField] AudioClip[] WarriorSoundEffects;
    [SerializeField] AudioClip[] MageSoundEffects;

    public override void OnNetworkSpawn()
    {
        if (!IsLocalPlayer)
        {
            transform.GetChild(6).gameObject.SetActive(false);
            GetComponent<AudioListener>().enabled = false; //disable audioListener if not local player
        }

        gmScript = GameObject.FindWithTag("GameManager").GetComponent<LanGameManager>();
        sliderHealthWS = transform.GetChild(1).GetChild(0).GetComponent<Slider>();
        StartCoroutine(UpdateName());
    }

    IEnumerator UpdateName() //wait for network variables to load then do action
    {
        while (string.IsNullOrEmpty(playerName.Value.ToString()) && wrist.Value == 0)
        {
            yield return null; // wait until next frame
        }

        // execute set name
        transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().SetText(playerName.Value + "   Lv. " + level.Value);

        //execute setCostumization 
        SetCostumization();
        SubscribeHealthBars();

        if (IsLocalPlayer)
        {
            Debug.Log("Player Spawned");
            gmScript.PopulateInventory();
        }
        else
        {
            Debug.Log("Player: " + playerName.Value + " joined.");
        }
    }

    void Start()
    {
        //rename player object for easy debugging
        if (IsOwnedByServer)
        {
            gameObject.name = "Player: Host";
        }
        else
        {
            gameObject.name = "Player: Client";
        }
        mobsParent = GameObject.FindWithTag("EnemyManager").transform.GetChild(0).gameObject;
        bloodEffectsParent = GameObject.FindWithTag("BloodEffects").transform;
        skillEffectsParent = GameObject.FindWithTag("SkillEffects").transform;
        maps = GameObject.FindWithTag("Maps").transform;
        anim = transform.GetChild(0).GetComponent<Animator>(); //for animations
        ui = GameObject.FindWithTag("UI").transform;
        itemsPool = ui.GetChild(4).GetChild(1);
        itemsPoolWS = GameObject.FindWithTag("ItemsPoolWS").transform;
        rewardsLabelPool = GameObject.FindWithTag("RewardsLabelPool").transform;
        damagePool = GameObject.FindWithTag("DamagePool").transform; // for damage pops

        //initialize healthbar in world space


        // if (!IsLocalPlayer)
        // {
        //     transform.GetChild(6).gameObject.SetActive(false);
        //     GetComponent<AudioListener>().enabled = false; //disable audioListener if not local player
        // }
        //check if is owner if not, return;
        if (!IsOwner) return;



        //assign network variables values from PlayerPrefs
        belt.Value = PlayerPrefs.GetInt("belt");
        boots.Value = PlayerPrefs.GetInt("boots");
        elbow.Value = PlayerPrefs.GetInt("elbow");
        face.Value = PlayerPrefs.GetInt("face");
        hood.Value = PlayerPrefs.GetInt("hood");
        legs.Value = PlayerPrefs.GetInt("legs");
        shoulder.Value = PlayerPrefs.GetInt("shoulder");
        torso.Value = PlayerPrefs.GetInt("torso");
        wrist.Value = PlayerPrefs.GetInt("wrist");


        transform.GetChild(0).GetComponent<ClientNetworkAnimator>().Animator = anim; //to sync animations across clients
        //transform.parent.GetComponent<LanCameraController>().player = gameObject;
        joystick = GameObject.FindWithTag("UI").transform.GetChild(2).GetChild(0).GetComponent<FixedJoystick>(); //joystick for movement
        //GameObject.FindWithTag("UI").transform.GetChild(2).GetChild(1).GetComponent<LanAttackButton>().player = GetComponent<LanPlayer>();
        rb = GetComponent<Rigidbody2D>();
        deathPanel = GameObject.FindWithTag("UI").transform.GetChild(5).GetChild(3).gameObject;
        cooldownImage = GameObject.FindWithTag("Controls").transform.GetChild(1).GetChild(0).GetComponent<Image>();
        audioSource = GetComponent<AudioSource>();
        inventoryPanel = GameObject.FindWithTag("UI").transform.GetChild(4).GetChild(0);
        hitAudioSource = transform.GetChild(7).GetComponent<AudioSource>();
        dieAudioSource = transform.GetChild(8).GetComponent<AudioSource>();
        hitAudioSource.clip = gmScript.playerHitSoundEffect;
        dieAudioSource.clip = gmScript.playerDieSoundEffect;
        dungeonParent = GameObject.FindWithTag("Environment").transform.GetChild(2).transform;
        deathTimerText = deathPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        playerCollider = GetComponent<BoxCollider2D>();
        StartCoroutine(DetectEnemyWait());



        //initialize GameManager
        gmScript.Initialize();

        //updateStats();




        //call server to call PlayerCustomizationClientRpc method on all client
        //PlayerCustomizationServerRpc(); //load costomization
        UpdatePlayerListInfoServerRpc();
        //HOST = server and player ///network objects OWNER
        //CLIENT = players
        if (!IsOwnedByServer)
        { //executed except on HOST
            GetDifficultyServerRpc();  //get difficulty
        }

        StartCoroutine(ManaRegen()); //start mana regen, will regen 5% of max mana every .5s

        //basic attack sound
        switch (playerClass)
        {
            case "Warrior":
                audioSource.clip = gmScript.WarriorSoundEffects[0];
                break;
        }





        if (!IsOwnedByServer) return; //codes below is executed on server only ///////////////////
        RandomWeatherServerRpc(); //weather
        gmScript.SetLighting();
    }

    IEnumerator DetectEnemyWait()
    {
        while (true)
        {
            DetectEnemy();
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void UpdateStats()
    {
        if (currentExp != 0 && currentExp >= finalRequiredExp && level.Value <= 30)  //called on level up
        {
            gmScript.levelUp.Play();


            currentExp = 0; //reset current Exp
            level.Value++;

            gmScript.SavePlayerData(); //save data
            gmScript.UpdateUI();
        }

        //exp
        baseRequiredExp = 75 + .20f * level.Value;
        finalRequiredExp = baseRequiredExp;

        //damage
        baseDamage = 25 * Mathf.Pow(1.05f, level.Value);
        finalDamage = baseDamage + weaponDmg;

        //armor
        finalArmor = baseArmor + itemArmor;

        //heatl
        baseHealth.Value = 100 + 50 * level.Value;
        finalHealth.Value = baseHealth.Value;

        sliderHealthWS.maxValue = finalHealth.Value;
        sliderHealthWS.value = currentHealth.Value;
        gmScript.UpdateUI();
    }

    void FixedUpdate()
    {

        //check if owner
        if (!IsOwner) return;
        if (isDead)
        {
            deathTimer -= Time.fixedDeltaTime;
            deathTimerText.SetText(deathTimer.ToString("F2"));
        }
        else
        {
            attackCooldown -= Time.fixedDeltaTime;
            if (attackCooldown >= 0)
            {
                cooldownImage.gameObject.SetActive(true); //enable cooldown image
                cooldownImage.fillAmount = (attackCooldown - 0) / (attackSpeed - 0);
            }

            if (!isUsingSkill)
            {
                Vector2 movement = new Vector2(joystick.Horizontal, joystick.Vertical) * moveSpeed;
                rb.MovePosition(rb.position + movement * Time.fixedDeltaTime);
            }



            //flip object
            if (joystick.Horizontal < 0)
            {
                transform.GetChild(0).localScale = new Vector3(-0.025f, 0.025f, 0);
            }
            else if (joystick.Horizontal > 0)
            {
                transform.GetChild(0).localScale = new Vector3(0.025f, 0.025f, 0);
            }


            //animation here
            if (joystick.Horizontal < 0 || joystick.Horizontal > 0 || joystick.Vertical < 0 || joystick.Vertical > 0)
            {
                anim.SetBool("isIdle", false);
                anim.SetBool("isRun", true);
            }
            else
            {
                anim.SetBool("isRun", false);
                anim.SetBool("isIdle", true);
            }
        }
    }



    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    void DetectEnemy()
    {
        targetList = Physics2D.OverlapCircleAll(transform.position, attackRange, 1 << 7);
        if (targetList.Length > 0)
        {
            targetDirection = (targetList[0].transform.position - transform.GetChild(0).position).normalized;
        }


    }

    public void EventAttack()
    {
        if (targetList.Length > 0)
        {
            int targetIndex = targetList[0].transform.GetSiblingIndex();
            var target = targetList[0].GetComponent<LanMobsMelee>();

            if (!target.hasHit.Value && !isDead)
            {
                npc = target.gameObject;
                AttackServerRpc(targetIndex, 0, NetworkObjectId);
                if (!target.GetIsBoss())
                {
                    target.SetHasHitServerRpc(true);
                }
                //target.hasHit = true;
                target.target = playerCollider;
                target.isAttacking = true;
                gmScript.InteractionManager.gameObject.SetActive(true);

            }
            else
            {
                AttackServerRpc(targetIndex, finalDamage, NetworkObjectId);
            }
        }
    }


    public void Attack()
    {
        switch (playerClass)
        {

            case "Warrior":
                if (targetDirection.x < 0)
                { //attack anim up
                    transform.GetChild(0).localScale = new Vector3(-0.025f, 0.025f, 0);    //face the enemy before attacking
                    anim.Play("Attack1");

                    audioSource.Play();
                }
                else if (targetDirection.x > 0)
                { //attack anim left
                    transform.GetChild(0).localScale = new Vector3(0.025f, 0.025f, 0);    //face the enemy before attacking
                    //play animation
                    anim.Play("Attack1");
                }
                break;



            case "Mage":
                Vector2 targetPosition = targetList[0].transform.position - transform.position; //get enemy position
                SpawnMagicBulletServerRpc(targetPosition, NetworkObjectId);

                break;

            case "Assassin":
                if (targetDirection.x < 0)
                {
                    transform.GetChild(0).localScale = new Vector3(-0.025f, 0.025f, 0);    //face the enemy before attacking
                    anim.Play("Attack1");
                }
                else if (targetDirection.x > 0)
                {
                    transform.GetChild(0).localScale = new Vector3(0.025f, 0.025f, 0);    //face the enemy before attacking
                    anim.Play("Attack1"); //play animation
                }
                break;
        }
    }


    public void Attacked(float damage, ulong playerID)
    { //damage = 10
        if (NetworkObjectId == playerID && IsLocalPlayer)
        {


            anim.Play("Hit");
            hitAudioSource.Play();





            if (isManaShieldOn)
            {
                float tempDamage = damage * .30f; //expample: damage = 10, 30% is = 3
                currentMana -= tempDamage;
                currentHealth.Value -= damage - ((finalArmor * .5f) / 100) * damage - tempDamage;
            }
            else
            {
                currentHealth.Value -= damage - ((finalArmor * .5f) / 100) * damage;
            }
            gmScript.UpdateUI();





            if (currentHealth.Value <= 0)
            {
                //dieAudioSource.Play();
                isDead = true;
                anim.Play("Death");
                playerCollider.enabled = false;
                deathPanel.SetActive(true); //show Died message
                gmScript.InteractionManager.gameObject.SetActive(false);
                if (level.Value > 1)
                {
                    currentExp = 0;
                    level.Value -= 1;
                }
                if (inventoryPanel.childCount > 0)
                { //wipe inventory when player die
                    for (int i = 0; i < inventoryPanel.childCount; i++)
                    {
                        Destroy(inventoryPanel.GetChild(i).gameObject);
                    }
                    inventory = Enumerable.Repeat("0", 30).ToArray();
                    equipedWeaponIndex = 0;
                    weaponIndexAtInventory = 0;
                    equipedArmorIndex = 0;
                    armorIndexAtInventory = 0;
                    weaponDmg = 0;
                    itemArmor = 0;
                    EquipItemServerRpc(0, NetworkObjectId, true); //for weapon
                    EquipItemServerRpc(0, NetworkObjectId, false); //for armor
                }
                UpdateStats();
                gmScript.SavePlayerData();
                StartCoroutine(playerRespawnWait());
            }
        }

    }

    IEnumerator playerRespawnWait()
    {
        yield return new WaitForSeconds(15);
        deathTimer = 15f;
        isDead = false;
        anim.SetBool("isDead", false);
        playerCollider.enabled = true;

        if (finishIntro == 1)
        {
            if (gmScript.isInsideDungeon)
            {
                transform.position = gmScript.GetDungeonSpawnLocation();
            }
            else
            {
                transform.position = Vector3.zero;
            }
        }
        else
        {
            transform.position = new(-39.317f, -29.308f); //then go to training ground
        }

        //set Stats
        currentHealth.Value = finalHealth.Value;
        currentMana = finalMana;
        currentExp -= currentExp * .20f;
        deathPanel.SetActive(false);
        gmScript.UpdateUI();
    }



    public void UpdateHealthBar(float oldValue, float newValue)
    {
        sliderHealthWS.value = finalHealth.Value;
        sliderHealthWS.value = newValue;

        if (newValue < oldValue)
        { //less than means it recieve damage
            Transform bloodEffectTemp = bloodEffectsParent.GetChild(0);
            bloodEffectTemp.SetParent(transform.GetChild(3));
            bloodEffectTemp.gameObject.SetActive(true);

            if (IsLocalPlayer)
            {
                Transform temp;
                temp = damagePool.GetChild(0);
                temp.GetComponent<TextMeshProUGUI>().color = Color.red;
                temp.GetComponent<TextMeshProUGUI>().SetText(((int)(oldValue - newValue)).ToString());
                temp.SetParent(transform.GetChild(1));
                temp.gameObject.SetActive(true);
            }
        }
    }


    void SetCostumization()
    {
        //belt
        transform.GetChild(0).GetChild(0).GetChild(3).GetComponent<SpriteRenderer>().sprite = gmScript.characterCreation.belt[belt.Value];

        //boots
        transform.GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite = gmScript.characterCreation.boots_l[boots.Value];
        transform.GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite = gmScript.characterCreation.boots_r[boots.Value];

        //elbow
        transform.GetChild(0).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetChild(1).GetComponent<SpriteRenderer>().sprite = gmScript.characterCreation.elbow_l[elbow.Value];
        transform.GetChild(0).GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(1).GetComponent<SpriteRenderer>().sprite = gmScript.characterCreation.elbow_r[elbow.Value];

        //face
        transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(1).GetComponent<SpriteRenderer>().sprite = gmScript.characterCreation.face[face.Value];

        //hood
        transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(2).GetComponent<SpriteRenderer>().sprite = gmScript.characterCreation.hood[hood.Value];

        //legs
        transform.GetChild(0).GetChild(0).GetChild(1).GetChild(1).GetComponent<SpriteRenderer>().sprite = gmScript.characterCreation.legs_l[legs.Value];
        transform.GetChild(0).GetChild(0).GetChild(2).GetChild(1).GetComponent<SpriteRenderer>().sprite = gmScript.characterCreation.legs_r[legs.Value];

        //shoulder
        transform.GetChild(0).GetChild(0).GetChild(0).GetChild(1).GetChild(1).GetComponent<SpriteRenderer>().sprite = gmScript.characterCreation.shoulder_l[shoulder.Value];
        transform.GetChild(0).GetChild(0).GetChild(0).GetChild(2).GetChild(1).GetComponent<SpriteRenderer>().sprite = gmScript.characterCreation.shoulder_r[shoulder.Value];

        //Torso
        transform.GetChild(0).GetChild(0).GetChild(0).GetChild(3).GetComponent<SpriteRenderer>().sprite = gmScript.characterCreation.torso[torso.Value];

        //torso
        transform.GetChild(0).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite = gmScript.characterCreation.wrist_l[wrist.Value];
        transform.GetChild(0).GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite = gmScript.characterCreation.wrist_r[wrist.Value];

    }

    public void SubscribeHealthBars()
    {
        sliderHealthWS.maxValue = finalHealth.Value;
        sliderHealthWS.value = currentHealth.Value;

        currentHealth.OnValueChanged += (float oldValue, float newValue) =>
        {
            UpdateHealthBar(oldValue, newValue);
        };

        level.OnValueChanged += (float oldValue, float newValue) =>
        {
            UpdateLevelBar(newValue);
        };
    }

    public void UpdateLevelBar(float newValue)
    {
        transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().SetText(playerName.Value + "   Lv. " + newValue);
    }
    //////////////////////////////////////////////////////////////////////NETWORKING///////////////////////////////////////////////////////////

    [ServerRpc(RequireOwnership = false)]
    public void SubtractHealthServerRpc(float damage)
    {
        currentHealth.Value -= (damage - (finalArmor * .5f));
    }


    [ServerRpc(RequireOwnership = false)]
    public void AttackServerRpc(int targetIndex, float finalDamage, ulong networkId)
    { //

        int temp = 100;
        LanMobsMelee targetObject = null; ;
        if (temp != targetIndex)
        {   //executed once, then procced to else
            targetObject = mobsParent.transform.GetChild(targetIndex).GetComponent<LanMobsMelee>();
            temp = targetIndex;
            targetObject.AttackedClientRpc(finalDamage, networkId, 0);
        }
        else
        {
            targetObject.AttackedClientRpc(finalDamage, networkId, 0);
        }
    }


    //get difficulty from server
    [ServerRpc(RequireOwnership = false)]
    public void GetDifficultyServerRpc()
    {
        SetDifficultyClientRpc(gmScript.difficulty);
    }
    [ClientRpc]
    public void SetDifficultyClientRpc(int difficulty) ///executed on players except host 
    {
        gmScript.difficulty = difficulty;
        gmScript.hasSetDifficultyForClient = true;
        gmScript.InstantiateMaps();
        gmScript.StartBackgroundMusic();
        gmScript.SetLighting();
        skillEffectsParent.GetChild(2).GetChild(1).gameObject.SetActive(true); //enable portal

    }





    //load customization
    [ServerRpc(RequireOwnership = false)]
    public void PlayerCustomizationServerRpc()
    {
        PlayerCustomizationClientRpc();
    }
    [ClientRpc]
    public void PlayerCustomizationClientRpc()
    {
        gmScript.LoadPlayerCostumization();
    }



    //weather
    [ServerRpc(RequireOwnership = false)]
    public void RandomWeatherServerRpc()
    {
        if (IsOwnedByServer && !gmScript.isInsideDungeon)
        {
            int draw = Random.Range(0, 2);
            if (draw == 1)
            {
                StartWeatherClientRpc(gmScript.difficulty);
            }
            else if (draw == 0)
            {
                StartCoroutine(gmScript.RedrawWeather());
            }
        }
    }

    [ClientRpc]
    public void StartWeatherClientRpc(int difficulty)
    {
        switch (difficulty)
        {
            case 0:
                gmScript.RainWeather();
                break;

            case 1:
                gmScript.DesertWeather();
                break;

            case 2:
                gmScript.SnowWeather();
                break;

            case 3:
                gmScript.AcidWeather();
                break;

        }

    }




    [ServerRpc(RequireOwnership = false)]
    public void SpawnMagicBulletServerRpc(Vector2 targetPosition, ulong playerID)
    {
        SpawnMagicBulletClientRpc(targetPosition, playerID);
    }
    [ClientRpc]
    public void SpawnMagicBulletClientRpc(Vector2 enemyPosition, ulong playerID)
    {
        foreach (var item in gmScript.players)
        {
            if (item.NetworkObjectId == playerID)
            {
                Transform temp = Instantiate(skillEffectsParent.GetChild(1).GetChild(0).GetChild(0), skillEffectsParent.GetChild(4));
                MagicBullet tempScript = temp.GetComponent<MagicBullet>();

                tempScript.direction = enemyPosition;
                tempScript.playerID = playerID;
                temp.localPosition = item.transform.position;
                //temp.SetParent(skillEffectsParent.GetChild(1).GetChild(1)); //move to in Use object
                temp.gameObject.SetActive(true);
            }
        }
    }





    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayerListInfoServerRpc()
    {
        UpdatePlayerListInfoClientRpc();
    }
    [ClientRpc]
    public void UpdatePlayerListInfoClientRpc()
    {
        //Debug.Log("updated player info");
        //find all player scripts
        LanPlayer[] temp = FindObjectsOfType<LanPlayer>();
        gmScript.players = temp;

        foreach (var item in temp)
        {
            if (!item.IsLocalPlayer)
            {
                item.transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().SetText(item.playerName.Value + "   Lv. " + item.level.Value);
            }
        }
    }



    public void StartIntroduction()
    {
        if (IsHost && finishIntro == 0)
        {
            ui.GetChild(11).gameObject.SetActive(true); //start intro
            gmScript.SetMissionPanelState(false);
        }
        else if (IsClient && finishIntro == 0)
        {
            ui.GetChild(7).gameObject.SetActive(false); //disable welcome object
            ui.GetChild(11).gameObject.SetActive(true); //start intro
            gmScript.SetMissionPanelState(false);
        }
        else
        {
            ui.GetChild(10).gameObject.SetActive(true);
            ui.GetChild(14).gameObject.SetActive(true);
            ui.GetChild(7).gameObject.SetActive(false); //disable welcome object
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void EquipItemServerRpc(int itemIndex, ulong playerID, bool isWeapon)
    {
        EquipItemClientRpc(itemIndex, playerID, isWeapon);

    }
    [ClientRpc]
    void EquipItemClientRpc(int itemIndex, ulong playerID, bool isWeapon)
    {
        foreach (var item in gmScript.players)
        {
            if (item.NetworkObjectId == playerID)
            {
                if (isWeapon)
                { //is a weapon
                    //Debug.Log("set weapon design");
                    if (itemIndex == 0)
                    {
                        item.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetChild(0).GetChild(1).GetChild(1).GetComponent<SpriteRenderer>().sprite = null;
                    }
                    else
                    {
                        item.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetChild(0).GetChild(1).GetChild(1).GetComponent<SpriteRenderer>().sprite = itemsPool.GetChild(itemIndex - 1).GetComponent<LanItemSS>().itemImageWS;
                    }
                }
                else
                { //is armor
                    //Debug.Log("set armor design");
                    if (itemIndex == 0)
                    {
                        Debug.Log("itemIndex" + itemIndex);
                        item.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(3).GetComponent<SpriteRenderer>().sprite = gmScript.characterCreation.torso[item.torso.Value];
                    }
                    else
                    {
                        item.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(3).GetComponent<SpriteRenderer>().sprite = itemsPool.GetChild(itemIndex - 1).GetComponent<LanItemSS>().itemImageWS;
                        //Debug.Log("armor design !null " + itemsPool.GetChild(itemIndex-1).GetComponent<LanItemSS>().itemImageWS);
                        //Debug.Log(item.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(3).GetComponent<SpriteRenderer>().sprite);
                    }
                }
            }
        }

    }




    IEnumerator ManaRegen()
    { //
        while (true)
        {
            if (currentMana < finalMana)
            { //mana regen 2.5% of max mana every 1s

                currentMana += finalMana * .025f;
                gmScript.UpdateUI();
            }
            else if (currentHealth.Value < finalHealth.Value)
            { //health regen 1% of max health every 1s
                currentHealth.Value += finalHealth.Value * .01f;
                gmScript.UpdateUI();
            }
            yield return new WaitForSeconds(1);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void DisableEnemyServerRpc(int enemyIndex)
    {
        DisableEnemyClientRpc(enemyIndex);
    }
    [ClientRpc]
    void DisableEnemyClientRpc(int enemyIndex)
    {
        print("disabled  boss");
        Transform enemy = mobsParent.transform.GetChild(enemyIndex);
        enemy.gameObject.SetActive(false);
        enemy.GetComponent<Collider2D>().enabled = false;
    }

    /* REMOVED
    [ServerRpc(RequireOwnership = false)]
    public void DisableStatueServerRpc(int statueIndex, int statueParentIndex, int statueParentParentIndex) { //index as siblingIndex()
        DisableStatueClientRpc(statueIndex, statueParentIndex, statueParentParentIndex);
    }
    [ClientRpc]
    void DisableStatueClientRpc(int statueIndex, int statueParentIndex, int statueParentParentIndex) {
        gmScript.dungeonStatues++;
        gmScript.UpdateMission();

        dungeonParent.GetChild(statueParentParentIndex).GetChild(statueParentIndex).GetChild(statueIndex).gameObject.SetActive(false);
    } */

    [ServerRpc(RequireOwnership = false)]
    public void GiveRewardsServerRpc(float exp, bool isFinalItem)
    {
        float sharedExp = exp / gmScript.players.Length; //shared exp, exp reward divided by the number of players
        int consumableOrItem = Random.Range(0, 3); //draw 0-2, if 0 give potion or hint, if 1 give weapon, 2 give armor
        int potionOrHint = Random.Range(0, 2); //draw 0-1
        float rarity = Random.Range(0, 1); //the rarity of the weapon/armor
        GiveRewardsClientRpc(sharedExp, consumableOrItem, potionOrHint, rarity, isFinalItem);
    }
    [ClientRpc]
    void GiveRewardsClientRpc(float sharedExp, int consumableOrItem, int potionOrHint, float rarity, bool isFinalItem)
    {
        int itemIndex;
        foreach (var item in gmScript.players)
        {
            if (item.IsLocalPlayer)
            {
                if (isFinalItem)
                {
                    reward = Instantiate(itemsPoolWS.transform.GetChild(5).GetChild(10), itemsPoolWS.transform);
                }
                else
                {
                    item.currentExp += sharedExp; //give exp

                    //consumables or item rewards
                    if (consumableOrItem == 0) //potion or hint 33% chance
                    { //give consumables
                        reward = Instantiate(itemsPoolWS.transform.GetChild(0), itemsPoolWS.transform);
                        reward.GetComponent<LanPotion>().draw = potionOrHint;

                        //rename object
                        if (potionOrHint != 1) return;
                        reward.name = "Hint";
                    }
                    else if (consumableOrItem == 2) //33% chance to get armor
                    {
                        itemIndex = Random.Range(0, itemsPoolWS.transform.GetChild(7).childCount);
                        reward = Instantiate(itemsPoolWS.transform.GetChild(7).GetChild(itemIndex), itemsPoolWS.transform);
                    }
                    else //weapon //33% chance
                    {

                        if (rarity >= 0 && rarity < .40)
                        { //chance 40% common
                            itemIndex = Random.Range(0, itemsPoolWS.transform.GetChild(1).childCount);
                            reward = Instantiate(itemsPoolWS.transform.GetChild(1).GetChild(itemIndex), itemsPoolWS.transform);
                        }
                        else if (rarity >= .40 && rarity < .70)
                        { //30% uncommon
                            itemIndex = Random.Range(0, itemsPoolWS.transform.GetChild(2).childCount);
                            reward = Instantiate(itemsPoolWS.transform.GetChild(2).GetChild(itemIndex), itemsPoolWS.transform);
                        }
                        else if (rarity >= .70 && rarity < .85)
                        { //15% rare
                            itemIndex = Random.Range(0, itemsPoolWS.transform.GetChild(3).childCount);
                            reward = Instantiate(itemsPoolWS.transform.GetChild(3).GetChild(itemIndex), itemsPoolWS.transform);
                        }
                        else if (rarity >= .85 && rarity < .95)
                        { //10% epic
                            itemIndex = Random.Range(0, itemsPoolWS.transform.GetChild(4).childCount);
                            reward = Instantiate(itemsPoolWS.transform.GetChild(4).GetChild(itemIndex), itemsPoolWS.transform);
                        }
                        else if (rarity >= .95 && rarity <= 1)
                        {  //5% legendary
                            itemIndex = Random.Range(0, itemsPoolWS.transform.GetChild(5).childCount);
                            reward = Instantiate(itemsPoolWS.transform.GetChild(5).GetChild(itemIndex), itemsPoolWS.transform);
                        }
                    }
                }

                reward.position = gmScript.player.transform.position;
                reward.gameObject.SetActive(true);


                //show reward
                TextMeshProUGUI temp = rewardsLabelPool.GetChild(0).GetComponent<TextMeshProUGUI>();
                temp.SetText("+1 " + reward.gameObject.name.Replace("(Clone)", ""));
                temp.transform.SetParent(gmScript.player.transform.GetChild(1));
                temp.transform.position = gmScript.player.transform.position;
                temp.gameObject.SetActive(true);

                //item is from foreach current iteration
                item.score.Value += 100; // Increase the score of the current item by 100
                item.UpdateStats(); // Update the stats of the current item
                gmScript.UpdateUI(); // Update the UI elements such as player health bar, exp bar, etc.
                gmScript.SavePlayerData(); // Save the current state of player data
                break;
            }
        }
        gmScript.UpdateUI();
    }

    [ServerRpc]
    public void SetPlayerPositionZeroServerRpc()
    {
        SetPlayerPositionZeroClientRpc();
    }

    [ClientRpc]
    void SetPlayerPositionZeroClientRpc()
    {
        transform.position = Vector3.zero;
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartEndingServerRpc()
    {
        StartEndingServerRpcClientRpc();
    }

    [ClientRpc]
    void StartEndingServerRpcClientRpc()
    {
        gmScript.StartEnding();
    }
}