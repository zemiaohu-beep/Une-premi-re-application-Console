using System.Globalization;
using System.Text;

namespace TP1;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("TP 01_01 - Une premiere application Console");

        if (args.Length == 0)
            args = ReadArguments();

        DisplayArguments(args);
        Dictionary<string, string> values = ParseCommandLineArgument(args);
        ReadKeyValue(values);
        DisplayDictionary(values);

        Console.WriteLine("\n--- GetIntOrDefault ---");
        Console.WriteLine($"Program : CLE2 = {GetIntOrDefault(values, "CLE2", -1)}");
        Console.WriteLine($"Classe du meme fichier : CLE2 = {SameFileUtilities.GetIntOrDefault(values, "CLE2", -1)}");
        Console.WriteLine($"Classe separee : CLE2 = {DictionaryUtilities.GetIntOrDefault(values, "CLE2", -1)}");
        Console.WriteLine($"Cle absente : {GetIntOrDefault(values, "__cle_absente__", -1)}");

        Console.WriteLine("\n--- GetValueOrDefault<T> ---");
        Console.WriteLine($"int CLE2 = {DictionaryUtilities.GetValueOrDefault(values, "CLE2", -1)}");
        Console.WriteLine($"double CLE4 = {DictionaryUtilities.GetValueOrDefault(values, "CLE4", 0.0).ToString(CultureInfo.InvariantCulture)}");
        Console.WriteLine($"bool ACTIF = {DictionaryUtilities.GetValueOrDefault(values, "ACTIF", false)}");
        Console.WriteLine($"string CLE1 = {DictionaryUtilities.GetValueOrDefault(values, "CLE1", "inconnue")}");
        Person person = DictionaryUtilities.GetValueOrDefault(values, "PERSONNE", new Person { Nom = "Inconnu", Age = 0 });
        Console.WriteLine($"Person PERSONNE = {person.Nom}, {person.Age}");

        // Pas de blocage lors d'une redirection ou des tests automatiques.
        if (!Console.IsInputRedirected)
        {
            Console.WriteLine("\nPressez une touche pour quitter.");
            Console.ReadKey(intercept: true);
        }
    }

    private static string[] ReadArguments()
    {
        var arguments = new List<string>();
        Console.WriteLine("Aucun parametre. Saisissez les arguments un par un (CLE:VALEUR ou CLE|VALEUR).");
        Console.WriteLine("Une ligne vide termine la saisie.");
        while (true)
        {
            Console.Write($"Arg[{arguments.Count}] = ");
            string? line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
                break;
            arguments.Add(line);
        }
        return arguments.ToArray();
    }

    private static void DisplayArguments(string[] args)
    {
        Console.WriteLine($"\nNombre de parametre = {args.Length}");
        for (int i = 0; i < args.Length; i++)
            Console.WriteLine($"Arg[{i}] = [{args[i]}]");
    }

    private static Dictionary<string, string> ParseCommandLineArgument(string[] args)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (string arg in args)
        {
            // Separer une seule fois pour conserver ':' ou '|' dans la valeur (JSON, URL...).
            int separator = arg.IndexOfAny([':', '|']);
            if (separator <= 0 || string.IsNullOrWhiteSpace(arg[..separator]))
            {
                Console.WriteLine($"Argument ignore (format CLE:VALEUR attendu) : {arg}");
                continue;
            }
            string key = arg[..separator].Trim();
            string value = arg[(separator + 1)..].Trim();
            if (!values.TryAdd(key, value))
                Console.WriteLine($"Cle dupliquee ignoree : {key} (premiere valeur conservee)");
        }
        return values;
    }

    private static void ReadKeyValue(Dictionary<string, string> values)
    {
        Console.Write("\nCle supplementaire (Entree pour ignorer) : ");
        string? key = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(key))
            return;
        Console.Write("Valeur : ");
        string? value = Console.ReadLine();
        if (value is null)
        {
            Console.WriteLine("Fin de saisie : paire non ajoutee.");
            return;
        }
        if (!values.TryAdd(key, value.Trim()))
            Console.WriteLine($"Cle dupliquee ignoree : {key} (premiere valeur conservee)");
    }

    private static void DisplayDictionary(Dictionary<string, string> values)
    {
        Console.WriteLine("\n--- Dictionnaire ---");
        if (values.Count == 0)
            Console.WriteLine("(vide)");
        foreach (KeyValuePair<string, string> entry in values)
            Console.WriteLine($"{entry.Key} = {entry.Value}");
    }

    // Premier exercice : utilisee uniquement par Program, donc private.
    private static int GetIntOrDefault(Dictionary<string, string> values, string key, int defaultValue)
    {
        return values.TryGetValue(key, out string? value)
            && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result)
            ? result : defaultValue;
    }
}

// Etape intermediaire du TP : classe placee dans le meme fichier que Program.
internal static class SameFileUtilities
{
    internal static int GetIntOrDefault(Dictionary<string, string> values, string key, int defaultValue)
        => DictionaryUtilities.GetIntOrDefault(values, key, defaultValue);
}
