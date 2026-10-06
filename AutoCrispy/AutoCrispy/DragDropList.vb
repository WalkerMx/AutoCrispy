
Public Class DragDropList

    Private ListImage As Bitmap
    Private WithEvents ListCanvas As PictureBox
    Public ListItems As New List(Of DragDropItem)
    Public TempListItems As New List(Of DragDropItem)

    Private ImageWidth As Integer
    Private ImageHeight As Integer
    Private ViewportWidth As Integer
    Private ViewportHeight As Integer

    Private ThumbsPerRow As Integer
    Private ColumnsPerRow As Integer
    Private ThumbSize As Integer
    Private Const ItemPadding As Integer = 10
    Private Const CaptionHeight As Integer = 32
    Private Const MinThumbSize As Integer = 64

    Private ClickedIndex As Integer = -1
    Private CurrentIndex As Integer = -1
    Private IsDragging As Boolean
    Public Property SelectedIndex As Integer = -1

    Public Event ItemsReordered(ByRef Accepted As Boolean)
    Public Event SelectionChanged()
    Public Event DeleteRequested As EventHandler
    Public Event EditRequested As EventHandler

    Private WithEvents DragTimer As New Timer With {.Interval = 50, .Enabled = False}

    Public Sub New(ByRef _PictureBox As PictureBox, _ThumbsPerRow As Integer)
        ListCanvas = _PictureBox
        ListCanvas.TabStop = True
        ListCanvas.BackgroundImageLayout = ImageLayout.None
        ViewportWidth = Math.Max(1, _PictureBox.Width)
        ViewportHeight = Math.Max(1, _PictureBox.Height)
        ImageWidth = ViewportWidth
        ImageHeight = ViewportHeight
        ThumbsPerRow = Math.Max(1, _ThumbsPerRow)
        ColumnsPerRow = ThumbsPerRow
        ThumbSize = MinThumbSize
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

    Public Function GetCurrentIndex() As Integer
        If IsDragging Then Return CurrentIndex
        Return SelectedIndex
    End Function

    Private Sub DragTimer_Tick(sender As Object, e As EventArgs) Handles DragTimer.Tick
        If Not IsDragging Then Return
        AutoScrollWhileDragging()
        Dim NewIndex As Integer = GetCurrentIndexFromMouse()
        If NewIndex < 0 Then Return
        If CurrentIndex <> NewIndex Then
            TempListItems.RemoveAt(CurrentIndex)
            TempListItems.Insert(NewIndex, ListItems(ClickedIndex))
            CurrentIndex = NewIndex
            SelectedIndex = NewIndex
            DrawList(TempListItems)
        End If
    End Sub

    Public Sub ListCanvas_MouseDown(sender As Object, e As MouseEventArgs) Handles ListCanvas.MouseDown
        ListCanvas.Focus()
        Dim HitIndex As Integer = HitTestIndex(e.Location)
        If HitIndex >= 0 Then
            ClickedIndex = HitIndex
            CurrentIndex = HitIndex
            ChangeSelection(HitIndex)
        Else
            ClickedIndex = -1
            CurrentIndex = -1
            ChangeSelection(-1)
        End If
        DrawList(ListItems)
        If e.Button = MouseButtons.Left AndAlso HitIndex >= 0 Then
            TempListItems.Clear()
            TempListItems.AddRange(ListItems)
            IsDragging = True
            ListCanvas.Capture = True
            ListCanvas.Cursor = Cursors.SizeAll
            DragTimer.Enabled = True
        End If
    End Sub

    Public Sub ListCanvas_MouseUp(sender As Object, e As MouseEventArgs) Handles ListCanvas.MouseUp
        If e.Button = MouseButtons.Left Then CommitDrag()
    End Sub

    Private Sub ListCanvas_MouseCaptureChanged(sender As Object, e As EventArgs) Handles ListCanvas.MouseCaptureChanged
        If IsDragging AndAlso Not ListCanvas.Capture Then CommitDrag()
    End Sub

    Private Sub ListCanvas_PreviewKeyDown(sender As Object, e As PreviewKeyDownEventArgs) Handles ListCanvas.PreviewKeyDown
        Select Case e.KeyCode
            Case Keys.Left, Keys.Right, Keys.Up, Keys.Down, Keys.Delete, Keys.Enter
                e.IsInputKey = True
        End Select
    End Sub

    Private Sub ListCanvas_KeyDown(sender As Object, e As KeyEventArgs) Handles ListCanvas.KeyDown
        If IsDragging Then Return
        If e.KeyCode = Keys.Delete Then
            RaiseEvent DeleteRequested(Me, EventArgs.Empty)
            e.Handled = True
            Return
        End If
        If e.KeyCode = Keys.Enter Then
            RaiseEvent EditRequested(Me, EventArgs.Empty)
            e.Handled = True
            Return
        End If
        If ListItems.Count = 0 Then Return
        Dim Delta As Integer = 0
        Select Case e.KeyCode
            Case Keys.Left
                Delta = -1
            Case Keys.Right
                Delta = 1
            Case Keys.Up
                Delta = -Math.Max(1, ColumnsPerRow)
            Case Keys.Down
                Delta = Math.Max(1, ColumnsPerRow)
            Case Else
                Return
        End Select
        Dim NextIndex As Integer = If(SelectedIndex < 0, 0, SelectedIndex + Delta)
        NextIndex = Math.Max(0, Math.Min(ListItems.Count - 1, NextIndex))
        ChangeSelection(NextIndex)
        DrawList(ListItems)
        ScrollSelectionIntoView()
        e.Handled = True
    End Sub

    Private Sub CommitDrag()
        If Not IsDragging Then Return
        DragTimer.Enabled = False
        IsDragging = False
        If ListCanvas.Capture Then ListCanvas.Capture = False
        ListCanvas.Cursor = Cursors.Default
        Dim HostForm As Form = ListCanvas.FindForm()
        If HostForm IsNot Nothing Then HostForm.Cursor = Cursors.Default

        Dim Backup As New List(Of DragDropItem)(ListItems)
        ListItems.Clear()
        ListItems.AddRange(TempListItems)
        Dim Accepted As Boolean = False
        RaiseEvent ItemsReordered(Accepted)
        If Not Accepted Then
            ListItems.Clear()
            ListItems.AddRange(Backup)
        End If
        If ListItems.Count = 0 Then
            ChangeSelection(-1)
        ElseIf SelectedIndex >= ListItems.Count Then
            ChangeSelection(ListItems.Count - 1)
        End If
        ReorderList()
        DrawList(ListItems)
    End Sub

    Private Sub ChangeSelection(Index As Integer)
        If SelectedIndex = Index Then Return
        SelectedIndex = Index
        RaiseEvent SelectionChanged()
    End Sub

    Public Sub ReorderList()
        For i = 0 To ListItems.Count - 1
            ListItems(i) = New DragDropItem(i, ListItems(i).Name, ListItems(i).Thumbnail)
        Next
    End Sub

    Private Function GetCurrentIndexFromMouse() As Integer
        Dim HitIndex As Integer = HitTestIndex(ListCanvas.PointToClient(Control.MousePosition))
        If HitIndex >= 0 Then Return HitIndex
        Return CurrentIndex
    End Function

    Private Function HitTestIndex(LocalPosition As Point) As Integer
        Dim ItemCount As Integer = If(IsDragging, TempListItems.Count, ListItems.Count)
        If ItemCount = 0 OrElse ThumbSize <= 0 OrElse ColumnsPerRow <= 0 Then Return -1
        If LocalPosition.X < 0 OrElse LocalPosition.X >= ImageWidth OrElse LocalPosition.Y < 0 OrElse LocalPosition.Y >= ImageHeight Then Return -1

        For ItemIndex As Integer = 0 To ItemCount - 1
            Dim ItemBounds As Rectangle = GetItemBounds(ItemIndex, ItemCount)
            Dim HitBounds As New Rectangle(ItemBounds.X, ItemBounds.Y, ItemBounds.Width, ThumbSize + 2 + CaptionHeight)
            If HitBounds.Contains(LocalPosition) Then Return ItemIndex
        Next
        Return -1
    End Function

    Private Function GetItemBounds(ItemIndex As Integer, ItemCount As Integer) As Rectangle
        Dim BaseX As Integer = ItemIndex Mod ColumnsPerRow
        Dim BaseY As Integer = ItemIndex \ ColumnsPerRow
        Dim ItemsInRow As Integer = Math.Min(ColumnsPerRow, ItemCount - (BaseY * ColumnsPerRow))
        Dim RowWidth As Integer = (ItemsInRow * ThumbSize) + ((ItemsInRow - 1) * ItemPadding)
        Dim RowStartX As Integer = CInt(Math.Floor((ImageWidth - RowWidth) / 2.0))
        Dim RealX As Integer = RowStartX + ((ThumbSize + ItemPadding) * BaseX)
        Dim RealY As Integer = ItemPadding + ((ThumbSize + CaptionHeight + ItemPadding) * BaseY)
        Return New Rectangle(RealX, RealY, ThumbSize, ThumbSize)
    End Function

    Private Sub AutoScrollWhileDragging()
        Dim Host As ScrollableControl = TryCast(ListCanvas.Parent, ScrollableControl)
        If Host Is Nothing OrElse Not Host.AutoScroll Then Return
        Dim HostPosition As Point = Host.PointToClient(Control.MousePosition)
        Dim Delta As Integer = 0
        If HostPosition.Y < 18 Then Delta = -16
        If HostPosition.Y > Host.ClientSize.Height - 18 Then Delta = 16
        If Delta = 0 Then Return
        Host.AutoScrollPosition = New Point(0, Math.Max(0, -Host.AutoScrollPosition.Y + Delta))
    End Sub

    Private Sub ScrollSelectionIntoView()
        Dim Host As ScrollableControl = TryCast(ListCanvas.Parent, ScrollableControl)
        If Host Is Nothing OrElse Not Host.AutoScroll OrElse SelectedIndex < 0 OrElse SelectedIndex >= ListItems.Count Then Return
        Dim ItemBounds As Rectangle = GetItemBounds(SelectedIndex, ListItems.Count)
        Dim ViewTop As Integer = -Host.AutoScrollPosition.Y
        Dim ViewBottom As Integer = ViewTop + Host.ClientSize.Height
        If ItemBounds.Top >= ViewTop AndAlso ItemBounds.Bottom + CaptionHeight <= ViewBottom Then Return
        Dim TargetY As Integer = If(ItemBounds.Top < ViewTop, ItemBounds.Top - ItemPadding, ItemBounds.Bottom + CaptionHeight - Host.ClientSize.Height + ItemPadding)
        Host.AutoScrollPosition = New Point(0, Math.Max(0, TargetY))
    End Sub

    Public Sub DrawList(ItemList As List(Of DragDropItem))
        Dim PreviousListImage As Bitmap = ListImage
        Dim CanvasWidth As Integer = ViewportWidth
        Dim CanvasHeight As Integer = ViewportHeight
        ThumbSize = MeasureThumb(Math.Max(0, ItemList.Count), CanvasWidth, CanvasHeight)
        ImageWidth = Math.Max(1, CanvasWidth)
        ImageHeight = Math.Max(1, CanvasHeight)
        ColumnsPerRow = Math.Max(1, Math.Min(ThumbsPerRow, Math.Max(1, ItemList.Count)))

        ListImage = New Bitmap(ImageWidth, ImageHeight)
        Using Gr As Graphics = Graphics.FromImage(ListImage)
            Gr.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            Gr.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit
            Gr.Clear(SystemColors.Window)
            If ItemList.Count = 0 Then
                DrawEmptyState(Gr)
            Else
                Using ItemFont As New Font(SystemFonts.MessageBoxFont.FontFamily, 12.0!, FontStyle.Regular, GraphicsUnit.Pixel)
                    Using CaptionFormat As New StringFormat With {
                        .LineAlignment = StringAlignment.Center,
                        .Alignment = StringAlignment.Center,
                        .Trimming = StringTrimming.EllipsisCharacter,
                        .FormatFlags = StringFormatFlags.LineLimit
                    }
                        Using TextBrush As New SolidBrush(SystemColors.WindowText)
                            Using SelectedPen As New Pen(SystemColors.Highlight, 2.0!)
                                For ItemIndex As Integer = 0 To ItemList.Count - 1
                                    Dim ItemBounds As Rectangle = GetItemBounds(ItemIndex, ItemList.Count)
                                    If ItemList(ItemIndex).Thumbnail IsNot Nothing Then
                                        Gr.DrawImage(ItemList(ItemIndex).Thumbnail, ItemBounds)
                                    Else
                                        Gr.FillRectangle(SystemBrushes.Control, ItemBounds)
                                    End If
                                    If ItemIndex = SelectedIndex Then
                                        Gr.DrawRectangle(SelectedPen, ItemBounds.X - 1, ItemBounds.Y - 1, ItemBounds.Width + 2, ItemBounds.Height + 2)
                                    End If
                                    Dim CaptionBounds As New RectangleF(ItemBounds.X, ItemBounds.Bottom + 2, ItemBounds.Width, CaptionHeight)
                                    Gr.DrawString(ItemList(ItemIndex).Name, ItemFont, TextBrush, CaptionBounds, CaptionFormat)
                                Next
                            End Using
                        End Using
                    End Using
                End Using
            End If
        End Using

        ListCanvas.Size = New Size(ImageWidth, ImageHeight)
        ListCanvas.BackgroundImage = ListImage
        ListCanvas.Refresh()
        If PreviousListImage IsNot Nothing Then PreviousListImage.Dispose()
    End Sub

    Private Sub DrawEmptyState(Gr As Graphics)
        Using HintFont As New Font(SystemFonts.MessageBoxFont.FontFamily, 13.0!, FontStyle.Regular, GraphicsUnit.Pixel)
            Using HintBrush As New SolidBrush(SystemColors.GrayText)
                Using HintFormat As New StringFormat With {
                    .Alignment = StringAlignment.Center,
                    .LineAlignment = StringAlignment.Center,
                    .Trimming = StringTrimming.EllipsisWord
                }
                    Dim Hint As String = "Add a backend to build a chain." & Environment.NewLine & "An empty chain uses the settings currently shown."
                    Gr.DrawString(Hint, HintFont, HintBrush, New RectangleF(12, 12, Math.Max(1, ImageWidth - 24), Math.Max(1, ImageHeight - 24)), HintFormat)
                End Using
            End Using
        End Using
    End Sub

    Private Function MeasureThumb(ItemCount As Integer, ByRef CanvasWidth As Integer, ByRef CanvasHeight As Integer) As Integer
        Dim Columns As Integer = Math.Max(1, Math.Min(ThumbsPerRow, Math.Max(1, ItemCount)))
        Dim Rows As Integer = 1
        If ItemCount > 0 Then Rows = Math.Max(1, CInt(Math.Ceiling(ItemCount / CDbl(Columns))))
        Dim WidthForThumbs As Integer = ViewportWidth
        Dim ThumbByWidth As Integer = CInt(Math.Floor((WidthForThumbs - ItemPadding - (ItemPadding * Columns)) / CDbl(Columns)))
        Dim ThumbByHeight As Integer = CInt(Math.Floor((ViewportHeight - ItemPadding - (Rows * (CaptionHeight + ItemPadding))) / CDbl(Rows)))
        Dim Fitted As Integer = Math.Max(1, Math.Min(Math.Max(1, ThumbByWidth), Math.Max(1, ThumbByHeight)))
        Dim Thumb As Integer = Fitted
        Dim CanvasH As Integer = ViewportHeight
        If ItemCount > 0 AndAlso Fitted < MinThumbSize Then
            WidthForThumbs = Math.Max(1, ViewportWidth - SystemInformation.VerticalScrollBarWidth)
            ThumbByWidth = CInt(Math.Floor((WidthForThumbs - ItemPadding - (ItemPadding * Columns)) / CDbl(Columns)))
            Thumb = Math.Max(1, Math.Min(Math.Max(1, ThumbByWidth), MinThumbSize))
            CanvasH = ItemPadding + (Rows * (Thumb + CaptionHeight + ItemPadding))
            If CanvasH < ViewportHeight Then CanvasH = ViewportHeight
        End If
        CanvasWidth = If(CanvasH > ViewportHeight, WidthForThumbs, ViewportWidth)
        CanvasHeight = CanvasH
        Return Thumb
    End Function

End Class
