using System;
using System.IO;
using System.Text.Json;

namespace GameLogin
{
    public sealed class AppConfig
    {
        public static AppConfig Current { get; private set; } = new AppConfig();

        public static string FilePath =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mir3dConfig.json");

        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            PropertyNameCaseInsensitive = true
        };

        public string AccountServerAddressIP { get; set; } = "127.0.0.1";
        public ushort AccountServerAddressPort { get; set; } = 8000;

        public string AccountName { get; set; } = "";
        public string ServerName { get; set; } = "";

        public static void Load()
        {
            try
            {
                if (!File.Exists(FilePath)) { Save(); return; }
                var json = File.ReadAllText(FilePath);
                Current = JsonSerializer.Deserialize<AppConfig>(json, Options) ?? new AppConfig();
            }
            catch
            {
                // Backup bad file and write defaults
                try { File.Copy(FilePath, FilePath + ".bad", overwrite: true); } catch { }
                Current = new AppConfig();
                Save();
            }
        }

        public static void Save()
        {
            var json = JsonSerializer.Serialize(Current, Options);
            File.WriteAllText(FilePath, json);
        }
    }
}
