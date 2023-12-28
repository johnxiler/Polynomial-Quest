using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanContinueButton : MonoBehaviour
{
    [SerializeField] LanInteractionManager interaction;

    public void ButtonPressed()
    {
        interaction.Continue();
    }
}
