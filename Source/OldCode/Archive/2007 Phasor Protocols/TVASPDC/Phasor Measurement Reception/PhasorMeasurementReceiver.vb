'*******************************************************************************************************
'  PhasorMeasurementReceiver.vb - Phasor measurement acquisition class
'  Copyright � 2008 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2008
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  05/19/2006 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Text
Imports System.Data.OleDb
Imports System.Threading
Imports System.Reflection
Imports TVA.Common
Imports TVA.Assembly
Imports TVA.DateTime.Common
Imports TVA.Data.Common
Imports TVA.Communication
Imports TVA.Measurements
Imports TVA.Text.Common
Imports TVA.ErrorManagement
Imports TVA.IO
Imports TVA.Services
Imports InterfaceAdapters
Imports PhasorProtocols
Imports PhasorProtocols.Common
Imports PCS

Public Class PhasorMeasurementReceiver

    Implements IServiceComponent

    Public Event NewMeasurements(ByVal measurements As ICollection(Of IMeasurement))
    Public Event StatusMessage(ByVal status As String)

    Private WithEvents m_reportingStatus As Timers.Timer
    Private WithEvents m_historianAdapter As IHistorianAdapter
    Private m_archiverSource As String
    Private m_connectionString As String
    Private m_dataLossInterval As Integer
    Private m_mappers As Dictionary(Of String, PhasorMeasurementMapper)
    Private m_listeningAdapters As List(Of IListeningAdapter)
    Private m_reportingTolerance As Integer
    Private m_measurementWarningThreshold As Integer
    Private m_measurementDumpingThreshold As Integer
    Private m_intializing As Boolean
    Private m_disposed As Boolean
    Private m_hasVirtualDevices As Boolean
    Private m_exceptionLogger As GlobalExceptionLogger

    Public Sub New( _
        ByVal historianAdapter As IHistorianAdapter, _
        ByVal archiverSource As String, _
        ByVal connectionString As String, _
        ByVal reportingTolerance As Integer, _
        ByVal statusReportingInterval As Integer, _
        ByVal dataLossInterval As Integer, _
        ByVal measurementWarningThreshold As Integer, _
        ByVal measurementDumpingThreshold As Integer, _
        ByVal exceptionLogger As GlobalExceptionLogger)

        m_historianAdapter = historianAdapter
        m_archiverSource = archiverSource
        m_connectionString = connectionString
        m_reportingTolerance = reportingTolerance
        m_dataLossInterval = dataLossInterval
        m_measurementWarningThreshold = measurementWarningThreshold
        m_measurementDumpingThreshold = measurementDumpingThreshold
        m_exceptionLogger = exceptionLogger
        m_reportingStatus = New Timers.Timer

        With m_reportingStatus
            .AutoReset = True
            .Interval = statusReportingInterval * 1000
            .Enabled = False
        End With

    End Sub

    Protected Overridable Sub Dispose(ByVal disposing As Boolean)

        If Not m_disposed Then
            If disposing Then
                If m_reportingStatus IsNot Nothing Then m_reportingStatus.Dispose()
                m_reportingStatus = Nothing

                DisposeAdapters()

                If m_historianAdapter IsNot Nothing Then m_historianAdapter.Dispose()
            End If
        End If

        m_disposed = True

    End Sub

    Private Sub DisposeAdapters()

        If m_mappers IsNot Nothing Then
            For Each mapper As PhasorMeasurementMapper In m_mappers.Values
                RemoveHandler mapper.ParsingStatus, AddressOf UpdateStatus
                RemoveHandler mapper.NewParsedMeasurements, AddressOf NewParsedMeasurements
                mapper.Dispose()
            Next

            m_mappers.Clear()
        End If

        m_mappers = Nothing

        ' Disconnect from any listening adapters...
        If m_listeningAdapters IsNot Nothing Then
            For Each listener As IListeningAdapter In m_listeningAdapters
                RemoveHandler listener.StatusMessage, AddressOf UpdateStatus
                RemoveHandler listener.NewMeasurements, AddressOf QueueMeasurementsForArchival
                listener.Dispose()
            Next

            m_listeningAdapters.Clear()
        End If

        m_listeningAdapters = Nothing

    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose

        GC.SuppressFinalize(Me)
        Dispose(True)

    End Sub

    Public Sub Connect()

        m_historianAdapter.Connect()

        ' Connect to devices...
        If m_mappers IsNot Nothing Then
            For Each mapper As PhasorMeasurementMapper In m_mappers.Values
                mapper.Connect()
            Next
        End If

        m_reportingStatus.Enabled = True

    End Sub

    Public Sub Disconnect()

        m_reportingStatus.Enabled = False

        ' Disconnect from devices...
        If m_mappers IsNot Nothing Then
            For Each mapper As PhasorMeasurementMapper In m_mappers.Values
                mapper.Disconnect()
            Next
        End If

        ' Disconnect from any listening adapters...
        If m_listeningAdapters IsNot Nothing Then
            For Each listener As IListeningAdapter In m_listeningAdapters
                listener.Disconnect()
            Next
        End If

        m_historianAdapter.Disconnect()

    End Sub

    Public Sub Initialize(ByVal connection As OleDbConnection)

        ' Disconnect archiver and all phasor measurement mappers...
        Disconnect()

        ' This function will recreate mappers and adapters, so we need to dispose of any existing ones...
        DisposeAdapters()

        UpdateStatus("Initializing phasor measurement receiver...")

        Try
            m_intializing = True

            Dim parser As MultiProtocolFrameParser
            Dim configurationCells As Dictionary(Of UInt16, ConfigurationCell)
            Dim signalMeasurements As Dictionary(Of String, IMeasurement)
            Dim configCell As ConfigurationCell
            Dim connectionString As String
            Dim keys As Dictionary(Of String, String)
            Dim transport As String
            Dim iniFileName As String
            Dim row As DataRow
            Dim source As String
            Dim timezone As String
            Dim timeAdjustmentTicks As Long
            Dim accessID As UInt16
            Dim virtualDevice As Boolean
            Dim alternateSource As Boolean
            Dim setting As String
            Dim x, y As Integer

            m_mappers = New Dictionary(Of String, PhasorMeasurementMapper)(StringComparer.OrdinalIgnoreCase)
            m_listeningAdapters = New List(Of IListeningAdapter)

            ' Initialize each data connection
            With RetrieveData(String.Format("SELECT * FROM ActiveDeviceConnections WHERE Historian='{0}'", m_archiverSource), connection)
                For x = 0 To .Rows.Count - 1
                    ' Get current row
                    row = .Rows(x)

                    parser = New MultiProtocolFrameParser
                    configurationCells = New Dictionary(Of UInt16, ConfigurationCell)
                    signalMeasurements = New Dictionary(Of String, IMeasurement)

                    connectionString = row("ConnectionString").ToString()
                    source = row("Acronym").ToString().ToUpper().Trim()
                    timezone = row("TimeZone").ToString()
                    timeAdjustmentTicks = Convert.ToInt64(row("TimeOffsetTicks"))
                    accessID = Convert.ToUInt16(row("AccessID"))

                    ' We pre-parse the connection string for special parameters
                    keys = connectionString.ParseKeyValuePairs()

                    ' See if this is a virtual device
                    virtualDevice = False

                    If keys.TryGetValue("virtual", setting) Then
                        ' Virtual devices consist entirely of composed points so there
                        ' will be no physical device to connect to
                        virtualDevice = ParseBoolean(setting)
                    End If

                    ' See if this is non-standard streaming data source
                    alternateSource = False

                    If keys.TryGetValue("source", setting) Then
                        ' Non-standard data sources will use a plug-in adapter to directly
                        ' stream in data points (no standard phasor parsing)
                        alternateSource = True

                        ' Because of their nature, non-standard data sources will act like
                        ' a virtual device (points coming from another source)
                        virtualDevice = True
                    End If

                    If virtualDevice Then
                        ' Nothing to parse for virtual devices...
                        parser = Nothing

                        ' We track whether or not this receiver has virtual devices
                        If Not m_hasVirtualDevices Then m_hasVirtualDevices = True
                    Else
                        ' Initialize connection string
                        parser.ConnectionString = connectionString

                        ' Define phasor protocol
                        Try
                            parser.PhasorProtocol = DirectCast([Enum].Parse(GetType(PhasorProtocol), row("PhasorProtocol").ToString(), True), PhasorProtocol)
                        Catch ex As ArgumentException
                            UpdateStatus(String.Format("Unexpected phasor protocol encountered for ""{0}"": {1} - defaulting to IEEE C37.118 V1.", source, row("PhasorProtocol")))
                            m_exceptionLogger.Log(ex)
                            parser.PhasorProtocol = PhasorProtocol.IeeeC37_118V1
                        End Try

                        ' Define transport protocol
                        Try
                            If keys.TryGetValue("protocol", transport) Then
                                parser.TransportProtocol = DirectCast([Enum].Parse(GetType(TransportProtocol), transport, True), TransportProtocol)
                            Else
                                UpdateStatus(String.Format("Didn't find transport protocol in connection string for ""{0}"": ""{1}"" - defaulting to TCP.", source, parser.ConnectionString))
                                parser.TransportProtocol = TransportProtocol.Tcp
                            End If
                        Catch ex As ArgumentException
                            UpdateStatus(String.Format("Unexpected transport protocol encountered for ""{0}"": {1} - defaulting to TCP.", source, transport))
                            m_exceptionLogger.Log(ex)
                            parser.TransportProtocol = TransportProtocol.Tcp
                        End Try

                        ' Handle special connection parameters
                        If parser.PhasorProtocol = PhasorProtocol.BpaPdcStream Then
                            ' Make sure required INI configuration file parameter gets initialized for BPA streams
                            With DirectCast(parser.ConnectionParameters, BpaPdcStream.ConnectionParameters)
                                If keys.TryGetValue("inifilename", iniFileName) Then
                                    .ConfigurationFileName = String.Concat(FilePath.GetApplicationPath(), iniFileName)
                                Else
                                    UpdateStatus(String.Format("Didn't find INI filename setting (e.g., ""inifilename=DEVICE_PDC.ini"") required for BPA PDCstream protocol in connection string for ""{0}"": ""{1}"" - device may fail to connect.", source, parser.ConnectionString))
                                End If
                                .RefreshConfigurationFileOnChange = True
                                .ParseWordCountFromByte = False
                            End With
                        ElseIf parser.PhasorProtocol = PhasorProtocol.FNet Then
                            ' Check for overriden FNET parameters
                            With DirectCast(parser.ConnectionParameters, FNet.ConnectionParameters)
                                Dim fnetParam As String
                                If keys.TryGetValue("ticksoffset", fnetParam) Then .TicksOffset = CLng(fnetParam)
                                If keys.TryGetValue("stationname", fnetParam) Then .StationName = fnetParam
                            End With
                        End If

                        ' For captured data simulations we will inject a simulated timestamp and auto-repeat file stream...
                        If parser.TransportProtocol = TransportProtocol.File Then
                            parser.InjectSimulatedTimestamp = True
                            parser.AutoRepeatCapturedPlayback = True
                        End If

                        parser.DeviceID = accessID
                        parser.SourceName = source
                    End If

                    If ParseBoolean(row("IsConcentrator").ToString()) Then
                        Dim loadedPmuStatus As New StringBuilder

                        loadedPmuStatus.AppendLine()
                        loadedPmuStatus.AppendLine()
                        loadedPmuStatus.AppendFormat("Loading expected PMU list for ""{0}"":", source)
                        loadedPmuStatus.AppendLine()
                        loadedPmuStatus.AppendLine()

                        ' Making a connection to a concentrator - this may support multiple PMU's
                        With RetrieveData(String.Format("SELECT ID, AccessID, Acronym FROM PdcPmus WHERE PdcID={0} AND Historian='{1}' ORDER BY IOIndex", row("ID"), m_archiverSource), connection)
                            For y = 0 To .Rows.Count - 1
                                With .Rows(y)
                                    configCell = New ConfigurationCell(Convert.ToUInt16(.Item("AccessID")), .Item("Acronym").ToString().ToUpper().Trim(), virtualDevice)
                                    configCell.Tag = .Item("ID")
                                    configurationCells.Add(configCell.IDCode, configCell)

                                    ' Create status display string for loaded PMU
                                    loadedPmuStatus.Append("   PDC Device ")
                                    loadedPmuStatus.Append(y.ToString("00"))
                                    loadedPmuStatus.Append(": ")
                                    loadedPmuStatus.Append(configCell.IDLabel)
                                    loadedPmuStatus.Append(" (")
                                    loadedPmuStatus.Append(configCell.IDCode)
                                    loadedPmuStatus.Append(")"c)
                                    loadedPmuStatus.AppendLine()
                                End With
                            Next
                        End With

                        UpdateStatus(loadedPmuStatus.ToString())
                    Else
                        ' Making a connection to a single device
                        configCell = New ConfigurationCell(accessID, source, virtualDevice)
                        configCell.Tag = row("ID")
                        configurationCells.Add(accessID, configCell)
                    End If

                    ' Initialize measurement list for this device connection keyed on the signal reference field
                    With RetrieveData(String.Format("SELECT * FROM ActiveDeviceMeasurements WHERE Acronym='{0}' AND Historian='{1}'", source, m_archiverSource), connection)
                        For y = 0 To .Rows.Count - 1
                            With .Rows(y)
                                signalMeasurements.Add( _
                                    .Item("SignalReference").ToString(), _
                                    New Measurement( _
                                        Convert.ToInt32(.Item("PointID")), _
                                        m_archiverSource, _
                                        .Item("SignalReference").ToString(), _
                                        Convert.ToDouble(.Item("Adder")), _
                                        Convert.ToDouble(.Item("Multiplier"))))
                            End With
                        Next
                    End With

                    UpdateStatus(String.Format("Loaded {0} active measurements for {1}...", signalMeasurements.Count, source))

                    ' Initialize alternate data-source
                    If alternateSource Then
                        Dim externalAssemblyName As String
                        Dim externalAssemblyTypeName As String
                        Dim externalAssembly As Assembly
                        Dim listeningAdapter As IListeningAdapter

                        If keys.TryGetValue("source", setting) Then externalAssemblyName = setting
                        If keys.TryGetValue("typename", setting) Then externalAssemblyTypeName = setting

                        Try
                            ' Load the external assembly for the historian adapter
                            externalAssembly = Assembly.LoadFrom(Service.PrefixLocalPath(externalAssemblyName))

                            ' Create a new instance of the historian adpater
                            listeningAdapter = DirectCast(Activator.CreateInstance(externalAssembly.GetType(externalAssemblyTypeName)), IListeningAdapter)
                            listeningAdapter.Initialize(connectionString)

                            AddHandler listeningAdapter.StatusMessage, AddressOf UpdateStatus
                            AddHandler listeningAdapter.NewMeasurements, AddressOf NewAlternateDataSourceMeasuremeents

                            ' Start connection cycle for listening adapter
                            listeningAdapter.Connect()

                            m_listeningAdapters.Add(listeningAdapter)
                        Catch ex As Exception
                            UpdateStatus(String.Format("Failed to load data source listener ""{0}"" from assembly ""{1}"" due to exception: {2}", externalAssemblyTypeName, externalAssemblyName, ex.Message))
                            m_exceptionLogger.Log(ex)
                        End Try
                    End If

                    ' Create a new mapper for this connection - in virtual and alternate data sources, the mapper is still used as
                    ' a place holder for status and statistical information even though it technically does no parsing or mapping
                    ' TODO: Eventually may want to make this a base class and let each implementation decide its needs, e.g., you
                    ' would have dervied PhasorMapper, VirtualDeviceMapper, AlternateDataSourceMapper
                    With New PhasorMeasurementMapper(parser, m_archiverSource, source, configurationCells, signalMeasurements, m_dataLossInterval, m_exceptionLogger)
                        ' Add timezone mapping if not UTC...
                        If String.Compare(timezone, "GMT Standard Time", True) <> 0 Then
                            Try
                                .TimeZone = GetWin32TimeZone(timezone)
                            Catch ex As Exception
                                UpdateStatus(String.Format("Failed to assign timezone offset ""{0}"" to PDC/PMU ""{1}"" due to exception: {2}", timezone, source, ex.Message))
                                m_exceptionLogger.Log(ex)
                            End Try
                        End If

                        ' Define time adjustment ticks
                        .TimeAdjustmentTicks = timeAdjustmentTicks

                        ' Bubble mapper status messages out to local update status function
                        AddHandler .ParsingStatus, AddressOf UpdateStatus

                        ' Bubble newly parsed measurements out to functions that need the real-time data
                        AddHandler .NewParsedMeasurements, AddressOf NewParsedMeasurements

                        ' Add mapper to collection
                        m_mappers.Add(source, .This)

                        ' Attempt to start connection cycle
                        .Connect()
                    End With
                Next
            End With

            UpdateStatus("Phasor measurement receiver initialized successfully.")
        Catch ex As Exception
            UpdateStatus(String.Format("Phasor measurement receiver failed to initialize: {0}", ex.Message))
            m_exceptionLogger.Log(ex)
        Finally
            m_intializing = False
        End Try

    End Sub

    Public ReadOnly Property HistorianName() As String Implements IServiceComponent.Name
        Get
            Return String.Format("[{0}] {1}", m_archiverSource, m_historianAdapter.Name)
        End Get
    End Property

    Public ReadOnly Property Mappers() As Dictionary(Of String, PhasorMeasurementMapper)
        Get
            Return m_mappers
        End Get
    End Property

    Public ReadOnly Property Status() As String Implements IServiceComponent.Status
        Get
            With New StringBuilder
                .Append("  Total device connections: ")
                .Append(m_mappers.Count)
                .AppendLine()
                .Append(m_historianAdapter.Status)
                .AppendLine()
                .AppendFormat("*** {0} detailed connection status for {1} devices follows ***", m_archiverSource, m_mappers.Count)
                .AppendLine()
                .AppendLine()

                For Each parser As PhasorMeasurementMapper In m_mappers.Values
                    .Append(parser.Status)
                    .AppendLine()
                Next

                .Append(New String("-"c, 80))
                .AppendLine()
                .AppendLine()

                Return .ToString()
            End With
        End Get
    End Property

    ''' <summary>
    ''' This function is used to archive and handle reporting status of non-standard data sources
    ''' </summary>
    Public Sub QueueMeasurementForArchival(ByVal measurement As IMeasurement)

        ' Filter incoming measurements to just the ones destined for this archive
        QueueMeasurementsForArchival(New IMeasurement() {measurement})

    End Sub

    ''' <summary>
    ''' This function is used to archive and handle reporting status of non-standard data sources
    ''' </summary>
    Public Sub QueueMeasurementsForArchival(ByVal measurements As ICollection(Of IMeasurement))

        ' Filter incoming measurements to just the ones destined for this archive
        Dim queuedMeasurements As New List(Of IMeasurement)

        For Each measurement As IMeasurement In measurements
            If String.Compare(measurement.Source, m_archiverSource, True) = 0 Then
                queuedMeasurements.Add(measurement)
            End If
        Next

        If queuedMeasurements.Count > 0 Then
            ' Queue points for archival
            m_historianAdapter.QueueMeasurementsForArchival(queuedMeasurements)

            ' For cases of "virtual" devices we'll pass calculated points back into the phasor
            ' measurement mappers (a little backwards flow) to allow "reporting" functionality
            ' for virtual devices that don't otherwise make a "connection"
            If Not m_intializing AndAlso m_hasVirtualDevices AndAlso m_mappers IsNot Nothing Then
                ' Since this is just for handling "reporting" status of virtual devices and
                ' we need to return thread control to the calculated measurement, we throw this
                ' update activity onto the thread pool - no rush, order not important
