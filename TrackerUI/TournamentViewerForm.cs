using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrackerLibrary;
using TrackerLibrary.Models;

namespace TrackerUI
{
    public partial class TournamentViewerForm : Form
    {
        private TournamentModel tournament;
        /*
        Разница в том, что биндингЛист сообщает об изменении своих свойств контролу к которому привязан
        т.е добавился элемент в список и, внезапно, в листБоксе(например) к которому привязан этот список, тоже добавляется элемент
        */

        /*
        Small note: Усли мы ввели значение балла дял команды, нажали кнопку Score, то эта команда должна перейти в другой раунд!!!
        */
        BindingList<int> rounds = new BindingList<int>();
        BindingList<MatchupModel> selectedMatchups = new BindingList<MatchupModel>();

        public TournamentViewerForm(TournamentModel tournamentModel)
        {
            InitializeComponent();

            //Subscribe to events
            tournament = tournamentModel;
            tournament.OnTournamentComplete += Tournament_OnTournamentComplete;

            //we take that model and we store it into the private variable at the form level/. So anuthing on the form can have an access to this tournament object
            tournament = tournamentModel;

            WireUpLists();

            LoadFormData();

            LoadRounds();
        }

        private void Tournament_OnTournamentComplete(object sender, DateTime e)
        {
            this.Close();
        }

        private void LoadFormData()
        {
            tournamentName.Text = tournament.TournamentName;
        }

        private void TournamentViewerForm_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
         
        /* WE NEED TO SEPARATE THEM OR WE WILL HAVE AN ERROR!!!
        private void WireUpList()
        {
            roundDropDown.DataSource = null; //To wipe out some other values which are alredy there
            roundDropDown.DataSource = rounds;

            matchupListBox.DataSource = null;
            matchupListBox.DataSource = selectedMatchups;
            matchupListBox.DisplayMember = "DisplayName";
        }
        */

        private void WireUpLists()
        {
            //roundDropDown.DataSource = null; //To wipe out some other values which are alredy there
            //roundsBinding.DataSource = rounds;
            roundDropDown.DataSource = rounds;
            matchupListBox.DataSource = selectedMatchups;
            matchupListBox.DisplayMember = "DisplayName";
        }


        private void LoadRounds()
        {
            //rounds = new BindingList<int>(); //We initialize the list of rounds everytime. If we dont do it, we can duplicate rounds. So we better start up from fresh every time
            rounds.Clear(); //Заместо обновления попробуем очищать

            rounds.Add(1); //we always have at least one round
            int currRound = 1;

            foreach (List<MatchupModel> matchups in tournament.Rounds)
            {
                if (matchups.First().MatchupRound > currRound)
                {
                    currRound = matchups.First().MatchupRound;
                    rounds.Add(currRound);
                }
            }
            LoadMatchups(1);
        }
        
        //Event/ See lesson 22 - timing: 17:14
        private void roundDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadMatchups((int)roundDropDown.SelectedItem);
        }

        private void LoadMatchups(int round)
        {
            foreach (List<MatchupModel> matchups in tournament.Rounds)
            {
                if (matchups.First().MatchupRound == round)
                {
                    selectedMatchups.Clear();
                    //Вот тут вероятно надо сделать проверку на то, выбрано ли чтото в листбоксе
                    
                    foreach (MatchupModel m in matchups)
                    {
                        if (m.Winner == null || !unplayedOnlyCheckbox.Checked) // no winner means the match is unplayed
                        {
                            selectedMatchups.Add(m);
                        }
                    }                   
                }
            }

            if (selectedMatchups.Count > 0)
            {
                LoadMatchup(selectedMatchups.First());
            }

            DisplayMatchupInfo();
        }
        
        //Если список матчей пустой, то не надо отобрадать ничего о командах. ведь их нет.
        private void DisplayMatchupInfo()
        {
            //true or false
            bool isVisible = (selectedMatchups.Count > 0);

            teamOneName.Visible = isVisible; //no need to specify if its true or false, we are based on the line above
            teamOneScoreLabel.Visible = isVisible;
            teamOneScoreValue.Visible = isVisible;
            teamTwoName.Visible = isVisible;
            teamTwoScoreLabel.Visible = isVisible;
            teamTwoScoreValue.Visible = isVisible;

            versusLabel.Visible = isVisible;
            scoreButton.Visible = isVisible;
        }

