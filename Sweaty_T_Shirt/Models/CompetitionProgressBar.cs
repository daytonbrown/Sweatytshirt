using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sweaty_T_Shirt.Models
{
    public class CompetitionProgressBar
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public int UserID { get; set; }
        public int Amount { get; set; }
    }
}