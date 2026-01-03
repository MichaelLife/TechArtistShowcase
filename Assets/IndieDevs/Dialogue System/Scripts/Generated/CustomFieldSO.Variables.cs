// Auto-generated code, do not modify by hand
using System;
using UnityEngine;

namespace DialogueSystem
{
    public partial class CustomFieldSO
    {
        [HideInInspector] public TextPosition textPositionValue;
        public TextPosition TextPositionValue => textPositionValue;
        [HideInInspector] public Emotion emotionValue;
        public Emotion EmotionValue => emotionValue;
        [HideInInspector] public DialogueCharacterPosition dialogueCharacterPositionValue;
        public DialogueCharacterPosition DialogueCharacterPositionValue => dialogueCharacterPositionValue;

#if UNITY_EDITOR
        public void OnTextPositionChanged(Enum value)
        {
            textPositionValue = (TextPosition)value;
            Save();
        }
        public void OnEmotionChanged(Enum value)
        {
            emotionValue = (Emotion)value;
            Save();
        }
        public void OnDialogueCharacterPositionChanged(Enum value)
        {
            dialogueCharacterPositionValue = (DialogueCharacterPosition)value;
            Save();
        }
#endif

        public Enum GetEnumValue(FieldSO fieldSO)
        {
            if (fieldSO.label == nameof(TextPosition))
            {
                return TextPositionValue;
            }
            if (fieldSO.label == nameof(Emotion))
            {
                return EmotionValue;
            }
            if (fieldSO.label == nameof(DialogueCharacterPosition))
            {
                return DialogueCharacterPositionValue;
            }
            return null;
        }
        public void CopyEnumsFrom(CustomFieldSO other)
        {
            textPositionValue = other.textPositionValue;
            emotionValue = other.emotionValue;
            dialogueCharacterPositionValue = other.dialogueCharacterPositionValue;
        }
    }
}
