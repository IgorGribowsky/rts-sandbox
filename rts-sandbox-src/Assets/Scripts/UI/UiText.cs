using System.Globalization;
using UnityEngine;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// Every string the HUD shows, in one place (M-022). The interface speaks
    /// English, like the names of units and skills in the data; a translation
    /// is an edit of this file only.
    /// </summary>
    public static class UiText
    {
        public const string Passive = "Passive";
        public const string CastPoint = "Active · cast at a point";
        public const string CastUnit = "Active · cast at a unit";
        public const string CastArea = "Active · cast at an area";
        public const string CastInstant = "Active";

        public const string Mana = "Mana";
        public const string Cooldown = "Cooldown";
        public const string CastTime = "Cast time";
        public const string Range = "Range";
        public const string Radius = "Radius";
        public const string Key = "Key";
        public const string Cost = "Cost";
        public const string Time = "Time";
        public const string Health = "Health";
        public const string Damage = "Damage";
        public const string AttackRange = "Attack range";
        public const string Size = "Size";
        public const string Supply = "Supply";

        public const string Building = "Building";
        public const string Unit = "Unit";

        /// <summary>"420/500". A living unit never reads 0: the current value rounds up.</summary>
        public static string Fraction(float current, float max)
        {
            return Mathf.CeilToInt(current) + "/" + Mathf.RoundToInt(max);
        }

        /// <summary>Seconds of a cooldown on the button: "4", and "0.6" for the last second.</summary>
        public static string CooldownLeft(float seconds)
        {
            return seconds >= 1f
                ? Mathf.CeilToInt(seconds).ToString(CultureInfo.InvariantCulture)
                : seconds.ToString("0.0", CultureInfo.InvariantCulture);
        }

        public static string Seconds(float seconds)
        {
            return Number(seconds) + " s";
        }

        public static string Number(float value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        /// <summary>A key as the player sees it on the keyboard: "Q", "F1", "=".</summary>
        public static string KeyName(KeyCode key)
        {
            switch (key)
            {
                case KeyCode.None: return "";
                case KeyCode.Equals: return "=";
                case KeyCode.KeypadPlus: return "+";
                case KeyCode.Minus: return "-";
                default:
                    var name = key.ToString();
                    return name.StartsWith("Alpha") ? name.Substring(5) : name;
            }
        }
    }
}
