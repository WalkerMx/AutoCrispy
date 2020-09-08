<Serializable()> Public Class FormSettings

    Public Shared Sub LoadSettings(Source As Form1, LoadedSettings As Settings)
        'Load Caffe Settings
        Source.CaffeMode.Text = LoadedSettings.CaffePak.Mode
        Source.CaffeScale.Value = LoadedSettings.CaffePak.Scale
        Source.CaffeNoise.Value = LoadedSettings.CaffePak.Noise
        Source.CaffeProcess.Text = LoadedSettings.CaffePak.Process
        Source.CaffeTAA.Checked = LoadedSettings.CaffePak.TAA

        'Load Vulkan Settings
        Source.VulkanScale.Value = LoadedSettings.VulkanPak.Scale
        Source.VulkanNoise.Value = LoadedSettings.VulkanPak.Noise
        Source.VulkanFormat.Text = LoadedSettings.VulkanPak.Format
        Source.VulkanTAA.Checked = LoadedSettings.VulkanPak.TAA

        'Load Cpp Settings
        Source.WaifuCPPMode.Text = LoadedSettings.CppPak.Mode
        Source.WaifuCPPScale.Value = LoadedSettings.CppPak.Scale
        Source.WaifuCPPNoise.Value = LoadedSettings.CppPak.Noise
        Source.WaifuCPPFormat.Text = LoadedSettings.CppPak.Format
        Source.WaifuCppGPU.Checked = LoadedSettings.CppPak.GPU
        Source.WaifuCPPOpenCL.Checked = LoadedSettings.CppPak.ForceOpenCL

        'Load Anime4k Settings
        Source.AnimeCPPScale.Value = LoadedSettings.AnimePak.Scale
        Source.AnimeCppPre.Checked = LoadedSettings.AnimePak.PreProcess
        Source.AnimeCppPost.Checked = LoadedSettings.AnimePak.PostProcess
        Source.AnimeCppPreFilter.Checked = LoadedSettings.AnimePak.PreFilter
        Source.AnimeCppPostFilter.Checked = LoadedSettings.AnimePak.PostFilter
        SetFilters(Source.AnimeCppPreFilters, LoadedSettings.AnimePak.PreFilterType)
        SetFilters(Source.AnimeCppPostFilters, LoadedSettings.AnimePak.PostFilterType)
        Source.AnimeCPPGpu.Checked = LoadedSettings.AnimePak.GPU
        Source.AnimeCPPCnn.Checked = LoadedSettings.AnimePak.CNN

        'Load Python Settings
        Source.PyBatchSize.Value = LoadedSettings.PythonPak.BatchSize
        Source.PyInputFlag.Text = LoadedSettings.PythonPak.InputFlag
        Source.PyOutputFlag.Text = LoadedSettings.PythonPak.OutputFlag

        'Load UI Paths
        Source.InputTextBox.Text = LoadedSettings.Paths.InputPath
        Source.OutputTextBox.Text = LoadedSettings.Paths.OutputPath

        'Load Basic UI Settings
        Source.ThreadComboBox.SelectedIndex = LoadedSettings.BasicSettings.ThreadIndex
        Source.NumericThreads.Value = LoadedSettings.BasicSettings.ThreadCount
        Source.DefringeCheck.Checked = LoadedSettings.BasicSettings.Defringe
        Source.DefringeThresh.Value = LoadedSettings.BasicSettings.DefringeThreshold
        Source.TabGroup.SelectedIndex = LoadedSettings.BasicSettings.SelectedTab

        'Load Expert UI Settings
        Source.DebugCheckbox.Checked = LoadedSettings.ExpertSettings.Logging
        Source.ExpertSettingsBox.Text = LoadedSettings.ExpertSettings.ExpertFlags
        Source.CleanupCheckBox.Checked = LoadedSettings.ExpertSettings.ClearInput
        Source.PortableCheckBox.Checked = LoadedSettings.ExpertSettings.Portable

        'Load Internal Settings
        Source.ChainList = LoadedSettings.Chain
        For Each ChainItem As ChainObject In Source.ChainList
            Source.ChainPreview.Items.Add(New ListViewItem(ChainItem.Name, ChainItem.IconIndex))
        Next
    End Sub

    Private Shared Function GetFilters(Source As CheckedListBox) As Integer
        Dim StringResult As String = ""
        For i = 0 To 6
            If Source.GetItemChecked(i) = True Then
                StringResult = "1"c & StringResult
            Else
                StringResult = "0"c & StringResult
            End If
        Next
        Return Convert.ToInt32(StringResult, 2)
    End Function

    Private Shared Sub SetFilters(Source As CheckedListBox, Value As Integer)
        Dim BinChecked As String = Convert.ToString(Value, 2).PadLeft(7, "0"c)
        For i = 0 To 6
            Source.SetItemChecked(i, IIf(BinChecked(i) = "1"c, True, False))
        Next
    End Sub

    <Xml.Serialization.XmlInclude(GetType(Waifu2xCaffePackage))>
    <Xml.Serialization.XmlInclude(GetType(VulkanNcnnPackage))>
    <Xml.Serialization.XmlInclude(GetType(Waifu2xCppPackage))>
    <Xml.Serialization.XmlInclude(GetType(Anime4kPackage))>
    <Xml.Serialization.XmlInclude(GetType(PythonPackage))>
    <Xml.Serialization.XmlInclude(GetType(ProgramPaths))>
    <Xml.Serialization.XmlInclude(GetType(ProgramSettings))>
    <Xml.Serialization.XmlInclude(GetType(AdvancedSettings))>
    <Xml.Serialization.XmlInclude(GetType(ChainObject))>
    <Serializable()> Public Structure Settings
        Public Property CaffePak As Waifu2xCaffePackage
        Public Property VulkanPak As VulkanNcnnPackage
        Public Property CppPak As Waifu2xCppPackage
        Public Property AnimePak As Anime4kPackage
        Public Property PythonPak As PythonPackage
        Public Property Paths As ProgramPaths
        Public Property BasicSettings As ProgramSettings
        Public Property ExpertSettings As AdvancedSettings
        Public Property Chain As List(Of ChainObject)
        Public Sub New(Source As Form1)
            CaffePak = New Waifu2xCaffePackage(Source.CaffeMode.Text.ToLower, Source.CaffeScale.Value, Source.CaffeNoise.Value, Source.CaffeProcess.Text.ToLower, Source.CaffeTAA.Checked)
            VulkanPak = New VulkanNcnnPackage(Source.VulkanScale.Value, Source.VulkanNoise.Value, Source.VulkanFormat.Text.ToLower, Source.VulkanTAA.Checked)
            CppPak = New Waifu2xCppPackage(Source.WaifuCPPMode.Text.ToLower, Source.WaifuCPPScale.Value, Source.WaifuCPPNoise.Value, Source.WaifuCPPFormat.Text.ToLower, Source.WaifuCppGPU.Checked, Source.WaifuCPPOpenCL.Checked)
            AnimePak = New Anime4kPackage(Source.AnimeCPPScale.Value, Source.AnimeCppPre.Checked, Source.AnimeCppPost.Checked, Source.AnimeCppPreFilter.Checked, Source.AnimeCppPostFilter.Checked, GetFilters(Source.AnimeCppPreFilters), GetFilters(Source.AnimeCppPostFilters), Source.AnimeCPPGpu.Checked, Source.AnimeCPPCnn.Checked)
            PythonPak = New PythonPackage(GetPyStr(Source.PyPaths, Source.PyScript.SelectedIndex), GetPyStr(Source.PyModels, Source.PyModel.SelectedIndex), Source.PyBatchSize.Value, Source.PyInputFlag.Text.Trim, Source.PyOutputFlag.Text.Trim, Source.PyArguements)
            Paths = New ProgramPaths(Source.InputTextBox.Text, Source.OutputTextBox.Text)
            BasicSettings = New ProgramSettings(Source.ExeComboBox.SelectedItem, Source.ThreadComboBox.SelectedIndex, Source.NumericThreads.Value, Source.DefringeCheck.Checked, Source.DefringeThresh.Value, Source.TabGroup.SelectedIndex)
            ExpertSettings = New AdvancedSettings(Source.DebugCheckbox.Checked, Source.ExpertSettingsBox.Text, Source.CleanupCheckBox.Checked, Source.PortableCheckBox.Checked)
            Chain = Source.ChainList
        End Sub
        Public Function GetPyStr(PyList As List(Of String), Index As Integer) As String
            If PyList.Count > 0 Then
                Return PyList(Index)
            End If
            Return ""
        End Function
    End Structure

    <Serializable()> Public Structure ProgramPaths
        Public Property InputPath As String
        Public Property OutputPath As String
        Public Sub New(_Input As String, _Output As String)
            InputPath = _Input
            OutputPath = _Output
        End Sub
    End Structure

    <Serializable()> Public Structure ProgramSettings
        Public Property Backend As String
        Public Property ThreadIndex As Integer
        Public Property ThreadCount As Integer
        Public Property Defringe As Boolean
        Public Property DefringeThreshold As Integer
        Public Property SelectedTab As Integer
        Public Sub New(_Backend As String, _ThreadIndex As Integer, _ThreadCount As Integer, _Defringe As Boolean, _DefringeThreshold As Integer, _SelectedTab As Integer)
            Backend = _Backend
            ThreadIndex = _ThreadIndex
            ThreadCount = _ThreadCount
            Defringe = _Defringe
            DefringeThreshold = _DefringeThreshold
            SelectedTab = _SelectedTab
        End Sub
    End Structure

    <Serializable()> Public Structure AdvancedSettings
        Public Property Logging As Boolean
        Public Property ExpertFlags As String
        Public Property ClearInput As Boolean
        Public Property Portable As Boolean
        Public Sub New(_Logging As Boolean, _ExpertFlags As String, _ClearInput As Boolean, _Portable As Boolean)
            Logging = _Logging
            ExpertFlags = _ExpertFlags
            ClearInput = _ClearInput
            Portable = _Portable
        End Sub
    End Structure

    <Xml.Serialization.XmlInclude(GetType(Waifu2xCaffePackage))>
    <Xml.Serialization.XmlInclude(GetType(VulkanNcnnPackage))>
    <Xml.Serialization.XmlInclude(GetType(Waifu2xCppPackage))>
    <Xml.Serialization.XmlInclude(GetType(Anime4kPackage))>
    <Xml.Serialization.XmlInclude(GetType(PythonPackage))>
    <Serializable()> Public Structure ChainObject
        Public Property Name As String
        Public Property IconIndex As Integer
        Public Property FileLocation As String
        Public Property Package As Object
        Public Property PackageType As String
        Public Sub New(_Name As String, _IconIndex As Integer, _FileLocation As String, _PackageType As String, Source As Form1)
            Name = _Name
            IconIndex = _IconIndex
            FileLocation = _FileLocation
            PackageType = _PackageType
            Select Case PackageType
                Case "Waifu2x Caffe"
                    Package = New Waifu2xCaffePackage(Source.CaffeMode.Text.ToLower, Source.CaffeScale.Value, Source.CaffeNoise.Value, Source.CaffeProcess.Text.ToLower, Source.CaffeTAA.Checked)
                Case "Waifu2x Vulkan"
                    Package = New VulkanNcnnPackage(Source.VulkanScale.Value, Source.VulkanNoise.Value, Source.VulkanFormat.Text.ToLower, Source.VulkanTAA.Checked)
                Case "RealSR Vulkan"
                    Package = New VulkanNcnnPackage(Source.VulkanScale.Value, Source.VulkanNoise.Value, Source.VulkanFormat.Text.ToLower, Source.VulkanTAA.Checked)
                Case "SRMD Vulkan"
                    Package = New VulkanNcnnPackage(Source.VulkanScale.Value, Source.VulkanNoise.Value, Source.VulkanFormat.Text.ToLower, Source.VulkanTAA.Checked)
                Case "Waifu2x CPP"
                    Package = New Waifu2xCppPackage(Source.WaifuCPPMode.Text.ToLower, Source.WaifuCPPScale.Value, Source.WaifuCPPNoise.Value, Source.WaifuCPPFormat.Text.ToLower, Source.WaifuCppGPU.Checked, Source.WaifuCPPOpenCL.Checked)
                Case "Anime4k CPP"
                    Package = New Anime4kPackage(Source.AnimeCPPScale.Value, Source.AnimeCppPre.Checked, Source.AnimeCppPost.Checked, Source.AnimeCppPreFilter.Checked, Source.AnimeCppPostFilter.Checked, GetFilters(Source.AnimeCppPreFilters), GetFilters(Source.AnimeCppPostFilters), Source.AnimeCPPGpu.Checked, Source.AnimeCPPCnn.Checked)
                Case "Python"
                    Package = New PythonPackage(Source.PyPaths(Source.PyScript.SelectedIndex), Source.PyModels(Source.PyModel.SelectedIndex), Source.PyBatchSize.Value, Source.PyInputFlag.Text.Trim, Source.PyOutputFlag.Text.Trim, Source.PyArguements)
            End Select
        End Sub
    End Structure

    <Serializable()> Public Structure Waifu2xCaffePackage
        Public Property Mode As String
        Public Property Scale As Decimal
        Public Property Noise As Decimal
        Public Property Process As String
        Public Property TAA As Boolean
        Public Property FileTypes As List(Of String)
        Public Sub New(_Mode As String, _Scale As Decimal, _Noise As Decimal, _Process As String, _TAA As Boolean)
            Mode = _Mode
            Scale = _Scale
            Noise = _Noise
            Process = _Process
            TAA = _TAA
            FileTypes = {".png", ".jpg", ".jpeg", ".tif", ".tiff", ".bmp", ".tga"}.ToList
        End Sub
    End Structure

    <Serializable()> Public Structure VulkanNcnnPackage
        Public Property Scale As Decimal
        Public Property Noise As Decimal
        Public Property Format As String
        Public Property TAA As Boolean
        Public Property FileTypes As List(Of String)
        Public Sub New(_Scale As Decimal, _Noise As Decimal, _Format As String, _TAA As Boolean)
            Scale = _Scale
            Noise = _Noise
            Format = _Format
            TAA = _TAA
            FileTypes = {".png", ".webp", ".jpg"}.ToList
        End Sub
    End Structure

    <Serializable()> Public Structure Waifu2xCppPackage
        Public Property Mode As String
        Public Property Scale As Decimal
        Public Property Noise As Decimal
        Public Property Format As String
        Public Property GPU As Boolean
        Public Property ForceOpenCL As Boolean
        Public Property FileTypes As List(Of String)
        Public Sub New(_Mode As String, _Scale As Decimal, _Noise As Decimal, _Format As String, _GPU As Boolean, _ForceOpenCL As Boolean)
            Mode = _Mode
            Scale = _Scale
            Noise = _Noise
            Format = _Format
            GPU = _GPU
            ForceOpenCL = _ForceOpenCL
            FileTypes = {".bmp", ".dib", ".exr", ".hdr", ".jpe", ".jpeg", ".jpg", ".pbm", ".pgm", ".pic", ".png", ".pnm", ".ppm", ".pxm", ".ras", ".sr", ".tif", ".tiff", ".webp"}.ToList
        End Sub
    End Structure

    <Serializable()> Public Structure Anime4kPackage
        Public Property Passes As Decimal
        Public Property PushColors As Decimal
        Public Property PushColorStrength As Decimal
        Public Property PushGradStrength As Decimal
        Public Property Scale As Decimal
        Public Property PreProcess As Boolean
        Public Property PostProcess As Boolean
        Public Property PreFilter As Boolean
        Public Property PostFilter As Boolean
        Public Property PreFilterType As Integer
        Public Property PostFilterType As Integer
        Public Property GPU As Boolean
        Public Property CNN As Boolean
        Public Property FileTypes As List(Of String)
        Public Sub New(_Scale As Decimal, _PreProcess As Boolean, _PostProcess As Boolean, _PreFilter As Boolean, _PostFilter As Boolean, _PreFilterType As Integer, _PostFilterType As Integer, _GPU As Boolean, _CNN As Boolean)
            Passes = 2
            PushColors = 2
            PushColorStrength = 0.3
            PushGradStrength = 1
            PreProcess = _PreProcess
            PostProcess = _PostProcess
            PreFilter = _PreFilter
            PostFilter = _PostFilter
            PreFilterType = _PreFilterType
            PostFilterType = _PostFilterType
            Scale = _Scale
            GPU = _GPU
            CNN = _CNN
            FileTypes = {".png", ".jpg"}.ToList
        End Sub
    End Structure

    <Serializable()> Public Structure PythonPackage
        Public Property Script As String
        Public Property Model As String
        Public Property BatchSize As Integer
        Public Property InputFlag As String
        Public Property OutputFlag As String
        Public Property ScriptFlags As List(Of String())
        Public Property FileTypes As List(Of String)
        Public Sub New(_Script As String, _Model As String, _BatchSize As String, _InputFlag As String, _OutputFlag As String, Data As DataGridView)
            Script = _Script
            Model = _Model
            BatchSize = _BatchSize
            InputFlag = _InputFlag
            OutputFlag = _OutputFlag
            ScriptFlags = New List(Of String())
            For i = 0 To Data.Rows.Count - 2
                If Not Data.Rows(i).Cells(0).Value = Nothing Then
                    If Data.Rows(i).Cells(1).Value = Nothing Then
                        ScriptFlags.Add({Data.Rows(i).Cells(0).Value.ToString, ""})
                    Else
                        ScriptFlags.Add({Data.Rows(i).Cells(0).Value.ToString, Data.Rows(i).Cells(1).Value.ToString})
                    End If
                End If
            Next
            FileTypes = {".png", ".jpg", ".bmp"}.ToList
        End Sub
    End Structure

End Class