using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Data.Sqlite;

namespace ChromeCookieExtractor;

public class CookieExtractor
{
    class LocalStateDto
    {
        [JsonPropertyName("os_crypt")]
        public OsCrypt OsCrypt { get; set; }
    }

    class OsCrypt
    {
        [JsonPropertyName("encrypted_key")]
        public string EncryptedKey { get; set; }
    }

    private const string CookiesFileName = @"Default\Network\Cookies";
    private const string LocalStateFileName = "Local State";

    public CookieExtractor()
    {
    }

    public ICollection<Cookie> GetCookies(string baseFolder)
    {
        byte[] key = GetKey(baseFolder);
        ICollection<Cookie> cookies = ReadFromDb(baseFolder, key);
        return cookies;
    }
    
    private byte[] GetKey(string baseFolder)
    {
        string file = Path.Combine(baseFolder, LocalStateFileName);
        string localStateContent = File.ReadAllText(file);
        LocalStateDto localState = JsonSerializer.Deserialize<LocalStateDto>(localStateContent);
        string encryptedKey = localState?.OsCrypt?.EncryptedKey;

        var keyWithPrefix = Convert.FromBase64String(encryptedKey);
        var key = keyWithPrefix[5..];
        var masterKey = ProtectedData.Unprotect(key, null, DataProtectionScope.CurrentUser);
        return masterKey;
    }
    
    private ICollection<Cookie> ReadFromDb(string baseFolder, byte[] key)
    {
        ICollection<Cookie> result = new List<Cookie>();
        string dbFileName = Path.Combine(baseFolder, CookiesFileName);
        using (SqliteConnection connection = new SqliteConnection($"Data Source={dbFileName}"))
        {
            connection.Open();

            long expireTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            SqliteCommand command = connection.CreateCommand();
            command.CommandText =
                @"select   creation_utc,
                           host_key,
                           top_frame_site_key,
                           name,
                           value,
                           encrypted_value,
                           path,
                           expires_utc,
                           is_secure,
                           is_httponly,
                           last_access_utc,
                           has_expires,
                           is_persistent,
                           priority,
                           samesite,
                           source_scheme,
                           source_port,
                           is_same_party
                    from cookies
                    WHERE has_expires = 0 or (has_expires = 1 and expires_utc > $expireTime)
                    ";
            command.Parameters.AddWithValue("$expireTime", expireTime);
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string name = reader["name"].ToString();
                    string path = reader["path"].ToString();
                    string domain = reader["host_key"].ToString();
                    byte[] encrypted_value = (byte[])reader["encrypted_value"];


                    string value = DecryptCookie(key, encrypted_value);

                    Cookie cookie = new Cookie(name, value, path, domain);
                    result.Add(cookie);
                }
            }

            return result;
        }
    }

    private string DecryptCookie(byte[] masterKey, byte[] cookie)
    {
        byte[] nonce = cookie[3..15];
        byte[] ciphertext = cookie[15..^16];
        byte[] tag = cookie[^16..(cookie.Length)];

        byte[] resultBytes = new byte[ciphertext.Length];
        
        using AesGcm aesGcm = new AesGcm(masterKey);
        aesGcm.Decrypt(nonce, ciphertext, tag, resultBytes);
        string cookieValue = Encoding.UTF8.GetString(resultBytes);
        return cookieValue;
    }
}