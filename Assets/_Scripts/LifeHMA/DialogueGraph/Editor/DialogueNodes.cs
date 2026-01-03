using UnityEngine;
using Unity.GraphToolkit.Editor;
using System;
using LifeHMA.Dialogue;
using UnityEngine.UIElements;
using System.Collections.Generic;

[Serializable]
public class StartNode : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddOutputPort("out").Build();
    }
}

[Serializable]
public class EndNode : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("in").Build();
    }
}


public class DialogueNode : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("in").Build();
        context.AddOutputPort("out").Build();

        context.AddInputPort<string>("Speaker").Build();
        context.AddInputPort<string>("Dialogue").Build();
    }
}

public class EventNode : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("in").Build();
        context.AddOutputPort("out").Build();

        context.AddInputPort<DialogueEventSO>("Dialogue Event").Build();
    }
}

[UseWithContext(typeof(IfNode))]
[Serializable]
public class DialogueConditionBlock : BlockNode
{
    [Serializable]
    public enum Operator
    {
        equal,
        notEqual,
        isGreater,
        isLower,
        isGreaterOrEqual,
        isLowerOrEqual
    }

    [Serializable]
    public struct Condition
    {
        public DialogueVariableSO variable;
        public Operator _operator;
        public float value;
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddOutputPort($"Condition Out").Build();

        context.AddInputPort<DialogueVariableSO>("Variable");
        context.AddInputPort<Operator>("Operator");
        context.AddInputPort<float>("Value");

        context.AddOutputPort("Else Out").Build();
    }
}

public class IfNode : ContextNode
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("in").Build();

        context.AddOutputPort("TRUE out").Build();
        context.AddOutputPort("FALSE out").Build();
    }
}
