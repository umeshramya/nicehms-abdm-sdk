# Nice HMS ABDM Integration SDK for .NET

A simple DLL that lets you integrate your hospital management software with India's ABDM (Ayushman Bharat Digital Mission) national health ecosystem — **without touching Postman or raw API calls.**

## Who Is This For?

If you build desktop hospital software in Visual Basic .NET or C# (WinForms, WPF, or any .NET Framework app), this SDK lets you:

- Register patients using their ABHA ID
- Submit OPD consultations, discharge summaries, and diagnostic reports
- Admit and discharge patients
- All data flows to the ABDM national registry automatically

**You don't need to know REST, JSON, OAuth, or Postman.** Add the DLL, write 4 lines, done.

## Requirements

- **Visual Studio** 2017 or later (any edition)
- **.NET Framework 4.7.2** or higher
- Your Nice HMS API credentials (email + password)
  - Don't have them? Contact `admin@nicehms.com` or call +919611560555

## Quick Start — 3 Steps

### Step 1: Add the DLL to your project

1. Copy `NiceHms.Abdm.Client.dll` into your project folder
2. In Visual Studio: **Project → Add Reference → Browse →** select the DLL
3. Add this at the top of your file:

```csharp
using NiceHms.Abdm;
```

### Step 2: Initialize the client

```csharp
// For production (real hospitals):
var abdm = new AbdmClient(
    email: "your-hospital@example.com",     // Your Nice HMS login email
    password: "your-password"               // Your Nice HMS password
);

// For testing (ABDM sandbox):
var abdm = new AbdmClient(
    email: "ndhm-pc13@nha.gov.in",
    password: "Nice@123",
    baseUrl: "https://asia-south1-psychic-city-328609.cloudfunctions.net"
);
```

### Step 3: Make API calls

```csharp
// ── Register a patient with their ABHA ────────────────────────────
var otp = abdm.Patients.RegisterByOtpInit(
    abhaAddress: "patient@abdm",
    authMode: "AADHAAR_OTP"              // or "MOBILE_OTP"
);
// Patient enters OTP → you confirm it
var patient = abdm.Patients.RegisterByOtpConfirm(
    abhaAddress: "patient@abdm",
    otp: "181540",
    transactionId: otp.TransactionId
);
Console.WriteLine(patient.FirstName + " " + patient.LastName);

// ── Create a doctor (one-time setup) ──────────────────────────────
var doc = abdm.Doctors.CreateDoctor(
    user: new UserInfo {
        Email = "doctor@hospital.com",
        FirstName = "Ramesh",
        LastName = "Kumar",
        Mobile = "9876543210"
    },
    doctorPrintName: "Dr Ramesh Kumar",
    medicalLicenseNumber: "KMC-12345",
    specialty: "Cardiology",
    qualification: "MBBS MD DM",
    abdmProfessionalId: "rameshkumar@hpr.abdm"
);
string doctorFhirId = doc.DoctorGcpFhirId;  // save this — use it everywhere

// ── Register an OPD visit ─────────────────────────────────────────
abdm.Visits.RegisterOpdPatient(
    abhaAddress: "patient@abdm",
    doctors: new[] { new DoctorRef {
        DoctorName = "Dr Ramesh Kumar",
        DoctorGcpId = doctorFhirId
    }}
);

// ── Submit OP consultation ────────────────────────────────────────
abdm.Documents.SubmitOpConsultation(
    patientIdentifier: "patient@abdm",
    chiefComplaints: "C/O Fever since 3 days",
    medicalHistory: "Known case of Type 2 DM",
    physicalExamination: "BP 130/80, PR 72/min",
    doctors: new[] { new DoctorRef {
        DoctorName = "Dr Ramesh Kumar",
        DoctorGcpId = doctorFhirId
    }},
    medicines: new[] {
        new MedicineInfo {
            Drug = "Tab Dolo 650",
            Frequency = "1-1-1",
            Instruction = "After Food",
            Duration = "For 5 Days",
            Route = "Oral"
        }
    }
);

// ── Submit a diagnostic report ────────────────────────────────────
abdm.Documents.SubmitDiagnosticReport(
    abhaAddress: "patient@abdm",
    testName: "CT Abdomen",
    category: "Radiology",
    conclusion: "Normal study",
    htmlText: "<b>Findings:</b> No abnormalities detected",
    performers: new[] { new DoctorRef {
        DoctorName = "Dr Nice Doctor",
        DoctorGcpId = "399ae751-9f60-432b-bdee-fae4eb4daa49"
    }},
    requester: new DoctorRef {
        DoctorName = "Dr Ramesh Kumar",
        DoctorGcpId = doctorFhirId
    }
);

// ── Admit a patient ───────────────────────────────────────────────
var admit = abdm.Visits.AdmitPatient(
    abhaAddress: "patient@abdm",
    doctors: new[] { new DoctorRef {
        DoctorName = "Dr Ramesh Kumar",
        DoctorGcpId = doctorFhirId
    }}
);

// ── Submit discharge summary ──────────────────────────────────────
abdm.Documents.SubmitDischargeSummary(
    abhaAddress: "patient@abdm",
    htmlText: "<p>Patient admitted with pneumonia. Treated with IV antibiotics. Discharged stable.</p>",
    doctors: new[] { new DoctorRef {
        DoctorName = "Dr Ramesh Kumar",
        DoctorGcpId = doctorFhirId
    }}
);

// ── Discharge the patient ─────────────────────────────────────────
abdm.Visits.DischargePatient(
    abhaAddress: "patient@abdm",
    encounterId: admit.EncounterId         // from AdmitPatient call
);
```

