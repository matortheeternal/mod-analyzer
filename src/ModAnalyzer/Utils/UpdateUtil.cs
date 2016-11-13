using System;
using System.Reflection;
using System.Threading.Tasks;
using Octokit;

namespace ModAnalyzer.Utils
{
    public static class UpdateUtil
    {
        public static async Task<bool> IsUpdateAvailable()
        {
            var gitHubClient = new GitHubClient(new ProductHeaderValue("mod-analyzer"));
            var latestRelease = await gitHubClient.Repository.Release.GetLatest("matortheeternal", "mod-analyzer");
            var latestReleaseVersion = new Version(latestRelease.TagName);
            return latestReleaseVersion > Assembly.GetExecutingAssembly().GetName().Version;
        }
    }
}
