using Modules.DialogueSystem.Runtime;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Modules.DialogueSystem.Editor
{
    public class DialogueHelper : EditorWindow
    {
        [MenuItem("RV/Window/Dialogue Helper")]
        public static void ShowEditorWindow()
        {
            GetWindow(typeof(DialogueHelper), true, "Dialogue Helper");
        }

        [OnOpenAsset(1)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var dialogue = EditorUtility.InstanceIDToObject(instanceID) as Dialogue;
            return dialogue is not null;
        }
    }
}