#If ThreadTracking Then
                With TVA.Threading.ManagedThreadPool.QueueUserWorkItem(AddressOf UpdateVirtualDevices, queuedMeasurements)
                    .Name = "TVASPDC.PhasorMeasurementReceiver.UpdateVirtualDevices()"
                End With
#Else
                ThreadPool.UnsafeQueueUserWorkItem(AddressOf UpdateVirtualDevices, queuedMeasurements)
#End If
            End If
        End If

    End Sub

    Private Sub UpdateVirtualDevices(ByVal state As Object)

        Dim queuedMeasurements As ICollection(Of IMeasurement) = TryCast(state, ICollection(Of IMeasurement))

        If queuedMeasurements IsNot Nothing Then
            For Each mapper As PhasorMeasurementMapper In m_mappers.Values
                ' No need to send data to mapper unless it references any virtual cells
                If mapper.HasVirtualCells Then
                    ' Pass the calculated points along to any virutal devices...
                    For Each cell As ConfigurationCell In mapper.ConfigurationCells.Values
                        If cell.IsVirtual Then mapper.ReceivedNewVirtualMeasurements(cell, queuedMeasurements)
                    Next
                End If
            Next
        End If

    End Sub

    Private Sub UpdateStatus(ByVal status As String) Handles m_historianAdapter.StatusMessage

        RaiseEvent StatusMessage(String.Format("[{0}]: {1}", m_archiverSource, status))

    End Sub

    Private Sub NewParsedMeasurements(ByVal measurements As ICollection(Of IMeasurement))

        ' Queue all of the measurements up for archival
        m_historianAdapter.QueueMeasurementsForArchival(measurements)

        ' Bubble real-time parsed measurements outside of receiver as needed...
        RaiseEvent NewMeasurements(measurements)

    End Sub

    Private Sub NewAlternateDataSourceMeasuremeents(ByVal measurements As ICollection(Of IMeasurement))

        ' Handle parsed measurements like any other data source
        NewParsedMeasurements(measurements)

        ' Update status for purely virtual alternate data source
        If Not m_intializing AndAlso m_hasVirtualDevices AndAlso m_mappers IsNot Nothing Then
            ' Since this is just for handling "reporting" status of virtual devices and
            ' we need to return thread control to the calculated measurement, we throw this
            ' update activity onto the thread pool - no rush, order not important
