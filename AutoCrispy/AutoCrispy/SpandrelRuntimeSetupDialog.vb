Imports System.Diagnostics
Imports System.Drawing
Imports System.IO
Imports System.Text
Imports System.Threading.Tasks
Imports System.Windows.Forms

Friend NotInheritable Class SpandrelRuntimeSetupDialog
    Inherits Form

    Private Const PythonDownloadsUrl As String = "https://www.python.org/downloads/windows/"
    Private Const PyTorchInstallUrl As String = "https://pytorch.org/get-started/locally/"
    Private Const SpandrelVersion As String = "0.4.2"

    Private Const EnvironmentProbeScript As String =
        "import sys,struct,site,importlib.util as u; " &
        "print('PYTHON_VERSION=' + sys.version.split()[0]); " &
        "print('PYTHON_BITS=' + str(struct.calcsize('P') * 8)); " &
        "print('IS_VENV=' + str(sys.prefix != sys.base_prefix)); " &
        "print('USER_SITE_ENABLED=' + str(bool(site.ENABLE_USER_SITE))); " &
        "print('HAS_TORCH=' + str(u.find_spec('torch') is not None)); " &
        "print('HAS_TORCHVISION=' + str(u.find_spec('torchvision') is not None)); " &
        "print('HAS_SPANDREL=' + str(u.find_spec('spandrel') is not None)); " &
        "print('HAS_NUMPY=' + str(u.find_spec('numpy') is not None)); " &
        "print('HAS_PIL=' + str(u.find_spec('PIL') is not None))"

    Private Const RuntimeVerifyScript As String =
        "import torch,torchvision,spandrel,numpy,PIL,importlib.metadata as m; " &
        "print('READY=true'); " &
        "print('TORCH=' + m.version('torch')); " &
        "print('TORCHVISION=' + m.version('torchvision')); " &
        "print('SPANDREL=' + m.version('spandrel')); " &
        "print('NUMPY=' + m.version('numpy')); " &
        "print('PILLOW=' + m.version('Pillow')); " &
        "print('CUDA_AVAILABLE=' + str(torch.cuda.is_available()))"

    Private ReadOnly _pythonExecutable As String
    Private ReadOnly _descriptionLabel As New Label()
    Private ReadOnly _pythonPathLabel As New Label()
    Private ReadOnly _statusLabel As New Label()
    Private ReadOnly _logBox As New RichTextBox()
    Private ReadOnly _progressBar As New ProgressBar()
    Private ReadOnly _installButton As New Button()
    Private ReadOnly _closeButton As New Button()
    Private ReadOnly _pythonDownloadsLink As New LinkLabel()
    Private ReadOnly _pytorchGuideLink As New LinkLabel()

    Private _activeProcess As Process
    Private _isRunning As Boolean
    Private _cancelRequested As Boolean
    Private _pythonIsSupported As Boolean
    Private _isVirtualEnvironment As Boolean
    Private _userSiteEnabled As Boolean
    Private _useUserSite As Boolean
    Private _runtimeReady As Boolean

    Public ReadOnly Property SetupCompleted As Boolean
        Get
            Return _runtimeReady
        End Get
    End Property

    Public Sub New(PythonExecutable As String)
        _pythonExecutable = If(PythonExecutable, String.Empty)
        InitializeSetupDialog()
    End Sub

    Private Sub InitializeSetupDialog()
        Text = "Spandrel Python dependencies"
        StartPosition = FormStartPosition.CenterParent
        AutoScaleDimensions = New SizeF(6.0!, 13.0!)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(820, 570)
        MinimumSize = New Size(740, 500)
        ShowInTaskbar = False
        MinimizeBox = False
        MaximizeBox = True

        _descriptionLabel.Location = New Point(12, 12)
        _descriptionLabel.Size = New Size(796, 66)
        _descriptionLabel.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        _descriptionLabel.Text =
            "Installs Spandrel 0.4.2, NumPy and Pillow; pip adds PyTorch, TorchVision and their required dependencies. Downloads may be large." & Environment.NewLine &
            "For NVIDIA CUDA acceleration, use the official PyTorch selector first to choose a build for your GPU/driver, then return here."
        Controls.Add(_descriptionLabel)

        _pythonPathLabel.Location = New Point(12, 84)
        _pythonPathLabel.Size = New Size(796, 20)
        _pythonPathLabel.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        _pythonPathLabel.AutoEllipsis = True
        Controls.Add(_pythonPathLabel)

        _statusLabel.Location = New Point(12, 108)
        _statusLabel.Size = New Size(796, 36)
        _statusLabel.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        _statusLabel.ForeColor = SystemColors.GrayText
        _statusLabel.Text = "Checking for a supported Python installation…"
        Controls.Add(_statusLabel)

        _logBox.Location = New Point(12, 151)
        _logBox.Size = New Size(796, 332)
        _logBox.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        _logBox.ReadOnly = True
        _logBox.BackColor = SystemColors.Window
        _logBox.Font = New Font("Consolas", 8.5!, FontStyle.Regular)
        _logBox.WordWrap = False
        _logBox.ScrollBars = RichTextBoxScrollBars.Both
        _logBox.DetectUrls = False
        Controls.Add(_logBox)

        _progressBar.Location = New Point(12, 491)
        _progressBar.Size = New Size(796, 18)
        _progressBar.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        _progressBar.Style = ProgressBarStyle.Blocks
        Controls.Add(_progressBar)

        _pythonDownloadsLink.AutoSize = True
        _pythonDownloadsLink.Location = New Point(12, 532)
        _pythonDownloadsLink.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        _pythonDownloadsLink.Text = "Python 3.10+ downloads"
        AddHandler _pythonDownloadsLink.LinkClicked, AddressOf PythonDownloadsLink_Click
        Controls.Add(_pythonDownloadsLink)

        _pytorchGuideLink.AutoSize = True
        _pytorchGuideLink.Location = New Point(180, 532)
        _pytorchGuideLink.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        _pytorchGuideLink.Text = "PyTorch CUDA guide"
        AddHandler _pytorchGuideLink.LinkClicked, AddressOf PyTorchGuideLink_Click
        Controls.Add(_pytorchGuideLink)

        _installButton.Location = New Point(590, 523)
        _installButton.Size = New Size(128, 32)
        _installButton.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        _installButton.Text = "Install / repair"
        _installButton.Enabled = False
        _installButton.UseVisualStyleBackColor = True
        AddHandler _installButton.Click, AddressOf InstallButton_Click
        Controls.Add(_installButton)

        _closeButton.Location = New Point(724, 523)
        _closeButton.Size = New Size(84, 32)
        _closeButton.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        _closeButton.Text = "Close"
        _closeButton.UseVisualStyleBackColor = True
        AddHandler _closeButton.Click, AddressOf CloseButton_Click
        Controls.Add(_closeButton)

        UiToolTip.SetToolTip(_installButton,
            "Install Spandrel 0.4.2, NumPy and Pillow into the Python shown above. PyTorch and TorchVision are installed as Spandrel dependencies.")
        UiToolTip.SetToolTip(_pythonPathLabel, _pythonExecutable)
        UiToolTip.SetToolTip(_pythonDownloadsLink, "Open the official Python for Windows downloads page.")
        UiToolTip.SetToolTip(_pytorchGuideLink, "Choose the official PyTorch build matching your GPU and driver.")

        AcceptButton = _installButton
        CancelButton = _closeButton
        AddHandler Me.Load, AddressOf SetupDialog_Load
        AddHandler Me.FormClosing, AddressOf SetupDialog_FormClosing
        AddHandler Me.FormClosed, AddressOf SetupDialog_FormClosed
    End Sub

    Private ReadOnly UiToolTip As New ToolTip()

    Private Async Sub SetupDialog_Load(sender As Object, e As EventArgs)
        If String.IsNullOrWhiteSpace(_pythonExecutable) OrElse Not File.Exists(_pythonExecutable) Then
            _pythonPathLabel.Text = "Python executable: not found"
            _statusLabel.Text = "Install Python 3.10+ (64-bit), then restart AutoCrispy or set AUTOCRISPY_PYTHON."
            AcceptButton = _closeButton
            AppendLog("No Python executable was found by AutoCrispy.")
            AppendLog("Install Python using the link below. Keep Add python.exe to PATH enabled, then restart AutoCrispy.")
            Return
        End If

        _pythonPathLabel.Text = "Python: " & _pythonExecutable
        SetBusy(True)
        _statusLabel.Text = "Checking Python version and installed packages…"
        Try
            Dim Probe As RuntimeProcessResult = Await RunPythonAsync("-c " & QuotePythonCode(EnvironmentProbeScript), False)
            If _cancelRequested Then
                _statusLabel.Text = "Python check cancelled."
                Return
            End If
            If Probe.ExitCode <> 0 Then
                AppendResultOutput(Probe)
                _statusLabel.Text = "Could not run the detected Python. Check its path and installation."
                AppendLog("Python check failed with exit code " & Probe.ExitCode.ToString() & ".")
                Return
            End If

            Dim VersionText As String = GetTaggedValue(Probe.StandardOutput, "PYTHON_VERSION")
            Dim PythonVersion As Version = Nothing
            Dim Bits As Integer
            Integer.TryParse(GetTaggedValue(Probe.StandardOutput, "PYTHON_BITS"), Bits)
            _isVirtualEnvironment = String.Equals(GetTaggedValue(Probe.StandardOutput, "IS_VENV"), "True", StringComparison.OrdinalIgnoreCase)
            _userSiteEnabled = String.Equals(GetTaggedValue(Probe.StandardOutput, "USER_SITE_ENABLED"), "True", StringComparison.OrdinalIgnoreCase)
            _useUserSite = Not _isVirtualEnvironment AndAlso _userSiteEnabled

            Dim IsVersionValid As Boolean = Version.TryParse(VersionText, PythonVersion) AndAlso
                (PythonVersion.Major > 3 OrElse (PythonVersion.Major = 3 AndAlso PythonVersion.Minor >= 10))
            If Not IsVersionValid Then
                _statusLabel.Text = "Python 3.10 or newer is required. Detected: " & If(VersionText = "", "unknown version", VersionText) & "."
                AppendLog("This Python version is too old for the current Spandrel/PyTorch packages.")
                Return
            End If
            If Bits <> 64 Then
                _statusLabel.Text = "PyTorch requires 64-bit Python. Detected a " & Bits.ToString() & "-bit interpreter."
                AppendLog("Install 64-bit Python, then restart AutoCrispy.")
                Return
            End If

            _pythonIsSupported = True
            Dim InstalledPackages As New List(Of String)
            If String.Equals(GetTaggedValue(Probe.StandardOutput, "HAS_TORCH"), "True", StringComparison.OrdinalIgnoreCase) Then InstalledPackages.Add("PyTorch")
            If String.Equals(GetTaggedValue(Probe.StandardOutput, "HAS_TORCHVISION"), "True", StringComparison.OrdinalIgnoreCase) Then InstalledPackages.Add("TorchVision")
            If String.Equals(GetTaggedValue(Probe.StandardOutput, "HAS_SPANDREL"), "True", StringComparison.OrdinalIgnoreCase) Then InstalledPackages.Add("Spandrel")
            If String.Equals(GetTaggedValue(Probe.StandardOutput, "HAS_NUMPY"), "True", StringComparison.OrdinalIgnoreCase) Then InstalledPackages.Add("NumPy")
            If String.Equals(GetTaggedValue(Probe.StandardOutput, "HAS_PIL"), "True", StringComparison.OrdinalIgnoreCase) Then InstalledPackages.Add("Pillow")

            Dim Pip As RuntimeProcessResult = Await RunPythonAsync("-m pip --version", False)
            Dim PipStatus As String = If(Pip.ExitCode = 0, "pip is ready", "pip is missing; Install will try to bootstrap it")
            _statusLabel.Text = "Python " & VersionText & " · 64-bit · " & PipStatus & Environment.NewLine &
                "Found: " & If(InstalledPackages.Count = 0, "no required packages", String.Join(", ", InstalledPackages))
            AppendLog("Python " & VersionText & " (64-bit)")
            AppendLog("Environment: " & If(_isVirtualEnvironment, "virtual environment", "standard Python installation"))
            AppendLog("User site-packages: " & If(_useUserSite, "enabled (packages will be installed for this Windows user)", "not used"))
            AppendLog("Found packages: " & If(InstalledPackages.Count = 0, "none", String.Join(", ", InstalledPackages)))
        Catch ex As Exception
            _statusLabel.Text = "Python check failed: " & ex.GetBaseException().Message
            AppendLog("Python check failed: " & ex.GetBaseException().Message)
        Finally
            SetBusy(False)
        End Try
    End Sub

    Private Async Sub InstallButton_Click(sender As Object, e As EventArgs)
        If _isRunning OrElse Not _pythonIsSupported Then Return
        Dim InstallWarning As String = "This installs Spandrel " & SpandrelVersion & ", NumPy and Pillow into the Python shown above." & Environment.NewLine &
            "PyTorch, TorchVision and their supporting packages are installed automatically if missing. The download may be large. Continue?"
        If MessageBox.Show(Me, InstallWarning, "Install Spandrel support?", MessageBoxButtons.YesNo,
                           MessageBoxIcon.Information, MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then Return

        _cancelRequested = False
        _runtimeReady = False
        _logBox.Clear()
        AppendLog("Python: " & _pythonExecutable)
        AppendLog("Installing Spandrel " & SpandrelVersion & ", NumPy and Pillow (plus required PyTorch dependencies)…")
        SetBusy(True)
        _statusLabel.Text = "Preparing the Python package installer…"

        Try
            Dim PipCheck As RuntimeProcessResult = Await RunPythonAsync("-m pip --version", False)
            If PipCheck.ExitCode <> 0 Then
                If _cancelRequested Then Return
                AppendLog("pip is not available. Trying Python's bundled ensurepip…")
                Dim Bootstrap As RuntimeProcessResult = Await RunPythonAsync("-m ensurepip --upgrade", True)
                If Bootstrap.ExitCode <> 0 Then
                    AppendResultOutput(Bootstrap)
                    _statusLabel.Text = "Could not install pip. Repair Python with pip included, then try again."
                    Return
                End If
            End If
            If _cancelRequested Then Return

            Dim InstallArguments As String = "-m pip install --disable-pip-version-check --no-input --progress-bar off --prefer-binary"
            If _useUserSite Then InstallArguments &= " --user"
            InstallArguments &= " spandrel==" & SpandrelVersion & " numpy Pillow"
            _statusLabel.Text = "Downloading and installing packages… This can take several minutes."
            Dim InstallResult As RuntimeProcessResult = Await RunPythonAsync(InstallArguments, True)
            If _cancelRequested Then
                _statusLabel.Text = "Installation cancelled. Any completed packages are kept; run Install / repair again to finish."
                AppendLog("Installation cancelled by the user.")
                Return
            End If
            If InstallResult.ExitCode <> 0 Then
                _statusLabel.Text = "Package installation failed (exit code " & InstallResult.ExitCode.ToString() & "). See the log below."
                AppendLog("Package installation failed with exit code " & InstallResult.ExitCode.ToString() & ".")
                Return
            End If

            _statusLabel.Text = "Verifying PyTorch, TorchVision and Spandrel imports…"
            Dim Verification As RuntimeProcessResult = Await RunPythonAsync("-c " & QuotePythonCode(RuntimeVerifyScript), True)
            If _cancelRequested Then
                _statusLabel.Text = "Verification cancelled. Run Install / repair again to check the runtime."
                Return
            End If
            If Verification.ExitCode <> 0 OrElse Not String.Equals(GetTaggedValue(Verification.StandardOutput, "READY"), "true", StringComparison.OrdinalIgnoreCase) Then
                _statusLabel.Text = "Packages were installed, but verification failed. See the log for the import error."
                AppendResultOutput(Verification)
                Return
            End If

            Dim TorchVersion As String = GetTaggedValue(Verification.StandardOutput, "TORCH")
            Dim SpandrelVersionText As String = GetTaggedValue(Verification.StandardOutput, "SPANDREL")
            If Not String.Equals(SpandrelVersionText, SpandrelVersion, StringComparison.OrdinalIgnoreCase) Then
                _statusLabel.Text = "Spandrel " & SpandrelVersionText & " is active; AutoCrispy requires " & SpandrelVersion & ". Check this Python environment."
                AppendLog("Expected Spandrel " & SpandrelVersion & " but imported " & SpandrelVersionText & ".")
                Return
            End If

            _runtimeReady = True
            Dim CudaAvailable As Boolean = String.Equals(GetTaggedValue(Verification.StandardOutput, "CUDA_AVAILABLE"), "True", StringComparison.OrdinalIgnoreCase)
            If CudaAvailable Then
                _statusLabel.Text = "Ready · PyTorch " & TorchVersion & " · Spandrel " & SpandrelVersionText & " · NVIDIA CUDA available."
            Else
                _statusLabel.Text = "Ready · PyTorch " & TorchVersion & " · Spandrel " & SpandrelVersionText & ". CUDA is not available; CPU inference remains possible."
            End If
            AppendLog("Spandrel runtime verification passed.")

        Catch ex As Exception
            _statusLabel.Text = "Setup failed: " & ex.GetBaseException().Message
            AppendLog("Setup failed: " & ex.GetBaseException().Message)
        Finally
            SetBusy(False)
        End Try
    End Sub

    Private Async Function RunPythonAsync(Arguments As String, LogOutput As Boolean) As Task(Of RuntimeProcessResult)
        If String.IsNullOrWhiteSpace(_pythonExecutable) OrElse Not File.Exists(_pythonExecutable) Then
            Throw New FileNotFoundException("The selected Python executable was not found.", _pythonExecutable)
        End If

        Dim OutputBuilder As New StringBuilder()
        Dim ErrorBuilder As New StringBuilder()
        Dim OutputLock As New Object()
        Dim ErrorLock As New Object()
        Using RuntimeProcess As New Process()
            RuntimeProcess.StartInfo = New ProcessStartInfo With {
                .FileName = _pythonExecutable,
                .Arguments = Arguments,
                .WorkingDirectory = Application.StartupPath,
                .UseShellExecute = False,
                .CreateNoWindow = True,
                .RedirectStandardOutput = True,
                .RedirectStandardError = True,
                .StandardOutputEncoding = Encoding.UTF8,
                .StandardErrorEncoding = Encoding.UTF8
            }
            AddHandler RuntimeProcess.OutputDataReceived,
                Sub(ProcessSender As Object, Data As DataReceivedEventArgs)
                    If Data.Data Is Nothing Then Return
                    SyncLock OutputLock
                        OutputBuilder.AppendLine(Data.Data)
                    End SyncLock
                    If LogOutput Then AppendLog(Data.Data)
                End Sub
            AddHandler RuntimeProcess.ErrorDataReceived,
                Sub(ProcessSender As Object, Data As DataReceivedEventArgs)
                    If Data.Data Is Nothing Then Return
                    SyncLock ErrorLock
                        ErrorBuilder.AppendLine(Data.Data)
                    End SyncLock
                    If LogOutput Then AppendLog(Data.Data)
                End Sub

            If Not RuntimeProcess.Start() Then Throw New InvalidOperationException("Could not start Python.")
            _activeProcess = RuntimeProcess
            RuntimeProcess.BeginOutputReadLine()
            RuntimeProcess.BeginErrorReadLine()
            Await Task.Run(Sub() RuntimeProcess.WaitForExit())

            Dim Result As New RuntimeProcessResult With {
                .ExitCode = RuntimeProcess.ExitCode,
                .StandardOutput = OutputBuilder.ToString(),
                .StandardError = ErrorBuilder.ToString()
            }
            _activeProcess = Nothing
            Return Result
        End Using
    End Function

    Private Sub SetBusy(IsBusy As Boolean)
        _isRunning = IsBusy
        _progressBar.Style = If(IsBusy, ProgressBarStyle.Marquee, ProgressBarStyle.Blocks)
        _progressBar.MarqueeAnimationSpeed = If(IsBusy, 30, 0)
        _installButton.Enabled = Not IsBusy AndAlso _pythonIsSupported
        _closeButton.Text = If(IsBusy, "Cancel", "Close")
        If IsBusy Then
            AcceptButton = Nothing
        ElseIf _pythonIsSupported AndAlso Not _runtimeReady Then
            AcceptButton = _installButton
        Else
            AcceptButton = _closeButton
        End If
    End Sub

    Private Sub CloseButton_Click(sender As Object, e As EventArgs)
        If _isRunning Then
            CancelActiveProcess()
            _statusLabel.Text = "Stopping the current Python command…"
            Return
        End If
        DialogResult = If(_runtimeReady, DialogResult.OK, DialogResult.Cancel)
        Close()
    End Sub

    Private Sub SetupDialog_FormClosing(sender As Object, e As FormClosingEventArgs)
        If _isRunning Then
            e.Cancel = True
            CancelActiveProcess()
            _statusLabel.Text = "Stopping the current Python command…"
        ElseIf DialogResult = DialogResult.None Then
            DialogResult = If(_runtimeReady, DialogResult.OK, DialogResult.Cancel)
        End If
    End Sub

    Private Sub SetupDialog_FormClosed(sender As Object, e As FormClosedEventArgs)
        UiToolTip.Dispose()
    End Sub

    Private Sub CancelActiveProcess()
        _cancelRequested = True
        Dim ProcessToStop As Process = _activeProcess
        If ProcessToStop Is Nothing Then Return
        Try
            If Not ProcessToStop.HasExited Then ProcessToStop.Kill()
        Catch ex As InvalidOperationException
            ' The pip or Python command may have exited between the check and Kill().
        Catch ex As System.ComponentModel.Win32Exception
            AppendLog("Could not stop the process: " & ex.Message)
        End Try
    End Sub

    Private Sub AppendLog(Line As String)
        If IsDisposed OrElse Disposing Then Return
        If InvokeRequired Then
            Try
                BeginInvoke(New MethodInvoker(Sub() AppendLog(Line)))
            Catch ex As ObjectDisposedException
            Catch ex As InvalidOperationException
                ' The window may be closing while a redirected output event is being delivered.
            End Try
            Return
        End If
        _logBox.AppendText(Line & Environment.NewLine)
        _logBox.SelectionStart = _logBox.TextLength
        _logBox.ScrollToCaret()
    End Sub

    Private Sub AppendResultOutput(Result As RuntimeProcessResult)
        If Not String.IsNullOrWhiteSpace(Result.StandardOutput) Then AppendLog(Result.StandardOutput.TrimEnd())
        If Not String.IsNullOrWhiteSpace(Result.StandardError) Then AppendLog(Result.StandardError.TrimEnd())
    End Sub

    Private Shared Function GetTaggedValue(Output As String, Tag As String) As String
        If String.IsNullOrEmpty(Output) Then Return String.Empty
        Dim NormalizedOutput As String = Output.Replace(ControlChars.Cr, ControlChars.Lf)
        For Each Line As String In NormalizedOutput.Split(New Char() {ControlChars.Lf}, StringSplitOptions.RemoveEmptyEntries)
            If Line.StartsWith(Tag & "=", StringComparison.Ordinal) Then Return Line.Substring(Tag.Length + 1).Trim()
        Next
        Return String.Empty
    End Function

    Private Shared Function QuotePythonCode(Code As String) As String
        Return ControlChars.Quote & Code & ControlChars.Quote
    End Function

    Private Sub PythonDownloadsLink_Click(sender As Object, e As LinkLabelLinkClickedEventArgs)
        OpenExternalUrl(PythonDownloadsUrl)
    End Sub

    Private Sub PyTorchGuideLink_Click(sender As Object, e As LinkLabelLinkClickedEventArgs)
        OpenExternalUrl(PyTorchInstallUrl)
    End Sub

    Private Sub OpenExternalUrl(Url As String)
        Try
            Process.Start(New ProcessStartInfo(Url) With {.UseShellExecute = True})
        Catch ex As Exception
            MessageBox.Show(Me, "Could not open the web page." & Environment.NewLine & ex.GetBaseException().Message,
                            "Open link failed", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Class RuntimeProcessResult
        Public Property ExitCode As Integer
        Public Property StandardOutput As String = String.Empty
        Public Property StandardError As String = String.Empty
    End Class

End Class