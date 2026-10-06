Imports System.IO
Imports System.Reflection

Public Class Form1

#Region "VARS"

    Dim Root As String = Application.StartupPath
    Dim AppData As String = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
    Dim WaitScale As Integer = 0
    Dim SettingsLoc As Point = New Point(240, 166)
    Dim LoadedSettings As FormSettings.Settings
    Dim SkipList As New List(Of String)
    ' Chain entries that were added implicitly for a single run (empty chain + watchdog / Run
    ' Once).  They live in ChainList so MakeUpscale has something to walk, but they are never
    ' shown in the preview and are removed again when the run ends.
    Dim ImplicitChainCount As Integer = 0

    Const HotToggle As String = "%`"

    Public Property ChainControl As DragDropList
    Public Property ChainList As New List(Of FormSettings.ChainObject)
    Public Property ChainThumbs As New List(Of Image)
    Public Property CaffePath As String
    Public Property WaifuNcnnPath As String
    Public Property RealSRNcnnPath As String
    Public Property RealESRGNcnnPath As String
    Public Property SRMDNcnnPath As String
    Public Property WaifuCppPath As String
    Public Property Anime4kPath As String
    Public Property TexConvPath As String
    Public Property xBRZPath As String
    Public Property PyPath As String
    Public Property PyModels As New List(Of String)

#End Region

#Region "Structs"

    <Serializable()> Public Structure ArguementString
        Private Property Arguements As String
        Public Function GetArguements() As String
            Return System.Text.RegularExpressions.Regex.Replace(Arguements, " {2,}", " ").Trim
        End Function
        Public Sub AddArguement(Flag As String)
            Arguements += " " & Flag
        End Sub
        Public Sub AddArguement(Flag As String, Value As String)
            Arguements += " " & Flag & " " & Value
        End Sub
        Public Sub AddArguement(Flag As String, Enabled As Boolean)
            If Enabled Then Arguements += " " & Flag
        End Sub
    End Structure

#End Region

#Region "Backend Processes"

    ' A backend process paired with its asynchronously captured stdout/stderr.
    '
    ' The old code redirected both pipes and then blocked in WaitForExit() (or spun on HasExited)
    ' without ever reading from them.  As soon as the backend wrote more than the ~4 KB the OS
    ' buffers for a pipe, it blocked on write while AutoCrispy blocked on WaitForExit: a hard
    ' deadlock that looked like a frozen program.  "Threads: All" hit this constantly, because a
    ' single process then reports progress for every texture in the queue.
    '
    ' BeginOutputReadLine/BeginErrorReadLine drain both pipes on thread-pool threads, so the
    ' backend can never block on write, and the captured text is still there for the log option.
    Private Class BackendJob
        Implements IDisposable

        Private ReadOnly BackendProcess As Process
        Private ReadOnly OutputBuffer As New Text.StringBuilder
        Private ReadOnly ErrorBuffer As New Text.StringBuilder
        Private ReadOnly BufferLock As New Object
        Private IsDisposed As Boolean

        Public Sub New(StartInfo As ProcessStartInfo)
            BackendProcess = New Process()
            BackendProcess.StartInfo = StartInfo
            AddHandler BackendProcess.OutputDataReceived, AddressOf OnOutputReceived
            AddHandler BackendProcess.ErrorDataReceived, AddressOf OnErrorReceived
        End Sub

        Public ReadOnly Property CommandLine As String
            Get
                Return BackendProcess.StartInfo.FileName & " " & BackendProcess.StartInfo.Arguments
            End Get
        End Property

        Public ReadOnly Property ProcessId As Integer
            Get
                Try
                    Return BackendProcess.Id
                Catch ex As Exception
                    Return 0
                End Try
            End Get
        End Property

        Public ReadOnly Property HasExited As Boolean
            Get
                Try
                    Return BackendProcess.HasExited
                Catch ex As Exception
                    Return True
                End Try
            End Get
        End Property

        Public Function GetOutput() As String
            SyncLock BufferLock
                Return OutputBuffer.ToString()
            End SyncLock
        End Function

        Public Function GetErrors() As String
            SyncLock BufferLock
                Return ErrorBuffer.ToString()
            End SyncLock
        End Function

        Public Sub Start()
            BackendProcess.Start()
            ' Only valid after Start(): the redirected streams do not exist before then.
            BackendProcess.BeginOutputReadLine()
            BackendProcess.BeginErrorReadLine()
        End Sub

        Public Sub Wait(Cancelled As Func(Of Boolean))
            Do While Not HasExited
                If Cancelled IsNot Nothing AndAlso Cancelled() Then
                    Try
                        BackendProcess.Kill()
                    Catch ex As Exception
                        ' Already gone or not killable; either way there is nothing left to wait for.
                    End Try
                    Exit Do
                End If
                ' Sleep rather than the old tight Do/Loop, which pinned a core at 100 %.
                Threading.Thread.Sleep(50)
            Loop
            ' The parameterless overload also waits for the async output handlers to finish, so
            ' GetOutput()/GetErrors() are complete once it returns.
            Try
                BackendProcess.WaitForExit()
            Catch ex As Exception
            End Try
        End Sub

        Private Sub OnOutputReceived(sender As Object, e As DataReceivedEventArgs)
            If e.Data IsNot Nothing Then
                SyncLock BufferLock
                    OutputBuffer.AppendLine(e.Data)
                End SyncLock
            End If
        End Sub

        Private Sub OnErrorReceived(sender As Object, e As DataReceivedEventArgs)
            If e.Data IsNot Nothing Then
                SyncLock BufferLock
                    ErrorBuffer.AppendLine(e.Data)
                End SyncLock
            End If
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If IsDisposed Then Return
            IsDisposed = True
            Try
                BackendProcess.CancelOutputRead()
                BackendProcess.CancelErrorRead()
            Catch ex As Exception
                ' Async reading was never started, or has already been torn down.
            End Try
            RemoveHandler BackendProcess.OutputDataReceived, AddressOf OnOutputReceived
            RemoveHandler BackendProcess.ErrorDataReceived, AddressOf OnErrorReceived
            BackendProcess.Dispose()
        End Sub
    End Class

#End Region

