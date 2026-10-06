Imports System.IO
Imports System.Reflection
Imports System.Threading
Imports System.Threading.Tasks

Public Class Form1

#Region "VARS"

    Dim Root As String = Application.StartupPath
    Dim AppData As String = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
    Dim WaitScale As Integer = 0
    Dim SettingsLoc As Point
    Dim LoadedSettings As FormSettings.Settings
    Dim SkipList As New List(Of String)
    Private LastSelectedSpandrelModelPath As String = ""
    Private ReadOnly UiToolTip As New ToolTip()
    Private WatcherEnabled As Boolean
    Private StopRequested As Boolean
    Private SettingsLoaded As Boolean
    Private SuppressCleanupPrompt As Boolean
    Private WatchScanRunning As Boolean
    Private ProgressScanRunning As Boolean
    Private ChainScrollPanel As Panel
    Private SpandrelModelPathLabel As Label

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
    Public Property PLKSRModelPath As String
    Public Property DAT2ModelPath As String
    Private Property SupportedSpandrelModels As New List(Of SpandrelModelInfo)

    Private Const PLKSRBackendName As String = "PLKSR"
    Private Const PLKSRCheckpointName As String = "4x-PBRify_RPLKSRd_V3.pth"
    Private Const DAT2BackendName As String = "DAT2"
    Private Const DAT2CheckpointName As String = "4x-PBRify_UpscalerV4.pth"
    Private Const SpandrelBackendName As String = "Spandrel"
    Private Const GenericModelFolderName As String = "models"
    Private Const LegacySpandrelModelFolderName As String = "Spandrel"
    Private Const SpandrelRunnerName As String = "spandrel_upscale.py"
    Private SpandrelScanGeneration As Integer = 0
    Private SpandrelScanCancellation As CancellationTokenSource
    Private ReadOnly ActiveProcessLock As New Object()
    Private ActiveProcesses As New List(Of Process)

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

    Private Class SpandrelModelInfo
        Public Property FilePath As String
        Public Property Architecture As String
        Public Property Scale As Integer
        Public Property Purpose As String
        Public Property InputChannels As Integer
        Public Property OutputChannels As Integer

        Public ReadOnly Property SelectorText As String
            Get
                If Scale <= 0 Then Return Path.GetFileName(FilePath) & " · Spandrel image model"
                Return Scale.ToString() & "× " & Purpose & " · " & Path.GetFileName(FilePath) & " (" & Architecture & ")"
            End Get
        End Property
    End Class

#End Region

