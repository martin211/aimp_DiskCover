using System;
using System.Linq;
using System.IO;
using System.Diagnostics.Contracts;

namespace AIMP.DiskCover.Resources
{
    /// <summary>
    /// Provides methods to load data from resource file.
    /// </summary>
    internal static class ResourceManager
    {
        /// <summary>
        /// Returns string value associated with the passed resource key.
        /// </summary>
        /// <param name="key">A key for the value.</param>
        /// <returns>Result string.</returns>
        public static String GetResourceString(String key)
        {
            return GetResourceString(key, System.Threading.Thread.CurrentThread.CurrentUICulture.ToString());
        }

        /// <summary>
        /// Returns an image object associated with the passed resource key. 
        /// </summary>
        /// <param name="key">A key for the value.</param>
        /// <returns>Result image.</returns>
        public static System.Drawing.Bitmap GetResourceImage(string key)
        {
            String imagePath = GetResourceString(key);

            if (String.IsNullOrEmpty(imagePath))
            {
                throw new ApplicationException("Incorrect path to the resource image \"" + key + "\"");
            }

            Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("AIMP.DiskCover.Resources." + imagePath);

            Contract.Assume(stream != null);

            return (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(stream);
        }

        /// <summary>
        /// Basing on the passed language, this function tries to find an 
        /// appropriate fallback language.
        /// If the neutral language is passed, the function returns <see langword="null" />
        /// </summary>
        /// <param name="language">The language to find fallback for.</param>
        /// <returns>A language that is the fallback language for the passed one.</returns>
        private static string GetFallbackLanguage(String language)
        {
            const String neutralLanguage = "neutral";

            if (String.Equals(language, neutralLanguage, StringComparison.Ordinal))
            {
                return null;
            }

            if (language.Length > 2)
            {
                return language.Substring(0, 2);
            }

            return neutralLanguage;
        }

        /// <summary>
        /// Returns string value associated with the passed resource key for the specific language.
        /// </summary>
        /// <param name="key">A key for the value.</param>
        /// <param name="language">A language that will be used to search for the value.</param>
        /// <returns>Result string.</returns>
        private static string GetResourceString(string key, string language)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("empty language query string");
            }

            if (string.IsNullOrEmpty(language))
            {
                throw new ArgumentNullException("no language resource given");
            }

            var asm = System.Reflection.Assembly.GetExecutingAssembly();

            // Find the stream with data using language fallback.
            Stream stream = null;
            do
            {
                stream = asm.GetManifestResourceStream("AIMP.DiskCover.Resources." + language.Replace('-', '_') + ".Common.xml");
            }
            while (stream == null && (language = GetFallbackLanguage(language)) != null);

            var xd = System.Xml.Linq.XDocument.Load(stream);

            String result = (from data in xd.Element("root").Elements("data")
                             where data.Attribute("name").Value.Equals(key, StringComparison.Ordinal)
                             select data.Element("value").Value).SingleOrDefault();

            // If such key was not found - perform the fallback.
            if (result == null)
            {
                String fallbackLanguage = GetFallbackLanguage(language);

                if (fallbackLanguage != null)
                {
                    return GetResourceString(key, fallbackLanguage);
                }

                // Fallback cannot be performed.
                throw new ApplicationException("Resource key \"" + key + "\" is not found.");
            }

            return result;
        }
    }
}
