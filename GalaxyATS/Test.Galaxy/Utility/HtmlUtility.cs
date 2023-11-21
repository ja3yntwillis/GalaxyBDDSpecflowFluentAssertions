using System;
using System.Drawing;
using System.Globalization;

namespace Tests.Galaxy.Utility
{
    /// <summary>
    /// Utility methods for extracting different HTML attributes of elements of a webpage
    /// </summary>
    public class HtmlUtility
    {
        /// <summary>
        /// Method to parse string representation of color values and generate the Color object
        /// </summary>
        /// <param name="cssColor">Input color</param>
        /// <returns>Color representation of given string</returns>
        public Color ParseColor(string cssColor)
        {
            cssColor = cssColor.Trim();

            if (cssColor.StartsWith("#"))
            {
                return ColorTranslator.FromHtml(cssColor);
            }
            else if (cssColor.StartsWith("rgb")) //rgb or argb
            {
                int left = cssColor.IndexOf('(');
                int right = cssColor.IndexOf(')');

                if (left < 0 || right < 0)
                    throw new FormatException("rgba format error");
                string noBrackets = cssColor.Substring(left + 1, right - left - 1);

                string[] parts = noBrackets.Split(',');

                int r = int.Parse(parts[0], CultureInfo.InvariantCulture);
                int g = int.Parse(parts[1], CultureInfo.InvariantCulture);
                int b = int.Parse(parts[2], CultureInfo.InvariantCulture);

                if (parts.Length == 3)
                {
                    return Color.FromArgb(r, g, b);
                }
                else if (parts.Length == 4)
                {
                    float a = float.Parse(parts[3], CultureInfo.InvariantCulture);
                    return Color.FromArgb((int)(a * 255), r, g, b);
                }
            }
            throw new FormatException("Not rgb, rgba or hexa color string");
        }

        /// <summary>
        /// Method to convert Color object to corresponding hexadecimal string
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public string GetHexFromRgb(Color color)
        {
            return ColorTranslator.ToHtml(color);
        }

        /// <summary>
        /// Method to convert rem to pixel
        /// </summary>
        /// <param name="size">Size in rem dimension; e.g. "n.00 rem" or "n.00rem" or simply "n.00"</param>
        /// <returns>Size in pixel dimension up to 2 decimal</returns>
        public string ConvertRemToPx(string size)
        {
            if (size.EndsWith("rem"))
            {
                size = size.Substring(0, size.IndexOf("rem")).Trim();
            }
            double d = double.Parse(size, CultureInfo.InvariantCulture);
            return string.Format("{0:0.00}", d * 16);                         // 1 rem = 16 pixels
        }

        /// <summary>
        /// Method to format the pixel value up to 2 decimal
        /// </summary>
        /// <param name="pixel">Raw pixel value with px unit</param>
        /// <returns>Size in pixel dimension up to 2 decimal</returns>
        public string FormatPixelValue(string pixel)
        {
            if (pixel.EndsWith("px"))
            {
                pixel = pixel.Substring(0, pixel.IndexOf("px")).Trim();
            }
            double d = double.Parse(pixel, CultureInfo.InvariantCulture);
            return string.Format("{0:0.00}", d);
        }
    }
}
