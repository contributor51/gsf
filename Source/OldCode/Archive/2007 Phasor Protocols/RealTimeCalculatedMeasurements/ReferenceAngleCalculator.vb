'*******************************************************************************************************
'  ReferenceAngleCalculator.vb - Reference phase angle calculator
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
'  01/16/2007 - Jian Zuo(Ryan) jrzuo@tva.gov
'       Implement the unwrap offset of the angle
'  01/17/2006 - J. Ritchie Carroll
'       Added code to detect data set changes (i.e., PMU's online/offline)
'  01/17/2007 - Jian Zuo(Ryan) jrzuo@tva.gov
'       Added code to handle unwrap offset initialization and reset
'
'*******************************************************************************************************

Imports System.Text
Imports System.Math
Imports TVA.Common
Imports TVA.Math.Common
Imports TVA.Measurements
Imports TVA.Collections.Common
Imports TVA.IO
Imports TVA.IO.FilePath
Imports InterfaceAdapters

Public Class ReferenceAngleCalculator

    Inherits CalculatedMeasurementAdapterBase

    Private Const BackupQueueSize As Integer = 10
    Private m_phaseResetAngle As Double
    Private m_lastAngles As Dictionary(Of MeasurementKey, Double)
    Private m_unwrapOffsets As Dictionary(Of MeasurementKey, Double)
    Private m_latestCalculatedAngles As List(Of Double)
    Private m_measurements As IMeasurement()

    '#If DEBUG Then
    '    Private m_frameLog As LogFile
    '#End If

    Public Sub New()

        m_lastAngles = New Dictionary(Of MeasurementKey, Double)
        m_unwrapOffsets = New Dictionary(Of MeasurementKey, Double)
        m_latestCalculatedAngles = New List(Of Double)

        '#If DEBUG Then
        '        m_frameLog = New LogFile(GetApplicationPath() & "ReferenceAngleLog.txt")
        '#End If

    End Sub

    Public Overrides Sub Initialize(ByVal calculationName As String, ByVal configurationSection As String, ByVal outputMeasurements As IMeasurement(), ByVal inputMeasurementKeys As MeasurementKey(), ByVal minimumMeasurementsToUse As Integer, ByVal expectedMeasurementsPerSecond As Integer, ByVal lagTime As Double, ByVal leadTime As Double)

        MyBase.Initialize(calculationName, configurationSection, outputMeasurements, inputMeasurementKeys, minimumMeasurementsToUse, expectedMeasurementsPerSecond, lagTime, leadTime)
        MyClass.MinimumMeasurementsToUse = minimumMeasurementsToUse

    End Sub

    Public Overrides Property MinimumMeasurementsToUse() As Integer
        Get
            Return MyBase.MinimumMeasurementsToUse
        End Get
        Set(ByVal value As Integer)
            MyBase.MinimumMeasurementsToUse = value
            m_phaseResetAngle = value * 360
        End Set
    End Property

    Public Overrides ReadOnly Property Status() As String
        Get
            Const ValuesToShow As Integer = 4

            With New StringBuilder
                .Append("  Last " & ValuesToShow & " calculated angles: ")
                SyncLock m_latestCalculatedAngles
                    If m_latestCalculatedAngles.Count > ValuesToShow Then
                        .Append(ListToString(m_latestCalculatedAngles.GetRange(m_latestCalculatedAngles.Count - ValuesToShow, ValuesToShow), ","c))
                    Else
                        .Append("Not enough values calculated yet...")
                    End If
                End SyncLock
                .AppendLine()
                .Append(MyBase.Status)
                Return .ToString()
            End With
        End Get
    End Property

    ''' <summary>
    ''' Calculates the "virtual" Eastern Interconnect reference angle
    ''' </summary>
    ''' <param name="frame">Single frame of measurement data within a one second sample</param>
    ''' <param name="index">Index of frame within the one second sample</param>
    ''' <remarks>
    ''' The frame.Measurements property references a dictionary, keyed on each measurement's MeasurementKey, containing
    ''' all available measurements as defined by the InputMeasurementKeys property that arrived within the specified
    ''' LagTime.  Note that this function will be called with a frequency specified by the ExpectedMeasurementsPerSecond
    ''' property, so make sure all work to be done is executed as efficiently as possible.
    ''' </remarks>
    Protected Overrides Sub PublishFrame(ByVal frame As IFrame, ByVal index As Integer)

        Dim calculatedMeasurement As Measurement = Measurement.Clone(OutputMeasurements(0), frame.Ticks)
        Dim currentAngle As IMeasurement
        Dim angle, deltaAngle, angleTotal, angleAverage, lastAngle, unwrapOffset As Double
        Dim key As MeasurementKey
        Dim dataSetChanged As Boolean
        Dim x As Integer

        '#If DEBUG Then
        '        LogFrameDetail(frame, index)
        '#End If

        ' Attempt to get minimum needed reporting set of composite angles used to calculate reference angle
        If TryGetMinimumNeededMeasurements(frame, m_measurements) Then
            ' See if data set has changed since last run
            If m_lastAngles.Count > 0 AndAlso m_lastAngles.Count = m_measurements.Length Then
                For x = 0 To m_measurements.Length - 1
                    If Not m_lastAngles.ContainsKey(m_measurements(x).Key) Then
                        dataSetChanged = True
                        Exit For
                    End If
                Next
            Else
                dataSetChanged = True
            End If

            ' Reinitialize all angle calculation data if data set has changed
            If dataSetChanged Then
                Dim angleRef, angleDelta0, angleDelta1, angleDelta2 As Double

                ' Clear last angles and unwrap offsets
                m_lastAngles.Clear()
                m_unwrapOffsets.Clear()

                ' Calculate new unwrap offsets
                angleRef = m_measurements(0).AdjustedValue

                For x = 0 To m_measurements.Length - 1
                    angleDelta0 = Abs(m_measurements(x).AdjustedValue - angleRef)
                    angleDelta1 = Abs(m_measurements(x).AdjustedValue + 360.0R - angleRef)
                    angleDelta2 = Abs(m_measurements(x).AdjustedValue - 360.0R - angleRef)

                    If angleDelta0 < angleDelta1 AndAlso angleDelta0 < angleDelta2 Then
                        unwrapOffset = 0.0R
                    ElseIf angleDelta1 < angleDelta2 Then
                        unwrapOffset = 360.0R
                    Else
                        unwrapOffset = -360.0R
                    End If

                    m_unwrapOffsets(m_measurements(x).Key) = unwrapOffset
                Next
            End If

            ' Total all phase angles, unwrapping angles if needed
            For x = 0 To m_measurements.Length - 1
                ' Get current angle value and key
                With m_measurements(x)
                    angle = .AdjustedValue
                    key = .Key
                End With

                ' Get the unwrap offset for this angle
                unwrapOffset = m_unwrapOffsets(key)

                ' Get angle value from last run (if there was a last run)
                If m_lastAngles.TryGetValue(key, lastAngle) Then
                    ' Calculate angle difference from last run
                    deltaAngle = angle - lastAngle

                    ' Adjust angle unwrap offset, if needed
                    If deltaAngle > 300 Then
                        unwrapOffset -= 360
                    ElseIf deltaAngle < -300 Then
                        unwrapOffset += 360
                    End If

                    ' Reset angle unwrap offset, if needed
                    If unwrapOffset > m_phaseResetAngle Then
                        unwrapOffset -= m_phaseResetAngle
                    ElseIf unwrapOffset < -m_phaseResetAngle Then
                        unwrapOffset += m_phaseResetAngle
                    End If

                    ' Track last angle unwrap offset
                    m_unwrapOffsets(key) = unwrapOffset
                End If

                ' Total composite angles so average can be calculated
                angleTotal += (angle + unwrapOffset)
            Next

            ' We use modulus function to make sure angle is in range of 0 to 359
            angleAverage = (angleTotal / m_measurements.Length) Mod 360

            ' Track last angles for next run
            m_lastAngles.Clear()

            For x = 0 To m_measurements.Length - 1
                currentAngle = m_measurements(x)
                m_lastAngles.Add(currentAngle.Key, currentAngle.AdjustedValue)
            Next
        Else
            ' Use stack average when minimum set is below specified angle count
            angleAverage = Average(m_latestCalculatedAngles) Mod 360

            ' We mark quality bad on measurement when we fall back to stack average
            calculatedMeasurement.ValueQualityIsGood = False

            '#If DEBUG Then
            '            'RaiseCalculationException(
            '            LogFrameWarning("WARNING: Minimum set of PMU's not available for reference angle calculation - using rolling average")
            '#End If
        End If

        ' Slide angle value in range of -179 to +180
        If angleAverage > 180 Then angleAverage -= 360
        If angleAverage < -179 Then angleAverage += 360
        calculatedMeasurement.Value = angleAverage

        '#If DEBUG Then
        '        LogFrameWarning("Calculated reference angle: " & angleAverage)
        '#End If

        ' Provide calculated measurement for external consumption
        PublishNewCalculatedMeasurement(calculatedMeasurement)

        ' Add calculated reference angle to latest angle queue used as backup in case
        ' needed minimum number of PMU's go offline or are slow reporting
        SyncLock m_latestCalculatedAngles
            With m_latestCalculatedAngles
                .Add(angleAverage)
                While .Count > BackupQueueSize
                    .RemoveAt(0)
                End While
            End With
        End SyncLock

    End Sub

    '#If DEBUG Then

    '    Private Sub LogFrameDetail(ByVal frame As IFrame, ByVal frameIndex As Integer)

    '        With m_frameLog
    '            ' Received frame to publish
    '            .WriteLine("***************************************************************************************")
    '            .WriteLine("   Frame Time: " & frame.Timestamp.ToString("HH:mm:ss.fff"))
    '            .WriteLine("  Frame Index: " & frameIndex)
    '            .WriteLine(" Measurement Detail - " & frame.Measurements.Values.Count & " total:")

    '            .Write("        Keys: ")
    '            For Each measurement As IMeasurement In frame.Measurements.Values
    '                .Write(measurement.Key.ToString().PadLeft(10) & " ")
    '            Next
    '            .WriteLine("")

    '            .Write("      Values: ")
    '            For Each measurement As IMeasurement In frame.Measurements.Values
    '                .Write(measurement.Value.ToString("0.000").PadLeft(10) & " ")
    '            Next
    '            .WriteLine("")
    '        End With

    '    End Sub

    '    Private Sub LogFrameWarning(ByVal warning As String)

    '        m_frameLog.WriteLine("[" & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") & "]: " & warning)

    '    End Sub

    '#End If

End Class