using LibGit2Sharp;
using System.Data;
using System.Globalization;
using System.Net.Http.Json;

namespace SharpGit.Classes
{
    public class GitService
    {
        public static void InitRepo()
        {
            string repoPath = Path.Combine(Directory.GetCurrentDirectory(), "MyRepo");
            Repository.Init(repoPath);
            Console.WriteLine($"Initialized empty Git repository in {repoPath}");
        }

        //
        // Push through
        //
        //No en tiiä
        //Ehkä Refactor tästä initial setup / joku muu funktio kun login
        //koska tää tekee jo aika paljon muuta kun vain login
        public static async Task Login()
        {
            Console.WriteLine("LOGIN");
            Console.WriteLine("Give your username");
            var username = Console.ReadLine();

            Console.WriteLine("Give your password");
            var password = Console.ReadLine();

            Console.WriteLine("Give your email");
            var email = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(username) ||
                    string.IsNullOrWhiteSpace(password) ||
                    string.IsNullOrWhiteSpace(email))
            {
                Console.WriteLine("Invalid input.");
                return;
            }

            try
            {
                var config = GitUtils.GetConfig();
                var loginAddress = config.ServerAddress.TrimEnd('/') + "/cli/login";

                if (!GitUtils.HasSSHKey())
                    GitUtils.SSHKeyGeneration();
                var payload = new
                {
                    username,
                    password,
                    sshkey = GitUtils.GetSSHKey()
                };
                using var client = new HttpClient();
                {
                    var response = await client.PostAsJsonAsync(loginAddress, payload);

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Login failed");
                        return;
                    }
                }
                GitUtils.UpdateLocalConfig(username, email);

