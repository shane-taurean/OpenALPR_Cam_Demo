Imports System.Data.SQLite
Imports System.IO

Public Class Form2
    Private Sub btnAdd_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAdd.Click
        Dim name As String = txtName.Text
        name = name.ToLower
        name = StrConv(name, VbStrConv.ProperCase)

        Dim vehNum As String = txtVehNum.Text
        Dim barred As Boolean = chkBarred.CheckState

        If name.Length > 0 And vehNum.Length > 0 Then
            If Not vehContact.exists(name) Then
                vehContact.add(name, vehNum, barred)
                updateListBox()
            End If
        End If
    End Sub

    Private Sub btnDelete_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDelete.Click
        Dim name As String = txtName.Text
        name = name.ToLower
        name = StrConv(name, VbStrConv.ProperCase)

        Dim selectedIndex As Int16 = ListBox1.SelectedIndex

        If name.Length > 0 Then
            If vehContact.exists(name) Then
                vehContact.delete(name)
                updateListBox()
                If selectedIndex > 0 Then
                    ListBox1.SelectedIndex = selectedIndex - 1
                Else
                    ListBox1.SelectedIndex = 0
                End If
            End If
        End If
    End Sub


    Private Sub bttnUpdate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles bttnUpdate.Click
        Dim name As String = txtName.Text
        name = name.ToLower
        name = StrConv(name, VbStrConv.ProperCase)

        Dim vehNum As String = txtVehNum.Text
        Dim barred As Boolean = chkBarred.CheckState

        If name.Length > 0 And vehNum.Length > 0 Then
            If vehContact.exists(name) Then
                vehContact.update(name, vehNum, barred)
                updateListBox()
            End If
        End If
    End Sub

    Private Sub bttnListContacts_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles bttnListContacts.Click
        updateListBox()
    End Sub

    Private Sub updateListBox()
        Dim dt As DataTable = vehContact.getNamesList()
        dt.DefaultView.Sort = "Name"

        ListBox1.Items.Clear()

        If dt.Rows.Count > 0 Then
            For Each dtRow As DataRowView In dt.DefaultView
                ListBox1.Items.Add(dtRow("Name").ToString)
            Next
        End If
    End Sub

    Private Sub ListBox1_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ListBox1.SelectedIndexChanged
        If ListBox1.SelectedIndex >= 0 And ListBox1.Items.Count > 0 Then
            Dim dt As DataTable = vehContact.getDetails(ListBox1.SelectedItem.ToString)

            If dt.Rows.Count > 0 Then
                txtName.Text = dt(0)("name")
                txtVehNum.Text = dt(0)("veh_number")
                chkBarred.Checked = dt(0)("barred")
            End If
        End If
    End Sub

    Private Sub Form2_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        If Not File.Exists(dbName) Then
            vehContact.createDB(My.Application.Info.DirectoryPath & "\" & dbName)
        End If
        updateListBox()
    End Sub

    Private Sub bttnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles bttnClose.Click
        Me.Close()
    End Sub
End Class