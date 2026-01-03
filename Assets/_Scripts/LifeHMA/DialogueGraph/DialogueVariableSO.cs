using UnityEngine;
using LifeHMA.Utilities;

namespace LifeHMA.Dialogue
{
    [CreateAssetMenu(fileName = "DialogueVariableScriptableObject", menuName = "LifeHMA/Dialogue/Dialogue Variable Scriptable Object")]
    public class DialogueVariableSO : ScriptableObject
    {
        [ScriptableObjectId]
        public string Id;
    }
}
