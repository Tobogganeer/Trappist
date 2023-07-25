using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tobo.DevConsole
{
    public class DevConsole : MonoBehaviour
    {
        public static DevConsole Instance { get; private set; }
        private void Awake()
        {
            Instance = this;
            Application.logMessageReceived += LogCallback;
            messages = new Queue<Message>(_maxMessages);
        }

        public static bool Open => Instance.show;
        int _maxMessages = 100;
        public int MaxMessages
        {
            get
            {
                return _maxMessages;
            }
            set
            {
                _maxMessages = value;
                messages = new Queue<Message>(_maxMessages);
            }
        }

        public bool show;
        public Key toggleKey = Key.Backquote;
        public Vector2 position = new Vector2(20, 20);
        public Vector2 size = new Vector2(800, 600);

        [Space]
        public Texture2D boxTexture;
        public Font font;
        public int fontSize = 10;
        public Color windowColour = new Color(140f / 255f, 140f / 255f, 140f / 255f);
        public Color backgroundColour = new Color(61f / 255f, 61f / 255f, 61f / 255f);
        [Space]
        public Color logColour = new Color(255f / 255f, 255f / 255f, 255f / 255f);
        public Color warningColour = new Color(250f / 255f, 209f / 255f, 95f / 255f);
        public Color errorColour = new Color(242f / 255f, 82f / 255f, 68f / 255f);

        DevConsoleGUI gui;

        Queue<Message> messages;

        string inputBuffer;

        private void Start()
        {
            gui = new DevConsoleGUI(this);
        }

        public void Toggle(bool? setVisible = null)
        {
            if (setVisible.HasValue)
                show = setVisible.Value;
            else
                show = !show;

            if (show)
                gui.FocusInput();
        }

        private void Update()
        {
            if (Keyboard.current[toggleKey].wasPressedThisFrame)
                Toggle();
            if (Keyboard.current[Key.Enter].wasPressedThisFrame)
                gui.ReturnPressed();

            if (inputBuffer != null)
            {
                ProcessInput();
                inputBuffer = null;
            }
        }

        private void OnGUI()
        {
            if (!show) return;

            gui.Draw(messages);
        }


        public void OnCommandEntered(string input)
        {
            //Debug.Log("> " + input);
            // Basically, OnGUI is called multiple times per frame.
            // When we logged this, it would create another message.
            // Then, the second GUI loop would have a different amount of things to draw
            //  so it would throw errors.
            // Now, we store the input until Update,
            //  so we are sure that all the GUI calls have finished.
            inputBuffer = input;

        }

        void ProcessInput()
        {
            string input = inputBuffer.Trim();
            Debug.Log("> " + input);
        }

        private void LogCallback(string condition, string stackTrace, LogType type)
        {
            while (messages.Count > MaxMessages - 1)
                messages.Dequeue();
            if (type == LogType.Error || type == LogType.Exception)
            {
                messages.Enqueue(new Message(Message.Type.Error, condition + " - Stacktrace: " + stackTrace));
            }
            else if (type == LogType.Warning)
            {
                messages.Enqueue(new Message(Message.Type.Warning, condition));
            }
            else if (type == LogType.Log)
            {
                messages.Enqueue(new Message(Message.Type.Log, condition));
            }
        }

        public class Message
        {
            public Type type;
            public string message;

            public Message(Type type, string message)
            {
                this.type = type;
                this.message = message;
            }

            public enum Type
            {
                Log,
                Warning,
                Error,
            }
        }
    }
}