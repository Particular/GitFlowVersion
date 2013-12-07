﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GitFlowVersionEnvironment.cs" company="CatenaLogic">
//   Copyright (c) 2012 - 2013 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitFlowVersion
{
    static class GitFlowVersionEnvironment
    {
        public static void Initialize(Arguments arguments)
        {
            TargetPath = arguments.TargetPath;
            LogFilePath = arguments.LogFilePath;
        }

        public static string TargetPath { get; private set; }
        public static string TargetBranch { get; private set; }
        public static string LogFilePath { get; private set; }
    }
}