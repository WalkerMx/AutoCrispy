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
    Private SpandrelPrecisionBaseWidth As Integer
    Private SpandrelLayoutBaseDpi As Integer = 96
    Private IsUpdatingAutoRouteModelSelectors As Boolean = False
    Private IsUpdatingPrecisionSelection As Boolean
    Private IsResponsiveLayoutReady As Boolean
    Private IsApplyingResponsiveLayout As Boolean
    Private ResponsiveTabMinimumWidth As Integer
    Private ResponsiveTabRightMargin As Integer
    Private ResponsiveSettingsMinimumHeight As Integer
    Private ResponsiveSettingsBottomMargin As Integer
    Private ResponsiveActionBottomMargin As Integer
    Private ResponsivePanelMinimumWidth As Integer
    Private ResponsivePanelRightMargin As Integer
    Private ReadOnly ResponsivePanelMinimumHeights As New Dictionary(Of GroupBox, Integer)
    Private ReadOnly ResponsivePanelBottomMargins As New Dictionary(Of GroupBox, Integer)
    Private AutoRoutePreviewHasResult As Boolean = False
    Private LastChainPreviewTooltip As String = String.Empty
    Private ReadOnly UiToolTip As New ToolTip()
    Private ReadOnly SpandrelTilingBadge As New System.Windows.Forms.Label()
    Private ReadOnly BrowseOpenModelDbButton As New Button()
    Private ReadOnly InstallSpandrelRuntimeButton As New Button()
    Private ReadOnly SpandrelRuntimeSectionLabel As New Label()
    Private ReadOnly SpandrelRuntimeHintLabel As New Label()
    Private ReadOnly GamePathProfiles As New BindingList(Of FormSettings.GamePathProfile)
    Private CurrentRunGamePaths As List(Of FormSettings.GamePathProfile)
    Private CurrentRunTempRoot As String = String.Empty
    Private ProgressScanInFlight As Boolean
    Private ProgressScanCancellation As CancellationTokenSource
    Private WatchDogScanInFlight As Boolean
    Private WatchDogScanCancellation As CancellationTokenSource
    Private PreviewProcessCancellation As CancellationTokenSource
    Private CloseAfterWorkerCancellation As Boolean
    Private FormClosingRequested As Boolean

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
    Private Shared ReadOnly ResumeCheckpointLock As New Object()
    Private ActiveProcesses As New List(Of Process)
    Private ReadOnly ProcessesBeingTerminated As New HashSet(Of Process)
    Private ReadOnly ProcessJobHandles As New Dictionary(Of Process, IntPtr)

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
        Public Property Tiling As String = "unknown"
        Public Property SupportsHalf As Boolean
        Public Property PrecisionSupportKnown As Boolean
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

    Public Class BatchResumeCheckpoint
        Public Property Version As Integer = 2
        Public Property InProgress As New List(Of BatchResumeEntry)
    End Class

    Public Class BatchResumeEntry
        Public Property InputPath As String = String.Empty
        Public Property AttemptId As String = String.Empty
        Public Property OutputExtensions As New List(Of String)
    End Class

    Private Class OverallProgressInfo
        Public Property DoneCount As Integer
        Public Property TotalCount As Integer
        Public Property UnsupportedCount As Integer
        Public Property AlphaFilteredCount As Integer
        Public Property Percent As Integer
        Public Property ErrorMessage As String = String.Empty
    End Class

    Private Class WatchdogScanResult
        Public Property PendingProfiles As New List(Of FormSettings.GamePathProfile)
        Public Property MissingProfileCount As Integer
        Public Property ErrorMessage As String = String.Empty
    End Class

    Private Class ProcessOutputCapture
        Public Property ActiveProcess As Process
        Public Property StartInfo As ProcessStartInfo
        Public Property StandardOutputTask As Task(Of String)
        Public Property StandardErrorTask As Task(Of String)
        Public Property BackendName As String
        Public Property SaveLocation As String
        Public Property InputPath As String
        Public Property StandardOutput As String
        Public Property StandardError As String
        Public Property ExitCode As Integer
    End Class

    <Runtime.InteropServices.StructLayout(Runtime.InteropServices.LayoutKind.Sequential)>
    Private Structure JobObjectBasicLimitInformation
        Public PerProcessUserTimeLimit As Long
        Public PerJobUserTimeLimit As Long
        Public LimitFlags As UInteger
        Public MinimumWorkingSetSize As UIntPtr
        Public MaximumWorkingSetSize As UIntPtr
        Public ActiveProcessLimit As UInteger
        Public Affinity As UIntPtr
        Public PriorityClass As UInteger
        Public SchedulingClass As UInteger
    End Structure

    <Runtime.InteropServices.StructLayout(Runtime.InteropServices.LayoutKind.Sequential)>
    Private Structure JobObjectIoCounters
        Public ReadOperationCount As ULong
        Public WriteOperationCount As ULong
        Public OtherOperationCount As ULong
        Public ReadTransferCount As ULong
        Public WriteTransferCount As ULong
        Public OtherTransferCount As ULong
    End Structure

    <Runtime.InteropServices.StructLayout(Runtime.InteropServices.LayoutKind.Sequential)>
    Private Structure JobObjectExtendedLimitInformation
        Public BasicLimitInformation As JobObjectBasicLimitInformation
        Public IoInfo As JobObjectIoCounters
        Public ProcessMemoryLimit As UIntPtr
        Public JobMemoryLimit As UIntPtr
        Public PeakProcessMemoryUsed As UIntPtr
        Public PeakJobMemoryUsed As UIntPtr
    End Structure

    <Runtime.InteropServices.DllImport("kernel32.dll", CharSet:=Runtime.InteropServices.CharSet.Unicode, SetLastError:=True)>
    Private Shared Function CreateJobObject(lpJobAttributes As IntPtr, lpName As String) As IntPtr
    End Function

    <Runtime.InteropServices.DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function SetInformationJobObject(hJob As IntPtr, JobObjectInfoClass As Integer, lpJobObjectInfo As IntPtr, cbJobObjectInfoLength As UInteger) As Boolean
    End Function

    <Runtime.InteropServices.DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function AssignProcessToJobObject(hJob As IntPtr, hProcess As IntPtr) As Boolean
    End Function

    <Runtime.InteropServices.DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function TerminateJobObject(hJob As IntPtr, uExitCode As UInteger) As Boolean
    End Function

    <Runtime.InteropServices.DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function CloseHandle(hObject As IntPtr) As Boolean
    End Function

#End Region

