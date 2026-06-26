using System;
using System.Drawing;
using System.Windows.Forms;
using NiceHms.Abdm;

namespace NiceHms.Abdm.Demo
{
    public partial class Form1 : Form
    {
        private AbdmClient _abdm;
        private TextBox _txtEmail, _txtPassword, _txtBaseUrl;
        private Button _btnConnect;
        private Label _lblStatus;
        private TabControl _tabs;
        private RichTextBox _rtfOutput;

        // Patient tab
        private TextBox _pAbha, _pOtp, _pTxnId, _pAuthMode;
        private Button _btnInitOtp, _btnConfirmOtp, _btnGetPatient;

        // Visit tab
        private TextBox _vAbha, _vDoctorName, _vDoctorId, _vEncounterId;
        private Button _btnOpd, _btnAdmit, _btnDischarge;

        // Document tab
        private TextBox _dAbha, _dTestName, _dCategory, _dConclusion, _dHtmlText;
        private TextBox _dDoctorName, _dDoctorId, _dComplaints, _dHistory, _dExam;
        private Button _btnDischargeSum, _btnOpConsult, _btnDiagReport;

        // Doctor tab
        private TextBox _drEmail, _drFirst, _drLast, _drMobile, _drPrintName;
        private TextBox _drLicense, _drSpecialty, _drQual, _drHprId, _drGender;
        private TextBox _drEditFhirId;
        private Button _btnCreateDoc, _btnEditDoc;

        public Form1()
        {
            Text = "Nice HMS ABDM SDK Demo";
            Size = new Size(950, 750);
            StartPosition = FormStartPosition.CenterScreen;
            BuildUI();
        }

        // ── Build entire UI programmatically ─────────────────────────

        private void BuildUI()
        {
            var y = 8;

            // Connection row
            var lbl = Label("Email:", 8, y + 4, 45);
            _txtEmail = TextBox("admin@nicehms.com", 55, y, 180);
            Controls.AddRange(new Control[] { lbl, _txtEmail });

            lbl = Label("Password:", 245, y + 4, 60);
            _txtPassword = TextBox("", 305, y, 140);
            _txtPassword.PasswordChar = '*';
            Controls.AddRange(new Control[] { lbl, _txtPassword });

            lbl = Label("Base URL:", 455, y + 4, 55);
            _txtBaseUrl = TextBox("https://asia-south1-psychic-city-328609.cloudfunctions.net", 510, y, 280);
            Controls.AddRange(new Control[] { lbl, _txtBaseUrl });

            _btnConnect = Button("Connect", 800, y, 80);
            _btnConnect.Click += BtnConnect_Click;
            Controls.Add(_btnConnect);

            _lblStatus = Label("Not connected", 8, y + 26, 600);
            _lblStatus.ForeColor = Color.Gray;
            Controls.Add(_lblStatus);

            // TabControl
            _tabs = new TabControl { Left = 8, Top = 55, Width = 915, Height = 430 };
            _tabs.TabPages.Add(BuildPatientTab());
            _tabs.TabPages.Add(BuildVisitTab());
            _tabs.TabPages.Add(BuildDocumentTab());
            _tabs.TabPages.Add(BuildDoctorTab());
            Controls.Add(_tabs);

            // Response output
            var rtfLbl = Label("Response:", 8, 492, 70);
            Controls.Add(rtfLbl);
            _rtfOutput = new RichTextBox
            {
                Left = 8, Top = 512, Width = 915, Height = 185,
                Font = new Font("Consolas", 9), ReadOnly = true,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.FromArgb(0, 255, 0)
            };
            Controls.Add(_rtfOutput);
        }

        // ── Tab builders ─────────────────────────────────────────────

