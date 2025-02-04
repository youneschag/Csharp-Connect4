
# Lancement du jeu Connect 4

#### Membres du groupe
    - CHAGUER Younes
    - AIT BOUSERHANE Maria

Le jeu Connect 4 a été conçu en utilisant l'architecture MVC en dotnet avec une base de données SQLite. 

Ce document fournit une vue d'ensemble complète du projet, en mettant en évidence les principales fonctionnalités, l'architecture, et les ajustements nécessaires pour respecter les règles et logiques métiers.

L'application est une application 3-tiers (Connect_Web, Connect_API et Base de données) :

Connect_Web est une application frontend développée en ASP.NET MVC, tandis que Connect_API est une API backend qui gère les règles métiers, l'accès aux données et la logique de l'application. 

Objectif principal :

Offrir une expérience fluide aux utilisateurs, respectant strictement les responsabilités des couches MVC et API.

# Structure du projet

#### Connect_Web

```http
    Frontend basé sur MVC : Gestion de l'interface utilisateur et des appels aux API.
 ```

  - Controlleurs : 
    - **BaseController** : 
        - Responsabilité : 
            - Ce contrôleur sert de base pour d'autres contrôleurs, notamment pour gérer les appels API. 
            - Il inclut des méthodes génériques pour effectuer des requêtes HTTP (GET, POST, PUT, DELETE) vers les API.
        - Principales fonctionnalités :
            -  Gestion des délais d'attente des API via une configuration (ApiMaxWaitingTime).
            - Calcul du temps total des appels API.
            - Configuration d'URL pour simplifier les appels API.
    - **AccountController** : 
        - Responsabilité : Gérer la logique liée aux utilisateurs (inscription, connexion, déconnexion) et aux interactions avec les parties (création, gestion, et participation).
        - Principales fonctionnalités :
            - Inscription : Permet à un utilisateur de s'inscrire avec un mot de passe sécurisé et un nom d'utilisateur unique.
            - Connexion : Valide les informations d'identification pour permettre à un utilisateur d'accéder à ses parties.
            - Création de partie : Permet à un utilisateur (hôte) de créer une nouvelle partie en générant un GameCode unique.
            - Rejoindre une partie : Permet à un utilisateur invité d'accéder à une partie existante via le GameCode.
            - Affichage des parties actives : Affiche toutes les parties en cours, que l'utilisateur soit hôte ou invité.
            - Gestion des mouvements : Valide les mouvements des joueurs et met à jour l'état du jeu via des appels à l'API backend.

  - Views  :
    - **Sign.cshtml** : Permet à l'utilisateur de s'inscrire pour la première fois dans l'application en créant un nom d'utilisateur et son mot de passe (Contrainte : le mot de passe doit être au minimum de 8 caractères avec au moins 1 miniscule, 1 majuscule, 1 chiffre et 1 caractère spécial).
    - **Connect.cshtml** : Permet à l'utilisateur de se connecter à ses différentes parties en vérifiant son nom d'utilisateur et mot de passe. 
    - **Game.cshtml** : Mise en page de la grille de jeu, ainsi que les différentes parties propres à l'utilisateur (Parties Hôtes, Guest et Actives). 

  - Models : 
    - **AspNetUser** : Gestion des utilisateurs
    - **AspNetGame** : Gestion des parties
    - **AspNetGameMove** : Gestion des mouvements des joueurs
    - **AspNetGameStatus** : Statut des parties
    - **Token** : Représentation des jetons du jeu
    - **Grid** : Gestion de la grille

#### Connect_API

```http
    Backend RESTful API : gestion des données, logique métier et validation.
    Base de données : SQLite via Entity Framework Core.
 ```

  - Controlleurs : 
    - **AccountController** : 
        - Responsabilité : Gérer toutes les règles métiers et les interactions avec la base de données pour les utilisateurs, les parties, et les mouvements.
        - Principales fonctionnalités :
            - Gestion des utilisateurs :
                - Ajouter des utilisateurs à la base de données avec un hachage sécurisé des mots de passe.
                - Valider les informations d'identification des utilisateurs lors de la connexion.
            - Gestion des parties :
                - Créer une nouvelle partie et l'enregistrer dans la base de données.
                - Permettre à un invité de rejoindre une partie via un GameCode.
                - Mettre à jour l'état des parties (En attente d'un invité, En cours, Terminée).
            - Gestion des mouvements :
                - Valider les tours des joueurs.
                - Vérifier si un mouvement est valide (colonne pleine, joueur autorisé, etc.).
                - Ajouter un mouvement à la base de données et vérifier les conditions de victoire.

  - Models : 
    - **AspNetUser** : Gestion des utilisateurs.
    - **AspNetGame** : Gestion des parties.
    - **AspNetGameMove** : Gestion des mouvements des joueurs.
    - **AspNetGameStatus** : Enumération des statuts des parties
    - **SqlDbContext** : Contexte de la base de données

#### Base de données

```http
    SQLite : Connect.db
    Migrations : Contient les migrations EF Core pour gérer la structure de la base de données.
 ```

# Fonctionnement de l'application

#### Inscription :
    Un utilisateur s'inscrit en fournissant un nom d'utilisateur unique et un mot de passe conforme aux règles de sécurité.
    Les informations sont stockées dans la base de données, avec un mot de passe haché pour plus de sécurité.

#### Connexion :
    Un utilisateur se connecte en saisissant son nom d'utilisateur et son mot de passe.
    Les informations sont validées via l'API backend.
    
