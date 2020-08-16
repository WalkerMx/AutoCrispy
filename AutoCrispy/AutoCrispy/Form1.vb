Imports System.IO
Imports AutoCrispy.ConfigPackages

Public Class Form1

    Dim WaitScale As Integer = 0
    Dim SettingsLoc As Point = New Point(240, 98)
    Dim PyPaths As New List(Of String)
    Dim PyModels As New List(Of String)
    Dim LoadedPackage As Object
    Dim LoadedPath As String
    Dim LoadedExtensions As String()
    Dim LoadedMode As String
    Dim PyEmbedded As Boolean
    Dim HandOff As Object

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Size = New Size(660, 345)
        LoadBindingString()
        StartUpCheckPy()
        If File.Exists(Application.StartupPath & "\python\python.exe") Then PyEmbedded = True
        If File.Exists(Application.StartupPath() & "\waifu2x-caffe-cui.exe") Then ExeComboBox.Items.Add("Waifu2x Caffe")
        If File.Exists(Application.StartupPath() & "\waifu2x-ncnn-vulkan.exe") Then ExeComboBox.Items.Add("Waifu2x Vulkan")
        If File.Exists(Application.StartupPath() & "\realsr-ncnn-vulkan.exe") Then ExeComboBox.Items.Add("RealSR Vulkan")
        If File.Exists(Application.StartupPath() & "\srmd-ncnn-vulkan.exe") Then ExeComboBox.Items.Add("SRMD Vulkan")
        If File.Exists(Application.StartupPath() & "\waifu2x-converter-cpp.exe") Then ExeComboBox.Items.Add("Waifu2x CPP")
        If File.Exists(Application.StartupPath() & "\Anime4KCPP_CLI.exe") Then ExeComboBox.Items.Add("Anime4k CPP")
        If PyPaths.Count > 0 Then
            ExeComboBox.Items.Add("Python")
            PyScript.SelectedIndex = 0
        End If
        If PyModels.Count > 0 Then
            PyModel.SelectedIndex = 0
        End If
        If ExeComboBox.Items.Count > 0 Then
            ExeComboBox.SelectedIndex = 0
            SetSettingsWindow()
        End If
        WatchDogButton.Select()
    End Sub

    Private Sub Form1_Closing(sender As Object, e As EventArgs) Handles MyBase.Closing
        SaveBindingString()
    End Sub

    Private Sub ExeComboBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ExeComboBox.SelectedIndexChanged
        SetSettingsWindow()
    End Sub

    Private Sub InputBrowse_Click(sender As Object, e As EventArgs) Handles InputBrowse.Click
        InputTextBox.Text = GetFolder()
    End Sub

    Private Sub OutputBrowse_Click(sender As Object, e As EventArgs) Handles OutputBrowse.Click
        OutputTextBox.Text = GetFolder()
    End Sub

    Private Sub RunOnceButton_Click(sender As Object, e As EventArgs) Handles RunOnceButton.Click
        Using OFD As New OpenFileDialog With {.Filter = "Image Files|*.png;*.jpg;*.bmp"}
            If OFD.ShowDialog = DialogResult.OK Then
                Using FBD As New FolderBrowserDialog
                    If FBD.ShowDialog = DialogResult.OK Then
                        Dim SourceImage = {OFD.FileName}
                        Dim TempOutput As String = OutputTextBox.Text
                        OutputTextBox.Text = FBD.SelectedPath
                        PreloadSettings()
                        HandOff = {SourceImage.ToArray, OutputTextBox.Text, GetThreads(), False}
                        WorkHorse.RunWorkerAsync()
                        OutputTextBox.Text = TempOutput
                    End If
                End Using
            End If
        End Using
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles WatchDogButton.Click
        If (Not (Directory.Exists(InputTextBox.Text) = True)) OrElse (Not (Directory.Exists(OutputTextBox.Text) = True)) Then
            MsgBox("No path specified, or path invalid!", MsgBoxStyle.Critical, "Error")
        ElseIf WorkHorse.IsBusy = True Then
            WatchDogButton.Enabled = False
            WorkHorse.CancelAsync()
        Else
            WatchDog.Enabled = Not WatchDog.Enabled
            WatchDogButton.Text = "Running: " & WatchDog.Enabled
            SwitchGroups()
        End If
    End Sub

    Private Sub PyScript_SelectedIndexChanged(sender As Object, e As EventArgs) Handles PyScript.SelectedIndexChanged
        If PyScript.Items.Count > 0 Then
            CheckPyArgs(File.ReadAllText(PyPaths(PyScript.SelectedIndex)).Replace(vbCrLf, " "))
        End If
    End Sub

    Private Sub ThreadComboBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ThreadComboBox.SelectedIndexChanged
        If ThreadComboBox.SelectedIndex = 1 Then
            NumericThreads.Enabled = True
        Else
            NumericThreads.Enabled = False
        End If
    End Sub

    Private Sub WatchDog_Tick(sender As Object, e As EventArgs) Handles WatchDog.Tick
        Dim Source As List(Of String) = GetFileNameListNoExtension(InputTextBox.Text)
        Dim Dest As List(Of String) = GetFileNameListNoExtension(OutputTextBox.Text)
        Dim FileCheck As Boolean = Dest.SequenceEqual(Source)
        If FileCheck = True Or Source.Count = 0 Then
            WaitScale = Math.Min(WaitScale + 1, 100)
            WatchDog.Interval = 1000 + (WaitScale * 590)
        Else
            WaitScale = 0
            WatchDog.Interval = 1000
            PreloadSettings()
            Source = GetFileNameList(InputTextBox.Text)
            Dest = GetFileNameList(OutputTextBox.Text)
            Dim NewImages As New List(Of String)
            Dim DiffImages = Source.Except(Dest)
            For Each NewImage As String In DiffImages
                Dim AcceptExt As Boolean = LoadedExtensions.Contains(Path.GetExtension(NewImage).ToLower)
                If File.Exists(InputTextBox.Text & "\" & NewImage) AndAlso AcceptExt = True Then
                    NewImages.Add(InputTextBox.Text & "\" & NewImage)
                End If
            Next
            If NewImages.Count > 0 Then
                HandOff = {NewImages.ToArray, OutputTextBox.Text, GetThreads(), True}
                WorkHorse.RunWorkerAsync()
            End If
        End If
    End Sub

    Private Sub WorkHorse_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles WorkHorse.DoWork
        WatchDog.Stop()
        If LoadedMode = "Python" Then
            MakePyUpscale(HandOff(0), HandOff(1), HandOff(2))
        Else
            MakeExeUpscale(HandOff(0), HandOff(1), HandOff(2))
        End If
        If WorkHorse.CancellationPending = True Then
            e.Cancel = True
        End If
    End Sub

    Private Sub WorkHorse_ProgressChanged(sender As Object, e As System.ComponentModel.ProgressChangedEventArgs) Handles WorkHorse.ProgressChanged
        UpscaleProgress.Value = e.ProgressPercentage
    End Sub

    Private Sub WorkHorse_RunWorkerCompleted(sender As Object, e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles WorkHorse.RunWorkerCompleted
        UpscaleProgress.Value = 0
        If e.Cancelled = True Then
            WatchDog.Enabled = False
            WatchDogButton.Text = "Running: " & False
            SwitchGroups()
            WatchDogButton.Enabled = True
            Exit Sub
        End If
        If HandOff(3) = True Then
            WatchDog.Start()
        End If
    End Sub

    Private Sub MakeExeUpscale(Source As String(), Dest As String, Limit As Integer)
        For i = 0 To Source.Count - 1
            Dim NewImage As String = Dest & "\" & Path.GetFileName(Source(i))
            Dim BuildProcess As ProcessStartInfo = New ProcessStartInfo(LoadedPath, MakeCommand(Source(i), NewImage))
            BuildProcess.WindowStyle = ProcessWindowStyle.Hidden
            Dim BatchProcess As Process = Process.Start(BuildProcess)
            If (i + 1) Mod Limit = 0 OrElse (i = Source.Count - 1) Then
                BatchProcess.WaitForExit()
            End If
            WorkHorse.ReportProgress(Math.Round(((i * 100) + 1) / Source.Count, 0))
            If WorkHorse.CancellationPending = True Then
                Exit Sub
            End If
        Next
        If CleanupCheckBox.Checked = True Then
            For Each SourceImage As String In Source
                File.Delete(SourceImage)
            Next
        End If
    End Sub

    Private Sub MakePyUpscale(Source As String(), Dest As String, Limit As Integer)
        Dim TempLoc As String = Path.GetTempPath & "aaa" & Path.GetRandomFileName
        Directory.CreateDirectory(TempLoc)
        For i = 0 To Source.Count - 1 Step Limit
            For j = i To i + Limit - 1
                If j <= Source.Count - 1 Then
                    File.Copy(Source(j), TempLoc & "\" & Path.GetFileName(Source(j)))
                End If
            Next
            Dim BuildProcess As ProcessStartInfo
            If PyEmbedded = True Then
                BuildProcess = New ProcessStartInfo(Application.StartupPath & "\python\python.exe", LoadedPath & " " & MakeCommand(TempLoc, Dest))
            Else
                BuildProcess = New ProcessStartInfo(LoadedPath, MakeCommand(TempLoc, Dest))
            End If
            BuildProcess.UseShellExecute = True
            BuildProcess.WindowStyle = ProcessWindowStyle.Minimized
            Dim BatchProcess As Process = Process.Start(BuildProcess)
            BatchProcess.WaitForExit()
            For Each TempImage As String In Directory.GetFiles(TempLoc)
                File.Delete(TempImage)
            Next
            WorkHorse.ReportProgress(Math.Round(((i * 100) + 1) / Source.Count, 0))
            If WorkHorse.CancellationPending = True Then
                Exit Sub
            End If
        Next
        Directory.Delete(TempLoc)
        If CleanupCheckBox.Checked = True Then
            For Each SourceImage As String In Source
                File.Delete(SourceImage)
            Next
        End If
    End Sub

    Private Sub SetSettingsWindow()
        CaffeGroup.Visible = False
        VulkanGroup.Visible = False
        WaifuCPPGroup.Visible = False
        AnimeCPPGroup.Visible = False
        PyGroup.Visible = False
        Select Case ExeComboBox.SelectedItem
            Case "Waifu2x Caffe"
                MoveShowGroup(CaffeGroup)
            Case "Waifu2x Vulkan"
                MoveShowGroup(VulkanGroup)
                VulkanScale.Minimum = 1
                VulkanScale.Maximum = 2
            Case "RealSR Vulkan"
                MoveShowGroup(VulkanGroup)
                VulkanScale.Minimum = 4
                VulkanScale.Maximum = 4
            Case "SRMD Vulkan"
                MoveShowGroup(VulkanGroup)
                VulkanScale.Minimum = 2
                VulkanScale.Maximum = 4
            Case "Waifu2x CPP"
                MoveShowGroup(WaifuCPPGroup)
            Case "Anime4k CPP"
                MoveShowGroup(AnimeCPPGroup)
            Case "Python"
                MoveShowGroup(PyGroup)
        End Select
    End Sub

    Sub MoveShowGroup(ByRef Source As GroupBox)
        Source.Location = SettingsLoc
        Source.Visible = True
    End Sub

    Private Sub SaveBindingString()
        Dim Binding As String = ""
        Binding += ThreadComboBox.SelectedIndex.ToString
        Binding += CaffeMode.SelectedIndex.ToString
        Binding += CaffeProcess.SelectedIndex.ToString
        Binding += VulkanFormat.SelectedIndex.ToString
        Binding += WaifuCPPMode.SelectedIndex.ToString
        Binding += WaifuCPPFormat.SelectedIndex.ToString
        Binding += AnimeCPPFilterType.SelectedIndex.ToString
        For i = 0 To 6
            If AnimeCPPFilters.GetItemChecked(i) = True Then
                Binding += "1"
            Else
                Binding += "0"
            End If
        Next
        My.Settings.BindingString = Binding
    End Sub

    Private Sub LoadBindingString()
        If My.Settings.BindingString.Length <> 14 Then
            My.Settings.BindingString = "02102010001010"
        End If
        ThreadComboBox.SelectedIndex = Val(My.Settings.BindingString(0))
        CaffeMode.SelectedIndex = Val(My.Settings.BindingString(1))
        CaffeProcess.SelectedIndex = Val(My.Settings.BindingString(2))
        VulkanFormat.SelectedIndex = Val(My.Settings.BindingString(3))
        WaifuCPPMode.SelectedIndex = Val(My.Settings.BindingString(4))
        WaifuCPPFormat.SelectedIndex = Val(My.Settings.BindingString(5))
        AnimeCPPFilterType.SelectedIndex = Val(My.Settings.BindingString(6))
        For i = 7 To 13
            If Val(My.Settings.BindingString(i)) = 1 Then
                AnimeCPPFilters.SetItemChecked(i - 7, True)
            End If
        Next
    End Sub

    Private Function MakeCommand(Source As String, Dest As String) As String
        Select Case LoadedMode
            Case "Waifu2x Caffe"
                Return MakeCaffeCommand(Source, Dest, LoadedPackage)
            Case "Waifu2x Vulkan"
                Return MakeVulkanCommand(Source, Dest, LoadedPackage)
            Case "RealSR Vulkan"
                Return MakeVulkanCommand(Source, Dest, LoadedPackage)
            Case "SRMD Vulkan"
                Return MakeVulkanCommand(Source, Dest, LoadedPackage)
            Case "Waifu2x CPP"
                Return MakeCPPCommand(Source, Dest, LoadedPackage)
            Case "Anime4k CPP"
                Return MakeA4KCommand(Source, Dest, LoadedPackage)
            Case "Python"
                Return MakePyCommand(Source, Dest, LoadedPackage)
        End Select
        Return ""
    End Function

    Private Sub PreloadSettings()
        LoadedMode = ExeComboBox.SelectedItem
        Select Case ExeComboBox.SelectedItem
            Case "Waifu2x Caffe"
                LoadedPath = Application.StartupPath() & "\waifu2x-caffe-cui.exe"
                LoadedExtensions = {".png", ".jpg", ".jpeg", ".tif", ".tiff", ".bmp", ".tga"}
                LoadedPackage = MakeCaffePackage()
            Case "Waifu2x Vulkan"
                LoadedPath = Application.StartupPath() & "\waifu2x-ncnn-vulkan.exe"
                LoadedExtensions = {".png", ".webp"}
                LoadedPackage = MakeVulkanPackage()
            Case "RealSR Vulkan"
                LoadedPath = Application.StartupPath() & "\realsr-ncnn-vulkan.exe"
                LoadedExtensions = {".png", ".webp"}
                LoadedPackage = MakeVulkanPackage(True)
            Case "SRMD Vulkan"
                LoadedPath = Application.StartupPath() & "\srmd-ncnn-vulkan.exe"
                LoadedExtensions = {".png", ".webp"}
                LoadedPackage = MakeVulkanPackage()
            Case "Waifu2x CPP"
                LoadedPath = Application.StartupPath() & "\waifu2x-converter-cpp.exe"
                LoadedExtensions = {".bmp", ".dib", ".exr", ".hdr", ".jpe", ".jpeg", ".jpg", ".pbm", ".pgm", ".pic", ".png", ".pnm", ".ppm", ".pxm", ".ras", ".sr", ".tif", ".tiff", ".webp"}
                LoadedPackage = MakeCPPPackage()
            Case "Anime4k CPP"
                LoadedPath = Application.StartupPath() & "\Anime4KCPP_CLI.exe"
                LoadedExtensions = {".png", ".jpg"}
                LoadedPackage = MakeA4KPackage()
            Case "Python"
                LoadedPath = PyPaths(PyScript.SelectedIndex)
                LoadedExtensions = {".png", ".jpg", ".bmp"}
                LoadedPackage = MakePyPackage()
        End Select
    End Sub

    Private Function MakeCaffePackage() As Object
        Return New CaffePackage(CaffeMode.Text.ToLower, CaffeScale.Value, CaffeNoise.Value, CaffeProcess.Text.ToLower, CaffeTAA.Checked)
    End Function

    Private Function MakeVulkanPackage(Optional NoNoise As Boolean = False) As Object
        Return New VulkanPackage(VulkanScale.Value, VulkanNoise.Value, VulkanFormat.Text.ToLower, VulkanTAA.Checked, NoNoise)
    End Function

    Private Function MakeCPPPackage() As Object
        Return New CPPPackage(WaifuCPPMode.Text.ToLower, WaifuCPPScale.Value, WaifuCPPNoise.Value, WaifuCPPFormat.Text.ToLower,
                              WaifuCPPNoGPU.Checked, WaifuCPPOpenCL.Checked, WaifuCPPTTA.Checked)
    End Function

    Private Function MakeA4KPackage() As Object
        Return New A4KPackage(AnimeCPPPasses.Value, AnimeCPPPushColors.Value, AnimeCPPColorStrength.Value,
                              AnimeCPPGradStrength.Value, AnimeCPPScale.Value, AnimeCPPPre.Checked, AnimeCPPPost.Checked,
                              GetFilters, AnimeCPPFilterType.SelectedIndex, AnimeCPPGpu.Checked, AnimeCPPCnn.Checked)
    End Function

    Private Function MakePyPackage() As Object
        Return New PyPackage(PyModels(PyModel.SelectedIndex), PyInputFlag.Text.Trim, PyOutputFlag.Text.Trim, PyArguements)
    End Function

    Private Sub SwitchGroups()
        PathGroup.Enabled = Not WatchDog.Enabled
        SettingsGroup.Enabled = Not WatchDog.Enabled
        CaffeGroup.Enabled = Not WatchDog.Enabled
        VulkanGroup.Enabled = Not WatchDog.Enabled
        WaifuCPPGroup.Enabled = Not WatchDog.Enabled
        AnimeCPPGroup.Enabled = Not WatchDog.Enabled
        PyGroup.Enabled = Not WatchDog.Enabled
    End Sub

    Private Function GetThreads()
        Select Case ThreadComboBox.SelectedIndex
            Case 0
                Return 1
            Case 1
                Return NumericThreads.Value
            Case 2
                Return Environment.ProcessorCount
            Case Else
                Return 4096
        End Select
    End Function

    Private Sub StartUpCheckPy()
        For Each sFile As String In Directory.GetFiles(Application.StartupPath)
            If Path.GetExtension(sFile) = ".pth" Then
                PyModels.Add(sFile)
                PyModel.Items.Add(Path.GetFileName(sFile))
            ElseIf Path.GetExtension(sFile) = ".py" Then
                PyPaths.Add(sFile)
                PyScript.Items.Add(Path.GetFileName(sFile))
            End If
        Next
        If Directory.Exists(Application.StartupPath & "\models") Then
            For Each sFile As String In Directory.GetFiles(Application.StartupPath & "\models")
                If Path.GetExtension(sFile) = ".pth" Then
                    PyModels.Add(sFile)
                    PyModel.Items.Add(Path.GetFileName(sFile))
                End If
            Next
        End If
    End Sub

    Private Sub CheckPyArgs(Source As String)
        PyArguements.Rows.Clear()
        Dim Result As New List(Of String)
        Dim matches As Text.RegularExpressions.MatchCollection = System.Text.RegularExpressions.Regex.Matches(Source, "parser\.add_argument\((.*?)\)")
        For Each m As Text.RegularExpressions.Match In matches
            For Each c As Text.RegularExpressions.Capture In m.Captures
                Result.Add(c.Value.Remove(0, 20).Replace(")", ""))
            Next
        Next
        If Result.Count > 0 Then
            Dim CurrentRow As Integer = 0
            For i = 0 To Result.Count - 1
                Dim ArgCheck = Split(Result(i), ",")
                If Not ArgCheck(0).Contains("model") AndAlso Not ArgCheck(0).Contains("-i") AndAlso Not ArgCheck(0).Contains("-o") Then
                    PyArguements.Rows.Add()
                    PyArguements.Rows(CurrentRow).Cells(0).Value = ArgCheck(0).Replace("'", "")
                    If ArgCheck(1).Split("=")(0).Trim = "default" Then
                        PyArguements.Rows(CurrentRow).Cells(1).Value = ArgCheck(1).Split("=")(1)
                    End If
                    CurrentRow += 1
                End If
            Next
        End If
    End Sub

    Private Function GetFilters() As Integer
        Dim StringResult As String = ""
        For i = 0 To 6
            If AnimeCPPFilters.GetItemChecked(i) = True Then
                StringResult = "1"c & StringResult
            Else
                StringResult = "0"c & StringResult
            End If
        Next
        Return Convert.ToInt32(StringResult, 2)
    End Function

    Private Function GetFolder() As String
        Using FBD As New FolderBrowserDialog
            If FBD.ShowDialog = DialogResult.OK Then
                Return FBD.SelectedPath
            End If
        End Using
        Return ""
    End Function

    Private Function GetFileNameList(Source As String) As List(Of String)
        Dim Result As New List(Of String)
        For Each File As String In Directory.GetFileSystemEntries(Source, "*.*", SearchOption.TopDirectoryOnly).ToList
            Result.Add(Path.GetFileName(File))
        Next
        Return Result
    End Function

    Private Function GetFileNameListNoExtension(Source As String) As List(Of String)
        Dim Result As New List(Of String)
        For Each File As String In Directory.GetFileSystemEntries(Source, "*.*", SearchOption.TopDirectoryOnly).ToList
            Result.Add(Path.GetFileNameWithoutExtension(File))
        Next
        Return Result
    End Function

End Class