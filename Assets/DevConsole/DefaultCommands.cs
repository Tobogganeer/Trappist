using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tobo.DevConsole
{
    public static class DefaultCommands
    {
        public static ConVar sv_cheats;

        public static ConVar cl_testcheat;
        public static ConVar cl_test;
        public static ConVar mp_thing;
        public static ConVar fps_max;

        public static ConCommand help;

        public static void Register()
        {
            sv_cheats = new ConVar("sv_cheats", "0", CVarFlags.ServerOnly | CVarFlags.Replicated | CVarFlags.Notify, "Enables cheats", CVarType.Bool);

            cl_testcheat = new ConVar("cl_testcheat", "0", CVarFlags.Cheat, "test", CVarType.Bool);
            cl_test = new ConVar("cl_test", "test value", CVarFlags.None, "test");
            mp_thing = new ConVar("mp_thing", "idk", CVarFlags.None, "test");
            fps_max = new ConVar("fps_max", "144", CVarFlags.None, "test (does nothing)");

            help = new ConCommand("help", Help, "Prints help for a command, or all commands if none are specified.", CVarFlags.None, HelpCompletion);
        }

        static void Help(CmdArgs args)
        {
            // TODO: help command
        }

        static void HelpCompletion(string partialFirstArg, string[] resultBuffer)
        {
            // Completion doesn't work yet
            throw new NotImplementedException();
        }
    }
}
