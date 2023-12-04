//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Mix_Master.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class TBLUserInfo
    {
        public int IdUs { get; set; }

        [Required(ErrorMessage ="This field is required")]
        [Display(Name ="Name")]
        public string UName { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [Display(Name = "Username")]
        [DataType(DataType.EmailAddress)]
        public string UsernameUs { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string PasswordUs { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("PasswordUs", ErrorMessage ="Confirm password doesn't match, type again!!")]
        public string RePasswordUs { get; set; }
    }
}
