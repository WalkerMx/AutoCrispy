Imports System.IO

Public Class Form1

    Dim vbQuote As Char = """"c
    Dim WaitScale As Integer = 0
    Dim SettingsLoc As Point = New Point(240, 98)
    Dim VulkanPath As String = Application.StartupPath() & "\waifu2x-ncnn-vulkan.exe"
    Dim CaffePath As String = Application.StartupPath() & "\waifu2x-caffe-cui.exe"
    Dim CPPPath As String = Application.StartupPath() & "\waifu2x-converter-cpp.exe"
    Dim A4KPath As String = Application.StartupPath() & "\Anime4KCPP_CLI.exe"
    Dim CaffeExtensions As String() = {".png", ".jpg", ".jpeg", ".tif", ".tiff", ".bmp", ".tga"}
    Dim VulkanExtensions As String() = {".png", ".webp"}
    Dim CPPExtensions As String() = {".bmp", ".dib", ".exr", ".hdr", ".jpe", ".jpeg", ".jpg", ".pbm", ".pgm", ".pic", ".png", ".pnm", ".ppm", ".pxm", ".ras", ".sr", ".tif", ".tiff", ".webp"}
    Dim A4KExtensions As String() = {".png"}

    Public Structure ArguementString
        Dim Arguements As String
        Public Function GetArguements()
            Return Arguements
        End Function
        Public Sub AddArguement(Flag As String)
            Arguements += " " & Flag
        End Sub
        Public Sub AddArguement(Flag As String, Value As String)
            Arguements += " " & Flag & " " & Value
        End Sub
    End Structure

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Size = New Size(660, 375)
        LoadBindingString()
        If File.Exists(CaffePath) Then ExeComboBox.Items.Add("Waifu2x Caffe")
        If File.Exists(VulkanPath) Then ExeComboBox.Items.Add("Waifu2x Vulkan")
        If File.Exists(CPPPath) Then ExeComboBox.Items.Add("Waifu2x CPP")
        If File.Exists(A4KPath) Then ExeComboBox.Items.Add("Anime4k CPP")
        If ExeComboBox.Items.Count > 0 Then
            ExeComboBox.SelectedIndex = 0
            SetSettingsWindow()
        End If
    End Sub

    Private Sub Form1_Closing(sender As Object, e As EventArgs) Handles MyBase.Closing
        SaveBindingString()
    End Sub

    Private Sub ExeComboBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ExeComboBox.SelectedIndexChanged
        SetSettingsWindow()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles InputBrowse.Click
        InputTextBox.Text = GetFolder()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles OutputBrowse.Click
        OutputTextBox.Text = GetFolder()
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles WatchDogButton.Click
        If (Not (Directory.Exists(InputTextBox.Text) = True)) OrElse (Not (Directory.Exists(OutputTextBox.Text) = True)) Then
            MsgBox("No path specified, or path invalid!", MsgBoxStyle.Critical, "Error")
        ElseIf (Not File.Exists(CaffePath)) AndAlso (Not File.Exists(VulkanPath)) AndAlso (Not File.Exists(CPPPath)) AndAlso (Not File.Exists(A4KPath)) Then
            MsgBox("No compatible executable found!", MsgBoxStyle.Critical, "Error")
        Else
            WatchDog.Enabled = Not WatchDog.Enabled
            WatchDogButton.Text = "Running: " & WatchDog.Enabled
            SwitchGroups()
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
            Source = GetFileNameList(InputTextBox.Text)
            Dest = GetFileNameList(OutputTextBox.Text)
            Dim NewImages As New List(Of String)
            Dim DiffImages = Source.Except(Dest)
            For Each NewImage As String In DiffImages
                Dim AcceptExt As Boolean = False
                Select Case ExeComboBox.SelectedItem
                    Case "Waifu2x Caffe"
                        AcceptExt = CaffeExtensions.Contains(Path.GetExtension(NewImage).ToLower)
                    Case "Waifu2x Vulkan"
                        AcceptExt = VulkanExtensions.Contains(Path.GetExtension(NewImage).ToLower)
                    Case "Waifu2x CPP"
                        AcceptExt = CPPExtensions.Contains(Path.GetExtension(NewImage).ToLower)
                    Case "Anime4k CPP"
                        AcceptExt = A4KExtensions.Contains(Path.GetExtension(NewImage).ToLower)
                End Select
                If File.Exists(InputTextBox.Text & "\" & NewImage) AndAlso AcceptExt = True Then
                    NewImages.Add(InputTextBox.Text & "\" & NewImage)
                End If
            Next
            If NewImages.Count > 0 Then
                Select Case ThreadComboBox.SelectedIndex
                    Case 0
                        MakeWaifus(NewImages.ToArray, 1)
                    Case 1
                        MakeWaifus(NewImages.ToArray, NumericThreads.Value)
                    Case 2
                        MakeWaifus(NewImages.ToArray, Environment.ProcessorCount)
                    Case 3
                        MakeWaifus(NewImages.ToArray, 4096)
                End Select
            End If
        End If
    End Sub

    Private Sub MakeWaifus(Source As String(), Limit As Integer)
        WatchDog.Enabled = False
        Dim BuildProcess As New ProcessStartInfo()
        For i = 0 To Source.Count - 1
            Dim NewImage As String = OutputTextBox.Text & "\" & Path.GetFileName(Source(i))
            Select Case ExeComboBox.SelectedItem
                Case "Waifu2x Caffe"
                    BuildProcess = New ProcessStartInfo(CaffePath, MakeCaffeCommand(Source(i), NewImage).Trim)
                Case "Waifu2x Vulkan"
                    BuildProcess = New ProcessStartInfo(VulkanPath, MakeVulkanCommand(Source(i), NewImage).Trim)
                Case "Waifu2x CPP"
                    BuildProcess = New ProcessStartInfo(CPPPath, MakeCPPCommand(Source(i), NewImage).Trim)
                Case "Anime4k CPP"
                    BuildProcess = New ProcessStartInfo(A4KPath, MakeA4KCommand(Source(i), NewImage).Trim)
            End Select
            If DebugCheckBox.Checked = True Then
                BuildProcess.UseShellExecute = False
                BuildProcess.RedirectStandardInput = True
                BuildProcess.RedirectStandardOutput = True
                BuildProcess.RedirectStandardError = True
            Else
                BuildProcess.WindowStyle = ProcessWindowStyle.Hidden
            End If
            Dim BatchProcess As Process = Process.Start(BuildProcess)
            If DebugCheckBox.Checked = True Then
                Dim DebugOutput As String = BatchProcess.StandardError.ReadToEnd
                If DebugOutput <> "" Then
                    MsgBox(DebugOutput, MsgBoxStyle.Critical, "Error")
                    WatchDogButton.Text = "Running: False"
                    SwitchGroups()
                    Exit Sub
                End If
            End If
            If (i + 1) Mod Limit = 0 OrElse (i = Source.Count - 1) Then
                BatchProcess.WaitForExit()
            End If
        Next
        If CleanupCheckBox.Checked = True Then
            For Each SourceImage As String In Source
                File.Delete(SourceImage)
            Next
        End If
        WatchDog.Enabled = True
    End Sub

    Private Function MakeCaffeCommand(SourceImage As String, NewImage As String) As String
        Dim Result As New ArguementString
        Result.AddArguement("-i", Quote(SourceImage))
        Result.AddArguement("-o", Quote(NewImage))
        Result.AddArguement("-m", CaffeMode.Text.ToLower)
        Result.AddArguement("-s", CaffeScale.Value)
        Result.AddArguement("-n", CaffeNoise.Value)
        Result.AddArguement("-p", CaffeProcess.Text.ToLower)
        Result.AddArguement("-t", IIf(CaffeTAA.Checked = True, 1, 0))
        Return Result.GetArguements
    End Function

    Private Function MakeVulkanCommand(SourceImage As String, NewImage As String) As String
        Dim Result As New ArguementString
        Result.AddArguement("-i", Quote(SourceImage))
        Result.AddArguement("-o", Quote(NewImage))
        Result.AddArguement("-s", VulkanScale.Value)
        Result.AddArguement("-n", VulkanNoise.Value)
        Result.AddArguement("-f", VulkanFormat.Text.ToLower)
        Result.AddArguement(IIf(VulkanTAA.Checked = True, "-x", ""))
        Return Result.GetArguements
    End Function

    Private Function MakeCPPCommand(SourceImage As String, NewImage As String) As String
        Dim Result As New ArguementString
        Result.AddArguement("-i", Quote(SourceImage))
        Result.AddArguement("-o", Quote(NewImage))
        Result.AddArguement("-m", WaifuCPPMode.Text.ToLower)
        Result.AddArguement("--scale-ratio", WaifuCPPScale.Value)
        Result.AddArguement("--noise-level", WaifuCPPNoise.Value)
        Result.AddArguement("-f", WaifuCPPFormat.Text.ToLower)
        Result.AddArguement(IIf(WaifuCPPNoGPU.Checked = True, "--disable-gpu", ""))
        Result.AddArguement(IIf(WaifuCPPOpenCL.Checked = True, "--force-OpenCL", ""))
        Result.AddArguement(IIf(WaifuCPPTTA.Checked = True, "-t", ""))
        Return Result.GetArguements
    End Function

    Private Function MakeA4KCommand(SourceImage As String, NewImage As String) As String
        Dim Result As New ArguementString
        Result.AddArguement("-i", Quote(SourceImage))
        Result.AddArguement("-o", Quote(NewImage))
        Result.AddArguement("-p", AnimeCPPPasses.Value)
        Result.AddArguement("-n", AnimeCPPPushColors.Value)
        Result.AddArguement("-c", AnimeCPPColorStrength.Value)
        Result.AddArguement("-g", AnimeCPPGradStrength.Value)
        Result.AddArguement("-z", AnimeCPPScale.Value)
        Result.AddArguement(IIf(AnimeCPPPre.Checked = True, "-b", ""))
        Result.AddArguement(IIf(AnimeCPPPost.Checked = True, "-a", ""))
        Select Case AnimeCPPFilterType.SelectedIndex
            Case 1
                Result.AddArguement("-r", GetFilters)
            Case 2
                Result.AddArguement("-e", GetFilters)
        End Select
        Result.AddArguement(IIf(AnimeCPPGpu.Checked = True, "-q", ""))
        Result.AddArguement(IIf(AnimeCPPCnn.Checked = True, "-w", ""))
        Result.AddArguement("-A")
        Return Result.GetArguements
    End Function

    Private Sub SetSettingsWindow()
        CaffeGroup.Visible = False
        VulkanGroup.Visible = False
        WaifuCPPGroup.Visible = False
        AnimeCPPGroup.Visible = False
        Select Case ExeComboBox.SelectedItem
            Case "Waifu2x Caffe"
                CaffeGroup.Location = SettingsLoc
                CaffeGroup.Visible = True
            Case "Waifu2x Vulkan"
                VulkanGroup.Location = SettingsLoc
                VulkanGroup.Visible = True
            Case "Waifu2x CPP"
                WaifuCPPGroup.Location = SettingsLoc
                WaifuCPPGroup.Visible = True
            Case "Anime4k CPP"
                AnimeCPPGroup.Location = SettingsLoc
                AnimeCPPGroup.Visible = True
        End Select
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

    Private Sub SwitchGroups()
        PathGroup.Enabled = Not WatchDog.Enabled
        SettingsGroup.Enabled = Not WatchDog.Enabled
        CaffeGroup.Enabled = Not WatchDog.Enabled
        VulkanGroup.Enabled = Not WatchDog.Enabled
        WaifuCPPGroup.Enabled = Not WatchDog.Enabled
        AnimeCPPGroup.Enabled = Not WatchDog.Enabled
    End Sub

    Private Function Quote(Source As String) As String
        Return vbQuote & Source & vbQuote
    End Function

    Private Function GetFilters() As Integer
        Dim StringResult As String = ""
        For i = 0 To 6
            If AnimeCPPFilters.GetItemChecked(i) = True Then
                StringResult += "1"c
            Else
                StringResult += "0"c
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