using System;
using Modules.DialogueSystem.Runtime;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Modules.DialogueSystem.Editor
{
    public class DialogueHelper : EditorWindow
    {
        private Dialogue _selectedDialogue;
        private Vector2 _scrollPosition;
        
        [NonSerialized] private GUIStyle _nodeStyle;
        [NonSerialized] private GUIStyle _playerNodeStyle;
        [NonSerialized] private Vector2 _draggingOffset;
        [NonSerialized] private Vector2 _draggingCanvasOffset;
        [NonSerialized] private bool _draggingCanvas;
        [NonSerialized] private DialogueNode _draggingNode;
        [NonSerialized] private DialogueNode _creatingNode;
        [NonSerialized] private DialogueNode _deletingNode;
        [NonSerialized] private DialogueNode _linkingParentNode;
        
        private const float CANVAS_SIZE = 4000;
        private const float BACKGROUND_SIZE = 50;
        
        
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
            
            _playerNodeStyle = new GUIStyle
            {
                normal =
                {
                    background = EditorGUIUtility.Load("node1") as Texture2D,
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
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

                var canvas = GUILayoutUtility.GetRect(CANVAS_SIZE, CANVAS_SIZE);
                var backgroundTexture = Resources.Load("background") as Texture2D;
                var textureCoords = new Rect(0, 0, CANVAS_SIZE / BACKGROUND_SIZE, CANVAS_SIZE /  BACKGROUND_SIZE);
                GUI.DrawTextureWithTexCoords(canvas, backgroundTexture, textureCoords);
                    
                
                foreach (var dialogueNode in _selectedDialogue.GetAllNodes())
                {
                    DrawConnections(dialogueNode);
                }
                foreach (var dialogueNode in _selectedDialogue.GetAllNodes())
                {
                    DrawNode(dialogueNode);
                }
                
                EditorGUILayout.EndScrollView();
               

                if (_creatingNode is not null)
                {
                    _selectedDialogue.CreateNode(_creatingNode);
                    _creatingNode = null;
                }

                if (_deletingNode is not null)
                {
                   _selectedDialogue.RemoveNode(_deletingNode);
                   _deletingNode = null;
                }
            }
        }

        private void ProcessEvents()
        {
            if (Event.current.type == EventType.MouseDown && _draggingNode is null)
            {
                _draggingNode = GetNodeAtPoint(Event.current.mousePosition + _scrollPosition);
                if (_draggingNode is not null)
                {
                    _draggingOffset = _draggingNode.GetRectPosition().position - Event.current.mousePosition;
                    Selection.activeObject = _draggingNode;
                }
                else
                {
                    _draggingCanvas = true;
                    _draggingCanvasOffset = Event.current.mousePosition + _scrollPosition;
                    Selection.activeObject = _selectedDialogue;
                }
            }
            else if (Event.current.type == EventType.MouseDrag && _draggingNode is not null)
            {
                _draggingNode.SetPosition(Event.current.mousePosition + _draggingOffset);
                
                GUI.changed = true;
            }
            else if (Event.current.type == EventType.MouseDrag && _draggingCanvas)
            {
                _scrollPosition = _draggingCanvasOffset - Event.current.mousePosition;
                
                GUI.changed = true;
            }
            else if (Event.current.type == EventType.MouseUp && _draggingNode is not null)
            {
                _draggingNode = null;
            }
            else if (Event.current.type == EventType.MouseUp && _draggingCanvas)
            {
                _draggingCanvas = false;
            }
        }

        private void DrawNode(DialogueNode dialogueNode)
        {
            var style = _nodeStyle;
            if (dialogueNode.IsPlayerSpeaking())
            {
                style = _playerNodeStyle;
            }
            GUILayout.BeginArea(dialogueNode.GetRectPosition(), style);
            EditorGUI.BeginChangeCheck();
           
            dialogueNode.SetText(EditorGUILayout.TextField(dialogueNode.GetText()));
           
            if (GUILayout.Button("Add Node"))
            {
                _creatingNode = dialogueNode;
            }

            DrawLinkButtons(dialogueNode);

            if (GUILayout.Button("Remove Node"))
            {
                _deletingNode = dialogueNode;
            }
            
            GUILayout.EndArea();
        }

        private void DrawLinkButtons(DialogueNode dialogueNode)
        {
            if (_linkingParentNode is null)
            {
                if (GUILayout.Button("Link Node"))
                {
                    _linkingParentNode = dialogueNode;
                }
            }
            else if (_linkingParentNode == dialogueNode)
            {
                if (GUILayout.Button("Cancel Node"))
                {
                    _linkingParentNode = null;
                }
            }
            else if (_linkingParentNode.GetChildren().Contains(dialogueNode.name))
            {
                if (GUILayout.Button("Unlink Node"))
                {
                    _linkingParentNode.RemoveChild(dialogueNode.name);
                    _linkingParentNode = null;
                }
            }
            else
            {
                if (GUILayout.Button("Child Node"))
                {
                    Undo.RecordObject(_selectedDialogue, "Add Dialogue Link");
                    _linkingParentNode.AddChild(dialogueNode.name);
                    _linkingParentNode = dialogueNode;
                }
            }
        }

        private void DrawConnections(DialogueNode dialogueNode)
        {
            var startPosition = new Vector2(dialogueNode.GetRectPosition().xMax, dialogueNode.GetRectPosition().center.y); 
            foreach (var childNode in _selectedDialogue.GetAllChildren(dialogueNode))
            { 
               var endPosition = new Vector2(childNode.GetRectPosition().xMin, childNode.GetRectPosition().center.y);
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
                if (dialogueNode.GetRectPosition().Contains(point))
                {
                    foundNode = dialogueNode;
                }
            }
            return foundNode;
        }
    }
}