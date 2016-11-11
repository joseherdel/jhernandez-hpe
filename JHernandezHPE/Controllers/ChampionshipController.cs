using JHernandezHPE.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace JHernandezHPE.Controllers
{
    public class ChampionshipController : Controller
    {
        private JHernandezHPEContext db = new JHernandezHPEContext();

        // GET: Championship
        public ActionResult Index()
        {
            return View();
        }

        // GET: Top Players
        [HttpGet]
        public JsonResult Top(int? count)
        {
            return Json(db.Players.Select(c => new { c.Username, c.Points })
                .OrderByDescending(player => player.Points).ThenBy(p => p.Username)
                .Take(count ?? default(int))
                .ToList(), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Receive 2 winners, first and second and add their respective points, request must 
        /// have Content-Type: application/x-www-form-urlencoded and params sent as first=Dave&second=Armando
        /// </summary>
        /// <param name="first">First Place, will receive 3 points</param>
        /// <param name="second">Second Place, will receive 1 point</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Result(string first, string second)
        {
            //Assign points to winners:
            AssignPoints(first, 3);
            AssignPoints(second, 1);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        /// <summary>
        /// Receives the championship data and computes it to identify the winner. 
        /// The first and second place are stored into a database with their respective scores, request must 
        /// have Content-Type: application/x-www-form-urlencoded
        /// </summary>
        /// <param name="data">JSON string with the tournament data. </param>
        /// <returns>Returns the winner of the championship.</returns>
        [HttpPost]
        public ActionResult New(string data)
        {
            Dictionary<string, string> tournament = ConfigTournamentFromRequest(data);
            tournament = Play(tournament);

            return Json(tournament);
        }

        /// <summary>
        /// Reset the current championship to start with a new one.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Reset()
        {
            var all = from c in db.Players select c;
            db.Players.RemoveRange(all);
            db.SaveChanges();

            TempData["successMessage"] = "The tournament has been reset successfully";

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Starts a new championship with the data provided on the text file.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult NewFromPage()
        {
            Dictionary<string, string> tournament = ConfigTournamentFromFile();
            if (tournament != null)
            {
                tournament = Play(tournament);
                TempData["successMessage"] = "We have a winner! \n Its " + tournament.ElementAt(0).Key +
                    " using: '" + tournament.ElementAt(0).Value + "'";
            }
            else
            {
                TempData["errorMessage"] = "The file uploaded is invalid, please use one on the sample files provided on menu as template.";
            }

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Read and parse the text file to create the tournament keys
        /// </summary>
        /// <returns>Dictionary with all the players and their strategies</returns>
        public Dictionary<string, string> ConfigTournamentFromRequest(string data)
        {
            return ParseJSON(data);
        }

        /// <summary>
        /// Read and parse the text file to create the tournament keys
        /// </summary>
        /// <returns>Dictionary with all the players and their strategies</returns>
        public Dictionary<string, string> ConfigTournamentFromFile()
        {

            Dictionary<string, string> tournament = new Dictionary<string, string>();

            // Read the file from server
            string filePath = Server.MapPath(@"~/Championship/tournament01.txt");

            // Parse the JSON data into a dictionary with the tournament keys:
            using (StreamReader r = new StreamReader(filePath))
            {
                string json = r.ReadToEnd();
                tournament = ParseJSON(json);
            }

            return tournament;
        }

        /// <summary>
        /// Resolves the championship provided on the Dictionary received as parameter 
        /// and assign the points to the winners and persist the database. 
        /// </summary>
        /// <param name="dictionary">Contains the participants and their specific strategies</param>
        public Dictionary<string, string> Play(Dictionary<string, string> dictionary)
        {

            Dictionary<string, string> winners = new Dictionary<string, string>(dictionary);

            // Loop through championship until only 2 finalists are get to the grand final.
            while (winners.Count > 2)
            {
                int index = 0;
                dictionary = new Dictionary<string, string>(winners);

                while (index < dictionary.Count)
                {
                    winners.Remove(getLoser(new Dictionary<string, string> {
                        { dictionary.ElementAt(index).Key, dictionary.ElementAt(index).Value },
                        { dictionary.ElementAt(index + 1).Key, dictionary.ElementAt(index + 1).Value } }));

                    index = index + 2;
                }
            }

            string username = getLoser(new Dictionary<string, string> {
                        { winners.ElementAt(0).Key, winners.ElementAt(0).Value },
                        { winners.ElementAt(1).Key, winners.ElementAt(1).Value } });

            //Assign points to second place:
            AssignPoints(username, 1);
            winners.Remove(username);

            //Assign points to first place:
            AssignPoints(winners.ElementAt(0).Key, 3);

            return winners;

        }

        /// <summary>
        /// Parse from Json to a tournament dictionary
        /// </summary>
        /// <param name="json">Data of the tournament on JSON format</param>
        /// <returns>Dictionary filled with tournament keys</returns>
        public Dictionary<string, string> ParseJSON(string json)
        {

            Dictionary<string, string> tournament = new Dictionary<string, string>();
            dynamic dynObj = null;

            //Validate param is valid JSON
            if (isValidJSON(json))
                dynObj = JsonConvert.DeserializeObject(json);
            else
                return null;

            foreach (dynamic item in dynObj)
            {
                foreach (dynamic item2 in item)
                {
                    // Validate there are 2 players on the current match
                    if (((JContainer)item).Count == 2)
                    {
                        foreach (dynamic item3 in item2)
                        {
                            string username = ((JValue)((JContainer)item3).First).Value.ToString();
                            string strategy = ((JValue)((JContainer)item3).Last).Value.ToString();

                            if (isValidStrategy(strategy))
                            {
                                tournament.Add(username, strategy);
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            // Validate if keys are balanced:
            if (isBalanced(tournament.Count))
                return tournament;
            else
                return null;
        }

        /// <summary>
        /// Validate that the string provided is a valid JSON
        /// </summary>
        /// <param name="json">String that we are going to parse</param>
        /// <returns>True if valid, otherwise no</returns>
        public bool isValidJSON(string json)
        {
            try
            {
                JToken.Parse(json);
                return true;
            }
            catch (JsonReaderException ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Valid that the user is using a valid strategy (R, P or S)
        /// </summary>
        /// <param name="strategy">Strategy that is going to be validated</param>
        /// <returns></returns>
        public bool isValidStrategy(string strategy)
        {
            if (strategy.Equals("R") || strategy.Equals("P") || strategy.Equals("S"))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Validate that the keys are balanced,  (that is, there are 2^n players, 
        /// and each one participates in exactly one match per round).
        /// </summary>
        /// <param name="numOfPlayers">Number of participants provided on the torunament file.</param>
        /// <returns>True if the values are right</returns>
        public bool isBalanced(double numOfPlayers)
        {
            for (int i = 1; i <= 7; i++)
            {
                if (Math.Pow(2, i) == numOfPlayers)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Assign the number of points to the username specified on the params
        /// </summary>
        /// <param name="username">Winner username that will receive the points</param>
        /// <param name="points">Number of points won</param>
        private void AssignPoints(string username, int points)
        {
            Player player = db.Players.SingleOrDefault(p => p.Username == username);
            if (player == null)
            {
                player = new Player();
                player.Username = username;
                player.Points = points;
                db.Players.Add(player);
            }
            else
            {
                player.Points = player.Points + points;
                db.Entry(player).State = System.Data.Entity.EntityState.Modified;
            }

            db.SaveChanges();
        }

        /// <summary>
        /// Compares 2 players to figure out a winner
        /// </summary>
        /// <param name="match">Dictionary with the 2 players and their strategies</param>
        /// <returns>The username of the loser</returns>
        public string getLoser(IDictionary<string, string> match)
        {
            switch (match.ElementAt(0).Value)
            {
                case "R":
                    switch (match.ElementAt(1).Value)
                    {
                        case "R": return match.ElementAt(1).Key;
                        case "P": return match.ElementAt(0).Key;
                        case "S": return match.ElementAt(1).Key;
                        default: throw new Exception("Invalid Strategy.");
                    }
                case "P":
                    switch (match.ElementAt(1).Value)
                    {
                        case "R": return match.ElementAt(1).Key;
                        case "P": return match.ElementAt(1).Key;
                        case "S": return match.ElementAt(0).Key;
                        default: throw new Exception("Invalid Strategy.");
                    }
                case "S":
                    switch (match.ElementAt(1).Value)
                    {
                        case "R": return match.ElementAt(0).Key;
                        case "P": return match.ElementAt(1).Key;
                        case "S": return match.ElementAt(1).Key;
                        default: throw new Exception("Invalid Strategy.");
                    }
                default: throw new Exception("Invalid Strategy.");
            }

        }
    }
}