using System.ComponentModel.DataAnnotations.Schema; 
using Microsoft.EntityFrameworkCore; 

namespace Gliese.Models;

[Table("accounts")]
[PrimaryKey(nameof(Pk))]
public class AccountTable
{
    [Column("pk")]
    public string Pk { get; set; } = "";

    [Column("createat")]
    public DateTime CreateTime { get; set; } = DateTime.MinValue;

    [Column("updateat")]
    public DateTime UpdateTime { get; set; } = DateTime.MinValue;

    [Column("account")]
    public string Account { get; set; } = "";

    [Column("password")]
    public string? Password { get; set; } = "";

    [Column("image")]
    public string? Image { get; set; } = "";

    [Column("description")]
    public string? Description { get; set; } = "";

    [Column("mail")]
    public string? Mail { get; set; } = "";

    [Column("status")]
    public int Status { get; set; } = 0;

    [Column("nickname")]
    public string? Nickname { get; set; } = ""; 

    [Column("credentials")]
    public string? Credentials { get; set; } = ""; 

    [Column("session")]
    public string? Session { get; set; } = ""; 
 
}

public class AccountMakeAssertion
{
    public string Authorization { get; set; } = "";
}

public class AccountValidate
{
    public string Name { get; set; } = "";
}