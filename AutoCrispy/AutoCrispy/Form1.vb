Imports System.IO

Public Class Form1

#Region "VARS"

    Dim Root As String = Application.StartupPath
    Dim WaitScale As Integer = 0
    Dim SettingsLoc As Point = New Point(240, 166)
    Dim PyPaths As New List(Of String)
    Dim PyModels As New List(Of String)
    Dim PyEmbedded As Boolean
    Dim ChainedModels As New List(Of Settings)
    Dim HandOff As ExtSettings

    Dim CaffePath As String
    Dim WaifuNcnnPath As String
    Dim RealSRNcnnPath As String
    Dim SRMDNcnnPath As String
    Dim WaifuCppPath As String
    Dim Anime4kPath As String
    Dim EmbeddedPyPath As String

#End Region

#Region "Structs"

    <Xml.Serialization.XmlInclude(GetType(CaffePackage))>
    <Xml.Serialization.XmlInclude(GetType(VulkanPackage))>
    <Xml.Serialization.XmlInclude(GetType(CPPPackage))>
    <Xml.Serialization.XmlInclude(GetType(A4KPackage))>
    <Xml.Serialization.XmlInclude(GetType(PyPackage))>
    <Serializable()> Public Structure Settings
        Public Property LoadedPackage As Object
        Public Property LoadedPath As String
        Public Property LoadedExtensions As String()
        Public Property LoadedMode As String
    End Structure

    <Serializable()> Public Structure ExtSettings
        Public Property InputPath As String
        Public Property OutputPath As String
        Public Property Threads As Integer
        Public Property ResumeTimer As Boolean
        Public Property Defringe As Boolean
        Public Property Threshold As Integer
        Public Sub New(_InputPath As String, _OutputPath As String, _Threads As Integer, _ResumeTimer As Boolean, _Defringe As Boolean, _Threshold As Integer)
            InputPath = _InputPath
            OutputPath = _OutputPath
            Threads = _Threads
            ResumeTimer = _ResumeTimer
            Defringe = _Defringe
            Threshold = _Threshold
        End Sub
    End Structure

    <Serializable()> Public Structure ArguementString
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

    <Serializable()> Public Structure CaffePackage
        Public Property Mode As String
        Public Property Scale As Decimal
        Public Property Noise As Decimal
        Public Property Process As String
        Public Property TAA As Boolean
        Public Sub New(_Mode As String, _Scale As Decimal, _Noise As Decimal, _Process As String, _TAA As Boolean)
            Mode = _Mode
            Scale = _Scale
            Noise = _Noise
            Process = _Process
            TAA = _TAA
        End Sub
    End Structure

    <Serializable()> Public Structure VulkanPackage
        Public Property Scale As Decimal
        Public Property Noise As Decimal
        Public Property Format As String
        Public Property TAA As Boolean
        Public Property NoNoise As Boolean
        Public Sub New(_Scale As Decimal, _Noise As Decimal, _Format As String, _TAA As Boolean, Optional _NoNoise As Boolean = False)
            Scale = _Scale
            Noise = _Noise
            Format = _Format
            TAA = _TAA
            NoNoise = _NoNoise
        End Sub
    End Structure

    <Serializable()> Public Structure CPPPackage
        Public Property Mode As String
        Public Property Scale As Decimal
        Public Property Noise As Decimal
        Public Property Format As String
        Public Property NoGPU As Boolean
        Public Property OpenCL As Boolean
        Public Property TTA As Boolean
        Public Sub New(_Mode As String, _Scale As Decimal, _Noise As Decimal, _Format As String, _NoGPU As Boolean, _OpenCL As Boolean, _TTA As Boolean)
            Mode = _Mode
            Scale = _Scale
            Noise = _Noise
            Format = _Format
            NoGPU = _NoGPU
            OpenCL = _OpenCL
            TTA = _TTA
        End Sub
    End Structure

    <Serializable()> Public Structure A4KPackage
        Public Property Passes As Decimal
        Public Property PushColors As Decimal
        Public Property ColorStrength As Decimal
        Public Property GradStrength As Decimal
        Public Property Scale As Decimal
        Public Property Pre As Boolean
        Public Property Post As Boolean
        Public Property Filter As Integer
        Public Property FilterType As Integer
        Public Property GPU As Boolean
        Public Property CNN As Boolean
        Public Sub New(_Passes As Decimal, _PushColors As Decimal, _ColorStrength As Decimal, _GradStrength As Decimal, _Scale As Decimal, _Pre As Boolean, _Post As Boolean, _Filter As Integer, _FilterType As Integer, _GPU As Boolean, _CNN As Boolean)
            Passes = _Passes
            PushColors = _PushColors
            ColorStrength = _ColorStrength
            GradStrength = _GradStrength
            Scale = _Scale
            Pre = _Pre
            Post = _Post
            Filter = _Filter
            FilterType = _FilterType
            GPU = _GPU
            CNN = _CNN
        End Sub
    End Structure

    <Serializable()> Public Structure PyPackage
        Public Property Model As String
        Public Property InputFlag As String
        Public Property OutputFlag As String
        Public Property Arguements As List(Of String)
        Public Sub New(_Model As String, _InputFlag As String, _OutputFlag As String, Data As DataGridView)
            Model = _Model
            InputFlag = _InputFlag
            OutputFlag = _OutputFlag
            Arguements = New List(Of String)
            For i = 0 To Data.Rows.Count - 2
                If Not Data.Rows(i).Cells(0).Value = Nothing Then
                    If Data.Rows(i).Cells(1).Value = Nothing Then
                        Arguements.Add(Data.Rows(i).Cells(0).Value.ToString)
                        Arguements.Add("")
                    Else
                        Arguements.Add(Data.Rows(i).Cells(0).Value.ToString)
                        Arguements.Add(Data.Rows(i).Cells(1).Value.ToString)
                    End If
                End If
            Next
        End Sub
    End Structure

