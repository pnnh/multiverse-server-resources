using System.ComponentModel.DataAnnotations.Schema;
using Fido2NetLib;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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

    [Column("counter")]
    public uint? Counter { get; set; } = 0;

    public Fido2User ToFido2User()
    {
        return new Fido2User
        {
            DisplayName = this.Nickname,
            Name = this.Account,
            Id = System.Text.Encoding.UTF8.GetBytes(this.Pk),
        };
    }

    public static AccountTable FromFido2User(Fido2User user)
    {
        var table = new AccountTable();
        table.Pk =  System.Text.Encoding.UTF8.GetString(user.Id);//Guid.NewGuid().ToString();
        table.Nickname = user.DisplayName;
        table.Account = user.Name; 
        table.CreateTime = DateTime.UtcNow;
        table.UpdateTime = DateTime.UtcNow;
        return table;
    }
}

public class AccountMakeAssertion
{
    public string Authorization { get; set; } = "";
}

public class AccountValidate
{
    public string Name { get; set; } = "";
}