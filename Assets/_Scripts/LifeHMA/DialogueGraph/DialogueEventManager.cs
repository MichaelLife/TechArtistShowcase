using UnityEngine;
using System.Collections.Generic;
using LifeHMA.Dialogue;
using UnityEngine.Events;
using System;

[Serializable]
public struct DialogueEventStruct
{
    public DialogueEventSO Key;
    public UnityEvent DialogueEvent;
}
public class DialogueEventManager : MonoBehaviour
{
    public List<DialogueEventStruct> Events = new List<DialogueEventStruct>();

    public void CheckForKey(string _key)
    {
        foreach(DialogueEventStruct _event in Events)
        {
            if(_event.Key.Id == _key)
            {
                DoEvent(_event.DialogueEvent);
            }
        }
    }

    public void DoEvent(UnityEvent _event)
    {
        _event.Invoke();
    }
}
