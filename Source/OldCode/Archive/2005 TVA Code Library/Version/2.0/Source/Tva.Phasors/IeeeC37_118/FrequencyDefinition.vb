'*******************************************************************************************************
'  FrequencyDefinition.vb - IEEE C37.118 Frequency definition
'  Copyright � 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Text
Imports Tva.Common
Imports Tva.Interop
Imports Tva.Interop.Bit

Namespace IeeeC37_118

    <CLSCompliant(False)> _
    Public Class FrequencyDefinition

        Inherits FrequencyDefinitionBase

        Public Sub New(ByVal parent As ConfigurationCell)

            MyBase.New(parent)
            ScalingFactor = 1000
            DfDtScalingFactor = 100

        End Sub

        Public Sub New(ByVal parent As ConfigurationCell, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            MyBase.New(parent, binaryImage, startIndex)
            ScalingFactor = 1000
            DfDtScalingFactor = 100

        End Sub

        Public Sub New(ByVal parent As ConfigurationCell, ByVal dataFormat As DataFormat, ByVal index As Int32, ByVal label As String, ByVal scale As Int32, ByVal offset As Single, ByVal dfdtScale As Int32, ByVal dfdtOffset As Single)

            MyBase.New(parent, dataFormat, index, label, scale, offset, dfdtScale, dfdtOffset)

        End Sub

        Public Sub New(ByVal frequencyDefinition As IFrequencyDefinition)

            MyBase.New(frequencyDefinition)

        End Sub

        Friend Shared Function CreateNewFrequencyDefintion(ByVal parent As IConfigurationCell, ByVal binaryImage As Byte(), ByVal startIndex As Int32) As IFrequencyDefinition

            Return New FrequencyDefinition(parent, binaryImage, startIndex)

        End Function

        Public Overrides ReadOnly Property InheritedType() As System.Type
            Get
                Return Me.GetType
            End Get
        End Property

        Protected Overrides ReadOnly Property BodyLength() As UInt16
            Get
                Return 2
            End Get
        End Property

        Protected Overrides ReadOnly Property BodyImage() As Byte()
            Get
                Return EndianOrder.BigEndian.GetBytes(System.Convert.ToInt16(IIf(Parent.NominalFrequency = LineFrequency.Hz50, Bit0, Nill)))
            End Get
        End Property

        Protected Overrides Sub ParseBodyImage(ByVal state As IChannelParsingState, ByVal binaryImage() As Byte, ByVal startIndex As Int32)

            Parent.NominalFrequency = IIf(EndianOrder.BigEndian.ToInt16(binaryImage, startIndex) And Bit0 > 0, LineFrequency.Hz50, LineFrequency.Hz60)

        End Sub

    End Class

End Namespace