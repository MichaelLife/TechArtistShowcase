using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LifeHMA.Interaction;

public class ExampleOfInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string _interactionPrompt;
    public string InteractionPrompt => _interactionPrompt;

    public bool Interact(Interactor interactor)
    {
        Debug.Log("INTERACTUAR");
        return true;
    }
}
