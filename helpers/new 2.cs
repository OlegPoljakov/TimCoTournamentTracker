public void CreateTournament(TournamentModel model)
	{
		List<TournamentModel> tournaments = TournamentFile.FullFilePath().LoadFile().ConvertToTournamentModels(TeamFile, PeopleFile, PrizesFile);

		int currentId = 1;

		if (tournaments.Count > 0)
		{
			currentId = tournaments.OrderByDescending(x => x.Id).First().Id + 1; //Order the list descending by ID
		}

		model.Id = currentId;

		model.SaveRoundsToFile(MatchupFile, MatchupEntryFile);

		tournaments.Add(model);

		tournaments.SaveToTournamentFile(TournamentFile);
	}
	
public static List<TournamentModel> ConvertToTournamentModels(this List<string> lines, string teamFileName, string peopleFileName, string prizeFileName)
	{
		//Id, TournamentName, EntryFee, (id|id|id - Entered Teams), (id|id|id - Prizes), (Rounds - id^id^id|id^id^id|id^id^id)
		List<TournamentModel> output = new List<TournamentModel>();
		List<TeamModel> teams = teamFileName.FullFilePath().LoadFile().ConvertToTeamModels(peopleFileName);
		List<PrizeModel> prizes = prizeFileName.FullFilePath().LoadFile().ConvertToPrizeModel();
		List<MatchupModel> matchups = GlobalConfig.MatchupFile.FullFilePath().LoadFile().ConvertToMatchupModels();

		foreach (string line in lines)
		{
			string[] cols = line.Split(',');
			TournamentModel tm = new TournamentModel();

			tm.Id = int.Parse(cols[0]); //Id
			tm.TournamentName = cols[1]; //TournamentName
			tm.EntryFee = decimal.Parse(cols[2]); //EntryFee

			string[] teamIds = cols[3].Split('|'); //(id|id|id - Entered Teams)
			foreach (string id in teamIds)
			{
				tm.EnteredTeams.Add(teams.Where(x => x.Id == int.Parse(id)).First());
			}

			string[] prizeIds = cols[4].Split('|'); //(id|id|id - Prizes)
			foreach (string id in prizeIds)
			{
				tm.Prizes.Add(prizes.Where(x => x.Id == int.Parse(id)).First());
			}

			//Capture rounds information
			string[] rounds = cols[5].Split('|'); //id^id^id|id^id^id|id^id^id
			

			foreach (string round in rounds)
			{
				string[] msText = round.Split('^');
				List<MatchupModel> ms = new List<MatchupModel>();

				foreach (string matchModelTextId in msText)
				{
					ms.Add(matchups.Where(x => x.Id == int.Parse(matchModelTextId)).First());
				}

				tm.Rounds.Add(ms);
			}
			
			output.Add(tm);
		}
		return output;
	}
//List<TeamModel> teams = teamFileName.FullFilePath().LoadFile().ConvertToTeamModels(peopleFileName);	
//teamFileName.FullFilePath().LoadFile(). --- this List<string> lines
public static List<TeamModel> ConvertToTeamModels(this List<string> lines,string peopleFileName)
	{
		//id, team name, list of ids separated by he pipe
		//3, Tim's Team, 1|3|5
		List<TeamModel> output = new List<TeamModel>(); //Конечный результат, который выдает метод
		List<PersonModel> people = peopleFileName.FullFilePath().LoadFile().ConvertToPersonModels();

		foreach (string line in lines)
		{
			string[] cols = line.Split(','); //разделяем запятыми и кладем в массив под названием cols
			TeamModel t = new TeamModel();
			t.Id = int.Parse(cols[0]);
			t.TeamName = cols[1];

			string[] personIds = cols[2].Split('|');
			foreach (string id in personIds)
			{
				//What its doing: Take the list of people in our textfile, and says search for it and filte where the ID of the
				//person in the list equals the ID from "(string id in personIds)". So in theory it should find only one person.
				//First() - give us a first item in the LIST!!!
				t.TeamMembers.Add(people.Where(x => x.Id == int.Parse(id)).First()); //First - возвращает первый элемент массива. Фильтрация внутри списка
			}
			output.Add(t);
		}
		return output;
	}
	
public static List<PersonModel> ConvertToPersonModels(this List<string> lines)
	{
		List<PersonModel> output = new List<PersonModel>();
		foreach (string line in lines)
		{
			string[] cols = line.Split(','); //разделяем запятыми и кладем в массив под названием cols
			PersonModel p = new PersonModel();
			p.Id = int.Parse(cols[0]); //проверка на номер, на число
			p.FirstName = cols[1];
			p.LastName = cols[2];
			p.EmailAddress = cols[3];
			p.CellphoneNumber = cols[4];

			output.Add(p);
		}

		return output;
	}	
	