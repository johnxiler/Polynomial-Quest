using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using UnityEngine.Rendering.Universal;

public class LanGameManager : MonoBehaviour
{
    [SerializeField] Transform Controls;
    public Slider sliderExp, sliderHealth, sliderMana, sliderHealthWS; //sliderHealthWS worlds space slider

    //initialize variables
    TextMeshProUGUI healthText, manaText, expText;
    public TextMeshProUGUI usernameSS, usernameWS; //SS = screen Space, WS = world space
    public TextMeshProUGUI potionText, hintText, console;
    public LanPlayer player;
    public float enemyStatsModifier = 100f;
    public GameObject inventoryManager, itemPool, characterCreationObject, customizationCamera;
    [SerializeField] public LanCreateCharacter characterCreation;
    LanCameraController playerCamera;
    public int difficulty = 0, dungeonStatues;
    AudioSource audioSource;
    public AudioSource correctSound, wrongSound, dungeonEntranceFail, levelUp;
    public AudioClip[] backgroundMusic;
    public LanPlayer[] players;
    public LanKnights[] knights;
    public bool isPortalFound, hasSetEquippedWeapon, hasSetEquippedArmor, hasMapInstaniated, isInsideDungeon, isWeatherDone, hasSetDifficultyForClient;
    public AudioClip[] WarriorSoundEffects;
    public AudioClip[] MageSoundEffects;
    public AudioClip playerHitSoundEffect, playerDieSoundEffect;
    Light2D light2D;
    public Sprite[] warriorSkillIcons, mageSkillIcons, assassinSkillIcons;
    [SerializeField] TextMeshProUGUI weatherTitleText, weatherInfoText, scoreText;
    [SerializeField] LanItemInfo lanItem;
    float elapseTime, weatherDuration = 30;

    [Header("Maps")]
    [SerializeField] GameObject[] maps;
    [SerializeField] Transform mapsParent, minimap, MissionPanel, skillsEffectParent, ending;

    //training ground missions variables
    public int librarian, topicsDiscovered, interactedStatues, exmapleStatues;
    [SerializeField] LanNpc[] statueBySequence;

