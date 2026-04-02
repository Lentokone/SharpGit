using LibGit2Sharp;
using System.Text.Json;

namespace SharpGit.Classes
{
    public class GitUtils
    {
        // Tänne viellä ne gitignore template ja muut.
        // Funktio joka sitten tekee sen annetulla numerolla (esim. 1 = vstudio, 2 = node, 3 = python jne)

        /// <summary>
        /// 
        /// 
        /// </summary>
        public static Repository? TryFindRepositoryFromCurrentDirectory()
        {
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());

            while (dir != null)
            {
                var gitPath = Path.Combine(dir.FullName, ".git");
                if (Directory.Exists(gitPath) || File.Exists(gitPath)) // file in case it's a submodule or worktree
                {
                    return new Repository(dir.FullName);
                }
                dir = dir.Parent;
            }
            return null;
        }

        private static SharpGitConfig CreateDefaultConfig()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var sharpgitDir = Path.Combine(path, ".sharpgit");
            var configPath = Path.Combine(sharpgitDir, "config.json");
            if (!Directory.Exists(sharpgitDir))
                Directory.CreateDirectory(sharpgitDir);

            var config = new SharpGitConfig
            {
                User = new UserConfig
                {
                    Name = "Unset",
                    Email = "Unset"
                },
                Server = new ServerConfig
                {
                },
            };
            var json = JsonSerializer.Serialize(
                config,
                new JsonSerializerOptions { WriteIndented = true }
            );
            File.WriteAllText(configPath, json);
            return config;
        }

        public static SharpGitConfig GetConfig()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var sharpgitDir = Path.Combine(path, ".sharpgit");
            var configPath = Path.Combine(sharpgitDir, "config.json");

            if (!File.Exists(configPath))
            {
                return CreateDefaultConfig();
            }
            else
            {
                var json = File.ReadAllText(configPath);
                return JsonSerializer.Deserialize<SharpGitConfig>(json) ?? new SharpGitConfig();
            }
        }
    }
}
