# TP 01_01 - Une première application Console

Ce projet réalise les exercices du TP consacrés aux arguments de ligne de commande, aux interactions avec la console, aux dictionnaires et aux méthodes génériques. Tout le code est regroupé dans **un seul projet console C#, TP1**, ciblant .NET 10 et sans dépendance tierce. Les messages du programme et les commentaires du code sont en français.

## Ouvrir et exécuter le projet

Ouvrir `TP1.slnx` dans Visual Studio et définir TP1 comme projet de démarrage. Deux profils de lancement sont disponibles :

- `TP1 - Exemples` : transmet automatiquement des exemples d'entier, de nombre décimal, de booléen et d'objet JSON.
- `TP1 - Saisie interactive` : lance le programme sans argument pour permettre une saisie argument par argument ; une ligne vide termine la saisie.

Appuyer sur F5 pour déboguer ou sur Ctrl+F5 pour exécuter sans débogage. Depuis le dossier de la solution, il est également possible d'utiliser :

```powershell
dotnet build .\TP1.slnx
dotnet run --project .\TP1\TP1.csproj --no-launch-profile -- "CLE1:Bonjour" "CLE2:5" "CLE4:3.5" "ACTIF:true"
dotnet run --project .\TP1\TP1.csproj --no-launch-profile
```

Avec ou sans arguments de ligne de commande, le programme propose ensuite de saisir une paire clé-valeur supplémentaire. Une clé vide permet d'ignorer cette étape. À la fin, le programme attend une touche avant de se fermer. Lorsque l'entrée standard est redirigée, cette attente est désactivée et la fin du flux est gérée normalement.

## Format des arguments et conventions

Les formats `CLE:VALEUR` et `CLE|VALEUR` sont acceptés. La séparation s'effectue uniquement au premier séparateur, ce qui permet à la valeur de contenir du JSON, une URL ou d'autres séparateurs. Les espaces en début et en fin de clé ou de valeur sont supprimés. Les clés vides et les arguments sans séparateur sont signalés puis ignorés ; les valeurs vides sont autorisées. Les clés sont sensibles à la casse. En cas de doublon, la première valeur est conservée.

Exemple :

```text
CLE1:Bonjour
CLE2:5
CLE4:3.5
ACTIF:true
PERSONNE:{"Nom":"Alice","Age":23}
```

Les résultats comprennent `CLE2 = 5`, `CLE4 = 3.5`, `ACTIF = True` et `Person PERSONNE = Alice, 23`.
Avec `CLE2:abc`, `CLE2:2147483648` ou une clé CLE2 absente, la récupération de l'entier renvoie la valeur par défaut `-1`, sans interrompre le programme.

## Correspondance entre le sujet et l'implémentation

| Étape du TP | Implémentation |
|---|---|
| Afficher les arguments dans une méthode dédiée | `Program.DisplayArguments` |
| Saisir les arguments un par un lorsqu'aucun n'est fourni | `Program.ReadArguments` |
| Saisir une clé et une valeur | `Program.ReadKeyValue` |
| Charger les arguments dans un dictionnaire et les afficher | `ParseCommandLineArgument`, `DisplayDictionary` |
| Définir et appeler la méthode de récupération d'un entier dans Program | `Program.GetIntOrDefault`, avec affichage du résultat dans Main |
| Créer une classe utilitaire dans Program.cs | `SameFileUtilities`, appelée depuis Main |
| Placer la classe utilitaire dans un fichier distinct | `DictionaryUtilities.cs`, appelée depuis Main |
| Généraliser la méthode et prendre en charge un type utilisateur complexe | `GetValueOrDefault<T>`, `Person.cs` et les exemples dans Main |
| Attendre une action de l'utilisateur | `Console.ReadKey`, uniquement lorsque l'entrée n'est pas redirigée |

`GetValueOrDefault<T>` vérifie d'abord la présence de la clé, puis tente une conversion. Les chaînes sont renvoyées directement. Les nombres, booléens, dates, énumérations, Guid et autres types disposant d'un convertisseur depuis une chaîne utilisent `TypeDescriptor`. Les objets, tableaux et collections compatibles avec la désérialisation JSON sont lus avec `System.Text.Json`. Un contenu invalide entraîne le retour de la valeur par défaut fournie. Les conversions numériques utilisent la culture invariante : le séparateur décimal est `.`.

Pour les types personnalisés sans convertisseur intégré, incompatibles avec une désérialisation JSON directe ou utilisant un format textuel particulier, un convertisseur `Func<string,T>` peut être fourni :

```csharp
Person p = DictionaryUtilities.GetValueOrDefault(
    values, "CONTACT", new Person(),
    text => { var parts = text.Split(','); return new Person { Nom = parts[0], Age = int.Parse(parts[1]) }; });
```

L'argument peut alors s'écrire `CONTACT:Alice,23`. Les erreurs de format, de dépassement numérique et d'argument du convertisseur entraînent également le retour de la valeur par défaut. Les autres exceptions de logique ne sont pas masquées systématiquement. Un type complexe nécessite une structure JSON adaptée ou un convertisseur personnalisé ; un objet ne peut pas être déduit automatiquement de n'importe quelle chaîne.

## Réponses sur les modificateurs d'accès

- Pour `GetIntOrDefault`, appelée uniquement à l'intérieur de Program : `private static`.
- Pour une autre classe de premier niveau située dans le même fichier et appelée par Program : la classe et la méthode peuvent être `internal` ; `static` évite de créer une instance. Le partage d'un fichier n'autorise pas l'accès aux membres privés d'une autre classe.
- Après déplacement dans un autre fichier `.cs` du même projet : la classe reste `internal` et la méthode `internal static`, car l'assembly ne change pas.
- Si la classe est déplacée dans un projet de bibliothèque distinct référencé par le projet console : la classe utilitaire et ses méthodes exposées doivent normalement être `public`. Pour respecter la demande de conserver un seul projet, aucun second projet de bibliothèque n'a été créé ici.
- La classe `Person` est `internal` et ses propriétés mappées en JSON sont `public`, afin que le sérialiseur puisse les lire et les écrire.

## Débogage dans Visual Studio et fichiers générés

Placer un point d'arrêt en cliquant dans la marge à gauche de la ligne `DisplayArguments(args)` dans Main, puis démarrer avec F5. Utiliser F10 pour exécuter pas à pas sans entrer dans les méthodes et F11 pour y entrer. Observer `args`, `values`, `defaultValue` et les résultats des conversions. Ces manipulations doivent être effectuées dans Visual Studio ; le code source ne remplace pas l'exercice de débogage interactif.

Après `dotnet build`, examiner le dossier `TP1/bin/Debug/net10.0/` :

- `TP1.exe` : lanceur Windows nécessitant le runtime .NET correspondant.
- `TP1.dll` : assembly managé du projet, contenant Main et les autres classes ; il peut être exécuté avec `dotnet TP1.dll`.
- `TP1.pdb` : symboles de débogage ; `.deps.json` et `.runtimeconfig.json` : configuration des dépendances et du runtime.
- `obj/` : dossier des fichiers intermédiaires de compilation, situé à la racine du projet.

Répartir le code entre plusieurs fichiers `.cs` d'un même projet ne crée pas de DLL utilitaire supplémentaire. Un projet de bibliothèque distinct produirait sa propre DLL et nécessiterait une référence de projet.
