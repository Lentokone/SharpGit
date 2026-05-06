using LibGit2Sharp;
using System.Diagnostics;
using System.Text.Json;

namespace SharpGit.Classes
{
    public class GitUtils
    {
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
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".sharpgit");
            var configPath = Path.Combine(path, "config.json");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var config = new SharpGitConfig();
            var json = JsonSerializer.Serialize(
                config,
                new JsonSerializerOptions { WriteIndented = true }
            );
            File.WriteAllText(configPath, json);
            return config;
        }

        public static SharpGitConfig GetConfig()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".sharpgit");
            var configPath = Path.Combine(path, "config.json");

            if (!File.Exists(configPath))
                return CreateDefaultConfig();
            try
            {
                var json = File.ReadAllText(configPath);
                return JsonSerializer.Deserialize<SharpGitConfig>(json) ?? CreateDefaultConfig();
            }
            catch
            {
                return CreateDefaultConfig();
            }
        }

        public static void WriteToConfig(SharpGitConfig config)
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".sharpgit");
            var configPath = Path.Combine(path, "config.json");

            Directory.CreateDirectory(path);

            var json = JsonSerializer.Serialize(
                config,
                new JsonSerializerOptions { WriteIndented = true }
            );
            File.WriteAllText(configPath, json);
        }

        public static void UpdateLocalConfig(string? username = null, string? email = null, string? serverAddress = null, string? sshKeyPath = null)
        {
            var config = GetConfig();
            if (username != null)
                config.Username = username;
            if (email != null)
                config.Email = email;
            if (serverAddress != null)
                config.ServerAddress = serverAddress;
            if (sshKeyPath != null)
                config.SSHKeyPath = sshKeyPath;

            WriteToConfig(config);
        }

        // Add functionality here to:::
        //TODO: Write the ssh key path into the config.json/SSHKeyPath
        //
        // Rename and refactor this
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

        public static string GetSSHKey()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var sharpgitDir = Path.Combine(path, ".sharpgit");
            var sshKeyDir = Path.Combine(sharpgitDir, "ssh");

            var sshKeyName = Path.Combine(sshKeyDir, "SharpHub_key.pub");
            return File.ReadAllText(sshKeyName).Trim();
        }

        public static (string Username, string Email) GetUserFromLocalRepo(Repository repo)
        {
            return (
                    repo.Config.Get<string>("user.name").Value,
                    repo.Config.Get<string>("user.email").Value
                    );
        }

        public static void WriteUserToLocalRepo(Repository repo, string Username, string Email)
        {
            repo.Config.Set("user.name", Username, ConfigurationLevel.Local);
            repo.Config.Set("user.email", Email, ConfigurationLevel.Local);
        }

        // <summary>
        // This function will check if the user is logged in, has a valid JWT token, has an SSH key.
        //
        //
        //Unfinished
        //
        // </summary>
        public static bool UserIsValid()
        {
            return false;
        }
    }
}
