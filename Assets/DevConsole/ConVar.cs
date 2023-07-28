using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tobo.DevConsole
{
    /// <summary>
    /// A console variable
    /// </summary>
    public class ConVar
    {
        // https://developer.valvesoftware.com/wiki/ConVar
        // ConVar( const char *pName, const char *pDefaultValue, int flags, const char *pHelpString, bool bMin, float fMin, bool bMax, float fMax );

        /// <summary>
        /// The name of the convar. What the user enters on the command line.
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// What the default value should be. Note: This value is not set by default. Use SetValue().
        /// </summary>
        public readonly string DefaultValue;
        /// <summary>
        /// Flags that dictate the convar's behaviour.
        /// </summary>
        public readonly CVarFlags Flags;
        /// <summary>
        /// Printed when the help command is used.
        /// </summary>
        public readonly string HelpString;
        /// <summary>
        /// What type of convar is this?
        /// </summary>
        public readonly CVarType Type; // TODO: Implement this
        /// <summary>
        /// The minimum value, if it has one.
        /// </summary>
        public readonly float? Min;
        /// <summary>
        /// The maxiumum value, if it has one.
        /// </summary>
        public readonly float? Max;
        /// <summary>
        /// Called when this convar is changed.
        /// </summary>
        private readonly ConVarChangedCallback OnChanged;

        //string oldValue;

        string _stringValue;
        /// <summary>
        /// Gets/Sets the string value.
        /// </summary>
        public string StringValue
        {
            get
            {
                return _stringValue;
            }
            set
            {
                UpdateValues(value);
            }
        }

        bool _boolValue;
        /// <summary>
        /// Gets/Sets the boolean value.
        /// </summary>
        public bool BoolValue
        {
            get
            {
                return _boolValue;
            }
            set
            {
                UpdateValues(value);
            }
        }

        int _intValue;
        /// <summary>
        /// Gets/Sets the integer value.
        /// </summary>
        public int IntValue
        {
            get
            {
                return _intValue;
            }
            set
            {
                UpdateValues(value);
            }
        }

        float _floatValue;
        /// <summary>
        /// Gets/Sets the float value.
        /// </summary>
        public float FloatValue
        {
            get
            {
                return _floatValue;
            }
            set
            {
                UpdateValues(value);
            }
        }

        public void SetValue(string value) => StringValue = value;
        public void SetValue(bool value) => BoolValue = value;
        public void SetValue(int value) => IntValue = value;
        public void SetValue(float value) => FloatValue = value;


        public static Dictionary<string, ConVar> cVars = new Dictionary<string, ConVar>();

        /// <summary>
        /// ConVar changed callback
        /// </summary>
        /// <param name="cVar">A reference to the convar</param>
        /// <param name="strOldValue">The old value, as a string</param>
        /// <param name="flOldValue">The old value, as a float</param>
        public delegate void ConVarChangedCallback(ConVar cVar, string strOldValue, float flOldValue);

        /// <summary>
        /// Creates and registers a new convar.
        /// </summary>
        /// <param name="name">The name of the convar. What the user enters on the command line.</param>
        /// <param name="defaultValue">What the default value should be. Note: This value is not set by default. Use SetValue().</param>
        /// <param name="flags">Flags that dictate the convar's behaviour.</param>
        /// <param name="helpString">Printed when the help command is used.</param>
        /// <param name="type">What type of convar is this?</param>
        /// <param name="setDefaultValue">Should the value be set to <paramref name="defaultValue"/>?</param>
        public ConVar(string name, string defaultValue, CVarFlags flags, string helpString, CVarType type = CVarType.String, bool setDefaultValue = true)
            : this(name, defaultValue, flags, helpString, null, null, type, setDefaultValue) { }
        /// <summary>
        /// Creates and registers a new convar with a minimum and maximum value.
        /// </summary>
        /// <param name="name">The name of the convar. What the user enters on the command line.</param>
        /// <param name="defaultValue">What the default value should be. Note: This value is not set by default. Use SetValue().</param>
        /// <param name="flags">Flags that dictate the convar's behaviour.</param>
        /// <param name="helpString">Printed when the help command is used.</param>
        /// <param name="min">The minimum value, if it has one.</param>
        /// <param name="max">The maximum value, if it has one.</param>
        /// <param name="type">What type of convar is this?</param>
        /// <param name="setDefaultValue">Should the value be set to <paramref name="defaultValue"/>?</param>
        public ConVar(string name, string defaultValue, CVarFlags flags, string helpString, float? min, float? max, CVarType type = CVarType.String, bool setDefaultValue = true)
            : this(name, defaultValue, flags, helpString, min, max, null, type, setDefaultValue) { }
        /// <summary>
        /// Creates and registers a new convar with a callback when the value is changed.
        /// </summary>
        /// <param name="name">The name of the convar. What the user enters on the command line.</param>
        /// <param name="defaultValue">What the default value should be. Note: This value is not set by default. Use SetValue().</param>
        /// <param name="flags">Flags that dictate the convar's behaviour.</param>
        /// <param name="helpString">Printed when the help command is used.</param>
        /// <param name="onChanged">Called when this convar is changed.</param>
        /// <param name="type">What type of convar is this?</param>
        /// <param name="setDefaultValue">Should the value be set to <paramref name="defaultValue"/>?</param>
        public ConVar(string name, string defaultValue, CVarFlags flags, string helpString, ConVarChangedCallback onChanged, CVarType type = CVarType.String, bool setDefaultValue = true)
            : this(name, defaultValue, flags, helpString, null, null, onChanged, type, setDefaultValue) { }
        /// <summary>
        /// Creates and registers a new convar with a minimum and maximum value, and a callback when the value is changed.
        /// </summary>
        /// <param name="name">The name of the convar. What the user enters on the command line.</param>
        /// <param name="defaultValue">What the default value should be. Note: This value is not set by default. Use SetValue().</param>
        /// <param name="flags">Flags that dictate the convar's behaviour.</param>
        /// <param name="helpString">Printed when the help command is used.</param>
        /// <param name="min">The minimum value, if it has one.</param>
        /// <param name="max">The maximum value, if it has one.</param>
        /// <param name="onChanged">Called when this convar is changed.</param>
        /// <param name="type">What type of convar is this?</param>
        /// <param name="setDefaultValue">Should the value be set to <paramref name="defaultValue"/>?</param>
        public ConVar(string name, string defaultValue, CVarFlags flags, string helpString, float? min, float? max, ConVarChangedCallback onChanged, CVarType type = CVarType.String, bool setDefaultValue = true)
        {
            if (name == null || name.Trim().Length == 0)
                throw new System.ArgumentNullException("name");

            Name = name.ToLower().Trim();
            if (Name.Contains(' '))
            {
                Debug.LogWarning($"Cannot create convar '{Name}': no spaces allowed in convar name!");
                return;
            }

            DefaultValue = defaultValue;
            Flags = flags;
            HelpString = helpString;
            Min = min;
            Max = max;
            Type = type;

            if (setDefaultValue)
            {
                //SetValue(defaultValue);
                SetDefaultValue();
            }

            OnChanged = onChanged;

            cVars.Add(Name, this);
        }

        void UpdateValues(string newValue)
        {
            if (!CanEditConvar()) return;

            string oldStr = _stringValue;
            float oldFl = _floatValue;

            bool constrain = Type != CVarType.String;

            if (constrain && !float.TryParse(newValue, out _))
            {
                Debug.Log($"Invalid value for \"{Name}\": \"{newValue}\"");
                return;
            }

            _stringValue = newValue;

            if ((float.TryParse(_stringValue, out float val) && val == 0) || _stringValue.ToLower().Trim() == "false")
                _boolValue = false;
            else
                _boolValue = true;
            if (!int.TryParse(_stringValue, out _intValue))
                _intValue = 0;
            if (!float.TryParse(_stringValue, out _floatValue))
                _floatValue = 0;

            if (oldStr != _stringValue)
                OnChanged?.Invoke(this, oldStr, oldFl);
        }

        void SetDefaultValue()
        {
            _stringValue = DefaultValue;

            if ((float.TryParse(_stringValue, out float val) && val == 0) || _stringValue.ToLower().Trim() == "false")
                _boolValue = false;
            else
                _boolValue = true;
            if (!int.TryParse(_stringValue, out _intValue))
                _intValue = 0;
            if (!float.TryParse(_stringValue, out _floatValue))
                _floatValue = 0;
        }

        void UpdateValues(bool newValue)
        {
            if (!CanEditConvar()) return;

            string oldStr = _stringValue;
            float oldFl = _floatValue;

            _stringValue = newValue ? "1" : "0";
            _boolValue = newValue;
            _intValue = Mathf.RoundToInt(Mathf.Clamp(newValue ? 1 : 0, Min ?? int.MinValue, Max ?? int.MaxValue));
            _floatValue = _intValue;

            if (oldStr != _stringValue)
                OnChanged?.Invoke(this, oldStr, oldFl);
        }

        void UpdateValues(int newValue)
        {
            if (!CanEditConvar()) return;

            string oldStr = _stringValue;
            float oldFl = _floatValue;

            newValue = Mathf.RoundToInt(Mathf.Clamp(newValue, Min ?? int.MinValue, Max ?? int.MaxValue));
            _stringValue = newValue.ToString();
            _intValue = newValue;
            _floatValue = _intValue;
            _boolValue = newValue != 0;

            if (oldStr != _stringValue)
                OnChanged?.Invoke(this, oldStr, oldFl);
        }

        void UpdateValues(float newValue)
        {
            if (!CanEditConvar()) return;

            string oldStr = _stringValue;
            float oldFl = _floatValue;

            newValue = Mathf.Clamp(newValue, Min ?? float.NegativeInfinity, Max ?? float.PositiveInfinity);
            _stringValue = newValue.ToString();
            _floatValue = newValue;
            _intValue = Mathf.RoundToInt(_floatValue);
            _boolValue = newValue != 0;

            if (oldStr != _stringValue)
                OnChanged?.Invoke(this, oldStr, oldFl);
        }

        bool CanEditConvar()
        {
            if (Flags.HasFlag(CVarFlags.Cheat) && DefaultCommands.sv_cheats.BoolValue == false)
            {
                Debug.Log($"sv_cheats must be enabled to change \"{Name}\"");
                return false;
            }

            // TODO: Other flags

            return true;
        }

        public static bool TryGet(string token, out ConVar cVar)
            => cVars.TryGetValue(token, out cVar);

        public override string ToString()
        {
            return $"\"{Name}\" ({Type}) = \"{StringValue}\" {Flags}\t\t{HelpString}";
        }
    }

    [System.Flags]
    public enum CVarFlags
    {
        /// <summary>
        /// Default behaviour
        /// </summary>
        None = 0,
        /// <summary>
        /// Save this convar to disk
        /// </summary>
        Save = 1 << 0,
        /// <summary>
        /// Must have cheats enabled to edit this convar/use this command
        /// </summary>
        Cheat = 1 << 1,
        /// <summary>
        /// Synced across the network/call command across network
        /// </summary>
        Replicated = 1 << 2,
        /// <summary>
        /// Log changes to chat
        /// </summary>
        Notify = 1 << 3,
        /// <summary>
        /// Can only be run/edited on the server
        /// </summary>
        ServerOnly = 1 << 4,
        ServerOnlyReplicated = ServerOnly | Replicated,
        ServerOnlyReplicatedCheat = ServerOnly | Replicated | Cheat,
    }

    /// <summary>
    /// Dictates how the data is sent over the network and stored on disk
    /// </summary>
    public enum CVarType
    {
        String,
        Float,
        Int,
        Bool,
    }
}
