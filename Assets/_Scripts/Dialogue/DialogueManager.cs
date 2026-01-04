using UnityEngine;
using DialogueSystem;
using UnityEngine.UIElements;
using Febucci.TextAnimatorForUnity;
using LifeHMA.Dialogue;
using LifeHMA.Player;
using System.Collections.Generic;

public class DialogueManager : DialogueUIManager
{
    public DialogueTreeSO currentDialogue;
    private BasicPlayerMovement player;

    [Header("Dialogue UI")]
    public DialogueUI dialogueUI;
    private VisualElement root;
    private AnimatedLabel speakerLabel;
    private AnimatedLabel textLabel;

    [SerializeField] private Language language;

    public static DialogueManager instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        player = GameObject.FindAnyObjectByType<BasicPlayerMovement>().GetComponent<BasicPlayerMovement>();
    }

    public void StartOrContinueDialogue(DialogueTreeSO currentDialogue, bool immobilizePlayer = true)
    {
        if (dialogueManager.DialogueStarted)
        {
            if (dialogueUI.isWritingText)
            {
                dialogueUI.SkipText();
            }
            else
            {
                this.currentDialogue = currentDialogue;
                dialogueManager.NextNode();
            }
        }
        else
        {
            dialogueUI.Canvas.SetActive(true);
            player.Immobilize(immobilizePlayer);
            this.currentDialogue = currentDialogue;
            StartDialogue();
        }
    }

    public void StartDialogue()
    {
        dialogueUI.gameObject.SetActive(true);
        dialogueUI.StartDialogue();
        dialogueManager.StartDialogue(currentDialogue);
    }

    public override void OnChoiceNode(ChoiceNode choiceNode)
    {
        string _speaker = choiceNode.Speaker.CharacterName;
        string _text = choiceNode.GetMessage(language);

        UpdateCharacters(choiceNode.Speaker, choiceNode.Listeners);

        SetTextMessage(_speaker, _text, choiceNode.TextPosition, choiceNode.HideNonSpeakerBubble, choiceNode.WaitTime);

        dialogueUI.StartAnimationButtons();

        for (int i = 0; i < choiceNode.Choices.Count; i++)
        {
            dialogueUI.AddButton(this, i, choiceNode.Choices[i].GetMessage(language));
        }
    }

    public void OnChoiceClick(int index)
    {
        dialogueManager.NextNode(index); // Move to the next node using the choice index

        //REMOVE BUTTONS
        dialogueUI.EndAnimationButtons();
    }


    public override void OnDialogueNode(DialogueNode dialogueNode)
    {
        string _speaker = dialogueNode.Speaker.CharacterName;
        string _text = dialogueNode.GetMessage(language);

        UpdateCharacters(dialogueNode.Speaker, dialogueNode.Listeners);

        SetTextMessage(_speaker, _text, dialogueNode.TextPosition, dialogueNode.HideNonSpeakerBubble, dialogueNode.WaitTime);
    }

    private void UpdateCharacters(Character speaker, List<Character> listeners)
    {
        UpdateCharacter(speaker, false);

        foreach(Character listener in listeners)
        {
            UpdateCharacter(listener, true);
        }
    }

    private void UpdateCharacter(Character _character, bool listener)
    {
        LifeHMA.Dialogue.CharacterPosition pos;
        if (_character.DialogueCharacterPosition == DialogueSystem.DialogueCharacterPosition.Left) pos = LifeHMA.Dialogue.CharacterPosition.Left;
        else pos = LifeHMA.Dialogue.CharacterPosition.Right;

        dialogueUI.UpdateCharactersUI(pos, _character.CharacterName, _character.ShowCharater, listener);
    }

    private void SetTextMessage(string _speaker, string _text, DialogueSystem.TextPosition nodePos, bool _hideBubble, float waitTime)
    {
        LifeHMA.Dialogue.TextPosition pos;
        if (nodePos == DialogueSystem.TextPosition.Top) pos = LifeHMA.Dialogue.TextPosition.Top;
        else pos = LifeHMA.Dialogue.TextPosition.Bottom;

        StartCoroutine(dialogueUI.SetTextToUI(pos, _speaker, _text, _hideBubble, new WaitForSeconds(waitTime)));
    }

    public override void OnEventNode(EventNode eventNode, DialogueSystem.DialogueEventManager dialogueEventManager)
    {
        dialogueEventManager.Invoke(eventNode.DialogueEvent);

        dialogueManager.NextNode();
    }

    public override void OnIfNode(IfNode ifNode)
    {
        dialogueManager.NextNode();
    }
    public override void OnEndNode(EndNode endNode)
    {
        player.Immobilize(false);
        currentDialogue = endNode.NextDialogue;
        dialogueUI.EndDialogue();
    }
}