    public LanInteractionManager InteractionManager;
    Vector3 dungeonSpawnLocation;
    public void Initialize()
    {

        players = FindObjectsOfType<LanPlayer>();
        foreach (LanPlayer p in players)
        {
            if (p.IsLocalPlayer)
            {
                player = p;
                break;
            }
        }

        knights = FindObjectsOfType<LanKnights>();

        sliderHealthWS = player.transform.GetChild(1).GetChild(0).GetComponent<Slider>(); //get world space healthbar
        sliderHealth = GameObject.FindWithTag("PlayerInfoBar").transform.GetChild(1).GetComponent<Slider>();
        healthText = sliderHealth.transform.GetChild(2).GetComponent<TextMeshProUGUI>();

        sliderMana = GameObject.FindWithTag("UI").transform.GetChild(6).GetChild(2).GetComponent<Slider>();
        manaText = GameObject.FindWithTag("UI").transform.GetChild(6).GetChild(2).GetChild(2).GetComponent<TextMeshProUGUI>();

        sliderExp = GameObject.FindWithTag("UI").transform.GetChild(6).GetChild(3).GetComponent<Slider>();
        expText = sliderExp.transform.GetChild(2).GetComponent<TextMeshProUGUI>();


        potionText = Controls.GetChild(5).GetChild(0).GetComponent<TextMeshProUGUI>();
        hintText = Controls.GetChild(12).GetChild(0).GetComponent<TextMeshProUGUI>();
        usernameSS = GameObject.FindWithTag("PlayerInfoBar").transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        usernameWS = player.transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>();
        audioSource = GetComponent<AudioSource>(); //music component
        light2D = GameObject.FindWithTag("Light").GetComponent<Light2D>();

        //SOUNDS
        correctSound = GameObject.FindWithTag("Sounds").transform.GetChild(0).GetChild(0).GetComponent<AudioSource>();
        wrongSound = GameObject.FindWithTag("Sounds").transform.GetChild(0).GetChild(1).GetComponent<AudioSource>();
        //initialize NPC questions and their dialogues
        LanNpc[] npc = FindObjectsOfType<LanNpc>(); //find all NPC
        foreach (LanNpc item in npc)
        {
            item.Initialize();        //Initialize() for each NPC
        }


        //LoadPlayerData();



        //initialize camera
        playerCamera = GameObject.FindWithTag("PlayerCamera").GetComponent<LanCameraController>();
        playerCamera.Initialize();

        LoadPlayerData();



        //initialize skills warrior
        //GameObject.FindWithTag("SkillEffects").transform.GetChild(0).GetChild(0).GetComponent<WarriorSkill1>().Initialize();
        //GameObject.FindWithTag("SkillEffects").transform.GetChild(0).GetChild(1).GetComponent<WarriorSKill2>().Initialize();
        //GameObject.FindWithTag("SkillEffects").transform.GetChild(0).GetChild(2).GetComponent<WarriorSkill3>().Initialize();
        //GameObject.FindWithTag("SkillEffects").transform.GetChild(0).GetChild(3).GetComponent<WarriorSkill4>().Initialize();


        //initialize skills icon
        switch (player.playerClass)
        {
            case "Warrior":
                //skill 1
                Controls.GetChild(8).GetComponent<Image>().sprite = warriorSkillIcons[0];
                Controls.GetChild(8).GetChild(1).GetComponent<Image>().sprite = warriorSkillIcons[0];

                //skill 2
                Controls.GetChild(9).GetComponent<Image>().sprite = warriorSkillIcons[1];
                Controls.GetChild(9).GetChild(1).GetComponent<Image>().sprite = warriorSkillIcons[1];

                //skill 3
                Controls.GetChild(10).GetComponent<Image>().sprite = warriorSkillIcons[2];
                Controls.GetChild(10).GetChild(1).GetComponent<Image>().sprite = warriorSkillIcons[2];

                //skill 4
                Controls.GetChild(11).GetComponent<Image>().sprite = warriorSkillIcons[3];
                Controls.GetChild(11).GetChild(1).GetComponent<Image>().sprite = warriorSkillIcons[3];
                break;

            case "Mage":
                break;

            case "Assassin":
                break;
        }

        //Application.targetFrameRate = 60;
        customizationCamera.SetActive(false);
        InitializeSkills();
        //check if has finish training
        //if false go to training area
        ChangeSpawnLocation();

        //pacomment po sir nung original code
        //12/12/2023
        UpdateMission();
    }

    ////////////////////////////////////////RAIN/////////////////////////////////////
    public void RainWeather()
    {
        StartRain();
        StartCoroutine(RainWait());
    }

    private void StartRain()
    {
        weatherTitleText.gameObject.SetActive(true);
        weatherTitleText.SetText("raining");
        weatherInfoText.SetText("-10% movespeed");
        player.transform.GetChild(2).GetChild(0).gameObject.SetActive(true);

        player.moveSpeed -= player.moveSpeed * .10f; //debuffs
        light2D.intensity = .7f;
    }

    IEnumerator RainWait()
    {
        yield return new WaitForSeconds(30f);
        StopRain();
        player.RandomWeatherServerRpc();
    }

    private void StopRain()
    {
        weatherTitleText.gameObject.SetActive(false);
        player.transform.GetChild(2).GetChild(0).gameObject.SetActive(false);
        player.moveSpeed += player.moveSpeed * .10f;
        light2D.intensity = 1f;
    }

    //////////////////////////////////////END////////////////////////////////////////////

    ////////////////////////////////////////DESERT/////////////////////////////////////
    public void DesertWeather()
    {
        StartDesert();

        StartCoroutine(DesertWait());
    }

    private void StartDesert()
    {
        weatherTitleText.gameObject.SetActive(true);
        weatherTitleText.SetText("SandStorm");
        weatherInfoText.SetText("-50% movespeed \n-10% attack Speed");
        player.transform.GetChild(2).GetChild(1).gameObject.SetActive(true); //disable particle system

        player.attackSpeed -= player.attackSpeed * .10f;
        player.moveSpeed -= player.moveSpeed * .50f; //debuffs
        light2D.intensity = 1.5f;
    }

    IEnumerator DesertWait()
    {
        yield return new WaitForSeconds(30f);
        StopDesert();
        player.RandomWeatherServerRpc();
    }

