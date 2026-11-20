using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;

namespace EnergyPortal
{
    /// <summary>
    /// Resolves Vite entries (e.g. "app.ts") to their hashed build output via wwwroot/assets.manifest.json.
    /// Hashed names keep browsers from mixing old and new modules after a deploy.
    /// </summary>
    public class ViteManifest(IWebHostEnvironment environment)
    {
        private record Entry(string File);

        private Dictionary<string, Entry> entries = new();
        private DateTime loadedWriteTime;

        /// <summary>Path below wwwroot, e.g. "js/app-1a2b3c.js". Falls back to "js/{name}.js" without a build.</summary>
        public string File(string entry)
        {
            var path = Path.Combine(environment.WebRootPath ?? "", "assets.manifest.json");
            var writeTime = System.IO.File.Exists(path) ? System.IO.File.GetLastWriteTimeUtc(path) : default;
            // Re-read after a rebuild
            if (writeTime != loadedWriteTime)
            {
                entries = writeTime == default
                    ? new Dictionary<string, Entry>()
                    : JsonSerializer.Deserialize<Dictionary<string, Entry>>(System.IO.File.ReadAllText(path),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                loadedWriteTime = writeTime;
            }

            return entries.TryGetValue(entry, out var e) ? e.File : $"js/{Path.GetFileNameWithoutExtension(entry)}.js";
        }
    }
}
