using Fido2NetLib;
using Fido2NetLib.Development;
using Fido2NetLib.Objects;
using Gliese.Models;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Gliese.Models;

public class Fido2Storage
{

    private readonly BloggingContext dataContext;
    public Fido2Storage(BloggingContext dataContext)
    {
        this.dataContext = dataContext;
    }

    public void AddCredentialToUser(Fido2User user, StoredCredential credential)
    {
        var credentialTable = new CredentialTable
        {
            Pk = Guid.NewGuid().ToString(),
            Id = Base64UrlEncoder.Encode(credential.Descriptor.Id),
            User = System.Text.Encoding.UTF8.GetString(user.Id),
            UserHandle = Base64UrlEncoder.Encode(credential.UserHandle),
            PublicKey = Base64UrlEncoder.Encode(credential.PublicKey),
            SignatureCounter = credential.SignatureCounter,
            CredType = credential.CredType,
            AaGuid = credential.AaGuid.ToString(),
            Type = (int)(credential.Descriptor.Type ?? 0),
            CreateTime = DateTime.UtcNow,
            UpdateTime = DateTime.UtcNow,
        };
        if (credential.Descriptor.Transports != null && credential.Descriptor.Transports.Length > 0)
        {
            credentialTable.Transports = string.Join(",", credential.Descriptor.Transports);
        }
        dataContext.Credentials.Add(credentialTable);
        dataContext.SaveChanges();
    }

    public StoredCredential? GetCredentialById(byte[] id)
    {
        var base64Id = Base64UrlEncoder.Encode(id);
        var model = dataContext.Credentials.Where(x => x.Id == base64Id).FirstOrDefault();
        if (model == null)
        {
            return null;
        }

        return model.ToStoredCredential();
    }

    // 目前单个人只允许单个Credential
    public List<StoredCredential> GetCredentialsByUser(Fido2User user)
    {
        var list = new List<StoredCredential>();
        var base64UserId = Base64UrlEncoder.Encode(user.Id);
        var model = dataContext.Credentials.Where(x => x.User == base64UserId).FirstOrDefault();
        if (model == null)
        {
            return list;
        }

        list.Add(model.ToStoredCredential());
        return list;
    }
    public Task<List<StoredCredential>> GetCredentialsByUserHandleAsync(byte[] userHandle, CancellationToken cancellationToken = default)
    {
        var base64UserHandle = Base64UrlEncoder.Encode(userHandle);
        var list = dataContext.Credentials.Where(x => x.UserHandle == base64UserHandle).Select(o => o.ToStoredCredential()).ToList();

        return Task.FromResult(list);
    }

    public Fido2User GetOrAddUser(string username, Func<Fido2User> addCallback)
    {
        var table = dataContext.Accounts.Where(x => x.Account == username).FirstOrDefault();
        if (table == null)
        {
            var user = addCallback();
            var account = AccountTable.FromFido2User(user);
            dataContext.Accounts.Add(account);
            dataContext.SaveChanges();
            return user;
        }
        return table.ToFido2User();
    }
    public Fido2User? GetUser(string username)
    {
        var a = dataContext.Accounts.Where(x => x.Account == username).FirstOrDefault();
        if (a == null)
        {
            return null;
        }
        return new Fido2User
        {
            DisplayName = a.Nickname,
            Name = a.Account,
            Id = Base64UrlEncoder.DecodeBytes(a.Pk)
        };
    }
    public Task<List<Fido2User>> GetUsersByCredentialIdAsync(byte[] credentialId, CancellationToken cancellationToken = default)
    {
        var list = new List<Fido2User>();
        var base64CredentialId = Base64UrlEncoder.Encode(credentialId);
        var userModel = dataContext.Credentials.Join(dataContext.Accounts, c => c.User, a => a.Pk, (c, a) => new { c, a })
            .Where(x => x.c.Id == base64CredentialId).Select(o => o.a).FirstOrDefault();
        if (userModel == null)
        {
            return Task.FromResult(list);
        }
        list.Add(userModel.ToFido2User());
        return Task.FromResult(list);
    }
    public void UpdateCounter(byte[] credentialId, uint counter)
    {
        var base64CredentialId = Base64UrlEncoder.Encode(credentialId);
        var a = dataContext.Credentials.Where(x => x.Id == base64CredentialId).FirstOrDefault();
        if (a == null)
        {
            return;
        }
        a.SignatureCounter = counter;
        dataContext.SaveChanges();
    }
}