using System;

namespace NiceHms.Abdm
{
    public class VisitModule
    {
        private readonly AbdmClient _client;

        internal VisitModule(AbdmClient client) { _client = client; }

        /// <summary>
        /// Register an OPD (outpatient) visit for a patient.
        /// </summary>
        public VisitResponse RegisterOpdPatient(string abhaAddress, DoctorRef[] doctors, DateTime? date = null)
        {
            if (string.IsNullOrEmpty(abhaAddress)) throw new ArgumentException("abhaAddress is required");
            if (doctors == null || doctors.Length == 0) throw new ArgumentException("At least one doctor is required");

            var visitDate = date ?? DateTime.Now;
            var doctorJson = BuildDoctorArray(doctors);

            var json = "{\"abhaAddress\":\"" + JsonHelper.Escape(abhaAddress)
                + "\",\"doctorDetails\":" + doctorJson
                + ",\"date\":\"" + JsonHelper.ToIso(visitDate) + "\"}";

            var body = _client.Post("/opd_patient", json);

            return new VisitResponse
            {
                EncounterId = JsonHelper.ExtractString(body, "encounterId"),
                Status = JsonHelper.ExtractString(body, "status"),
                Success = JsonHelper.ExtractBool(body, "success"),
                Message = JsonHelper.ExtractString(body, "message")
            };
        }

        /// <summary>
        /// Admit a patient for IPD (inpatient) care.
        /// </summary>
        public VisitResponse AdmitPatient(string abhaAddress, DoctorRef[] doctors, DateTime? admissionDate = null)
        {
            if (string.IsNullOrEmpty(abhaAddress)) throw new ArgumentException("abhaAddress is required");
            if (doctors == null || doctors.Length == 0) throw new ArgumentException("At least one doctor is required");

            var doa = admissionDate ?? DateTime.Now;
            var doctorJson = BuildDoctorArray(doctors);

            var json = "{\"doctorDetails\":" + doctorJson
                + ",\"doa\":\"" + JsonHelper.ToIso(doa)
                + "\",\"abhaAddress\":\"" + JsonHelper.Escape(abhaAddress) + "\"}";

            var body = _client.Post("/admit_patient", json);

            return new VisitResponse
            {
                EncounterId = JsonHelper.ExtractString(body, "encounterId"),
                Status = JsonHelper.ExtractString(body, "status"),
                Success = JsonHelper.ExtractBool(body, "success"),
                Message = JsonHelper.ExtractString(body, "message")
            };
        }

        /// <summary>
        /// Discharge a patient. Requires the encounterId from AdmitPatient.
        /// </summary>
        public VisitResponse DischargePatient(string abhaAddress, string encounterId, DateTime? dischargeDate = null)
        {
            if (string.IsNullOrEmpty(abhaAddress)) throw new ArgumentException("abhaAddress is required");
            if (string.IsNullOrEmpty(encounterId)) throw new ArgumentException("encounterId is required (returned by AdmitPatient)");

            var dod = dischargeDate ?? DateTime.Now;

            var json = "{\"dod\":\"" + JsonHelper.ToIso(dod)
                + "\",\"abhaAddress\":\"" + JsonHelper.Escape(abhaAddress)
                + "\",\"encounterId\":\"" + JsonHelper.Escape(encounterId) + "\"}";

            // This is a PUT endpoint
            var body = _client.Put("/discharge_patient", json);

            return new VisitResponse
            {
                EncounterId = encounterId,
                Status = JsonHelper.ExtractString(body, "status"),
                Success = JsonHelper.ExtractBool(body, "success"),
                Message = JsonHelper.ExtractString(body, "message")
            };
        }

        private static string BuildDoctorArray(DoctorRef[] doctors)
        {
            var parts = new string[doctors.Length];
            for (int i = 0; i < doctors.Length; i++)
            {
                parts[i] = "{\"doctorName\":\"" + JsonHelper.Escape(doctors[i].DoctorName)
                    + "\",\"doctorGcpId\":\"" + JsonHelper.Escape(doctors[i].DoctorGcpId) + "\"}";
            }
            return "[" + string.Join(",", parts) + "]";
        }
    }
}
