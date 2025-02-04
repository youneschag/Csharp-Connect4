using Connect_API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Reflection.Metadata;
using System.Xml.Linq;
using Microsoft.Extensions.Hosting;

namespace Connect_API.Controllers
{ 
    [ApiController]  
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {

        private readonly ILogger<AccountController> _logger;
        private readonly SqlDbContext _context;
        private readonly PasswordHasher<IdentityUser> _passwordHasher;

        public AccountController(ILogger<AccountController> logger, SqlDbContext context)
        {
            _context = context;
            _logger = logger;
            _passwordHasher = new PasswordHasher<IdentityUser>();
        }

        [HttpPut("Create/{username}/{password}/{confirmPassword}")]
        public ActionResult RegisterUser(string username, string password, string confirmPassword)
        {
            // Vérification des données d'entrée (modèle valide)
            if (ModelState.IsValid)
            {
                // Vérification de la longueur du nom d'utilisateur (plus de 3 caractères)
                if (username.Length <= 3)
                {
                    return BadRequest("Le nom d'utilisateur doit contenir plus de 3 caractères");
                }

                // Vérification du mot de passe (au moins 8 caractères, 1 minuscule, 1 majuscule, 1 chiffre, 1 caractère spécial)
                var passwordRegex = new System.Text.RegularExpressions.Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$");
                if (!passwordRegex.IsMatch(password))
                {
                    return BadRequest("Le mot de passe doit contenir au moins 8 caractères, une majuscule, une minuscule, un chiffre et un caractère spécial");
                }

                // Vérification que les mots de passe correspondent
                if (password != confirmPassword)
                {
                    return BadRequest("Les mots de passe ne correspondent pas");
                }

                // Vérifiez si l'utilisateur existe déjà dans la base de données
                if (_context.AspNetUsers.Any(u => u.Username == username))
                {
                    return BadRequest("Un utilisateur avec ce nom d'utilisateur existe déjà");
                }

                // Création d'un nouvel utilisateur
                var user = new AspNetUser
                {
                    Id = Guid.NewGuid(),
                    Username = username,
                    PasswordHash = _passwordHasher.HashPassword(null, password), // Hachage sécurisé du mot de passe
                    CreationDate = DateTime.UtcNow
                };

                // Ajout de l'utilisateur dans la base de données
                _context.AspNetUsers.Add(user);
                _context.SaveChanges();

                return Ok("Utilisateur créé avec succès");
            }

            // Retour si les données sont invalides
            return BadRequest("Données invalides");
        }

        [HttpPost("Connect/{username}/{password}")]
        public ActionResult ConnectUser(string username, string password)
        {
            if (ModelState.IsValid)
            {

                // Vérification de la longueur du nom d'utilisateur (plus de 3 caractères)
                if (username.Length <= 3)
                {
                    return BadRequest("Le nom d'utilisateur doit contenir plus de 3 caractères");
                }

                // Vérification du mot de passe (au moins 8 caractères, 1 minuscule, 1 majuscule, 1 chiffre, 1 caractère spécial)
                var passwordRegex = new System.Text.RegularExpressions.Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$");
                if (!passwordRegex.IsMatch(password))
                {
                    return BadRequest("Le mot de passe doit contenir au moins 8 caractères, une majuscule, une minuscule, un chiffre et un caractère spécial");
                }

                // Vérifiez si l'utilisateur existe déjà
                var user = _context.AspNetUsers.FirstOrDefault(u => u.Username == username);
                if (user == null)
                {
                    return BadRequest("Veuillez vous inscrire avant de vous connecter");
                }

                // Vérification du mot de passe haché
                var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(null, user.PasswordHash, password);
                if (passwordVerificationResult == PasswordVerificationResult.Success)
                {
                    return Ok(username);
                }
                else
                {
                    return BadRequest("Mot de passe incorrect");
                }
            }
            return BadRequest("Données invalides");
        }
        
        [HttpPut("GameCreation/{HostName}/{GameCode}")]
        public ActionResult GameCreation(string HostName, string GameCode)
        {
            try
            {
                // Initialisation de la nouvelle partie
                var game = new AspNetGame
                {
                    HostName = HostName,
                    GuestName = null,
                    CurrentTurn = "red", // L'invité commence par défaut
                    Id = Guid.NewGuid(),
                    Winner = null,
                    GameCode = GameCode,
                    Status = AspNetGameStatus.AwaitingGuest, // Toujours en attente d'un invité
                    CreationDate = DateTime.UtcNow
                };

                // Enregistrer dans la base de données
                _context.AspNetGames.Add(game);
                _context.SaveChanges();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur serveur : {ex.Message}");
            }
        }

        [HttpPost("GameJoin/{GuestName}/{GameCode}")]
        public ActionResult GameJoin(string GuestName, string GameCode)
        {
            try
            {
                // Vérifier que le GameCode est valide
                if (string.IsNullOrWhiteSpace(GameCode))
                {
                    return BadRequest("Le code de la partie est invalide.");
                }

                // Recherche de la partie correspondante au GameCode
                var game = _context.AspNetGames.FirstOrDefault(g => g.GameCode == GameCode);

                if (game == null)
                {
                    return NotFound("Partie introuvable avec ce code.");
                }

                // Vérifier que le joueur invité n'est pas l'hôte
                if (game.HostName == GuestName)
                {
                    return BadRequest("Le joueur hôte ne peut pas rejoindre en tant qu'invité.");
                }

                // Vérifier que la partie est en attente d'un invité
                if (game.Status != AspNetGameStatus.AwaitingGuest)
                {
                    return BadRequest("La partie n'est pas en attente d'un invité.");
                }

                // Vérifier que l'utilisateur invité n'a pas déjà rejoint la partie
                if (!string.IsNullOrEmpty(game.GuestName) && game.GuestName == GuestName)
                {
                    return BadRequest("Vous avez déjà rejoint cette partie.");
                }

                // Mise à jour des informations de la partie
                game.GuestName = GuestName;
                game.Status = AspNetGameStatus.InProgress; // Changer le statut en "En cours"

                // Sauvegarde des modifications dans la base de données
                _context.SaveChanges();

                return Ok(new { success = true, message = "Partie rejointe avec succès." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur serveur : {ex.Message}");
            }
        }

        [HttpGet("InfosGame/{GameCode}/{username}")]
        public ActionResult InfosGame(string GameCode, string username)
        {
            try
            {
                // Vérifier si une partie existe avec le GameCode
                var gameList = _context.AspNetGames
                    .Where(g => g.GameCode == GameCode)
                    .Select(game => new
                    {
                        game.HostName,
                        game.GuestName,
                        game.Id,
                        game.CurrentTurn,
                        game.Status,
                        game.Winner
                    })
                    .ToList();

                if (!gameList.Any())
                {
                    return NotFound("Aucune partie trouvée avec ce code.");
                }

                // Obtenir la première partie
                var game = gameList.First();

                // Vérifiez si l'utilisateur actuel est autorisé à accéder à cette partie
                if (username != game.HostName && username != game.GuestName)
                {
                    return Forbid("Vous n'êtes pas autorisé à accéder à cette partie, le nombre maximal de joueurs est atteint");
                }

                return Ok(gameList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur serveur : {ex.Message}");
            }
        }

        [HttpGet("GetHostGames/{username}")]
        public ActionResult GetHostGames(string username)
        {
            try
            {
                // Récupérer toutes les parties où HostName correspond
                var games = _context.AspNetGames
                    .Where(game => game.HostName == username)
                    .Select(game => new
                    {
                        game.Id,
                        game.HostName,
                        game.GuestName,
                        game.Winner,
                        game.GameCode,
                        StatusText = MapStatusText(game.Status), // Appeler la fonction pour mapper le statut
                        CreationDate = game.CreationDate.ToString("yyyy-MM-dd") // Formatter la date
                    })
                    .ToList();

                return Ok(games);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur serveur : {ex.Message}");
            }
        }

        [HttpGet("GetGuestGames/{username}")]
        public ActionResult GetGuestGames(string username)
        {
            try
            {
                // Récupérer toutes les parties où GuestName correspond
                var games = _context.AspNetGames
                    .Where(game => game.GuestName == username)
                    .Select(game => new
                    {
                        game.Id,
                        game.HostName,
                        game.GuestName,
                        game.Winner,
                        game.GameCode,
                        StatusText = MapStatusText(game.Status), // Appeler la fonction pour mapper le statut
                        CreationDate = game.CreationDate.ToString("yyyy-MM-dd") // Formatter la date
                    })
                    .ToList();

                return Ok(games);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur serveur : {ex.Message}");
            }
        }

        [HttpGet("GetActiveGames/{username}")]
        public ActionResult GetActiveGames(string username)
        {
            try
            {
                // Récupérer toutes les parties actives où l'utilisateur est soit Host soit Guest
                var activeGames = _context.AspNetGames
                    .Where(game => (game.GuestName == username || game.HostName == username) && game.Status == AspNetGameStatus.InProgress) // Filtrer par joueur et statut actif
                    .Select(game => new
                    {
                        game.Id,
                        game.HostName,
                        game.GuestName,
                        game.Winner,
                        game.GameCode,
                        game.CurrentTurn,
                        StatusText = MapStatusText(game.Status), // Mapper le statut en texte lisible
                        CreationDate = game.CreationDate.ToString("yyyy-MM-dd") // Formatter la date
                    })
                    .ToList();

                return Ok(activeGames);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur serveur : {ex.Message}");
            }
        }

        // Fonction locale pour mapper le statut
        private static string MapStatusText(AspNetGameStatus status)
        {
            return status switch
            {
                AspNetGameStatus.AwaitingGuest => "En attente d'un invité",
                AspNetGameStatus.InProgress => "En cours",
                AspNetGameStatus.Finished => "Terminée",
                _ => "Statut inconnu"
            };
        }

        [HttpPost("UpdateGameStatus/{Id}/{status}")]
        public ActionResult UpdateGameStatus(Guid Id, int status)
        {
            try
            {
                var game = _context.AspNetGames.FirstOrDefault(g => g.Id == Id);

                if (game == null)
                {
                    return NotFound("Partie introuvable.");
                }

                // Si la partie est "en attente d'un invité", on change son status en "en cours"
                switch (status)
                {
                    case 1:
                        game.Status = AspNetGameStatus.InProgress;
                        break;

                    case 2:
                        game.Status = AspNetGameStatus.Finished;
                        break;

                    default:
                        return BadRequest(new { success = false, message = "Statut de la partie non modifiable." });
                }

                _context.SaveChanges();

                return Ok(new { success = true, message = "Statut de la partie mis à jour." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur serveur : {ex.Message}");
            }
        }

        [HttpPost("DeclareWinner/{Id}/{Winner}")]
        public ActionResult DeclareWinner(Guid Id, string Winner)
        {
            try
            {
                var game = _context.AspNetGames.FirstOrDefault(g => g.Id == Id);

                if (game == null)
                {
                    return NotFound("Partie introuvable.");
                }

                game.Winner = Winner;

                _context.SaveChanges();

                return Ok(new { success = true, message = "Le joueur gagnant a été mis à jour" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur serveur : {ex.Message}");
            }
        }

        [HttpPut("AddToken")]
        public ActionResult AddToken([FromBody] AspNetGameMove token)
        {
            try
            {
                // Récupérer le jeu correspondant à GameId
                var game = _context.AspNetGames.FirstOrDefault(g => g.Id == token.GameId);
                if (game == null)
                {
                    return NotFound($"Aucun jeu trouvé avec l'ID {token.GameId}");
                }

                // Vérifier si l'invité a rejoint
                if (string.IsNullOrEmpty(game.GuestName))
                {
                    return BadRequest("En attente d'un invité. Vous ne pouvez pas jouer pour le moment");
                }

                // Vérifier si la partie est active
                if (game.Status != AspNetGameStatus.InProgress)
                {
                    return BadRequest("La partie n'est pas active");
                }

                // Vérifier si c'est bien au joueur actuel de jouer
                if ((game.CurrentTurn == "yellow" && token.PlayerName != game.HostName) ||
                    (game.CurrentTurn == "red" && token.PlayerName != game.GuestName))
                {
                    return BadRequest($"C'est à {(game.CurrentTurn == "yellow" ? game.HostName : game.GuestName)} de jouer.");
                }

                // Vérifier si la colonne est pleine
                var movesInColumn = _context.GameMoves.Where(m => m.GameId == game.Id && m.X == token.X).Count();
                if (movesInColumn >= 6)
                {
                    return BadRequest("Colonne pleine");
                }

                // Calculer la position Y (dernière cellule vide)
                var y = 5 - movesInColumn; // Calculer Y en partant du bas

                // Ajouter le mouvement
                var gameMove = new AspNetGameMove
                {
                    Id = Guid.NewGuid(),
                    GameId = token.GameId,
                    PlayerName = token.PlayerName,
                    X = token.X,
                    Y = y,
                    Color = token.Color,
                    MoveTime = DateTime.UtcNow,
                    Duration = token.Duration
                };

                // Vérifier si ce mouvement mène à une victoire
                if (CheckVictory(game.Id, token.X, y, token.Color))
                {
                    game.Winner = token.PlayerName;
                    game.Status = AspNetGameStatus.Finished;
                    _context.SaveChanges();

                    return Ok(new { message = "Victoire !", NextTurn = game.CurrentTurn, winner = token.PlayerName, Y = y });
                }

                // Vérifier si la grille est pleine (match nul)
                if (IsGridFull(game.Id))
                {
                    game.Winner = "Both";
                    game.Status = AspNetGameStatus.Finished;
                    _context.SaveChanges();

                    return Ok(new { message = "Match nul !", NextTurn = game.CurrentTurn, Y = y });
                }

                _context.GameMoves.Add(gameMove);

                // Mettre à jour le joueur dont c'est le tour
                game.CurrentTurn = game.CurrentTurn == "red" ? "yellow" : "red";
                _context.AspNetGames.Update(game);

                _context.SaveChanges();

                return Ok(new { NextTurn = game.CurrentTurn, Y = y });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur serveur : {ex.Message}");
            }
        }

        private bool CheckVictory(Guid gameId, int x, int y, string color)
        {
            // Vérification dans toutes les directions
            return CheckDirection(gameId, x, y, color, 1, 0) || // Horizontal
                   CheckDirection(gameId, x, y, color, 0, 1) || // Vertical
                   CheckDirection(gameId, x, y, color, 1, 1) || // Diagonale haut-gauche -> bas-droite
                   CheckDirection(gameId, x, y, color, 1, -1);  // Diagonale bas-gauche -> haut-droite
        }

        private bool CheckDirection(Guid gameId, int x, int y, string color, int dx, int dy)
        {
            int count = 1; // Inclure le jeton actuel

            // Vérification dans la direction positive (dx, dy)
            count += CountInDirection(gameId, x, y, color, dx, dy);

            // Vérification dans la direction négative (-dx, -dy)
            count += CountInDirection(gameId, x, y, color, -dx, -dy);

            return count >= 4; // Si on trouve 4 jetons ou plus
        }

        private int CountInDirection(Guid gameId, int x, int y, string color, int dx, int dy)
        {
            int count = 0;

            for (int step = 1; step < 4; step++)
            {
                int newX = x + step * dx;
                int newY = y + step * dy;

                // Vérification des limites de la grille
                if (newX < 0 || newY < 0 || newX >= 7 || newY >= 6)
                    break;

                // Vérification de la correspondance des couleurs
                var move = _context.GameMoves.FirstOrDefault(m =>
                    m.GameId == gameId && m.X == newX && m.Y == newY && m.Color == color);

                if (move != null)
                {
                    count++;
                }
                else
                {
                    break;
                }
            }

            return count;
        }

        private bool IsGridFull(Guid gameId)
        {
            for (int x = 0; x < 7; x++)
            {
                var movesInColumn = _context.GameMoves.Count(m => m.GameId == gameId && m.X == x);
                if (movesInColumn < 6)
                {
                    return false; // Il reste encore de l'espace dans cette colonne
                }
            }
            return true; // Toutes les colonnes sont pleines
        }

        [HttpGet("GameMoves/{GameId}")]
        public ActionResult GetGameMoves(Guid GameId)
        {
            try
            {
                var moves = _context.GameMoves
                    .Where(move => move.GameId == GameId)
                    .Select(move => new
                    {
                        move.X,
                        move.Y,
                        move.Color,
                        move.PlayerName,
                        move.MoveTime
                    })
                    .ToList();

                return Ok(moves);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}