        private TabPage BuildPatientTab()
        {
            var page = new TabPage("Patients");
            int y = 12;

            AddLabel(page, "ABHA Address:", 10, y + 4, 85);
            _pAbha = AddTextBox(page, "patient@abdm", 100, y, 180); y += 28;

            AddLabel(page, "Auth Mode:", 10, y + 4, 85);
            _pAuthMode = AddTextBox(page, "AADHAAR_OTP", 100, y, 120);
            _btnInitOtp = AddButton(page, "1. Init OTP", 230, y, 95);
            _btnInitOtp.Click += (s, e) => Run("RegisterByOtpInit", () =>
            {
                var r = _abdm.Patients.RegisterByOtpInit(_pAbha.Text, _pAuthMode.Text);
                _pTxnId.Text = r.TransactionId ?? "";
                return r;
            });
            y += 28;

            AddLabel(page, "OTP:", 10, y + 4, 85);
            _pOtp = AddTextBox(page, "", 100, y, 100);
            AddLabel(page, "Txn ID:", 210, y + 4, 50);
            _pTxnId = AddTextBox(page, "", 265, y, 200);
            _btnConfirmOtp = AddButton(page, "2. Confirm OTP", 475, y, 100);
            _btnConfirmOtp.Click += (s, e) => Run("RegisterByOtpConfirm", () =>
                _abdm.Patients.RegisterByOtpConfirm(_pAbha.Text, _pOtp.Text, _pTxnId.Text));
            y += 32;

            _btnGetPatient = AddButton(page, "Get By ABHA Address", 10, y, 150);
            _btnGetPatient.Click += (s, e) => Run("GetByAbhaAddress", () =>
                _abdm.Patients.GetByAbhaAddress(_pAbha.Text));

            return page;
        }

        private TabPage BuildVisitTab()
        {
            var page = new TabPage("Visits");
            int y = 12;

            AddLabel(page, "ABHA Address:", 10, y + 4, 85);
            _vAbha = AddTextBox(page, "patient@abdm", 100, y, 180); y += 28;

            AddLabel(page, "Dr Name:", 10, y + 4, 55);
            _vDoctorName = AddTextBox(page, "Dr Umesh Bilagi", 70, y, 150);
            AddLabel(page, "Dr GCP ID:", 230, y + 4, 65);
            _vDoctorId = AddTextBox(page, "cf4a6ab1-3f32-4b92-adc5-89489da6ca14", 300, y, 250); y += 28;

            _btnOpd = AddButton(page, "Register OPD", 10, y, 100);
            _btnOpd.Click += (s, e) => Run("RegisterOpdPatient", () =>
                _abdm.Visits.RegisterOpdPatient(_vAbha.Text, new[] { GetDoctorRef() }));
            _btnAdmit = AddButton(page, "Admit Patient", 120, y, 100);
            _btnAdmit.Click += (s, e) => Run("AdmitPatient", () =>
                _abdm.Visits.AdmitPatient(_vAbha.Text, new[] { GetDoctorRef() }));
            y += 28;

            AddLabel(page, "Encounter ID:", 10, y + 4, 80);
            _vEncounterId = AddTextBox(page, "", 95, y, 250); y += 28;

            _btnDischarge = AddButton(page, "Discharge Patient", 10, y, 120);
            _btnDischarge.Click += (s, e) => Run("DischargePatient", () =>
                _abdm.Visits.DischargePatient(_vAbha.Text, _vEncounterId.Text));

            return page;
        }