    private void StopDesert()
    {
        weatherTitleText.gameObject.SetActive(false);
        player.transform.GetChild(2).GetChild(1).gameObject.SetActive(false); //disable particle system

        player.moveSpeed += player.moveSpeed * .10f;
        player.attackSpeed += player.attackSpeed * .50f;
        light2D.intensity = 2f;
    }

    //////////////////////////////////////END////////////////////////////////////////////

    ////////////////////////////////////////SNOW/////////////////////////////////////
    public void SnowWeather()
    {
        StartSnow();

        UpdateUI();
        StartCoroutine(SnowWait());
    }

    private void StartSnow()
    {
        weatherTitleText.gameObject.SetActive(true);
        weatherTitleText.SetText("Snow");
        weatherInfoText.SetText("-25% movespeed \n-15% attack Speed \n-25% max mana");
        player.transform.GetChild(2).GetChild(2).gameObject.SetActive(true); //enable particle

        //debuffs
        player.attackSpeed -= player.attackSpeed * .10f;
        player.moveSpeed -= player.moveSpeed * .20f;
        player.finalMana -= player.finalMana * .25f;
    }

    IEnumerator SnowWait()
    {
        yield return new WaitForSeconds(30f);
        StopSnow();
        UpdateUI();
        player.RandomWeatherServerRpc();
    }

    private void StopSnow()
    {
        weatherTitleText.gameObject.SetActive(false);
        player.transform.GetChild(2).GetChild(2).gameObject.SetActive(false); //disable particle system

        player.attackSpeed += player.attackSpeed * .10f;
        player.moveSpeed += player.moveSpeed * .20f;
        player.finalMana += player.finalMana * .25f;
    }

    //////////////////////////////////////END////////////////////////////////////////////


    ////////////////////////////////////////SNOW/////////////////////////////////////
    public void AcidWeather()
    {
        StartAcid();

        UpdateUI();
        StartCoroutine(AcidWait());
    }

    private void StartAcid()
    {
        weatherTitleText.gameObject.SetActive(true);
        weatherTitleText.SetText("Acid Rain");
        weatherInfoText.SetText("-15% movespeed \n-15% attack Speed \n-25% max mana \n0 regeneration");
        player.transform.GetChild(2).GetChild(3).gameObject.SetActive(true);

        //debuffs
        player.attackSpeed -= player.attackSpeed * .10f;
        player.moveSpeed -= player.moveSpeed * .10f;
        player.finalMana -= player.finalMana * .25f;
    }

    IEnumerator AcidWait()
    {
        if (elapseTime < weatherDuration)
        {
            elapseTime += Time.fixedDeltaTime;
            while (true && !isInsideDungeon)
            {
                player.currentHealth.Value -= player.currentHealth.Value * .01f;
                yield return new WaitForSeconds(2f);
            }
        }
        StopAcid();
        UpdateUI();
        player.RandomWeatherServerRpc();
    }

    public void StopAcid()
    {
        //yield return new WaitForSeconds(30f);
        weatherTitleText.gameObject.SetActive(false);
        player.transform.GetChild(2).GetChild(3).gameObject.SetActive(false); //disable particle system

        player.attackSpeed += player.attackSpeed * .10f;
        player.moveSpeed += player.moveSpeed * .10f;
        player.finalMana += player.finalMana * .25f;
    }

    //////////////////////////////////////END////////////////////////////////////////////


    public IEnumerator RedrawWeather()
    {
        weatherTitleText.gameObject.SetActive(false);
        yield return new WaitForSeconds(30f);
        player.RandomWeatherServerRpc();
    }



















