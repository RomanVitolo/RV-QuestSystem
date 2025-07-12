using UnityEngine;

namespace Modules.DialogueSystem.Runtime
{
    [CreateAssetMenu(fileName = "New Dialogue", menuName = "RV/Dialogue System/Dialogue", order = 0)]
    public class Dialogue : ScriptableObject
    {
        [SerializeField] private DialogueNode[] dialogueNodes;
    }
}