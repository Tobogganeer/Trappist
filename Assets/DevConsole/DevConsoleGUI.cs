using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tobo.DevConsole
{
    public class DevConsoleGUI
    {
        DevConsole console;

        GUIStyle boxStyle;
        GUIStyle headerStyle;
        GUIStyle scrollbarStyle;
        GUIStyle fontStyle;
        //GUIStyle windowStyle;
        //GUIStyle backgroundStyle;

        bool inited;

        // Padding etc
        int backgroundPaddingTop = 24;
        int backgroundPaddingBottomLeftRight = 4;
        int headerPaddingX = 5;
        int headerPaddingY = 8;
        int headerFontSize = 12;
        int autoCompletePaddingX = 8;
        int autoCompletePaddingY = 4;

        Vector2 scrollPos;

        string input;

        int focusTrigger;
        bool returnTrigger;
        bool autoCompleteTrigger;
        bool wasFocused; // It keeps unfocusing when a new message comes in

        List<string> autoCompleteStrings;
        const int MaxAutoCompleteStrings = 16;

        static readonly string InputControlName = "DevConsole Input";
        int desiredCaret = -1;

        int bufferSelectionIndex = -1;
        Color autoCompleteGrey = new Color(0.65f, 0.65f, 0.65f);

        public DevConsoleGUI(DevConsole console)
        {
            this.console = console;
        }

        public void Update()
        {
            if (autoCompleteTrigger)
            {
                autoCompleteTrigger = false;
                FillAutoCompleteStrings(input);
                bufferSelectionIndex = -1; // Everytime we change autocomplete, reset the buffer index (previous commands)
            }

            if (log != null)
            {
                Debug.Log(log);
                log = null;
            }

            if (Keyboard.current[Key.LeftCtrl].wasPressedThisFrame)
                Debug.Log("Test");
        }

        public void Draw(Queue<DevConsole.Message> messages)
        {
            if (!inited)
                Init();

            using (new GUIBackgroundColour(console.windowColour))
                GUI.Box(new Rect(console.position, console.size), "", boxStyle);

            Vector2 headerPosition = console.position + new Vector2(backgroundPaddingBottomLeftRight + headerPaddingX, backgroundPaddingTop - headerFontSize - headerPaddingY);
            using (new GUIBackgroundColour(Color.clear))
                GUI.Label(new Rect(headerPosition, new Vector2(200, 40)), "Console", headerStyle);

            Vector2 bgPosition = console.position;
            Vector2 bgSize = console.size;
            bgSize.x -= backgroundPaddingBottomLeftRight * 2;
            bgSize.y -= backgroundPaddingBottomLeftRight + backgroundPaddingTop;
            bgPosition.x += backgroundPaddingBottomLeftRight;
            bgPosition.y += backgroundPaddingTop; // Top is y-min
            Rect backgroundRect = new Rect(bgPosition, bgSize);

            using (new GUILayout.AreaScope(backgroundRect))
            {
                using (new GUIBackgroundColour(console.backgroundColour))
                {
                    using (new GUILayout.VerticalScope(boxStyle, GUILayout.Width(bgSize.x), GUILayout.Height(bgSize.y)))
                    {
                        //GUILayout.Box(input, boxStyle, GUILayout.ExpandHeight(true));
                        using (GUILayout.ScrollViewScope scroll = new GUILayout.ScrollViewScope(scrollPos, GUI.skin.horizontalScrollbar, scrollbarStyle, GUILayout.ExpandHeight(true)))
                        {
                            using (new GUIBackgroundColour(Color.clear))
                            {
                                /*
                                GUILayout.Box("Test\nSecond Line", fontStyle);
                                GUILayout.Box("Test 2", fontStyle);
                                GUILayout.Box("Test 3", fontStyle, GUILayout.Height(450));
                                GUILayout.Box("Test 4", fontStyle, GUILayout.Height(650));
                                */
                                foreach (DevConsole.Message message in messages)
                                {
                                    using (new GUIColour(GetColour(message.type)))
                                    {
                                        GUILayout.Box(message.message, fontStyle);
                                    }
                                }
                            }
                            scrollPos = scroll.scrollPosition;
                        }

                        using (new GUILayout.HorizontalScope(GUILayout.Height(35)))
                        {
                            string old = input;
                            
                            GUI.SetNextControlName(InputControlName);
                            input = GUILayout.TextField(input, boxStyle);
                            if (GUI.GetNameOfFocusedControl() == InputControlName)
                                MoveCaret();

                            if (old != input)
                                autoCompleteTrigger = true; // Update autocomplete

                            if (focusTrigger > 0)
                            {
                                GUI.FocusControl(InputControlName);
                                focusTrigger--;
                                //autoCompleteTrigger = true; // Update on focus
                                // Actually don't

                                // Just turned back on
                                if (input != null && input.EndsWith("`"))
                                {
                                    input = input.Substring(0, input.Length - 1);
                                    // autoCompleteTrigger = true; // Do it here instead
                                    // nvm lol im indecisive
                                }
                            }

                            if (returnTrigger)
                            {
                                if (GUI.GetNameOfFocusedControl() == InputControlName)
                                {
                                    SubmitInput();
                                    //GUI.FocusControl(InputControlName);
                                    focusTrigger = 5; // Focus a few times bc it wasn't really working before
                                }
                                returnTrigger = false;
                            }

                            // Idk if this actually does anything, the input won't end with \n I don't think...
                            bool hitReturn = input != null && input.EndsWith("\n");

                            if (GUILayout.Button("Submit", boxStyle, GUILayout.Width(60)) || hitReturn)
                            {
                                SubmitInput();
                            }
                        }
                    }
                }
            }

            wasFocused = GUI.GetNameOfFocusedControl() == InputControlName;

            DrawAutoComplete();
        }

        void DrawAutoComplete()
        {
            if (autoCompleteStrings == null || autoCompleteStrings.Count == 0) return;

            Vector2 startPos = console.position + Vector2.up * console.size.y; // Bottom left
            Vector2 size = Vector2.zero;

            foreach (string str in autoCompleteStrings)
            {
                Vector2 strSize = fontStyle.CalcSize(new GUIContent(str));
                size.x = Mathf.Max(size.x, strSize.x);
                size.y += strSize.y;
            }

            Vector2 padding = new Vector2(autoCompletePaddingX, autoCompletePaddingY);

            Rect contentRect = new Rect(startPos + padding / 2, size);
            Rect rect = new Rect(startPos, size + padding);

            using (new GUIBackgroundColour(console.windowColour))
                GUI.Box(rect, "", boxStyle);

            using (new GUILayout.AreaScope(contentRect))
            {
                using (new GUILayout.VerticalScope())
                {
                    using (new GUIBackgroundColour(Color.clear))
                    {
                        for (int i = 0; i < autoCompleteStrings.Count; i++)
                        {
                            using (new GUIColour(i == bufferSelectionIndex ? Color.white : autoCompleteGrey))
                                GUILayout.Box(autoCompleteStrings[i], fontStyle);
                        }
                    }
                }
            }
        }

        string log;
        void MoveCaret()
        {
            if (desiredCaret < 0) return;

            int controlID = GUIUtility.keyboardControl;
            TextEditor textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), controlID);
            if (!string.IsNullOrWhiteSpace(textEditor.text))
                textEditor.selectIndex = textEditor.cursorIndex = desiredCaret;
            desiredCaret = -1;
            //log = textEditor.cursorIndex.ToString();
        }

        public void SubmitInput()
        {
            //Keyboard.current[Key.Enter].wasPressedThisFrame
            console.OnCommandEntered(input);
            input = string.Empty;
            autoCompleteTrigger = true; // Update autocomplete
        }

        public void FocusInput()
        {
            focusTrigger = 1;
        }

        public void UpArrow()
        {
            string newInput = null;

            if (autoCompleteStrings.Count > 0) // Go through these
            {
                // Ascending down = decrease index
                bufferSelectionIndex--;
                if (bufferSelectionIndex < 0)
                    bufferSelectionIndex = autoCompleteStrings.Count - 1;
                newInput = input = autoCompleteStrings[bufferSelectionIndex];
                // Setting input without the autoCompleteTrigger won't prompt autoComplete refresh
            }
            else // Look through history
            {
                // Ascending up, but most recent at top of list
                if (console.previousInputBuffer.Count > 0)
                {
                    bufferSelectionIndex--;
                    if (bufferSelectionIndex < 0)
                        bufferSelectionIndex = console.previousInputBuffer.Count - 1;
                    newInput = input = console.previousInputBuffer[bufferSelectionIndex];
                }
            }

            if (newInput != null)
                desiredCaret = newInput.Length;
        }

        public void DownArrow()
        {
            string newInput = null;

            if (autoCompleteStrings.Count > 0) // Go through these
            {
                bufferSelectionIndex++;
                if (bufferSelectionIndex >= autoCompleteStrings.Count)
                    bufferSelectionIndex = 0;
                newInput = input = autoCompleteStrings[bufferSelectionIndex];
            }
            else // Look through history
            {
                if (console.previousInputBuffer.Count > 0)
                {
                    bufferSelectionIndex++;
                    if (bufferSelectionIndex >= console.previousInputBuffer.Count)
                        bufferSelectionIndex = 0;
                    newInput = input = console.previousInputBuffer[bufferSelectionIndex];
                }
            }

            if (newInput != null)
                desiredCaret = newInput.Length;
        }

        public void Tab()
        {
            if (autoCompleteStrings.Count > 0)
                DownArrow();
            // Don't go through history with tab
        }

        public void ReturnPressed()
        {
            returnTrigger = true;
        }

        public void OnNewMessage()
        {
            scrollPos = new Vector2(0, 1000000); // Scroll to bottom
            if (wasFocused) // If we were typing before, keep typing
                focusTrigger = 1;
        }

        private void FillAutoCompleteStrings(string partialString)
        {
            if (autoCompleteStrings == null)
                autoCompleteStrings = new List<string>(MaxAutoCompleteStrings);

            if (partialString == null || partialString.Trim().Length == 0)
            {
                autoCompleteStrings.Clear();
                return;
            }

            partialString = partialString.ToLower().Trim();

            autoCompleteStrings.Clear();

            int matches = 0;

            char space = ' ';

            // cvar start > command start > cvar contains > command contains

            foreach (ConVar cVar in ConVar.cVars.Values)
            {
                if (cVar.Name.StartsWith(partialString))
                {
                    // Add a space onto everything so it's easier to type arguments
                    autoCompleteStrings.Add(cVar.Name + space);
                    matches++;
                    if (matches == MaxAutoCompleteStrings)
                        return;
                }
            }
            foreach (ConCommand command in ConCommand.cCommands.Values)
            {
                if (command.Name.StartsWith(partialString))
                {
                    autoCompleteStrings.Add(command.Name + space);
                    matches++;
                    if (matches == MaxAutoCompleteStrings)
                        return;
                }
            }

            foreach (ConVar cVar in ConVar.cVars.Values)
            {
                // These might have been added already
                if (cVar.Name.Contains(partialString) && !autoCompleteStrings.Contains(cVar.Name + space))
                {
                    autoCompleteStrings.Add(cVar.Name + space);
                    matches++;
                    if (matches == MaxAutoCompleteStrings)
                        return;
                }
            }
            foreach (ConCommand command in ConCommand.cCommands.Values)
            {
                if (command.Name.Contains(partialString) && !autoCompleteStrings.Contains(command.Name + space))
                {
                    autoCompleteStrings.Add(command.Name + space);
                    matches++;
                    if (matches == MaxAutoCompleteStrings)
                        return;
                }
            }
        }

        Color GetColour(DevConsole.Message.Type type)
        {
            return type switch
            {
                DevConsole.Message.Type.Log => console.logColour,
                DevConsole.Message.Type.Warning => console.warningColour,
                DevConsole.Message.Type.Error => console.errorColour,
                _ => console.logColour,
            };
        }

        void Init()
        {
            inited = true;

            //font = Font.CreateDynamicFontFromOSFont("Tahoma", 16);
            // Couldn't disable anti-aliasing

            boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = console.boxTexture;
            boxStyle.border = new RectOffset(8, 8, 8, 8);
            boxStyle.fontSize = console.fontSize;
            boxStyle.alignment = TextAnchor.UpperLeft;
            boxStyle.font = console.font;
            boxStyle.normal.textColor = Color.white;

            fontStyle = new GUIStyle(boxStyle);
            fontStyle.margin = new RectOffset(0, 0, 0, 0);
            fontStyle.border = new RectOffset(0, 0, 0, 0);
            fontStyle.wordWrap = true;

            headerStyle = new GUIStyle(boxStyle);
            headerStyle.fontSize = headerFontSize;

            scrollbarStyle = new GUIStyle(GUI.skin.verticalScrollbar);
            scrollbarStyle.normal.background = console.boxTexture;
        }


        // Util classes
        class GUIColour : GUI.Scope
        {
            Color stored;

            public GUIColour(Color c)
            {
                stored = GUI.color;
                GUI.color = c;
            }

            protected override void CloseScope()
            {
                GUI.color = stored;
            }
        }
        class GUIBackgroundColour : GUI.Scope
        {
            Color stored;

            public GUIBackgroundColour(Color c)
            {
                stored = GUI.backgroundColor;
                GUI.backgroundColor = c;
            }

            protected override void CloseScope()
            {
                GUI.backgroundColor = stored;
            }
        }
    }
}
