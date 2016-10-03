using Octokit;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace ModAnalyzer.Utils {
    public static class UpdateUtil {
        public static async Task<bool> IsUpdateAvailable() {
            GitHubClient gitHubClient = new GitHubClient(new ProductHeaderValue("mod-analyzer"));
            Release latestRelease = await gitHubClient.Repository.Release.GetLatest("matortheeternal", "mod-analyzer");
            Version latestReleaseVersion = new Version(latestRelease.TagName);

            return latestReleaseVersion > Assembly.GetExecutingAssembly().GetName().Version;
        }
    }
}
