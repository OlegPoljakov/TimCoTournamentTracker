using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary.Models;

namespace TrackerLibrary
{
    public static class TournamentLogic
    {
        //data come in, data go out - thats why static

        //Order our list randomly of teams
        //Check if its big enough - if its not - add in byes(automatic win)
        //2*2*2*2 - 2^4
        //Create our first round of matchups
        //Create every tound after that - 8 matchups - 4 matchups - 2 matchups - 1 matchups

        public static void CreateRounds(TournamentModel model)
        {
            List<TeamModel> randomizedTeams = RandomizeTeamOrder(model.EnteredTeams); //Заносим список учавствующих в турнире команд. Перемешиваем их и возвращаем в виде списка
            int rounds = FindNumberOfRounds(randomizedTeams.Count);
            int byes = NumberOfByes(rounds, randomizedTeams.Count);

            model.Rounds.Add(CreateFirstRound(byes, randomizedTeams));

            CreateOtherRounds(model, rounds);

            //UpdateTournamentResults(model);
        }

        private static void CreateOtherRounds(TournamentModel model, int rounds) 
        {
            int round = 2;
            List<MatchupModel> previousRound = model.Rounds[0]; //Grab the first list of MatchUpModel in our Rounds variable
            List<MatchupModel> currRound = new List<MatchupModel>();
            MatchupModel currMatchup = new MatchupModel();

            while (round <= rounds)
            {
                foreach (MatchupModel match in previousRound)
                {
                    currMatchup.Entries.Add(new MatchupEntryModel { ParentMatchup = match }); //Parent from the previous round! 
                    if (currMatchup.Entries.Count > 1) //If we have more than one entry
                    {
                        currMatchup.MatchupRound = round;
                        currRound.Add(currMatchup);
                        currMatchup = new MatchupModel();
                    }
                }
                model.Rounds.Add(currRound);
                previousRound = currRound;
                currRound = new List<MatchupModel>();
                round += 1;
            }
        }

        private static List<MatchupModel> CreateFirstRound(int byes, List<TeamModel> teams)
        {
            List<MatchupModel> output = new List<MatchupModel>();

            MatchupModel curr = new MatchupModel();

            foreach (TeamModel team in teams) //Идем по всем командам
            {
                curr.Entries.Add(new MatchupEntryModel { TeamCompeting = team });

                if (byes > 0 || curr.Entries.Count > 1)
                {
                    curr.MatchupRound = 1;
                    output.Add(curr);
                    curr = new MatchupModel();

                    if (byes > 0)
                    {
                        byes-=1;
                    }
                }
            }
            return output; 
        }

        private static int NumberOfByes(int rounds, int numberOfTeams)
        {
            int output=0;
            int totalTeams=1;

            for (int i = 1; i <= rounds; i++)
            {
                totalTeams *= 2; //Т.е. если rounds - 1, то totalTeams=2, если rounds - 4, то 
            }

            output = totalTeams - numberOfTeams;
            return output;
        }

        private static int FindNumberOfRounds(int teamCount)
        {
            int output = 1;
            int val = 2;

            while (val<teamCount )
            {
                output += 1;
                val *= 2;
            }
            return output; //output  - это раунд! So now we know the number of rounds
        }

        private static List<TeamModel> RandomizeTeamOrder(List<TeamModel> teams)
        {
            teams.OrderBy(x=>Guid.NewGuid()).ToList();
            return teams;
        }

