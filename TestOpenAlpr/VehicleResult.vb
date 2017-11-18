Public Class VehicleResult
    Public number As String
    Public listID As Int16
    Public imageID As Int16

    Public Sub New(ByVal num As String, ByVal listId As Int16, ByVal imageId As Int16)
        Me.number = num
        Me.listID = listId
        Me.imageID = imageId
    End Sub
End Class
