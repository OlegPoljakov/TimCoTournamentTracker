using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary.Models;

namespace TrackerUI
{
    public interface IPrizeRequester
    {
        //its says whoever implement this interface will have one method PrizeComplete. It returns nothing but it takes the a PrizeModel 
        void PrizeComplete(PrizeModel model);
    }
}
