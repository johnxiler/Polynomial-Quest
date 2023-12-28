using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Options : MonoBehaviour
{
    Transform ui, rankingPanel, textBoxPanel, wiki, glosarryTextBoxPanel;
    [SerializeField] GameObject missionPanel;
    [SerializeField] TextMeshProUGUI menuEnterButtonText;
    [SerializeField] LanGameManager gmScript;
    [SerializeField] Transform lan;
    private void Start()
    { //initialize
        ui = transform.parent;
        rankingPanel = transform.GetChild(1).transform;
        glosarryTextBoxPanel = transform.GetChild(2).GetChild(1).GetChild(0);
    }
    public void OpenOption()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }


    public void ReturnMenu()
    {
        gameObject.SetActive(false);
        ui.GetChild(10).gameObject.SetActive(false); //disable minimap
        ui.GetChild(7).gameObject.SetActive(true); //disable welcome/menu
        //ui.GetChild(7).GetChild(1).gameObject.SetActive(true); //disable welcome/menu
        menuEnterButtonText.SetText("return");

        gmScript.SetMissionPanelState(false);

        if (NetworkManager.Singleton.IsHost)
        {
            lan.GetChild(5).gameObject.SetActive(false); //hide ip input
            lan.GetChild(3).gameObject.SetActive(true);
        }
        else  //client
        {
            lan.GetChild(8).gameObject.SetActive(true); //show backbutton
            lan.GetChild(1).gameObject.SetActive(false); //hide host button
            lan.GetChild(4).gameObject.SetActive(false); //hide join button
            lan.GetChild(5).gameObject.SetActive(false); //hide ip input

            lan.GetChild(2).gameObject.SetActive(true); //show return button
        }
    }

    public void OptionResume()
    {
        ui.GetChild(7).gameObject.SetActive(false); //disable welcome/menu
        ui.GetChild(10).gameObject.SetActive(true); //enable minimap
    }

    public void OpenRankingButton()
    {
        if (rankingPanel.gameObject.activeSelf)
        {
            rankingPanel.gameObject.SetActive(false);
            transform.GetChild(3).gameObject.SetActive(true);
        }
        else
        {
            rankingPanel.gameObject.SetActive(true);
            transform.GetChild(3).gameObject.SetActive(false);
        }
    }

    public void Button0()
    {
        for (int i = 0; i < textBoxPanel.childCount; i++)
        {
            textBoxPanel.GetChild(i).gameObject.SetActive(false);
        }
        textBoxPanel.GetChild(0).gameObject.SetActive(true);
    }

    public void Button1()
    {
        for (int i = 0; i < textBoxPanel.childCount; i++)
        {
            textBoxPanel.GetChild(i).gameObject.SetActive(false);
        }
        textBoxPanel.GetChild(1).gameObject.SetActive(true);
    }

    public void Button2()
    {
        for (int i = 0; i < textBoxPanel.childCount; i++)
        {
            textBoxPanel.GetChild(i).gameObject.SetActive(false);
        }
        textBoxPanel.GetChild(2).gameObject.SetActive(true);
    }

    public void Button3()
    {
        for (int i = 0; i < textBoxPanel.childCount; i++)
        {
            textBoxPanel.GetChild(i).gameObject.SetActive(false);
        }
        textBoxPanel.GetChild(3).gameObject.SetActive(true);
    }

    public void Button4()
    {
        for (int i = 0; i < textBoxPanel.childCount; i++)
        {
            textBoxPanel.GetChild(i).gameObject.SetActive(false);
        }
        textBoxPanel.GetChild(4).gameObject.SetActive(true);
    }

    public void Button5()
    {
        for (int i = 0; i < textBoxPanel.childCount; i++)
        {
            textBoxPanel.GetChild(i).gameObject.SetActive(false);
        }
        textBoxPanel.GetChild(5).gameObject.SetActive(true);
    }

    public void Button6()
    {
        for (int i = 0; i < textBoxPanel.childCount; i++)
        {
            textBoxPanel.GetChild(i).gameObject.SetActive(false);
        }
        textBoxPanel.GetChild(6).gameObject.SetActive(true);
    }

    public void Button7()
    {
        for (int i = 0; i < textBoxPanel.childCount; i++)
        {
            textBoxPanel.GetChild(i).gameObject.SetActive(false);
        }
        textBoxPanel.GetChild(7).gameObject.SetActive(true);
    }

    public void Button8()
    {
        for (int i = 0; i < textBoxPanel.childCount; i++)
        {
            textBoxPanel.GetChild(i).gameObject.SetActive(false);
        }
        textBoxPanel.GetChild(8).gameObject.SetActive(true);
    }

    public void Button9()
    {
        for (int i = 0; i < textBoxPanel.childCount; i++)
        {
            textBoxPanel.GetChild(i).gameObject.SetActive(false);
        }
        textBoxPanel.GetChild(9).gameObject.SetActive(true);
    }

    public void OpenWiki()
    {
        wiki.gameObject.SetActive(true);
        missionPanel.gameObject.SetActive(false);
        transform.GetChild(4).gameObject.SetActive(false);
    }

    public void CloseWiki()
    {
        wiki.gameObject.SetActive(false);
        missionPanel.SetActive(true);
        transform.GetChild(4).gameObject.SetActive(true);
    }


    public void OpenGlosarry()
    {
        missionPanel.SetActive(false);
        transform.GetChild(2).gameObject.SetActive(true);
        transform.GetChild(3).gameObject.SetActive(false);
    }
    public void CloseGlosarry()
    {
        //missionPanel.SetActive(false);
        transform.GetChild(2).gameObject.SetActive(false);
        transform.GetChild(3).gameObject.SetActive(true);
        missionPanel.SetActive(true);
    }

    public void Glosarry1()
    {
        for (int i = 0; i < glosarryTextBoxPanel.childCount; i++)
        {
            glosarryTextBoxPanel.GetChild(i).gameObject.SetActive(false);
        }
        glosarryTextBoxPanel.GetChild(0).gameObject.SetActive(true);
    }

    public void Glosarry2()
    {
        for (int i = 0; i < glosarryTextBoxPanel.childCount; i++)
        {
            glosarryTextBoxPanel.GetChild(i).gameObject.SetActive(false);
        }
        glosarryTextBoxPanel.GetChild(1).gameObject.SetActive(true);
    }

    public void Glosarry3()
    {
        for (int i = 0; i < glosarryTextBoxPanel.childCount; i++)
        {
            glosarryTextBoxPanel.GetChild(i).gameObject.SetActive(false);
        }
        glosarryTextBoxPanel.GetChild(2).gameObject.SetActive(true);
    }

    public void Glosarry4()
    {
        for (int i = 0; i < glosarryTextBoxPanel.childCount; i++)
        {
            glosarryTextBoxPanel.GetChild(i).gameObject.SetActive(false);
        }
        glosarryTextBoxPanel.GetChild(3).gameObject.SetActive(true);
    }
}
