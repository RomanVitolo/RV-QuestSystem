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

#if Unity_Editor
        private void Awake()
        {
            if (dialogueNodes.Count == 0)
            {
                dialogueNodes.Add(new DialogueNode());
            }
        }
        
       
#endif
        private void OnValidate()
        {
            _dialogueNodeLookup.Clear();
            foreach (var dialogueNode in GetAllNodes())
            {
                _dialogueNodeLookup[dialogueNode.ID] = dialogueNode;
            }
        }

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
    }
}