        private TabPage BuildDocumentTab()
        {
            var page = new TabPage("Documents");
            int y = 12;

            AddLabel(page, "ABHA Address:", 10, y + 4, 85);
            _dAbha = AddTextBox(page, "patient@abdm", 100, y, 180); y += 28;

            AddLabel(page, "Dr Name:", 10, y + 4, 55);
            _dDoctorName = AddTextBox(page, "Dr Umesh Bilagi", 70, y, 150);
            AddLabel(page, "Dr GCP ID:", 230, y + 4, 65);
            _dDoctorId = AddTextBox(page, "cf4a6ab1-3f32-4b92-adc5-89489da6ca14", 300, y, 250); y += 28;

            // Discharge summary
            AddLabel(page, "HTML Text:", 10, y + 4, 60);
            _dHtmlText = AddTextBox(page, "<p>Patient treated for pneumonia. Discharged stable.</p>", 75, y, 500); y += 24;

            _btnDischargeSum = AddButton(page, "Submit Discharge Summary", 10, y, 170);
            _btnDischargeSum.Click += (s, e) => Run("SubmitDischargeSummary", () =>
                _abdm.Documents.SubmitDischargeSummary(_dAbha.Text, _dHtmlText.Text, new[] { GetDocumentDoctorRef() }));
            y += 28;

            // OP Consultation
            AddLabel(page, "Complaints:", 10, y + 4, 65);
            _dComplaints = AddTextBox(page, "C/O Fever", 80, y, 200);
            AddLabel(page, "History:", 290, y + 4, 50);
            _dHistory = AddTextBox(page, "K/C/O DM", 340, y, 200);
            AddLabel(page, "Exam:", 550, y + 4, 40);
            _dExam = AddTextBox(page, "BP 130/80", 590, y, 150); y += 24;

            _btnOpConsult = AddButton(page, "Submit OP Consultation", 10, y, 160);
            _btnOpConsult.Click += (s, e) => Run("SubmitOpConsultation", () =>
                _abdm.Documents.SubmitOpConsultation(_dAbha.Text, _dComplaints.Text, _dHistory.Text,
                    _dExam.Text, new[] { GetDocumentDoctorRef() }));
            y += 28;

            // Diagnostic Report
            AddLabel(page, "Test Name:", 10, y + 4, 65);
            _dTestName = AddTextBox(page, "CT Abdomen", 80, y, 120);
            AddLabel(page, "Category:", 210, y + 4, 55);
            _dCategory = AddTextBox(page, "Radiology", 270, y, 100);
            AddLabel(page, "Conclusion:", 380, y + 4, 65);
            _dConclusion = AddTextBox(page, "Normal study", 450, y, 200); y += 24;

            _btnDiagReport = AddButton(page, "Submit Diagnostic Report", 10, y, 170);
            _btnDiagReport.Click += (s, e) => Run("SubmitDiagnosticReport", () =>
                _abdm.Documents.SubmitDiagnosticReport(_dAbha.Text, _dTestName.Text, _dCategory.Text,
                    _dConclusion.Text, _dHtmlText.Text,
                    new[] { new DoctorRef { DoctorName = _dDoctorName.Text, DoctorGcpId = _dDoctorId.Text } },
                    new DoctorRef { DoctorName = _dDoctorName.Text, DoctorGcpId = _dDoctorId.Text }));

            return page;
        }

        private TabPage BuildDoctorTab()
        {
            var page = new TabPage("Doctors");
            int y = 12;

            AddLabel(page, "Email:", 10, y + 4, 45);
            _drEmail = AddTextBox(page, "doctor@hospital.com", 60, y, 170);
            AddLabel(page, "Mobile:", 240, y + 4, 45);
            _drMobile = AddTextBox(page, "9876543210", 290, y, 130); y += 24;

            AddLabel(page, "First:", 10, y + 4, 35);
            _drFirst = AddTextBox(page, "Ramesh", 50, y, 100);
            AddLabel(page, "Last:", 160, y + 4, 35);
            _drLast = AddTextBox(page, "Kumar", 200, y, 100);
            AddLabel(page, "Gender:", 310, y + 4, 50);
            _drGender = AddTextBox(page, "Male", 365, y, 70); y += 24;

            AddLabel(page, "Print Name:", 10, y + 4, 65);
            _drPrintName = AddTextBox(page, "Dr Ramesh Kumar", 80, y, 170);
            AddLabel(page, "License:", 260, y + 4, 45);
            _drLicense = AddTextBox(page, "KMC-12345", 310, y, 120);
            AddLabel(page, "HPR ID:", 440, y + 4, 50);
            _drHprId = AddTextBox(page, "rameshkumar@hpr.abdm", 495, y, 180); y += 24;

            AddLabel(page, "Specialty:", 10, y + 4, 55);
            _drSpecialty = AddTextBox(page, "Cardiology", 70, y, 120);
            AddLabel(page, "Qualification:", 200, y + 4, 70);
            _drQual = AddTextBox(page, "MBBS MD DM", 275, y, 150); y += 28;

            _btnCreateDoc = AddButton(page, "Create Doctor", 10, y, 110);
            _btnCreateDoc.Click += (s, e) => Run("CreateDoctor", () =>
                _abdm.Doctors.CreateDoctor(
                    new UserInfo { Email = _drEmail.Text, FirstName = _drFirst.Text, LastName = _drLast.Text, Mobile = _drMobile.Text },
                    _drPrintName.Text, _drLicense.Text, _drSpecialty.Text, _drQual.Text, _drHprId.Text, _drGender.Text));
            y += 30;

            AddLabel(page, "Edit — FHIR ID:", 10, y + 4, 85);
            _drEditFhirId = AddTextBox(page, "", 100, y, 300);
            _btnEditDoc = AddButton(page, "Edit Doctor", 410, y, 110);
            _btnEditDoc.Click += (s, e) => Run("EditDoctor", () =>
                _abdm.Doctors.EditDoctor(_drEditFhirId.Text, _drPrintName.Text, _drLicense.Text,
                    _drSpecialty.Text, _drQual.Text, _drHprId.Text, _drGender.Text));

            return page;
        }

