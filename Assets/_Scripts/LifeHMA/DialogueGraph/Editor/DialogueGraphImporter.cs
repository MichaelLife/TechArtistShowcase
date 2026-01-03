using UnityEngine;
using UnityEditor.AssetImporters;
using Unity.GraphToolkit.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using LifeHMA.Dialogue;


[ScriptedImporter(1, DialogueGraph.AssetExtension)]
public class DialogueGraphImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        DialogueGraph editorGraph = GraphDatabase.LoadGraphForImporter<DialogueGraph>(ctx.assetPath);
        RuntimeDialogueGraph runtimeGraph = ScriptableObject.CreateInstance<RuntimeDialogueGraph>();
        var nodeIDMap = new Dictionary<INode, string>();

        foreach(var node in editorGraph.GetNodes())
        {
            nodeIDMap[node] = Guid.NewGuid().ToString();
        }

        var startNode = editorGraph.GetNodes().OfType<StartNode>().FirstOrDefault();
        if(startNode != null)
        {
            var entryPort = startNode.GetOutputPorts().FirstOrDefault()?.firstConnectedPort;
            if(entryPort != null)
            {
                runtimeGraph.EntryNodeID = nodeIDMap[entryPort.GetNode()];
            }
        }

        foreach(var iNode in editorGraph.GetNodes())
        {
            if (iNode is StartNode || iNode is EndNode) continue;

            var runtimeNode = new RuntimeDialogueNode { NodeID = nodeIDMap[iNode] };
            if(iNode is DialogueNode dialogueNode)
            {
                ProcessDialogueNode(dialogueNode, runtimeNode, nodeIDMap);
            }
            else if(iNode is EventNode eventNode)
            {
                ProcessEventNode(eventNode, runtimeNode, nodeIDMap);
            }
            else if (iNode is IfNode ifNode)
            {
                ProcessIfNode(ifNode, runtimeNode, nodeIDMap);
            }

            runtimeGraph.AllNodes.Add(runtimeNode);
        }

        ctx.AddObjectToAsset("RuntimeData", runtimeGraph);
        ctx.SetMainObject(runtimeGraph);
    }

    private void ProcessDialogueNode(DialogueNode node, RuntimeDialogueNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        runtimeNode.SpeakerName = GetPortValue<string>(node.GetInputPortByName("Speaker"));
        runtimeNode.DialogueText = GetPortValue<string>(node.GetInputPortByName("Dialogue"));

        var nextNodePort = node.GetOutputPortByName("out")?.firstConnectedPort;
        if (nextNodePort != null)
            runtimeNode.NextNodeID = nodeIDMap[nextNodePort.GetNode()];
    }

    private void ProcessEventNode(EventNode node, RuntimeDialogueNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        DialogueEventSO _ev = GetPortValue<DialogueEventSO>(node.GetInputPortByName("Dialogue Event"));
        if(_ev != null)
            runtimeNode.eventKey = _ev.Id;

        var nextNodePort = node.GetOutputPortByName("out")?.firstConnectedPort;
        if (nextNodePort != null)
            runtimeNode.NextNodeID = nodeIDMap[nextNodePort.GetNode()];
    }

    private void ProcessIfNode(IfNode node, RuntimeDialogueNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        runtimeNode.isIfNode = true;

        foreach(BlockNode _n in node.blockNodes)
        {
            DialogueVariableSO variable = GetPortValue<DialogueVariableSO>(_n.GetInputPortByName("Variable"));
            string variableID = "";
            if (variable != null) variableID = variable.Id;

            var node1 = _n.GetOutputPortByName("Condition Out")?.firstConnectedPort;
            string node1ID = "";
            if (node1 != null) node1ID = nodeIDMap[node1.GetNode()];

            var node2 = _n.GetOutputPortByName("Else Out")?.firstConnectedPort;
            string node2ID = "";
            if (node2 != null) node2ID = nodeIDMap[node1.GetNode()];

            runtimeNode.ifNodeData.conditions.Add(
                new RuntimeCondition(
                    variableID,
                    (int)GetPortValue<DialogueConditionBlock.Operator>(_n.GetInputPortByName("Operator")),
                    GetPortValue<float>(_n.GetInputPortByName("Value")),
                    node1ID,
                    node2ID
                    ));
        }

        var nextNodePort = node.GetOutputPortByName("FALSE out")?.firstConnectedPort;
        if (nextNodePort != null)
            runtimeNode.ifNodeData.FalseOutNode = nodeIDMap[nextNodePort.GetNode()];

        nextNodePort = node.GetOutputPortByName("TRUE out")?.firstConnectedPort;
        if (nextNodePort != null)
            runtimeNode.ifNodeData.TrueOutNode = nodeIDMap[nextNodePort.GetNode()];
    }


    private T GetPortValue<T>(IPort port)
    {
        if (port == null) return default;

        if(port.isConnected)
        {
            if(port.firstConnectedPort.GetNode() is IVariableNode variableNode)
            {
                variableNode.variable.TryGetDefaultValue(out T value);
                return value;
            }
        }

        port.TryGetValue(out T fallbackValue);
        return fallbackValue;
    }
}
