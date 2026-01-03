using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
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

    [Serializable]
    public class DialogueBubble
    {
        public UnityEngine.UI.Image background;
        public TypewriterComponent speaker;
        public TypewriterComponent text;

        [HideInInspector]
        public RectTransform backgroundRT;
        public TextMeshProUGUI speakerRT, textRT;
    }

    public class DialogueUI : MonoBehaviour
    {
        private enum TextBubbleState
        {
            Hidden,
            Shown,
            Darken
        }

        private VisualElement root, charRoot;
        public UIDocument CharactersUI;
        AnimatedLabel TopSpeakerLabel, TopTextLabel, BotSpeakerLabel, BotTextLabel;

        VisualElement TopBubble, BotBubble, ButtonContainer;
        UnityEngine.UIElements.Image LeftCharacter, RightCharacter;

        private TextPosition currentBubble;
        private TextBubbleState topBubbleState, botBubbleState;
        private string LeftSpeaker, RightSpeaker;
        private string currentSpeaker;

        public bool isWritingText;

        //>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
        public DialogueBubble topBubble, botBubble;
        public float bubbleTransitionTime = 0.25f;
        public GameObject buttonParent;
        public GameObject dialogueBubblesParent;
        public GameObject choiceButton;
        public CanvasGroup dialogueUIGroup;

        private void Start()
        {
            botBubble.speakerRT = botBubble.speaker.GetComponent<TextMeshProUGUI>();
            botBubble.textRT = botBubble.text.GetComponent<TextMeshProUGUI>();
            botBubble.backgroundRT = botBubble.background.GetComponent<RectTransform>();

            topBubble.speakerRT = topBubble.speaker.GetComponent<TextMeshProUGUI>();
            topBubble.textRT = topBubble.text.GetComponent<TextMeshProUGUI>();
            topBubble.backgroundRT = topBubble.background.GetComponent<RectTransform>();

            topBubble.text.onTypewriterStart.AddListener(() => WritingStart());
            topBubble.text.onTextShowed.AddListener(() => WritingStop());

            gameObject.SetActive(false);
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

        
        private void ResetTextBubbles()
        {
            topBubble.text.TextAnimator.SetText(" ");
            botBubble.text.TextAnimator.SetText(" ");
            topBubble.speaker.TextAnimator.SetText(" ");
            botBubble.speaker.TextAnimator.SetText(" ");

            if (topBubbleState == TextBubbleState.Darken) ShowTextBubble(topBubble, 0);
            HideTextBubble(topBubble, 0);
            topBubbleState = TextBubbleState.Hidden;

            if (botBubbleState == TextBubbleState.Darken) ShowTextBubble(botBubble, 0);
            HideTextBubble(botBubble, 0);
            botBubbleState = TextBubbleState.Hidden;
        }

        /*
        private void ResetCharacterUI()
        {
            LightCharacter(LeftCharacter);
            HideCharacter(LeftCharacter, CharacterPosition.Left);

            LightCharacter(RightCharacter);
            HideCharacter(RightCharacter, CharacterPosition.Right);

            LeftSpeaker = "";
            RightSpeaker = "";
            currentSpeaker = "";
        }
        */
        public void StartDialogue()
        {
            ResetTextBubbles();

            ResizeTextBubble(botBubble, 0f);
            ResizeTextBubble(topBubble, 0f);

            LeanTween.alphaCanvas(dialogueUIGroup, 1.0f, 0.5f);
        }

        public void EndDialogue()
        {
            ResizeTextBubble(botBubble, 0f);
            ResizeTextBubble(topBubble, 0f);

            LeanTween.alphaCanvas(dialogueUIGroup, 0.0f, 0.5f).setOnComplete(() => gameObject.SetActive(false));
        }

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

            yield return wait;

            botBubbleState = UpdateTextBubbleState(botBubbleState, currentBubble, TextPosition.Bottom, botBubble, hideBubbles);
            topBubbleState = UpdateTextBubbleState(topBubbleState, currentBubble, TextPosition.Top, topBubble, hideBubbles);

            yield return new WaitForSeconds(0.3f);
            _bubble.text.ShowText(" ");

            if(speaker != _bubble.speaker.TextAnimator.textFull)
                _bubble.speaker.ShowText(speaker);

            _bubble.text.ShowText(text);

            ResizeTextBubble(_bubble, bubbleTransitionTime);
        }

        private void ResizeTextBubble(DialogueBubble _bubble, float time)
        {
            int _lineCount = _bubble.textRT.textInfo.lineCount;
            //Tamaño base de la burbuja + tamaño de cada línea * número de líneas
            Vector2 _to = new Vector2(1000, 171.78f)
                + (_lineCount * new Vector2(0, 41.4f));
            
            //Restarle el número de caracteres si es de solo una línea
            if(_lineCount == 1) 
            { _to -= (46 - Mathf.Max(_bubble.textRT.text.Length, _bubble.speakerRT.text.Length + 2)) * new Vector2(19.5f, 0); }

            LeanTween.LeanRectTransformDeltaSize(_bubble.backgroundRT, _to, time);
        }

        public void UpdateCharactersUI(CharacterPosition charPos, string speaker, bool showSpeaker, bool listener)
        {
            /*
            string updateSpeaker = RightSpeaker;
            Image updateCharacter = RightCharacter;
            if (charPos == CharacterPosition.Left) { updateSpeaker = LeftSpeaker; updateCharacter = LeftCharacter; }

            if (string.IsNullOrEmpty(updateSpeaker) && showSpeaker) //Si no hay speaker en el lado que toca
            {
                //Show speaker
                ShowCharacter(updateCharacter, charPos);
                if(listener) DarkenCharacter(updateCharacter);
            }
            else //Si hay speaker en ese lado, comprobar si el speaker ha cambiado
            {
                if(!showSpeaker) 
                {
                    //Hide speaker
                    //Light speaker
                    LightCharacter(updateCharacter);
                    HideCharacter(updateCharacter, charPos);
                }
                else if (updateSpeaker != speaker) //Si ha cambiado el speaker, hacer animación de cambio
                {
                    //Light speaker
                    //Cambiar speaker
                    LightCharacter(updateCharacter);
                    StartCoroutine(ChangeCharacters(updateCharacter, charPos));
                }
                else if(listener) //Si speaker no ha cambiado ni se ha escondido, pero no está hablando
                {
                    //Oscurecer speaker
                    DarkenCharacter(updateCharacter);
                }
                else //Si el speaker es el mismo pero no esta escuchando
                {
                    //Light speaker
                    LightCharacter(updateCharacter);
                }
            }
            if (showSpeaker)
            {
                if (charPos == CharacterPosition.Left) LeftSpeaker = speaker;
                else RightSpeaker = speaker;
                if (!listener) currentSpeaker = speaker;
            }else
            {
                if (charPos == CharacterPosition.Left) LeftSpeaker = "";
                else RightSpeaker = "";
                if (!listener) currentSpeaker = "";
            }*/
        }

        private TextBubbleState UpdateTextBubbleState(TextBubbleState _bubbleState, TextPosition _currentSpeaker, TextPosition me, DialogueBubble _bubble, bool hideBubbles)
        {
            if (currentBubble == me) //Si el que está hablando soy yo 
            {
                //Si estoy escondido me enseño
                if (_bubbleState == TextBubbleState.Hidden) ShowTextBubble(_bubble, bubbleTransitionTime);

                //Si estoy oscurecido me aclaro
                if (_bubbleState == TextBubbleState.Darken) ShowTextBubble(_bubble, bubbleTransitionTime);

                _bubbleState = TextBubbleState.Shown;
            }
            else //Si no estoy hablando yo
            {
                //ver si tengo que esconderme o oscurecerme
                if (hideBubbles)
                {
                    //Si no estoy escondido ya
                    if (_bubbleState != TextBubbleState.Hidden)
                    {
                        //Si estoy oscurecido me aclaro
                        if (_bubbleState == TextBubbleState.Darken) ShowTextBubble(_bubble, bubbleTransitionTime);

                        //Me escondo
                        HideTextBubble(_bubble, bubbleTransitionTime);
                        _bubbleState = TextBubbleState.Hidden;
                    }
                }
                else
                {
                    //Si estoy escondido me enseño
                    if (_bubbleState == TextBubbleState.Hidden) ShowTextBubble(_bubble, bubbleTransitionTime);

                    //Me oscurezco
                    DarkenTextBubble(_bubble, bubbleTransitionTime);
                    _bubbleState = TextBubbleState.Darken;
                }
            }

            return _bubbleState;
        }

        public void AddButton(DialogueManager manager, int i, string optionString)
        {
            var _g = (GameObject)Instantiate(choiceButton, buttonParent.transform);
            var _button = _g.GetComponent<UnityEngine.UI.Button>();

            _button.onClick.AddListener(() => manager.OnChoiceClick(i));
            TextAnimator_TMP _text = _g.GetComponentInChildren<TextAnimator_TMP>();
            _text.SetText(optionString);
        }

        private void RemoveButtons() { foreach (Transform child in buttonParent.transform) { Destroy(child.gameObject); } }
        public void StartAnimationButtons() => LeanTween.moveLocalY(dialogueBubblesParent, 125.0f, 0.5f);
        public void EndAnimationButtons() { LeanTween.moveLocalY(dialogueBubblesParent, 0f, 0.75f); Invoke("RemoveButtons", 0.75f); }

        //TEXT BUBBLE ANIMATIONS
        /*
        private void ShowTextBubble(VisualElement bubble, TextPosition pos)
        {
            string name = "TextBubbleFADEDTOP";
            if (pos == TextPosition.Bottom) name = "TextBubbleFADEDBOT";
            bubble.RemoveFromClassList(name);
        }
        private void HideTextBubble(VisualElement bubble, TextPosition pos)
        {
            string name = "TextBubbleFADEDTOP";
            if (pos == TextPosition.Bottom) name = "TextBubbleFADEDBOT";
            bubble.AddToClassList(name);
        }

        private void DarkenTextBubble(VisualElement bubble) => bubble.AddToClassList("TextBubbleDARK");
        private void LightTextBubble(VisualElement bubble) => bubble.RemoveFromClassList("TextBubbleDARK");
        */
        private void ShowTextBubble(DialogueBubble _bubble, float time)
        {
            LeanTween.alpha(_bubble.backgroundRT, 1.0f, time);
            LeanTween.LeanTMPAlpha(_bubble.speakerRT, 1.0f, time);
            LeanTween.LeanTMPAlpha(_bubble.textRT, 1.0f, time);
        }
        private void HideTextBubble(DialogueBubble _bubble, float time)
        {
            LeanTween.alpha(_bubble.backgroundRT, 0.0f, time);
            LeanTween.LeanTMPAlpha(_bubble.speakerRT, 0.0f, time);
            LeanTween.LeanTMPAlpha(_bubble.textRT, 0.0f, time);
        }
        private void DarkenTextBubble(DialogueBubble _bubble, float time)
        {
            LeanTween.alpha(_bubble.backgroundRT, 0.5f, time);
            LeanTween.LeanTMPAlpha(_bubble.speakerRT, 0.5f, time);
            LeanTween.LeanTMPAlpha(_bubble.textRT, 0.5f, time);
        }

        //CHARACTER ANIMATIONS
        /*
        private void ShowCharacter(Image _character, CharacterPosition pos)
        {
            string name = "CharacterHIDDENRIGHT";
            if (pos == CharacterPosition.Left) name = "CharacterHIDDENLEFT";
            _character.RemoveFromClassList(name);
        }
        private void HideCharacter(Image _character, CharacterPosition pos)
        {
            string name = "CharacterHIDDENRIGHT";
            if (pos == CharacterPosition.Left) name = "CharacterHIDDENLEFT";
            _character.AddToClassList(name);
        }
        private void DarkenCharacter(Image _character) => _character.AddToClassList("CharacterDARK");
        private void LightCharacter(Image _character) => _character.RemoveFromClassList("CharacterDARK");

        private IEnumerator ChangeCharacters(Image _character, CharacterPosition pos)
        {
            HideCharacter(_character, pos);
            yield return new WaitForSeconds(0.6f);
            ShowCharacter(_character, pos);
        }*/
    }
}
