Imports System.Reflection

Public Class DragDropList

    Private ListImage As Bitmap
    Private WithEvents ListCanvas As PictureBox
    Public ListItems As New List(Of DragDropItem)
    Public TempListItems As New List(Of DragDropItem)

    Private ImageWidth As Integer
    Private ImageHeight As Integer

    Private ThumbsPerRow As Integer
    Private ThumbSize As Integer

    Private ClickedIndex As Integer
    Private CurrentIndex As Integer

    Private WithEvents DragTimer As New Timer With {.Interval = 100, .Enabled = False}

    Public Sub New(ByRef _PictureBox As PictureBox, _ThumbsPerRow As Integer)
        ListCanvas = _PictureBox
        ImageWidth = _PictureBox.Width
        ImageHeight = _PictureBox.Height
        ThumbsPerRow = _ThumbsPerRow
        ThumbSize = Math.Floor((ImageWidth - (10 + (10 * ThumbsPerRow))) / ThumbsPerRow)
    End Sub

    Public Structure DragDropItem
        Public Property Index As Integer
        Public Property Name As String
        Public Property Thumbnail As Bitmap
        Public Sub New(_Index As Integer, _Name As String, _Thumbnail As Bitmap)
            Index = _Index
            Name = _Name
            Thumbnail = _Thumbnail
        End Sub
    End Structure

    Private Sub DragTimer_Tick(sender As Object, e As EventArgs) Handles DragTimer.Tick
        Dim NewIndex As Integer = GetCurrentIndex()
        If CurrentIndex <> NewIndex Then
            TempListItems.RemoveAt(CurrentIndex)
            TempListItems.Insert(NewIndex, ListItems(ClickedIndex))
            CurrentIndex = NewIndex
            DrawList(TempListItems)
        End If
    End Sub

    Public Sub ListCanvas_MouseDown(sender As Object, e As MouseEventArgs) Handles ListCanvas.MouseDown
        ClickedIndex = GetCurrentIndex()
        CurrentIndex = GetCurrentIndex()
        If e.Button = MouseButtons.Left Then
            TempListItems.Clear()
            TempListItems.AddRange(ListItems)
            Form1.Cursor = Cursors.SizeAll
            DragTimer.Enabled = True
        End If
    End Sub

    Public Sub ListCanvas_MouseUp(sender As Object, e As MouseEventArgs) Handles ListCanvas.MouseUp
        If e.Button = MouseButtons.Left Then
            DragTimer.Enabled = False
            Form1.Cursor = Cursors.Default
            ListItems.Clear()
            ListItems.AddRange(TempListItems)
            ReorderList()
        End If
    End Sub

    Public Sub ReorderList()
        For i = 0 To ListItems.Count - 1
            ListItems(i) = New DragDropList.DragDropItem(i, ListItems(i).Name, ListItems(i).Thumbnail)
        Next
    End Sub

    Public Function GetCurrentIndex()
        Dim MPos As Point = ListCanvas.PointToClient(Control.MousePosition)
        If (MPos.X >= 0) AndAlso (MPos.X <= ListCanvas.Width) AndAlso (MPos.Y >= 0) AndAlso (MPos.Y <= ListCanvas.Height) Then
            Dim XPos As Integer = Math.Floor(MPos.X / (ThumbSize + 10))
            Dim YPos As Integer = Math.Floor(MPos.Y / (ThumbSize + 10))
            Dim Width As Integer = Math.Floor((ImageWidth - 10) / (ThumbSize + 10))
            Dim Index As Integer = (YPos * Width) + XPos
            If Index < ListItems.Count Then
                Return Index
            Else
                Return ClickedIndex
            End If
        Else
            Return ClickedIndex
        End If
    End Function

    Public Sub DrawList(ItemList As List(Of DragDropItem))
        ListImage = New Bitmap(ImageWidth, ImageHeight)
        ListImage.SetResolution(300, 300)
        Using Gr As Graphics = Graphics.FromImage(ListImage)
            Gr.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            Gr.FillRectangle(Brushes.White, 0, 0, ImageWidth, ImageHeight)
            Gr.TextRenderingHint = 3
            Dim ItemFont = New Font(New FontFamily("Times New Roman"), 10, FontStyle.Regular, GraphicsUnit.Pixel)
            Dim FormatFlags As StringFormat = New StringFormat With {.LineAlignment = StringAlignment.Center, .Alignment = StringAlignment.Center}
            For Each Item As DragDropItem In ItemList
                Dim BaseX As Integer = Math.Floor(ItemList.IndexOf(Item) Mod ThumbsPerRow)
                Dim BaseY As Integer = Math.Floor(ItemList.IndexOf(Item) / ThumbsPerRow)
                Dim RealX As Integer = 10 + ((ThumbSize + 10) * BaseX)
                Dim RealY As Integer = 10 + ((ThumbSize + 10) * BaseY)
                Gr.DrawImage(Item.Thumbnail, RealX, RealY, ThumbSize, ThumbSize)
                Gr.DrawString(Item.Name, ItemFont, Brushes.Black, New Point(RealX + Math.Floor(ThumbSize / 2), RealY + ThumbSize + 10), FormatFlags)
            Next
        End Using
        ListCanvas.BackgroundImage = ListImage
        ListCanvas.Refresh()
    End Sub

End Class
