Public Class Form1

    Dim vbQuote As Char = """"c
    Dim CaffePath As String = Application.StartupPath() & "\" & "waifu2x-caffe-cui.exe"
    Dim WaitScale As Integer = 0
    Dim Mode As String = "noise"

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ModeComboBox.SelectedIndex = My.Settings.Mode
        ComputeComboBox.SelectedIndex = My.Settings.Method
        TAAComboBox.SelectedIndex = My.Settings.TAA
    End Sub

    Private Sub Form1_Closing(sender As Object, e As EventArgs) Handles MyBase.Closing
        My.Settings.Mode = ModeComboBox.SelectedIndex
        My.Settings.Method = ComputeComboBox.SelectedIndex
        My.Settings.TAA = TAAComboBox.SelectedIndex
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        InputTextBox.Text = GetFolder()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        OutputTextBox.Text = GetFolder()
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        If (Not (IO.Directory.Exists(InputTextBox.Text) = True)) OrElse (Not (IO.Directory.Exists(OutputTextBox.Text) = True)) Then
            MsgBox("No path specified, or path invalid!", MsgBoxStyle.Critical, "Error")
        ElseIf Not IO.File.Exists(CaffePath) Then
            MsgBox("Waifu2x-Caffe not found!", MsgBoxStyle.Critical, "Error")
        Else
            Button3.Text = "Running: " & Not WatchDog.Enabled
            WatchDog.Enabled = Not WatchDog.Enabled
            GroupBox1.Enabled = Not WatchDog.Enabled
            GroupBox2.Enabled = Not WatchDog.Enabled
            GroupBox3.Enabled = Not WatchDog.Enabled
        End If
    End Sub

    Private Sub ModeComboBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ModeComboBox.SelectedIndexChanged
        Select Case ModeComboBox.SelectedIndex
            Case 0
                NumericNoise.Enabled = True
                NumericScale.Enabled = False
                Mode = "noise"
            Case 1
                NumericNoise.Enabled = False
                NumericScale.Enabled = True
                Mode = "scale"
            Case 2
                NumericNoise.Enabled = True
                NumericScale.Enabled = True
                Mode = "noise_scale"
            Case 3
                NumericNoise.Enabled = True
                NumericScale.Enabled = True
                Mode = "auto_scale"
        End Select
    End Sub

    Private Sub WatchDog_Tick(sender As Object, e As EventArgs) Handles WatchDog.Tick
        Dim Source As New List(Of String)
        For Each File As String In IO.Directory.GetFileSystemEntries(InputTextBox.Text, "*.*", IO.SearchOption.TopDirectoryOnly)
            Source.Add(IO.Path.GetFileName(File))
        Next
        Dim Dest As New List(Of String)
        For Each File As String In IO.Directory.GetFileSystemEntries(OutputTextBox.Text, "*.*", IO.SearchOption.TopDirectoryOnly).ToList
            Dest.Add(IO.Path.GetFileName(File))
        Next
        Dim FileCheck As Boolean = Dest.SequenceEqual(Source)
        If FileCheck = True Or Source.Count = 0 Then
            WaitScale = Math.Min(WaitScale + 1, 100)
            WatchDog.Interval = 1000 + (WaitScale * 590)
        Else
            WaitScale = 0
            WatchDog.Interval = 1000 + (WaitScale * 590)
            Dim NewImages As New List(Of String)
            Dim DiffImages = Source.Except(Dest)
            For Each NewImage As String In DiffImages
                If IO.File.Exists(InputTextBox.Text & "\" & NewImage) AndAlso {".png", ".jpg", ".jpeg", ".tif", ".tiff", ".bmp", ".tga"}.Contains(IO.Path.GetExtension(NewImage).ToLower) Then
                    NewImages.Add(InputTextBox.Text & "\" & NewImage)
                End If
            Next
            If NewImages.Count > 0 Then
                MakeWaifus(NewImages.ToArray)
            End If
        End If
    End Sub

    Private Sub MakeWaifus(Source As String())
        WatchDog.Enabled = False
        Dim BatchText As String = ""
        For Each NewImage As String In Source
            Dim NewImageLoc As String = OutputTextBox.Text & "\" & IO.Path.GetFileName(NewImage)
            BatchText += vbQuote & CaffePath & vbQuote
            BatchText += " -m " & Mode
            BatchText += " -i " & vbQuote & NewImage & vbQuote
            BatchText += " -o " & vbQuote & NewImageLoc & vbQuote
            BatchText += " -n " & NumericNoise.Value
            BatchText += " -s " & NumericScale.Value
            BatchText += " -t " & TAAComboBox.SelectedIndex
            BatchText += " -p " & ComputeComboBox.SelectedItem.ToString.ToLower
            BatchText += " --gpu " & NumericGPU.Value
            BatchText += vbNewLine
        Next
        IO.File.WriteAllText(Application.StartupPath() & "\make.bat", BatchText)
        Dim BuildProcess As New ProcessStartInfo(Application.StartupPath() & "\make.bat")
        BuildProcess.WindowStyle = ProcessWindowStyle.Hidden
        Dim BatchProcess As Process = Process.Start(BuildProcess)
        BatchProcess.WaitForExit()
        IO.File.Delete(Application.StartupPath() & "\make.bat")
        If CheckBox1.Checked = True Then
            For Each OldImage As String In Source
                IO.File.Delete(OldImage)
            Next
        End If
        WatchDog.Enabled = True
    End Sub

    Private Function GetFolder() As String
        Using FBD As New FolderBrowserDialog
            If FBD.ShowDialog = DialogResult.OK Then
                Return FBD.SelectedPath
            End If
        End Using
        Return ""
    End Function

End Class