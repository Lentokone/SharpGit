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

        // init
        var initCommand = new Command("init", "Initialize a new repository");
        initCommand.SetHandler(() => {
            Console.WriteLine("Init command called");
        });

        // add
        var addCommand = new Command("add", "Add files to staging area");
        var addPathArg = new Argument<string>("path", "Path to file or directory");
        addCommand.AddArgument(addPathArg);
        addCommand.SetHandler((string path) => {
            Console.WriteLine($"Add command called with path: {path}");
        }, addPathArg);

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
        });

        // Add all to root
        rootCommand.AddCommand(initCommand);
        rootCommand.AddCommand(addCommand);
        rootCommand.AddCommand(commitCommand);
        rootCommand.AddCommand(cloneCommand);
        rootCommand.AddCommand(pushCommand);
        rootCommand.AddCommand(pullCommand);
        rootCommand.AddCommand(statusCommand);

        return await rootCommand.InvokeAsync(args);
    }
}
