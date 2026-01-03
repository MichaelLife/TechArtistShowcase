using UnityEngine;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using TMPro;
using LifeHMA.Player;
using UnityEngine.UIElements;
using Febucci.TextAnimatorForUnity;
using Unity.GraphToolkit.Editor;

namespace LifeHMA.Dialogue
{
    public class LifeHMADialogueManager : MonoBehaviour
    {
        private Dictionary<string, RuntimeDialogueNode> _nodeLookup = new Dictionary<string, RuntimeDialogueNode>();
        private RuntimeDialogueNode _currentNode;
        private BasicPlayerMovement player;
        private DialogueEventManager eventManager;

        public DialogueUI dialogueUI;
        private VisualElement root;
        private AnimatedLabel speakerLabel, textLabel;
        private void ChacheAllNodes(RuntimeDialogueGraph runtimeGraph)
        {
            foreach (var node in runtimeGraph.AllNodes)
            {
                _nodeLookup[node.NodeID] = node;
            }
        }

        public static LifeHMADialogueManager instance;

        private void Awake()
        {
            if (instance == null)
                instance = this;
        }

        private void Start()
        {
            player = GameObject.FindAnyObjectByType<BasicPlayerMovement>().GetComponent<BasicPlayerMovement>();
            eventManager = GameObject.FindAnyObjectByType<DialogueEventManager>().GetComponent<DialogueEventManager>();
        }

        #region NODES
        private void GetNode(string nodeID)
        {
            if (!_nodeLookup.ContainsKey(nodeID))
            {
                EndDialogue();
                return;
            }
            _currentNode = _nodeLookup[nodeID];

            //Check if node is event node
            if (_currentNode.eventKey != "")
            {
                //Checks all events in scene to see if the key is the same, if it is, it invokes the event
                eventManager.CheckForKey(_currentNode.eventKey);
                //Continue to next node
                GetNextNode();
            }
            else if(_currentNode.isIfNode) //Is If node
            {
                checkAllIfConditions(_currentNode);
            }
            else //Is dialogue node
            {
                //dialogueUI.SetTextToUI(_currentNode.SpeakerName, _currentNode.DialogueText);
            }
        }

        private void checkAllIfConditions(RuntimeDialogueNode node)
        {
            //Check for TRUE out or FALSE out
            bool generalOut = true;
            string firstHitId = "";
            string firstElseId = "";
            foreach (RuntimeCondition condition in node.ifNodeData.conditions)
            {
                //Check the ifs
                bool _v = DoIfNodeOperation(condition._operator, 1, condition.value); //Sustituir 1 por la variable que se saque del variable manager, HAY QUE HACERLO
                generalOut &= _v;

                //In case all there is no node in TRUE out or ELSE out, save the first condition match that has an exit
                if (_v && firstHitId == "") firstHitId = condition.conditionTrueOutNode;
                else if (!_v && firstElseId == "") firstElseId = condition.elseOutNode;
            }

            //In case the result is true and there is no TRUE out node
            if (generalOut && node.ifNodeData.TrueOutNode == "")
            {
                CheckRemenatsOfIfNode(firstHitId, firstElseId);
            }
            else if(generalOut) //TRUE out result
            {
                GetNode(node.ifNodeData.TrueOutNode);
            }
            //In case the result is false and there is no ELSE out node
            else if (!generalOut && node.ifNodeData.FalseOutNode == "")
            {
                CheckRemenatsOfIfNode(firstHitId, firstElseId);
            }
            else if (!generalOut) //ELSE out result
            {
                GetNode(node.ifNodeData.FalseOutNode);
            }
        }
        private void CheckRemenatsOfIfNode(string firstHitId, string firstElseId)
        {
            if (!string.IsNullOrEmpty(firstHitId)) GetNode(firstHitId);
            else if (!string.IsNullOrEmpty(firstElseId)) GetNode(firstElseId);
            else EndDialogue();
        }

        private bool DoIfNodeOperation(int _operator, float value1, float value2)
        {
            bool result = false;

            switch (_operator)
            {
                case 0: //==
                    if (value1 == value2) result = true;
                    break;
                case 2: //!=
                    if (value1 != value2) result = true;
                    break;
                case 3: //>
                    if (value1 > value2) result = true;
                    break;
                case 4: //<
                    if (value1 < value2) result = true;
                    break;
                case 5: //<
                    if (value1 >= value2) result = true;
                    break;
                case 6: //<
                    if (value1 <= value2) result = true;
                    break;
            }

            return result;
        }
        private bool DoIfNodeOperation(int _operator, int value1, float value2) => DoIfNodeOperation(_operator, (float)value1, value2);
        private bool DoIfNodeOperation(int _operator, bool value1, float value2) { return value1; }

        private void GetStartNode(RuntimeDialogueGraph runtimeGraph)
        {
            GetNode(runtimeGraph.EntryNodeID);
        }

        private void GetNextNode()
        {
            if (!string.IsNullOrEmpty(_currentNode.NextNodeID))
                GetNode(_currentNode.NextNodeID);
            else
                EndDialogue();
        }

        public void StartOrAdvanceDialogue(RuntimeDialogueGraph runtimeGraph)
        {
            if (_currentNode == null)
                StartDialogue(runtimeGraph);
            else
                GetNextNode();
        }

        public void StartDialogue(RuntimeDialogueGraph runtimeGraph)
        {
            EnableOrDisableDialogueUI(true);
            player.Immobilize(true);
            ChacheAllNodes(runtimeGraph);
            GetStartNode(runtimeGraph);
        }

        public void EndDialogue()
        {
            player.Immobilize(false);
            _currentNode = null;
            EnableOrDisableDialogueUI(false);
        }
        #endregion

        public void EnableOrDisableDialogueUI(bool value)
        {
            dialogueUI.gameObject.SetActive(value);
        }
    }
}
