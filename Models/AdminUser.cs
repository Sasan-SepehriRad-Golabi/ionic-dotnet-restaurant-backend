using System.ComponentModel.DataAnnotations;

public class AdminUser
{
    [Required]
    [DataType(DataType.Text)]
    public string userName { get; set; } = "";
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

}