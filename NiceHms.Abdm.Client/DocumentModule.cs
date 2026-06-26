using System;

namespace NiceHms.Abdm
{
    public class DocumentModule
    {
        private readonly AbdmClient _client;

        internal DocumentModule(AbdmClient client) { _client = client; }

        /// <summary>
        /// Submit a discharge summary for an admitted patient.
        /// </summary>
        public DocumentResponse SubmitDischargeSummary(string abhaAddress, string htmlText, DoctorRef[] doctors,
            string status = "final", DateTime? date = null)
        {
            if (string.IsNullOrEmpty(abhaAddress)) throw new ArgumentException("abhaAddress is required");
            if (string.IsNullOrEmpty(htmlText)) throw new ArgumentException("htmlText is required");
            if (doctors == null || doctors.Length == 0) throw new ArgumentException("At least one doctor is required");

            var docDate = date ?? DateTime.Now;
            var doctorJson = BuildDoctorArray(doctors);

            var json = "{\"abhaAddress\":\"" + JsonHelper.Escape(abhaAddress)
                + "\",\"text\":\"" + JsonHelper.Escape(htmlText)
                + "\",\"doctorDetails\":" + doctorJson
                + ",\"status\":\"" + JsonHelper.Escape(status)
                + "\",\"date\":\"" + JsonHelper.ToIso(docDate) + "\"}";

            var body = _client.Post("/discharge_summary", json);

            return ParseResult(body);
        }

        /// <summary>
        /// Record an OP consultation note with full clinical details.
        /// </summary>
        public DocumentResponse SubmitOpConsultation(string patientIdentifier, string chiefComplaints,
            string medicalHistory, string physicalExamination, DoctorRef[] doctors,
            MedicineInfo[] medicines = null, FollowUpInfo followUp = null,
            ProcedureInfo opdProcedure = null, string status = "final", DateTime? date = null)
        {
            if (string.IsNullOrEmpty(patientIdentifier)) throw new ArgumentException("patientIdentifier (abhaAddress or patientId) is required");
            if (doctors == null || doctors.Length == 0) throw new ArgumentException("At least one doctor is required");

            var docDate = date ?? DateTime.Now;

            // Determine if this is an abhaAddress or a numeric patientId
            string idField;
            string idValue;
            int pid;
            if (int.TryParse(patientIdentifier, out pid))
            {
                idField = "\"patientId\":";
                idValue = pid.ToString();
            }
            else
            {
                idField = "\"abhaAddress\":\"";
                idValue = JsonHelper.Escape(patientIdentifier) + "\"";
            }

            var parts = new System.Collections.Generic.List<string>
            {
                idField + idValue,
                "\"chiefComplaints\":\"" + JsonHelper.Escape(chiefComplaints ?? "") + "\"",
                "\"medicalHistory\":\"" + JsonHelper.Escape(medicalHistory ?? "") + "\"",
                "\"physicalExamination\":\"" + JsonHelper.Escape(physicalExamination ?? "") + "\"",
                "\"doctorDetails\":" + BuildDoctorArray(doctors),
                "\"status\":\"" + JsonHelper.Escape(status) + "\"",
                "\"date\":\"" + JsonHelper.ToIso(docDate) + "\""
            };

            if (followUp != null)
            {
                parts.Add("\"followUp\":{\"startDate\":\"" + JsonHelper.ToIso(followUp.StartDate)
                    + "\",\"endDate\":\"" + JsonHelper.ToIso(followUp.EndDate)
                    + "\",\"comment\":\"" + JsonHelper.Escape(followUp.Comment ?? "") + "\"}");
            }

            if (opdProcedure != null)
            {
                parts.Add("\"opdProcedure\":{\"procedureName\":\"" + JsonHelper.Escape(opdProcedure.ProcedureName)
                    + "\",\"procedureDescription\":\"" + JsonHelper.Escape(opdProcedure.ProcedureDescription ?? "")
                    + "\",\"date\":\"" + JsonHelper.ToIso(opdProcedure.Date) + "\"}");
            }

            if (medicines != null && medicines.Length > 0)
            {
                var medParts = new string[medicines.Length];
                for (int i = 0; i < medicines.Length; i++)
                {
                    medParts[i] = "{\"drug\":\"" + JsonHelper.Escape(medicines[i].Drug ?? "")
                        + "\",\"frequency\":\"" + JsonHelper.Escape(medicines[i].Frequency ?? "")
                        + "\",\"instruction\":\"" + JsonHelper.Escape(medicines[i].Instruction ?? "")
                        + "\",\"duration\":\"" + JsonHelper.Escape(medicines[i].Duration ?? "")
                        + "\",\"route\":\"" + JsonHelper.Escape(medicines[i].Route ?? "") + "\"}";
                }
                parts.Add("\"medicines\":[" + string.Join(",", medParts) + "]");
            }

            var json = "{" + string.Join(",", parts) + "}";
            var body = _client.Post("/op_consultation", json);

            return ParseResult(body);
        }

        /// <summary>
        /// Submit a diagnostic report for a patient.
        /// </summary>
        public DocumentResponse SubmitDiagnosticReport(string abhaAddress, string testName, string category,
            string conclusion, string htmlText, DoctorRef[] performers, DoctorRef requester,
            string status = "final", DateTime? date = null)
        {
            if (string.IsNullOrEmpty(abhaAddress)) throw new ArgumentException("abhaAddress is required");
            if (string.IsNullOrEmpty(testName)) throw new ArgumentException("testName is required");
            if (performers == null || performers.Length == 0) throw new ArgumentException("At least one performer is required");
            if (requester == null) throw new ArgumentException("requester is required");

            var reportDate = date ?? DateTime.Now;

            var performerJson = "[";
            for (int i = 0; i < performers.Length; i++)
            {
                if (i > 0) performerJson += ",";
                performerJson += "{\"doctorName\":\"" + JsonHelper.Escape(performers[i].DoctorName)
                    + "\",\"doctorGcpId\":\"" + JsonHelper.Escape(performers[i].DoctorGcpId) + "\"}";
            }
            performerJson += "]";

            var requesterJson = "{\"doctorName\":\"" + JsonHelper.Escape(requester.DoctorName)
                + "\",\"doctorGcpId\":\"" + JsonHelper.Escape(requester.DoctorGcpId) + "\"}";

            var json = "{\"abhaAddress\":\"" + JsonHelper.Escape(abhaAddress)
                + "\",\"text\":\"" + JsonHelper.Escape(htmlText ?? "")
                + "\",\"performer\":" + performerJson
                + ",\"requester\":" + requesterJson
                + ",\"status\":\"" + JsonHelper.Escape(status)
                + "\",\"date\":\"" + JsonHelper.ToIso(reportDate)
                + "\",\"testName\":\"" + JsonHelper.Escape(testName)
                + "\",\"category\":\"" + JsonHelper.Escape(category)
                + "\",\"conclusion\":\"" + JsonHelper.Escape(conclusion ?? "") + "\"}";

            var body = _client.Post("/diagnostic_report", json);

            return ParseResult(body);
        }

        private static DocumentResponse ParseResult(string body)
        {
            return new DocumentResponse
            {
                DocumentId = JsonHelper.ExtractString(body, "documentId")
                    ?? JsonHelper.ExtractString(body, "id"),
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
