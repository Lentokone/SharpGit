using LibGit2Sharp;
using System.Text.Json;

namespace SharpGit.Classes
{
    internal class GitUtils
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

        // Unfinished.
        // Split the Directory.CreateDirectory into its own function.
        // !  Make this function return the username from the config.
        public static string GetUsernameFromConfig()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var sharpgitDir = Path.Combine(path, ".sharpgit");
            var configPath = Path.Combine(sharpgitDir, "config.json");

            Directory.CreateDirectory(sharpgitDir);
            if (!File.Exists(configPath))
            {
                var defaultConfig = new SharpGitConfig
                {
                    User = new UserConfig
                    {
                        Name = "unknown",
                        Email = "unknown"
                    },
                    Server = new ServerConfig
                    {
                        BaseUrl = "not implemented yet"
                    }
                };

                var json = JsonSerializer.Serialize(
                    defaultConfig,
                    new JsonSerializerOptions { WriteIndented = true }
                );

                File.WriteAllText(configPath, json);

                Console.WriteLine("config.json creation.");
            }
            else
            {
                Console.WriteLine("4skin");

            }
            return path;
        }
    }
}
