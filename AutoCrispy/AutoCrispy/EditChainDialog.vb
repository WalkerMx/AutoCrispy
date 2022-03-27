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
        Colorize(EditTextBox, Color.DarkBlue, Color.Maroon)
    End Sub

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