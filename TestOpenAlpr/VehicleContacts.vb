Imports System.Data.SQLite

Public Class VehicleContacts

    Function add(ByVal name As String, ByVal vehNum As String, ByVal barred As Boolean) As Boolean
        Dim barredInt As Int16 = 0
        If barred Then barredInt = 1
        Dim sql As String = "insert into vehiclet (name, veh_number, barred) values (""" & name & """, """ & vehNum & """, " & barredInt & ")"
        Return mydb.executeNonQuery(sql, dbName)
    End Function

    Function delete(ByVal name As String) As Boolean
        Dim sql As String = "delete from vehiclet where name=""" & name & """"
        Return mydb.executeNonQuery(sql, dbName)
    End Function

    Function update(ByVal name As String, ByVal vehNum As String, ByVal barred As Boolean) As Boolean
        Dim barredInt As Int16 = 0
        If barred Then barredInt = 1
        Dim sql As String = "update vehiclet set name = """ & name & """, veh_number = """ & vehNum & """, barred = " & barredInt & " where name = """ & name & """"
        Return mydb.executeNonQuery(sql, dbName)
    End Function

    Function createDB(ByVal dbPath As String) As Boolean
        SQLiteConnection.CreateFile(dbPath)
        Dim sql As String = "CREATE TABLE vehiclet(ID INTEGER PRIMARY KEY AUTOINCREMENT, Name VARCHAR(25), Veh_Number VARCHAR(25), barred INTEGER DEFAULT 0);"
        Return mydb.executeNonQuery(sql, dbName)
    End Function

    Function getNamesList() As DataTable
        Dim sql As String = "select name from vehiclet"
        Return mydb.executeDataAdapter(sql, dbName)
    End Function

    Function getDetails(ByVal name As String) As DataTable
        Dim sql As String = "select * from vehiclet where name = """ & name & """"
        Return mydb.executeDataAdapter(sql, dbName)
    End Function

    Function isBarred(ByVal vehNum As String) As Boolean
        Dim sql As String = "select barred from vehiclet where veh_number = """ & vehNum & """"
        Dim val As Int16 = CInt(mydb.executeScalar(sql, dbName))

        If Not val = 0 Then
            Return True
        End If

        Return False
    End Function

    Function exists(ByVal name As String) As Boolean
        Dim sql As String = "select case when exists ( select * from vehiclet where name = """ & name & """ ) then 1 else 0 end"

        If CInt(mydb.executeScalar(sql, dbName)) Then
            Return True
        End If

        Return False
    End Function
End Class
