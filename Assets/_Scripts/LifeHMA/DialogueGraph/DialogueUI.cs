using UnityEngine;
using UnityEngine.UI;
using Febucci.TextAnimatorForUnity;
using System.Linq;
using System.Collections;
using System;
using TMPro;
using Febucci.TextAnimatorForUnity.TextMeshPro; 


namespace LifeHMA.Dialogue
{
    public enum TextPosition
    {
        Top,
        Bottom
    }
    public enum CharacterPosition
    {
        Left,
        Right
    }
    public enum TextBubbleState
    {
        Hidden,
        Shown,
        Darken
    }

    [Serializable]
    public class DialogueBubble
    {
        public Image background;
        public TypewriterComponent speaker;
        public TypewriterComponent text;

        [HideInInspector] public RectTransform backgroundRT;
        [HideInInspector] public TextMeshProUGUI speakerRT;
        [HideInInspector] public TextMeshProUGUI textRT;

        [HideInInspector] public TextBubbleState state;
    }

    public class DialogueUI : MonoBehaviour
    {
        [Header("DIALOGUE BUBBLES")]
        [SerializeField] private DialogueBubble topBubble;
        [SerializeField] private DialogueBubble botBubble;
        private TextPosition currentBubble;

        [SerializeField] private float bubbleTransitionTime = 0.25f;
        [SerializeField] private float bubbleShowTime = 0.5f;

        [Header("CHARACTERS UI")]
        [SerializeField] private RawImage leftCharacter, rightCharacter;
        [SerializeField] private float characterTransitionTime = 0.5f;

        //Current speakers for each side, and the character that is speaking at the moment
        private string LeftSpeaker, RightSpeaker, currentSpeaker;

        //If the text is been written with the typewritter
        public bool isWritingText;

        [Header("BUTTON PREFAB")]
        [SerializeField] private GameObject choiceButton;

        [Header("PARENTS")]
        [SerializeField] private GameObject buttonParent;
        [SerializeField] private GameObject dialogueBubblesParent;
        public GameObject Canvas;
        private CanvasGroup dialogueUIGroup;



        private void Start()
        {
            //Get componets for bubbles
            botBubble.speakerRT = botBubble.speaker.GetComponent<TextMeshProUGUI>();
            botBubble.textRT = botBubble.text.GetComponent<TextMeshProUGUI>();
            botBubble.backgroundRT = botBubble.background.GetComponent<RectTransform>();

            topBubble.speakerRT = topBubble.speaker.GetComponent<TextMeshProUGUI>();
            topBubble.textRT = topBubble.text.GetComponent<TextMeshProUGUI>();
            topBubble.backgroundRT = topBubble.background.GetComponent<RectTransform>();

            dialogueUIGroup = Canvas.GetComponent<CanvasGroup>();

            //Add listeners so that isWriting updates correctly
            topBubble.text.onTypewriterStart.AddListener(() => WritingStart());
            topBubble.text.onTextShowed.AddListener(() => WritingStop());

            //If UI is active, hide it
            if(gameObject.activeSelf) Canvas.SetActive(false);
        }

        private void WritingStart() => isWritingText = true;
        private void WritingStop() => isWritingText = false;
        public void SkipText()
        {
            isWritingText = false;
            topBubble.text.SkipTypewriter();
            topBubble.speaker.SkipTypewriter();
            botBubble.text.SkipTypewriter();
            botBubble.speaker.SkipTypewriter();
        }

        //Funtion that set the text from the dialogue manager to the UI and triggers the animations
        public IEnumerator SetTextToUI(TextPosition pos, string speaker, string text, bool hideBubbles, WaitForSeconds wait = null)
        {
            DialogueBubble _bubble;

            if (pos == TextPosition.Bottom)
            {
                _bubble = botBubble;
                currentBubble = TextPosition.Bottom;
            }
            else
            {
                _bubble = topBubble;
                currentBubble = TextPosition.Top;
            }

            yield return wait; //Custom wait time is set in dialogue node tree

            //Updates text bubble animations
            botBubble.state = UpdateTextBubbleState(botBubble.state, currentBubble, TextPosition.Bottom, botBubble, hideBubbles);
            topBubble.state = UpdateTextBubbleState(topBubble.state, currentBubble, TextPosition.Top, topBubble, hideBubbles);

            //Waits for small time to catch up to the animations 
            yield return new WaitForSeconds(0.3f);

            //Reset bubble text to avoid Febucci text animator flashing old text for one frame
            _bubble.text.ShowText(" ");

            //If speaker name has changed => Update speaker name 
            if (speaker != _bubble.speaker.TextAnimator.textFull)
                _bubble.speaker.ShowText(speaker);

            //Show text message
            _bubble.text.ShowText(text);

            //Resize text bubble to fit text message
            ResizeTextBubble(_bubble, bubbleTransitionTime);
        }


