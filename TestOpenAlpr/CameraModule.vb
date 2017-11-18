Imports DirectShowLib

Module CameraModule
    Public Function getCameraList() As List(Of KeyValuePair(Of Int16, String))
        Dim ListCamerasData As List(Of KeyValuePair(Of Int16, String)) = New List(Of KeyValuePair(Of Int16, String))

        Dim _SystemCamereas As DsDevice() = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice)

        Dim _DeviceIndex As Int16 = 0

        For Each _Camera As DirectShowLib.DsDevice In _SystemCamereas
            ListCamerasData.Add(New KeyValuePair(Of Int16, String)(_DeviceIndex, _Camera.Name))
            _DeviceIndex += 1
        Next

        Return ListCamerasData
    End Function
End Module