    public void UpdateUI()
    {
        sliderHealth.maxValue = player.finalHealth.Value;
        sliderHealth.value = player.currentHealth.Value;
        sliderHealthWS.maxValue = player.finalHealth.Value;
        sliderHealthWS.value = player.currentHealth.Value;
        healthText.SetText(Mathf.FloorToInt(sliderHealth.value).ToString() + " / " + Mathf.FloorToInt(sliderHealth.maxValue).ToString());

        sliderMana.maxValue = player.finalMana;
        sliderMana.value = player.currentMana;
        manaText.SetText(Mathf.FloorToInt(sliderMana.value).ToString() + " /" + Mathf.FloorToInt(sliderMana.maxValue).ToString());

        sliderExp.maxValue = player.finalRequiredExp;
        sliderExp.value = player.currentExp;
        expText.SetText(Mathf.FloorToInt(sliderExp.value).ToString() + "/" + Mathf.FloorToInt(sliderExp.maxValue).ToString());

        potionText.SetText(player.potion.ToString());
        hintText.SetText(player.hint.ToString());


        usernameSS.SetText(player.playerName.Value + "  Lv. " + player.level.Value);
        usernameWS.SetText(player.playerName.Value + "  Lv. " + player.level.Value);

        scoreText.SetText("SCORE: " + player.score.Value.ToString());

    }


    public void LoadPlayerData()
    {
        //executed once lifetime
        if (PlayerPrefs.GetInt("hasStatsInitialized") == 0)
        {
            PlayerPrefs.SetInt("hasStatsInitialized", 1);
            player.playerName.Value = PlayerPrefs.GetString("username");
            player.username = PlayerPrefs.GetString("username");
            player.playerClass = PlayerPrefs.GetString("playerClass");
            player.equipedWeaponIndex = PlayerPrefs.GetInt("equipedWeaponIndex");

            //initialize variables
            player.finalDamage = player.baseDamage + player.weaponDmg;
            player.finalHealth.Value = player.baseHealth.Value;
            player.currentHealth.Value = 100;
            player.finalMana = player.baseMana;
            player.currentMana = player.finalMana;
            player.finalRequiredExp = player.baseRequiredExp;
            player.potion = 10;
            player.hint = 10;
            player.finalArmor = player.baseArmor;
            player.level.Value = 30f;

            SavePlayerData();
        }
        else
        {
            player.playerName.Value = PlayerPrefs.GetString("username");
            player.username = PlayerPrefs.GetString("username");
            player.playerClass = PlayerPrefs.GetString("playerClass");

            player.level.Value = PlayerPrefs.GetInt("level");
            player.currentExp = PlayerPrefs.GetInt("currentExp");
            player.baseRequiredExp = PlayerPrefs.GetInt("baseRequiredExp");
            player.finalRequiredExp = PlayerPrefs.GetInt("finalRequiredExp");
            player.potion = PlayerPrefs.GetInt("potion");
            player.equipedWeaponIndex = PlayerPrefs.GetInt("equipedWeaponIndex");
            player.weaponIndexAtInventory = PlayerPrefs.GetInt("weaponIndexAtInventory");
            player.equipedArmorIndex = PlayerPrefs.GetInt("equipedArmorIndex");
            player.armorIndexAtInventory = PlayerPrefs.GetInt("armorIndexAtInventory");
            player.hint = PlayerPrefs.GetInt("hint");
            player.finishIntro = PlayerPrefs.GetInt("finishIntro");
            player.score.Value = PlayerPrefs.GetInt("score");
            player.finalHealth.Value = PlayerPrefs.GetInt("finalHealth");
            player.baseArmor = PlayerPrefs.GetInt("baseArmor");
            player.finalMana = player.baseMana;
            player.currentMana = player.finalMana;

            //initialize
            //player.finalDamage = player.baseDamage + player.weaponDmg;
            player.UpdateStats();
            player.currentHealth.Value = player.finalHealth.Value;
        }







        //set Player attack range base on player class
        switch (player.playerClass)
        {
            case "Warrior":
                player.attackRange = 0.30f;
                break;

            case "Mage":
                player.attackRange = 0.75f;
                break;

            case "Assassin":
                player.attackRange = 0.30f;
                break;
        }

        //player.UpdatePlayerListInfoServerRpc();
        //player.PlayerCustomizationServerRpc();  
        //player.SubscribeHealthBars();
        UpdateUI();

        //END LOAD PLAYER DATA
        //Debug.Log("LOAD DATA SUCCESSFULLY");
    }

