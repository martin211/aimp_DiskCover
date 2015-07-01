﻿using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

namespace AIMP.DiskCover.Core
{
    /// <summary>
    /// Contains algorithmic methods that can be 
    /// used separately from the other code.
    /// </summary>
    public static class Algorithms
    {
        /// <summary>
        /// Calculate width and height for proportionally resized image.
        /// </summary>
        public static void ProportionalImageResize(Single neededWidth, Single neededHeight, Single actualWidth, Single actualHeight, out Single resultWidth, out Single resultHeigth)
        {
            if (neededWidth <= 0)
            {
                throw new ArgumentException("Needed width must be a positive number", "neededWidth");
            }
            if (neededHeight <= 0)
            {
                throw new ArgumentException("Needed height must be a positive number", "neededHeight");
            }
            if (actualWidth <= 0)
            {
                throw new ArgumentException("Actual width must be a positive number", "actualWidth");
            }
            if (actualHeight <= 0)
            {
                throw new ArgumentException("Actual height must be a positive number", "actualHeight");
            }

            if (actualHeight > neededHeight || actualWidth > neededWidth)
            {
                Single newHeight, newWidth;
                Single ratio = actualWidth / actualHeight;

                //If image both dimensions exceed maximum
                if (actualHeight > neededHeight && actualWidth > neededWidth)
                {
                    if (actualWidth >= actualHeight)
                    {
                        //Compress by width first
                        newWidth = neededWidth;
                        newHeight = newWidth / ratio;
                        if (newHeight < 1) newHeight = 1;

                        //If after compression height still more than needed one, compress by height
                        if (newHeight > neededHeight)
                        {
                            newHeight = neededHeight;
                            newWidth = newHeight * ratio;
                            if (newWidth < 1) newWidth = 1;
                        }
                    }
                    else
                    {
                        //Compress by height first
                        newHeight = neededHeight;
                        newWidth = newHeight * ratio;
                        if (newWidth < 1) newWidth = 1;

                        //If after compression width still more than needed one, compress by width
                        if (newWidth > neededWidth)
                        {
                            newWidth = neededWidth;
                            newHeight = newWidth / ratio;
                            if (newHeight < 1) newHeight = 1;
                        }
                    }

                }
                else if (actualHeight > neededHeight)
                {
                    newHeight = neededHeight;
                    newWidth = newHeight * ratio;
                    if (newWidth < 1) newWidth = 1;
                }
                else
                {
                    newWidth = neededWidth;
                    newHeight = newWidth / ratio;
                    if (newHeight < 1) newHeight = 1;
                }

                resultWidth = newWidth;
                resultHeigth = newHeight;
            }
            else
            {
                resultWidth = actualWidth;
                resultHeigth = actualHeight;
            }
        }

        /// <summary>
        /// Checks if passed string can be used as a filename or it contains
        /// some invalid characters.
        /// </summary>
        /// <param name="source">A string to test.</param>
        /// <returns><see langword="true"/> if string contains invalid characters.</returns>
        public static Boolean ContainsInvalidFileNameChars(String source)
        {
            Contract.Requires(source != null);

            Char[] chars = Path.GetInvalidFileNameChars();

            Contract.Assert(chars != null);

            return source.All(s => chars.All(c => s != c));
        }
    }
}