#Region "Loading"

    Public Sub New()
        InitializeComponent()
        HideBackendGroups()
        ApplyStaticUiFixes()
    End Sub

    Private Async Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
        Application.CurrentCulture = New Globalization.CultureInfo("EN-US")
        PreloadImageList()
        PrepareChainViewport()
        ChainControl = New DragDropList(ChainPreview, 7)
        AddHandler ChainControl.ItemsReordered, AddressOf SyncChainListFromPreview
        AddHandler ChainControl.DeleteRequested, AddressOf RemoveSelectedChainItem
        AddHandler ChainControl.EditRequested, AddressOf EditSelectedChainItem
        AddHandler ChainControl.SelectionChanged, AddressOf UpdateChainCommandState
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
            Dim PreferredBackendIndex As Integer = GetPreferredSpandrelBackendIndex()
            If PreferredBackendIndex < 0 Then PreferredBackendIndex = 0
            ExeComboBox.SelectedIndex = PreferredBackendIndex
            If GetPreferredSpandrelBackendIndex() >= 0 Then ReplaceLegacyUpscalerChain()
            SetSettingsWindow()
        End If
        Await RefreshSupportedSpandrelModels(Root)
        If ChainControl.ListItems.Count > 0 Then ChainControl.SelectedIndex = 0
        ChainControl.DrawList(ChainControl.ListItems)
        UpdateChainCommandState()
        SettingsLoaded = True
        ' Show the current input/output completion immediately, then keep it refreshed.
        ProgressPollTimer.Enabled = True
        ProgressPollTimer_Tick(ProgressPollTimer, EventArgs.Empty)
        If Environment.GetCommandLineArgs.Count > 1 Then
            WatchDogButton_Click(sender, e)
        End If
    End Sub

    Private Sub Form1_Closing(sender As Object, e As EventArgs) Handles MyBase.Closing
        If SpandrelScanCancellation IsNot Nothing Then
            Try
                SpandrelScanCancellation.Cancel()
            Catch ex As ObjectDisposedException
                ' The scan already finished while the form was closing.
            End Try
        End If
        If PortableCheckBox.Checked = True Then
            File.WriteAllText(Root & "\portable.xml", Serialize(New FormSettings.Settings(Me)))
        Else
            File.WriteAllText(AppData & "\AutoCrispy\settings.xml", Serialize(New FormSettings.Settings(Me)))
        End If
        UiToolTip.Dispose()
    End Sub

    Private Sub StartUpCheckEXE()
        ExeComboBox.Items.Clear()
        PyModels.Clear()
        PyModel.Items.Clear()
        PyPath = ""
        PLKSRModelPath = ""
        DAT2ModelPath = ""
        SupportedSpandrelModels.Clear()

        If Not Directory.Exists(Root) Then
            PLKSRModelPath = FindSpandrelModel(Application.StartupPath, PLKSRCheckpointName)
            DAT2ModelPath = FindSpandrelModel(Application.StartupPath, DAT2CheckpointName)
            AddSpandrelBackends()
            Exit Sub
        End If

        Dim RootFolders As List(Of String) = Directory.GetDirectories(Root).ToList
        RootFolders.Add(Root)
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
                If Not ExeComboBox.Items.Contains("ESRGAN") Then ExeComboBox.Items.Add("ESRGAN")
                PyPath = "\" & IIf(Folder <> Root, Path.GetFileName(Folder), "") & "\esrgan.exe"
                For Each SubFolder As String In Directory.GetDirectories(Folder)
                    For Each PythonModel As String In Directory.EnumerateFiles(SubFolder, "*.pth")
                        Dim ModelName As String = Path.GetFileName(PythonModel)
                        Dim IsSpandrelCheckpoint As Boolean = ModelName.Equals(PLKSRCheckpointName, StringComparison.OrdinalIgnoreCase) OrElse ModelName.Equals(DAT2CheckpointName, StringComparison.OrdinalIgnoreCase)
                        If Not IsSpandrelCheckpoint AndAlso Not PyModels.Contains(PythonModel) Then
                            PyModels.Add(PythonModel)
                        End If
                    Next
                Next
            End If
        Next

        PLKSRModelPath = FindSpandrelModel(Root, PLKSRCheckpointName)
        DAT2ModelPath = FindSpandrelModel(Root, DAT2CheckpointName)
        If Not String.Equals(Root, Application.StartupPath, StringComparison.OrdinalIgnoreCase) Then
            If PLKSRModelPath = "" Then PLKSRModelPath = FindSpandrelModel(Application.StartupPath, PLKSRCheckpointName)
            If DAT2ModelPath = "" Then DAT2ModelPath = FindSpandrelModel(Application.StartupPath, DAT2CheckpointName)
        End If
        AddSpandrelBackends()

        If ExeComboBox.Items.Contains("ESRGAN") AndAlso PyModels.Count = 0 AndAlso PLKSRModelPath = "" AndAlso DAT2ModelPath = "" Then
            MsgBox("No ESRGAN Models Found!", MsgBoxStyle.Critical)
        End If
    End Sub

    Private Sub AddSpandrelBackends()
        If Not File.Exists(Path.Combine(Application.StartupPath, SpandrelRunnerName)) Then Return
        If SupportedSpandrelModels.Count > 0 AndAlso FindPythonExecutable() <> "" AndAlso
            Not ExeComboBox.Items.Contains(SpandrelBackendName) Then
            ExeComboBox.Items.Add(SpandrelBackendName)
        End If
    End Sub

    Private Function FindGenericSpandrelModelFolders(SearchRoot As String) As List(Of String)
        Dim Result As New List(Of String)
        If Not Directory.Exists(SearchRoot) Then Return Result

        Dim NormalizedSearchRoot As String = SearchRoot
        If Not String.Equals(SearchRoot, Path.GetPathRoot(SearchRoot), StringComparison.OrdinalIgnoreCase) Then
            NormalizedSearchRoot = SearchRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
        End If
        Dim RootFolderName As String = Path.GetFileName(NormalizedSearchRoot)
        If RootFolderName.Equals(GenericModelFolderName, StringComparison.OrdinalIgnoreCase) OrElse
            RootFolderName.Equals(LegacySpandrelModelFolderName, StringComparison.OrdinalIgnoreCase) Then
            Result.Add(NormalizedSearchRoot)
            Return Result
        End If

        For Each FolderName As String In New String() {GenericModelFolderName, LegacySpandrelModelFolderName}
            Dim ModelFolder As String = Path.Combine(NormalizedSearchRoot, FolderName)
            If Directory.Exists(ModelFolder) AndAlso Not Result.Contains(ModelFolder, StringComparer.OrdinalIgnoreCase) Then
                Result.Add(ModelFolder)
            End If
        Next
        Return Result
    End Function

    Private Async Function RefreshSupportedSpandrelModels(SearchRoot As String) As Task
        SpandrelScanGeneration += 1
        Dim ScanGeneration As Integer = SpandrelScanGeneration
        Dim SelectedBackendBeforeScan As String = If(ExeComboBox.SelectedItem, "").ToString()
        If String.Equals(SelectedBackendBeforeScan, SpandrelBackendName, StringComparison.OrdinalIgnoreCase) AndAlso
            PyModel.SelectedIndex >= 0 AndAlso PyModel.SelectedIndex < SupportedSpandrelModels.Count Then
            LastSelectedSpandrelModelPath = SupportedSpandrelModels(PyModel.SelectedIndex).FilePath
        End If

        If SpandrelScanCancellation IsNot Nothing Then
            Try
                SpandrelScanCancellation.Cancel()
            Catch ex As ObjectDisposedException
                ' A completed scan may dispose its source before a new request cancels it.
            End Try
            SpandrelScanCancellation = Nothing
        End If
        SupportedSpandrelModels.Clear()
        RefreshSpandrelModelsButton.Enabled = False
        SetModelScanStatus("Checking for model folders and Python…")

        Dim ModelFolders As New List(Of String)
        For Each ModelFolder As String In FindGenericSpandrelModelFolders(SearchRoot)
            If Not ModelFolders.Contains(ModelFolder, StringComparer.OrdinalIgnoreCase) Then ModelFolders.Add(ModelFolder)
        Next
        If Not String.Equals(SearchRoot, Application.StartupPath, StringComparison.OrdinalIgnoreCase) Then
            For Each ModelFolder As String In FindGenericSpandrelModelFolders(Application.StartupPath)
                If Not ModelFolders.Contains(ModelFolder, StringComparer.OrdinalIgnoreCase) Then ModelFolders.Add(ModelFolder)
            Next
        End If

        Dim PythonExecutable As String = FindPythonExecutable()
        Dim RunnerPath As String = Path.Combine(Application.StartupPath, SpandrelRunnerName)
        Dim SetupProblem As String = ""
        If ModelFolders.Count = 0 Then
            SetupProblem = "No models folder found. Add checkpoints to the shared 'models' folder."
        ElseIf PythonExecutable = "" Then
            SetupProblem = "Python was not found. See PLKSR_SETUP.md for setup instructions."
        ElseIf Not File.Exists(RunnerPath) Then
            SetupProblem = "The Spandrel runner was not found beside AutoCrispy."
        End If
        If SetupProblem <> "" Then
            SetModelScanStatus(SetupProblem)
            If ScanGeneration = SpandrelScanGeneration AndAlso Not IsDisposed Then
                RestoreWatcherButtonAfterScan()
                RefreshSpandrelModelsButton.Enabled = True
                If String.Equals(If(ExeComboBox.SelectedItem, "").ToString(), SpandrelBackendName, StringComparison.OrdinalIgnoreCase) Then
                    ConfigurePythonModelSelector(SpandrelBackendName)
                End If
            End If
            Return
        End If

        If String.Equals(If(ExeComboBox.SelectedItem, "").ToString(), SpandrelBackendName, StringComparison.OrdinalIgnoreCase) AndAlso
            Not WorkHorse.IsBusy AndAlso Not StopRequested Then
            WatchDogButton.Enabled = False
        End If
        RefreshSpandrelModelsButton.Enabled = False
        SetModelScanStatus("Scanning checkpoints for compatible 1× restoration and 4× SR models…")
        Dim DebugEnabled As Boolean = DebugCheckbox.Checked
        Dim ScanCancellation As New CancellationTokenSource()
        SpandrelScanCancellation = ScanCancellation
        Try
            ' Avoid launching model scans while the backend path is still being edited.
            Await Task.Delay(250, ScanCancellation.Token)
            If ScanGeneration <> SpandrelScanGeneration Then Return

            Dim FoundModels As List(Of SpandrelModelInfo) = Await Task.Run(
                Function() ScanSpandrelModelFolders(ModelFolders, PythonExecutable, RunnerPath, DebugEnabled, ScanCancellation.Token),
                ScanCancellation.Token)
            If ScanGeneration <> SpandrelScanGeneration OrElse ScanCancellation.IsCancellationRequested Then Return

            SupportedSpandrelModels = FoundModels
            If SupportedSpandrelModels.Count > 0 Then
                SetModelScanStatus("Found " & SupportedSpandrelModels.Count.ToString() & " compatible model(s).")
                AddSpandrelBackends()
                Dim CurrentBackend As String = If(ExeComboBox.SelectedItem, "").ToString()
                If String.Equals(CurrentBackend, SelectedBackendBeforeScan, StringComparison.OrdinalIgnoreCase) Then
                    Dim PreferredBackendIndex As Integer = GetPreferredSpandrelBackendIndex()
                    If PreferredBackendIndex >= 0 Then ExeComboBox.SelectedIndex = PreferredBackendIndex
                End If
                If String.Equals(If(ExeComboBox.SelectedItem, "").ToString(), SpandrelBackendName, StringComparison.OrdinalIgnoreCase) Then
                    SetSettingsWindow()
                End If
                If GetPreferredSpandrelBackendIndex() >= 0 Then ReplaceLegacyUpscalerChain()
            Else
                SetModelScanStatus("No compatible checkpoints found (need 1× RGB restoration or 4× RGB SR).")
                If String.Equals(If(ExeComboBox.SelectedItem, "").ToString(), SpandrelBackendName, StringComparison.OrdinalIgnoreCase) Then
                    ConfigurePythonModelSelector(SpandrelBackendName)
                End If
            End If
        Catch ex As OperationCanceledException
            ' A newer folder scan superseded this request.
        Catch ex As Exception
            If ScanGeneration = SpandrelScanGeneration Then
                SetModelScanStatus("Model scan failed: " & ex.GetBaseException().Message)
            End If
            If DebugEnabled Then System.Diagnostics.Debug.WriteLine("Spandrel model scan failed: " & ex.Message)
        Finally
            If ScanGeneration = SpandrelScanGeneration Then
                SpandrelScanCancellation = Nothing
                RestoreWatcherButtonAfterScan()
                If Not IsDisposed Then RefreshSpandrelModelsButton.Enabled = True
            End If
            ScanCancellation.Dispose()
        End Try
    End Function

    Private Sub SetModelScanStatus(Message As String)
        If IsDisposed OrElse SpandrelScanStatusLabel Is Nothing Then Return
        SpandrelScanStatusLabel.Text = Message
        UiToolTip.SetToolTip(SpandrelScanStatusLabel, Message)
        UiToolTip.SetToolTip(RefreshSpandrelModelsButton, Message)
    End Sub

    Private Sub RestoreWatcherButtonAfterScan()
        If IsDisposed OrElse WorkHorse.IsBusy OrElse StopRequested Then Return
        WatchDogButton.Enabled = True
    End Sub

    Private Function ScanSpandrelModelFolders(ModelFolders As List(Of String), PythonExecutable As String, RunnerPath As String, DebugEnabled As Boolean, ScanToken As CancellationToken) As List(Of SpandrelModelInfo)
        Dim Result As New List(Of SpandrelModelInfo)
        Dim SeenModels As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each ModelFolder As String In ModelFolders
            ScanToken.ThrowIfCancellationRequested()
            Dim Models As List(Of SpandrelModelInfo) = ScanSpandrelModelFolder(ModelFolder, PythonExecutable, RunnerPath, DebugEnabled, ScanToken)
            For Each Model As SpandrelModelInfo In Models
                If SeenModels.Add(Model.FilePath) Then Result.Add(Model)
            Next
        Next
        Return Result
    End Function

    Private Function ScanSpandrelModelFolder(ModelFolder As String, PythonExecutable As String, RunnerPath As String, DebugEnabled As Boolean, ScanToken As CancellationToken) As List(Of SpandrelModelInfo)
        Dim Result As New List(Of SpandrelModelInfo)
        Try
            Dim ScanInfo As New ProcessStartInfo(PythonExecutable, MakeSpandrelListCommand(RunnerPath, ModelFolder, DebugEnabled))
            ScanInfo.WorkingDirectory = Application.StartupPath
            ScanInfo.RedirectStandardOutput = True
            ScanInfo.RedirectStandardError = True
            ScanInfo.UseShellExecute = False
            ScanInfo.CreateNoWindow = True
            Using ScanProcess As Process = Process.Start(ScanInfo)
                If ScanProcess Is Nothing Then Return Result
                Using CancellationRegistration As CancellationTokenRegistration = ScanToken.Register(
                    Sub()
                        Try
                            If Not ScanProcess.HasExited Then ScanProcess.Kill()
                        Catch ex As Exception
                        End Try
                    End Sub)
                    Dim StandardOutputTask = ScanProcess.StandardOutput.ReadToEndAsync()
                    Dim StandardErrorTask = ScanProcess.StandardError.ReadToEndAsync()
                    ScanProcess.WaitForExit()
                    Dim StandardOutput As String = StandardOutputTask.Result
                    Dim StandardError As String = StandardErrorTask.Result
                    ScanToken.ThrowIfCancellationRequested()
                    If ScanProcess.ExitCode <> 0 Then
                        If DebugEnabled AndAlso StandardError.Trim() <> "" Then
                            System.Diagnostics.Debug.WriteLine("Spandrel model scan failed: " & StandardError.Trim())
                        End If
                        Return Result
                    End If

                    Dim SeenModels As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
                    For Each OutputLine As String In StandardOutput.Split(New String() {vbCrLf, vbLf}, StringSplitOptions.RemoveEmptyEntries)
                        If OutputLine.StartsWith("MODEL:", StringComparison.Ordinal) Then
                            Try
                                Dim ModelFields As String() = OutputLine.Substring("MODEL:".Length).Split(ControlChars.Tab)
                                Dim ModelPath As String = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(ModelFields(0)))
                                If File.Exists(ModelPath) AndAlso SeenModels.Add(ModelPath) Then
                                    Dim Model As New SpandrelModelInfo With {
                                        .FilePath = ModelPath,
                                        .Architecture = "Unknown",
                                        .Purpose = "Image model"
                                    }
                                    If ModelFields.Length >= 6 Then
                                        Model.Architecture = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(ModelFields(1)))
                                        Dim ModelScale As Integer = 0
                                        Integer.TryParse(ModelFields(2), ModelScale)
                                        Model.Scale = ModelScale
                                        Model.Purpose = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(ModelFields(3)))
                                        Dim InputChannels As Integer = 0
                                        Dim OutputChannels As Integer = 0
                                        Integer.TryParse(ModelFields(4), InputChannels)
                                        Integer.TryParse(ModelFields(5), OutputChannels)
                                        Model.InputChannels = InputChannels
                                        Model.OutputChannels = OutputChannels
                                    End If
                                    Result.Add(Model)
                                End If
                            Catch ex As Exception
                                If DebugEnabled Then System.Diagnostics.Debug.WriteLine("Invalid Spandrel model scan result: " & ex.Message)
                            End Try
                        End If
                    Next
                End Using
            End Using
        Catch ex As OperationCanceledException
            Throw
        Catch ex As Exception
            If DebugEnabled Then System.Diagnostics.Debug.WriteLine("Spandrel model scan failed: " & ex.Message)
        End Try
        Return Result
    End Function

    Private Function FindSpandrelModel(SearchRoot As String, CheckpointName As String) As String
        If Not Directory.Exists(SearchRoot) Then Return ""

        Dim SearchFolders As New List(Of String) From {SearchRoot}
        For Each FirstLevel As String In Directory.GetDirectories(SearchRoot)
            SearchFolders.Add(FirstLevel)
            For Each SecondLevel As String In Directory.GetDirectories(FirstLevel)
                SearchFolders.Add(SecondLevel)
            Next
        Next

        For Each SearchFolder As String In SearchFolders
            For Each Candidate As String In Directory.GetFiles(SearchFolder)
                If Path.GetFileName(Candidate).Equals(CheckpointName, StringComparison.OrdinalIgnoreCase) Then
                    Return Candidate
                End If
            Next
        Next

        Return ""
    End Function

    Private Function GetPreferredSpandrelBackendIndex() As Integer
        Return ExeComboBox.Items.IndexOf(SpandrelBackendName)
    End Function

    Private Function GetPreferredSpandrelBackendName() As String
        If ExeComboBox.Items.Contains(SpandrelBackendName) Then Return SpandrelBackendName
        Return ""
    End Function

    Private Function GetPreferredSpandrelModelIndex() As Integer
        If LastSelectedSpandrelModelPath <> "" Then
            For i As Integer = 0 To SupportedSpandrelModels.Count - 1
                If String.Equals(SupportedSpandrelModels(i).FilePath, LastSelectedSpandrelModelPath, StringComparison.OrdinalIgnoreCase) Then Return i
            Next
        End If
        For i As Integer = 0 To SupportedSpandrelModels.Count - 1
            If Path.GetFileName(SupportedSpandrelModels(i).FilePath).Equals(PLKSRCheckpointName, StringComparison.OrdinalIgnoreCase) Then
                Return i
            End If
        Next
        For i As Integer = 0 To SupportedSpandrelModels.Count - 1
            If Path.GetFileName(SupportedSpandrelModels(i).FilePath).Equals(DAT2CheckpointName, StringComparison.OrdinalIgnoreCase) Then
                Return i
            End If
        Next
        If SupportedSpandrelModels.Count > 0 Then Return 0
        Return -1
    End Function

    Private Function GetPreferredPBRifyModelPath() As String
        For Each Model As SpandrelModelInfo In SupportedSpandrelModels
            If Path.GetFileName(Model.FilePath).Equals(PLKSRCheckpointName, StringComparison.OrdinalIgnoreCase) Then Return Model.FilePath
        Next
        For Each Model As SpandrelModelInfo In SupportedSpandrelModels
            If Path.GetFileName(Model.FilePath).Equals(DAT2CheckpointName, StringComparison.OrdinalIgnoreCase) Then Return Model.FilePath
        Next
        Return ""
    End Function

    Private Function GetSpandrelModelPath(BackendName As String) As String
        Select Case BackendName
            Case PLKSRBackendName
                Return PLKSRModelPath
            Case DAT2BackendName
                Return DAT2ModelPath
            Case SpandrelBackendName
                If PyModel.SelectedIndex >= 0 AndAlso PyModel.SelectedIndex < SupportedSpandrelModels.Count Then
                    Return SupportedSpandrelModels(PyModel.SelectedIndex).FilePath
                End If
        End Select
        Return ""
    End Function

    Private Sub ReplaceLegacyUpscalerChain()
        Dim PreferredBackend As String = GetPreferredSpandrelBackendName()
        If PreferredBackend = "" Then Return
        ' Keep the old GameAI migration limited to the known 4x PBRify models;
        ' a generic 1x restoration model must not replace a saved 4x chain step.
        Dim PreferredModel As String = GetPreferredPBRifyModelPath()
        If PreferredModel = "" Then Return
        Dim PreferredName As String = "Spandrel - " & Path.GetFileName(PreferredModel)
        Dim PreferredPackageType As String = SpandrelBackendName

        Dim ChainWasUpdated As Boolean = False
        For i As Integer = 0 To ChainList.Count - 1
            Dim ChainItem As FormSettings.ChainObject = ChainList(i)
            If ChainItem.PackageType = "ESRGAN" AndAlso ChainItem.Package IsNot Nothing Then
                Dim PythonSettings As FormSettings.PythonPackage = CType(ChainItem.Package, FormSettings.PythonPackage)
                If String.Equals(Path.GetFileNameWithoutExtension(PythonSettings.Model), "4x_gameai_2.0", StringComparison.OrdinalIgnoreCase) Then
                    PythonSettings = New FormSettings.PythonPackage(PreferredModel, PythonSettings.TileSize, PythonSettings.CPUOnly, True)
                    ChainItem.Name = PreferredName
                    ChainItem.FileLocation = ""
                    ChainItem.Package = PythonSettings
                    ChainItem.PackageType = PreferredPackageType
                    ChainList(i) = ChainItem
                    ChainWasUpdated = True
                End If
            End If
        Next

        If ChainWasUpdated Then
            ChainControl.ListItems.Clear()
            For Each ChainItem As FormSettings.ChainObject In ChainList
                ChainControl.ListItems.Add(New DragDropList.DragDropItem(ChainControl.ListItems.Count, ChainItem.Name, ChainThumbs.Item(ChainItem.IconIndex)))
            Next
            ChainControl.DrawList(ChainControl.ListItems)
        End If
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
        BrowseFolderInto(InputTextBox, "Choose the folder textures are dumped into.")
    End Sub

    Private Sub OutputBrowse_Click(sender As Object, e As EventArgs) Handles OutputBrowse.Click
        BrowseFolderInto(OutputTextBox, "Choose the folder upscaled textures should be written to.")
    End Sub

    Private Sub ExeBrowse_Click(sender As Object, e As EventArgs) Handles ExeBrowse.Click
        BrowseFolderInto(ExeTextBox, "Choose the folder that contains the upscale backends.")
    End Sub

    Private Async Sub ExeTextBox_TextChanged(sender As Object, e As EventArgs) Handles ExeTextBox.TextChanged
        If Directory.Exists(ExeTextBox.Text) = True Then
            Root = ExeTextBox.Text
        Else
            Root = Application.StartupPath
        End If
        StartUpCheckEXE()
        If ExeComboBox.Items.Count > 0 Then
            Dim PreferredBackendIndex As Integer = GetPreferredSpandrelBackendIndex()
            If PreferredBackendIndex < 0 Then PreferredBackendIndex = 0
            ExeComboBox.SelectedIndex = PreferredBackendIndex
            If GetPreferredSpandrelBackendIndex() >= 0 Then ReplaceLegacyUpscalerChain()
            SetSettingsWindow()
        End If
        Await RefreshSupportedSpandrelModels(Root)
    End Sub

    Private Sub DefringeCheck_CheckedChanged(sender As Object, e As EventArgs) Handles DefringeCheck.CheckedChanged
        DefringeThresh.Enabled = DefringeCheck.Checked
    End Sub

    Private Sub ChainSave_Click(sender As Object, e As EventArgs) Handles ChainSave.Click
        Using SFD As New SaveFileDialog With {.Filter = "XML Files|*.xml|All Files|*.*", .Title = "Save chain"}
            If SFD.ShowDialog <> DialogResult.OK Then Return
            Try
                File.WriteAllText(SFD.FileName, Serialize(ChainList))
            Catch ex As Exception
                MessageBox.Show("Could not save the chain." & Environment.NewLine & ex.Message, "AutoCrispy", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Using
    End Sub

    Private Sub ChainLoad_Click(sender As Object, e As EventArgs) Handles ChainLoad.Click
        Using OFD As New OpenFileDialog With {.Filter = "XML Files|*.xml|All Files|*.*", .Title = "Load chain"}
            If OFD.ShowDialog <> DialogResult.OK Then Return
            Dim PreviousChain As List(Of FormSettings.ChainObject) = ChainList.ToList()
            Dim PreviousItems As New List(Of DragDropList.DragDropItem)(ChainControl.ListItems)
            Dim PreviousSelection As Integer = ChainControl.SelectedIndex
            Try
                Dim LoadedChain As List(Of FormSettings.ChainObject) = Deserialize(Of List(Of FormSettings.ChainObject))(File.ReadAllText(OFD.FileName))
                If LoadedChain Is Nothing Then Throw New InvalidDataException("The file did not contain a chain.")
                Dim NewItems As New List(Of DragDropList.DragDropItem)
                For ItemIndex As Integer = 0 To LoadedChain.Count - 1
                    Dim ChainItem As FormSettings.ChainObject = LoadedChain(ItemIndex)
                    If ChainItem.IconIndex < 0 OrElse ChainItem.IconIndex >= ChainThumbs.Count Then
                        Throw New InvalidDataException("The icon index for """ & ChainItem.Name & """ is out of range.")
                    End If
                    NewItems.Add(New DragDropList.DragDropItem(ItemIndex, ChainItem.Name, ChainThumbs(ChainItem.IconIndex)))
                Next
                ChainList = LoadedChain
                ChainControl.ListItems.Clear()
                ChainControl.ListItems.AddRange(NewItems)
                ChainControl.SelectedIndex = If(NewItems.Count = 0, -1, 0)
                If GetPreferredSpandrelBackendIndex() >= 0 Then ReplaceLegacyUpscalerChain()
                ChainControl.DrawList(ChainControl.ListItems)
                UpdateChainCommandState()
            Catch ex As Exception
                ChainList = PreviousChain
                ChainControl.ListItems.Clear()
                ChainControl.ListItems.AddRange(PreviousItems)
                ChainControl.SelectedIndex = PreviousSelection
                ChainControl.DrawList(ChainControl.ListItems)
                UpdateChainCommandState()
                MessageBox.Show("Could not load that chain. The current chain was kept." & Environment.NewLine & ex.Message, "AutoCrispy", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Using
    End Sub

    Private Sub ChainAdd_Click(sender As Object, e As EventArgs) Handles ChainAdd.Click
        If ExeComboBox.SelectedItem Is Nothing Then
            MessageBox.Show("Choose a backend before adding it to the chain.", "AutoCrispy", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        AddModelToChain(ExeComboBox.SelectedItem.ToString())
    End Sub

    Private Sub RemoveSelectedChainItem(sender As Object, e As EventArgs) Handles ChainRemove.Click, ChainContextDelete.Click
        If ChainControl Is Nothing Then Return
        Dim RemoveIndex As Integer = ChainControl.SelectedIndex
        If RemoveIndex < 0 OrElse RemoveIndex >= ChainList.Count OrElse RemoveIndex >= ChainControl.ListItems.Count Then Return
        ChainList.RemoveAt(RemoveIndex)
        ChainControl.ListItems.RemoveAt(RemoveIndex)
        ChainControl.ReorderList()
        If ChainControl.ListItems.Count = 0 Then
            ChainControl.SelectedIndex = -1
        Else
            ChainControl.SelectedIndex = Math.Min(RemoveIndex, ChainControl.ListItems.Count - 1)
        End If
        ChainControl.DrawList(ChainControl.ListItems)
        UpdateChainCommandState()
    End Sub

    Private Sub EditSelectedChainItem(sender As Object, e As EventArgs) Handles ChainContextEdit.Click
        If ChainControl Is Nothing Then Return
        Dim ItemIndex As Integer = ChainControl.SelectedIndex
        If ItemIndex < 0 OrElse ItemIndex >= ChainList.Count OrElse ItemIndex >= ChainControl.ListItems.Count Then Return
        Using ECD As New EditChainDialog(Serialize(ChainList(ItemIndex)))
            If ECD.ShowDialog <> DialogResult.OK Then Return
            Try
                Dim NewChainItem As FormSettings.ChainObject = Deserialize(Of FormSettings.ChainObject)(ECD.ResultText)
                If NewChainItem.IconIndex < 0 OrElse NewChainItem.IconIndex >= ChainThumbs.Count Then
                    MessageBox.Show("Icon index is out of range.", "AutoCrispy", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    Return
                End If
                ChainList(ItemIndex) = NewChainItem
                ChainControl.ListItems(ItemIndex) = New DragDropList.DragDropItem(ItemIndex, NewChainItem.Name, ChainThumbs(NewChainItem.IconIndex))
                ChainControl.DrawList(ChainControl.ListItems)
            Catch ex As Exception
                MessageBox.Show("The new settings could not be parsed." & Environment.NewLine & ex.Message, "AutoCrispy", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End Try
        End Using
    End Sub

    Private Sub ChainContext_Opening(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles ChainContext.Opening
        UpdateChainCommandState()
        If ChainControl Is Nothing OrElse ChainControl.SelectedIndex < 0 Then e.Cancel = True
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
        If WorkHorse.IsBusy OrElse WatchDog.Enabled Then
            MessageBox.Show("Stop the current run before upscaling a single image.", "AutoCrispy", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        Using OFD As New OpenFileDialog With {.Filter = "Image Files|*.png;*.jpg;*.bmp", .Title = "Choose an image"}
            If OFD.ShowDialog <> DialogResult.OK Then Return
            Using SFD As New SaveFileDialog With {.Filter = "PNG Images|*.png", .Title = "Save upscaled image"}
                If SFD.ShowDialog <> DialogResult.OK Then Return
                Dim TempPath As String = Path.Combine(Path.GetTempPath(), "Single_0")
                Try
                    Directory.CreateDirectory(TempPath)
                    File.Copy(OFD.FileName, Path.Combine(TempPath, Path.GetFileName(SFD.FileName)), True)
                    QueueActivityLabel.Text = "Starting one-off image run…"
                    LoadedSettings = New FormSettings.Settings(Me)
                    LoadedSettings.Paths = New FormSettings.ProgramPaths(TempPath, Directory.GetParent(SFD.FileName).FullName, Root)
                    If ChainControl.ListItems.Count = 0 AndAlso Not AddModelToChain(If(ExeComboBox.SelectedItem, "").ToString(), False) Then
                        If Directory.Exists(TempPath) Then Directory.Delete(TempPath, True)
                        QueueActivityLabel.Text = "Choose a backend before upscaling"
                        MessageBox.Show("Choose a backend before upscaling.", "AutoCrispy", MessageBoxButtons.OK, MessageBoxIcon.Information)
                        Return
                    End If
                    WatcherEnabled = False
                    StopRequested = False
                    SwitchGroups(False)
                    ProgressPollTimer.Interval = 1000
                    WorkHorse.RunWorkerAsync()
                Catch ex As Exception
                    SwitchGroups(True)
                    If Not WorkHorse.IsBusy AndAlso Directory.Exists(TempPath) Then Directory.Delete(TempPath, True)
                    MessageBox.Show("Could not start the image run." & Environment.NewLine & ex.Message, "AutoCrispy", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End Using
        End Using
    End Sub

    Private Sub WatchDogButton_Click(sender As Object, e As EventArgs) Handles WatchDogButton.Click
        If WorkHorse.IsBusy Then
            WatchDog.Stop()
            SetWatcherButtonStopping()
            QueueActivityLabel.Text = "Stopping active processing…"
            WorkHorse.CancelAsync()
            StopActiveProcesses()
            Return
        End If

        If WatchDog.Enabled Then
            WatchDog.Enabled = False
            SetWatcherButtonIdle()
            QueueActivityLabel.Text = "Watcher stopped"
            SwitchGroups(True)
            Return
        End If

        If Not Directory.Exists(InputTextBox.Text) OrElse Not Directory.Exists(OutputTextBox.Text) Then
            MessageBox.Show("Choose a valid input folder and output folder first.", "AutoCrispy", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If

        WatchDog.Enabled = True
        SetWatcherButtonRunning()
        QueueActivityLabel.Text = "Watching for new textures…"
        SwitchGroups(False)
    End Sub

    Private Sub ThreadComboBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ThreadComboBox.SelectedIndexChanged
        If ThreadComboBox.SelectedIndex = 1 Then
            NumericThreads.Enabled = True
        Else
            NumericThreads.Enabled = False
        End If
        If SettingsLoaded AndAlso ThreadComboBox.SelectedIndex = 3 Then
            Dim Answer As DialogResult = MessageBox.Show("Max starts up to 512 processes and can exhaust memory. Continue?", "High thread count", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2)
            If Answer <> DialogResult.Yes Then ThreadComboBox.SelectedIndex = 0
        End If
    End Sub

    Private Sub CleanupCheckBox_CheckedChanged(sender As Object, e As EventArgs) Handles CleanupCheckBox.CheckedChanged
        If Not SettingsLoaded OrElse SuppressCleanupPrompt OrElse Not CleanupCheckBox.Checked Then Return
        Dim Answer As DialogResult = MessageBox.Show("Delete each input file after it has been upscaled? This cannot be undone.", "Delete input files", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2)
        If Answer = DialogResult.Yes Then Return
        SuppressCleanupPrompt = True
        CleanupCheckBox.Checked = False
        SuppressCleanupPrompt = False
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
        ' Place backend options beside the auto-scaled program settings panel.
        SettingsLoc = New Point(SettingsGroup.Right + 16, SettingsGroup.Top)
        CaffeGroup.Visible = False
        VulkanGroup.Visible = False
        WaifuCPPGroup.Visible = False
        AnimeCPPGroup.Visible = False
        DDxGroup.Visible = False
        xBRZGroup.Visible = False
        PyGroup.Visible = False
        VulkanNoise.Enabled = True
        Select Case If(ExeComboBox.SelectedItem, "").ToString()
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
                ConfigurePythonModelSelector("ESRGAN")
                PyGroup.Text = "ESRGAN"
                SetWrappedHint(TileSizeHint, "0 = no tiling; the full image must fit in available memory.")
                MoveShowGroup(PyGroup)
            Case PLKSRBackendName
                ConfigurePythonModelSelector(PLKSRBackendName)
                PyGroup.Text = "PLKSR"
                SetWrappedHint(TileSizeHint, "0 = full image first; falls back to smaller tiles on GPU memory errors.")
                MoveShowGroup(PyGroup)
            Case DAT2BackendName
                ConfigurePythonModelSelector(DAT2BackendName)
                PyGroup.Text = "PBRify DAT2"
                SetWrappedHint(TileSizeHint, "0 = full image first; falls back to smaller tiles on GPU memory errors.")
                MoveShowGroup(PyGroup)
            Case SpandrelBackendName
                ConfigurePythonModelSelector(SpandrelBackendName)
                PyGroup.Text = "Spandrel"
                SetWrappedHint(TileSizeHint, "0 = full image first; falls back to smaller tiles on GPU memory errors.")
                MoveShowGroup(PyGroup)
        End Select
        UpdateSpandrelModelInfo()
    End Sub

    Private Sub ConfigurePythonModelSelector(BackendName As String)
        PyModel.BeginUpdate()
        PyModel.Items.Clear()
        If BackendName = PLKSRBackendName OrElse BackendName = DAT2BackendName Then
            Dim ModelPath As String = GetSpandrelModelPath(BackendName)
            If ModelPath <> "" Then
                PyModel.Items.Add(Path.GetFileName(ModelPath))
                PyModel.SelectedIndex = 0
            End If
            PyModel.Enabled = False
        ElseIf BackendName = SpandrelBackendName Then
            For Each Model As SpandrelModelInfo In SupportedSpandrelModels
                PyModel.Items.Add(Model.SelectorText)
            Next
            Dim PreferredModelIndex As Integer = GetPreferredSpandrelModelIndex()
            If PreferredModelIndex >= 0 Then PyModel.SelectedIndex = PreferredModelIndex
            PyModel.Enabled = (PreferredModelIndex >= 0)
        Else
            For Each ModelPath As String In PyModels
                PyModel.Items.Add(Path.GetFileName(ModelPath))
            Next
            If PyModel.Items.Count > 0 Then PyModel.SelectedIndex = 0
            PyModel.Enabled = True
        End If
        PyModel.EndUpdate()
        UpdateSpandrelModelInfo()
    End Sub

    Private Sub UpdateSpandrelModelInfo()
        Dim IsSpandrelSelected As Boolean = String.Equals(If(ExeComboBox.SelectedItem, "").ToString(), SpandrelBackendName, StringComparison.OrdinalIgnoreCase)
        SpandrelModelInfoLabel.Visible = IsSpandrelSelected
        If SpandrelModelPathLabel IsNot Nothing Then SpandrelModelPathLabel.Visible = IsSpandrelSelected
        SpandrelScanStatusLabel.Visible = IsSpandrelSelected
        RefreshSpandrelModelsButton.Visible = IsSpandrelSelected
        If Not IsSpandrelSelected Then
            LayoutSpandrelDetails()
            Return
        End If

        If PyModel.SelectedIndex < 0 OrElse PyModel.SelectedIndex >= SupportedSpandrelModels.Count Then
            SetWrappedHint(SpandrelModelInfoLabel, "No compatible model is selected. Refresh the scan or check your setup.")
            If SpandrelModelPathLabel IsNot Nothing Then
                SpandrelModelPathLabel.Text = ""
                UiToolTip.SetToolTip(SpandrelModelPathLabel, "")
            End If
            UiToolTip.SetToolTip(PyModel, "")
            LayoutSpandrelDetails()
            Return
        End If

        Dim Model As SpandrelModelInfo = SupportedSpandrelModels(PyModel.SelectedIndex)
        Dim ModelSummary As String
        If Model.Scale > 0 Then
            Dim ScaleDescription As String = If(Model.Scale = 1, "preserves image dimensions", "enlarges " & Model.Scale.ToString() & "×")
            ModelSummary = "Architecture: " & Model.Architecture & " · " & Model.Scale.ToString() & "× " & Model.Purpose &
                " · RGB " & Model.InputChannels.ToString() & "→" & Model.OutputChannels.ToString() & " · " & ScaleDescription
        Else
            ModelSummary = "Architecture: " & Model.Architecture & " · Spandrel-compatible image model"
        End If
        SetWrappedHint(SpandrelModelInfoLabel, ModelSummary)
        If SpandrelModelPathLabel IsNot Nothing Then
            SpandrelModelPathLabel.Text = FitPathText(Model.FilePath, SpandrelContentWidth() - 16, SpandrelModelPathLabel.Font)
            UiToolTip.SetToolTip(SpandrelModelPathLabel, Model.FilePath)
        End If
        UiToolTip.SetToolTip(PyModel, ModelSummary & Environment.NewLine & Model.FilePath)
        LayoutSpandrelDetails()
    End Sub

    Private Sub PyModel_SelectedIndexChanged(sender As Object, e As EventArgs) Handles PyModel.SelectedIndexChanged
        If String.Equals(If(ExeComboBox.SelectedItem, "").ToString(), SpandrelBackendName, StringComparison.OrdinalIgnoreCase) AndAlso
            PyModel.SelectedIndex >= 0 AndAlso PyModel.SelectedIndex < SupportedSpandrelModels.Count Then
            LastSelectedSpandrelModelPath = SupportedSpandrelModels(PyModel.SelectedIndex).FilePath
        End If
        UpdateSpandrelModelInfo()
    End Sub

    Private Async Sub RefreshSpandrelModelsButton_Click(sender As Object, e As EventArgs) Handles RefreshSpandrelModelsButton.Click
        Await RefreshSupportedSpandrelModels(Root)
    End Sub

    Public Function GetSelectedUpscaleModel() As String
        Dim BackendName As String = If(ExeComboBox.SelectedItem, "").ToString()
        If BackendName = PLKSRBackendName OrElse BackendName = DAT2BackendName OrElse BackendName = SpandrelBackendName Then
            Return GetSpandrelModelPath(BackendName)
        End If
        If PyModel.SelectedIndex >= 0 AndAlso PyModel.SelectedIndex < PyModels.Count Then
            Return PyModels(PyModel.SelectedIndex)
        End If
        Return ""
    End Function

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
        xBRZGroup.Enabled = Enabled
        PyGroup.Enabled = Enabled
        RunOnceButton.Enabled = Enabled
        UpdateChainCommandState()
    End Sub

#End Region

#Region "Background"

    Private Async Sub WatchDog_Tick(sender As Object, e As EventArgs) Handles WatchDog.Tick
        If WatchScanRunning OrElse WorkHorse.IsBusy OrElse Not WatcherEnabled Then Return
        WatchScanRunning = True
        Dim InputPath As String = InputTextBox.Text
        Dim OutputPath As String = OutputTextBox.Text
        Try
            Dim Snapshot As ProgressSnapshot = Await Task.Run(Function() ReadWatchSnapshot(InputPath, OutputPath))
            If IsDisposed OrElse Not WatcherEnabled OrElse WorkHorse.IsBusy Then Return
            If Snapshot.Failed Then
                QueueActivityLabel.Text = "Could not read the input folder"
                UiToolTip.SetToolTip(QueueActivityLabel, Snapshot.ErrorMessage)
                WaitScale = Math.Min(WaitScale + 1, 100)
                WatchDog.Interval = 1000 + (WaitScale * 590)
                Return
            End If
            If Snapshot.SourceCount = 0 OrElse Snapshot.MissingCount = 0 Then
                QueueActivityLabel.Text = "Watching for new textures…"
                WaitScale = Math.Min(WaitScale + 1, 100)
                WatchDog.Interval = 1000 + (WaitScale * 590)
                Return
            End If
            QueueActivityLabel.Text = "Starting next batch…"
            WaitScale = 0
            WatchDog.Interval = 1000
            LoadedSettings = New FormSettings.Settings(Me)
            If ChainControl.ListItems.Count = 0 AndAlso Not AddModelToChain(If(ExeComboBox.SelectedItem, "").ToString(), False) Then
                WatchDog.Enabled = False
                SetWatcherButtonIdle()
                SwitchGroups(True)
                QueueActivityLabel.Text = "Choose a backend before watching"
                Return
            End If
            ProgressPollTimer.Interval = 1000
            If Not WorkHorse.IsBusy Then WorkHorse.RunWorkerAsync()
        Catch ex As Exception
            If Not IsDisposed Then
                QueueActivityLabel.Text = "Could not read the input folder"
                UiToolTip.SetToolTip(QueueActivityLabel, ex.GetBaseException().Message)
            End If
        Finally
            WatchScanRunning = False
        End Try
    End Sub

    ' Progress is overall completion: inputs with matching output files divided by all inputs.
    ' Match by basename so format conversions (for example PNG input to DDS output) count as done.
    Private Async Sub ProgressPollTimer_Tick(sender As Object, e As EventArgs) Handles ProgressPollTimer.Tick
        If ProgressScanRunning Then
            ProgressPollTimer.Interval = 1000
            Return
        End If
        ProgressScanRunning = True
        Dim InputPath As String = InputTextBox.Text
        Dim OutputPath As String = OutputTextBox.Text
        Try
            Dim Snapshot As ProgressSnapshot = Await Task.Run(Function() ReadProgressSnapshot(InputPath, OutputPath))
            If IsDisposed Then Return
            If Not Snapshot.Failed Then
                Dim Percent As Integer = Snapshot.Percent
                If Percent < UpscaleProgress.Minimum Then Percent = UpscaleProgress.Minimum
                If Percent > UpscaleProgress.Maximum Then Percent = UpscaleProgress.Maximum
                UpscaleProgress.Value = Percent
                QueueSummaryLabel.Text = Snapshot.DoneCount.ToString() & " / " & Snapshot.TotalCount.ToString() & " textures complete (" & Percent.ToString() & "%)"
                UiToolTip.SetToolTip(QueueSummaryLabel, QueueSummaryLabel.Text)

                If Not WorkHorse.IsBusy AndAlso Not WatchScanRunning AndAlso Not QueueActivityLabel.Text.StartsWith("Failed") Then
                    If WatchDog.Enabled Then
                        If Snapshot.TotalCount = 0 OrElse Snapshot.DoneCount >= Snapshot.TotalCount Then
                            QueueActivityLabel.Text = "Watching for new textures…"
                        Else
                            QueueActivityLabel.Text = "Watching · " & (Snapshot.TotalCount - Snapshot.DoneCount).ToString() & " remaining"
                        End If
                    ElseIf Snapshot.TotalCount = 0 Then
                        QueueActivityLabel.Text = "Ready"
                    ElseIf Snapshot.DoneCount >= Snapshot.TotalCount Then
                        QueueActivityLabel.Text = "Complete"
                    Else
                        QueueActivityLabel.Text = "Paused · " & (Snapshot.TotalCount - Snapshot.DoneCount).ToString() & " remaining"
                    End If
                    UiToolTip.SetToolTip(QueueActivityLabel, QueueActivityLabel.Text)
                End If
            End If
        Catch ex As Exception
            ' A removable or network-backed input/output folder can disappear during a scan.
        Finally
            ProgressScanRunning = False
        End Try
        If Not IsDisposed Then ProgressPollTimer.Interval = If(WorkHorse.IsBusy, 1000, 5000)
    End Sub

    Private Function ReadProgressSnapshot(InputPath As String, OutputPath As String) As ProgressSnapshot
        Dim Snapshot As ProgressSnapshot
        Dim DoneCount As Integer = 0
        Dim TotalCount As Integer = 0
        Try
            If Not Directory.Exists(InputPath) OrElse Not Directory.Exists(OutputPath) Then
                Snapshot.Failed = True
                Snapshot.ErrorMessage = "Input or output folder is missing."
                Return Snapshot
            End If
            Snapshot.Percent = GetOverallProgress(InputPath, OutputPath, DoneCount, TotalCount)
            Snapshot.DoneCount = DoneCount
            Snapshot.TotalCount = TotalCount
        Catch ex As Exception
            Snapshot.Failed = True
            Snapshot.ErrorMessage = ex.GetBaseException().Message
        End Try
        Return Snapshot
    End Function

    Private Function ReadWatchSnapshot(InputPath As String, OutputPath As String) As ProgressSnapshot
        Dim Snapshot As ProgressSnapshot
        Try
            If Not Directory.Exists(InputPath) Then
                Snapshot.Failed = True
                Snapshot.ErrorMessage = "Input folder is missing."
                Return Snapshot
            End If
            Snapshot.SourceCount = Directory.GetFiles(InputPath, "*.*", SearchOption.AllDirectories).Length
            Snapshot.MissingCount = GetMissingFiles(InputPath, OutputPath).Length
        Catch ex As Exception
            Snapshot.Failed = True
            Snapshot.ErrorMessage = ex.GetBaseException().Message
        End Try
        Return Snapshot
    End Function

    Private Function GetOverallProgress(InputPath As String, OutputPath As String, ByRef DoneCount As Integer, ByRef TotalCount As Integer) As Integer
        DoneCount = 0
        TotalCount = 0
        If Not Directory.Exists(InputPath) OrElse Not Directory.Exists(OutputPath) Then Return 0

        Dim InputFiles As String() = Directory.GetFiles(InputPath, "*.*", SearchOption.AllDirectories)
        TotalCount = InputFiles.Length
        If TotalCount = 0 Then Return 0

        DoneCount = TotalCount - GetMissingFiles(InputFiles, OutputPath).Length
        If DoneCount <= 0 Then Return 0
        If DoneCount >= TotalCount Then Return 100
        Return CInt(Math.Floor((DoneCount * 100.0) / TotalCount))
    End Function

    Private Sub WorkHorse_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles WorkHorse.DoWork
        WatchDog.Stop()
        MakeUpscale()
        If WorkHorse.CancellationPending = True Then
            e.Cancel = True
        End If
    End Sub

    Private Sub WorkHorse_ProgressChanged(sender As Object, e As System.ComponentModel.ProgressChangedEventArgs) Handles WorkHorse.ProgressChanged
        If TypeOf e.UserState Is String Then
            QueueActivityLabel.Text = CStr(e.UserState)
            UiToolTip.SetToolTip(QueueActivityLabel, CStr(e.UserState))
            Return
        End If

        ' The timer owns the progress bar; this event still triggers the texture-reload hotkey.
        If (HotKeyCheckbox.Checked = True) AndAlso (GetActiveWindow <> Me.Handle) Then
            SendKeys.Send(HotToggle)
            Threading.Thread.Sleep(200)
            SendKeys.Send(HotToggle)
        End If
    End Sub

    Private Sub WorkHorse_RunWorkerCompleted(sender As Object, e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles WorkHorse.RunWorkerCompleted
        If ChainControl.ListItems.Count = 0 Then
            ChainList.Clear()
        End If
        If e.Error IsNot Nothing AndAlso Not e.Cancelled AndAlso Not StopRequested Then
            WatchDog.Stop()
            SetWatcherButtonIdle()
            SwitchGroups(True)
            SkipList.Clear()
            QueueActivityLabel.Text = "Failed — see error details"
            UiToolTip.SetToolTip(QueueActivityLabel, e.Error.GetBaseException().Message)
            Dim FailedRunPath As String = Path.Combine(Path.GetTempPath(), "Single_0")
            If Directory.Exists(FailedRunPath) Then Directory.Delete(FailedRunPath, True)
            MessageBox.Show("Upscaling failed: " & e.Error.GetBaseException().Message, "AutoCrispy error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Exit Sub
        End If
        If e.Cancelled OrElse StopRequested Then
            WatchDog.Stop()
            SetWatcherButtonIdle()
            SwitchGroups(True)
            SkipList.Clear()
            QueueActivityLabel.Text = "Cancelled"
            Dim CancelledRunPath As String = Path.Combine(Path.GetTempPath(), "Single_0")
            If Directory.Exists(CancelledRunPath) Then Directory.Delete(CancelledRunPath, True)
            Exit Sub
        End If
        If WatcherEnabled Then
            QueueActivityLabel.Text = "Watching for new textures…"
            WatchDog.Start()
        Else
            QueueActivityLabel.Text = "One-off run complete"
            SetWatcherButtonIdle()
            Dim SingleRunPath As String = Path.Combine(Path.GetTempPath(), "Single_0")
            If Directory.Exists(SingleRunPath) Then Directory.Delete(SingleRunPath, True)
            SwitchGroups(True)
        End If
    End Sub

#End Region

#Region "Upscale Routine"

    Private Sub MakeUpscale()
        Dim TempPath As String = GetChainPath("Temp", 0)
        Dim ThreadCount As Integer = GetThreads(LoadedSettings.BasicSettings.ThreadIndex, LoadedSettings.BasicSettings.ThreadCount)
        If ThreadCount < 1 Then ThreadCount = 1
        Dim Source As String() = GetMissingFiles(LoadedSettings.Paths.InputPath, LoadedSettings.Paths.OutputPath)
        If WorkHorse.CancellationPending Then
            CleanupUpscaleTemporaryFolders()
            Return
        End If
        ' CopyFiles advances this cursor, so the outer loop must not also add a batch step.
        Dim CurrentIndex As Integer = 0
        While CurrentIndex < Source.Count
            If WorkHorse.CancellationPending Then
                CleanupUpscaleTemporaryFolders()
                Return
            End If
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
            CopyFiles(Source, SkipList, TempPath, CurrentIndex, ThreadCount)
            If WorkHorse.CancellationPending Then
                CleanupUpscaleTemporaryFolders()
                Return
            End If
            Dim BatchFiles As String() = Directory.GetFiles(TempPath)
            Array.Sort(BatchFiles, StringComparer.OrdinalIgnoreCase)
            Dim BatchDescription As String = "no images matched the current alpha settings"
            If BatchFiles.Length > 0 Then
                BatchDescription = Path.GetFileName(BatchFiles(0))
                If BatchFiles.Length > 1 Then BatchDescription &= " (+" & (BatchFiles.Length - 1).ToString() & " more)"
            End If
            WorkHorse.ReportProgress(0, "Prepared " & BatchFiles.Length.ToString() & " texture(s): " & BatchDescription)
            Dim StageIndex As Integer = 0
            For Each Model In ChainList
                StageIndex += 1
                If WorkHorse.CancellationPending Then
                    CleanupUpscaleTemporaryFolders()
                    Return
                End If
                WorkHorse.ReportProgress(0, "Step " & StageIndex.ToString() & "/" & ChainList.Count.ToString() & " · " & Model.Name & " · " & BatchDescription)
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
                StartBuilder(ChainPaths(0), ChainPaths(1), NewImages, Model)
                If WorkHorse.CancellationPending Then
                    CleanupUpscaleTemporaryFolders()
                    Return
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
                If WorkHorse.CancellationPending Then
                    CleanupUpscaleTemporaryFolders()
                    Return
                End If
            Next
            For Each ChainDir As String In DeletedChainPaths
                Directory.Delete(ChainDir, True)
            Next

            ' Keep BackgroundWorker progress within its valid range; the progress poller reads
            ' overall completion from the input/output folders instead of this batch percentage.
            Dim ProgressPercentage As Integer = CInt(Math.Floor((CurrentIndex * 100.0) / Source.Count))
            WorkHorse.ReportProgress(Math.Max(0, Math.Min(100, ProgressPercentage)))
        End While
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
        Do While CurrentIndex < FileList.Count AndAlso CopyCounter < BatchSize AndAlso Not WorkHorse.CancellationPending
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
            CurrentIndex += 1
        Loop
    End Sub

    Private Sub StartBuilder(SourcePath As String, DestPath As String, ImageList As List(Of String), Model As FormSettings.ChainObject)
        If ImageList.Count = 0 OrElse WorkHorse.CancellationPending Then Return

        Dim BuildProcess As ProcessStartInfo
        Dim IsSpandrelBackend As Boolean = IsSpandrelPackageType(Model.PackageType)
        Dim BackendDisplay As String = If(Model.PackageType = DAT2BackendName, "DAT2", If(Model.PackageType = SpandrelBackendName, "Spandrel", "RealPLKSR"))
        If Model.PackageType = "ESRGAN" OrElse IsSpandrelBackend OrElse Model.PackageType.Contains("Vulkan") Then
            If IsSpandrelBackend Then
                Dim PythonExecutable As String = FindPythonExecutable()
                Dim RunnerPath As String = Path.Combine(Application.StartupPath, SpandrelRunnerName)
                If PythonExecutable = "" Then
                    Throw New InvalidOperationException(BackendDisplay & " needs Python 3.10 or newer. Add python.exe to PATH, place it beside AutoCrispy, or set AUTOCRISPY_PYTHON to its full path. See PLKSR_SETUP.md.")
                End If
                If Not File.Exists(RunnerPath) Then
                    Throw New FileNotFoundException("The AutoCrispy Spandrel runner was not found.", RunnerPath)
                End If
                BuildProcess = New ProcessStartInfo(PythonExecutable, MakeCommand(SourcePath, DestPath, Model.PackageType, Model.Package))
                BuildProcess.WorkingDirectory = Application.StartupPath
            Else
                BuildProcess = New ProcessStartInfo(Root & Model.FileLocation, MakeCommand(SourcePath, DestPath, Model.PackageType, Model.Package))
                BuildProcess.WorkingDirectory = Directory.GetParent(Root & Model.FileLocation).FullName
            End If
            BuildProcess.RedirectStandardOutput = True
            BuildProcess.RedirectStandardError = True
            BuildProcess.UseShellExecute = False
            BuildProcess.CreateNoWindow = True
            Dim BatchProcess As Process = Process.Start(BuildProcess)
            If BatchProcess Is Nothing Then Throw New InvalidOperationException("Failed to start " & BackendDisplay & ".")
            RegisterActiveProcess(BatchProcess)
            Try
                If IsSpandrelBackend Then
                    Dim StandardOutputTask = BatchProcess.StandardOutput.ReadToEndAsync()
                    Dim StandardErrorTask = BatchProcess.StandardError.ReadToEndAsync()
                    WaitForActiveProcess(BatchProcess)
                    Dim StandardOutput As String = StandardOutputTask.Result
                    Dim StandardError As String = StandardErrorTask.Result
                    If WorkHorse.CancellationPending Then
                        DeleteCancelledOutputs(DestPath, ImageList)
                        Return
                    End If
                    If LoadedSettings.ExpertSettings.Logging OrElse BatchProcess.ExitCode <> 0 Then
                        WriteProcessLog(BuildProcess, StandardOutput, StandardError, LoadedSettings.Paths.OutputPath, Model.PackageType)
                    End If
                    If BatchProcess.ExitCode <> 0 Then
                        Dim Details As String = If(StandardError.Trim() <> "", StandardError.Trim(), StandardOutput.Trim())
                        If Details.Length > 2000 Then Details = Details.Substring(0, 2000) & "..."
                        Throw New InvalidOperationException(BackendDisplay & " inference failed (exit code " & BatchProcess.ExitCode.ToString() & "). " & Details)
                    End If
                Else
                    WaitForActiveProcess(BatchProcess)
                    If WorkHorse.CancellationPending Then
                        DeleteCancelledOutputs(DestPath, ImageList)
                        Return
                    End If
                    If LoadedSettings.ExpertSettings.Logging = True Then
                        WriteLog(BatchProcess, LoadedSettings.Paths.OutputPath)
                    End If
                End If
            Finally
                UnregisterActiveProcess(BatchProcess)
                BatchProcess.Dispose()
            End Try
        Else
            Dim ProcessBag As New List(Of Process)
            Try
                For j = 0 To ImageList.Count - 1
                    If WorkHorse.CancellationPending Then
                        StopActiveProcesses()
                        Exit For
                    End If
                    Dim NewImage As String = DestPath & "\" & Path.GetFileName(ImageList(j))
                    BuildProcess = New ProcessStartInfo(Root & Model.FileLocation, MakeCommand(ImageList(j), NewImage, Model.PackageType, Model.Package))
                    BuildProcess.WorkingDirectory = Directory.GetParent(Root & Model.FileLocation).FullName
                    BuildProcess.RedirectStandardOutput = True
                    BuildProcess.RedirectStandardError = True
                    BuildProcess.UseShellExecute = False
                    BuildProcess.CreateNoWindow = True
                    Dim BatchProcess As Process = Process.Start(BuildProcess)
                    If BatchProcess Is Nothing Then Throw New InvalidOperationException("Failed to start " & Model.PackageType & ".")
                    ProcessBag.Add(BatchProcess)
                    RegisterActiveProcess(BatchProcess)
                    If LoadedSettings.ExpertSettings.Logging = True Then
                        WriteLog(BatchProcess, LoadedSettings.Paths.OutputPath)
                    End If
                Next

                Do
                    If WorkHorse.CancellationPending Then StopActiveProcesses()
                    Dim AnyProcessRunning As Boolean = False
                    For Each Job As Process In ProcessBag
                        If Not Job.HasExited Then AnyProcessRunning = True
                    Next
                    If Not AnyProcessRunning Then Exit Do
                    Threading.Thread.Sleep(50)
                Loop

                If WorkHorse.CancellationPending Then
                    DeleteCancelledOutputs(DestPath, ImageList)
                    Return
                End If
            Finally
                For Each Job As Process In ProcessBag
                    UnregisterActiveProcess(Job)
                    Job.Dispose()
                Next
            End Try
        End If

        For Each TempImage As String In Directory.GetFiles(SourcePath)
            File.Delete(TempImage)
        Next
    End Sub

    Private Sub RegisterActiveProcess(ActiveProcess As Process)
        SyncLock ActiveProcessLock
            ActiveProcesses.Add(ActiveProcess)
        End SyncLock
        ' Cancellation can arrive between Process.Start and registration. Recheck here
        ' so that a process created in that window is terminated before we wait on it.
        If WorkHorse.CancellationPending Then StopActiveProcesses()
    End Sub

    Private Sub UnregisterActiveProcess(ActiveProcess As Process)
        SyncLock ActiveProcessLock
            ActiveProcesses.Remove(ActiveProcess)
        End SyncLock
    End Sub

    Private Sub StopActiveProcesses()
        Dim ProcessesToStop As New List(Of Process)
        SyncLock ActiveProcessLock
            ProcessesToStop.AddRange(ActiveProcesses)
        End SyncLock
        For Each ActiveProcess As Process In ProcessesToStop
            Try
                If Not ActiveProcess.HasExited Then ActiveProcess.Kill()
            Catch ex As Exception
                ' A process may exit between HasExited and Kill.
            End Try
        Next
    End Sub

    Private Sub WaitForActiveProcess(ActiveProcess As Process)
        While Not ActiveProcess.WaitForExit(200)
            If WorkHorse.CancellationPending Then StopActiveProcesses()
        End While
    End Sub

    Private Sub DeleteCancelledOutputs(DestPath As String, ImageList As List(Of String))
        For Each ImagePath As String In ImageList
            Dim OutputPath As String = Path.Combine(DestPath, Path.GetFileName(ImagePath))
            Try
                If File.Exists(OutputPath) AndAlso
                    Not String.Equals(Path.GetFullPath(OutputPath), Path.GetFullPath(ImagePath), StringComparison.OrdinalIgnoreCase) Then
                    File.Delete(OutputPath)
                End If
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine("Could not remove cancelled output: " & ex.Message)
            End Try
        Next
    End Sub

    Private Sub CleanupUpscaleTemporaryFolders()
        Dim TemporaryFolders As New List(Of String) From {GetChainPath("Temp", 0)}
        For j = 0 To ChainList.Count - 2
            TemporaryFolders.Add(GetChainPath("Chain", j))
        Next
        For Each TemporaryFolder As String In TemporaryFolders
            Try
                If Directory.Exists(TemporaryFolder) Then Directory.Delete(TemporaryFolder, True)
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine("Could not remove cancelled working folder: " & ex.Message)
            End Try
        Next
    End Sub

    Private Function IsSpandrelPackageType(PackageType As String) As Boolean
        Return PackageType = "RealPLKSR" OrElse PackageType = DAT2BackendName OrElse PackageType = SpandrelBackendName
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
            Case "RealPLKSR", DAT2BackendName
                Return MakeSpandrelCommand(Source, Dest, Package, False)
            Case SpandrelBackendName
                Return MakeSpandrelCommand(Source, Dest, Package, True)
        End Select
        Return ""
    End Function

    Private Function AddModelToChain(Mode As String, Optional AddPreview As Boolean = True) As Boolean
        Dim Added As Boolean = False
        Select Case Mode
            Case "Waifu2x Caffe"
                ChainControl.ListItems.Add(New DragDropList.DragDropItem(ChainList.Count, "Caffe", ChainThumbs.Item(0)))
                ChainList.Add(New FormSettings.ChainObject("Caffe", 0, CaffePath, "Waifu2x Caffe", Me))
                Added = True
            Case "Waifu2x Vulkan"
                ChainControl.ListItems.Add(New DragDropList.DragDropItem(ChainList.Count, "Waifu Vulkan", ChainThumbs.Item(1)))
                ChainList.Add(New FormSettings.ChainObject("Waifu Vulkan", 1, WaifuNcnnPath, "Waifu2x Vulkan", Me))
                Added = True
            Case "RealSR Vulkan"
                ChainControl.ListItems.Add(New DragDropList.DragDropItem(ChainList.Count, "RealSR Vulkan", ChainThumbs.Item(2)))
                ChainList.Add(New FormSettings.ChainObject("RealSR Vulkan", 2, RealSRNcnnPath, "RealSR Vulkan", Me))
                Added = True
            Case "RealESRGAN Vulkan"
                ChainControl.ListItems.Add(New DragDropList.DragDropItem(ChainList.Count, "RealESRGAN Vulkan", ChainThumbs.Item(8)))
                ChainList.Add(New FormSettings.ChainObject("RealESRGAN Vulkan", 8, RealESRGNcnnPath, "RealESRGAN Vulkan", Me))
                Added = True
            Case "SRMD Vulkan"
                ChainControl.ListItems.Add(New DragDropList.DragDropItem(ChainList.Count, "SRMD Vulkan", ChainThumbs.Item(3)))
                ChainList.Add(New FormSettings.ChainObject("SRMD Vulkan", 3, SRMDNcnnPath, "SRMD Vulkan", Me))
                Added = True
            Case "Waifu2x CPP"
                ChainControl.ListItems.Add(New DragDropList.DragDropItem(ChainList.Count, "Waifu CPP", ChainThumbs.Item(4)))
                ChainList.Add(New FormSettings.ChainObject("Waifu CPP", 4, WaifuCppPath, "Waifu2x CPP", Me))
                Added = True
            Case "Anime4k CPP"
                ChainControl.ListItems.Add(New DragDropList.DragDropItem(ChainList.Count, "Anime4k", ChainThumbs.Item(5)))
                ChainList.Add(New FormSettings.ChainObject("Anime4k", 5, Anime4kPath, "Anime4k CPP", Me))
                Added = True
            Case "TexConv"
                ChainControl.ListItems.Add(New DragDropList.DragDropItem(ChainList.Count, "TexConv", ChainThumbs.Item(7)))
                ChainList.Add(New FormSettings.ChainObject("TexConv", 7, TexConvPath, "TexConv", Me))
                Added = True
            Case "xBRZ"
                ChainControl.ListItems.Add(New DragDropList.DragDropItem(ChainList.Count, "xBRZ", ChainThumbs.Item(0)))
                ChainList.Add(New FormSettings.ChainObject("xBRZ", 0, xBRZPath, "xBRZ", Me))
                Added = True
            Case "ESRGAN"
                If PyModel.SelectedIndex < 0 OrElse PyModel.SelectedIndex >= PyModels.Count Then
                    If AddPreview Then MessageBox.Show("Select an ESRGAN model before adding it to the chain.", "AutoCrispy", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Else
                    ChainControl.ListItems.Add(New DragDropList.DragDropItem(ChainList.Count, "ESRGAN", ChainThumbs.Item(6)))
                    ChainList.Add(New FormSettings.ChainObject("ESRGAN", 6, PyPath, "ESRGAN", Me))
                    Added = True
                End If
            Case PLKSRBackendName
                ChainControl.ListItems.Add(New DragDropList.DragDropItem(ChainList.Count, "PLKSR 4x", ChainThumbs.Item(6)))
                ChainList.Add(New FormSettings.ChainObject("PLKSR 4x", 6, "", "RealPLKSR", Me))
                Added = True
            Case DAT2BackendName
                ChainControl.ListItems.Add(New DragDropList.DragDropItem(ChainList.Count, "PBRify V4 DAT2 4x", ChainThumbs.Item(6)))
                ChainList.Add(New FormSettings.ChainObject("PBRify V4 DAT2 4x", 6, "", DAT2BackendName, Me))
                Added = True
            Case SpandrelBackendName
                Dim ModelFile As String = GetSelectedUpscaleModel()
                If ModelFile = "" Then
                    If AddPreview Then MessageBox.Show("Select a Spandrel model before adding it to the chain.", "AutoCrispy", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Else
                    Dim ModelDisplayName As String = "Spandrel - " & Path.GetFileName(ModelFile)
                    ChainControl.ListItems.Add(New DragDropList.DragDropItem(ChainList.Count, ModelDisplayName, ChainThumbs.Item(6)))
                    ChainList.Add(New FormSettings.ChainObject(ModelDisplayName, 6, "", SpandrelBackendName, Me))
                    Added = True
                End If
        End Select
        If Added Then ChainControl.SelectedIndex = ChainList.Count - 1
        ChainControl.DrawList(ChainControl.ListItems)
        UpdateChainCommandState()
        Return Added
    End Function

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

    Private Function MakeSpandrelCommand(SourceFolder As String, DestFolder As String, Package As FormSettings.PythonPackage, GenericModel As Boolean) As String
        Dim Result As New ArguementString
        Result.AddArguement(Quote(Path.Combine(Application.StartupPath, SpandrelRunnerName)))
        Result.AddArguement(Quote(Package.Model))
        Result.AddArguement("--input", Quote(SourceFolder))
        Result.AddArguement("--output", Quote(DestFolder))
        Result.AddArguement("--tile-size", Package.TileSize.ToString())
        Result.AddArguement("--cpu", Package.CPUOnly)
        If GenericModel Then Result.AddArguement("--generic-model")
        If LoadedSettings.ExpertSettings.Logging Then Result.AddArguement("--debug")
        Return Result.GetArguements
    End Function

    Private Function MakeSpandrelListCommand(RunnerPath As String, ModelFolder As String, DebugEnabled As Boolean) As String
        Dim Result As New ArguementString
        Result.AddArguement(Quote(RunnerPath))
        Result.AddArguement("--list-models", Quote(ModelFolder))
        If DebugEnabled Then Result.AddArguement("--debug")
        Return Result.GetArguements
    End Function

    Private Function FindPythonExecutable() As String
        Dim OverridePath As String = Environment.GetEnvironmentVariable("AUTOCRISPY_PYTHON")
        If Not String.IsNullOrWhiteSpace(OverridePath) Then
            OverridePath = OverridePath.Trim().Trim(ControlChars.Quote)
            If File.Exists(OverridePath) Then Return Path.GetFullPath(OverridePath)
        End If

        Dim Candidates As New List(Of String) From {
            Path.Combine(Root, "python.exe"),
            Path.Combine(Root, "python", "python.exe"),
            Path.Combine(Application.StartupPath, "python.exe"),
            Path.Combine(Application.StartupPath, "python", "python.exe")
        }
        For Each Candidate As String In Candidates
            If File.Exists(Candidate) Then Return Candidate
        Next

        Dim PathEntries As String() = If(Environment.GetEnvironmentVariable("PATH"), "").Split(Path.PathSeparator)
        For Each PathEntry As String In PathEntries
            If Not String.IsNullOrWhiteSpace(PathEntry) Then
                Dim Candidate As String = Path.Combine(PathEntry.Trim().Trim(ControlChars.Quote), "python.exe")
                If File.Exists(Candidate) Then Return Candidate
            End If
        Next

        Return ""
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
        Dim NewImage As New DirectBitmap(GetUnlockedImage(Source))
        For X = 0 To NewImage.Width - 1
            For Y = 0 To NewImage.Height - 1
                If NewImage.GetPixel(X, Y).A < Threshold Then
                    NewImage.SetPixel(X, Y, Color.Transparent)
                End If
            Next
        Next
        NewImage.Bitmap.Save(Source)
    End Sub

    Private Sub RemovePS2Alpha(Source As String)
        Dim NewImage As New DirectBitmap(GetUnlockedImage(Source))
        Dim AlphaMax As Integer = 0
        For X = 0 To NewImage.Width - 1
            For Y = 0 To NewImage.Height - 1
                Dim TempColor As Color = NewImage.GetPixel(X, Y)
                If TempColor.A > AlphaMax Then
                    AlphaMax = TempColor.A
                End If
                If Not AlphaMax <= 128 Then
                    NewImage.Dispose()
                    Exit Sub
                End If
                If TempColor.A <> 0 Then
                    NewImage.SetPixel(X, Y, Color.FromArgb((TempColor.A * 2) - 1, TempColor.R, TempColor.G, TempColor.B))
                End If
            Next
        Next
        NewImage.Bitmap.Save(Source)
    End Sub

    Private Sub AddPS2Alpha(Source As String)
        Dim NewImage As New DirectBitmap(GetUnlockedImage(Source))
        For X = 0 To NewImage.Width - 1
            For Y = 0 To NewImage.Height - 1
                Dim TempColor As Color = NewImage.GetPixel(X, Y)
                If TempColor.A <> 0 Then
                    NewImage.SetPixel(X, Y, Color.FromArgb((TempColor.A + 1) / 2, TempColor.R, TempColor.G, TempColor.B))
                End If
            Next
        Next
        NewImage.Bitmap.Save(Source)
    End Sub

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

    Private Function GetMissingFiles(InputFiles As String(), Path2 As String) As String()
        Dim DoneNames As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        If Directory.Exists(Path2) Then
            For Each DoneFile As String In Directory.GetFiles(Path2, "*.*", SearchOption.AllDirectories)
                DoneNames.Add(Path.GetFileNameWithoutExtension(DoneFile))
            Next
        End If

        Dim Result As New List(Of String)
        For Each InputFile As String In InputFiles
            If Not DoneNames.Contains(Path.GetFileNameWithoutExtension(InputFile)) Then Result.Add(InputFile)
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
        End Select
        Return 512
    End Function

    Private Function GetUnlockedImage(Source As String) As Bitmap
        Dim SourceImage As Bitmap = Image.FromFile(Source)
        Dim UnlockedImage As New Bitmap(SourceImage)
        SourceImage.Dispose()
        Return UnlockedImage
    End Function

    Private Sub BrowseFolderInto(Target As TextBox, Description As String)
        Using FBD As New FolderBrowserDialog
            FBD.Description = Description
            If Directory.Exists(Target.Text) Then FBD.SelectedPath = Target.Text
            If FBD.ShowDialog() <> DialogResult.OK OrElse FBD.SelectedPath = "" Then Return
            Target.Text = FBD.SelectedPath
        End Using
    End Sub

    Private Structure ProgressSnapshot
        Public Percent As Integer
        Public DoneCount As Integer
        Public TotalCount As Integer
        Public SourceCount As Integer
        Public MissingCount As Integer
        Public Failed As Boolean
        Public ErrorMessage As String
    End Structure

    Private Sub HideBackendGroups()
        CaffeGroup.Visible = False
        VulkanGroup.Visible = False
        WaifuCPPGroup.Visible = False
        AnimeCPPGroup.Visible = False
        DDxGroup.Visible = False
        xBRZGroup.Visible = False
        PyGroup.Visible = False
    End Sub

    Private Sub ApplyStaticUiFixes()
        UiToolTip.AutoPopDelay = 16000
        UiToolTip.InitialDelay = 400
        UiToolTip.ReshowDelay = 200
        UiToolTip.ShowAlways = True

        InputBrowse.AccessibleName = "Browse for input folder"
        OutputBrowse.AccessibleName = "Browse for output folder"
        ExeBrowse.AccessibleName = "Browse for backend folder"
        InputBrowse.AccessibleDescription = "Choose the folder textures are dumped into."
        OutputBrowse.AccessibleDescription = "Choose the folder upscaled textures should be written to."
        ExeBrowse.AccessibleDescription = "Choose the folder that contains the upscale backends."
        WatchDogButton.AccessibleDescription = "Starts or stops watching the input folder for new textures."
        RunOnceButton.AccessibleDescription = "Upscale one image with the current settings."
        UpscaleProgress.AccessibleName = "Upscale progress"
        QueueSummaryLabel.AccessibleName = "Queue summary"
        ChainRemove.AccessibleDescription = "Remove the selected chain step."
        ChainRemove.Enabled = False
        SpandrelModelInfoLabel.UseMnemonic = False
        SpandrelModelInfoLabel.AutoEllipsis = True
        SpandrelModelInfoLabel.AutoSize = False
        SpandrelModelInfoLabel.Location = New Point(9, 64)
        SpandrelModelInfoLabel.Size = New Size(570, 20)
        TileSizeHint.UseMnemonic = False
        SpandrelScanStatusLabel.UseMnemonic = False
        SpandrelModelPathLabel = New Label With {
            .Name = "SpandrelModelPathLabel",
            .AutoSize = False,
            .AutoEllipsis = False,
            .UseMnemonic = False,
            .ForeColor = SystemColors.ControlText,
            .BackColor = Color.Transparent,
            .TextAlign = ContentAlignment.TopLeft,
            .Location = New Point(9, 86),
            .Size = New Size(570, 20),
            .Visible = False,
            .AccessibleName = "Model file path"
        }
        PyGroup.Controls.Add(SpandrelModelPathLabel)
        SpandrelModelPathLabel.BringToFront()

        AlphaComboBox.Location = New Point(264, 86)
        AlphaComboBox.Size = New Size(150, AlphaComboBox.Height)
        Dim AlphaLabel As New Label With {
            .AutoSize = True,
            .Name = "AlphaModeLabel",
            .Text = "Alpha:",
            .Location = New Point(208, 90)
        }
        AdvSettingsGroup.Controls.Add(AlphaLabel)
        Dim AlphaHelp As String = "Off processes every image. Skip Alpha ignores images with transparency. Alpha Only processes images that have transparency."
        UiToolTip.SetToolTip(AlphaComboBox, AlphaHelp)
        UiToolTip.SetToolTip(AlphaLabel, AlphaHelp)
        UiToolTip.SetToolTip(HotKeyCheckbox, "After each batch, sends Alt+` to the active window. In Dolphin this toggles custom textures.")
        UiToolTip.SetToolTip(CleanupCheckBox, "Deletes each input file after a successful upscale. This cannot be undone.")
        UiToolTip.SetToolTip(DefringeCheck, "Removes halo artifacts from textures with transparency.")
        UiToolTip.SetToolTip(DefringeThresh, "Alpha values below this threshold become fully transparent.")
        UiToolTip.SetToolTip(VulkanNoise, "Denoise strength. -1 turns denoising off.")
        UiToolTip.SetToolTip(SeamsBox, "Pads seamless textures before upscaling so the edges stay continuous.")
        UiToolTip.SetToolTip(SeamScale, "Final upscale factor for the whole chain, as a product of each step.")
        UiToolTip.SetToolTip(SeamMargin, "Padding kept around the texture. Do not set this wider than the source image.")
        UiToolTip.SetToolTip(ExpertSettingsBox, "Extra arguments passed through to the selected backend.")
        UiToolTip.SetToolTip(PortableCheckBox, "Save settings beside AutoCrispy instead of in AppData.")
        UiToolTip.SetToolTip(ThreadComboBox, "Max starts up to 512 processes and can exhaust memory.")
        UiToolTip.SetToolTip(PyTileSize, "Maximum inference tile size. 0 tries the whole image first.")
        UiToolTip.SetToolTip(PyCPU, "Force CPU inference. This is much slower than a GPU.")
        SetWrappedHint(TileSizeHint, TileSizeHint.Text)
    End Sub

    Private Sub PrepareChainViewport()
        ChainScrollPanel = New Panel With {
            .Name = "ChainScrollPanel",
            .Location = ChainPreview.Location,
            .Size = ChainPreview.Size,
            .AutoScroll = True,
            .BorderStyle = BorderStyle.FixedSingle,
            .BackColor = SystemColors.Window,
            .TabStop = False,
            .AccessibleName = "Model chain"
        }
        ChainGroup.Controls.Add(ChainScrollPanel)
        ChainScrollPanel.Controls.Add(ChainPreview)
        ChainPreview.Location = Point.Empty
        ChainPreview.Size = ChainScrollPanel.ClientSize
        ChainPreview.Anchor = AnchorStyles.Top Or AnchorStyles.Left
        ChainPreview.BackColor = SystemColors.Window
        ChainPreview.TabStop = True
        ChainPreview.AccessibleName = "Model chain"
        ChainPreview.AccessibleDescription = "Drag to reorder. Arrow keys move the selection. Delete removes the selected step."
    End Sub

    Private Sub SetWrappedHint(Target As Label, Hint As String)
        If Target Is Nothing Then Return
        Target.Text = Hint
        UiToolTip.SetToolTip(Target, Hint)
    End Sub

    Private Function SpandrelContentWidth() As Integer
        Dim LeftX As Integer = If(Label26 Is Nothing, 9, Label26.Left)
        Return Math.Max(160, PyGroup.ClientSize.Width - LeftX - 12)
    End Function

    Private Function DesignScaleY() As Single
        If Label26 Is Nothing OrElse Label26.Top <= 0 Then Return 1.0F
        Return Label26.Top / 35.0F
    End Function

    Private Function ScaledDesignY(DesignY As Integer) As Integer
        Return CInt(Math.Round(DesignY * DesignScaleY()))
    End Function

    Private Sub RestoreEsrganDetailRow()
        Dim LeftX As Integer = If(Label26 Is Nothing, ScaledDesignY(9), Label26.Left)
        Label25.Location = New Point(LeftX, ScaledDesignY(143))
        PyTileSize.Location = New Point(PyTileSize.Left, ScaledDesignY(139))
        PyCPU.Location = New Point(PyCPU.Left, ScaledDesignY(142))
        TileSizeHint.SetBounds(LeftX, ScaledDesignY(177), SpandrelContentWidth(), Math.Max(28, ScaledDesignY(36)))
    End Sub

    Private Sub LayoutSpandrelDetails()
        Dim LeftX As Integer = If(Label26 Is Nothing, 9, Label26.Left)
        Dim ContentWidth As Integer = SpandrelContentWidth()
        Dim LineHeight As Integer = Math.Max(18, SpandrelModelInfoLabel.Font.Height + 6)
        Dim ShowPath As Boolean = SpandrelModelPathLabel IsNot Nothing AndAlso SpandrelModelPathLabel.Visible AndAlso SpandrelModelPathLabel.Text <> ""
        Dim ShowStatus As Boolean = SpandrelScanStatusLabel.Visible

        If Not ShowPath AndAlso Not ShowStatus Then
            SpandrelModelInfoLabel.SetBounds(LeftX, PyModel.Bottom + 6, ContentWidth, LineHeight)
            RestoreEsrganDetailRow()
            Return
        End If

        Dim NextY As Integer = PyModel.Bottom + Math.Max(4, LineHeight \ 5)
        SpandrelModelInfoLabel.SetBounds(LeftX, NextY, ContentWidth, LineHeight)
        NextY += LineHeight + 2
        If ShowPath Then
            Dim PathLines As Integer = If(SpandrelModelPathLabel.Text.Contains(Environment.NewLine), 2, 1)
            Dim PathHeight As Integer = Math.Max(LineHeight * PathLines, MeasureTextSize(SpandrelModelPathLabel.Text, SpandrelModelPathLabel.Font, ContentWidth).Height + 4)
            SpandrelModelPathLabel.SetBounds(LeftX, NextY, ContentWidth, PathHeight)
            SpandrelModelPathLabel.BringToFront()
            NextY += PathHeight + 2
        End If
        If ShowStatus Then
            SpandrelScanStatusLabel.SetBounds(LeftX, NextY, ContentWidth, LineHeight)
            NextY += LineHeight + 8
        Else
            NextY += 6
        End If
        Label25.Location = New Point(LeftX, NextY + 4)
        PyTileSize.Location = New Point(PyTileSize.Left, NextY)
        PyCPU.Location = New Point(PyCPU.Left, NextY + 3)
        TileSizeHint.SetBounds(LeftX, NextY + PyTileSize.Height + 8, ContentWidth, LineHeight * 2)
        Dim BottomLimit As Integer = PyGroup.ClientSize.Height - 8
        If TileSizeHint.Bottom > BottomLimit AndAlso TileSizeHint.Top < BottomLimit Then
            TileSizeHint.Height = Math.Max(LineHeight, BottomLimit - TileSizeHint.Top)
        End If
    End Sub

    Private Function FitPathText(FullPath As String, MaxWidth As Integer, PathFont As Font) As String
        If String.IsNullOrEmpty(FullPath) Then Return ""
        If MaxWidth < 40 Then MaxWidth = 560
        If MeasureTextWidth(FullPath, PathFont) <= MaxWidth Then Return FullPath

        Dim BestSplit As Integer = -1
        For Index As Integer = 1 To FullPath.Length - 1
            If FullPath(Index) <> "\"c AndAlso FullPath(Index) <> "/"c Then Continue For
            Dim Head As String = FullPath.Substring(0, Index + 1)
            If MeasureTextWidth(Head, PathFont) <= MaxWidth Then BestSplit = Index + 1
        Next
        If BestSplit > 0 Then
            Dim Tail As String = FullPath.Substring(BestSplit)
            If MeasureTextWidth(Tail, PathFont) > MaxWidth Then Tail = MiddleEllipsis(Tail, PathFont, MaxWidth)
            Return FullPath.Substring(0, BestSplit) & Environment.NewLine & Tail
        End If
        Return MiddleEllipsis(FullPath, PathFont, MaxWidth)
    End Function

    Private Function MeasureTextWidth(Value As String, TextFont As Font) As Integer
        Return MeasureTextSize(Value, TextFont, Integer.MaxValue).Width
    End Function

    Private Function MeasureTextSize(Value As String, TextFont As Font, MaxWidth As Integer) As Size
        Return TextRenderer.MeasureText(Value, TextFont, New Size(Math.Max(1, MaxWidth), Integer.MaxValue), TextFormatFlags.NoPadding Or TextFormatFlags.NoPrefix Or TextFormatFlags.TextBoxControl)
    End Function

    Private Function MiddleEllipsis(Value As String, TextFont As Font, MaxWidth As Integer) As String
        Const Ellipsis As String = "..."
        If MeasureTextWidth(Value, TextFont) <= MaxWidth Then Return Value
        Dim HeadLength As Integer = Math.Max(1, Value.Length \ 2)
        Dim TailLength As Integer = Math.Max(1, Value.Length - HeadLength)
        Do While HeadLength + TailLength > 2 AndAlso MeasureTextWidth(Value.Substring(0, HeadLength) & Ellipsis & Value.Substring(Value.Length - TailLength), TextFont) > MaxWidth
            If HeadLength >= TailLength AndAlso HeadLength > 1 Then
                HeadLength -= 1
            ElseIf TailLength > 1 Then
                TailLength -= 1
            Else
                Exit Do
            End If
        Loop
        Return Value.Substring(0, HeadLength) & Ellipsis & Value.Substring(Value.Length - TailLength)
    End Function

    Private Sub SetWatcherButtonIdle()
        WatcherEnabled = False
        StopRequested = False
        WatchDog.Stop()
        WatchDog.Enabled = False
        WatchDogButton.Text = "Start Watching"
        WatchDogButton.Enabled = True
    End Sub

    Private Sub SetWatcherButtonRunning()
        WatcherEnabled = True
        StopRequested = False
        WatchDogButton.Text = "Stop Watching"
        WatchDogButton.Enabled = True
    End Sub

    Private Sub SetWatcherButtonStopping()
        WatcherEnabled = False
        StopRequested = True
        WatchDog.Stop()
        WatchDog.Enabled = False
        WatchDogButton.Text = "Stopping..."
        WatchDogButton.Enabled = False
    End Sub

    Private Sub SyncChainListFromPreview(ByRef Accepted As Boolean)
        Accepted = False
        If ChainControl Is Nothing OrElse ChainControl.ListItems.Count <> ChainList.Count Then Return
        Dim Reordered As New List(Of FormSettings.ChainObject)
        Dim Seen As New HashSet(Of Integer)
        For Each Item As DragDropList.DragDropItem In ChainControl.ListItems
            If Item.Index < 0 OrElse Item.Index >= ChainList.Count OrElse Not Seen.Add(Item.Index) Then Return
            Reordered.Add(ChainList(Item.Index))
        Next
        ChainList = Reordered
        Accepted = True
    End Sub

    Private Sub UpdateChainCommandState()
        Dim HasSelection As Boolean = ChainControl IsNot Nothing AndAlso ChainControl.SelectedIndex >= 0 AndAlso ChainControl.SelectedIndex < ChainList.Count
        ChainRemove.Enabled = HasSelection
        ChainContextEdit.Enabled = HasSelection
        ChainContextDelete.Enabled = HasSelection
    End Sub

    Private Function Quote(Source As String) As String
        Return ControlChars.Quote & Source & ControlChars.Quote
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

    Private Sub WriteProcessLog(StartInfo As ProcessStartInfo, StandardOutput As String, StandardError As String, SaveLoc As String, BackendName As String)
        Dim Filename As String = Path.Combine(SaveLoc, BackendName & "_" & Now.ToString("yyyy-MM-dd_HH-mm-ss") & ".txt")
        Dim Output As String = StartInfo.FileName & " " & StartInfo.Arguments & vbNewLine & vbNewLine
        Output &= StandardOutput & vbNewLine & vbNewLine & StandardError
        File.WriteAllText(Filename, Output)
    End Sub

#End Region

End Class