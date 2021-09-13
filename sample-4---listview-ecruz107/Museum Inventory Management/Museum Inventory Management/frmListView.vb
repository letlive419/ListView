Imports System.IO
Public Class frmListView

    Private strFileName As String
    Private dblTotalInvValue As Double
    Private intTotalInvCount As Integer
    Private arrCategories As ArrayList
    Private stats As frmStats

    Private Const ARTID As Integer = 0
    Private Const ARTIST As Integer = 1
    Private Const TITLE As Integer = 2
    Private Const DATE_ACQUIRED As Integer = 3
    Private Const CATEGORY As Integer = 4
    Private Const CONDITION As Integer = 5
    Private Const ITEM_LOCATION As Integer = 6
    Private Const ITEM_VALUE As Integer = 7

    Private Sub openFile()
        Dim intResult As Integer
        ofdOpen.InitialDirectory = Application.StartupPath
        ofdOpen.Filter = "All files (*.*)|*.*| Text files (*.txt)| *.txt"
        ofdOpen.FilterIndex = 2
        intResult = ofdOpen.ShowDialog
        If intResult = DialogResult.Cancel Then
            Exit Sub

        End If
        strFileName = ofdOpen.FileName
        Try
            ReadInputFile(strFileName)
        Catch exNotFound As FileNotFoundException
            MessageBox.Show(exNotFound.ToString)

        Catch exIOError As IOException
            MessageBox.Show(exIOError.ToString)

        Catch exOther As Exception
            MessageBox.Show(exOther.ToString)
        End Try
    End Sub
    Private Sub ReadInputFile(strIn As String)
        Dim fileIn As StreamReader
        Dim strLineIn As String
        Dim strFields() As String
        Dim i As Integer
        lvwInventory.Items.Clear()
        Try
            fileIn = New StreamReader(strIn)
            If Not fileIn.EndOfStream Then
                strLineIn = fileIn.ReadLine
                strFields = strLineIn.Split(",")
                For i = 0 To strFields.Length - 1
                    lvwInventory.columns.add(strFields(i))
                Next

                With lvwInventory
                    .columns(ARTID).width = 80
                    .columns(ARTIST).width = 80
                    .columns(TITLE).width = 150
                    .columns(DATE_ACQUIRED).width = 80
                    .columns(CATEGORY).width = 80
                    .columns(CONDITION).width = 80
                    .columns(ITEM_LOCATION).width = 80
                    .columns(ITEM_VALUE).width = 100
                    .columns(ITEM_VALUE).textAlign = HorizontalAlignment.Right
                End With
            End If


            While Not fileIn.EndOfStream
                strLineIn = fileIn.ReadLine
                strFields = strLineIn.Split(",")
                Dim lviRow As New ListViewItem(strFields(0))
                For i = 1 To strFields.Length - 1
                    Dim lsiCol As New ListViewItem.ListViewSubItem
                    If i <> ITEM_VALUE Then
                        lsiCol.Text = strFields(i)
                    Else
                        lsiCol.Text = FormatCurrency(strFields(i), 0)
                    End If
                    lviRow.SubItems.add(lsiCol)
                Next
                lvwInventory.Items.Add(lviRow)
                UpdateStatistics(lviRow)
            End While
            fileIn.Close()
            fileIn.Dispose()
        Catch ex As Exception
            MessageBox.Show("ReadInputFile: " & ex.ToString)
            Throw ex
        End Try
    End Sub

    Private Sub WriteOutputFile(strName As String)
        Dim fileOut As StreamWriter
        Dim strLineOut As String = ""
        Dim i As Integer
        Dim j As Integer
        Try
            fileOut = New StreamWriter(strName)
            For i = 0 To lvwInventory.Columns.Count - 1
                If i <> lvwInventory.Columns.Count - 1 Then
                    strLineOut &= lvwInventory.Columns(i).Text & ","
                Else
                    strLineOut &= lvwInventory.Columns(i).Text
                End If


            Next
            fileOut.WriteLine(strLineOut)

            For i = 0 To lvwInventory.Items.Count - 1
                strLineOut = lvwInventory.Items(i).Text
                For j = 1 To lvwInventory.Items(i).SubItems.Count - 1
                    strLineOut &= "," & lvwInventory.Items(i).SubItems(j).Text
                Next
                fileOut.WriteLine(strLineOut)
            Next
            fileOut.Close()
            fileOut.Dispose()
        Catch ex As Exception
            Throw ex
        End Try
    End Sub
    Private Sub SaveFile()
        Dim intResult As Integer
        sfdSave.InitialDirectory = Application.StartupPath
        sfdSave.Filter = "All files (*.*)|*.*| Text files (*.txt)| *.txt"
        ofdOpen.FilterIndex = 2
        intResult = sfdSave.ShowDialog
        If intResult = DialogResult.Cancel Then

        End If
        strFileName = sfdSave.FileName
        Try
            WriteOutputFile(strFileName)
        Catch exNotFound As FileNotFoundException

        Catch exIOError As IOException

        Catch exOther As Exception


        End Try

    End Sub
    Private Sub UpdateStatistics(aRow As ListViewItem)
        Dim blnFoundIt As Boolean
        For Each aCat As CCategory In arrCategories
            If aCat.CatName = aRow.SubItems(CATEGORY).Text Then
                aCat.TotalValue += CDbl(aRow.SubItems(ITEM_VALUE).Text)
                aCat.TotalCount += 1
                blnFoundIt = True
                Exit For
            End If
        Next
        If Not blnFoundIt Then
            Dim newCat As New CCategory(aRow.SubItems(CATEGORY).Text, CDbl(aRow.SubItems(ITEM_VALUE).Text))
            arrCategories.Add(newCat)

        End If
        dblTotalInvValue += CDbl(aRow.SubItems(ITEM_VALUE).Text)
        intTotalInvCount += 1

    End Sub

    Private Sub btnLoad_Click(sender As Object, e As EventArgs) Handles btnLoad.Click
        openFile()

    End Sub

    Private Sub mnuOpen_Click(sender As Object, e As EventArgs) Handles mnuOpen.Click
        openFile()

    End Sub

    Private Sub frmListView_Load(sender As Object, e As EventArgs) Handles Me.Load
        arrCategories = New ArrayList
        stats = New frmStats
    End Sub

    Private Sub mnuStats_Click(sender As Object, e As EventArgs) Handles mnuStats.Click
        stats.lstStats.Items.Clear()
        With stats.lstStats
            .Items.Add("Total Inventory Value = " & FormatCurrency(dblTotalInvValue))
            .Items.Add("Total Inventory Count = " & CStr(intTotalInvCount))
            For Each aCat As CCategory In arrCategories
                .Items.Add(aCat.CatName & ":")
                .Items.Add("  Value = " & FormatCurrency(aCat.TotalValue))
                .Items.Add("  Count = " & CStr(aCat.TotalCount))

            Next
        End With
        stats.ShowDialog()
    End Sub

    Private Sub btnExit_Click(sender As Object, e As EventArgs) Handles btnExit.Click
        Application.Exit()
    End Sub

    Private Sub btnClear_Click(sender As Object, e As EventArgs) Handles btnClear.Click
        lvwInventory.Items.Clear()
        dblTotalInvValue = 0
        intTotalInvCount = 0
        arrCategories = New ArrayList
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        SaveFile()

    End Sub

    Private Sub mnuSave_Click(sender As Object, e As EventArgs) Handles mnuSave.Click
        SaveFile()
    End Sub
End Class
