using System.ComponentModel.DataAnnotations;

namespace APIwithSQLLite.Model
{
    public class UserDetails
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; }
    }
}
