using System.ComponentModel.DataAnnotations.Schema;

namespace Sweaty_T_Shirt.Models
{
    public class BaseModel
    {
        /// <summary>
        /// The name of the action that the controller redirects to after the CRUD activity is completed.
        /// </summary>
        [NotMapped]
        public string RedirectToAction { get; set; }

        /// <summary>
        /// The name of the controller that the controller redirects to after the CRUD activity is completed.
        /// </summary>
        [NotMapped]
        public string RedirectToController { get; set; }

    }
}
