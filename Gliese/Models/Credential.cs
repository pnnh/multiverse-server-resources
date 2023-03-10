using System.ComponentModel.DataAnnotations.Schema; 
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Gliese.Models;

[Table("credentials")]
[PrimaryKey(nameof(Pk))]
public class CredentialTable
{
    [Column("pk")]
    public string Pk { get; set; } = "";
    [Column("id")]
    public string Id { get; set; } = "";
    [Column("type")]
    public int Type { get; set; } = 0;
    [Column("transports")]
    public string Transports { get; set; } = "";
    [Column("user")]
    public string User { get; set; } = "";
    [Column("public_key")]
    public string PublicKey { get; set; } = "";
    [Column("user_handle")]
    public string UserHandle { get; set; } = "";
    [Column("signature_counter")]
    public uint SignatureCounter { get; set; } = 0;
    [Column("cred_type")]
    public string CredType { get; set; } = "";
    [Column("aa_guid")]
    public string AaGuid { get; set; } = "";

    [Column("create_time")]
    public DateTime CreateTime { get; set; } = DateTime.MinValue;

    [Column("update_time")]
    public DateTime UpdateTime { get; set; } = DateTime.MinValue;

 
}
