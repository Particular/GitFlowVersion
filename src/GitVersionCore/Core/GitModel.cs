using System;
using System.Collections;
using System.Collections.Generic;
using LibGit2Sharp;

namespace GitVersion
{
    public class Branch
    {
        private readonly LibGit2Sharp.Branch innerBranch;

        private Branch(LibGit2Sharp.Branch branch)
        {
            innerBranch = branch;
        }

        protected Branch()
        {
        }

        public static implicit operator LibGit2Sharp.Branch(Branch d) => d?.innerBranch;
        public static explicit operator Branch(LibGit2Sharp.Branch b) => b is null ? null : new Branch(b);

        public virtual string CanonicalName => innerBranch?.CanonicalName;
        public virtual string FriendlyName => innerBranch?.FriendlyName;
        public virtual Commit Tip => innerBranch?.Tip;
        public virtual CommitCollection Commits => CommitCollection.FromCommitLog(innerBranch?.Commits);
        public virtual bool IsRemote => innerBranch != null && innerBranch.IsRemote;
        public virtual bool IsTracking => innerBranch != null && innerBranch.IsTracking;
    }

    public class BranchCollection : IEnumerable<Branch>
    {
        private readonly LibGit2Sharp.BranchCollection innerCollection;
        private BranchCollection(LibGit2Sharp.BranchCollection collection) => innerCollection = collection;

        protected BranchCollection()
        {
        }

        public static implicit operator LibGit2Sharp.BranchCollection(BranchCollection d) => d.innerCollection;
        public static explicit operator BranchCollection(LibGit2Sharp.BranchCollection b) => b is null ? null : new BranchCollection(b);

        public virtual IEnumerator<Branch> GetEnumerator()
        {
            foreach (var branch in innerCollection)
                yield return (Branch)branch;
        }

        public virtual Branch Add(string name, Commit commit)
        {
            return (Branch)innerCollection.Add(name, commit);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public virtual Branch this[string friendlyName] => (Branch)innerCollection[friendlyName];

        public void Update(Branch branch, params Action<BranchUpdater>[] actions)
        {
            innerCollection.Update(branch, actions);
        }
    }

    public class TagCollection : IEnumerable<Tag>
    {
        private readonly LibGit2Sharp.TagCollection innerCollection;
        private TagCollection(LibGit2Sharp.TagCollection collection) => innerCollection = collection;

        protected TagCollection()
        {
        }

        public static implicit operator LibGit2Sharp.TagCollection(TagCollection d) => d.innerCollection;
        public static explicit operator TagCollection(LibGit2Sharp.TagCollection b) => b is null ? null : new TagCollection(b);

        public virtual IEnumerator<Tag> GetEnumerator()
        {
            foreach (var branch in innerCollection)
                yield return branch;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public virtual Tag this[string name] => innerCollection[name];
    }

    public class ReferenceCollection : IEnumerable<Reference>
    {
        private readonly LibGit2Sharp.ReferenceCollection innerCollection;
        private ReferenceCollection(LibGit2Sharp.ReferenceCollection collection) => innerCollection = collection;

        protected ReferenceCollection()
        {
        }

        public static implicit operator LibGit2Sharp.ReferenceCollection(ReferenceCollection d) => d.innerCollection;
        public static explicit operator ReferenceCollection(LibGit2Sharp.ReferenceCollection b) => b is null ? null : new ReferenceCollection(b);

        public IEnumerator<Reference> GetEnumerator()
        {
            foreach (var reference in innerCollection)
                yield return reference;
        }

        public virtual Reference Add(string name, string canonicalRefNameOrObjectish)
        {
            return innerCollection.Add(name, canonicalRefNameOrObjectish);
        }

        public virtual DirectReference Add(string name, ObjectId targetId)
        {
            return innerCollection.Add(name, targetId);
        }

        public virtual DirectReference Add(string name, ObjectId targetId, bool allowOverwrite)
        {
            return innerCollection.Add(name, targetId, allowOverwrite);
        }

        public virtual Reference UpdateTarget(Reference directRef, ObjectId targetId)
        {
            return innerCollection.UpdateTarget(directRef, targetId);
        }

        public virtual ReflogCollection Log(string canonicalName)
        {
            return innerCollection.Log(canonicalName);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public virtual Reference this[string name] => innerCollection[name];
        public virtual Reference Head => this["HEAD"];

        public virtual IEnumerable<Reference> FromGlob(string pattern)
        {
            return innerCollection.FromGlob(pattern);
        }
    }

    public class CommitCollection : IEnumerable<Commit>
    {
        private readonly ICommitLog innerCollection;
        private CommitCollection(ICommitLog collection) => innerCollection = collection;

        protected CommitCollection()
        {
        }

        public static ICommitLog ToCommitLog(CommitCollection d) => d.innerCollection;

        public static CommitCollection FromCommitLog(ICommitLog b) => b is null ? null : new CommitCollection(b);

        public virtual IEnumerator<Commit> GetEnumerator()
        {
            foreach (var commit in innerCollection)
                yield return commit;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public virtual CommitCollection QueryBy(CommitFilter commitFilter)
        {
            var commitLog = ((IQueryableCommitLog)innerCollection).QueryBy(commitFilter);
            return FromCommitLog(commitLog);
        }
    }
}
