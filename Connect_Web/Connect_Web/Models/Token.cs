using System;
namespace Connect_Web.Models
{
	public class Token
	{
        public int X { get; set; } // Position en colonne
        public int Y { get; set; } // Position en ligne
        public string Color { get; set; } // 'yellow' ou 'red'
        public Guid GameId { get; set; }
        public string PlayerName { get; set; }
        public int Duration { get; set; }
        
        public Token(int x, int y, string color, Guid gameId, string playerName, int duration)
        {
            X = x;
            Y = y;
            Color = color;
            GameId = gameId;
            PlayerName = playerName;
            Duration = duration;
        }
    }
}

