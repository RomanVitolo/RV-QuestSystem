using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Modules.DialogueSystem.Runtime
{
    [Serializable]
    public class DialogueNode
    {
        public string UniqueID;
        public string Text;
        public List<string> Children = new();
        public Rect RectPosition = new Rect(0, 0, 200, 200);
    }
}