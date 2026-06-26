using System;
using System.Collections.Generic;

namespace NiceHms.Abdm
{
    // ── Auth ──────────────────────────────────────────────────────────

    public class AuthResponse
    {
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    // ── Patient ───────────────────────────────────────────────────────

    public class OtpInitResponse
    {
        public string TransactionId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class OtpConfirmResponse
    {
        public string AbhaAddress { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Dob { get; set; }
        public string Mobile { get; set; }
        public string State { get; set; }
        public string District { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class PatientByAbhaResponse
    {
        public string AbhaAddress { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Dob { get; set; }
        public string Mobile { get; set; }
        public string State { get; set; }
        public string District { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    // ── Visit ─────────────────────────────────────────────────────────

    public class VisitResponse
    {
        public string EncounterId { get; set; }
        public string Status { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    // ── Doctor ────────────────────────────────────────────────────────

    public class DoctorResponse
    {
        public string DoctorGcpFhirId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    // ── Document ──────────────────────────────────────────────────────

    public class DocumentResponse
    {
        public string DocumentId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    // ── Supporting types ──────────────────────────────────────────────

    public class DoctorRef
    {
        public string DoctorName { get; set; }
        public string DoctorGcpId { get; set; }
    }

    public class PerformerRef
    {
        public string DoctorName { get; set; }
        public string DoctorGcpId { get; set; }
    }

    public class FollowUpInfo
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Comment { get; set; }
    }

    public class ProcedureInfo
    {
        public string ProcedureName { get; set; }
        public string ProcedureDescription { get; set; }
        public DateTime Date { get; set; }
    }

    public class MedicineInfo
    {
        public string Drug { get; set; }
        public string Frequency { get; set; }
        public string Instruction { get; set; }
        public string Duration { get; set; }
        public string Route { get; set; }
    }

    public class UserInfo
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Mobile { get; set; }
    }
}
