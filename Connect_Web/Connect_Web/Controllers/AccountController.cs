using System.Net.Http;
using System.Text;
using System;
using System.Configuration;
using System.Security.Principal;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

using Connect_Web.Models;
using System.Net.NetworkInformation;
using System.Reflection;

namespace Connect_Web.Controllers
{
    public class AccountController : BaseController
    {
        public AccountController(IConfiguration configuration) : base(configuration)
        {
            ApiUrl = _configuration["ApiAuth"];
        }

        public ActionResult Sign()
        {
            HttpContext.Session.SetInt32("IsAuthenticated", 0); // Utilisez 0 pour false, 1 pour true
            return View();
        }

        public ActionResult Connect()
        {
            HttpContext.Session.SetInt32("IsAuthenticated", 0);
            return View();
        }

        public ActionResult Logout()
        {
            // Invalider la session et supprimer les informations d'authentification
            HttpContext.Session.SetInt32("IsAuthenticated", 0);
            return RedirectToAction("Sign");
        }

        public ActionResult Propos()
        {
            HttpContext.Session.SetInt32("IsAuthenticated", 0);
            return View();
        }

        public ActionResult Game()
        {
            HttpContext.Session.SetInt32("IsAuthenticated", 1);
            return View();
        }

        public ActionResult Register(string username, string password, string confirmPassword)
        {
            try
            {
                HttpResponseMessage httpResponseMessage = CallApiPut(String.Format("Account/Create/{0}/{1}/{2}", username, password, confirmPassword));
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }
                else
                {
                    // Si l'API renvoie une erreur, capturer le message d'erreur
                    string errorMessage = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    return Json(new { success = false, message = errorMessage });
                }
            }
            catch (Exception ex)
            {
                // En cas d'exception, retourner l'erreur au format JSON
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult Connexion([FromBody] AspNetUser user)
        {
            try
            {
                HttpResponseMessage httpResponseMessage = CallApiPost(String.Format("Account/Connect/{0}/{1}", user.username, user.password));

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var infos = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    return Json(new { success = true, infos = infos });
                }
                else
                {
                    // Si l'API renvoie une erreur, capturer le message d'erreur
                    string errorMessage = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    return Json(new { success = false, message = errorMessage });
                }
            }
            catch (Exception ex)
            {
                // En cas d'exception, retourner l'erreur au format JSON
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult GameCreation([FromBody] AspNetGame Game)
        {
            try
            {
                HttpResponseMessage httpResponseMessage = CallApiPut(String.Format("Account/GameCreation/{0}/{1}", Game.HostName, Game.GameCode));

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }
                else
                {
                    // Si l'API renvoie une erreur, capturer le message d'erreur
                    string errorMessage = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    return Json(new { success = false, message = errorMessage });
                }
            }
            catch (Exception ex)
            {
                // En cas d'exception, retourner l'erreur au format JSON
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult GameJoin([FromQuery] string GuestName, [FromQuery] string GameCode)
        {
            try
            {
                HttpResponseMessage httpResponseMessage = CallApiPost(String.Format("Account/GameJoin/{0}/{1}", GuestName, GameCode));

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }
                else
                {
                    // Si l'API renvoie une erreur, capturer le message d'erreur
                    string errorMessage = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    return Json(new { success = false, message = errorMessage });
                }
            }
            catch (Exception ex)
            {
                // En cas d'exception, retourner l'erreur au format JSON
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult InfosGame(string GameCode, string username)
        {
            try
            {
                HttpResponseMessage httpResponseMessage = CallApiGet(String.Format("Account/InfosGame/{0}/{1}", GameCode, username));

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var Game = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    return Json(new { success = true, Game = Game });
                }
                else
                {
                    // Si l'API renvoie une erreur, capturer le message d'erreur
                    string errorMessage = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    return Json(new { success = false, message = errorMessage });
                }
            }
            catch (Exception ex)
            {
                // En cas d'exception, retourner l'erreur au format JSON
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult GetHostGames(string username)
        {
            try
            {
                HttpResponseMessage httpResponseMessage = CallApiGet(String.Format("Account/GetHostGames/{0}", username));

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var games = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    return Json(new { success = true, games = games });
                }
                else
                {
                    // Si l'API renvoie une erreur, capturer le message d'erreur
                    string errorMessage = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    return Json(new { success = false, message = errorMessage });
                }
            }
            catch (Exception ex)
            {
                // En cas d'exception, retourner l'erreur au format JSON
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult GetGuestGames(string username)
        {
            try
            {
                HttpResponseMessage httpResponseMessage = CallApiGet(String.Format("Account/GetGuestGames/{0}", username));

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var games = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    return Json(new { success = true, games = games });
                }
                else
                {
                    // Si l'API renvoie une erreur, capturer le message d'erreur
                    string errorMessage = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    return Json(new { success = false, message = errorMessage });
                }
            }
            catch (Exception ex)
            {
                // En cas d'exception, retourner l'erreur au format JSON
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult GetActiveGames(string username)
        {
            try
            {
                HttpResponseMessage httpResponseMessage = CallApiGet(String.Format("Account/GetActiveGames/{0}", username));

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var games = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    return Json(new { success = true, games = games });
                }
                else
                {
                    // Si l'API renvoie une erreur, capturer le message d'erreur
                    string errorMessage = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    return Json(new { success = false, message = errorMessage });
                }
            }
            catch (Exception ex)
            {
                // En cas d'exception, retourner l'erreur au format JSON
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult UpdateGameStatus(Guid Id, int status)
        {
            try
            {
                HttpResponseMessage httpResponseMessage = CallApiPost(String.Format("Account/UpdateGameStatus/{0}/{1}", Id, status));

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    return Json(new { success = true});
                }
                else
                {
                    // Si l'API renvoie une erreur, capturer le message d'erreur
                    string errorMessage = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    return Json(new { success = false, message = errorMessage });
                }
            }
            catch (Exception ex)
            {
                // En cas d'exception, retourner l'erreur au format JSON
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult DeclareWinner([FromQuery] Guid Id, [FromQuery] string Winner)
        {
            try
            {
                HttpResponseMessage httpResponseMessage = CallApiPost(String.Format("Account/DeclareWinner/{0}/{1}", Id, Winner));

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }
                else
                {
                    // Si l'API renvoie une erreur, capturer le message d'erreur
                    string errorMessage = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    return Json(new { success = false, message = errorMessage });
                }
            }
            catch (Exception ex)
            {
                // En cas d'exception, retourner l'erreur au format JSON
                return Json(new { success = false, message = ex.Message });
            }
        }
         
        public ActionResult AddToken([FromBody] Token request)
        {
            try
            {
                // Sérialisez l'objet Token en JSON
                string tokenJson = JsonSerializer.Serialize(request);

                // Créez un HttpContent à partir du JSON
                HttpContent content = new StringContent(tokenJson, Encoding.UTF8, "application/json");

                // Appelez l'API en passant le contenu
                HttpResponseMessage httpResponseMessage = CallApiPut("Account/AddToken", content);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var result = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    return Json(new { success = true, result = result });
                }
                else
                {
                    // Si l'API renvoie une erreur, capturer le message d'erreur
                    string errorMessage = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    return Json(new { success = false, message = errorMessage });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        public ActionResult GameMoves(Guid GameId)
        {
            try
            {
                HttpResponseMessage httpResponseMessage = CallApiGet(String.Format("Account/GameMoves/{0}", GameId));

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var moves = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    return Json(new { success = true, moves });
                }
                else
                {
                    // Si l'API renvoie une erreur, capturer le message d'erreur
                    string errorMessage = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    return Json(new { success = false, message = errorMessage });
                }
            }
            catch (Exception ex)
            {
                // En cas d'exception, retourner l'erreur au format JSON
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}