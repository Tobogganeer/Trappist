using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tobo.DevConsole
{
    public class ConCommand
    {
        // https://developer.valvesoftware.com/wiki/Developer_Console_Control

        // ConCommand( char const *pName, FnCommandCallback callback, char const * pHelpString = 0, int flags = 0, FnCommandCompletionCallback completionFunc = 0 );

        public readonly string Name;
        private readonly CommandCallback Callback;
        public readonly string HelpString;
        public readonly CVarFlags Flags;
        private readonly CommandCompletionCallback CompletionCallback;

        public delegate void CommandCallback(string[] args);
        public delegate string[] CommandCompletionCallback(string partial);

        public ConCommand(string name, CommandCallback callback, string helpString = "", CVarFlags flags = CVarFlags.None, CommandCompletionCallback completionCallback = null)
        {
            if (name == null)
                throw new System.ArgumentNullException("name");
            if (callback == null)
                throw new System.ArgumentNullException("callback");

            Name = name.ToLower().Trim();
            if (Name.Contains(' '))
            {
                Debug.LogWarning($"Cannot create command '{Name}': no spaces allowed in command name!");
                return;
            }

            Callback = callback;
            HelpString = helpString;
            Flags = flags;
            CompletionCallback = completionCallback;
        }
    }

    public class CmdArgs
    {
        /// <summary>
        /// The command, as it was entered.
        /// </summary>
        public readonly string CommandString;
        /// <summary>
        /// The arguments, including the 0th.
        /// </summary>
        public readonly string[] Args;
        /// <summary>
        /// A string containing the arguments, but not the 0th.
        /// Eg. "say Hello World!" = "Hello World!"
        /// </summary>
        public readonly string ArgsString;
        /// <summary>
        /// The number of arguments.
        /// </summary>
        public readonly int ArgC;

        public CmdArgs(string command)
        {
            CommandString = command;
            Args = Parser.ReadArgs(command).ToArray();
            ArgsString = string.Join(' ', Args, 1, Args.Length - 1); // Skip first
            ArgC = Args.Length;
        }

        


        public string this[int index]
        {
            get => Args[index];
        }

        public bool TryGet(int index, out string value)
        {
            if (index < ArgC && index >= 0)
            {
                value = Args[index];
                return true;
            }

            value = null;
            return false;
        }

        public bool TryGet(int index, out float value)
        {
            if (TryGet(index, out string strValue))
            {
                return float.TryParse(strValue, out value);
            }

            value = 0;
            return false;
        }

        public bool TryGet(int index, out int value)
        {
            if (TryGet(index, out string strValue))
            {
                return int.TryParse(strValue, out value);
            }

            value = 0;
            return false;
        }

        public bool TryGet(int index, out bool value)
        {
            if (TryGet(index, out string strValue))
            {
                return bool.TryParse(strValue, out value);
            }
            if (TryGet(index, out float flVal))
            {
                value = flVal == 0 ? false : true;
                return true;
            }

            value = false;
            return false;
        }

        static class Parser
        {
            // Used to handle values in parens

            static readonly char QuoteChar = '\"';
            static readonly char SpaceChar = ' ';

            public static List<string> ReadArgs(string commandString)
            {
                if (commandString == null)
                {
                    throw new System.ArgumentNullException(nameof(commandString));
                }

                List<string> values = new List<string>();

                while (true)
                {
                    string value = ReadNextArg(commandString);
                    if (value == null)
                    {
                        // End of list
                        return values;
                    }
                    else
                    {
                        values.Add(value);
                        if (commandString.StartsWith(QuoteChar))
                            commandString = commandString.Remove(0, value.Length + 2); // Remove the value and the quotes from the start
                        else
                            commandString = commandString.Remove(0, value.Length); // Remove the value from the start
                        if (commandString.StartsWith(SpaceChar))
                        {
                            commandString = commandString.Remove(0, 1); // Remove space
                        }
                        else
                        {
                            // If ReadNextArg() returns a value without a trailing space,
                            //  that is the last value in the list as well
                            return values;
                        }
                    }
                }
            }

            static string ReadNextArg(string value)
            {
                if (value.StartsWith('\"'))
                {
                    return ReadQuotedString(value);
                }
                else
                {
                    return ReadUntilNextSpace(value);
                }
            }

            static string ReadQuotedString(string value)
            {
                if (value == null || value.Length == 0) return null;

                if (value.Length < 2)
                    return value;

                int index = 1; // First char is quote
                while (index < value.Length)
                {
                    if (value[index] == QuoteChar)
                    {
                        if (index == 1) return string.Empty;
                        // If quote found at say index 4 (char 5), return chars 1-3
                        return value.Substring(1, index - 1); // -1 for first quote
                                                              // "Hello,"
                                                              // 01234567
                                                              // index 1, length 6
                    }
                    else
                    {
                        // Else search more
                        index++;
                    }
                }

                // No end quote found, return value itself
                return value;
            }

            static string ReadUntilNextSpace(string value)
            {
                if (value == null || value.Length == 0) return null;

                int index = 0;
                while (index < value.Length)
                {
                    if (value[index] == SpaceChar)
                    {
                        // If comma found at say index 1 (char 2), return just the first char
                        return value.Substring(0, index);
                    }
                    else
                    {
                        // Else search more
                        index++;
                    }
                }

                // No end comma found, return value itself
                return value;
            }
        }
    }
}
