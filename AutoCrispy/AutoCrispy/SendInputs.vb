Imports System.Runtime.InteropServices

Public Class SendInputs

    Private Const KeyDown As Integer = &H0
    Private Const KeyUp As Integer = &H2

    <DllImport("user32.dll")>
    Private Shared Function SendInput(nInputs As Integer, pInputs() As INPUT, cbSize As Integer) As Integer
    End Function

    <StructLayout(LayoutKind.Explicit)>
    Private Structure INPUT
        <FieldOffset(0)>
        Public type As Integer
        <FieldOffset(8)>
        Public mi As MOUSEINPUT
        <FieldOffset(8)>
        Public ki As KEYBDINPUT
        <FieldOffset(8)>
        Public hi As HARDWAREINPUT
    End Structure

    Private Structure MOUSEINPUT
        Public dx As Integer
        Public dy As Integer
        Public mouseData As Integer
        Public dwFlags As Integer
        Public time As Integer
        Public dwExtraInfo As IntPtr
    End Structure

    Private Structure KEYBDINPUT
        Public wVk As Short
        Public wScan As Short
        Public dwFlags As Integer
        Public time As Integer
        Public dwExtraInfo As IntPtr
    End Structure

    Private Structure HARDWAREINPUT
        Public uMsg As Integer
        Public wParamL As Short
        Public wParamH As Short
    End Structure

    Public Shared Sub SendKey(key As Char)
        Dim Inpts(1) As INPUT

        Inpts(0).type = 1
        Inpts(0).ki.wVk = Convert.ToInt16(CChar(key))
        Inpts(0).ki.dwFlags = KeyDown

        Inpts(1).type = 1
        Inpts(1).ki.wVk = Convert.ToInt16(CChar(key))
        Inpts(1).ki.dwFlags = KeyUp

        SendInput(2, Inpts, Marshal.SizeOf(GetType(INPUT)))
    End Sub
End Class