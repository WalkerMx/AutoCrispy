Imports System.Text.RegularExpressions

Public Class EditChainDialog

    Public Property InternalText As String
    Public Property ResultText As String

    Public Sub New(Xml As String)
        InitializeComponent()
        InternalText = Xml
        EditTextBox.Text = Xml
    End Sub

    Public Sub EditChainDialog_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim ButtonArea As Integer = 48
        Dim DesiredSplitter As Integer = SplitContainer1.Height - SplitContainer1.SplitterWidth - ButtonArea
        If DesiredSplitter > SplitContainer1.Panel1MinSize AndAlso DesiredSplitter < SplitContainer1.Height - SplitContainer1.SplitterWidth - 44 Then
            SplitContainer1.Panel2MinSize = 44
            SplitContainer1.SplitterDistance = DesiredSplitter
        End If
        TableLayoutPanel1.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        TableLayoutPanel2.Anchor = AnchorStyles.Top Or AnchorStyles.Left
        TableLayoutPanel1.Location = New Point(Math.Max(0, SplitContainer1.Panel2.ClientSize.Width - TableLayoutPanel1.Width - 8), 8)
        TableLayoutPanel2.Location = New Point(8, 8)
        Colorize(EditTextBox, Color.DarkBlue, Color.Maroon)
    End Sub

    Private Sub OK_Button_Click(sender As Object, e As EventArgs) Handles OK_Button.Click
        Dim Parsed As FormSettings.ChainObject
        Try
            Parsed = Form1.Deserialize(Of FormSettings.ChainObject)(EditTextBox.Text)
        Catch ex As Exception
            MessageBox.Show("These settings could not be parsed." & Environment.NewLine & ex.Message, "Edit Chain", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End Try
        If String.IsNullOrWhiteSpace(Parsed.Name) Then
            MessageBox.Show("The chain step needs a name.", "Edit Chain", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        If String.IsNullOrWhiteSpace(Parsed.PackageType) Then
            MessageBox.Show("The chain step needs a package type.", "Edit Chain", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        If Parsed.IconIndex < 0 OrElse Parsed.IconIndex > 8 Then
            MessageBox.Show("Icon index must be between 0 and 8.", "Edit Chain", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
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