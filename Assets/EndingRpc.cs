using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EndingRpc : NetworkBehaviour
{
    [SerializeField] Ending ending;

    [ServerRpc(RequireOwnership = false)]
    public void StartDialogue1ServerRpc()
    {
        StartDialogue1ClientRpc();
    }
    [ClientRpc]
    void StartDialogue1ClientRpc()
    {
        StartCoroutine(ending.WilsonDialogue1());
    }


    [ServerRpc(RequireOwnership = false)]
    public void StartDialogue2ServerRpc()
    {
        StartDialogue2ClientRpc();
    }
    [ClientRpc]
    void StartDialogue2ClientRpc()
    {
        StartCoroutine(ending.EmmanuelDialogue1());
    }


    [ServerRpc(RequireOwnership = false)]
    public void StartDialogue3ServerRpc()
    {
        StartDialogue3ClientRpc();
    }
    [ClientRpc]
    void StartDialogue3ClientRpc()
    {
        StartCoroutine(ending.WilsonDialogue2());
    }


    [ServerRpc(RequireOwnership = false)]
    public void StartDialogue4ServerRpc()
    {
        StartDialogue4ClientRpc();
    }
    [ClientRpc]
    void StartDialogue4ClientRpc()
    {
        StartCoroutine(ending.EmmanuelDialogue2());
    }


    [ServerRpc(RequireOwnership = false)]
    public void StartDialogue5ServerRpc()
    {
        StartDialogue5ClientRpc();
    }
    [ClientRpc]
    void StartDialogue5ClientRpc()
    {
        StartCoroutine(ending.WilsonDialogue3());
    }
}
