// Auto-generated code, do not modify by hand
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem
{
    public partial class CustomFieldView
    {
        EnumField GetEnumField(FieldSO fieldSO) {
            if (fieldSO.label == nameof(TextPosition))
            {
                EnumField enumField = new EnumField("TextPosition", customFieldSO.TextPositionValue);
                enumField.RegisterValueChangedCallback(evt => customFieldSO.OnTextPositionChanged(evt.newValue));
                return enumField;
            }
            if (fieldSO.label == nameof(Emotion))
            {
                EnumField enumField = new EnumField("Emotion", customFieldSO.EmotionValue);
                enumField.RegisterValueChangedCallback(evt => customFieldSO.OnEmotionChanged(evt.newValue));
                return enumField;
            }
            if (fieldSO.label == nameof(DialogueCharacterPosition))
            {
                EnumField enumField = new EnumField("DialogueCharacterPosition", customFieldSO.DialogueCharacterPositionValue);
                enumField.RegisterValueChangedCallback(evt => customFieldSO.OnDialogueCharacterPositionChanged(evt.newValue));
                return enumField;
            }
            return null;
        }
    }
}
