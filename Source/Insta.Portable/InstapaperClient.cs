using AsyncOAuth;
using Insta.Portable.Models;
using Newtonsoft.Json;
using PCLCrypto;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Insta.Portable
{
    public partial class InstapaperClient : IInstapaperClient
    {
        private readonly string _consumerKey;
        private readonly string _consumerSecret;
        private const string BaseUrl = "https://www.instapaper.com/api";
        private const string AuthUrl = BaseUrl + "/1.1/oauth";

        private InstapaperClient()
        {
            OAuthUtility.ComputeHash = (key, buffer) =>
            {
                var crypt = WinRTCrypto.MacAlgorithmProvider.OpenAlgorithm(MacAlgorithm.HmacSha1);
                var keyBuffer = WinRTCrypto.CryptographicBuffer.CreateFromByteArray(key);
                var cryptKey = crypt.CreateKey(keyBuffer);

                var dataBuffer = WinRTCrypto.CryptographicBuffer.CreateFromByteArray(buffer);
                var signBuffer = WinRTCrypto.CryptographicEngine.Sign(cryptKey, dataBuffer);

                byte[] value;
                WinRTCrypto.CryptographicBuffer.CopyToByteArray(signBuffer, out value);
                return value;
            };
        }

        public InstapaperClient(string consumerKey, string consumerSecret, string oauthToken = null, string oauthSecret = null)
            : this()
        {
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;

            if (!string.IsNullOrEmpty(oauthToken) && !string.IsNullOrEmpty(oauthSecret))
            {
                AccessToken = new AccessToken(oauthToken, oauthSecret);
            }
        }

        public AccessToken AccessToken { get; set; }

        /// <summary>
        /// Get an AccessToken for the user with the given email and password.
        /// </summary>
        /// <param name="emailAddress">The email address for the user.</param>
        /// <param name="password">The password for the user.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The AccessToken if the user is authenticated. Null if the user is not authenticated.</returns>
        public async Task<AccessToken> GetAuthTokenAsync(string emailAddress, string password, CancellationToken cancellationToken = default(CancellationToken))
        {
            //
            // Acquire an access token
            //

            const string authUrl = AuthUrl + "/access_token";

            var parameters = new Dictionary<string, string>
            {
                { "x_auth_username", emailAddress },
                {"x_auth_password", password},
                {"x_auth_mode","client_auth"}
            };

            var handler = new OAuthMessageHandler(_consumerKey, _consumerSecret);
            var client = new HttpClient(handler);

            var response = await client.PostAsync(authUrl, new FormUrlEncodedContent(parameters), cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode == false) return null;

            var tokenBase = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var splitted = tokenBase.Split('&').Select(s => s.Split('=')).ToLookup(xs => xs[0], xs => xs[1]);
            AccessToken = new AccessToken(splitted["oauth_token"].First(), splitted["oauth_token_secret"].First());
            return AccessToken;
        }

        public async Task<InstaResponse<User>> VerifyUserAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            const string url = BaseUrl + "/1.1/account/verify_credentials";

            var response = await GetResponse(url, new List<KeyValuePair<string, string>>(), cancellationToken).ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var result = ProcessResponse<List<User>>(json);
            return result.Error == null
                ? new InstaResponse<User> { Response = result.Response.FirstOrDefault() }
                : new InstaResponse<User> { Error = result.Error };
        }

        private static InstaResponse<TReturnType> ProcessResponse<TReturnType>(string json) where TReturnType : class
        {
            if (string.IsNullOrEmpty(json))
            {
                return new InstaResponse<TReturnType> { Error = new Error { ErrorCode = 0000, Message = "API response contained no information", Type = "error" } };
            }

            if (json.Contains("error_code"))
            {
                var error = JsonConvert.DeserializeObject<List<Error>>(json);
                return new InstaResponse<TReturnType> { Error = error.FirstOrDefault() };
            }

            var response = JsonConvert.DeserializeObject<TReturnType>(json);
            return new InstaResponse<TReturnType> { Response = response };
        }
    }
}
