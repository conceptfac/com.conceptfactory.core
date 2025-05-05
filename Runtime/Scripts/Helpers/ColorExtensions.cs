using System;
using UnityEngine;

namespace Concept.Helpers
{
    /// <summary>
    /// Extension methods for working with color-related functionalities.
    /// </summary>
    public static class ColorExtensions
    {
        /// <summary>
        /// Converts a hexadecimal color string (e.g., "#RRGGBB") to a <see cref="Color32"/>.
        /// </summary>
        /// <param name="hex">The hexadecimal color code as a string (without the '#').</param>
        /// <returns>A <see cref="Color32"/> that represents the hexadecimal color.</returns>
        /// <exception cref="FormatException">Thrown if the input hex string is not valid.</exception>
        public static Color32 HexToColor32(string hex)
        {
            if (hex.StartsWith("#"))
            {
                hex = hex.Substring(1);
            }

            // Ensure the hex string is the correct length.
            if (hex.Length != 6)
            {
                throw new FormatException("Hex string must be 6 characters long.");
            }

            byte r = (byte)Convert.ToInt32(hex.Substring(0, 2), 16);
            byte g = (byte)Convert.ToInt32(hex.Substring(2, 2), 16);
            byte b = (byte)Convert.ToInt32(hex.Substring(4, 2), 16);
            byte a = 255;  // Default alpha value of 255 (fully opaque)

            return new Color32(r, g, b, a);
        }
    }
}
