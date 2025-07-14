using Modules.DialogueSystem.Runtime;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Modules.DialogueSystem.Editor
{
    public class DialogueHelper : EditorWindow
    {
        private Dialogue _selectedDialogue;
        private GUIStyle _nodeStyle;
        private DialogueNode _draggingNode;
        private Vector2 _draggingOffset;
        
        private Sprite _nodeBackgroundSprite;
        
        [MenuItem("RV/Window/Dialogue Helper")]
        public static void ShowEditorWindow()
        {
            GetWindow(typeof(DialogueHelper), false, "Dialogue Helper");
        }

        [OnOpenAsset(1)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var dialogue = EditorUtility.InstanceIDToObject(instanceID) as Dialogue;
            return dialogue is not null;
        }

        private void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;
            
            _nodeStyle = new GUIStyle
            {
                normal =
                {
                    background = EditorGUIUtility.Load("node0") as Texture2D,
                    textColor = Color.white
                },
                padding = new RectOffset(20, 20, 20, 20),
                border = new RectOffset(12, 12, 12, 12)
            };
        }

        private void OnSelectionChanged()
        {
            if (Selection.activeObject is Dialogue newDialogue)
            {
                _selectedDialogue = newDialogue;
                Repaint();
            }
        }

        private void OnGUI()
        {
           
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Node Background Sprite", GUILayout.Width(150));
            var newSprite = (Sprite)EditorGUILayout.ObjectField(_nodeBackgroundSprite, typeof(Sprite), false);
            EditorGUILayout.EndHorizontal();

            if (newSprite != _nodeBackgroundSprite)
            {
                _nodeBackgroundSprite = newSprite;
                _nodeStyle.normal.background = _nodeBackgroundSprite != null
                    ? _nodeBackgroundSprite.texture
                    : EditorGUIUtility.Load("node0") as Texture2D;
            }
            
            if (_selectedDialogue is null)
            {
                EditorGUILayout.LabelField("No Dialogue Selected");
            }
            else
            {
                ProcessEvents();
                foreach (var dialogueNode in _selectedDialogue.GetAllNodes())
                {
                    DrawConnections(dialogueNode);
                }
                foreach (var dialogueNode in _selectedDialogue.GetAllNodes())
                {
                    DrawNode(dialogueNode);
                }
            }
        }

        private void ProcessEvents()
        {
            if (Event.current.type == EventType.MouseDown && _draggingNode is null)
            {
                _draggingNode = GetNodeAtPoint(Event.current.mousePosition);
                if (_draggingNode is not null)
                {
                    _draggingOffset = _draggingNode.RectPosition.position - Event.current.mousePosition;
                }
            }
            else if (Event.current.type == EventType.MouseDrag && _draggingNode is not null)
            {
                Undo.RecordObject(_selectedDialogue, "Drag Dialogue Node");
                _draggingNode.RectPosition.position = Event.current.mousePosition + _draggingOffset;
                GUI.changed = true;
            }
            else if (Event.current.type == EventType.MouseUp && _draggingNode is not null)
            {
                _draggingNode = null;
            }
        }

        private void DrawNode(DialogueNode dialogueNode)
        {
            GUILayout.BeginArea(dialogueNode.RectPosition, _nodeStyle);
            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.LabelField("Dialogue Node:", EditorStyles.whiteBoldLabel);
                    
            var newText = EditorGUILayout.TextField(dialogueNode.Text);
            var newID = EditorGUILayout.TextField(dialogueNode.ID);
                    
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_selectedDialogue, "Update Dialogue Text");
                        
                dialogueNode.Text = newText;
                dialogueNode.ID = newID;
            }

            foreach (var childrenNode in _selectedDialogue.GetAllChildren(dialogueNode))
            {
                EditorGUILayout.LabelField(childrenNode.Text);
            }
            
            GUILayout.EndArea();
        }
        
        private void DrawConnections(DialogueNode dialogueNode)
        {
            var startPosition = new Vector2(dialogueNode.RectPosition.xMax, dialogueNode.RectPosition.center.y); 
            foreach (var childNode in _selectedDialogue.GetAllChildren(dialogueNode))
            { 
               var endPosition = new Vector2(childNode.RectPosition.xMin, childNode.RectPosition.center.y);
               var controlPointOffset = endPosition - startPosition;
               controlPointOffset.y = 0;
               controlPointOffset.x *= 0.8f; 
               Handles.DrawBezier(startPosition, endPosition, startPosition + controlPointOffset, 
                   endPosition - controlPointOffset, Color.white, null, 4f); 
            }
        }

        private DialogueNode GetNodeAtPoint(Vector2 point)
        {
            DialogueNode foundNode = null;
            foreach (var dialogueNode in _selectedDialogue.GetAllNodes())
            {
                if (dialogueNode.RectPosition.Contains(point))
                {
                    foundNode = dialogueNode;
                }
            }
            return foundNode;
        }
    }
}