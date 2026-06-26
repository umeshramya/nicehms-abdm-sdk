using System;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace NiceHms.Abdm
{
    public class AbdmClient
    {
        private static readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        private readonly AuthToken _auth = new AuthToken();

        public string BaseUrl { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public PatientModule Patients { get; private set; }
        public VisitModule Visits { get; private set; }
        public DocumentModule Documents { get; private set; }
        public DoctorModule Doctors { get; private set; }

        public AbdmClient(string email, string password, string baseUrl = "https://asia-south1-psychic-city-328609.cloudfunctions.net")
        {
            if (email == null) throw new ArgumentNullException("email");
            if (password == null) throw new ArgumentNullException("password");
            if (baseUrl == null) throw new ArgumentNullException("baseUrl");
            Email = email;
            Password = password;
            BaseUrl = baseUrl;

            Patients = new PatientModule(this);
            Visits = new VisitModule(this);
            Documents = new DocumentModule(this);
            Doctors = new DoctorModule(this);
        }

        // ── Internal HTTP helpers ──────────────────────────────────────

        internal string Post(string path, string jsonBody)
        {
            return Send("POST", path, jsonBody);
        }

        internal string Put(string path, string jsonBody)
        {
            return Send("PUT", path, jsonBody);
        }

        private string Send(string method, string path, string jsonBody)
        {
            EnsureAuth();

            var request = new HttpRequestMessage(new HttpMethod(method), BaseUrl + path)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Authorization", "Bearer " + _auth.Token);

            HttpResponseMessage response;
            try
            {
                response = _http.SendAsync(request).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw new AbdmApiException(0, "Network error: " + ex.Message);
            }

            var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
            {
                // Token expired — refresh and retry once
                if ((int)response.StatusCode == 401)
                {
                    RefreshToken();
                    request = new HttpRequestMessage(new HttpMethod(method), BaseUrl + path)
                    {
                        Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
                    };
                    request.Headers.Add("Authorization", "Bearer " + _auth.Token);
                    response = _http.SendAsync(request).GetAwaiter().GetResult();
                    body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    if (!response.IsSuccessStatusCode)
                        throw new AbdmApiException((int)response.StatusCode, body);
                }
                else
                {
                    throw new AbdmApiException((int)response.StatusCode, body);
                }
            }

            return body;
        }

        private void EnsureAuth()
        {
            if (_auth.NeedsRefresh())
                RefreshToken();
        }

        private void RefreshToken()
        {
            // Authenticate (no Bearer token for auth request itself)
            var authJson = "{\"email\":\"" + JsonHelper.Escape(Email)
                + "\",\"password\":\"" + JsonHelper.Escape(Password)
                + "\",\"returnSecureToken\":true}";

            var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl + "/auth_token")
            {
                Content = new StringContent(authJson, Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response;
            try
            {
                response = _http.SendAsync(request).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw new AbdmApiException(0, "Auth network error: " + ex.Message);
            }

            var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
                throw new AbdmApiException((int)response.StatusCode, "Auth failed: " + body);

            // Firebase JWT format: {"idToken":"...", "expiresIn":"3600", ...}
            var idToken = JsonHelper.ExtractString(body, "idToken");
            if (string.IsNullOrEmpty(idToken))
                idToken = JsonHelper.ExtractString(body, "id_token");
            if (string.IsNullOrEmpty(idToken))
                throw new AbdmApiException(0, "Auth response missing token: " + body);

            // Firebase tokens have ~1 hour expiry. We expire 5 min early for safety.
            _auth.Set(idToken, DateTime.UtcNow.AddMinutes(55));
        }
    }

    // ── Minimal JSON helper (no external deps) ─────────────────────────

    internal static class JsonHelper
    {
        public static string Escape(string value)
        {
            if (value == null) return "";
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"")
                       .Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
        }

        public static string ExtractString(string json, string key)
        {
            var search = "\"" + key + "\"";
            var idx = json.IndexOf(search, StringComparison.Ordinal);
            if (idx < 0) return null;

            var colonIdx = json.IndexOf(':', idx + search.Length);
            if (colonIdx < 0) return null;

            var valueStart = colonIdx + 1;
            while (valueStart < json.Length && (json[valueStart] == ' ' || json[valueStart] == '"'))
                valueStart++;
            if (valueStart >= json.Length) return null;
            if (json[valueStart - 1] != '"') return null; // not a string value

            var valueEnd = json.IndexOf('"', valueStart);
            if (valueEnd < 0) return null;

            return json.Substring(valueStart, valueEnd - valueStart);
        }

        public static int ExtractInt(string json, string key)
        {
            var s = ExtractString(json, key);
            int v;
            if (s != null && int.TryParse(s, out v)) return v;
            // try unquoted numeric
            var search = "\"" + key + "\"";
            var idx = json.IndexOf(search, StringComparison.Ordinal);
            if (idx < 0) return 0;
            var colonIdx = json.IndexOf(':', idx + search.Length);
            if (colonIdx < 0) return 0;
            var valStart = colonIdx + 1;
            while (valStart < json.Length && (json[valStart] == ' ' || json[valStart] == '"'))
                valStart++;
            var valEnd = valStart;
            while (valEnd < json.Length && (char.IsDigit(json[valEnd]) || json[valEnd] == '-'))
                valEnd++;
            int n;
            if (int.TryParse(json.Substring(valStart, valEnd - valStart), out n)) return n;
            return 0;
        }

        public static bool ExtractBool(string json, string key)
        {
            var search = "\"" + key + "\"";
            var idx = json.IndexOf(search, StringComparison.Ordinal);
            if (idx < 0) return false;
            return json.IndexOf("true", idx, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static string ToIso(DateTime dt)
        {
            return dt.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        }

        public static string Dob(DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd");
        }
    }
}
