
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
    Private Const CaptionHeight As Integer = 40

    Private ClickedIndex As Integer = -1
    Private CurrentIndex As Integer = -1
    Private IsDragging As Boolean
    Private DidReorder As Boolean

    Public Event SelectionChanged As EventHandler
    Public Event ItemsReordered As EventHandler

    Public ReadOnly Property SelectedIndex As Integer
        Get
            Return ClickedIndex
        End Get
    End Property

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

    Private Sub ListCanvas_Resize(sender As Object, e As EventArgs) Handles ListCanvas.Resize
        If ListCanvas Is Nothing OrElse ListCanvas.ClientSize.Width <= 0 OrElse ListCanvas.ClientSize.Height <= 0 Then Return
        ImageWidth = ListCanvas.ClientSize.Width
        ImageHeight = ListCanvas.ClientSize.Height
        DrawList(ListItems)
    End Sub

    Private Sub DragTimer_Tick(sender As Object, e As EventArgs) Handles DragTimer.Tick
        If Not IsDragging Then Return
        Dim NewIndex As Integer = GetCurrentIndex()
        If NewIndex < 0 OrElse NewIndex >= TempListItems.Count OrElse CurrentIndex = NewIndex Then Return
        TempListItems.RemoveAt(CurrentIndex)
        TempListItems.Insert(NewIndex, ListItems(ClickedIndex))
        CurrentIndex = NewIndex
        DidReorder = True
        DrawList(TempListItems)
    End Sub

    Public Sub ListCanvas_MouseDown(sender As Object, e As MouseEventArgs) Handles ListCanvas.MouseDown
        Dim HitIndex As Integer = HitTestIndex(e.Location)
        If HitIndex >= 0 Then
            ClickedIndex = HitIndex
            CurrentIndex = HitIndex
        ElseIf e.Button = MouseButtons.Left OrElse e.Button = MouseButtons.Right Then
            ClickedIndex = -1
            CurrentIndex = -1
        End If

        If e.Button = MouseButtons.Left AndAlso HitIndex >= 0 Then
            TempListItems.Clear()
            TempListItems.AddRange(ListItems)
            Form1.Cursor = Cursors.SizeAll
            IsDragging = True
            DidReorder = False
            DragTimer.Enabled = True
        End If
        DrawList(ListItems)
        RaiseEvent SelectionChanged(Me, EventArgs.Empty)
    End Sub

    Public Sub ListCanvas_MouseUp(sender As Object, e As MouseEventArgs) Handles ListCanvas.MouseUp
        If e.Button = MouseButtons.Left AndAlso IsDragging Then
            Dim ReleaseIndex As Integer = HitTestIndex(e.Location)
            If ReleaseIndex >= 0 AndAlso ReleaseIndex < TempListItems.Count AndAlso ReleaseIndex <> CurrentIndex Then
                TempListItems.RemoveAt(CurrentIndex)
                TempListItems.Insert(ReleaseIndex, ListItems(ClickedIndex))
                CurrentIndex = ReleaseIndex
                DidReorder = True
            End If

            DragTimer.Enabled = False
            IsDragging = False
            Form1.Cursor = Cursors.Default
            ListItems.Clear()
            ListItems.AddRange(TempListItems)
            ClickedIndex = CurrentIndex
            If DidReorder Then RaiseEvent ItemsReordered(Me, EventArgs.Empty)
            ReorderList()
            DidReorder = False
        End If
        DrawList(ListItems)
        RaiseEvent SelectionChanged(Me, EventArgs.Empty)
    End Sub

    Public Sub ClearSelection()
        ClickedIndex = -1
        CurrentIndex = -1
        IsDragging = False
        DidReorder = False
        DrawList(ListItems)
        RaiseEvent SelectionChanged(Me, EventArgs.Empty)
    End Sub

    Public Sub SelectIndex(Index As Integer)
        If Index < 0 OrElse Index >= ListItems.Count Then
            ClearSelection()
            Return
        End If
        ClickedIndex = Index
        CurrentIndex = Index
        IsDragging = False
        DidReorder = False
        DrawList(ListItems)
        RaiseEvent SelectionChanged(Me, EventArgs.Empty)
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

    Public Function GetItemNameAt(LocalPosition As Point) As String
        Dim HitIndex As Integer = HitTestIndex(LocalPosition)
        If HitIndex < 0 OrElse HitIndex >= ListItems.Count Then Return String.Empty
        If IsDragging AndAlso HitIndex < TempListItems.Count Then Return TempListItems(HitIndex).Name
        Return ListItems(HitIndex).Name
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

    Private Function GetCaptionText(Name As String) As String
        Dim Caption As String = If(Name, String.Empty)
        Const SpandrelPrefix As String = "Spandrel - "
        If Caption.StartsWith(SpandrelPrefix, StringComparison.OrdinalIgnoreCase) Then
            Caption = Caption.Substring(SpandrelPrefix.Length)
            Dim ModeSuffix As String = String.Empty
            Dim SuffixStart As Integer = Caption.IndexOf(" · ", StringComparison.Ordinal)
            If SuffixStart >= 0 Then
                ModeSuffix = Caption.Substring(SuffixStart)
                Caption = Caption.Substring(0, SuffixStart)
            End If
            If Caption.StartsWith("Auto (", StringComparison.OrdinalIgnoreCase) Then
                Caption = System.Text.RegularExpressions.Regex.Replace(
                    Caption, "\.(pth|pt|ckpt|onnx|safetensors)\b", String.Empty,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            Else
                Caption = System.IO.Path.GetFileNameWithoutExtension(Caption)
            End If
            Caption &= ModeSuffix
        End If
        Return Caption.Replace("_", " ")
    End Function

    Public Sub DrawList(ItemList As List(Of DragDropItem))
        If ListCanvas Is Nothing Then Return
        ImageWidth = ListCanvas.ClientSize.Width
        ImageHeight = ListCanvas.ClientSize.Height
        If ImageWidth <= 0 OrElse ImageHeight <= 0 Then Return

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

            If ItemList.Count = 0 Then
                Using EmptyTitleFont As New Font("Segoe UI", 14.0!, FontStyle.Bold, GraphicsUnit.Pixel)
                    Using EmptyBodyFont As New Font("Segoe UI", 12.0!, FontStyle.Regular, GraphicsUnit.Pixel)
                        Using CenterFormat As New StringFormat With {
                            .Alignment = StringAlignment.Center,
                            .LineAlignment = StringAlignment.Center,
                            .FormatFlags = StringFormatFlags.NoWrap
                        }
                            Gr.DrawString("No extra steps yet", EmptyTitleFont, Brushes.SlateGray,
                                New RectangleF(0, (ImageHeight \ 2) - 24, ImageWidth, 28), CenterFormat)
                            Gr.DrawString("The selected backend runs by itself. Add steps to chain multiple backends.",
                                EmptyBodyFont, Brushes.DimGray,
                                New RectangleF(8, (ImageHeight \ 2) + 5, ImageWidth - 16, 26), CenterFormat)
                        End Using
                    End Using
                End Using
            Else
                Using ItemFont As New Font("Segoe UI", 12.0!, FontStyle.Regular, GraphicsUnit.Pixel)
                    Using StepFont As New Font("Segoe UI", 12.0!, FontStyle.Bold, GraphicsUnit.Pixel)
                        Using CaptionFormat As New StringFormat With {
                            .LineAlignment = StringAlignment.Center,
                            .Alignment = StringAlignment.Center,
                            .Trimming = StringTrimming.EllipsisCharacter
                        }
                            Using BadgeFormat As New StringFormat With {
                                .LineAlignment = StringAlignment.Center,
                                .Alignment = StringAlignment.Center,
                                .FormatFlags = StringFormatFlags.NoWrap
                            }
                                Using SelectionBrush As New SolidBrush(Color.FromArgb(226, 239, 255))
                                    Using BadgeBrush As New SolidBrush(Color.FromArgb(43, 113, 178))
                                        Dim HighlightedIndex As Integer = If(IsDragging, CurrentIndex, ClickedIndex)
                                        For ItemIndex As Integer = 0 To ItemList.Count - 1
                                            Dim BaseX As Integer = ItemIndex Mod ColumnsPerRow
                                            Dim BaseY As Integer = ItemIndex \ ColumnsPerRow
                                            Dim ItemsInRow As Integer = Math.Min(ColumnsPerRow, ItemList.Count - (BaseY * ColumnsPerRow))
                                            Dim RowWidth As Integer = (ItemsInRow * ThumbSize) + ((ItemsInRow - 1) * ItemPadding)
                                            Dim RowStartX As Integer = CInt(Math.Floor((ImageWidth - RowWidth) / 2.0))
                                            Dim RealX As Integer = RowStartX + ((ThumbSize + ItemPadding) * BaseX)
                                            Dim RealY As Integer = ItemPadding + ((ThumbSize + CaptionHeight + ItemPadding) * BaseY)
                                            Dim IsSelected As Boolean = ItemIndex = HighlightedIndex
                                            Dim CaptionHighlightBounds As New Rectangle(RealX - 3, RealY + ThumbSize,
                                                ThumbSize + 6, CaptionHeight + 5)
                                            If IsSelected Then Gr.FillRectangle(SelectionBrush, CaptionHighlightBounds)
                                            Gr.DrawImage(ItemList(ItemIndex).Thumbnail, RealX, RealY, ThumbSize, ThumbSize)

                                            Dim BadgeSize As Integer = Math.Min(26, Math.Max(18, ThumbSize \ 5))
                                            Dim BadgeBounds As New Rectangle(RealX + 4, RealY + 4, BadgeSize, BadgeSize)
                                            Gr.FillEllipse(BadgeBrush, BadgeBounds)
                                            Gr.DrawString((ItemIndex + 1).ToString(), StepFont, Brushes.White, BadgeBounds, BadgeFormat)

                                            Dim CaptionBounds As New RectangleF(RealX, RealY + ThumbSize + 2, ThumbSize, CaptionHeight)
                                            Gr.DrawString(GetCaptionText(ItemList(ItemIndex).Name), ItemFont, Brushes.Black, CaptionBounds, CaptionFormat)
                                        Next
                                    End Using
                                End Using
                            End Using
                        End Using
                    End Using
                End Using
            End If
        End Using
        ListCanvas.BackgroundImage = ListImage
        ListCanvas.Refresh()
        If PreviousListImage IsNot Nothing Then PreviousListImage.Dispose()
    End Sub

End Class