#### Création de partie :
    L'utilisateur (hôte) crée une nouvelle partie en générant un GameCode unique.
    Ce GameCode est utilisé pour identifier la partie dans la base de données.
    La partie reste en statut "En attente d'un invité" jusqu'à ce qu'un autre utilisateur la rejoigne.

#### Rejoindre une partie :
    Un utilisateur invité rejoint une partie existante en saisissant le GameCode dans l'interface.
    L'API vérifie si le GameCode correspond à une partie existante et si l'invité n'est pas l'hôte.

#### Jouer :
    Une fois la partie commencée, les joueurs effectuent des mouvements tour par tour.
    Chaque mouvement est validé par l'API backend (vérification de la colonne, du tour, etc.).
    L'API vérifie également si un joueur a gagné après chaque mouvement ou si la partie est terminée par un match nul.

#### Fin de la partie :
    La partie passe en statut "Terminée" lorsque l'un des joueurs gagne ou si toutes les colonnes sont pleines.
    L'API met à jour la base de données avec les informations du gagnant.

#### Affichage des parties actives :
    Les utilisateurs peuvent consulter la liste des parties actives (en cours) auxquelles ils participent (en tant qu'hôte ou invité).

#### Les fichiers de log :
    Ajout de la configuration pour afficher les logs (Serilog) lors de l'utilisation de l'application classés par la date du jour.
## API Reference

Les différentes API sont consultables directement grâce à la documentation Swagger, qui s'affichera lors du lancement du projet Connect_API.sln

Voici quelques exemples d'API: 

#### Récupérer toutes les informations sur la partie en question depuis son GameCode

```http
  GET /Account/InfosGame/{GameCode}
```

| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `GameCode`| `string` | **Required**               |

#### Création de la partie souhaitée par le host en utilisant le token unique de la partie

```http
  PUT /Account/GameCreation/{HostName}/{GameCode}
```

| Parameter | Type     | Description                       |
| :-------- | :------- | :-------------------------------- |
| `HostName`| `string` | **Required**                      |
| `GameCode`| `string` | **Required**                      |

#### Récupérer toutes les parties où le status est toujours en cours, quelque soit si l'utilisateur est Host ou Guest

```http
  POST /Account/GetActivesGames/${username}
```

| Parameter | Type     | Description                       |
| :-------- | :------- | :-------------------------------- |
| `username`| `string` | **Required**                      |


## Ajout des utilisateurs et parties pré-définis

Les données initiales sont configurées dans le fichier SqlDbContext à l'aide de la méthode OnModelCreating.

#### Utilisateurs prédéfinis :

| Username  | Mot de Passe                |
| :-------- | :------------------------- |
| `Younes`  | **Younes123&**               |
| `Maria`   | **Maria456@**               |

```javascript
modelBuilder.Entity<AspNetUser>().HasData(
    new AspNetUser
    {
        Id = Guid.NewGuid(),
        Username = "Younes",
        PasswordHash = passwordHasher.HashPassword(null, "Younes123&"),
        CreationDate = DateTime.UtcNow
    },
    new AspNetUser
    {
        Id = Guid.NewGuid(),
        Username = "Maria",
        PasswordHash = passwordHasher.HashPassword(null, "Maria456@"),
        CreationDate = DateTime.UtcNow
    }
);
```

#### Parties prédéfinies :

| Host    | Guest    | Status      |  GameCode     |
| :-------| :------- | :---------- | :------------ |
| `Younes`| `Maria`  | En cours    | **gyxmd4c7m** |
| `Younes`| `null`   | En attente  | **bga0dgz47** |

```javascript
modelBuilder.Entity<AspNetGame>().HasData(
    new AspNetGame
    {
        Id = Guid.NewGuid(),
        HostName = "Younes",
        GuestName = "Maria",
        CurrentTurn = "red",
        Status = AspNetGameStatus.InProgress,
        GameCode = "gyxmd4c7m",
        Winner = null,
        CreationDate = DateTime.UtcNow,
        ModificationDate = DateTime.UtcNow
    },
    new AspNetGame
    {
        Id = Guid.NewGuid(),
        HostName = "Younes",
        GuestName = null,
        CurrentTurn = "red",
        Status = AspNetGameStatus.AwaitingGuest,
        GameCode = "bga0dgz47",
        Winner = null,
        CreationDate = DateTime.UtcNow,
        ModificationDate = DateTime.UtcNow
    }
);
```
## Authors

- [@chagueryounes](https://github.com/youneschag)

- [@aitbouserhane](https://www.github.com/aitbouserhane)

## 🛠 Skills
- C#, ASP.NET MVC, Entity Framework Core

- JavaScript, HTML, CSS

- Architecture RESTful API


## Instructions pour l'exécution du projet

Initialiser la base de données :

```bash
  dotnet ef database update
```
    
Se diriger sur le chemin du backend (Connect_API) et le lancer:

```bash
  dotnet run --project Connect_API
```
- En lançant le projet avec "Connect_API.sln",  la documentation Swagger s'ouvrira automatiquement, vous permettant de consulter et tester toutes les API disponibles.

Se diriger sur le chemin du frontend (Connect_Web) et le lancer:

```bash
  dotnet run --project Connect_Web
```

Si besoin, supprimez la base de données puis recréez-la :

```bash
  rm Connect.db
  dotnet ef database update
```
