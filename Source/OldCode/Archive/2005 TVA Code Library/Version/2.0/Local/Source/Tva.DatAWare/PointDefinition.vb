'*******************************************************************************************************
'  PointDefinition.vb - Standard DatAWare database structure element
'  Copyright � 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  05/03/2006 - J. Ritchie Carroll
'       Initial version of source imported from 1.1 code library
'
'*******************************************************************************************************

Imports System.Text
Imports Tva.Text.Common

' Standard database structure element
<Serializable()> _
Public Class PointDefinition
    Implements IComparable

    ' *******************************************************************************
    ' *                         Point Definition Structure                          *
    ' *******************************************************************************
    ' * # Of Bytes  Byte Index  Data Type   Description                             *
    ' * ----------  ----------  ----------  ----------------------------------------*
    ' * 40          0-39        Char(40)    Text description of the point           *
    ' * 2           40-41       Int16       Plant unit associated with the point    *
    ' * 2           42-43       Int16       Security level for the point            *
    ' * 64          44-107      Char(64)    Detail for hardware or datasource       *
    ' * 64          108-171     Byte(64)    ???                                     *
    ' * 4           172-175     Int32       Refer MetadataGeneralFlags              *
    ' * 4           176-179     Int32       Refer MetadataAlarmFlags                *
    ' * 4           180-183     Single      Expected transmission rate of the point *
    ' * 20          184-203     Char(20)    Descriptive name for the point          *
    ' * 20          204-223     Char(20)    Alternate name for the point            *
    ' * 20          224-243     Char(20)    Another alternate name for the point    *
    ' * 2           244-245     Char(2)     Plant ID from which the point originates*
    ' * 2           246-247     Int16       ID of the DAQ or external source device *
    ' * 4           248-251     Int32       Min time betweeen archive entries       *
    ' * 4           252-255     Int32       Max time after which point is archived  *
    ' * 4           256-259     Char(4)     System identifier for the point         *
    ' * 50          260-309     Char(50)    Email addresses for alarm notifications *
    ' * 30          310-339     Char(30)    Pager numbers for alarm notifications   *
    ' * 30          340-369     Char(30)    Phone numbers for alarm notifications   *
    ' * 128         370-497     Char(128)   Remarks for the point                   *
    ' * 256         498-753     Byte(256)   Analog/Digital/Composed/Constant fields *
    ' *******************************************************************************

#Region " Member Declaration "

    Private m_index As Integer
    Private m_description As String = ""
    Private m_unitID As Short
    Private m_securityLevel As Short
    Private m_hardwareInfo As String = ""
    Private m_spares As Byte()
    Private m_generalFlags As MetadataGeneralFlags
    Private m_alarmFlags As MetadataAlarmFlags
    Private m_scanRate As Single
    Private m_name As String = ""
    Private m_synonym1 As String = ""
    Private m_synonym2 As String = ""
    Private m_plantID As String = ""
    Private m_sourceID As Short
    Private m_compressionMinimumTime As Integer
    Private m_compressionMaximumTime As Integer
    Private m_system As String = ""
    Private m_email As String = ""
    Private m_pager As String = ""
    Private m_phone As String = ""
    Private m_remarks As String = ""
    Private m_binaryInfo As Byte()
    Private m_analogFields As MetadataAnalogFields
    Private m_digitalFields As MetadataDigitalFields
    Private m_composedFields As MetadataComposedFields
    Private m_constantFields As MetadataConstantFields
    Private m_textEncoding As Encoding

#End Region

