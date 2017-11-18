Imports System.Data.SQLite

Public Class DBHelper
    Public Function executeScalar(ByVal sqlQuery As String, ByVal connString As String) As Object
        Try
            Dim con As New SQLiteConnection("Data Source =" & connString)
            con.Open()

            Dim cmd As New SQLiteCommand(sqlQuery, con)

            Return cmd.ExecuteScalar()
        Catch ex As Exception
        End Try

        Return False
    End Function

    Public Function executeNonQuery(ByVal sqlQuery As String, ByVal connString As String) As Boolean
        Try
            Dim val As Integer

            Dim con As New SQLiteConnection("Data Source =" & connString)
            con.Open()

            Dim cmd As New SQLiteCommand(sqlQuery, con)

            val = cmd.ExecuteNonQuery()

            con.Close()

            Return val
        Catch ex As Exception
        End Try

        Return False
    End Function

    Public Function executeDataAdapter(ByVal sqlQuery As String, ByVal connString As String) As DataTable
        Try
            Dim con As New SQLiteConnection("Data Source =" & connString)
            con.Open()

            Dim da As New SQLiteDataAdapter(sqlQuery, con)
            Dim dt As New DataTable

            da.Fill(dt)

            Return dt
        Catch ex As Exception
            Return Nothing
        End Try
    End Function
End Class
