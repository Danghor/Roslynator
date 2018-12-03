﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CodeMetrics;
using static Roslynator.Logger;

namespace Roslynator.CommandLine
{
    internal abstract class AbstractLinesOfCodeCommandExecutor : MSBuildWorkspaceCommandExecutor
    {
        protected AbstractLinesOfCodeCommandExecutor(string language) : base(language)
        {
        }

        public static ImmutableDictionary<ProjectId, CodeMetricsInfo> CountLinesInParallel(
            IEnumerable<Project> projects,
            Func<string, CodeMetricsCounter> counterFactory,
            CodeMetricsOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var codeMetrics = new ConcurrentBag<(ProjectId projectId, CodeMetricsInfo codeMetrics)>();

            Parallel.ForEach(projects, project =>
            {
                CodeMetricsCounter counter = counterFactory(project.Language);

                CodeMetricsInfo projectMetrics = (counter != null)
                    ? counter.CountLinesAsync(project, options, cancellationToken).Result
                    : CodeMetricsInfo.NotAvailable;

                codeMetrics.Add((project.Id, codeMetrics: projectMetrics));
            });

            return codeMetrics.ToImmutableDictionary(f => f.projectId, f => f.codeMetrics);
        }

        public static async Task<ImmutableDictionary<ProjectId, CodeMetricsInfo>> CountLinesAsync(
            IEnumerable<Project> projects,
            Func<string, CodeMetricsCounter> counterFactory,
            CodeMetricsOptions options = null,
            CancellationToken cancellationToken = default)
        {
            ImmutableDictionary<ProjectId, CodeMetricsInfo>.Builder builder = ImmutableDictionary.CreateBuilder<ProjectId, CodeMetricsInfo>();

            foreach (Project project in projects)
            {
                CodeMetricsCounter counter = counterFactory(project.Language);

                CodeMetricsInfo projectMetrics = (counter != null)
                    ? await counter.CountLinesAsync(project, options, cancellationToken).ConfigureAwait(false)
                    : CodeMetricsInfo.NotAvailable;

                builder.Add(project.Id, projectMetrics);
            }

            return builder.ToImmutableDictionary();
        }

        public static void WriteLinesOfCode(Solution solution, ImmutableDictionary<ProjectId, CodeMetricsInfo> projectsMetrics)
        {
            int maxDigits = projectsMetrics.Max(f => f.Value.CodeLineCount).ToString("n0").Length;
            int maxNameLength = projectsMetrics.Max(f => solution.GetProject(f.Key).Name.Length);

            foreach (KeyValuePair<ProjectId, CodeMetricsInfo> kvp in projectsMetrics
                .OrderByDescending(f => f.Value.CodeLineCount)
                .ThenBy(f => solution.GetProject(f.Key).Name))
            {
                Project project = solution.GetProject(kvp.Key);
                CodeMetricsInfo codeMetrics = kvp.Value;

                string count = (codeMetrics.CodeLineCount >= 0)
                    ? codeMetrics.CodeLineCount.ToString("n0").PadLeft(maxDigits)
                    : "-";

                WriteLine($"{count} {project.Name.PadRight(maxNameLength)} {project.Language}", Verbosity.Normal);
            }
        }
    }
}