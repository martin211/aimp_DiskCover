using System;
using System.Drawing;

namespace AIMP.DiskCover.Resources
{
    /// <summary>
    /// Contains properties which load data from corresponding resource files.
    /// </summary>
    // ReSharper disable UnusedMember.Global
    public static class LocalizedData
    {
        public static String AIMPMenuItemName               { get { return ResourceManager.GetResourceString("AIMPMenuItemName"); } }
        public static Bitmap NoCoverImage                   { get { return ResourceManager.GetResourceImage("NoCoverImage"); } }
        public static String PluginName                     { get { return ResourceManager.GetResourceString("PluginName"); } }
        public static String UnusualProportionsMessage      { get { return ResourceManager.GetResourceString("UnusualProportionsMessage"); } }
        public static String CannotFindAIMP                 { get { return ResourceManager.GetResourceString("CannotFindAIMP"); } }
        public static String ConfigIsNewerThanSupported     { get { return ResourceManager.GetResourceString("ConfigIsNewerThanSupported"); } }
        public static String FailedToCreateConfigFolder     { get { return ResourceManager.GetResourceString("FailedToCreateConfigFolder"); } }
        public static String FailedToDeserializePlugin      { get { return ResourceManager.GetResourceString("FailedToDeserializePlugin"); } }
        public static String ErrorOnCoverImageSearch        { get { return ResourceManager.GetResourceString("ErrorOnCoverImageSearch"); } }

        /// <summary>
        /// Resources for settings dialog.
        /// </summary>
        public static class Settings
        {
            public static String Title                      { get { return ResourceManager.GetResourceString("Settings_Title"); } }
            public static String DisplayIconInTaskbar       { get { return ResourceManager.GetResourceString("Settings_DisplayIconInTaskbar"); } }
            public static String EnableResizeModeHotkeys    { get { return ResourceManager.GetResourceString("Settings_EnableResizeModeHotkeys"); } }
            public static String General                    { get { return ResourceManager.GetResourceString("Settings_General"); } }
            public static String SearchRules                { get { return ResourceManager.GetResourceString("Settings_SearchRules"); } }
            public static String Help                       { get { return ResourceManager.GetResourceString("Settings_Help"); } }
            public static String SaveButton                 { get { return ResourceManager.GetResourceString("Settings_SaveButton"); } }
            public static String CancelButton               { get { return ResourceManager.GetResourceString("Settings_CancelButton"); } }
            public static String AvailableRules             { get { return ResourceManager.GetResourceString("Settings_AvailableRules"); } }
            public static String AppliedRules               { get { return ResourceManager.GetResourceString("Settings_AppliedRules"); } }
            public static String ShiftDescription           { get { return ResourceManager.GetResourceString("Settings_ShiftDescription"); } }
            public static String AltDescription             { get { return ResourceManager.GetResourceString("Settings_AltDescription"); } }
            public static String CtrlDescription            { get { return ResourceManager.GetResourceString("Settings_CtrlDescription"); } }
        }

        /// <summary>
        /// Names of find rules.
        /// </summary>
        public static class FindRules
        {
            public static String CoverFile                  { get { return ResourceManager.GetResourceString("FindRules_CoverFile"); } }
            public static String FromTags                   { get { return ResourceManager.GetResourceString("FindRules_FromTags"); } }
            public static String AlbumFile                  { get { return ResourceManager.GetResourceString("FindRules_AlbumFile"); } }
            public static String LastFM                     { get { return ResourceManager.GetResourceString("FindRules_LastFM"); } }
        }
    }
    // ReSharper restore UnusedMember.Global
}