#Region "Loading"

    Private Async Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
        InitializeSpandrelModelCard()
        InitializeSpandrelRuntimeSetupControl()
        InitializeChainFriendlyUI()
        Application.CurrentCulture = New Globalization.CultureInfo("EN-US")
        PreloadImageList()
        ChainControl = New DragDropList(ChainPreview, 7)
        AddHandler ChainControl.SelectionChanged, AddressOf ChainSelectionChanged
        AddHandler ChainControl.ItemsReordered, AddressOf ChainItemsReordered
        ConfigureResponsiveLayout()
        ApplyResponsiveLayout()
        IsResponsiveLayoutReady = True
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
            Dim PreferredBackendIndex As Integer = -1
            If SupportedSpandrelModels.Count > 0 Then PreferredBackendIndex = GetPreferredSpandrelBackendIndex()
            If PreferredBackendIndex < 0 Then PreferredBackendIndex = 0
            ExeComboBox.SelectedIndex = PreferredBackendIndex
            If SupportedSpandrelModels.Count > 0 Then ReplaceLegacyUpscalerChain()
            SetSettingsWindow()
        End If
        Await RefreshSupportedSpandrelModels(Root)
        ChainControl.DrawList(ChainControl.ListItems)
        UpdateChainAddButtonState()
        WatchDogButton.Select()
        ' Show the current input/output completion immediately, then keep it refreshed.
        ProgressPollTimer.Enabled = True
        ProgressPollTimer_Tick(ProgressPollTimer, EventArgs.Empty)
        If Environment.GetCommandLineArgs.Count > 1 Then
            WatchDogButton_Click(sender, e)
        End If
    End Sub

    Private Sub ConfigureResponsiveLayout()
        Dim InitialClientSize As Size = ClientSize
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable
        MaximizeBox = True
        ClientSize = InitialClientSize
        ResponsiveTabMinimumWidth = TabGroup.Width
        ResponsiveTabRightMargin = ClientSize.Width - TabGroup.Right
        ResponsiveSettingsMinimumHeight = SettingsGroup.Height
        ResponsiveSettingsBottomMargin = ClientSize.Height - SettingsGroup.Bottom
        ResponsiveActionBottomMargin = ClientSize.Height - RunOnceButton.Bottom
        ResponsivePanelMinimumWidth = PyGroup.Width
        SettingsLoc = New Point(SettingsGroup.Right + 16, SettingsGroup.Top)
        ResponsivePanelRightMargin = ClientSize.Width - SettingsLoc.X - ResponsivePanelMinimumWidth
        ResponsivePanelMinimumHeights.Clear()
        ResponsivePanelBottomMargins.Clear()
        For Each Panel As GroupBox In New GroupBox() {CaffeGroup, VulkanGroup, WaifuCPPGroup, AnimeCPPGroup, DDxGroup, xBRZGroup, PyGroup}
            ResponsivePanelMinimumHeights(Panel) = Panel.Height
            ResponsivePanelBottomMargins(Panel) = ClientSize.Height - SettingsLoc.Y - Panel.Height
        Next
        MinimumSize = Size

        TabGroup.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        SettingsGroup.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left
        RunOnceButton.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        WatchDogButton.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        QueueSummaryLabel.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        QueueActivityLabel.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        UpscaleProgress.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        BackendStatusLabel.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right

        InputTextBox.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        OutputTextBox.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        ExeTextBox.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        InputBrowse.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        OutputBrowse.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        ExeBrowse.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        GamePathsSummaryLabel.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right

        ChainPreview.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        ChainAdd.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        ChainRemove.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        ChainSave.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        ChainLoad.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        ExpertSettingsBox.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        SeamsBox.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        SeamScale.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        SeamMargin.Anchor = AnchorStyles.Top Or AnchorStyles.Right

        ' The model selector and its adjacent Spandrel buttons are positioned together below.
        ' Avoid right-anchor auto-expansion, which can make the selector overlap those buttons.
        PyModel.Anchor = AnchorStyles.Top Or AnchorStyles.Left
        PyNormalMapModeLabel.Anchor = AnchorStyles.Top Or AnchorStyles.Left
        PyNormalMapModeComboBox.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        RefreshSpandrelModelsButton.Anchor = AnchorStyles.Top Or AnchorStyles.Left
        BrowseOpenModelDbButton.Anchor = AnchorStyles.Top Or AnchorStyles.Left
        SpandrelModelInfoLabel.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        SpandrelScanStatusLabel.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        TileSizeHint.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        PyPrecisionLabel.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        PyPrecisionComboBox.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        AutoArchitectModelComboBox.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        AutoPainterModelComboBox.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        AutoRoutePreviewStatusLabel.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right

        For Each Panel As GroupBox In New GroupBox() {CaffeGroup, VulkanGroup, WaifuCPPGroup, AnimeCPPGroup, DDxGroup, xBRZGroup, PyGroup}
            Panel.Anchor = AnchorStyles.Top Or AnchorStyles.Left
        Next
    End Sub

    Private Sub ApplyResponsiveLayout()
        If IsApplyingResponsiveLayout OrElse ClientSize.Width <= 0 OrElse ClientSize.Height <= 0 Then Return
        IsApplyingResponsiveLayout = True
        Try
            Dim ClientWidth As Integer = ClientSize.Width
            Dim ClientHeight As Integer = ClientSize.Height
            TabGroup.Width = Math.Max(ResponsiveTabMinimumWidth, ClientWidth - TabGroup.Left - ResponsiveTabRightMargin)
            SettingsGroup.Height = Math.Max(ResponsiveSettingsMinimumHeight, ClientHeight - SettingsGroup.Top - ResponsiveSettingsBottomMargin)
            RunOnceButton.Top = ClientHeight - ResponsiveActionBottomMargin - RunOnceButton.Height
            WatchDogButton.Top = ClientHeight - ResponsiveActionBottomMargin - WatchDogButton.Height

            SettingsLoc = New Point(SettingsGroup.Right + 16, SettingsGroup.Top)
            Dim PanelWidth As Integer = Math.Max(ResponsivePanelMinimumWidth, ClientWidth - SettingsLoc.X - ResponsivePanelRightMargin)
            For Each Panel As GroupBox In New GroupBox() {CaffeGroup, VulkanGroup, WaifuCPPGroup, AnimeCPPGroup, DDxGroup, xBRZGroup, PyGroup}
                Dim MinimumPanelHeight As Integer = ResponsivePanelMinimumHeights(Panel)
                Dim PanelBottomMargin As Integer = ResponsivePanelBottomMargins(Panel)
                Panel.Location = SettingsLoc
                Panel.Size = New Size(PanelWidth, Math.Max(MinimumPanelHeight, ClientHeight - SettingsLoc.Y - PanelBottomMargin))
            Next

            Dim IsSpandrelSelected As Boolean = String.Equals(If(ExeComboBox.SelectedItem, "").ToString(), SpandrelBackendName, StringComparison.OrdinalIgnoreCase)
            LayoutSpandrelModelCard(IsSpandrelSelected)
            LayoutOpenModelDbControls()
            LayoutSpandrelResponsiveControls()
        Finally
            IsApplyingResponsiveLayout = False
        End Try
    End Sub

    Private Sub Form1_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        If Not IsResponsiveLayoutReady OrElse IsApplyingResponsiveLayout Then Return
        ApplyResponsiveLayout()
    End Sub

    Private Sub Form1_DpiChanged(sender As Object, e As DpiChangedEventArgs) Handles MyBase.DpiChanged
        If Not IsResponsiveLayoutReady Then Return
        ' WinForms scales controls automatically; refresh the runtime button and cached margins at the new DPI.
        LayoutSpandrelRuntimeSetupControl()
        ConfigureResponsiveLayout()
        ApplyResponsiveLayout()
    End Sub

    Private Sub Form1_Closing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If WorkHorse.IsBusy OrElse SpandrelScanCancellation IsNot Nothing OrElse
            PreviewProcessCancellation IsNot Nothing OrElse ProgressScanCancellation IsNot Nothing OrElse
            WatchDogScanCancellation IsNot Nothing Then
            e.Cancel = True
            FormClosingRequested = True
            CloseAfterWorkerCancellation = True
            WatchDog.Stop()
            ProgressPollTimer.Stop()
            SwitchGroups(False)
            WatchDogButton.Enabled = False
            AutoRoutePreviewButton.Enabled = False
            RefreshSpandrelModelsButton.Enabled = False
            If WorkHorse.IsBusy Then
                QueueActivityLabel.Text = "Cancelling active work before closing…"
                WorkHorse.CancelAsync()
                StopActiveProcesses()
            End If
            If SpandrelScanCancellation IsNot Nothing Then
                Try
                    SpandrelScanCancellation.Cancel()
                Catch ex As ObjectDisposedException
                    ' The model scan already finished.
                End Try
            End If
            If PreviewProcessCancellation IsNot Nothing Then
                Try
                    PreviewProcessCancellation.Cancel()
                Catch ex As ObjectDisposedException
                    ' The route preview already finished.
                End Try
            End If
            If ProgressScanCancellation IsNot Nothing Then
                Try
                    ProgressScanCancellation.Cancel()
                Catch ex As ObjectDisposedException
                    ' The progress scan already finished.
                End Try
            End If
            If WatchDogScanCancellation IsNot Nothing Then
                Try
                    WatchDogScanCancellation.Cancel()
                Catch ex As ObjectDisposedException
                    ' The watcher scan already finished.
                End Try
            End If
            TryCompleteDeferredClose()
            Return
        End If

        If PortableCheckBox.Checked = True Then
            File.WriteAllText(Root & "\portable.xml", Serialize(New FormSettings.Settings(Me)))
        Else
            File.WriteAllText(AppData & "\AutoCrispy\settings.xml", Serialize(New FormSettings.Settings(Me)))
        End If
        UiToolTip.Dispose()
    End Sub

    Private Sub TryCompleteDeferredClose()
        If Not CloseAfterWorkerCancellation OrElse WorkHorse.IsBusy OrElse
            SpandrelScanCancellation IsNot Nothing OrElse PreviewProcessCancellation IsNot Nothing OrElse
            ProgressScanCancellation IsNot Nothing OrElse WatchDogScanCancellation IsNot Nothing OrElse
            IsDisposed OrElse Not IsHandleCreated Then Return
        CloseAfterWorkerCancellation = False
        BeginInvoke(New MethodInvoker(AddressOf Me.Close))
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

    Friend Shared Function DoGamePathsOverlap(InputPath As String, OutputPath As String) As Boolean
        Dim NormalizedInputPath As String = NormalizeFolderPath(InputPath)
        Dim NormalizedOutputPath As String = NormalizeFolderPath(OutputPath)
        Return IsSameOrNestedFolder(NormalizedInputPath, NormalizedOutputPath) OrElse
            IsSameOrNestedFolder(NormalizedOutputPath, NormalizedInputPath)
    End Function

    Private Shared Function NormalizeFolderPath(FolderPath As String) As String
        Dim FullPath As String = Path.GetFullPath(FolderPath)
        Dim RootPath As String = Path.GetPathRoot(FullPath)
        If Not String.Equals(FullPath, RootPath, StringComparison.OrdinalIgnoreCase) Then
            FullPath = FullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
        End If
        Return FullPath
    End Function

    Private Shared Function IsSameOrNestedFolder(CandidatePath As String, ParentPath As String) As Boolean
        If String.Equals(CandidatePath, ParentPath, StringComparison.OrdinalIgnoreCase) Then Return True
        Dim ParentPrefix As String = ParentPath
        If Not ParentPrefix.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) AndAlso
            Not ParentPrefix.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal) Then
            ParentPrefix &= Path.DirectorySeparatorChar
        End If
        Return CandidatePath.StartsWith(ParentPrefix, StringComparison.OrdinalIgnoreCase)
    End Function

    Private Shared Function GetInputFiles(InputPath As String, OutputPath As String) As String()
        If DoGamePathsOverlap(InputPath, OutputPath) Then
            Throw New InvalidOperationException(
                "Game input and output folders must be separate and must not contain one another.")
        End If
        Return Directory.GetFiles(InputPath, "*.*", SearchOption.AllDirectories)
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
        If FindPythonExecutable() <> "" AndAlso Not ExeComboBox.Items.Contains(SpandrelBackendName) Then
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
        BrowseOpenModelDbButton.Enabled = False
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
            SetupProblem = "Python was not found. Use Advanced > Install / repair… to see setup options, or see PLKSR_SETUP.md."
        ElseIf Not File.Exists(RunnerPath) Then
            SetupProblem = "The Spandrel runner was not found beside AutoCrispy."
        End If
        If SetupProblem <> "" Then
            SetModelScanStatus(SetupProblem)
            If ScanGeneration = SpandrelScanGeneration AndAlso Not IsDisposed Then
                WatchDogButton.Enabled = True
                RefreshSpandrelModelsButton.Enabled = True
                BrowseOpenModelDbButton.Enabled = True
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
                SetModelScanStatus("No compatible checkpoints found (need 1× RGB restoration or 4× RGB SR). If Python packages are missing, use Advanced > Install / repair…")
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
                If Not IsDisposed Then BrowseOpenModelDbButton.Enabled = True
            End If
            ScanCancellation.Dispose()
            TryCompleteDeferredClose()
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
                        Task.Run(Sub() TerminateProcessTree(ScanProcess))
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
                                        If ModelFields.Length >= 7 Then Model.Tiling = ModelFields(6)
                                        If ModelFields.Length >= 8 Then
                                            Model.SupportsHalf = String.Equals(ModelFields(7), "true", StringComparison.OrdinalIgnoreCase)
                                            Model.PrecisionSupportKnown = True
                                        End If
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
                    PythonSettings = New FormSettings.PythonPackage(PreferredModel, PythonSettings.TileSize, PythonSettings.CPUOnly, True,
                        _Precision:=PythonSettings.Precision, _NormalMapMode:=PythonSettings.NormalMapMode)
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
            UpdateChainAddButtonState()
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

    Private Sub InitializeChainFriendlyUI()
        ChainGroup.Text = "Processing chain"

        Dim PreviewBottomMargin As Integer = Math.Max(7, ChainGroup.ClientSize.Height - ChainPreview.Bottom)
        ChainPreview.Top = 8
        ChainPreview.Height = Math.Max(90, ChainGroup.ClientSize.Height - ChainPreview.Top - PreviewBottomMargin)

        Dim ButtonWidth As Integer = 112
        Dim ButtonGap As Integer = 10
        Dim RightMargin As Integer = 12
        Dim ButtonLeft As Integer = ChainGroup.ClientSize.Width - RightMargin - ButtonWidth
        For Each ChainButton As Button In New Button() {ChainAdd, ChainRemove, ChainSave, ChainLoad}
            ChainButton.Width = ButtonWidth
            ChainButton.Left = ButtonLeft
            ChainButton.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        Next
        ChainPreview.Width = Math.Max(180, ButtonLeft - ButtonGap - ChainPreview.Left)

        ChainAdd.Text = "Add step"
        ChainRemove.Text = "Remove step"
        ChainRemove.Enabled = False
        ChainSave.Text = "Save chain"
        ChainLoad.Text = "Load chain"
        ChainContextEdit.Text = "Advanced: edit XML…"
        ChainContextDelete.Text = "Remove step"
        UiToolTip.SetToolTip(ChainAdd, "Add the selected backend and its current settings as the next processing step.")
        UiToolTip.SetToolTip(ChainRemove, "Click a numbered step first, then remove it from the chain.")
        UiToolTip.SetToolTip(ChainSave, "Save this processing chain to a file.")
        UiToolTip.SetToolTip(ChainLoad, "Replace the current chain with a saved chain.")
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
            Dim PreferredBackendIndex As Integer = -1
            If SupportedSpandrelModels.Count > 0 Then PreferredBackendIndex = GetPreferredSpandrelBackendIndex()
            If PreferredBackendIndex < 0 Then PreferredBackendIndex = 0
            ExeComboBox.SelectedIndex = PreferredBackendIndex
            If SupportedSpandrelModels.Count > 0 Then ReplaceLegacyUpscalerChain()
            SetSettingsWindow()
        End If
        Await RefreshSupportedSpandrelModels(Root)
    End Sub

    Private Sub DefringeCheck_CheckedChanged(sender As Object, e As EventArgs) Handles DefringeCheck.CheckedChanged
        DefringeThresh.Enabled = DefringeCheck.Checked
    End Sub

    Private Sub ChainSave_Click(sender As Object, e As EventArgs) Handles ChainSave.Click
        If ChainList Is Nothing OrElse ChainList.Count = 0 Then Return
        Using SFD As New SaveFileDialog With {
            .Filter = "AutoCrispy chain (*.xml)|*.xml|All files (*.*)|*.*",
            .Title = "Save processing chain",
            .DefaultExt = "xml",
            .AddExtension = True,
            .FileName = "AutoCrispy chain.xml"
        }
            If SFD.ShowDialog(Me) <> DialogResult.OK Then Return
            Try
                File.WriteAllText(SFD.FileName, Serialize(ChainList))
                UiToolTip.SetToolTip(ChainSave, "Saved " & ChainList.Count.ToString() & " step(s) to " & Path.GetFileName(SFD.FileName) & ".")
            Catch ex As Exception
                MessageBox.Show(Me, "The chain could not be saved: " & ex.GetBaseException().Message,
                                "Save chain failed", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Using
    End Sub

    Private Sub ChainLoad_Click(sender As Object, e As EventArgs) Handles ChainLoad.Click
        Using OFD As New OpenFileDialog With {
            .Filter = "AutoCrispy chain (*.xml)|*.xml|All files (*.*)|*.*",
            .Title = "Load processing chain",
            .CheckFileExists = True
        }
            If OFD.ShowDialog(Me) <> DialogResult.OK Then Return
            Try
                Dim LoadedChain As List(Of FormSettings.ChainObject) =
                    Deserialize(Of List(Of FormSettings.ChainObject))(File.ReadAllText(OFD.FileName))
                If LoadedChain Is Nothing Then Throw New InvalidDataException("The selected file does not contain a processing chain.")
                For i As Integer = 0 To LoadedChain.Count - 1
                    Dim ChainItem As FormSettings.ChainObject = LoadedChain(i)
                    If ChainItem.IconIndex < 0 OrElse ChainItem.IconIndex >= ChainThumbs.Count Then
                        Throw New InvalidDataException("Step " & (i + 1).ToString() & " has an unsupported icon index.")
                    End If
                    If String.IsNullOrWhiteSpace(ChainItem.Name) Then
                        ChainItem.Name = If(String.IsNullOrWhiteSpace(ChainItem.PackageType), "Processing step", ChainItem.PackageType)
                        LoadedChain(i) = ChainItem
                    End If
                Next

                If ChainList IsNot Nothing AndAlso ChainList.Count > 0 AndAlso
                    MessageBox.Show(Me, "Replace the current " & ChainList.Count.ToString() & "-step chain with " &
                                    LoadedChain.Count.ToString() & " step(s) from " & Path.GetFileName(OFD.FileName) & "?",
                                    "Replace current chain?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return

                ChainList = LoadedChain
                ChainControl.ListItems.Clear()
                For i As Integer = 0 To ChainList.Count - 1
                    Dim ChainItem As FormSettings.ChainObject = ChainList(i)
                    ChainControl.ListItems.Add(New DragDropList.DragDropItem(i, ChainItem.Name, ChainThumbs(ChainItem.IconIndex)))
                Next
                ChainControl.ClearSelection()
                If GetPreferredSpandrelBackendIndex() >= 0 Then ReplaceLegacyUpscalerChain()
                ChainControl.DrawList(ChainControl.ListItems)
                UpdateChainAddButtonState()
            Catch ex As Exception
                MessageBox.Show(Me, "The chain could not be loaded. Your current chain was left unchanged." &
                                Environment.NewLine & ex.GetBaseException().Message,
                                "Load chain failed", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Using
    End Sub

    Private Function FindLatestSpandrelChainItemIndex() As Integer
        If ChainList Is Nothing Then Return -1
        For i As Integer = ChainList.Count - 1 To 0 Step -1
            If String.Equals(ChainList(i).PackageType, SpandrelBackendName, StringComparison.OrdinalIgnoreCase) Then Return i
        Next
        Return -1
    End Function

    Private Sub ChainSelectionChanged(sender As Object, e As EventArgs)
        UpdateChainAddButtonState()
    End Sub

    Private Sub UpdateChainAddButtonState()
        If ChainList Is Nothing Then ChainList = New List(Of FormSettings.ChainObject)
        Dim IsSpandrelSelected As Boolean = String.Equals(If(ExeComboBox.SelectedItem, "").ToString(), SpandrelBackendName, StringComparison.OrdinalIgnoreCase)
        Dim ExistingSpandrelIndex As Integer = FindLatestSpandrelChainItemIndex()
        If IsSpandrelSelected AndAlso ExistingSpandrelIndex >= 0 Then
            ChainAdd.Text = "Update step"
            UiToolTip.SetToolTip(ChainAdd,
                "Update the latest Spandrel step with these settings. Hold Shift while clicking to append another Spandrel step.")
        Else
            ChainAdd.Text = "Add step"
            UiToolTip.SetToolTip(ChainAdd, "Add the selected backend and its current settings as the next processing step.")
        End If

        Dim SelectedIndex As Integer = If(ChainControl Is Nothing, -1, ChainControl.SelectedIndex)
        ChainRemove.Enabled = SelectedIndex >= 0 AndAlso SelectedIndex < ChainList.Count
        ChainSave.Enabled = ChainList.Count > 0
    End Sub

    Private Sub ChainAdd_Click(sender As Object, e As EventArgs) Handles ChainAdd.Click
        Dim ForceAppend As Boolean = (Control.ModifierKeys And Keys.Shift) = Keys.Shift
        AddModelToChain(ExeComboBox.SelectedItem, ForceAppend)
        UpdateChainAddButtonState()
    End Sub

    Private Sub ChainContext_Opening(sender As Object, e As CancelEventArgs) Handles ChainContext.Opening
        Dim SelectedIndex As Integer = If(ChainControl Is Nothing, -1, ChainControl.SelectedIndex)
        Dim ChainCount As Integer = If(ChainList Is Nothing, 0, ChainList.Count)
        Dim HasSelectedStep As Boolean = SelectedIndex >= 0 AndAlso SelectedIndex < ChainCount
        ChainContextEdit.Enabled = HasSelectedStep
        ChainContextDelete.Enabled = HasSelectedStep
        e.Cancel = Not HasSelectedStep
    End Sub

    Private Sub RemoveItemFromChain(sender As Object, e As EventArgs) Handles ChainContextDelete.Click, ChainRemove.Click
        If ChainControl Is Nothing Then Return
        Dim Remove As Integer = ChainControl.SelectedIndex
        If Remove < 0 OrElse Remove >= ChainList.Count Then Return
        Dim StepName As String = If(String.IsNullOrWhiteSpace(ChainList(Remove).Name), "this step", ChainList(Remove).Name)
        If MessageBox.Show(Me, "Remove " & StepName & " from the processing chain?", "Remove chain step",
                           MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return

        ChainList.RemoveAt(Remove)
        ChainControl.ListItems.RemoveAt(Remove)
        ChainControl.ReorderList()
        ChainControl.ClearSelection()
        ChainControl.DrawList(ChainControl.ListItems)
        UpdateChainAddButtonState()
    End Sub

    Private Sub ChainContextEdit_Click(sender As Object, e As EventArgs) Handles ChainContextEdit.Click
        If ChainControl Is Nothing Then Return
        Dim ItemIndex As Integer = ChainControl.SelectedIndex
        If ItemIndex < 0 OrElse ItemIndex >= ChainList.Count Then Return
        Dim StepName As String = If(String.IsNullOrWhiteSpace(ChainList(ItemIndex).Name), "this step", ChainList(ItemIndex).Name)
        Dim AdvancedWarning As String = "This opens the advanced raw-XML editor for " & StepName & "." & Environment.NewLine &
            "It explains the step's on/off options. For normal changes, use the main settings panel; other XML edits can break the chain."
        If MessageBox.Show(Me, AdvancedWarning, "Advanced chain editor", MessageBoxButtons.OKCancel,
                           MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) <> DialogResult.OK Then Return

        Try
            Using ECD As New EditChainDialog(Serialize(ChainList(ItemIndex)), ChainList(ItemIndex).PackageType)
                If ECD.ShowDialog(Me) = DialogResult.OK Then
                    Dim NewChainItem As FormSettings.ChainObject = Deserialize(Of FormSettings.ChainObject)(ECD.ResultText)
                    If NewChainItem.IconIndex < 0 OrElse NewChainItem.IconIndex >= ChainThumbs.Count Then
                        Throw New InvalidDataException("The icon index is outside the supported range.")
                    End If
                    ChainList(ItemIndex) = NewChainItem
                    ChainControl.ListItems(ItemIndex) = New DragDropList.DragDropItem(
                        ItemIndex, NewChainItem.Name, ChainThumbs(NewChainItem.IconIndex))
                    ChainControl.DrawList(ChainControl.ListItems)
                End If
            End Using
        Catch ex As Exception
            MessageBox.Show(Me, "The chain step could not be updated: " & ex.GetBaseException().Message,
                            "Invalid chain settings", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
        UpdateChainAddButtonState()
    End Sub

    Private Sub ChainItemsReordered(sender As Object, e As EventArgs)
        If ChainControl Is Nothing OrElse ChainList Is Nothing Then Return
        Dim ReorderedChain As New List(Of FormSettings.ChainObject)
        For Each Item As DragDropList.DragDropItem In ChainControl.ListItems
            If Item.Index < 0 OrElse Item.Index >= ChainList.Count Then Return
            ReorderedChain.Add(ChainList(Item.Index))
        Next
        If ReorderedChain.Count = ChainList.Count Then ChainList = ReorderedChain
        UpdateChainAddButtonState()
    End Sub

    Private Sub ChainPreview_MouseUp(sender As Object, e As MouseEventArgs) Handles ChainPreview.MouseUp
        UpdateChainAddButtonState()
    End Sub

    Private Sub ChainPreview_MouseMove(sender As Object, e As MouseEventArgs) Handles ChainPreview.MouseMove
        If ChainControl Is Nothing Then Return
        Dim ItemName As String = ChainControl.GetItemNameAt(e.Location)
        If String.Equals(ItemName, LastChainPreviewTooltip, StringComparison.Ordinal) Then Return
        LastChainPreviewTooltip = ItemName
        UiToolTip.SetToolTip(ChainPreview, ItemName)
    End Sub

    Private Sub ChainPreview_MouseLeave(sender As Object, e As EventArgs) Handles ChainPreview.MouseLeave
        LastChainPreviewTooltip = String.Empty
        UiToolTip.SetToolTip(ChainPreview, String.Empty)
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
        If WatcherWasEnabled Then WatchDog.Enabled = False
        Dim RunStarted As Boolean = False
        Try
            Using OFD As New OpenFileDialog With {.Filter = "Image Files|*.png;*.jpg;*.bmp"}
                If OFD.ShowDialog() <> DialogResult.OK Then Return
                Using SFD As New SaveFileDialog With {.Filter = "PNG Images|*.png"}
                    If SFD.ShowDialog() <> DialogResult.OK Then Return

                    CurrentRunTempRoot = CreateRunTempRoot()
                    Dim TempPath As String = Path.Combine(CurrentRunTempRoot, "input")
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
            If Not RunStarted Then CleanupRunTemporaryRoot()
            If WatcherWasEnabled AndAlso Not RunStarted AndAlso WatchDogButton.Text = "Running: True" Then
                WatchDog.Enabled = True
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
            If DoGamePathsOverlap(Profile.InputPath, Profile.OutputPath) Then
                MsgBox("Input and output folders must be separate and must not contain one another for game profile '" &
                       Profile.Name & "'. Choose two non-overlapping folders to prevent recursive reprocessing.",
                       MsgBoxStyle.Critical, "Overlapping game paths")
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
        UpdateChainAddButtonState()
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

    Private Sub InitializeSpandrelModelCard()
        SpandrelPrecisionBaseWidth = PyPrecisionComboBox.Width
        SpandrelLayoutBaseDpi = Math.Max(1, PyGroup.DeviceDpi)
        SpandrelTilingBadge.Name = "SpandrelTilingBadge"
        SpandrelTilingBadge.AccessibleName = "Model tiling policy"
        SpandrelTilingBadge.AutoEllipsis = True
        SpandrelTilingBadge.BorderStyle = BorderStyle.FixedSingle
        SpandrelTilingBadge.Cursor = Cursors.Help
        SpandrelTilingBadge.Font = New System.Drawing.Font("Segoe UI", 8.25!, FontStyle.Bold)
        SpandrelTilingBadge.Padding = New Padding(4, 0, 4, 0)
        SpandrelTilingBadge.TextAlign = ContentAlignment.MiddleCenter
        SpandrelTilingBadge.Visible = False
        PyGroup.Controls.Add(SpandrelTilingBadge)
        SpandrelTilingBadge.BringToFront()

        BrowseOpenModelDbButton.Name = "BrowseOpenModelDbButton"
        BrowseOpenModelDbButton.Text = "Catalog…"
        BrowseOpenModelDbButton.AccessibleName = "Browse OpenModelDB compatible models"
        BrowseOpenModelDbButton.Size = New Size(112, 30)
        BrowseOpenModelDbButton.UseVisualStyleBackColor = True
        BrowseOpenModelDbButton.Visible = False
        PyGroup.Controls.Add(BrowseOpenModelDbButton)
        BrowseOpenModelDbButton.BringToFront()
        AddHandler BrowseOpenModelDbButton.Click, AddressOf BrowseOpenModelDbButton_Click

        SpandrelModelInfoLabel.BorderStyle = BorderStyle.FixedSingle
        SpandrelModelInfoLabel.BackColor = System.Drawing.Color.FromArgb(245, 247, 250)
        SpandrelModelInfoLabel.Padding = New Padding(6, 0, 6, 0)
        SpandrelModelInfoLabel.TextAlign = ContentAlignment.MiddleLeft
        LayoutSpandrelModelCard(False)
        AddHandler PyPrecisionComboBox.SelectedIndexChanged, AddressOf PyPrecisionComboBox_SelectedIndexChanged
        UiToolTip.SetToolTip(Label25, "Maximum input tile edge in pixels. Set to 0 to try a full image first.")
        UiToolTip.SetToolTip(PyNormalMapModeLabel, "Select a normal-map post-process only when the input textures are tangent-space normal maps.")
        UiToolTip.SetToolTip(PyNormalMapModeComboBox,
            "Off leaves RGB unchanged. Normalize XYZ decodes and normalizes all three vector channels. " &
            "Rebuild Z ignores blue and reconstructs positive Z from red/green (for BC5/RG or RG0 maps).")
    End Sub

    Private Sub InitializeSpandrelRuntimeSetupControl()
        SpandrelRuntimeSectionLabel.AutoSize = True
        SpandrelRuntimeSectionLabel.Location = New Point(208, 132)
        SpandrelRuntimeSectionLabel.Name = "SpandrelRuntimeSectionLabel"
        SpandrelRuntimeSectionLabel.Text = "Model runtime:"
        AdvSettingsGroup.Controls.Add(SpandrelRuntimeSectionLabel)

        InstallSpandrelRuntimeButton.AccessibleName = "Install or repair the Spandrel Python runtime"
        InstallSpandrelRuntimeButton.Location = New Point(322, 124)
        InstallSpandrelRuntimeButton.Name = "InstallSpandrelRuntimeButton"
        InstallSpandrelRuntimeButton.Size = New Size(196, 32)
        InstallSpandrelRuntimeButton.TabIndex = 18
        InstallSpandrelRuntimeButton.Text = "Install / repair…"
        InstallSpandrelRuntimeButton.UseVisualStyleBackColor = True
        AddHandler InstallSpandrelRuntimeButton.Click, AddressOf InstallSpandrelRuntimeButton_Click
        AdvSettingsGroup.Controls.Add(InstallSpandrelRuntimeButton)

        SpandrelRuntimeHintLabel.AutoEllipsis = True
        SpandrelRuntimeHintLabel.Location = New Point(208, 160)
        SpandrelRuntimeHintLabel.Name = "SpandrelRuntimeHintLabel"
        SpandrelRuntimeHintLabel.Size = New Size(356, 20)
        SpandrelRuntimeHintLabel.Text = "Python 3.10+ · PyTorch / TorchVision · Spandrel"
        SpandrelRuntimeHintLabel.ForeColor = SystemColors.GrayText
        AdvSettingsGroup.Controls.Add(SpandrelRuntimeHintLabel)

        UiToolTip.SetToolTip(InstallSpandrelRuntimeButton,
            "Check the Python AutoCrispy will use, then install or repair Spandrel, PyTorch, TorchVision, NumPy and Pillow.")
        UiToolTip.SetToolTip(SpandrelRuntimeHintLabel,
            "The Python packages are separate from model checkpoint weights. Use the PyTorch guide in the setup dialog for an NVIDIA CUDA build.")
        LayoutSpandrelRuntimeSetupControl()
    End Sub

    Private Sub LayoutSpandrelRuntimeSetupControl()
        Dim DpiScale As Double = Math.Max(1, AdvSettingsGroup.DeviceDpi) / 96.0
        SpandrelRuntimeSectionLabel.Location = New Point(CInt(Math.Round(208 * DpiScale)), CInt(Math.Round(132 * DpiScale)))
        InstallSpandrelRuntimeButton.Location = New Point(CInt(Math.Round(322 * DpiScale)), CInt(Math.Round(124 * DpiScale)))
        InstallSpandrelRuntimeButton.Size = New Size(CInt(Math.Round(196 * DpiScale)), CInt(Math.Round(32 * DpiScale)))
        SpandrelRuntimeHintLabel.Location = New Point(CInt(Math.Round(208 * DpiScale)), CInt(Math.Round(160 * DpiScale)))
        SpandrelRuntimeHintLabel.Size = New Size(CInt(Math.Round(356 * DpiScale)), CInt(Math.Round(20 * DpiScale)))
    End Sub

    Private Async Sub InstallSpandrelRuntimeButton_Click(sender As Object, e As EventArgs)
        If WorkHorse.IsBusy OrElse PreviewProcessCancellation IsNot Nothing Then
            MessageBox.Show(Me, "Wait for the current model preview or processing job to finish before changing Python packages.",
                            "Python runtime is busy", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        If SpandrelScanCancellation IsNot Nothing Then
            MessageBox.Show(Me, "Wait for the Spandrel model scan to finish before changing Python packages.",
                            "Model scan is busy", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim PythonExecutable As String = FindPythonExecutable()
        Using SetupDialog As New SpandrelRuntimeSetupDialog(PythonExecutable)
            If SetupDialog.ShowDialog(Me) <> DialogResult.OK OrElse Not SetupDialog.SetupCompleted Then Return
        End Using

        Await RefreshSupportedSpandrelModels(Root)
    End Sub

    Private Sub LayoutSpandrelModelCard(ShowTilingBadge As Boolean)
        Dim LeftInset As Integer = SpandrelModelInfoLabel.Left
        Dim RightInset As Integer = LeftInset + 6
        Dim AvailableWidth As Integer = Math.Max(100, PyGroup.ClientSize.Width - LeftInset - RightInset)
        SpandrelTilingBadge.Visible = ShowTilingBadge
        If ShowTilingBadge Then
            Dim BadgeWidth As Integer = Math.Min(220, Math.Max(175, CInt(AvailableWidth * 0.34)))
            SpandrelModelInfoLabel.Width = Math.Max(180, AvailableWidth - BadgeWidth - 8)
            SpandrelTilingBadge.Location = New Point(
                SpandrelModelInfoLabel.Right + 8,
                SpandrelModelInfoLabel.Top + (SpandrelModelInfoLabel.Height - Math.Max(20, SpandrelModelInfoLabel.Height - 10)) \ 2
            )
            SpandrelTilingBadge.Size = New Size(BadgeWidth, Math.Max(20, SpandrelModelInfoLabel.Height - 10))
        Else
            SpandrelModelInfoLabel.Width = AvailableWidth
        End If
    End Sub

    Private Sub LayoutOpenModelDbControls()
        If PyGroup Is Nothing OrElse PyGroup.ClientSize.Width <= 0 Then Return
        Dim ClientWidth As Integer = PyGroup.ClientSize.Width
        Dim RightMargin As Integer = Math.Max(5, PyGroup.Padding.Right + 2)
        Dim ButtonGap As Integer = 7
        RefreshSpandrelModelsButton.Width = 94
        RefreshSpandrelModelsButton.Left = ClientWidth - RightMargin - RefreshSpandrelModelsButton.Width
        BrowseOpenModelDbButton.Width = 112
        BrowseOpenModelDbButton.Height = 30
        BrowseOpenModelDbButton.Left = RefreshSpandrelModelsButton.Left - ButtonGap - BrowseOpenModelDbButton.Width
        BrowseOpenModelDbButton.Top = RefreshSpandrelModelsButton.Top

        Dim IsSpandrelSelected As Boolean = String.Equals(
            If(ExeComboBox.SelectedItem, "").ToString(), SpandrelBackendName, StringComparison.OrdinalIgnoreCase)
        If IsSpandrelSelected Then
            ' The selector list must not extend beneath the adjacent Catalog/Refresh buttons.
            PyModel.Width = Math.Max(1, BrowseOpenModelDbButton.Left - PyModel.Left - ButtonGap)
        Else
            PyModel.Width = Math.Max(1, ClientWidth - PyModel.Left - RightMargin)
        End If
        PyModel.DropDownWidth = Math.Min(500, Math.Max(1, PyModel.Width))
    End Sub

    Private Sub LayoutSpandrelResponsiveControls()
        If PyGroup Is Nothing OrElse PyGroup.ClientSize.Width <= 0 Then Return

        Dim ClientWidth As Integer = PyGroup.ClientSize.Width
        Dim RightMargin As Integer = Math.Max(4, PyGroup.Padding.Right + 2)
        Dim LabelGap As Integer = Math.Max(4, PyNormalMapModeComboBox.Left - PyNormalMapModeLabel.Right)
        Dim PrecisionLabelWidth As Integer = Math.Max(
            PyPrecisionLabel.Width,
            TextRenderer.MeasureText(PyPrecisionLabel.Text, PyPrecisionLabel.Font).Width
        )
        Dim PreferredPrecisionWidth As Integer = Math.Max(
            1,
            CInt(Math.Round(SpandrelPrecisionBaseWidth * PyGroup.DeviceDpi / CDbl(Math.Max(1, SpandrelLayoutBaseDpi))))
        )
        Dim PrecisionWidth As Integer = Math.Min(
            PreferredPrecisionWidth,
            Math.Max(1, ClientWidth - RightMargin - PrecisionLabelWidth - LabelGap)
        )
        Dim PrecisionLeft As Integer = ClientWidth - RightMargin - PrecisionWidth - LabelGap - PrecisionLabelWidth
        Dim CanKeepPrecisionBesideTileSize As Boolean = PrecisionLeft >= PyCPU.Right + (2 * LabelGap)
        Dim CanShareNormalMapRow As Boolean = PyNormalMapModeComboBox.Visible AndAlso PyPrecisionComboBox.Visible

        If CanKeepPrecisionBesideTileSize OrElse Not CanShareNormalMapRow Then
            PyPrecisionComboBox.Size = New Size(PrecisionWidth, PyPrecisionComboBox.Height)
            PyPrecisionComboBox.Location = New Point(ClientWidth - RightMargin - PrecisionWidth, PyTileSize.Top)
            PyPrecisionLabel.Location = New Point(
                PrecisionLeft,
                PyTileSize.Top + Math.Max(0, (PyTileSize.Height - PyPrecisionLabel.Height) \ 2)
            )
            If PyNormalMapModeComboBox.Visible Then
                PyNormalMapModeComboBox.Width = Math.Max(1, ClientWidth - PyNormalMapModeComboBox.Left - RightMargin)
                PyNormalMapModeComboBox.DropDownWidth = Math.Max(260, Math.Min(320, PyNormalMapModeComboBox.Width))
            End If
            Return
        End If

        ' On narrower forms share the Normal Map row: this keeps Precision away from CPU only
        ' and lets both dropdowns shrink safely instead of painting over neighbouring controls.
        Dim NormalMapLeft As Integer = PyNormalMapModeLabel.Right + LabelGap
        Dim PrecisionRowTop As Integer = PyNormalMapModeComboBox.Top
        Dim AvailableWidth As Integer = Math.Max(2, ClientWidth - RightMargin - NormalMapLeft)
        Dim ComboSpace As Integer = Math.Max(2, AvailableWidth - (2 * LabelGap) - PrecisionLabelWidth)
        Dim NormalMapWidth As Integer = Math.Max(1, CInt(Math.Floor(ComboSpace * 0.6)))
        Dim CompactPrecisionWidth As Integer = Math.Max(1, ComboSpace - NormalMapWidth)
        PyNormalMapModeComboBox.Location = New Point(NormalMapLeft, PrecisionRowTop)
        PyNormalMapModeComboBox.Size = New Size(NormalMapWidth, PyNormalMapModeComboBox.Height)
        PyNormalMapModeComboBox.DropDownWidth = Math.Max(260, Math.Min(320, PyNormalMapModeComboBox.Width))

        PrecisionLeft = PyNormalMapModeComboBox.Right + LabelGap
        PyPrecisionLabel.Location = New Point(
            PrecisionLeft,
            PrecisionRowTop + Math.Max(0, (PyNormalMapModeComboBox.Height - PyPrecisionLabel.Height) \ 2)
        )
        Dim CompactPrecisionLeft As Integer = PrecisionLeft + PrecisionLabelWidth + LabelGap
        CompactPrecisionWidth = Math.Max(1, ClientWidth - RightMargin - CompactPrecisionLeft)
        PyPrecisionComboBox.Location = New Point(CompactPrecisionLeft, PrecisionRowTop)
        PyPrecisionComboBox.Size = New Size(CompactPrecisionWidth, PyPrecisionComboBox.Height)
    End Sub

    Private Sub SetSpandrelTilingBadge(Text As String, Background As System.Drawing.Color,
                                       Foreground As System.Drawing.Color, Description As String)
        SpandrelTilingBadge.Text = Text
        SpandrelTilingBadge.BackColor = Background
        SpandrelTilingBadge.ForeColor = Foreground
        SpandrelTilingBadge.AccessibleDescription = Description
        UiToolTip.SetToolTip(SpandrelTilingBadge, Description)
    End Sub

    Private Function GetSpandrelTilingModeForModelPath(ModelPath As String) As String
        If String.IsNullOrWhiteSpace(ModelPath) Then Return "unknown"
        For Each Candidate As SpandrelModelInfo In SupportedSpandrelModels
            If Not Candidate.IsAutoTextureRouter AndAlso
                String.Equals(Candidate.FilePath, ModelPath, StringComparison.OrdinalIgnoreCase) Then
                Dim Mode As String = If(Candidate.Tiling, "unknown").Trim()
                Return If(Mode = "", "unknown", Mode.ToLowerInvariant())
            End If
        Next
        Return "unknown"
    End Function

    Private Function FindSpandrelModelByPath(ModelPath As String) As SpandrelModelInfo
        If String.IsNullOrWhiteSpace(ModelPath) Then Return Nothing
        For Each Candidate As SpandrelModelInfo In SupportedSpandrelModels
            If Not Candidate.IsAutoTextureRouter AndAlso
                String.Equals(Candidate.FilePath, ModelPath, StringComparison.OrdinalIgnoreCase) Then
                Return Candidate
            End If
        Next
        Return Nothing
    End Function

    Private Function TryGetSpandrelModelFP16Support(ModelPath As String, ByRef SupportsFP16 As Boolean) As Boolean
        SupportsFP16 = False
        Dim Model As SpandrelModelInfo = FindSpandrelModelByPath(ModelPath)
        If Model Is Nothing OrElse Not Model.PrecisionSupportKnown Then Return False
        SupportsFP16 = Model.SupportsHalf
        Return True
    End Function

    Private Function TryGetSelectedModelFP16Support(ByRef SupportsFP16 As Boolean) As Boolean
        SupportsFP16 = False
        Dim BackendName As String = If(ExeComboBox.SelectedItem, "").ToString()
        If BackendName = SpandrelBackendName Then
            If PyModel.SelectedIndex < 0 OrElse PyModel.SelectedIndex >= SupportedSpandrelModels.Count Then Return False
            Dim Model As SpandrelModelInfo = SupportedSpandrelModels(PyModel.SelectedIndex)
            If Model.IsAutoTextureRouter Then
                Dim ArchitectSupported As Boolean = False
                Dim PainterSupported As Boolean = False
                If Not TryGetSpandrelModelFP16Support(
                    GetSelectedAutoRouteModelPath(AutoArchitectModelComboBox), ArchitectSupported
                ) Then Return False
                If Not TryGetSpandrelModelFP16Support(
                    GetSelectedAutoRouteModelPath(AutoPainterModelComboBox), PainterSupported
                ) Then Return False
                SupportsFP16 = ArchitectSupported AndAlso PainterSupported
                Return True
            End If
            If Not Model.PrecisionSupportKnown Then Return False
            SupportsFP16 = Model.SupportsHalf
            Return True
        End If

        Dim ModelPath As String = ""
        If BackendName = PLKSRBackendName OrElse BackendName = "RealPLKSR" Then
            ModelPath = PLKSRModelPath
        ElseIf BackendName = DAT2BackendName Then
            ModelPath = DAT2ModelPath
        Else
            Return False
        End If
        Return TryGetSpandrelModelFP16Support(ModelPath, SupportsFP16)
    End Function

    Private Sub ApplyPrecisionCompatibilityHint(BaseTilingHint As String, PrecisionSupportKnown As Boolean,
                                                 SupportsFP16 As Boolean, SwitchedToAuto As Boolean)
        TileSizeHint.Text = BaseTilingHint
        TileSizeHint.BackColor = System.Drawing.Color.Transparent
        TileSizeHint.Padding = Padding.Empty
        PyPrecisionLabel.Text = "Precision:"
        PyPrecisionLabel.ForeColor = System.Drawing.SystemColors.ControlText
        PyPrecisionComboBox.BackColor = System.Drawing.SystemColors.Window

        If Not PrecisionSupportKnown Then
            UiToolTip.SetToolTip(TileSizeHint, BaseTilingHint)
            UiToolTip.SetToolTip(PyPrecisionLabel, "Inference precision selection.")
            LayoutSpandrelResponsiveControls()
            Return
        End If

        Dim PrecisionTooltip As String
        If Not SupportsFP16 Then
            If SwitchedToAuto Then
                PrecisionTooltip = "This checkpoint does not advertise FP16 support. The selection was reset to Auto; Auto uses FP32 for this model."
            Else
                PrecisionTooltip = "This checkpoint does not advertise FP16 support. Auto uses FP32; selecting FP16 switches back to Auto."
            End If
            PyPrecisionLabel.Text = "Precision (FP32):"
            PyPrecisionLabel.ForeColor = System.Drawing.Color.FromArgb(133, 83, 0)
            PyPrecisionComboBox.BackColor = System.Drawing.Color.FromArgb(255, 244, 214)
        Else
            PrecisionTooltip = "This checkpoint advertises FP16 support." & Environment.NewLine &
                "Auto uses FP16 on CUDA; FP32 remains available for maximum compatibility."
            If PyCPU.Checked Then
                PyPrecisionLabel.Text = "Precision (FP32):"
                PrecisionTooltip &= Environment.NewLine & "CPU inference always uses FP32."
            End If
        End If

        UiToolTip.SetToolTip(TileSizeHint, PrecisionTooltip & Environment.NewLine & BaseTilingHint)
        UiToolTip.SetToolTip(PyPrecisionComboBox, PrecisionTooltip)
        UiToolTip.SetToolTip(PyPrecisionLabel, PrecisionTooltip)
        LayoutSpandrelResponsiveControls()
    End Sub

    Private Sub PyPrecisionComboBox_SelectedIndexChanged(sender As Object, e As EventArgs)
        If IsUpdatingPrecisionSelection Then Return
        UpdateSpandrelModelInfo()
    End Sub

    Private Sub UpdateSpandrelModelInfo()
        Dim BackendName As String = If(ExeComboBox.SelectedItem, "").ToString()
        Dim BaseTilingHint As String = TileSizeHint.Text
        If BackendName = PLKSRBackendName OrElse BackendName = "RealPLKSR" OrElse BackendName = DAT2BackendName Then
            BaseTilingHint = "0 = full image first; retries smaller tiles on GPU memory errors."
        End If
        Dim SelectedModelSupportsFP16 As Boolean = False
        Dim PrecisionSupportKnown As Boolean = TryGetSelectedModelFP16Support(SelectedModelSupportsFP16)
        Dim PrecisionFallbackNotice As Boolean = False
        If PrecisionSupportKnown AndAlso Not SelectedModelSupportsFP16 AndAlso
            PyPrecisionComboBox.SelectedIndex = 1 AndAlso Not PyCPU.Checked Then
            IsUpdatingPrecisionSelection = True
            Try
                PyPrecisionComboBox.SelectedIndex = 0
                PrecisionFallbackNotice = True
            Finally
                IsUpdatingPrecisionSelection = False
            End Try
        End If
        Dim IsSpandrelSelected As Boolean = String.Equals(BackendName, SpandrelBackendName, StringComparison.OrdinalIgnoreCase)
        Dim UsesSpandrelRunner As Boolean = IsSpandrelPackageType(BackendName)
        PyPrecisionLabel.Visible = UsesSpandrelRunner
        PyPrecisionComboBox.Visible = UsesSpandrelRunner
        PyPrecisionComboBox.Enabled = UsesSpandrelRunner AndAlso Not PyCPU.Checked
        UiToolTip.SetToolTip(PyPrecisionComboBox,
            "Auto uses supported FP16 on CUDA. If a checkpoint cannot use FP16, AutoCrispy switches to Auto/FP32; FP32 disables TF32.")
        PyNormalMapModeLabel.Visible = UsesSpandrelRunner
        PyNormalMapModeComboBox.Visible = UsesSpandrelRunner
        PyNormalMapModeComboBox.Enabled = UsesSpandrelRunner
        SpandrelModelInfoLabel.Visible = IsSpandrelSelected
        LayoutSpandrelModelCard(IsSpandrelSelected)
        LayoutSpandrelResponsiveControls()
        SpandrelScanStatusLabel.Visible = IsSpandrelSelected
        RefreshSpandrelModelsButton.Visible = IsSpandrelSelected
        BrowseOpenModelDbButton.Visible = IsSpandrelSelected
        LayoutOpenModelDbControls()
        PyTileSize.Enabled = True
        Label25.Enabled = True
        TileSizeHint.ForeColor = System.Drawing.SystemColors.GrayText
        TileSizeHint.BackColor = System.Drawing.Color.Transparent
        TileSizeHint.Padding = Padding.Empty
        PyPrecisionComboBox.BackColor = System.Drawing.SystemColors.Window
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
        If Not IsSpandrelSelected Then
            If PrecisionSupportKnown Then
                ApplyPrecisionCompatibilityHint(
                    BaseTilingHint, PrecisionSupportKnown, SelectedModelSupportsFP16, PrecisionFallbackNotice
                )
            Else
                UiToolTip.SetToolTip(PyTileSize, BaseTilingHint)
            End If
            Return
        End If

        If PyModel.SelectedIndex < 0 OrElse PyModel.SelectedIndex >= SupportedSpandrelModels.Count Then
            SpandrelModelInfoLabel.Text = "No compatible model selected."
            SetSpandrelTilingBadge(
                "NO MODEL",
                System.Drawing.Color.FromArgb(238, 241, 245),
                System.Drawing.Color.FromArgb(92, 102, 114),
                "Choose a recognized Spandrel model to see its tiling policy."
            )
            TileSizeHint.Text = "Select a compatible model to see its tile and memory behavior."
            UiToolTip.SetToolTip(SpandrelModelInfoLabel, "")
            UiToolTip.SetToolTip(PyTileSize, "Select a model to see whether it uses external tiles.")
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
            Dim ArchitectTiling As String = GetSpandrelTilingModeForModelPath(ArchitectPath)
            Dim PainterTiling As String = GetSpandrelTilingModeForModelPath(PainterPath)
            Dim TilingDescription As String = "Architect: " & ArchitectTiling.ToUpperInvariant() &
                " · Painter: " & PainterTiling.ToUpperInvariant() &
                ". INTERNAL models bypass external tiling and OOM tile retries."
            SetSpandrelTilingBadge(
                "POLICY PER MODEL",
                System.Drawing.Color.FromArgb(232, 240, 254),
                System.Drawing.Color.FromArgb(31, 78, 121),
                TilingDescription
            )
            TileSizeHint.Text = "Architect and Painter apply their own model's tiling policy."
            TileSizeHint.ForeColor = System.Drawing.Color.FromArgb(75, 91, 109)
            ApplyPrecisionCompatibilityHint(
                TileSizeHint.Text, PrecisionSupportKnown, SelectedModelSupportsFP16, PrecisionFallbackNotice
            )
            AutoPainterShareSuffix.Text = "strict cap (floored)"
            AutoPainterThresholdSuffix.Text = "below uses Architect"
            SpandrelModelInfoLabel.Text = "Auto routing · " & PainterShareText & "% Painter cap · min score " & PainterThresholdText
            Dim RouterTooltip As String = "Feature-based routing (not semantic object recognition)." & Environment.NewLine &
                "The Painter cap applies to every batch: max Painter count = floor(texture count × share / 100)." & Environment.NewLine &
                "For example, a 30% cap on two textures allows zero Painter assignments. Eligible textures beyond the cap use Architect." & Environment.NewLine &
                "Minimum Painter score: " & PainterThresholdText & Environment.NewLine &
                "Architect model: " & ArchitectPath & " (tiling: " & ArchitectTiling & ")" & Environment.NewLine &
                "Painter model: " & PainterPath & " (tiling: " & PainterTiling & ")" & Environment.NewLine &
                "INTERNAL models bypass external tiles and OOM tile retries. Requires two different 4× RGB super-resolution models."
            UiToolTip.SetToolTip(SpandrelModelInfoLabel, RouterTooltip)
            UiToolTip.SetToolTip(PyModel, RouterTooltip)
            UiToolTip.SetToolTip(AutoArchitectModelComboBox, ArchitectPath & Environment.NewLine & "Tiling: " & ArchitectTiling)
            UiToolTip.SetToolTip(AutoPainterModelComboBox, PainterPath & Environment.NewLine & "Tiling: " & PainterTiling)
            UiToolTip.SetToolTip(AutoPainterSharePercent, "Strict maximum Painter share for every batch. The allowed count is floored to a whole number of textures; small batches can therefore allow zero Painter images.")
            UiToolTip.SetToolTip(AutoPainterThresholdLabel, "Textures below this feature score are assigned to Architect. This is a feature heuristic, not semantic classification.")
            UiToolTip.SetToolTip(AutoPainterThreshold, "Minimum feature score for Painter eligibility (0.05–1.00). Lower values make more textures eligible.")
            UiToolTip.SetToolTip(AutoRoutePreviewSampleLabel, "Analyze the first N supported textures, sorted by relative path. Preview is read-only.")
            UiToolTip.SetToolTip(AutoRoutePreviewSampleCount, "Number of supported textures to sample for a quick read-only route preview.")
            Return
        End If
        Dim TilingMode As String = If(Model.Tiling, "unknown").Trim().ToUpperInvariant()
        If TilingMode = "" Then TilingMode = "UNKNOWN"
        Dim TilingTooltip As String = "Spandrel tiling metadata: " & TilingMode & "."
        Select Case TilingMode
            Case "INTERNAL"
                TilingTooltip &= " The model handles its own tiling; AutoCrispy external tiles and OOM tile retries are disabled."
                SetSpandrelTilingBadge(
                    "INTERNAL TILING",
                    System.Drawing.Color.FromArgb(227, 239, 255),
                    System.Drawing.Color.FromArgb(31, 78, 121),
                    TilingTooltip
                )
                TileSizeHint.Text = "Model tiles internally · external tile size and OOM retries are ignored."
                TileSizeHint.ForeColor = System.Drawing.Color.FromArgb(31, 78, 121)
                PyTileSize.Enabled = False
                Label25.Enabled = False
                UiToolTip.SetToolTip(PyTileSize, TilingTooltip)
            Case "DISCOURAGED"
                TilingTooltip &= " External tiling may cause artifacts; AutoCrispy keeps its configured tile size and OOM fallback."
                SetSpandrelTilingBadge(
                    "TILING NOT ADVISED",
                    System.Drawing.Color.FromArgb(255, 244, 214),
                    System.Drawing.Color.FromArgb(133, 83, 0),
                    TilingTooltip
                )
                TileSizeHint.Text = "Tiling may cause artifacts · external tiling remains available."
                TileSizeHint.ForeColor = System.Drawing.Color.FromArgb(133, 83, 0)
                UiToolTip.SetToolTip(PyTileSize, TilingTooltip)
            Case "SUPPORTED"
                TilingTooltip &= " AutoCrispy uses the selected tile size and retries smaller tiles after GPU OOM."
                SetSpandrelTilingBadge(
                    "EXTERNAL TILING OK",
                    System.Drawing.Color.FromArgb(226, 244, 232),
                    System.Drawing.Color.FromArgb(35, 105, 65),
                    TilingTooltip
                )
                TileSizeHint.Text = "Tiling supported · smaller tiles are retried after GPU memory errors."
                TileSizeHint.ForeColor = System.Drawing.Color.FromArgb(57, 95, 73)
                UiToolTip.SetToolTip(PyTileSize, TilingTooltip)
            Case Else
                TilingTooltip &= " AutoCrispy uses its normal external tile and OOM fallback behavior."
                SetSpandrelTilingBadge(
                    "TILING POLICY UNKNOWN",
                    System.Drawing.Color.FromArgb(238, 241, 245),
                    System.Drawing.Color.FromArgb(92, 102, 114),
                    TilingTooltip
                )
                TileSizeHint.Text = "Unknown policy · AutoCrispy uses its normal tile and OOM fallback."
                TileSizeHint.ForeColor = System.Drawing.SystemColors.GrayText
                UiToolTip.SetToolTip(PyTileSize, TilingTooltip)
        End Select
        ApplyPrecisionCompatibilityHint(
            TileSizeHint.Text, PrecisionSupportKnown, SelectedModelSupportsFP16, PrecisionFallbackNotice
        )
        If Model.Scale > 0 Then
            SpandrelModelInfoLabel.Text = Model.Scale.ToString() & "× " & Model.Purpose & " · RGB " &
                Model.InputChannels.ToString() & "→" & Model.OutputChannels.ToString() & " · " & Model.Architecture
        Else
            SpandrelModelInfoLabel.Text = "Spandrel-compatible · " & Model.Architecture
        End If
        Dim ModelToolTip As String = Model.FilePath & Environment.NewLine & TilingTooltip
        UiToolTip.SetToolTip(SpandrelModelInfoLabel, ModelToolTip)
        UiToolTip.SetToolTip(PyModel, ModelToolTip)
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
        Dim PreviewCancellation As New CancellationTokenSource()
        PreviewProcessCancellation = PreviewCancellation
        Try
            Dim PreviewItems As List(Of AutoRoutePreviewItem) = Await Task.Run(
                Function() RunAutoRoutePreview(
                    PythonExecutable, RunnerPath, PreviewFolder, Package,
                    PreviewSampleLimit, PreviewDebugEnabled, PreviewCancellation.Token))
            If FormClosingRequested OrElse IsDisposed Then Return
            Dim PainterCount As Integer = PreviewItems.Where(Function(Item) String.Equals(Item.Role, "Painter", StringComparison.OrdinalIgnoreCase)).Count()
            AutoRoutePreviewStatusLabel.Text = "Sample " & PreviewItems.Count.ToString() & ": " &
                PainterCount.ToString() & " Painter / " & (PreviewItems.Count - PainterCount).ToString() & " Architect"
            AutoRoutePreviewHasResult = True
            Using PreviewDialog As New AutoRoutePreviewDialog(PreviewItems, PreviewFolder, PreviewSampleLimit,
                Package.PainterShare, CDbl(Package.PainterThreshold), Package.ArchitectModel, Package.PainterModel)
                PreviewDialog.ShowDialog(Me)
            End Using
        Catch ex As OperationCanceledException
            If Not FormClosingRequested AndAlso Not IsDisposed Then
                AutoRoutePreviewHasResult = False
                AutoRoutePreviewStatusLabel.Text = "Preview cancelled."
            End If
        Catch ex As Exception
            If Not FormClosingRequested AndAlso Not IsDisposed Then
                AutoRoutePreviewHasResult = False
                AutoRoutePreviewStatusLabel.Text = "Preview failed."
                MessageBox.Show(Me, "Could not preview texture routing." & Environment.NewLine & ex.Message,
                    "Auto Texture Routing preview", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        Finally
            If Object.ReferenceEquals(PreviewProcessCancellation, PreviewCancellation) Then
                PreviewProcessCancellation = Nothing
            End If
            PreviewCancellation.Dispose()
            If Not IsDisposed AndAlso Not FormClosingRequested Then
                AutoRoutePreviewButton.Enabled = True
                PyModel.Enabled = PreviousPyModelEnabled
                RefreshSpandrelModelsButton.Enabled = PreviousRefreshEnabled
                AutoArchitectModelComboBox.Enabled = PreviousArchitectEnabled
                AutoPainterModelComboBox.Enabled = PreviousPainterEnabled
                AutoPainterSharePercent.Enabled = PreviousShareEnabled
                AutoPainterThreshold.Enabled = PreviousThresholdEnabled
                AutoRoutePreviewSampleCount.Enabled = PreviousSampleCountEnabled
            End If
            TryCompleteDeferredClose()
        End Try
    End Sub

    Private Function RunAutoRoutePreview(PythonExecutable As String, RunnerPath As String, SourceFolder As String,
                                         Package As FormSettings.PythonPackage, SampleLimit As Integer,
                                         DebugEnabled As Boolean, PreviewToken As CancellationToken) As List(Of AutoRoutePreviewItem)
        PreviewToken.ThrowIfCancellationRequested()
        Dim StartInfo As New ProcessStartInfo(PythonExecutable,
            MakeSpandrelPreviewCommand(RunnerPath, SourceFolder, Package, SampleLimit, DebugEnabled))
        StartInfo.WorkingDirectory = Application.StartupPath
        StartInfo.RedirectStandardOutput = True
        StartInfo.RedirectStandardError = True
        StartInfo.UseShellExecute = False
        StartInfo.CreateNoWindow = True

        Using PreviewProcess As Process = Process.Start(StartInfo)
            If PreviewProcess Is Nothing Then Throw New InvalidOperationException("Failed to start the route preview process.")
            Try
                RegisterActiveProcess(PreviewProcess)
                Using CancellationRegistration As CancellationTokenRegistration = PreviewToken.Register(
                    Sub()
                        Task.Run(Sub() TerminateProcessTree(PreviewProcess))
                    End Sub)
                    Dim StandardOutputTask = PreviewProcess.StandardOutput.ReadToEndAsync()
                    Dim StandardErrorTask = PreviewProcess.StandardError.ReadToEndAsync()
                    While Not PreviewProcess.WaitForExit(200)
                        PreviewToken.ThrowIfCancellationRequested()
                    End While
                    Dim StandardOutput As String = StandardOutputTask.Result
                    Dim StandardError As String = StandardErrorTask.Result
                    PreviewToken.ThrowIfCancellationRequested()
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
            Catch
                TerminateProcessTree(PreviewProcess)
                Throw
            Finally
                UnregisterActiveProcess(PreviewProcess)
            End Try
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

    Private Async Sub BrowseOpenModelDbButton_Click(sender As Object, e As EventArgs)
        If WorkHorse.IsBusy Then
            MessageBox.Show(Me, "Stop the active batch before downloading or scanning a model.",
                "Model catalog", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        Dim PythonExecutable As String = FindPythonExecutable()
        Dim RunnerPath As String = Path.Combine(Application.StartupPath, SpandrelRunnerName)
        Dim CatalogHelperPath As String = Path.Combine(Application.StartupPath, "openmodeldb_catalog.py")
        If PythonExecutable = "" OrElse Not File.Exists(RunnerPath) OrElse Not File.Exists(CatalogHelperPath) Then
            MessageBox.Show(Me,
                "OpenModelDB downloads need the configured Python 3.10+ environment and both AutoCrispy Python helpers beside the application.",
                "Python setup required", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim DestinationFolder As String = Path.Combine(Root, GenericModelFolderName)
        Dim ExistingModelPaths As IEnumerable(Of String) = SupportedSpandrelModels.
            Where(Function(Model As SpandrelModelInfo) Not Model.IsAutoTextureRouter).
            Select(Function(Model As SpandrelModelInfo) Model.FilePath).ToList()
        Using CatalogDialog As New OpenModelDbDialog(PythonExecutable, CatalogHelperPath, DestinationFolder,
                                                      ExistingModelPaths, AddressOf InstallOpenModelDbModelAsync)
            If CatalogDialog.ShowDialog(Me) <> DialogResult.OK OrElse
                String.IsNullOrWhiteSpace(CatalogDialog.DownloadedModelPath) Then Return
            LastSelectedSpandrelModelPath = CatalogDialog.DownloadedModelPath
            Await RefreshSupportedSpandrelModels(Root)
            LastSelectedSpandrelModelPath = CatalogDialog.DownloadedModelPath
            ConfigurePythonModelSelector(SpandrelBackendName)
            Dim InstalledIndex As Integer = SupportedSpandrelModels.FindIndex(Function(Model As SpandrelModelInfo) _
                String.Equals(Model.FilePath, CatalogDialog.DownloadedModelPath, StringComparison.OrdinalIgnoreCase))
            If InstalledIndex >= 0 Then PyModel.SelectedIndex = InstalledIndex
            SetSettingsWindow()
        End Using
    End Sub

    Private Async Function InstallOpenModelDbModelAsync(Item As OpenModelDbCatalogItem,
                                                        Resource As OpenModelDbResource,
                                                        DownloadedPath As String,
                                                        ReportStatus As Action(Of String),
                                                        CancelToken As CancellationToken) As Task(Of String)
        CancelToken.ThrowIfCancellationRequested()
        If Item Is Nothing OrElse Resource Is Nothing OrElse Not File.Exists(DownloadedPath) Then
            Throw New InvalidDataException("The verified OpenModelDB download was not found.")
        End If
        If Item.Scale <> 1 AndAlso Item.Scale <> 4 Then
            Throw New InvalidDataException("AutoCrispy only supports 1× restoration and 4× super-resolution checkpoints.")
        End If
        If Not String.Equals(Path.GetFileName(Item.Id), Item.Id, StringComparison.Ordinal) OrElse
            Item.Id.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 Then
            Throw New InvalidDataException("The OpenModelDB model ID is not a safe filename.")
        End If

        Dim PythonExecutable As String = FindPythonExecutable()
        Dim RunnerPath As String = Path.Combine(Application.StartupPath, SpandrelRunnerName)
        If PythonExecutable = "" OrElse Not File.Exists(RunnerPath) Then
            Throw New InvalidOperationException("AutoCrispy's Spandrel Python environment is not available for model validation.")
        End If
        If ReportStatus IsNot Nothing Then ReportStatus("Testing the download with AutoCrispy's installed Spandrel…")
        Dim DebugEnabled As Boolean = DebugCheckbox.Checked
        Dim StagingFolder As String = Path.GetDirectoryName(Path.GetFullPath(DownloadedPath))
        Dim DetectedModels As List(Of SpandrelModelInfo) = Await Task.Run(
            Function() ScanSpandrelModelFolder(StagingFolder, PythonExecutable, RunnerPath, DebugEnabled, CancelToken),
            CancelToken)
        CancelToken.ThrowIfCancellationRequested()
        Dim DownloadedFullPath As String = Path.GetFullPath(DownloadedPath)
        Dim ValidatedModel As SpandrelModelInfo = Nothing
        For Each Candidate As SpandrelModelInfo In DetectedModels
            If Candidate.IsAutoTextureRouter Then Continue For
            If Not String.Equals(Path.GetFullPath(Candidate.FilePath), DownloadedFullPath, StringComparison.OrdinalIgnoreCase) Then Continue For
            If Candidate.Scale <> Item.Scale OrElse Candidate.InputChannels <> 3 OrElse Candidate.OutputChannels <> 3 Then Continue For
            ValidatedModel = Candidate
            Exit For
        Next
        If ValidatedModel Is Nothing Then
            Throw New InvalidDataException(
                "The checkpoint did not pass AutoCrispy's Spandrel check for " & Item.Scale.ToString() & "× RGB inference. It was not installed.")
        End If

        Dim DestinationFolder As String = Path.Combine(Root, GenericModelFolderName)
        Directory.CreateDirectory(DestinationFolder)
        Dim FinalPath As String = Path.Combine(DestinationFolder, Item.Id & "." & Resource.Format)
        If File.Exists(FinalPath) Then Throw New IOException("A file with this model ID already exists. Refresh the local model list or remove that file first.")
        Dim PartialPath As String = Path.Combine(DestinationFolder, ".openmodeldb-" & Guid.NewGuid().ToString("N") & ".part")
        If ReportStatus IsNot Nothing Then ReportStatus("Checkpoint passed Spandrel validation. Installing into the shared models folder…")
        Try
            Await Task.Run(
                Sub()
                    Using SourceStream As New FileStream(DownloadedPath, FileMode.Open, FileAccess.Read, FileShare.Read)
                        Using DestinationStream As New FileStream(PartialPath, FileMode.CreateNew, FileAccess.Write, FileShare.None)
                            Dim Buffer(1024 * 1024 - 1) As Byte
                            While True
                                CancelToken.ThrowIfCancellationRequested()
                                Dim BytesRead As Integer = SourceStream.Read(Buffer, 0, Buffer.Length)
                                If BytesRead <= 0 Then Exit While
                                DestinationStream.Write(Buffer, 0, BytesRead)
                            End While
                            DestinationStream.Flush(True)
                        End Using
                    End Using
                    CancelToken.ThrowIfCancellationRequested()
                    File.Move(PartialPath, FinalPath)
                End Sub,
                CancelToken)
            Return FinalPath
        Catch
            Try
                If File.Exists(PartialPath) Then File.Delete(PartialPath)
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine("Could not remove a partial OpenModelDB install: " & ex.Message)
            End Try
            Throw
        End Try
    End Function

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

    Public Function GetSelectedInferencePrecision() As String
        If PyPrecisionComboBox.SelectedItem Is Nothing Then Return "auto"
        Return PyPrecisionComboBox.SelectedItem.ToString().Trim().ToLowerInvariant()
    End Function

    Public Sub LoadInferencePrecision(Precision As String)
        Dim NormalizedPrecision As String = If(Precision, String.Empty).Trim().ToLowerInvariant()
        Select Case NormalizedPrecision
            Case "fp16"
                PyPrecisionComboBox.SelectedIndex = If(PyCPU.Checked, 0, 1)
            Case "fp32"
                PyPrecisionComboBox.SelectedIndex = 2
            Case Else
                PyPrecisionComboBox.SelectedIndex = 0
        End Select
        PyPrecisionComboBox.Enabled = Not PyCPU.Checked AndAlso IsSpandrelPackageType(If(ExeComboBox.SelectedItem, "").ToString())
    End Sub

    Public Function GetSelectedNormalMapMode() As String
        Select Case PyNormalMapModeComboBox.SelectedIndex
            Case 1
                Return "normalize-xyz"
            Case 2
                Return "rebuild-z"
            Case Else
                Return "none"
        End Select
    End Function

    Public Sub LoadNormalMapMode(NormalMapMode As String)
        Select Case If(NormalMapMode, String.Empty).Trim().ToLowerInvariant()
            Case "normalize-xyz"
                PyNormalMapModeComboBox.SelectedIndex = 1
            Case "rebuild-z"
                PyNormalMapModeComboBox.SelectedIndex = 2
            Case Else
                PyNormalMapModeComboBox.SelectedIndex = 0
        End Select
    End Sub

    Private Function AppendNormalMapModeSuffix(StepName As String, NormalMapMode As String) As String
        Select Case If(NormalMapMode, String.Empty).Trim().ToLowerInvariant()
            Case "normalize-xyz"
                Return StepName & " · Normalize XYZ"
            Case "rebuild-z"
                Return StepName & " · Rebuild Z"
            Case Else
                Return StepName
        End Select
    End Function

    Private Sub PyCPU_CheckedChanged(sender As Object, e As EventArgs) Handles PyCPU.CheckedChanged
        If PyCPU.Checked AndAlso PyPrecisionComboBox.SelectedIndex = 1 Then
            PyPrecisionComboBox.SelectedIndex = 0
        End If
        PyPrecisionComboBox.Enabled = Not PyCPU.Checked AndAlso IsSpandrelPackageType(If(ExeComboBox.SelectedItem, "").ToString())
        UpdateSpandrelModelInfo()
    End Sub

    Public Function GetSelectedPythonPackage(Optional IncludeRoutePreferences As Boolean = True) As FormSettings.PythonPackage
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
                    ArchitectPath, PainterPath, CInt(AutoPainterSharePercent.Value), CDec(AutoPainterThreshold.Value), GetSelectedInferencePrecision(), GetSelectedNormalMapMode())
            End If
        End If
        Dim ArchitectPreference As String = If(IncludeRoutePreferences, LastSelectedArchitectModelPath, String.Empty)
        Dim PainterPreference As String = If(IncludeRoutePreferences, LastSelectedPainterModelPath, String.Empty)
        Return New FormSettings.PythonPackage(GetSelectedUpscaleModel(), CInt(PyTileSize.Value), PyCPU.Checked, UseSpandrelFormats,
            False, ArchitectPreference, PainterPreference, CInt(AutoPainterSharePercent.Value), CDec(AutoPainterThreshold.Value), GetSelectedInferencePrecision(), GetSelectedNormalMapMode())
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

    Private Async Sub WatchDog_Tick(sender As Object, e As EventArgs) Handles WatchDog.Tick
        If WorkHorse.IsBusy OrElse WatchDogScanInFlight OrElse ProgressScanInFlight OrElse
            FormClosingRequested OrElse Not WatchDog.Enabled Then Return

        WatchDogScanInFlight = True
        Dim ScanCancellation As New CancellationTokenSource()
        WatchDogScanCancellation = ScanCancellation
        Dim ProfilesToWatch As List(Of FormSettings.GamePathProfile) = Nothing
        Dim ScanResult As WatchdogScanResult = Nothing
        Try
            ProfilesToWatch = New List(Of FormSettings.GamePathProfile)
            For Each Profile As FormSettings.GamePathProfile In GetEffectiveGamePathProfiles()
                ProfilesToWatch.Add(CloneGamePathProfile(Profile))
            Next
            Dim ActiveExtensions As HashSet(Of String) = GetActiveInputFileTypes()
            Dim SupportedExtensions As HashSet(Of String) = If(ActiveExtensions Is Nothing, Nothing,
                New HashSet(Of String)(ActiveExtensions, StringComparer.OrdinalIgnoreCase))
            Dim AlphaMode As Integer = GetActiveAlphaMode()
            ScanResult = Await Task.Run(Function() ScanWatchdogProfiles(ProfilesToWatch, SupportedExtensions,
                AlphaMode, ScanCancellation.Token), ScanCancellation.Token)
        Catch ex As Exception
            ScanResult = New WatchdogScanResult With {.ErrorMessage = ex.GetBaseException().Message}
        Finally
            If Object.ReferenceEquals(WatchDogScanCancellation, ScanCancellation) Then
                WatchDogScanCancellation = Nothing
            End If
            ScanCancellation.Dispose()
            WatchDogScanInFlight = False
            TryCompleteDeferredClose()
        End Try

        If IsDisposed OrElse FormClosingRequested OrElse Not WatchDog.Enabled OrElse WorkHorse.IsBusy Then Return
        If ScanResult Is Nothing Then Return
        If ScanResult.ErrorMessage <> String.Empty Then
            QueueActivityLabel.Text = "Watching blocked · " & ScanResult.ErrorMessage
            UiToolTip.SetToolTip(QueueActivityLabel, ScanResult.ErrorMessage)
            WatchDog.Interval = 5000
            Return
        End If

        If ScanResult.PendingProfiles.Count = 0 Then
            If ScanResult.MissingProfileCount > 0 Then
                QueueActivityLabel.Text = "Watching · " & ScanResult.MissingProfileCount.ToString() & " game path(s) unavailable"
            ElseIf ProfilesToWatch.Count = 0 Then
                QueueActivityLabel.Text = "No checked game paths to watch"
            Else
                QueueActivityLabel.Text = "Watching " & ProfilesToWatch.Count.ToString() & " game path(s)…"
            End If
            WaitScale = Math.Min(WaitScale + 1, 100)
            WatchDog.Interval = 1000 + (WaitScale * 590)
        Else
            QueueActivityLabel.Text = "Starting next batch for " & ScanResult.PendingProfiles.Count.ToString() & " game(s)…"
            WaitScale = 0
            WatchDog.Interval = 1000
            CurrentRunGamePaths = ScanResult.PendingProfiles
            LoadedSettings = New FormSettings.Settings(Me)
            If ChainControl.ListItems.Count = 0 Then
                AddModelToChain(ExeComboBox.SelectedItem, False)
            End If
            ProgressPollTimer.Interval = 1000
            WatchDog.Stop()
            WorkHorse.RunWorkerAsync()
        End If
    End Sub

    Private Function ScanWatchdogProfiles(ProfilesToWatch As List(Of FormSettings.GamePathProfile),
                                          SupportedExtensions As HashSet(Of String), AlphaMode As Integer,
                                          ScanToken As CancellationToken) As WatchdogScanResult
        Dim Result As New WatchdogScanResult
        For Each Profile As FormSettings.GamePathProfile In ProfilesToWatch
            If ScanToken.IsCancellationRequested Then Exit For
            Try
                If Not Directory.Exists(Profile.InputPath) OrElse Not Directory.Exists(Profile.OutputPath) Then
                    Result.MissingProfileCount += 1
                    Continue For
                End If
                Dim AllInputFiles As String() = GetInputFiles(Profile.InputPath, Profile.OutputPath)
                If ScanToken.IsCancellationRequested Then Exit For
                Dim UnsupportedCount As Integer = 0
                Dim AlphaFilteredCount As Integer = 0
                Dim SupportedCount As Integer = 0
                Dim MissingCount As Integer = 0
                Dim ResumeInProgressPaths As HashSet(Of String) = GetResumeInProgressPaths(Profile.OutputPath)
                Dim PendingFiles As String() = GetPendingInputFiles(AllInputFiles, Profile.OutputPath, SupportedExtensions, AlphaMode,
                                                                      UnsupportedCount, AlphaFilteredCount, SupportedCount, MissingCount,
                                                                      ResumeInProgressPaths)
                If PendingFiles.Length > 0 Then Result.PendingProfiles.Add(CloneGamePathProfile(Profile))
            Catch ex As InvalidDataException
                If Result.ErrorMessage = String.Empty Then Result.ErrorMessage = ex.GetBaseException().Message
            Catch ex As IOException
                Result.MissingProfileCount += 1
            Catch ex As UnauthorizedAccessException
                Result.MissingProfileCount += 1
            Catch ex As Exception
                If Result.ErrorMessage = String.Empty Then Result.ErrorMessage = ex.GetBaseException().Message
            End Try
        Next
        Return Result
    End Function

    ' Progress is overall completion for files the first pipeline stage can accept.
    ' Match by basename so format conversions (for example PNG input to DDS output) count as done.
    Private Async Sub ProgressPollTimer_Tick(sender As Object, e As EventArgs) Handles ProgressPollTimer.Tick
        If ProgressScanInFlight OrElse WatchDogScanInFlight OrElse FormClosingRequested Then Return

        ProgressScanInFlight = True
        Dim ScanCancellation As New CancellationTokenSource()
        ProgressScanCancellation = ScanCancellation
        Try
            Dim ProfilesToScan As New List(Of FormSettings.GamePathProfile)
            For Each Profile As FormSettings.GamePathProfile In GetEffectiveGamePathProfiles()
                ProfilesToScan.Add(CloneGamePathProfile(Profile))
            Next
            Dim ActiveExtensions As HashSet(Of String) = GetActiveInputFileTypes()
            Dim SupportedExtensions As HashSet(Of String) = If(ActiveExtensions Is Nothing, Nothing,
                New HashSet(Of String)(ActiveExtensions, StringComparer.OrdinalIgnoreCase))
            Dim AlphaMode As Integer = GetActiveAlphaMode()
            Dim Progress As OverallProgressInfo = Await Task.Run(
                Function() GetOverallProgress(ProfilesToScan, SupportedExtensions, AlphaMode, ScanCancellation.Token),
                ScanCancellation.Token)
            If IsDisposed OrElse FormClosingRequested Then Return

            Dim Percent As Integer = Math.Max(UpscaleProgress.Minimum, Math.Min(UpscaleProgress.Maximum, Progress.Percent))
            UpscaleProgress.Value = Percent
            Dim SkippedSummary As String = String.Empty
            If Progress.UnsupportedCount > 0 Then SkippedSummary &= " · " & Progress.UnsupportedCount.ToString() & " unsupported skipped"
            If Progress.AlphaFilteredCount > 0 Then SkippedSummary &= " · " & Progress.AlphaFilteredCount.ToString() & " skipped by alpha filter"
            QueueSummaryLabel.Text = Progress.DoneCount.ToString() & " / " & Progress.TotalCount.ToString() &
                " textures complete (" & Percent.ToString() & "%)" & SkippedSummary

            If Not WorkHorse.IsBusy AndAlso Not WatchDog.Enabled AndAlso WatchDogButton.Enabled Then
                If Progress.TotalCount > 0 AndAlso Progress.DoneCount < Progress.TotalCount Then
                    WatchDogButton.Text = "Resume batch"
                    UiToolTip.SetToolTip(WatchDogButton, "Resume unfinished textures. Successfully checkpointed outputs are kept; unconfirmed outputs are safely retried.")
                ElseIf WatchDogButton.Text <> "Stopping..." Then
                    WatchDogButton.Text = "Running: False"
                    UiToolTip.SetToolTip(WatchDogButton, "Start or stop the texture watcher.")
                End If
            End If

            If Progress.ErrorMessage <> String.Empty AndAlso Not WorkHorse.IsBusy Then
                QueueActivityLabel.Text = "Input issue · " & Progress.ErrorMessage
                UiToolTip.SetToolTip(QueueActivityLabel, Progress.ErrorMessage)
            ElseIf Not WorkHorse.IsBusy AndAlso RefreshSpandrelModelsButton.Enabled Then
                If WatchDog.Enabled Then
                    If Progress.TotalCount = 0 OrElse Progress.DoneCount >= Progress.TotalCount Then
                        QueueActivityLabel.Text = "Watching for new textures…"
                    Else
                        QueueActivityLabel.Text = "Watching · " & (Progress.TotalCount - Progress.DoneCount).ToString() & " remaining"
                    End If
                ElseIf Progress.TotalCount = 0 Then
                    QueueActivityLabel.Text = "Ready"
                ElseIf Progress.DoneCount >= Progress.TotalCount Then
                    QueueActivityLabel.Text = "Complete"
                Else
                    QueueActivityLabel.Text = "Paused · " & (Progress.TotalCount - Progress.DoneCount).ToString() & " remaining"
                End If
            End If
        Catch ex As Exception
            ' A removable or network-backed input/output folder can disappear during a scan.
        Finally
            If Object.ReferenceEquals(ProgressScanCancellation, ScanCancellation) Then
                ProgressScanCancellation = Nothing
            End If
            ScanCancellation.Dispose()
            ProgressScanInFlight = False
            If Not IsDisposed Then ProgressPollTimer.Interval = If(WorkHorse.IsBusy, 1000, 5000)
            TryCompleteDeferredClose()
        End Try
    End Sub

    Private Function GetOverallProgress(ProfilesToScan As List(Of FormSettings.GamePathProfile),
                                        SupportedExtensions As HashSet(Of String), AlphaMode As Integer,
                                        ScanToken As CancellationToken) As OverallProgressInfo
        Dim Result As New OverallProgressInfo
        For Each Profile As FormSettings.GamePathProfile In ProfilesToScan
            If ScanToken.IsCancellationRequested Then Exit For
            Try
                If Not Directory.Exists(Profile.InputPath) OrElse Not Directory.Exists(Profile.OutputPath) Then Continue For
                Dim AllInputFiles As String() = GetInputFiles(Profile.InputPath, Profile.OutputPath)
                If ScanToken.IsCancellationRequested Then Exit For
                Dim ProfileUnsupportedCount As Integer = 0
                Dim ProfileAlphaFilteredCount As Integer = 0
                Dim SupportedCount As Integer = 0
                Dim MissingCount As Integer = 0
                Dim ResumeInProgressPaths As HashSet(Of String) = GetResumeInProgressPaths(Profile.OutputPath)
                GetPendingInputFiles(AllInputFiles, Profile.OutputPath, SupportedExtensions, AlphaMode,
                    ProfileUnsupportedCount, ProfileAlphaFilteredCount, SupportedCount, MissingCount, ResumeInProgressPaths)
                Result.UnsupportedCount += ProfileUnsupportedCount
                Result.AlphaFilteredCount += ProfileAlphaFilteredCount
                Result.TotalCount += SupportedCount - ProfileAlphaFilteredCount
                Result.DoneCount += SupportedCount - MissingCount
            Catch ex As IOException
                ' The folder can disappear while the background scan is running.
            Catch ex As UnauthorizedAccessException
                ' A game folder may not be readable by the current user.
            Catch ex As Exception
                If Result.ErrorMessage = String.Empty Then Result.ErrorMessage = ex.GetBaseException().Message
            End Try
        Next

        If Result.TotalCount <= 0 OrElse Result.DoneCount <= 0 Then
            Result.Percent = 0
        ElseIf Result.DoneCount >= Result.TotalCount Then
            Result.Percent = 100
        Else
            Result.Percent = CInt(Math.Floor((Result.DoneCount * 100.0) / Result.TotalCount))
        End If
        Return Result
    End Function

    Private Sub WorkHorse_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles WorkHorse.DoWork
        Try
            If String.IsNullOrWhiteSpace(CurrentRunTempRoot) Then CurrentRunTempRoot = CreateRunTempRoot()
            MakeUpscale()
            If WorkHorse.CancellationPending = True Then e.Cancel = True
        Finally
            CleanupRunTemporaryRoot()
        End Try
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
        CleanupRunTemporaryRoot()
        If ChainControl.ListItems.Count = 0 Then ChainList.Clear()

        If CloseAfterWorkerCancellation Then
            TryCompleteDeferredClose()
            Return
        End If

        If e.Cancelled OrElse WatchDogButton.Text = "Stopping..." Then
            WatchDog.Stop()
            WatchDog.Enabled = False
            WatchDogButton.Text = "Running: False"
            SwitchGroups(True)
            WatchDogButton.Enabled = True
            ClearAlphaSkipList()
            QueueActivityLabel.Text = "Cancelled"
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
        If ChainList Is Nothing OrElse ChainList.Count = 0 Then
            Throw New InvalidOperationException("The processing chain is empty. Add a backend before resuming the batch.")
        End If
        Dim TempPath As String = GetChainPath("Temp", 0)
        Dim ResumeCheckpoint As BatchResumeCheckpoint = LoadResumeCheckpoint(LoadedSettings.Paths.OutputPath)
        Dim RecoveredCount As Integer = RecoverInterruptedBatch(LoadedSettings.Paths.OutputPath, ResumeCheckpoint)
        If RecoveredCount > 0 Then
            WorkHorse.ReportProgress(0, "Resuming · retrying " & RecoveredCount.ToString() & " interrupted texture(s)")
        End If
        Dim ThreadCount As Integer = GetThreads(LoadedSettings.BasicSettings.ThreadIndex, LoadedSettings.BasicSettings.ThreadCount)
        If ThreadCount < 1 Then ThreadCount = 1
        Dim AllInputFiles As String() = GetInputFiles(LoadedSettings.Paths.InputPath, LoadedSettings.Paths.OutputPath)
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

        Dim SuccessfullyProcessedInputs As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
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
            Dim CopiedInputs As New List(Of String)
            CopyFiles(Source, TempPath, CurrentIndex, BatchLimit, CopiedInputs)
            If WorkHorse.CancellationPending Then
                CleanupUpscaleTemporaryFolders()
                Return
            End If
            If CopiedInputs.Count > 0 Then
                Dim BatchOutputExtensions As HashSet(Of String) = GetExpectedOutputExtensions(
                    CopiedInputs, ChainList(ChainList.Count - 1).Package
                )
                MarkBatchInProgress(LoadedSettings.Paths.OutputPath, ResumeCheckpoint, CopiedInputs, BatchOutputExtensions)
            End If
            Dim OriginalInputByStem As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
            For Each CopiedInput As String In CopiedInputs
                OriginalInputByStem(Path.GetFileNameWithoutExtension(CopiedInput)) = Path.GetFullPath(CopiedInput)
            Next

            Dim BatchFiles As String() = Directory.GetFiles(TempPath)
            Array.Sort(BatchFiles, StringComparer.OrdinalIgnoreCase)
            Dim StageCounts As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
            For Each CopiedInput As String In CopiedInputs
                StageCounts(Path.GetFileNameWithoutExtension(CopiedInput)) = 0
            Next
            Dim FinalOutputExtensions As HashSet(Of String) = GetExpectedOutputExtensions(
                CopiedInputs, ChainList(ChainList.Count - 1).Package)
            Dim FinalOutputsForBatch As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
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
                    If WorkHorse.CancellationPending Then
                        CleanupUpscaleTemporaryFolders()
                        Return
                    End If
                    Dim AcceptExt As Boolean = Model.Package.FileTypes.Contains(Path.GetExtension(NewImage).ToLowerInvariant())
                    If File.Exists(NewImage) AndAlso AcceptExt Then
                        NewImages.Add(NewImage)
                        If LoadedSettings.BasicSettings.FixPS2 Then
                            If (ChainList.IndexOf(Model) = 0 AndAlso Model.Name <> "TexConv") OrElse
                                (ChainList(0).Name = "TexConv" AndAlso ChainList.IndexOf(Model) = 1) Then
                                RemovePS2Alpha(NewImage)
                            End If
                        End If
                        If WorkHorse.CancellationPending Then
                            CleanupUpscaleTemporaryFolders()
                            Return
                        End If
                        If LoadedSettings.ExpertSettings.SeamlessMode > 0 AndAlso
                            ((ChainList.IndexOf(Model) = 0 AndAlso Model.Name <> "TexConv") OrElse
                             (ChainList(0).Name = "TexConv" AndAlso ChainList.IndexOf(Model) = 1)) Then
                            Using SeamlessSource As Bitmap = GetUnlockedImage(NewImage)
                                Using SeamlessImage As Bitmap = MakeSeamless(SeamlessSource,
                                    LoadedSettings.ExpertSettings.SeamlessMode, LoadedSettings.ExpertSettings.SeamlessMargin)
                                    If Not WorkHorse.CancellationPending Then SeamlessImage.Save(NewImage)
                                End Using
                            End Using
                        End If
                    End If
                Next

                Dim IsFinalStage As Boolean = StageIndex = ChainList.Count
                Dim CompletedTextureCallback As Action(Of String) = Nothing
                If IsFinalStage AndAlso IsSpandrelPackageType(Model.PackageType) AndAlso
                    Not LoadedSettings.BasicSettings.Defringe AndAlso
                    LoadedSettings.ExpertSettings.SeamlessMode <= 0 AndAlso
                    Not LoadedSettings.BasicSettings.FixPS2 Then
                    CompletedTextureCallback = Sub(CompletedImageName As String)
                        Dim CompletedStem As String = Path.GetFileNameWithoutExtension(CompletedImageName)
                        Dim OriginalInputPath As String = Nothing
                        If OriginalInputByStem.TryGetValue(CompletedStem, OriginalInputPath) Then
                            MarkTextureComplete(LoadedSettings.Paths.OutputPath, ResumeCheckpoint, OriginalInputPath)
                        End If
                    End Sub
                End If
                StartBuilder(ChainPaths(0), ChainPaths(1), NewImages, Model, CompletedTextureCallback)
                If WorkHorse.CancellationPending Then
                    CleanupUpscaleTemporaryFolders()
                    Return
                End If
                For Each ProcessedImage As String In NewImages
                    Dim Stem As String = Path.GetFileNameWithoutExtension(ProcessedImage)
                    If StageCounts.ContainsKey(Stem) Then StageCounts(Stem) += 1
                Next

                DeletedChainPaths.Add(ChainPaths(0))
                ChainPaths.RemoveAt(0)
                If IsFinalStage Then
                    FinalOutputsForBatch = GetNonEmptyOutputsByStem(ChainPaths(0), FinalOutputExtensions)
                End If
                Dim RunsFinalPostProcessing As Boolean =
                    (ChainList.IndexOf(Model) = ChainList.Count - 1 AndAlso Model.Name <> "TexConv") OrElse
                    (ChainList(ChainList.Count - 1).Name = "TexConv" AndAlso ChainList.IndexOf(Model) = ChainList.Count - 2)
                If RunsFinalPostProcessing Then
                    For Each NewImage As String In NewImages
                        If WorkHorse.CancellationPending Then
                            CleanupUpscaleTemporaryFolders()
                            Return
                        End If
                        Dim OutputImage As String = Path.Combine(ChainPaths(0), Path.GetFileName(NewImage))
                        If LoadedSettings.BasicSettings.Defringe AndAlso File.Exists(OutputImage) Then
                            Defringe(OutputImage, LoadedSettings.BasicSettings.DefringeThreshold)
                        End If
                        If WorkHorse.CancellationPending Then
                            CleanupUpscaleTemporaryFolders()
                            Return
                        End If
                        If LoadedSettings.ExpertSettings.SeamlessMode > 0 AndAlso File.Exists(OutputImage) Then
                            Dim ScaleVal As Integer = LoadedSettings.ExpertSettings.SeamlessScale * LoadedSettings.ExpertSettings.SeamlessMargin
                            Using CroppedSource As Bitmap = GetUnlockedImage(OutputImage)
                                Using CroppedImage As Bitmap = CropImage(CroppedSource, ScaleVal, ScaleVal,
                                    CroppedSource.Width - (ScaleVal * 2), CroppedSource.Height - (ScaleVal * 2), 0)
                                    If Not WorkHorse.CancellationPending Then CroppedImage.Save(OutputImage)
                                End Using
                            End Using
                        End If
                        If WorkHorse.CancellationPending Then
                            CleanupUpscaleTemporaryFolders()
                            Return
                        End If
                        If LoadedSettings.BasicSettings.FixPS2 AndAlso File.Exists(OutputImage) Then AddPS2Alpha(OutputImage)
                        If WorkHorse.CancellationPending Then
                            CleanupUpscaleTemporaryFolders()
                            Return
                        End If
                        If IsFinalStage Then
                            MarkCheckpointedFinalTexture(LoadedSettings.Paths.OutputPath, ResumeCheckpoint,
                                OriginalInputByStem, StageCounts, FinalOutputsForBatch,
                                Path.GetFileNameWithoutExtension(NewImage), ChainList.Count)
                        End If
                    Next
                End If
                If IsFinalStage AndAlso Not WorkHorse.CancellationPending Then
                    For Each CopiedInput As String In CopiedInputs
                        If WorkHorse.CancellationPending Then Exit For
                        MarkCheckpointedFinalTexture(LoadedSettings.Paths.OutputPath, ResumeCheckpoint,
                            OriginalInputByStem, StageCounts, FinalOutputsForBatch,
                            Path.GetFileNameWithoutExtension(CopiedInput), ChainList.Count)
                    Next
                End If
                If WorkHorse.CancellationPending Then
                    CleanupUpscaleTemporaryFolders()
                    Return
                End If
            Next

            For Each ChainDir As String In DeletedChainPaths
                If Directory.Exists(ChainDir) Then Directory.Delete(ChainDir, True)
            Next
            Dim CompletedBatchInputs As New List(Of String)
            For Each CopiedInput As String In CopiedInputs
                Dim Stem As String = Path.GetFileNameWithoutExtension(CopiedInput)
                Dim CompletedStages As Integer = 0
                If ChainList.Count > 0 AndAlso StageCounts.TryGetValue(Stem, CompletedStages) AndAlso
                    CompletedStages = ChainList.Count AndAlso FinalOutputsForBatch.ContainsKey(Stem) Then
                    Dim FullInputPath As String = Path.GetFullPath(CopiedInput)
                    SuccessfullyProcessedInputs.Add(FullInputPath)
                    CompletedBatchInputs.Add(FullInputPath)
                End If
            Next
            If CompletedBatchInputs.Count > 0 Then
                MarkBatchComplete(LoadedSettings.Paths.OutputPath, ResumeCheckpoint, CompletedBatchInputs)
            End If

            Dim ProgressPercentage As Integer = CInt(Math.Floor((CurrentIndex * 100.0) / Source.Count))
            WorkHorse.ReportProgress(Math.Max(0, Math.Min(100, ProgressPercentage)))
        End While

        If WorkHorse.CancellationPending Then
            CleanupUpscaleTemporaryFolders()
            Return
        End If
        If LoadedSettings.ExpertSettings.ClearInput Then
            For Each SourceImage As String In SuccessfullyProcessedInputs
                If WorkHorse.CancellationPending Then Return
                If File.Exists(SourceImage) Then File.Delete(SourceImage)
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

    Private Sub CopyFiles(FileList As String(), RootPath As String, ByRef CurrentIndex As Integer,
                          BatchSize As Integer, CopiedInputs As List(Of String))
        Dim CopyCounter As Integer = 0
        Dim AlphaMode As Integer = LoadedSettings.ExpertSettings.AlphaMode
        Do While CurrentIndex < FileList.Count AndAlso CopyCounter < BatchSize AndAlso Not WorkHorse.CancellationPending
            Dim FilePath As String = FileList(CurrentIndex)
            If Not IsAlphaFiltered(FilePath, AlphaMode) Then
                Select Case AlphaMode
                    Case 0
                        File.Copy(FilePath, Path.Combine(RootPath, Path.GetFileName(FilePath)), True)
                        CopiedInputs.Add(FilePath)
                        CopyCounter += 1
                    Case 1
                        If Not GetHasTransparency(FilePath) Then
                            If WorkHorse.CancellationPending Then Exit Do
                            File.Copy(FilePath, Path.Combine(RootPath, Path.GetFileName(FilePath)), True)
                            CopiedInputs.Add(FilePath)
                            CopyCounter += 1
                        Else
                            MarkAlphaFiltered(FilePath, AlphaMode)
                        End If
                    Case 2
                        If GetHasTransparency(FilePath) Then
                            If WorkHorse.CancellationPending Then Exit Do
                            File.Copy(FilePath, Path.Combine(RootPath, Path.GetFileName(FilePath)), True)
                            CopiedInputs.Add(FilePath)
                            CopyCounter += 1
                        ElseIf Not WorkHorse.CancellationPending Then
                            MarkAlphaFiltered(FilePath, AlphaMode)
                        End If
                End Select
            End If
            CurrentIndex += 1
        Loop
    End Sub

    Private Sub StartBuilder(SourcePath As String, DestPath As String, ImageList As List(Of String),
                             Model As FormSettings.ChainObject,
                             Optional TextureCompletedCallback As Action(Of String) = Nothing)
        If ImageList.Count = 0 OrElse WorkHorse.CancellationPending Then Return

        Dim BuildProcess As ProcessStartInfo
        Dim IsSpandrelBackend As Boolean = IsSpandrelPackageType(Model.PackageType)
        Dim IsAutoRouteRun As Boolean = IsSpandrelBackend AndAlso
            TypeOf Model.Package Is FormSettings.PythonPackage AndAlso
            DirectCast(Model.Package, FormSettings.PythonPackage).AutoRouteEnabled
        Dim BackendDisplay As String = If(IsSpandrelBackend, "Spandrel", Model.PackageType)
        Dim ExpectedOutputExtensions As HashSet(Of String) = GetExpectedOutputExtensions(ImageList, Model.Package)
        Dim ProtectedOutputPaths As HashSet(Of String) = GetExistingOutputPaths(DestPath, ImageList, ExpectedOutputExtensions)
        Dim BackendTextureCompletedHandler As Action(Of String) = Nothing
        If IsSpandrelBackend AndAlso TextureCompletedCallback IsNot Nothing Then
            BackendTextureCompletedHandler = Sub(ReportedImagePath As String)
                Dim NormalizedReportedPath As String = ReportedImagePath.Replace("/"c, Path.DirectorySeparatorChar)
                Dim ReportedImageName As String = Path.GetFileName(NormalizedReportedPath)
                Dim MatchingInput As String = ImageList.FirstOrDefault(Function(Candidate As String) _
                    String.Equals(Path.GetFileName(Candidate), ReportedImageName, StringComparison.OrdinalIgnoreCase))
                If String.IsNullOrWhiteSpace(MatchingInput) Then Return
                Dim CompletedOutputPath As String = Path.Combine(DestPath, Path.GetFileName(MatchingInput))
                If Not ExpectedOutputExtensions.Contains(Path.GetExtension(CompletedOutputPath)) OrElse
                    Not IsNonEmptyFile(CompletedOutputPath) Then Return
                TextureCompletedCallback(Path.GetFileName(MatchingInput))
                ProtectedOutputPaths.Add(Path.GetFullPath(CompletedOutputPath))
            End Sub
        End If
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
            ConfigureCapturedProcess(BuildProcess)
            Dim Capture As ProcessOutputCapture = StartCapturedProcess(BuildProcess, BackendDisplay, LoadedSettings.Paths.OutputPath,
                                                                       If(IsSpandrelBackend, Nothing, ImageList(0)),
                                                                       IsSpandrelBackend, BackendTextureCompletedHandler)
            Try
                CompleteCapturedProcess(Capture)
                If WorkHorse.CancellationPending Then
                    DeleteCancelledOutputs(DestPath, ImageList, ExpectedOutputExtensions, ProtectedOutputPaths)
                    Return
                End If
                If LoadedSettings.ExpertSettings.Logging OrElse IsAutoRouteRun OrElse Capture.ExitCode <> 0 Then
                    WriteProcessLog(Capture.StartInfo, Capture.StandardOutput, Capture.StandardError,
                                    Capture.SaveLocation, Capture.BackendName, Capture.ExitCode)
                End If
                If Capture.ExitCode <> 0 Then
                    DeleteOutputsForInputs(DestPath, ImageList, ExpectedOutputExtensions, ProtectedOutputPaths)
                    Throw CreateBackendFailure(Capture)
                End If
                Try
                    ValidateBuilderOutputs(DestPath, ImageList, BackendDisplay, Model.Package)
                Catch
                    DeleteOutputsForInputs(DestPath, ImageList, ExpectedOutputExtensions, ProtectedOutputPaths)
                    Throw
                End Try
            Catch
                If Not Capture.ActiveProcess.HasExited Then TerminateProcessTree(Capture.ActiveProcess)
                Throw
            Finally
                UnregisterActiveProcess(Capture.ActiveProcess)
                Capture.ActiveProcess.Dispose()
            End Try
        Else
            Dim Captures As New List(Of ProcessOutputCapture)
            Try
                For Each ImagePath As String In ImageList
                    If WorkHorse.CancellationPending Then
                        StopActiveProcesses()
                        Exit For
                    End If
                    Dim NewImage As String = Path.Combine(DestPath, Path.GetFileName(ImagePath))
                    BuildProcess = New ProcessStartInfo(Root & Model.FileLocation,
                        MakeCommand(ImagePath, NewImage, Model.PackageType, Model.Package))
                    BuildProcess.WorkingDirectory = Directory.GetParent(Root & Model.FileLocation).FullName
                    ConfigureCapturedProcess(BuildProcess)
                    Captures.Add(StartCapturedProcess(BuildProcess, BackendDisplay, LoadedSettings.Paths.OutputPath, ImagePath, False))
                Next

                If WorkHorse.CancellationPending Then StopActiveProcesses()
                For Each Capture As ProcessOutputCapture In Captures
                    CompleteCapturedProcess(Capture)
                Next
                If WorkHorse.CancellationPending Then
                    DeleteCancelledOutputs(DestPath, ImageList, ExpectedOutputExtensions, ProtectedOutputPaths)
                    Return
                End If

                Dim FailureDetails As New List(Of String)
                For Each Capture As ProcessOutputCapture In Captures
                    If LoadedSettings.ExpertSettings.Logging OrElse Capture.ExitCode <> 0 Then
                        WriteProcessLog(Capture.StartInfo, Capture.StandardOutput, Capture.StandardError,
                                        Capture.SaveLocation, Capture.BackendName, Capture.ExitCode)
                    End If
                    If Capture.ExitCode <> 0 Then
                        FailureDetails.Add(CreateBackendFailure(Capture).Message)
                    End If
                Next
                If FailureDetails.Count > 0 Then
                    DeleteOutputsForInputs(DestPath, ImageList, ExpectedOutputExtensions, ProtectedOutputPaths)
                    Throw New InvalidOperationException(String.Join(Environment.NewLine, FailureDetails.ToArray()))
                End If
                Try
                    ValidateBuilderOutputs(DestPath, ImageList, BackendDisplay, Model.Package)
                Catch
                    DeleteOutputsForInputs(DestPath, ImageList, ExpectedOutputExtensions, ProtectedOutputPaths)
                    Throw
                End Try
            Catch
                StopAndWaitForCapturedProcesses(Captures)
                Throw
            Finally
                For Each Capture As ProcessOutputCapture In Captures
                    UnregisterActiveProcess(Capture.ActiveProcess)
                    Capture.ActiveProcess.Dispose()
                Next
            End Try
        End If

        If WorkHorse.CancellationPending Then Return
        For Each TempImage As String In Directory.GetFiles(SourcePath)
            If WorkHorse.CancellationPending Then Return
            File.Delete(TempImage)
        Next
    End Sub

    Private Sub ConfigureCapturedProcess(StartInfo As ProcessStartInfo)
        StartInfo.RedirectStandardOutput = True
        StartInfo.RedirectStandardError = True
        StartInfo.UseShellExecute = False
        StartInfo.CreateNoWindow = True
    End Sub

    Private Function StartCapturedProcess(StartInfo As ProcessStartInfo, BackendName As String, SaveLocation As String,
                                          InputPath As String, ParseProgress As Boolean,
                                          Optional TextureCompletedCallback As Action(Of String) = Nothing) As ProcessOutputCapture
        Dim ActiveProcess As Process = Process.Start(StartInfo)
        If ActiveProcess Is Nothing Then Throw New InvalidOperationException("Failed to start " & BackendName & ".")
        Dim OutputTask As Task(Of String) = Nothing
        Dim ErrorTask As Task(Of String) = Nothing
        Try
            RegisterActiveProcess(ActiveProcess)
            If ParseProgress Then
                OutputTask = Task.Run(Function() ReadSpandrelOutput(ActiveProcess, TextureCompletedCallback))
            Else
                OutputTask = ActiveProcess.StandardOutput.ReadToEndAsync()
            End If
            ErrorTask = ActiveProcess.StandardError.ReadToEndAsync()
            Return New ProcessOutputCapture With {
                .ActiveProcess = ActiveProcess,
                .StartInfo = StartInfo,
                .StandardOutputTask = OutputTask,
                .StandardErrorTask = ErrorTask,
                .BackendName = BackendName,
                .SaveLocation = SaveLocation,
                .InputPath = InputPath
            }
        Catch
            TerminateProcessTree(ActiveProcess)
            Try
                If Not ActiveProcess.HasExited Then ActiveProcess.WaitForExit(3000)
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine("Could not wait for failed backend startup: " & ex.Message)
            End Try
            WaitForCapturedStreamTask(OutputTask)
            WaitForCapturedStreamTask(ErrorTask)
            UnregisterActiveProcess(ActiveProcess)
            ActiveProcess.Dispose()
            Throw
        End Try
    End Function

    Private Sub CompleteCapturedProcess(Capture As ProcessOutputCapture)
        WaitForActiveProcess(Capture.ActiveProcess)
        Capture.StandardOutput = Capture.StandardOutputTask.Result
        Capture.StandardError = Capture.StandardErrorTask.Result
        Capture.ExitCode = Capture.ActiveProcess.ExitCode
    End Sub

    Private Function CreateBackendFailure(Capture As ProcessOutputCapture) As InvalidOperationException
        Dim Details As String = If(Capture.StandardError.Trim() <> String.Empty,
                                   Capture.StandardError.Trim(), Capture.StandardOutput.Trim())
        If Details.Length > 2000 Then Details = Details.Substring(0, 2000) & "..."
        Dim InputDescription As String = If(String.IsNullOrWhiteSpace(Capture.InputPath),
                                            String.Empty, " for " & Path.GetFileName(Capture.InputPath))
        Return New InvalidOperationException(Capture.BackendName & InputDescription &
            " failed (exit code " & Capture.ExitCode.ToString() & "). " & Details)
    End Function

    Private Function GetExpectedOutputExtensions(ImageList As List(Of String), Package As Object) As HashSet(Of String)
        Dim Result As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Dim PackageExtensions As HashSet(Of String) = GetPackageInputFileTypes(Package)
        If PackageExtensions IsNot Nothing Then Result.UnionWith(PackageExtensions)
        For Each InputImage As String In ImageList
            Result.Add(Path.GetExtension(InputImage))
        Next
        Return Result
    End Function

    Private Sub ValidateBuilderOutputs(DestPath As String, ImageList As List(Of String), BackendName As String, Package As Object)
        Dim ExpectedExtensions As HashSet(Of String) = GetExpectedOutputExtensions(ImageList, Package)
        Dim OutputsByStem As Dictionary(Of String, String) = GetNonEmptyOutputsByStem(DestPath, ExpectedExtensions)
        For Each InputImage As String In ImageList
            If WorkHorse.CancellationPending Then Return
            If Not OutputsByStem.ContainsKey(Path.GetFileNameWithoutExtension(InputImage)) Then
                Throw New InvalidOperationException(BackendName & " exited successfully but produced no non-empty image output for " &
                    Path.GetFileName(InputImage) & ". The source was preserved.")
            End If
        Next
    End Sub

    Private Function GetNonEmptyOutputsByStem(OutputFolder As String,
                                              Optional AllowedExtensions As HashSet(Of String) = Nothing) As Dictionary(Of String, String)
        Dim Result As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        If Not Directory.Exists(OutputFolder) Then Return Result
        For Each Candidate As String In Directory.GetFiles(OutputFolder, "*.*", SearchOption.AllDirectories)
            If AllowedExtensions IsNot Nothing AndAlso AllowedExtensions.Count > 0 AndAlso
                Not AllowedExtensions.Contains(Path.GetExtension(Candidate)) Then Continue For
            Dim Stem As String = Path.GetFileNameWithoutExtension(Candidate)
            If Result.ContainsKey(Stem) Then Continue For
            Try
                If New FileInfo(Candidate).Length > 0 Then Result.Add(Stem, Candidate)
            Catch ex As IOException
                ' An output may be replaced while a watcher is scanning.
            Catch ex As UnauthorizedAccessException
                ' Ignore outputs we cannot inspect and keep searching.
            End Try
        Next
        Return Result
    End Function

    Private Function GetExistingOutputPaths(OutputFolder As String, ImageList As List(Of String),
                                            AllowedExtensions As HashSet(Of String)) As HashSet(Of String)
        Dim Result As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        If Not Directory.Exists(OutputFolder) Then Return Result
        Dim InputStems As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each ImagePath As String In ImageList
            InputStems.Add(Path.GetFileNameWithoutExtension(ImagePath))
        Next
        For Each Candidate As String In Directory.GetFiles(OutputFolder, "*.*", SearchOption.AllDirectories)
            If InputStems.Contains(Path.GetFileNameWithoutExtension(Candidate)) AndAlso
                (AllowedExtensions Is Nothing OrElse AllowedExtensions.Count = 0 OrElse
                 AllowedExtensions.Contains(Path.GetExtension(Candidate))) Then
                Result.Add(Path.GetFullPath(Candidate))
            End If
        Next
        Return Result
    End Function

    Private Sub DeleteOutputsForInputs(DestPath As String, ImageList As IEnumerable(Of String),
                                       AllowedExtensions As HashSet(Of String), ProtectedOutputPaths As HashSet(Of String))
        If Not Directory.Exists(DestPath) Then Return
        Dim Stems As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each ImagePath As String In ImageList
            Stems.Add(Path.GetFileNameWithoutExtension(ImagePath))
        Next
        For Each Candidate As String In Directory.GetFiles(DestPath, "*.*", SearchOption.AllDirectories)
            If Not Stems.Contains(Path.GetFileNameWithoutExtension(Candidate)) Then Continue For
            If AllowedExtensions IsNot Nothing AndAlso AllowedExtensions.Count > 0 AndAlso
                Not AllowedExtensions.Contains(Path.GetExtension(Candidate)) Then Continue For
            If ProtectedOutputPaths IsNot Nothing AndAlso ProtectedOutputPaths.Contains(Path.GetFullPath(Candidate)) Then Continue For
            Try
                File.Delete(Candidate)
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine("Could not remove incomplete backend output: " & ex.Message)
            End Try
        Next
    End Sub

    Private Sub StopAndWaitForCapturedProcesses(Captures As List(Of ProcessOutputCapture))
        For Each Capture As ProcessOutputCapture In Captures
            Try
                TerminateProcessTree(Capture.ActiveProcess)
                If Not Capture.ActiveProcess.HasExited Then Capture.ActiveProcess.WaitForExit(3000)
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine("Could not stop backend process: " & ex.Message)
            End Try
            WaitForCapturedStreamTask(Capture.StandardOutputTask)
            WaitForCapturedStreamTask(Capture.StandardErrorTask)
        Next
    End Sub

    Private Sub WaitForCapturedStreamTask(StreamTask As Task)
        If StreamTask Is Nothing Then Return
        Try
            StreamTask.Wait(5000)
        Catch ex As AggregateException
            System.Diagnostics.Debug.WriteLine("Could not finish captured backend output: " & ex.GetBaseException().Message)
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("Could not finish captured backend output: " & ex.Message)
        End Try
    End Sub

    Private Function ReadSpandrelOutput(SpandrelProcess As Process,
                                        Optional TextureCompletedCallback As Action(Of String) = Nothing) As String
        Const CompletedPrefix As String = "AUTOCRISPY_TEXTURE_COMPLETED:"
        Dim CapturedOutput As New System.Text.StringBuilder()
        Dim LastProgressUpdate As DateTime = DateTime.MinValue
        While True
            Dim OutputLine As String = SpandrelProcess.StandardOutput.ReadLine()
            If OutputLine Is Nothing Then Exit While
            CapturedOutput.AppendLine(OutputLine)
            If OutputLine.StartsWith(CompletedPrefix, StringComparison.Ordinal) AndAlso TextureCompletedCallback IsNot Nothing Then
                Try
                    Dim EncodedImageName As String = OutputLine.Substring(CompletedPrefix.Length).Trim()
                    Dim CompletedImageName As String = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(EncodedImageName))
                    TextureCompletedCallback(CompletedImageName)
                Catch ex As Exception
                    System.Diagnostics.Debug.WriteLine("Could not persist a completed Spandrel texture: " & ex.GetBaseException().Message)
                End Try
            End If
            If OutputLine.StartsWith("AUTOCRISPY_PROGRESS:", StringComparison.Ordinal) AndAlso
                (DateTime.UtcNow - LastProgressUpdate).TotalMilliseconds >= 500 Then
                WorkHorse.ReportProgress(0, OutputLine.Substring("AUTOCRISPY_PROGRESS:".Length).Trim())
                LastProgressUpdate = DateTime.UtcNow
            End If
        End While
        Return CapturedOutput.ToString()
    End Function

    Private Sub RegisterActiveProcess(ActiveProcess As Process)
        Dim JobHandle As IntPtr = CreateKillOnCloseJob()
        If JobHandle <> IntPtr.Zero Then
            Try
                If AssignProcessToJobObject(JobHandle, ActiveProcess.Handle) Then
                    SyncLock ActiveProcessLock
                        ProcessJobHandles(ActiveProcess) = JobHandle
                    End SyncLock
                Else
                    CloseHandle(JobHandle)
                    System.Diagnostics.Debug.WriteLine("Could not assign process to a kill-on-close job object; taskkill fallback will be used.")
                End If
            Catch ex As Exception
                CloseHandle(JobHandle)
                System.Diagnostics.Debug.WriteLine("Could not create process job membership: " & ex.Message)
            End Try
        End If

        SyncLock ActiveProcessLock
            ActiveProcesses.Add(ActiveProcess)
        End SyncLock
        If WorkHorse.CancellationPending Then StopActiveProcesses()
    End Sub

    Private Function CreateKillOnCloseJob() As IntPtr
        Dim JobHandle As IntPtr = IntPtr.Zero
        Dim InfoPointer As IntPtr = IntPtr.Zero
        Try
            JobHandle = CreateJobObject(IntPtr.Zero, Nothing)
            If JobHandle = IntPtr.Zero Then Return IntPtr.Zero
            Dim Info As New JobObjectExtendedLimitInformation
            Info.BasicLimitInformation.LimitFlags = &H2000UI ' JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE
            Dim InfoSize As Integer = Runtime.InteropServices.Marshal.SizeOf(GetType(JobObjectExtendedLimitInformation))
            InfoPointer = Runtime.InteropServices.Marshal.AllocHGlobal(InfoSize)
            Runtime.InteropServices.Marshal.StructureToPtr(Info, InfoPointer, False)
            If Not SetInformationJobObject(JobHandle, 9, InfoPointer, CUInt(InfoSize)) Then
                CloseHandle(JobHandle)
                Return IntPtr.Zero
            End If
            Return JobHandle
        Catch ex As Exception
            If JobHandle <> IntPtr.Zero Then CloseHandle(JobHandle)
            System.Diagnostics.Debug.WriteLine("Could not configure process job object: " & ex.Message)
            Return IntPtr.Zero
        Finally
            If InfoPointer <> IntPtr.Zero Then Runtime.InteropServices.Marshal.FreeHGlobal(InfoPointer)
        End Try
    End Function

    Private Sub UnregisterActiveProcess(ActiveProcess As Process)
        Dim JobHandle As IntPtr = IntPtr.Zero
        SyncLock ActiveProcessLock
            ActiveProcesses.Remove(ActiveProcess)
            ProcessesBeingTerminated.Remove(ActiveProcess)
            If ProcessJobHandles.TryGetValue(ActiveProcess, JobHandle) Then ProcessJobHandles.Remove(ActiveProcess)
        End SyncLock
        If JobHandle <> IntPtr.Zero Then
            ' Closing a kill-on-close job also reaps any descendants the backend left running.
            If Not CloseHandle(JobHandle) Then
                System.Diagnostics.Debug.WriteLine("Could not close backend job handle: " & Runtime.InteropServices.Marshal.GetLastWin32Error().ToString())
            End If
        End If
    End Sub

    Private Sub StopActiveProcesses()
        Dim ProcessesToStop As New List(Of Process)
        SyncLock ActiveProcessLock
            For Each ActiveProcess As Process In ActiveProcesses
                If ProcessesBeingTerminated.Add(ActiveProcess) Then ProcessesToStop.Add(ActiveProcess)
            Next
        End SyncLock
        For Each ActiveProcess As Process In ProcessesToStop
            Dim ProcessToStop As Process = ActiveProcess
            Task.Run(Sub()
                         Try
                             TerminateProcessTree(ProcessToStop)
                         Catch ex As Exception
                             System.Diagnostics.Debug.WriteLine("Could not terminate backend process tree: " & ex.Message)
                         Finally
                             SyncLock ActiveProcessLock
                                 ProcessesBeingTerminated.Remove(ProcessToStop)
                             End SyncLock
                         End Try
                     End Sub)
        Next
    End Sub

    Private Sub TerminateProcessTree(ActiveProcess As Process)
        Dim JobHandle As IntPtr = IntPtr.Zero
        SyncLock ActiveProcessLock
            ProcessJobHandles.TryGetValue(ActiveProcess, JobHandle)
        End SyncLock

        Dim JobTerminated As Boolean = False
        If JobHandle <> IntPtr.Zero Then
            Try
                JobTerminated = TerminateJobObject(JobHandle, 1UI)
                If Not JobTerminated Then
                    System.Diagnostics.Debug.WriteLine("Could not terminate backend job: " & Runtime.InteropServices.Marshal.GetLastWin32Error().ToString())
                End If
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine("Could not terminate backend job: " & ex.Message)
            End Try
        End If

        If Not JobTerminated Then
            Try
                If Not ActiveProcess.HasExited Then
                    Dim TaskKillPath As String = Path.Combine(Environment.SystemDirectory, "taskkill.exe")
                    If File.Exists(TaskKillPath) Then
                        Using TreeKiller As New Process
                            TreeKiller.StartInfo = New ProcessStartInfo With {
                                .FileName = TaskKillPath,
                                .Arguments = "/PID " & ActiveProcess.Id.ToString(CultureInfo.InvariantCulture) & " /T /F",
                                .UseShellExecute = False,
                                .CreateNoWindow = True
                            }
                            If TreeKiller.Start() Then
                                If Not TreeKiller.WaitForExit(5000) Then
                                    Try
                                        TreeKiller.Kill()
                                    Catch ex As Exception
                                        System.Diagnostics.Debug.WriteLine("Could not stop taskkill helper: " & ex.Message)
                                    End Try
                                End If
                            End If
                        End Using
                    End If
                End If
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine("taskkill process-tree fallback failed: " & ex.Message)
            End Try
        End If
        Try
            If Not ActiveProcess.HasExited Then
                ActiveProcess.Kill()
                ActiveProcess.WaitForExit(3000)
            End If
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("Could not terminate backend process: " & ex.Message)
        End Try
    End Sub

    Private Sub WaitForActiveProcess(ActiveProcess As Process)
        While Not ActiveProcess.WaitForExit(200)
            If WorkHorse.CancellationPending Then StopActiveProcesses()
        End While
    End Sub

    Private Sub DeleteCancelledOutputs(DestPath As String, ImageList As List(Of String),
                                       AllowedExtensions As HashSet(Of String), ProtectedOutputPaths As HashSet(Of String))
        DeleteOutputsForInputs(DestPath, ImageList, AllowedExtensions, ProtectedOutputPaths)
    End Sub

    Private Sub CleanupUpscaleTemporaryFolders()
        CleanupRunTemporaryRoot()
    End Sub

    Private Function CreateRunTempRoot() As String
        Dim TempParent As String = Path.Combine(Path.GetTempPath(), "AutoCrispy")
        Directory.CreateDirectory(TempParent)
        Dim TempRoot As String = Path.Combine(TempParent, Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(TempRoot)
        Return TempRoot
    End Function

    Private Sub CleanupRunTemporaryRoot()
        Dim TempRoot As String = CurrentRunTempRoot
        CurrentRunTempRoot = String.Empty
        If String.IsNullOrWhiteSpace(TempRoot) Then Return
        Try
            If Directory.Exists(TempRoot) Then Directory.Delete(TempRoot, True)
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("Could not remove worker temporary folder: " & ex.Message)
        End Try
    End Sub

    Private Function IsSpandrelPackageType(PackageType As String) As Boolean
        Return PackageType = PLKSRBackendName OrElse PackageType = "RealPLKSR" OrElse
            PackageType = DAT2BackendName OrElse PackageType = SpandrelBackendName
    End Function

    Private Function GetChainPath(PathType As String, PathIndex As Integer) As String
        If String.IsNullOrWhiteSpace(CurrentRunTempRoot) Then CurrentRunTempRoot = CreateRunTempRoot()
        Return Path.Combine(CurrentRunTempRoot, PathType & "_" & PathIndex.ToString(CultureInfo.InvariantCulture) & "_" & LoadedSettings.ExpertSettings.AlphaMode.ToString(CultureInfo.InvariantCulture))
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

    Private Sub AddModelToChain(Mode As String, Optional ForceAppend As Boolean = False)
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
                Dim PLKSRStepName As String = AppendNormalMapModeSuffix("PLKSR 4x", GetSelectedNormalMapMode())
                ChainControl.ListItems.Add(New DragDropList.DragDropItem(ChainList.Count, PLKSRStepName, ChainThumbs.Item(6)))
                ChainList.Add(New FormSettings.ChainObject(PLKSRStepName, 6, "", "RealPLKSR", Me))
            Case DAT2BackendName
                Dim DAT2StepName As String = AppendNormalMapModeSuffix("PBRify V4 DAT2 4x", GetSelectedNormalMapMode())
                ChainControl.ListItems.Add(New DragDropList.DragDropItem(ChainList.Count, DAT2StepName, ChainThumbs.Item(6)))
                ChainList.Add(New FormSettings.ChainObject(DAT2StepName, 6, "", DAT2BackendName, Me))
            Case SpandrelBackendName
                Dim SelectedPackage As FormSettings.PythonPackage = GetSelectedPythonPackage()
                Dim ModelDisplayName As String = "Spandrel - " & Path.GetFileName(SelectedPackage.Model)
                If SelectedPackage.AutoRouteEnabled Then
                    ModelDisplayName = "Spandrel - Auto (" & Path.GetFileName(SelectedPackage.ArchitectModel) & " / " &
                        Path.GetFileName(SelectedPackage.PainterModel) & ", " & CInt(AutoPainterSharePercent.Value).ToString() & "% Painter)"
                End If
                ModelDisplayName = AppendNormalMapModeSuffix(ModelDisplayName, SelectedPackage.NormalMapMode)
                Dim UpdatedChainItem As New FormSettings.ChainObject(ModelDisplayName, 6, "", SpandrelBackendName, Me)
                Dim ExistingSpandrelIndex As Integer = If(ForceAppend, -1, FindLatestSpandrelChainItemIndex())
                If ExistingSpandrelIndex >= 0 Then
                    ChainList(ExistingSpandrelIndex) = UpdatedChainItem
                    If ExistingSpandrelIndex < ChainControl.ListItems.Count AndAlso ChainControl.ListItems.Count = ChainList.Count Then
                        ChainControl.ListItems(ExistingSpandrelIndex) = New DragDropList.DragDropItem(ExistingSpandrelIndex, ModelDisplayName, ChainThumbs.Item(6))
                    Else
                        ChainControl.ListItems.Clear()
                        For i As Integer = 0 To ChainList.Count - 1
                            ChainControl.ListItems.Add(New DragDropList.DragDropItem(i, ChainList(i).Name, ChainThumbs.Item(ChainList(i).IconIndex)))
                        Next
                    End If
                Else
                    ChainControl.ListItems.Add(New DragDropList.DragDropItem(ChainList.Count, ModelDisplayName, ChainThumbs.Item(6)))
                    ChainList.Add(UpdatedChainItem)
                End If
        End Select
        If ChainControl.ListItems.Count > 0 Then
            Dim FocusIndex As Integer = ChainControl.ListItems.Count - 1
            If String.Equals(Mode, SpandrelBackendName, StringComparison.OrdinalIgnoreCase) AndAlso Not ForceAppend Then
                Dim ExistingSpandrelIndex As Integer = FindLatestSpandrelChainItemIndex()
                If ExistingSpandrelIndex >= 0 Then FocusIndex = ExistingSpandrelIndex
            End If
            ChainControl.SelectIndex(FocusIndex)
        Else
            ChainControl.DrawList(ChainControl.ListItems)
        End If
        UpdateChainAddButtonState()
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
        Dim Precision As String = If(String.IsNullOrWhiteSpace(Package.Precision), "auto", Package.Precision.Trim().ToLowerInvariant())
        If Precision <> "auto" AndAlso Precision <> "fp16" AndAlso Precision <> "fp32" Then Precision = "auto"
        Result.AddArguement("--precision", Precision)
        Dim NormalMapMode As String = If(String.IsNullOrWhiteSpace(Package.NormalMapMode), "none", Package.NormalMapMode.Trim().ToLowerInvariant())
        If NormalMapMode <> "normalize-xyz" AndAlso NormalMapMode <> "rebuild-z" Then NormalMapMode = "none"
        Result.AddArguement("--normal-map-mode", NormalMapMode)
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
        If WorkHorse.CancellationPending OrElse Mirrored <= 0 Then Return New Bitmap(Source)
        Using Result As New Bitmap(Source.Width * 3, Source.Height * 3, Source.PixelFormat)
            Using g As Graphics = Graphics.FromImage(Result)
                g.CompositingMode = Drawing2D.CompositingMode.SourceCopy
                g.PixelOffsetMode = Drawing2D.PixelOffsetMode.None
                g.SmoothingMode = Drawing2D.SmoothingMode.None
                g.InterpolationMode = Drawing2D.InterpolationMode.NearestNeighbor
                If Mirrored = 2 Then
                    Dim X As Integer = Source.Width
                    Dim Y As Integer = Source.Height
                    Using fX As New Bitmap(Source), fY As New Bitmap(Source), fXY As New Bitmap(Source)
                        fX.RotateFlip(RotateFlipType.RotateNoneFlipX)
                        fY.RotateFlip(RotateFlipType.RotateNoneFlipY)
                        fXY.RotateFlip(RotateFlipType.RotateNoneFlipXY)
                        g.DrawImage(fXY, 0, 0, X, Y) : g.DrawImage(fY, X, 0, X, Y) : g.DrawImage(fXY, 2 * X, 0, X, Y)
                        g.DrawImage(fX, 0, Y, X, Y) : g.DrawImage(Source, X, Y, X, Y) : g.DrawImage(fX, 2 * X, Y, X, Y)
                        g.DrawImage(fXY, 0, 2 * Y, X, Y) : g.DrawImage(fY, X, 2 * Y, X, Y) : g.DrawImage(fXY, 2 * X, 2 * Y, X, Y)
                    End Using
                ElseIf Mirrored = 1 Then
                    For i As Integer = 0 To Source.Width * 2 Step Source.Width
                        If WorkHorse.CancellationPending Then Return New Bitmap(Source)
                        For j As Integer = 0 To Source.Height * 2 Step Source.Height
                            g.DrawImage(Source, i, j, Source.Width, Source.Height)
                        Next
                    Next
                Else
                    Return New Bitmap(Source)
                End If
            End Using
            If WorkHorse.CancellationPending Then Return New Bitmap(Source)
            Return CropImage(Result, Source.Width, Source.Height, Source.Width, Source.Height, Margin)
        End Using
    End Function

    Private Function GetHasTransparency(Source As String) As Boolean
        Using SourceImage As Bitmap = GetUnlockedImage(Source)
            Using ArgbImage As New Bitmap(SourceImage.Width, SourceImage.Height, Imaging.PixelFormat.Format32bppArgb)
                Using ConversionGraphics As System.Drawing.Graphics = System.Drawing.Graphics.FromImage(ArgbImage)
                    ConversionGraphics.CompositingMode = Drawing2D.CompositingMode.SourceCopy
                    ConversionGraphics.DrawImage(SourceImage, New Rectangle(0, 0, ArgbImage.Width, ArgbImage.Height))
                End Using

                Dim SourceRect As New Rectangle(0, 0, ArgbImage.Width, ArgbImage.Height)
                Dim SourceData As Imaging.BitmapData = Nothing
                Try
                    SourceData = ArgbImage.LockBits(SourceRect, Imaging.ImageLockMode.ReadOnly,
                                                     Imaging.PixelFormat.Format32bppArgb)
                    For Y As Integer = 0 To ArgbImage.Height - 1
                        If WorkHorse.CancellationPending Then Return False
                        Dim RowOffset As Integer = Y * SourceData.Stride
                        For X As Integer = 0 To ArgbImage.Width - 1
                            Dim Alpha As Byte = Runtime.InteropServices.Marshal.ReadByte(
                                SourceData.Scan0, RowOffset + (X * 4) + 3)
                            If Alpha < 255 Then Return True
                        Next
                    Next
                Finally
                    If SourceData IsNot Nothing Then ArgbImage.UnlockBits(SourceData)
                End Try
            End Using
        End Using
        Return False
    End Function

    Private Function CropImage(Source As Bitmap, OffsetX As Integer, OffsetY As Integer, Width As Integer, Height As Integer, Margins As Integer) As Bitmap
        If WorkHorse.CancellationPending Then Return New Bitmap(Source)
        Dim CropSize As New Rectangle(OffsetX - Margins, OffsetY - Margins, Width + (2 * Margins), Height + (2 * Margins))
        Dim Result As New Bitmap(CropSize.Width, CropSize.Height, Source.PixelFormat)
        Try
            Using g As Graphics = Graphics.FromImage(Result)
                g.CompositingMode = Drawing2D.CompositingMode.SourceCopy
                g.PixelOffsetMode = Drawing2D.PixelOffsetMode.None
                g.SmoothingMode = Drawing2D.SmoothingMode.None
                g.InterpolationMode = Drawing2D.InterpolationMode.NearestNeighbor
                g.DrawImage(Source, New Rectangle(0, 0, CropSize.Width, CropSize.Height), CropSize, GraphicsUnit.Pixel)
            End Using
            If WorkHorse.CancellationPending Then
                Result.Dispose()
                Return New Bitmap(Source)
            End If
            Return Result
        Catch
            Result.Dispose()
            Throw
        End Try
    End Function

    Private Sub Defringe(Source As String, Threshold As Integer)
        Using SourceImage As Bitmap = GetUnlockedImage(Source)
            Using NewImage As New DirectBitmap(SourceImage)
                For Y As Integer = 0 To NewImage.Height - 1
                    If WorkHorse.CancellationPending Then Return
                    For X As Integer = 0 To NewImage.Width - 1
                        If NewImage.GetPixel(X, Y).A < Threshold Then
                            NewImage.SetPixel(X, Y, Color.Transparent)
                        End If
                    Next
                Next
                If Not WorkHorse.CancellationPending Then NewImage.Bitmap.Save(Source)
            End Using
        End Using
    End Sub

    Private Sub RemovePS2Alpha(Source As String)
        Using SourceImage As Bitmap = GetUnlockedImage(Source)
            Using NewImage As New DirectBitmap(SourceImage)
                Dim AlphaMax As Integer = 0
                For Y As Integer = 0 To NewImage.Height - 1
                    If WorkHorse.CancellationPending Then Return
                    For X As Integer = 0 To NewImage.Width - 1
                        Dim TempColor As Color = NewImage.GetPixel(X, Y)
                        If TempColor.A > AlphaMax Then AlphaMax = TempColor.A
                        If AlphaMax > 128 Then Return
                        If TempColor.A <> 0 Then
                            NewImage.SetPixel(X, Y, Color.FromArgb((TempColor.A * 2) - 1,
                                TempColor.R, TempColor.G, TempColor.B))
                        End If
                    Next
                Next
                If Not WorkHorse.CancellationPending Then NewImage.Bitmap.Save(Source)
            End Using
        End Using
    End Sub

    Private Sub AddPS2Alpha(Source As String)
        Using SourceImage As Bitmap = GetUnlockedImage(Source)
            Using NewImage As New DirectBitmap(SourceImage)
                For Y As Integer = 0 To NewImage.Height - 1
                    If WorkHorse.CancellationPending Then Return
                    For X As Integer = 0 To NewImage.Width - 1
                        Dim TempColor As Color = NewImage.GetPixel(X, Y)
                        If TempColor.A <> 0 Then
                            NewImage.SetPixel(X, Y, Color.FromArgb((TempColor.A + 1) / 2,
                                TempColor.R, TempColor.G, TempColor.B))
                        End If
                    Next
                Next
                If Not WorkHorse.CancellationPending Then NewImage.Bitmap.Save(Source)
            End Using
        End Using
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
                Try
                    If New FileInfo(DoneFile).Length > 0 Then DoneNames.Add(Path.GetFileNameWithoutExtension(DoneFile))
                Catch ex As IOException
                    ' A file still being written is not a completed output.
                Catch ex As UnauthorizedAccessException
                    ' Do not count an unreadable output as complete.
                End Try
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

    Private Function GetResumeCheckpointPath(OutputFolder As String) As String
        If String.IsNullOrWhiteSpace(OutputFolder) Then Throw New ArgumentException("An output folder is required for a resume checkpoint.", NameOf(OutputFolder))
        Dim NormalizedOutputPath As String = Path.GetFullPath(OutputFolder)
        Dim OutputRoot As String = Path.GetPathRoot(NormalizedOutputPath)
        If Not String.Equals(NormalizedOutputPath, OutputRoot, StringComparison.OrdinalIgnoreCase) Then
            NormalizedOutputPath = NormalizedOutputPath.TrimEnd(New Char() {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar})
        End If
        NormalizedOutputPath = NormalizedOutputPath.ToUpperInvariant()
        Using Hasher As System.Security.Cryptography.SHA256 = System.Security.Cryptography.SHA256.Create()
            Dim PathHash As Byte() = Hasher.ComputeHash(System.Text.Encoding.UTF8.GetBytes(NormalizedOutputPath))
            Dim CheckpointName As String = BitConverter.ToString(PathHash).Replace("-", String.Empty) & ".xml"
            Return Path.Combine(AppData, "AutoCrispy", "Resume", CheckpointName)
        End Using
    End Function

    Private Function GetResumeCompletedLogPath(OutputFolder As String) As String
        Return GetResumeCheckpointPath(OutputFolder) & ".completed"
    End Function

    Private Function GetResumeEntryKey(AttemptId As String, InputPath As String) As String
        Return If(AttemptId, String.Empty) & "|" & Path.GetFullPath(InputPath)
    End Function

    Private Sub ClearResumeCompletedLog(OutputFolder As String)
        Dim CompletedLogPath As String = GetResumeCompletedLogPath(OutputFolder)
        If Not File.Exists(CompletedLogPath) Then Return
        Try
            File.Delete(CompletedLogPath)
        Catch ex As Exception
            ' Attempt IDs prevent old records from applying to later processing attempts.
            System.Diagnostics.Debug.WriteLine("Could not compact completed-texture resume log: " & ex.Message)
        End Try
    End Sub

    Private Function LoadResumeCheckpoint(OutputFolder As String) As BatchResumeCheckpoint
        Dim CheckpointPath As String = GetResumeCheckpointPath(OutputFolder)
        SyncLock ResumeCheckpointLock
            If Not File.Exists(CheckpointPath) Then Return New BatchResumeCheckpoint()
            Try
                Dim Checkpoint As BatchResumeCheckpoint = Nothing
                Using CheckpointStream As New FileStream(CheckpointPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite Or FileShare.Delete)
                    Dim Serializer As New Xml.Serialization.XmlSerializer(GetType(BatchResumeCheckpoint))
                    Checkpoint = DirectCast(Serializer.Deserialize(CheckpointStream), BatchResumeCheckpoint)
                End Using
                If Checkpoint Is Nothing OrElse Checkpoint.Version < 1 OrElse Checkpoint.Version > 2 Then
                    Throw New InvalidDataException("The resume checkpoint version is not supported.")
                End If
                If Checkpoint.InProgress Is Nothing Then Checkpoint.InProgress = New List(Of BatchResumeEntry)
                For Each Entry As BatchResumeEntry In Checkpoint.InProgress
                    Dim ParsedAttemptId As Guid
                    If Entry Is Nothing OrElse String.IsNullOrWhiteSpace(Entry.InputPath) OrElse
                        Entry.OutputExtensions Is Nothing OrElse Entry.OutputExtensions.Count = 0 OrElse
                        (Not String.IsNullOrWhiteSpace(Entry.AttemptId) AndAlso Not Guid.TryParse(Entry.AttemptId, ParsedAttemptId)) Then
                        Throw New InvalidDataException("The resume checkpoint contains an incomplete entry.")
                    End If
                Next
                Checkpoint.Version = 2
                Return Checkpoint
            Catch ex As Exception
                Throw New InvalidDataException("AutoCrispy could not read the saved resume checkpoint at " & CheckpointPath &
                    ". Processing has been stopped to avoid treating an incomplete texture as finished.", ex)
            End Try
        End SyncLock
    End Function

    Private Sub SaveResumeCheckpoint(OutputFolder As String, Checkpoint As BatchResumeCheckpoint)
        Dim CheckpointPath As String = GetResumeCheckpointPath(OutputFolder)
        SyncLock ResumeCheckpointLock
            If Checkpoint.InProgress Is Nothing Then Checkpoint.InProgress = New List(Of BatchResumeEntry)
            If Checkpoint.InProgress.Count = 0 Then
                If File.Exists(CheckpointPath) Then File.Delete(CheckpointPath)
                ClearResumeCompletedLog(OutputFolder)
                Return
            End If

            Checkpoint.Version = 2
            Dim CheckpointDirectory As String = Path.GetDirectoryName(CheckpointPath)
            Directory.CreateDirectory(CheckpointDirectory)
            Dim TemporaryPath As String = CheckpointPath & "." & Guid.NewGuid().ToString("N") & ".tmp"
            Try
                Using CheckpointStream As New FileStream(TemporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None)
                    Dim Serializer As New Xml.Serialization.XmlSerializer(GetType(BatchResumeCheckpoint))
                    Serializer.Serialize(CheckpointStream, Checkpoint)
                    CheckpointStream.Flush(True)
                End Using
                If File.Exists(CheckpointPath) Then
                    File.Replace(TemporaryPath, CheckpointPath, Nothing)
                Else
                    File.Move(TemporaryPath, CheckpointPath)
                End If
            Finally
                If File.Exists(TemporaryPath) Then
                    Try
                        File.Delete(TemporaryPath)
                    Catch ex As Exception
                        System.Diagnostics.Debug.WriteLine("Could not remove temporary resume checkpoint: " & ex.Message)
                    End Try
                End If
            End Try
            ClearResumeCompletedLog(OutputFolder)
        End SyncLock
    End Sub

    Private Function LoadResumeCompletedEntryKeys(OutputFolder As String) As HashSet(Of String)
        Dim Result As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Dim CompletedLogPath As String = GetResumeCompletedLogPath(OutputFolder)
        SyncLock ResumeCheckpointLock
            If Not File.Exists(CompletedLogPath) Then Return Result
            For Each LogLine As String In File.ReadAllLines(CompletedLogPath)
                If String.IsNullOrWhiteSpace(LogLine) Then Continue For
                Try
                    Dim DecodedRecord As String = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(LogLine.Trim()))
                    Dim SeparatorIndex As Integer = DecodedRecord.IndexOf("|", StringComparison.Ordinal)
                    If SeparatorIndex <= 0 OrElse SeparatorIndex = DecodedRecord.Length - 1 Then Continue For
                    Dim AttemptId As String = DecodedRecord.Substring(0, SeparatorIndex)
                    Dim InputPath As String = DecodedRecord.Substring(SeparatorIndex + 1)
                    Dim ParsedAttemptId As Guid
                    If Not Guid.TryParse(AttemptId, ParsedAttemptId) Then Continue For
                    Result.Add(GetResumeEntryKey(AttemptId, InputPath))
                Catch ex As Exception
                    ' An incomplete final log line is ignored; that entry remains eligible for safe retry.
                End Try
            Next
        End SyncLock
        Return Result
    End Function

    Private Function GetResumeInProgressPaths(OutputFolder As String) As HashSet(Of String)
        Dim Result As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Dim CompletedEntryKeys As HashSet(Of String) = LoadResumeCompletedEntryKeys(OutputFolder)
        For Each Entry As BatchResumeEntry In LoadResumeCheckpoint(OutputFolder).InProgress
            Dim FullInputPath As String = Path.GetFullPath(Entry.InputPath)
            If Not CompletedEntryKeys.Contains(GetResumeEntryKey(Entry.AttemptId, FullInputPath)) Then Result.Add(FullInputPath)
        Next
        Return Result
    End Function

    Private Sub MarkBatchInProgress(OutputFolder As String, Checkpoint As BatchResumeCheckpoint,
                                    Inputs As IEnumerable(Of String), OutputExtensions As HashSet(Of String))
        If Inputs Is Nothing Then Return
        If OutputExtensions Is Nothing OrElse OutputExtensions.Count = 0 Then
            Throw New InvalidOperationException("Cannot save a safe resume checkpoint because the output image format could not be determined.")
        End If
        Dim BatchAttemptId As String = Guid.NewGuid().ToString("N")
        For Each InputPath As String In Inputs
            If Not File.Exists(InputPath) Then Throw New FileNotFoundException("A batch input disappeared before it could be checkpointed.", InputPath)
            Dim FullInputPath As String = Path.GetFullPath(InputPath)
            Checkpoint.InProgress.RemoveAll(Function(Existing As BatchResumeEntry) _
                String.Equals(Path.GetFullPath(Existing.InputPath), FullInputPath, StringComparison.OrdinalIgnoreCase))
            Dim Entry As New BatchResumeEntry With {
                .InputPath = FullInputPath,
                .AttemptId = BatchAttemptId,
                .OutputExtensions = New List(Of String)(OutputExtensions)
            }
            Checkpoint.InProgress.Add(Entry)
        Next
        SaveResumeCheckpoint(OutputFolder, Checkpoint)
    End Sub

    Private Sub MarkTextureComplete(OutputFolder As String, Checkpoint As BatchResumeCheckpoint, InputPath As String)
        If Checkpoint Is Nothing OrElse Checkpoint.InProgress Is Nothing OrElse String.IsNullOrWhiteSpace(InputPath) Then Return
        Dim FullInputPath As String = Path.GetFullPath(InputPath)
        SyncLock ResumeCheckpointLock
            Dim Entry As BatchResumeEntry = Checkpoint.InProgress.FirstOrDefault(
                Function(Candidate As BatchResumeEntry) String.Equals(
                    Path.GetFullPath(Candidate.InputPath), FullInputPath, StringComparison.OrdinalIgnoreCase))
            If Entry Is Nothing OrElse String.IsNullOrWhiteSpace(Entry.AttemptId) Then Return

            Dim CompletedLogPath As String = GetResumeCompletedLogPath(OutputFolder)
            Directory.CreateDirectory(Path.GetDirectoryName(CompletedLogPath))
            Dim CompletedRecord As String = GetResumeEntryKey(Entry.AttemptId, FullInputPath)
            Dim RecordBytes As Byte() = System.Text.Encoding.UTF8.GetBytes(
                Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(CompletedRecord)) & Environment.NewLine)
            Using CompletedLogStream As New FileStream(CompletedLogPath, FileMode.Append, FileAccess.Write, FileShare.Read)
                CompletedLogStream.Write(RecordBytes, 0, RecordBytes.Length)
                CompletedLogStream.Flush()
            End Using
            If LoadedSettings.ExpertSettings.ClearInput AndAlso File.Exists(FullInputPath) Then
                File.Delete(FullInputPath)
            End If
            Checkpoint.InProgress.RemoveAll(Function(Candidate As BatchResumeEntry) _
                String.Equals(Path.GetFullPath(Candidate.InputPath), FullInputPath, StringComparison.OrdinalIgnoreCase))
            Dim ShouldCompactLog As Boolean = False
            Try
                ShouldCompactLog = New FileInfo(CompletedLogPath).Length >= 65536
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine("Could not inspect completed-texture resume log: " & ex.Message)
            End Try
            If ShouldCompactLog Then SaveResumeCheckpoint(OutputFolder, Checkpoint)
        End SyncLock
    End Sub

    Private Sub MarkBatchComplete(OutputFolder As String, Checkpoint As BatchResumeCheckpoint, Inputs As IEnumerable(Of String))
        If Inputs Is Nothing Then Return
        SyncLock ResumeCheckpointLock
            For Each InputPath As String In Inputs
                Dim FullInputPath As String = Path.GetFullPath(InputPath)
                Checkpoint.InProgress.RemoveAll(Function(Existing As BatchResumeEntry) _
                    String.Equals(Path.GetFullPath(Existing.InputPath), FullInputPath, StringComparison.OrdinalIgnoreCase))
            Next
            SaveResumeCheckpoint(OutputFolder, Checkpoint)
        End SyncLock
    End Sub

    Private Sub MarkCheckpointedFinalTexture(OutputFolder As String, Checkpoint As BatchResumeCheckpoint,
                                             OriginalInputByStem As Dictionary(Of String, String),
                                             StageCounts As Dictionary(Of String, Integer),
                                             FinalOutputs As Dictionary(Of String, String),
                                             Stem As String, RequiredStageCount As Integer)
        Dim CompletedStages As Integer = 0
        Dim OriginalInputPath As String = Nothing
        Dim FinalOutputPath As String = Nothing
        If Not StageCounts.TryGetValue(Stem, CompletedStages) OrElse CompletedStages <> RequiredStageCount OrElse
            Not OriginalInputByStem.TryGetValue(Stem, OriginalInputPath) OrElse
            Not FinalOutputs.TryGetValue(Stem, FinalOutputPath) OrElse Not IsNonEmptyFile(FinalOutputPath) Then Return
        MarkTextureComplete(OutputFolder, Checkpoint, OriginalInputPath)
    End Sub

    Private Function IsNonEmptyFile(FilePath As String) As Boolean
        If String.IsNullOrWhiteSpace(FilePath) OrElse Not File.Exists(FilePath) Then Return False
        Try
            Return New FileInfo(FilePath).Length > 0
        Catch ex As IOException
            Return False
        Catch ex As UnauthorizedAccessException
            Return False
        End Try
    End Function

    Private Function RecoverInterruptedBatch(OutputFolder As String, Checkpoint As BatchResumeCheckpoint) As Integer
        If Checkpoint Is Nothing OrElse Checkpoint.InProgress Is Nothing OrElse Checkpoint.InProgress.Count = 0 Then Return 0

        Dim CompletedEntryKeys As HashSet(Of String) = LoadResumeCompletedEntryKeys(OutputFolder)
        Dim EntriesToRetry As New List(Of BatchResumeEntry)
        Dim AllowedExtensionsByStem As New Dictionary(Of String, HashSet(Of String))(StringComparer.OrdinalIgnoreCase)
        For Each Entry As BatchResumeEntry In Checkpoint.InProgress
            Dim FullInputPath As String = Path.GetFullPath(Entry.InputPath)
            If CompletedEntryKeys.Contains(GetResumeEntryKey(Entry.AttemptId, FullInputPath)) Then
                If LoadedSettings.ExpertSettings.ClearInput AndAlso File.Exists(FullInputPath) Then
                    Try
                        File.Delete(FullInputPath)
                    Catch ex As Exception
                        Throw New IOException("AutoCrispy could not remove the source for a completed texture while resuming: " & FullInputPath, ex)
                    End Try
                End If
                Continue For
            End If
            EntriesToRetry.Add(Entry)
            Dim Stem As String = Path.GetFileNameWithoutExtension(Entry.InputPath)
            Dim EntryExtensions As HashSet(Of String) = Nothing
            If Not AllowedExtensionsByStem.TryGetValue(Stem, EntryExtensions) Then
                EntryExtensions = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
                AllowedExtensionsByStem.Add(Stem, EntryExtensions)
            End If
            For Each Extension As String In Entry.OutputExtensions
                If Not String.IsNullOrWhiteSpace(Extension) Then EntryExtensions.Add(Extension)
            Next
        Next

        If EntriesToRetry.Count = 0 Then
            Checkpoint.InProgress.Clear()
            SaveResumeCheckpoint(OutputFolder, Checkpoint)
            Return 0
        End If
        If AllowedExtensionsByStem.Count = 0 OrElse
            Not AllowedExtensionsByStem.Values.Any(Function(Extensions As HashSet(Of String)) Extensions.Count > 0) Then
            Throw New InvalidDataException("The saved resume checkpoint has no safe output paths to recover.")
        End If

        If Directory.Exists(OutputFolder) Then
            For Each Candidate As String In Directory.GetFiles(OutputFolder, "*.*", SearchOption.AllDirectories)
                Dim CandidateExtensions As HashSet(Of String) = Nothing
                If Not AllowedExtensionsByStem.TryGetValue(Path.GetFileNameWithoutExtension(Candidate), CandidateExtensions) OrElse
                    Not CandidateExtensions.Contains(Path.GetExtension(Candidate)) Then Continue For
                Try
                    File.Delete(Candidate)
                Catch ex As Exception
                    Throw New IOException("AutoCrispy could not remove an incomplete output while resuming: " & Candidate, ex)
                End Try
            Next
        End If

        Dim RecoveredCount As Integer = EntriesToRetry.Count
        Checkpoint.InProgress.Clear()
        SaveResumeCheckpoint(OutputFolder, Checkpoint)
        Return RecoveredCount
    End Function

    Private Function GetPendingInputFiles(InputFiles As String(), OutputPath As String, SupportedExtensions As HashSet(Of String),
                                          AlphaMode As Integer, ByRef UnsupportedCount As Integer,
                                          ByRef AlphaFilteredCount As Integer, ByRef SupportedCount As Integer,
                                          ByRef MissingCount As Integer,
                                          Optional ResumeInProgressPaths As HashSet(Of String) = Nothing) As String()
        Dim SupportedFiles As String() = GetSupportedInputFiles(InputFiles, SupportedExtensions)
        EnsureNoFlattenedNameCollisions(SupportedFiles)
        UnsupportedCount = If(InputFiles Is Nothing, 0, InputFiles.Length - SupportedFiles.Length)
        SupportedCount = SupportedFiles.Length
        Dim MissingList As New List(Of String)(GetMissingFiles(SupportedFiles, OutputPath))
        Dim MissingPaths As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each MissingFile As String In MissingList
            MissingPaths.Add(Path.GetFullPath(MissingFile))
        Next
        If ResumeInProgressPaths IsNot Nothing Then
            For Each SupportedFile As String In SupportedFiles
                Dim FullPath As String = Path.GetFullPath(SupportedFile)
                If ResumeInProgressPaths.Contains(FullPath) AndAlso MissingPaths.Add(FullPath) Then MissingList.Add(SupportedFile)
            Next
        End If
        MissingCount = MissingList.Count
        AlphaFilteredCount = 0

        Dim PendingFiles As New List(Of String)
        For Each MissingFile As String In MissingList
            If IsAlphaFiltered(MissingFile, AlphaMode) Then
                AlphaFilteredCount += 1
            Else
                PendingFiles.Add(MissingFile)
            End If
        Next
        Return PendingFiles.ToArray()
    End Function

    Private Sub EnsureNoFlattenedNameCollisions(SupportedFiles As String())
        If SupportedFiles Is Nothing OrElse SupportedFiles.Length < 2 Then Return
        Dim FirstPathByStem As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        For Each SupportedFile As String In SupportedFiles
            Dim Stem As String = Path.GetFileNameWithoutExtension(SupportedFile)
            Dim ExistingPath As String = Nothing
            If FirstPathByStem.TryGetValue(Stem, ExistingPath) Then
                Throw New InvalidOperationException(
                    "Cannot process files with the same name (ignoring extension) because AutoCrispy stages inputs in a flat folder: " &
                    ExistingPath & " and " & SupportedFile & ". Rename one file or separate the inputs into another watched folder.")
            End If
            FirstPathByStem.Add(Stem, SupportedFile)
        Next
    End Sub

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

    Private Sub WriteProcessLog(StartInfo As ProcessStartInfo, StandardOutput As String, StandardError As String, SaveLoc As String, BackendName As String, ExitCode As Integer)
        Dim Filename As String = Path.Combine(SaveLoc, BackendName & "_" & Now.ToString("yyyy-MM-dd_HH-mm-ss-fff") & "_" & Guid.NewGuid().ToString("N").Substring(0, 8) & ".txt")
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
        If Form1.DoGamePathsOverlap(InputPath, OutputPath) Then
            MessageBox.Show(Me,
                "Input and output folders must be separate and must not contain one another. This prevents AutoCrispy from rediscovering generated textures.",
                "Overlapping game folders", MessageBoxButtons.OK, MessageBoxIcon.Warning)
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
