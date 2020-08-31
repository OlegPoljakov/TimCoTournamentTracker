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
using TrackerLibrary.DataAccess;

namespace TrackerUI
{
    public partial class CreateTeamForm : Form
    {

        private List<PersonModel> availableTeamMembers = GlobalConfig.Connection.GetPerson_All();
        private List<PersonModel> selectedTeamMembers = new List<PersonModel>();
        private ITeamRequester callingForm;

        public CreateTeamForm(ITeamRequester caller)
        {
            InitializeComponent();

            callingForm = caller;
            //CreateSampleData(); //Добавляем данные в списки 
            WierUpLists(); //Вызываем метод сразу в нашем конструкторе с выводом имени и фамилии!
        }
        

        private void CreateSampleData()
        {
            availableTeamMembers.Add(new PersonModel { FirstName = "Tim", LastName = "Corey" });
            availableTeamMembers.Add(new PersonModel { FirstName = "Sue", LastName = "Storm" });

            selectedTeamMembers.Add(new PersonModel { FirstName = "Tim", LastName = "Corey" });
            selectedTeamMembers.Add(new PersonModel { FirstName = "Bill", LastName = "Jones" });
        }

        private void WierUpLists()
        {
            selectTeamMemberDropDown.DataSource = null; //kind of refresh

            selectTeamMemberDropDown.DataSource = availableTeamMembers;
            selectTeamMemberDropDown.DisplayMember = "FullName";

            teamMembersListBox.DataSource = null;   //kind of refresh

            teamMembersListBox.DataSource = selectedTeamMembers;
            teamMembersListBox.DisplayMember = "FullName"; //Получает или задает свойство отображения для объекта ListControl.
            /*
            ComboBox1.DataSource= dt; //the data table which contains data
            Example:ComboBox1.ValueMember = "id";   // column name which you want in SelectedValue
            */
        }

        private void CreateTeamForm_Load(object sender, EventArgs e)
        {

        }

        private void selectTeamMemberLabel_Click(object sender, EventArgs e)
        {

        }

        private void createMemberButton_Click(object sender, EventArgs e)
        {
            if (ValidateForm())
            {
                PersonModel p = new PersonModel();
                p.FirstName = firstNameValue.Text;
                p.LastName = lastNameValue.Text;
                p.EmailAddress = emailValue.Text;
                p.CellphoneNumber = cellphoneValue.Text;

                GlobalConfig.Connection.CreatePerson(p); //Создали человека, записали в базу

                selectedTeamMembers.Add(p); //Добавили этого человека в список, который выведется в окне справа
                WierUpLists(); //Refresh to show that new person in the up down list

                //Обнуляем форму
                firstNameValue.Text = "";
                lastNameValue.Text = "";
                emailValue.Text = "";
                cellphoneValue.Text = "";
            }
            else
            {
                MessageBox.Show("You need to fill in all of the fields.");
            }
        }

        private bool ValidateForm()
        {
            //TODO - add validation to the form
            if (firstNameValue.Text.Length == 0)
            {
                return false;
            }

            if (lastNameValue.Text.Length == 0)
            {
                return false;
            }

            if (emailValue.Text.Length == 0)
            {
                return false;
            }

            if (cellphoneValue.Text.Length == 0)
            {
                return false;
            }

            return true;
        }

        private void addMemberButton_Click(object sender, EventArgs e)
        {
            PersonModel p = (PersonModel)selectTeamMemberDropDown.SelectedItem;

            if (p != null)
            {
                availableTeamMembers.Remove(p); //Удалили выбранного человека из выпадающего списка
                selectedTeamMembers.Add(p); //Добавили его в окно формы справа

                WierUpLists();
            }
        }

        private void removeSelectedMemberButton_Click(object sender, EventArgs e)
        {
            PersonModel p = (PersonModel)teamMembersListBox.SelectedItem; //Создаем объект того человека, которого выбрали в listBox

            if (p!=null)    //Null - это если мы никого не выбрали и жмем кнопку "удалить"
            {
                selectedTeamMembers.Remove(p);
                availableTeamMembers.Add(p);

                WierUpLists();
            }
            
        }

        private void createTeamButton_Click(object sender, EventArgs e)
        {
            TeamModel t = new TeamModel(); //Создаем экземпляр команды
            t.TeamName = teamNameValue.Text; //Берем имя команды из поля "название команды"
            t.TeamMembers = selectedTeamMembers; //Берем список тех, кто находится в поле окна справа

            GlobalConfig.Connection.CreateTeam(t); //Сохраняем команду в базе данных

            callingForm.TeamComplete(t);
            this.Close();
        }
    }
}
