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
using TrackerLibrary.DataAccess;
using TrackerLibrary.Models;


namespace TrackerUI
{
    public partial class CreatePrizeForm : Form
    {
        IPrizeRequester callingForm;



        public CreatePrizeForm(IPrizeRequester caller)
        {
            InitializeComponent();
            callingForm = caller;
        }

        private void createPrizeButton_Click(object sender, EventArgs e)
        {
            if (ValidateForm())
            {
                PrizeModel model = new PrizeModel(placeNameValue.Text, placeNumberValue.Text, prizeAmountValue.Text, prizePercentageValue.Text);

                //Now we have to save our model in the selected base

                GlobalConfig.Connection.CreatePrize(model);

                callingForm.PrizeComplete(model); //т.е. callingForm.PrizeComplete(model) = caller.PrizeComplete(model);  

                this.Close();
                //placeNameValue.Text = ""; 
                //placeNumberValue.Text = "";
                //prizeAmountValue.Text = "0";
                //prizePercentageValue.Text = "0";
            }

            else
            {
                MessageBox.Show("This form has invalid information. Please check it and try again.");
            }
        }

        private bool ValidateForm()
        {
            bool output = true; //Result of this function
            int placeNumber = 0;

            //Возвращаемое значение - boolean
            bool placeNumberValidNumber = int.TryParse(placeNumberValue.Text, out placeNumber); //Возвращает введеное число и true false
            if (placeNumberValidNumber == false) //If its not invalid number
            {
                output = false;
            }

            if (placeNumber < 1) //Если что-то ввел нуль или отрицательное чесло в поле, то это тоже ошибка
            {
                output = false;
            }

            if (placeNameValue.Text.Length == 0) //Если ничего не ввели
            {
                output = false;
            }

            decimal prizeAmout = 0;
            double prizePercentage = 0;

            bool prizeAmountValid = decimal.TryParse(prizeAmountValue.Text, out prizeAmout);
            bool prizePercentageValid = double.TryParse(prizePercentageValue.Text, out prizePercentage);

            if (prizeAmountValid == false || prizePercentageValid == false)
            {
                output = false;
            }

            if (prizeAmout <= 0 && prizePercentage <= 0)
            {
                output = false;
            }

            if (prizePercentage < 0 || prizePercentage > 100)
            {
                output = false;
            }

            return output;
        }
    }
}
