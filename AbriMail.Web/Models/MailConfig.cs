using System.ComponentModel.DataAnnotations;

namespace AbriMail.Web.Models
{
    /// <summary>
    /// Represents email account configuration for IMAP and SMTP.
    /// </summary>
    public class MailConfig
    {
        [Required]
        [Display(Name = "IMAP Server")]
        public string ImapServer { get; set; } = string.Empty;

        [Required]
        [Range(1, 65535)]
        [Display(Name = "IMAP Port")]
        public int ImapPort { get; set; } = 993;

        [Required]
        [Display(Name = "IMAP Username")]
        public string ImapUsername { get; set; } = string.Empty;

        [Required]
        [Display(Name = "IMAP Password")]
        [DataType(DataType.Password)]
        public string ImapPassword { get; set; } = string.Empty;

        [Required]
        [Display(Name = "SMTP Server")]
        public string SmtpServer { get; set; } = string.Empty;

        [Required]
        [Range(1, 65535)]
        [Display(Name = "SMTP Port")]
        public int SmtpPort { get; set; } = 465;

        [Required]
        [Display(Name = "SMTP Username")]
        public string SmtpUsername { get; set; } = string.Empty;

        [Required]
        [Display(Name = "SMTP Password")]
        [DataType(DataType.Password)]
        public string SmtpPassword { get; set; } = string.Empty;
    }
}
