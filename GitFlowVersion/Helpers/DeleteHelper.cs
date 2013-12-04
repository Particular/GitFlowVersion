﻿namespace GitFlowVersion
{
    using System.IO;

    public static class DeleteHelper
    {
        public static void DeleteGitRepository(string directory)
        {
            foreach (var fileName in Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories))
            {
                var fileInfo = new FileInfo(fileName)
                {
                    IsReadOnly = false
                };

                fileInfo.Delete();
            }

            Directory.Delete(directory, true);
        }
    }
}
