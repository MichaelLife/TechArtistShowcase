using UnityEngine;
using UnityEngine.UIElements;
using Febucci.TextAnimatorForUnity;
using System.Linq;
using System.Collections;
using System;

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
        Image LeftCharacter, RightCharacter;

        private TextPosition currentBubble;
        private TextBubbleState topBubbleState, botBubbleState;
        private string LeftSpeaker, RightSpeaker;
        private string currentSpeaker;

        public bool isWritingText;

        private void OnEnable()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            charRoot = CharactersUI.rootVisualElement;

            TopSpeakerLabel = root.Q<AnimatedLabel>("TopSpeaker");
            TopTextLabel = root.Q<AnimatedLabel>("TopText");

            BotSpeakerLabel = root.Q<AnimatedLabel>("BotSpeaker");
            BotTextLabel = root.Q<AnimatedLabel>("BotText");

            TopBubble = root.Q<VisualElement>("TopBubble");
            BotBubble = root.Q<VisualElement>("BotBubble");

            ButtonContainer = root.Q<VisualElement>("ButtonContainer");

            LeftCharacter = charRoot.Q<Image>("RightImage");
            RightCharacter = charRoot.Q<Image>("LeftImage");

            ResetTextBubbles();
            ResetCharacterUI();

            TopTextLabel.Typewriter.OnTypewriterStart += WritingStart;
            TopTextLabel.Typewriter.OnTextShowed += WritingStop;
            BotTextLabel.Typewriter.OnTypewriterStart += WritingStart;
            BotTextLabel.Typewriter.OnTextShowed += WritingStop;

            root.Children().First().AddToClassList("DialogueContainerFADED");
        }

        private void WritingStart() => isWritingText = true;
        private void WritingStop() => isWritingText = false;

        public void SkipText()
        {
            isWritingText = false;
            TopTextLabel.Typewriter.SkipTypewriter();
            //BotTextLabel.Typewriter.SkipTypewriter();
        }

        private void ResetTextBubbles()
        {
            if (topBubbleState == TextBubbleState.Darken) LightTextBubble(TopBubble);
            HideTextBubble(TopBubble, TextPosition.Top);
            topBubbleState = TextBubbleState.Hidden;

            if (botBubbleState == TextBubbleState.Darken) LightTextBubble(BotBubble);
            HideTextBubble(BotBubble, TextPosition.Bottom);
            botBubbleState = TextBubbleState.Hidden;
        }

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

        public void StartDialogue()
        {
            root.Children().First().RemoveFromClassList("DialogueContainerFADED");
        }

        public void EndDialogue()
        {
            root.Children().First().AddToClassList("DialogueContainerFADED");
            ResetTextBubbles();
            ResetCharacterUI(); 
        }

        public IEnumerator SetTextToUI(TextPosition pos, string speaker, string text, bool hideBubbles, WaitForSeconds wait = null)
        {
            TopTextLabel.SetText(" ");
            BotTextLabel.SetText(" ");

            yield return wait;

            if (pos == TextPosition.Top)
            {
                currentBubble = TextPosition.Top;

                topBubbleState = UpdateTextBubbleState(topBubbleState, currentBubble, TextPosition.Top, TopBubble, hideBubbles);
                botBubbleState = UpdateTextBubbleState(botBubbleState, currentBubble, TextPosition.Bottom, BotBubble, hideBubbles);

                TopSpeakerLabel.SetText(speaker);

                yield return new WaitForSeconds(0.3f);
                TopTextLabel.Typewriter.ShowText(text);
            }
            else
            {
                currentBubble = TextPosition.Bottom;

                botBubbleState = UpdateTextBubbleState(botBubbleState, currentBubble, TextPosition.Bottom, BotBubble, hideBubbles);
                topBubbleState = UpdateTextBubbleState(topBubbleState, currentBubble, TextPosition.Top, TopBubble, hideBubbles);

                BotSpeakerLabel.SetText(speaker);

                yield return new WaitForSeconds(0.3f);
                BotTextLabel.Typewriter.ShowText(text);
            }
        }

        public void UpdateCharactersUI(CharacterPosition charPos, string speaker, bool showSpeaker, bool listener)
        {
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
            }
        }

        private TextBubbleState UpdateTextBubbleState(TextBubbleState _bubbleState, TextPosition _currentSpeaker, TextPosition me, VisualElement _bubble, bool hideBubbles)
        {
            if (currentBubble == me) //Si el que está hablando soy yo 
            {
                //Si estoy escondido me enseño
                if (_bubbleState == TextBubbleState.Hidden) ShowTextBubble(_bubble, me);

                //Si estoy oscurecido me aclaro
                if (_bubbleState == TextBubbleState.Darken) LightTextBubble(_bubble);

                _bubbleState = TextBubbleState.Shown;
            }
            else //Si no estoy hablando yo
            {
                //ver si tengo que esconderme o oscurecerme
                if(hideBubbles)
                {
                    //Si no estoy escondido ya
                    if (_bubbleState != TextBubbleState.Hidden)
                    {
                        //Si estoy oscurecido me aclaro
                        if (_bubbleState == TextBubbleState.Darken) LightTextBubble(_bubble);

                        //Me escondo
                        HideTextBubble(_bubble, me);
                        _bubbleState = TextBubbleState.Hidden;
                    }
                }
                else
                {
                    //Si estoy escondido me enseño
                    if (_bubbleState == TextBubbleState.Hidden) ShowTextBubble(_bubble, me);

                    //Me oscurezco
                    DarkenTextBubble(_bubble);
                    _bubbleState = TextBubbleState.Darken;
                }
            }

            return _bubbleState;
        }

        public void AddButton(DialogueManager manager, int i, string optionString)
        {
            var button = new Button(() =>
            {
                int index = i;
                manager.OnChoiceClick(i);
            })
            {
                text = optionString
            };

            ButtonContainer.Add(button);
        }

        private void RemoveButtons() => ButtonContainer.Clear();
        public void StartAnimationButtons() => ButtonContainer.AddToClassList("ButtonContainerFULL");
        public void EndAnimationButtons() { ButtonContainer.RemoveFromClassList("ButtonContainerFULL"); Invoke("RemoveButtons", 0.75f); }

        //TEXT BUBBLE ANIMATIONS
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

        //CHARACTER ANIMATIONS
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
        }
    }
}
