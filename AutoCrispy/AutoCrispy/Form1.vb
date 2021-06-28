Imports System.IO

Public Class Form1

#Region "VARS"

    Dim Root As String = Application.StartupPath
    Dim AppData As String = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
    Dim WaitScale As Integer = 0
    Dim SettingsLoc As Point = New Point(240, 166)
    Dim PyDetected As Boolean
    Dim LoadedSettings As FormSettings.Settings

    Public Property ChainList As New List(Of FormSettings.ChainObject)
    Public Property PyPaths As New List(Of String)
    Public Property PyModels As New List(Of String)
    Public Property CaffePath As String
    Public Property WaifuNcnnPath As String
    Public Property RealSRNcnnPath As String
    Public Property SRMDNcnnPath As String
    Public Property WaifuCppPath As String
    Public Property Anime4kPath As String
    Public Property TexConvPath As String
    Public Property DetectedPyPath As String

#End Region

#Region "Structs"

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

#End Region

#Region "Loading"

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Size = New Size(660, 413)
        Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
        Application.CurrentCulture = New Globalization.CultureInfo("EN-US")
        PreloadImageList()
        Try
            If File.Exists(Root & "\portable.xml") Then
                FormSettings.LoadSettings(Me, Deserialize(Of FormSettings.Settings)(File.ReadAllText(Root & "\portable.xml")))
            ElseIf File.Exists(AppData & "\AutoCrispy\settings.xml") Then
                FormSettings.LoadSettings(Me, Deserialize(Of FormSettings.Settings)(File.ReadAllText(AppData & "\AutoCrispy\settings.xml")))
            Else
                FormSettings.LoadSettings(Me, Deserialize(Of FormSettings.Settings)(My.Resources.default_settings))
                If Not Directory.Exists(AppData & "\AutoCrispy") Then
                    Directory.CreateDirectory(AppData & "\AutoCrispy")
                End If
            End If
        Catch ex As Exception
            MsgBox("Failed to load Settings!  Loading program defaults.")
            FormSettings.LoadSettings(Me, Deserialize(Of FormSettings.Settings)(My.Resources.default_settings))
        End Try
        StartUpCheckEXE()
        StartUpCheckPy()
        If ExeComboBox.Items.Count > 0 Then
            ExeComboBox.SelectedIndex = 0
            SetSettingsWindow()
        End If
        WatchDogButton.Select()
    End Sub

    Private Sub Form1_Closing(sender As Object, e As EventArgs) Handles MyBase.Closing
        If PortableCheckBox.Checked = True Then
            File.WriteAllText(Root & "\portable.xml", Serialize(New FormSettings.Settings(Me)))
        Else
            File.WriteAllText(AppData & "\AutoCrispy\settings.xml", Serialize(New FormSettings.Settings(Me)))
        End If
    End Sub

    Private Sub StartUpCheckEXE()
        Dim RootFolders As List(Of String) = Directory.GetDirectories(Root).ToList
        RootFolders.Add(Root)
        For Each Folder As String In RootFolders
            If File.Exists(Folder & "\waifu2x-caffe-cui.exe") Then
                ExeComboBox.Items.Add("Waifu2x Caffe")
                CaffePath = "\" & IIf(Folder <> Root, Path.GetFileName(Folder), "") & "\waifu2x-caffe-cui.exe"
            End If
            If File.Exists(Folder & "\waifu2x-ncnn-vulkan.exe") Then
                ExeComboBox.Items.Add("Waifu2x Vulkan")
                WaifuNcnnPath = "\" & IIf(Folder <> Root, Path.GetFileName(Folder), "") & "\waifu2x-ncnn-vulkan.exe"
            End If
            If File.Exists(Folder & "\realsr-ncnn-vulkan.exe") Then
                ExeComboBox.Items.Add("RealSR Vulkan")
                RealSRNcnnPath = "\" & IIf(Folder <> Root, Path.GetFileName(Folder), "") & "\realsr-ncnn-vulkan.exe"
            End If
            If File.Exists(Folder & "\srmd-ncnn-vulkan.exe") Then
                ExeComboBox.Items.Add("SRMD Vulkan")
                SRMDNcnnPath = "\" & IIf(Folder <> Root, Path.GetFileName(Folder), "") & "\srmd-ncnn-vulkan.exe"
            End If
            If File.Exists(Folder & "\waifu2x-converter-cpp.exe") Then
                ExeComboBox.Items.Add("Waifu2x CPP")
                WaifuCppPath = "\" & IIf(Folder <> Root, Path.GetFileName(Folder), "") & "\waifu2x-converter-cpp.exe"
            End If
            If File.Exists(Folder & "\Anime4KCPP_CLI.exe") Then
                ExeComboBox.Items.Add("Anime4k CPP")
                Anime4kPath = "\" & IIf(Folder <> Root, Path.GetFileName(Folder), "") & "\Anime4KCPP_CLI.exe"
            End If
            If File.Exists(Folder & "\texconv.exe") Then
                ExeComboBox.Items.Add("TexConv")
                TexConvPath = "\" & IIf(Folder <> Root, Path.GetFileName(Folder), "") & "\texconv.exe"
            End If
            If File.Exists(Folder & "\python\python.exe") Then
                PyDetected = True
                DetectedPyPath = "\" & IIf(Folder <> Root, Path.GetFileName(Folder), "") & "\python\python.exe"
            End If
        Next
        If PyDetected = False AndAlso GetPythonPath() <> "" Then
            PyDetected = True
            DetectedPyPath = GetPythonPath()
        End If
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
        ChainThumbs.Images.Add(Shrink(My.Resources._7, 64, 64))
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
                File.WriteAllText(SFD.FileName, Serialize(ChainList))
            End If
        End Using
    End Sub

    Private Sub ChainLoad_Click(sender As Object, e As EventArgs) Handles ChainLoad.Click
        Using OFD As New OpenFileDialog With {.Filter = "XML Files|*.xml|All Files|*.*"}
            If OFD.ShowDialog = DialogResult.OK Then
                ChainPreview.Clear()
                ChainList.Clear()
                ChainList = Deserialize(Of List(Of FormSettings.ChainObject))(File.ReadAllText(OFD.FileName))
                For Each ChainItem As FormSettings.ChainObject In ChainList
                    ChainPreview.Items.Add(New ListViewItem(ChainItem.Name, ChainItem.IconIndex))
                Next
            End If
        End Using
    End Sub

    Private Sub ChainAdd_Click(sender As Object, e As EventArgs) Handles ChainAdd.Click
        AddModelToChain(ExeComboBox.SelectedItem)
    End Sub

    Private Sub RemoveItemFromChain(sender As Object, e As EventArgs) Handles ChainRemove.Click, ChainContextDelete.Click
        If ChainPreview.SelectedIndices.Count > 0 Then
            Dim Remove As Integer = ChainPreview.SelectedIndices(0)
            ChainPreview.Items.RemoveAt(Remove)
            ChainList.RemoveAt(Remove)
            ChainPreview.AutoArrange = True
            ChainPreview.AutoArrange = False
        End If
    End Sub

    Private Sub ChainContextEdit_Click(sender As Object, e As EventArgs) Handles ChainContextEdit.Click
        If ChainPreview.SelectedItems.Count > 0 Then
            Dim ItemIndex As Integer = GetListIndex(ChainPreview.SelectedItems(0))
            Using ECD As New EditChainDialog(Serialize(ChainList(ItemIndex)))
                If ECD.ShowDialog = DialogResult.OK Then
                    Try
                        Dim NewChainItem As FormSettings.ChainObject = Deserialize(Of FormSettings.ChainObject)(ECD.ResultText)
                        ChainList(ItemIndex) = NewChainItem
                        ChainPreview.Items(ChainPreview.Items.IndexOf(ChainPreview.SelectedItems(0))) = New ListViewItem(NewChainItem.Name, NewChainItem.IconIndex)
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
            Result.Add(GetListIndex(Model) & "|" & Model.Index)
        Next
        Result.Sort()
        For Each Item As String In Result
            If Item.Split("|")(0) <> Item.Split("|")(1) Then
                Dim NewChainOrder As New List(Of FormSettings.ChainObject)
                For Each ReOrder As String In Result
                    NewChainOrder.Add(ChainList(ReOrder.Split("|")(1)))
                Next
                ChainList = NewChainOrder
                ChainPreview.Clear()
                For Each ChainItem As FormSettings.ChainObject In ChainList
                    ChainPreview.Items.Add(New ListViewItem(ChainItem.Name, ChainItem.IconIndex))
                Next
                Exit For
            End If
        Next
    End Sub

    Private Sub DDxFormatListBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles DDxFormatListBox.SelectedIndexChanged
        DDxFormatLabel.Text = "Format: " & DDxFormatListBox.SelectedItem
    End Sub

    Private Sub DDxModeBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles DDxModeBox.SelectedIndexChanged
        Select Case DDxModeBox.SelectedIndex
            Case 0
                DDxConvFormat.Enabled = True
            Case 1
                DDxConvFormat.Enabled = False
        End Select
    End Sub

    Private Sub RunOnceButton_Click(sender As Object, e As EventArgs) Handles RunOnceButton.Click
        Using OFD As New OpenFileDialog With {.Filter = "Image Files|*.png;*.jpg;*.bmp"}
            If OFD.ShowDialog = DialogResult.OK Then
                Using SFD As New SaveFileDialog With {.Filter = "PNG Images|*.png"}
                    If SFD.ShowDialog = DialogResult.OK Then
                        Dim TempPath As String = Path.GetTempPath & "Single_0"
                        Directory.CreateDirectory(Path.GetTempPath & "Single_0")
                        File.Copy(OFD.FileName, TempPath & "\" & Path.GetFileName(SFD.FileName), True)
                        LoadedSettings = New FormSettings.Settings(Me)
                        LoadedSettings.Paths = New FormSettings.ProgramPaths(TempPath, Directory.GetParent(SFD.FileName).FullName)
                        If ChainPreview.Items.Count = 0 Then
                            AddModelToChain(ExeComboBox.SelectedItem, False)
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
            GetPyArgs(File.ReadAllText(PyPaths(PyScript.SelectedIndex)).Replace(vbCr, " ").Replace(vbLf, " ").Replace(vbCrLf, " "))
        End If
    End Sub

    Private Sub ThreadComboBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ThreadComboBox.SelectedIndexChanged
        If ThreadComboBox.SelectedIndex = 1 Then
            NumericThreads.Enabled = True
        Else
            NumericThreads.Enabled = False
        End If
    End Sub

    Private Sub SeamsBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles SeamsBox.SelectedIndexChanged
        If SeamsBox.SelectedIndex > 0 Then
            SeamScale.Enabled = True
            SeamMargin.Enabled = True
        Else
            SeamScale.Enabled = False
            SeamMargin.Enabled = False
        End If
    End Sub

    Private Sub Anime4kCheck_Changes(sender As Object, e As EventArgs) Handles MyBase.Load, AnimeCppPre.CheckedChanged, AnimeCppPreFilter.CheckedChanged, AnimeCppPost.CheckedChanged, AnimeCppPostFilter.CheckedChanged, AnimeCPPCnn.CheckedChanged
        If AnimeCPPCnn.Checked = True Then
            AnimeCppPre.Checked = False
            AnimeCppPost.Checked = False
        End If
        AnimeCppPreFilter.Enabled = AnimeCppPre.Checked
        AnimeCppPreFilters.Enabled = AnimeCppPreFilter.Checked
        If AnimeCppPre.Checked = False Then AnimeCppPreFilter.Checked = False
        AnimeCppPostFilter.Enabled = AnimeCppPost.Checked
        AnimeCppPostFilters.Enabled = AnimeCppPostFilter.Checked
        If AnimeCppPost.Checked = False Then AnimeCppPostFilter.Checked = False
    End Sub

    Private Sub SetSettingsWindow()
        CaffeGroup.Visible = False
        VulkanGroup.Visible = False
        WaifuCPPGroup.Visible = False
        AnimeCPPGroup.Visible = False
        DDxGroup.Visible = False
        PyGroup.Visible = False
        VulkanNoise.Enabled = True
        Select Case ExeComboBox.SelectedItem
            Case "Waifu2x Caffe"
                MoveShowGroup(CaffeGroup)
            Case "Waifu2x Vulkan"
                MoveShowGroup(VulkanGroup)
                VulkanScale.Value = 2
            Case "RealSR Vulkan"
                MoveShowGroup(VulkanGroup)
                VulkanScale.Value = 4
                VulkanNoise.Enabled = False
            Case "SRMD Vulkan"
                MoveShowGroup(VulkanGroup)
                VulkanScale.Value = 4
            Case "Waifu2x CPP"
                MoveShowGroup(WaifuCPPGroup)
            Case "Anime4k CPP"
                MoveShowGroup(AnimeCPPGroup)
            Case "TexConv"
                MoveShowGroup(DDxGroup)
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
        DDxGroup.Enabled = Enabled
        PyGroup.Enabled = Enabled
    End Sub

#End Region

#Region "Background"

    Private Sub WatchDog_Tick(sender As Object, e As EventArgs) Handles WatchDog.Tick
        Dim Source = Directory.GetFiles(InputTextBox.Text).Count
        Dim FileCheck = GetMissingFiles(InputTextBox.Text, OutputTextBox.Text).Count
        If Source = 0 OrElse FileCheck = 0 Then
            WaitScale = Math.Min(WaitScale + 1, 100)
            WatchDog.Interval = 1000 + (WaitScale * 590)
        Else
            WaitScale = 0
            WatchDog.Interval = 1000
            LoadedSettings = New FormSettings.Settings(Me)
            If ChainPreview.Items.Count = 0 Then
                AddModelToChain(ExeComboBox.SelectedItem, False)
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
        If ChainPreview.Items.Count = 0 Then
            ChainList.Clear()
        End If
        If e.Cancelled = True Then
            WatchDog.Enabled = False
            WatchDogButton.Text = "Running: " & False
            SwitchGroups(Not WatchDog.Enabled)
            WatchDogButton.Enabled = True
            Exit Sub
        End If
        If WatchDogButton.Text = "Running: True" Then
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
        Dim Source = GetMissingFiles(LoadedSettings.Paths.InputPath, LoadedSettings.Paths.OutputPath)
        For i = 0 To Source.Count - 1 Step GetThreads(LoadedSettings.BasicSettings.ThreadIndex, LoadedSettings.BasicSettings.ThreadCount)
            Dim ChainPaths As New List(Of String)
            Dim DeletedChainPaths As New List(Of String)
            ChainPaths.Add(TempPath)
            For j = 0 To ChainList.Count - 2
                Dim TempName As String = Path.GetTempPath & "Chain_" & j
                ChainPaths.Add(TempName)
                Directory.CreateDirectory(TempName)
            Next
            ChainPaths.Add(LoadedSettings.Paths.OutputPath)
            Directory.CreateDirectory(TempPath)
            For j = i To i + GetThreads(LoadedSettings.BasicSettings.ThreadIndex, LoadedSettings.BasicSettings.ThreadCount) - 1
                If j <= Source.Count - 1 Then
                    File.Copy(Source(j), TempPath & "\" & Path.GetFileName(Source(j)), True)
                End If
            Next
            For Each Model In ChainList
                Dim NewImages As New List(Of String)
                Dim DiffImages = GetMissingFiles(ChainPaths(0), LoadedSettings.Paths.OutputPath)
                For Each NewImage As String In DiffImages
                    Dim AcceptExt As Boolean = Model.Package.FileTypes.Contains(Path.GetExtension(NewImage).ToLower)
                    If File.Exists(NewImage) AndAlso AcceptExt = True Then
                        NewImages.Add(NewImage)
                        If (ChainList.IndexOf(Model) = 0 AndAlso Model.Name <> "TexConv") OrElse (ChainList(0).Name = "TexConv" AndAlso ChainList.IndexOf(Model) = 1) Then
                            Dim SourceImage As Bitmap = Image.FromFile(NewImage)
                            Dim SeamlessImage As New Bitmap(SourceImage)
                            SourceImage.Dispose()
                            SeamlessImage = MakeSeamless(SeamlessImage, LoadedSettings.ExpertSettings.SeamlessMode, LoadedSettings.ExpertSettings.SeamlessMargin)
                            SeamlessImage.Save(NewImage)
                        End If
                    End If
                Next
                If NewImages.Count > 0 Then
                    Dim BuildProcess As ProcessStartInfo
                    If Model.PackageType = "Python" Then
                        If PyDetected = True Then
                            BuildProcess = New ProcessStartInfo(IIf(DetectedPyPath(0) = "\"c, Root & Model.FileLocation, Model.FileLocation), Quote(Model.Package.Script) & " " & MakeCommand(ChainPaths(0), ChainPaths(1), Model.PackageType, Model.Package))
                            BuildProcess.RedirectStandardOutput = True
                            BuildProcess.RedirectStandardError = True
                            BuildProcess.UseShellExecute = False
                            BuildProcess.CreateNoWindow = True
                        Else
                            BuildProcess = New ProcessStartInfo(Model.Package.Script, MakeCommand(ChainPaths(0), ChainPaths(1), Model.PackageType, Model.Package))
                            BuildProcess.UseShellExecute = True
                            BuildProcess.WindowStyle = ProcessWindowStyle.Minimized
                        End If
                        Dim BatchProcess As Process = Process.Start(BuildProcess)
                        BatchProcess.WaitForExit()
                        If LoadedSettings.ExpertSettings.Logging = True AndAlso PyDetected = True Then
                            WriteLog(BatchProcess, LoadedSettings.Paths.OutputPath)
                        End If
                    Else
                        Dim ProcessBag As New List(Of Process)
                        For j = 0 To NewImages.Count - 1
                            Dim NewImage As String = ChainPaths(1) & "\" & Path.GetFileName(NewImages(j))
                            BuildProcess = New ProcessStartInfo(Root & Model.FileLocation, MakeCommand(NewImages(j), NewImage, Model.PackageType, Model.Package))
                            BuildProcess.WorkingDirectory = Directory.GetParent(Root & Model.FileLocation).FullName
                            BuildProcess.RedirectStandardOutput = True
                            BuildProcess.RedirectStandardError = True
                            BuildProcess.UseShellExecute = False
                            BuildProcess.CreateNoWindow = True
                            Dim BatchProcess As Process = Process.Start(BuildProcess)
                            ProcessBag.Add(BatchProcess)
                            If LoadedSettings.ExpertSettings.Logging = True Then
                                WriteLog(BatchProcess, LoadedSettings.Paths.OutputPath)
                            End If
                        Next
                        Do
                            Dim CompletionStatus As New List(Of Boolean)
                            For Each Job As Process In ProcessBag
                                CompletionStatus.Add(Job.HasExited)
                            Next
                            If Not CompletionStatus.Contains(False) Then
                                Exit Do
                            End If
                        Loop
                    End If
                    For Each TempImage As String In Directory.GetFiles(ChainPaths(0))
                        File.Delete(TempImage)
                    Next
                End If
                DeletedChainPaths.Add(ChainPaths(0))
                ChainPaths.RemoveAt(0)
                If (ChainList.IndexOf(Model) = ChainList.Count - 1 AndAlso Model.Name <> "TexConv") OrElse (ChainList(ChainList.Count - 1).Name = "TexConv" AndAlso ChainList.IndexOf(Model) = ChainList.Count - 2) Then
                    If LoadedSettings.BasicSettings.Defringe = True Then
                        For Each NewImage In NewImages
                            If File.Exists(ChainPaths(0) & "\" & Path.GetFileName(NewImage)) Then Defringe(ChainPaths(0) & "\" & Path.GetFileName(NewImage), LoadedSettings.BasicSettings.DefringeThreshold)
                        Next
                    End If
                    If LoadedSettings.ExpertSettings.SeamlessMode > 0 Then
                        For Each NewImage In NewImages
                            If File.Exists(ChainPaths(0) & "\" & Path.GetFileName(NewImage)) Then
                                Dim ScaleVal As Integer = LoadedSettings.ExpertSettings.SeamlessScale * LoadedSettings.ExpertSettings.SeamlessMargin
                                Dim SourceImage As Bitmap = Image.FromFile(ChainPaths(0) & "\" & Path.GetFileName(NewImage))
                                Dim TrimmedImage As New Bitmap(SourceImage)
                                SourceImage.Dispose()
                                TrimmedImage = CropImage(TrimmedImage, ScaleVal, ScaleVal, TrimmedImage.Width - (ScaleVal * 2), TrimmedImage.Height - (ScaleVal * 2), 0)
                                TrimmedImage.Save(ChainPaths(0) & "\" & Path.GetFileName(NewImage))
                            End If
                        Next
                    End If
                End If
                If WorkHorse.CancellationPending = True Then
                    Directory.Delete(TempPath, True)
                    For j = 0 To ChainList.Count - 2
                        Dim TempName As String = Path.GetTempPath & "Chain_" & j
                        Directory.Delete(TempName, True)
                    Next
                    Exit Sub
                End If
            Next
            For Each ChainDir As String In DeletedChainPaths
                Directory.Delete(ChainDir, True)
            Next
            WorkHorse.ReportProgress(Math.Round(((i * 100) + 1) / Source.Count, 0))
        Next
        If CleanupCheckBox.Checked = True Then
            For Each SourceImage As String In Source
                File.Delete(SourceImage)
            Next
        End If
    End Sub

#End Region

#Region "Packageing"

    Private Function MakeCommand(Source As String, Dest As String, Mode As String, Package As Object) As String
        Select Case Mode
            Case "Waifu2x Caffe"
                Return MakeCaffeCommand(Source, Dest, Package)
            Case "Waifu2x Vulkan"
                Return MakeVulkanCommand(Source, Dest, False, Package)
            Case "RealSR Vulkan"
                Return MakeVulkanCommand(Source, Dest, True, Package)
            Case "SRMD Vulkan"
                Return MakeVulkanCommand(Source, Dest, False, Package)
            Case "Waifu2x CPP"
                Return MakeCPPCommand(Source, Dest, Package)
            Case "Anime4k CPP"
                Return MakeA4KCommand(Source, Dest, Package)
            Case "TexConv"
                Return MakeTexConvCommand(Source, Dest, Package)
            Case "Python"
                Return MakePyCommand(Source, Dest, Package)
        End Select
        Return ""
    End Function

    Private Sub AddModelToChain(Mode As String, Optional AddPreview As Boolean = True)
        Select Case Mode
            Case "Waifu2x Caffe"
                If AddPreview = True Then ChainPreview.Items.Add(New ListViewItem("Caffe", 0))
                ChainList.Add(New FormSettings.ChainObject("Caffe", 0, CaffePath, "Waifu2x Caffe", Me))
            Case "Waifu2x Vulkan"
                If AddPreview = True Then ChainPreview.Items.Add(New ListViewItem("Waifu Vulkan", 1))
                ChainList.Add(New FormSettings.ChainObject("Waifu Vulkan", 1, WaifuNcnnPath, "Waifu2x Vulkan", Me))
            Case "RealSR Vulkan"
                If AddPreview = True Then ChainPreview.Items.Add(New ListViewItem("RealSR Vulkan", 2))
                ChainList.Add(New FormSettings.ChainObject("RealSR Vulkan", 2, RealSRNcnnPath, "RealSR Vulkan", Me))
            Case "SRMD Vulkan"
                If AddPreview = True Then ChainPreview.Items.Add(New ListViewItem("SRMD Vulkan", 3))
                ChainList.Add(New FormSettings.ChainObject("SRMD Vulkan", 3, SRMDNcnnPath, "SRMD Vulkan", Me))
            Case "Waifu2x CPP"
                If AddPreview = True Then ChainPreview.Items.Add(New ListViewItem("Waifu CPP", 4))
                ChainList.Add(New FormSettings.ChainObject("Waifu CPP", 4, WaifuCppPath, "Waifu2x CPP", Me))
            Case "Anime4k CPP"
                If AddPreview = True Then ChainPreview.Items.Add(New ListViewItem("Anime4K", 5))
                ChainList.Add(New FormSettings.ChainObject("Anime4k", 5, Anime4kPath, "Anime4k CPP", Me))
            Case "TexConv"
                If AddPreview = True Then ChainPreview.Items.Add(New ListViewItem("TexConv", 7))
                ChainList.Add(New FormSettings.ChainObject("TexConv", 7, TexConvPath, "TexConv", Me))
            Case "Python"
                If AddPreview = True Then ChainPreview.Items.Add(New ListViewItem(PyModel.SelectedItem.ToString, 6))
                ChainList.Add(New FormSettings.ChainObject(PyModel.SelectedItem.ToString, 6, DetectedPyPath, "Python", Me))
        End Select
    End Sub

#End Region

#Region "Graphics"

    Private Function MakeSeamless(Source As Bitmap, Mirrored As Integer, Margin As Integer) As Bitmap
        Dim Result As New Bitmap(Source.Width * 3, Source.Height * 3, Source.PixelFormat)
        Using g As Graphics = Graphics.FromImage(Result)
            g.PixelOffsetMode = Drawing2D.PixelOffsetMode.None
            g.SmoothingMode = Drawing2D.SmoothingMode.None
            g.InterpolationMode = Drawing2D.InterpolationMode.NearestNeighbor
            If Mirrored = 2 Then
                Dim X = Source.Width
                Dim Y = Source.Height
                Dim fX As New Bitmap(Source) : fX.RotateFlip(RotateFlipType.RotateNoneFlipX)
                Dim fY As New Bitmap(Source) : fY.RotateFlip(RotateFlipType.RotateNoneFlipY)
                Dim fXY As New Bitmap(Source) : fXY.RotateFlip(RotateFlipType.RotateNoneFlipXY)
                g.DrawImage(fXY, 0, 0, X, Y) : g.DrawImage(fY, X, 0, X, Y) : g.DrawImage(fXY, 2 * X, 0, X, Y)
                g.DrawImage(fX, 0, Y, X, Y) : g.DrawImage(Source, X, Y, X, Y) : g.DrawImage(fX, 2 * X, Y, X, Y)
                g.DrawImage(fXY, 0, 2 * Y, X, Y) : g.DrawImage(fY, X, 2 * Y, X, Y) : g.DrawImage(fXY, 2 * X, 2 * Y, X, Y)
            ElseIf Mirrored = 1 Then
                For i = 0 To Source.Width * 2 Step Source.Width
                    For j = 0 To Source.Height * 2 Step Source.Height
                        g.DrawImage(Source, i, j, Source.Width, Source.Height)
                    Next
                Next
            Else
                Return Source
            End If
        End Using
        Return CropImage(Result, Source.Width, Source.Height, Source.Width, Source.Height, Margin)
    End Function

    Private Function CropImage(Source As Bitmap, OffsetX As Integer, OffsetY As Integer, Width As Integer, Height As Integer, Margins As Integer) As Bitmap
        Dim CropSize As New Rectangle(OffsetX - Margins, OffsetY - Margins, Width + (2 * Margins), Height + (2 * Margins))
        Dim Result = New Bitmap(CropSize.Width, CropSize.Height, Source.PixelFormat)
        Using g As Graphics = Graphics.FromImage(Result)
            g.PixelOffsetMode = Drawing2D.PixelOffsetMode.None
            g.SmoothingMode = Drawing2D.SmoothingMode.None
            g.InterpolationMode = Drawing2D.InterpolationMode.NearestNeighbor
            g.DrawImage(Source, New Rectangle(0, 0, CropSize.Width, CropSize.Height), CropSize, GraphicsUnit.Pixel)
        End Using
        Return Result
    End Function

    Private Sub Defringe(Source As String, Threshold As Integer)
        Dim SourceImage As Bitmap = Image.FromFile(Source)
        Dim NewImage As New Bitmap(SourceImage)
        SourceImage.Dispose()
        LockbitsDefringe(NewImage, Threshold)
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

    Private Function GetMissingFiles(Path1 As String, Path2 As String) As String()
        Dim Result As New List(Of String)
        Dim Path1MasterList = Directory.GetFiles(Path1)
        Dim Path1List = Directory.GetFiles(Path1).ToList
        Dim Path2List = Directory.GetFiles(Path2).ToList
        For i = 0 To Path1List.Count - 1
            Path1List(i) = Path.GetFileNameWithoutExtension(Path1List(i)).ToLower
        Next
        For i = 0 To Path2List.Count - 1
            Path2List(i) = Path.GetFileNameWithoutExtension(Path2List(i)).ToLower
        Next
        For i = 0 To Path1List.Count - 1
            If Not Path2List.Contains(Path1List(i)) Then Result.Add(Path1MasterList(i))
        Next
        Return Result.ToArray
    End Function

    Private Function GetThreads(Index As Integer, Count As Integer)
        Select Case Index
            Case 0
                Return 1
            Case 1
                Return Count
            Case 2
                Return Environment.ProcessorCount
            Case Else
                Return 512
        End Select
    End Function

    Private Function GetPythonPath() As String
        Dim Result As String = ""
        Dim PathVar As String = Environment.GetEnvironmentVariable("PATH")
        If PathVar IsNot Nothing Then
            For Each PathString In PathVar.Split(";"c)
                If File.Exists(PathString & "\python.exe") Then
                    Result = PathString & "\python.exe"
                    Exit For
                End If
            Next
        End If
        Return Result
    End Function

    Private Sub GetPyArgs(Source As String)
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

    Private Function GetListIndex(ListItem As ListViewItem) As Integer
        Return ((ListItem.Position.X - 21) / 107) + (4 * Math.Floor(ListItem.Position.Y / 107))
    End Function

    Private Function GetFolder() As String
        Using FBD As New FolderBrowserDialog
            If FBD.ShowDialog = DialogResult.OK Then
                Return FBD.SelectedPath
            End If
        End Using
        Return ""
    End Function

    Private Function Quote(Source As String) As String
        Return ControlChars.Quote & Source & ControlChars.Quote
    End Function

    Declare Auto Function GetShortPathName Lib "kernel32.dll" (lpszLongPath As String, lpszShortPath As Text.StringBuilder, cchBuffer As Integer) As Integer

    Private Function GetShortPath(Source As String)
        Dim sbShortPath As Text.StringBuilder = New Text.StringBuilder()
        GetShortPathName(Source, sbShortPath, 255)
        Return sbShortPath.ToString
    End Function

    Private Sub WriteLog(Source As Process, SaveLoc As String)
        Dim Filename As String = SaveLoc & "\Log_" & Now.ToString("yyyy-MM-dd_HH-mm-ss") & ".txt"
        Dim Output As String = ""
        Output += Source.StartInfo.FileName & " "
        Output += Source.StartInfo.Arguments
        Output += vbNewLine & vbNewLine
        Output += Source.StandardOutput.ReadToEnd
        Output += vbNewLine & vbNewLine
        Output += Source.StandardError.ReadToEnd
        File.WriteAllText(Filename, Output)
    End Sub

    Private Function MakeCaffeCommand(SourceImage As String, NewImage As String, Package As FormSettings.Waifu2xCaffePackage) As String
        Dim Result As New ArguementString
        Result.AddArguement("-i", Quote(SourceImage))
        Result.AddArguement("-o", Quote(NewImage))
        Result.AddArguement(LoadedSettings.ExpertSettings.ExpertFlags)
        Result.AddArguement("-m", Package.Mode)
        Result.AddArguement("-s", Package.Scale)
        Result.AddArguement("-n", Package.Noise)
        Result.AddArguement("-p", Package.Process)
        Result.AddArguement("-t", IIf(Package.TAA = True, 1, 0))
        Return Result.GetArguements
    End Function

    Private Function MakeVulkanCommand(SourceImage As String, NewImage As String, NoNoise As Boolean, Package As FormSettings.VulkanNcnnPackage) As String
        Dim Result As New ArguementString
        Result.AddArguement("-i", Quote(SourceImage))
        Result.AddArguement("-o", Quote(Path.ChangeExtension(NewImage, Package.Format)))
        Result.AddArguement(LoadedSettings.ExpertSettings.ExpertFlags)
        Result.AddArguement("-s", Package.Scale)
        If NoNoise = False Then Result.AddArguement("-n", Package.Noise)
        Result.AddArguement("-f", Package.Format)
        Result.AddArguement(IIf(Package.TAA = True, "-x", ""))
        Return Result.GetArguements
    End Function

    Private Function MakeCPPCommand(SourceImage As String, NewImage As String, Package As FormSettings.Waifu2xCppPackage) As String
        Dim Result As New ArguementString
        Result.AddArguement("-i", Quote(SourceImage))
        Result.AddArguement("-o", Quote(Path.ChangeExtension(NewImage, Package.Format)))
        Result.AddArguement(LoadedSettings.ExpertSettings.ExpertFlags)
        Result.AddArguement("-m", Package.Mode)
        Result.AddArguement("--scale-ratio", Package.Scale)
        Result.AddArguement("--noise-level", Package.Noise)
        Result.AddArguement("-f", Package.Format)
        Result.AddArguement(IIf(Package.GPU = False, "--disable-gpu", ""))
        Result.AddArguement(IIf(Package.ForceOpenCL = True, "--force-OpenCL", ""))
        Return Result.GetArguements
    End Function

    Private Function MakeA4KCommand(SourceImage As String, NewImage As String, Package As FormSettings.Anime4kPackage) As String
        Dim Result As New ArguementString
        Result.AddArguement("-i", Quote(SourceImage))
        Result.AddArguement("-o", Quote(NewImage))
        Result.AddArguement(LoadedSettings.ExpertSettings.ExpertFlags)
        Result.AddArguement("-p", Package.Passes)
        Result.AddArguement("-n", Package.PushColors)
        Result.AddArguement("-c", Package.PushColorStrength)
        Result.AddArguement("-g", Package.PushGradStrength)
        Result.AddArguement("-z", Package.Scale)
        Result.AddArguement(IIf(Package.PreProcess = True, "-b", ""))
        Result.AddArguement(IIf(Package.PreFilter = True, "-r " & Package.PreFilterType, ""))
        Result.AddArguement(IIf(Package.PostProcess = True, "-a", ""))
        Result.AddArguement(IIf(Package.PostFilter = True, "-e " & Package.PostFilterType, ""))
        Result.AddArguement(IIf(Package.GPU = True, "-q", ""))
        Result.AddArguement(IIf(Package.CNN = True, "-w", ""))
        Result.AddArguement("-A")
        Return Result.GetArguements
    End Function

    Private Function MakeTexConvCommand(SourceImage As String, NewImage As String, Package As FormSettings.DDxPackage) As String
        Dim Result As New ArguementString
        Result.AddArguement("-f", Package.Format)
        Select Case Package.Mode
            Case "DDS Input"
                Result.AddArguement("-ft " & Package.ConversionFormat.ToLower)
            Case "DDS Output"
                Result.AddArguement("-ft dds")
                Result.AddArguement("-fl", Package.FeatureLevel)
                Result.AddArguement(IIf(Package.ForceDx9 = True, "-dx9", ""))
                Result.AddArguement(IIf(Package.ForceDx10 = True, "-dx10", ""))
        End Select
        Result.AddArguement(IIf(Package.SeperateAlpha = True, "-sepalpha", ""))
        Result.AddArguement(IIf(Package.PremultiplyAlpha = True, "-pmalpha", ""))
        Result.AddArguement(IIf(Package.StraightAlpha = True, "-alpha", ""))
        Result.AddArguement("-o", GetShortPath(Path.GetDirectoryName(NewImage)))
        Result.AddArguement(GetShortPath(SourceImage))
        Return Result.GetArguements
    End Function

    Private Function MakePyCommand(SourceFolder As String, DestFolder As String, Package As FormSettings.PythonPackage) As String
        Dim Result As New ArguementString
        Result.AddArguement(Quote(Package.Model))
        Result.AddArguement(Package.InputFlag, Quote(SourceFolder))
        Result.AddArguement(Package.OutputFlag, Quote(DestFolder))
        For i = 0 To Package.ScriptFlags.Count - 1
            Result.AddArguement(Package.ScriptFlags(i)(0), Package.ScriptFlags(i)(1))
        Next
        Return Result.GetArguements
    End Function

#End Region

End Class