using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Connect_Web.Models
{
    public class AspNetGameMove
    {
        [Key]
        public Guid Id { get; set; }

        public required Guid GameId { get; set; }

        [ForeignKey("GameId")]
        public AspNetGame? Game { get; set; } // Relation avec la table AspNetGames

        public required string PlayerName { get; set; }

        public required int X { get; set; }

        public required int Y { get; set; }

        public required string Color { get; set; }

        public DateTime MoveTime { get; set; } = DateTime.UtcNow;

        public int Duration { get; set; } // Durée de la partie en secondes


        public AspNetGameMove(int x, int y, string color, Guid gameId, string playerName)
        {
            X = x;
            Y = y;
            Color = color;
            GameId = gameId;
            PlayerName = playerName;
        }
    }
}