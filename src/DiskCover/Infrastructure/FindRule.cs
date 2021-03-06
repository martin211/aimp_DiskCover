﻿using System;
using AIMP.DiskCover.CoverFinder;

namespace AIMP.DiskCover.Infrastructure
{
    public enum CoverRuleType
    {
        CoverFile,
        FromTags,
        AlbumFile,
        LastFM,
        AIMP
    }

    public class FindRule
    {
        /// <summary>
        /// Creates and returns an array of all currently available rules.
        /// All rules will be enabled.
        /// </summary>
        public static FindRule[] GetAvailableRules()
        {
            return new []
            {
                new FindRule { Module = LocalCoverFinder.ModuleName, Rule = CoverRuleType.CoverFile },
                //new FindRule { Module = LocalCoverFinder.ModuleName, Rule = CoverRuleType.FromTags },
                new FindRule { Module = LocalCoverFinder.ModuleName, Rule = CoverRuleType.AlbumFile },
                new FindRule { Module = LastFmFinder.ModuleName, Rule = CoverRuleType.LastFM },
                new FindRule { Module = AimpCoverFinder.ModuleName, Rule = CoverRuleType.AIMP }
            };
        }

        /// <summary>
        /// Creates an instance of <see cref="FindRule"/> class.
        /// </summary>
        public FindRule ()
        {
            Enabled = true;
        }

        /// <summary>
        /// Gets or sets value indicating whether this module can be used
        /// by a plugin. Is <see langword="true"/> by default.
        /// </summary>
        public Boolean Enabled { get; set; }

        /// <summary>
        /// Gets or sets name of the Module that handles this type of rule.
        /// </summary>
        public String Module { get; set; }

        /// <summary>
        /// Gets or sets Rule.
        /// </summary>
        public CoverRuleType Rule { get; set; }

        public string Title => this.ToString();

        public string ToolTipHelpText => GetHelpText();

        private string GetHelpText()
        {
            switch (Rule)
            {
                case CoverRuleType.CoverFile:
                    return Localization.DiskCover.Rules.HelpCoverFile;
                case CoverRuleType.AIMP:
                    return Localization.DiskCover.Rules.HelpFromAimp;
                case CoverRuleType.AlbumFile:
                    return Localization.DiskCover.Rules.HelpAlbumFile;
                case CoverRuleType.LastFM:
                    return Localization.DiskCover.Rules.HelpLastFm;
            }

            return string.Empty;
        }

        /// <summary>
        /// Returns a <see cref="String"/> which represents a user-friendly instance name.
        /// </summary>
        public override string ToString()
        {
            switch (Rule)
            {
                case CoverRuleType.CoverFile:
                    return Localization.DiskCover.Options.CoverFile;
                case CoverRuleType.FromTags:
                    return Localization.DiskCover.Options.FromTags;
                case CoverRuleType.AlbumFile:
                    return Localization.DiskCover.Options.AlbumFile;
                case CoverRuleType.LastFM:
                    return Localization.DiskCover.Options.LastFM;
                case CoverRuleType.AIMP:
                    return Localization.DiskCover.Options.Aimp;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}