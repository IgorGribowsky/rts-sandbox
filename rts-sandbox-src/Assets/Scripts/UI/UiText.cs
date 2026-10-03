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
        /// <summary>"420/500". A living unit never reads 0: the current value rounds up.</summary>
        public static string Fraction(float current, float max)
        {
            return Mathf.CeilToInt(current) + "/" + Mathf.RoundToInt(max);
        }
    }
}