#End Region

#Region "Loading"

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Size = New Size(660, 413)
        Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
        Application.CurrentCulture = New Globalization.CultureInfo("EN-US")
        LoadBindingString()
        PreloadImageList()
        StartUpCheckEXE()
        StartUpCheckPy()
        If ExeComboBox.Items.Count > 0 Then
            ExeComboBox.SelectedIndex = 0
            SetSettingsWindow()
        End If
        TabGroup.Refresh()
        WatchDogButton.Select()
    End Sub

    Private Sub Form1_Closing(sender As Object, e As EventArgs) Handles MyBase.Closing
        SaveBindingString()
    End Sub

    Private Sub StartUpCheckEXE()
        Dim RootFolders As List(Of String) = Directory.GetDirectories(Root).ToList
        RootFolders.Add(Root)
        For Each Folder As String In RootFolders
            If File.Exists(Folder & "\waifu2x-caffe-cui.exe") Then
                ExeComboBox.Items.Add("Waifu2x Caffe")
                CaffePath = Folder & "\waifu2x-caffe-cui.exe"
            End If
            If File.Exists(Folder & "\waifu2x-ncnn-vulkan.exe") Then
                ExeComboBox.Items.Add("Waifu2x Vulkan")
                WaifuNcnnPath = Folder & "\waifu2x-ncnn-vulkan.exe"
            End If
            If File.Exists(Folder & "\realsr-ncnn-vulkan.exe") Then
                ExeComboBox.Items.Add("RealSR Vulkan")
                RealSRNcnnPath = Folder & "\realsr-ncnn-vulkan.exe"
            End If
            If File.Exists(Folder & "\srmd-ncnn-vulkan.exe") Then
                ExeComboBox.Items.Add("SRMD Vulkan")
                SRMDNcnnPath = Folder & "\srmd-ncnn-vulkan.exe"
            End If
            If File.Exists(Folder & "\waifu2x-converter-cpp.exe") Then
                ExeComboBox.Items.Add("Waifu2x CPP")
                WaifuCppPath = Folder & "\waifu2x-converter-cpp.exe"
            End If
            If File.Exists(Folder & "\Anime4KCPP_CLI.exe") Then
                ExeComboBox.Items.Add("Anime4k CPP")
                Anime4kPath = Folder & "\Anime4KCPP_CLI.exe"
            End If
            If File.Exists(Folder & "\python\python.exe") Then
                PyEmbedded = True
                EmbeddedPyPath = Folder & "\python\python.exe"
            End If
        Next
    End Sub

    Private Sub StartUpCheckPy()
        Dim RootFolders As List(Of String) = Directory.GetDirectories(Root).ToList
        RootFolders.Add(Root)
        For Each Folder As String In RootFolders
            Dim PyFiles As String() = Directory.EnumerateFiles(Folder, "*.py").ToArray
            For Each PythonScript As String In PyFiles
                PyPaths.Add(PythonScript)
                PyScript.Items.Add(Path.GetFileName(PythonScript))
            Next
            For Each SubFolder As String In Directory.GetDirectories(Folder)
                Dim Models As String() = Directory.EnumerateFiles(SubFolder, "*.pth").ToArray
                For Each PythonModel As String In Models
                    PyModels.Add(PythonModel)
                    PyModel.Items.Add(Path.GetFileName(PythonModel))
                Next
            Next
        Next
        If PyPaths.Count > 0 Then
            ExeComboBox.Items.Add("Python")
            PyScript.SelectedIndex = 0
            If PyModels.Count > 0 Then
                PyModel.SelectedIndex = 0
            Else
                MsgBox("No ESRGAN Models Found!", MsgBoxStyle.Critical)
            End If
        End If
    End Sub

    Private Sub PreloadImageList()
        ChainThumbs.Images.Add(Shrink(My.Resources._0, 64, 64))
        ChainThumbs.Images.Add(Shrink(My.Resources._1, 64, 64))
        ChainThumbs.Images.Add(Shrink(My.Resources._2, 64, 64))
        ChainThumbs.Images.Add(Shrink(My.Resources._3, 64, 64))
        ChainThumbs.Images.Add(Shrink(My.Resources._4, 64, 64))
        ChainThumbs.Images.Add(Shrink(My.Resources._5, 64, 64))
        ChainThumbs.Images.Add(Shrink(My.Resources._6, 64, 64))
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

