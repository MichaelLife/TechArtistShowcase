using UnityEngine;
using LifeHMA.Utilities;

namespace LifeHMA.Dialogue
{
    [CreateAssetMenu(fileName = "DialogueEventScriptableObject", menuName = "LifeHMA/Dialogue/Dialogue Event Scriptable Object")]
    public class DialogueEventSO : ScriptableObject
    {
        [ScriptableObjectId]
        public string Id;
    }
}
