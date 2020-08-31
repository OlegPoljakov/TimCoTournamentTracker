private void createTournamentButton_Click(object sender, EventArgs e)
    {    
        //Validate Data
        decimal fee = 0;
        bool feeAcceptable = decimal.TryParse(entryFeeValue.Text, out fee);
        if (!feeAcceptable)
        {
            MessageBox.Show("You need to enter a valid Entry Fee!", "Invalid Fee", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
		}

		//Create our tournament model
		TournamentModel tm = new TournamentModel();
		tm.TournamentName = tournamentNameValue.Text;
		tm.EntryFee = fee;
		tm.Prizes = selectedPrizes;
		tm.EnteredTeams = selectedTeams; //List<TeamModel> EnteredTeams

		//Wire our matchups
		TournamentLogic.CreateRounds(tm);


		//Create tournament entry
		//Create all of the prizes entries
		//Create all of team entries
		GlobalConfig.Connection.CreateTournament(tm);
            
	}

public static void CreateRounds(TournamentModel model)
	{
		List<TeamModel> randomizedTeams = RandomizeTeamOrder(model.EnteredTeams); 
		int rounds = FindNumberOfRounds(randomizedTeams.Count);
		int byes = NumberOfByes(rounds, randomizedTeams.Count);

		model.Rounds.Add(CreateFirstRound(byes, randomizedTeams));

		CreateOtherRounds(model, rounds);
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