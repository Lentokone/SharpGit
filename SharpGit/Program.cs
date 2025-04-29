using LibGit2Sharp;
using System;
using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.Invocation;

////! Eli se libgit2sharp
////! Ja ssh
////! Ja auth tarkistus mvc puolelle

//// Kun on pushaamassa, lähettää ennen sitä jonkun viestin mvc puolelle, joka tekee sen git init --bare puolen

//// Vaikka git remote add kohdassa voisi tehdä sen auth jutun ja tag se local repo jotenkin että se tietää että on auth kontsa


namespace SharpGit;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("SharpGit CLI - a minimal git-like tool");

        var updateOption = new Option<bool>(
            name: "--update",
            description: "Update the local repository"
        );
        updateOption.AddAlias("-u"); // Short form

        // init
        var initCommand = new Command("init", "Initialize a new repository");
        initCommand.SetHandler(() => {
            Console.WriteLine("Init command called");
        });

        // add
        var addCommand = new Command("add", "Add files to staging area");
        var addPathArg = new Argument<IEnumerable<string>>("path", "Path to file or directory");

        addCommand.AddOption(updateOption);
        addCommand.AddArgument(addPathArg);

        addCommand.SetHandler((bool update, IEnumerable<string> paths) =>
        {
            // Handle the update option
            if (update)
            {
                Console.WriteLine("Using '--update' or '-u' to stage modified and deleted files.");
            }

            // Handle paths (files or directories)
            foreach (var path in paths)
            {
                Console.WriteLine($"Adding file or directory: {path}");
            }
        }, updateOption, addPathArg);

        // commit
        var commitCommand = new Command("commit", "Record changes to the repository");
        var messageOption = new Option<string>(
            name: "--message",
            description: "Commit message"
        );
        commitCommand.AddOption(messageOption);
        commitCommand.SetHandler((string message) => {
            Console.WriteLine($"Commit command called with message: {message}");
        }, messageOption);

        // clone
        var cloneCommand = new Command("clone", "Clone a repository");
        var repoUrlArg = new Argument<string>("url", "Repository URL");
        cloneCommand.AddArgument(repoUrlArg);
        cloneCommand.SetHandler((string url) => {
            Console.WriteLine($"Clone command called for: {url}");
        }, repoUrlArg);

        // push
        var pushCommand = new Command("push", "Push changes to remote");
        pushCommand.SetHandler(() => {
            Console.WriteLine("Push command called");
        });

        // pull
        var pullCommand = new Command("pull", "Pull changes from remote");
        pullCommand.SetHandler(() => {
            Console.WriteLine("Pull command called");
        });

        // status
        var statusCommand = new Command("status", "Show the working tree status");
        statusCommand.SetHandler(() => {
            Console.WriteLine("Status command called");
            string test = Directory.GetCurrentDirectory();
            var repo = GitUtils.TryFindRepositoryFromCurrentDirectory();
            var status = repo.RetrieveStatus();

            DisplayGitStatus(repo); // Display the status of the repository

        });
        // status
        var logCommand = new Command("log", "Show the commit tree");
        logCommand.SetHandler(() => {
            Console.WriteLine("Log command called");
        });

        // Add all to root
        rootCommand.AddCommand(initCommand);
        rootCommand.AddCommand(addCommand);
        rootCommand.AddCommand(commitCommand);
        rootCommand.AddCommand(cloneCommand);
        rootCommand.AddCommand(pushCommand);
        rootCommand.AddCommand(pullCommand);
        rootCommand.AddCommand(statusCommand);
        rootCommand.AddCommand(logCommand);

        return await rootCommand.InvokeAsync(args);
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

            // Changes to be committed (staged files)
            Console.WriteLine("\nChanges to be committed:");
            foreach (var item in statuses)
            {
                if ((item.State & (FileStatus.NewInIndex | FileStatus.ModifiedInIndex | FileStatus.RenamedInIndex | FileStatus.DeletedFromIndex)) != 0)
                {
                    Console.WriteLine($"        {item.FilePath}");
                    hasStagedChanges = true;
                }
            }

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

            // Untracked files
            Console.WriteLine("\nUntracked files:");
            foreach (var item in statuses)
            {
                if ((item.State & FileStatus.NewInWorkdir) != 0)
                {
                    Console.WriteLine($"  (use \"git add <file>...\" to include in what will be committed)");
                    Console.WriteLine($"        {item.FilePath}/");
                    hasChanges = true;
                }
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

public static class GitUtils
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
}