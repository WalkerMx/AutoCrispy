Imports System.Collections.Generic
Imports System.Drawing
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Windows.Forms

Public Class EditChainDialog

    Public Property InternalText As String
    Public Property ResultText As String

    Private ReadOnly XmlOptionHelp As New RichTextBox()
    Private ChainPackageType As String = String.Empty

    Public Sub New(Xml As String, Optional PackageType As String = "")
        InitializeComponent()
        InternalText = Xml
        ChainPackageType = If(PackageType, String.Empty)
        EditTextBox.Text = Xml

        Text = "Advanced chain editor"
        ClientSize = New Size(1040, 560)
        MinimumSize = New Size(850, 480)

        Dim EditorAndHelp As New SplitContainer()
        EditorAndHelp.Dock = DockStyle.Fill
        EditorAndHelp.Orientation = Orientation.Vertical
        EditorAndHelp.Size = New Size(1040, 500)
        EditorAndHelp.Panel1MinSize = 460
        EditorAndHelp.Panel2MinSize = 230
        EditorAndHelp.SplitterDistance = 730

        SplitContainer1.Panel1.Controls.Remove(EditTextBox)
        EditTextBox.Dock = DockStyle.Fill
        EditorAndHelp.Panel1.Controls.Add(EditTextBox)

        XmlOptionHelp.Dock = DockStyle.Fill
        XmlOptionHelp.ReadOnly = True
        XmlOptionHelp.BorderStyle = BorderStyle.None
        XmlOptionHelp.BackColor = SystemColors.Control
        XmlOptionHelp.ForeColor = SystemColors.ControlText
        XmlOptionHelp.Font = New Font("Segoe UI", 9.0!, FontStyle.Regular)
        XmlOptionHelp.DetectUrls = False
        XmlOptionHelp.ScrollBars = RichTextBoxScrollBars.Vertical
        XmlOptionHelp.WordWrap = True
        XmlOptionHelp.TabStop = False
        XmlOptionHelp.Padding = New Padding(8)
        EditorAndHelp.Panel2.Controls.Add(XmlOptionHelp)
        SplitContainer1.Panel1.Controls.Add(EditorAndHelp)

        AddHandler EditTextBox.TextChanged, AddressOf EditTextBox_TextChanged
        UpdateXmlOptionHelp()
    End Sub

    Public Sub EditChainDialog_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Colorize(EditTextBox, Color.DarkBlue, Color.Maroon)
    End Sub

    Private Sub EditTextBox_TextChanged(sender As Object, e As EventArgs)
        UpdateXmlOptionHelp()
    End Sub

    Private Sub UpdateXmlOptionHelp()
        If XmlOptionHelp Is Nothing OrElse XmlOptionHelp.IsDisposed Then Return
        XmlOptionHelp.Text = BuildXmlOptionHelp(EditTextBox.Text)
        XmlOptionHelp.Select(0, 0)
    End Sub

    Private Function BuildXmlOptionHelp(Xml As String) As String
        Dim Help As New StringBuilder()
        Help.AppendLine("What can I switch on or off?")
        If Not String.IsNullOrWhiteSpace(ChainPackageType) Then
            Help.AppendLine("Step type: " & ChainPackageType)
        End If
        Help.AppendLine()
        Help.AppendLine("true = on; false = off. This list updates as you edit the XML:")

        Dim Matches As MatchCollection = Regex.Matches(
            If(Xml, String.Empty),
            "<(?<name>[A-Za-z_][A-Za-z0-9_]*)>\s*(?<value>true|false)\s*</\k<name>\s*>",
            RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant)
        Dim SeenFields As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Dim FoundOption As Boolean = False
        For Each OptionMatch As Match In Matches
            Dim FieldName As String = OptionMatch.Groups("name").Value
            If Not SeenFields.Add(FieldName) Then Continue For
            FoundOption = True
            Dim IsEnabled As Boolean = String.Equals(OptionMatch.Groups("value").Value, "true", StringComparison.OrdinalIgnoreCase)
            Help.AppendLine()
            Help.AppendLine(FieldName & " — " & If(IsEnabled, "ON", "OFF"))
            Help.AppendLine("  " & DescribeBooleanOption(FieldName))
        Next

        If Not FoundOption Then
            Help.AppendLine()
            Help.AppendLine("No true/false switches were found in this step. It may only have numeric or text settings.")
        End If

        Help.AppendLine()
        Help.AppendLine("Safer editing: change ordinary options in the main settings panel and add/update the step there. Keep Name, IconIndex, PackageType, and FileTypes intact; changing the XML structure can make the chain fail to load.")
        If String.Equals(ChainPackageType, "Spandrel", StringComparison.OrdinalIgnoreCase) Then
            Help.AppendLine()
            Help.AppendLine("Spandrel: Update step replaces the latest Spandrel stage. Hold Shift while clicking Update step to append another stage.")
        End If
        Return Help.ToString()
    End Function

    Private Function DescribeBooleanOption(FieldName As String) As String
        Select Case FieldName.ToLowerInvariant()
            Case "taa"
                Return "Enables or disables the backend's test-time augmentation option."
            Case "cpuonly"
                Return "When on, inference uses the CPU only; when off, the backend may use the GPU."
            Case "autorouteenabled"
                If String.Equals(ChainPackageType, "Spandrel", StringComparison.OrdinalIgnoreCase) Then
                    Return "When on, Spandrel routes textures between the saved Architect and Painter models; when off, it uses the single saved model."
                End If
                Return "Automatic texture routing is Spandrel-only; leave this off for the current backend."
            Case "gpu"
                If String.Equals(ChainPackageType, "Waifu2x CPP", StringComparison.OrdinalIgnoreCase) Then
                    Return "For Waifu2x CPP, on passes --disable-gpu (GPU off); off allows GPU use."
                End If
                Return "Enables or disables GPU processing for this backend."
            Case "forceopencl"
                Return "When on, requests OpenCL for Waifu2x CPP GPU processing."
            Case "preprocess"
                Return "Runs Anime4K's pre-processing stage when on."
            Case "postprocess"
                Return "Runs Anime4K's post-processing stage when on."
            Case "prefilter"
                Return "Runs the selected Anime4K pre-filter when on."
            Case "postfilter"
                Return "Runs the selected Anime4K post-filter when on."
            Case "cnn"
                Return "Enables or disables Anime4K's CNN processing option."
            Case "forcedx9"
                Return "For TexConv DDS output, forces the DirectX 9 feature path when on."
            Case "forcedx10"
                Return "For TexConv DDS output, forces the DirectX 10 feature path when on. Avoid enabling both DirectX overrides together."
            Case "seperatealpha"
                Return "Passes TexConv's separate-alpha option when on."
            Case "premultiplyalpha"
                Return "Passes TexConv's premultiply-alpha option when on."
            Case "straightalpha"
                Return "Passes TexConv's straight-alpha option when on."
            Case Else
                Return "This is a saved on/off option for the current processing step."
        End Select
    End Function

    Private Sub OK_Button_Click(sender As Object, e As EventArgs) Handles OK_Button.Click
        Me.DialogResult = DialogResult.OK
        ResultText = EditTextBox.Text
        Me.Close()
    End Sub

    Private Sub Cancel_Button_Click(sender As Object, e As EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub Reset_Button_Click(sender As Object, e As EventArgs) Handles Reset_Button.Click
        EditTextBox.Text = InternalText
        Colorize(EditTextBox, Color.DarkBlue, Color.Maroon)
    End Sub

    Private Sub Colorize(ByRef PlainTextBox As RichTextBox, TagColor As Color, QuoteColor As Color)
        Dim TagMatches = Regex.Matches(PlainTextBox.Text, "(?<=<)(.*?)(?=>)", RegexOptions.Compiled)
        For Each TagMatch As Match In TagMatches
            PlainTextBox.Select(TagMatch.Index, TagMatch.Length)
            PlainTextBox.SelectionColor = TagColor
        Next
        Dim QuoteMatches = Regex.Matches(PlainTextBox.Text, "\" & ControlChars.Quote & "(.*?)" & "\" & ControlChars.Quote, RegexOptions.Compiled)
        For Each QuoteMatch As Match In QuoteMatches
            If Not QuoteMatch.Value.Contains("="c) Then
                PlainTextBox.Select(QuoteMatch.Index, QuoteMatch.Length)
                PlainTextBox.SelectionColor = QuoteColor
            End If
        Next
        PlainTextBox.Select(0, 0)
    End Sub

End Class
