﻿//******************************************************************************************************
//  DataQualityReportViewModel.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  03/06/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using GSF.Console;
using GSF.ServiceProcess;
using GSF.TimeSeries.UI.Commands;
using GSF.TimeSeries.UI.UserControls;
using Microsoft.Win32;

namespace GSF.TimeSeries.UI.ViewModels
{
    /// <summary>
    /// View model for the <see cref="DataQualityReportUserControl"/>.
    /// </summary>
    public class DataQualityReportViewModel : ViewModelBase, IDisposable
    {
        #region [ Members ]

        // Nested Types

        /// <summary>
        /// Represents a generated report that is available for download from the service.
        /// </summary>
        public class AvailableReport
        {
            #region [ Members ]

            // Fields
            private DataQualityReportViewModel m_parent;

            private string m_date;
            private string m_status;
            private ICommand m_getReportCommand;

            #endregion

            #region [ Constructors ]

            /// <summary>
            /// Creates a new instance of the <see cref="AvailableReport"/> class.
            /// </summary>
            internal AvailableReport(DataQualityReportViewModel parent)
            {
                m_parent = parent;
            }

            #endregion

            #region [ Properties ]

            /// <summary>
            /// Gets or sets the name of the report.
            /// </summary>
            public string Date
            {
                get
                {
                    return m_date;
                }
                set
                {
                    m_date = value;
                }
            }

            /// <summary>
            /// Gets or sets the status of the report.
            /// </summary>
            public string Status
            {
                get
                {
                    return m_status;
                }
                set
                {
                    m_status = value;
                }
            }

            /// <summary>
            /// Gets the command that retrieves the report from the service.
            /// </summary>
            public ICommand GetReportCommand
            {
                get
                {
                    return m_getReportCommand ?? (m_getReportCommand = new RelayCommand(GetReport, () => true));
                }
            }

            #endregion

            #region [ Methods ]

            private void GetReport()
            {
                DateTime reportDate;

                if (DateTime.TryParse(Date, out reportDate))
                    m_parent.GetReport(reportDate);
            }

            #endregion
        }

        // Fields
        private Visibility m_connectivityMessageVisibility;
        private bool m_serviceConnected;
        private bool m_reportingEnabled;

        private string m_reportTitle;
        private string m_reportsList;
        private string m_reportingConfiguration;
        private string m_originalReportLocation;
        private string m_reportLocation;
        private string m_reportGenerationTimeText;
        private DateTime m_originalReportGenerationTime;
        private DateTime m_reportGenerationTime;
        private DateTime m_reportDate;
        private double m_originalIdleReportLifetime;
        private double m_idleReportLifetime;

        private ObservableCollection<AvailableReport> m_availableReports;
        private ObservableCollection<string> m_pendingReports;
        private byte[] m_reportData;