                Console.WriteLine("Initial setup successful!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not connect to the server: " + ex.Message);
            }
        }

        public static GitResult AddToRepo(Repository repo, string filePath)
        {
            try
            {
                repo.Index.Add(filePath);
                repo.Index.Write();
                return GitResult.Ok();
            }
            catch (Exception ex)
            {
                return GitResult.Fail($"Failed to add '{filePath}' to repository", ex);
            }
        }

        public static GitResult AddToRepoUpdate(Repository repo)
        {
            try
            {
                var statuses = repo.RetrieveStatus();
                var filtered = statuses
                    .Where(i =>
                        (i.State & (FileStatus.ModifiedInWorkdir | FileStatus.DeletedFromWorkdir
                                    | FileStatus.TypeChangeInWorkdir | FileStatus.RenamedInWorkdir)) != 0);

                foreach (var item in filtered)
                {
                    repo.Index.Add(item.FilePath);
                    repo.Index.Write();
                }
                return GitResult.Ok();
            }
            catch (Exception ex)
            {
                return GitResult.Fail("Failed to update tracked files", ex);
            }
        }

        public static GitResult AddToRepoAll(Repository repo)
        {
            try
            {
                Commands.Stage(repo, "*");
                return GitResult.Ok();
            }
            catch (Exception ex)
            {
                return GitResult.Fail("Failed to add with *", ex);
            }
        }

        public static GitResult RestoreFileInWorkdir(Repository repo, string filePath)
        {
            try
            {
                var entry = repo.Index[filePath];

                if (entry == null)
                {
                    return GitResult.Fail($"File '{filePath}' not found in index.");
                }

                var blob = repo.Lookup<Blob>(entry.Id);

                if (blob == null)
                {
                    return GitResult.Fail($"Blob for '{filePath}' not found.");
                }

                var fullPath = Path.Combine(repo.Info.WorkingDirectory, filePath);

                var directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using var content = blob.GetContentStream();
                using var file = File.Create(fullPath);
                content.CopyTo(file);

                return GitResult.Ok();
            }
            catch (Exception ex)
            {
                return GitResult.Fail($"Failed to restore file: {filePath}", ex);
            }
        }

        public static GitResult RestoreFileStaged(Repository repo, string filePath)
        {
            try
            {
                var commit = repo.Head.Tip;
                if (commit == null)
                    return GitResult.Fail("No HEAD commit found.");

                var treeEntry = commit.Tree[filePath];
                if (treeEntry == null)
                    return GitResult.Fail($"File '{filePath}' not found in HEAD.");

                if (treeEntry.TargetType != TreeEntryTargetType.Blob)
                    return GitResult.Fail($"'{filePath}' is not a file.");

                var blob = (Blob)treeEntry.Target;

                repo.Index.Add(blob, filePath, Mode.NonExecutableFile);
                repo.Index.Write();

                return GitResult.Ok();
            }
            catch (Exception ex)
            {
                return GitResult.Fail($"Failed to restore file: {filePath}", ex);
            }
        }

        public static GitResult RemoveFromRepo(Repository repo, string filePath)
        {
            try
            {
                var status = repo.RetrieveStatus(filePath);
                if (status == FileStatus.NewInWorkdir)
                    return GitResult.Fail("File is not tracked");
                repo.Index.Remove(filePath);
                var realFilePath = Path.Combine(repo.Info.WorkingDirectory, filePath);
                if (File.Exists(realFilePath))
                    File.Delete(realFilePath);
                return GitResult.Ok();
            }
            catch (Exception ex)
            {
                return GitResult.Fail("Failed to remove '{filePath}' from repository", ex);
            }
        }

        public static GitResult CommitToRepo(Repository repo, string message)
        {
            try
            {
                var (name, email) = GitUtils.GetUserFromLocalRepo(repo);

                Signature author = new(name, email, DateTime.Now);
                Signature committer = author;

                Commit commit = repo.Commit(message, author, committer);
                return GitResult.Ok();
            }
            catch (Exception ex)
            {
                return GitResult.Fail("Commit failed", ex);
            }
        }

        public static GitResult CloneRepo(string remotePath, string? givenPath = null)
        {
            var directoryName = remotePath.TrimEnd('/').Split('/').Last();
            var targetDir = givenPath ?? Directory.GetCurrentDirectory();
            var fullDirectory = "";
            bool existedBefore = true;
            try
            {
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }
                if (Directory.EnumerateFileSystemEntries(targetDir).Any())
                {
                    return GitResult.Fail("Given directory was not empty.");
                }
                if (directoryName.EndsWith(".git"))
                {
                    directoryName = directoryName[..^4];
                }
                fullDirectory = Path.Combine(targetDir, directoryName);
                existedBefore = Directory.Exists(fullDirectory);

                Directory.CreateDirectory(fullDirectory);
                Repository.Clone(remotePath, fullDirectory);
                return GitResult.Ok();
            }
            catch (Exception ex)
            {
                if (!existedBefore)
                    Directory.Delete(fullDirectory);
                return GitResult.Fail($"Cloning failed with path :{remotePath}", ex);
            }
        }

        public static GitResult PushToRepo(Repository repo)
        {
            try
            {
                var pushOptions = new PushOptions();

                var branch = repo.Branches["main"] ?? repo.Branches["master"];
                repo.Network.Push(branch, pushOptions);
                return GitResult.Ok();
            }
            catch (Exception ex)
            {
                return GitResult.Fail("Pushing failed", ex);
            }
        }

        public static GitResult PullFromRepo(Repository repo)
        {
            try
            {
                var pullOptions = new PullOptions
                {
                    FetchOptions = new FetchOptions
                    {
                    }
                };
                var (name, email) = GitUtils.GetUserFromLocalRepo(repo);

                var signature = new Signature(
                    new Identity(name, email), DateTimeOffset.Now);

                var result = Commands.Pull(repo, signature, pullOptions);

                // This will never be triggered.
                // The "Commands.Pull()", throws an unhandled exception
                // Unhandled exception: LibGit2Sharp.CheckoutConflictException
                // It is being catched, as otherwise this breaks.
                // But since the exception is thrown, even the "var result" is never populated.
                // So I will never get to show conflicts with this.
                //
                // This does trigger in some cases.
                if (result.Status == MergeStatus.Conflicts)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Merge conflicts detected! Please resolve them manually.");
                    Console.ResetColor();
                    foreach (var conflict in repo.Index.Conflicts)
                    {
                        string path = conflict.Ours?.Path ?? conflict.Theirs?.Path ?? conflict.Ancestor?.Path ?? "(unknown path)";
                        Console.WriteLine($"Conflict in file: {path}");
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Pull successful! No merge conflicts.");
                    Console.ResetColor();
                }
                return GitResult.Ok();
            }
            catch (Exception ex)
            {
                return GitResult.Fail("Pull failed.", ex);
            }
        }

        public static GitResult DisplayGitStatus(Repository repo)
        {
            try
            {
                string branchName = repo.Head.FriendlyName;
                Console.WriteLine($"On branch {branchName}");

                var statusOptions = new StatusOptions();
                var statuses = repo.RetrieveStatus(statusOptions);
                var details = repo.Head.TrackingDetails;
                if (details.BehindBy > 0)
                {
                    Console.WriteLine($"Your branch is behind {branchName} by {details.BehindBy} commit, and can be fast-forwarded. (use sharpgit pull to update your local commits)\n");
                }
                if (details.AheadBy > 0)
                {
                    Console.WriteLine($"Your branch is ahead {branchName} by {details.AheadBy} commits. \n (use sharpgit push to update your local commits)\n");
                }
                if (details.AheadBy == 0 && details.BehindBy == 0)
                {
                    Console.WriteLine("Your branch is up to date.\n");
                }
                var stagedFilesList = new List<StatusEntry>();
                var unstagedFilesList = new List<StatusEntry>();
                var untrackedFilesList = new List<StatusEntry>();

                var stagedFlags = FileStatus.NewInIndex |
                                        FileStatus.ModifiedInIndex |
                                        FileStatus.RenamedInIndex |
                                        FileStatus.DeletedFromIndex;
                foreach (var item in statuses)
                {
                    if ((item.State & stagedFlags) != 0)
                        stagedFilesList.Add(item);
                    if ((item.State & FileStatus.ModifiedInWorkdir) != 0)
                        unstagedFilesList.Add(item);
                    if ((item.State & FileStatus.NewInWorkdir) != 0)
                        untrackedFilesList.Add(item);
                }

                if (stagedFilesList.Any())
                {
                    Console.WriteLine("Changes to be committed:");
                    Console.WriteLine($"    (use \"sharpgit restore --staged <file>...\" to unstage)");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    foreach (var item in stagedFilesList)
                    {
                        Console.WriteLine($"        modified:   {item.FilePath}");
                    }
                    Console.ResetColor();
                }

                if (unstagedFilesList.Any())
                {
                    Console.WriteLine("\nChanges not staged for commit:");
                    Console.WriteLine($"  (use \"sharpgit add <file>...\" to update what will be committed)");
                    Console.WriteLine($"  (use \"sharpgit restore <file>...\" to discard changes in working directory)");
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    foreach (var item in unstagedFilesList)
                    {
                        Console.WriteLine($"        modified:   {item.FilePath}");
                    }
                    Console.ResetColor();
                    Console.WriteLine();
                }

                if (untrackedFilesList.Any())
                {
                    Console.WriteLine("Untracked files:");
                    Console.WriteLine($"  (use \"sharpgit add <file>...\" to include in what will be committed)");
                    Console.ForegroundColor = ConsoleColor.Red;
                    foreach (var item in untrackedFilesList)
                    {
                        Console.WriteLine($"        {item.FilePath}");
                    }
                    Console.ResetColor();
                    Console.WriteLine();
                }
                if (!stagedFilesList.Any() && !unstagedFilesList.Any() && untrackedFilesList.Any())
                {
                    Console.WriteLine("nothing to commit, working tree clean");
                }
                return GitResult.Ok();
            }
            catch (Exception ex)
            {
                return GitResult.Fail("Error retrieving status", ex);
            }
        }

        public static GitResult DisplayLog(Repository repo, int length)
        {
            try
            {
                var RFC2822Format = "ddd dd MMM HH:mm:ss yyyy K";

                foreach (Commit c in repo.Commits.Take(length))
                {
                    Console.WriteLine(string.Format("commit {0}", c.Id));

                    if (c.Parents.Count() > 1)
                    {
                        Console.WriteLine("Merge: {0}",
                            string.Join(" ", c.Parents.Select(p => p.Id.Sha.Substring(0, 7)).ToArray()));
                    }

                    Console.WriteLine(string.Format("Author: {0} <{1}>", c.Author.Name, c.Author.Email));
                    Console.WriteLine("Date:   {0}", c.Author.When.ToString(RFC2822Format, CultureInfo.InvariantCulture));
                    Console.WriteLine();
                    Console.WriteLine(c.Message);
                    Console.WriteLine();
                }
                return GitResult.Ok();
            }
            catch (Exception ex)
            {
                return GitResult.Fail("Error retrieving log", ex);
            }
        }
    }
}