        public static void UpdateTournamentResults(TournamentModel model)
        {
            int startingRound = model.CheckCurrentRound();
            List<MatchupModel> toScore = new List<MatchupModel>();
            foreach (List<MatchupModel> round in model.Rounds)
            {
                foreach (MatchupModel rm in round) //Now we have each matchup
                {
                    //Check if any of the entries have a score not equal to zero. So if we have completed a matchthen we've put the value for two scores in
                    //We are not accepting ties for tournaments. Therefore one team at least has to have a score different than zero. They can be negative or positive. 
                    //It just cant both be zero. So if any entry can say ".Score != 0", then it return TRUE. But if rm.Entries.Count == 1, then there is one team in match. There are bye
                    //rm.Winner == null - we have not yet assigned to winner
                    // WE WILL KNOW WHICH ONE NEED TO BE SCORED
                    if (rm.Winner == null && (rm.Entries.Any(x => x.Score != 0) || rm.Entries.Count == 1))
                    {
                        toScore.Add(rm); //добавился тот , у кого bye
                    }
                }
            }

            MarkWinnersInMatchups(toScore); //Если у нас в метче есть bye, то почеаем, что в этом метче есть победитель m.Winner = Entry []

            AdvancedWinners(toScore, model);
            
            //Shorten down foreach
            toScore.ForEach(x => GlobalConfig.Connection.UpdateMatchup(x));
            //its the same as if we write
            /*
            foreach(MatchupModel x in toScore)
            {
                GlobalConfig.Connection.UpdateMatchup(x);
            }
            */
            
            //This is there EMAIL part starts
            
            int endingRound = model.CheckCurrentRound();

            if (endingRound > startingRound)
            {
                //Alert users
                model.AlertUsersToNewRound();
            }
            
        }

        public static void AlertUsersToNewRound(this TournamentModel model)
        {
            int CurrentRoundNumber = model.CheckCurrentRound();
            List<MatchupModel> currentRound = model.Rounds.Where(x => x.First().MatchupRound == CurrentRoundNumber).First();

            foreach (MatchupModel matchup in currentRound)
            {
                foreach (MatchupEntryModel me in matchup.Entries)
                {
                    foreach (PersonModel p in me.TeamCompeting.TeamMembers)
                    {
                        AlertPersonToNewRound(p, me.TeamCompeting.TeamName, matchup.Entries.Where(x => x.TeamCompeting != me.TeamCompeting).FirstOrDefault());
                    }
                }
            }
        }

        private static void AlertPersonToNewRound(PersonModel p, string teamName, MatchupEntryModel competitor)
        {
            if (p.EmailAddress.Length == 0)
            {
                return;
            }    

            //string fromAddress = "";
            string to = ""; //We can send the message more than to one person
            string subject = "";
            //string body = "";
            StringBuilder body = new StringBuilder(); //Соединение строк. 

            if (competitor != null)
            {
                subject = $"You have a new matchup with { competitor.TeamCompeting.TeamName }";

                body.AppendLine("<h1>You have a new matchup</h1>");
                body.Append("<strong>Competitor: ");
                body.Append(competitor.TeamCompeting.TeamName);
                body.AppendLine();
                body.AppendLine();
                body.AppendLine("Have a great time!");
                body.AppendLine("~Tournament Tracker");
            }
            else
            {
                subject = "You have a bye week this round";

                body.AppendLine("Enjoy your round off!");
                body.AppendLine("~Tournament Tracker");
            }

            to = p.EmailAddress;

            EmailLogic.SendEmail(to, subject, body.ToString());
        }

        private static int CheckCurrentRound(this TournamentModel model)
        {
            int output = 1;

            foreach (List<MatchupModel> round in model.Rounds)
            {
                //look at every single matchup inside the round and check if there all of them have a winner
                if (round.All(x => x.Winner != null)) //If ALL are true
                {
                    output += 1;
                }
                else
                {
                    return output;
                }
            }
            
            //Tournament is complete
            CompleteTournament(model);
            return output - 1;
        }

