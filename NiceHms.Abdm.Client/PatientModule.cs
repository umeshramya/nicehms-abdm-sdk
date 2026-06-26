using System;

namespace NiceHms.Abdm
{
    public class PatientModule
    {
        private readonly AbdmClient _client;

        internal PatientModule(AbdmClient client) { _client = client; }

        /// <summary>
        /// Initiate ABHA OTP verification (Path A — patient already has ABHA).
        /// Returns transactionId needed for the confirm step.
        /// </summary>
        public OtpInitResponse RegisterByOtpInit(string abhaAddress, string authMode = "AADHAAR_OTP")
        {
            if (string.IsNullOrEmpty(abhaAddress))
                throw new ArgumentException("abhaAddress is required");

            var json = "{\"abhaAddress\":\"" + JsonHelper.Escape(abhaAddress)
                + "\",\"authMode\":\"" + JsonHelper.Escape(authMode) + "\"}";

            var body = _client.Post("/patient_register_abha_otp_init", json);

            return new OtpInitResponse
            {
                TransactionId = JsonHelper.ExtractString(body, "transactionId"),
                Success = JsonHelper.ExtractBool(body, "success"),
                Message = JsonHelper.ExtractString(body, "message")
            };
        }

        /// <summary>
        /// Confirm ABHA OTP. Returns full patient demographics.
        /// No need to call GetByAbhaAddress after this — demographics are in the response.
        /// </summary>
        public OtpConfirmResponse RegisterByOtpConfirm(string abhaAddress, string otp, string transactionId)
        {
            if (string.IsNullOrEmpty(abhaAddress)) throw new ArgumentException("abhaAddress is required");
            if (string.IsNullOrEmpty(otp)) throw new ArgumentException("otp is required");
            if (string.IsNullOrEmpty(transactionId)) throw new ArgumentException("transactionId is required");

            var json = "{\"otp\":\"" + JsonHelper.Escape(otp)
                + "\",\"transactionId\":\"" + JsonHelper.Escape(transactionId)
                + "\",\"abhaAddress\":\"" + JsonHelper.Escape(abhaAddress) + "\"}";

            var body = _client.Post("/patient_register_abha_otp_confirm", json);

            return new OtpConfirmResponse
            {
                AbhaAddress = JsonHelper.ExtractString(body, "abhaAddress"),
                FirstName = JsonHelper.ExtractString(body, "firstName"),
                MiddleName = JsonHelper.ExtractString(body, "middleName"),
                LastName = JsonHelper.ExtractString(body, "lastName"),
                Gender = JsonHelper.ExtractString(body, "gender"),
                Dob = JsonHelper.ExtractString(body, "dob"),
                Mobile = JsonHelper.ExtractString(body, "mobile"),
                State = JsonHelper.ExtractString(body, "state"),
                District = JsonHelper.ExtractString(body, "district"),
                Success = JsonHelper.ExtractBool(body, "success"),
                Message = JsonHelper.ExtractString(body, "message")
            };
        }

        /// <summary>
        /// Get patient by ABHA address. Used for Path B (patient created via Nice HMS white-label portal).
        /// </summary>
        public PatientByAbhaResponse GetByAbhaAddress(string abhaAddress)
        {
            if (string.IsNullOrEmpty(abhaAddress))
                throw new ArgumentException("abhaAddress is required");

            var json = "{\"abhaAddress\":\"" + JsonHelper.Escape(abhaAddress) + "\"}";

            var body = _client.Post("/patient_by_abhaAddress", json);

            return new PatientByAbhaResponse
            {
                AbhaAddress = JsonHelper.ExtractString(body, "abhaAddress"),
                FirstName = JsonHelper.ExtractString(body, "firstName"),
                MiddleName = JsonHelper.ExtractString(body, "middleName"),
                LastName = JsonHelper.ExtractString(body, "lastName"),
                Gender = JsonHelper.ExtractString(body, "gender"),
                Dob = JsonHelper.ExtractString(body, "dob"),
                Mobile = JsonHelper.ExtractString(body, "mobile"),
                State = JsonHelper.ExtractString(body, "state"),
                District = JsonHelper.ExtractString(body, "district"),
                Success = JsonHelper.ExtractBool(body, "success"),
                Message = JsonHelper.ExtractString(body, "message")
            };
        }
    }
}
