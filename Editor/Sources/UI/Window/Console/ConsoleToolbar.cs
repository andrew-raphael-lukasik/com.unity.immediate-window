﻿using UnityEngine.Experimental.UIElements;
using UnityEditor.ImmediateWindow.Services;
using UnityEngine;
 
namespace UnityEditor.ImmediateWindow.UI
{
    internal class ConsoleToolbar : VisualElement
    {
        internal new class UxmlFactory : UxmlFactory<ConsoleToolbar> { }
        private readonly VisualElement root;
        private bool MultiLineMode
        {
            get { return State.Instance.MultiLineMode;}
            set { State.Instance.MultiLineMode = value; }
        }
        private bool ShowContext
        {
            get { return State.Instance.ShowContext;}
            set { State.Instance.ShowContext = value; }
        }

        public Console Console { get; set; }
        
        public ConsoleToolbar()
        {
            root = Resources.GetTemplate("ConsoleToolbar.uxml");
            Add(root);
            root.StretchToParentSize();
            
            ClearButton.RegisterCallback<MouseDownEvent>(ClearClick);
            ClearButton.RegisterCallback<MouseUpEvent>(ClearStopClick);
            MultilineButton.RegisterCallback<MouseDownEvent>(ConsoleExpandToggle);
            ContextButton.RegisterCallback<MouseDownEvent>(ContextExpandToggle);
            RunButton.RegisterCallback<MouseDownEvent>(RunClick);
            RunButton.RegisterCallback<MouseUpEvent>(ClearRunClick);
            ResetButton.RegisterCallback<MouseUpEvent>(ResetClick);
            TemplatesDropdown.RegisterCallback<MouseUpEvent>(TemplatesClick);
            PrivateToggle.OnValueChanged(OnPrivateToggle);

            PrivateToggle.value = State.Instance.ShowPrivate;

            OnPrivateToggle(null);
            RefreshConsoleState();
            RefreshContextState();
        }

        private void TemplatesClick(MouseUpEvent evt)
        {
            if (evt.propagationPhase != PropagationPhase.AtTarget)
                return;

            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Simple"), false, obj => CreateTemplate("Simple"), null);
            menu.AddItem(new GUIContent("Function"), false, obj => CreateTemplate("Function"), null);
            
            var menuPosition = new Vector2(TemplatesDropdown.layout.xMin, TemplatesDropdown.layout.center.y);
            menuPosition = this.LocalToWorld(menuPosition);
            var menuRect = new Rect(menuPosition, Vector2.zero);
            menu.DropDown(menuRect);
        }

        private void CreateTemplate(string name)
        {
            var code = "using UnityEditor;\nusing UnityEngine;\n\n";
            if (name == "Simple")
            {
                code += "public class Test\n{\n  public void Func()\n  {\n    Debug.Log(\"works!\");\n  }\n}\nvar x = new Test();\nx.Func();\nx\n";
            }
            else if (name == "Function")
            {
                code += "public class Test\n{\n  public object Func()\n  {\n    return 123;\n  }\n}\nvar x = new Test();\nx.Func()\n";                
            }

            Console.SetMultilineCode(code);
        }

        private void ResetClick(MouseUpEvent evt)
        {
            Evaluator.Instance.Init();
            Console.ConsoleOutput.ClearLog();
            History.Instance.Clear();        // Only clearing history in case anything went wrong with it as a way to reset to valid state
            ImmediateWindow.CurrentWindow.Context.Reset();
            Console.CurrentCommand = null;
        }

        private void ClearRunClick(MouseUpEvent evt)
        {
            RunButton.RemoveFromClassList("pressed");
            Console.CodeEvaluate();
        }

        private void RunClick(MouseDownEvent evt)
        {
            RunButton.AddToClassList("pressed");
        }

        private void ConsoleExpandToggle(MouseDownEvent evt)
        {
            MultiLineMode = !MultiLineMode;
            RefreshConsoleState();
        }

        private void ContextExpandToggle(MouseDownEvent evt)
        {
            ShowContext = !ShowContext;
            RefreshContextState();
        }

        void RefreshContextState()
        {
            ContextButton.RemoveFromClassList("pressed");
            ImmediateWindow.CurrentWindow.SetSideViewVisibility(ShowContext);
            
            if (ShowContext)
                ContextButton.AddToClassList("pressed");
        }

        void RefreshConsoleState()
        {
            if (MultiLineMode)
                MultilineButton.AddToClassList("pressed");
            else
                MultilineButton.RemoveFromClassList("pressed");
            
            UIUtils.SetElementDisplay(TemplatesDropdown, MultiLineMode);
            
            if (Console != null)
                Console.SetMode(MultiLineMode);
        }

        private void ClearStopClick(MouseUpEvent evt)
        {
            ClearButton.RemoveFromClassList("pressed");
        }

        private void ClearClick(MouseDownEvent evt)
        {
            ClearButton.AddToClassList("pressed");
            Console.ConsoleOutput.ClearLog();
        }

        private void OnPrivateToggle(ChangeEvent<bool> evt)
        {
            PropertyUtils.ShowPrivate = PrivateToggle.value;
            State.Instance.ShowPrivate = PrivateToggle.value;
        }

        private Label TemplatesDropdown {get { return root.Q<Label>("templates"); }}
        private Button ResetButton {get { return root.Q<Button>("reset"); }}
        private Label ClearButton {get { return root.Q<Label>("clear"); }}
        private Label RunButton {get { return root.Q<Label>("run"); }}
        private Label MultilineButton {get { return root.Q<Label>("multiline"); }}
        private Label ContextButton {get { return root.Q<Label>("context"); }}
        private Toggle PrivateToggle {get { return root.Q<Toggle>("viewPrivate"); }}
    }
}