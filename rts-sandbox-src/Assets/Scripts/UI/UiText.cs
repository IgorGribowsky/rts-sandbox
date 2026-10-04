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
        public const string CastAtOnce = "Active · cast at once, no aiming";

        public const string Buff = "Effect · helps";
        public const string Debuff = "Effect · harms";
        public const string TimeLeft = "Time left";
        public const string DamageBonus = "Damage";
        public const string AttackSpeedBonus = "Attack speed";
        public const string DamagePerSecond = "Damage per second";
        public const string AppliedBy = "From";

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

        public const string MaxLevel = "MAX";

        public const string Building = "Building";
        public const string Unit = "Unit";

        public const string InProduction = "In production";
        public const string Queued = "Queued";
        public const string CancelHint = "Click to cancel. The price comes back in full.";

        /// <summary>"420/500". A living unit never reads 0: the current value rounds up.</summary>
        public static string Fraction(float current, float max)
        {
            return Mathf.CeilToInt(current) + "/" + Mathf.RoundToInt(max);
        }

        /// <summary>Income over the place it came from: "+10".</summary>
        public static string Income(int amount)
        {
            return "+" + amount.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Seconds of a cooldown on the button: "4", and "0.6" for the last second.</summary>
        public static string CooldownLeft(float seconds)
        {
            return seconds >= 1f
                ? Mathf.CeilToInt(seconds).ToString(CultureInfo.InvariantCulture)
                : seconds.ToString("0.0", CultureInfo.InvariantCulture);
        }

        /// <summary>Time an effect has left, under its badge: "7", "0.6", "1:05" for a long one.</summary>
        public static string EffectLeft(float seconds)
        {
            if (seconds >= 60f)
            {
                var whole = Mathf.CeilToInt(seconds);
                return (whole / 60).ToString(CultureInfo.InvariantCulture) + ":" + (whole % 60).ToString("00", CultureInfo.InvariantCulture);
            }

            return CooldownLeft(seconds);
        }

        /// <summary>A change in percent: "+50%", "-20%".</summary>
        public static string Percent(float value)
        {
            return (value >= 0f ? "+" : "") + Number(value) + "%";
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
