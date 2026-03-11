using LibGit2Sharp;
using System;
using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.Invocation;
using SharpGit.Classes;

////! Eli se libgit2sharp
////! Ja ssh
////! Ja auth tarkistus mvc puolelle

//TODO Muista ottaa nuo writelinet pois

//NOTE 25/02/2026
//
// Aika tehdä loppuun
// Yritän tähdätä viikon eteenpäin deadlinen.
// Eli 4/03/2026

// Nonni
// Vähän ajatuksia tänne, etten unohda.
// Eli tässä tulee myös login
// Kun clone, push,pull jne. niin se kysyy käyttäjätunnuksen ja salasanan.
// Lähetetään se MVC endpoint, joka palauttaa ssh key ja short lived token
// Ja se tallennetaan johonkin, että ei tarvi joka kerta kysyä.

// Kun on action time, se tarkistaa tokenin ja sitten tekee magiaa

// Eli tulen tarviimaan jonkinsorting .sharpgit, johon tallennetaan ssh keyt ja config.json
// Joka on muotoa.

/*
{
  "monkey": {
    "username": "veijari",
    "token": "token_for_monkey",
    "token_expires": "2025-05-22T01:00:00Z",
    "ssh_key_path": "/home/user/.sharpgit/keys/monkey"
  },
  "giraffe": {
    "username": "veijari",
    "token": "token_for_giraffe",
    "token_expires": "2025-05-24T01:00:00Z",
    "ssh_key_path": "/home/user/.sharpgit/keys/giraffe"
  }
}
*/
namespace SharpGit;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("SharpGit CLI - a minimal git-like tool");


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
        var addPathArg = new Argument<IEnumerable<string>>("path", "Path to file or directory");

        addCommand.AddOption(updateOption);
        addCommand.AddArgument(addPathArg);

        addCommand.SetHandler((bool update, IEnumerable<string> paths) =>
        {
            var repo = GitUtils.TryFindRepositoryFromCurrentDirectory();
            if (repo == null)
            {
                Environment.Exit(1);
                return;
            }
            if (update)
            {
                Console.WriteLine("Using '--update' or '-u' to stage modified and deleted files.");
                var result = GitService.AddToRepoUpdate(repo);
                if (!result.Success)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Add failed: {result.Message}");
                    Console.ResetColor();
                }
            }

            foreach (var path in paths)
            {
                Console.WriteLine($"Adding file or directory: {path}");
                var result = GitService.AddToRepo(repo, path);
                if (!result.Success)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{result.Message}");
                    Console.ResetColor();
                    Environment.Exit(1);
                }
            }
        }, updateOption, addPathArg);

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
                return;
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
                Environment.Exit(1);
                Console.WriteLine("No repository found in the current directory.");
                return;
            }
            Console.WriteLine($"{message}");
            GitService.CommitToRepo(repo, message);
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
            GitService.CloneRepo(url, path);

            Console.WriteLine("Can not clone to current directory. Current directory is not empty.");
        }, repoUrlArg, targetDirArg);

        // push
        var pushCommand = new Command("push", "Push changes to remote");
        pushCommand.SetHandler(() =>
        {
            Console.WriteLine("Push command called");

            var repo = GitUtils.TryFindRepositoryFromCurrentDirectory();
            if (repo == null)
            {
                Environment.Exit(1);
                Console.WriteLine("No repository found in the current directory.");
                return;
            }
            GitService.PushToRepo(repo);
        });

        // pull
        var pullCommand = new Command("pull", "Pull changes from remote");
        pullCommand.SetHandler(() =>
        {
            Console.WriteLine("Pull command called");

            var repo = GitUtils.TryFindRepositoryFromCurrentDirectory();
            if (repo == null)
            {
                Environment.Exit(1);
                Console.WriteLine("No repository found in the current directory.");
                return;
            }
            GitService.PullFromRepo(repo);
        });

        // status
        var statusCommand = new Command("status", "Show the working tree status");
        statusCommand.SetHandler(() =>
        {
            Console.WriteLine("Status command called");

            var repo = GitUtils.TryFindRepositoryFromCurrentDirectory();
            if (repo == null)
            {
                Environment.Exit(1);
                Console.WriteLine("No repository found in the current directory.");
                return;
            }
            GitService.DisplayGitStatus(repo);
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
                Environment.Exit(1);
                Console.WriteLine("No repository found in the current directory.");
                return;
            }
            GitService.DisplayLog(repo, length);
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