        #region UPDATE DIALOGUE UI
        public void UpdateCharactersUI(CharacterPosition charPos, string speaker, bool showSpeaker, bool listener)
        {
            //Get variables from the side that is to be updated
            string updateSpeaker = RightSpeaker;
            RawImage updateCharacter = rightCharacter;
            if (charPos == CharacterPosition.Left) { updateSpeaker = LeftSpeaker; updateCharacter = leftCharacter; }

            if (string.IsNullOrEmpty(updateSpeaker) && showSpeaker) //If there is not a speaker in this side
            {
                //Show speaker
                if (!listener) ShowCharacter(updateCharacter, characterTransitionTime);
                else DarkenCharacter(updateCharacter, characterTransitionTime);
            }
            else //If there is a speaker in this side, check if it has changed
            {
                if (!showSpeaker) 
                {
                    //Hide speaker
                    HideCharacter(updateCharacter, characterTransitionTime);
                }
                else if (updateSpeaker != speaker) //If speaker has changed, perform change animation
                {
                    //Change speaker animation
                    StartCoroutine(ChangeCharacters(updateCharacter, charPos));
                }
                else if (listener) //If speaker hasn't changed, but it is a listener
                {
                    //Oscurecer speaker
                    DarkenCharacter(updateCharacter, characterTransitionTime);
                }
                else //If speaker hasn't changed, but it's speaking
                {
                    //Show speaker
                    ShowCharacter(updateCharacter, characterTransitionTime);
                }
            }

            //Update the speaker names and save them for future character UI updates
            if (showSpeaker)
            {
                if (charPos == CharacterPosition.Left) LeftSpeaker = speaker;
                else RightSpeaker = speaker;
                if (!listener) currentSpeaker = speaker;
            }
            else
            {
                if (charPos == CharacterPosition.Left) LeftSpeaker = "";
                else RightSpeaker = "";
                if (!listener) currentSpeaker = "";
            }
        }

        private TextBubbleState UpdateTextBubbleState(TextBubbleState _bubbleState, TextPosition _currentSpeaker, TextPosition me, DialogueBubble _bubble, bool hideBubbles)
        {
            if (currentBubble == me) //If the bubble currently speaking is me
            {
                //If the bubble is hidden or dark, show bubble
                if (_bubbleState == TextBubbleState.Hidden || _bubbleState == TextBubbleState.Darken) ShowTextBubble(_bubble, bubbleShowTime);

                _bubbleState = TextBubbleState.Shown;
            }
            else //If the bubble speaking is not me
            {
                //Check if the bubble has to hide or darken
                if (hideBubbles)
                {
                    if (_bubbleState != TextBubbleState.Hidden)
                    {
                        HideTextBubble(_bubble, bubbleShowTime);
                        _bubbleState = TextBubbleState.Hidden;
                    }
                }
                else
                {
                    if (_bubbleState != TextBubbleState.Darken)
                    {
                        DarkenTextBubble(_bubble, bubbleTransitionTime);
                        _bubbleState = TextBubbleState.Darken;
                    }
                }
            }

            //Update bubble state
            return _bubbleState;
        }

        #endregion

        #region START AND END DIALOGUE
        public void StartDialogue()
        {
            //Reset all elements
            ResetTextBubbles();
            ResetCharacterUI();

            ResizeTextBubble(botBubble, 0f);
            ResizeTextBubble(topBubble, 0f);

            CanvasStartAnimation();
        }

        public void EndDialogue()
        {
            CanvasEndAnimation();
        }

        #region RESETERS

            private void ResetTextBubbles()
            {
                topBubble.text.TextAnimator.SetText(" ");
                botBubble.text.TextAnimator.SetText(" ");
                topBubble.speaker.TextAnimator.SetText(" ");
                botBubble.speaker.TextAnimator.SetText(" ");

                if (topBubble.state == TextBubbleState.Darken) ShowTextBubble(topBubble, 0);
                HideTextBubble(topBubble, 0);
                topBubble.state = TextBubbleState.Hidden;

                if (botBubble.state == TextBubbleState.Darken) ShowTextBubble(botBubble, 0);
                HideTextBubble(botBubble, 0);
                botBubble.state = TextBubbleState.Hidden;
            }

            private void ResetCharacterUI()
            {
                HideCharacter(leftCharacter, 0);
                HideCharacter(rightCharacter, 0);

                LeftSpeaker = "";
                RightSpeaker = "";
                currentSpeaker = "";
            }