#If ThreadTracking Then
            With TVA.Threading.ManagedThreadPool.QueueUserWorkItem(AddressOf UpdateVirtualDevices, measurements)
                .Name = "TVASPDC.PhasorMeasurementReceiver.UpdateVirtualDevices()"
            End With
#Else
            ThreadPool.UnsafeQueueUserWorkItem(AddressOf UpdateVirtualDevices, measurements)
#End If
        End If

    End Sub

    Private Sub m_reportingStatus_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_reportingStatus.Elapsed

        If Not m_intializing Then
            Dim connection As OleDbConnection
            Dim reporting As Integer
            Dim releasePool As Boolean

            Try
                connection = New OleDbConnection(m_connectionString)
                connection.Open()

                ' Check all PMU's for "reporting status"...
                For Each mapper As PhasorMeasurementMapper In m_mappers.Values
                    ' Update reporting status for each PMU
                    For Each cell As ConfigurationCell In mapper.ConfigurationCells.Values
                        With New StringBuilder
                            reporting = IIf(Math.Abs(DateTime.UtcNow.Subtract(New DateTime(cell.LastReportTime)).Seconds) <= m_reportingTolerance, -1, 0)

                            .Append("UPDATE Pmu SET Reporting=")
                            .Append(reporting)
                            .Append(", ReportTime='")
                            .Append(DateTime.UtcNow.ToString())
                            .Append("' WHERE ID=")
                            .Append(cell.Tag.ToString())

                            ExecuteNonQuery(.ToString(), connection)
                        End With
                    Next
                Next
            Catch ex As Exception
                releasePool = True
                UpdateStatus(String.Format("[{0}] ERROR: Failed to update PMU reporting status due to exception: {1}", DateTime.Now, ex.Message))
                m_exceptionLogger.Log(ex)
            Finally
                If connection IsNot Nothing Then connection.Close()
                If releasePool Then OleDbConnection.ReleaseObjectPool()
            End Try
        End If

    End Sub

    Private Sub m_historianAdapter_ArchivalException(ByVal source As String, ByVal ex As System.Exception) Handles m_historianAdapter.ArchivalException

        UpdateStatus(String.Format("{0} data archival exception: {1}", source, ex.Message))
        m_exceptionLogger.Log(ex)

    End Sub

    Private Sub m_historianAdapter_UnarchivedMeasurements(ByVal total As Integer) Handles m_historianAdapter.UnarchivedMeasurements

        If total > m_measurementDumpingThreshold Then
            ' This event is typically caused by an offline historian, instead of throwing this data away we could offload these
            ' measurements into a temporary local cache and inject them back into the queue later...
            m_historianAdapter.GetMeasurements(m_measurementDumpingThreshold)
            UpdateStatus(String.Format("ERROR: System exercised evasive action and dumped {0} unarchived measurements from the historian queue :(", m_measurementDumpingThreshold))
        ElseIf total > m_measurementWarningThreshold Then
            UpdateStatus(String.Format("WARNING: There are {0} unarchived measurements in the historian queue", total))
        End If

    End Sub

    Private Sub ProcessStateChanged(ByVal processName As String, ByVal newState As ProcessState) Implements IServiceComponent.ProcessStateChanged

        ' Receivers won't normally be affected by changes in process state

    End Sub

    Private Sub ServiceStateChanged(ByVal newState As ServiceState) Implements IServiceComponent.ServiceStateChanged

        Select Case newState
            Case ServiceState.Paused
                Disconnect()
                UpdateStatus("Receiver disconnected due to pause request from service manager...")
            Case ServiceState.Resumed
                UpdateStatus("Receiver reconnecting due to resume request from service manager...")
                Connect()
            Case ServiceState.Shutdown
                Dispose()
        End Select

    End Sub

