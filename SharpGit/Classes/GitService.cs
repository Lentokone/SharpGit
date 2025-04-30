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

                bool hasChanges = false;
                bool hasStagedChanges = false;

                var itemStatusesList = new List<string>();

                foreach (var item in statuses)
                {
                    itemStatusesList.Add(item.State.ToString());
                }
                Console.WriteLine(string.Join(",", itemStatusesList));

                // Changes to be committed (staged files)
                
                
                if (itemStatusesList.Any(s => s.Contains("NewInIndex") || s.Contains("ModifiedInIndex") || s.Contains("RenamedInIndex") || s.Contains("DeletedFromIndex")))
                {
                    Console.WriteLine("\nChanges to be committed:");
                    
                    foreach (var item in statuses)
                    {
                        if ((item.State & (FileStatus.NewInIndex | FileStatus.ModifiedInIndex | FileStatus.RenamedInIndex | FileStatus.DeletedFromIndex)) != 0)
                        {
                            Console.WriteLine($"        {item.FilePath}");
                            hasStagedChanges = true;
                        }
                    }
                }
                if (itemStatusesList.Any(s => s.Contains("")))
                {
                    // Changes not staged for commit
                    Console.WriteLine("\nChanges not staged for commit:");
                    foreach (var item in statuses)
                    {
                        if ((item.State & FileStatus.ModifiedInWorkdir) != 0)
                        {
                            Console.WriteLine($"  (use \"git add <file>...\" to update what will be committed)");
                            Console.WriteLine($"  (use \"git restore <file>...\" to discard changes in working directory)");
                            Console.WriteLine($"        modified:   {item.FilePath}");
                            hasChanges = true;
                        }
                    }
                }

                // Untracked files
                Console.WriteLine("\nUntracked files:");
                if (itemStatusesList.Any(s => s.Contains("NewInWorkdir")))
                {
                    Console.WriteLine($"  (use \"git add <file>...\" to include in what will be committed)");
                    foreach (var item in statuses)
                    {
                        if ((item.State & FileStatus.NewInWorkdir) != 0)
                        {
                            Console.WriteLine($"        {item.FilePath}/");
                            hasChanges = true;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("monkey");
                }


                // If no changes, indicate no changes to commit
                if (!hasChanges && !hasStagedChanges)
                {
                    Console.WriteLine("\nno changes added to commit (use \"git add\" and/or \"git commit -a\")");
                }
                else if (!hasChanges && hasStagedChanges)
                {
                    Console.WriteLine("\nChanges are staged for commit.");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving status: " + ex.Message);
            }
        }
    }
}
