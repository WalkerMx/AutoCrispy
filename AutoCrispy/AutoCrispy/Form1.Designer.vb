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
        Me.PS2Check = New System.Windows.Forms.CheckBox()
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
        Me.PyArguements = New System.Windows.Forms.DataGridView()
        Me.PyFlag = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.PyValue = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.PyModel = New System.Windows.Forms.ComboBox()
        Me.Label26 = New System.Windows.Forms.Label()
        Me.WorkHorse = New System.ComponentModel.BackgroundWorker()
        Me.ChainGroup = New System.Windows.Forms.TabPage()
        Me.ChainPreview = New System.Windows.Forms.PictureBox()
        Me.ChainContext = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.ChainContextEdit = New System.Windows.Forms.ToolStripMenuItem()
        Me.ChainContextDelete = New System.Windows.Forms.ToolStripMenuItem()
        Me.ChainLoad = New System.Windows.Forms.Button()
        Me.ChainSave = New System.Windows.Forms.Button()
        Me.ChainRemove = New System.Windows.Forms.Button()
        Me.ChainAdd = New System.Windows.Forms.Button()
        Me.PathGroup = New System.Windows.Forms.TabPage()
        Me.ExeBrowse = New System.Windows.Forms.Button()
        Me.OutputBrowse = New System.Windows.Forms.Button()
        Me.InputTextBox = New System.Windows.Forms.TextBox()
        Me.ExeTextBox = New System.Windows.Forms.TextBox()
        Me.OutputTextBox = New System.Windows.Forms.TextBox()
        Me.InputBrowse = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label23 = New System.Windows.Forms.Label()
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
        Me.Label29 = New System.Windows.Forms.Label()
        Me.DDxConvFormat = New System.Windows.Forms.ComboBox()
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
        Me.xBRZGroup = New System.Windows.Forms.GroupBox()
        Me.Label30 = New System.Windows.Forms.Label()
        Me.xBRZScale = New System.Windows.Forms.NumericUpDown()
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
        CType(Me.PyArguements, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ChainGroup.SuspendLayout()
        CType(Me.ChainPreview, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ChainContext.SuspendLayout()
        Me.PathGroup.SuspendLayout()
        Me.TabGroup.SuspendLayout()
        Me.AdvSettingsGroup.SuspendLayout()
        CType(Me.SeamMargin, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.SeamScale, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.DDxGroup.SuspendLayout()
        Me.xBRZGroup.SuspendLayout()
        CType(Me.xBRZScale, System.ComponentModel.ISupportInitialize).BeginInit()
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
        Me.CaffeGroup.Location = New System.Drawing.Point(964, 18)
        Me.CaffeGroup.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.CaffeGroup.Name = "CaffeGroup"
        Me.CaffeGroup.Padding = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.CaffeGroup.Size = New System.Drawing.Size(594, 308)
        Me.CaffeGroup.TabIndex = 11
        Me.CaffeGroup.TabStop = False
        Me.CaffeGroup.Text = "Caffe Settings"
        '
        'CaffeTAA
        '
        Me.CaffeTAA.AutoSize = True
        Me.CaffeTAA.Location = New System.Drawing.Point(452, 115)
        Me.CaffeTAA.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.CaffeTAA.Name = "CaffeTAA"
        Me.CaffeTAA.Size = New System.Drawing.Size(66, 24)
        Me.CaffeTAA.TabIndex = 15
        Me.CaffeTAA.Text = "TAA"
        Me.CaffeTAA.UseVisualStyleBackColor = True
        '
        'CaffeProcess
        '
        Me.CaffeProcess.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.CaffeProcess.FormattingEnabled = True
        Me.CaffeProcess.Items.AddRange(New Object() {"CPU", "GPU", "cuDNN"})
        Me.CaffeProcess.Location = New System.Drawing.Point(351, 29)
        Me.CaffeProcess.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.CaffeProcess.Name = "CaffeProcess"
        Me.CaffeProcess.Size = New System.Drawing.Size(169, 28)
        Me.CaffeProcess.TabIndex = 14
        '
        'CaffeScale
        '
        Me.CaffeScale.DecimalPlaces = 1
        Me.CaffeScale.Increment = New Decimal(New Integer() {1, 0, 0, 65536})
        Me.CaffeScale.Location = New System.Drawing.Point(90, 111)
        Me.CaffeScale.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.CaffeScale.Maximum = New Decimal(New Integer() {4, 0, 0, 0})
        Me.CaffeScale.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.CaffeScale.Name = "CaffeScale"
        Me.CaffeScale.Size = New System.Drawing.Size(171, 26)
        Me.CaffeScale.TabIndex = 13
        Me.CaffeScale.Value = New Decimal(New Integer() {2, 0, 0, 0})
        '
        'CaffeNoise
        '
        Me.CaffeNoise.Location = New System.Drawing.Point(90, 71)
        Me.CaffeNoise.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.CaffeNoise.Maximum = New Decimal(New Integer() {3, 0, 0, 0})
        Me.CaffeNoise.Name = "CaffeNoise"
        Me.CaffeNoise.Size = New System.Drawing.Size(171, 26)
        Me.CaffeNoise.TabIndex = 12
        Me.CaffeNoise.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(270, 34)
        Me.Label6.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(70, 20)
        Me.Label6.TabIndex = 1
        Me.Label6.Text = "Process:"
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(9, 114)
        Me.Label5.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(53, 20)
        Me.Label5.TabIndex = 1
        Me.Label5.Text = "Scale:"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(9, 74)
        Me.Label4.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(53, 20)
        Me.Label4.TabIndex = 1
        Me.Label4.Text = "Noise:"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(9, 34)
        Me.Label3.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(53, 20)
        Me.Label3.TabIndex = 1
        Me.Label3.Text = "Mode:"
        '
        'CaffeMode
        '
        Me.CaffeMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.CaffeMode.FormattingEnabled = True
        Me.CaffeMode.Items.AddRange(New Object() {"Noise", "Scale", "Noise_Scale", "Auto_Scale"})
        Me.CaffeMode.Location = New System.Drawing.Point(90, 29)
        Me.CaffeMode.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.CaffeMode.Name = "CaffeMode"
        Me.CaffeMode.Size = New System.Drawing.Size(169, 28)
        Me.CaffeMode.TabIndex = 11
        '
        'WatchDog
        '
        Me.WatchDog.Interval = 1000
        '
        'WatchDogButton
        '
        Me.WatchDogButton.Location = New System.Drawing.Point(189, 528)
        Me.WatchDogButton.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.WatchDogButton.Name = "WatchDogButton"
        Me.WatchDogButton.Size = New System.Drawing.Size(162, 35)
        Me.WatchDogButton.TabIndex = 10
        Me.WatchDogButton.Text = "Running: False"
        Me.WatchDogButton.UseVisualStyleBackColor = True
        '
        'SettingsGroup
        '
        Me.SettingsGroup.Controls.Add(Me.PS2Check)
        Me.SettingsGroup.Controls.Add(Me.DefringeThresh)
        Me.SettingsGroup.Controls.Add(Me.DefringeCheck)
        Me.SettingsGroup.Controls.Add(Me.UpscaleProgress)
        Me.SettingsGroup.Controls.Add(Me.NumericThreads)
        Me.SettingsGroup.Controls.Add(Me.ThreadComboBox)
        Me.SettingsGroup.Controls.Add(Me.ExeComboBox)
        Me.SettingsGroup.Controls.Add(Me.Label21)
        Me.SettingsGroup.Controls.Add(Me.Label22)
        Me.SettingsGroup.Controls.Add(Me.Label12)
        Me.SettingsGroup.Location = New System.Drawing.Point(18, 255)
        Me.SettingsGroup.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.SettingsGroup.Name = "SettingsGroup"
        Me.SettingsGroup.Padding = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.SettingsGroup.Size = New System.Drawing.Size(333, 263)
        Me.SettingsGroup.TabIndex = 4
        Me.SettingsGroup.TabStop = False
        Me.SettingsGroup.Text = "Program Settings"
        '
        'PS2Check
        '
        Me.PS2Check.AutoSize = True
        Me.PS2Check.Location = New System.Drawing.Point(9, 150)
        Me.PS2Check.Name = "PS2Check"
        Me.PS2Check.Size = New System.Drawing.Size(134, 24)
        Me.PS2Check.TabIndex = 12
        Me.PS2Check.Text = "Fix PS2 Alpha"
        Me.PS2Check.UseVisualStyleBackColor = True
        '
        'DefringeThresh
        '
        Me.DefringeThresh.Enabled = False
        Me.DefringeThresh.Location = New System.Drawing.Point(212, 111)
        Me.DefringeThresh.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.DefringeThresh.Maximum = New Decimal(New Integer() {255, 0, 0, 0})
        Me.DefringeThresh.Name = "DefringeThresh"
        Me.DefringeThresh.Size = New System.Drawing.Size(112, 26)
        Me.DefringeThresh.TabIndex = 11
        Me.DefringeThresh.Value = New Decimal(New Integer() {215, 0, 0, 0})
        '
        'DefringeCheck
        '
        Me.DefringeCheck.AutoSize = True
        Me.DefringeCheck.Location = New System.Drawing.Point(9, 112)
        Me.DefringeCheck.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.DefringeCheck.Name = "DefringeCheck"
        Me.DefringeCheck.Size = New System.Drawing.Size(96, 24)
        Me.DefringeCheck.TabIndex = 10
        Me.DefringeCheck.Text = "Defringe"
        Me.DefringeCheck.UseVisualStyleBackColor = True
        '
        'UpscaleProgress
        '
        Me.UpscaleProgress.Location = New System.Drawing.Point(14, 218)
        Me.UpscaleProgress.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.UpscaleProgress.Name = "UpscaleProgress"
        Me.UpscaleProgress.Size = New System.Drawing.Size(310, 35)
        Me.UpscaleProgress.TabIndex = 9
        '
        'NumericThreads
        '
        Me.NumericThreads.Location = New System.Drawing.Point(264, 71)
        Me.NumericThreads.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.NumericThreads.Maximum = New Decimal(New Integer() {64, 0, 0, 0})
        Me.NumericThreads.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.NumericThreads.Name = "NumericThreads"
        Me.NumericThreads.Size = New System.Drawing.Size(60, 26)
        Me.NumericThreads.TabIndex = 6
        Me.NumericThreads.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'ThreadComboBox
        '
        Me.ThreadComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ThreadComboBox.FormattingEnabled = True
        Me.ThreadComboBox.Items.AddRange(New Object() {"Single", "Custom", "All", "Max (512) (!!)"})
        Me.ThreadComboBox.Location = New System.Drawing.Point(98, 71)
        Me.ThreadComboBox.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.ThreadComboBox.Name = "ThreadComboBox"
        Me.ThreadComboBox.Size = New System.Drawing.Size(156, 28)
        Me.ThreadComboBox.TabIndex = 5
        '
        'ExeComboBox
        '
        Me.ExeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ExeComboBox.FormattingEnabled = True
        Me.ExeComboBox.Location = New System.Drawing.Point(98, 29)
        Me.ExeComboBox.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.ExeComboBox.Name = "ExeComboBox"
        Me.ExeComboBox.Size = New System.Drawing.Size(224, 28)
        Me.ExeComboBox.TabIndex = 4
        '
        'Label21
        '
        Me.Label21.AutoSize = True
        Me.Label21.Location = New System.Drawing.Point(9, 34)
        Me.Label21.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label21.Name = "Label21"
        Me.Label21.Size = New System.Drawing.Size(76, 20)
        Me.Label21.TabIndex = 0
        Me.Label21.Text = "Backend:"
        '
        'Label22
        '
        Me.Label22.AutoSize = True
        Me.Label22.Location = New System.Drawing.Point(117, 114)
        Me.Label22.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label22.Name = "Label22"
        Me.Label22.Size = New System.Drawing.Size(83, 20)
        Me.Label22.TabIndex = 0
        Me.Label22.Text = "Threshold:"
        '
        'Label12
        '
        Me.Label12.AutoSize = True
        Me.Label12.Location = New System.Drawing.Point(9, 74)
        Me.Label12.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label12.Name = "Label12"
        Me.Label12.Size = New System.Drawing.Size(71, 20)
        Me.Label12.TabIndex = 0
        Me.Label12.Text = "Threads:"
        '
        'CleanupCheckBox
        '
        Me.CleanupCheckBox.AutoSize = True
        Me.CleanupCheckBox.Location = New System.Drawing.Point(14, 88)
        Me.CleanupCheckBox.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.CleanupCheckBox.Name = "CleanupCheckBox"
        Me.CleanupCheckBox.Size = New System.Drawing.Size(160, 24)
        Me.CleanupCheckBox.TabIndex = 7
        Me.CleanupCheckBox.Text = "Delete Input Files"
        Me.CleanupCheckBox.UseVisualStyleBackColor = True
        '
        'RunOnceButton
        '
        Me.RunOnceButton.Location = New System.Drawing.Point(18, 528)
        Me.RunOnceButton.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.RunOnceButton.Name = "RunOnceButton"
        Me.RunOnceButton.Size = New System.Drawing.Size(162, 35)
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
        Me.VulkanGroup.Location = New System.Drawing.Point(1568, 18)
        Me.VulkanGroup.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.VulkanGroup.Name = "VulkanGroup"
        Me.VulkanGroup.Padding = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.VulkanGroup.Size = New System.Drawing.Size(594, 308)
        Me.VulkanGroup.TabIndex = 11
        Me.VulkanGroup.TabStop = False
        Me.VulkanGroup.Text = "Vulkan Settings"
        '
        'VulkanTAA
        '
        Me.VulkanTAA.AutoSize = True
        Me.VulkanTAA.Location = New System.Drawing.Point(452, 71)
        Me.VulkanTAA.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.VulkanTAA.Name = "VulkanTAA"
        Me.VulkanTAA.Size = New System.Drawing.Size(66, 24)
        Me.VulkanTAA.TabIndex = 14
        Me.VulkanTAA.Text = "TAA"
        Me.VulkanTAA.UseVisualStyleBackColor = True
        '
        'VulkanFormat
        '
        Me.VulkanFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.VulkanFormat.FormattingEnabled = True
        Me.VulkanFormat.Items.AddRange(New Object() {"PNG", "JPG", "WEBP"})
        Me.VulkanFormat.Location = New System.Drawing.Point(351, 29)
        Me.VulkanFormat.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.VulkanFormat.Name = "VulkanFormat"
        Me.VulkanFormat.Size = New System.Drawing.Size(169, 28)
        Me.VulkanFormat.TabIndex = 13
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(270, 32)
        Me.Label7.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(62, 20)
        Me.Label7.TabIndex = 1
        Me.Label7.Text = "Output:"
        '
        'VulkanScale
        '
        Me.VulkanScale.DecimalPlaces = 1
        Me.VulkanScale.Increment = New Decimal(New Integer() {1, 0, 0, 65536})
        Me.VulkanScale.Location = New System.Drawing.Point(90, 69)
        Me.VulkanScale.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.VulkanScale.Maximum = New Decimal(New Integer() {4, 0, 0, 0})
        Me.VulkanScale.Minimum = New Decimal(New Integer() {2, 0, 0, 0})
        Me.VulkanScale.Name = "VulkanScale"
        Me.VulkanScale.Size = New System.Drawing.Size(171, 26)
        Me.VulkanScale.TabIndex = 12
        Me.VulkanScale.Value = New Decimal(New Integer() {2, 0, 0, 0})
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(9, 32)
        Me.Label8.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(53, 20)
        Me.Label8.TabIndex = 1
        Me.Label8.Text = "Noise:"
        '
        'VulkanNoise
        '
        Me.VulkanNoise.Location = New System.Drawing.Point(90, 29)
        Me.VulkanNoise.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.VulkanNoise.Maximum = New Decimal(New Integer() {3, 0, 0, 0})
        Me.VulkanNoise.Minimum = New Decimal(New Integer() {1, 0, 0, -2147483648})
        Me.VulkanNoise.Name = "VulkanNoise"
        Me.VulkanNoise.Size = New System.Drawing.Size(171, 26)
        Me.VulkanNoise.TabIndex = 11
        Me.VulkanNoise.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Location = New System.Drawing.Point(9, 72)
        Me.Label9.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(53, 20)
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
        Me.WaifuCPPGroup.Location = New System.Drawing.Point(964, 335)
        Me.WaifuCPPGroup.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.WaifuCPPGroup.Name = "WaifuCPPGroup"
        Me.WaifuCPPGroup.Padding = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.WaifuCPPGroup.Size = New System.Drawing.Size(594, 308)
        Me.WaifuCPPGroup.TabIndex = 11
        Me.WaifuCPPGroup.TabStop = False
        Me.WaifuCPPGroup.Text = "Waifu2x CPP Settings"
        '
        'WaifuCppGPU
        '
        Me.WaifuCppGPU.AutoSize = True
        Me.WaifuCppGPU.Checked = True
        Me.WaifuCppGPU.CheckState = System.Windows.Forms.CheckState.Checked
        Me.WaifuCppGPU.Location = New System.Drawing.Point(448, 112)
        Me.WaifuCppGPU.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.WaifuCppGPU.Name = "WaifuCppGPU"
        Me.WaifuCppGPU.Size = New System.Drawing.Size(70, 24)
        Me.WaifuCppGPU.TabIndex = 17
        Me.WaifuCppGPU.Text = "GPU"
        Me.WaifuCppGPU.UseVisualStyleBackColor = True
        '
        'WaifuCPPOpenCL
        '
        Me.WaifuCPPOpenCL.AutoSize = True
        Me.WaifuCPPOpenCL.Location = New System.Drawing.Point(274, 112)
        Me.WaifuCPPOpenCL.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.WaifuCPPOpenCL.Name = "WaifuCPPOpenCL"
        Me.WaifuCPPOpenCL.Size = New System.Drawing.Size(139, 24)
        Me.WaifuCPPOpenCL.TabIndex = 15
        Me.WaifuCPPOpenCL.Text = "Force OpenCL"
        Me.WaifuCPPOpenCL.UseVisualStyleBackColor = True
        '
        'WaifuCPPMode
        '
        Me.WaifuCPPMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.WaifuCPPMode.FormattingEnabled = True
        Me.WaifuCPPMode.Items.AddRange(New Object() {"Noise", "Scale", "Noise-Scale"})
        Me.WaifuCPPMode.Location = New System.Drawing.Point(90, 29)
        Me.WaifuCPPMode.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.WaifuCPPMode.Name = "WaifuCPPMode"
        Me.WaifuCPPMode.Size = New System.Drawing.Size(169, 28)
        Me.WaifuCPPMode.TabIndex = 11
        '
        'WaifuCPPFormat
        '
        Me.WaifuCPPFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.WaifuCPPFormat.FormattingEnabled = True
        Me.WaifuCPPFormat.Items.AddRange(New Object() {"PNG", "JPG", "WEBP"})
        Me.WaifuCPPFormat.Location = New System.Drawing.Point(351, 29)
        Me.WaifuCPPFormat.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.WaifuCPPFormat.Name = "WaifuCPPFormat"
        Me.WaifuCPPFormat.Size = New System.Drawing.Size(169, 28)
        Me.WaifuCPPFormat.TabIndex = 14
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Location = New System.Drawing.Point(9, 34)
        Me.Label10.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(53, 20)
        Me.Label10.TabIndex = 1
        Me.Label10.Text = "Mode:"
        '
        'Label14
        '
        Me.Label14.AutoSize = True
        Me.Label14.Location = New System.Drawing.Point(270, 34)
        Me.Label14.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label14.Name = "Label14"
        Me.Label14.Size = New System.Drawing.Size(62, 20)
        Me.Label14.TabIndex = 1
        Me.Label14.Text = "Output:"
        '
        'WaifuCPPScale
        '
        Me.WaifuCPPScale.DecimalPlaces = 1
        Me.WaifuCPPScale.Increment = New Decimal(New Integer() {1, 0, 0, 65536})
        Me.WaifuCPPScale.Location = New System.Drawing.Point(90, 111)
        Me.WaifuCPPScale.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.WaifuCPPScale.Maximum = New Decimal(New Integer() {8, 0, 0, 0})
        Me.WaifuCPPScale.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.WaifuCPPScale.Name = "WaifuCPPScale"
        Me.WaifuCPPScale.Size = New System.Drawing.Size(171, 26)
        Me.WaifuCPPScale.TabIndex = 13
        Me.WaifuCPPScale.Value = New Decimal(New Integer() {2, 0, 0, 0})
        '
        'Label11
        '
        Me.Label11.AutoSize = True
        Me.Label11.Location = New System.Drawing.Point(9, 74)
        Me.Label11.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label11.Name = "Label11"
        Me.Label11.Size = New System.Drawing.Size(53, 20)
        Me.Label11.TabIndex = 1
        Me.Label11.Text = "Noise:"
        '
        'WaifuCPPNoise
        '
        Me.WaifuCPPNoise.Location = New System.Drawing.Point(90, 71)
        Me.WaifuCPPNoise.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.WaifuCPPNoise.Maximum = New Decimal(New Integer() {3, 0, 0, 0})
        Me.WaifuCPPNoise.Name = "WaifuCPPNoise"
        Me.WaifuCPPNoise.Size = New System.Drawing.Size(171, 26)
        Me.WaifuCPPNoise.TabIndex = 12
        Me.WaifuCPPNoise.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'Label13
        '
        Me.Label13.AutoSize = True
        Me.Label13.Location = New System.Drawing.Point(9, 114)
        Me.Label13.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label13.Name = "Label13"
        Me.Label13.Size = New System.Drawing.Size(53, 20)
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
        Me.AnimeCPPGroup.Location = New System.Drawing.Point(1568, 335)
        Me.AnimeCPPGroup.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.AnimeCPPGroup.Name = "AnimeCPPGroup"
        Me.AnimeCPPGroup.Padding = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.AnimeCPPGroup.Size = New System.Drawing.Size(594, 308)
        Me.AnimeCPPGroup.TabIndex = 11
        Me.AnimeCPPGroup.TabStop = False
        Me.AnimeCPPGroup.Text = "Anime4k CPP Settings"
        '
        'AnimeCppPostFilter
        '
        Me.AnimeCppPostFilter.AutoSize = True
        Me.AnimeCppPostFilter.Location = New System.Drawing.Point(416, 71)
        Me.AnimeCppPostFilter.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.AnimeCppPostFilter.Name = "AnimeCppPostFilter"
        Me.AnimeCppPostFilter.Size = New System.Drawing.Size(70, 24)
        Me.AnimeCppPostFilter.TabIndex = 22
        Me.AnimeCppPostFilter.Text = "Filter"
        Me.AnimeCppPostFilter.UseVisualStyleBackColor = True
        '
        'AnimeCppPreFilter
        '
        Me.AnimeCppPreFilter.AutoSize = True
        Me.AnimeCppPreFilter.Location = New System.Drawing.Point(147, 71)
        Me.AnimeCppPreFilter.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.AnimeCppPreFilter.Name = "AnimeCppPreFilter"
        Me.AnimeCppPreFilter.Size = New System.Drawing.Size(70, 24)
        Me.AnimeCppPreFilter.TabIndex = 22
        Me.AnimeCppPreFilter.Text = "Filter"
        Me.AnimeCppPreFilter.UseVisualStyleBackColor = True
        '
        'AnimeCPPCnn
        '
        Me.AnimeCPPCnn.AutoSize = True
        Me.AnimeCPPCnn.Checked = True
        Me.AnimeCPPCnn.CheckState = System.Windows.Forms.CheckState.Checked
        Me.AnimeCPPCnn.Location = New System.Drawing.Point(357, 32)
        Me.AnimeCPPCnn.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.AnimeCPPCnn.Name = "AnimeCPPCnn"
        Me.AnimeCPPCnn.Size = New System.Drawing.Size(68, 24)
        Me.AnimeCPPCnn.TabIndex = 19
        Me.AnimeCPPCnn.Text = "CNN"
        Me.AnimeCPPCnn.UseVisualStyleBackColor = True
        '
        'AnimeCPPGpu
        '
        Me.AnimeCPPGpu.AutoSize = True
        Me.AnimeCPPGpu.Checked = True
        Me.AnimeCPPGpu.CheckState = System.Windows.Forms.CheckState.Checked
        Me.AnimeCPPGpu.Location = New System.Drawing.Point(274, 32)
        Me.AnimeCPPGpu.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.AnimeCPPGpu.Name = "AnimeCPPGpu"
        Me.AnimeCPPGpu.Size = New System.Drawing.Size(70, 24)
        Me.AnimeCPPGpu.TabIndex = 18
        Me.AnimeCPPGpu.Text = "GPU"
        Me.AnimeCPPGpu.UseVisualStyleBackColor = True
        '
        'AnimeCppPost
        '
        Me.AnimeCppPost.AutoSize = True
        Me.AnimeCppPost.Location = New System.Drawing.Point(274, 71)
        Me.AnimeCppPost.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.AnimeCppPost.Name = "AnimeCppPost"
        Me.AnimeCppPost.Size = New System.Drawing.Size(129, 24)
        Me.AnimeCppPost.TabIndex = 17
        Me.AnimeCppPost.Text = "Post-Process"
        Me.AnimeCppPost.UseVisualStyleBackColor = True
        '
        'AnimeCppPre
        '
        Me.AnimeCppPre.AutoSize = True
        Me.AnimeCppPre.Location = New System.Drawing.Point(14, 71)
        Me.AnimeCppPre.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.AnimeCppPre.Name = "AnimeCppPre"
        Me.AnimeCppPre.Size = New System.Drawing.Size(121, 24)
        Me.AnimeCppPre.TabIndex = 16
        Me.AnimeCppPre.Text = "Pre-Process"
        Me.AnimeCppPre.UseVisualStyleBackColor = True
        '
        'AnimeCPPScale
        '
        Me.AnimeCPPScale.DecimalPlaces = 1
        Me.AnimeCPPScale.Increment = New Decimal(New Integer() {1, 0, 0, 65536})
        Me.AnimeCPPScale.Location = New System.Drawing.Point(90, 31)
        Me.AnimeCPPScale.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.AnimeCPPScale.Maximum = New Decimal(New Integer() {8, 0, 0, 0})
        Me.AnimeCPPScale.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.AnimeCPPScale.Name = "AnimeCPPScale"
        Me.AnimeCPPScale.Size = New System.Drawing.Size(171, 26)
        Me.AnimeCPPScale.TabIndex = 15
        Me.AnimeCPPScale.Value = New Decimal(New Integer() {2, 0, 0, 0})
        '
        'AnimeCppPostFilters
        '
        Me.AnimeCppPostFilters.FormattingEnabled = True
        Me.AnimeCppPostFilters.Items.AddRange(New Object() {"Median Blur", "Mean Blur", "CAS Sharpening", "Weak Gaussian Blur", "Gaussian Blur", "Bilateral Filter", "Fast Bilateral Filter"})
        Me.AnimeCppPostFilters.Location = New System.Drawing.Point(274, 106)
        Me.AnimeCppPostFilters.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.AnimeCppPostFilters.Name = "AnimeCppPostFilters"
        Me.AnimeCppPostFilters.Size = New System.Drawing.Size(193, 165)
        Me.AnimeCppPostFilters.TabIndex = 21
        '
        'AnimeCppPreFilters
        '
        Me.AnimeCppPreFilters.FormattingEnabled = True
        Me.AnimeCppPreFilters.Items.AddRange(New Object() {"Median Blur", "Mean Blur", "CAS Sharpening", "Weak Gaussian Blur", "Gaussian Blur", "Bilateral Filter", "Fast Bilateral Filter"})
        Me.AnimeCppPreFilters.Location = New System.Drawing.Point(14, 106)
        Me.AnimeCppPreFilters.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.AnimeCppPreFilters.Name = "AnimeCppPreFilters"
        Me.AnimeCppPreFilters.Size = New System.Drawing.Size(193, 165)
        Me.AnimeCppPreFilters.TabIndex = 21
        '
        'Label20
        '
        Me.Label20.AutoSize = True
        Me.Label20.Location = New System.Drawing.Point(9, 34)
        Me.Label20.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label20.Name = "Label20"
        Me.Label20.Size = New System.Drawing.Size(53, 20)
        Me.Label20.TabIndex = 1
        Me.Label20.Text = "Scale:"
        '
        'PyGroup
        '
        Me.PyGroup.Controls.Add(Me.PyArguements)
        Me.PyGroup.Controls.Add(Me.PyModel)
        Me.PyGroup.Controls.Add(Me.Label26)
        Me.PyGroup.Location = New System.Drawing.Point(964, 652)
        Me.PyGroup.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.PyGroup.Name = "PyGroup"
        Me.PyGroup.Padding = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.PyGroup.Size = New System.Drawing.Size(594, 308)
        Me.PyGroup.TabIndex = 12
        Me.PyGroup.TabStop = False
        Me.PyGroup.Text = "Python"
        '
        'PyArguements
        '
        Me.PyArguements.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells
        Me.PyArguements.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.PyArguements.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.PyFlag, Me.PyValue})
        Me.PyArguements.Location = New System.Drawing.Point(274, 29)
        Me.PyArguements.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.PyArguements.Name = "PyArguements"
        Me.PyArguements.RowHeadersWidth = 5
        Me.PyArguements.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.PyArguements.Size = New System.Drawing.Size(310, 269)
        Me.PyArguements.TabIndex = 3
        '
        'PyFlag
        '
        Me.PyFlag.HeaderText = "Flag"
        Me.PyFlag.MinimumWidth = 8
        Me.PyFlag.Name = "PyFlag"
        Me.PyFlag.Width = 76
        '
        'PyValue
        '
        Me.PyValue.HeaderText = "Value"
        Me.PyValue.MinimumWidth = 8
        Me.PyValue.Name = "PyValue"
        Me.PyValue.Width = 86
        '
        'PyModel
        '
        Me.PyModel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.PyModel.FormattingEnabled = True
        Me.PyModel.Location = New System.Drawing.Point(90, 31)
        Me.PyModel.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.PyModel.Name = "PyModel"
        Me.PyModel.Size = New System.Drawing.Size(169, 28)
        Me.PyModel.TabIndex = 2
        '
        'Label26
        '
        Me.Label26.AutoSize = True
        Me.Label26.Location = New System.Drawing.Point(9, 35)
        Me.Label26.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label26.Name = "Label26"
        Me.Label26.Size = New System.Drawing.Size(56, 20)
        Me.Label26.TabIndex = 1
        Me.Label26.Text = "Model:"
        '
        'WorkHorse
        '
        Me.WorkHorse.WorkerReportsProgress = True
        Me.WorkHorse.WorkerSupportsCancellation = True
        '
        'ChainGroup
        '
        Me.ChainGroup.Controls.Add(Me.ChainPreview)
        Me.ChainGroup.Controls.Add(Me.ChainLoad)
        Me.ChainGroup.Controls.Add(Me.ChainSave)
        Me.ChainGroup.Controls.Add(Me.ChainRemove)
        Me.ChainGroup.Controls.Add(Me.ChainAdd)
        Me.ChainGroup.Location = New System.Drawing.Point(4, 29)
        Me.ChainGroup.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.ChainGroup.Name = "ChainGroup"
        Me.ChainGroup.Padding = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.ChainGroup.Size = New System.Drawing.Size(928, 195)
        Me.ChainGroup.TabIndex = 1
        Me.ChainGroup.Text = "Chaining"
        Me.ChainGroup.UseVisualStyleBackColor = True
        '
        'ChainPreview
        '
        Me.ChainPreview.ContextMenuStrip = Me.ChainContext
        Me.ChainPreview.Location = New System.Drawing.Point(7, 8)
        Me.ChainPreview.Name = "ChainPreview"
        Me.ChainPreview.Size = New System.Drawing.Size(812, 179)
        Me.ChainPreview.TabIndex = 17
        Me.ChainPreview.TabStop = False
        '
        'ChainContext
        '
        Me.ChainContext.ImageScalingSize = New System.Drawing.Size(24, 24)
        Me.ChainContext.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ChainContextEdit, Me.ChainContextDelete})
        Me.ChainContext.Name = "ChainContext"
        Me.ChainContext.Size = New System.Drawing.Size(135, 68)
        '
        'ChainContextEdit
        '
        Me.ChainContextEdit.Name = "ChainContextEdit"
        Me.ChainContextEdit.Size = New System.Drawing.Size(134, 32)
        Me.ChainContextEdit.Text = "&Edit"
        '
        'ChainContextDelete
        '
        Me.ChainContextDelete.Name = "ChainContextDelete"
        Me.ChainContextDelete.Size = New System.Drawing.Size(134, 32)
        Me.ChainContextDelete.Text = "&Delete"
        '
        'ChainLoad
        '
        Me.ChainLoad.Location = New System.Drawing.Point(826, 143)
        Me.ChainLoad.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.ChainLoad.Name = "ChainLoad"
        Me.ChainLoad.Size = New System.Drawing.Size(90, 35)
        Me.ChainLoad.TabIndex = 15
        Me.ChainLoad.Text = "Load"
        Me.ChainLoad.UseVisualStyleBackColor = True
        '
        'ChainSave
        '
        Me.ChainSave.Location = New System.Drawing.Point(826, 98)
        Me.ChainSave.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.ChainSave.Name = "ChainSave"
        Me.ChainSave.Size = New System.Drawing.Size(90, 35)
        Me.ChainSave.TabIndex = 15
        Me.ChainSave.Text = "Save"
        Me.ChainSave.UseVisualStyleBackColor = True
        '
        'ChainRemove
        '
        Me.ChainRemove.Location = New System.Drawing.Point(826, 54)
        Me.ChainRemove.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.ChainRemove.Name = "ChainRemove"
        Me.ChainRemove.Size = New System.Drawing.Size(90, 35)
        Me.ChainRemove.TabIndex = 16
        Me.ChainRemove.Text = "Delete"
        Me.ChainRemove.UseVisualStyleBackColor = True
        '
        'ChainAdd
        '
        Me.ChainAdd.Location = New System.Drawing.Point(826, 9)
        Me.ChainAdd.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.ChainAdd.Name = "ChainAdd"
        Me.ChainAdd.Size = New System.Drawing.Size(90, 35)
        Me.ChainAdd.TabIndex = 15
        Me.ChainAdd.Text = "Add"
        Me.ChainAdd.UseVisualStyleBackColor = True
        '
        'PathGroup
        '
        Me.PathGroup.Controls.Add(Me.ExeBrowse)
        Me.PathGroup.Controls.Add(Me.OutputBrowse)
        Me.PathGroup.Controls.Add(Me.InputTextBox)
        Me.PathGroup.Controls.Add(Me.ExeTextBox)
        Me.PathGroup.Controls.Add(Me.OutputTextBox)
        Me.PathGroup.Controls.Add(Me.InputBrowse)
        Me.PathGroup.Controls.Add(Me.Label1)
        Me.PathGroup.Controls.Add(Me.Label23)
        Me.PathGroup.Controls.Add(Me.Label2)
        Me.PathGroup.Location = New System.Drawing.Point(4, 29)
        Me.PathGroup.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.PathGroup.Name = "PathGroup"
        Me.PathGroup.Padding = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.PathGroup.Size = New System.Drawing.Size(928, 195)
        Me.PathGroup.TabIndex = 0
        Me.PathGroup.Text = "Paths"
        Me.PathGroup.UseVisualStyleBackColor = True
        '
        'ExeBrowse
        '
        Me.ExeBrowse.Location = New System.Drawing.Point(870, 89)
        Me.ExeBrowse.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.ExeBrowse.Name = "ExeBrowse"
        Me.ExeBrowse.Size = New System.Drawing.Size(45, 31)
        Me.ExeBrowse.TabIndex = 5
        Me.ExeBrowse.Text = "..."
        Me.ExeBrowse.UseVisualStyleBackColor = True
        '
        'OutputBrowse
        '
        Me.OutputBrowse.Location = New System.Drawing.Point(870, 49)
        Me.OutputBrowse.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.OutputBrowse.Name = "OutputBrowse"
        Me.OutputBrowse.Size = New System.Drawing.Size(45, 31)
        Me.OutputBrowse.TabIndex = 3
        Me.OutputBrowse.Text = "..."
        Me.OutputBrowse.UseVisualStyleBackColor = True
        '
        'InputTextBox
        '
        Me.InputTextBox.Location = New System.Drawing.Point(146, 9)
        Me.InputTextBox.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.InputTextBox.Name = "InputTextBox"
        Me.InputTextBox.Size = New System.Drawing.Size(714, 26)
        Me.InputTextBox.TabIndex = 0
        '
        'ExeTextBox
        '
        Me.ExeTextBox.Location = New System.Drawing.Point(146, 89)
        Me.ExeTextBox.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.ExeTextBox.Name = "ExeTextBox"
        Me.ExeTextBox.Size = New System.Drawing.Size(714, 26)
        Me.ExeTextBox.TabIndex = 4
        '
        'OutputTextBox
        '
        Me.OutputTextBox.Location = New System.Drawing.Point(146, 49)
        Me.OutputTextBox.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.OutputTextBox.Name = "OutputTextBox"
        Me.OutputTextBox.Size = New System.Drawing.Size(714, 26)
        Me.OutputTextBox.TabIndex = 2
        '
        'InputBrowse
        '
        Me.InputBrowse.Location = New System.Drawing.Point(870, 9)
        Me.InputBrowse.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.InputBrowse.Name = "InputBrowse"
        Me.InputBrowse.Size = New System.Drawing.Size(45, 31)
        Me.InputBrowse.TabIndex = 1
        Me.InputBrowse.Text = "..."
        Me.InputBrowse.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(9, 14)
        Me.Label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(99, 20)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Input Folder:"
        '
        'Label23
        '
        Me.Label23.AutoSize = True
        Me.Label23.Location = New System.Drawing.Point(9, 95)
        Me.Label23.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label23.Name = "Label23"
        Me.Label23.Size = New System.Drawing.Size(125, 20)
        Me.Label23.TabIndex = 0
        Me.Label23.Text = "Backend Folder:"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(9, 54)
        Me.Label2.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(111, 20)
        Me.Label2.TabIndex = 0
        Me.Label2.Text = "Output Folder:"
        '
        'TabGroup
        '
        Me.TabGroup.Controls.Add(Me.PathGroup)
        Me.TabGroup.Controls.Add(Me.ChainGroup)
        Me.TabGroup.Controls.Add(Me.AdvSettingsGroup)
        Me.TabGroup.Location = New System.Drawing.Point(18, 18)
        Me.TabGroup.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.TabGroup.Name = "TabGroup"
        Me.TabGroup.SelectedIndex = 0
        Me.TabGroup.Size = New System.Drawing.Size(936, 228)
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
        Me.AdvSettingsGroup.Location = New System.Drawing.Point(4, 29)
        Me.AdvSettingsGroup.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.AdvSettingsGroup.Name = "AdvSettingsGroup"
        Me.AdvSettingsGroup.Padding = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.AdvSettingsGroup.Size = New System.Drawing.Size(928, 195)
        Me.AdvSettingsGroup.TabIndex = 2
        Me.AdvSettingsGroup.Text = "Advanced"
        Me.AdvSettingsGroup.UseVisualStyleBackColor = True
        '
        'SeamMargin
        '
        Me.SeamMargin.Location = New System.Drawing.Point(688, 131)
        Me.SeamMargin.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.SeamMargin.Maximum = New Decimal(New Integer() {128, 0, 0, 0})
        Me.SeamMargin.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.SeamMargin.Name = "SeamMargin"
        Me.SeamMargin.Size = New System.Drawing.Size(226, 26)
        Me.SeamMargin.TabIndex = 16
        Me.SeamMargin.Value = New Decimal(New Integer() {16, 0, 0, 0})
        '
        'SeamScale
        '
        Me.SeamScale.Location = New System.Drawing.Point(688, 91)
        Me.SeamScale.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.SeamScale.Maximum = New Decimal(New Integer() {2048, 0, 0, 0})
        Me.SeamScale.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.SeamScale.Name = "SeamScale"
        Me.SeamScale.Size = New System.Drawing.Size(226, 26)
        Me.SeamScale.TabIndex = 15
        Me.SeamScale.Value = New Decimal(New Integer() {2, 0, 0, 0})
        '
        'SeamsBox
        '
        Me.SeamsBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.SeamsBox.FormattingEnabled = True
        Me.SeamsBox.Items.AddRange(New Object() {"Default (Off)", "Tiled Seamless", "Flipped Seamless"})
        Me.SeamsBox.Location = New System.Drawing.Point(688, 49)
        Me.SeamsBox.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.SeamsBox.Name = "SeamsBox"
        Me.SeamsBox.Size = New System.Drawing.Size(224, 28)
        Me.SeamsBox.TabIndex = 14
        '
        'Label18
        '
        Me.Label18.AutoSize = True
        Me.Label18.Location = New System.Drawing.Point(572, 134)
        Me.Label18.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label18.Name = "Label18"
        Me.Label18.Size = New System.Drawing.Size(107, 20)
        Me.Label18.TabIndex = 13
        Me.Label18.Text = "Seam Margin:"
        '
        'Label17
        '
        Me.Label17.AutoSize = True
        Me.Label17.Location = New System.Drawing.Point(572, 94)
        Me.Label17.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label17.Name = "Label17"
        Me.Label17.Size = New System.Drawing.Size(99, 20)
        Me.Label17.TabIndex = 13
        Me.Label17.Text = "Seam Scale:"
        '
        'Label16
        '
        Me.Label16.AutoSize = True
        Me.Label16.Location = New System.Drawing.Point(572, 54)
        Me.Label16.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label16.Name = "Label16"
        Me.Label16.Size = New System.Drawing.Size(63, 20)
        Me.Label16.TabIndex = 13
        Me.Label16.Text = "Seams:"
        '
        'PortableCheckBox
        '
        Me.PortableCheckBox.AutoSize = True
        Me.PortableCheckBox.Location = New System.Drawing.Point(14, 128)
        Me.PortableCheckBox.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.PortableCheckBox.Name = "PortableCheckBox"
        Me.PortableCheckBox.Size = New System.Drawing.Size(138, 24)
        Me.PortableCheckBox.TabIndex = 10
        Me.PortableCheckBox.Text = "Portable Mode"
        Me.PortableCheckBox.UseVisualStyleBackColor = True
        '
        'Label15
        '
        Me.Label15.AutoSize = True
        Me.Label15.Location = New System.Drawing.Point(9, 14)
        Me.Label15.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label15.Name = "Label15"
        Me.Label15.Size = New System.Drawing.Size(122, 20)
        Me.Label15.TabIndex = 9
        Me.Label15.Text = "Expert Settings:"
        '
        'ExpertSettingsBox
        '
        Me.ExpertSettingsBox.Location = New System.Drawing.Point(140, 9)
        Me.ExpertSettingsBox.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.ExpertSettingsBox.Name = "ExpertSettingsBox"
        Me.ExpertSettingsBox.Size = New System.Drawing.Size(774, 26)
        Me.ExpertSettingsBox.TabIndex = 8
        '
        'DebugCheckbox
        '
        Me.DebugCheckbox.AutoSize = True
        Me.DebugCheckbox.Location = New System.Drawing.Point(14, 48)
        Me.DebugCheckbox.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.DebugCheckbox.Name = "DebugCheckbox"
        Me.DebugCheckbox.Size = New System.Drawing.Size(145, 24)
        Me.DebugCheckbox.TabIndex = 0
        Me.DebugCheckbox.Text = "Output Logging"
        Me.DebugCheckbox.UseVisualStyleBackColor = True
        '
        'DDxGroup
        '
        Me.DDxGroup.Controls.Add(Me.Label29)
        Me.DDxGroup.Controls.Add(Me.DDxConvFormat)
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
        Me.DDxGroup.Location = New System.Drawing.Point(1568, 652)
        Me.DDxGroup.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.DDxGroup.Name = "DDxGroup"
        Me.DDxGroup.Padding = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.DDxGroup.Size = New System.Drawing.Size(594, 308)
        Me.DDxGroup.TabIndex = 16
        Me.DDxGroup.TabStop = False
        Me.DDxGroup.Text = "TexConv Settings"
        '
        'Label29
        '
        Me.Label29.AutoSize = True
        Me.Label29.Location = New System.Drawing.Point(270, 34)
        Me.Label29.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label29.Name = "Label29"
        Me.Label29.Size = New System.Drawing.Size(86, 20)
        Me.Label29.TabIndex = 12
        Me.Label29.Text = "Convert to:"
        '
        'DDxConvFormat
        '
        Me.DDxConvFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.DDxConvFormat.FormattingEnabled = True
        Me.DDxConvFormat.Items.AddRange(New Object() {"BMP", "JPG", "PNG", "TGA", "HDR", "TIF", "WDP", "HDP", "JXR", "PPM", "PFM"})
        Me.DDxConvFormat.Location = New System.Drawing.Point(392, 29)
        Me.DDxConvFormat.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.DDxConvFormat.Name = "DDxConvFormat"
        Me.DDxConvFormat.Size = New System.Drawing.Size(128, 28)
        Me.DDxConvFormat.TabIndex = 11
        '
        'DDxFormatLabel
        '
        Me.DDxFormatLabel.AutoSize = True
        Me.DDxFormatLabel.Location = New System.Drawing.Point(270, 125)
        Me.DDxFormatLabel.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.DDxFormatLabel.Name = "DDxFormatLabel"
        Me.DDxFormatLabel.Size = New System.Drawing.Size(119, 20)
        Me.DDxFormatLabel.TabIndex = 10
        Me.DDxFormatLabel.Text = "Format:  BGRA"
        '
        'DDxFormatListBox
        '
        Me.DDxFormatListBox.FormattingEnabled = True
        Me.DDxFormatListBox.ItemHeight = 20
        Me.DDxFormatListBox.Items.AddRange(New Object() {"R32G32B32A32_FLOAT", "R32G32B32A32_UINT", "R32G32B32A32_SINT", "R32G32B32_FLOAT", "R32G32B32_UINT", "R32G32B32_SINT", "R16G16B16A16_FLOAT", "R16G16B16A16_UNORM", "R16G16B16A16_UINT", "R16G16B16A16_SNORM", "R16G16B16A16_SINT", "R32G32_FLOAT", "R32G32_UINT", "R32G32_SINT", "R10G10B10A2_UNORM", "R10G10B10A2_UINT", "R11G11B10_FLOAT", "R8G8B8A8_UNORM", "R8G8B8A8_UNORM_SRGB", "R8G8B8A8_UINT", "R8G8B8A8_SNORM", "R8G8B8A8_SINT", "R16G16_FLOAT", "R16G16_UNORM", "R16G16_UINT", "R16G16_SNORM", "R16G16_SINT", "R32_FLOAT", "R32_UINT", "R32_SINT", "R8G8_UNORM", "R8G8_UINT", "R8G8_SNORM", "R8G8_SINT", "R16_FLOAT", "R16_UNORM", "R16_UINT", "R16_SNORM", "R16_SINT", "R8_UNORM", "R8_UINT", "R8_SNORM", "R8_SINT", "A8_UNORM", "R9G9B9E5_SHAREDEXP", "R8G8_B8G8_UNORM", "G8R8_G8B8_UNORM", "BC1_UNORM", "BC1_UNORM_SRGB", "BC2_UNORM", "BC2_UNORM_SRGB", "BC3_UNORM", "BC3_UNORM_SRGB", "BC4_UNORM", "BC4_SNORM", "BC5_UNORM", "BC5_SNORM", "B5G6R5_UNORM", "B5G5R5A1_UNORM", "B8G8R8A8_UNORM", "B8G8R8X8_UNORM", "R10G10B10_XR_BIAS_A2_UNORM", "B8G8R8A8_UNORM_SRGB", "B8G8R8X8_UNORM_SRGB", "BC6H_UF16", "BC6H_SF16", "BC7_UNORM", "BC7_UNORM_SRGB", "AYUV", "Y410", "Y416", "YUY2", "Y210", "Y216", "B4G4R4A4_UNORM", "DXT1", "DXT2", "DXT3", "DXT4", "DXT5", "RGBA", "BGRA", "BGR", "FP16", "FP32", "BPTC", "BPTC_FLOAT"})
        Me.DDxFormatListBox.Location = New System.Drawing.Point(14, 71)
        Me.DDxFormatListBox.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.DDxFormatListBox.Name = "DDxFormatListBox"
        Me.DDxFormatListBox.Size = New System.Drawing.Size(246, 224)
        Me.DDxFormatListBox.TabIndex = 9
        '
        'Dx10Checkbox
        '
        Me.Dx10Checkbox.AutoSize = True
        Me.Dx10Checkbox.Location = New System.Drawing.Point(452, 235)
        Me.Dx10Checkbox.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.Dx10Checkbox.Name = "Dx10Checkbox"
        Me.Dx10Checkbox.Size = New System.Drawing.Size(117, 24)
        Me.Dx10Checkbox.TabIndex = 8
        Me.Dx10Checkbox.Text = "Force Dx10"
        Me.Dx10Checkbox.UseVisualStyleBackColor = True
        '
        'Dx9CheckBox
        '
        Me.Dx9CheckBox.AutoSize = True
        Me.Dx9CheckBox.Location = New System.Drawing.Point(452, 200)
        Me.Dx9CheckBox.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.Dx9CheckBox.Name = "Dx9CheckBox"
        Me.Dx9CheckBox.Size = New System.Drawing.Size(108, 24)
        Me.Dx9CheckBox.TabIndex = 7
        Me.Dx9CheckBox.Text = "Force Dx9"
        Me.Dx9CheckBox.UseVisualStyleBackColor = True
        '
        'FlComboBox
        '
        Me.FlComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.FlComboBox.FormattingEnabled = True
        Me.FlComboBox.Items.AddRange(New Object() {"9.1", "9.2", "9.3", "10.0", "10.1", "11.0", "11.1", "12.0", "12.1"})
        Me.FlComboBox.Location = New System.Drawing.Point(392, 71)
        Me.FlComboBox.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.FlComboBox.Name = "FlComboBox"
        Me.FlComboBox.Size = New System.Drawing.Size(128, 28)
        Me.FlComboBox.TabIndex = 6
        '
        'Label24
        '
        Me.Label24.AutoSize = True
        Me.Label24.Location = New System.Drawing.Point(270, 75)
        Me.Label24.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label24.Name = "Label24"
        Me.Label24.Size = New System.Drawing.Size(110, 20)
        Me.Label24.TabIndex = 5
        Me.Label24.Text = "Feature Level:"
        '
        'AlphaCheckBox
        '
        Me.AlphaCheckBox.AutoSize = True
        Me.AlphaCheckBox.Location = New System.Drawing.Point(274, 200)
        Me.AlphaCheckBox.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.AlphaCheckBox.Name = "AlphaCheckBox"
        Me.AlphaCheckBox.Size = New System.Drawing.Size(136, 24)
        Me.AlphaCheckBox.TabIndex = 4
        Me.AlphaCheckBox.Text = "Straight Alpha"
        Me.AlphaCheckBox.UseVisualStyleBackColor = True
        '
        'PmAlphaCheckBox
        '
        Me.PmAlphaCheckBox.AutoSize = True
        Me.PmAlphaCheckBox.Location = New System.Drawing.Point(274, 235)
        Me.PmAlphaCheckBox.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.PmAlphaCheckBox.Name = "PmAlphaCheckBox"
        Me.PmAlphaCheckBox.Size = New System.Drawing.Size(156, 24)
        Me.PmAlphaCheckBox.TabIndex = 3
        Me.PmAlphaCheckBox.Text = "Premultiply Alpha"
        Me.PmAlphaCheckBox.UseVisualStyleBackColor = True
        '
        'SepAlphaCheckBox
        '
        Me.SepAlphaCheckBox.AutoSize = True
        Me.SepAlphaCheckBox.Location = New System.Drawing.Point(274, 271)
        Me.SepAlphaCheckBox.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.SepAlphaCheckBox.Name = "SepAlphaCheckBox"
        Me.SepAlphaCheckBox.Size = New System.Drawing.Size(146, 24)
        Me.SepAlphaCheckBox.TabIndex = 2
        Me.SepAlphaCheckBox.Text = "Seperate Alpha"
        Me.SepAlphaCheckBox.UseVisualStyleBackColor = True
        '
        'Label19
        '
        Me.Label19.AutoSize = True
        Me.Label19.Location = New System.Drawing.Point(9, 34)
        Me.Label19.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label19.Name = "Label19"
        Me.Label19.Size = New System.Drawing.Size(53, 20)
        Me.Label19.TabIndex = 1
        Me.Label19.Text = "Mode:"
        '
        'DDxModeBox
        '
        Me.DDxModeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.DDxModeBox.FormattingEnabled = True
        Me.DDxModeBox.Items.AddRange(New Object() {"DDS Input", "DDS Output"})
        Me.DDxModeBox.Location = New System.Drawing.Point(90, 29)
        Me.DDxModeBox.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.DDxModeBox.Name = "DDxModeBox"
        Me.DDxModeBox.Size = New System.Drawing.Size(169, 28)
        Me.DDxModeBox.TabIndex = 0
        '
        'xBRZGroup
        '
        Me.xBRZGroup.Controls.Add(Me.Label30)
        Me.xBRZGroup.Controls.Add(Me.xBRZScale)
        Me.xBRZGroup.Location = New System.Drawing.Point(363, 652)
        Me.xBRZGroup.Name = "xBRZGroup"
        Me.xBRZGroup.Size = New System.Drawing.Size(594, 308)
        Me.xBRZGroup.TabIndex = 17
        Me.xBRZGroup.TabStop = False
        Me.xBRZGroup.Text = "xBRZ"
        '
        'Label30
        '
        Me.Label30.AutoSize = True
        Me.Label30.Location = New System.Drawing.Point(7, 32)
        Me.Label30.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label30.Name = "Label30"
        Me.Label30.Size = New System.Drawing.Size(53, 20)
        Me.Label30.TabIndex = 1
        Me.Label30.Text = "Scale:"
        '
        'xBRZScale
        '
        Me.xBRZScale.DecimalPlaces = 1
        Me.xBRZScale.Location = New System.Drawing.Point(88, 29)
        Me.xBRZScale.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.xBRZScale.Maximum = New Decimal(New Integer() {6, 0, 0, 0})
        Me.xBRZScale.Minimum = New Decimal(New Integer() {2, 0, 0, 0})
        Me.xBRZScale.Name = "xBRZScale"
        Me.xBRZScale.Size = New System.Drawing.Size(171, 26)
        Me.xBRZScale.TabIndex = 13
        Me.xBRZScale.Value = New Decimal(New Integer() {2, 0, 0, 0})
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(9.0!, 20.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(2184, 978)
        Me.Controls.Add(Me.xBRZGroup)
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
        Me.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
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
        CType(Me.PyArguements, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ChainGroup.ResumeLayout(False)
        CType(Me.ChainPreview, System.ComponentModel.ISupportInitialize).EndInit()
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
        Me.xBRZGroup.ResumeLayout(False)
        Me.xBRZGroup.PerformLayout()
        CType(Me.xBRZScale, System.ComponentModel.ISupportInitialize).EndInit()
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
    Friend WithEvents PyModel As ComboBox
    Friend WithEvents Label26 As Label
    Friend WithEvents PyArguements As DataGridView
    Friend WithEvents PyFlag As DataGridViewTextBoxColumn
    Friend WithEvents PyValue As DataGridViewTextBoxColumn
    Friend WithEvents UpscaleProgress As ProgressBar
    Friend WithEvents WorkHorse As System.ComponentModel.BackgroundWorker
    Friend WithEvents DefringeThresh As NumericUpDown
    Friend WithEvents DefringeCheck As CheckBox
    Friend WithEvents Label22 As Label
    Friend WithEvents ChainGroup As TabPage
    Friend WithEvents ChainLoad As Button
    Friend WithEvents ChainSave As Button
    Friend WithEvents ChainRemove As Button
    Friend WithEvents ChainAdd As Button
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
    Friend WithEvents Label29 As Label
    Friend WithEvents DDxConvFormat As ComboBox
    Friend WithEvents ExeBrowse As Button
    Friend WithEvents ExeTextBox As TextBox
    Friend WithEvents Label23 As Label
    Friend WithEvents xBRZGroup As GroupBox
    Friend WithEvents Label30 As Label
    Friend WithEvents xBRZScale As NumericUpDown
    Friend WithEvents PS2Check As CheckBox
    Friend WithEvents ChainPreview As PictureBox
End Class
