using System.ComponentModel.DataAnnotations;

namespace AbriMail.Web.Models
{
    /// <summary>
    /// Model for composing a new email.
    /// </summary>
    public class ComposeModel
    {
        [Required]
        [Display(Name = "To (comma-separated)")]
        public string To { get; set; } = string.Empty;

        [Required]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;
    }
}
