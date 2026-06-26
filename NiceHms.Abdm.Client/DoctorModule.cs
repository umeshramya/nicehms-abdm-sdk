using System;

namespace NiceHms.Abdm
{
    public class DoctorModule
    {
        private readonly AbdmClient _client;

        internal DoctorModule(AbdmClient client) { _client = client; }

        /// <summary>
        /// Register a doctor with their ABDM HPR (Healthcare Professional Registry) ID.
        /// Returns the GCP FHIR ID needed for all other API calls.
        /// </summary>
        public DoctorResponse CreateDoctor(UserInfo user, string doctorPrintName, string medicalLicenseNumber,
            string specialty, string qualification, string abdmProfessionalId, string gender = "Male")
        {
            if (user == null) throw new ArgumentException("user is required");
            if (string.IsNullOrEmpty(doctorPrintName)) throw new ArgumentException("doctorPrintName is required");
            if (string.IsNullOrEmpty(medicalLicenseNumber)) throw new ArgumentException("medicalLicenseNumber is required");
            if (string.IsNullOrEmpty(abdmProfessionalId)) throw new ArgumentException("abdmProfessionalId is required");

            var json = "{\"user\":{\"email\":\"" + JsonHelper.Escape(user.Email ?? "")
                + "\",\"firstName\":\"" + JsonHelper.Escape(user.FirstName ?? "")
                + "\",\"lastName\":\"" + JsonHelper.Escape(user.LastName ?? "")
                + "\",\"mobile\":\"" + JsonHelper.Escape(user.Mobile ?? "") + "\"}"
                + ",\"doctorPrintName\":\"" + JsonHelper.Escape(doctorPrintName)
                + "\",\"medicalLicenseNumber\":\"" + JsonHelper.Escape(medicalLicenseNumber)
                + "\",\"specialty\":\"" + JsonHelper.Escape(specialty ?? "")
                + "\",\"qualification\":\"" + JsonHelper.Escape(qualification ?? "")
                + "\",\"abdmProfessionalId\":\"" + JsonHelper.Escape(abdmProfessionalId)
                + "\",\"gender\":\"" + JsonHelper.Escape(gender) + "\"}";

            var body = _client.Post("/create_doctor", json);

            return new DoctorResponse
            {
                DoctorGcpFhirId = JsonHelper.ExtractString(body, "doctorGcpFhirId")
                    ?? JsonHelper.ExtractString(body, "doctorGcpFhirId"),
                Success = JsonHelper.ExtractBool(body, "success"),
                Message = JsonHelper.ExtractString(body, "message")
            };
        }

        /// <summary>
        /// Update an existing doctor's details using their GCP FHIR ID.
        /// </summary>
        public DoctorResponse EditDoctor(string doctorGcpFhirId, string doctorPrintName = null,
            string medicalLicenseNumber = null, string specialty = null, string qualification = null,
            string abdmProfessionalId = null, string gender = null)
        {
            if (string.IsNullOrEmpty(doctorGcpFhirId)) throw new ArgumentException("doctorGcpFhirId is required");

            var parts = new System.Collections.Generic.List<string>
            {
                "\"doctorGcpFhirId\":\"" + JsonHelper.Escape(doctorGcpFhirId) + "\""
            };

            if (!string.IsNullOrEmpty(doctorPrintName))
                parts.Add("\"doctorPrintName\":\"" + JsonHelper.Escape(doctorPrintName) + "\"");
            if (!string.IsNullOrEmpty(medicalLicenseNumber))
                parts.Add("\"medicalLicenseNumber\":\"" + JsonHelper.Escape(medicalLicenseNumber) + "\"");
            if (!string.IsNullOrEmpty(specialty))
                parts.Add("\"specialty\":\"" + JsonHelper.Escape(specialty) + "\"");
            if (!string.IsNullOrEmpty(qualification))
                parts.Add("\"qualification\":\"" + JsonHelper.Escape(qualification) + "\"");
            if (!string.IsNullOrEmpty(abdmProfessionalId))
                parts.Add("\"abdmProfessionalId\":\"" + JsonHelper.Escape(abdmProfessionalId) + "\"");
            if (!string.IsNullOrEmpty(gender))
                parts.Add("\"gender\":\"" + JsonHelper.Escape(gender) + "\"");

            var json = "{" + string.Join(",", parts) + "}";
            var body = _client.Post("/edit_doctor", json);

            return new DoctorResponse
            {
                DoctorGcpFhirId = doctorGcpFhirId,
                Success = JsonHelper.ExtractBool(body, "success"),
                Message = JsonHelper.ExtractString(body, "message")
            };
        }
    }
}