        // ── Helpers ───────────────────────────────────────────────────

        private DoctorRef GetDoctorRef()
        {
            return new DoctorRef { DoctorName = _vDoctorName.Text, DoctorGcpId = _vDoctorId.Text };
        }
        private DoctorRef GetDocumentDoctorRef()
        {
            return new DoctorRef { DoctorName = _dDoctorName.Text, DoctorGcpId = _dDoctorId.Text };
        }

        private void Run(string label, Func<object> action)
        {
            if (_abdm == null) { ShowOutput("ERROR: Click Connect first."); return; }
            try
            {
                var result = action();
                ShowOutput("[" + label + "] SUCCESS:\r\n" + FormatJson(result));
            }
            catch (AbdmApiException ex)
            {
                ShowOutput("[" + label + "] API ERROR " + ex.StatusCode + ":\r\n" + ex.ResponseBody);
            }
            catch (Exception ex)
            {
                ShowOutput("[" + label + "] ERROR:\r\n" + ex.Message);
            }
        }

        private void ShowOutput(string text)
        {
            _rtfOutput.Text = text + "\r\n\r\n" + _rtfOutput.Text;
        }

        private static string FormatJson(object obj)
        {
            if (obj == null) return "null";
            var json = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(obj);
            return json;
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                _abdm = new AbdmClient(_txtEmail.Text, _txtPassword.Text, _txtBaseUrl.Text);
                _abdm.Patients.RegisterByOtpInit("test@abdm");
                _lblStatus.Text = "Connected — token OK";
                _lblStatus.ForeColor = Color.Green;
                ShowOutput("[Connect] SUCCESS — token obtained. DLL is working.");
            }
            catch (AbdmApiException ex)
            {
                _lblStatus.Text = "Auth failed (HTTP " + ex.StatusCode + ") — check credentials";
                _lblStatus.ForeColor = Color.Red;
                ShowOutput("[Connect] AUTH FAILED " + ex.StatusCode + ":\r\n" + ex.ResponseBody);
            }
            catch (Exception ex)
            {
                _lblStatus.Text = "Error: " + ex.Message;
                _lblStatus.ForeColor = Color.Red;
                ShowOutput("[Connect] ERROR:\r\n" + ex);
            }
        }

        // ── UI factory methods ────────────────────────────────────────

        private static Label Label(string text, int x, int y, int w)
        {
            return new Label { Text = text, Left = x, Top = y, Width = w, TextAlign = ContentAlignment.MiddleLeft };
        }
        private static TextBox TextBox(string text, int x, int y, int w)
        {
            return new TextBox { Text = text, Left = x, Top = y, Width = w };
        }
        private static Button Button(string text, int x, int y, int w)
        {
            return new Button { Text = text, Left = x, Top = y, Width = w, Height = 22 };
        }
        private void AddLabel(TabPage page, string text, int x, int y, int w)
        {
            page.Controls.Add(Label(text, x, y, w));
        }
        private TextBox AddTextBox(TabPage page, string text, int x, int y, int w)
        {
            var tb = TextBox(text, x, y, w);
            page.Controls.Add(tb);
            return tb;
        }
        private Button AddButton(TabPage page, string text, int x, int y, int w)
        {
            var btn = Button(text, x, y, w);
            page.Controls.Add(btn);
            return btn;
        }
    }
}
