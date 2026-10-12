Imports System.Globalization
Imports System.IO

Friend Class AutoRoutePreviewItem
    Public Property FilePath As String
    Public Property Role As String
    Public Property Score As Double
    Public Property Detail As Double
    Public Property EdgeDensity As Double
    Public Property OrientationEntropy As Double
    Public Property LocalPatternEntropy As Double
    Public Property Periodicity As Double
    Public Property DecisionReason As String

    Public ReadOnly Property DisplayText As String
        Get
            Return Path.GetFileName(FilePath) & "  ·  " & Score.ToString("0.000", CultureInfo.InvariantCulture)
        End Get
    End Property
End Class

Friend NotInheritable Class AutoRoutePreviewDialog
    Inherits Form

    Private ReadOnly _imagePreview As New PictureBox()
    Private ReadOnly _featureDetails As New Label()
    Private ReadOnly _painterList As New ListBox()
    Private ReadOnly _architectList As New ListBox()
    Private ReadOnly _items As List(Of AutoRoutePreviewItem)

    Public Sub New(PreviewItems As IEnumerable(Of AutoRoutePreviewItem), SourceFolder As String,
                   SampleLimit As Integer, PainterShare As Integer, PainterThreshold As Double,
                   ArchitectModel As String, PainterModel As String)
        If PreviewItems Is Nothing Then
            _items = New List(Of AutoRoutePreviewItem)()
        Else
            _items = PreviewItems.ToList()
        End If

        Text = "Auto Texture Routing preview"
        StartPosition = FormStartPosition.CenterParent
        ClientSize = New Size(1080, 700)
        MinimumSize = New Size(850, 560)
        ShowInTaskbar = False

        Dim PainterItems As List(Of AutoRoutePreviewItem) = _items.Where(Function(Item) String.Equals(Item.Role, "Painter", StringComparison.OrdinalIgnoreCase)).OrderByDescending(Function(Item) Item.Score).ThenBy(Function(Item) Item.FilePath, StringComparer.OrdinalIgnoreCase).ToList()
        Dim ArchitectItems As List(Of AutoRoutePreviewItem) = _items.Where(Function(Item) String.Equals(Item.Role, "Architect", StringComparison.OrdinalIgnoreCase)).OrderByDescending(Function(Item) Item.Score).ThenBy(Function(Item) Item.FilePath, StringComparer.OrdinalIgnoreCase).ToList()

        Dim PainterModelName As String = If(String.IsNullOrWhiteSpace(PainterModel), "(not selected)", Path.GetFileName(PainterModel))
        Dim ArchitectModelName As String = If(String.IsNullOrWhiteSpace(ArchitectModel), "(not selected)", Path.GetFileName(ArchitectModel))
        Dim Summary As New Label With {
            .Dock = DockStyle.Fill,
            .AutoEllipsis = True,
            .Padding = New Padding(10, 6, 10, 4),
            .Text = String.Format(CultureInfo.InvariantCulture,
                "Read-only sample: first up to {1} supported images by path ({2} analyzed) · Painter {3} · Architect {4} · cap {5}% for 10+ · threshold {6:0.00}{0}" &
                "Folder: {7}{0}" &
                "Painter model: {8}    |    Architect model: {9}{0}" &
                "Score = 0.08×detail + 0.05×normalized edge score + 0.15×direction entropy + 0.60×patterns + 0.12×repetition×patterns.{0}" &
                "Sample roles are relative to these images; the full-folder Painter cap may select different textures.",
                Environment.NewLine, SampleLimit, _items.Count, PainterItems.Count, ArchitectItems.Count,
                PainterShare, PainterThreshold, SourceFolder, PainterModelName, ArchitectModelName)
        }

        Dim Split As New SplitContainer With {
            .Dock = DockStyle.Fill,
            .Size = New Size(1000, 540),
            .Orientation = Orientation.Vertical,
            .Panel1MinSize = 240,
            .Panel2MinSize = 320,
            .SplitterDistance = 440
        }
        Dim Tabs As New TabControl With {.Dock = DockStyle.Fill}
        Dim PainterPage As New TabPage("Painter (" & PainterItems.Count.ToString() & ")")
        Dim ArchitectPage As New TabPage("Architect (" & ArchitectItems.Count.ToString() & ")")

        ConfigureList(_painterList)
        ConfigureList(_architectList)
        AddHandler _painterList.SelectedIndexChanged, AddressOf PreviewSelectionChanged
        AddHandler _architectList.SelectedIndexChanged, AddressOf PreviewSelectionChanged
        For Each Item As AutoRoutePreviewItem In PainterItems
            _painterList.Items.Add(Item)
        Next
        For Each Item As AutoRoutePreviewItem In ArchitectItems
            _architectList.Items.Add(Item)
        Next
        PainterPage.Controls.Add(_painterList)
        ArchitectPage.Controls.Add(_architectList)
        Tabs.TabPages.Add(PainterPage)
        Tabs.TabPages.Add(ArchitectPage)
        Split.Panel1.Controls.Add(Tabs)

        Dim PreviewLayout As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 2,
            .Padding = New Padding(6)
        }
        PreviewLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 72.0!))
        PreviewLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 28.0!))
        _imagePreview.Dock = DockStyle.Fill
        _imagePreview.SizeMode = PictureBoxSizeMode.Zoom
        _imagePreview.BackColor = SystemColors.ControlDark
        _featureDetails.Dock = DockStyle.Fill
        _featureDetails.BorderStyle = BorderStyle.FixedSingle
        _featureDetails.Padding = New Padding(10)
        _featureDetails.TextAlign = ContentAlignment.MiddleLeft
        _featureDetails.AutoEllipsis = True
        _featureDetails.Text = "Select an image to see the route score and feature values."
        PreviewLayout.Controls.Add(_imagePreview, 0, 0)
        PreviewLayout.Controls.Add(_featureDetails, 0, 1)
        Split.Panel2.Controls.Add(PreviewLayout)

        Dim CloseButton As New Button With {
            .Text = "Close",
            .Width = 100,
            .Height = 30,
            .DialogResult = DialogResult.OK,
            .Margin = New Padding(4)
        }
        Dim ButtonPanel As New FlowLayoutPanel With {
            .Dock = DockStyle.Fill,
            .FlowDirection = FlowDirection.RightToLeft,
            .WrapContents = False,
            .Padding = New Padding(0, 3, 4, 3)
        }
        ButtonPanel.Controls.Add(CloseButton)

        Dim Layout As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 3,
            .Padding = New Padding(8)
        }
        Layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 124.0!))
        Layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0!))
        Layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 42.0!))
        Layout.Controls.Add(Summary, 0, 0)
        Layout.Controls.Add(Split, 0, 1)
        Layout.Controls.Add(ButtonPanel, 0, 2)
        Controls.Add(Layout)
        AcceptButton = CloseButton
        CancelButton = CloseButton

        If _painterList.Items.Count > 0 Then
            _painterList.SelectedIndex = 0
        ElseIf _architectList.Items.Count > 0 Then
            Tabs.SelectedTab = ArchitectPage
            _architectList.SelectedIndex = 0
        Else
            _featureDetails.Text = "No supported image textures were found in this folder."
        End If
    End Sub

    Private Shared Sub ConfigureList(Source As ListBox)
        Source.Dock = DockStyle.Fill
        Source.DisplayMember = "DisplayText"
        Source.HorizontalScrollbar = True
        Source.IntegralHeight = False
        Source.SelectionMode = SelectionMode.One
    End Sub

    Private Sub PreviewSelectionChanged(sender As Object, e As EventArgs)
        Dim Source As ListBox = TryCast(sender, ListBox)
        If Source Is Nothing OrElse Source.SelectedItem Is Nothing Then Return
        Dim Item As AutoRoutePreviewItem = TryCast(Source.SelectedItem, AutoRoutePreviewItem)
        If Item Is Nothing Then Return

        Dim ExistingImage As Image = _imagePreview.Image
        _imagePreview.Image = Nothing
        If ExistingImage IsNot Nothing Then ExistingImage.Dispose()

        _featureDetails.Text = String.Format(CultureInfo.InvariantCulture,
            "Route: {1} — {2}{0}Feature score: {3:0.000}{0}Detail: {4:0.000}    Edge density: {5:0.000}{0}Direction entropy: {6:0.000}    Local-pattern entropy: {7:0.000}{0}Periodicity: {8:0.000}{0}{9}",
            Environment.NewLine, Item.Role, Item.DecisionReason, Item.Score, Item.Detail,
            Item.EdgeDensity, Item.OrientationEntropy, Item.LocalPatternEntropy,
            Item.Periodicity, Item.FilePath)

        Try
            Using SourceStream As New FileStream(Item.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                Using SourceImage As Image = Image.FromStream(SourceStream)
                    Dim ResizeScale As Double = Math.Min(1.0R, 1400.0R / Math.Max(SourceImage.Width, SourceImage.Height))
                    Dim ThumbnailSize As New Size(
                        Math.Max(1, CInt(Math.Round(SourceImage.Width * ResizeScale))),
                        Math.Max(1, CInt(Math.Round(SourceImage.Height * ResizeScale))))
                    Dim Thumbnail As New Bitmap(ThumbnailSize.Width, ThumbnailSize.Height)
                    Using Canvas As Graphics = Graphics.FromImage(Thumbnail)
                        Canvas.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic
                        Canvas.DrawImage(SourceImage, New Rectangle(Point.Empty, ThumbnailSize))
                    End Using
                    _imagePreview.Image = Thumbnail
                End Using
            End Using
        Catch ex As Exception
            _featureDetails.Text &= Environment.NewLine & "Thumbnail unavailable: " & ex.Message
        End Try
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            If _imagePreview.Image IsNot Nothing Then
                _imagePreview.Image.Dispose()
                _imagePreview.Image = Nothing
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub
End Class
