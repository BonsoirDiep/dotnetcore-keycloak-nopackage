using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
    public static class TypeConverterExtension
    {
        public static byte[] ToByteArray(this string value) =>
               Convert.FromBase64String(value);
    }
    public class Token
    {
        [JsonPropertyName("access_token")]
        public string AcessToken { get; set; }


        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }


        [JsonPropertyName("id_token")]
        public string IdToken { get; set; }


        [JsonPropertyName("status")]
        public bool Status { get; set; }
    }

    class SsoVinorsoft
    {

        String clientId;
        String secret;
        String callbackUrl;
        String realmUrl;
        String pubkey = null;

        public SsoVinorsoft(String realmUrl, String clientId, String secret, String callbackUrl, String pubkey)
        {
            this.realmUrl = realmUrl;
            this.clientId = clientId;
            this.secret = secret;
            this.callbackUrl = callbackUrl;
            this.pubkey = pubkey;
        }
        public SsoVinorsoft(String realmUrl, String clientId, String secret, String callbackUrl)
        {
            this.realmUrl = realmUrl;
            this.clientId = clientId;
            this.secret = secret;
            this.callbackUrl = callbackUrl;
        }

        public bool validateToken(string token)
        {
            // https://vmsdurano.com/-net-core-3-1-signing-jwt-with-rsa/
            // https://github.com/proudmonkey/NetCoreJwtRsaDemo
            using RSA rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(this.pubkey.ToByteArray(), out _);

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    //ValidIssuer = issuer,
                    //ValidAudience = audience,

                    /*
                     * Other predefined fields that can be included in the JWT are nbf (not before),
                     * which defines a point in time in the future at which the token becomes valid, 
                     * iss (issuer), aud (audience) and iat (issued at)
                     */
                    ValidIssuer = "http://117.4.247.68:10825/realms/DemoRealm",
                    ValidAudience = "account",


                    IssuerSigningKey = new RsaSecurityKey(rsa),
                    CryptoProviderFactory = new CryptoProviderFactory()
                    {
                        CacheSignatureProviders = false
                    }
                }, out SecurityToken validatedToken);
            }
            catch(Exception _ex)
            {
                Console.WriteLine(_ex.Message);
                return false;
            }
            return true;
        }

        public async Task<Token> CreatePostApi(String pathUrl, Dictionary<string, string> data)
        {
            Token token = new Token();
            token.Status = false;
            HttpClient client = new HttpClient();
            // client.BaseAddress = new Uri(this.realmUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
            client.DefaultRequestHeaders.Add("X-Client", "vinorsoft-sso");
            String plainText = this.clientId + ":" + this.secret;
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            String encodedString = System.Convert.ToBase64String(plainTextBytes);

            client.DefaultRequestHeaders.Add("Authorization", "Basic " + encodedString);

            Console.WriteLine(this.realmUrl + pathUrl);

            var req = new HttpRequestMessage(HttpMethod.Post, this.realmUrl+ pathUrl)
            {
                Content = new FormUrlEncodedContent(data)
            };

            try
            {
                var response = await client.SendAsync(req);
                var respStream = await response.Content.ReadAsStreamAsync();

                token = await System.Text.Json.JsonSerializer.DeserializeAsync<Token>(
                    respStream,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        IgnoreNullValues = true,
                        PropertyNameCaseInsensitive = true
                    });
                token.Status = true;

            } catch (Exception _ex)
            {
                Console.WriteLine(_ex.Message);
                
                token.Status = false;
            }
            return token;
        }



        public Token GetCode(String code)
        {
            if (code == null) return new Token() { Status= false };
            var dict = new Dictionary<string, string>();
            String cbUrl = System.Text.Encodings.Web.UrlEncoder.Default.Encode(this.callbackUrl);
            dict.Add("code", code);
            dict.Add("grant_type", "authorization_code");

            //dict.Add("redirect_uri", cbUrl);
            //dict.Add("scope", "openid+email+profile");

            dict.Add("redirect_uri", this.callbackUrl);
            dict.Add("scope", "openid email profile");

            return CreatePostApi("protocol/openid-connect/token", dict)
                .GetAwaiter().GetResult();
        }

        public Token RefreshToken(String rfToken)
        {
            if (rfToken == null) return new Token() { Status = false };
            var dict = new Dictionary<string, string>();
            dict.Add("refresh_token", rfToken);
            dict.Add("grant_type", "refresh_token");

            return CreatePostApi("protocol/openid-connect/token", dict).GetAwaiter().GetResult();
        }
    }
}