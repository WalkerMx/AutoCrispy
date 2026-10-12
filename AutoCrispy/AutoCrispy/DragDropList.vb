
Public Class DragDropList

    Private ListImage As Bitmap
    Private WithEvents ListCanvas As PictureBox
    Public ListItems As New List(Of DragDropItem)
    Public TempListItems As New List(Of DragDropItem)

    Private ImageWidth As Integer
    Private ImageHeight As Integer

    Private ThumbsPerRow As Integer
    Private ColumnsPerRow As Integer
    Private ThumbSize As Integer
    Private Const ItemPadding As Integer = 10
    Private Const CaptionHeight As Integer = 32

    Private ClickedIndex As Integer
    Private CurrentIndex As Integer
    Private IsDragging As Boolean

    Private WithEvents DragTimer As New Timer With {.Interval = 100, .Enabled = False}

    Public Sub New(ByRef _PictureBox As PictureBox, _ThumbsPerRow As Integer)
        ListCanvas = _PictureBox
        ImageWidth = _PictureBox.Width
        ImageHeight = _PictureBox.Height
        ThumbsPerRow = Math.Max(1, _ThumbsPerRow)
        ColumnsPerRow = ThumbsPerRow
        ThumbSize = Math.Max(1, Math.Floor((ImageWidth - (ItemPadding + (ItemPadding * ThumbsPerRow))) / ThumbsPerRow))
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
        If Not IsDragging Then Return
        Dim NewIndex As Integer = GetCurrentIndex()
        If CurrentIndex <> NewIndex Then
            TempListItems.RemoveAt(CurrentIndex)
            TempListItems.Insert(NewIndex, ListItems(ClickedIndex))
            CurrentIndex = NewIndex
            DrawList(TempListItems)
        End If
    End Sub

    Public Sub ListCanvas_MouseDown(sender As Object, e As MouseEventArgs) Handles ListCanvas.MouseDown
        Dim HitIndex As Integer = HitTestIndex(e.Location)
        If HitIndex >= 0 Then
            ClickedIndex = HitIndex
            CurrentIndex = HitIndex
        End If
        If e.Button = MouseButtons.Left AndAlso HitIndex >= 0 Then
            TempListItems.Clear()
            TempListItems.AddRange(ListItems)
            Form1.Cursor = Cursors.SizeAll
            IsDragging = True
            DragTimer.Enabled = True
        End If
    End Sub

    Public Sub ListCanvas_MouseUp(sender As Object, e As MouseEventArgs) Handles ListCanvas.MouseUp
        If e.Button = MouseButtons.Left AndAlso IsDragging Then
            DragTimer.Enabled = False
            IsDragging = False
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

    Public Function GetCurrentIndex() As Integer
        Dim HitIndex As Integer = HitTestIndex(ListCanvas.PointToClient(Control.MousePosition))
        If HitIndex >= 0 Then Return HitIndex
        If IsDragging Then Return CurrentIndex
        Return ClickedIndex
    End Function

    Private Function HitTestIndex(LocalPosition As Point) As Integer
        If ListItems.Count = 0 OrElse ThumbSize <= 0 OrElse ColumnsPerRow <= 0 Then Return -1
        If LocalPosition.X < 0 OrElse LocalPosition.X >= ImageWidth OrElse LocalPosition.Y < 0 OrElse LocalPosition.Y >= ImageHeight Then Return -1

        Dim ContentY As Integer = LocalPosition.Y - ItemPadding
        If ContentY < 0 Then Return -1
        Dim CellHeight As Integer = ThumbSize + CaptionHeight + ItemPadding
        Dim RowIndex As Integer = ContentY \ CellHeight
        Dim FirstItemIndex As Integer = RowIndex * ColumnsPerRow
        If FirstItemIndex >= ListItems.Count Then Return -1

        Dim ItemsInRow As Integer = Math.Min(ColumnsPerRow, ListItems.Count - FirstItemIndex)
        Dim RowWidth As Integer = (ItemsInRow * ThumbSize) + ((ItemsInRow - 1) * ItemPadding)
        Dim RowStartX As Integer = CInt(Math.Floor((ImageWidth - RowWidth) / 2.0))
        Dim RelativeX As Integer = LocalPosition.X - RowStartX
        If RelativeX < 0 OrElse RelativeX >= RowWidth Then Return -1

        Dim ColumnIndex As Integer = CInt(Math.Floor(RelativeX / CDbl(ThumbSize + ItemPadding)))
        If ColumnIndex >= ItemsInRow Then Return -1
        Dim CellX As Integer = RelativeX Mod (ThumbSize + ItemPadding)
        If CellX >= ThumbSize Then Return -1

        Dim CellY As Integer = ContentY Mod CellHeight
        If CellY >= ThumbSize AndAlso CellY < ThumbSize + 2 Then Return -1
        If CellY >= ThumbSize + 2 + CaptionHeight Then Return -1
        Return FirstItemIndex + ColumnIndex
    End Function

    Public Sub DrawList(ItemList As List(Of DragDropItem))
        Dim PreviousListImage As Bitmap = ListImage
        ColumnsPerRow = Math.Max(1, Math.Min(ThumbsPerRow, Math.Max(1, ItemList.Count)))
        Dim RowCount As Integer = Math.Max(1, CInt(Math.Ceiling(ItemList.Count / CDbl(ColumnsPerRow))))
        Dim MaxThumbByWidth As Integer = CInt(Math.Floor((ImageWidth - ItemPadding - (ItemPadding * ColumnsPerRow)) / CDbl(ColumnsPerRow)))
        Dim MaxThumbByHeight As Integer = CInt(Math.Floor((ImageHeight - ItemPadding - (RowCount * (CaptionHeight + ItemPadding))) / CDbl(RowCount)))
        ThumbSize = Math.Max(1, Math.Min(MaxThumbByWidth, MaxThumbByHeight))

        ListImage = New Bitmap(ImageWidth, ImageHeight)
        Using Gr As Graphics = Graphics.FromImage(ListImage)
            Gr.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            Gr.TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAliasGridFit
            Gr.FillRectangle(Brushes.White, 0, 0, ImageWidth, ImageHeight)
            Using ItemFont As New Font("Segoe UI", 12.0!, FontStyle.Regular, GraphicsUnit.Pixel)
                Using CaptionFormat As New StringFormat With {
                    .LineAlignment = StringAlignment.Center,
                    .Alignment = StringAlignment.Center,
                    .Trimming = StringTrimming.EllipsisWord,
                    .FormatFlags = StringFormatFlags.LineLimit
                }
                    For ItemIndex As Integer = 0 To ItemList.Count - 1
                        Dim BaseX As Integer = ItemIndex Mod ColumnsPerRow
                        Dim BaseY As Integer = ItemIndex \ ColumnsPerRow
                        Dim ItemsInRow As Integer = Math.Min(ColumnsPerRow, ItemList.Count - (BaseY * ColumnsPerRow))
                        Dim RowWidth As Integer = (ItemsInRow * ThumbSize) + ((ItemsInRow - 1) * ItemPadding)
                        Dim RowStartX As Integer = CInt(Math.Floor((ImageWidth - RowWidth) / 2.0))
                        Dim RealX As Integer = RowStartX + ((ThumbSize + ItemPadding) * BaseX)
                        Dim RealY As Integer = ItemPadding + ((ThumbSize + CaptionHeight + ItemPadding) * BaseY)
                        Gr.DrawImage(ItemList(ItemIndex).Thumbnail, RealX, RealY, ThumbSize, ThumbSize)
                        Dim CaptionBounds As New RectangleF(RealX, RealY + ThumbSize + 2, ThumbSize, CaptionHeight)
                        Gr.DrawString(ItemList(ItemIndex).Name, ItemFont, Brushes.Black, CaptionBounds, CaptionFormat)
                    Next
                End Using
            End Using
        End Using

        ListCanvas.BackgroundImage = ListImage
        ListCanvas.Refresh()
        If PreviousListImage IsNot Nothing Then PreviousListImage.Dispose()
    End Sub

End Class
