Public Class ConfigPackages

    Public Shared vbQuote As Char = ControlChars.Quote

    Public Structure ArguementString
        Dim Arguements As String
        Public Function GetArguements()
            Return Arguements
        End Function
        Public Sub AddArguement(Flag As String)
            Arguements += " " & Flag
        End Sub
        Public Sub AddArguement(Flag As String, Value As String)
            Arguements += " " & Flag & " " & Value
        End Sub
    End Structure

    Public Structure CaffePackage
        Public Property Mode As String
        Public Property Scale As Decimal
        Public Property Noise As Decimal
        Public Property Process As String
        Public Property TAA As Boolean
        Public Sub New(_Mode As String, _Scale As Decimal, _Noise As Decimal, _Process As String, _TAA As Boolean)
            Mode = _Mode
            Scale = _Scale
            Noise = _Noise
            Process = _Process
            TAA = _TAA
        End Sub
    End Structure

    Public Structure VulkanPackage
        Public Property Scale As Decimal
        Public Property Noise As Decimal
        Public Property Format As String
        Public Property TAA As Boolean
        Public Sub New(_Scale As Decimal, _Noise As Decimal, _Format As String, _TAA As Boolean)
            Scale = _Scale
            Noise = _Noise
            Format = _Format
            TAA = _TAA
        End Sub
    End Structure

    Public Structure CPPPackage
        Public Property Mode As String
        Public Property Scale As Decimal
        Public Property Noise As Decimal
        Public Property Format As String
        Public Property NoGPU As Boolean
        Public Property OpenCL As Boolean
        Public Property TTA As Boolean
        Public Sub New(_Mode As String, _Scale As Decimal, _Noise As Decimal, _Format As String, _NoGPU As Boolean, _OpenCL As Boolean, _TTA As Boolean)
            Mode = _Mode
            Scale = _Scale
            Noise = _Noise
            Format = _Format
            NoGPU = _NoGPU
            OpenCL = _OpenCL
            TTA = _TTA
        End Sub
    End Structure

    Public Structure A4KPackage
        Public Property Passes As Decimal
        Public Property PushColors As Decimal
        Public Property ColorStrength As Decimal
        Public Property GradStrength As Decimal
        Public Property Scale As Decimal
        Public Property Pre As Boolean
        Public Property Post As Boolean
        Public Property Filter As Integer
        Public Property FilterType As Integer
        Public Property GPU As Boolean
        Public Property CNN As Boolean
        Public Sub New(_Passes As Decimal, _PushColors As Decimal, _ColorStrength As Decimal, _GradStrength As Decimal, _Scale As Decimal, _Pre As Boolean, _Post As Boolean, _Filter As Integer, _FilterType As Integer, _GPU As Boolean, _CNN As Boolean)
            Passes = _Passes
            PushColors = _PushColors
            ColorStrength = _ColorStrength
            GradStrength = _GradStrength
            Scale = _Scale
            Pre = _Pre
            Post = _Post
            Filter = _Filter
            FilterType = _FilterType
            GPU = _GPU
            CNN = _CNN
        End Sub
    End Structure

    Public Structure PyPackage
        Public Property Model As String
        Public Property InputFlag As String
        Public Property OutputFlag As String
        Public Property Arguements As List(Of String)
        Public Sub New(_Model As String, _InputFlag As String, _OutputFlag As String, Data As DataGridView)
            Model = _Model
            InputFlag = _InputFlag
            OutputFlag = _OutputFlag
            Arguements = New List(Of String)
            For i = 0 To Data.Rows.Count - 2
                If Not Data.Rows(i).Cells(0).Value = Nothing Then
                    If Data.Rows(i).Cells(1).Value = Nothing Then
                        Arguements.Add(Data.Rows(i).Cells(0).Value.ToString)
                        Arguements.Add("")
                    Else
                        Arguements.Add(Data.Rows(i).Cells(0).Value.ToString)
                        Arguements.Add(Data.Rows(i).Cells(1).Value.ToString)
                    End If
                End If
            Next
        End Sub
    End Structure

    Public Shared Function MakeCaffeCommand(SourceImage As String, NewImage As String, Package As CaffePackage) As String
        Dim Result As New ArguementString
        Result.AddArguement("-i", Quote(SourceImage))
        Result.AddArguement("-o", Quote(NewImage))
        Result.AddArguement("-m", Package.Mode)
        Result.AddArguement("-s", Package.Scale)
        Result.AddArguement("-n", Package.Noise)
        Result.AddArguement("-p", Package.Process)
        Result.AddArguement("-t", IIf(Package.TAA = True, 1, 0))
        Return Result.GetArguements
    End Function

    Public Shared Function MakeVulkanCommand(SourceImage As String, NewImage As String, Package As VulkanPackage) As String
        Dim Result As New ArguementString
        Result.AddArguement("-i", Quote(SourceImage))
        Result.AddArguement("-o", Quote(NewImage))
        Result.AddArguement("-s", Package.Scale)
        Result.AddArguement("-n", Package.Noise)
        Result.AddArguement("-f", Package.Format)
        Result.AddArguement(IIf(Package.TAA = True, "-x", ""))
        Return Result.GetArguements
    End Function

    Public Shared Function MakeCPPCommand(SourceImage As String, NewImage As String, Package As CPPPackage) As String
        Dim Result As New ArguementString
        Result.AddArguement("-i", Quote(SourceImage))
        Result.AddArguement("-o", Quote(NewImage))
        Result.AddArguement("-m", Package.Mode)
        Result.AddArguement("--scale-ratio", Package.Scale)
        Result.AddArguement("--noise-level", Package.Noise)
        Result.AddArguement("-f", Package.Format)
        Result.AddArguement(IIf(Package.NoGPU = True, "--disable-gpu", ""))
        Result.AddArguement(IIf(Package.OpenCL = True, "--force-OpenCL", ""))
        Result.AddArguement(IIf(Package.TTA = True, "-t", ""))
        Return Result.GetArguements
    End Function

    Public Shared Function MakeA4KCommand(SourceImage As String, NewImage As String, Package As A4KPackage) As String
        Dim Result As New ArguementString
        Result.AddArguement("-i", Quote(SourceImage))
        Result.AddArguement("-o", Quote(NewImage))
        Result.AddArguement("-p", Package.Passes)
        Result.AddArguement("-n", Package.PushColors)
        Result.AddArguement("-c", Package.ColorStrength)
        Result.AddArguement("-g", Package.GradStrength)
        Result.AddArguement("-z", Package.Scale)
        Result.AddArguement(IIf(Package.Pre = True, "-b", ""))
        Result.AddArguement(IIf(Package.Post = True, "-a", ""))
        Select Case Package.FilterType
            Case 1
                Result.AddArguement("-r", Package.Filter)
            Case 2
                Result.AddArguement("-e", Package.Filter)
        End Select
        Result.AddArguement(IIf(Package.GPU = True, "-q", ""))
        Result.AddArguement(IIf(Package.CNN = True, "-w", ""))
        Result.AddArguement("-A")
        Return Result.GetArguements
    End Function

    Public Shared Function MakePyCommand(SourceFolder As String, DestFolder As String, Package As PyPackage) As String
        Dim Result As New ArguementString
        Result.AddArguement(Quote(Package.Model))
        Result.AddArguement(Package.InputFlag, Quote(SourceFolder))
        Result.AddArguement(Package.OutputFlag, Quote(DestFolder))
        For i = 0 To Package.Arguements.Count - 1 Step 2
            Result.AddArguement(Package.Arguements(i), Package.Arguements(i + 1))
        Next
        Return Result.GetArguements
    End Function

    Public Shared Function Quote(Source As String) As String
        Return vbQuote & Source & vbQuote
    End Function

End Class