using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// Grey copies of HUD pictures for cooldown and disabled states, made once
    /// per picture on first use. A tint cannot do this — it only multiplies the
    /// colour and leaves a dark coloured picture, not a grey one.
    /// </summary>
    public static class GrayscaleCache
    {
        private static readonly Dictionary<Texture2D, Texture2D> _cache = new Dictionary<Texture2D, Texture2D>();

        public static Texture2D Get(Texture2D source)
        {
            if (source == null)
            {
                return null;
            }

            if (_cache.TryGetValue(source, out var gray) && gray != null)
            {
                return gray;
            }

            // Pictures outside Assets/UI/Icons may not be readable; they simply stay coloured.
            if (!source.isReadable)
            {
                return source;
            }

            var pixels = source.GetPixels32();
            for (var i = 0; i < pixels.Length; i++)
            {
                var p = pixels[i];
                var luminance = (byte)Mathf.Clamp(p.r * 0.299f + p.g * 0.587f + p.b * 0.114f, 0f, 255f);
                pixels[i] = new Color32(luminance, luminance, luminance, p.a);
            }

            gray = new Texture2D(source.width, source.height, TextureFormat.RGBA32, true)
            {
                name = source.name + " (gray)",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Trilinear,
            };
            gray.SetPixels32(pixels);
            gray.Apply(true, true);

            _cache[source] = gray;
            return gray;
        }
    }
}
