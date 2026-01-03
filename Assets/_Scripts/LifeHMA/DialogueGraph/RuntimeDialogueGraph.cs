using System.Collections.Generic;
using System;
using UnityEngine;
using LifeHMA.Dialogue;
using Unity.GraphToolkit.Editor;

[Serializable]
public class RuntimeCondition
{
    public string VariableKey;
    public int _operator;
    public float value;
    public string conditionTrueOutNode;
    public string elseOutNode;

    public RuntimeCondition(string VariableKey, int _operator, float value, string conditionTrueOutNode, string elseOutNode)
    {
        this.VariableKey = VariableKey;
        this._operator = _operator;
        this.value = value;
        this.conditionTrueOutNode = conditionTrueOutNode;
        this.elseOutNode = elseOutNode;
    }
}

public class RuntimeDialogueGraph : ScriptableObject
{
    public string EntryNodeID;
    public List<RuntimeDialogueNode> AllNodes = new List<RuntimeDialogueNode>();
}

[Serializable]
public class RuntimeIfNodeData
{
    public string TrueOutNode;
    public string FalseOutNode;

    public List<RuntimeCondition> conditions = new List<RuntimeCondition>();
}

[Serializable]
public class RuntimeDialogueNode
{
    public string NodeID;
    public string SpeakerName;
    public string DialogueText;
    public string NextNodeID;

    //Event node
    public string eventKey = "";

    //If node
    public bool isIfNode = false;
    public RuntimeIfNodeData ifNodeData = new RuntimeIfNodeData();

}
