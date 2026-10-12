Imports System.Diagnostics
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks

Friend Delegate Function OpenModelDbInstallHandler(Item As OpenModelDbCatalogItem,
                                                  Resource As OpenModelDbResource,
                                                  DownloadedPath As String,
                                                  ReportStatus As Action(Of String),
                                                  CancelToken As CancellationToken) As Task(Of String)

Friend Class OpenModelDbCatalogItem
    Public Property Id As String = String.Empty
    Public Property Name As String = String.Empty
    Public Property Author As String = String.Empty
    Public Property License As String = String.Empty
    Public Property Architecture As String = String.Empty
    Public Property Scale As Integer
    Public Property Description As String = String.Empty
    Public Property Tags As String = String.Empty
    Public Property PageUrl As String = String.Empty
    Public Property Resources As New List(Of OpenModelDbResource)

    Public ReadOnly Property FormatsText As String
        Get
            Return String.Join(", ", Resources.Select(Function(Resource As OpenModelDbResource) Resource.Format.ToUpperInvariant()).Distinct())
        End Get
    End Property
End Class

Friend Class OpenModelDbResource
    Public Property Format As String = String.Empty
    Public Property SizeBytes As Long

    Public ReadOnly Property DisplayText As String
        Get
            Return Format.ToUpperInvariant() & " · " & OpenModelDbDialog.FormatBytes(SizeBytes)
        End Get
    End Property
End Class

