using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class LanSkill1 : NetworkBehaviour
{
    [SerializeField] LanGameManager gmScript;
    Joystick joystick;
    Image outerCircle, innerCircle, skillImage, inCooldownSkillImage;
    public float baseDamage, damageModifier, finalDamage;
    Transform player, target, range, arrow, cone, skillEffectParent, tempSkillIndicator, instantiatedSkill, warriorSkillObject, mageSkillObject,
    AssassinSkillObject;
    public Transform tempSkill, inUse;
    bool hasInitialized, hasPressed, hasReleased;
    float skillRange = .73f, cooldown = 6f, cooldownTimer, tempCooldownTimer, isDirectCast, manaCost, rotationZ;
    GameObject cooldownImageObject, skillJoyStick;
    Image cooldownImage;
    TextMeshProUGUI cooldownText;
    string playerClass;
    Rigidbody2D rb;
    Vector2 joystickDirection;
    Color outerColor, innerColor;
    Vector3 targetPosition, difference;




    public void Initialize()
    {
        joystick = transform.GetChild(0).GetComponent<Joystick>();
        outerCircle = transform.GetChild(0).GetComponent<Image>(); //joystick
        innerCircle = transform.GetChild(0).GetChild(0).GetComponent<Image>(); //joystick
        skillImage = GetComponent<Image>();
        cooldownText = transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        cooldownImageObject = transform.GetChild(1).gameObject;
        skillJoyStick = transform.GetChild(0).gameObject;
        skillEffectParent = GameObject.FindWithTag("SkillEffects").transform;
        cooldownImage = cooldownImageObject.GetComponent<Image>();
        inCooldownSkillImage = transform.GetChild(1).GetComponent<Image>();
        tempCooldownTimer = cooldownTimer / cooldown;

        player = gmScript.player.transform;
        rb = gmScript.player.GetComponent<Rigidbody2D>();
        playerClass = gmScript.player.playerClass;


        target = player.GetChild(1).GetChild(2).GetChild(2);
        range = player.GetChild(1).GetChild(2).GetChild(0);
        cone = player.transform.GetChild(1).GetChild(2).GetChild(1);
        arrow = player.transform.GetChild(1).GetChild(2).GetChild(3);

        warriorSkillObject = skillEffectParent.GetChild(0).GetChild(0);
        mageSkillObject = skillEffectParent.GetChild(1).GetChild(2).GetChild(0); //
        AssassinSkillObject = skillEffectParent.GetChild(3).GetChild(0); //

        switch (playerClass)
        {
            case "Warrior":
                tempSkillIndicator = arrow;
                skillImage.sprite = gmScript.warriorSkillIcons[0];
                manaCost = 15;
                cooldown = 8;
                break;

            case "Mage":
                skillImage.sprite = gmScript.mageSkillIcons[0];
                inCooldownSkillImage.sprite = gmScript.mageSkillIcons[0];
                tempSkillIndicator = arrow;
                manaCost = 10;
                break;

            case "Assassin":
                skillImage.sprite = gmScript.assassinSkillIcons[0];
                inCooldownSkillImage.sprite = gmScript.assassinSkillIcons[0];
                tempSkillIndicator = arrow;
                manaCost = 12;
                break;
        }

        hasInitialized = true;

        //initialize joystick transparancy
        outerColor = outerCircle.color; //get color from joystick outer Circle
        innerColor = innerCircle.color;

        //StartCoroutine(ManaCheck()); //start mana check
    }

    private void Update()
    {
        if (!hasInitialized) return;
        cooldownTimer -= Time.deltaTime;
        //Debug.Log(cooldownTimer);
        //start here
        if (cooldownTimer >= 0)
        { //
            skillJoyStick.SetActive(false); //disable skill joystick
            cooldownImageObject.SetActive(true); //enable cooldown image
            cooldownImage.fillAmount = (cooldownTimer - 0) / (cooldown - 0);
            cooldownText.gameObject.SetActive(true);
            cooldownText.fontSize = 120;
            cooldownText.SetText(cooldownTimer.ToString("0.0"));
        }
        else if ((gmScript.player.currentMana - manaCost) < 0)
        {
            cooldownImage.gameObject.SetActive(true);
            cooldownImage.fillAmount = 1;
            cooldownText.gameObject.SetActive(true);
            cooldownText.fontSize = 80;
            cooldownText.SetText("NO MANA");
        }
        else
        {
            skillJoyStick.SetActive(true); //disable skill joystick
            cooldownImageObject.SetActive(false);
            cooldownText.gameObject.SetActive(false);
            cooldownImage.fillAmount = 1;
        }

        if (joystick.Horizontal != 0 || joystick.Vertical != 0 && (gmScript.player.currentMana - manaCost) >= 0)
        {
            hasPressed = true;
            range.gameObject.SetActive(true); //range indicator
            tempSkillIndicator.gameObject.SetActive(true); //enable skill target indicator

            //set scale
            transform.GetChild(0).localScale = new Vector3(2f, 2f, 1f);

            //start
            switch (playerClass)
            {
                case "Warrior":
                    tempSkillIndicator.gameObject.SetActive(false); //disable target indicator
                    range.localScale = new Vector2(0.075f, 0.075f); //set range indicator scale

                    //set joystick transparancy
                    outerColor.a = 0;
                    innerColor.a = 0;
                    break;

                case "Mage":
                    range.localScale = new Vector2(.25f, .25f); //range indicator

                    // Calculate target position         
                    targetPosition = player.position + new Vector3(joystick.Horizontal * skillRange, joystick.Vertical * skillRange, 0) * 1;
                    // Move circle towards target position
                    tempSkillIndicator.position = targetPosition;

                    difference = tempSkillIndicator.position - player.position;
                    rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
                    tempSkillIndicator.rotation = Quaternion.Euler(0.0f, 0.0f, rotationZ - 90.0f);
                    break;

                case "Assassin":
                    range.localScale = new Vector2(.25f, .25f); //range indicator

                    // Calculate target position         
                    targetPosition = player.position + new Vector3(joystick.Horizontal * skillRange, joystick.Vertical * skillRange, 0) * 1;
                    // Move circle towards target position
                    tempSkillIndicator.position = targetPosition;


                    //projectile rotation
                    difference = tempSkillIndicator.position - player.position; //direction
                    rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
                    tempSkillIndicator.rotation = Quaternion.Euler(0.0f, 0.0f, rotationZ - 90.0f);
                    break;
            }
            joystickDirection = new Vector2(joystick.Horizontal, joystick.Vertical).normalized;
        }

        else
        {
            if (!hasPressed) return;
            hasReleased = true;
            //initialize color
            Color outerColor = outerCircle.color;
            Color innerColor = innerCircle.color;
            outerColor.a = 0;
            innerColor.a = 0;

            //set transparancy
            outerCircle.color = outerColor;
            innerCircle.color = innerColor;

            //set scale
            transform.GetChild(0).localScale = new Vector3(1f, 1f, 1f);

            //disable skill shot
            range.gameObject.SetActive(false);
            tempSkillIndicator.gameObject.SetActive(false);


            //start 
            if (cooldownTimer <= 0 && hasPressed == true && hasReleased == true)
            {
                switch (gmScript.player.playerClass)
                {
                    case "Warrior":
                        if ((gmScript.player.currentMana - 15) >= 0)
                        {  //15
                            gmScript.player.currentMana -= 15;
                            gmScript.UpdateUI();
                            Warriorskill1ServerRpc(gmScript.player.NetworkObjectId);

                            //start skill cooldown
                            cooldownTimer = cooldown;
                            hasPressed = false;
                            hasReleased = false;
                        }
                        break;

                    case "Mage":
                        if ((gmScript.player.currentMana - 10) >= 0)
                        {  //15
                            gmScript.player.currentMana -= 10;
                            gmScript.UpdateUI();
                            MageSkill1ServerRpc(gmScript.player.NetworkObjectId, joystickDirection);

                            //start skill cooldown
                            cooldownTimer = cooldown;
                            hasPressed = false;
                            hasReleased = false;
                        }
                        break;

                    case "Assassin":
                        if ((gmScript.player.currentMana - 10) >= 0)
                        {  //15
                            gmScript.player.currentMana -= 10;
                            gmScript.UpdateUI();
                            AssassinSkill1ServerRpc(gmScript.player.NetworkObjectId, joystickDirection);

                            //start skill cooldown
                            cooldownTimer = cooldown;
                            hasPressed = false;
                            hasReleased = false;
                        }
                        break;
                }

                //start skill cooldown
                cooldownTimer = cooldown;
                hasPressed = false;
                hasReleased = false;
            }
        }
    }


    ///////////////////////////////////////////////////////WARRIOR SKill 1
    [ServerRpc(RequireOwnership = false)]  //1
    public void Warriorskill1ServerRpc(ulong playerID)
    { //
        WarriorSkill1ClientRpc(playerID); //1
    }
    [ClientRpc]   //2
    void WarriorSkill1ClientRpc(ulong playerID)
    { //playerID = the network object id of the player who cast the skill
        foreach (var item in gmScript.players)
        {
            if (item.NetworkObjectId == playerID)
            {
                instantiatedSkill = Instantiate(warriorSkillObject, item.transform.GetChild(3));
                WarriorSkill1SetData(playerID);
                break;
            }
        }
    }

    void WarriorSkill1SetData(ulong playerID)
    {
        instantiatedSkill.GetComponent<WarriorSkill1>().ownerID = playerID; //1
        instantiatedSkill.gameObject.SetActive(true);
    }





    //////////////////////////////////////////////////////MAGE SKILL 1
    [ServerRpc(RequireOwnership = false)]  //1
    public void MageSkill1ServerRpc(ulong playerID, Vector2 joystickDirection)
    { //
        MageSkill1ClientRpc(playerID, joystickDirection); //1
    }
    [ClientRpc]
    void MageSkill1ClientRpc(ulong playerID, Vector2 joystickDirection)
    { //playerID = the network object id of the player who cast the skill
        foreach (var item in gmScript.players)
        {
            if (item.NetworkObjectId == playerID)
            {
                instantiatedSkill = Instantiate(mageSkillObject, inUse);
                instantiatedSkill.localPosition = item.transform.position;
                MageSkill1SetData(playerID, joystickDirection);
                break;
            }
        }
    }

    void MageSkill1SetData(ulong playerID, Vector2 joystickDirection)
    {
        instantiatedSkill.GetComponent<MagicBullet>().playerID = playerID;
        instantiatedSkill.GetComponent<MagicBullet>().direction = joystickDirection;
        instantiatedSkill.gameObject.SetActive(true);

    }


    [ServerRpc(RequireOwnership = false)]
    public void AssassinSkill1ServerRpc(ulong playerID, Vector2 joystickDirection)
    { //
        AssassinSkill1ClientRpc(playerID, joystickDirection); //1
    }
    [ClientRpc]
    void AssassinSkill1ClientRpc(ulong playerID, Vector2 joystickDirection)
    {
        foreach (var item in gmScript.players)
        {
            if (item.NetworkObjectId == playerID)
            {
                instantiatedSkill = Instantiate(AssassinSkillObject, inUse);
                instantiatedSkill.localPosition = item.transform.position;
                instantiatedSkill.GetComponent<AssassinSKill1>().playerID = playerID; ;
                instantiatedSkill.GetComponent<AssassinSKill1>().direction = joystickDirection;

                instantiatedSkill.gameObject.SetActive(true);
                break;
            }
        }
    }
}
