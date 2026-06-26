using System;

namespace NiceHms.Abdm
{
    internal class AuthToken
    {
        private readonly object _lock = new object();

        public string Token { get; private set; }
        public DateTime ExpiresAt { get; private set; }

        public bool NeedsRefresh()
        {
            if (string.IsNullOrEmpty(Token)) return true;
            return DateTime.UtcNow.AddMinutes(5) >= ExpiresAt;
        }

        public void Set(string token, DateTime expiresAt)
        {
            lock (_lock)
            {
                Token = token;
                ExpiresAt = expiresAt;
            }
        }
    }
}
