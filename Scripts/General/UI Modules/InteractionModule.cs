using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionModule : UIModule
{
    InteractableObject lastInteractable;

    public TMP_Text interactText;

    public Image interactFill;

    public RectTransform interactRoot;

    private void Start()
    {
        interactRoot.gameObject.SetActive(false);
    }

    public override void UpdateModule()
    {

        if(Player.currentInteractTarget != null )
        {
            if (lastInteractable != Player.currentInteractTarget && Player.currentInteractTarget.displayInteractText)
            {
                interactText.text = Player.currentInteractTarget.interactText;
            }
            if (Player.currentInteractTarget.holdToInteract)
            {
                interactFill.fillAmount = Mathf.InverseLerp(0, Player.currentInteractTarget.interactTime, Player.currentInteractTarget.currentInteractTime);
            }
        }
        lastInteractable = Player.currentInteractTarget;
        interactRoot.gameObject.SetActive(Player.currentInteractTarget != null && Player.currentInteractTarget.displayInteractText);

    }
}