    public void PopulateInventory()
    {
        //Load Inventory String
        string tempString = PlayerPrefs.GetString("inventory");
        if (tempString == "")
        {
            tempString = "0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0";
        }
        player.inventory = tempString.Split(",");


        //Instantiate items to Inventory Panel
        for (int i = 0; i < player.inventory.Length; i++)
        { //30
            int temp = int.Parse(player.inventory[i]) - 1; //5

            for (int j = 0; j < itemPool.transform.childCount; j++)
            {
                if (temp == itemPool.transform.GetChild(j).GetSiblingIndex() && temp >= 0)
                {
                    GameObject tempInstance = Instantiate(itemPool.transform.GetChild(j).gameObject, inventoryManager.transform.GetChild(0)); //instantiate
                    LanItemSS tempitemScript = tempInstance.GetComponent<LanItemSS>();
                    tempitemScript.itemIndexAtInventory = i;
                    tempInstance.gameObject.SetActive(true);
                    //tempitemScript.itemIndex = j;
                    if (player.weaponIndexAtInventory - 1 == i && !hasSetEquippedWeapon && tempitemScript.itemType == "sword")
                    { //show equipped item status if true
                        hasSetEquippedWeapon = true;
                        tempitemScript.isEquipped = true;
                        tempInstance.transform.GetChild(0).gameObject.SetActive(true);

                        //set stats
                        player.weaponDmg = tempitemScript.damage;
                        player.UpdateStats();
                        player.EquipItemServerRpc(tempitemScript.itemIndex, player.NetworkObjectId, true);
                    }
                    else if (player.armorIndexAtInventory - 1 == i && !hasSetEquippedArmor)
                    { //show 
                        hasSetEquippedArmor = true;
                        tempitemScript.isEquipped = true;
                        tempInstance.transform.GetChild(0).gameObject.SetActive(true);

                        //set stats
                        player.itemArmor = tempitemScript.armor;
                        player.UpdateStats();
                        player.EquipItemServerRpc(tempitemScript.itemIndex, player.NetworkObjectId, false);

                    }
                    break;
                }
            }

        }
    }
    public void SavePlayerData()
    {

        PlayerPrefs.SetInt("currentExp", (int)player.currentExp);
        PlayerPrefs.SetInt("potion", (int)player.potion);
        PlayerPrefs.SetInt("equipedWeaponIndex", (int)player.equipedWeaponIndex);
        PlayerPrefs.SetInt("weaponIndexAtInventory", (int)player.weaponIndexAtInventory);
        PlayerPrefs.SetInt("equipedArmorIndex", (int)player.equipedArmorIndex);
        PlayerPrefs.SetInt("armorIndexAtInventory", (int)player.armorIndexAtInventory);
        PlayerPrefs.SetInt("baseRequiredExp", (int)player.baseRequiredExp);
        PlayerPrefs.SetInt("finalRequiredExp", (int)player.finalRequiredExp);
        PlayerPrefs.SetInt("finishIntro", (int)player.finishIntro);
        PlayerPrefs.SetInt("hint", (int)player.hint);
        PlayerPrefs.SetInt("level", (int)player.level.Value);
        PlayerPrefs.SetInt("score", (int)player.score.Value);
        PlayerPrefs.SetInt("finalHealth", (int)player.finalHealth.Value);
        PlayerPrefs.SetInt("currentHealth", (int)player.currentHealth.Value);
        PlayerPrefs.SetInt("baseArmor", (int)player.baseArmor);


        string tempString = string.Join(",", player.inventory);
        PlayerPrefs.SetString("inventory", tempString);

        Debug.Log("SAVE DATA SUCCESSFULLY");
    }



