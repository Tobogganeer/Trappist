using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tobo.DevConsole
{
    /// <summary>
    /// A console command
    /// </summary>
    public class ConCommand
    {
        // https://developer.valvesoftware.com/wiki/Developer_Console_Control

        // ConCommand( char const *pName, FnCommandCallback callback, char const * pHelpString = 0, int flags = 0, FnCommandCompletionCallback completionFunc = 0 );

        /// <summary>
        /// The name of the command. What the user enters on the command line.
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// The function called when the command is entered.
        /// </summary>
        private readonly CommandCallback Callback;
        /// <summary>
        /// Printed when the help command is used.
        /// </summary>
        public readonly string HelpString;
        /// <summary>
        /// Flags that dictate the command's behaviour.
        /// </summary>
        public readonly CVarFlags Flags;
        /// <summary>
        /// Optional. Auto-completion for the first argument.
        /// </summary>
        private readonly CommandCompletionCallback CompletionCallback;

        public delegate void CommandCallback(CmdArgs args);
        public delegate void CommandCompletionCallback(string partialFirstArg, string[] resultBuffer);

        public static Dictionary<string, ConCommand> cCommands = new Dictionary<string, ConCommand>();

        /// <summary>
        /// Creates and registers a new console command.
        /// </summary>
        /// <param name="name">The name of the command. What the user enters on the command line.</param>
        /// <param name="callback">The function called when the command is entered.</param>
        /// <param name="helpString">Printed when the help command is used.</param>
        /// <param name="flags">Flags for how the command should be used.</param>
        /// <param name="completionCallback">Optional. Auto-completion for the first argument.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="name"/> and <paramref name="callback"/> cannot be null.</exception>
        public ConCommand(string name, CommandCallback callback, string helpString = "", CVarFlags flags = CVarFlags.None, CommandCompletionCallback completionCallback = null)
        {
            if (name == null || name.Trim().Length == 0)
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

            cCommands.Add(Name, this);
        }

        /// <summary>
        /// Calls the command
        /// </summary>
        /// <param name="args">The arguments to pass</param>
        public void Invoke(CmdArgs args)
        {
            if (!CanExecute()) return;

            Callback(args);
        }

        public void GetFirstArgumentAutoCompletionOptions(string partialFirstArg, string[] resultBuffer)
        {
            CompletionCallback?.Invoke(partialFirstArg, resultBuffer);
        }

        public static bool TryGet(string token, out ConCommand cCommand)
            => cCommands.TryGetValue(token, out cCommand);

        bool CanExecute()
        {
            if (Flags.HasFlag(CVarFlags.Cheat) && DefaultCommands.sv_cheats.BoolValue == false)
            {
                Debug.Log($"sv_cheats must be enabled to use \"{Name}\"");
                return false;
            }

            // TODO: Other flags

            return true;
        }

        public override string ToString()
        {
            return $"\"{Name}\" {Flags}\t\t{HelpString}";
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
        /// The 0th argument, aka the command/convar.
        /// </summary>
        public readonly string Token;
        /// <summary>
        /// The number of arguments.
        /// </summary>
        public readonly int ArgC;

        public CmdArgs(string command)
        {
            if (command == null || command.Trim().Length == 0)
                throw new System.ArgumentNullException("command");

            CommandString = command.Trim();
            Args = Parser.ReadArgs(command).ToArray();

            if (Args.Length == 0)
                throw new System.ArgumentException("No arguments found in command > " + command, "command");

            ArgsString = string.Join(' ', Args, 1, Args.Length - 1); // Skip first
            ArgC = Args.Length;
            Token = Args[0];
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
