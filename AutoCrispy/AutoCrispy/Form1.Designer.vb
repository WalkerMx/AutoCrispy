<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        Me.CaffeGroup = New System.Windows.Forms.GroupBox()
        Me.CaffeTAA = New System.Windows.Forms.CheckBox()
        Me.CaffeProcess = New System.Windows.Forms.ComboBox()
        Me.CaffeScale = New System.Windows.Forms.NumericUpDown()
        Me.CaffeNoise = New System.Windows.Forms.NumericUpDown()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.CaffeMode = New System.Windows.Forms.ComboBox()
        Me.WatchDog = New System.Windows.Forms.Timer(Me.components)
        Me.WatchDogButton = New System.Windows.Forms.Button()
        Me.SettingsGroup = New System.Windows.Forms.GroupBox()
        Me.DefringeThresh = New System.Windows.Forms.NumericUpDown()
        Me.DefringeCheck = New System.Windows.Forms.CheckBox()
        Me.UpscaleProgress = New System.Windows.Forms.ProgressBar()
        Me.NumericThreads = New System.Windows.Forms.NumericUpDown()
        Me.ThreadComboBox = New System.Windows.Forms.ComboBox()
        Me.ExeComboBox = New System.Windows.Forms.ComboBox()
        Me.Label21 = New System.Windows.Forms.Label()
        Me.Label22 = New System.Windows.Forms.Label()
        Me.Label12 = New System.Windows.Forms.Label()
        Me.CleanupCheckBox = New System.Windows.Forms.CheckBox()
        Me.RunOnceButton = New System.Windows.Forms.Button()
        Me.VulkanGroup = New System.Windows.Forms.GroupBox()
        Me.VulkanTAA = New System.Windows.Forms.CheckBox()
        Me.VulkanFormat = New System.Windows.Forms.ComboBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.VulkanScale = New System.Windows.Forms.NumericUpDown()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.VulkanNoise = New System.Windows.Forms.NumericUpDown()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.WaifuCPPGroup = New System.Windows.Forms.GroupBox()
        Me.WaifuCppGPU = New System.Windows.Forms.CheckBox()
        Me.WaifuCPPOpenCL = New System.Windows.Forms.CheckBox()
        Me.WaifuCPPMode = New System.Windows.Forms.ComboBox()
        Me.WaifuCPPFormat = New System.Windows.Forms.ComboBox()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.Label14 = New System.Windows.Forms.Label()
        Me.WaifuCPPScale = New System.Windows.Forms.NumericUpDown()
        Me.Label11 = New System.Windows.Forms.Label()
        Me.WaifuCPPNoise = New System.Windows.Forms.NumericUpDown()
        Me.Label13 = New System.Windows.Forms.Label()
        Me.AnimeCPPGroup = New System.Windows.Forms.GroupBox()
        Me.AnimeCppPostFilter = New System.Windows.Forms.CheckBox()
        Me.AnimeCppPreFilter = New System.Windows.Forms.CheckBox()
        Me.AnimeCPPCnn = New System.Windows.Forms.CheckBox()
        Me.AnimeCPPGpu = New System.Windows.Forms.CheckBox()
        Me.AnimeCppPost = New System.Windows.Forms.CheckBox()
        Me.AnimeCppPre = New System.Windows.Forms.CheckBox()
        Me.AnimeCPPScale = New System.Windows.Forms.NumericUpDown()
        Me.AnimeCppPostFilters = New System.Windows.Forms.CheckedListBox()
        Me.AnimeCppPreFilters = New System.Windows.Forms.CheckedListBox()
        Me.Label20 = New System.Windows.Forms.Label()
        Me.PyGroup = New System.Windows.Forms.GroupBox()
        Me.PyBatchSize = New System.Windows.Forms.NumericUpDown()
        Me.Label23 = New System.Windows.Forms.Label()
        Me.PyArguements = New System.Windows.Forms.DataGridView()
        Me.PyFlag = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.PyValue = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.PyModel = New System.Windows.Forms.ComboBox()
        Me.PyInputFlag = New System.Windows.Forms.TextBox()
        Me.PyScript = New System.Windows.Forms.ComboBox()
        Me.PyOutputFlag = New System.Windows.Forms.TextBox()
        Me.Label26 = New System.Windows.Forms.Label()
        Me.Label28 = New System.Windows.Forms.Label()
        Me.Label25 = New System.Windows.Forms.Label()
        Me.Label27 = New System.Windows.Forms.Label()
        Me.WorkHorse = New System.ComponentModel.BackgroundWorker()
        Me.ChainThumbs = New System.Windows.Forms.ImageList(Me.components)
        Me.ChainGroup = New System.Windows.Forms.TabPage()
        Me.ChainLoad = New System.Windows.Forms.Button()
        Me.ChainSave = New System.Windows.Forms.Button()
        Me.ChainRemove = New System.Windows.Forms.Button()
        Me.ChainAdd = New System.Windows.Forms.Button()
        Me.ChainPreview = New System.Windows.Forms.ListView()
        Me.ChainContext = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.ChainContextEdit = New System.Windows.Forms.ToolStripMenuItem()
        Me.ChainContextDelete = New System.Windows.Forms.ToolStripMenuItem()
        Me.PathGroup = New System.Windows.Forms.TabPage()
        Me.OutputBrowse = New System.Windows.Forms.Button()
        Me.InputTextBox = New System.Windows.Forms.TextBox()
        Me.OutputTextBox = New System.Windows.Forms.TextBox()
        Me.InputBrowse = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.TabGroup = New System.Windows.Forms.TabControl()
        Me.AdvSettingsGroup = New System.Windows.Forms.TabPage()
        Me.SeamMargin = New System.Windows.Forms.NumericUpDown()
        Me.SeamScale = New System.Windows.Forms.NumericUpDown()
        Me.SeamsBox = New System.Windows.Forms.ComboBox()
        Me.Label18 = New System.Windows.Forms.Label()
        Me.Label17 = New System.Windows.Forms.Label()
        Me.Label16 = New System.Windows.Forms.Label()
        Me.PortableCheckBox = New System.Windows.Forms.CheckBox()
        Me.Label15 = New System.Windows.Forms.Label()
        Me.ExpertSettingsBox = New System.Windows.Forms.TextBox()
        Me.DebugCheckbox = New System.Windows.Forms.CheckBox()
        Me.DDxGroup = New System.Windows.Forms.GroupBox()
        Me.DDxFormatLabel = New System.Windows.Forms.Label()
        Me.DDxFormatListBox = New System.Windows.Forms.ListBox()
        Me.Dx10Checkbox = New System.Windows.Forms.CheckBox()
        Me.Dx9CheckBox = New System.Windows.Forms.CheckBox()
        Me.FlComboBox = New System.Windows.Forms.ComboBox()
        Me.Label24 = New System.Windows.Forms.Label()
        Me.AlphaCheckBox = New System.Windows.Forms.CheckBox()
        Me.PmAlphaCheckBox = New System.Windows.Forms.CheckBox()
        Me.SepAlphaCheckBox = New System.Windows.Forms.CheckBox()
        Me.Label19 = New System.Windows.Forms.Label()
        Me.DDxModeBox = New System.Windows.Forms.ComboBox()
        Me.CaffeGroup.SuspendLayout()
        CType(Me.CaffeScale, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.CaffeNoise, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SettingsGroup.SuspendLayout()
        CType(Me.DefringeThresh, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.NumericThreads, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.VulkanGroup.SuspendLayout()
        CType(Me.VulkanScale, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.VulkanNoise, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.WaifuCPPGroup.SuspendLayout()
        CType(Me.WaifuCPPScale, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.WaifuCPPNoise, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.AnimeCPPGroup.SuspendLayout()
        CType(Me.AnimeCPPScale, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.PyGroup.SuspendLayout()
        CType(Me.PyBatchSize, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PyArguements, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ChainGroup.SuspendLayout()
        Me.ChainContext.SuspendLayout()
        Me.PathGroup.SuspendLayout()
        Me.TabGroup.SuspendLayout()
        Me.AdvSettingsGroup.SuspendLayout()
        CType(Me.SeamMargin, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.SeamScale, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.DDxGroup.SuspendLayout()
        Me.SuspendLayout()
        '
        'CaffeGroup
        '
        Me.CaffeGroup.Controls.Add(Me.CaffeTAA)
        Me.CaffeGroup.Controls.Add(Me.CaffeProcess)
        Me.CaffeGroup.Controls.Add(Me.CaffeScale)
        Me.CaffeGroup.Controls.Add(Me.CaffeNoise)
        Me.CaffeGroup.Controls.Add(Me.Label6)
        Me.CaffeGroup.Controls.Add(Me.Label5)
        Me.CaffeGroup.Controls.Add(Me.Label4)
        Me.CaffeGroup.Controls.Add(Me.Label3)
        Me.CaffeGroup.Controls.Add(Me.CaffeMode)
        Me.CaffeGroup.Location = New System.Drawing.Point(643, 12)
        Me.CaffeGroup.Name = "CaffeGroup"
        Me.CaffeGroup.Size = New System.Drawing.Size(396, 200)
        Me.CaffeGroup.TabIndex = 11
        Me.CaffeGroup.TabStop = False
        Me.CaffeGroup.Text = "Caffe Settings"
        '
        'CaffeTAA
        '
        Me.CaffeTAA.AutoSize = True
        Me.CaffeTAA.Location = New System.Drawing.Point(301, 75)
        Me.CaffeTAA.Name = "CaffeTAA"
        Me.CaffeTAA.Size = New System.Drawing.Size(47, 17)
        Me.CaffeTAA.TabIndex = 15
        Me.CaffeTAA.Text = "TAA"
        Me.CaffeTAA.UseVisualStyleBackColor = True
        '
        'CaffeProcess
        '
        Me.CaffeProcess.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.CaffeProcess.FormattingEnabled = True
        Me.CaffeProcess.Items.AddRange(New Object() {"CPU", "GPU", "cuDNN"})
        Me.CaffeProcess.Location = New System.Drawing.Point(234, 19)
        Me.CaffeProcess.Name = "CaffeProcess"
        Me.CaffeProcess.Size = New System.Drawing.Size(114, 21)
        Me.CaffeProcess.TabIndex = 14
        '
        'CaffeScale
        '
        Me.CaffeScale.DecimalPlaces = 1
        Me.CaffeScale.Increment = New Decimal(New Integer() {1, 0, 0, 65536})
        Me.CaffeScale.Location = New System.Drawing.Point(60, 72)
        Me.CaffeScale.Maximum = New Decimal(New Integer() {4, 0, 0, 0})
        Me.CaffeScale.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.CaffeScale.Name = "CaffeScale"
        Me.CaffeScale.Size = New System.Drawing.Size(114, 20)
        Me.CaffeScale.TabIndex = 13
        Me.CaffeScale.Value = New Decimal(New Integer() {2, 0, 0, 0})
        '
        'CaffeNoise
        '
        Me.CaffeNoise.Location = New System.Drawing.Point(60, 46)
        Me.CaffeNoise.Maximum = New Decimal(New Integer() {3, 0, 0, 0})
        Me.CaffeNoise.Name = "CaffeNoise"
        Me.CaffeNoise.Size = New System.Drawing.Size(114, 20)
        Me.CaffeNoise.TabIndex = 12
        Me.CaffeNoise.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(180, 22)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(48, 13)
        Me.Label6.TabIndex = 1
        Me.Label6.Text = "Process:"
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(6, 74)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(37, 13)
        Me.Label5.TabIndex = 1
        Me.Label5.Text = "Scale:"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(6, 48)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(37, 13)
        Me.Label4.TabIndex = 1
        Me.Label4.Text = "Noise:"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(6, 22)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(37, 13)
        Me.Label3.TabIndex = 1
        Me.Label3.Text = "Mode:"
        '
        'CaffeMode
        '
        Me.CaffeMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.CaffeMode.FormattingEnabled = True
        Me.CaffeMode.Items.AddRange(New Object() {"Noise", "Scale", "Noise_Scale", "Auto_Scale"})
        Me.CaffeMode.Location = New System.Drawing.Point(60, 19)
        Me.CaffeMode.Name = "CaffeMode"
        Me.CaffeMode.Size = New System.Drawing.Size(114, 21)
        Me.CaffeMode.TabIndex = 11
        '
        'WatchDog
        '
        Me.WatchDog.Interval = 1000
        '
        'WatchDogButton
        '
        Me.WatchDogButton.Location = New System.Drawing.Point(126, 343)
        Me.WatchDogButton.Name = "WatchDogButton"
        Me.WatchDogButton.Size = New System.Drawing.Size(108, 23)
        Me.WatchDogButton.TabIndex = 10
        Me.WatchDogButton.Text = "Running: False"
        Me.WatchDogButton.UseVisualStyleBackColor = True
        '
        'SettingsGroup
        '
        Me.SettingsGroup.Controls.Add(Me.DefringeThresh)
        Me.SettingsGroup.Controls.Add(Me.DefringeCheck)
        Me.SettingsGroup.Controls.Add(Me.UpscaleProgress)
        Me.SettingsGroup.Controls.Add(Me.NumericThreads)
        Me.SettingsGroup.Controls.Add(Me.ThreadComboBox)
        Me.SettingsGroup.Controls.Add(Me.ExeComboBox)
        Me.SettingsGroup.Controls.Add(Me.Label21)
        Me.SettingsGroup.Controls.Add(Me.Label22)
        Me.SettingsGroup.Controls.Add(Me.Label12)
        Me.SettingsGroup.Location = New System.Drawing.Point(12, 166)
        Me.SettingsGroup.Name = "SettingsGroup"
        Me.SettingsGroup.Size = New System.Drawing.Size(222, 171)
        Me.SettingsGroup.TabIndex = 4
        Me.SettingsGroup.TabStop = False
        Me.SettingsGroup.Text = "Program Settings"
        '
        'DefringeThresh
        '
        Me.DefringeThresh.Enabled = False
        Me.DefringeThresh.Location = New System.Drawing.Point(141, 72)
        Me.DefringeThresh.Maximum = New Decimal(New Integer() {255, 0, 0, 0})
        Me.DefringeThresh.Name = "DefringeThresh"
        Me.DefringeThresh.Size = New System.Drawing.Size(75, 20)
        Me.DefringeThresh.TabIndex = 11
        Me.DefringeThresh.Value = New Decimal(New Integer() {215, 0, 0, 0})
        '
        'DefringeCheck
        '
        Me.DefringeCheck.AutoSize = True
        Me.DefringeCheck.Location = New System.Drawing.Point(6, 73)
        Me.DefringeCheck.Name = "DefringeCheck"
        Me.DefringeCheck.Size = New System.Drawing.Size(66, 17)
        Me.DefringeCheck.TabIndex = 10
        Me.DefringeCheck.Text = "Defringe"
        Me.DefringeCheck.UseVisualStyleBackColor = True
        '
        'UpscaleProgress
        '
        Me.UpscaleProgress.Location = New System.Drawing.Point(9, 142)
        Me.UpscaleProgress.Name = "UpscaleProgress"
        Me.UpscaleProgress.Size = New System.Drawing.Size(207, 23)
        Me.UpscaleProgress.TabIndex = 9
        '
        'NumericThreads
        '
        Me.NumericThreads.Location = New System.Drawing.Point(176, 46)
        Me.NumericThreads.Maximum = New Decimal(New Integer() {64, 0, 0, 0})
        Me.NumericThreads.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.NumericThreads.Name = "NumericThreads"
        Me.NumericThreads.Size = New System.Drawing.Size(40, 20)
        Me.NumericThreads.TabIndex = 6
        Me.NumericThreads.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'ThreadComboBox
        '
        Me.ThreadComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ThreadComboBox.FormattingEnabled = True
        Me.ThreadComboBox.Items.AddRange(New Object() {"Single", "Custom", "All", "Max (512) (!!)"})
        Me.ThreadComboBox.Location = New System.Drawing.Point(65, 46)
        Me.ThreadComboBox.Name = "ThreadComboBox"
        Me.ThreadComboBox.Size = New System.Drawing.Size(105, 21)
        Me.ThreadComboBox.TabIndex = 5
        '
        'ExeComboBox
        '
        Me.ExeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ExeComboBox.FormattingEnabled = True
        Me.ExeComboBox.Location = New System.Drawing.Point(65, 19)
        Me.ExeComboBox.Name = "ExeComboBox"
        Me.ExeComboBox.Size = New System.Drawing.Size(151, 21)
        Me.ExeComboBox.TabIndex = 4
        '
        'Label21
        '
        Me.Label21.AutoSize = True
        Me.Label21.Location = New System.Drawing.Point(6, 22)
        Me.Label21.Name = "Label21"
        Me.Label21.Size = New System.Drawing.Size(53, 13)
        Me.Label21.TabIndex = 0
        Me.Label21.Text = "Backend:"
        '
        'Label22
        '
        Me.Label22.AutoSize = True
        Me.Label22.Location = New System.Drawing.Point(78, 74)
        Me.Label22.Name = "Label22"
        Me.Label22.Size = New System.Drawing.Size(57, 13)
        Me.Label22.TabIndex = 0
        Me.Label22.Text = "Threshold:"
        '
        'Label12
        '
        Me.Label12.AutoSize = True
        Me.Label12.Location = New System.Drawing.Point(6, 48)
        Me.Label12.Name = "Label12"
        Me.Label12.Size = New System.Drawing.Size(49, 13)
        Me.Label12.TabIndex = 0
        Me.Label12.Text = "Threads:"
        '
        'CleanupCheckBox
        '
        Me.CleanupCheckBox.AutoSize = True
        Me.CleanupCheckBox.Location = New System.Drawing.Point(9, 57)
        Me.CleanupCheckBox.Name = "CleanupCheckBox"
        Me.CleanupCheckBox.Size = New System.Drawing.Size(108, 17)
        Me.CleanupCheckBox.TabIndex = 7
        Me.CleanupCheckBox.Text = "Delete Input Files"
        Me.CleanupCheckBox.UseVisualStyleBackColor = True
        '
        'RunOnceButton
        '
        Me.RunOnceButton.Location = New System.Drawing.Point(12, 343)
        Me.RunOnceButton.Name = "RunOnceButton"
        Me.RunOnceButton.Size = New System.Drawing.Size(108, 23)
        Me.RunOnceButton.TabIndex = 9
        Me.RunOnceButton.Text = "Upscale Image"
        Me.RunOnceButton.UseVisualStyleBackColor = True
        '
        'VulkanGroup
        '
        Me.VulkanGroup.Controls.Add(Me.VulkanTAA)
        Me.VulkanGroup.Controls.Add(Me.VulkanFormat)
        Me.VulkanGroup.Controls.Add(Me.Label7)
        Me.VulkanGroup.Controls.Add(Me.VulkanScale)
        Me.VulkanGroup.Controls.Add(Me.Label8)
        Me.VulkanGroup.Controls.Add(Me.VulkanNoise)
        Me.VulkanGroup.Controls.Add(Me.Label9)
        Me.VulkanGroup.Location = New System.Drawing.Point(1045, 12)
        Me.VulkanGroup.Name = "VulkanGroup"
        Me.VulkanGroup.Size = New System.Drawing.Size(396, 200)
        Me.VulkanGroup.TabIndex = 11
        Me.VulkanGroup.TabStop = False
        Me.VulkanGroup.Text = "Vulkan Settings"
        '
        'VulkanTAA
        '
        Me.VulkanTAA.AutoSize = True
        Me.VulkanTAA.Location = New System.Drawing.Point(301, 46)
        Me.VulkanTAA.Name = "VulkanTAA"
        Me.VulkanTAA.Size = New System.Drawing.Size(47, 17)
        Me.VulkanTAA.TabIndex = 14
        Me.VulkanTAA.Text = "TAA"
        Me.VulkanTAA.UseVisualStyleBackColor = True
        '
        'VulkanFormat
        '
        Me.VulkanFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.VulkanFormat.FormattingEnabled = True
        Me.VulkanFormat.Items.AddRange(New Object() {"PNG", "JPG", "WEBP"})
        Me.VulkanFormat.Location = New System.Drawing.Point(234, 19)
        Me.VulkanFormat.Name = "VulkanFormat"
        Me.VulkanFormat.Size = New System.Drawing.Size(114, 21)
        Me.VulkanFormat.TabIndex = 13
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(180, 21)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(42, 13)
        Me.Label7.TabIndex = 1
        Me.Label7.Text = "Output:"
        '
        'VulkanScale
        '
        Me.VulkanScale.DecimalPlaces = 1
        Me.VulkanScale.Increment = New Decimal(New Integer() {1, 0, 0, 65536})
        Me.VulkanScale.Location = New System.Drawing.Point(60, 45)
        Me.VulkanScale.Maximum = New Decimal(New Integer() {4, 0, 0, 0})
        Me.VulkanScale.Minimum = New Decimal(New Integer() {2, 0, 0, 0})
        Me.VulkanScale.Name = "VulkanScale"
        Me.VulkanScale.Size = New System.Drawing.Size(114, 20)
        Me.VulkanScale.TabIndex = 12
        Me.VulkanScale.Value = New Decimal(New Integer() {2, 0, 0, 0})
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(6, 21)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(37, 13)
        Me.Label8.TabIndex = 1
        Me.Label8.Text = "Noise:"
        '
        'VulkanNoise
        '
        Me.VulkanNoise.Location = New System.Drawing.Point(60, 19)
        Me.VulkanNoise.Maximum = New Decimal(New Integer() {3, 0, 0, 0})
        Me.VulkanNoise.Minimum = New Decimal(New Integer() {1, 0, 0, -2147483648})
        Me.VulkanNoise.Name = "VulkanNoise"
        Me.VulkanNoise.Size = New System.Drawing.Size(114, 20)
        Me.VulkanNoise.TabIndex = 11
        Me.VulkanNoise.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Location = New System.Drawing.Point(6, 47)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(37, 13)
        Me.Label9.TabIndex = 1
        Me.Label9.Text = "Scale:"
        '
        'WaifuCPPGroup
        '
        Me.WaifuCPPGroup.Controls.Add(Me.WaifuCppGPU)
        Me.WaifuCPPGroup.Controls.Add(Me.WaifuCPPOpenCL)
        Me.WaifuCPPGroup.Controls.Add(Me.WaifuCPPMode)
        Me.WaifuCPPGroup.Controls.Add(Me.WaifuCPPFormat)
        Me.WaifuCPPGroup.Controls.Add(Me.Label10)
        Me.WaifuCPPGroup.Controls.Add(Me.Label14)
        Me.WaifuCPPGroup.Controls.Add(Me.WaifuCPPScale)
        Me.WaifuCPPGroup.Controls.Add(Me.Label11)
        Me.WaifuCPPGroup.Controls.Add(Me.WaifuCPPNoise)
        Me.WaifuCPPGroup.Controls.Add(Me.Label13)
        Me.WaifuCPPGroup.Location = New System.Drawing.Point(643, 218)
        Me.WaifuCPPGroup.Name = "WaifuCPPGroup"
        Me.WaifuCPPGroup.Size = New System.Drawing.Size(396, 200)
        Me.WaifuCPPGroup.TabIndex = 11
        Me.WaifuCPPGroup.TabStop = False
        Me.WaifuCPPGroup.Text = "Waifu2x CPP Settings"
        '
        'WaifuCppGPU
        '
        Me.WaifuCppGPU.AutoSize = True
        Me.WaifuCppGPU.Checked = True
        Me.WaifuCppGPU.CheckState = System.Windows.Forms.CheckState.Checked
        Me.WaifuCppGPU.Location = New System.Drawing.Point(299, 73)
        Me.WaifuCppGPU.Name = "WaifuCppGPU"
        Me.WaifuCppGPU.Size = New System.Drawing.Size(49, 17)
        Me.WaifuCppGPU.TabIndex = 17
        Me.WaifuCppGPU.Text = "GPU"
        Me.WaifuCppGPU.UseVisualStyleBackColor = True
        '
        'WaifuCPPOpenCL
        '
        Me.WaifuCPPOpenCL.AutoSize = True
        Me.WaifuCPPOpenCL.Location = New System.Drawing.Point(183, 73)
        Me.WaifuCPPOpenCL.Name = "WaifuCPPOpenCL"
        Me.WaifuCPPOpenCL.Size = New System.Drawing.Size(95, 17)
        Me.WaifuCPPOpenCL.TabIndex = 15
        Me.WaifuCPPOpenCL.Text = "Force OpenCL"
        Me.WaifuCPPOpenCL.UseVisualStyleBackColor = True
        '
        'WaifuCPPMode
        '
        Me.WaifuCPPMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.WaifuCPPMode.FormattingEnabled = True
        Me.WaifuCPPMode.Items.AddRange(New Object() {"Noise", "Scale", "Noise-Scale"})
        Me.WaifuCPPMode.Location = New System.Drawing.Point(60, 19)
        Me.WaifuCPPMode.Name = "WaifuCPPMode"
        Me.WaifuCPPMode.Size = New System.Drawing.Size(114, 21)
        Me.WaifuCPPMode.TabIndex = 11
        '
        'WaifuCPPFormat
        '
        Me.WaifuCPPFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.WaifuCPPFormat.FormattingEnabled = True
        Me.WaifuCPPFormat.Items.AddRange(New Object() {"PNG", "JPG", "WEBP"})
        Me.WaifuCPPFormat.Location = New System.Drawing.Point(234, 19)
        Me.WaifuCPPFormat.Name = "WaifuCPPFormat"
        Me.WaifuCPPFormat.Size = New System.Drawing.Size(114, 21)
        Me.WaifuCPPFormat.TabIndex = 14
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Location = New System.Drawing.Point(6, 22)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(37, 13)
        Me.Label10.TabIndex = 1
        Me.Label10.Text = "Mode:"
        '
        'Label14
        '
        Me.Label14.AutoSize = True
        Me.Label14.Location = New System.Drawing.Point(180, 22)
        Me.Label14.Name = "Label14"
        Me.Label14.Size = New System.Drawing.Size(42, 13)
        Me.Label14.TabIndex = 1
        Me.Label14.Text = "Output:"
        '
        'WaifuCPPScale
        '
        Me.WaifuCPPScale.DecimalPlaces = 1
        Me.WaifuCPPScale.Increment = New Decimal(New Integer() {1, 0, 0, 65536})
        Me.WaifuCPPScale.Location = New System.Drawing.Point(60, 72)
        Me.WaifuCPPScale.Maximum = New Decimal(New Integer() {8, 0, 0, 0})
        Me.WaifuCPPScale.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.WaifuCPPScale.Name = "WaifuCPPScale"
        Me.WaifuCPPScale.Size = New System.Drawing.Size(114, 20)
        Me.WaifuCPPScale.TabIndex = 13
        Me.WaifuCPPScale.Value = New Decimal(New Integer() {2, 0, 0, 0})
        '
        'Label11
        '
        Me.Label11.AutoSize = True
        Me.Label11.Location = New System.Drawing.Point(6, 48)
        Me.Label11.Name = "Label11"
        Me.Label11.Size = New System.Drawing.Size(37, 13)
        Me.Label11.TabIndex = 1
        Me.Label11.Text = "Noise:"
        '
        'WaifuCPPNoise
        '
        Me.WaifuCPPNoise.Location = New System.Drawing.Point(60, 46)
        Me.WaifuCPPNoise.Maximum = New Decimal(New Integer() {3, 0, 0, 0})
        Me.WaifuCPPNoise.Name = "WaifuCPPNoise"
        Me.WaifuCPPNoise.Size = New System.Drawing.Size(114, 20)
        Me.WaifuCPPNoise.TabIndex = 12
        Me.WaifuCPPNoise.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'Label13
        '
        Me.Label13.AutoSize = True
        Me.Label13.Location = New System.Drawing.Point(6, 74)
        Me.Label13.Name = "Label13"
        Me.Label13.Size = New System.Drawing.Size(37, 13)
        Me.Label13.TabIndex = 1
        Me.Label13.Text = "Scale:"
        '
        'AnimeCPPGroup
        '
        Me.AnimeCPPGroup.Controls.Add(Me.AnimeCppPostFilter)
        Me.AnimeCPPGroup.Controls.Add(Me.AnimeCppPreFilter)
        Me.AnimeCPPGroup.Controls.Add(Me.AnimeCPPCnn)
        Me.AnimeCPPGroup.Controls.Add(Me.AnimeCPPGpu)
        Me.AnimeCPPGroup.Controls.Add(Me.AnimeCppPost)
        Me.AnimeCPPGroup.Controls.Add(Me.AnimeCppPre)
        Me.AnimeCPPGroup.Controls.Add(Me.AnimeCPPScale)
        Me.AnimeCPPGroup.Controls.Add(Me.AnimeCppPostFilters)
        Me.AnimeCPPGroup.Controls.Add(Me.AnimeCppPreFilters)
        Me.AnimeCPPGroup.Controls.Add(Me.Label20)
        Me.AnimeCPPGroup.Location = New System.Drawing.Point(1045, 218)
        Me.AnimeCPPGroup.Name = "AnimeCPPGroup"
        Me.AnimeCPPGroup.Size = New System.Drawing.Size(396, 200)
        Me.AnimeCPPGroup.TabIndex = 11
        Me.AnimeCPPGroup.TabStop = False
        Me.AnimeCPPGroup.Text = "Anime4k CPP Settings"
        '
        'AnimeCppPostFilter
        '
        Me.AnimeCppPostFilter.AutoSize = True
        Me.AnimeCppPostFilter.Location = New System.Drawing.Point(277, 46)
        Me.AnimeCppPostFilter.Name = "AnimeCppPostFilter"
        Me.AnimeCppPostFilter.Size = New System.Drawing.Size(48, 17)
        Me.AnimeCppPostFilter.TabIndex = 22
        Me.AnimeCppPostFilter.Text = "Filter"
        Me.AnimeCppPostFilter.UseVisualStyleBackColor = True
        '
        'AnimeCppPreFilter
        '
        Me.AnimeCppPreFilter.AutoSize = True
        Me.AnimeCppPreFilter.Location = New System.Drawing.Point(98, 46)
        Me.AnimeCppPreFilter.Name = "AnimeCppPreFilter"
        Me.AnimeCppPreFilter.Size = New System.Drawing.Size(48, 17)
        Me.AnimeCppPreFilter.TabIndex = 22
        Me.AnimeCppPreFilter.Text = "Filter"
        Me.AnimeCppPreFilter.UseVisualStyleBackColor = True
        '
        'AnimeCPPCnn
        '
        Me.AnimeCPPCnn.AutoSize = True
        Me.AnimeCPPCnn.Checked = True
        Me.AnimeCPPCnn.CheckState = System.Windows.Forms.CheckState.Checked
        Me.AnimeCPPCnn.Location = New System.Drawing.Point(238, 21)
        Me.AnimeCPPCnn.Name = "AnimeCPPCnn"
        Me.AnimeCPPCnn.Size = New System.Drawing.Size(49, 17)
        Me.AnimeCPPCnn.TabIndex = 19
        Me.AnimeCPPCnn.Text = "CNN"
        Me.AnimeCPPCnn.UseVisualStyleBackColor = True
        '
        'AnimeCPPGpu
        '
        Me.AnimeCPPGpu.AutoSize = True
        Me.AnimeCPPGpu.Checked = True
        Me.AnimeCPPGpu.CheckState = System.Windows.Forms.CheckState.Checked
        Me.AnimeCPPGpu.Location = New System.Drawing.Point(183, 21)
        Me.AnimeCPPGpu.Name = "AnimeCPPGpu"
        Me.AnimeCPPGpu.Size = New System.Drawing.Size(49, 17)
        Me.AnimeCPPGpu.TabIndex = 18
        Me.AnimeCPPGpu.Text = "GPU"
        Me.AnimeCPPGpu.UseVisualStyleBackColor = True
        '
        'AnimeCppPost
        '
        Me.AnimeCppPost.AutoSize = True
        Me.AnimeCppPost.Location = New System.Drawing.Point(183, 46)
        Me.AnimeCppPost.Name = "AnimeCppPost"
        Me.AnimeCppPost.Size = New System.Drawing.Size(88, 17)
        Me.AnimeCppPost.TabIndex = 17
        Me.AnimeCppPost.Text = "Post-Process"
        Me.AnimeCppPost.UseVisualStyleBackColor = True
        '
        'AnimeCppPre
        '
        Me.AnimeCppPre.AutoSize = True
        Me.AnimeCppPre.Location = New System.Drawing.Point(9, 46)
        Me.AnimeCppPre.Name = "AnimeCppPre"
        Me.AnimeCppPre.Size = New System.Drawing.Size(83, 17)
        Me.AnimeCppPre.TabIndex = 16
        Me.AnimeCppPre.Text = "Pre-Process"
        Me.AnimeCppPre.UseVisualStyleBackColor = True
        '
        'AnimeCPPScale
        '
        Me.AnimeCPPScale.DecimalPlaces = 1
        Me.AnimeCPPScale.Increment = New Decimal(New Integer() {1, 0, 0, 65536})
        Me.AnimeCPPScale.Location = New System.Drawing.Point(60, 20)
        Me.AnimeCPPScale.Maximum = New Decimal(New Integer() {8, 0, 0, 0})
        Me.AnimeCPPScale.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.AnimeCPPScale.Name = "AnimeCPPScale"
        Me.AnimeCPPScale.Size = New System.Drawing.Size(114, 20)
        Me.AnimeCPPScale.TabIndex = 15
        Me.AnimeCPPScale.Value = New Decimal(New Integer() {2, 0, 0, 0})
        '
        'AnimeCppPostFilters
        '
        Me.AnimeCppPostFilters.FormattingEnabled = True
        Me.AnimeCppPostFilters.Items.AddRange(New Object() {"Median Blur", "Mean Blur", "CAS Sharpening", "Weak Gaussian Blur", "Gaussian Blur", "Bilateral Filter", "Fast Bilateral Filter"})
        Me.AnimeCppPostFilters.Location = New System.Drawing.Point(183, 69)
        Me.AnimeCppPostFilters.Name = "AnimeCppPostFilters"
        Me.AnimeCppPostFilters.Size = New System.Drawing.Size(130, 109)
        Me.AnimeCppPostFilters.TabIndex = 21
        '
        'AnimeCppPreFilters
        '
        Me.AnimeCppPreFilters.FormattingEnabled = True
        Me.AnimeCppPreFilters.Items.AddRange(New Object() {"Median Blur", "Mean Blur", "CAS Sharpening", "Weak Gaussian Blur", "Gaussian Blur", "Bilateral Filter", "Fast Bilateral Filter"})
        Me.AnimeCppPreFilters.Location = New System.Drawing.Point(9, 69)
        Me.AnimeCppPreFilters.Name = "AnimeCppPreFilters"
        Me.AnimeCppPreFilters.Size = New System.Drawing.Size(130, 109)
        Me.AnimeCppPreFilters.TabIndex = 21
        '
        'Label20
        '
        Me.Label20.AutoSize = True
        Me.Label20.Location = New System.Drawing.Point(6, 22)
        Me.Label20.Name = "Label20"
        Me.Label20.Size = New System.Drawing.Size(37, 13)
        Me.Label20.TabIndex = 1
        Me.Label20.Text = "Scale:"
        '
        'PyGroup
        '
        Me.PyGroup.Controls.Add(Me.PyBatchSize)
        Me.PyGroup.Controls.Add(Me.Label23)
        Me.PyGroup.Controls.Add(Me.PyArguements)
        Me.PyGroup.Controls.Add(Me.PyModel)
        Me.PyGroup.Controls.Add(Me.PyInputFlag)
        Me.PyGroup.Controls.Add(Me.PyScript)
        Me.PyGroup.Controls.Add(Me.PyOutputFlag)
        Me.PyGroup.Controls.Add(Me.Label26)
        Me.PyGroup.Controls.Add(Me.Label28)
        Me.PyGroup.Controls.Add(Me.Label25)
        Me.PyGroup.Controls.Add(Me.Label27)
        Me.PyGroup.Location = New System.Drawing.Point(643, 424)
        Me.PyGroup.Name = "PyGroup"
        Me.PyGroup.Size = New System.Drawing.Size(396, 200)
        Me.PyGroup.TabIndex = 12
        Me.PyGroup.TabStop = False
        Me.PyGroup.Text = "Python"
        '
        'PyBatchSize
        '
        Me.PyBatchSize.Location = New System.Drawing.Point(77, 122)
        Me.PyBatchSize.Maximum = New Decimal(New Integer() {256, 0, 0, 0})
        Me.PyBatchSize.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.PyBatchSize.Name = "PyBatchSize"
        Me.PyBatchSize.Size = New System.Drawing.Size(97, 20)
        Me.PyBatchSize.TabIndex = 5
        Me.PyBatchSize.Value = New Decimal(New Integer() {16, 0, 0, 0})
        '
        'Label23
        '
        Me.Label23.AutoSize = True
        Me.Label23.Location = New System.Drawing.Point(6, 124)
        Me.Label23.Name = "Label23"
        Me.Label23.Size = New System.Drawing.Size(61, 13)
        Me.Label23.TabIndex = 4
        Me.Label23.Text = "Batch Size:"
        '
        'PyArguements
        '
        Me.PyArguements.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells
        Me.PyArguements.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.PyArguements.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.PyFlag, Me.PyValue})
        Me.PyArguements.Location = New System.Drawing.Point(183, 19)
        Me.PyArguements.Name = "PyArguements"
        Me.PyArguements.RowHeadersWidth = 5
        Me.PyArguements.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.PyArguements.Size = New System.Drawing.Size(207, 175)
        Me.PyArguements.TabIndex = 3
        '
        'PyFlag
        '
        Me.PyFlag.HeaderText = "Flag"
        Me.PyFlag.Name = "PyFlag"
        Me.PyFlag.Width = 52
        '
        'PyValue
        '
        Me.PyValue.HeaderText = "Value"
        Me.PyValue.Name = "PyValue"
        Me.PyValue.Width = 59
        '
        'PyModel
        '
        Me.PyModel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.PyModel.FormattingEnabled = True
        Me.PyModel.Location = New System.Drawing.Point(60, 46)
        Me.PyModel.Name = "PyModel"
        Me.PyModel.Size = New System.Drawing.Size(114, 21)
        Me.PyModel.TabIndex = 2
        '
        'PyInputFlag
        '
        Me.PyInputFlag.Location = New System.Drawing.Point(77, 148)
        Me.PyInputFlag.Name = "PyInputFlag"
        Me.PyInputFlag.Size = New System.Drawing.Size(97, 20)
        Me.PyInputFlag.TabIndex = 1
        Me.PyInputFlag.Text = "--input"
        '
        'PyScript
        '
        Me.PyScript.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.PyScript.FormattingEnabled = True
        Me.PyScript.Location = New System.Drawing.Point(60, 19)
        Me.PyScript.Name = "PyScript"
        Me.PyScript.Size = New System.Drawing.Size(114, 21)
        Me.PyScript.TabIndex = 0
        '
        'PyOutputFlag
        '
        Me.PyOutputFlag.Location = New System.Drawing.Point(77, 174)
        Me.PyOutputFlag.Name = "PyOutputFlag"
        Me.PyOutputFlag.Size = New System.Drawing.Size(97, 20)
        Me.PyOutputFlag.TabIndex = 1
        Me.PyOutputFlag.Text = "--output"
        '
        'Label26
        '
        Me.Label26.AutoSize = True
        Me.Label26.Location = New System.Drawing.Point(6, 49)
        Me.Label26.Name = "Label26"
        Me.Label26.Size = New System.Drawing.Size(39, 13)
        Me.Label26.TabIndex = 1
        Me.Label26.Text = "Model:"
        '
        'Label28
        '
        Me.Label28.AutoSize = True
        Me.Label28.Location = New System.Drawing.Point(6, 151)
        Me.Label28.Name = "Label28"
        Me.Label28.Size = New System.Drawing.Size(60, 13)
        Me.Label28.TabIndex = 0
        Me.Label28.Text = "Intput Flag:"
        '
        'Label25
        '
        Me.Label25.AutoSize = True
        Me.Label25.Location = New System.Drawing.Point(6, 22)
        Me.Label25.Name = "Label25"
        Me.Label25.Size = New System.Drawing.Size(37, 13)
        Me.Label25.TabIndex = 1
        Me.Label25.Text = "Script:"
        '
        'Label27
        '
        Me.Label27.AutoSize = True
        Me.Label27.Location = New System.Drawing.Point(6, 177)
        Me.Label27.Name = "Label27"
        Me.Label27.Size = New System.Drawing.Size(65, 13)
        Me.Label27.TabIndex = 0
        Me.Label27.Text = "Output Flag:"
        '
        'WorkHorse
        '
        Me.WorkHorse.WorkerReportsProgress = True
        Me.WorkHorse.WorkerSupportsCancellation = True
        '
        'ChainThumbs
        '
        Me.ChainThumbs.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit
        Me.ChainThumbs.ImageSize = New System.Drawing.Size(64, 64)
        Me.ChainThumbs.TransparentColor = System.Drawing.Color.Transparent
        '
        'ChainGroup
        '
        Me.ChainGroup.Controls.Add(Me.ChainLoad)
        Me.ChainGroup.Controls.Add(Me.ChainSave)
        Me.ChainGroup.Controls.Add(Me.ChainRemove)
        Me.ChainGroup.Controls.Add(Me.ChainAdd)
        Me.ChainGroup.Controls.Add(Me.ChainPreview)
        Me.ChainGroup.Location = New System.Drawing.Point(4, 22)
        Me.ChainGroup.Name = "ChainGroup"
        Me.ChainGroup.Padding = New System.Windows.Forms.Padding(3)
        Me.ChainGroup.Size = New System.Drawing.Size(616, 122)
        Me.ChainGroup.TabIndex = 1
        Me.ChainGroup.Text = "Chaining"
        Me.ChainGroup.UseVisualStyleBackColor = True
        '
        'ChainLoad
        '
        Me.ChainLoad.Location = New System.Drawing.Point(551, 93)
        Me.ChainLoad.Name = "ChainLoad"
        Me.ChainLoad.Size = New System.Drawing.Size(60, 23)
        Me.ChainLoad.TabIndex = 15
        Me.ChainLoad.Text = "Load"
        Me.ChainLoad.UseVisualStyleBackColor = True
        '
        'ChainSave
        '
        Me.ChainSave.Location = New System.Drawing.Point(551, 64)
        Me.ChainSave.Name = "ChainSave"
        Me.ChainSave.Size = New System.Drawing.Size(60, 23)
        Me.ChainSave.TabIndex = 15
        Me.ChainSave.Text = "Save"
        Me.ChainSave.UseVisualStyleBackColor = True
        '
        'ChainRemove
        '
        Me.ChainRemove.Location = New System.Drawing.Point(551, 35)
        Me.ChainRemove.Name = "ChainRemove"
        Me.ChainRemove.Size = New System.Drawing.Size(60, 23)
        Me.ChainRemove.TabIndex = 16
        Me.ChainRemove.Text = "Delete"
        Me.ChainRemove.UseVisualStyleBackColor = True
        '
        'ChainAdd
        '
        Me.ChainAdd.Location = New System.Drawing.Point(551, 6)
        Me.ChainAdd.Name = "ChainAdd"
        Me.ChainAdd.Size = New System.Drawing.Size(60, 23)
        Me.ChainAdd.TabIndex = 15
        Me.ChainAdd.Text = "Add"
        Me.ChainAdd.UseVisualStyleBackColor = True
        '
        'ChainPreview
        '
        Me.ChainPreview.Alignment = System.Windows.Forms.ListViewAlignment.SnapToGrid
        Me.ChainPreview.AllowDrop = True
        Me.ChainPreview.AutoArrange = False
        Me.ChainPreview.ContextMenuStrip = Me.ChainContext
        Me.ChainPreview.GridLines = True
        Me.ChainPreview.HideSelection = False
        Me.ChainPreview.LargeImageList = Me.ChainThumbs
        Me.ChainPreview.Location = New System.Drawing.Point(6, 6)
        Me.ChainPreview.MultiSelect = False
        Me.ChainPreview.Name = "ChainPreview"
        Me.ChainPreview.Scrollable = False
        Me.ChainPreview.Size = New System.Drawing.Size(539, 110)
        Me.ChainPreview.TabIndex = 13
        Me.ChainPreview.UseCompatibleStateImageBehavior = False
        '
        'ChainContext
        '
        Me.ChainContext.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ChainContextEdit, Me.ChainContextDelete})
        Me.ChainContext.Name = "ChainContext"
        Me.ChainContext.Size = New System.Drawing.Size(108, 48)
        '
        'ChainContextEdit
        '
        Me.ChainContextEdit.Name = "ChainContextEdit"
        Me.ChainContextEdit.Size = New System.Drawing.Size(107, 22)
        Me.ChainContextEdit.Text = "&Edit"
        '
        'ChainContextDelete
        '
        Me.ChainContextDelete.Name = "ChainContextDelete"
        Me.ChainContextDelete.Size = New System.Drawing.Size(107, 22)
        Me.ChainContextDelete.Text = "&Delete"
        '
        'PathGroup
        '
        Me.PathGroup.Controls.Add(Me.OutputBrowse)
        Me.PathGroup.Controls.Add(Me.InputTextBox)
        Me.PathGroup.Controls.Add(Me.OutputTextBox)
        Me.PathGroup.Controls.Add(Me.InputBrowse)
        Me.PathGroup.Controls.Add(Me.Label1)
        Me.PathGroup.Controls.Add(Me.Label2)
        Me.PathGroup.Location = New System.Drawing.Point(4, 22)
        Me.PathGroup.Name = "PathGroup"
        Me.PathGroup.Padding = New System.Windows.Forms.Padding(3)
        Me.PathGroup.Size = New System.Drawing.Size(616, 122)
        Me.PathGroup.TabIndex = 0
        Me.PathGroup.Text = "Paths"
        Me.PathGroup.UseVisualStyleBackColor = True
        '
        'OutputBrowse
        '
        Me.OutputBrowse.Location = New System.Drawing.Point(580, 32)
        Me.OutputBrowse.Name = "OutputBrowse"
        Me.OutputBrowse.Size = New System.Drawing.Size(30, 20)
        Me.OutputBrowse.TabIndex = 3
        Me.OutputBrowse.Text = "..."
        Me.OutputBrowse.UseVisualStyleBackColor = True
        '
        'InputTextBox
        '
        Me.InputTextBox.Location = New System.Drawing.Point(88, 6)
        Me.InputTextBox.Name = "InputTextBox"
        Me.InputTextBox.Size = New System.Drawing.Size(486, 20)
        Me.InputTextBox.TabIndex = 0
        '
        'OutputTextBox
        '
        Me.OutputTextBox.Location = New System.Drawing.Point(88, 32)
        Me.OutputTextBox.Name = "OutputTextBox"
        Me.OutputTextBox.Size = New System.Drawing.Size(486, 20)
        Me.OutputTextBox.TabIndex = 2
        '
        'InputBrowse
        '
        Me.InputBrowse.Location = New System.Drawing.Point(580, 6)
        Me.InputBrowse.Name = "InputBrowse"
        Me.InputBrowse.Size = New System.Drawing.Size(30, 20)
        Me.InputBrowse.TabIndex = 1
        Me.InputBrowse.Text = "..."
        Me.InputBrowse.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(6, 9)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(66, 13)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Input Folder:"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(6, 35)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(74, 13)
        Me.Label2.TabIndex = 0
        Me.Label2.Text = "Output Folder:"
        '
        'TabGroup
        '
        Me.TabGroup.Controls.Add(Me.PathGroup)
        Me.TabGroup.Controls.Add(Me.ChainGroup)
        Me.TabGroup.Controls.Add(Me.AdvSettingsGroup)
        Me.TabGroup.Location = New System.Drawing.Point(12, 12)
        Me.TabGroup.Name = "TabGroup"
        Me.TabGroup.SelectedIndex = 0
        Me.TabGroup.Size = New System.Drawing.Size(624, 148)
        Me.TabGroup.TabIndex = 14
        '
        'AdvSettingsGroup
        '
        Me.AdvSettingsGroup.Controls.Add(Me.SeamMargin)
        Me.AdvSettingsGroup.Controls.Add(Me.SeamScale)
        Me.AdvSettingsGroup.Controls.Add(Me.SeamsBox)
        Me.AdvSettingsGroup.Controls.Add(Me.Label18)
        Me.AdvSettingsGroup.Controls.Add(Me.Label17)
        Me.AdvSettingsGroup.Controls.Add(Me.Label16)
        Me.AdvSettingsGroup.Controls.Add(Me.PortableCheckBox)
        Me.AdvSettingsGroup.Controls.Add(Me.Label15)
        Me.AdvSettingsGroup.Controls.Add(Me.ExpertSettingsBox)
        Me.AdvSettingsGroup.Controls.Add(Me.DebugCheckbox)
        Me.AdvSettingsGroup.Controls.Add(Me.CleanupCheckBox)
        Me.AdvSettingsGroup.Location = New System.Drawing.Point(4, 22)
        Me.AdvSettingsGroup.Name = "AdvSettingsGroup"
        Me.AdvSettingsGroup.Padding = New System.Windows.Forms.Padding(3)
        Me.AdvSettingsGroup.Size = New System.Drawing.Size(616, 122)
        Me.AdvSettingsGroup.TabIndex = 2
        Me.AdvSettingsGroup.Text = "Advanced"
        Me.AdvSettingsGroup.UseVisualStyleBackColor = True
        '
        'SeamMargin
        '
        Me.SeamMargin.Location = New System.Drawing.Point(459, 85)
        Me.SeamMargin.Maximum = New Decimal(New Integer() {128, 0, 0, 0})
        Me.SeamMargin.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.SeamMargin.Name = "SeamMargin"
        Me.SeamMargin.Size = New System.Drawing.Size(151, 20)
        Me.SeamMargin.TabIndex = 16
        Me.SeamMargin.Value = New Decimal(New Integer() {16, 0, 0, 0})
        '
        'SeamScale
        '
        Me.SeamScale.Location = New System.Drawing.Point(459, 59)
        Me.SeamScale.Maximum = New Decimal(New Integer() {2048, 0, 0, 0})
        Me.SeamScale.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.SeamScale.Name = "SeamScale"
        Me.SeamScale.Size = New System.Drawing.Size(151, 20)
        Me.SeamScale.TabIndex = 15
        Me.SeamScale.Value = New Decimal(New Integer() {2, 0, 0, 0})
        '
        'SeamsBox
        '
        Me.SeamsBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.SeamsBox.FormattingEnabled = True
        Me.SeamsBox.Items.AddRange(New Object() {"Default (Off)", "Tiled Seamless", "Flipped Seamless"})
        Me.SeamsBox.Location = New System.Drawing.Point(459, 32)
        Me.SeamsBox.Name = "SeamsBox"
        Me.SeamsBox.Size = New System.Drawing.Size(151, 21)
        Me.SeamsBox.TabIndex = 14
        '
        'Label18
        '
        Me.Label18.AutoSize = True
        Me.Label18.Location = New System.Drawing.Point(381, 87)
        Me.Label18.Name = "Label18"
        Me.Label18.Size = New System.Drawing.Size(72, 13)
        Me.Label18.TabIndex = 13
        Me.Label18.Text = "Seam Margin:"
        '
        'Label17
        '
        Me.Label17.AutoSize = True
        Me.Label17.Location = New System.Drawing.Point(381, 61)
        Me.Label17.Name = "Label17"
        Me.Label17.Size = New System.Drawing.Size(67, 13)
        Me.Label17.TabIndex = 13
        Me.Label17.Text = "Seam Scale:"
        '
        'Label16
        '
        Me.Label16.AutoSize = True
        Me.Label16.Location = New System.Drawing.Point(381, 35)
        Me.Label16.Name = "Label16"
        Me.Label16.Size = New System.Drawing.Size(42, 13)
        Me.Label16.TabIndex = 13
        Me.Label16.Text = "Seams:"
        '
        'PortableCheckBox
        '
        Me.PortableCheckBox.AutoSize = True
        Me.PortableCheckBox.Location = New System.Drawing.Point(9, 83)
        Me.PortableCheckBox.Name = "PortableCheckBox"
        Me.PortableCheckBox.Size = New System.Drawing.Size(95, 17)
        Me.PortableCheckBox.TabIndex = 10
        Me.PortableCheckBox.Text = "Portable Mode"
        Me.PortableCheckBox.UseVisualStyleBackColor = True
        '
        'Label15
        '
        Me.Label15.AutoSize = True
        Me.Label15.Location = New System.Drawing.Point(6, 9)
        Me.Label15.Name = "Label15"
        Me.Label15.Size = New System.Drawing.Size(81, 13)
        Me.Label15.TabIndex = 9
        Me.Label15.Text = "Expert Settings:"
        '
        'ExpertSettingsBox
        '
        Me.ExpertSettingsBox.Location = New System.Drawing.Point(93, 6)
        Me.ExpertSettingsBox.Name = "ExpertSettingsBox"
        Me.ExpertSettingsBox.Size = New System.Drawing.Size(517, 20)
        Me.ExpertSettingsBox.TabIndex = 8
        '
        'DebugCheckbox
        '
        Me.DebugCheckbox.AutoSize = True
        Me.DebugCheckbox.Location = New System.Drawing.Point(9, 31)
        Me.DebugCheckbox.Name = "DebugCheckbox"
        Me.DebugCheckbox.Size = New System.Drawing.Size(99, 17)
        Me.DebugCheckbox.TabIndex = 0
        Me.DebugCheckbox.Text = "Output Logging"
        Me.DebugCheckbox.UseVisualStyleBackColor = True
        '
        'DDxGroup
        '
        Me.DDxGroup.Controls.Add(Me.DDxFormatLabel)
        Me.DDxGroup.Controls.Add(Me.DDxFormatListBox)
        Me.DDxGroup.Controls.Add(Me.Dx10Checkbox)
        Me.DDxGroup.Controls.Add(Me.Dx9CheckBox)
        Me.DDxGroup.Controls.Add(Me.FlComboBox)
        Me.DDxGroup.Controls.Add(Me.Label24)
        Me.DDxGroup.Controls.Add(Me.AlphaCheckBox)
        Me.DDxGroup.Controls.Add(Me.PmAlphaCheckBox)
        Me.DDxGroup.Controls.Add(Me.SepAlphaCheckBox)
        Me.DDxGroup.Controls.Add(Me.Label19)
        Me.DDxGroup.Controls.Add(Me.DDxModeBox)
        Me.DDxGroup.Location = New System.Drawing.Point(1045, 424)
        Me.DDxGroup.Name = "DDxGroup"
        Me.DDxGroup.Size = New System.Drawing.Size(396, 200)
        Me.DDxGroup.TabIndex = 16
        Me.DDxGroup.TabStop = False
        Me.DDxGroup.Text = "TexConv Settings"
        '
        'DDxFormatLabel
        '
        Me.DDxFormatLabel.AutoSize = True
        Me.DDxFormatLabel.Location = New System.Drawing.Point(180, 49)
        Me.DDxFormatLabel.Name = "DDxFormatLabel"
        Me.DDxFormatLabel.Size = New System.Drawing.Size(78, 13)
        Me.DDxFormatLabel.TabIndex = 10
        Me.DDxFormatLabel.Text = "Format:  BGRA"
        '
        'DDxFormatListBox
        '
        Me.DDxFormatListBox.FormattingEnabled = True
        Me.DDxFormatListBox.Items.AddRange(New Object() {"R32G32B32A32_FLOAT", "R32G32B32A32_UINT", "R32G32B32A32_SINT", "R32G32B32_FLOAT", "R32G32B32_UINT", "R32G32B32_SINT", "R16G16B16A16_FLOAT", "R16G16B16A16_UNORM", "R16G16B16A16_UINT", "R16G16B16A16_SNORM", "R16G16B16A16_SINT", "R32G32_FLOAT", "R32G32_UINT", "R32G32_SINT", "R10G10B10A2_UNORM", "R10G10B10A2_UINT", "R11G11B10_FLOAT", "R8G8B8A8_UNORM", "R8G8B8A8_UNORM_SRGB", "R8G8B8A8_UINT", "R8G8B8A8_SNORM", "R8G8B8A8_SINT", "R16G16_FLOAT", "R16G16_UNORM", "R16G16_UINT", "R16G16_SNORM", "R16G16_SINT", "R32_FLOAT", "R32_UINT", "R32_SINT", "R8G8_UNORM", "R8G8_UINT", "R8G8_SNORM", "R8G8_SINT", "R16_FLOAT", "R16_UNORM", "R16_UINT", "R16_SNORM", "R16_SINT", "R8_UNORM", "R8_UINT", "R8_SNORM", "R8_SINT", "A8_UNORM", "R9G9B9E5_SHAREDEXP", "R8G8_B8G8_UNORM", "G8R8_G8B8_UNORM", "BC1_UNORM", "BC1_UNORM_SRGB", "BC2_UNORM", "BC2_UNORM_SRGB", "BC3_UNORM", "BC3_UNORM_SRGB", "BC4_UNORM", "BC4_SNORM", "BC5_UNORM", "BC5_SNORM", "B5G6R5_UNORM", "B5G5R5A1_UNORM", "B8G8R8A8_UNORM", "B8G8R8X8_UNORM", "R10G10B10_XR_BIAS_A2_UNORM", "B8G8R8A8_UNORM_SRGB", "B8G8R8X8_UNORM_SRGB", "BC6H_UF16", "BC6H_SF16", "BC7_UNORM", "BC7_UNORM_SRGB", "AYUV", "Y410", "Y416", "YUY2", "Y210", "Y216", "B4G4R4A4_UNORM", "DXT1", "DXT2", "DXT3", "DXT4", "DXT5", "RGBA", "BGRA", "BGR", "FP16", "FP32", "BPTC", "BPTC_FLOAT"})
        Me.DDxFormatListBox.Location = New System.Drawing.Point(9, 46)
        Me.DDxFormatListBox.Name = "DDxFormatListBox"
        Me.DDxFormatListBox.Size = New System.Drawing.Size(165, 147)
        Me.DDxFormatListBox.TabIndex = 9
        '
        'Dx10Checkbox
        '
        Me.Dx10Checkbox.AutoSize = True
        Me.Dx10Checkbox.Location = New System.Drawing.Point(183, 176)
        Me.Dx10Checkbox.Name = "Dx10Checkbox"
        Me.Dx10Checkbox.Size = New System.Drawing.Size(81, 17)
        Me.Dx10Checkbox.TabIndex = 8
        Me.Dx10Checkbox.Text = "Force Dx10"
        Me.Dx10Checkbox.UseVisualStyleBackColor = True
        '
        'Dx9CheckBox
        '
        Me.Dx9CheckBox.AutoSize = True
        Me.Dx9CheckBox.Location = New System.Drawing.Point(183, 153)
        Me.Dx9CheckBox.Name = "Dx9CheckBox"
        Me.Dx9CheckBox.Size = New System.Drawing.Size(75, 17)
        Me.Dx9CheckBox.TabIndex = 7
        Me.Dx9CheckBox.Text = "Force Dx9"
        Me.Dx9CheckBox.UseVisualStyleBackColor = True
        '
        'FlComboBox
        '
        Me.FlComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.FlComboBox.FormattingEnabled = True
        Me.FlComboBox.Items.AddRange(New Object() {"9.1", "9.2", "9.3", "10.0", "10.1", "11.0", "11.1", "12.0", "12.1"})
        Me.FlComboBox.Location = New System.Drawing.Point(261, 19)
        Me.FlComboBox.Name = "FlComboBox"
        Me.FlComboBox.Size = New System.Drawing.Size(87, 21)
        Me.FlComboBox.TabIndex = 6
        '
        'Label24
        '
        Me.Label24.AutoSize = True
        Me.Label24.Location = New System.Drawing.Point(180, 22)
        Me.Label24.Name = "Label24"
        Me.Label24.Size = New System.Drawing.Size(75, 13)
        Me.Label24.TabIndex = 5
        Me.Label24.Text = "Feature Level:"
        '
        'AlphaCheckBox
        '
        Me.AlphaCheckBox.AutoSize = True
        Me.AlphaCheckBox.Location = New System.Drawing.Point(183, 84)
        Me.AlphaCheckBox.Name = "AlphaCheckBox"
        Me.AlphaCheckBox.Size = New System.Drawing.Size(92, 17)
        Me.AlphaCheckBox.TabIndex = 4
        Me.AlphaCheckBox.Text = "Straight Alpha"
        Me.AlphaCheckBox.UseVisualStyleBackColor = True
        '
        'PmAlphaCheckBox
        '
        Me.PmAlphaCheckBox.AutoSize = True
        Me.PmAlphaCheckBox.Location = New System.Drawing.Point(183, 107)
        Me.PmAlphaCheckBox.Name = "PmAlphaCheckBox"
        Me.PmAlphaCheckBox.Size = New System.Drawing.Size(106, 17)
        Me.PmAlphaCheckBox.TabIndex = 3
        Me.PmAlphaCheckBox.Text = "Premultiply Alpha"
        Me.PmAlphaCheckBox.UseVisualStyleBackColor = True
        '
        'SepAlphaCheckBox
        '
        Me.SepAlphaCheckBox.AutoSize = True
        Me.SepAlphaCheckBox.Location = New System.Drawing.Point(183, 130)
        Me.SepAlphaCheckBox.Name = "SepAlphaCheckBox"
        Me.SepAlphaCheckBox.Size = New System.Drawing.Size(99, 17)
        Me.SepAlphaCheckBox.TabIndex = 2
        Me.SepAlphaCheckBox.Text = "Seperate Alpha"
        Me.SepAlphaCheckBox.UseVisualStyleBackColor = True
        '
        'Label19
        '
        Me.Label19.AutoSize = True
        Me.Label19.Location = New System.Drawing.Point(6, 22)
        Me.Label19.Name = "Label19"
        Me.Label19.Size = New System.Drawing.Size(37, 13)
        Me.Label19.TabIndex = 1
        Me.Label19.Text = "Mode:"
        '
        'DDxModeBox
        '
        Me.DDxModeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.DDxModeBox.FormattingEnabled = True
        Me.DDxModeBox.Items.AddRange(New Object() {"DDS Input", "DDS Output"})
        Me.DDxModeBox.Location = New System.Drawing.Point(60, 19)
        Me.DDxModeBox.Name = "DDxModeBox"
        Me.DDxModeBox.Size = New System.Drawing.Size(114, 21)
        Me.DDxModeBox.TabIndex = 0
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1456, 718)
        Me.Controls.Add(Me.DDxGroup)
        Me.Controls.Add(Me.TabGroup)
        Me.Controls.Add(Me.PyGroup)
        Me.Controls.Add(Me.RunOnceButton)
        Me.Controls.Add(Me.AnimeCPPGroup)
        Me.Controls.Add(Me.WaifuCPPGroup)
        Me.Controls.Add(Me.VulkanGroup)
        Me.Controls.Add(Me.SettingsGroup)
        Me.Controls.Add(Me.WatchDogButton)
        Me.Controls.Add(Me.CaffeGroup)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Name = "Form1"
        Me.Text = "AutoCrispy"
        Me.CaffeGroup.ResumeLayout(False)
        Me.CaffeGroup.PerformLayout()
        CType(Me.CaffeScale, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.CaffeNoise, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SettingsGroup.ResumeLayout(False)
        Me.SettingsGroup.PerformLayout()
        CType(Me.DefringeThresh, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.NumericThreads, System.ComponentModel.ISupportInitialize).EndInit()
        Me.VulkanGroup.ResumeLayout(False)
        Me.VulkanGroup.PerformLayout()
        CType(Me.VulkanScale, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.VulkanNoise, System.ComponentModel.ISupportInitialize).EndInit()
        Me.WaifuCPPGroup.ResumeLayout(False)
        Me.WaifuCPPGroup.PerformLayout()
        CType(Me.WaifuCPPScale, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.WaifuCPPNoise, System.ComponentModel.ISupportInitialize).EndInit()
        Me.AnimeCPPGroup.ResumeLayout(False)
        Me.AnimeCPPGroup.PerformLayout()
        CType(Me.AnimeCPPScale, System.ComponentModel.ISupportInitialize).EndInit()
        Me.PyGroup.ResumeLayout(False)
        Me.PyGroup.PerformLayout()
        CType(Me.PyBatchSize, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PyArguements, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ChainGroup.ResumeLayout(False)
        Me.ChainContext.ResumeLayout(False)
        Me.PathGroup.ResumeLayout(False)
        Me.PathGroup.PerformLayout()
        Me.TabGroup.ResumeLayout(False)
        Me.AdvSettingsGroup.ResumeLayout(False)
        Me.AdvSettingsGroup.PerformLayout()
        CType(Me.SeamMargin, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.SeamScale, System.ComponentModel.ISupportInitialize).EndInit()
        Me.DDxGroup.ResumeLayout(False)
        Me.DDxGroup.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents CaffeGroup As GroupBox
    Friend WithEvents WatchDog As Timer
    Friend WithEvents WatchDogButton As Button
    Friend WithEvents SettingsGroup As GroupBox
    Friend WithEvents CleanupCheckBox As CheckBox
    Friend WithEvents ExeComboBox As ComboBox
    Friend WithEvents ThreadComboBox As ComboBox
    Friend WithEvents NumericThreads As NumericUpDown
    Friend WithEvents Label12 As Label
    Friend WithEvents VulkanGroup As GroupBox
    Friend WithEvents WaifuCPPGroup As GroupBox
    Friend WithEvents AnimeCPPGroup As GroupBox
    Friend WithEvents CaffeScale As NumericUpDown
    Friend WithEvents CaffeNoise As NumericUpDown
    Friend WithEvents Label3 As Label
    Friend WithEvents CaffeMode As ComboBox
    Friend WithEvents Label4 As Label
    Friend WithEvents Label5 As Label
    Friend WithEvents CaffeProcess As ComboBox
    Friend WithEvents Label6 As Label
    Friend WithEvents CaffeTAA As CheckBox
    Friend WithEvents VulkanTAA As CheckBox
    Friend WithEvents VulkanFormat As ComboBox
    Friend WithEvents Label7 As Label
    Friend WithEvents VulkanScale As NumericUpDown
    Friend WithEvents Label8 As Label
    Friend WithEvents VulkanNoise As NumericUpDown
    Friend WithEvents Label9 As Label
    Friend WithEvents WaifuCPPMode As ComboBox
    Friend WithEvents WaifuCPPFormat As ComboBox
    Friend WithEvents Label10 As Label
    Friend WithEvents Label14 As Label
    Friend WithEvents WaifuCPPScale As NumericUpDown
    Friend WithEvents Label11 As Label
    Friend WithEvents WaifuCPPNoise As NumericUpDown
    Friend WithEvents Label13 As Label
    Friend WithEvents WaifuCppGPU As CheckBox
    Friend WithEvents WaifuCPPOpenCL As CheckBox
    Friend WithEvents AnimeCppPreFilters As CheckedListBox
    Friend WithEvents AnimeCPPScale As NumericUpDown
    Friend WithEvents Label20 As Label
    Friend WithEvents AnimeCPPCnn As CheckBox
    Friend WithEvents AnimeCPPGpu As CheckBox
    Friend WithEvents AnimeCppPost As CheckBox
    Friend WithEvents AnimeCppPre As CheckBox
    Friend WithEvents RunOnceButton As Button
    Friend WithEvents Label21 As Label
    Friend WithEvents PyGroup As GroupBox
    Friend WithEvents PyScript As ComboBox
    Friend WithEvents Label25 As Label
    Friend WithEvents PyModel As ComboBox
    Friend WithEvents Label26 As Label
    Friend WithEvents PyInputFlag As TextBox
    Friend WithEvents PyOutputFlag As TextBox
    Friend WithEvents Label28 As Label
    Friend WithEvents Label27 As Label
    Friend WithEvents PyArguements As DataGridView
    Friend WithEvents PyFlag As DataGridViewTextBoxColumn
    Friend WithEvents PyValue As DataGridViewTextBoxColumn
    Friend WithEvents UpscaleProgress As ProgressBar
    Friend WithEvents WorkHorse As System.ComponentModel.BackgroundWorker
    Friend WithEvents DefringeThresh As NumericUpDown
    Friend WithEvents DefringeCheck As CheckBox
    Friend WithEvents Label22 As Label
    Friend WithEvents ChainThumbs As ImageList
    Friend WithEvents ChainGroup As TabPage
    Friend WithEvents ChainLoad As Button
    Friend WithEvents ChainSave As Button
    Friend WithEvents ChainRemove As Button
    Friend WithEvents ChainAdd As Button
    Friend WithEvents ChainPreview As ListView
    Friend WithEvents PathGroup As TabPage
    Friend WithEvents OutputBrowse As Button
    Friend WithEvents InputTextBox As TextBox
    Friend WithEvents OutputTextBox As TextBox
    Friend WithEvents InputBrowse As Button
    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents TabGroup As TabControl
    Friend WithEvents ChainContext As ContextMenuStrip
    Friend WithEvents ChainContextEdit As ToolStripMenuItem
    Friend WithEvents ChainContextDelete As ToolStripMenuItem
    Friend WithEvents AdvSettingsGroup As TabPage
    Friend WithEvents DebugCheckbox As CheckBox
    Friend WithEvents PyBatchSize As NumericUpDown
    Friend WithEvents Label23 As Label
    Friend WithEvents AnimeCppPostFilters As CheckedListBox
    Friend WithEvents AnimeCppPostFilter As CheckBox
    Friend WithEvents AnimeCppPreFilter As CheckBox
    Friend WithEvents Label15 As Label
    Friend WithEvents ExpertSettingsBox As TextBox
    Friend WithEvents PortableCheckBox As CheckBox
    Friend WithEvents SeamScale As NumericUpDown
    Friend WithEvents SeamsBox As ComboBox
    Friend WithEvents Label17 As Label
    Friend WithEvents Label16 As Label
    Friend WithEvents SeamMargin As NumericUpDown
    Friend WithEvents Label18 As Label
    Friend WithEvents DDxGroup As GroupBox
    Friend WithEvents DDxFormatLabel As Label
    Friend WithEvents DDxFormatListBox As ListBox
    Friend WithEvents Dx10Checkbox As CheckBox
    Friend WithEvents Dx9CheckBox As CheckBox
    Friend WithEvents FlComboBox As ComboBox
    Friend WithEvents Label24 As Label
    Friend WithEvents AlphaCheckBox As CheckBox
    Friend WithEvents PmAlphaCheckBox As CheckBox
    Friend WithEvents SepAlphaCheckBox As CheckBox
    Friend WithEvents Label19 As Label
    Friend WithEvents DDxModeBox As ComboBox
End Class
