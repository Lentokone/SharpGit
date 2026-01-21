using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGit.Classes
{
    internal class GitService
    {
        public static void InitRepo()
        {
            // Initialize a new repository
            string repoPath = Path.Combine(Directory.GetCurrentDirectory(), "MyRepo");
            Repository.Init(repoPath);
            Console.WriteLine($"Initialized empty Git repository in {repoPath}");
        }

        // Tämä hyväksymään sen argumentin
        // Ja pilkkoo sen urlin. Esimerkillä tämän projektin repo "https://github.com/Lentokone/FSWADP_Console_side"
        // Muuttaa sen kunnolliseen remotePath muotoon.
        // "kayttis@server:/home/kayttis/shubrepos/Lentokone/FSWADP..."
        public static void AddToRepo(Repository repo, string filePath)
        {
            try
            {
                repo.Index.Add(filePath);
                repo.Index.Write();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Add failed: {ex.Message}");
                Console.ResetColor();
            }
        }

        public static void AddToRepoUpdate(Repository repo)
        {
            try
            {
                Commands.Stage(repo, "*");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Add failed: {ex.Message}");
                Console.ResetColor();
            }
        }

        // TODO TÄMÄ ON VIELÄ KESKEN
        // Puuttuu oikea signature ja muut
        // Ne pitää hakea jonkin sortin config.json tiedostosta
        public static void CommitToRepo(Repository repo, string message)
        {
            try
            {
                // Write content to file system
                var content = "Commit this!";
                File.WriteAllText(Path.Combine(repo.Info.WorkingDirectory, "fileToCommit.txt"), content);

                // Stage the file
                repo.Index.Add("fileToCommit.txt");
                repo.Index.Write();

                // Create the committer's signature and commit
                Signature author = new Signature("James", "@jugglingnutcase", DateTime.Now);
                Signature committer = author;

                // Commit to the repository
                Commit commit = repo.Commit("Here's a commit i made!", author, committer);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Commit failed: {ex.Message}");
                Console.ResetColor();
            }
        }

        public static void CloneRepo(string remotePath, string? givenPath)
        {
            try
            {
                // Tähän se että se tekee directory sille repository
                // Jepjep. Nyt on aika testata throttleeko tämäkin paska.
                var targetDir = givenPath ?? Directory.GetCurrentDirectory();
                var directoryName = remotePath.TrimEnd('/').Split('/').Last();

                if (directoryName.EndsWith(".git"))
                {
                    directoryName = directoryName[..^4];
                }

                Console.WriteLine(directoryName);
                var fullDirectory = Path.Combine(targetDir, directoryName);
                Directory.CreateDirectory(fullDirectory);
                Repository.Clone(remotePath, fullDirectory);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Clone failed: {ex.Message}");
                Console.ResetColor();
            }
        }

        public static void PushToRepo(Repository repo)
        {
            try
            {
                var pushOptions = new PushOptions();

                if (repo == null)
                {
                    Console.WriteLine("No repository found in the current directory.");
                    return;
                }
                var branch = repo.Branches["main"] ?? repo.Branches["master"];
                repo.Network.Push(branch, pushOptions);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Push failed: {ex.Message}");
                Console.ResetColor();
            }
        }

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
                // Pull from the remote repository
                var result = Commands.Pull(repo, signature, pullOptions);

                if (result.Status == MergeStatus.Conflicts)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Merge conflicts detected! Please resolve them manually.");
                    Console.ResetColor();

                    Console.WriteLine(repo.Index.Conflicts.Any());
                    // Optionally: List conflicted files-
                    foreach (var conflict in repo.Index.Conflicts)
                    {
                        Console.WriteLine("Ttest");
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
                // Display the branch information
                string branchName = repo.Head.FriendlyName;
                Console.WriteLine($"On branch {branchName}");

                // Separate modified and untracked files
                var statusOptions = new StatusOptions();
                var statuses = repo.RetrieveStatus(statusOptions);
                var details = repo.Head.TrackingDetails;
                Console.WriteLine(details.BehindBy);
                Console.WriteLine(details.AheadBy);
                Console.WriteLine($"Testing {details.AheadBy}");
                if (details.BehindBy < 0)
                {
                    Console.WriteLine($"Your branch is behind {branchName} by {details.BehindBy} commit, and can be fast-forwarded. (use sharpgit pull to update your local branch)");
                }
                if (details.AheadBy < 0)
                {
                    Console.WriteLine($"Your branch is ahead {branchName} by {details.BehindBy} commit, and can be fast-forwarded.\n ŋµŋµ(use sharpgit pull to update your local branch)");
                }
                Console.WriteLine($"Your branch is behind {branchName} by {details.BehindBy} commit, and can be fast-forwarded. (use sharpgit pull to update your local branch)");
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

                if (itemStatusesList.Any(s => s.Contains("NewInIndex") || s.Contains("ModifiedInIndex") || s.Contains("ModifiedInWorkdir") || s.Contains("RenamedInIndex") || s.Contains("DeletedFromIndex")))
                {
                    // Changes not staged for commit
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
                    // Untracked files
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