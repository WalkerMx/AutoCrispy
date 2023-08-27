Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices

' DirectBitmap Class by A.Konzel from StackOverflow
' https://stackoverflow.com/questions/24701703/
' Converted to VB.NET by WalkerMx

Public Class DirectBitmap
    Implements IDisposable

    Public Property Bitmap As Bitmap
    Public Property Width As Integer
    Public Property Height As Integer
    Public Property Bits As Integer()
    Public Property Disposed As Boolean
    Protected Property BitsHandle As GCHandle

    Public Sub New(_Width As Integer, _Height As Integer)
        Width = _Width
        Height = _Height
        Bits = New Integer(Width * Height - 1) {}
        BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned)
        Bitmap = New Bitmap(Width, Height, Width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject())
    End Sub

    Public Sub New(Source As Bitmap)
        Me.New(Source.Width, Source.Height)
        Using g As Graphics = Graphics.FromImage(Bitmap)
            g.CompositingMode = Drawing2D.CompositingMode.SourceCopy
            g.InterpolationMode = Drawing2D.InterpolationMode.NearestNeighbor
            g.DrawImage(Source, 0, 0, Width, Height)
        End Using
    End Sub

    Public Sub SetPixel(X As Integer, Y As Integer, Color As Color)
        Bits(X + (Y * Width)) = Color.ToArgb()
    End Sub

    Public Function GetPixel(X As Integer, Y As Integer) As Color
        Return Color.FromArgb(Bits(X + (Y * Width)))
    End Function

    Protected Overridable Sub Dispose(Disposing As Boolean)
        If Not Disposed Then
            If Disposing Then Bitmap.Dispose()
            BitsHandle.Free()
        End If
        Disposed = True
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
        MyBase.Finalize()
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

End Class
