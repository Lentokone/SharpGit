using LibGit2Sharp;
using System.Data;
using System.Globalization;

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

        // Tämä hyväksymään sen argumentin
        // Ja pilkkoo sen urlin. Esimerkillä tämän projektin repo "https://github.com/Lentokone/FSWADP_Console_side"
        // Muuttaa sen kunnolliseen remotePath muotoon.
        // "kayttis@server:/home/kayttis/shubrepos/Lentokone/FSWADP..."
        //
        // 25/02/2026
        // Noin tehdään koska tämä Git välinen liikenne toimii ssh kautta
        //
        //Moro. 12/03/2026 Olli tässä
        //Ei mitään tietoa mitä tuo ylempi tarkoitti

        // Terve. 1/04/2026 Olli tässä
        // Ei vieläkään mitään ymmärrystä tosta

        //TODO: Jep
        //
        //TODO ee
        // Push through
        public static void Login()
        {
            // Ask credentials
            Console.WriteLine("LOGIN");
            Console.WriteLine("Give your username");
            var username = Console.ReadLine();

            Console.WriteLine("Give your password");
            var password = Console.ReadLine();
            // Generate SSH key for server
            // Send credentials and public ssh key, with HTTP to server's endpoint for validation
            // Get JWT token and username and email in return
            // Store username and email in .sharpgit/config.json
            // Store JWT token there too, with token expiration time 

            //Lastly, wrap every single function to be checking for the JWT token, if not, then prompting login credentials
            //Login could just be for cloning, push/pull, and so.
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
                return GitResult.Fail("Failed to add '{filePath}' to repository", ex);
            }
        }

        public static GitResult AddToRepoUpdate(Repository repo)
        {
            try
            {
                var statuses = repo.RetrieveStatus();
                var filtered = statuses
                    .Where(i =>
                        (i.State & (FileStatus.ModifiedInWorkdir | FileStatus.DeletedFromWorkdir | FileStatus.TypeChangeInWorkdir | FileStatus.RenamedInWorkdir)) != 0);

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
                var config = GitUtils.GetConfig();
                var name = config.User.Name;
                var email = config.User.Email;

                Signature author = new Signature(name, email, DateTime.Now);
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
            try
            {
                var targetDir = givenPath ?? Directory.GetCurrentDirectory();
                var directoryName = remotePath.TrimEnd('/').Split('/').Last();
                if (!Directory.Exists(targetDir))
                    Directory.CreateDirectory(targetDir);
                if (Directory.EnumerateFileSystemEntries(targetDir).Any())
                {
                    return GitResult.Fail("Given directory was not empty.");
                }
                if (directoryName.EndsWith(".git"))
                {
                    directoryName = directoryName[..^4];
                }

                var fullDirectory = Path.Combine(targetDir, directoryName);
                Directory.CreateDirectory(fullDirectory);
                Repository.Clone(remotePath, fullDirectory);
                return GitResult.Ok();
            }
            catch (Exception ex)
            {
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

        //TODO: Refactor this
        // Ei ole testattu
        // 19/01/ 22:25, testattu, toimii
        public static GitResult PullFromRepo(Repository repo)
        {
            try
            {
                var pullOptions = new PullOptions
                {
                    FetchOptions = new FetchOptions
                    {
                        // Muista lisätä tänne ssh avain hommat ja muut
                    }
                };
                var signature = new LibGit2Sharp.Signature(
                    new Identity("MERGE_USER_NAME", "MERGE_USER_EMAIL"), DateTimeOffset.Now);

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
                    // Optionally: List conflicted files-
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

        // TODO VÄRIT PUUTTUU
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
                var itemStatusesList = new List<string>();

                foreach (var item in statuses)
                {
                    itemStatusesList.Add(item.State.ToString());
                }

                // Changes to be committed (staged files)
                if (itemStatusesList.Any(s => s.Contains("NewInIndex") || s.Contains("ModifiedInIndex") || s.Contains("RenamedInIndex") || s.Contains("DeletedFromIndex")))
                {
                    Console.WriteLine("\nChanges to be committed:");

                    foreach (var item in statuses)
                    {
                        if ((item.State & (FileStatus.NewInIndex | FileStatus.ModifiedInIndex | FileStatus.RenamedInIndex | FileStatus.DeletedFromIndex)) != 0)
                        {
                            Console.WriteLine($"        {item.FilePath}");
                        }
                    }
                }

                if (itemStatusesList.Any(s => s.Contains("NewInIndex") || s.Contains("ModifiedInWorkdir") || s.Contains("RenamedInIndex") || s.Contains("DeletedFromIndex")))
                {
                    Console.WriteLine("\nChanges not staged for commit:");
                    Console.WriteLine($"  (use \"git add <file>...\" to update what will be committed)");
                    Console.WriteLine($"  (use \"git restore <file>...\" to discard changes in working directory)");
                    foreach (var item in statuses)
                    {
                        if ((item.State & FileStatus.ModifiedInWorkdir) != 0)
                        {
                            Console.WriteLine($"        modified:   {item.FilePath}");
                        }
                    }
                    Console.WriteLine();
                    Console.WriteLine("no changes added to commit (use \"git add\" and/or \"git commit -a\")");
                }

                if (itemStatusesList.Any(s => s.Contains("NewInWorkdir")))
                {
                    Console.WriteLine("Untracked files:");
                    Console.WriteLine($"  (use \"git add <file>...\" to include in what will be committed)");
                    foreach (var item in statuses)
                    {
                        if ((item.State & FileStatus.NewInWorkdir) != 0)
                        {
                            Console.WriteLine($"        {item.FilePath}");
                        }
                    }
                    Console.WriteLine();
                }
                if (!itemStatusesList.Any(s =>
                            s.Contains("NewInIndex") ||
                            s.Contains("ModifiedInIndex") ||
                            s.Contains("RenamedInIndex") ||
                            s.Contains("DeletedFromIndex") ||
                            s.Contains("ModifiedInWorkdir") ||
                            s.Contains("NewInWorkdir")))
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
            catch
            (Exception ex)
            {
                return GitResult.Fail("Error retrieving log", ex);
            }
        }
    }
}
