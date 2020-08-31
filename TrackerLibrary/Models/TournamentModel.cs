using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackerLibrary.Models
{
    public class TournamentModel
    {
        /// <summary>
        /// The unique identifier for the tournament
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// The name given to this tournament.
        /// </summary>
        public string TournamentName { get; set; }   //Name of the tornament

        /// <summary>
        /// The amount of money each team needs to put up to enter.
        /// </summary>
        public decimal EntryFee { get; set; }   //Entry fee cost

        /// <summary>
        /// The set of teams that have been entered.
        /// </summary>
        public List<TeamModel> EnteredTeams { get; set; } = new List<TeamModel>();  //List of teams that play this tournament

        /// <summary>
        /// The list of prizes for the various places.
        /// </summary>
        public List<PrizeModel> Prizes { get; set; } = new List<PrizeModel>();  //List of prizes for this tournament
        public List<List<MatchupModel>> Rounds { get; set; } = new List<List<MatchupModel>>();    //Lust of ???

        //EVENT!!!
        //We create event
        public event EventHandler<DateTime> OnTournamentComplete;
        //Now we create the method that fires that event
        //We need to check the subscribers, cause id theres no subscriber, we cant fire the event
        public void CompleteTournament()
        {
            OnTournamentComplete?.Invoke(this, DateTime.Now); //If you have a subscriber, fire the event
            
        }
    }
}
