using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EncryptTool.Services
{
    public sealed class UpdateManifest
    {
        public string Version { get; init; } = string.Empty;
        public string? DownloadUrl { get; init; }
        public string? ReleaseUrl { get; init; }
        public string? ReleaseNotes { get; init; }
        public bool ForceUpdate { get; init; }
        public List<UpdatePackage> Packages { get; init; } = new();
    }

    public sealed class UpdatePackage
    {
        public string Rid { get; init; } = string.Empty;
        public string DownloadUrl { get; init; } = string.Empty;
        public string? FileName { get; init; }
        public string? Sha256 { get; init; }
    }

    public sealed record UpdateCheckResult(
        bool HasUpdate,
        Version CurrentVersion,
        Version? LatestVersion,
        string CurrentRid,
        UpdateManifest? Manifest,
        UpdatePackage? MatchedPackage,
        string? ErrorMessage);

    public static class UpdateService
    {
        private const string UpdateManifestUrlKey = "UpdateManifestUrl";
        private static readonly HttpClient HttpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(6)
        };

        public static async Task<UpdateCheckResult> CheckForUpdateAsync(CancellationToken cancellationToken = default)
        {
            Version currentVersion = GetCurrentVersion();
            string currentRid = GetCurrentRid();
            string manifestUrl = GetUpdateManifestUrl();

            if (string.IsNullOrWhiteSpace(manifestUrl))
                return new UpdateCheckResult(false, currentVersion, null, currentRid, null, null, null);

            try
            {
                using var response = await HttpClient.GetAsync(manifestUrl, cancellationToken);
                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                var manifest = await JsonSerializer.DeserializeAsync<UpdateManifest>(
                    stream,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                    cancellationToken);

                if (manifest is null || string.IsNullOrWhiteSpace(manifest.Version))
                    return new UpdateCheckResult(false, currentVersion, null, currentRid, null, null, "更新清单格式不正确");

                Version latestVersion = ParseVersion(manifest.Version);
                UpdatePackage? matchedPackage = FindPackageForCurrentRid(manifest, currentRid);

                return new UpdateCheckResult(
                    latestVersion.CompareTo(currentVersion) > 0,
                    currentVersion,
                    latestVersion,
                    currentRid,
                    manifest,
                    matchedPackage,
                    null);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or ArgumentException)
            {
                return new UpdateCheckResult(false, currentVersion, null, currentRid, null, null, ex.Message);
            }
        }

        public static void OpenDownloadPage(UpdateCheckResult result)
        {
            if (result.Manifest is null)
                return;

            string? url = result.MatchedPackage?.DownloadUrl;
            if (string.IsNullOrWhiteSpace(url))
                url = result.Manifest.DownloadUrl;

            if (string.IsNullOrWhiteSpace(url))
                url = result.Manifest.ReleaseUrl;

            OpenUrl(url);
        }

        public static void OpenReleasePage(UpdateManifest manifest)
        {
            OpenUrl(manifest.ReleaseUrl ?? manifest.DownloadUrl);
        }

        private static UpdatePackage? FindPackageForCurrentRid(UpdateManifest manifest, string currentRid)
        {
            if (manifest.Packages.Count == 0)
                return null;

            UpdatePackage? exact = manifest.Packages
                .FirstOrDefault(package => string.Equals(package.Rid, currentRid, StringComparison.OrdinalIgnoreCase));

            if (exact is not null)
                return exact;

            string osPrefix = currentRid.Split('-')[0];
            return manifest.Packages
                .FirstOrDefault(package => package.Rid.StartsWith(osPrefix + "-", StringComparison.OrdinalIgnoreCase));
        }

        private static void OpenUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return;

            try
            {
                Process.Start(new ProcessStartInfo(url)
                {
                    UseShellExecute = true
                });
            }
            catch
            {
                // URL launch failure should not crash the main application.
            }
        }

        private static string GetUpdateManifestUrl()
        {
            return Assembly.GetExecutingAssembly()
                .GetCustomAttributes<AssemblyMetadataAttribute>()
                .FirstOrDefault(attr => attr.Key == UpdateManifestUrlKey)
                ?.Value ?? string.Empty;
        }

        private static Version GetCurrentVersion()
        {
            string version = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion ?? "0.0.0";

            return ParseVersion(version);
        }

        private static string GetCurrentRid()
        {
            string os = OperatingSystem.IsWindows()
                ? "win"
                : OperatingSystem.IsLinux()
                    ? "linux"
                    : OperatingSystem.IsMacOS()
                        ? "osx"
                        : "unknown";

            string arch = RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64 => "x64",
                Architecture.X86 => "x86",
                Architecture.Arm64 => "arm64",
                Architecture.Arm => "arm",
                _ => RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant()
            };

            return $"{os}-{arch}";
        }

        private static Version ParseVersion(string version)
        {
            string normalized = version.Trim().TrimStart('v', 'V');
            int metadataIndex = normalized.IndexOfAny(new[] { '+', '-' });
            if (metadataIndex >= 0)
                normalized = normalized[..metadataIndex];

            if (!Version.TryParse(normalized, out Version? parsed))
                throw new ArgumentException($"版本号格式不正确：{version}");

            return parsed;
        }
    }
}
