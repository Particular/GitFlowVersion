using System;
using GitVersion.Logging;
using GitVersion.OutputVariables;
using GitVersion.VersionCalculation;
using GitVersion.VersionCalculation.Cache;
using Microsoft.Extensions.Options;

namespace GitVersion
{
    public class GitVersionCalculator : IGitVersionCalculator
    {
        private readonly ILog log;
        private readonly IGitVersionCache gitVersionCache;
        private readonly INextVersionCalculator nextVersionCalculator;
        private readonly IVariableProvider variableProvider;
        private readonly IOptions<Arguments> options;
        private readonly IGitVersionCacheKeyFactory cacheKeyFactory;
        private readonly GitVersionContext context;

        public GitVersionCalculator(ILog log, IGitVersionCache gitVersionCache, INextVersionCalculator nextVersionCalculator, IVariableProvider variableProvider,
            IOptions<Arguments> options, IGitVersionCacheKeyFactory cacheKeyFactory, IOptions<GitVersionContext> versionContext)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
            this.gitVersionCache = gitVersionCache ?? throw new ArgumentNullException(nameof(gitVersionCache));
            this.nextVersionCalculator = nextVersionCalculator ?? throw new ArgumentNullException(nameof(nextVersionCalculator));
            this.variableProvider = variableProvider ?? throw new ArgumentNullException(nameof(variableProvider));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.cacheKeyFactory = cacheKeyFactory ?? throw new ArgumentNullException(nameof(cacheKeyFactory));
            context = versionContext.Value;
        }

        public VersionVariables CalculateVersionVariables()
        {
            var arguments = options.Value;

            var cacheKey = cacheKeyFactory.Create(arguments.OverrideConfig);
            var versionVariables = arguments.NoCache ? default : gitVersionCache.LoadVersionVariablesFromDiskCache(cacheKey);

            if (versionVariables != null) return versionVariables;

            versionVariables = ExecuteInternal();

            if (arguments.NoCache) return versionVariables;
            try
            {
                gitVersionCache.WriteVariablesToDiskCache(cacheKey, versionVariables);
            }
            catch (AggregateException e)
            {
                log.Warning($"One or more exceptions during cache write:{System.Environment.NewLine}{e}");
            }

            return versionVariables;
        }

        private VersionVariables ExecuteInternal()
        {
            var semanticVersion = nextVersionCalculator.FindVersion();
            return variableProvider.GetVariablesFor(semanticVersion, context.Configuration, context.IsCurrentCommitTagged);
        }
    }
}
