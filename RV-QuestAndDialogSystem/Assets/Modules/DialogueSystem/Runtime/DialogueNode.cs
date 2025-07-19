using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Modules.DialogueSystem.Runtime
{
    public class DialogueNode : ScriptableObject
    {
        [SerializeField] private bool isPlayerSpeaking;
        [SerializeField] private string text;
        [SerializeField] private List<string> children = new List<string>();
        [SerializeField] private Rect rect = new Rect(0, 0, 200, 150);

        public Rect GetRectPosition() => rect;

        public string GetText() => text;

        public List<string> GetChildren() => children;
        
        public bool IsPlayerSpeaking() => isPlayerSpeaking;

#if UNITY_EDITOR
        public void SetPosition(Vector2 newPosition)
        {
            Undo.RecordObject(this, "Move Dialogue Node");
            rect.position = newPosition;
            EditorUtility.SetDirty(this);
        }

        public void SetText(string newText)
        {
            if (newText != text)
            {
                Undo.RecordObject(this, "Update Dialogue Text");
                text = newText;
                EditorUtility.SetDirty(this);
            }
        }

        public void AddChild(string childID)
        {
            Undo.RecordObject(this, "Add Dialogue Link");
            children.Add(childID);
            EditorUtility.SetDirty(this);
        }

        public void RemoveChild(string childID)
        {
            Undo.RecordObject(this, "Remove Dialogue Link");
            children.Remove(childID);
            EditorUtility.SetDirty(this);
        }
        
        public void SetPlayerSpeaking(bool newIsPlayerSpeaking)
        {
            isPlayerSpeaking = newIsPlayerSpeaking;
            Undo.RecordObject(this, "Change Dialogue Speaker");
            
            
            EditorUtility.SetDirty(this);
        }
#endif
       
    }
}