#Region "Loading"

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Size = New Size(660, 413)
        Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
        Application.CurrentCulture = New Globalization.CultureInfo("EN-US")
        PreloadImageList()
        ChainControl = New DragDropList(ChainPreview, 7)
        ' ChainControl is created at runtime, so the reorder notification has to be attached
        ' with AddHandler rather than Handles.
        AddHandler ChainControl.ListReordered, AddressOf ChainReordered
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
        If ExeTextBox.Text <> "" Then
            Root = ExeTextBox.Text
        End If
        StartUpCheckEXE()
        If ExeComboBox.Items.Count > 0 Then
            ExeComboBox.SelectedIndex = 0
            SetSettingsWindow()
        End If
        ChainControl.DrawList(ChainControl.ListItems)
        WatchDogButton.Select()
        ' Show the real state of the dump straight away instead of waiting for the first tick.
        ProgressPollTimer.Enabled = True
        ProgressPollTimer_Tick(ProgressPollTimer, EventArgs.Empty)
        If Environment.GetCommandLineArgs.Count > 1 Then
            WatchDogButton_Click(sender, e)
        End If
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
        ExeComboBox.Items.Clear()
        For Each Folder As String In RootFolders
            AddEXE(Folder, "\waifu2x-caffe-cui.exe", "Waifu2x Caffe", CaffePath)
            AddEXE(Folder, "\waifu2x-ncnn-vulkan.exe", "Waifu2x Vulkan", WaifuNcnnPath)
            AddEXE(Folder, "\realsr-ncnn-vulkan.exe", "RealSR Vulkan", RealSRNcnnPath)
            AddEXE(Folder, "\realesrgan-ncnn-vulkan.exe", "RealESRGAN Vulkan", RealESRGNcnnPath)
            AddEXE(Folder, "\srmd-ncnn-vulkan.exe", "SRMD Vulkan", SRMDNcnnPath)
            AddEXE(Folder, "\waifu2x-converter-cpp.exe", "Waifu2x CPP", WaifuCppPath)
            AddEXE(Folder, "\Anime4KCPP_CLI.exe", "Anime4k CPP", Anime4kPath)
            AddEXE(Folder, "\texconv.exe", "TexConv", TexConvPath)
            AddEXE(Folder, "\ScalerTest_Windows.exe", "xBRZ", xBRZPath)
            If File.Exists(Folder & "\esrgan.exe") Then
                PyModels.Clear()
                PyModel.Items.Clear()
                ExeComboBox.Items.Add("ESRGAN")
                PyPath = "\" & IIf(Folder <> Root, Path.GetFileName(Folder), "") & "\esrgan.exe"
                For Each SubFolder As String In Directory.GetDirectories(Folder)
                    Dim Models As String() = Directory.EnumerateFiles(SubFolder, "*.pth").ToArray
                    For Each PythonModel As String In Models
                        PyModels.Add(PythonModel)
                        PyModel.Items.Add(Path.GetFileName(PythonModel))
                    Next
                Next
                If PyModels.Count > 0 Then
                    PyModel.SelectedIndex = 0
                Else
                    MsgBox("No ESRGAN Models Found!", MsgBoxStyle.Critical)
                End If
            End If
        Next
    End Sub

    Private Sub AddEXE(Source As String, ExeName As String, ModelName As String, ByRef ModelPath As String)
        If File.Exists(Source & ExeName) Then
            ExeComboBox.Items.Add(ModelName)
            ModelPath = "\" & IIf(Source <> Root, Path.GetFileName(Source), "") & ExeName
        End If
    End Sub

    Private Sub PreloadImageList()
        ChainThumbs.Add(My.Resources._0)
        ChainThumbs.Add(My.Resources._1)
        ChainThumbs.Add(My.Resources._2)
        ChainThumbs.Add(My.Resources._3)
        ChainThumbs.Add(My.Resources._4)
        ChainThumbs.Add(My.Resources._5)
        ChainThumbs.Add(My.Resources._6)
        ChainThumbs.Add(My.Resources._7)
        ChainThumbs.Add(My.Resources._8)
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

    Private Sub ExeBrowse_Click(sender As Object, e As EventArgs) Handles ExeBrowse.Click
        ExeTextBox.Text = GetFolder()
    End Sub

    Private Sub ExeTextBox_TextChanged(sender As Object, e As EventArgs) Handles ExeTextBox.TextChanged
        If Directory.Exists(ExeTextBox.Text) = True Then
            Root = ExeTextBox.Text
        Else
            Root = Application.StartupPath
        End If
        StartUpCheckEXE()
        If ExeComboBox.Items.Count > 0 Then
            ExeComboBox.SelectedIndex = 0
            SetSettingsWindow()
        End If
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
                Dim LoadedChain As List(Of FormSettings.ChainObject) = Nothing
                Try
                    LoadedChain = Deserialize(Of List(Of FormSettings.ChainObject))(File.ReadAllText(OFD.FileName))
                Catch ex As Exception
                    LoadedChain = Nothing
                End Try
                If LoadedChain Is Nothing Then
                    MsgBox("Error: That file could not be parsed as a chain.", MsgBoxStyle.Exclamation)
                    Return
                End If
                ChainList = LoadedChain
                RebuildChainPreview()
            End If
        End Using
    End Sub

    ' Rebuilds the preview from ChainList.  Captions come from GetChainDisplayName, so a chain
    ' loaded from a settings file or an .xml preset shows the real model again - the persisted Name
    ' only identifies the backend.
    Public Sub RebuildChainPreview()
        If ChainControl Is Nothing Then Return
        ChainControl.SelectedIndex = -1
        ChainControl.ListItems.Clear()
        For i = 0 To ChainList.Count - 1
            ChainControl.ListItems.Add(New DragDropList.DragDropItem(i, GetChainDisplayName(ChainList(i)), GetChainThumb(ChainList(i).IconIndex)))
        Next
        ChainControl.ReorderList()
        ChainControl.DrawList(ChainControl.ListItems)
    End Sub

    Private Function GetChainThumb(IconIndex As Integer) As Bitmap
        If IconIndex < 0 OrElse IconIndex >= ChainThumbs.Count Then IconIndex = 0
        Return TryCast(ChainThumbs.Item(IconIndex), Bitmap)
    End Function

    ' Caption shown under a chain thumbnail, derived from the model that will actually run.
    '
    ' ChainObject.Name cannot carry it: MakeUpscale compares Name against "TexConv" to decide where
    ' the pre/post-processing steps belong, so Name has to stay the backend id.  ESRGAN is the case
    ' that matters - it is the only backend where the user picks an actual model file, and every
    ' ESRGAN step used to be labelled just "ESRGAN", making two different models indistinguishable.
    Public Shared Function GetChainDisplayName(Item As FormSettings.ChainObject) As String
        If Item.PackageType = "ESRGAN" Then
            Try
                Dim PyPackage As FormSettings.PythonPackage = DirectCast(Item.Package, FormSettings.PythonPackage)
                If Not String.IsNullOrEmpty(PyPackage.Model) Then
                    Dim ModelName As String = Path.GetFileNameWithoutExtension(PyPackage.Model)
                    If ModelName.Length > 0 Then Return ModelName
                End If
            Catch ex As Exception
                ' Unboxing fails on a hand-edited or older preset; fall back to the backend id.
            End Try
        End If
        If String.IsNullOrEmpty(Item.Name) Then Return Item.PackageType
        Return Item.Name
    End Function

    Private Sub ChainAdd_Click(sender As Object, e As EventArgs) Handles ChainAdd.Click
        AddModelToChain(ExeComboBox.SelectedItem)
    End Sub

    ' The item the context menu was opened on, resolved while the cursor is still over the
    ' thumbnail.  -1 when the menu was opened on empty space.
    Private ContextIndex As Integer = -1

    Private Sub ChainContext_Opening(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles ChainContext.Opening
        ' Once the menu is on screen the mouse is over the MENU, not over the preview.  Asking for
        ' the index inside the click handlers therefore returned a stale value, and Edit/Delete hit
        ' whichever model happened to be clicked last - or threw on an out-of-range index.
        ContextIndex = ChainControl.GetIndexAt(ChainPreview.PointToClient(Control.MousePosition))
        Dim Valid As Boolean = ContextIndex >= 0 AndAlso ContextIndex < ChainList.Count
        ChainContextEdit.Enabled = Valid
        ChainContextDelete.Enabled = Valid
    End Sub

    Private Sub ChainContextEdit_Click(sender As Object, e As EventArgs) Handles ChainContextEdit.Click
        If ContextIndex < 0 OrElse ContextIndex >= ChainList.Count Then Return
        Using ECD As New EditChainDialog(Serialize(ChainList(ContextIndex)))
            If ECD.ShowDialog = DialogResult.OK Then
                Try
                    ChainList(ContextIndex) = Deserialize(Of FormSettings.ChainObject)(ECD.ResultText)
                Catch ex As Exception
                    MsgBox("Error: New settings could not be parsed.")
                    Return
                End Try
                ' The edited XML can point the step at a different model, so refresh the caption.
                RefreshChainNames()
            End If
        End Using
    End Sub

    Private Sub ChainContextDelete_Click(sender As Object, e As EventArgs) Handles ChainContextDelete.Click
        RemoveChainItemAt(ContextIndex)
        ContextIndex = -1
    End Sub

    Private Sub ChainRemove_Click(sender As Object, e As EventArgs) Handles ChainRemove.Click
        ' This button was in the UI but had no handler at all, so it silently did nothing.
        RemoveChainItemAt(ChainControl.SelectedIndex)
    End Sub

    Private Sub RemoveChainItemAt(Index As Integer)
        If ChainControl Is Nothing Then Return
        If Index < 0 Then Return
        If Index >= ChainList.Count OrElse Index >= ChainControl.ListItems.Count Then Return
        ChainList.RemoveAt(Index)
        ChainControl.ListItems.RemoveAt(Index)
        ChainControl.ReorderList()
        ChainControl.DrawList(ChainControl.ListItems)
    End Sub

    ' Rewrites only the captions, keeping order, selection and thumbnails intact.
    Private Sub RefreshChainNames()
        If ChainControl Is Nothing Then Return
        If ChainControl.ListItems.Count <> ChainList.Count Then Return
        For i = 0 To ChainControl.ListItems.Count - 1
            Dim Item As DragDropList.DragDropItem = ChainControl.ListItems(i)
            ChainControl.ListItems(i) = New DragDropList.DragDropItem(Item.Index, GetChainDisplayName(ChainList(i)), GetChainThumb(ChainList(i).IconIndex))
        Next
        ChainControl.DrawList(ChainControl.ListItems)
    End Sub

    ' A drag has been committed.  DragDropList raises this BEFORE it renumbers the items, so
    ' Item.Index is still the ChainList position that thumbnail came from.
    '
    ' The old code did this from ChainPreview's MouseUp event.  WinForms runs that handler before
    ' DragDropList commits the drag, so it rebuilt ChainList from the pre-drag order - the reorder
    ' was thrown away and the renumbering that followed made the two lists disagree, which is how
    ' models ended up overwritten.
    Private Sub ChainReordered(sender As Object, e As EventArgs)
        If ChainControl Is Nothing Then Return
        If ChainControl.ListItems.Count <> ChainList.Count Then Return
        Dim Reordered As New List(Of FormSettings.ChainObject)
        For Each Item As DragDropList.DragDropItem In ChainControl.ListItems
            If Item.Index < 0 OrElse Item.Index >= ChainList.Count Then Return
            Reordered.Add(ChainList(Item.Index))
        Next
        ChainList = Reordered
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
                        LoadedSettings.Paths = New FormSettings.ProgramPaths(TempPath, Directory.GetParent(SFD.FileName).FullName, Root)
                        If ChainList.Count = 0 Then
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
            SkipList.Clear()
        Else
            WatchDog.Enabled = Not WatchDog.Enabled
            WatchDogButton.Text = "Running: " & WatchDog.Enabled
            SwitchGroups(Not WatchDog.Enabled)
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
        xBRZGroup.Visible = False
        PyGroup.Visible = False
        VulkanNoise.Enabled = True
        Select Case ExeComboBox.SelectedItem
            Case "Waifu2x Caffe"
                MoveShowGroup(CaffeGroup)
            Case "Waifu2x Vulkan"
                MoveShowGroup(VulkanGroup)
                VulkanScale.Value = 2
                VulkanScale.Enabled = True
            Case "RealSR Vulkan", "RealESRGAN Vulkan"
                MoveShowGroup(VulkanGroup)
                VulkanScale.Value = 4
                VulkanScale.Enabled = False
                VulkanNoise.Enabled = False
            Case "SRMD Vulkan"
                MoveShowGroup(VulkanGroup)
                VulkanScale.Value = 4
                VulkanScale.Enabled = False
            Case "Waifu2x CPP"
                MoveShowGroup(WaifuCPPGroup)
            Case "Anime4k CPP"
                MoveShowGroup(AnimeCPPGroup)
            Case "TexConv"
                MoveShowGroup(DDxGroup)
            Case "xBRZ"
                MoveShowGroup(xBRZGroup)
            Case "ESRGAN"
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
        Dim Source = Directory.GetFiles(InputTextBox.Text, "*.*", SearchOption.AllDirectories).Count
        Dim FileCheck = GetMissingFiles(InputTextBox.Text, OutputTextBox.Text).Count
        If Source = 0 OrElse FileCheck = 0 Then
            WaitScale = Math.Min(WaitScale + 1, 100)
            WatchDog.Interval = 1000 + (WaitScale * 590)
        Else
            WaitScale = 0
            WatchDog.Interval = 1000
            LoadedSettings = New FormSettings.Settings(Me)
            If ChainList.Count = 0 Then
                AddModelToChain(ExeComboBox.SelectedItem, False)
            End If
            WorkHorse.RunWorkerAsync()
        End If
    End Sub

    ' The progress bar shows how much of the dump is upscaled overall - how many input files
    ' already have a counterpart in the output folder - rather than how far the current batch got.
    '
    ' Reporting once per batch cannot express that: with "Threads: All" a whole run is a single
    ' batch, so the bar sat at 0 for the entire queue, jumped to 100 at the end, and went back to 0
    ' when the watchdog started the next run.  Polling the two folders keeps it meaningful across
    ' batches, across runs, and even while nothing is running at all.
    Private Sub ProgressPollTimer_Tick(sender As Object, e As EventArgs) Handles ProgressPollTimer.Tick
        Try
            Dim Percent As Integer = GetOverallProgress()
            If Percent < UpscaleProgress.Minimum Then Percent = UpscaleProgress.Minimum
            If Percent > UpscaleProgress.Maximum Then Percent = UpscaleProgress.Maximum
            UpscaleProgress.Value = Percent
        Catch ex As Exception
            ' A folder on a removable or network drive can vanish mid-scan; never take the UI down.
        End Try
        ' Both folder trees are walked on the UI thread every tick, so poll once a second while
        ' textures are actually being written and back off to five seconds when idle.
        ProgressPollTimer.Interval = IIf(WorkHorse.IsBusy, 1000, 5000)
    End Sub

    Private Function GetOverallProgress() As Integer
        Dim InputPath As String = InputTextBox.Text
        Dim OutputPath As String = OutputTextBox.Text
        If Not Directory.Exists(InputPath) OrElse Not Directory.Exists(OutputPath) Then Return 0
        Dim InputFiles As String() = Directory.GetFiles(InputPath, "*.*", SearchOption.AllDirectories)
        If InputFiles.Count = 0 Then Return 0
        Dim Done As Integer = InputFiles.Count - GetMissingFiles(InputFiles, OutputPath).Count
        If Done <= 0 Then Return 0
        If Done >= InputFiles.Count Then Return 100
        ' Double arithmetic: an Integer Done * 100 would overflow on an absurdly large dump.
        Return CInt(Math.Floor((Done * 100.0R) / InputFiles.Count))
    End Function

    Private Sub WorkHorse_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles WorkHorse.DoWork
        WatchDog.Stop()
        MakeUpscale()
        If WorkHorse.CancellationPending = True Then
            e.Cancel = True
        End If
    End Sub

    Private Sub WorkHorse_ProgressChanged(sender As Object, e As System.ComponentModel.ProgressChangedEventArgs) Handles WorkHorse.ProgressChanged
        ' The progress bar is deliberately not touched here - ProgressPollTimer owns it, because
        ' this event only fires once per batch.  What this handler is for is poking the emulator to
        ' reload its textures as soon as a batch has landed.
        If (HotKeyCheckbox.Checked = True) AndAlso (GetActiveWindow <> Me.Handle) Then
            SendKeys.Send(HotToggle)
            Threading.Thread.Sleep(200)
            SendKeys.Send(HotToggle)
        End If
    End Sub

    Private Sub WorkHorse_RunWorkerCompleted(sender As Object, e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles WorkHorse.RunWorkerCompleted
        ' Drop the backend that was added implicitly for this run.  The old check tested the
        ' preview, which the implicit entry had already been added to, so it never cleared: the
        ' silently added model stayed in the chain and was written to settings.xml on exit.
        If ImplicitChainCount > 0 Then
            For i = 1 To ImplicitChainCount
                If ChainList.Count > 0 Then ChainList.RemoveAt(ChainList.Count - 1)
            Next
            ImplicitChainCount = 0
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
        Dim TempPath As String = GetChainPath("Temp", 0)
        Dim Source As String() = GetMissingFiles(LoadedSettings.Paths.InputPath, LoadedSettings.Paths.OutputPath)
        Dim ThreadCount As Integer = GetThreads(LoadedSettings.BasicSettings.ThreadIndex, LoadedSettings.BasicSettings.ThreadCount, Source.Count)
        ' "Threads: All" now hands the whole queue over in a single batch.  That is exactly what
        ' the folder-based backends (ESRGAN / *-ncnn-vulkan) want: one Python/Vulkan launch for
        ' every pending texture instead of a restart per batch.  A per-image backend however
        ' spawns one process per texture, so in that mode its concurrency is capped at the core
        ' count to keep a large queue from flooding the system with processes.
        Dim PerImageLimit As Integer = ThreadCount
        If LoadedSettings.BasicSettings.ThreadIndex = 2 Then
            PerImageLimit = Math.Max(1, Environment.ProcessorCount)
        End If
        For i = 0 To Source.Count - 1 Step ThreadCount
            Dim ChainPaths As New List(Of String)
            Dim DeletedChainPaths As New List(Of String)
            ChainPaths.Add(TempPath)
            For j = 0 To ChainList.Count - 2
                Dim TempName As String = GetChainPath("Chain", j)
                ChainPaths.Add(TempName)
                Directory.CreateDirectory(TempName)
            Next
            ChainPaths.Add(LoadedSettings.Paths.OutputPath)
            Directory.CreateDirectory(TempPath)
            CopyFiles(Source, SkipList, TempPath, i, ThreadCount)
            For Each Model In ChainList
                Dim NewImages As New List(Of String)
                Dim DiffImages = GetMissingFiles(ChainPaths(0), LoadedSettings.Paths.OutputPath)
                For Each NewImage As String In DiffImages
                    Dim AcceptExt As Boolean = Model.Package.FileTypes.Contains(Path.GetExtension(NewImage).ToLower)
                    If File.Exists(NewImage) AndAlso AcceptExt = True Then
                        NewImages.Add(NewImage)
                        If LoadedSettings.BasicSettings.FixPS2 = True Then
                            If (ChainList.IndexOf(Model) = 0 AndAlso Model.Name <> "TexConv") OrElse (ChainList(0).Name = "TexConv" AndAlso ChainList.IndexOf(Model) = 1) Then
                                RemovePS2Alpha(NewImage)
                            End If
                        End If
                        If LoadedSettings.ExpertSettings.SeamlessMode > 0 Then
                            If (ChainList.IndexOf(Model) = 0 AndAlso Model.Name <> "TexConv") OrElse (ChainList(0).Name = "TexConv" AndAlso ChainList.IndexOf(Model) = 1) Then
                                Dim SeamlessImage As Bitmap = GetUnlockedImage(NewImage)
                                SeamlessImage = MakeSeamless(SeamlessImage, LoadedSettings.ExpertSettings.SeamlessMode, LoadedSettings.ExpertSettings.SeamlessMargin)
                                SeamlessImage.Save(NewImage)
                            End If
                        End If
                    End If
                Next
                StartBuilder(ChainPaths(0), ChainPaths(1), NewImages, Model, PerImageLimit)
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
                                Dim CroppedImage As Bitmap = GetUnlockedImage(ChainPaths(0) & "\" & Path.GetFileName(NewImage))
                                CroppedImage = CropImage(CroppedImage, ScaleVal, ScaleVal, CroppedImage.Width - (ScaleVal * 2), CroppedImage.Height - (ScaleVal * 2), 0)
                                CroppedImage.Save(ChainPaths(0) & "\" & Path.GetFileName(NewImage))
                            End If
                        Next
                    End If
                    If LoadedSettings.BasicSettings.FixPS2 = True Then
                        For Each NewImage In NewImages
                            If File.Exists(ChainPaths(0) & "\" & Path.GetFileName(NewImage)) Then
                                AddPS2Alpha(ChainPaths(0) & "\" & Path.GetFileName(NewImage))
                            End If
                        Next
                    End If
                End If
                If WorkHorse.CancellationPending = True Then
                    Directory.Delete(TempPath, True)
                    For j = 0 To ChainList.Count - 2
                        Dim TempName As String = GetChainPath("Chain", j)
                        Directory.Delete(TempName, True)
                    Next
                    Exit Sub
                End If
            Next
            For Each ChainDir As String In DeletedChainPaths
                Directory.Delete(ChainDir, True)
            Next
            ' Drives the texture-reload hotkey, once per batch.  ReportProgress throws outside
            ' 0-100, so the value is clamped; the bar itself is owned by ProgressPollTimer.
            WorkHorse.ReportProgress(Math.Max(0, Math.Min(100, CInt(Math.Floor(((i + ThreadCount) * 100) / Source.Count)))))
        Next
        If CleanupCheckBox.Checked = True Then
            For Each SourceImage As String In Source
                File.Delete(SourceImage)
            Next
        End If
    End Sub

#End Region

#Region "Upscale Subroutines"

    Private Sub CopyFiles(FileList As String(), ByRef SkipList As List(Of String), RootPath As String, ByRef CurrentIndex As Integer, BatchSize As Integer)
        Dim CopyCounter As Integer = 0
        Do While CopyCounter < BatchSize
            Dim FilePath As String = FileList(CurrentIndex)
            If Not SkipList.Contains(FilePath) Then
                Select Case LoadedSettings.ExpertSettings.AlphaMode
                    Case 0
                        File.Copy(FilePath, RootPath & "\" & Path.GetFileName(FilePath), True)
                        CopyCounter += 1
                    Case 1
                        If Not GetHasTransparency(FilePath) Then
                            File.Copy(FilePath, RootPath & "\" & Path.GetFileName(FilePath), True)
                            CopyCounter += 1
                        Else
                            SkipList.Add(FilePath)
                        End If
                    Case 2
                        If GetHasTransparency(FilePath) Then
                            File.Copy(FilePath, RootPath & "\" & Path.GetFileName(FilePath), True)
                            CopyCounter += 1
                        Else
                            Skiplist.Add(filepath)
                        End If
                End Select
            End If
            If CurrentIndex >= FileList.Count - 1 Then Exit Do
            CurrentIndex += 1
        Loop
    End Sub

    Private Sub StartBuilder(SourcePath As String, DestPath As String, ImageList As List(Of String), Model As FormSettings.ChainObject, Optional PerImageLimit As Integer = 0)
        If ImageList.Count = 0 Then Return

        Dim ExePath As String = Root & Model.FileLocation
        Dim WorkingDir As String = Directory.GetParent(ExePath).FullName

        If Model.PackageType = "ESRGAN" OrElse Model.PackageType.Contains("Vulkan") Then
            ' Folder-based backend: a single process consumes the whole batch, so "Threads: All"
            ' means exactly one Python/Vulkan launch for the entire queue.
            Dim FolderJob As New BackendJob(MakeStartInfo(ExePath, WorkingDir, MakeCommand(SourcePath, DestPath, Model.PackageType, Model.Package)))
            Try
                FolderJob.Start()
                FolderJob.Wait(AddressOf IsCancelled)
                LogJob(FolderJob)
            Finally
                FolderJob.Dispose()
            End Try
        Else
            ' Per-image backend: one process per texture.  A full "Threads: All" queue would
            ' otherwise spawn hundreds of processes at once, so the number in flight is capped.
            Dim MaxConcurrent As Integer = ImageList.Count
            If PerImageLimit > 0 AndAlso PerImageLimit < MaxConcurrent Then MaxConcurrent = PerImageLimit

            Dim Active As New List(Of BackendJob)
            Try
                For j = 0 To ImageList.Count - 1
                    Dim NewImage As String = DestPath & "\" & Path.GetFileName(ImageList(j))
                    Dim ItemJob As BackendJob = New BackendJob(MakeStartInfo(ExePath, WorkingDir, MakeCommand(ImageList(j), NewImage, Model.PackageType, Model.Package)))
                    Active.Add(ItemJob)
                    ItemJob.Start()

                    If Active.Count >= MaxConcurrent Then
                        RetireFinished(Active)
                        If Active.Count >= MaxConcurrent Then
                            ' Still saturated: block on the oldest job until a slot frees up.
                            Active(0).Wait(AddressOf IsCancelled)
                            LogJob(Active(0))
                            Active(0).Dispose()
                            Active.RemoveAt(0)
                        End If
                    End If
                Next
                For Each Pending As BackendJob In Active
                    Pending.Wait(AddressOf IsCancelled)
                    LogJob(Pending)
                Next
            Finally
                For Each Remaining As BackendJob In Active
                    Remaining.Dispose()
                Next
                Active.Clear()
            End Try
        End If

        For Each TempImage As String In Directory.GetFiles(SourcePath)
            File.Delete(TempImage)
        Next
    End Sub

    Private Function MakeStartInfo(ExePath As String, WorkingDir As String, Arguments As String) As ProcessStartInfo
        Dim Result As New ProcessStartInfo(ExePath, Arguments)
        Result.WorkingDirectory = WorkingDir
        Result.RedirectStandardOutput = True
        Result.RedirectStandardError = True
        Result.UseShellExecute = False
        Result.CreateNoWindow = True
        Return Result
    End Function

    ' Logs and releases every job that has already finished.  Iterating backwards keeps the
    ' remaining indices valid while items are removed.
    Private Sub RetireFinished(Active As List(Of BackendJob))
        For i = Active.Count - 1 To 0 Step -1
            If Active(i).HasExited Then
                Active(i).Wait(AddressOf IsCancelled) ' cheap: already exited, only drains the pipes
                LogJob(Active(i))
                Active(i).Dispose()
                Active.RemoveAt(i)
            End If
        Next
    End Sub

    Private Sub LogJob(Job As BackendJob)
        If LoadedSettings.ExpertSettings.Logging = True Then
            WriteLog(Job, LoadedSettings.Paths.OutputPath)
        End If
    End Sub

    Private Function IsCancelled() As Boolean
        Return WorkHorse.CancellationPending
    End Function

    Private Function GetChainPath(PathType As String, PathIndex As Integer) As String
        Return Path.GetTempPath & PathType & "_" & PathIndex & "_" & LoadedSettings.ExpertSettings.AlphaMode
    End Function

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
            Case "RealESRGAN Vulkan"
                Return MakeVulkanCommand(Source, Dest, True, Package)
            Case "SRMD Vulkan"
                Return MakeVulkanCommand(Source, Dest, False, Package)
            Case "Waifu2x CPP"
                Return MakeCPPCommand(Source, Dest, Package)
            Case "Anime4k CPP"
                Return MakeA4KCommand(Source, Dest, Package)
            Case "TexConv"
                Return MakeTexConvCommand(Source, Dest, Package)
            Case "xBRZ"
                Return MakeXBRZCommand(Source, Dest, Package)
            Case "ESRGAN"
                Return MakePyCommand(Source, Dest, Package)
        End Select
        Return ""
    End Function

    Private Sub AddModelToChain(Mode As String, Optional AddPreview As Boolean = True)
        If String.IsNullOrEmpty(Mode) Then Return
        Dim NewItem As FormSettings.ChainObject
        Select Case Mode
            Case "Waifu2x Caffe"
                NewItem = New FormSettings.ChainObject("Caffe", 0, CaffePath, "Waifu2x Caffe", Me)
            Case "Waifu2x Vulkan"
                NewItem = New FormSettings.ChainObject("Waifu Vulkan", 1, WaifuNcnnPath, "Waifu2x Vulkan", Me)
            Case "RealSR Vulkan"
                NewItem = New FormSettings.ChainObject("RealSR Vulkan", 2, RealSRNcnnPath, "RealSR Vulkan", Me)
            Case "RealESRGAN Vulkan"
                NewItem = New FormSettings.ChainObject("RealESRGAN Vulkan", 8, RealESRGNcnnPath, "RealESRGAN Vulkan", Me)
            Case "SRMD Vulkan"
                NewItem = New FormSettings.ChainObject("SRMD Vulkan", 3, SRMDNcnnPath, "SRMD Vulkan", Me)
            Case "Waifu2x CPP"
                NewItem = New FormSettings.ChainObject("Waifu CPP", 4, WaifuCppPath, "Waifu2x CPP", Me)
            Case "Anime4k CPP"
                NewItem = New FormSettings.ChainObject("Anime4k", 5, Anime4kPath, "Anime4k CPP", Me)
            Case "TexConv"
                NewItem = New FormSettings.ChainObject("TexConv", 7, TexConvPath, "TexConv", Me)
            Case "xBRZ"
                NewItem = New FormSettings.ChainObject("xBRZ", 0, xBRZPath, "xBRZ", Me)
            Case "ESRGAN"
                NewItem = New FormSettings.ChainObject("ESRGAN", 6, PyPath, "ESRGAN", Me)
            Case Else
                Return ' Backend not present in the ExeComboBox, nothing to add.
        End Select

        ChainList.Add(NewItem)

        ' AddPreview was declared but never honoured: a backend picked implicitly for one run was
        ' added to the visible chain as well, and then never removed again.
        If Not AddPreview Then
            ImplicitChainCount += 1
            Return
        End If

        ChainControl.ListItems.Add(New DragDropList.DragDropItem(ChainList.Count - 1, GetChainDisplayName(NewItem), GetChainThumb(NewItem.IconIndex)))
        ChainControl.DrawList(ChainControl.ListItems)
    End Sub

#End Region

#Region "Commands"

    Private Function MakeCaffeCommand(SourceImage As String, NewImage As String, Package As FormSettings.Waifu2xCaffePackage) As String
        Dim Result As New ArguementString
        Result.AddArguement("-i", Quote(SourceImage))
        Result.AddArguement("-o", Quote(NewImage))
        Result.AddArguement(LoadedSettings.ExpertSettings.ExpertFlags)
        Result.AddArguement("-m", Package.Mode)
        Result.AddArguement("-s", Package.Scale.ToString)
        Result.AddArguement("-n", Package.Noise.ToString)
        Result.AddArguement("-p", Package.Process)
        Result.AddArguement("-t", IIf(Package.TAA = True, 1, 0))
        Return Result.GetArguements
    End Function

    Private Function MakeVulkanCommand(SourceImage As String, NewImage As String, NoNoise As Boolean, Package As FormSettings.VulkanNcnnPackage) As String
        Dim Result As New ArguementString
        Result.AddArguement("-i", Quote(SourceImage))
        Result.AddArguement("-o", Quote(NewImage))
        Result.AddArguement(LoadedSettings.ExpertSettings.ExpertFlags)
        Result.AddArguement("-s", Package.Scale.ToString)
        If NoNoise = False Then Result.AddArguement("-n", Package.Noise.ToString)
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
        Result.AddArguement("--scale-ratio", Package.Scale.ToString)
        Result.AddArguement("--noise-level", Package.Noise.ToString)
        Result.AddArguement("-f", Package.Format)
        Result.AddArguement("--disable-gpu", Package.GPU)
        Result.AddArguement("--force-OpenCL", Package.ForceOpenCL)
        Return Result.GetArguements
    End Function

    Private Function MakeA4KCommand(SourceImage As String, NewImage As String, Package As FormSettings.Anime4kPackage) As String
        Dim Result As New ArguementString
        Result.AddArguement("-i", Quote(SourceImage))
        Result.AddArguement("-o", Quote(NewImage))
        Result.AddArguement(LoadedSettings.ExpertSettings.ExpertFlags)
        Result.AddArguement("-p", Package.Passes.ToString)
        Result.AddArguement("-n", Package.PushColors.ToString)
        Result.AddArguement("-c", Package.PushColorStrength.ToString)
        Result.AddArguement("-g", Package.PushGradStrength.ToString)
        Result.AddArguement("-z", Package.Scale.ToString)
        Result.AddArguement("-b", Package.PreProcess)
        Result.AddArguement("-r " & Package.PreFilterType, Package.PreFilter)
        Result.AddArguement("-a", Package.PostProcess)
        Result.AddArguement("-e " & Package.PostFilterType, Package.PostFilter)
        Result.AddArguement("-q", Package.GPU)
        Result.AddArguement("-w", Package.CNN)
        Result.AddArguement("-A")
        Return Result.GetArguements
    End Function

    Private Function MakeTexConvCommand(SourceImage As String, NewImage As String, Package As FormSettings.DDxPackage) As String
        Dim Result As New ArguementString
        Result.AddArguement("-f", Package.Format)
        Result.AddArguement("-nologo")
        Select Case Package.Mode
            Case "DDS Input"
                Result.AddArguement("-ft " & Package.ConversionFormat.ToLower)
            Case "DDS Output"
                Result.AddArguement("-fl " & Package.FeatureLevel, Package.FeatureLevel <> "11.0")
                Result.AddArguement("-dx9", Package.ForceDx9)
                Result.AddArguement("-dx10", Package.ForceDx10)
        End Select
        Result.AddArguement("-sepalpha", Package.SeperateAlpha)
        Result.AddArguement("-pmalpha", Package.PremultiplyAlpha)
        Result.AddArguement("-alpha", Package.StraightAlpha)
        Result.AddArguement("-o", Quote(Path.GetDirectoryName(NewImage).TrimEnd({"\"c, "/"c})))
        Result.AddArguement(Quote(SourceImage))
        Return Result.GetArguements
    End Function

    Private Function MakeXBRZCommand(SourceImage As String, NewImage As String, Package As FormSettings.xBRZPackage) As String
        Dim Result As New ArguementString
        Result.AddArguement("", "-" & CInt(Package.Scale) & "xBRZ")
        Result.AddArguement("", Quote(SourceImage))
        Result.AddArguement("", Quote(NewImage))
        Return Result.GetArguements
    End Function

    Private Function MakePyCommand(SourceFolder As String, DestFolder As String, Package As FormSettings.PythonPackage) As String
        Dim Result As New ArguementString
        Result.AddArguement(Quote(Package.Model))
        Result.AddArguement("--input", Quote(SourceFolder))
        Result.AddArguement("--output", Quote(DestFolder))
        Result.AddArguement("--tile_size", Package.TileSize.ToString)
        Result.AddArguement("--cpu", Package.CPUOnly.ToString)
        Return Result.GetArguements
    End Function

#End Region

#Region "Graphics"

    Private Function MakeSeamless(Source As Bitmap, Mirrored As Integer, Margin As Integer) As Bitmap
        Dim Result As New Bitmap(Source.Width * 3, Source.Height * 3, Source.PixelFormat)
        Using g As Graphics = Graphics.FromImage(Result)
            g.CompositingMode = Drawing2D.CompositingMode.SourceCopy
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

    Private Function GetHasTransparency(Source As String) As Boolean
        Dim SourceImage As Bitmap = GetUnlockedImage(Source)
        Dim SourceRect As Rectangle = New Rectangle(0, 0, SourceImage.Width, SourceImage.Height)
        Dim SourceData As Imaging.BitmapData = SourceImage.LockBits(SourceRect, Imaging.ImageLockMode.ReadWrite, SourceImage.PixelFormat)
        Dim SourcePtr As IntPtr = SourceData.Scan0
        Dim SourceByteCount As Integer = Math.Abs(SourceData.Stride) * SourceImage.Height
        Dim SourceBytes As Byte() = New Byte(SourceByteCount - 1) {}
        Runtime.InteropServices.Marshal.Copy(SourcePtr, SourceBytes, 0, SourceByteCount)
        For i = 3 To SourceBytes.Length - 1 Step 4
            If SourceBytes(i) = 0 Then Return True
        Next
        SourceImage.UnlockBits(SourceData)
        SourceImage.Dispose()
        Return False
    End Function

    Private Function CropImage(Source As Bitmap, OffsetX As Integer, OffsetY As Integer, Width As Integer, Height As Integer, Margins As Integer) As Bitmap
        Dim CropSize As New Rectangle(OffsetX - Margins, OffsetY - Margins, Width + (2 * Margins), Height + (2 * Margins))
        Dim Result = New Bitmap(CropSize.Width, CropSize.Height, Source.PixelFormat)
        Using g As Graphics = Graphics.FromImage(Result)
            g.CompositingMode = Drawing2D.CompositingMode.SourceCopy
            g.PixelOffsetMode = Drawing2D.PixelOffsetMode.None
            g.SmoothingMode = Drawing2D.SmoothingMode.None
            g.InterpolationMode = Drawing2D.InterpolationMode.NearestNeighbor
            g.DrawImage(Source, New Rectangle(0, 0, CropSize.Width, CropSize.Height), CropSize, GraphicsUnit.Pixel)
        End Using
        Return Result
    End Function

    Private Sub Defringe(Source As String, Threshold As Integer)
        Using NewImage As DirectBitmap = LoadDirectBitmap(Source)
            ' Work straight on the pinned buffer: one array reference, one pass, pure integer
            ' math.  The old GetPixel/SetPixel path built a Color structure (and re-resolved
            ' the Bits property) for every single pixel, twice over.
            Dim Bits As Integer() = NewImage.Bits
            For i = 0 To Bits.Length - 1
                ' Alpha is the high byte.  VB's >> is an arithmetic shift, so mask afterwards
                ' to get an unsigned 0-255 value even when the packed ARGB integer is negative.
                If ((Bits(i) >> 24) And &HFF) < Threshold Then
                    Bits(i) = 0 ' Color.Transparent.ToArgb()
                End If
            Next
            NewImage.Bitmap.Save(Source)
        End Using
    End Sub

    Private Sub RemovePS2Alpha(Source As String)
        Using NewImage As DirectBitmap = LoadDirectBitmap(Source)
            Dim Bits As Integer() = NewImage.Bits
            Dim AlphaMax As Integer = 0
            For i = 0 To Bits.Length - 1
                Dim Pixel As Integer = Bits(i)
                Dim Alpha As Integer = (Pixel >> 24) And &HFF
                If Alpha > AlphaMax Then AlphaMax = Alpha
                ' PCSX2 dumps carry 0-128 alpha.  If this texture already uses the full 0-255
                ' range it is not a PS2 dump, so leave the file completely untouched.
                If AlphaMax > 128 Then Return
                If Alpha <> 0 Then
                    ' Keep the low 24 bits as-is and swap in the doubled alpha byte.  Alpha is
                    ' <= 128 here, so (Alpha * 2) - 1 can never exceed 255.
                    Bits(i) = (((Alpha * 2) - 1) << 24) Or (Pixel And &HFFFFFF)
                End If
            Next
            NewImage.Bitmap.Save(Source)
        End Using
    End Sub

    Private Sub AddPS2Alpha(Source As String)
        Using NewImage As DirectBitmap = LoadDirectBitmap(Source)
            Dim Bits As Integer() = NewImage.Bits
            For i = 0 To Bits.Length - 1
                Dim Pixel As Integer = Bits(i)
                Dim Alpha As Integer = (Pixel >> 24) And &HFF
                If Alpha <> 0 Then
                    ' Explicit CInt preserves VB's banker's rounding, which is what the old
                    ' implicit Double -> Integer conversion at Color.FromArgb did.
                    Dim NewAlpha As Integer = CInt((Alpha + 1) / 2)
                    Bits(i) = (NewAlpha << 24) Or (Pixel And &HFFFFFF)
                End If
            Next
            NewImage.Bitmap.Save(Source)
        End Using
    End Sub

    ' Loads a file into a pinned DirectBitmap and immediately releases the intermediate GDI+
    ' copy.  The old post-processing code disposed neither the source Bitmap nor the
    ' DirectBitmap, leaking a GDI+ handle and a pinned GCHandle for every texture processed.
    Private Function LoadDirectBitmap(Source As String) As DirectBitmap
        Dim SourceImage As Bitmap = GetUnlockedImage(Source)
        Try
            Return New DirectBitmap(SourceImage)
        Finally
            SourceImage.Dispose()
        End Try
    End Function

#End Region

#Region "XML"

    Public Shared Function Serialize(Of T)(Source As T) As String
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

    Public Shared Function Deserialize(Of T)(Xml As String) As T
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

    Private Declare Function GetActiveWindow Lib "user32" Alias "GetActiveWindow" () As IntPtr

    Private Function GetMissingFiles(Path1 As String, Path2 As String) As String()
        If Not Directory.Exists(Path1) Then Return New String() {}
        Return GetMissingFiles(Directory.GetFiles(Path1, "*.*", SearchOption.AllDirectories), Path2)
    End Function

    ' Files from InputFiles that have no counterpart in Path2 yet, matched on the file name without
    ' its extension (so a .png dump counts as done once TexConv has written the .dds).
    '
    ' The lookup runs through a HashSet rather than List.Contains: the old version did an O(n*m)
    ' string comparison per call, which was tolerable once per batch but is now also called once
    ' per second by the progress poller on dumps with thousands of textures.
    Private Function GetMissingFiles(InputFiles As String(), Path2 As String) As String()
        Dim DoneNames As New HashSet(Of String)
        If Directory.Exists(Path2) Then
            For Each DoneFile As String In Directory.GetFiles(Path2, "*.*", SearchOption.AllDirectories)
                DoneNames.Add(Path.GetFileNameWithoutExtension(DoneFile).ToLower)
            Next
        End If
        Dim Result As New List(Of String)
        For Each InputFile As String In InputFiles
            If Not DoneNames.Contains(Path.GetFileNameWithoutExtension(InputFile).ToLower) Then
                Result.Add(InputFile)
            End If
        Next
        Return Result.ToArray
    End Function

    ' Batch size used by MakeUpscale.  Index matches ThreadComboBox:
    '   0 = Single, 1 = Custom, 2 = All, 3 = Max (512)
    ' "All" returns the number of files still waiting, so the Step loop in MakeUpscale runs a
    ' single time and the backend receives the entire queue in one batch.  Folder-based
    ' backends (ESRGAN, *-ncnn-vulkan) therefore start Python/Vulkan exactly once instead of
    ' once per batch, which is what made the queue stutter on large dumps.
    Private Function GetThreads(Index As Integer, Count As Integer, TotalFiles As Integer) As Integer
        Select Case Index
            Case 0
                Return 1
            Case 1
                Return Math.Max(1, Count)
            Case 2
                Return Math.Max(1, TotalFiles)
            Case 3
                Return 512
        End Select
        Return 512
    End Function

    Private Function GetUnlockedImage(Source As String) As Bitmap
        Dim SourceImage As Bitmap = Image.FromFile(Source)
        Dim UnlockedImage As New Bitmap(SourceImage)
        SourceImage.Dispose()
        Return UnlockedImage
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

    ' Writes the output captured by a BackendJob.  Reading StandardOutput/StandardError here
    ' is no longer possible (and was itself a deadlock source): the pipes are drained
    ' asynchronously while the process runs.
    Private Sub WriteLog(Job As BackendJob, SaveLoc As String)
        Try
            Directory.CreateDirectory(SaveLoc)
            ' The process id keeps concurrent per-image backends that finish within the same
            ' second from overwriting each other's log file.
            Dim Filename As String = SaveLoc & "\Log_" & Now.ToString("yyyy-MM-dd_HH-mm-ss") & "_" & Job.ProcessId & ".txt"
            Dim Output As New Text.StringBuilder
            Output.AppendLine(Job.CommandLine)
            Output.AppendLine()
            Output.AppendLine(Job.GetOutput())
            Output.AppendLine()
            Output.AppendLine(Job.GetErrors())
            File.WriteAllText(Filename, Output.ToString())
        Catch ex As Exception
            ' Logging must never take the pipeline down with it.
        End Try
    End Sub

#End Region

End Class