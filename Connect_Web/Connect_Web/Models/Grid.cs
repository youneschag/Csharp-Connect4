using System;
namespace Connect_Web.Models
{
	public class Grid
	{
        public int Rows { get; set; } = 6;
        public int Columns { get; set; } = 7;
        public List<Token> Tokens { get; set; } = new List<Token>();
    }
}

