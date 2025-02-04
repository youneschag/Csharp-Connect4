using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Connect_API.Models
{
    public class AspNetGameMove
    {
        public Guid Id { get; set; }

        public required Guid GameId { get; set; }

        public AspNetGame? Game { get; set; }

        public required string PlayerName { get; set; }

        public required int X { get; set; }

        public required int Y { get; set; }

        public required string Color { get; set; }

        public DateTime MoveTime { get; set; } = DateTime.UtcNow;

        public int? Duration { get; set; } // Durée de la partie en secondes
    }
}