        private void LoadMatchup(MatchupModel m)
        {
            for(int i =0; i < m.Entries.Count; i++)
            {
                if (i == 0)
                {
                    if (m.Entries[0].TeamCompeting != null)
                    {
                        teamOneName.Text = m.Entries[0].TeamCompeting.TeamName;
                        teamOneScoreValue.Text = m.Entries[0].Score.ToString();

                        teamTwoName.Text = "<bye>";
                        teamTwoScoreValue.Text = "0";
                    }
                    else
                    {
                        teamOneName.Text = "Not Yet Set";
                        teamOneScoreValue.Text = "";
                    }
                }

                if (i == 1)
                {
                    if (m.Entries[1].TeamCompeting != null)
                    {
                        teamTwoName.Text = m.Entries[1].TeamCompeting.TeamName;
                        teamTwoScoreValue.Text = m.Entries[1].Score.ToString();
                    }
                    else
                    {
                        teamTwoName.Text = "Not Yet Set";
                        teamTwoScoreValue.Text = "";
                    }
                }
            }
        }

        //Реакция на выбор объекта в окне
        private void matchupListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (matchupListBox.SelectedIndex >= 0)
            {
                LoadMatchup((MatchupModel)matchupListBox.SelectedItem);
            }
            else
            {
                return;
            }
        }

        private void unplayedOnlyCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            LoadMatchups((int)roundDropDown.SelectedItem); //Загрузили все метчи определенного раунда

        }

        private void scoreButton_Click(object sender, EventArgs e)
        {
            string errorMessage = ValidateData();
            if (errorMessage.Length > 0)  //method to scheck data
            {
                MessageBox.Show($"Input Error: {errorMessage}");
                return; //Выйти из метода, так как дальнейшее его продолжние не имеет смысла
             }
            
            //определяем кого мы выбрали в блоке метчей.
            MatchupModel m = (MatchupModel)matchupListBox.SelectedItem;
            double teamOneScore = 0;
            double teamTwoScore = 0;

            for (int i = 0; i < m.Entries.Count; i++)
            {
                if (i == 0)
                {
                    if (m.Entries[0].TeamCompeting != null)
                    {
                        bool scoreValid = double.TryParse(teamOneScoreValue.Text, out teamOneScore);
                        if (scoreValid)
                        {
                            m.Entries[0].Score = teamOneScore;
                        }
                        else
                        {
                            MessageBox.Show("Please enter a valid score for team 1.");
                            return;
                        }
                    }
                }

                if (i == 1)
                {
                    if (m.Entries[1].TeamCompeting != null)
                    {
                        bool scoreValid = double.TryParse(teamTwoScoreValue.Text, out teamTwoScore);
                        if (scoreValid)
                        {
                            m.Entries[1].Score = teamTwoScore;
                        }
                        else
                        {
                            MessageBox.Show("Please enter a valid score for team 2.");
                            return;
                        }
                    }
                }
            }

            //Error might be thrown here. In case if the score is the same for both teams - its wrong!!!
            try
            {
                TournamentLogic.UpdateTournamentResults(tournament);
            }
            catch (Exception ex) //lets give a name to an exception. Ex is the name.
            {
                MessageBox.Show($"The application had the following error: {ex.Message}");
                return;
            }
            
            
            //Если ввели score для команды. т.е. повявился победитель - список должен обновиться. (если нажата кнопка unplayed only)
            //Выводим на дисплей данное состояние, так как метч второго раунда еще не сформирован  
            //Мы загружаем метчи без изменения того метча, в котором определили победителя, так как иначе у нас winner будет не null и этот метч больше не отобразится.
            LoadMatchups((int)roundDropDown.SelectedItem);
        }

        private string ValidateData()
        {
            string output = "";
            
            double teamOneScore = 0;
            double teamTwoScore = 0;

            bool scoreOneValid = double.TryParse(teamOneScoreValue.Text, out teamOneScore);

            bool scoreTwoValid = double.TryParse(teamTwoScoreValue.Text, out teamTwoScore);
            
            //we can put return in the end of each if, but i prefer use else if. 
            //If one if is valid, we skip all the rest
            if (!scoreOneValid)
            {
                output = "The score One value is not a valid number.";
            }
            else if (!scoreTwoValid)
            {
                output = "The score Two value is not a valid number.";
            }
            else if (teamOneScore == 0 && teamTwoScore == 0)
            {
                output = "You didn't enter a score for either team.";
            }

            else if (teamOneScore == teamTwoScore)
            {
                output = "We do not allow ties in this application";
            } 
            return output;
        }
    }
}
