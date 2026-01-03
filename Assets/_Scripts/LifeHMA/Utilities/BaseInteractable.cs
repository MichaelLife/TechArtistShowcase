using UnityEngine;
using LifeHMA.Interaction;
using System.Collections.Generic;
using UnityEngine.Events;

namespace LifeHMA.Interaction
{
    public class BaseInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _interactionPrompt;
        public string InteractionPrompt => _interactionPrompt;

        public UnityEvent interactionActions;

        public bool Interact(Interactor interactor)
        {
            OnInteract();
            return true;
        }

        public virtual void OnInteract()
        {
            interactionActions.Invoke();
        }
    }
}
