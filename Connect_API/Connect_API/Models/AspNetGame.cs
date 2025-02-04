using System;
using System.ComponentModel.DataAnnotations;

namespace Connect_API.Models
{
    public class AspNetGame
    {
        public Guid Id { get; set; }

        // Rôle des joueurs
        public required string HostName { get; set; } // Nom de l'hôte
        public string? GuestName { get; set; } // Nom de l'invité, null si pas encore attribué
        public string? CurrentTurn { get; set; }

        // Statut de la partie
        public AspNetGameStatus Status { get; set; }

        public string? Winner { get; set; }
        public required string GameCode { get; set; }

        public DateTime CreationDate { get; set; }
        public DateTime ModificationDate { get; set; }
    }
}