#Region " Public Code "

    Public Const BinaryLength As Integer = 754

    Public Sub New(ByVal index As Integer)

        MyBase.New()
        m_index = index
        m_spares = CreateArray(Of Byte)(64)
        m_generalFlags = New MetadataGeneralFlags(0)
        m_alarmFlags = New MetadataAlarmFlags(0)
        m_binaryInfo = CreateArray(Of Byte)(256)
        m_analogFields = New MetadataAnalogFields(m_binaryInfo)
        m_digitalFields = New MetadataDigitalFields(m_binaryInfo)
        m_composedFields = New MetadataComposedFields(m_binaryInfo)
        m_constantFields = New MetadataConstantFields(m_binaryInfo)
        m_textEncoding = Encoding.Default ' Default to system's current ANSI code page.

    End Sub

    Public Sub New(ByVal index As Integer, ByVal binaryImage As Byte())

        MyClass.New(index, binaryImage, 0)

    End Sub

    Public Sub New(ByVal index As Integer, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

        MyClass.New(index, binaryImage, startIndex, Nothing)

    End Sub

    Public Sub New(ByVal index As Integer, ByVal binaryImage As Byte(), ByVal startIndex As Integer, ByVal encoding As Encoding)

        Me.New(index)

        If encoding IsNot Nothing Then m_textEncoding = encoding

        If binaryImage IsNot Nothing Then
            If binaryImage.Length - startIndex >= BinaryLength Then
                m_description = m_textEncoding.GetString(binaryImage, startIndex, 40).Trim()
                m_unitID = BitConverter.ToInt16(binaryImage, startIndex + 40)
                m_securityLevel = BitConverter.ToInt16(binaryImage, startIndex + 42)
                m_hardwareInfo = m_textEncoding.GetString(binaryImage, startIndex + 44, 64).Trim()
                Array.Copy(binaryImage, startIndex + 108, m_spares, 0, 64)
                m_generalFlags.Value = BitConverter.ToInt32(binaryImage, startIndex + 172)
                m_alarmFlags.Value = BitConverter.ToInt32(binaryImage, startIndex + 176)
                m_scanRate = BitConverter.ToSingle(binaryImage, startIndex + 180)
                m_name = m_textEncoding.GetString(binaryImage, startIndex + 184, 20).Trim()
                m_synonym1 = m_textEncoding.GetString(binaryImage, startIndex + 204, 20).Trim()
                m_synonym2 = m_textEncoding.GetString(binaryImage, startIndex + 224, 20).Trim()
                m_plantID = m_textEncoding.GetString(binaryImage, startIndex + 244, 2).Trim()
                m_sourceID = BitConverter.ToInt16(binaryImage, startIndex + 246)
                m_compressionMinimumTime = BitConverter.ToInt32(binaryImage, startIndex + 248)
                m_compressionMaximumTime = BitConverter.ToInt32(binaryImage, startIndex + 252)
                m_system = m_textEncoding.GetString(binaryImage, startIndex + 256, 4).Trim()
                m_email = m_textEncoding.GetString(binaryImage, startIndex + 260, 50).Trim()
                m_pager = m_textEncoding.GetString(binaryImage, startIndex + 310, 30).Trim()
                m_phone = m_textEncoding.GetString(binaryImage, startIndex + 340, 30).Trim()
                m_remarks = m_textEncoding.GetString(binaryImage, startIndex + 370, 128).Trim()
                Array.Copy(binaryImage, startIndex + 498, m_binaryInfo, 0, 256)
                Select Case m_generalFlags.PointType
                    Case PointType.Analog
                        m_analogFields.Update(m_binaryInfo)
                    Case PointType.Digital
                        m_digitalFields.Update(m_binaryInfo)
                    Case PointType.Composed
                        m_composedFields.Update(m_binaryInfo)
                    Case PointType.Constant
                        m_constantFields.Update(m_binaryInfo)
                End Select
            Else
                Throw New ArgumentException("Binary image size from startIndex is too small.")
            End If
        Else
            Throw New ArgumentNullException("binaryImage")
        End If

    End Sub

    Public Shared Function Clone(ByVal pointDefinition As PointDefinition, ByVal newIndex As Integer) As PointDefinition

        Dim newPointDefinition As New PointDefinition(newIndex)

        With newPointDefinition
            .Description = pointDefinition.Description
            .UnitID = pointDefinition.UnitID
            .SecurityLevel = pointDefinition.SecurityLevel
            .HardwareInfo = pointDefinition.HardwareInfo
            Array.Copy(pointDefinition.Spares, .Spares, 64)
            .GeneralFlags = pointDefinition.GeneralFlags
            .AlarmFlags = pointDefinition.AlarmFlags
            .ScanRate = pointDefinition.ScanRate
            .Name = pointDefinition.Name
            .Synonym1 = pointDefinition.Synonym1
            .Synonym2 = pointDefinition.Synonym2
            .PlantID = pointDefinition.PlantID
            .SourceID = pointDefinition.SourceID
            .CompressionMinimumTime = pointDefinition.CompressionMinimumTime
            .CompressionMaximumTime = pointDefinition.CompressionMaximumTime
            .System = pointDefinition.System
            .Email = pointDefinition.Email
            .Pager = pointDefinition.Pager
            .Phone = pointDefinition.Phone
            .Remarks = pointDefinition.Remarks
            .AnalogFields = pointDefinition.AnalogFields
            .DigitalFields = pointDefinition.DigitalFields
            .ComposedFields = pointDefinition.ComposedFields
            .ConstantFields = pointDefinition.ConstantFields
            .TextEncoding = pointDefinition.TextEncoding
        End With

        Return newPointDefinition

    End Function

    Public ReadOnly Property Index() As Integer
        Get
            Return m_index
        End Get
    End Property

    Public Property Description() As String
        Get
            Return m_description
        End Get
        Set(ByVal value As String)
            m_description = TruncateString(value, 40)
        End Set
    End Property

    Public Property UnitID() As Short
        Get
            Return m_unitID
        End Get
        Set(ByVal value As Short)
            m_unitID = value
        End Set
    End Property

    Public Property SecurityLevel() As Short
        Get
            Return m_securityLevel
        End Get
        Set(ByVal value As Short)
            m_securityLevel = value
        End Set
    End Property

    Public Property HardwareInfo() As String
        Get
            Return m_hardwareInfo
        End Get
        Set(ByVal value As String)
            m_hardwareInfo = TruncateString(value, 64)
        End Set
    End Property

    Public Property Spares() As Byte()
        Get
            Return m_spares
        End Get
        Set(ByVal value As Byte())
            m_spares = value
        End Set
    End Property

    Public Property GeneralFlags() As MetadataGeneralFlags
        Get
            Return m_generalFlags
        End Get
        Set(ByVal value As MetadataGeneralFlags)
            m_generalFlags = value
        End Set
    End Property

    Public Property AlarmFlags() As MetadataAlarmFlags
        Get
            Return m_alarmFlags
        End Get
        Set(ByVal value As MetadataAlarmFlags)
            m_alarmFlags = value
        End Set
    End Property

    Public Property ScanRate() As Single
        Get
            Return m_scanRate
        End Get
        Set(ByVal value As Single)
            m_scanRate = value
        End Set
    End Property

    Public Property Name() As String
        Get
            Return m_name
        End Get
        Set(ByVal value As String)
            m_name = TruncateString(value, 20)
        End Set
    End Property

    Public Property Synonym1() As String
        Get
            Return m_synonym1
        End Get
        Set(ByVal value As String)
            m_synonym1 = TruncateString(value, 20)
        End Set
    End Property

    Public Property Synonym2() As String
        Get
            Return m_synonym2
        End Get
        Set(ByVal value As String)
            m_synonym2 = TruncateString(value, 20)
        End Set
    End Property

    Public Property PlantID() As String
        Get
            Return m_plantID
        End Get
        Set(ByVal value As String)
            m_plantID = TruncateString(value, 2)
        End Set
    End Property

    Public Property SourceID() As Short
        Get
            Return m_sourceID
        End Get
        Set(ByVal value As Short)
            m_sourceID = value
        End Set
    End Property

    Public Property CompressionMinimumTime() As Integer
        Get
            Return m_compressionMinimumTime
        End Get
        Set(ByVal value As Integer)
            m_compressionMinimumTime = value
        End Set
    End Property

    Public Property CompressionMaximumTime() As Integer
        Get
            Return m_compressionMaximumTime
        End Get
        Set(ByVal value As Integer)
            m_compressionMaximumTime = value
        End Set
    End Property

    Public Property System() As String
        Get
            Return m_system
        End Get
        Set(ByVal value As String)
            m_system = TruncateString(value, 4)
        End Set
    End Property

    Public Property Email() As String
        Get
            Return m_email
        End Get
        Set(ByVal value As String)
            m_email = TruncateString(value, 50)
        End Set
    End Property

    Public Property Pager() As String
        Get
            Return m_pager
        End Get
        Set(ByVal value As String)
            m_pager = TruncateString(value, 30)
        End Set
    End Property

    Public Property Phone() As String
        Get
            Return m_phone
        End Get
        Set(ByVal value As String)
            m_phone = TruncateString(value, 30)
        End Set
    End Property

    Public Property Remarks() As String
        Get
            Return m_remarks
        End Get
        Set(ByVal value As String)
            m_remarks = TruncateString(value, 128)
        End Set
    End Property

    Public Property AnalogFields() As MetadataAnalogFields
        Get
            Return m_analogFields
        End Get
        Set(ByVal value As MetadataAnalogFields)
            m_analogFields = value
        End Set
    End Property

    Public Property DigitalFields() As MetadataDigitalFields
        Get
            Return m_digitalFields
        End Get
        Set(ByVal value As MetadataDigitalFields)
            m_digitalFields = value
        End Set
    End Property

    Public Property ComposedFields() As MetadataComposedFields
        Get
            Return m_composedFields
        End Get
        Set(ByVal value As MetadataComposedFields)
            m_composedFields = value
        End Set
    End Property

    Public Property ConstantFields() As MetadataConstantFields
        Get
            Return m_constantFields
        End Get
        Set(ByVal value As MetadataConstantFields)
            m_constantFields = value
        End Set
    End Property

    Public Property TextEncoding() As Encoding
        Get
            Return m_textEncoding
        End Get
        Set(ByVal value As Encoding)
            m_textEncoding = value
        End Set
    End Property

    Public ReadOnly Property BinaryImage() As Byte()
        Get
            Dim image As Byte() = CreateArray(Of Byte)(BinaryLength)

            ' Construct the binary IP buffer for this event
            Array.Copy(m_textEncoding.GetBytes(m_description.PadRight(40)), 0, image, 0, 40)
            Array.Copy(BitConverter.GetBytes(m_unitID), 0, image, 40, 2)
            Array.Copy(BitConverter.GetBytes(m_securityLevel), 0, image, 42, 2)
            Array.Copy(m_textEncoding.GetBytes(m_hardwareInfo.PadRight(64)), 0, image, 44, 64)
            Array.Copy(m_spares, 0, image, 108, 64)
            Array.Copy(BitConverter.GetBytes(m_generalFlags.Value), 0, image, 172, 4)
            Array.Copy(BitConverter.GetBytes(m_alarmFlags.Value), 0, image, 176, 4)
            Array.Copy(BitConverter.GetBytes(m_scanRate), 0, image, 180, 4)
            Array.Copy(m_textEncoding.GetBytes(m_name.PadRight(20)), 0, image, 184, 20)
            Array.Copy(m_textEncoding.GetBytes(m_synonym1.PadRight(20)), 0, image, 204, 20)
            Array.Copy(m_textEncoding.GetBytes(m_synonym2.PadRight(20)), 0, image, 224, 20)
            Array.Copy(m_textEncoding.GetBytes(m_plantID.PadRight(2)), 0, image, 244, 2)
            Array.Copy(BitConverter.GetBytes(m_sourceID), 0, image, 246, 2)
            Array.Copy(BitConverter.GetBytes(m_compressionMinimumTime), 0, image, 248, 4)
            Array.Copy(BitConverter.GetBytes(m_compressionMaximumTime), 0, image, 252, 4)
            Array.Copy(m_textEncoding.GetBytes(m_system.PadRight(4)), 0, image, 256, 4)
            Array.Copy(m_textEncoding.GetBytes(m_email.PadRight(50)), 0, image, 260, 50)
            Array.Copy(m_textEncoding.GetBytes(m_pager.PadRight(30)), 0, image, 310, 30)
            Array.Copy(m_textEncoding.GetBytes(m_phone.PadRight(30)), 0, image, 340, 30)
            Array.Copy(m_textEncoding.GetBytes(m_remarks.PadRight(128)), 0, image, 370, 128)
            Select Case m_generalFlags.PointType
                Case PointType.Analog
                    Array.Copy(m_analogFields.BinaryImage, m_binaryInfo, MetadataAnalogFields.BinaryLength)
                Case PointType.Digital
                    Array.Copy(m_digitalFields.BinaryImage, m_binaryInfo, MetadataDigitalFields.BinaryLength)
                Case PointType.Composed
                    Array.Copy(m_composedFields.BinaryImage, m_binaryInfo, MetadataComposedFields.BinaryLength)
                Case PointType.Constant
                    Array.Copy(m_constantFields.BinaryImage, m_binaryInfo, MetadataConstantFields.BinaryLength)
            End Select
            Array.Copy(m_binaryInfo, 0, image, 498, 256)

            Return image
        End Get
    End Property

    Public Overrides Function Equals(ByVal obj As Object) As Boolean

        Return Me.CompareTo(obj) = 0

    End Function

#Region " IComparable Implementation "

    Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

        Dim other As PointDefinition = TryCast(obj, PointDefinition)
        If other IsNot Nothing Then
            Return m_index.CompareTo(other.Index)
        Else
            Throw New ArgumentException(String.Format("Cannot compare {0} with {1}.", Me.GetType().Name, other.GetType().Name))
        End If

    End Function

#End Region

#End Region

End Class