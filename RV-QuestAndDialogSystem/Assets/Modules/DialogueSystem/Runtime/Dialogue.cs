using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modules.DialogueSystem.Runtime
{
    [CreateAssetMenu(fileName = "New Dialogue", menuName = "RV/Dialogue System/Dialogue", order = 0)]
    public class Dialogue : ScriptableObject
    {
        [SerializeField] private List<DialogueNode> dialogueNodes = new List<DialogueNode>();
        
        private Dictionary<string, DialogueNode> _dialogueNodeLookup = new();

#if UNITY_EDITOR
        private void Awake()
        {
            if (dialogueNodes.Count == 0)
            {
                var rootNode = new DialogueNode
                {
                    UniqueID = Guid.NewGuid().ToString()
                };
                dialogueNodes.Add(rootNode);
            }
        }
        
        private void OnValidate()
        {
            _dialogueNodeLookup.Clear();
            foreach (var dialogueNode in GetAllNodes())
            {
                _dialogueNodeLookup[dialogueNode.UniqueID] = dialogueNode;
            }
        }
#endif
        

        public IEnumerable<DialogueNode> GetAllNodes()
        {
            return dialogueNodes;
        }

        public DialogueNode GetRootNode() => dialogueNodes[0];

        public IEnumerable<DialogueNode> GetAllChildren(DialogueNode parentNode)
        {
            foreach (var childID in parentNode.Children)
            {
                if (_dialogueNodeLookup.ContainsKey(childID))
                {
                    yield return _dialogueNodeLookup[childID];
                }
            }
           
        }

        public void CreateNode(DialogueNode parent)
        {
            var newNode = new DialogueNode
            {
                UniqueID = Guid.NewGuid().ToString()
            };
            parent.Children.Add(newNode.UniqueID);
            dialogueNodes.Add(newNode);
            #if UNITY_EDITOR
            OnValidate();
            #endif
        }

        public void RemoveNode(DialogueNode nodeToRemove)
        {
            dialogueNodes.Remove(nodeToRemove);
            #if UNITY_EDITOR
            OnValidate();
            #endif
            CleanDanglingChildren(nodeToRemove);
        }

        private void CleanDanglingChildren(DialogueNode nodeToRemove)
        {
            foreach (var dialogueNode in GetAllNodes())
            {
                dialogueNode.Children.Remove(nodeToRemove.UniqueID);
            }
        }
    }
}