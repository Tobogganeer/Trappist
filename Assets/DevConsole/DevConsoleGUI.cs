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

        Vector2 scrollPos;

        string input;

        bool focusTrigger;
        bool returnTrigger;

        static readonly string InputControlName = "DevConsole Input";

        public DevConsoleGUI(DevConsole console)
        {
            this.console = console;
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
                            GUI.SetNextControlName(InputControlName);
                            input = GUILayout.TextField(input, boxStyle);

                            if (returnTrigger)
                            {
                                if (GUI.GetNameOfFocusedControl() == InputControlName)
                                {
                                    SubmitInput();
                                    GUI.FocusControl(InputControlName);
                                }
                                returnTrigger = false;
                            }

                            if (focusTrigger)
                            {
                                GUI.FocusControl(InputControlName);
                                focusTrigger = false;

                                // Just turned back on
                                if (input != null && input.EndsWith("`"))
                                    input = input.Substring(0, input.Length - 1);
                            }

                            bool hitReturn = input != null && input.EndsWith("\n");

                            if (GUILayout.Button("Submit", boxStyle, GUILayout.Width(60)) || hitReturn)
                            {
                                SubmitInput();
                            }
                        }
                    }
                }
            }
        }

        public void SubmitInput()
        {
            //Keyboard.current[Key.Enter].wasPressedThisFrame
            console.OnCommandEntered(input);
            input = string.Empty;
        }

        public void FocusInput()
        {
            focusTrigger = true;
        }

        public void ReturnPressed()
        {
            returnTrigger = true;
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
