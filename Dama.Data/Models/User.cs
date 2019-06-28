using Dama.Data.Interfaces;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Dama.Data.Models
{
    public class User : IdentityUser, IEntity
    {
        #region Properties
        [DisplayName("Username")]
        public override string UserName { get; set; }

        [DisplayName("Email")]
        public override string Email { get; set; }

        [DisplayName("First name")]
        public string FirstName { get; set; }

        [DisplayName("Last name")]
        public string LastName { get; set; }

        [DisplayName("Password")]
        public string Password { get; set; }

        [DisplayName("Confirm password")]
        public string PasswordConfirm { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
        [DisplayName("Registered")]
        public DateTime DateOfRegistration { get; set; }

        public List<Role> RolesCollection { get; set; }

        public bool Blocked { get; set; }
        #endregion

        public User()
        {
            DateOfRegistration = DateTime.Now;
            Blocked = false;
            RolesCollection = new List<Role>();
        }
    }
}
