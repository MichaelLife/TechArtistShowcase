// Auto-generated code, do not modify by hand
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem
{
    public partial class CharacterInternal
    {
        public bool ShowCharater => customFieldSO.GetCustomFieldValue<bool>(DialogueFields.ShowCharater);
        public Emotion Emotion => customFieldSO.GetCustomFieldValue<Emotion>(DialogueFields.Emotion);
        public DialogueCharacterPosition DialogueCharacterPosition => customFieldSO.GetCustomFieldValue<DialogueCharacterPosition>(DialogueFields.DialogueCharacterPosition);
    }
}
