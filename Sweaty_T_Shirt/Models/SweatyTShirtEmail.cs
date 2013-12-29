using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sweaty_T_Shirt.Models
{
    public class SweatyTShirtEmail
    {
        public int RecipientUserID { get; set; }
        public string RecipientEmailAddress { get; set; }
        public string SweatyTShirtFullName { get; set; }
        public string SweatyTShirtEmailAddress { get; set; }
        public string RecipientFullName { get; set; }
        public string Description { get; set; }
        public int Amount { get; set; }
    }
}