End Class

#Region " Old Code "

'Public Sub Poll(ByRef IntIPBuf() As Byte, ByRef nBytes As Integer, ByRef iReturn As Integer, ByRef Status As Integer)

'    If m_mappers Is Nothing Then Initialize()

'    Try

'        If iReturn = -2 Then
'            ' Intialization request poll event
'            Busy = True
'            m_pollEvents += 1

'            nBytes = FillIPBuffer(IntIPBuf, LoadDatabase())
'            iReturn = 1

'            UpdateStatus("Local database intialized..." & vbCrLf)
'        Else
'            ' Standard poll event
'            Busy = True
'            m_pollEvents += 1

'            If m_pollEvents Mod 500 = 0 Then UpdateStatus(m_pollEvents & " have been processed...")

'            Dim events As StandardEvent() = LoadEvents()

'            If events IsNot Nothing Then
'                nBytes = FillIPBuffer(IntIPBuf, events)
'            End If

'            If m_measurementBuffer.Count > 0 Or m_lastTotal > 0 Then UpdateStatus("    >> Uploading measurements, " & m_measurementBuffer.Count & " remaining...")
'            If m_measurementBuffer.Count = 0 And m_lastTotal > 0 Then UpdateStatus(Environment.NewLine)
'            m_lastTotal = m_measurementBuffer.Count