        private string m_responseMessage;
        private ManualResetEventSlim m_responseComplete;
        private ICommand m_changeReportingEnabledCommand;
        private ICommand m_listReportsCommand;
        private ICommand m_generateReportCommand;
        private ICommand m_applyReportingConfigCommand;

        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="DataQualityReportViewModel"/> class.
        /// </summary>
        public DataQualityReportViewModel()
        {
            ReportGenerationTime = new DateTime(1, 1, 1, 2, 0, 0);
            m_originalReportGenerationTime = m_reportGenerationTime;

            ReportLocation = "Reports";
            OriginalReportLocation = m_reportLocation;

            IdleReportLifetime = 14.0D;
            OriginalIdleReportLifetime = m_idleReportLifetime;

            ReportDate = DateTime.Now - TimeSpan.FromDays(1);
            m_availableReports = new ObservableCollection<AvailableReport>();
            m_pendingReports = new ObservableCollection<string>();
            m_responseComplete = new ManualResetEventSlim(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a flag to indicate whether the service is connected.
        /// </summary>
        public bool ServiceConnected
        {
            get
            {
                return m_serviceConnected;
            }
            set
            {
                m_serviceConnected = value;
                m_connectivityMessageVisibility = value ? Visibility.Collapsed : Visibility.Visible;
                OnPropertyChanged("ServiceConnected");
                OnPropertyChanged("ConnectivityMessageVisibility");

                if (m_serviceConnected)
                {
                    GetSchedules();
                    GetReportingConfiguration();
                    ListReports();
                }
            }
        }

        /// <summary>
        /// Gets a value that determines whether the connectivity message should be displayed.
        /// </summary>
        public Visibility ConnectivityMessageVisibility
        {
            get
            {
                return m_connectivityMessageVisibility;
            }
        }

        /// <summary>
        /// Gets or sets a flag that indicates whether reporting is currently enabled.
        /// </summary>
        public bool ReportingEnabled
        {
            get
            {
                return m_reportingEnabled;
            }
            set
            {
                m_reportingEnabled = value;
                OnPropertyChanged("ReportingEnabled");
            }
        }

        /// <summary>
        /// Gets or sets the original report location since the last
        /// time settings were applied, before the user changed it.
        /// </summary>
        public string OriginalReportLocation
        {
            get
            {
                return m_originalReportLocation;
            }
            set
            {
                m_originalReportLocation = value;
                OnPropertyChanged("OriginalReportLocation");
            }
        }

        /// <summary>
        /// Gets or sets the location to which reports will be generated.
        /// </summary>
        public string ReportLocation
        {
            get
            {
                return m_reportLocation;
            }
            set
            {
                m_reportLocation = value;
                OnPropertyChanged("ReportLocation");

                if (m_reportLocation.Any(c => Path.GetInvalidPathChars().Contains(c)))
                    throw new FormatException("Invalid characters found in path: " + string.Join(", ", Path.GetInvalidPathChars().Where(c => m_reportLocation.Contains(c)).Select(c => string.Format("'{0}'", c))));
            }
        }

        /// <summary>
        /// Gets the original report generation time text since the last
        /// time settings were applied, before the user changed it.
        /// </summary>
        public string OriginalReportGenerationTimeText
        {
            get
            {
                return m_originalReportGenerationTime.ToString("HH:mm");
            }
        }
        
        /// <summary>
        /// Gets or sets the text used to display or modify the value of the <see cref="ReportGenerationTime"/> property.
        /// </summary>
        public string ReportGenerationTimeText
        {
            get
            {
                return m_reportGenerationTimeText;
            }
            set
            {
                DateTime reportGenerationTime;

                m_reportGenerationTimeText = value;
                OnPropertyChanged("ReportGenerationTimeText");

                if (!DateTime.TryParseExact(value, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out reportGenerationTime))
                    throw new FormatException("Invalid time format. Use \"HH:mm\" format.");
            }
        }

        /// <summary>
        /// Gets or sets the report generation time.
        /// </summary>
        public DateTime ReportGenerationTime
        {
            get
            {
                return m_reportGenerationTime;
            }
            set
            {
                m_reportGenerationTime = value;
                m_reportGenerationTimeText = m_reportGenerationTime.ToString("HH:mm");
                OnPropertyChanged("ReportGenerationTime");
                OnPropertyChanged("ReportGenerationTimeText");
            }
        }

        /// <summary>
        /// Gets or sets the date of the report to be generated manually.
        /// </summary>
        public DateTime ReportDate
        {
            get
            {
                return m_reportDate;
            }
            set
            {
                m_reportDate = value;
                OnPropertyChanged("ReportDate");
            }
        }

        /// <summary>
        /// Gets or sets the original idle report lifetime since the last
        /// time settings were applied, before the user changed it.
        /// </summary>
        public double OriginalIdleReportLifetime
        {
            get
            {
                return m_originalIdleReportLifetime;
            }
            set
            {
                m_originalIdleReportLifetime = value;
                OnPropertyChanged("OriginalIdleReportLifetime");
            }
        }

        /// <summary>
        /// Gets or sets the amount of time to pass since the last time a
        /// report was accessed before the report is purged automatically.
        /// </summary>
        public double IdleReportLifetime
        {
            get
            {
                return m_idleReportLifetime;
            }
            set
            {
                m_idleReportLifetime = value;
                OnPropertyChanged("IdleReportLifetime");
            }
        }

        /// <summary>
        /// Gets the list of available reports.
        /// </summary>
        public ObservableCollection<AvailableReport> AvailableReports
        {
            get
            {
                return m_availableReports;
            }
        }

        /// <summary>
        /// Gets the list of pending reports.
        /// </summary>
        public ObservableCollection<string> PendingReports
        {
            get
            {
                return m_pendingReports;
            }
        }

        /// <summary>
        /// Gets the command to change the enabled state of reporting services.
        /// </summary>
        public ICommand ChangeReportingEnabledCommand
        {
            get
            {
                return m_changeReportingEnabledCommand ?? (m_changeReportingEnabledCommand = new RelayCommand(ChangeReportingEnabled, () => true));
            }
        }

        /// <summary>
        /// Gets the command to generate a new report manually.
        /// </summary>
        public ICommand ListReportsCommand
        {
            get
            {
                return m_listReportsCommand ?? (m_listReportsCommand = new RelayCommand(ListReports, () => true));
            }
        }

        /// <summary>
        /// Gets the command to generate a new report manually.
        /// </summary>
        public ICommand GenerateReportCommand
        {
            get
            {
                return m_generateReportCommand ?? (m_generateReportCommand = new RelayCommand(GenerateReport, () => true));
            }
        }

        /// <summary>
        /// Gets the command to update the configuration of the reporting services.
        /// </summary>
        public ICommand ApplyReportingConfigCommand
        {
            get
            {
                return m_applyReportingConfigCommand ?? (m_applyReportingConfigCommand = new RelayCommand(ApplyReportingConfig, () => true));
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="DataQualityReportViewModel"/> object.
        /// </summary>
        public void Dispose()
        {
            if (!m_disposed)
            {
                try
                {
                    if ((object)m_responseComplete != null)
                    {
                        // Release any waiting threads before disposing wait handle
                        m_responseComplete.Set();
                        m_responseComplete.Dispose();
                    }

                    m_responseComplete = null;
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }

            GC.SuppressFinalize(this);
        }

        private void GetSchedules()
        {
            WindowsServiceClient serviceClient = null;

            try
            {
                m_responseComplete.Reset();
                serviceClient = CommonFunctions.GetWindowsServiceClient();
                CheckSchedules(serviceClient);
            }
            catch (Exception ex)
            {
                string message = string.Format("Unable to determine whether reporting services are enabled: {0}", ex.Message);
                Popup(message, "GetSchedules Error", MessageBoxImage.Error);
            }
            finally
            {
                if ((object)serviceClient != null && (object)serviceClient.Helper != null)
                    serviceClient.Helper.ReceivedServiceUpdate -= Helper_ReceivedServiceUpdate;
            }
        }

        private void GetReportingConfiguration()
        {
            WindowsServiceClient serviceClient = null;

            Arguments args;
            string reportLocation;
            string idleReportLifetimeArg;
            double idleReportLifetime;

            try
            {
                m_responseComplete.Reset();
                serviceClient = CommonFunctions.GetWindowsServiceClient();
                serviceClient.Helper.ReceivedServiceResponse += Helper_ReceivedServiceResponse;
                serviceClient.Helper.SendRequest("ReportingConfig");

                // Wait for command response allowing for processing time
                if ((object)m_responseComplete != null)
                {
                    if (!m_responseComplete.Wait(5000))
                        throw new TimeoutException("Timed-out after 5 seconds waiting for service response.");
                }

                if (!string.IsNullOrEmpty(m_reportingConfiguration))
                {
                    args = new Arguments(m_reportingConfiguration);

                    m_reportTitle = args["title"].ToNonNullString().Trim();
                    reportLocation = args["reportLocation"];
                    idleReportLifetimeArg = args["idleReportLifetime"];

                    if (!string.IsNullOrEmpty(reportLocation))
                    {
                        ReportLocation = reportLocation.Trim();
                        OriginalReportLocation = m_reportLocation;
                    }

                    if (!string.IsNullOrEmpty(idleReportLifetimeArg) && double.TryParse(idleReportLifetimeArg, out idleReportLifetime))
                    {
                        IdleReportLifetime = idleReportLifetime;
                        OriginalIdleReportLifetime = idleReportLifetime;
                    }

                    m_reportingConfiguration = null;
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("Unable to retrieve reporting services configuration: {0}", ex.Message);
                Popup(message, "GetReportingConfiguration Error", MessageBoxImage.Error);
            }
            finally
            {
                if ((object)serviceClient != null && (object)serviceClient.Helper != null)
                    serviceClient.Helper.ReceivedServiceResponse -= Helper_ReceivedServiceResponse;
            }
        }

        private void ListReports()
        {
            WindowsServiceClient serviceClient = null;

            AvailableReport report;
            int statusIndex;

            try
            {
                m_responseComplete.Reset();
                serviceClient = CommonFunctions.GetWindowsServiceClient();
                serviceClient.Helper.ReceivedServiceResponse += Helper_ReceivedServiceResponse;
                serviceClient.Helper.SendRequest("ListReports");

                // Wait for command response allowing for processing time
                if ((object)m_responseComplete != null)
                {
                    if (!m_responseComplete.Wait(5000))
                        throw new TimeoutException("Timed-out after 5 seconds waiting for service response.");
                }

                if (!string.IsNullOrEmpty(m_reportsList))
                {
                    statusIndex = m_reportsList.IndexOf("Status");
                    m_availableReports.Clear();
                    m_pendingReports.Clear();

                    foreach (string line in m_reportsList.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Skip(2))
                    {
                        report = new AvailableReport(this)
                        {
                            Date = GetReportDate(line.Remove(statusIndex).Trim()),
                            Status = line.Substring(statusIndex).Trim()
                        };

                        if (report.Status != "Pending")
                            m_availableReports.Add(report);
                        else
                            m_pendingReports.Add(report.Date);
                    }

                    m_reportsList = null;
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("Unable to retrieve list of available reports: {0}", ex.Message);
                Popup(message, "ListReports Error", MessageBoxImage.Error);
            }
            finally
            {
                if ((object)serviceClient != null && (object)serviceClient.Helper != null)
                    serviceClient.Helper.ReceivedServiceResponse -= Helper_ReceivedServiceResponse;
            }
        }

        private void GetReport(DateTime reportDate)
        {
            WindowsServiceClient serviceClient = null;
            FileDialog fileDialog;

            try
            {
                fileDialog = new SaveFileDialog();

                fileDialog.DefaultExt = "pdf";
                fileDialog.Filter = "PDF files|*.pdf|All files|*.*";
                fileDialog.FileName = string.Format("{0} {1:yyyy-MM-dd}.pdf", m_reportTitle, reportDate);

                if (fileDialog.ShowDialog() == true)
                {
                    m_responseComplete.Reset();
                    serviceClient = CommonFunctions.GetWindowsServiceClient();
                    serviceClient.Helper.ReceivedServiceResponse += Helper_ReceivedServiceResponse;
                    serviceClient.Helper.SendRequest(string.Format("GetReport {0:yyyy-MM-dd}", reportDate));

                    // Wait for command response allowing for processing time
                    if ((object)m_responseComplete != null)
                    {
                        if (!m_responseComplete.Wait(5000))
                            throw new TimeoutException("Timed-out after 5 seconds waiting for service response.");
                    }

                    if ((object)m_reportData != null)
                    {
                        File.WriteAllBytes(fileDialog.FileName, m_reportData);
                        m_reportData = null;

                        try
                        {
                            using (Process.Start(fileDialog.FileName))
                            {
                            }
                        }
                        catch (Exception ex)
                        {
                            string message = string.Format("Report saved, but unable to open: {0}", ex.Message);
                            Popup(message, "GetReport", MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("Unable to download report: {0}", ex.Message);
                Popup(message, "GetReport", MessageBoxImage.Error);
            }
            finally
            {
                if ((object)serviceClient != null && (object)serviceClient.Helper != null)
                    serviceClient.Helper.ReceivedServiceResponse -= Helper_ReceivedServiceResponse;
            }
        }

        private void GenerateReport()
        {
            WindowsServiceClient serviceClient = null;

            try
            {
                m_responseComplete.Reset();
                serviceClient = CommonFunctions.GetWindowsServiceClient();
                serviceClient.Helper.ReceivedServiceResponse += Helper_ReceivedServiceResponse;
                serviceClient.Helper.SendRequest(string.Format("GenerateReport {0:yyyy-MM-dd}", m_reportDate));

                // Wait for command response allowing for processing time
                if ((object)m_responseComplete != null)
                {
                    if (!m_responseComplete.Wait(5000))
                        throw new TimeoutException("Timed-out after 5 seconds waiting for service response.");
                }

                ListReports();
            }
            catch (Exception ex)
            {
                string message = string.Format("Unable to generate report: {0}", ex.Message);
                Popup(message, "GenerateReport", MessageBoxImage.Error);
            }
            finally
            {
                if ((object)serviceClient != null && (object)serviceClient.Helper != null)
                    serviceClient.Helper.ReceivedServiceResponse -= Helper_ReceivedServiceResponse;
            }
        }

        private void ChangeReportingEnabled()
        {
            WindowsServiceClient serviceClient = null;

            try
            {
                m_responseComplete.Reset();
                serviceClient = CommonFunctions.GetWindowsServiceClient();

                if (ReportingEnabled)
                    serviceClient.Helper.SendRequest("Unschedule Reporting -save");
                else
                    serviceClient.Helper.SendRequest(string.Format("Reschedule Reporting \"{0} {1} * * *\" -save", m_reportGenerationTime.Minute, m_reportGenerationTime.Hour));

                CheckSchedules(serviceClient);
            }
            catch (Exception ex)
            {
                string message = string.Format("Unable to {0} reporting services: {1}", m_reportingEnabled ? "disable" : "enable", ex.Message);
                Popup(message, "ChangeReportingEnabled Error", MessageBoxImage.Error);
            }
            finally
            {
                if ((object)serviceClient != null && (object)serviceClient.Helper != null)
                    serviceClient.Helper.ReceivedServiceUpdate -= Helper_ReceivedServiceUpdate;
            }
        }

        private void ApplyReportingConfig()
        {
            WindowsServiceClient serviceClient = null;
            DateTime reportGenerationTime;

            try
            {
                serviceClient = CommonFunctions.GetWindowsServiceClient();

                if (m_reportingEnabled && DateTime.TryParseExact(m_reportGenerationTimeText, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out reportGenerationTime) && reportGenerationTime != m_originalReportGenerationTime)
                {
                    m_responseComplete.Reset();
                    serviceClient.Helper.ReceivedServiceUpdate += Helper_ReceivedServiceUpdate;
                    serviceClient.Helper.SendRequest(string.Format("Reschedule Reporting \"{0} {1} * * *\" -save", reportGenerationTime.Minute, reportGenerationTime.Hour));
                    CheckSchedules(serviceClient);
                }

                if (m_idleReportLifetime != m_originalIdleReportLifetime || !string.Equals(m_reportLocation, m_originalReportLocation, StringComparison.OrdinalIgnoreCase))
                {
                    m_responseComplete.Reset();
                    serviceClient.Helper.ReceivedServiceResponse += Helper_ReceivedServiceResponse;

                    serviceClient.Helper.SendRequest(string.Format("ReportingConfig -set --reportLocation=\" {0} \" --idleReportLifetime=\" {1} \"",
                        m_reportLocation.Replace("\"", "\\\""), m_idleReportLifetime));

                    // Wait for command response allowing for processing time
                    if ((object)m_responseComplete != null)
                    {
                        if (!m_responseComplete.Wait(5000))
                            throw new TimeoutException("Timed-out after 5 seconds waiting for service response.");
                    }

                    if ((object)m_reportingConfiguration != null)
                    {
                        OriginalReportLocation = m_reportLocation;
                        OriginalIdleReportLifetime = m_idleReportLifetime;
                        m_reportingConfiguration = null;
                        ListReports();
                    }
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("Unable to modify reporting services configuration: {0}", ex.Message);
                Popup(message, "UpdateReportingConfig Error", MessageBoxImage.Error);
            }
            finally
            {
                if ((object)serviceClient != null && (object)serviceClient.Helper != null)
                {
                    serviceClient.Helper.ReceivedServiceUpdate -= Helper_ReceivedServiceUpdate;
                    serviceClient.Helper.ReceivedServiceResponse -= Helper_ReceivedServiceResponse;
                }
            }
        }

        private void CheckSchedules(WindowsServiceClient serviceClient = null)
        {
            Match match;
            int hour;
            int minute;

            if ((object)serviceClient == null)
                serviceClient = CommonFunctions.GetWindowsServiceClient();

            serviceClient.Helper.ReceivedServiceUpdate += Helper_ReceivedServiceUpdate;
            serviceClient.Helper.SendRequest("Schedules");

            // Wait for command response allowing for processing time
            if ((object)m_responseComplete != null)
            {
                if (!m_responseComplete.Wait(5000))
                    throw new TimeoutException("Timed-out after 5 seconds waiting for service response.");
            }

            if ((object)m_responseMessage != null)
            {
                match = Regex.Match(m_responseMessage, "^Reporting +(?<Minute>[^ ]+) +(?<Hour>[^ ]+)", RegexOptions.Multiline);

                if (match.Success)
                {
                    ReportingEnabled = true;

                    if (int.TryParse(match.Groups["Hour"].Value, out hour) && int.TryParse(match.Groups["Minute"].Value, out minute))
                    {
                        ReportGenerationTime = DateTime.ParseExact(string.Format("{0:00}:{1:00}", hour, minute), "HH:mm", CultureInfo.InvariantCulture);
                        m_originalReportGenerationTime = m_reportGenerationTime;
                        OnPropertyChanged("OriginalReportGenerationTimeText");
                    }
                }
                else
                {
                    ReportingEnabled = false;
                }

                m_responseMessage = null;
            }
        }

        private string GetReportDate(string reportName)
        {
            string regex;
            Match match;
            DateTime reportDate;

            regex = string.Format(@"{0} (?<Date>[^.]+)\.pdf", m_reportTitle);
            match = Regex.Match(reportName, regex);

            if (!match.Success || !DateTime.TryParse(match.Groups["Date"].Value, out reportDate))
                reportDate = DateTime.Today - TimeSpan.FromDays(1);

            return reportDate.ToString("MM/dd/yyyy");
        }

        private void Helper_ReceivedServiceUpdate(object sender, EventArgs<UpdateType, string> e)
        {
            if (Regex.IsMatch(e.Argument2, "^Process schedules"))
            {
                m_responseMessage = e.Argument2;

                if ((object)m_responseComplete != null)
                    m_responseComplete.Set();
            }
        }

        /// <summary>
        /// Handles ReceivedServiceResponse event.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void Helper_ReceivedServiceResponse(object sender, EventArgs<ServiceResponse> e)
        {
            ServiceResponse response;
            string sourceCommand;
            bool responseSuccess;

            if ((object)e != null)
            {
                response = e.Argument;

                if ((object)response != null)
                {
                    if (ClientHelper.TryParseActionableResponse(response, out sourceCommand, out responseSuccess))
                    {
                        switch (sourceCommand.Trim().ToUpper())
                        {
                            case "LISTREPORTS":
                                if (responseSuccess)
                                    m_reportsList = response.Message;
                                else
                                    Application.Current.Dispatcher.BeginInvoke(new Action(() => Popup(response.Message, "ListReports", MessageBoxImage.Error)));

                                break;

                            case "GETREPORT":
                                if (responseSuccess)
                                {
                                    List<object> attachments = response.Attachments;

                                    if ((object)attachments != null && attachments.Count > 1)
                                        m_reportData = attachments[0] as byte[];
                                }
                                else
                                {
                                    Application.Current.Dispatcher.BeginInvoke(new Action(() => Popup(response.Message, "GetReport", MessageBoxImage.Error)));
                                }

                                break;

                            case "GENERATEREPORT":
                                if (responseSuccess)
                                    Application.Current.Dispatcher.BeginInvoke(new Action(() => Popup(response.Message, "GenerateReport", MessageBoxImage.Information)));
                                else
                                    Application.Current.Dispatcher.BeginInvoke(new Action(() => Popup(response.Message, "GenerateReport", MessageBoxImage.Error)));

                                break;

                            case "REPORTINGCONFIG":
                                if (responseSuccess)
                                    m_reportingConfiguration = response.Message;
                                else
                                    Application.Current.Dispatcher.BeginInvoke(new Action(() => Popup(response.Message, "ReportingConfig", MessageBoxImage.Error)));

                                break;
                        }

                        if ((object)m_responseComplete != null)
                            m_responseComplete.Set();
                    }
                }
            }
        }

        #endregion
    }
}