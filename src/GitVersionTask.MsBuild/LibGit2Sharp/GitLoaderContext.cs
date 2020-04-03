// This code originally copied from https://raw.githubusercontent.com/dotnet/sourcelink/master/src/Microsoft.Build.Tasks.Git/GitLoaderContext.cs
#if !NETFRAMEWORK
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace GitVersion.MSBuildTask.LibGit2Sharp
{
    public sealed class GitLoaderContext : AssemblyLoadContext
    {
        public static GitLoaderContext Instance { get; private set; }

        public static void Init() => Instance = new GitLoaderContext();

        protected override Assembly Load(AssemblyName assemblyName)
        {
            var path = Path.Combine(Path.GetDirectoryName(typeof(GitLoaderContext).Assembly.Location), assemblyName.Name + ".dll");

            if (File.Exists(path))
            {
                return LoadFromAssemblyPath(path);
            }

            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            var modulePtr = IntPtr.Zero;

            if (unmanagedDllName.StartsWith("git2-", StringComparison.Ordinal) ||
                unmanagedDllName.StartsWith("libgit2-", StringComparison.Ordinal))
            {
                var directory = GetNativeLibraryDirectory();
                var extension = GetNativeLibraryExtension();

                if (!unmanagedDllName.EndsWith(extension, StringComparison.Ordinal))
                {
                    unmanagedDllName += extension;
                }

                var nativeLibraryPath = Path.Combine(directory, unmanagedDllName);
                if (!File.Exists(nativeLibraryPath))
                {
                    nativeLibraryPath = Path.Combine(directory, "lib" + unmanagedDllName);
                }

                modulePtr = LoadUnmanagedDllFromPath(nativeLibraryPath);
            }

            return modulePtr != IntPtr.Zero ? modulePtr : base.LoadUnmanagedDll(unmanagedDllName);
        }

        private static string GetNativeLibraryDirectory()
        {
            var dir = Path.GetDirectoryName(typeof(GitLoaderContext).Assembly.Location);
            return Path.Combine(dir, "runtimes", RuntimeIdMap.GetNativeLibraryDirectoryName(), "native");
        }

        private static string GetNativeLibraryExtension()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return ".dll";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return ".dylib";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return ".so";
            }

            throw new PlatformNotSupportedException();
        }
    }
}
#endif
