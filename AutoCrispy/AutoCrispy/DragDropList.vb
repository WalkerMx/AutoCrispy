
Public Class DragDropList

    ' Raised once a drag has been committed, while ListItems still carries the pre-drag Index
    ' values.  The owner needs that mapping to reorder its own data.
    '
    ' Subscribing to the canvas' MouseUp event instead is not safe: WinForms delivers the owner's
    ' Handles-based handler BEFORE this class commits the new order, so the owner rebuilt its data
    ' from stale indices and silently overwrote models.
    Public Event ListReordered As EventHandler

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
    Private IsDragging As Boolean

    ' Item the user clicked last; the Delete button acts on this.  -1 when nothing is selected.
    Public Property SelectedIndex As Integer

    Private WithEvents DragTimer As New Timer With {.Interval = 50, .Enabled = False}

    Public Sub New(ByRef _PictureBox As PictureBox, _ThumbsPerRow As Integer)
        ListCanvas = _PictureBox
        ImageWidth = _PictureBox.Width
        ImageHeight = _PictureBox.Height
        ThumbsPerRow = _ThumbsPerRow
        ThumbSize = Math.Floor((ImageWidth - (10 + (10 * ThumbsPerRow))) / ThumbsPerRow)
        SelectedIndex = -1
        ClickedIndex = -1
        CurrentIndex = -1
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
        ' Keeps the drag alive while the pointer is stationary or has left the canvas.
        UpdateDrag(ListCanvas.PointToClient(Control.MousePosition))
    End Sub

    Public Sub ListCanvas_MouseDown(sender As Object, e As MouseEventArgs) Handles ListCanvas.MouseDown
        ' e.Location is already canvas-relative, so there is no need to re-read the global cursor.
        Dim Index As Integer = GetIndexAt(e.Location)
        SelectedIndex = Index
        If e.Button = MouseButtons.Left Then
            If Index >= 0 Then
                ClickedIndex = Index
                CurrentIndex = Index
                TempListItems.Clear()
                TempListItems.AddRange(ListItems)
                IsDragging = True
                ListCanvas.Cursor = Cursors.SizeAll
                DragTimer.Enabled = True
            End If
            DrawList(ListItems)
        End If
    End Sub

    Public Sub ListCanvas_MouseMove(sender As Object, e As MouseEventArgs) Handles ListCanvas.MouseMove
        ' Reacting to the pointer directly instead of only to the 100 ms timer is what makes the
        ' drag follow the mouse.
        If IsDragging Then UpdateDrag(e.Location)
    End Sub

    Public Sub ListCanvas_MouseUp(sender As Object, e As MouseEventArgs) Handles ListCanvas.MouseUp
        If e.Button <> MouseButtons.Left Then Return
        DragTimer.Enabled = False
        ListCanvas.Cursor = Cursors.Default
        If Not IsDragging Then Return
        IsDragging = False
        ' TempListItems only holds a full copy once a drag has actually started.  Committing it
        ' unconditionally, as before, emptied the whole chain on a stray MouseUp.
        If TempListItems.Count <> ListItems.Count Then
            TempListItems.Clear()
            DrawList(ListItems)
            Return
        End If
        ListItems.Clear()
        ListItems.AddRange(TempListItems)
        TempListItems.Clear()
        If ClickedIndex >= 0 Then SelectedIndex = CurrentIndex
        RaiseEvent ListReordered(Me, EventArgs.Empty)
        ReorderList()
        DrawList(ListItems)
    End Sub

    Private Sub UpdateDrag(Position As Point)
        If Not IsDragging Then Return
        If ClickedIndex < 0 OrElse ClickedIndex >= ListItems.Count Then Return

        Dim NewIndex As Integer = GetIndexAt(Position)
        If NewIndex < 0 Then
            ' Outside the canvas there is nothing to reorder onto; keep the last good slot.
            If Position.X < 0 OrElse Position.Y < 0 Then Return
            If Position.X >= ImageWidth OrElse Position.Y >= ImageHeight Then Return
            NewIndex = ListItems.Count - 1
        End If
        If NewIndex = CurrentIndex Then Return

        If CurrentIndex >= 0 AndAlso CurrentIndex < TempListItems.Count Then
            TempListItems.RemoveAt(CurrentIndex)
        End If
        If NewIndex > TempListItems.Count Then NewIndex = TempListItems.Count
        If NewIndex < 0 Then NewIndex = 0
        TempListItems.Insert(NewIndex, ListItems(ClickedIndex))
        CurrentIndex = NewIndex
        DrawList(TempListItems)
    End Sub

    ' Renumbers the items so that ListItems(i).Index = i.  Call this only once the owner has
    ' finished reading the old indices, otherwise the mapping is destroyed.
    Public Sub ReorderList()
        For i = 0 To ListItems.Count - 1
            ListItems(i) = New DragDropList.DragDropItem(i, ListItems(i).Name, ListItems(i).Thumbnail)
        Next
        If SelectedIndex >= ListItems.Count Then SelectedIndex = ListItems.Count - 1
    End Sub

    ' Index of the item under a canvas-relative point, or -1 when the point is outside the canvas,
    ' sits in one of the 10 px gutters, or is past the last item.
    '
    ' The old version ignored the 10 px left margin, so a click near a thumbnail's right edge
    ' selected the next column, and it fell back to the previously clicked index instead of
    ' reporting "nothing here".  That fallback is why the context menu's Edit/Delete kept acting
    ' on the wrong model.
    Public Function GetIndexAt(Position As Point) As Integer
        If ListItems.Count = 0 Then Return -1
        If Position.X < 0 OrElse Position.Y < 0 Then Return -1
        If Position.X >= ImageWidth OrElse Position.Y >= ImageHeight Then Return -1

        Dim Cell As Integer = ThumbSize + 10
        If Cell <= 0 Then Return -1
        Dim XPos As Integer = (Position.X - 10) \ Cell
        Dim YPos As Integer = (Position.Y - 10) \ Cell
        If XPos < 0 OrElse XPos >= ThumbsPerRow Then Return -1
        ' The caption is drawn in the gutter below its thumbnail, so only the horizontal gutters
        ' count as a miss.
        If (Position.X - 10) Mod Cell >= ThumbSize Then Return -1

        Dim Index As Integer = (YPos * ThumbsPerRow) + XPos
        If Index < 0 OrElse Index >= ListItems.Count Then Return -1
        Return Index
    End Function

    Public Function GetCurrentIndex() As Integer
        Return GetIndexAt(ListCanvas.PointToClient(Control.MousePosition))
    End Function

    Public Sub DrawList(ItemList As List(Of DragDropItem))
        Dim NewImage As New Bitmap(ImageWidth, ImageHeight)
        NewImage.SetResolution(300, 300)
        Using Gr As Graphics = Graphics.FromImage(NewImage)
            Gr.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            Gr.FillRectangle(Brushes.White, 0, 0, ImageWidth, ImageHeight)
            Gr.TextRenderingHint = 3
            Using ItemFont As New Font(New FontFamily("Times New Roman"), 10, FontStyle.Regular, GraphicsUnit.Pixel)
                Using FormatFlags As StringFormat = New StringFormat With {.LineAlignment = StringAlignment.Center, .Alignment = StringAlignment.Center}
                    For i = 0 To ItemList.Count - 1
                        Dim Item As DragDropItem = ItemList(i)
                        ' Position from the loop index.  The old code used ItemList.IndexOf(Item),
                        ' which is O(n) and, worse, returns the FIRST value-equal entry: two
                        ' identical chain steps were drawn on top of each other and everything
                        ' after them shifted.
                        Dim BaseX As Integer = i Mod ThumbsPerRow
                        Dim BaseY As Integer = i \ ThumbsPerRow
                        Dim RealX As Integer = 10 + ((ThumbSize + 10) * BaseX)
                        Dim RealY As Integer = 10 + ((ThumbSize + 10) * BaseY)
                        If RealY >= ImageHeight Then Exit For
                        If Item.Thumbnail IsNot Nothing Then
                            Gr.DrawImage(Item.Thumbnail, RealX, RealY, ThumbSize, ThumbSize)
                        End If
                        If (Not IsDragging) AndAlso i = SelectedIndex Then
                            Using SelectionPen As New Pen(Color.DodgerBlue, 2)
                                Gr.DrawRectangle(SelectionPen, RealX + 1, RealY + 1, ThumbSize - 2, ThumbSize - 2)
                            End Using
                        End If
                        Gr.DrawString(FitCaption(Item.Name, Gr, ItemFont, ThumbSize + 8), ItemFont, Brushes.Black, New Point(RealX + (ThumbSize \ 2), RealY + ThumbSize + 10), FormatFlags)
                    Next
                End Using
            End Using
        End Using
        ' Setting BackgroundImage does not dispose the previous bitmap, so every redraw leaked a
        ' GDI+ surface; a drag now redraws on every mouse move.
        Dim OldImage As Bitmap = ListImage
        ListImage = NewImage
        ListCanvas.BackgroundImage = ListImage
        ListCanvas.Refresh()
        If OldImage IsNot Nothing Then OldImage.Dispose()
    End Sub

    ' Thumbnails are only ~104 px wide, so a real model name such as "4x_NMKD-Superscale_SPAN_3"
    ' has to be clipped rather than allowed to run into the neighbouring item.
    Private Function FitCaption(Caption As String, Gr As Graphics, ItemFont As Font, MaxWidth As Integer) As String
        If String.IsNullOrEmpty(Caption) Then Return ""
        If Gr.MeasureString(Caption, ItemFont).Width <= MaxWidth Then Return Caption
        Dim Low As Integer = 1
        Dim High As Integer = Caption.Length - 1
        Do While Low < High
            Dim Probe As Integer = (Low + High + 1) \ 2
            If Gr.MeasureString(Caption.Substring(0, Probe) & "...", ItemFont).Width <= MaxWidth Then
                Low = Probe
            Else
                High = Probe - 1
            End If
        Loop
        Return Caption.Substring(0, Low).TrimEnd() & "..."
    End Function

End Class
