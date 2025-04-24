// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");


// Eli se libgit2sharp
// Ja ssh
// Ja auth tarkistus mvc puolelle

// Kun on pushaamassa, lähettää ennen sitä jonkun viestin mvc puolelle, joka tekee sen git init --bare puolen

// Vaikka git remote add kohdassa voisi tehdä sen auth jutun ja tag se local repo jotenkin että se tietää että on auth kontsa

string[] options = { "Init", "Clone", "Commit", "Push", "Pull" };
int index = 0;

ConsoleKey key;
do
{
    Console.Clear();
    for (int i = 0; i < options.Length; i++)
    {
        if (i == index)
        {
            Console.BackgroundColor = ConsoleColor.Gray;
            Console.ForegroundColor = ConsoleColor.Black;
        }

        Console.WriteLine(options[i]);
        Console.ResetColor();
    }

    key = Console.ReadKey(true).Key;

    if (key == ConsoleKey.UpArrow)
        index = (index == 0) ? options.Length - 1 : index - 1;
    else if (key == ConsoleKey.DownArrow)
        index = (index + 1) % options.Length;

} while (key != ConsoleKey.Enter);

Console.WriteLine($"You selected: {options[index]}");
