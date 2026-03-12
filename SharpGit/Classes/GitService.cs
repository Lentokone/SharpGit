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
                Commands.Stage(repo, "*");
                return GitResult.Ok();
            }
            catch (Exception ex)
            {
                return GitResult.Fail("Failed to add with *Failed to add with *", ex);
            }
        }

        public static GitResult RemoveFromRepo(Repository repo, string filePath)
        {
            try
            {
                repo.Index.Remove(filePath);
                return GitResult.Ok();
            }
            catch (Exception ex)
            {
                return GitResult.Fail("Failed to remove '{filePath}' from repository", ex);
            }
        }

        // TODO TÄMÄ ON VIELÄ KESKEN
        // Puuttuu oikea signature ja muut
        // Ne pitää hakea jonkin sortin config.json tiedostosta
        public static GitResult CommitToRepo(Repository repo, string message)
        {
            try
            {
                // Create the committer's signature and commit
                var name = repo.Config.Get<string>("user.name")?.Value;
                var email = repo.Config.Get<string>("user.email")?.Value;

                // Have this grab the author name from the .sharpgit local directory instead of Git's global config.
                // Will probably save some mental pain from doing it with that instead.
                Console.WriteLine();
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

        public static GitResult CloneRepo(string remotePath, string? givenPath)
        {
            try
            {
                var targetDir = givenPath ?? Directory.GetCurrentDirectory();
                var directoryName = remotePath.TrimEnd('/').Split('/').Last();

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
        public static void PullFromRepo(Repository repo)
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
                if (repo == null)
                {
                    Console.WriteLine("No repository found in the current directory.");
                    return;
                }

                var signature = new LibGit2Sharp.Signature(
                    new Identity("MERGE_USER_NAME", "MERGE_USER_EMAIL"), DateTimeOffset.Now);

                var result = Commands.Pull(repo, signature, pullOptions);

                // This will never be triggered.
                // The "Commands.Pull()", throws an unhandled exception
                // Unhandled exception: LibGit2Sharp.CheckoutConflictException
                // It is being catched, as otherwise this breaks.
                // But since the exception is thrown, even the "var result" is never populated.
                // So I will never get to show conflicts with this.
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
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Pull failed: {ex.Message}");
                Console.ResetColor();
            }
        }

        // TODO VÄRIT PUUTTUU
        public static void DisplayGitStatus(Repository repo)
        {
            try
            {
                string branchName = repo.Head.FriendlyName;
                Console.WriteLine($"On branch {branchName}");

                // Separate modified and untracked files
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
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving status: " + ex.Message);
            }
        }

        public static void DisplayLog(Repository repo, int length)
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
            }
            catch
            (Exception ex)
            {
                Console.WriteLine("Error retrieving log: " + ex.Message);
            }
        }
        //! This is for testing only.
        public static void SetRemoteRepo()
        {
            // Local repo path (not bare)
            var repo = GitUtils.TryFindRepositoryFromCurrentDirectory();

            //welho@192.168.1.114:/home/welho/sUPERTEST/SharGitRepo
            //Tuossa on oman palvelimen osoite, joka sitten kulkee ssh yhteydellä.
            // Remote repo path (the bare repo)
            string remotePath = "file:///C:/FULLSTACK_WEBDEV_FSWDAP/heha.git";

            if (repo == null)
            {
                Console.WriteLine("No repository found in the current directory.");
                return;
            }
            // Add remote if it doesn't exist yet
            if (repo.Network.Remotes["origin"] == null)
            {
                repo.Network.Remotes.Add("origin", remotePath);
                Console.WriteLine("Testermaan");
            }
        }
    }
}