    public void LoadPlayerCostumization()
    {
        //find all player scripts
        LanPlayer[] temp = FindObjectsOfType<LanPlayer>();
        foreach (LanPlayer p in temp)
        {
            //Debug.Log("LoadPlayerCostumization()");
            //belt
            p.transform.GetChild(0).GetChild(0).GetChild(3).GetComponent<SpriteRenderer>().sprite = characterCreation.belt[p.belt.Value];

            //boots
            p.transform.GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite = characterCreation.boots_l[p.boots.Value];
            p.transform.GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite = characterCreation.boots_r[p.boots.Value];

            //elbow
            p.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetChild(1).GetComponent<SpriteRenderer>().sprite = characterCreation.elbow_l[p.elbow.Value];
            p.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(1).GetComponent<SpriteRenderer>().sprite = characterCreation.elbow_r[p.elbow.Value];

            //face
            p.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(1).GetComponent<SpriteRenderer>().sprite = characterCreation.face[p.face.Value];

            //hood
            p.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(2).GetComponent<SpriteRenderer>().sprite = characterCreation.hood[p.hood.Value];

            //legs
            p.transform.GetChild(0).GetChild(0).GetChild(1).GetChild(1).GetComponent<SpriteRenderer>().sprite = characterCreation.legs_l[p.legs.Value];
            p.transform.GetChild(0).GetChild(0).GetChild(2).GetChild(1).GetComponent<SpriteRenderer>().sprite = characterCreation.legs_r[p.legs.Value];

            //shoulder
            p.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(1).GetChild(1).GetComponent<SpriteRenderer>().sprite = characterCreation.shoulder_l[p.shoulder.Value];
            p.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(2).GetChild(1).GetComponent<SpriteRenderer>().sprite = characterCreation.shoulder_r[p.shoulder.Value];

            //Torso
            p.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(3).GetComponent<SpriteRenderer>().sprite = characterCreation.torso[p.torso.Value];

            //torso
            p.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite = characterCreation.wrist_l[p.wrist.Value];
            p.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite = characterCreation.wrist_r[p.wrist.Value];
        }
    }


    public void StartBackgroundMusic()
    {
        switch (difficulty)
        {
            case 0:
                audioSource.clip = backgroundMusic[0];
                break;

            case 1:
                audioSource.clip = backgroundMusic[1];
                break;

            case 2:
                audioSource.clip = backgroundMusic[2];
                break;

            case 3:
                audioSource.clip = backgroundMusic[0];
                break;
        }
        audioSource.volume = .15f;
        audioSource.Play();
    }

    public void ChangeBackgroundMusic(int index)
    {
        audioSource.clip = backgroundMusic[index];
        audioSource.volume = .35f;
        audioSource.Play();
    }



    ////////////////////////////////////////////////////////MISSION/////////////////////////////////////////////
    public void UpdateMission()
    {
        var mission1 = MissionPanel.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
        var mission2 = MissionPanel.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>();

        //training ARENA missions
        var training1 = MissionPanel.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>();
        var training2 = MissionPanel.GetChild(2).GetChild(1).GetComponent<TextMeshProUGUI>();
        var training3 = MissionPanel.GetChild(2).GetChild(2).GetComponent<TextMeshProUGUI>();
        var training4 = MissionPanel.GetChild(2).GetChild(3).GetComponent<TextMeshProUGUI>();


        mission1.SetText("Dungeon statues completed: " + dungeonStatues + "/ 21");
        training1.SetText("talk to the librarian: " + librarian + "/ 1");
        training2.SetText("Discover topics in the library: " + topicsDiscovered + "/ 9");
        training3.SetText("Interacted Statues: " + interactedStatues + "/ 9");
        training4.SetText("example questionairs: " + exmapleStatues + "/ 2");
        if (dungeonStatues == 21 || dungeonStatues < 21) //original: if (dungeonStatues == 21) {}
        {
            mission1.color = Color.gray;

            //show next mission
            MissionPanel.GetChild(1).GetChild(1).gameObject.SetActive(true);

            if (difficulty == 3)
            {
                //show portal
                skillsEffectParent.GetChild(2).GetChild(1).gameObject.SetActive(true); //enable portal
            }
        }

        if (isPortalFound) ////start next mission
        {
            mission2.color = Color.gray;

            //
            MissionPanel.GetChild(1).GetChild(2).gameObject.SetActive(true);
        }

        //TRAINING ARENA MISSIONS
        if (librarian == 1)
        {
            training1.color = Color.gray;
        }
        if (topicsDiscovered == 9)
        {
            training2.color = Color.gray;
        }
        if (interactedStatues == 9)
        {
            training3.color = Color.gray;
        }
        if (exmapleStatues == 2)
        {
            training4.color = Color.gray;
        }

        //check if all mission on training completed
        if (librarian == 1 && topicsDiscovered == 9 && interactedStatues == 9 && exmapleStatues == 2)
        {
            MissionPanel.GetChild(2).GetChild(4).gameObject.SetActive(true);
            player.isDoneTraining = true;
        }
    }

