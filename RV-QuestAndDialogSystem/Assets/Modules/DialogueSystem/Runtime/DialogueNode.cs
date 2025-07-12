using System;

namespace Modules.DialogueSystem.Runtime
{
    [Serializable]
    public class DialogueNode
    {
        public string ID;
        public string Text;
        public string[] Children;
    }
}