        private static void CompleteTournament(TournamentModel model)
        {
            GlobalConfig.Connection.CompleteTournament(model);      
            TeamModel winners = model.Rounds.Last().First().Winner; //Заменяет LOOP THROUGH!!!! Linking
            TeamModel runnerUp = model.Rounds.Last().First().Entries.Where(x => x.TeamCompeting != winners).First().TeamCompeting;

            decimal winnerPrize = 0;
            decimal runnerUpPrize = 0;

            if (model.Prizes.Count > 0)
            {
                decimal totalIncome = model.EnteredTeams.Count * model.EntryFee;

                PrizeModel firstPlacePrize = model.Prizes.Where(x => x.PlaceNumber == 1).FirstOrDefault();
                PrizeModel secondPlacePrize = model.Prizes.Where(x => x.PlaceNumber == 2).FirstOrDefault();

                if (firstPlacePrize != null)
                {
                    winnerPrize = firstPlacePrize.CalculatePrizePayout(totalIncome);
                }

                if (secondPlacePrize != null)
                {
                    runnerUpPrize = secondPlacePrize.CalculatePrizePayout(totalIncome);
                }
            }

            //Send Email to all Tournament
            string subject = "";
            //string body = "";
            StringBuilder body = new StringBuilder(); //Соединение строк. 
   
            subject = $"In { model.TournamentName }, { winners.TeamName } has won!";

            body.AppendLine("<h1>You have a WINNER!</h1>");
            body.AppendLine("<p>Congratulations to our winner!</p>");
            body.AppendLine("<br />");

            if (winnerPrize > 0)
            {
                body.AppendLine($"<p>{ winners.TeamName} will receive ${winnerPrize}</p>");
            }

            if (runnerUpPrize > 0)
            {
                body.AppendLine($"<p>{ runnerUp.TeamName} will receive ${runnerUpPrize}</p>");
            }

            body.AppendLine("<p>Thanks for a great tournament everyone!</p>");
            body.AppendLine("~Tournament Tracker");

            List<string> bcc = new List<string>();

            foreach (TeamModel t in model.EnteredTeams)
            {
                foreach (PersonModel p in t.TeamMembers)
                {
                    if (p.EmailAddress.Length > 0) //Check if its valid address
                    {
                        bcc.Add(p.EmailAddress);
                    }
                }
            }

            EmailLogic.SendEmail(new List<string>(), bcc, subject, body.ToString());

            model.CompleteTournament();//Now that event will be fired if tournament is complete!!!!!
        }

        private static decimal CalculatePrizePayout(this PrizeModel prize, decimal totalIncome)
        {
            decimal output = 0;
            if (prize.PrizeAmount > 0)
            {
                output = prize.PrizeAmount;
            }
            else
            {
                output = Decimal.Multiply(totalIncome, Convert.ToDecimal(prize.PrizePercentage / 100));
            }
            return output;
        }

        private static void AdvancedWinners(List<MatchupModel> models, TournamentModel tournament)
        {
            foreach (MatchupModel m in models)
            {
                //Перенос команды победителя в другой раунд
                foreach (List<MatchupModel> round in tournament.Rounds)
                {
                    foreach (MatchupModel rm in round) //Now we have each matchup
                    {
                        foreach (MatchupEntryModel me in rm.Entries)
                        {
                            //Не у всех метчей есть parentmatch, поэтому тут может выпадать null.
                            if (me.ParentMatchup != null)
                            {
                                if (me.ParentMatchup.Id == m.Id) //rm.Id - which is our current matchup id
                                {
                                    me.TeamCompeting = m.Winner;
                                    GlobalConfig.Connection.UpdateMatchup(rm);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void MarkWinnersInMatchups(List<MatchupModel> models)
        {
            //greater or lesser
            string greaterWins = ConfigurationManager.AppSettings["greaterWins"];

            foreach (MatchupModel m in models)
            {
                //this is for bye week entry
                if (m.Entries.Count == 1)
                {
                    m.Winner = m.Entries[0].TeamCompeting;
                    continue; //So we will not go to the next if's but we will go next in foreach. Break would mean that foreach is done
                }
                //0 means false, or low score wins
                if (greaterWins == "0")
                {
                    if (m.Entries[0].Score > m.Entries[1].Score)
                    {
                        m.Winner = m.Entries[0].TeamCompeting;
                    }
                    else if (m.Entries[1].Score > m.Entries[0].Score)
                    {
                        m.Winner = m.Entries[1].TeamCompeting;
                    }
                    else
                    {
                        throw new Exception("We do not allow ties in this application.");
                    }
                }
                else
                {
                    //1 mean true, or high score wins
                    if (m.Entries[0].Score < m.Entries[1].Score)
                    {
                        m.Winner = m.Entries[0].TeamCompeting;
                    }
                    else if (m.Entries[1].Score < m.Entries[0].Score)
                    {
                        m.Winner = m.Entries[1].TeamCompeting;
                    }
                    else
                    {
                        throw new Exception("We do not allow ties in this application.");
                    }
                }
            }
        }
    }
}