    public void SetLighting()
    {
        //2D light Config
        switch (difficulty)
        {
            case 0:
                light2D.intensity = 1;
                light2D.color = new Color32(255, 255, 255, 255);
                break;


            case 1: //desert
                light2D.intensity = 2;
                light2D.color = new Color32(255, 216, 162, 255); // Yellow Orange
                break;

            case 2:
                light2D.intensity = 1.5f;
                light2D.color = new Color32(248, 248, 255, 255);
                break;

            case 3:
                light2D.color = new Color32(85, 107, 47, 255);
                break;
        }

        //Debug.Log("2D config called" + difficulty);
    }



    public int HasFinishTraining()
    {
        var hasFinish = PlayerPrefs.GetInt("hasFinishTraining");
        return hasFinish;
    }

    public void SetMissionPanelState(bool state)
    {
        MissionPanel.gameObject.SetActive(state);
    }

    public void InstantiateMaps()
    {
        Instantiate(maps[difficulty], mapsParent);
        if (difficulty == 3)
        {
            GameObject easyMap = Instantiate(maps[0], mapsParent);
            easyMap.SetActive(false);
        }
        hasMapInstaniated = true;
    }

    public void ChangeSpawnLocation()
    {
        if (HasFinishTraining() == 1)
        {
            ChangeMission(1);
        }
        else
        {
            //if has not finish training, go to
            player.transform.position = new(-39.317f, -29.308f); //then go to training ground
            ChangeMission(0);
        }

    }

    public void ChangeMission(int mission)
    {
        switch (mission)
        {
            case 0:
                MissionPanel.GetChild(1).gameObject.SetActive(false);
                MissionPanel.GetChild(2).gameObject.SetActive(true);
                break;

            case 1:
                MissionPanel.GetChild(1).gameObject.SetActive(true);
                MissionPanel.GetChild(2).gameObject.SetActive(false);
                break;
        }

    }

    public void EnableMinimap(bool state)
    {
        minimap.gameObject.SetActive(state);
    }

    public void InitializeSkills()
    {
        //initialize skills
        //initialize spell and skills
        Controls.GetChild(7).GetComponent<LanSpell1>().Initialize(); //spell flicker
        Controls.GetChild(8).GetComponent<LanSkill1>().Initialize(); //first skill
        Controls.GetChild(9).GetComponent<LanSkill2>().Initialize(); //second skill
        Controls.GetChild(10).GetComponent<LanSkill3>().Initialize(); //third skill
        Controls.GetChild(11).GetComponent<LanSkill4>().Initialize(); //fourth skill
    }


    public void HideWeather()
    {
        player.transform.GetChild(2).gameObject.SetActive(false); //disable weather particle system
        switch (difficulty)
        {
            case 0:
                StopRain();
                break;

            case 1:
                StopDesert();
                break;

            case 2:
                StopSnow();
                break;

            case 3:
                StopAcid();
                break;
        }
    }

    public void ShowWeather()
    {
        player.transform.GetChild(2).gameObject.SetActive(true); //enable weather particle system
        switch (difficulty)
        {
            case 0:
                StartRain();
                break;

            case 1:
                StartDesert();
                break;

            case 2:
                StartSnow();
                break;

            case 3:
                StartAcid();
                break;
        }
    }

    public void StopWeatherCourotines()
    {
        StopRain();
        StopDesert();
        StopSnow();
        StopAcid();
    }

    public void DeleteInstantiatedMaps()
    {
        for (int i = 1; i < mapsParent.childCount; i++)
        {
            Destroy(mapsParent.GetChild(i).gameObject);
        }
    }
    public void BySequenceCheck()
    {
        foreach (var item in statueBySequence)
        {
            item.BySequenceCheck();
        }
    }

    public void SetDungeonSpawnLocation(Vector3 location)
    {
        dungeonSpawnLocation = location;
    }

    public Vector3 GetDungeonSpawnLocation()
    {
        return dungeonSpawnLocation;
    }

    public GameObject GetMap(int index)
    {
        return maps[index];
    }

    public void StartEnding()
    {
        //START ENDING
        ending.gameObject.SetActive(true);
    }
}
