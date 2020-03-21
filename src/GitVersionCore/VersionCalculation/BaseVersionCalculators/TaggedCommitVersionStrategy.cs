using System;
using System.Collections.Generic;
using System.Linq;
using GitVersion.Common;
using GitVersion.Extensions;
using LibGit2Sharp;

namespace GitVersion.VersionCalculation
{
    /// <summary>
    /// Version is extracted from all tags on the branch which are valid, and not newer than the current commit.
    /// BaseVersionSource is the tag's commit.
    /// Increments if the tag is not the current commit.
    /// </summary>
    public class TaggedCommitVersionStrategy : VersionStrategyBase
    {
        private readonly IGitRepoMetadataProvider gitRepoMetadataProvider;

        public TaggedCommitVersionStrategy(IGitRepoMetadataProvider gitRepoMetadataProvider, IGitVersionContextFactory gitVersionContextFactory) : base(gitVersionContextFactory)
        {
            this.gitRepoMetadataProvider = gitRepoMetadataProvider ?? throw new ArgumentNullException(nameof(gitRepoMetadataProvider));
        }

        public override IEnumerable<BaseVersion> GetVersions()
        {
            var context = ContextFactory.Context;
            return GetTaggedVersions(context.CurrentBranch, context.CurrentCommit.When());
        }

        internal IEnumerable<BaseVersion> GetTaggedVersions(Branch currentBranch, DateTimeOffset? olderThan)
        {
            var context = ContextFactory.Context;
            var allTags = gitRepoMetadataProvider.GetValidVersionTags(context.Configuration.GitTagPrefix, olderThan);

            var tagsOnBranch = currentBranch
                .Commits
                .SelectMany(commit => { return allTags.Where(t => IsValidTag(t.Item1, commit)); })
                .Select(t =>
                {
                    if (t.Item1.PeeledTarget() is Commit)
                        return new VersionTaggedCommit(t.Item1.PeeledTarget() as Commit, t.Item2, t.Item1.FriendlyName);

                    return null;
                })
                .Where(a => a != null)
                .Take(5)
                .ToList();

            return tagsOnBranch.Select(t => CreateBaseVersion(context, t));
        }

        private BaseVersion CreateBaseVersion(GitVersionContext context, VersionTaggedCommit version)
        {
            var shouldUpdateVersion = version.Commit.Sha != context.CurrentCommit.Sha;
            var baseVersion = new BaseVersion(FormatSource(version), shouldUpdateVersion, version.SemVer, version.Commit, null);
            return baseVersion;
        }

        protected virtual string FormatSource(VersionTaggedCommit version)
        {
            return $"Git tag '{version.Tag}'";
        }

        protected virtual bool IsValidTag(Tag tag, Commit commit)
        {
            return tag.PeeledTarget() == commit;
        }

        protected class VersionTaggedCommit
        {
            public string Tag;
            public Commit Commit;
            public SemanticVersion SemVer;

            public VersionTaggedCommit(Commit commit, SemanticVersion semVer, string tag)
            {
                Tag = tag;
                Commit = commit;
                SemVer = semVer;
            }

            public override string ToString()
            {
                return $"{Tag} | {Commit} | {SemVer}";
            }
        }
    }
}
