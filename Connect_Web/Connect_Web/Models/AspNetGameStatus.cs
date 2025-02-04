using System;
namespace Connect_Web.Models
{
    // Enumération des statuts possibles pour une partie
    public enum AspNetGameStatus
    {
        AwaitingGuest, // En attente d'un invité
        InProgress,    // En cours
        Finished       // Terminée
    }
}

