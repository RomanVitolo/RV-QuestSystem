using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Modules.DialogueSystem.Runtime
{
    [CreateAssetMenu(fileName = "New Dialogue", menuName = "RV/Dialogue System/Dialogue", order = 0)]
    public class Dialogue : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] private List<DialogueNode> dialogueNodes = new List<DialogueNode>();
        [SerializeField] private Vector2 newNodeOffset = new Vector2(250, 0);
        
        private Dictionary<string, DialogueNode> _dialogueNodeLookup = new();

#if UNITY_EDITOR
        private void OnValidate()
        {
            _dialogueNodeLookup.Clear();
            foreach (var dialogueNode in GetAllNodes())
            {
                _dialogueNodeLookup[dialogueNode.name] = dialogueNode;
            }
        }
        
        public void OnBeforeSerialize()
        {
            if (dialogueNodes.Count == 0)
            {
                DialogueNode newNode = MakeNode(null);
                AddNode(newNode);
            }

            if (AssetDatabase.GetAssetPath(this) != "")
            {
                foreach (DialogueNode node in GetAllNodes())
                {
                    if (AssetDatabase.GetAssetPath(node) == "")
                    {
                        AssetDatabase.AddObjectToAsset(node, this);
                    }
                }
            }
        }

        public void OnAfterDeserialize()
        {
        }
#endif      

        public IEnumerable<DialogueNode> GetAllNodes()
        {
            return dialogueNodes;
        }

        public DialogueNode GetRootNode() => dialogueNodes[0];

        public IEnumerable<DialogueNode> GetAllChildren(DialogueNode parentNode)
        {
            foreach (var childID in parentNode.GetChildren())
            {
                if (_dialogueNodeLookup.ContainsKey(childID))
                {
                    yield return _dialogueNodeLookup[childID];
                }
            }
           
        }

        public void CreateNode(DialogueNode parent)
        {
            DialogueNode newNode = MakeNode(parent);
            Undo.RegisterCreatedObjectUndo(newNode, "Created Dialogue Node");
            Undo.RecordObject(this, "Added Dialogue Node");
            AddNode(newNode);
        }

        public void RemoveNode(DialogueNode nodeToRemove)
        {
            Undo.RecordObject(this, "Deleted Dialogue Node");
            dialogueNodes.Remove(nodeToRemove);
            OnValidate();
            CleanDanglingChildren(nodeToRemove);
            Undo.DestroyObjectImmediate(nodeToRemove);
        }

        private void CleanDanglingChildren(DialogueNode nodeToRemove)
        {
            foreach (var dialogueNode in GetAllNodes())
            {
                dialogueNode.GetChildren().Remove(nodeToRemove.name);
                dialogueNode.RemoveChild(nodeToRemove.name);
            }
        }

        private void AddNode(DialogueNode newNode)
        {
            dialogueNodes.Add(newNode);
            OnValidate();
        }

        private DialogueNode MakeNode(DialogueNode parent)
        {
            DialogueNode newNode = CreateInstance<DialogueNode>();
            newNode.name = Guid.NewGuid().ToString();
            if (parent != null)
            {
                parent.AddChild(newNode.name);
                newNode.SetPlayerSpeaking(!parent.IsPlayerSpeaking());
                newNode.SetPosition(parent.GetRectPosition().position + newNodeOffset);
            }
            return newNode;
        }
    }
}