Friend NotInheritable Class OpenModelDbDialog
    Inherits Form

    Private Const CatalogLinePrefix As String = "OPENMODELDB_RESOURCE" & vbTab
    Private Const ProgressLinePrefix As String = "OPENMODELDB_PROGRESS" & vbTab

    Private ReadOnly _pythonExecutable As String
    Private ReadOnly _scriptPath As String
    Private ReadOnly _destinationFolder As String
    Private ReadOnly _existingModels As HashSet(Of String)
    Private ReadOnly _installHandler As OpenModelDbInstallHandler
    Private ReadOnly _catalogItems As New List(Of OpenModelDbCatalogItem)
    Private ReadOnly _searchBox As New TextBox()
    Private ReadOnly _scaleFilter As New ComboBox()
    Private ReadOnly _modelGrid As New DataGridView()
    Private ReadOnly _detailsBox As New RichTextBox()
    Private ReadOnly _resourceComboBox As New ComboBox()
    Private ReadOnly _licenseReviewedCheckBox As New CheckBox()
    Private ReadOnly _modelPageLink As New LinkLabel()
    Private ReadOnly _statusLabel As New Label()
    Private ReadOnly _progressBar As New ProgressBar()
    Private ReadOnly _downloadButton As New Button()
    Private ReadOnly _refreshButton As New Button()
    Private ReadOnly _closeButton As New Button()
    Private ReadOnly _cancelDownloadButton As New Button()
    Private _cancelSource As CancellationTokenSource
    Private _isBusy As Boolean
    Private _isClosing As Boolean
    Private _selectedItem As OpenModelDbCatalogItem
    Private _existingDestinationPaths As HashSet(Of String)

    Public Property DownloadedModelPath As String = String.Empty

    Public Sub New(PythonExecutable As String, HelperScriptPath As String, DestinationFolder As String,
                   ExistingModelPaths As IEnumerable(Of String), InstallHandler As OpenModelDbInstallHandler)
        _pythonExecutable = If(PythonExecutable, String.Empty)
        _scriptPath = If(HelperScriptPath, String.Empty)
        _destinationFolder = If(DestinationFolder, String.Empty)
        _installHandler = InstallHandler
        _existingModels = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        If ExistingModelPaths IsNot Nothing Then
            For Each ModelPath As String In ExistingModelPaths
                If Not String.IsNullOrWhiteSpace(ModelPath) Then
                    _existingModels.Add(Path.GetFullPath(ModelPath))
                End If
            Next
        End If
        _existingDestinationPaths = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        ReloadExistingDestinationPaths()
        BuildInterface()
    End Sub

    Private Sub BuildInterface()
        Text = "OpenModelDB · compatible Spandrel models"
        StartPosition = FormStartPosition.CenterParent
        ClientSize = New Size(1100, 740)
        MinimumSize = New Size(900, 620)
        ShowInTaskbar = False
        AutoScaleMode = AutoScaleMode.Font
        KeyPreview = True

        Dim HeaderLabel As New Label With {
            .Dock = DockStyle.Fill,
            .AutoEllipsis = True,
            .Padding = New Padding(2, 2, 2, 4),
            .Text = "Filtered to 1× RGB restoration / 4× RGB super-resolution checkpoints with core Spandrel architectures and direct HTTPS downloads. Each file is hash-checked and tested with your installed Spandrel before installation."
        }

        _searchBox.Dock = DockStyle.Fill
        _searchBox.Margin = New Padding(2, 4, 10, 4)
        _searchBox.AccessibleName = "Search compatible OpenModelDB models"
        AddHandler _searchBox.TextChanged, AddressOf SearchChanged

        _scaleFilter.DropDownStyle = ComboBoxStyle.DropDownList
        _scaleFilter.Items.AddRange(New Object() {"All compatible models", "4× super-resolution", "1× restoration"})
        _scaleFilter.SelectedIndex = 0
        _scaleFilter.Dock = DockStyle.Fill
        _scaleFilter.Margin = New Padding(2, 4, 2, 4)
        AddHandler _scaleFilter.SelectedIndexChanged, AddressOf SearchChanged

        Dim SearchLayout As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 3,
            .RowCount = 1,
            .Padding = New Padding(0)
        }
        SearchLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 72.0!))
        SearchLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0!))
        SearchLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 230.0!))
        Dim SearchLabel As New Label With {
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Text = "Search:"
        }
        SearchLayout.Controls.Add(SearchLabel, 0, 0)
        SearchLayout.Controls.Add(_searchBox, 1, 0)
        SearchLayout.Controls.Add(_scaleFilter, 2, 0)

        ConfigureModelGrid()

        Dim DetailLayout As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 2,
            .RowCount = 1,
            .Padding = New Padding(0, 4, 0, 2)
        }
        DetailLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 68.0!))
        DetailLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 32.0!))
        _detailsBox.Dock = DockStyle.Fill
        _detailsBox.ReadOnly = True
        _detailsBox.BorderStyle = BorderStyle.FixedSingle
        _detailsBox.BackColor = SystemColors.Window
        _detailsBox.Font = New Font("Segoe UI", 9.0!)
        _detailsBox.Text = "Select a model to inspect its license, format, and source details."
        DetailLayout.Controls.Add(_detailsBox, 0, 0)

        Dim ChoicePanel As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 6,
            .Padding = New Padding(10, 0, 2, 0)
        }
        ChoicePanel.RowStyles.Add(New RowStyle(SizeType.Absolute, 20.0!))
        ChoicePanel.RowStyles.Add(New RowStyle(SizeType.Absolute, 30.0!))
        ChoicePanel.RowStyles.Add(New RowStyle(SizeType.Absolute, 34.0!))
        ChoicePanel.RowStyles.Add(New RowStyle(SizeType.Absolute, 54.0!))
        ChoicePanel.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0!))
        ChoicePanel.RowStyles.Add(New RowStyle(SizeType.Absolute, 25.0!))

        Dim FormatLabel As New Label With {
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Text = "Download format:"
        }
        _resourceComboBox.DropDownStyle = ComboBoxStyle.DropDownList
        _resourceComboBox.Dock = DockStyle.Fill
        _resourceComboBox.DisplayMember = "DisplayText"
        AddHandler _resourceComboBox.SelectedIndexChanged, AddressOf ResourceChanged

        _licenseReviewedCheckBox.Dock = DockStyle.Fill
        _licenseReviewedCheckBox.Text = "I reviewed the model license and trust this third-party checkpoint source."
        _licenseReviewedCheckBox.AutoSize = False
        AddHandler _licenseReviewedCheckBox.CheckedChanged, AddressOf UpdateDownloadButtonState

        _modelPageLink.Dock = DockStyle.Fill
        _modelPageLink.Text = "Open this model's OpenModelDB page"
        _modelPageLink.TextAlign = ContentAlignment.MiddleLeft
        AddHandler _modelPageLink.LinkClicked, AddressOf OpenModelPageClicked

        Dim SecurityWarning As New Label With {
            .Dock = DockStyle.Fill,
            .AutoEllipsis = True,
            .ForeColor = Color.FromArgb(145, 53, 43),
            .Text = "Security: .pth files may contain executable pickle data; SafeTensors is data-only. A matching hash does not verify the author. Only load models from sources you trust."
        }
        ChoicePanel.Controls.Add(FormatLabel, 0, 0)
        ChoicePanel.Controls.Add(_resourceComboBox, 0, 1)
        ChoicePanel.Controls.Add(_licenseReviewedCheckBox, 0, 2)
        ChoicePanel.Controls.Add(_modelPageLink, 0, 3)
        ChoicePanel.Controls.Add(SecurityWarning, 0, 4)
        Dim FolderLabel As New Label With {
            .Dock = DockStyle.Fill,
            .AutoEllipsis = True,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Text = "Install folder: " & _destinationFolder
        }
        ChoicePanel.Controls.Add(FolderLabel, 0, 5)
        DetailLayout.Controls.Add(ChoicePanel, 1, 0)

        Dim StatusLayout As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 2,
            .Padding = New Padding(0, 3, 0, 0)
        }
        StatusLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 22.0!))
        StatusLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 16.0!))
        _statusLabel.Dock = DockStyle.Fill
        _statusLabel.AutoEllipsis = True
        _statusLabel.Text = "Connecting to the OpenModelDB catalog…"
        _progressBar.Dock = DockStyle.Fill
        _progressBar.Style = ProgressBarStyle.Marquee
        _progressBar.MarqueeAnimationSpeed = 25
        StatusLayout.Controls.Add(_statusLabel, 0, 0)
        StatusLayout.Controls.Add(_progressBar, 0, 1)

        _refreshButton.Text = "Refresh catalog"
        _refreshButton.Width = 118
        _refreshButton.Height = 30
        _refreshButton.Margin = New Padding(4)
        AddHandler _refreshButton.Click, AddressOf RefreshClicked

        _downloadButton.Text = "Download and install"
        _downloadButton.Width = 150
        _downloadButton.Height = 30
        _downloadButton.Margin = New Padding(4)
        _downloadButton.Enabled = False
        AddHandler _downloadButton.Click, AddressOf DownloadClicked

        _cancelDownloadButton.Text = "Cancel download"
        _cancelDownloadButton.Width = 116
        _cancelDownloadButton.Height = 30
        _cancelDownloadButton.Margin = New Padding(4)
        _cancelDownloadButton.Visible = False
        AddHandler _cancelDownloadButton.Click, AddressOf CancelDownloadClicked

        _closeButton.Text = "Close"
        _closeButton.Width = 92
        _closeButton.Height = 30
        _closeButton.Margin = New Padding(4)
        _closeButton.DialogResult = DialogResult.Cancel
        AddHandler _closeButton.Click, AddressOf CloseClicked

        Dim Footer As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 2,
            .RowCount = 1,
            .Padding = New Padding(0, 2, 0, 0)
        }
        Footer.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0!))
        Footer.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        Footer.Controls.Add(_refreshButton, 0, 0)
        Dim ButtonFlow As New FlowLayoutPanel With {
            .Dock = DockStyle.Fill,
            .FlowDirection = FlowDirection.RightToLeft,
            .WrapContents = False,
            .AutoSize = True,
            .Margin = New Padding(0)
        }
        ButtonFlow.Controls.Add(_closeButton)
        ButtonFlow.Controls.Add(_cancelDownloadButton)
        ButtonFlow.Controls.Add(_downloadButton)
        Footer.Controls.Add(ButtonFlow, 1, 0)

        Dim MainLayout As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 6,
            .Padding = New Padding(10)
        }
        MainLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 48.0!))
        MainLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 38.0!))
        MainLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 50.0!))
        MainLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 50.0!))
        MainLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 45.0!))
        MainLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 42.0!))
        MainLayout.Controls.Add(HeaderLabel, 0, 0)
        MainLayout.Controls.Add(SearchLayout, 0, 1)
        MainLayout.Controls.Add(_modelGrid, 0, 2)
        MainLayout.Controls.Add(DetailLayout, 0, 3)
        MainLayout.Controls.Add(StatusLayout, 0, 4)
        MainLayout.Controls.Add(Footer, 0, 5)
        Controls.Add(MainLayout)
        AcceptButton = _downloadButton
        CancelButton = _closeButton
        AddHandler MyBase.Load, AddressOf DialogLoaded
    End Sub

    Private Sub ConfigureModelGrid()
        _modelGrid.Dock = DockStyle.Fill
        _modelGrid.AllowUserToAddRows = False
        _modelGrid.AllowUserToDeleteRows = False
        _modelGrid.AllowUserToResizeRows = False
        _modelGrid.AutoGenerateColumns = False
        _modelGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        _modelGrid.BackgroundColor = SystemColors.Window
        _modelGrid.BorderStyle = BorderStyle.FixedSingle
        _modelGrid.MultiSelect = False
        _modelGrid.ReadOnly = True
        _modelGrid.RowHeadersVisible = False
        _modelGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        _modelGrid.Columns.Add(New DataGridViewTextBoxColumn With {
            .Name = "ModelName", .HeaderText = "Model", .FillWeight = 34.0!, .SortMode = DataGridViewColumnSortMode.NotSortable
        })
        _modelGrid.Columns.Add(New DataGridViewTextBoxColumn With {
            .Name = "ModelScale", .HeaderText = "Scale", .FillWeight = 9.0!, .SortMode = DataGridViewColumnSortMode.NotSortable
        })
        _modelGrid.Columns.Add(New DataGridViewTextBoxColumn With {
            .Name = "ModelArchitecture", .HeaderText = "Architecture", .FillWeight = 18.0!, .SortMode = DataGridViewColumnSortMode.NotSortable
        })
        _modelGrid.Columns.Add(New DataGridViewTextBoxColumn With {
            .Name = "ModelFormats", .HeaderText = "Files", .FillWeight = 17.0!, .SortMode = DataGridViewColumnSortMode.NotSortable
        })
        _modelGrid.Columns.Add(New DataGridViewTextBoxColumn With {
            .Name = "ModelLicense", .HeaderText = "License", .FillWeight = 22.0!, .SortMode = DataGridViewColumnSortMode.NotSortable
        })
        AddHandler _modelGrid.SelectionChanged, AddressOf ModelSelectionChanged
    End Sub

    Private Async Sub DialogLoaded(sender As Object, e As EventArgs)
        Await LoadCatalogAsync()
    End Sub

    Private Async Sub RefreshClicked(sender As Object, e As EventArgs)
        Await LoadCatalogAsync()
    End Sub

    Private Async Function LoadCatalogAsync() As Task
        If _isBusy Then Return
        If String.IsNullOrWhiteSpace(_pythonExecutable) OrElse Not File.Exists(_pythonExecutable) Then
            SetStatus("Python 3.10+ was not found. Configure the Spandrel Python environment first.", False)
            Return
        End If
        If Not File.Exists(_scriptPath) Then
            SetStatus("The OpenModelDB helper is missing beside AutoCrispy.", False)
            Return
        End If

        _catalogItems.Clear()
        RebuildGrid()
        Dim OperationCancellation As New CancellationTokenSource()
        _cancelSource = OperationCancellation
        _cancelDownloadButton.Text = "Cancel request"
        _cancelDownloadButton.Visible = True
        SetBusy(True, "Fetching compatible models from OpenModelDB…", True)
        Try
            Dim Lines As List(Of String) = Await RunHelperAsync("--list", Nothing, OperationCancellation.Token)
            ParseCatalogLines(Lines)
            RebuildGrid()
            SetStatus(_catalogItems.Count.ToString() & " compatible catalog model(s) loaded. Select one to review its details and license.", False)
            _progressBar.Value = 0
            _progressBar.Style = ProgressBarStyle.Blocks
        Catch ex As OperationCanceledException
            SetStatus("Catalog request cancelled.", False)
        Catch ex As Exception
            SetStatus("Could not load the OpenModelDB catalog: " & ex.GetBaseException().Message, False)
        Finally
            SetBusy(False, _statusLabel.Text, False)
            _cancelDownloadButton.Visible = False
            _cancelDownloadButton.Text = "Cancel download"
            If ReferenceEquals(_cancelSource, OperationCancellation) Then _cancelSource = Nothing
            OperationCancellation.Dispose()
            UpdateDownloadButtonState(Nothing, EventArgs.Empty)
        End Try
    End Function

    Private Sub ParseCatalogLines(Lines As IEnumerable(Of String))
        Dim ItemsById As New Dictionary(Of String, OpenModelDbCatalogItem)(StringComparer.OrdinalIgnoreCase)
        For Each OutputLine As String In Lines
            If Not OutputLine.StartsWith(CatalogLinePrefix, StringComparison.Ordinal) Then Continue For
            Try
                Dim Fields As String() = OutputLine.Substring(CatalogLinePrefix.Length).Split(ControlChars.Tab)
                If Fields.Length <> 11 Then Continue For
                Dim Values As String() = Fields.Select(Function(Value As String) DecodeField(Value)).ToArray()
                Dim Scale As Integer = 0
                Dim SizeBytes As Long = 0
                If Not Integer.TryParse(Values(5), Scale) OrElse Not Long.TryParse(Values(10), SizeBytes) Then Continue For
                If Scale <> 1 AndAlso Scale <> 4 Then Continue For
                If Values(9) <> "pth" AndAlso Values(9) <> "safetensors" Then Continue For

                Dim Item As OpenModelDbCatalogItem = Nothing
                If Not ItemsById.TryGetValue(Values(0), Item) Then
                    Item = New OpenModelDbCatalogItem With {
                        .Id = Values(0),
                        .Name = Values(1),
                        .Author = Values(2),
                        .License = Values(3),
                        .Architecture = Values(4),
                        .Scale = Scale,
                        .Description = Values(6),
                        .Tags = Values(7),
                        .PageUrl = Values(8)
                    }
                    ItemsById.Add(Item.Id, Item)
                End If
                If Not Item.Resources.Any(Function(Resource As OpenModelDbResource) Resource.Format = Values(9)) Then
                    Item.Resources.Add(New OpenModelDbResource With {.Format = Values(9), .SizeBytes = SizeBytes})
                End If
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine("Skipping invalid OpenModelDB catalog row: " & ex.Message)
            End Try
        Next
        _catalogItems.AddRange(ItemsById.Values.OrderBy(Function(Item As OpenModelDbCatalogItem) Item.Scale).
            ThenBy(Function(Item As OpenModelDbCatalogItem) Item.Name, StringComparer.OrdinalIgnoreCase))
        ReloadExistingDestinationPaths()
    End Sub

    Private Shared Function DecodeField(Value As String) As String
        Return Encoding.UTF8.GetString(Convert.FromBase64String(Value))
    End Function

    Private Sub ReloadExistingDestinationPaths()
        _existingDestinationPaths.Clear()
        If String.IsNullOrWhiteSpace(_destinationFolder) OrElse Not Directory.Exists(_destinationFolder) Then Return
        Try
            For Each ExistingFile As String In Directory.GetFiles(_destinationFolder, "*.*", SearchOption.AllDirectories)
                If String.Equals(Path.GetExtension(ExistingFile), ".pth", StringComparison.OrdinalIgnoreCase) OrElse
                    String.Equals(Path.GetExtension(ExistingFile), ".safetensors", StringComparison.OrdinalIgnoreCase) Then
                    _existingDestinationPaths.Add(Path.GetFullPath(ExistingFile))
                End If
            Next
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("Could not inspect installed OpenModelDB models: " & ex.Message)
        End Try
    End Sub

    Private Sub SearchChanged(sender As Object, e As EventArgs)
        RebuildGrid()
    End Sub

    Private Sub RebuildGrid()
        _modelGrid.SuspendLayout()
        Try
            _modelGrid.Rows.Clear()
            Dim Query As String = _searchBox.Text.Trim()
            Dim ScaleChoice As Integer = If(_scaleFilter.SelectedIndex = 1, 4, If(_scaleFilter.SelectedIndex = 2, 1, 0))
            For Each Item As OpenModelDbCatalogItem In _catalogItems
                If ScaleChoice > 0 AndAlso Item.Scale <> ScaleChoice Then Continue For
                Dim SearchText As String = String.Join(" ", New String() {
                    Item.Name, Item.Id, Item.Author, Item.Architecture, Item.License, Item.Tags, Item.Description
                })
                If Query <> "" AndAlso SearchText.IndexOf(Query, StringComparison.OrdinalIgnoreCase) < 0 Then Continue For
                Dim RowIndex As Integer = _modelGrid.Rows.Add(
                    Item.Name,
                    Item.Scale.ToString() & "×",
                    Item.Architecture,
                    Item.FormatsText,
                    If(String.IsNullOrWhiteSpace(Item.License), "Not specified", Item.License)
                )
                _modelGrid.Rows(RowIndex).Tag = Item
            Next
            If _modelGrid.Rows.Count > 0 Then _modelGrid.Rows(0).Selected = True
        Finally
            _modelGrid.ResumeLayout()
        End Try
        ModelSelectionChanged(_modelGrid, EventArgs.Empty)
    End Sub

    Private Sub ModelSelectionChanged(sender As Object, e As EventArgs)
        _selectedItem = Nothing
        If _modelGrid.SelectedRows.Count > 0 Then
            _selectedItem = TryCast(_modelGrid.SelectedRows(0).Tag, OpenModelDbCatalogItem)
        ElseIf _modelGrid.CurrentRow IsNot Nothing Then
            _selectedItem = TryCast(_modelGrid.CurrentRow.Tag, OpenModelDbCatalogItem)
        End If

        _resourceComboBox.BeginUpdate()
        Try
            _resourceComboBox.Items.Clear()
            If _selectedItem IsNot Nothing Then
                For Each Resource As OpenModelDbResource In _selectedItem.Resources.OrderBy(
                    Function(Value As OpenModelDbResource) If(Value.Format = "safetensors", 0, 1))
                    _resourceComboBox.Items.Add(Resource)
                Next
            End If
            If _resourceComboBox.Items.Count > 0 Then _resourceComboBox.SelectedIndex = 0
        Finally
            _resourceComboBox.EndUpdate()
        End Try

        _licenseReviewedCheckBox.Checked = False
        If _selectedItem Is Nothing Then
            _detailsBox.Text = "No models match the current search."
            _modelPageLink.Enabled = False
            SetStatus("No models match the current search.", False)
        Else
            Dim LicenseText As String = If(String.IsNullOrWhiteSpace(_selectedItem.License), "Not specified in the catalog", _selectedItem.License)
            Dim AuthorText As String = If(String.IsNullOrWhiteSpace(_selectedItem.Author), "Unknown author", _selectedItem.Author)
            Dim ScaleText As String = If(_selectedItem.Scale = 1, "1× restoration", "4× super-resolution")
            _detailsBox.Text = _selectedItem.Name & Environment.NewLine &
                "OpenModelDB ID: " & _selectedItem.Id & Environment.NewLine &
                "Author: " & AuthorText & Environment.NewLine &
                "Scale / task: " & ScaleText & Environment.NewLine &
                "Architecture: " & _selectedItem.Architecture & Environment.NewLine &
                "License: " & LicenseText & Environment.NewLine &
                "Tags: " & If(String.IsNullOrWhiteSpace(_selectedItem.Tags), "(none listed)", _selectedItem.Tags) & Environment.NewLine & Environment.NewLine &
                "Description" & Environment.NewLine & _selectedItem.Description & Environment.NewLine & Environment.NewLine &
                "Compatibility is verified again with the installed Spandrel loader before the checkpoint is installed."
            _modelPageLink.Enabled = Uri.IsWellFormedUriString(_selectedItem.PageUrl, UriKind.Absolute)
            Dim SelectedResource As OpenModelDbResource = TryCast(_resourceComboBox.SelectedItem, OpenModelDbResource)
            SetStatus("License: " & If(String.IsNullOrWhiteSpace(_selectedItem.License), "not specified", _selectedItem.License) &
                If(SelectedResource Is Nothing, String.Empty, " · " & SelectedResource.DisplayText), False)
        End If
        UpdateDownloadButtonState(Nothing, EventArgs.Empty)
    End Sub

    Private Sub ResourceChanged(sender As Object, e As EventArgs)
        UpdateDownloadButtonState(Nothing, EventArgs.Empty)
        If _isBusy OrElse _selectedItem Is Nothing Then Return
        Dim Resource As OpenModelDbResource = TryCast(_resourceComboBox.SelectedItem, OpenModelDbResource)
        If Resource IsNot Nothing Then
            SetStatus("License: " & If(String.IsNullOrWhiteSpace(_selectedItem.License), "not specified", _selectedItem.License) &
                " · " & Resource.DisplayText, False)
        End If
    End Sub

    Private Sub UpdateDownloadButtonState(sender As Object, e As EventArgs)
        Dim Resource As OpenModelDbResource = TryCast(_resourceComboBox.SelectedItem, OpenModelDbResource)
        Dim IsAlreadyInstalled As Boolean = False
        If _selectedItem IsNot Nothing AndAlso Resource IsNot Nothing Then
            Dim FinalPath As String = Path.GetFullPath(Path.Combine(_destinationFolder, _selectedItem.Id & "." & Resource.Format))
            IsAlreadyInstalled = _existingModels.Contains(FinalPath) OrElse _existingDestinationPaths.Contains(FinalPath)
        End If
        _downloadButton.Enabled = Not _isBusy AndAlso _selectedItem IsNot Nothing AndAlso Resource IsNot Nothing AndAlso
            _licenseReviewedCheckBox.Checked AndAlso Not IsAlreadyInstalled
        _downloadButton.Text = If(IsAlreadyInstalled, "Already installed", "Download and install")
    End Sub

    Private Async Sub DownloadClicked(sender As Object, e As EventArgs)
        If _isBusy OrElse _selectedItem Is Nothing OrElse _resourceComboBox.SelectedItem Is Nothing Then Return
        If Not _licenseReviewedCheckBox.Checked Then Return
        If _installHandler Is Nothing Then
            SetStatus("The application could not prepare the model installer.", False)
            Return
        End If

        Dim SelectedItem As OpenModelDbCatalogItem = _selectedItem
        Dim SelectedResource As OpenModelDbResource = DirectCast(_resourceComboBox.SelectedItem, OpenModelDbResource)
        Dim OperationCancellation As New CancellationTokenSource()
        _cancelSource = OperationCancellation
        SetBusy(True, "Preparing a verified download…", True)
        _progressBar.Style = ProgressBarStyle.Blocks
        _progressBar.Value = 0
        _cancelDownloadButton.Text = "Cancel download"
        _cancelDownloadButton.Visible = True
        Dim TempDirectory As String = Path.Combine(Path.GetTempPath(), "AutoCrispy", "OpenModelDB", Guid.NewGuid().ToString("N"))
        Dim TemporaryFile As String = Path.Combine(TempDirectory, SelectedItem.Id & "." & SelectedResource.Format)
        Try
            Directory.CreateDirectory(TempDirectory)
            Dim Arguments As String = "--download --id " & QuoteArgument(SelectedItem.Id) &
                " --format " & QuoteArgument(SelectedResource.Format) &
                " --destination " & QuoteArgument(TemporaryFile)
            Dim Lines As List(Of String) = Await RunHelperAsync(Arguments, AddressOf DownloadProgress, OperationCancellation.Token)
            If Not Lines.Any(Function(Line As String) Line.StartsWith("OPENMODELDB_DOWNLOAD_COMPLETE" & vbTab, StringComparison.Ordinal)) Then
                Throw New InvalidDataException("The downloader did not report a verified file.")
            End If
            OperationCancellation.Token.ThrowIfCancellationRequested()
            SetBusy(True, "Testing the checkpoint with AutoCrispy's installed Spandrel…", True)
            DownloadedModelPath = Await _installHandler(
                SelectedItem, SelectedResource, TemporaryFile, AddressOf SetStatusFromWorker, OperationCancellation.Token)
            If String.IsNullOrWhiteSpace(DownloadedModelPath) OrElse Not File.Exists(DownloadedModelPath) Then
                Throw New IOException("The model was validated but was not installed in the shared models folder.")
            End If
            _isClosing = True
            DialogResult = DialogResult.OK
            Close()
        Catch ex As OperationCanceledException
            SetStatus("Download cancelled. No unverified checkpoint was installed.", False)
            _cancelDownloadButton.Visible = False
        Catch ex As Exception
            SetStatus("Could not install this model: " & ex.GetBaseException().Message, False)
            _cancelDownloadButton.Visible = False
        Finally
            Try
                If Directory.Exists(TempDirectory) Then Directory.Delete(TempDirectory, True)
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine("Could not remove temporary OpenModelDB files: " & ex.Message)
            End Try
            If ReferenceEquals(_cancelSource, OperationCancellation) Then _cancelSource = Nothing
            OperationCancellation.Dispose()
            If Not IsDisposed AndAlso Not _isClosing Then
                SetBusy(False, _statusLabel.Text, False)
                _cancelDownloadButton.Visible = False
                UpdateDownloadButtonState(Nothing, EventArgs.Empty)
            End If
        End Try
    End Sub

    Private Async Function RunHelperAsync(Arguments As String, ProgressHandler As Action(Of String),
                                          Token As CancellationToken) As Task(Of List(Of String))
        Dim StartInfo As New ProcessStartInfo(_pythonExecutable, QuoteArgument(_scriptPath) & " " & Arguments) With {
            .WorkingDirectory = Path.GetDirectoryName(_scriptPath),
            .UseShellExecute = False,
            .CreateNoWindow = True,
            .RedirectStandardOutput = True,
            .RedirectStandardError = True
        }
        Using HelperProcess As New Process With {.StartInfo = StartInfo}
            If Not HelperProcess.Start() Then Throw New InvalidOperationException("Could not start the OpenModelDB helper process.")
            Dim OutputLines As New List(Of String)
            Dim LinesLock As New Object()
            Dim ErrorTask As Task(Of String) = HelperProcess.StandardError.ReadToEndAsync()
            Dim OutputTask As Task = Task.Run(
                Sub()
                    While True
                        Dim OutputLine As String = HelperProcess.StandardOutput.ReadLine()
                        If OutputLine Is Nothing Then Exit While
                        SyncLock LinesLock
                            OutputLines.Add(OutputLine)
                        End SyncLock
                        If ProgressHandler IsNot Nothing Then ProgressHandler(OutputLine)
                    End While
                End Sub)
            Dim WaitTask As Task = Task.Run(Sub() HelperProcess.WaitForExit())
            Using CancellationRegistration As CancellationTokenRegistration = Token.Register(
                Sub()
                    Try
                        If Not HelperProcess.HasExited Then HelperProcess.Kill()
                    Catch ex As Exception
                        System.Diagnostics.Debug.WriteLine("Could not stop OpenModelDB helper: " & ex.Message)
                    End Try
                End Sub)
                Await Task.WhenAll(OutputTask, ErrorTask, WaitTask)
                Token.ThrowIfCancellationRequested()
                If HelperProcess.ExitCode <> 0 Then
                    Dim ErrorText As String = ErrorTask.Result.Trim()
                    If ErrorText = "" Then ErrorText = "OpenModelDB helper exited with code " & HelperProcess.ExitCode.ToString() & "."
                    Throw New InvalidOperationException(ErrorText)
                End If
            End Using
            SyncLock LinesLock
                Return OutputLines.ToList()
            End SyncLock
        End Using
    End Function

    Private Sub DownloadProgress(OutputLine As String)
        If Not OutputLine.StartsWith(ProgressLinePrefix, StringComparison.Ordinal) Then Return
        Dim Fields As String() = OutputLine.Substring(ProgressLinePrefix.Length).Split(ControlChars.Tab)
        If Fields.Length < 2 Then Return
        Dim Received As Long = 0
        Dim Total As Long = 0
        If Not Long.TryParse(Fields(0), Received) OrElse Not Long.TryParse(Fields(1), Total) OrElse Total <= 0 Then Return
        Dim Percent As Integer = CInt(Math.Max(0, Math.Min(100, Math.Floor(Received * 100.0R / Total))))
        SetStatusFromWorker("Downloading and verifying… " & FormatBytes(Received) & " / " & FormatBytes(Total) & " (" & Percent.ToString() & "%)")
        If IsHandleCreated AndAlso Not IsDisposed Then
            Try
                BeginInvoke(New Action(Sub()
                    _progressBar.Style = ProgressBarStyle.Blocks
                    _progressBar.Value = Percent
                End Sub))
            Catch ex As InvalidOperationException
                ' The form can close while a canceled helper flushes its last output line.
            End Try
        End If
    End Sub

    Private Sub SetStatusFromWorker(Message As String)
        If IsDisposed OrElse Not IsHandleCreated Then Return
        Try
            BeginInvoke(New Action(Sub()
                If Not IsDisposed Then _statusLabel.Text = Message
            End Sub))
        Catch ex As InvalidOperationException
            ' A completed dialog may dispose its controls before a worker reports its final status.
        End Try
    End Sub

    Private Sub CancelDownloadClicked(sender As Object, e As EventArgs)
        If Not _isBusy Then Return
        _statusLabel.Text = "Canceling download…"
        _cancelDownloadButton.Enabled = False
        Try
            If _cancelSource IsNot Nothing Then _cancelSource.Cancel()
        Catch ex As ObjectDisposedException
            ' Dialog is already closing.
        End Try
    End Sub

    Private Sub CloseClicked(sender As Object, e As EventArgs)
        If _isBusy Then
            CancelDownloadClicked(sender, e)
            DialogResult = DialogResult.None
        End If
    End Sub

    Private Sub OpenModelPageClicked(sender As Object, e As LinkLabelLinkClickedEventArgs)
        If _selectedItem Is Nothing OrElse Not Uri.IsWellFormedUriString(_selectedItem.PageUrl, UriKind.Absolute) Then Return
        Try
            Process.Start(New ProcessStartInfo(_selectedItem.PageUrl) With {.UseShellExecute = True})
        Catch ex As Exception
            SetStatus("Could not open the OpenModelDB page: " & ex.GetBaseException().Message, False)
        End Try
    End Sub

    Private Sub SetBusy(IsBusy As Boolean, Message As String, IsMarquee As Boolean)
        _isBusy = IsBusy
        _searchBox.Enabled = Not IsBusy
        _scaleFilter.Enabled = Not IsBusy
        _modelGrid.Enabled = Not IsBusy
        _resourceComboBox.Enabled = Not IsBusy
        _licenseReviewedCheckBox.Enabled = Not IsBusy
        _refreshButton.Enabled = Not IsBusy
        _downloadButton.Enabled = Not IsBusy AndAlso _selectedItem IsNot Nothing AndAlso
            _resourceComboBox.SelectedItem IsNot Nothing AndAlso _licenseReviewedCheckBox.Checked
        _cancelDownloadButton.Enabled = IsBusy
        _closeButton.Enabled = Not IsBusy
        _progressBar.Style = If(IsMarquee, ProgressBarStyle.Marquee, ProgressBarStyle.Blocks)
        _progressBar.MarqueeAnimationSpeed = If(IsMarquee, 25, 0)
        _statusLabel.Text = Message
    End Sub

    Private Sub SetStatus(Message As String, IsBusy As Boolean)
        _statusLabel.Text = Message
        If Not IsBusy Then
            _progressBar.Style = ProgressBarStyle.Blocks
            _progressBar.Value = 0
            _progressBar.MarqueeAnimationSpeed = 0
        End If
    End Sub

    Private Shared Function QuoteArgument(Value As String) As String
        Return ControlChars.Quote & If(Value, String.Empty) & ControlChars.Quote
    End Function

    Friend Shared Function FormatBytes(Value As Long) As String
        If Value < 0 Then Return "unknown size"
        Dim Size As Double = Value
        Dim Units As String() = {"B", "KB", "MB", "GB", "TB"}
        Dim UnitIndex As Integer = 0
        While Size >= 1024 AndAlso UnitIndex < Units.Length - 1
            Size /= 1024
            UnitIndex += 1
        End While
        Return Size.ToString("0.##", Globalization.CultureInfo.InvariantCulture) & " " & Units(UnitIndex)
    End Function

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        If _isBusy AndAlso Not _isClosing Then
            e.Cancel = True
            CancelDownloadClicked(Me, EventArgs.Empty)
            Return
        End If
        MyBase.OnFormClosing(e)
    End Sub
End Class
