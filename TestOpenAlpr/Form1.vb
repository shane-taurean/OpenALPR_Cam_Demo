Imports openalprnet
Imports System.IO
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Linq
Imports Emgu.CV
Imports Emgu.CV.Structure
Imports Emgu.CV.UI
Imports Emgu.CV.CvEnum
Imports Emgu.Util
Imports DirectShowLib

Public Class Form1
    Public Class ArgumentType
        Public topN As Int16
        Public filePath As String
    End Class

    Dim vehicleResults As List(Of VehicleResult)
    Dim images As List(Of Image)

    Dim capWebcam As VideoCapture
    Dim capturePaused As Boolean = True

    Dim alpr As AlprNet

    Dim tempImagePath As String = My.Application.Info.DirectoryPath + "\temp_image.jpg"

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        OpenFileDialog1.InitialDirectory = My.Application.Info.DirectoryPath
        cmbMaxResults.SelectedIndex = 0
        cmbMaxVehicles.SelectedIndex = 0
        get_cameras()
        Try
            initOpenAlpr()
        Catch ex As Exception
            'ignore opencl error
        End Try
    End Sub

    Private Sub processImage()
        Try
            If Not BackgroundWorker1.IsBusy And Not IsNothing(capWebcam) And Not IsNothing(ibOriginal.Image) Then
                Dim tempBmp As Bitmap = New Bitmap(ibOriginal.Image.Bitmap)
                Dim tempImage As Image = New Bitmap(tempBmp)
                tempBmp.Dispose()

                tempImage.Save(tempImagePath, ImageFormat.Jpeg)
                'tempImage.Dispose()

                If File.Exists(tempImagePath) Then
                    Dim args As ArgumentType = New ArgumentType
                    args.topN = Val(cmbMaxResults.Text)
                    args.filePath = tempImagePath

                    BackgroundWorker1.RunWorkerAsync(args)
                End If
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        Form2.ShowDialog()
        StatusLabel1.Text = ""
    End Sub

    Public Function boundingRectangle(ByVal points As List(Of Point)) As Rectangle
        Dim minX As Integer = points.Min(Function(p) p.X)
        Dim minY As Integer = points.Min(Function(p) p.Y)
        Dim maxX As Integer = points.Max(Function(p) p.X)
        Dim maxY As Integer = points.Max(Function(p) p.Y)
        Return New Rectangle(New Point(minX, minY), New Size(maxX - minX, maxY - minY))
    End Function

    Private Function cropImage(ByVal img As Image, ByVal cropArea As Rectangle) As Image
        Dim bmpImage As Bitmap = New Bitmap(img)
        img.Dispose()
        Return bmpImage.Clone(cropArea, bmpImage.PixelFormat)
    End Function


    Public Function resizeImage(ByVal image As Image) As Bitmap
        Dim BOXHEIGHT As Int16 = 30
        Dim BOXWIDTH As Int16 = 135

        Dim scaleHeight As Double = CDbl(BOXHEIGHT) / CDbl(image.Height)
        Dim scaleWidth As Double = CDbl(BOXWIDTH) / CDbl(image.Width)

        Dim scale As Double = Math.Min(scaleHeight, scaleWidth)

        Dim resizedImage As Bitmap = New Bitmap(image, CInt(image.Width * scale), CInt(image.Height * scale))

        image.Dispose()

        Return resizedImage
    End Function

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        If Not IsNothing(PictureBox3.Image) Then
            Dim fileName As String = lstBoxVehicles.SelectedItem.ToString
            Dim fileExtenstion As String = ""
            Dim fileType As ImageFormat = Nothing
            SaveFileDialog1.Filter = "Image Files |*.JPG; *.JPEG; *.PNG; *.BMP|JPEG Image |*.JPG; *.JPEG|PNG Image |*.PNG|Bitmap |*.BMP"
            SaveFileDialog1.Title = "Save Image"
            SaveFileDialog1.DefaultExt = "jpg"
            SaveFileDialog1.FileName = fileName

            If SaveFileDialog1.ShowDialog = DialogResult.OK Then
                fileName = SaveFileDialog1.FileName

                If fileName <> "" Then
                    If Not fileName = tempImagePath Then
                        fileExtenstion = Path.GetExtension(fileName).ToLower
                        If fileExtenstion = ".jpg" Or fileExtenstion = ".jpeg" Then
                            fileType = ImageFormat.Jpeg
                        ElseIf fileExtenstion = ".bmp" Then
                            fileType = ImageFormat.Bmp
                        ElseIf fileExtenstion = ".png" Then
                            fileType = ImageFormat.Png
                        End If

                        If Not IsNothing(fileType) Then
                            PictureBox3.Image.Save(fileName, fileType)
                        End If
                    Else
                        MessageBox.Show("please choose a different file name")
                    End If
                End If
            End If
        End If
    End Sub

    Private Sub lstBoxVehicles_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lstBoxVehicles.SelectedIndexChanged
        Try
            If vehicleResults.Any Then
                Dim vehResult As VehicleResult = vehicleResults.Find(Function(x) x.listID = lstBoxVehicles.SelectedIndex)
                PictureBox3.Image = images(vehResult.imageID)
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    Private Sub get_cameras()
        ComboBoxCameraList.DataSource = Nothing
        ComboBoxCameraList.Items.Clear()

        ComboBoxCameraList.DataSource = New BindingSource(getCameraList(), Nothing)
        ComboBoxCameraList.DisplayMember = "Value"
        ComboBoxCameraList.ValueMember = "Key"
    End Sub

    Private Sub disposeCamera()
        Try
            If Not IsNothing(capWebcam) Then
                RemoveHandler Application.Idle, New EventHandler(AddressOf Me.ProcessFrameAndUpdateGUI)
                capWebcam.Stop()
                capWebcam.Dispose()
                capWebcam = Nothing
            End If

            If Not IsNothing(ibOriginal.Image) Then
                ibOriginal.Image.Dispose()
                ibOriginal.Image = Nothing
            End If

            If Not IsNothing(PictureBox1.Image) Then
                PictureBox1.Image.Dispose()
                PictureBox1.Image = Nothing
            End If
        Catch ex As Exception
        End Try
    End Sub


    Sub ProcessFrameAndUpdateGUI(ByVal sender As Object, ByVal arg As EventArgs)
        Try
            If Not capturePaused Then
                Dim imgOriginal As Mat = capWebcam.QueryFrame()

                If (imgOriginal Is Nothing) Then         'if we did not get a frame
                    MessageBox.Show("unable to read frame from webcam" + Environment.NewLine + Environment.NewLine + _
                                    "exiting program")
                    Environment.Exit(0)                                         'and exit program
                    Return
                End If

                If Not IsNothing(ibOriginal.Image) Then
                    ibOriginal.Image.Dispose()
                End If

                ibOriginal.Image = imgOriginal              'update image boxes
            End If
        Catch ex As Exception

        End Try
    End Sub

    Private Sub bttnStartCapture_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles bttnStartCapture.Click
        If bttnStartCapture.Text = "Start Capture" Then
            If IsNothing(capWebcam) Then
                startCapture()
            Else
                capWebcam.Start()
            End If

            bttnStartCapture.Text = "Pause"
            Timer1.Start()
            capturePaused = False
        Else
            bttnStartCapture.Text = "Start Capture"
            capWebcam.Pause()
            Timer1.Stop()
            capturePaused = True
        End If
    End Sub

    Private Sub ComboBoxCameraList_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ComboBoxCameraList.SelectedIndexChanged
        If bttnStartCapture.Text = "Pause" Then
            disposeCamera()
        End If

        bttnStartCapture.Text = "Start Capture"
        If Not IsNothing(ibOriginal.Image) Then
            ibOriginal.Image.Dispose()
            ibOriginal.Image = Nothing
        End If
    End Sub

    Private Sub startCapture()
        If ComboBoxCameraList.Items.Count > 0 And ComboBoxCameraList.SelectedIndex >= 0 Then
            Try
                disposeCamera()
                capWebcam = New VideoCapture(ComboBoxCameraList.SelectedIndex)
                capWebcam.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, 1280)
                capWebcam.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, 720)
                capturePaused = False
                AddHandler Application.Idle, New EventHandler(AddressOf Me.ProcessFrameAndUpdateGUI)
            Catch ex As Exception                                       'catch error if unsuccessful
                'show error via message box
                MessageBox.Show("unable to read from webcam, error: " + Environment.NewLine + Environment.NewLine + _
                                ex.Message + Environment.NewLine + Environment.NewLine + _
                                "exiting program")
                Environment.Exit(0)                                     'and exit program
                Return
            End Try
        End If
    End Sub

    Private Sub initOpenAlpr()
        Try
            Dim confPath As String = My.Application.Info.DirectoryPath & "\openalpr.conf"
            Dim runtimePath As String = My.Application.Info.DirectoryPath & "\runtime_data\"

            alpr = New AlprNet("eu", confPath, runtimePath)
        Catch ex As Exception
        End Try
    End Sub

    Private Sub BackgroundWorker1_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        Try
            Dim topN As Int16 = e.Argument.topN
            Dim filePath As String = e.Argument.filePath

            If Not alpr.IsLoaded Then
                Throw New System.Exception("Error loading openalpr library")
            End If

            alpr.DetectRegion = True
            alpr.TopN = topN

            e.Result = alpr.Recognize(filePath)
        Catch ex As Exception
        End Try
    End Sub

    Private Sub BackgroundWorker1_RunWorkerCompleted(ByVal sender As System.Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted
        Dim foundBanned As Boolean = False
        Try
            Dim results As AlprResultsNet = e.Result

            Dim count As Int16 = 1
            Dim listIndex As Int16 = 0

            If results.Plates.Count > 0 Then
                disposeOldResults()

                'lstBoxVehicles.Text = ""
                lblCaution.Visible = False
                Button2.Enabled = True

                images = New List(Of Image)(results.Plates.Count())
                vehicleResults = New List(Of VehicleResult)

                For Each result As AlprPlateResultNet In results.Plates
                    If count > Val(cmbMaxVehicles.Text) Then
                        Exit For
                    End If

                    Dim rect As Rectangle = boundingRectangle(result.PlatePoints)

                    Dim tmpBmp As Bitmap = New Bitmap(tempImagePath)
                    Dim img As Image = New Bitmap(tmpBmp)
                    tmpBmp.Dispose()

                    Dim cropped As Image = cropImage(img, rect)
                    img.Dispose()
                    cropped = resizeImage(cropped)
                    images.Add(cropped)

                    For Each plate As AlprPlateNet In result.TopNPlates
                        lstBoxVehicles.Items.Add(plate.Characters)

                        If Not foundBanned And vehContact.isBarred(plate.Characters) Then
                            foundBanned = True
                        End If

                        vehicleResults.Add(New VehicleResult(plate.Characters, listIndex, count - 1))

                        listIndex += 1
                    Next

                    count += 1
                Next

                If foundBanned Then
                    lblCaution.Visible = True
                End If

                lstBoxVehicles.SelectedIndex = 0
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub disposeOldResults()
        Try
            Dim tempBmp As Bitmap = New Bitmap(tempImagePath)
            Dim tempImage As Image = New Bitmap(tempBmp)
            tempBmp.Dispose()

            If Not IsNothing(PictureBox1.Image) Then
                PictureBox1.Image.Dispose()
                PictureBox1.Image = Nothing
            End If

            PictureBox1.Image = tempImage

            If Not IsNothing(PictureBox3.Image) Then
                PictureBox3.Image.Dispose()
                PictureBox3.Image = Nothing
            End If

            If Not IsNothing(images) Then
                For Each img As Image In images
                    img.Dispose()
                Next
                images = Nothing
            End If

            If Not IsNothing(vehicleResults) Then
                vehicleResults.Clear()
                vehicleResults = Nothing
            End If

            lstBoxVehicles.Items.Clear()
        Catch ex As Exception
        End Try
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        processImage()
    End Sub
End Class
