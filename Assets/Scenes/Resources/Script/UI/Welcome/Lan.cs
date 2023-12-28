using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Net;
using System.Net.Sockets;
using TMPro;
using System.Collections;




public class Lan : MonoBehaviour
{
	[SerializeField] TextMeshProUGUI ipAddressLabel, welcomeBackText, backButtonText;
	[SerializeField] TMP_InputField ipInput;
	[SerializeField] private NetworkPrefabsList _networkPrefabsList;
	[SerializeField] string ipAddress;
	[SerializeField] UnityTransport transport;
	public GameObject welcome, startClient, startHost, enter, difficultyPanel, createCharacterButton, ui, backButton, mapsParent;
	public LanCreateCharacter characterCreation;
	[SerializeField] LanGameManager gmScript;
	bool hasCharacter;
	string tempUsername;
	float finishIntroClient;
	LanNpc[] npcs;
	[SerializeField] AudioListener menuAudioListener;

	void Start()
	{

		ipAddress = "0.0.0.0";
		SetIpAddress(); // Set the Ip to the above address
						//InvokeRepeating("assignPlayerController", 0.1f, 0.1f);
						//RegisterNetworkPrefabs();
		finishIntroClient = PlayerPrefs.GetInt("finishIntro");
		npcs = FindObjectsOfType<LanNpc>();

		backButton = transform.GetChild(8).gameObject;
		backButtonText = backButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
	}

	private void OnEnable()
	{
		if ((tempUsername = PlayerPrefs.GetString("username")) != "")
		{
			hasCharacter = true;
			ipInput.gameObject.SetActive(true);
			welcomeBackText.gameObject.SetActive(true);
			welcomeBackText.SetText("Welcome back, " + tempUsername + "\n    Level: " + PlayerPrefs.GetInt("level"));
		}
	}


	// To Host a game
	public void StartHost()
	{
		//reset to null error msg
		ipAddressLabel.color = Color.white;
		ipAddressLabel.SetText("");
		//disable menu audio listener
		menuAudioListener.enabled = false;

		NetworkManager.Singleton.StartHost();
		GetLocalIPAddress();
		difficultyPanel.SetActive(false);
		enter.SetActive(true); //show the enter button

		//disable menu audio listener
		menuAudioListener.enabled = false;

		//instantiate maps
		gmScript.InstantiateMaps();

		BackButtonSetText();
		enter.transform.GetChild(0).GetComponent<TextMeshProUGUI>().SetText("enter");
		ipAddressLabel.gameObject.SetActive(true);
	}


	public void StartClient()
	{
		// // IP address is reachable, proceed with starting the client
		ipAddress = ipInput.text;

		//set to default
		ipAddressLabel.color = Color.white;
		ipAddressLabel.SetText("");



		//bool canConnect = VerifyConnection();

		if (hasCharacter == false)
		{
			welcomeBackText.SetText("no player data found, create new character");
			welcomeBackText.gameObject.SetActive(true);
			createCharacterButton.SetActive(true);
		}
		else if (ipInput.text == "")
		{
			ipAddressLabel.color = Color.red;
			ipAddressLabel.SetText("Error: IP is empty!");
			return;
		}
		else //attempt to connect
		{
			Debug.Log("Connecting at: " + ipAddress);
			SetIpAddress();
			NetworkManager.Singleton.StartClient();

			StartCoroutine(TryConnect()); //verify connection status
		}


	}




	/* Gets the Ip Address of your connected network and
	shows on the screen in order to let other players join
	by inputing that Ip in the input field */
	// ONLY FOR HOST SIDE 
	public void GetLocalIPAddress()
	{
		var host = Dns.GetHostEntry(Dns.GetHostName());

		var count = 0;
		foreach (var ip in host.AddressList)
		{
			count++;
			if (count == host.AddressList.Length)
			{
				ipAddressLabel.SetText(ip.ToString()); //show the IP
													   //ipAddress = ip.ToString();
			}
		}
	}

	/* Sets the Ip Address of the Connection Data in Unity Transport
	to the Ip Address which was input in the Input Field */
	// ONLY FOR CLIENT SIDE
	public void SetIpAddress()
	{
		transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
		transport.ConnectionData.Address = ipAddress;
	}


	public void ButtonPressedEnter()
	{
		if (gmScript.HasFinishTraining() == 1)
		{
			gmScript.player.StartIntroduction();
		}
		else
		{
			welcome.SetActive(false);
			gmScript.EnableMinimap(true);
		}
		gmScript.StartBackgroundMusic();
		gmScript.SetMissionPanelState(true);
	}

	public void ButtonHost()
	{
		string usernametemp = PlayerPrefs.GetString("username");
		if (usernametemp != "")
		{
			welcomeBackText.gameObject.SetActive(true); //enable welcomeback Object
			welcomeBackText.SetText("Welcome back, " + PlayerPrefs.GetString("username") + "\n    Level: " + PlayerPrefs.GetInt("level"));

			ipInput.gameObject.SetActive(false); //hide the ip inputBox
			startClient.SetActive(false); //hide the start Client Button
			startHost.SetActive(false); //hide the start Host Button
			difficultyPanel.SetActive(true); //show select difficulty

			BackButtonSetText();
			backButton.SetActive(true); //show back button
		}
		else
		{
			welcomeBackText.SetText("no player data found, create new character");
			createCharacterButton.SetActive(true);
		}
	}

	public void CreateCharacterPressed()
	{
		ui.SetActive(false);
		characterCreation.gameObject.SetActive(true);
	}


	//testing
	IEnumerator TryConnect()
	{
		yield return new WaitForSeconds(1);
		if (NetworkManager.Singleton.IsConnectedClient) //connecting is success
		{
			menuAudioListener.enabled = false; //disable menu audio listener
			welcome.SetActive(false);
			gmScript.SetMissionPanelState(true);
			gmScript.EnableMinimap(true);

			BackButtonSetText();
		}
		else
		{
			NetworkManager.Singleton.Shutdown();
			ipAddressLabel.color = Color.red;
			ipAddressLabel.SetText("Connection failed");
		}
	}

	public void BackToSelection()
	{
		if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient) //stop host 
		{
			gmScript.StopWeatherCourotines();
			gmScript.DeleteInstantiatedMaps();
			NetworkManager.Singleton.Shutdown();
			menuAudioListener.enabled = true; //enable menu audio listener
			ipAddressLabel.gameObject.SetActive(false);
			enter.SetActive(false);

		}
		ipInput.gameObject.SetActive(true); //hide the ip inputBox
		startClient.SetActive(true); //hide the start Client Button
		startHost.SetActive(true); //hide the start Host Button
		difficultyPanel.SetActive(false); //show select difficulty

		backButton.SetActive(false); //hide back button
	}

	void BackButtonSetText()
	{
		if (NetworkManager.Singleton.IsHost)
		{
			backButtonText.SetText("Stop Host");
		}
		else if (NetworkManager.Singleton.IsClient)
		{
			backButtonText.SetText("disconnect");
		}
		else
		{
			backButtonText.SetText("back");
		}
	}
}
