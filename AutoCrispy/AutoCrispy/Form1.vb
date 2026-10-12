Imports System.IO
Imports System.Reflection
Imports System.ComponentModel
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Globalization

Public Class Form1

#Region "VARS"

    Dim Root As String = Application.StartupPath
    Dim AppData As String = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
    Dim WaitScale As Integer = 0
    Dim SettingsLoc As Point
    Dim LoadedSettings As FormSettings.Settings
    Private ReadOnly SkipList As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly SkipListLock As New Object()
    Private LastSelectedSpandrelModelPath As String = ""
    Private LastSelectedArchitectModelPath As String = ""
    Private LastSelectedPainterModelPath As String = ""
    Private IsUpdatingAutoRouteModelSelectors As Boolean = False
    Private AutoRoutePreviewHasResult As Boolean = False
    Private ReadOnly UiToolTip As New ToolTip()
    Private ReadOnly GamePathProfiles As New BindingList(Of FormSettings.GamePathProfile)
    Private CurrentRunGamePaths As List(Of FormSettings.GamePathProfile)

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
    Private Const AutoTextureRouterToken As String = "__AUTO_TEXTURE_ROUTER__"
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
        Public Property IsAutoTextureRouter As Boolean
        Public Property ArchitectModelPath As String
        Public Property PainterModelPath As String

        Public ReadOnly Property SelectorText As String
            Get
                If IsAutoTextureRouter Then Return "Auto texture routing · Architect / Painter"
                If Scale <= 0 Then Return Path.GetFileName(FilePath) & " · Spandrel image model"
                Return Scale.ToString() & "× " & Purpose & " · " & Path.GetFileName(FilePath) & " (" & Architecture & ")"
            End Get
        End Property
    End Class

#End Region

