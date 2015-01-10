﻿namespace GitVersion.VersionCalculation
{
    using System;
    using BaseVersionCalculators;

    public class NewNextVersionCalculator
    {
        IBaseVersionCalculator baseVersionFinder;

        public NewNextVersionCalculator(IBaseVersionCalculator baseVersionCalculator = null)
        {
            baseVersionFinder = baseVersionCalculator ??
                new BaseVersionCalculator(
                new ConfigNextVersionBaseVersionStrategy(),
                new LastTagBaseVersionStrategy(),
                new MergeMessageBaseVersionStrategy());
        }

        public SemanticVersion FindVersion(GitVersionContext context)
        {
            var baseVersion = baseVersionFinder.GetBaseVersion(context);

            if (baseVersion.ShouldIncrement) IncrementVersion(context, baseVersion);

            return baseVersion.SemanticVersion;
        }

        static void IncrementVersion(GitVersionContext context, BaseVersion baseVersion)
        {
            if (!baseVersion.SemanticVersion.PreReleaseTag.HasTag())
            {
                switch (context.Configuration.Increment)
                {
                    case IncrementStrategy.None:
                        break;
                    case IncrementStrategy.Major:
                        baseVersion.SemanticVersion.Major++;
                        break;
                    case IncrementStrategy.Minor:
                        baseVersion.SemanticVersion.Minor++;
                        break;
                    case IncrementStrategy.Patch:
                        baseVersion.SemanticVersion.Patch++;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                baseVersion.SemanticVersion.PreReleaseTag.Number = baseVersion.SemanticVersion.PreReleaseTag.Number ?? 0;
                baseVersion.SemanticVersion.PreReleaseTag.Number++;
            }
        }
    }
}