## Complete API Reference

### `AbdmClient` (constructor)

```csharp
new AbdmClient(string email, string password, string baseUrl = "...")
```

| Param | Required | Default |
|-------|----------|---------|
| `email` | Yes | — |
| `password` | Yes | — |
| `baseUrl` | No | `https://asia-south1-psychic-city-328609.cloudfunctions.net` |

All three can be changed at runtime:
```csharp
abdm.BaseUrl = "https://your-custom-url";
abdm.Email = "new-email@x.com";
abdm.Password = "new-password";
```

### `abdm.Patients` — Patient Registration

| Method | Returns | Purpose |
|--------|---------|---------|
| `RegisterByOtpInit(abhaAddress, authMode)` | `OtpInitResponse` | Start ABHA OTP verification |
| `RegisterByOtpConfirm(abhaAddress, otp, transactionId)` | `OtpConfirmResponse` | Complete OTP, get demographics |
| `GetByAbhaAddress(abhaAddress)` | `PatientByAbhaResponse` | Look up patient by ABHA address |

### `abdm.Visits` — Patient Visits

| Method | Returns | Purpose |
|--------|---------|---------|
| `RegisterOpdPatient(abhaAddress, doctors[], date?)` | `VisitResponse` | Register OPD visit |
| `AdmitPatient(abhaAddress, doctors[], admissionDate?)` | `VisitResponse` | Admit patient (IPD) |
| `DischargePatient(abhaAddress, encounterId, dischargeDate?)` | `VisitResponse` | Discharge patient |

### `abdm.Documents` — Clinical Documents

| Method | Returns | Purpose |
|--------|---------|---------|
| `SubmitDischargeSummary(abhaAddress, htmlText, doctors[], status?, date?)` | `DocumentResponse` | Submit discharge summary |
| `SubmitOpConsultation(patientId, chiefComplaints, medicalHistory, physicalExam, doctors[], medicines?, followUp?, procedure?, status?, date?)` | `DocumentResponse` | Record OP consultation |
| `SubmitDiagnosticReport(abhaAddress, testName, category, conclusion, htmlText, performers[], requester, status?, date?)` | `DocumentResponse` | Submit lab/radiology report |

### `abdm.Doctors` — Doctor Management

| Method | Returns | Purpose |
|--------|---------|---------|
| `CreateDoctor(user, doctorPrintName, licenseNumber, specialty, qualification, hprId, gender?)` | `DoctorResponse` | Register doctor with ABDM HPR |
| `EditDoctor(doctorGcpFhirId, printName?, license?, specialty?, qualification?, hprId?, gender?)` | `DoctorResponse` | Update doctor details |

### Response Objects

All responses have `.Success` (bool) and `.Message` (string) fields. Check these after every call:

```csharp
var result = abdm.Patients.RegisterByOtpInit("patient@abdm");
if (!result.Success)
{
    MessageBox.Show("Error: " + result.Message);
    return;
}
// Use result.TransactionId for the next step...
```

### Error Handling

```csharp
try
{
    var patient = abdm.Patients.RegisterByOtpConfirm(...);
}
catch (AbdmApiException ex)
{
    // HTTP error (401, 403, 500, network timeout)
    Console.WriteLine("API Error " + ex.StatusCode + ": " + ex.ResponseBody);
}
catch (ArgumentException ex)
{
    // Missing required parameter
    Console.WriteLine("Validation: " + ex.Message);
}
```

## Patient Registration — Two Paths

### Path A: Patient already has ABHA (most common)

```
RegisterByOtpInit()
  → Patient enters OTP on their phone
  → RegisterByOtpConfirm()
  → Patient is registered and demographics are returned
```

### Path B: Patient does NOT have ABHA

1. Open the Nice HMS white-label ABHA creation portal
2. Patient creates their ABHA number
3. Call `GetByAbhaAddress()` to pull them into your software

## Token & Session Management

- **Auth token refreshes automatically.** You don't need to call any login method.
- Token is cached for ~55 minutes, then auto-refreshed on the next API call.
- If a call fails with 401 (expired token), the SDK retries once with a fresh token.
- **Timeout:** 30 seconds per API call. If the server doesn't respond in time, an `AbdmApiException` is thrown.

## VB.NET Usage

Everything works identically in VB.NET:

```vb
Imports NiceHms.Abdm

Dim abdm = New AbdmClient("email@x.com", "password")
Dim otp = abdm.Patients.RegisterByOtpInit("patient@abdm", "AADHAAR_OTP")
Dim patient = abdm.Patients.RegisterByOtpConfirm("patient@abdm", txtOTP.Text, otp.TransactionId)
MessageBox.Show("Patient: " & patient.FirstName & " " & patient.LastName)
```

## Demo App

Open `DemoApp/DemoApp.sln` in Visual Studio. It's a working WinForms app with tabs for each API category — fill in fields, click Send, see the JSON response. Use it to test your credentials before integrating into your own project.

## Pricing

Each hospital gets **5,000 API calls/month** included. Beyond that, Rs 1 per call. Only M2 (HIP) REST API calls count toward this limit — M1 (ABHA creation) and M3 (HIU) calls are unlimited.

## Getting Help

| | |
|---|---|
| Email | admin@nicehms.com |
| Phone/WhatsApp | +919611560555 |
| Developer Portal | https://www.nicehms.com/developer |
| ABDM Sandbox | https://abhasbx.abdm.gov.in |

## License

Proprietary — provided to Nice HMS integrator partners. Do not redistribute without permission.
