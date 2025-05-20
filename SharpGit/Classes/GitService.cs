using LibGit2Sharp;
using System;
using System.Collections.Generic;
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

        public static void PushToRepo()
        {
            var repo = GitUtils.TryFindRepositoryFromCurrentDirectory();

            
            // Set up push options (no credentials needed for local)
            var pushOptions = new PushOptions();

            if (repo == null)
            {
                Console.WriteLine("No repository found in the current directory.");
                return;
            }
            // Push current branch to origin
            var branch = repo.Branches["main"] ?? repo.Branches["master"];
            repo.Network.Push(branch, pushOptions);
        }

        public static void PullFromRepo()
        {

        }

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

                if (itemStatusesList.Any(s => s.Contains("NewInIndex") || s.Contains("ModifiedInIndex") || s.Contains("RenamedInIndex") || s.Contains("DeletedFromIndex")))
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
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving status: " + ex.Message);
            }
        }
    }
}