#End Region

#Region "UI"

    Private Sub ExeComboBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ExeComboBox.SelectedIndexChanged
        SetSettingsWindow()
    End Sub

    Private Sub InputBrowse_Click(sender As Object, e As EventArgs) Handles InputBrowse.Click
        InputTextBox.Text = GetFolder()
    End Sub

    Private Sub OutputBrowse_Click(sender As Object, e As EventArgs) Handles OutputBrowse.Click
        OutputTextBox.Text = GetFolder()
    End Sub

    Private Sub DefringeCheck_CheckedChanged(sender As Object, e As EventArgs) Handles DefringeCheck.CheckedChanged
        DefringeThresh.Enabled = DefringeCheck.Checked
    End Sub

    Private Sub ChainSave_Click(sender As Object, e As EventArgs) Handles ChainSave.Click
        Using SFD As New SaveFileDialog With {.Filter = "XML Files|*.xml|All Files|*.*"}
            If SFD.ShowDialog = DialogResult.OK Then
                File.WriteAllText(SFD.FileName, Serialize(ChainedModels))
            End If
        End Using
    End Sub

    Private Sub ChainLoad_Click(sender As Object, e As EventArgs) Handles ChainLoad.Click
        Using OFD As New OpenFileDialog With {.Filter = "XML Files|*.xml|All Files|*.*"}
            If OFD.ShowDialog = DialogResult.OK Then
                ChainPreview.Clear()
                ChainedModels.Clear()
                ChainedModels = Deserialize(Of List(Of Settings))(File.ReadAllText(OFD.FileName))
                For Each Model As Settings In ChainedModels
                    AddModelToChain(Model.LoadedMode)
                Next
            End If
        End Using
    End Sub

    Private Sub ChainAdd_Click(sender As Object, e As EventArgs) Handles ChainAdd.Click
        PackageSettings()
        AddModelToChain(ExeComboBox.SelectedItem)
    End Sub

    Private Sub RemoveItemFromChain(sender As Object, e As EventArgs) Handles ChainRemove.Click, ChainContextDelete.Click
        If ChainPreview.SelectedIndices.Count > 0 Then
            Dim Remove As Integer = ChainPreview.SelectedIndices(0)
            ChainPreview.Items.RemoveAt(Remove)
            ChainedModels.RemoveAt(Remove)
            ChainPreview.AutoArrange = True
            ChainPreview.AutoArrange = False
        End If
    End Sub

    Private Sub ChainContextEdit_Click(sender As Object, e As EventArgs) Handles ChainContextEdit.Click
        If ChainPreview.SelectedItems.Count > 0 Then
            Dim ItemIndex = (ChainPreview.SelectedItems(0).Position.X - 21) / (ChainPreview.SelectedItems(0).Bounds.Width + 1)
            Using ECD As New EditChainDialog(Serialize(ChainedModels(ItemIndex)))
                If ECD.ShowDialog = DialogResult.OK Then
                    Try
                        Dim NewSettings As Settings = Deserialize(Of Settings)(ECD.ResultText)
                        ChainedModels(ItemIndex) = NewSettings
                    Catch ex As Exception
                        MsgBox("Error: New settings could not be parsed.")
                    End Try
                End If
            End Using
        End If
    End Sub

    Private Sub ChainPreview_ItemDrag(ByVal sender As Object, ByVal e As ItemDragEventArgs) Handles ChainPreview.ItemDrag
        Dim lvi As ListViewItem = CType(e.Item, ListViewItem)
        ChainPreview.DoDragDrop(New DataObject("System.Windows.Forms.ListViewItem", lvi), DragDropEffects.Move)
    End Sub

    Private Sub ChainPreview_DragEnter(ByVal sender As Object, ByVal e As DragEventArgs) Handles ChainPreview.DragEnter
        If e.Data.GetDataPresent("System.Windows.Forms.ListViewItem") Then
            e.Effect = DragDropEffects.Move
        End If
    End Sub

    Private Sub ChainPreview_DragOver(ByVal sender As Object, ByVal e As DragEventArgs) Handles ChainPreview.DragOver
        Dim lvi As ListViewItem = CType(e.Data.GetData("System.Windows.Forms.ListViewItem"), ListViewItem)
        If lvi IsNot Nothing Then
            Dim Offset As Size = Size.Subtract(Cursor.Size, New Size(Cursor.HotSpot.X, Cursor.HotSpot.Y))
            lvi.Position = Point.Subtract(ChainPreview.PointToClient(New Point(e.X, e.Y)), Offset)
            e.Effect = DragDropEffects.Move
        End If
    End Sub

    Private Sub ChainPreview_MouseUp(sender As Object, e As MouseEventArgs) Handles ChainPreview.MouseUp
        ChainPreview.AutoArrange = True
        ChainPreview.AutoArrange = False
        Dim Result As New List(Of String)
        For Each Model As ListViewItem In ChainPreview.Items
            Result.Add((Model.Position.X - 21) / (Model.Bounds.Width + 1) & "|" & Model.Index)
        Next
        Result.Sort()
        Dim NeuModelOrder As New List(Of Settings)
        For Each ReOrder As String In Result
            NeuModelOrder.Add(ChainedModels(ReOrder.Split("|")(1)))
        Next
        ChainedModels = NeuModelOrder
    End Sub

    Private Sub RunOnceButton_Click(sender As Object, e As EventArgs) Handles RunOnceButton.Click
        Using OFD As New OpenFileDialog With {.Filter = "Image Files|*.png;*.jpg;*.bmp"}
            If OFD.ShowDialog = DialogResult.OK Then
                Using SFD As New SaveFileDialog With {.Filter = "PNG Images|*.png"}
                    If SFD.ShowDialog = DialogResult.OK Then
                        Dim TempPath As String = Path.GetTempPath & "Single_0"
                        Directory.CreateDirectory(Path.GetTempPath & "Single_0")
                        File.Copy(OFD.FileName, TempPath & "\" & Path.GetFileName(SFD.FileName))
                        HandOff = New ExtSettings(TempPath, Directory.GetParent(SFD.FileName).FullName, GetThreads(), False, DefringeCheck.Checked, DefringeThresh.Value)
                        If ChainPreview.Items.Count = 0 Then
                            PackageSettings()
                        End If
                        SwitchGroups(False)
                        WorkHorse.RunWorkerAsync()
                    End If
                End Using
            End If
        End Using
    End Sub

    Private Sub WatchDogButton_Click(sender As Object, e As EventArgs) Handles WatchDogButton.Click
        If (Not (Directory.Exists(InputTextBox.Text) = True)) OrElse (Not (Directory.Exists(OutputTextBox.Text) = True)) Then
            MsgBox("No path specified, or path invalid!", MsgBoxStyle.Critical, "Error")
        ElseIf WorkHorse.IsBusy = True Then
            WatchDogButton.Enabled = False
            WorkHorse.CancelAsync()
        Else
            WatchDog.Enabled = Not WatchDog.Enabled
            WatchDogButton.Text = "Running: " & WatchDog.Enabled
            SwitchGroups(Not WatchDog.Enabled)
        End If
    End Sub

    Private Sub PyScript_SelectedIndexChanged(sender As Object, e As EventArgs) Handles PyScript.SelectedIndexChanged
        If PyScript.Items.Count > 0 Then
            CheckPyArgs(File.ReadAllText(PyPaths(PyScript.SelectedIndex)).Replace(vbCr, " ").Replace(vbLf, " ").Replace(vbCrLf, " "))
        End If
    End Sub

    Private Sub ThreadComboBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ThreadComboBox.SelectedIndexChanged
        If ThreadComboBox.SelectedIndex = 1 Then
            NumericThreads.Enabled = True
        Else
            NumericThreads.Enabled = False
        End If
    End Sub

    Private Sub SetSettingsWindow()
        CaffeGroup.Visible = False
        VulkanGroup.Visible = False
        WaifuCPPGroup.Visible = False
        AnimeCPPGroup.Visible = False
        PyGroup.Visible = False
        VulkanNoise.Enabled = True
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
                VulkanNoise.Enabled = False
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

    Private Sub SwitchGroups(Enabled As Boolean)
        TabGroup.Enabled = Enabled
        SettingsGroup.Enabled = Enabled
        CaffeGroup.Enabled = Enabled
        VulkanGroup.Enabled = Enabled
        WaifuCPPGroup.Enabled = Enabled
        AnimeCPPGroup.Enabled = Enabled
        PyGroup.Enabled = Enabled
    End Sub

#End Region

#Region "Background"

    Private Sub WatchDog_Tick(sender As Object, e As EventArgs) Handles WatchDog.Tick
        Dim Source As List(Of String) = GetFileNameList(InputTextBox.Text, False)
        Dim Dest As List(Of String) = GetFileNameList(OutputTextBox.Text, False)
        Dim FileCheck As Boolean = Dest.SequenceEqual(Source)
        If FileCheck = True Or Source.Count = 0 Then
            WaitScale = Math.Min(WaitScale + 1, 100)
            WatchDog.Interval = 1000 + (WaitScale * 590)
        Else
            WaitScale = 0
            WatchDog.Interval = 1000
            HandOff = New ExtSettings(InputTextBox.Text, OutputTextBox.Text, GetThreads(), True, DefringeCheck.Checked, DefringeThresh.Value)
            If ChainPreview.Items.Count = 0 Then
                PackageSettings()
            End If
            WorkHorse.RunWorkerAsync()
        End If
    End Sub

    Private Sub WorkHorse_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles WorkHorse.DoWork
        WatchDog.Stop()
        MakeUpscale()
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
            SwitchGroups(Not WatchDog.Enabled)
            WatchDogButton.Enabled = True
            If ChainPreview.Items.Count = 0 Then
                ChainedModels.Clear()
            End If
            Exit Sub
        End If
        If ChainPreview.Items.Count = 0 Then
            ChainedModels.Clear()
        End If
        If HandOff.ResumeTimer = True Then
            WatchDog.Start()
        Else
            Directory.Delete(Path.GetTempPath & "Single_0", True)
            SwitchGroups(True)
        End If
    End Sub

#End Region

#Region "Upscale Routine"

    Private Sub MakeUpscale()
        Dim TempPath As String = Path.GetTempPath & "Temp_0"
        Dim Source = Directory.GetFiles(HandOff.InputPath).Except(Directory.GetFiles(HandOff.OutputPath))
        For i = 0 To Source.Count - 1 Step HandOff.Threads
            Dim ChainPaths As New List(Of String)
            Dim DeletedChainPaths As New List(Of String)
            ChainPaths.Add(TempPath)
            For j = 0 To ChainedModels.Count - 2
                Dim TempName As String = Path.GetTempPath & "Chain_" & j
                ChainPaths.Add(TempName)
                Directory.CreateDirectory(TempName)
            Next
            ChainPaths.Add(HandOff.OutputPath)
            Directory.CreateDirectory(TempPath)
            For j = i To i + HandOff.Threads - 1
                If j <= Source.Count - 1 Then
                    File.Copy(Source(j), TempPath & "\" & Path.GetFileName(Source(j)))
                End If
            Next
            For Each Model In ChainedModels
                Dim NewImages As New List(Of String)
                Dim DiffImages = GetFileNameList(ChainPaths(0), True).Except(GetFileNameList(HandOff.OutputPath, True))
                For Each NewImage As String In DiffImages
                    Dim AcceptExt As Boolean = Model.LoadedExtensions.Contains(Path.GetExtension(NewImage).ToLower)
                    If File.Exists(ChainPaths(0) & "\" & NewImage) AndAlso AcceptExt = True Then
                        NewImages.Add(ChainPaths(0) & "\" & NewImage)
                    End If
                Next
                If NewImages.Count > 0 Then
                    If Model.LoadedMode = "Python" Then
                        Dim BuildProcess As ProcessStartInfo
                        If PyEmbedded = True Then
                            BuildProcess = New ProcessStartInfo(EmbeddedPyPath, Quote(Model.LoadedPath) & " " & MakeCommand(ChainPaths(0), ChainPaths(1), Model))
                        Else
                            BuildProcess = New ProcessStartInfo(Model.LoadedPath, MakeCommand(ChainPaths(0), ChainPaths(1), Model))
                        End If
                        BuildProcess.UseShellExecute = True
                        BuildProcess.WindowStyle = ProcessWindowStyle.Minimized
                        Dim BatchProcess As Process = Process.Start(BuildProcess)
                        BatchProcess.WaitForExit()
                    Else
                        For j = 0 To NewImages.Count - 1
                            Dim NewImage As String = ChainPaths(1) & "\" & Path.GetFileName(NewImages(j))
                            Dim BuildProcess As ProcessStartInfo = New ProcessStartInfo(Path.GetFileName(Model.LoadedPath), MakeCommand(NewImages(j), NewImage, Model))
                            BuildProcess.WorkingDirectory = Directory.GetParent(Model.LoadedPath).FullName
                            BuildProcess.WindowStyle = ProcessWindowStyle.Hidden
                            Dim BatchProcess As Process = Process.Start(BuildProcess)
                            If (j + 1) Mod HandOff.Threads = 0 OrElse (j = NewImages.Count - 1) Then
                                BatchProcess.WaitForExit()
                            End If
                        Next
                    End If
                    For Each TempImage As String In Directory.GetFiles(ChainPaths(0))
                        File.Delete(TempImage)
                    Next
                End If
                DeletedChainPaths.Add(ChainPaths(0))
                ChainPaths.RemoveAt(0)
                If WorkHorse.CancellationPending = True Then
                    Exit Sub
                End If
            Next
            For Each ChainDir As String In DeletedChainPaths
                Directory.Delete(ChainDir, True)
            Next
            WorkHorse.ReportProgress(Math.Round(((i * 100) + 1) / Source.Count, 0))
        Next
        If HandOff.Defringe = True Then
            For Each DestImage As String In Directory.EnumerateFiles(HandOff.OutputPath, "*.png")
                Defringe(HandOff.OutputPath & "\" & Path.GetFileName(DestImage))
            Next
        End If
        If CleanupCheckBox.Checked = True Then
            For Each SourceImage As String In Directory.GetFiles(HandOff.InputPath)
                File.Delete(SourceImage)
            Next
        End If
    End Sub

#End Region

#Region "Packageing"

    Private Function MakeCommand(Source As String, Dest As String, Setting As Settings) As String
        Select Case Setting.LoadedMode
            Case "Waifu2x Caffe"
                Return MakeCaffeCommand(Source, Dest, Setting.LoadedPackage)
            Case "Waifu2x Vulkan"
                Return MakeVulkanCommand(Source, Dest, Setting.LoadedPackage)
            Case "RealSR Vulkan"
                Return MakeVulkanCommand(Source, Dest, Setting.LoadedPackage)
            Case "SRMD Vulkan"
                Return MakeVulkanCommand(Source, Dest, Setting.LoadedPackage)
            Case "Waifu2x CPP"
                Return MakeCPPCommand(Source, Dest, Setting.LoadedPackage)
            Case "Anime4k CPP"
                Return MakeA4KCommand(Source, Dest, Setting.LoadedPackage)
            Case "Python"
                Return MakePyCommand(Source, Dest, Setting.LoadedPackage)
        End Select
        Return ""
    End Function

    Private Sub AddModelToChain(Mode As String)
        Select Case Mode
            Case "Waifu2x Caffe"
                ChainPreview.Items.Add(New ListViewItem("Caffe", 0))
            Case "Waifu2x Vulkan"
                ChainPreview.Items.Add(New ListViewItem("Waifu Vulkan", 1))
            Case "RealSR Vulkan"
                ChainPreview.Items.Add(New ListViewItem("RealSR Vulkan", 2))
            Case "SRMD Vulkan"
                ChainPreview.Items.Add(New ListViewItem("SRMD Vulkan", 3))
            Case "Waifu2x CPP"
                ChainPreview.Items.Add(New ListViewItem("Waifu CPP", 4))
            Case "Anime4k CPP"
                ChainPreview.Items.Add(New ListViewItem("Anime4K", 5))
            Case "Python"
                ChainPreview.Items.Add(New ListViewItem("Python", 6))
        End Select
    End Sub

    Private Sub PackageSettings()
        Dim Pack As New Settings
        Pack.LoadedMode = ExeComboBox.SelectedItem
        Select Case ExeComboBox.SelectedItem
            Case "Waifu2x Caffe"
                Pack.LoadedPath = CaffePath
                Pack.LoadedExtensions = {".png", ".jpg", ".jpeg", ".tif", ".tiff", ".bmp", ".tga"}
                Pack.LoadedPackage = MakeCaffePackage()
            Case "Waifu2x Vulkan"
                Pack.LoadedPath = WaifuNcnnPath
                Pack.LoadedExtensions = {".png", ".webp", ".jpg"}
                Pack.LoadedPackage = MakeVulkanPackage()
            Case "RealSR Vulkan"
                Pack.LoadedPath = RealSRNcnnPath
                Pack.LoadedExtensions = {".png", ".webp", ".jpg"}
                Pack.LoadedPackage = MakeVulkanPackage(True)
            Case "SRMD Vulkan"
                Pack.LoadedPath = SRMDNcnnPath
                Pack.LoadedExtensions = {".png", ".webp", ".jpg"}
                Pack.LoadedPackage = MakeVulkanPackage()
            Case "Waifu2x CPP"
                Pack.LoadedPath = WaifuCppPath
                Pack.LoadedExtensions = {".bmp", ".dib", ".exr", ".hdr", ".jpe", ".jpeg", ".jpg", ".pbm", ".pgm", ".pic", ".png", ".pnm", ".ppm", ".pxm", ".ras", ".sr", ".tif", ".tiff", ".webp"}
                Pack.LoadedPackage = MakeCPPPackage()
            Case "Anime4k CPP"
                Pack.LoadedPath = Anime4kPath
                Pack.LoadedExtensions = {".png", ".jpg"}
                Pack.LoadedPackage = MakeA4KPackage()
            Case "Python"
                Pack.LoadedPath = PyPaths(PyScript.SelectedIndex)
                Pack.LoadedExtensions = {".png", ".jpg", ".bmp"}
                Pack.LoadedPackage = MakePyPackage()
        End Select
        ChainedModels.Add(Pack)
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

#End Region

#Region "Graphics"

    Private Sub Defringe(Source As String)
        Dim SourceImage As Bitmap = Image.FromFile(Source)
        Dim NewImage As New Bitmap(SourceImage)
        SourceImage.Dispose()
        LockbitsDefringe(NewImage, HandOff.Threshold)
        NewImage.Save(Source)
    End Sub

    Private Sub LockbitsDefringe(ByRef Source As Bitmap, Threshold As Integer)
        Dim rect As Rectangle = New Rectangle(0, 0, Source.Width, Source.Height)
        Dim bmpData As Imaging.BitmapData = Source.LockBits(rect, Imaging.ImageLockMode.ReadWrite, Imaging.PixelFormat.Format32bppArgb)
        Dim ptr As IntPtr = bmpData.Scan0
        Dim bytes As Integer = Math.Abs(bmpData.Stride) * Source.Height
        Dim rgbValues As Byte() = New Byte(bytes - 1) {}
        Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes)
        For i = 3 To rgbValues.Length - 1 Step 4
            If rgbValues(i) < Threshold Then
                rgbValues(i) = 0
            ElseIf rgbValues(i) < 255 Then
                rgbValues(i) = 255
            End If
        Next
        Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes)
        Source.UnlockBits(bmpData)
    End Sub

    Private Function Shrink(Source As Image, Width As Integer, Height As Integer) As Image
        Dim SmallImage As New Bitmap(Width, Height)
        SmallImage.SetResolution(300, 300)
        Using g As Graphics = Graphics.FromImage(SmallImage)
            g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            g.DrawImage(Source, 0, 0, Width, Height)
        End Using
        Return SmallImage
    End Function

#End Region

#Region "XML"

    Public Shared Function Serialize(Of T)(ByVal Source As T) As String
        Dim Result As String = ""
        Using XmlStream As New MemoryStream
            Dim XmlSerializer As New Xml.Serialization.XmlSerializer(GetType(T))
            Dim XmlSettings As New Xml.XmlWriterSettings With {.Indent = True, .CloseOutput = True}
            Dim XmlWriter As Xml.XmlWriter = Xml.XmlWriter.Create(XmlStream, XmlSettings)
            XmlSerializer.Serialize(XmlWriter, Source)
            Dim XmlReader As New StreamReader(XmlStream)
            XmlStream.Position = 0
            Result = XmlReader.ReadToEnd()
            XmlWriter.Flush()
            XmlWriter.Close()
            XmlReader.Dispose()
        End Using
        Return Result
    End Function

    Public Shared Function Deserialize(Of T)(ByVal Xml As String) As T
        Dim Result As New Object
        Using XmlStream As New MemoryStream
            Dim XmlSerializer As New Xml.Serialization.XmlSerializer(GetType(T))
            Dim XmlWriter As New StreamWriter(XmlStream)
            XmlWriter.Write(Xml)
            XmlWriter.Flush()
            XmlStream.Position = 0
            Dim XmlReader As New Xml.XmlTextReader(XmlStream)
            If XmlSerializer.CanDeserialize(XmlReader) Then
                Result = DirectCast(XmlSerializer.Deserialize(XmlReader), T)
            End If
            XmlWriter.Close()
            XmlReader.Dispose()
        End Using
        Return Result
    End Function

#End Region

#Region "Utils"

    Private Function GetThreads()
        Select Case ThreadComboBox.SelectedIndex
            Case 0
                Return 1
            Case 1
                Return NumericThreads.Value
            Case 2
                Return Environment.ProcessorCount
            Case Else
                Return 512
        End Select
    End Function

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

    Private Function GetFileNameList(Source As String, Extension As Boolean) As List(Of String)
        Dim Result As New List(Of String)
        For Each File As String In Directory.GetFiles(Source).ToList
            If Extension = True Then
                Result.Add(Path.GetFileName(File))
            Else
                Result.Add(Path.GetFileNameWithoutExtension(File))
            End If
        Next
        Return Result
    End Function

    Private Function MakeCaffeCommand(SourceImage As String, NewImage As String, Package As CaffePackage) As String
        Dim Result As New ArguementString
        Result.AddArguement("-i", Quote(SourceImage))
        Result.AddArguement("-o", Quote(NewImage))
        Result.AddArguement("-m", Package.Mode)
        Result.AddArguement("-s", Package.Scale)
        Result.AddArguement("-n", Package.Noise)
        Result.AddArguement("-p", Package.Process)
        Result.AddArguement("-t", IIf(Package.TAA = True, 1, 0))
        Return Result.GetArguements
    End Function

    Private Function MakeVulkanCommand(SourceImage As String, NewImage As String, Package As VulkanPackage) As String
        Dim Result As New ArguementString
        Result.AddArguement("-i", Quote(SourceImage))
        Result.AddArguement("-o", Quote(NewImage))
        Result.AddArguement("-s", Package.Scale)
        If Package.NoNoise = False Then Result.AddArguement("-n", Package.Noise)
        Result.AddArguement("-f", Package.Format)
        Result.AddArguement(IIf(Package.TAA = True, "-x", ""))
        Return Result.GetArguements
    End Function

    Private Function MakeCPPCommand(SourceImage As String, NewImage As String, Package As CPPPackage) As String
        Dim Result As New ArguementString
        Result.AddArguement("-i", Quote(SourceImage))
        Result.AddArguement("-o", Quote(NewImage))
        Result.AddArguement("-m", Package.Mode)
        Result.AddArguement("--scale-ratio", Package.Scale)
        Result.AddArguement("--noise-level", Package.Noise)
        Result.AddArguement("-f", Package.Format)
        Result.AddArguement(IIf(Package.NoGPU = True, "--disable-gpu", ""))
        Result.AddArguement(IIf(Package.OpenCL = True, "--force-OpenCL", ""))
        Result.AddArguement(IIf(Package.TTA = True, "-t", ""))
        Return Result.GetArguements
    End Function

    Private Function MakeA4KCommand(SourceImage As String, NewImage As String, Package As A4KPackage) As String
        Dim Result As New ArguementString
        Result.AddArguement("-i", Quote(SourceImage))
        Result.AddArguement("-o", Quote(NewImage))
        Result.AddArguement("-p", Package.Passes)
        Result.AddArguement("-n", Package.PushColors)
        Result.AddArguement("-c", Package.ColorStrength)
        Result.AddArguement("-g", Package.GradStrength)
        Result.AddArguement("-z", Package.Scale)
        Result.AddArguement(IIf(Package.Pre = True, "-b", ""))
        Result.AddArguement(IIf(Package.Post = True, "-a", ""))
        Select Case Package.FilterType
            Case 1
                Result.AddArguement("-r", Package.Filter)
            Case 2
                Result.AddArguement("-e", Package.Filter)
        End Select
        Result.AddArguement(IIf(Package.GPU = True, "-q", ""))
        Result.AddArguement(IIf(Package.CNN = True, "-w", ""))
        Result.AddArguement("-A")
        Return Result.GetArguements
    End Function

    Private Function MakePyCommand(SourceFolder As String, DestFolder As String, Package As PyPackage) As String
        Dim Result As New ArguementString
        Result.AddArguement(Quote(Package.Model))
        Result.AddArguement(Package.InputFlag, Quote(SourceFolder))
        Result.AddArguement(Package.OutputFlag, Quote(DestFolder))
        For i = 0 To Package.Arguements.Count - 1 Step 2
            Result.AddArguement(Package.Arguements(i), Package.Arguements(i + 1))
        Next
        Return Result.GetArguements
    End Function

    Private Function Quote(Source As String) As String
        Return ControlChars.Quote & Source & ControlChars.Quote
    End Function

#End Region

End Class