using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tobo.DevConsole
{
    public class ConVar
    {
        // https://developer.valvesoftware.com/wiki/ConVar
        // ConVar( const char *pName, const char *pDefaultValue, int flags, const char *pHelpString, bool bMin, float fMin, bool bMax, float fMax );

        public readonly string Name;
        public readonly string DefaultValue;
        public readonly CVarFlags Flags;
        public readonly string HelpString;
        public readonly float? Min;
        public readonly float? Max;
        private readonly ConVarChangedCallback OnChanged;

        string oldValue;

        string _stringValue;
        public string StringValue
        {
            get => _stringValue;
            set
            {
                _stringValue = value;
                UpdateValues();
            }
        }

        bool _boolValue;
        public bool BoolValue
        {
            get => _boolValue;
            set
            {
                _stringValue = value == true ? "1" : "0";
                UpdateValues();
            }
        }

        int _intValue;
        public int IntValue
        {
            get => _intValue;
            set
            {
                float min = Min.HasValue ? Min.Value : Mathf.NegativeInfinity;
                float max = Max.HasValue ? Max.Value : Mathf.Infinity;
                _stringValue = Mathf.RoundToInt(Mathf.Clamp(value, min, max)).ToString();
                UpdateValues();
            }
        }

        float _floatValue;
        public float FloatValue
        {
            get => _floatValue;
            set
            {
                float min = Min.HasValue ? Min.Value : Mathf.NegativeInfinity;
                float max = Max.HasValue ? Max.Value : Mathf.Infinity;
                _stringValue = Mathf.Clamp(value, min, max).ToString();
                UpdateValues();
            }
        }

        public void SetValue(string value) => StringValue = value;
        public void SetValue(bool value) => BoolValue = value;
        public void SetValue(int value) => IntValue = value;
        public void SetValue(float value) => FloatValue = value;


        public Dictionary<string, ConVar> cVars = new Dictionary<string, ConVar>();

        public delegate void ConVarChangedCallback(ConVar cvar, string strOldValue, float flOldValue);


        public ConVar(string name, string defaultValue, CVarFlags flags, string helpString)
            : this(name, defaultValue, flags, helpString, null, null) { }
        public ConVar(string name, string defaultValue, CVarFlags flags, string helpString, float? min, float? max)
            : this(name, defaultValue, flags, helpString, min, max, null) { }
        public ConVar(string name, string defaultValue, CVarFlags flags, string helpString, ConVarChangedCallback onChanged)
            : this(name, defaultValue, flags, helpString, null, null, onChanged) { }
        public ConVar(string name, string defaultValue, CVarFlags flags, string helpString, float? min, float? max, ConVarChangedCallback onChanged)
        {
            if (name == null)
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
            OnChanged = onChanged;

            cVars.Add(name, this);
        }

        void UpdateValues()
        {
            float flOld = _floatValue;

            if (_stringValue == "0" || _stringValue.ToLower() == "false")
                _boolValue = false;
            else
                _boolValue = true;
            if (!int.TryParse(_stringValue, out _intValue))
                _intValue = 0;
            if (!float.TryParse(_stringValue, out _floatValue))
                _floatValue = 0;

            if (oldValue != _stringValue)
                OnChanged?.Invoke(this, oldValue, flOld);

            oldValue = _stringValue;
        }
    }

    [System.Flags]
    public enum CVarFlags
    {
        None = 0,        // Nothing special
        Save = 1 << 0,   // Save this convar to disk
        Cheat = 1 << 1,   // Must have cheats enabled to use
        Replicated = 1 << 2,   // Synced across the network
        Notify = 1 << 3,   // Log changes to chat
        ServerOnly = 1 << 4    // Can only be run on the server
    }
}
