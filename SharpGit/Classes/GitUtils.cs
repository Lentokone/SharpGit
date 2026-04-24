using LibGit2Sharp;
using System.Diagnostics;
using System.Text.Json;

namespace SharpGit.Classes
{
    public class GitUtils
    {
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

        public static bool HasSSHKeygen()
        {
            try
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var sharpgitDir = Path.Combine(path, ".sharpgit");
                var sshKeyDir = Path.Combine(sharpgitDir, "ssh");
                if (!Directory.Exists(sshKeyDir))
                    Directory.CreateDirectory(sshKeyDir);

                var sshKeyName = Path.Combine(sshKeyDir, "SharpHub_key");
                Console.WriteLine(sshKeyName);
                var psi = new ProcessStartInfo
                {
                    FileName = "ssh-keygen",
                    Arguments = $"-f {sshKeyName} -N \"\" ",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };
                var process = Process.Start(psi);
                if (process != null)
                    process.WaitForExit();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string GetSSHKey(string filePath)
        {
            return File.ReadAllText(filePath).Trim();
        }
    }
}
