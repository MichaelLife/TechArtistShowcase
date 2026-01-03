using UnityEngine;
using LifeHMA.Interaction;
using System.Collections.Generic;
using UnityEngine.Events;
using DialogueSystem;

namespace LifeHMA.Interaction
{
    public class DialogueInteractable : BaseInteractable
    {
        public DialogueTreeSO dialogue;
        [SerializeField] private bool immovilizePlayer = true;

        private void Start()
        {
            interactionActions.AddListener(DialogueAction);
        }

        public override void OnInteract()
        {
            base.OnInteract();
        }

        private void DialogueAction()
        {
            DialogueManager.instance.StartOrContinueDialogue(dialogue, immovilizePlayer);
        }
    }
}
