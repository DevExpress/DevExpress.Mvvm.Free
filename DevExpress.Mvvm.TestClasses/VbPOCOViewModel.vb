Public Class VbPOCOViewModel
    Public Overridable Property AutoImlementedProperty As Integer
    Public AutoImlementedPropertyOldValue As Integer
    Protected Sub OnAutoImlementedPropertyChanged(ByVal oldValue As Integer)
        AutoImlementedPropertyOldValue = oldValue
    End Sub


    Public Property AutoImlementedNonVirtualProperty As Integer
    Public Overridable Property AutoImlementedEntityProperty As TestEntity

    Private __PseudoAutoImplementedProperty_WrongFieldName As Integer
    Public Overridable Property PseudoAutoImplementedProperty_WrongFieldName As Integer
        Get
            Return Me.__PseudoAutoImplementedProperty_WrongFieldName
        End Get
        Set(ByVal AutoPropertyValue As Integer)
            Me.__PseudoAutoImplementedProperty_WrongFieldName = AutoPropertyValue
        End Set
    End Property

    Private _PseudoAutoImplementedProperty_NoAttributeOnField As Integer
    Public Overridable Property PseudoAutoImplementedProperty_NoAttributeOnField As Integer
        Get
            Return Me._PseudoAutoImplementedProperty_NoAttributeOnField
        End Get
        Set(ByVal AutoPropertyValue As Integer)
            Me._PseudoAutoImplementedProperty_NoAttributeOnField = AutoPropertyValue
        End Set
    End Property

    <Runtime.CompilerServices.CompilerGenerated> _
    Private _PseudoAutoImplementedProperty As Integer
    Public Overridable Property PseudoAutoImplementedProperty As Integer
        Get
            Return Me._PseudoAutoImplementedProperty
        End Get
        Set(ByVal AutoPropertyValue As Integer)
            Me._PseudoAutoImplementedProperty = AutoPropertyValue
        End Set
    End Property

    <Runtime.CompilerServices.CompilerGenerated> _
    Private _PseudoAutoImplementedProperty_WrongParameterName As Integer
    Public Overridable Property PseudoAutoImplementedProperty_WrongParameterName As Integer
        Get
            Return Me._PseudoAutoImplementedProperty_WrongParameterName
        End Get
        Set(ByVal val As Integer)
            Me._PseudoAutoImplementedProperty_WrongParameterName = val
        End Set
    End Property

    <Runtime.CompilerServices.CompilerGenerated> _
    Private _PseudoAutoImplementedProperty_WrongFieldType As String
    Private __PseudoAutoImplementedProperty_WrongFieldType As Integer
    Public Overridable Property PseudoAutoImplementedProperty_WrongFieldType As Integer
        Get
            Return Me.__PseudoAutoImplementedProperty_WrongFieldType
        End Get
        Set(ByVal AutoPropertyValue As Integer)
            Me.__PseudoAutoImplementedProperty_WrongFieldType = AutoPropertyValue
        End Set
    End Property
End Class

Public Class TestEntity
    Public Overridable Property ID As Integer
End Class