'            ' We set iReturn to zero to have DatAWare call the poll event again immediately, else set to one
'            ' (i.e., set to zero if you still have more items in the queue to be processed)
'            iReturn = IIf(m_measurementBuffer.Count > 0, 0, 1)
'        End If
'    Catch ex As Exception
'        UpdateStatus("Exception occured during poll event: " & ex.Message)
'        nBytes = 0
'        Status = 1
'        iReturn = 1
'    Finally
'        Busy = False
'    End Try

'End Sub

'Private Function LoadDatabase() As StandardEvent()

'    Dim standardEvents As New List(Of StandardEvent)

'    For Each measurementID As Integer In m_measurementIDs.Values
'        standardEvents.Add(New StandardEvent(measurementID, New TimeTag(Date.UtcNow), 0.0!, Quality.Good))
'    Next

'    Return standardEvents.ToArray()

'End Function

'Private Sub LoadAssembly(ByVal assemblyName As String)

'    ' Hook into assembly resolve event for current domain so we can load assembly from an alternate path
'    If m_assemblyCache Is Nothing Then
'        ' Create a new assembly cache
'        m_assemblyCache = New Dictionary(Of String, Assembly)

'        ' DAQ Path: HKEY_LOCAL_MACHINE\SOFTWARE\DatAWare\DAQ Configuration\
'        With Registry.LocalMachine.OpenSubKey("SOFTWARE\DatAWare\DAQ Configuration")
'            ' We define alternate path as a subfolder of defined DAQ DLL path...
'            m_assemblyFolder = .GetValue("DAQDLLPath") & "\bin\"
'            .Close()
'        End With

'        ' Add hook into standard assembly resolution
'        AddHandler AppDomain.CurrentDomain.AssemblyResolve, AddressOf ResolveAssemblyFromAlternatePath
'    End If

'    ' Load the assembly (this will invoke event that will resolve assembly from resource)
'    AppDomain.CurrentDomain.Load(assemblyName)

'End Sub

#End Region