        #endregion

        #endregion

        #region BUTTONS
        public void AddButton(DialogueManager manager, int i, string optionString)
        {
            var _g = (GameObject)Instantiate(choiceButton, buttonParent.transform);
            var _button = _g.GetComponent<UnityEngine.UI.Button>();

            _button.onClick.AddListener(() => manager.OnChoiceClick(i));
            TextAnimator_TMP _text = _g.GetComponentInChildren<TextAnimator_TMP>();
            _text.SetText(optionString);
        }

        private void RemoveButtons() { foreach (Transform child in buttonParent.transform) { Destroy(child.gameObject); } }
        #endregion

        #region UI ANIMATIONS

            #region BUTTON ANIMATIONS
            public void StartAnimationButtons() => LeanTween.moveLocalY(dialogueBubblesParent, 125.0f, 0.5f);
            public void EndAnimationButtons() { LeanTween.moveLocalY(dialogueBubblesParent, 0f, 0.75f); Invoke("RemoveButtons", 0.75f); }
            #endregion

            #region TEXT BUBBLE ANIMATIONS
            private void ResizeTextBubble(DialogueBubble _bubble, float time)
            {
                int _lineCount = _bubble.textRT.textInfo.lineCount;
                //Tamaño base de la burbuja + tamaño de cada línea * número de líneas
                Vector2 _to = new Vector2(1000, 171.78f)
                    + (_lineCount * new Vector2(0, 41.4f));

                //Restarle el número de caracteres si es de solo una línea
                if (_lineCount == 1)
                { _to -= (46 - Mathf.Max(_bubble.textRT.text.Length, _bubble.speakerRT.text.Length + 2)) * new Vector2(19.5f, 0); }

                LeanTween.LeanRectTransformDeltaSize(_bubble.backgroundRT, _to, time);
            }
            private void ShowTextBubble(DialogueBubble _bubble, float time)
            {
                TextBubbleBaseAnimation(_bubble, time, time / 1.5f, 1f, 0.0f);
            }
            private void HideTextBubble(DialogueBubble _bubble, float time)
            {
                float _mult = -1f;
                if (_bubble == botBubble) _mult = 1f;

                TextBubbleBaseAnimation(_bubble, time, time, 0f, _mult * 25.0f);
            }
            private void DarkenTextBubble(DialogueBubble _bubble, float time)
            {
                TextBubbleBaseAnimation(_bubble, time, time/1.5f, 0.5f, 0.0f);
            }

            private void TextBubbleBaseAnimation(DialogueBubble _bubble, float time, float rotateTime, float alpha, float Zangle)
            {
                LeanTween.alpha(_bubble.backgroundRT, alpha, time);
                LeanTween.LeanTMPAlpha(_bubble.speakerRT, alpha, time);
                LeanTween.LeanTMPAlpha(_bubble.textRT, alpha, time);

                LeanTween.rotateZ(_bubble.backgroundRT.gameObject, Zangle, rotateTime).setEaseInOutBack();
            }
            #endregion

            #region CHARACTER ANIMATIONS
            private void ShowCharacter(RawImage _character, float time)
            {
                CharacterUIBaseAnimation(_character, time, 1f, -560f, Vector3.zero);
            }
            private void HideCharacter(RawImage _character, float time)
            {
                float _mult = -1f;
                if (_character == rightCharacter) _mult = 1f;

                CharacterUIBaseAnimation(_character, time, 0f, -760f, new Vector3(0, _mult * 25, 0));
            }
            private void DarkenCharacter(RawImage _character, float time)
            {
                CharacterUIBaseAnimation(_character, time, 0.5f, -660f, Vector3.zero);
            }
            private void CharacterUIBaseAnimation(RawImage _character, float time, float alpha, float localY, Vector3 localRot)
            {
                LeanTween.AlphaRawImage(_character, alpha, time);
                LeanTween.rotateLocal(_character.gameObject, localRot, time);
                LeanTween.moveLocalY(_character.gameObject, localY, time);
            }
            private IEnumerator ChangeCharacters(RawImage _character, CharacterPosition pos)
            {
                HideCharacter(_character, characterTransitionTime);
                yield return new WaitForSeconds(characterTransitionTime + 0.1f);
                ShowCharacter(_character, characterTransitionTime);
            }
        #endregion

            private void CanvasStartAnimation() => LeanTween.alphaCanvas(dialogueUIGroup, 1.0f, 0.5f);
            private void CanvasEndAnimation() => LeanTween.alphaCanvas(dialogueUIGroup, 0.0f, 0.5f).setOnComplete(() => gameObject.SetActive(false)); //Disable UI after animation


        #endregion
    }
}