#Region "Loading"

    Private Async Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
        Application.CurrentCulture = New Globalization.CultureInfo("EN-US")
        PreloadImageList()
        ChainControl = New DragDropList(ChainPreview, 7)
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
        ChainControl.DrawList(ChainControl.ListItems)
        WatchDogButton.Select()
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

    Public Sub LoadGamePathProfiles(SavedProfiles As List(Of FormSettings.GamePathProfile), ProfilesConfigured As Boolean)
        GamePathProfiles.Clear()
        If SavedProfiles IsNot Nothing Then
            For Each SavedProfile As FormSettings.GamePathProfile In SavedProfiles
                If SavedProfile IsNot Nothing Then
                    GamePathProfiles.Add(CloneGamePathProfile(SavedProfile))
                End If
            Next
        End If

        ' Migrate the legacy single-pair settings once, so existing users retain their setup.
        If GamePathProfiles.Count = 0 AndAlso Not ProfilesConfigured AndAlso
            Not String.IsNullOrWhiteSpace(InputTextBox.Text) AndAlso Not String.IsNullOrWhiteSpace(OutputTextBox.Text) Then
            GamePathProfiles.Add(New FormSettings.GamePathProfile(GetGamePathProfileName(InputTextBox.Text), InputTextBox.Text, OutputTextBox.Text, True))
        End If
        UpdateGamePathsSummary()
    End Sub

    Public Function GetGamePathProfileSnapshot() As List(Of FormSettings.GamePathProfile)
        Dim Snapshot As New List(Of FormSettings.GamePathProfile)
        For Each Profile As FormSettings.GamePathProfile In GamePathProfiles
            Snapshot.Add(CloneGamePathProfile(Profile))
        Next
        Return Snapshot
    End Function

    Private Shared Function CloneGamePathProfile(Profile As FormSettings.GamePathProfile) As FormSettings.GamePathProfile
        Return New FormSettings.GamePathProfile(Profile.Name, Profile.InputPath, Profile.OutputPath, Profile.Enabled)
    End Function

    Private Shared Function GetGamePathProfileName(InputPath As String) As String
        If String.IsNullOrWhiteSpace(InputPath) Then Return "Game"
        Dim TrimmedPath As String = InputPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
        Dim FolderName As String = Path.GetFileName(TrimmedPath)
        If String.IsNullOrWhiteSpace(FolderName) Then Return InputPath
        Return FolderName
    End Function

    Private Function GetEffectiveGamePathProfiles() As List(Of FormSettings.GamePathProfile)
        Dim Profiles As New List(Of FormSettings.GamePathProfile)
        If GamePathProfiles.Count > 0 Then
            For Each Profile As FormSettings.GamePathProfile In GamePathProfiles
                If Profile.Enabled Then Profiles.Add(CloneGamePathProfile(Profile))
            Next
            Return Profiles
        End If

        ' Keep the original single input/output fields as a fallback when no profiles are saved.
        If Not String.IsNullOrWhiteSpace(InputTextBox.Text) OrElse Not String.IsNullOrWhiteSpace(OutputTextBox.Text) Then
            Profiles.Add(New FormSettings.GamePathProfile(GetGamePathProfileName(InputTextBox.Text), InputTextBox.Text, OutputTextBox.Text, True))
        End If
        Return Profiles
    End Function

    Private Sub UpdateGamePathsSummary()
        If GamePathProfiles.Count = 0 Then
            GamePathsSummaryLabel.Text = "No saved game profiles; using the Input/Output folders above."
            Return
        End If
        Dim EnabledCount As Integer = 0
        For Each Profile As FormSettings.GamePathProfile In GamePathProfiles
            If Profile.Enabled Then EnabledCount += 1
        Next
        GamePathsSummaryLabel.Text = EnabledCount.ToString() & " of " & GamePathProfiles.Count.ToString() & " game profiles checked; only checked profiles are watched."
    End Sub

    Private Sub ManageGamePathsButton_Click(sender As Object, e As EventArgs) Handles ManageGamePathsButton.Click
        Using Dialog As New GamePathProfilesDialog(GamePathProfiles, InputTextBox.Text, OutputTextBox.Text)
            Dialog.ShowDialog(Me)
        End Using
        UpdateGamePathsSummary()
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
                WatchDogButton.Enabled = True
                RefreshSpandrelModelsButton.Enabled = True
                If String.Equals(If(ExeComboBox.SelectedItem, "").ToString(), SpandrelBackendName, StringComparison.OrdinalIgnoreCase) Then
                    ConfigurePythonModelSelector(SpandrelBackendName)
                End If
            End If
            Return
        End If

        WatchDogButton.Enabled = False
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

            Dim CompatibleModelCount As Integer = FoundModels.Count
            SupportedSpandrelModels = FoundModels
            Dim AutoRouterChoice As SpandrelModelInfo = CreateAutoTextureRouterChoice(FoundModels)
            If AutoRouterChoice IsNot Nothing Then SupportedSpandrelModels.Insert(0, AutoRouterChoice)
            ConfigureAutoRouteModelSelectors()
            If CompatibleModelCount > 0 Then
                Dim ScanSummary As String = "Found " & CompatibleModelCount.ToString() & " compatible model(s)."
                If AutoRouterChoice IsNot Nothing Then ScanSummary &= " Auto Texture Routing is available for compatible 4× models."
                SetModelScanStatus(ScanSummary)
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
                If Not IsDisposed Then WatchDogButton.Enabled = True
                If Not IsDisposed Then RefreshSpandrelModelsButton.Enabled = True
            End If
            ScanCancellation.Dispose()
        End Try
    End Function

    Private Sub SetModelScanStatus(Message As String)
        If IsDisposed OrElse SpandrelScanStatusLabel Is Nothing Then Return
        SpandrelScanStatusLabel.Text = Message
        BackendStatusLabel.Text = "Spandrel: " & Message
        UiToolTip.SetToolTip(SpandrelScanStatusLabel, Message)
        UiToolTip.SetToolTip(BackendStatusLabel, Message)
        If Not WorkHorse.IsBusy Then
            QueueActivityLabel.Text = Message
            UiToolTip.SetToolTip(QueueActivityLabel, Message)
        End If
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

    Private Function CreateAutoTextureRouterChoice(Models As List(Of SpandrelModelInfo)) As SpandrelModelInfo
        Dim Candidates As List(Of SpandrelModelInfo) = GetAutoRouteCandidateModels(Models)
        If Candidates.Count < 2 Then Return Nothing

        Dim ArchitectModel As SpandrelModelInfo = FindAutoRouteRoleModel(
            Candidates, LastSelectedArchitectModelPath, "", New String() {"best_realesrnet", "architect"})
        If ArchitectModel Is Nothing Then Return Nothing
        Dim PainterModel As SpandrelModelInfo = FindAutoRouteRoleModel(
            Candidates, LastSelectedPainterModelPath, ArchitectModel.FilePath, New String() {"best_swinir", "painter"})
        If PainterModel Is Nothing Then Return Nothing

        Return New SpandrelModelInfo With {
            .FilePath = AutoTextureRouterToken,
            .Architecture = "Automatic texture router",
            .Scale = 4,
            .Purpose = "SR",
            .InputChannels = 3,
            .OutputChannels = 3,
            .IsAutoTextureRouter = True,
            .ArchitectModelPath = ArchitectModel.FilePath,
            .PainterModelPath = PainterModel.FilePath
        }
    End Function

    Private Function GetAutoRouteCandidateModels(Models As IEnumerable(Of SpandrelModelInfo)) As List(Of SpandrelModelInfo)
        Dim Candidates As New List(Of SpandrelModelInfo)
        If Models Is Nothing Then Return Candidates
        For Each Model As SpandrelModelInfo In Models
            If IsAutoRouteCompatibleModel(Model) Then Candidates.Add(Model)
        Next
        Return Candidates
    End Function

    Private Shared Function IsAutoRouteCompatibleModel(Model As SpandrelModelInfo) As Boolean
        Return Model IsNot Nothing AndAlso Not Model.IsAutoTextureRouter AndAlso
            Model.Scale = 4 AndAlso Model.InputChannels = 3 AndAlso Model.OutputChannels = 3 AndAlso
            String.Equals(Model.Purpose, "SR", StringComparison.OrdinalIgnoreCase) AndAlso
            Not String.IsNullOrWhiteSpace(Model.FilePath)
    End Function

    Private Shared Function FindAutoRouteRoleModel(Models As List(Of SpandrelModelInfo), PreferredPath As String,
                                                   ExcludedPath As String, PreferredStems As String()) As SpandrelModelInfo
        If Not String.IsNullOrWhiteSpace(PreferredPath) Then
            For Each Model As SpandrelModelInfo In Models
                If Not String.Equals(Model.FilePath, ExcludedPath, StringComparison.OrdinalIgnoreCase) AndAlso
                    String.Equals(Model.FilePath, PreferredPath, StringComparison.OrdinalIgnoreCase) Then Return Model
            Next
        End If

        If PreferredStems IsNot Nothing Then
            For Each PreferredStem As String In PreferredStems
                For Each Model As SpandrelModelInfo In Models
                    If String.Equals(Model.FilePath, ExcludedPath, StringComparison.OrdinalIgnoreCase) Then Continue For
                    If String.Equals(Path.GetFileNameWithoutExtension(Model.FilePath), PreferredStem, StringComparison.OrdinalIgnoreCase) Then Return Model
                Next
            Next
        End If

        For Each Model As SpandrelModelInfo In Models
            If Not String.Equals(Model.FilePath, ExcludedPath, StringComparison.OrdinalIgnoreCase) Then Return Model
        Next
        Return Nothing
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
            If Not SupportedSpandrelModels(i).IsAutoTextureRouter AndAlso
                Path.GetFileName(SupportedSpandrelModels(i).FilePath).Equals(DAT2CheckpointName, StringComparison.OrdinalIgnoreCase) Then
                Return i
            End If
        Next
        ' Routing is opt-in: prefer a real single checkpoint unless the user explicitly
        ' selected the saved router entry in a previous session.
        For i As Integer = 0 To SupportedSpandrelModels.Count - 1
            If Not SupportedSpandrelModels(i).IsAutoTextureRouter Then Return i
        Next
        For i As Integer = 0 To SupportedSpandrelModels.Count - 1
            If SupportedSpandrelModels(i).IsAutoTextureRouter Then Return i
        Next
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
        InputTextBox.Text = GetFolder()
    End Sub

    Private Sub OutputBrowse_Click(sender As Object, e As EventArgs) Handles OutputBrowse.Click
        OutputTextBox.Text = GetFolder()
    End Sub

    Private Sub ExeBrowse_Click(sender As Object, e As EventArgs) Handles ExeBrowse.Click
        ExeTextBox.Text = GetFolder()
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
        Using SFD As New SaveFileDialog With {.Filter = "XML Files|*.xml|All Files|*.*"}
            If SFD.ShowDialog = DialogResult.OK Then
                File.WriteAllText(SFD.FileName, Serialize(ChainList))
            End If
        End Using
    End Sub

    Private Sub ChainLoad_Click(sender As Object, e As EventArgs) Handles ChainLoad.Click
        Using OFD As New OpenFileDialog With {.Filter = "XML Files|*.xml|All Files|*.*"}
            If OFD.ShowDialog = DialogResult.OK Then
                ChainControl.ListItems.Clear()
                ChainList.Clear()
                ChainList = Deserialize(Of List(Of FormSettings.ChainObject))(File.ReadAllText(OFD.FileName))
                For Each ChainItem As FormSettings.ChainObject In ChainList
                    ChainControl.ListItems.Add(New DragDropList.DragDropItem(ChainList.IndexOf(ChainItem), ChainItem.Name, ChainThumbs.Item(ChainItem.IconIndex)))
                Next
                If GetPreferredSpandrelBackendIndex() >= 0 Then ReplaceLegacyUpscalerChain()
                ChainControl.DrawList(ChainControl.ListItems)
            End If
        End Using
    End Sub

    Private Sub ChainAdd_Click(sender As Object, e As EventArgs) Handles ChainAdd.Click
        AddModelToChain(ExeComboBox.SelectedItem)
    End Sub

    Private Sub RemoveItemFromChain(sender As Object, e As EventArgs) Handles ChainContextDelete.Click
        Dim Remove As Integer = ChainControl.GetCurrentIndex
        ChainList.RemoveAt(Remove)
        ChainControl.ListItems.RemoveAt(Remove)
        ChainControl.ReorderList()
        ChainControl.DrawList(ChainControl.ListItems)
    End Sub

    Private Sub ChainContextEdit_Click(sender As Object, e As EventArgs) Handles ChainContextEdit.Click
        Dim ItemIndex As Integer = ChainControl.GetCurrentIndex
        Using ECD As New EditChainDialog(Serialize(ChainList(ItemIndex)))
            If ECD.ShowDialog = DialogResult.OK Then
                Try
                    Dim NewChainItem As FormSettings.ChainObject = Deserialize(Of FormSettings.ChainObject)(ECD.ResultText)
                    ChainList(ItemIndex) = NewChainItem
                Catch ex As Exception
                    MsgBox("Error: New settings could not be parsed.")
                End Try
            End If
        End Using
    End Sub

    Private Sub ChainPreview_MouseUp(sender As Object, e As MouseEventArgs) Handles ChainPreview.MouseUp
        If e.Button = MouseButtons.Left Then
            Dim TempList As New List(Of FormSettings.ChainObject)
            For Each Item As DragDropList.DragDropItem In ChainControl.ListItems
                TempList.Add(ChainList(Item.Index))
            Next
            ChainList = TempList
            ChainControl.ReorderList()
        End If
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
        If WorkHorse.IsBusy Then Return

        Dim WatcherWasEnabled As Boolean = WatchDog.Enabled
        If WatcherWasEnabled Then WatchDog.Stop()
        Dim RunStarted As Boolean = False
        Try
            Using OFD As New OpenFileDialog With {.Filter = "Image Files|*.png;*.jpg;*.bmp"}
                If OFD.ShowDialog() <> DialogResult.OK Then Return
                Using SFD As New SaveFileDialog With {.Filter = "PNG Images|*.png"}
                    If SFD.ShowDialog() <> DialogResult.OK Then Return

                    Dim TempPath As String = Path.Combine(Path.GetTempPath(), "Single_0")
                    Directory.CreateDirectory(TempPath)
                    File.Copy(OFD.FileName, Path.Combine(TempPath, Path.GetFileName(SFD.FileName)), True)
                    Dim OutputPath As String = Directory.GetParent(SFD.FileName).FullName
                    QueueActivityLabel.Text = "Starting one-off image run…"
                    LoadedSettings = New FormSettings.Settings(Me)
                    LoadedSettings.Paths = New FormSettings.ProgramPaths(TempPath, OutputPath, Root)
                    CurrentRunGamePaths = New List(Of FormSettings.GamePathProfile) From {
                        New FormSettings.GamePathProfile("One-off", TempPath, OutputPath, True)
                    }
                    If ChainControl.ListItems.Count = 0 Then
                        AddModelToChain(ExeComboBox.SelectedItem, False)
                    End If
                    SwitchGroups(False)
                    ProgressPollTimer.Interval = 1000
                    WorkHorse.RunWorkerAsync()
                    RunStarted = True
                End Using
            End Using
        Finally
            If WatcherWasEnabled AndAlso Not RunStarted AndAlso WatchDogButton.Text = "Running: True" Then
                WatchDog.Start()
            End If
        End Try
    End Sub

    Private Sub WatchDogButton_Click(sender As Object, e As EventArgs) Handles WatchDogButton.Click
        If WorkHorse.IsBusy Then
            WatchDog.Stop()
            WatchDogButton.Enabled = False
            WatchDogButton.Text = "Stopping..."
            QueueActivityLabel.Text = "Stopping active processing…"
            WorkHorse.CancelAsync()
            StopActiveProcesses()
            Return
        End If

        If WatchDog.Enabled Then
            WatchDog.Enabled = False
            WatchDogButton.Text = "Running: False"
            QueueActivityLabel.Text = "Watcher stopped"
            SwitchGroups(True)
            Return
        End If

        Dim ProfilesToWatch As List(Of FormSettings.GamePathProfile) = GetEffectiveGamePathProfiles()
        If ProfilesToWatch.Count = 0 Then
            MsgBox("No folders are selected. Check at least one game profile, or clear the profile table and set the fallback input/output folders.", MsgBoxStyle.Critical, "No paths selected")
            Return
        End If
        For Each Profile As FormSettings.GamePathProfile In ProfilesToWatch
            If Not Directory.Exists(Profile.InputPath) OrElse Not Directory.Exists(Profile.OutputPath) Then
                Dim FolderHint As String = If(GamePathProfiles.Count > 0,
                    "Check the folders in Paths > Manage game paths.",
                    "Check the fallback input/output folders on the Paths tab.")
                MsgBox("The input or output folder is invalid for game profile '" & Profile.Name & "'." & Environment.NewLine & FolderHint,
                       MsgBoxStyle.Critical, "Invalid game path")
                Return
            End If
        Next

        WatchDog.Enabled = True
        WatchDogButton.Text = "Running: True"
        QueueActivityLabel.Text = "Watching " & ProfilesToWatch.Count.ToString() & " game path(s)…"
        SwitchGroups(False)
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
                TileSizeHint.Text = "0 = no tiling; the full image must fit in available memory."
                MoveShowGroup(PyGroup)
            Case PLKSRBackendName
                ConfigurePythonModelSelector(PLKSRBackendName)
                PyGroup.Text = "PLKSR"
                TileSizeHint.Text = "0 = full image first; retries smaller tiles on GPU memory errors."
                MoveShowGroup(PyGroup)
            Case DAT2BackendName
                ConfigurePythonModelSelector(DAT2BackendName)
                PyGroup.Text = "PBRify DAT2"
                TileSizeHint.Text = "0 = full image first; retries smaller tiles on GPU memory errors."
                MoveShowGroup(PyGroup)
            Case SpandrelBackendName
                ConfigurePythonModelSelector(SpandrelBackendName)
                PyGroup.Text = "Spandrel"
                TileSizeHint.Text = "0 = full image first; retries smaller tiles on GPU memory errors."
                MoveShowGroup(PyGroup)
        End Select
        UpdateSpandrelModelInfo()
    End Sub

    Public Sub LoadSpandrelModelPreferences(ModelPath As String, ArchitectPath As String, PainterPath As String)
        LastSelectedSpandrelModelPath = If(ModelPath, String.Empty)
        LastSelectedArchitectModelPath = If(ArchitectPath, String.Empty)
        LastSelectedPainterModelPath = If(PainterPath, String.Empty)
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

    Private Sub ConfigureAutoRouteModelSelectors()
        Dim Candidates As List(Of SpandrelModelInfo) = GetAutoRouteCandidateModels(SupportedSpandrelModels)
        IsUpdatingAutoRouteModelSelectors = True
        AutoArchitectModelComboBox.BeginUpdate()
        Try
            AutoArchitectModelComboBox.Items.Clear()
            For Each Model As SpandrelModelInfo In Candidates
                AutoArchitectModelComboBox.Items.Add(Model)
            Next

            Dim ArchitectModel As SpandrelModelInfo = FindAutoRouteRoleModel(
                Candidates, LastSelectedArchitectModelPath, "", New String() {"best_realesrnet", "architect"})
            If ArchitectModel IsNot Nothing Then
                For i As Integer = 0 To AutoArchitectModelComboBox.Items.Count - 1
                    Dim Item As SpandrelModelInfo = DirectCast(AutoArchitectModelComboBox.Items(i), SpandrelModelInfo)
                    If String.Equals(Item.FilePath, ArchitectModel.FilePath, StringComparison.OrdinalIgnoreCase) Then
                        AutoArchitectModelComboBox.SelectedIndex = i
                        Exit For
                    End If
                Next
            End If
        Finally
            AutoArchitectModelComboBox.EndUpdate()
            IsUpdatingAutoRouteModelSelectors = False
        End Try

        Dim SelectedArchitectPath As String = GetSelectedAutoRouteModelPath(AutoArchitectModelComboBox)
        If SelectedArchitectPath <> "" Then LastSelectedArchitectModelPath = SelectedArchitectPath
        ConfigurePainterRoleSelector(LastSelectedPainterModelPath)
        UpdateAutoTextureRouterDefaultPaths()
        UpdateSpandrelModelInfo()
    End Sub

    Private Sub ConfigurePainterRoleSelector(PreferredPath As String)
        Dim Candidates As List(Of SpandrelModelInfo) = GetAutoRouteCandidateModels(SupportedSpandrelModels)
        Dim ArchitectPath As String = GetSelectedAutoRouteModelPath(AutoArchitectModelComboBox)
        Dim PainterModel As SpandrelModelInfo = FindAutoRouteRoleModel(
            Candidates, PreferredPath, ArchitectPath, New String() {"best_swinir", "painter"})

        IsUpdatingAutoRouteModelSelectors = True
        AutoPainterModelComboBox.BeginUpdate()
        Try
            AutoPainterModelComboBox.Items.Clear()
            For Each Model As SpandrelModelInfo In Candidates
                If Not String.Equals(Model.FilePath, ArchitectPath, StringComparison.OrdinalIgnoreCase) Then
                    AutoPainterModelComboBox.Items.Add(Model)
                End If
            Next

            If PainterModel IsNot Nothing Then
                For i As Integer = 0 To AutoPainterModelComboBox.Items.Count - 1
                    Dim Item As SpandrelModelInfo = DirectCast(AutoPainterModelComboBox.Items(i), SpandrelModelInfo)
                    If String.Equals(Item.FilePath, PainterModel.FilePath, StringComparison.OrdinalIgnoreCase) Then
                        AutoPainterModelComboBox.SelectedIndex = i
                        Exit For
                    End If
                Next
            End If
        Finally
            AutoPainterModelComboBox.EndUpdate()
            IsUpdatingAutoRouteModelSelectors = False
        End Try

        Dim SelectedPainterPath As String = GetSelectedAutoRouteModelPath(AutoPainterModelComboBox)
        If SelectedPainterPath <> "" Then LastSelectedPainterModelPath = SelectedPainterPath
    End Sub

    Private Function GetSelectedAutoRouteModelPath(Selector As ComboBox) As String
        If Selector Is Nothing OrElse Selector.SelectedIndex < 0 Then Return ""
        Dim Model As SpandrelModelInfo = TryCast(Selector.SelectedItem, SpandrelModelInfo)
        If Model Is Nothing Then Return ""
        Return Model.FilePath
    End Function

    Private Sub UpdateAutoTextureRouterDefaultPaths()
        Dim ArchitectPath As String = GetSelectedAutoRouteModelPath(AutoArchitectModelComboBox)
        Dim PainterPath As String = GetSelectedAutoRouteModelPath(AutoPainterModelComboBox)
        For Each Model As SpandrelModelInfo In SupportedSpandrelModels
            If Model.IsAutoTextureRouter Then
                If ArchitectPath <> "" Then Model.ArchitectModelPath = ArchitectPath
                If PainterPath <> "" Then Model.PainterModelPath = PainterPath
                Exit For
            End If
        Next
    End Sub

    Private Sub UpdateSpandrelModelInfo()
        Dim IsSpandrelSelected As Boolean = String.Equals(If(ExeComboBox.SelectedItem, "").ToString(), SpandrelBackendName, StringComparison.OrdinalIgnoreCase)
        SpandrelModelInfoLabel.Visible = IsSpandrelSelected
        SpandrelScanStatusLabel.Visible = IsSpandrelSelected
        RefreshSpandrelModelsButton.Visible = IsSpandrelSelected
        AutoPainterShareLabel.Visible = False
        AutoPainterSharePercent.Visible = False
        AutoPainterShareSuffix.Visible = False
        AutoPainterThresholdLabel.Visible = False
        AutoPainterThreshold.Visible = False
        AutoPainterThresholdSuffix.Visible = False
        AutoRoutePreviewButton.Visible = False
        AutoRoutePreviewStatusLabel.Visible = False
        AutoRoutePreviewSampleLabel.Visible = False
        AutoRoutePreviewSampleCount.Visible = False
        AutoArchitectModelLabel.Visible = False
        AutoArchitectModelComboBox.Visible = False
        AutoPainterModelLabel.Visible = False
        AutoPainterModelComboBox.Visible = False
        If Not IsSpandrelSelected Then Return

        If PyModel.SelectedIndex < 0 OrElse PyModel.SelectedIndex >= SupportedSpandrelModels.Count Then
            SpandrelModelInfoLabel.Text = "No compatible model is selected. Refresh the scan or check your setup."
            UiToolTip.SetToolTip(SpandrelModelInfoLabel, "")
            Return
        End If

        Dim Model As SpandrelModelInfo = SupportedSpandrelModels(PyModel.SelectedIndex)
        AutoPainterShareLabel.Visible = Model.IsAutoTextureRouter
        AutoPainterSharePercent.Visible = Model.IsAutoTextureRouter
        AutoPainterShareSuffix.Visible = Model.IsAutoTextureRouter
        AutoPainterThresholdLabel.Visible = Model.IsAutoTextureRouter
        AutoPainterThreshold.Visible = Model.IsAutoTextureRouter
        AutoPainterThresholdSuffix.Visible = Model.IsAutoTextureRouter
        AutoRoutePreviewButton.Visible = Model.IsAutoTextureRouter
        AutoRoutePreviewStatusLabel.Visible = Model.IsAutoTextureRouter
        AutoRoutePreviewSampleLabel.Visible = Model.IsAutoTextureRouter
        AutoRoutePreviewSampleCount.Visible = Model.IsAutoTextureRouter
        AutoArchitectModelLabel.Visible = Model.IsAutoTextureRouter
        AutoArchitectModelComboBox.Visible = Model.IsAutoTextureRouter
        AutoPainterModelLabel.Visible = Model.IsAutoTextureRouter
        AutoPainterModelComboBox.Visible = Model.IsAutoTextureRouter
        If Model.IsAutoTextureRouter Then
            Dim PainterShareText As String = CInt(AutoPainterSharePercent.Value).ToString()
            Dim PainterThresholdText As String = AutoPainterThreshold.Value.ToString("0.00", CultureInfo.InvariantCulture)
            Dim ArchitectPath As String = GetSelectedAutoRouteModelPath(AutoArchitectModelComboBox)
            Dim PainterPath As String = GetSelectedAutoRouteModelPath(AutoPainterModelComboBox)
            If ArchitectPath = "" Then ArchitectPath = Model.ArchitectModelPath
            If PainterPath = "" Then PainterPath = Model.PainterModelPath
            AutoPainterShareSuffix.Text = "cap for batches of 10+"
            AutoPainterThresholdSuffix.Text = "below uses Architect"
            SpandrelModelInfoLabel.Text = "Auto Texture Routing: feature-based; " & PainterShareText & "% max to Painter for 10+ textures; scores below " & PainterThresholdText & " use Architect."
            Dim RouterTooltip As String = "Feature-based routing (not semantic object recognition)." & Environment.NewLine &
                "For batches under 10 images, every texture meeting the minimum score is sent to Painter. Larger batches rank eligible textures by score and cap Painter at the selected share." & Environment.NewLine &
                "Minimum Painter score: " & PainterThresholdText & Environment.NewLine &
                "Architect model: " & ArchitectPath & Environment.NewLine &
                "Painter model: " & PainterPath & Environment.NewLine &
                "Requires two different 4× RGB super-resolution models."
            UiToolTip.SetToolTip(SpandrelModelInfoLabel, RouterTooltip)
            UiToolTip.SetToolTip(PyModel, RouterTooltip)
            UiToolTip.SetToolTip(AutoArchitectModelComboBox, ArchitectPath)
            UiToolTip.SetToolTip(AutoPainterModelComboBox, PainterPath)
            UiToolTip.SetToolTip(AutoPainterSharePercent, "Maximum Painter share for batches of 10 or more textures. Smaller batches use the score threshold without a percentage cap.")
            UiToolTip.SetToolTip(AutoPainterThresholdLabel, "Textures below this feature score are assigned to Architect. This is a feature heuristic, not semantic classification.")
            UiToolTip.SetToolTip(AutoPainterThreshold, "Minimum feature score for Painter eligibility (0.05–1.00). Lower values make more textures eligible.")
            UiToolTip.SetToolTip(AutoRoutePreviewSampleLabel, "Analyze the first N supported textures, sorted by relative path. Preview is read-only.")
            UiToolTip.SetToolTip(AutoRoutePreviewSampleCount, "Number of supported textures to sample for a quick read-only route preview.")
            Return
        End If
        If Model.Scale > 0 Then
            Dim ScaleDescription As String = If(Model.Scale = 1, "preserves image dimensions", "enlarges " & Model.Scale.ToString() & "×")
            SpandrelModelInfoLabel.Text = "Architecture: " & Model.Architecture & " · " & Model.Scale.ToString() & "× " & Model.Purpose &
                " · RGB " & Model.InputChannels.ToString() & "→" & Model.OutputChannels.ToString() & " · " & ScaleDescription
        Else
            SpandrelModelInfoLabel.Text = "Architecture: " & Model.Architecture & " · Spandrel-compatible image model"
        End If
        UiToolTip.SetToolTip(SpandrelModelInfoLabel, Model.FilePath)
        UiToolTip.SetToolTip(PyModel, Model.FilePath)
    End Sub

    Private Sub PyModel_SelectedIndexChanged(sender As Object, e As EventArgs) Handles PyModel.SelectedIndexChanged
        If String.Equals(If(ExeComboBox.SelectedItem, "").ToString(), SpandrelBackendName, StringComparison.OrdinalIgnoreCase) AndAlso
            PyModel.SelectedIndex >= 0 AndAlso PyModel.SelectedIndex < SupportedSpandrelModels.Count Then
            LastSelectedSpandrelModelPath = SupportedSpandrelModels(PyModel.SelectedIndex).FilePath
        End If
        MarkAutoRoutePreviewStale()
        UpdateSpandrelModelInfo()
    End Sub

    Private Sub AutoArchitectModelComboBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles AutoArchitectModelComboBox.SelectedIndexChanged
        If IsUpdatingAutoRouteModelSelectors Then Return
        Dim ArchitectPath As String = GetSelectedAutoRouteModelPath(AutoArchitectModelComboBox)
        If ArchitectPath <> "" Then LastSelectedArchitectModelPath = ArchitectPath
        ConfigurePainterRoleSelector(LastSelectedPainterModelPath)
        UpdateAutoTextureRouterDefaultPaths()
        MarkAutoRoutePreviewStale()
        UpdateSpandrelModelInfo()
    End Sub

    Private Sub AutoPainterModelComboBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles AutoPainterModelComboBox.SelectedIndexChanged
        If IsUpdatingAutoRouteModelSelectors Then Return
        Dim PainterPath As String = GetSelectedAutoRouteModelPath(AutoPainterModelComboBox)
        If PainterPath <> "" Then LastSelectedPainterModelPath = PainterPath
        UpdateAutoTextureRouterDefaultPaths()
        MarkAutoRoutePreviewStale()
        UpdateSpandrelModelInfo()
    End Sub

    Private Sub AutoPainterSharePercent_ValueChanged(sender As Object, e As EventArgs) Handles AutoPainterSharePercent.ValueChanged
        MarkAutoRoutePreviewStale()
        UpdateSpandrelModelInfo()
    End Sub

    Private Sub AutoPainterThreshold_ValueChanged(sender As Object, e As EventArgs) Handles AutoPainterThreshold.ValueChanged
        MarkAutoRoutePreviewStale()
        UpdateSpandrelModelInfo()
    End Sub

    Private Sub AutoRoutePreviewSampleCount_ValueChanged(sender As Object, e As EventArgs) Handles AutoRoutePreviewSampleCount.ValueChanged
        MarkAutoRoutePreviewStale()
    End Sub

    Private Sub MarkAutoRoutePreviewStale()
        If Not AutoRoutePreviewHasResult Then Return
        AutoRoutePreviewHasResult = False
        AutoRoutePreviewStatusLabel.Text = "Settings changed; run preview again."
    End Sub

    Private Async Sub AutoRoutePreviewButton_Click(sender As Object, e As EventArgs) Handles AutoRoutePreviewButton.Click
        Dim PreviewProfiles As List(Of FormSettings.GamePathProfile) = GetEffectiveGamePathProfiles()
        Dim InitialFolder As String = If(PreviewProfiles.Count > 0,
            PreviewProfiles(0).InputPath, InputTextBox.Text.Trim())
        If Not Directory.Exists(InitialFolder) Then InitialFolder = Application.StartupPath
        Dim PreviewFolder As String = ""
        Using FolderPicker As New FolderBrowserDialog
            FolderPicker.Description = "Choose an existing input texture folder to inspect." & Environment.NewLine &
                "Read-only: nothing is saved or upscaled; only the first N supported textures are analyzed."
            FolderPicker.ShowNewFolderButton = False
            FolderPicker.SelectedPath = InitialFolder
            If FolderPicker.ShowDialog(Me) <> DialogResult.OK Then Return
            PreviewFolder = FolderPicker.SelectedPath
        End Using

        Dim PythonExecutable As String = FindPythonExecutable()
        If PythonExecutable = "" Then
            MessageBox.Show(Me, "Auto Texture Routing preview needs Python 3.10 or newer. Add python.exe to PATH, place it beside AutoCrispy, or set AUTOCRISPY_PYTHON to its full path.", "Python not found", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        Dim RunnerPath As String = Path.Combine(Application.StartupPath, SpandrelRunnerName)
        If Not File.Exists(RunnerPath) Then
            MessageBox.Show(Me, "The AutoCrispy Spandrel runner was not found: " & RunnerPath, "Runner not found", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim Package As FormSettings.PythonPackage
        Try
            Package = GetSelectedPythonPackage()
        Catch ex As Exception
            MessageBox.Show(Me, ex.Message, "Auto Texture Routing", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End Try

        Dim PreviousPyModelEnabled As Boolean = PyModel.Enabled
        Dim PreviousRefreshEnabled As Boolean = RefreshSpandrelModelsButton.Enabled
        Dim PreviousArchitectEnabled As Boolean = AutoArchitectModelComboBox.Enabled
        Dim PreviousPainterEnabled As Boolean = AutoPainterModelComboBox.Enabled
        Dim PreviousShareEnabled As Boolean = AutoPainterSharePercent.Enabled
        Dim PreviousThresholdEnabled As Boolean = AutoPainterThreshold.Enabled
        Dim PreviousSampleCountEnabled As Boolean = AutoRoutePreviewSampleCount.Enabled
        Dim PreviewSampleLimit As Integer = CInt(AutoRoutePreviewSampleCount.Value)
        AutoRoutePreviewButton.Enabled = False
        PyModel.Enabled = False
        RefreshSpandrelModelsButton.Enabled = False
        AutoArchitectModelComboBox.Enabled = False
        AutoPainterModelComboBox.Enabled = False
        AutoPainterSharePercent.Enabled = False
        AutoPainterThreshold.Enabled = False
        AutoRoutePreviewSampleCount.Enabled = False
        AutoRoutePreviewStatusLabel.Text = "Analyzing sample…"
        Dim PreviewDebugEnabled As Boolean = DebugCheckbox.Checked
        Try
            Dim PreviewItems As List(Of AutoRoutePreviewItem) = Await Task.Run(
                Function() RunAutoRoutePreview(
                    PythonExecutable, RunnerPath, PreviewFolder, Package,
                    PreviewSampleLimit, PreviewDebugEnabled))
            Dim PainterCount As Integer = PreviewItems.Where(Function(Item) String.Equals(Item.Role, "Painter", StringComparison.OrdinalIgnoreCase)).Count()
            AutoRoutePreviewStatusLabel.Text = "Sample " & PreviewItems.Count.ToString() & ": " &
                PainterCount.ToString() & " Painter / " & (PreviewItems.Count - PainterCount).ToString() & " Architect"
            AutoRoutePreviewHasResult = True
            Using PreviewDialog As New AutoRoutePreviewDialog(PreviewItems, PreviewFolder, PreviewSampleLimit,
                Package.PainterShare, CDbl(Package.PainterThreshold), Package.ArchitectModel, Package.PainterModel)
                PreviewDialog.ShowDialog(Me)
            End Using
        Catch ex As Exception
            AutoRoutePreviewHasResult = False
            AutoRoutePreviewStatusLabel.Text = "Preview failed."
            MessageBox.Show(Me, "Could not preview texture routing." & Environment.NewLine & ex.Message,
                "Auto Texture Routing preview", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            AutoRoutePreviewButton.Enabled = True
            PyModel.Enabled = PreviousPyModelEnabled
            RefreshSpandrelModelsButton.Enabled = PreviousRefreshEnabled
            AutoArchitectModelComboBox.Enabled = PreviousArchitectEnabled
            AutoPainterModelComboBox.Enabled = PreviousPainterEnabled
            AutoPainterSharePercent.Enabled = PreviousShareEnabled
            AutoPainterThreshold.Enabled = PreviousThresholdEnabled
            AutoRoutePreviewSampleCount.Enabled = PreviousSampleCountEnabled
        End Try
    End Sub

    Private Function RunAutoRoutePreview(PythonExecutable As String, RunnerPath As String, SourceFolder As String,
                                         Package As FormSettings.PythonPackage, SampleLimit As Integer,
                                         DebugEnabled As Boolean) As List(Of AutoRoutePreviewItem)
        Dim StartInfo As New ProcessStartInfo(PythonExecutable,
            MakeSpandrelPreviewCommand(RunnerPath, SourceFolder, Package, SampleLimit, DebugEnabled))
        StartInfo.WorkingDirectory = Application.StartupPath
        StartInfo.RedirectStandardOutput = True
        StartInfo.RedirectStandardError = True
        StartInfo.UseShellExecute = False
        StartInfo.CreateNoWindow = True

        Using PreviewProcess As Process = Process.Start(StartInfo)
            If PreviewProcess Is Nothing Then Throw New InvalidOperationException("Failed to start the route preview process.")
            Dim StandardOutputTask = PreviewProcess.StandardOutput.ReadToEndAsync()
            Dim StandardErrorTask = PreviewProcess.StandardError.ReadToEndAsync()
            PreviewProcess.WaitForExit()
            Dim StandardOutput As String = StandardOutputTask.Result
            Dim StandardError As String = StandardErrorTask.Result
            If PreviewProcess.ExitCode <> 0 Then
                Dim Details As String = If(StandardError.Trim() <> "", StandardError.Trim(), StandardOutput.Trim())
                If Details.Length > 2000 Then Details = Details.Substring(0, 2000) & "..."
                Throw New InvalidOperationException("Python route analysis failed (exit code " & PreviewProcess.ExitCode.ToString() & "). " & Details)
            End If

            Dim Result As New List(Of AutoRoutePreviewItem)
            Const PreviewItemPrefix As String = "AUTOCRISPY_ROUTE_PREVIEW_ITEM:"
            For Each OutputLine As String In StandardOutput.Split(New String() {vbCrLf, vbLf}, StringSplitOptions.RemoveEmptyEntries)
                If Not OutputLine.StartsWith(PreviewItemPrefix, StringComparison.Ordinal) Then Continue For
                Dim Fields As String() = OutputLine.Substring(PreviewItemPrefix.Length).Split(ControlChars.Tab)
                If Fields.Length < 9 Then Continue For
                Dim Item As New AutoRoutePreviewItem With {
                    .FilePath = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(Fields(0))),
                    .Role = Fields(1),
                    .Score = ParsePreviewMetric(Fields(2)),
                    .Detail = ParsePreviewMetric(Fields(3)),
                    .EdgeDensity = ParsePreviewMetric(Fields(4)),
                    .OrientationEntropy = ParsePreviewMetric(Fields(5)),
                    .LocalPatternEntropy = ParsePreviewMetric(Fields(6)),
                    .Periodicity = ParsePreviewMetric(Fields(7)),
                    .DecisionReason = Fields(8)
                }
                Result.Add(Item)
            Next
            Return Result
        End Using
    End Function

    Private Shared Function ParsePreviewMetric(Value As String) As Double
        Dim Result As Double = 0.0R
        Double.TryParse(Value, NumberStyles.Float, CultureInfo.InvariantCulture, Result)
        Return Result
    End Function

    Private Async Sub RefreshSpandrelModelsButton_Click(sender As Object, e As EventArgs) Handles RefreshSpandrelModelsButton.Click
        Await RefreshSupportedSpandrelModels(Root)
    End Sub

    Public Function GetSelectedUpscaleModel() As String
        Dim BackendName As String = If(ExeComboBox.SelectedItem, "").ToString()
        If BackendName = PLKSRBackendName OrElse BackendName = DAT2BackendName OrElse BackendName = SpandrelBackendName Then
            If BackendName = SpandrelBackendName AndAlso PyModel.SelectedIndex >= 0 AndAlso PyModel.SelectedIndex < SupportedSpandrelModels.Count AndAlso
                SupportedSpandrelModels(PyModel.SelectedIndex).IsAutoTextureRouter Then Return AutoTextureRouterToken
            Return GetSpandrelModelPath(BackendName)
        End If
        If PyModel.SelectedIndex >= 0 AndAlso PyModel.SelectedIndex < PyModels.Count Then
            Return PyModels(PyModel.SelectedIndex)
        End If
        Return ""
    End Function

    Public Function GetSelectedPythonPackage() As FormSettings.PythonPackage
        Dim BackendName As String = If(ExeComboBox.SelectedItem, "").ToString()
        Dim UseSpandrelFormats As Boolean = BackendName = PLKSRBackendName OrElse BackendName = DAT2BackendName OrElse BackendName = SpandrelBackendName
        If BackendName = SpandrelBackendName AndAlso PyModel.SelectedIndex >= 0 AndAlso PyModel.SelectedIndex < SupportedSpandrelModels.Count Then
            Dim Model As SpandrelModelInfo = SupportedSpandrelModels(PyModel.SelectedIndex)
            If Model.IsAutoTextureRouter Then
                Dim ArchitectPath As String = GetSelectedAutoRouteModelPath(AutoArchitectModelComboBox)
                Dim PainterPath As String = GetSelectedAutoRouteModelPath(AutoPainterModelComboBox)
                If ArchitectPath = "" Then ArchitectPath = Model.ArchitectModelPath
                If PainterPath = "" Then PainterPath = Model.PainterModelPath
                If String.IsNullOrWhiteSpace(ArchitectPath) OrElse String.IsNullOrWhiteSpace(PainterPath) OrElse
                    String.Equals(ArchitectPath, PainterPath, StringComparison.OrdinalIgnoreCase) Then
                    Throw New InvalidOperationException("Auto Texture Routing needs two different compatible 4× RGB models.")
                End If
                LastSelectedArchitectModelPath = ArchitectPath
                LastSelectedPainterModelPath = PainterPath
                Return New FormSettings.PythonPackage(AutoTextureRouterToken, CInt(PyTileSize.Value), PyCPU.Checked, True, True,
                    ArchitectPath, PainterPath, CInt(AutoPainterSharePercent.Value), CDec(AutoPainterThreshold.Value))
            End If
        End If
        Return New FormSettings.PythonPackage(GetSelectedUpscaleModel(), CInt(PyTileSize.Value), PyCPU.Checked, UseSpandrelFormats,
            False, LastSelectedArchitectModelPath, LastSelectedPainterModelPath, CInt(AutoPainterSharePercent.Value), CDec(AutoPainterThreshold.Value))
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
        PyGroup.Enabled = Enabled
    End Sub

#End Region

#Region "Background"

    Private Sub WatchDog_Tick(sender As Object, e As EventArgs) Handles WatchDog.Tick
        Dim ProfilesToWatch As List(Of FormSettings.GamePathProfile) = GetEffectiveGamePathProfiles()
        Dim PendingProfiles As New List(Of FormSettings.GamePathProfile)
        Dim MissingProfileCount As Integer = 0
        ' Use the first stage's input types and cached alpha-filter results so skipped files do not
        ' keep scheduling empty batches (or an empty Python process) forever.
        Dim SupportedExtensions As HashSet(Of String) = GetActiveInputFileTypes()
        Dim AlphaMode As Integer = GetActiveAlphaMode()

        For Each Profile As FormSettings.GamePathProfile In ProfilesToWatch
            Try
                If Not Directory.Exists(Profile.InputPath) OrElse Not Directory.Exists(Profile.OutputPath) Then
                    MissingProfileCount += 1
                    Continue For
                End If
                Dim AllInputFiles As String() = Directory.GetFiles(Profile.InputPath, "*.*", SearchOption.AllDirectories)
                Dim UnsupportedCount As Integer = 0
                Dim AlphaFilteredCount As Integer = 0
                Dim SupportedCount As Integer = 0
                Dim MissingCount As Integer = 0
                Dim PendingFiles As String() = GetPendingInputFiles(AllInputFiles, Profile.OutputPath, SupportedExtensions, AlphaMode,
                                                                      UnsupportedCount, AlphaFilteredCount, SupportedCount, MissingCount)
                If PendingFiles.Length > 0 Then PendingProfiles.Add(CloneGamePathProfile(Profile))
            Catch ex As IOException
                MissingProfileCount += 1
            Catch ex As UnauthorizedAccessException
                MissingProfileCount += 1
            End Try
        Next

        If PendingProfiles.Count = 0 Then
            If MissingProfileCount > 0 Then
                QueueActivityLabel.Text = "Watching · " & MissingProfileCount.ToString() & " game path(s) unavailable"
            ElseIf ProfilesToWatch.Count = 0 Then
                QueueActivityLabel.Text = "No checked game paths to watch"
            Else
                QueueActivityLabel.Text = "Watching " & ProfilesToWatch.Count.ToString() & " game path(s)…"
            End If
            WaitScale = Math.Min(WaitScale + 1, 100)
            WatchDog.Interval = 1000 + (WaitScale * 590)
        Else
            QueueActivityLabel.Text = "Starting next batch for " & PendingProfiles.Count.ToString() & " game(s)…"
            WaitScale = 0
            WatchDog.Interval = 1000
            CurrentRunGamePaths = PendingProfiles
            LoadedSettings = New FormSettings.Settings(Me)
            If ChainControl.ListItems.Count = 0 Then
                AddModelToChain(ExeComboBox.SelectedItem, False)
            End If
            ProgressPollTimer.Interval = 1000
            WorkHorse.RunWorkerAsync()
        End If
    End Sub

    ' Progress is overall completion for files the first pipeline stage can accept.
    ' Match by basename so format conversions (for example PNG input to DDS output) count as done.
    Private Sub ProgressPollTimer_Tick(sender As Object, e As EventArgs) Handles ProgressPollTimer.Tick
        Try
            Dim DoneCount As Integer = 0
            Dim TotalCount As Integer = 0
            Dim UnsupportedCount As Integer = 0
            Dim AlphaFilteredCount As Integer = 0
            Dim Percent As Integer = GetOverallProgress(DoneCount, TotalCount, UnsupportedCount, AlphaFilteredCount)
            If Percent < UpscaleProgress.Minimum Then Percent = UpscaleProgress.Minimum
            If Percent > UpscaleProgress.Maximum Then Percent = UpscaleProgress.Maximum
            UpscaleProgress.Value = Percent
            Dim SkippedSummary As String = ""
            If UnsupportedCount > 0 Then SkippedSummary &= " · " & UnsupportedCount.ToString() & " unsupported skipped"
            If AlphaFilteredCount > 0 Then SkippedSummary &= " · " & AlphaFilteredCount.ToString() & " skipped by alpha filter"
            QueueSummaryLabel.Text = DoneCount.ToString() & " / " & TotalCount.ToString() & " textures complete (" & Percent.ToString() & "%)" & SkippedSummary

            If Not WorkHorse.IsBusy AndAlso RefreshSpandrelModelsButton.Enabled Then
                If WatchDog.Enabled Then
                    If TotalCount = 0 OrElse DoneCount >= TotalCount Then
                        QueueActivityLabel.Text = "Watching for new textures…"
                    Else
                        QueueActivityLabel.Text = "Watching · " & (TotalCount - DoneCount).ToString() & " remaining"
                    End If
                ElseIf TotalCount = 0 Then
                    QueueActivityLabel.Text = "Ready"
                ElseIf DoneCount >= TotalCount Then
                    QueueActivityLabel.Text = "Complete"
                Else
                    QueueActivityLabel.Text = "Paused · " & (TotalCount - DoneCount).ToString() & " remaining"
                End If
            End If
        Catch ex As Exception
            ' A removable or network-backed input/output folder can disappear during a scan.
        End Try
        ProgressPollTimer.Interval = If(WorkHorse.IsBusy, 1000, 5000)
    End Sub

    Private Function GetOverallProgress(ByRef DoneCount As Integer, ByRef TotalCount As Integer,
                                        ByRef UnsupportedCount As Integer, ByRef AlphaFilteredCount As Integer) As Integer
        DoneCount = 0
        TotalCount = 0
        UnsupportedCount = 0
        AlphaFilteredCount = 0
        Dim SupportedExtensions As HashSet(Of String) = GetActiveInputFileTypes()
        Dim AlphaMode As Integer = GetActiveAlphaMode()
        For Each Profile As FormSettings.GamePathProfile In GetEffectiveGamePathProfiles()
            If Not Directory.Exists(Profile.InputPath) OrElse Not Directory.Exists(Profile.OutputPath) Then Continue For
            Dim AllInputFiles As String() = Directory.GetFiles(Profile.InputPath, "*.*", SearchOption.AllDirectories)
            Dim ProfileUnsupportedCount As Integer = 0
            Dim ProfileAlphaFilteredCount As Integer = 0
            Dim SupportedCount As Integer = 0
            Dim MissingCount As Integer = 0
            Dim PendingFiles As String() = GetPendingInputFiles(AllInputFiles, Profile.OutputPath, SupportedExtensions, AlphaMode,
                                                                  ProfileUnsupportedCount, ProfileAlphaFilteredCount, SupportedCount, MissingCount)
            UnsupportedCount += ProfileUnsupportedCount
            AlphaFilteredCount += ProfileAlphaFilteredCount
            TotalCount += SupportedCount - ProfileAlphaFilteredCount
            DoneCount += SupportedCount - MissingCount
        Next

        If TotalCount = 0 OrElse DoneCount <= 0 Then Return 0
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
        CurrentRunGamePaths = Nothing
        If ChainControl.ListItems.Count = 0 Then
            ChainList.Clear()
        End If
        If e.Cancelled OrElse WatchDogButton.Text = "Stopping..." Then
            WatchDog.Stop()
            WatchDog.Enabled = False
            WatchDogButton.Text = "Running: False"
            SwitchGroups(True)
            WatchDogButton.Enabled = True
            ClearAlphaSkipList()
            QueueActivityLabel.Text = "Cancelled"
            Dim SingleRunPath As String = Path.Combine(Path.GetTempPath(), "Single_0")
            If Directory.Exists(SingleRunPath) Then Directory.Delete(SingleRunPath, True)
            Exit Sub
        End If
        If e.Error IsNot Nothing Then
            WatchDog.Stop()
            WatchDog.Enabled = False
            WatchDogButton.Text = "Running: False"
            WatchDogButton.Enabled = True
            SwitchGroups(True)
            ClearAlphaSkipList()
            QueueActivityLabel.Text = "Failed — see error details"
            UiToolTip.SetToolTip(QueueActivityLabel, e.Error.GetBaseException().Message)
            Dim SingleRunPath As String = Path.Combine(Path.GetTempPath(), "Single_0")
            If Directory.Exists(SingleRunPath) Then Directory.Delete(SingleRunPath, True)
            MessageBox.Show("Upscaling failed: " & e.Error.GetBaseException().Message, "AutoCrispy error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Exit Sub
        End If
        If WatchDogButton.Text = "Running: True" Then
            QueueActivityLabel.Text = "Watching for new textures…"
            WatchDog.Start()
        Else
            ClearAlphaSkipList()
            QueueActivityLabel.Text = "One-off run complete"
            WatchDog.Stop()
            WatchDog.Enabled = False
            WatchDogButton.Text = "Running: False"
            WatchDogButton.Enabled = True
            Dim SingleRunPath As String = Path.Combine(Path.GetTempPath(), "Single_0")
            If Directory.Exists(SingleRunPath) Then Directory.Delete(SingleRunPath, True)
            SwitchGroups(True)
        End If
    End Sub

#End Region

#Region "Upscale Routine"

    Private Sub MakeUpscale()
        Dim ProfilesToProcess As List(Of FormSettings.GamePathProfile) = CurrentRunGamePaths
        If ProfilesToProcess Is Nothing OrElse ProfilesToProcess.Count = 0 Then Return

        Dim BackendPath As String = LoadedSettings.Paths.ExePath
        For Each Profile As FormSettings.GamePathProfile In ProfilesToProcess
            If WorkHorse.CancellationPending Then Return
            LoadedSettings.Paths = New FormSettings.ProgramPaths(Profile.InputPath, Profile.OutputPath, BackendPath)
            WorkHorse.ReportProgress(0, "Game " & Profile.Name & " · processing input/output folders")
            MakeUpscaleForCurrentPaths(Profile.Name)
        Next
    End Sub

    Private Sub MakeUpscaleForCurrentPaths(ProfileName As String)
        Dim TempPath As String = GetChainPath("Temp", 0)
        Dim ThreadCount As Integer = GetThreads(LoadedSettings.BasicSettings.ThreadIndex, LoadedSettings.BasicSettings.ThreadCount)
        If ThreadCount < 1 Then ThreadCount = 1
        Dim AllInputFiles As String() = Directory.GetFiles(LoadedSettings.Paths.InputPath, "*.*", SearchOption.AllDirectories)
        Dim UnsupportedCount As Integer = 0
        Dim AlphaFilteredCount As Integer = 0
        Dim SupportedCount As Integer = 0
        Dim MissingCount As Integer = 0
        Dim Source As String() = GetPendingInputFiles(AllInputFiles, LoadedSettings.Paths.OutputPath, GetActiveInputFileTypes(),
                                                      LoadedSettings.ExpertSettings.AlphaMode, UnsupportedCount,
                                                      AlphaFilteredCount, SupportedCount, MissingCount)
        If WorkHorse.CancellationPending Then
            CleanupUpscaleTemporaryFolders()
            Return
        End If
        ' CopyFiles advances this cursor, so the outer loop must not also add a batch step.
        Dim CurrentIndex As Integer = 0
        Dim BatchLimit As Integer = ThreadCount
        If HasAutoTextureRouterInChain() Then BatchLimit = Math.Max(BatchLimit, Source.Length)
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
            CopyFiles(Source, TempPath, CurrentIndex, BatchLimit)
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
            WorkHorse.ReportProgress(0, "Prepared " & BatchFiles.Length.ToString() & " texture(s) for " & ProfileName & ": " & BatchDescription)
            Dim StageIndex As Integer = 0
            For Each Model In ChainList
                StageIndex += 1
                If WorkHorse.CancellationPending Then
                    CleanupUpscaleTemporaryFolders()
                    Return
                End If
                WorkHorse.ReportProgress(0, "Step " & StageIndex.ToString() & "/" & ChainList.Count.ToString() & " · " & ProfileName & " · " & Model.Name & " · " & BatchDescription)
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

    Private Function HasAutoTextureRouterInChain() As Boolean
        For Each ChainItem As FormSettings.ChainObject In ChainList
            If ChainItem.PackageType = SpandrelBackendName AndAlso ChainItem.Package IsNot Nothing AndAlso
                TypeOf ChainItem.Package Is FormSettings.PythonPackage Then
                Dim PythonSettings As FormSettings.PythonPackage = CType(ChainItem.Package, FormSettings.PythonPackage)
                If PythonSettings.AutoRouteEnabled Then Return True
            End If
        Next
        Return False
    End Function

    Private Sub CopyFiles(FileList As String(), RootPath As String, ByRef CurrentIndex As Integer, BatchSize As Integer)
        Dim CopyCounter As Integer = 0
        Dim AlphaMode As Integer = LoadedSettings.ExpertSettings.AlphaMode
        Do While CurrentIndex < FileList.Count AndAlso CopyCounter < BatchSize AndAlso Not WorkHorse.CancellationPending
            Dim FilePath As String = FileList(CurrentIndex)
            If Not IsAlphaFiltered(FilePath, AlphaMode) Then
                Select Case AlphaMode
                    Case 0
                        File.Copy(FilePath, RootPath & "\" & Path.GetFileName(FilePath), True)
                        CopyCounter += 1
                    Case 1
                        If Not GetHasTransparency(FilePath) Then
                            File.Copy(FilePath, RootPath & "\" & Path.GetFileName(FilePath), True)
                            CopyCounter += 1
                        Else
                            MarkAlphaFiltered(FilePath, AlphaMode)
                        End If
                    Case 2
                        If GetHasTransparency(FilePath) Then
                            File.Copy(FilePath, RootPath & "\" & Path.GetFileName(FilePath), True)
                            CopyCounter += 1
                        Else
                            MarkAlphaFiltered(FilePath, AlphaMode)
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
        Dim IsAutoRouteRun As Boolean = IsSpandrelBackend AndAlso
            TypeOf Model.Package Is FormSettings.PythonPackage AndAlso
            DirectCast(Model.Package, FormSettings.PythonPackage).AutoRouteEnabled
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
                    Dim StandardOutputTask = Task.Run(Function() ReadSpandrelOutput(BatchProcess))
                    Dim StandardErrorTask = BatchProcess.StandardError.ReadToEndAsync()
                    WaitForActiveProcess(BatchProcess)
                    Dim StandardOutput As String = StandardOutputTask.Result
                    Dim StandardError As String = StandardErrorTask.Result
                    If WorkHorse.CancellationPending Then
                        DeleteCancelledOutputs(DestPath, ImageList)
                        Return
                    End If
                    If LoadedSettings.ExpertSettings.Logging OrElse IsAutoRouteRun OrElse BatchProcess.ExitCode <> 0 Then
                        WriteProcessLog(BuildProcess, StandardOutput, StandardError, LoadedSettings.Paths.OutputPath, Model.PackageType, BatchProcess.ExitCode)
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

    Private Function ReadSpandrelOutput(SpandrelProcess As Process) As String
        Dim CapturedOutput As New System.Text.StringBuilder()
        Dim LastProgressUpdate As DateTime = DateTime.MinValue
        While True
            Dim OutputLine As String = SpandrelProcess.StandardOutput.ReadLine()
            If OutputLine Is Nothing Then Exit While
            CapturedOutput.AppendLine(OutputLine)
            If OutputLine.StartsWith("AUTOCRISPY_PROGRESS:", StringComparison.Ordinal) AndAlso
                (DateTime.UtcNow - LastProgressUpdate).TotalMilliseconds >= 500 Then
                WorkHorse.ReportProgress(0, OutputLine.Substring("AUTOCRISPY_PROGRESS:".Length).Trim())
                LastProgressUpdate = DateTime.UtcNow
            End If
        End While
        Return CapturedOutput.ToString()
    End Function

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

    Private Sub AddModelToChain(Mode As String, Optional AddPreview As Boolean = True)
        Select Case Mode
            Case "Waifu2x Caffe"
                ChainControl.ListItems.Add(New DragDropList.DragDropItem(ChainList.Count, "Caffe", ChainThumbs.Item(0)))
                ChainList.Add(New FormSettings.ChainObject("Caffe", 0, CaffePath, "Waifu2x Caffe", Me))
            Case "Waifu2x Vulkan"
                ChainControl.ListItems.Add(New DragDropList.DragDropItem(ChainList.Count, "Waifu Vulkan", ChainThumbs.Item(1)))
                ChainList.Add(New FormSettings.ChainObject("Waifu Vulkan", 1, WaifuNcnnPath, "Waifu2x Vulkan", Me))
            Case "RealSR Vulkan"
                ChainControl.ListItems.Add(New DragDropList.DragDropItem(ChainList.Count, "RealSR Vulkan", ChainThumbs.Item(2)))
                ChainList.Add(New FormSettings.ChainObject("RealSR Vulkan", 2, RealSRNcnnPath, "RealSR Vulkan", Me))
            Case "RealESRGAN Vulkan"
                ChainControl.ListItems.Add(New DragDropList.DragDropItem(ChainList.Count, "RealESRGAN Vulkan", ChainThumbs.Item(8)))
                ChainList.Add(New FormSettings.ChainObject("RealESRGAN Vulkan", 8, RealESRGNcnnPath, "RealESRGAN Vulkan", Me))
            Case "SRMD Vulkan"
                ChainControl.ListItems.Add(New DragDropList.DragDropItem(ChainList.Count, "SRMD Vulkan", ChainThumbs.Item(3)))
                ChainList.Add(New FormSettings.ChainObject("SRMD Vulkan", 3, SRMDNcnnPath, "SRMD Vulkan", Me))
            Case "Waifu2x CPP"
                ChainControl.ListItems.Add(New DragDropList.DragDropItem(ChainList.Count, "Waifu CPP", ChainThumbs.Item(4)))
                ChainList.Add(New FormSettings.ChainObject("Waifu CPP", 4, WaifuCppPath, "Waifu2x CPP", Me))
            Case "Anime4k CPP"
                ChainControl.ListItems.Add(New DragDropList.DragDropItem(ChainList.Count, "Anime4k", ChainThumbs.Item(5)))
                ChainList.Add(New FormSettings.ChainObject("Anime4k", 5, Anime4kPath, "Anime4k CPP", Me))
            Case "TexConv"
                ChainControl.ListItems.Add(New DragDropList.DragDropItem(ChainList.Count, "TexConv", ChainThumbs.Item(7)))
                ChainList.Add(New FormSettings.ChainObject("TexConv", 7, TexConvPath, "TexConv", Me))
            Case "xBRZ"
                ChainControl.ListItems.Add(New DragDropList.DragDropItem(ChainList.Count, "xBRZ", ChainThumbs.Item(0)))
                ChainList.Add(New FormSettings.ChainObject("xBRZ", 0, xBRZPath, "xBRZ", Me))
            Case "ESRGAN"
                ChainControl.ListItems.Add(New DragDropList.DragDropItem(ChainList.Count, "ESRGAN", ChainThumbs.Item(6)))
                ChainList.Add(New FormSettings.ChainObject("ESRGAN", 6, PyPath, "ESRGAN", Me))
            Case PLKSRBackendName
                ChainControl.ListItems.Add(New DragDropList.DragDropItem(ChainList.Count, "PLKSR 4x", ChainThumbs.Item(6)))
                ChainList.Add(New FormSettings.ChainObject("PLKSR 4x", 6, "", "RealPLKSR", Me))
            Case DAT2BackendName
                ChainControl.ListItems.Add(New DragDropList.DragDropItem(ChainList.Count, "PBRify V4 DAT2 4x", ChainThumbs.Item(6)))
                ChainList.Add(New FormSettings.ChainObject("PBRify V4 DAT2 4x", 6, "", DAT2BackendName, Me))
            Case SpandrelBackendName
                Dim SelectedPackage As FormSettings.PythonPackage = GetSelectedPythonPackage()
                Dim ModelDisplayName As String = "Spandrel - " & Path.GetFileName(SelectedPackage.Model)
                If SelectedPackage.AutoRouteEnabled Then
                    ModelDisplayName = "Spandrel - Auto (" & Path.GetFileName(SelectedPackage.ArchitectModel) & " / " &
                        Path.GetFileName(SelectedPackage.PainterModel) & ", " & CInt(AutoPainterSharePercent.Value).ToString() & "% Painter)"
                End If
                ChainControl.ListItems.Add(New DragDropList.DragDropItem(ChainList.Count, ModelDisplayName, ChainThumbs.Item(6)))
                ChainList.Add(New FormSettings.ChainObject(ModelDisplayName, 6, "", SpandrelBackendName, Me))
        End Select
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

    Private Function MakeSpandrelCommand(SourceFolder As String, DestFolder As String, Package As FormSettings.PythonPackage, GenericModel As Boolean) As String
        Dim Result As New ArguementString
        Result.AddArguement(Quote(Path.Combine(Application.StartupPath, SpandrelRunnerName)))
        If Package.AutoRouteEnabled Then
            Result.AddArguement("--auto-route")
            Result.AddArguement("--architect-model", Quote(Package.ArchitectModel))
            Result.AddArguement("--painter-model", Quote(Package.PainterModel))
            Dim PainterShare As Integer = Package.PainterShare
            If PainterShare < 10 OrElse PainterShare > 90 Then PainterShare = 30
            Result.AddArguement("--painter-share", PainterShare.ToString())
            Dim PainterThreshold As Decimal = Package.PainterThreshold
            If PainterThreshold < 0.05D OrElse PainterThreshold > 1D Then PainterThreshold = 0.34D
            Result.AddArguement("--painter-threshold", PainterThreshold.ToString(CultureInfo.InvariantCulture))
        Else
            Result.AddArguement(Quote(Package.Model))
        End If
        Result.AddArguement("--input", Quote(SourceFolder))
        Result.AddArguement("--output", Quote(DestFolder))
        Result.AddArguement("--tile-size", Package.TileSize.ToString())
        Result.AddArguement("--cpu", Package.CPUOnly)
        If GenericModel Then Result.AddArguement("--generic-model")
        If LoadedSettings.ExpertSettings.Logging Then Result.AddArguement("--debug")
        Return Result.GetArguements
    End Function

    Private Function MakeSpandrelPreviewCommand(RunnerPath As String, SourceFolder As String,
                                                 Package As FormSettings.PythonPackage, SampleLimit As Integer,
                                                 DebugEnabled As Boolean) As String
        Dim Result As New ArguementString
        Result.AddArguement(Quote(RunnerPath))
        Result.AddArguement("--preview-route")
        Result.AddArguement("--input", Quote(SourceFolder))
        Result.AddArguement("--preview-limit", SampleLimit.ToString(CultureInfo.InvariantCulture))
        Dim PainterShare As Integer = Package.PainterShare
        If PainterShare < 10 OrElse PainterShare > 90 Then PainterShare = 30
        Result.AddArguement("--painter-share", PainterShare.ToString())
        Dim PainterThreshold As Decimal = Package.PainterThreshold
        If PainterThreshold < 0.05D OrElse PainterThreshold > 1D Then PainterThreshold = 0.34D
        Result.AddArguement("--painter-threshold", PainterThreshold.ToString(CultureInfo.InvariantCulture))
        If DebugEnabled Then Result.AddArguement("--debug")
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
        Using SourceImage As Bitmap = GetUnlockedImage(Source)
            Dim SourceRect As Rectangle = New Rectangle(0, 0, SourceImage.Width, SourceImage.Height)
            Dim SourceData As Imaging.BitmapData = Nothing
            Try
                SourceData = SourceImage.LockBits(SourceRect, Imaging.ImageLockMode.ReadOnly, SourceImage.PixelFormat)
                Dim SourcePtr As IntPtr = SourceData.Scan0
                Dim SourceByteCount As Integer = Math.Abs(SourceData.Stride) * SourceImage.Height
                Dim SourceBytes As Byte() = New Byte(SourceByteCount - 1) {}
                Runtime.InteropServices.Marshal.Copy(SourcePtr, SourceBytes, 0, SourceByteCount)
                For i = 3 To SourceBytes.Length - 1 Step 4
                    If SourceBytes(i) = 0 Then Return True
                Next
            Finally
                If SourceData IsNot Nothing Then SourceImage.UnlockBits(SourceData)
            End Try
        End Using
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

    Private Function GetPackageInputFileTypes(Package As Object) As HashSet(Of String)
        If Package Is Nothing Then Return Nothing
        Dim FileTypesProperty As PropertyInfo = Package.GetType().GetProperty("FileTypes")
        If FileTypesProperty Is Nothing Then Return Nothing
        Dim PackageFileTypes As IEnumerable(Of String) = TryCast(FileTypesProperty.GetValue(Package, Nothing), IEnumerable(Of String))
        If PackageFileTypes Is Nothing Then Return Nothing

        Dim SupportedExtensions As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each FileType As String In PackageFileTypes
            If String.IsNullOrWhiteSpace(FileType) Then Continue For
            Dim NormalizedExtension As String = FileType.Trim()
            If Not NormalizedExtension.StartsWith(".", StringComparison.Ordinal) Then NormalizedExtension = "." & NormalizedExtension
            SupportedExtensions.Add(NormalizedExtension)
        Next
        If SupportedExtensions.Count = 0 Then Return Nothing
        Return SupportedExtensions
    End Function

    Private Function GetActiveInputFileTypes() As HashSet(Of String)
        If ChainList IsNot Nothing AndAlso ChainList.Count > 0 Then
            Return GetPackageInputFileTypes(ChainList(0).Package)
        End If

        Dim BackendName As String = If(ExeComboBox.SelectedItem, "").ToString()
        If String.IsNullOrWhiteSpace(BackendName) Then Return Nothing
        Dim PackageType As String = If(BackendName = PLKSRBackendName, "RealPLKSR", BackendName)
        Try
            Dim SelectedPackage As New FormSettings.ChainObject(BackendName, 0, "", PackageType, Me)
            Return GetPackageInputFileTypes(SelectedPackage.Package)
        Catch ex As Exception
            ' Leave the input list unfiltered if the selected package is not ready yet.
            Return Nothing
        End Try
    End Function

    Private Function GetSupportedInputFiles(InputFiles As String(), SupportedExtensions As HashSet(Of String)) As String()
        If InputFiles Is Nothing Then Return New String() {}
        If SupportedExtensions Is Nothing OrElse SupportedExtensions.Count = 0 Then Return InputFiles

        Dim Result As New List(Of String)
        For Each InputFile As String In InputFiles
            If SupportedExtensions.Contains(Path.GetExtension(InputFile)) Then Result.Add(InputFile)
        Next
        Return Result.ToArray()
    End Function

    Private Function GetPendingInputFiles(InputFiles As String(), OutputPath As String, SupportedExtensions As HashSet(Of String),
                                          AlphaMode As Integer, ByRef UnsupportedCount As Integer,
                                          ByRef AlphaFilteredCount As Integer, ByRef SupportedCount As Integer,
                                          ByRef MissingCount As Integer) As String()
        Dim SupportedFiles As String() = GetSupportedInputFiles(InputFiles, SupportedExtensions)
        UnsupportedCount = If(InputFiles Is Nothing, 0, InputFiles.Length - SupportedFiles.Length)
        SupportedCount = SupportedFiles.Length
        Dim MissingFiles As String() = GetMissingFiles(SupportedFiles, OutputPath)
        MissingCount = MissingFiles.Length
        AlphaFilteredCount = 0

        Dim PendingFiles As New List(Of String)
        For Each MissingFile As String In MissingFiles
            If IsAlphaFiltered(MissingFile, AlphaMode) Then
                AlphaFilteredCount += 1
            Else
                PendingFiles.Add(MissingFile)
            End If
        Next
        Return PendingFiles.ToArray()
    End Function

    Private Function GetActiveAlphaMode() As Integer
        If WorkHorse.IsBusy Then Return LoadedSettings.ExpertSettings.AlphaMode
        Return Math.Max(0, AlphaComboBox.SelectedIndex)
    End Function

    Private Function GetAlphaSkipKey(FilePath As String, AlphaMode As Integer) As String
        Return AlphaMode.ToString(CultureInfo.InvariantCulture) & "|" & Path.GetFullPath(FilePath)
    End Function

    Private Function IsAlphaFiltered(FilePath As String, AlphaMode As Integer) As Boolean
        If AlphaMode <= 0 Then Return False
        Dim SkipKey As String = GetAlphaSkipKey(FilePath, AlphaMode)
        SyncLock SkipListLock
            Return SkipList.Contains(SkipKey)
        End SyncLock
    End Function

    Private Sub MarkAlphaFiltered(FilePath As String, AlphaMode As Integer)
        If AlphaMode <= 0 Then Return
        Dim SkipKey As String = GetAlphaSkipKey(FilePath, AlphaMode)
        SyncLock SkipListLock
            SkipList.Add(SkipKey)
        End SyncLock
    End Sub

    Private Sub ClearAlphaSkipList()
        SyncLock SkipListLock
            SkipList.Clear()
        End SyncLock
    End Sub

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

    Private Sub WriteProcessLog(StartInfo As ProcessStartInfo, StandardOutput As String, StandardError As String, SaveLoc As String, BackendName As String, ExitCode As Integer)
        Dim Filename As String = Path.Combine(SaveLoc, BackendName & "_" & Now.ToString("yyyy-MM-dd_HH-mm-ss") & ".txt")
        Dim Output As String = StartInfo.FileName & " " & StartInfo.Arguments & vbNewLine & "Exit code: " & ExitCode.ToString() & vbNewLine & vbNewLine
        Output &= StandardOutput & vbNewLine & vbNewLine & StandardError
        File.WriteAllText(Filename, Output)
    End Sub

#End Region

End Class

Friend NotInheritable Class GamePathProfilesDialog
    Inherits Form

    Private ReadOnly _profiles As BindingList(Of FormSettings.GamePathProfile)
    Private ReadOnly _grid As DataGridView
    Private ReadOnly _defaultInputPath As String
    Private ReadOnly _defaultOutputPath As String

    Public Sub New(Profiles As BindingList(Of FormSettings.GamePathProfile), DefaultInputPath As String, DefaultOutputPath As String)
        _profiles = Profiles
        _defaultInputPath = DefaultInputPath
        _defaultOutputPath = DefaultOutputPath

        Text = "Game input/output paths"
        StartPosition = FormStartPosition.CenterParent
        FormBorderStyle = FormBorderStyle.Sizable
        MinimizeBox = False
        MaximizeBox = False
        MinimumSize = New Size(820, 430)
        ClientSize = New Size(1000, 560)

        _grid = New DataGridView With {
            .Name = "GamePathProfilesGrid",
            .Dock = DockStyle.Fill,
            .AllowUserToAddRows = False,
            .AllowUserToDeleteRows = False,
            .AllowUserToResizeRows = False,
            .AutoGenerateColumns = False,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            .BackgroundColor = SystemColors.Window,
            .BorderStyle = BorderStyle.Fixed3D,
            .ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
            .MultiSelect = False,
            .ReadOnly = False,
            .RowHeadersVisible = False,
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect
        }

        Dim WatchColumn As New DataGridViewCheckBoxColumn With {
            .Name = "WatchColumn",
            .HeaderText = "Watch",
            .DataPropertyName = "Enabled",
            .FillWeight = 8,
            .MinimumWidth = 65,
            .ReadOnly = False,
            .SortMode = DataGridViewColumnSortMode.NotSortable
        }
        Dim NameColumn As New DataGridViewTextBoxColumn With {
            .Name = "ProfileNameColumn",
            .HeaderText = "Game / profile",
            .DataPropertyName = "Name",
            .FillWeight = 17,
            .MinimumWidth = 120,
            .ReadOnly = True,
            .SortMode = DataGridViewColumnSortMode.NotSortable
        }
        Dim InputColumn As New DataGridViewTextBoxColumn With {
            .Name = "InputPathColumn",
            .HeaderText = "Input folder",
            .DataPropertyName = "InputPath",
            .FillWeight = 37.5,
            .MinimumWidth = 180,
            .ReadOnly = True,
            .SortMode = DataGridViewColumnSortMode.NotSortable
        }
        Dim OutputColumn As New DataGridViewTextBoxColumn With {
            .Name = "OutputPathColumn",
            .HeaderText = "Output folder",
            .DataPropertyName = "OutputPath",
            .FillWeight = 37.5,
            .MinimumWidth = 180,
            .ReadOnly = True,
            .SortMode = DataGridViewColumnSortMode.NotSortable
        }
        _grid.Columns.AddRange(New DataGridViewColumn() {WatchColumn, NameColumn, InputColumn, OutputColumn})
        _grid.DataSource = _profiles
        AddHandler _grid.CurrentCellDirtyStateChanged, AddressOf Grid_CurrentCellDirtyStateChanged
        AddHandler _grid.CellDoubleClick, AddressOf Grid_CellDoubleClick
        AddHandler Me.FormClosing, AddressOf Dialog_FormClosing

        Dim Description As New Label With {
            .AutoEllipsis = True,
            .Dock = DockStyle.Fill,
            .Text = "Check each game folder AutoCrispy should watch. A checked row uses its own input and output folders.",
            .TextAlign = ContentAlignment.MiddleLeft
        }

        Dim Footer As New FlowLayoutPanel With {
            .Dock = DockStyle.Fill,
            .FlowDirection = FlowDirection.RightToLeft,
            .Padding = New Padding(0, 7, 0, 0),
            .WrapContents = False
        }
        Dim AddButton As Button = CreateButton("Add...", 82)
        Dim EditButton As Button = CreateButton("Edit...", 82)
        Dim RemoveButton As Button = CreateButton("Remove", 82)
        Dim CheckAllButton As Button = CreateButton("Check all", 88)
        Dim UncheckAllButton As Button = CreateButton("Uncheck all", 100)
        Dim CloseButton As Button = CreateButton("Close", 82)
        CloseButton.DialogResult = DialogResult.OK
        Footer.Controls.AddRange(New Control() {CloseButton, UncheckAllButton, CheckAllButton, RemoveButton, EditButton, AddButton})

        AddHandler AddButton.Click, AddressOf AddButton_Click
        AddHandler EditButton.Click, AddressOf EditButton_Click
        AddHandler RemoveButton.Click, AddressOf RemoveButton_Click
        AddHandler CheckAllButton.Click, AddressOf CheckAllButton_Click
        AddHandler UncheckAllButton.Click, AddressOf UncheckAllButton_Click

        Dim Layout As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 3,
            .Padding = New Padding(10)
        }
        Layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0!))
        Layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 36.0!))
        Layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0!))
        Layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 48.0!))
        Layout.Controls.Add(Description, 0, 0)
        Layout.Controls.Add(_grid, 0, 1)
        Layout.Controls.Add(Footer, 0, 2)
        Controls.Add(Layout)
    End Sub

    Private Shared Function CreateButton(Caption As String, Width As Integer) As Button
        Return New Button With {
            .Text = Caption,
            .Width = Width,
            .Height = 32,
            .UseVisualStyleBackColor = True
        }
    End Function

    Private Sub Grid_CurrentCellDirtyStateChanged(sender As Object, e As EventArgs)
        If _grid.IsCurrentCellDirty AndAlso TypeOf _grid.CurrentCell Is DataGridViewCheckBoxCell Then
            _grid.CommitEdit(DataGridViewDataErrorContexts.Commit)
        End If
    End Sub

    Private Sub Grid_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs)
        If e.RowIndex >= 0 Then EditSelectedProfile()
    End Sub

    Private Sub Dialog_FormClosing(sender As Object, e As FormClosingEventArgs)
        If _grid.IsCurrentCellDirty Then _grid.CommitEdit(DataGridViewDataErrorContexts.Commit)
        _grid.EndEdit()
    End Sub

    Private Function GetSelectedProfile() As FormSettings.GamePathProfile
        If _grid.CurrentRow Is Nothing Then Return Nothing
        Return TryCast(_grid.CurrentRow.DataBoundItem, FormSettings.GamePathProfile)
    End Function

    Private Sub AddButton_Click(sender As Object, e As EventArgs)
        Dim InputDefault As String = If(_profiles.Count = 0, _defaultInputPath, "")
        Dim OutputDefault As String = If(_profiles.Count = 0, _defaultOutputPath, "")
        Using Editor As New GamePathProfileEditorDialog(Nothing, InputDefault, OutputDefault)
            If Editor.ShowDialog(Me) = DialogResult.OK Then
                _profiles.Add(Editor.Profile)
                _grid.ClearSelection()
                Dim NewRowIndex As Integer = _grid.Rows.Count - 1
                If NewRowIndex >= 0 Then
                    _grid.CurrentCell = _grid.Rows(NewRowIndex).Cells(0)
                    _grid.Rows(NewRowIndex).Selected = True
                End If
            End If
        End Using
    End Sub

    Private Sub EditButton_Click(sender As Object, e As EventArgs)
        EditSelectedProfile()
    End Sub

    Private Sub EditSelectedProfile()
        Dim SelectedProfile As FormSettings.GamePathProfile = GetSelectedProfile()
        If SelectedProfile Is Nothing Then
            MessageBox.Show(Me, "Select a game path profile to edit.", "No profile selected", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        Using Editor As New GamePathProfileEditorDialog(SelectedProfile, SelectedProfile.InputPath, SelectedProfile.OutputPath)
            If Editor.ShowDialog(Me) = DialogResult.OK Then
                SelectedProfile.Name = Editor.Profile.Name
                SelectedProfile.InputPath = Editor.Profile.InputPath
                SelectedProfile.OutputPath = Editor.Profile.OutputPath
                _grid.Refresh()
            End If
        End Using
    End Sub

    Private Sub RemoveButton_Click(sender As Object, e As EventArgs)
        Dim SelectedProfile As FormSettings.GamePathProfile = GetSelectedProfile()
        If SelectedProfile Is Nothing Then
            MessageBox.Show(Me, "Select a game path profile to remove.", "No profile selected", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        If MessageBox.Show(Me, "Remove the '" & SelectedProfile.Name & "' game path profile?", "Remove profile", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            _profiles.Remove(SelectedProfile)
        End If
    End Sub

    Private Sub CheckAllButton_Click(sender As Object, e As EventArgs)
        For Each Profile As FormSettings.GamePathProfile In _profiles
            Profile.Enabled = True
        Next
        _grid.Refresh()
    End Sub

    Private Sub UncheckAllButton_Click(sender As Object, e As EventArgs)
        For Each Profile As FormSettings.GamePathProfile In _profiles
            Profile.Enabled = False
        Next
        _grid.Refresh()
    End Sub
End Class

Friend NotInheritable Class GamePathProfileEditorDialog
    Inherits Form

    Private ReadOnly _nameBox As TextBox
    Private ReadOnly _inputPathBox As TextBox
    Private ReadOnly _outputPathBox As TextBox
    Public Property Profile As FormSettings.GamePathProfile

    Public Sub New(ExistingProfile As FormSettings.GamePathProfile, DefaultInputPath As String, DefaultOutputPath As String)
        Text = If(ExistingProfile Is Nothing, "Add game path profile", "Edit game path profile")
        StartPosition = FormStartPosition.CenterParent
        FormBorderStyle = FormBorderStyle.FixedDialog
        ShowInTaskbar = False
        MinimizeBox = False
        MaximizeBox = False
        ClientSize = New Size(760, 190)

        _nameBox = New TextBox With {.Dock = DockStyle.Fill, .Text = If(ExistingProfile Is Nothing, "", ExistingProfile.Name)}
        _inputPathBox = New TextBox With {.Dock = DockStyle.Fill, .Text = If(ExistingProfile Is Nothing, DefaultInputPath, ExistingProfile.InputPath)}
        _outputPathBox = New TextBox With {.Dock = DockStyle.Fill, .Text = If(ExistingProfile Is Nothing, DefaultOutputPath, ExistingProfile.OutputPath)}

        Dim Layout As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 3,
            .RowCount = 4,
            .Padding = New Padding(12)
        }
        Layout.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 112.0!))
        Layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0!))
        Layout.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 92.0!))
        Layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 34.0!))
        Layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 38.0!))
        Layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 38.0!))
        Layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0!))

        Layout.Controls.Add(CreateFieldLabel("Profile name:"), 0, 0)
        Layout.Controls.Add(_nameBox, 1, 0)
        Layout.SetColumnSpan(_nameBox, 2)
        Layout.Controls.Add(CreateFieldLabel("Input folder:"), 0, 1)
        Layout.Controls.Add(_inputPathBox, 1, 1)
        Layout.Controls.Add(CreateBrowseButton("Browse input folder", _inputPathBox), 2, 1)
        Layout.Controls.Add(CreateFieldLabel("Output folder:"), 0, 2)
        Layout.Controls.Add(_outputPathBox, 1, 2)
        Layout.Controls.Add(CreateBrowseButton("Browse output folder", _outputPathBox), 2, 2)

        Dim ButtonPanel As New FlowLayoutPanel With {
            .Dock = DockStyle.Fill,
            .FlowDirection = FlowDirection.LeftToRight,
            .WrapContents = False
        }
        Dim OkButton As New Button With {.Text = "OK", .Width = 86, .Height = 30, .DialogResult = DialogResult.None, .UseVisualStyleBackColor = True}
        Dim CancelButton As New Button With {.Text = "Cancel", .Width = 86, .Height = 30, .DialogResult = DialogResult.Cancel, .UseVisualStyleBackColor = True}
        ButtonPanel.Controls.Add(OkButton)
        ButtonPanel.Controls.Add(CancelButton)
        Layout.Controls.Add(ButtonPanel, 0, 3)
        Layout.SetColumnSpan(ButtonPanel, 3)
        Controls.Add(Layout)

        AddHandler OkButton.Click, AddressOf OkButton_Click
        AcceptButton = OkButton
        Me.CancelButton = CancelButton
    End Sub

    Private Shared Function CreateFieldLabel(Caption As String) As Label
        Return New Label With {
            .AutoSize = True,
            .Dock = DockStyle.Fill,
            .Text = Caption,
            .TextAlign = ContentAlignment.MiddleLeft
        }
    End Function

    Private Shared Function CreateBrowseButton(Description As String, Target As TextBox) As Button
        Dim BrowseButton As New Button With {
            .Dock = DockStyle.Fill,
            .Text = "Browse...",
            .UseVisualStyleBackColor = True
        }
        AddHandler BrowseButton.Click,
            Sub(sender As Object, e As EventArgs)
                Using FolderPicker As New FolderBrowserDialog
                    FolderPicker.Description = Description
                    If Directory.Exists(Target.Text) Then FolderPicker.SelectedPath = Target.Text
                    If FolderPicker.ShowDialog() = DialogResult.OK Then Target.Text = FolderPicker.SelectedPath
                End Using
            End Sub
        Return BrowseButton
    End Function

    Private Sub OkButton_Click(sender As Object, e As EventArgs)
        Dim InputPath As String = _inputPathBox.Text.Trim()
        Dim OutputPath As String = _outputPathBox.Text.Trim()
        If Not Directory.Exists(InputPath) Then
            MessageBox.Show(Me, "Choose an existing input folder.", "Invalid input folder", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            _inputPathBox.Focus()
            Return
        End If
        If Not Directory.Exists(OutputPath) Then
            MessageBox.Show(Me, "Choose an existing output folder.", "Invalid output folder", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            _outputPathBox.Focus()
            Return
        End If

        Dim ProfileName As String = _nameBox.Text.Trim()
        If String.IsNullOrWhiteSpace(ProfileName) Then
            Dim TrimmedPath As String = InputPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            ProfileName = Path.GetFileName(TrimmedPath)
        End If
        If String.IsNullOrWhiteSpace(ProfileName) Then ProfileName = "Game"

        Profile = New FormSettings.GamePathProfile(ProfileName, InputPath, OutputPath, True)
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Close()
    End Sub
End Class
