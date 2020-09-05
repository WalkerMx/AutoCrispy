Public Class EditChainDialog

    Public Property InternalText As String
    Public Property ResultText As String

    Public Sub New(Xml As String)
        InitializeComponent()
        InternalText = Xml
        EditTextBox.Text = Xml
    End Sub

    Private Sub OK_Button_Click(ByVal sender As Object, ByVal e As EventArgs) Handles OK_Button.Click
        Me.DialogResult = DialogResult.OK
        ResultText = EditTextBox.Text
        Me.Close()
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As Object, ByVal e As EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub Reset_Button_Click(sender As Object, e As EventArgs) Handles Reset_Button.Click
        EditTextBox.Text = InternalText
    End Sub

End Class