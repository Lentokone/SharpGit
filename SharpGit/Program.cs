using System;
using System.Threading.Tasks;
using System.CommandLine;
using SharpGit.Classes;

////! Eli se libgit2sharp
////! Ja ssh
////! Ja auth tarkistus mvc puolelle

//TODO Muista ottaa nuo writelinet pois

namespace SharpGit;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("SharpGit CLI - a minimal git-like tool");

        // Refactor name and something to be more reasonable
        var loginCommand = new Command("login", "name is temporary for now!!!!");
        loginCommand.SetHandler(async () =>
        {
            await GitService.Login();
        });

        // init
        var initCommand = new Command("init", "Initialize a new repository. Unsupported for now.");
        initCommand.SetHandler(() =>
        {
            Console.WriteLine("Init command called");
            Console.WriteLine("This is still unsupported.");
            // GitService.InitRepo();
        });

        // add
        var addCommand = new Command("add", "Add files to staging area");
        var updateOption = new Option<bool>(
            name: "--update",
            description: "Update the local repository"
        );
        updateOption.AddAlias("-u");

        var allOption = new Option<bool>(
                name: "--all",
                description: "Add every file to be tracked"
                );
        allOption.AddAlias("-a");
        var addPathArg = new Argument<IEnumerable<string>>("path", "Path to file or directory");

        addCommand.AddOption(updateOption);
        addCommand.AddOption(allOption);
        addCommand.AddArgument(addPathArg);

        addCommand.SetHandler((bool update, bool all, IEnumerable<string> paths) =>
        {
            var repo = GitUtils.TryFindRepositoryFromCurrentDirectory();
            var result = new GitResult();
            if (repo == null)
            {
                Environment.Exit(1);
            }
            if (update)
            {
                Console.WriteLine("Using '--update' or '-u' to stage changes and deletions");
                result = GitService.AddToRepoUpdate(repo);
            }
            if (all)
            {
                Console.WriteLine("Using '--all' to add every file to be tracked");
                result = GitService.AddToRepoAll(repo);
            }
            foreach (var path in paths)
            {
                Console.WriteLine($"Adding file or directory: {path}");
                result = GitService.AddToRepo(repo, path);
            }
            if (!result.Success)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{result.Message}");
                Console.ResetColor();
                Environment.Exit(1);
            }
        }, updateOption, allOption, addPathArg);

        // remove
        var removeCommand = new Command("remove", "Remove tracked object");
        var removePathArg = new Argument<IEnumerable<string>>("path", "Path to file or directory");

        removeCommand.AddArgument(addPathArg);
        removeCommand.SetHandler((IEnumerable<string> path) =>
        {
            var repo = GitUtils.TryFindRepositoryFromCurrentDirectory();
            if (repo == null)
            {
                Environment.Exit(1);
            }

        }, removePathArg);


        // commit
        var commitCommand = new Command("commit", "Record changes to the repository");
        var messageOption = new Option<string>(
            name: "--message",
            description: "Commit message"
        );
        messageOption.AddAlias("-m");
        commitCommand.AddOption(messageOption);
        commitCommand.SetHandler((string message) =>
        {
            Console.WriteLine($"Commit command called with message: {message}");

            var repo = GitUtils.TryFindRepositoryFromCurrentDirectory();
            if (repo == null)
            {
                Console.WriteLine("No repository found in the current directory.");
                Environment.Exit(1);
            }
            Console.WriteLine($"{message}");
            var result = GitService.CommitToRepo(repo, message);
            if (!result.Success)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result.Message);
                Console.ResetColor();
                Environment.Exit(1);
            }
        }, messageOption);

        // clone
        // KESKEN
        var cloneCommand = new Command("clone", "Clone a repository");
        var repoUrlArg = new Argument<string>("url", "Repository URL");
        var targetDirArg = new Argument<string?>("path", () => null, "Target directory (optional)");
        cloneCommand.AddArgument(repoUrlArg);
        cloneCommand.AddArgument(targetDirArg);
        cloneCommand.SetHandler((string url, string? path) =>
        {
            Console.WriteLine($"Clone command called for: {url}");
            var result = GitService.CloneRepo(url, path);
            if (!result.Success)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result.Message);
                Console.ResetColor();
                Environment.Exit(1);
            }
        }, repoUrlArg, targetDirArg);

        // push
        var pushCommand = new Command("push", "Push changes to remote");
        pushCommand.SetHandler(() =>
        {
            Console.WriteLine("Push command called");

            var repo = GitUtils.TryFindRepositoryFromCurrentDirectory();
            if (repo == null)
            {
                Console.WriteLine("No repository found in the current directory.");
                Environment.Exit(1);
            }
            var result = GitService.PushToRepo(repo);
            Console.WriteLine("Pushing successful");
            if (!result.Success)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result.Message);
                Console.ResetColor();
                Environment.Exit(1);
            }
        });

        // pull
        var pullCommand = new Command("pull", "Pull changes from remote");
        pullCommand.SetHandler(() =>
        {
            Console.WriteLine("Pull command called");

            var repo = GitUtils.TryFindRepositoryFromCurrentDirectory();
            if (repo == null)
            {
                Console.WriteLine("No repository found in the current directory.");
                Environment.Exit(1);
            }
            var result = GitService.PullFromRepo(repo);

            if (!result.Success)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result.Message);
                Console.ResetColor();
                Environment.Exit(1);
            }
        });

        // status
        var statusCommand = new Command("status", "Show the working tree status");
        statusCommand.SetHandler(() =>
        {
            Console.WriteLine("Status command called");

            var repo = GitUtils.TryFindRepositoryFromCurrentDirectory();
            if (repo == null)
            {
                Console.WriteLine("No repository found in the current directory.");
                Environment.Exit(1);
            }
            var result = GitService.DisplayGitStatus(repo);
            if (!result.Success)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result.Message);
                Console.ResetColor();
                Environment.Exit(1);
            }
        });

        // log
        var logCommand = new Command("log", "Show the commit tree");
        var logLengthArgument = new Argument<int>("length", () => 15, "Length of displayed commit log");
        logCommand.AddArgument(logLengthArgument);
        logCommand.SetHandler((int length) =>
        {
            var repo = GitUtils.TryFindRepositoryFromCurrentDirectory();
            if (repo == null)
            {
                Console.WriteLine("No repository found in the current directory.");
                Environment.Exit(1);
            }
            var result = GitService.DisplayLog(repo, length);
            if (!result.Success)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result.Message);
                Console.ResetColor();
                Environment.Exit(1);
            }
        }, logLengthArgument);

        // remote
        var SetRemoteCommand = new Command("remote", "Set the remote destination of the repository. Currently not supported");
        SetRemoteCommand.SetHandler(() =>
        {
            Console.WriteLine("Set Remote command called");
            Console.WriteLine("This is currently not supported");
        });

        // Add all to root
        rootCommand.AddCommand(initCommand);
        rootCommand.AddCommand(loginCommand);
        rootCommand.AddCommand(addCommand);
        rootCommand.AddCommand(removeCommand);
        rootCommand.AddCommand(commitCommand);
        rootCommand.AddCommand(cloneCommand);
        rootCommand.AddCommand(pushCommand);
        rootCommand.AddCommand(pullCommand);
        rootCommand.AddCommand(statusCommand);
        rootCommand.AddCommand(logCommand);
        rootCommand.AddCommand(SetRemoteCommand);

        return await rootCommand.InvokeAsync(args);
    }
}
