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
        Me.PathGroup = New System.Windows.Forms.GroupBox()
        Me.OutputBrowse = New System.Windows.Forms.Button()
        Me.InputBrowse = New System.Windows.Forms.Button()
        Me.OutputTextBox = New System.Windows.Forms.TextBox()
        Me.InputTextBox = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
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
        Me.UpscaleProgress = New System.Windows.Forms.ProgressBar()
        Me.NumericThreads = New System.Windows.Forms.NumericUpDown()
        Me.ThreadComboBox = New System.Windows.Forms.ComboBox()
        Me.ExeComboBox = New System.Windows.Forms.ComboBox()
        Me.CleanupCheckBox = New System.Windows.Forms.CheckBox()
        Me.Label21 = New System.Windows.Forms.Label()
        Me.Label12 = New System.Windows.Forms.Label()
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
        Me.WaifuCPPNoGPU = New System.Windows.Forms.CheckBox()
        Me.WaifuCPPOpenCL = New System.Windows.Forms.CheckBox()
        Me.WaifuCPPTTA = New System.Windows.Forms.CheckBox()
        Me.WaifuCPPMode = New System.Windows.Forms.ComboBox()
        Me.WaifuCPPFormat = New System.Windows.Forms.ComboBox()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.Label14 = New System.Windows.Forms.Label()
        Me.WaifuCPPScale = New System.Windows.Forms.NumericUpDown()
        Me.Label11 = New System.Windows.Forms.Label()
        Me.WaifuCPPNoise = New System.Windows.Forms.NumericUpDown()
        Me.Label13 = New System.Windows.Forms.Label()
        Me.AnimeCPPGroup = New System.Windows.Forms.GroupBox()
        Me.AnimeCPPCnn = New System.Windows.Forms.CheckBox()
        Me.AnimeCPPGpu = New System.Windows.Forms.CheckBox()
        Me.AnimeCPPPost = New System.Windows.Forms.CheckBox()
        Me.AnimeCPPPre = New System.Windows.Forms.CheckBox()
        Me.AnimeCPPScale = New System.Windows.Forms.NumericUpDown()
        Me.AnimeCPPGradStrength = New System.Windows.Forms.NumericUpDown()
        Me.AnimeCPPColorStrength = New System.Windows.Forms.NumericUpDown()
        Me.AnimeCPPPushColors = New System.Windows.Forms.NumericUpDown()
        Me.AnimeCPPPasses = New System.Windows.Forms.NumericUpDown()
        Me.AnimeCPPFilterType = New System.Windows.Forms.ComboBox()
        Me.AnimeCPPFilters = New System.Windows.Forms.CheckedListBox()
        Me.Label20 = New System.Windows.Forms.Label()
        Me.Label19 = New System.Windows.Forms.Label()
        Me.Label18 = New System.Windows.Forms.Label()
        Me.Label17 = New System.Windows.Forms.Label()
        Me.Label16 = New System.Windows.Forms.Label()
        Me.Label15 = New System.Windows.Forms.Label()
        Me.PyGroup = New System.Windows.Forms.GroupBox()
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
        Me.PathGroup.SuspendLayout()
        Me.CaffeGroup.SuspendLayout()
        CType(Me.CaffeScale, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.CaffeNoise, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SettingsGroup.SuspendLayout()
        CType(Me.NumericThreads, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.VulkanGroup.SuspendLayout()
        CType(Me.VulkanScale, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.VulkanNoise, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.WaifuCPPGroup.SuspendLayout()
        CType(Me.WaifuCPPScale, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.WaifuCPPNoise, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.AnimeCPPGroup.SuspendLayout()
        CType(Me.AnimeCPPScale, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.AnimeCPPGradStrength, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.AnimeCPPColorStrength, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.AnimeCPPPushColors, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.AnimeCPPPasses, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.PyGroup.SuspendLayout()
        CType(Me.PyArguements, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'PathGroup
        '
        Me.PathGroup.Controls.Add(Me.OutputBrowse)
        Me.PathGroup.Controls.Add(Me.InputBrowse)
        Me.PathGroup.Controls.Add(Me.OutputTextBox)
        Me.PathGroup.Controls.Add(Me.InputTextBox)
        Me.PathGroup.Controls.Add(Me.Label2)
        Me.PathGroup.Controls.Add(Me.Label1)
        Me.PathGroup.Location = New System.Drawing.Point(12, 12)
        Me.PathGroup.Name = "PathGroup"
        Me.PathGroup.Size = New System.Drawing.Size(624, 80)
        Me.PathGroup.TabIndex = 0
        Me.PathGroup.TabStop = False
        Me.PathGroup.Text = "Paths"
        '
        'OutputBrowse
        '
        Me.OutputBrowse.Location = New System.Drawing.Point(578, 45)
        Me.OutputBrowse.Name = "OutputBrowse"
        Me.OutputBrowse.Size = New System.Drawing.Size(30, 20)
        Me.OutputBrowse.TabIndex = 3
        Me.OutputBrowse.Text = "..."
        Me.OutputBrowse.UseVisualStyleBackColor = True
        '
        'InputBrowse
        '
        Me.InputBrowse.Location = New System.Drawing.Point(578, 19)
        Me.InputBrowse.Name = "InputBrowse"
        Me.InputBrowse.Size = New System.Drawing.Size(30, 20)
        Me.InputBrowse.TabIndex = 1
        Me.InputBrowse.Text = "..."
        Me.InputBrowse.UseVisualStyleBackColor = True
        '
        'OutputTextBox
        '
        Me.OutputTextBox.DataBindings.Add(New System.Windows.Forms.Binding("Text", Global.AutoCrispy.My.MySettings.Default, "Output", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
        Me.OutputTextBox.Location = New System.Drawing.Point(86, 45)
        Me.OutputTextBox.Name = "OutputTextBox"
        Me.OutputTextBox.Size = New System.Drawing.Size(486, 20)
        Me.OutputTextBox.TabIndex = 2
        Me.OutputTextBox.Text = Global.AutoCrispy.My.MySettings.Default.Output
        '
        'InputTextBox
        '
        Me.InputTextBox.DataBindings.Add(New System.Windows.Forms.Binding("Text", Global.AutoCrispy.My.MySettings.Default, "Input", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
        Me.InputTextBox.Location = New System.Drawing.Point(86, 19)
        Me.InputTextBox.Name = "InputTextBox"
        Me.InputTextBox.Size = New System.Drawing.Size(486, 20)
        Me.InputTextBox.TabIndex = 0
        Me.InputTextBox.Text = Global.AutoCrispy.My.MySettings.Default.Input
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(6, 48)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(74, 13)
        Me.Label2.TabIndex = 0
        Me.Label2.Text = "Output Folder:"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(6, 22)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(66, 13)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Input Folder:"
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
        Me.CaffeTAA.Checked = Global.AutoCrispy.My.MySettings.Default.TAA
        Me.CaffeTAA.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Global.AutoCrispy.My.MySettings.Default, "TAA", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
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
        Me.CaffeScale.DataBindings.Add(New System.Windows.Forms.Binding("Value", Global.AutoCrispy.My.MySettings.Default, "Scale", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
        Me.CaffeScale.DecimalPlaces = 1
        Me.CaffeScale.Increment = New Decimal(New Integer() {1, 0, 0, 65536})
        Me.CaffeScale.Location = New System.Drawing.Point(60, 72)
        Me.CaffeScale.Maximum = New Decimal(New Integer() {4, 0, 0, 0})
        Me.CaffeScale.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.CaffeScale.Name = "CaffeScale"
        Me.CaffeScale.Size = New System.Drawing.Size(114, 20)
        Me.CaffeScale.TabIndex = 13
        Me.CaffeScale.Value = Global.AutoCrispy.My.MySettings.Default.Scale
        '
        'CaffeNoise
        '
        Me.CaffeNoise.DataBindings.Add(New System.Windows.Forms.Binding("Value", Global.AutoCrispy.My.MySettings.Default, "Noise", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
        Me.CaffeNoise.Location = New System.Drawing.Point(60, 46)
        Me.CaffeNoise.Maximum = New Decimal(New Integer() {3, 0, 0, 0})
        Me.CaffeNoise.Name = "CaffeNoise"
        Me.CaffeNoise.Size = New System.Drawing.Size(114, 20)
        Me.CaffeNoise.TabIndex = 12
        Me.CaffeNoise.Value = Global.AutoCrispy.My.MySettings.Default.Noise
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
        Me.WatchDogButton.Location = New System.Drawing.Point(126, 275)
        Me.WatchDogButton.Name = "WatchDogButton"
        Me.WatchDogButton.Size = New System.Drawing.Size(108, 23)
        Me.WatchDogButton.TabIndex = 10
        Me.WatchDogButton.Text = "Running: False"
        Me.WatchDogButton.UseVisualStyleBackColor = True
        '
        'SettingsGroup
        '
        Me.SettingsGroup.Controls.Add(Me.UpscaleProgress)
        Me.SettingsGroup.Controls.Add(Me.NumericThreads)
        Me.SettingsGroup.Controls.Add(Me.ThreadComboBox)
        Me.SettingsGroup.Controls.Add(Me.ExeComboBox)
        Me.SettingsGroup.Controls.Add(Me.CleanupCheckBox)
        Me.SettingsGroup.Controls.Add(Me.Label21)
        Me.SettingsGroup.Controls.Add(Me.Label12)
        Me.SettingsGroup.Location = New System.Drawing.Point(12, 98)
        Me.SettingsGroup.Name = "SettingsGroup"
        Me.SettingsGroup.Size = New System.Drawing.Size(222, 171)
        Me.SettingsGroup.TabIndex = 4
        Me.SettingsGroup.TabStop = False
        Me.SettingsGroup.Text = "Program Settings"
        '
        'UpscaleProgress
        '
        Me.UpscaleProgress.Location = New System.Drawing.Point(6, 142)
        Me.UpscaleProgress.Name = "UpscaleProgress"
        Me.UpscaleProgress.Size = New System.Drawing.Size(210, 23)
        Me.UpscaleProgress.TabIndex = 9
        '
        'NumericThreads
        '
        Me.NumericThreads.DataBindings.Add(New System.Windows.Forms.Binding("Value", Global.AutoCrispy.My.MySettings.Default, "ThreadCount", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
        Me.NumericThreads.Location = New System.Drawing.Point(170, 46)
        Me.NumericThreads.Maximum = New Decimal(New Integer() {64, 0, 0, 0})
        Me.NumericThreads.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.NumericThreads.Name = "NumericThreads"
        Me.NumericThreads.Size = New System.Drawing.Size(40, 20)
        Me.NumericThreads.TabIndex = 6
        Me.NumericThreads.Value = Global.AutoCrispy.My.MySettings.Default.ThreadCount
        '
        'ThreadComboBox
        '
        Me.ThreadComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ThreadComboBox.FormattingEnabled = True
        Me.ThreadComboBox.Items.AddRange(New Object() {"Single", "Custom", "All", "Unlimited (!!)"})
        Me.ThreadComboBox.Location = New System.Drawing.Point(65, 46)
        Me.ThreadComboBox.Name = "ThreadComboBox"
        Me.ThreadComboBox.Size = New System.Drawing.Size(99, 21)
        Me.ThreadComboBox.TabIndex = 5
        '
        'ExeComboBox
        '
        Me.ExeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ExeComboBox.FormattingEnabled = True
        Me.ExeComboBox.Location = New System.Drawing.Point(65, 19)
        Me.ExeComboBox.Name = "ExeComboBox"
        Me.ExeComboBox.Size = New System.Drawing.Size(145, 21)
        Me.ExeComboBox.TabIndex = 4
        '
        'CleanupCheckBox
        '
        Me.CleanupCheckBox.AutoSize = True
        Me.CleanupCheckBox.Location = New System.Drawing.Point(9, 73)
        Me.CleanupCheckBox.Name = "CleanupCheckBox"
        Me.CleanupCheckBox.Size = New System.Drawing.Size(108, 17)
        Me.CleanupCheckBox.TabIndex = 7
        Me.CleanupCheckBox.Text = "Delete Input Files"
        Me.CleanupCheckBox.UseVisualStyleBackColor = True
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
        'Label12
        '
        Me.Label12.AutoSize = True
        Me.Label12.Location = New System.Drawing.Point(6, 49)
        Me.Label12.Name = "Label12"
        Me.Label12.Size = New System.Drawing.Size(49, 13)
        Me.Label12.TabIndex = 0
        Me.Label12.Text = "Threads:"
        '
        'RunOnceButton
        '
        Me.RunOnceButton.Location = New System.Drawing.Point(12, 275)
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
        Me.VulkanTAA.Checked = Global.AutoCrispy.My.MySettings.Default.TAA
        Me.VulkanTAA.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Global.AutoCrispy.My.MySettings.Default, "TAA", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
        Me.VulkanTAA.Location = New System.Drawing.Point(301, 52)
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
        Me.VulkanFormat.Items.AddRange(New Object() {"PNG", "WEBP"})
        Me.VulkanFormat.Location = New System.Drawing.Point(234, 22)
        Me.VulkanFormat.Name = "VulkanFormat"
        Me.VulkanFormat.Size = New System.Drawing.Size(114, 21)
        Me.VulkanFormat.TabIndex = 13
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(180, 25)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(42, 13)
        Me.Label7.TabIndex = 1
        Me.Label7.Text = "Output:"
        '
        'VulkanScale
        '
        Me.VulkanScale.DataBindings.Add(New System.Windows.Forms.Binding("Value", Global.AutoCrispy.My.MySettings.Default, "Scale", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
        Me.VulkanScale.DecimalPlaces = 1
        Me.VulkanScale.Increment = New Decimal(New Integer() {1, 0, 0, 65536})
        Me.VulkanScale.Location = New System.Drawing.Point(60, 49)
        Me.VulkanScale.Maximum = New Decimal(New Integer() {2, 0, 0, 0})
        Me.VulkanScale.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.VulkanScale.Name = "VulkanScale"
        Me.VulkanScale.Size = New System.Drawing.Size(114, 20)
        Me.VulkanScale.TabIndex = 12
        Me.VulkanScale.Value = Global.AutoCrispy.My.MySettings.Default.Scale
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(6, 25)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(37, 13)
        Me.Label8.TabIndex = 1
        Me.Label8.Text = "Noise:"
        '
        'VulkanNoise
        '
        Me.VulkanNoise.DataBindings.Add(New System.Windows.Forms.Binding("Value", Global.AutoCrispy.My.MySettings.Default, "Noise", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
        Me.VulkanNoise.Location = New System.Drawing.Point(60, 23)
        Me.VulkanNoise.Maximum = New Decimal(New Integer() {3, 0, 0, 0})
        Me.VulkanNoise.Minimum = New Decimal(New Integer() {1, 0, 0, -2147483648})
        Me.VulkanNoise.Name = "VulkanNoise"
        Me.VulkanNoise.Size = New System.Drawing.Size(114, 20)
        Me.VulkanNoise.TabIndex = 11
        Me.VulkanNoise.Value = Global.AutoCrispy.My.MySettings.Default.Noise
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Location = New System.Drawing.Point(6, 51)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(37, 13)
        Me.Label9.TabIndex = 1
        Me.Label9.Text = "Scale:"
        '
        'WaifuCPPGroup
        '
        Me.WaifuCPPGroup.Controls.Add(Me.WaifuCPPNoGPU)
        Me.WaifuCPPGroup.Controls.Add(Me.WaifuCPPOpenCL)
        Me.WaifuCPPGroup.Controls.Add(Me.WaifuCPPTTA)
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
        'WaifuCPPNoGPU
        '
        Me.WaifuCPPNoGPU.AutoSize = True
        Me.WaifuCPPNoGPU.Checked = Global.AutoCrispy.My.MySettings.Default.WaifuDGPU
        Me.WaifuCPPNoGPU.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Global.AutoCrispy.My.MySettings.Default, "WaifuDGPU", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
        Me.WaifuCPPNoGPU.Location = New System.Drawing.Point(183, 70)
        Me.WaifuCPPNoGPU.Name = "WaifuCPPNoGPU"
        Me.WaifuCPPNoGPU.Size = New System.Drawing.Size(87, 17)
        Me.WaifuCPPNoGPU.TabIndex = 17
        Me.WaifuCPPNoGPU.Text = "Disable GPU"
        Me.WaifuCPPNoGPU.UseVisualStyleBackColor = True
        '
        'WaifuCPPOpenCL
        '
        Me.WaifuCPPOpenCL.AutoSize = True
        Me.WaifuCPPOpenCL.Checked = Global.AutoCrispy.My.MySettings.Default.WaifuOCL
        Me.WaifuCPPOpenCL.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Global.AutoCrispy.My.MySettings.Default, "WaifuOCL", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
        Me.WaifuCPPOpenCL.Location = New System.Drawing.Point(183, 47)
        Me.WaifuCPPOpenCL.Name = "WaifuCPPOpenCL"
        Me.WaifuCPPOpenCL.Size = New System.Drawing.Size(95, 17)
        Me.WaifuCPPOpenCL.TabIndex = 15
        Me.WaifuCPPOpenCL.Text = "Force OpenCL"
        Me.WaifuCPPOpenCL.UseVisualStyleBackColor = True
        '
        'WaifuCPPTTA
        '
        Me.WaifuCPPTTA.AutoSize = True
        Me.WaifuCPPTTA.Checked = Global.AutoCrispy.My.MySettings.Default.WaifuTTA
        Me.WaifuCPPTTA.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Global.AutoCrispy.My.MySettings.Default, "WaifuTTA", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
        Me.WaifuCPPTTA.Location = New System.Drawing.Point(301, 47)
        Me.WaifuCPPTTA.Name = "WaifuCPPTTA"
        Me.WaifuCPPTTA.Size = New System.Drawing.Size(47, 17)
        Me.WaifuCPPTTA.TabIndex = 16
        Me.WaifuCPPTTA.Text = "TTA"
        Me.WaifuCPPTTA.UseVisualStyleBackColor = True
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
        Me.WaifuCPPScale.DataBindings.Add(New System.Windows.Forms.Binding("Value", Global.AutoCrispy.My.MySettings.Default, "Scale", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
        Me.WaifuCPPScale.DecimalPlaces = 1
        Me.WaifuCPPScale.Increment = New Decimal(New Integer() {1, 0, 0, 65536})
        Me.WaifuCPPScale.Location = New System.Drawing.Point(60, 72)
        Me.WaifuCPPScale.Maximum = New Decimal(New Integer() {8, 0, 0, 0})
        Me.WaifuCPPScale.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.WaifuCPPScale.Name = "WaifuCPPScale"
        Me.WaifuCPPScale.Size = New System.Drawing.Size(114, 20)
        Me.WaifuCPPScale.TabIndex = 13
        Me.WaifuCPPScale.Value = Global.AutoCrispy.My.MySettings.Default.Scale
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
        Me.WaifuCPPNoise.DataBindings.Add(New System.Windows.Forms.Binding("Value", Global.AutoCrispy.My.MySettings.Default, "Noise", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
        Me.WaifuCPPNoise.Location = New System.Drawing.Point(60, 46)
        Me.WaifuCPPNoise.Maximum = New Decimal(New Integer() {3, 0, 0, 0})
        Me.WaifuCPPNoise.Name = "WaifuCPPNoise"
        Me.WaifuCPPNoise.Size = New System.Drawing.Size(114, 20)
        Me.WaifuCPPNoise.TabIndex = 12
        Me.WaifuCPPNoise.Value = Global.AutoCrispy.My.MySettings.Default.Noise
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
        Me.AnimeCPPGroup.Controls.Add(Me.AnimeCPPCnn)
        Me.AnimeCPPGroup.Controls.Add(Me.AnimeCPPGpu)
        Me.AnimeCPPGroup.Controls.Add(Me.AnimeCPPPost)
        Me.AnimeCPPGroup.Controls.Add(Me.AnimeCPPPre)
        Me.AnimeCPPGroup.Controls.Add(Me.AnimeCPPScale)
        Me.AnimeCPPGroup.Controls.Add(Me.AnimeCPPGradStrength)
        Me.AnimeCPPGroup.Controls.Add(Me.AnimeCPPColorStrength)
        Me.AnimeCPPGroup.Controls.Add(Me.AnimeCPPPushColors)
        Me.AnimeCPPGroup.Controls.Add(Me.AnimeCPPPasses)
        Me.AnimeCPPGroup.Controls.Add(Me.AnimeCPPFilterType)
        Me.AnimeCPPGroup.Controls.Add(Me.AnimeCPPFilters)
        Me.AnimeCPPGroup.Controls.Add(Me.Label20)
        Me.AnimeCPPGroup.Controls.Add(Me.Label19)
        Me.AnimeCPPGroup.Controls.Add(Me.Label18)
        Me.AnimeCPPGroup.Controls.Add(Me.Label17)
        Me.AnimeCPPGroup.Controls.Add(Me.Label16)
        Me.AnimeCPPGroup.Controls.Add(Me.Label15)
        Me.AnimeCPPGroup.Location = New System.Drawing.Point(1045, 218)
        Me.AnimeCPPGroup.Name = "AnimeCPPGroup"
        Me.AnimeCPPGroup.Size = New System.Drawing.Size(396, 200)
        Me.AnimeCPPGroup.TabIndex = 11
        Me.AnimeCPPGroup.TabStop = False
        Me.AnimeCPPGroup.Text = "Anime4k CPP Settings"
        '
        'AnimeCPPCnn
        '
        Me.AnimeCPPCnn.AutoSize = True
        Me.AnimeCPPCnn.Checked = Global.AutoCrispy.My.MySettings.Default.A4KCNNCheck
        Me.AnimeCPPCnn.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Global.AutoCrispy.My.MySettings.Default, "A4KCNNCheck", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
        Me.AnimeCPPCnn.Location = New System.Drawing.Point(64, 173)
        Me.AnimeCPPCnn.Name = "AnimeCPPCnn"
        Me.AnimeCPPCnn.Size = New System.Drawing.Size(49, 17)
        Me.AnimeCPPCnn.TabIndex = 19
        Me.AnimeCPPCnn.Text = "CNN"
        Me.AnimeCPPCnn.UseVisualStyleBackColor = True
        '
        'AnimeCPPGpu
        '
        Me.AnimeCPPGpu.AutoSize = True
        Me.AnimeCPPGpu.Checked = Global.AutoCrispy.My.MySettings.Default.A4KGPUCheck
        Me.AnimeCPPGpu.CheckState = System.Windows.Forms.CheckState.Checked
        Me.AnimeCPPGpu.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Global.AutoCrispy.My.MySettings.Default, "A4KGPUCheck", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
        Me.AnimeCPPGpu.Location = New System.Drawing.Point(9, 173)
        Me.AnimeCPPGpu.Name = "AnimeCPPGpu"
        Me.AnimeCPPGpu.Size = New System.Drawing.Size(49, 17)
        Me.AnimeCPPGpu.TabIndex = 18
        Me.AnimeCPPGpu.Text = "GPU"
        Me.AnimeCPPGpu.UseVisualStyleBackColor = True
        '
        'AnimeCPPPost
        '
        Me.AnimeCPPPost.AutoSize = True
        Me.AnimeCPPPost.Checked = Global.AutoCrispy.My.MySettings.Default.A4KPostCheck
        Me.AnimeCPPPost.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Global.AutoCrispy.My.MySettings.Default, "A4KPostCheck", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
        Me.AnimeCPPPost.Location = New System.Drawing.Point(98, 150)
        Me.AnimeCPPPost.Name = "AnimeCPPPost"
        Me.AnimeCPPPost.Size = New System.Drawing.Size(88, 17)
        Me.AnimeCPPPost.TabIndex = 17
        Me.AnimeCPPPost.Text = "Post-Process"
        Me.AnimeCPPPost.UseVisualStyleBackColor = True
        '
        'AnimeCPPPre
        '
        Me.AnimeCPPPre.AutoSize = True
        Me.AnimeCPPPre.Checked = Global.AutoCrispy.My.MySettings.Default.A4KPreCheck
        Me.AnimeCPPPre.CheckState = System.Windows.Forms.CheckState.Checked
        Me.AnimeCPPPre.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Global.AutoCrispy.My.MySettings.Default, "A4KPreCheck", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
        Me.AnimeCPPPre.Location = New System.Drawing.Point(9, 150)
        Me.AnimeCPPPre.Name = "AnimeCPPPre"
        Me.AnimeCPPPre.Size = New System.Drawing.Size(83, 17)
        Me.AnimeCPPPre.TabIndex = 16
        Me.AnimeCPPPre.Text = "Pre-Process"
        Me.AnimeCPPPre.UseVisualStyleBackColor = True
        '
        'AnimeCPPScale
        '
        Me.AnimeCPPScale.DataBindings.Add(New System.Windows.Forms.Binding("Value", Global.AutoCrispy.My.MySettings.Default, "Scale", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
        Me.AnimeCPPScale.DecimalPlaces = 1
        Me.AnimeCPPScale.Increment = New Decimal(New Integer() {1, 0, 0, 65536})
        Me.AnimeCPPScale.Location = New System.Drawing.Point(116, 124)
        Me.AnimeCPPScale.Maximum = New Decimal(New Integer() {8, 0, 0, 0})
        Me.AnimeCPPScale.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.AnimeCPPScale.Name = "AnimeCPPScale"
        Me.AnimeCPPScale.Size = New System.Drawing.Size(58, 20)
        Me.AnimeCPPScale.TabIndex = 15
        Me.AnimeCPPScale.Value = Global.AutoCrispy.My.MySettings.Default.Scale
        '
        'AnimeCPPGradStrength
        '
        Me.AnimeCPPGradStrength.DataBindings.Add(New System.Windows.Forms.Binding("Value", Global.AutoCrispy.My.MySettings.Default, "A4KGradStrength", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
        Me.AnimeCPPGradStrength.DecimalPlaces = 1
        Me.AnimeCPPGradStrength.Location = New System.Drawing.Point(116, 98)
        Me.AnimeCPPGradStrength.Maximum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.AnimeCPPGradStrength.Name = "AnimeCPPGradStrength"
        Me.AnimeCPPGradStrength.Size = New System.Drawing.Size(58, 20)
        Me.AnimeCPPGradStrength.TabIndex = 14
        Me.AnimeCPPGradStrength.Value = Global.AutoCrispy.My.MySettings.Default.A4KGradStrength
        '
        'AnimeCPPColorStrength
        '
        Me.AnimeCPPColorStrength.DataBindings.Add(New System.Windows.Forms.Binding("Value", Global.AutoCrispy.My.MySettings.Default, "A4KCStrength", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
        Me.AnimeCPPColorStrength.DecimalPlaces = 1
        Me.AnimeCPPColorStrength.Location = New System.Drawing.Point(116, 72)
        Me.AnimeCPPColorStrength.Maximum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.AnimeCPPColorStrength.Name = "AnimeCPPColorStrength"
        Me.AnimeCPPColorStrength.Size = New System.Drawing.Size(58, 20)
        Me.AnimeCPPColorStrength.TabIndex = 13
        Me.AnimeCPPColorStrength.Value = Global.AutoCrispy.My.MySettings.Default.A4KCStrength
        '
        'AnimeCPPPushColors
        '
        Me.AnimeCPPPushColors.DataBindings.Add(New System.Windows.Forms.Binding("Value", Global.AutoCrispy.My.MySettings.Default, "A4KPColors", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
        Me.AnimeCPPPushColors.Location = New System.Drawing.Point(116, 46)
        Me.AnimeCPPPushColors.Maximum = New Decimal(New Integer() {10, 0, 0, 0})
        Me.AnimeCPPPushColors.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.AnimeCPPPushColors.Name = "AnimeCPPPushColors"
        Me.AnimeCPPPushColors.Size = New System.Drawing.Size(58, 20)
        Me.AnimeCPPPushColors.TabIndex = 12
        Me.AnimeCPPPushColors.Value = Global.AutoCrispy.My.MySettings.Default.A4KPColors
        '
        'AnimeCPPPasses
        '
        Me.AnimeCPPPasses.DataBindings.Add(New System.Windows.Forms.Binding("Value", Global.AutoCrispy.My.MySettings.Default, "A4KPasses", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
        Me.AnimeCPPPasses.Location = New System.Drawing.Point(116, 20)
        Me.AnimeCPPPasses.Maximum = New Decimal(New Integer() {10, 0, 0, 0})
        Me.AnimeCPPPasses.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.AnimeCPPPasses.Name = "AnimeCPPPasses"
        Me.AnimeCPPPasses.Size = New System.Drawing.Size(58, 20)
        Me.AnimeCPPPasses.TabIndex = 11
        Me.AnimeCPPPasses.Value = Global.AutoCrispy.My.MySettings.Default.A4KPasses
        '
        'AnimeCPPFilterType
        '
        Me.AnimeCPPFilterType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.AnimeCPPFilterType.FormattingEnabled = True
        Me.AnimeCPPFilterType.Items.AddRange(New Object() {"None", "Pre-Filter", "Post-Filter"})
        Me.AnimeCPPFilterType.Location = New System.Drawing.Point(260, 19)
        Me.AnimeCPPFilterType.Name = "AnimeCPPFilterType"
        Me.AnimeCPPFilterType.Size = New System.Drawing.Size(130, 21)
        Me.AnimeCPPFilterType.TabIndex = 20
        '
        'AnimeCPPFilters
        '
        Me.AnimeCPPFilters.FormattingEnabled = True
        Me.AnimeCPPFilters.Items.AddRange(New Object() {"Median Blur", "Mean Blur", "CAS Sharpening", "Weak Gaussian Blur", "Gaussian Blur", "Bilateral Filter", "Fast Bilateral Filter"})
        Me.AnimeCPPFilters.Location = New System.Drawing.Point(260, 46)
        Me.AnimeCPPFilters.Name = "AnimeCPPFilters"
        Me.AnimeCPPFilters.Size = New System.Drawing.Size(130, 109)
        Me.AnimeCPPFilters.TabIndex = 21
        '
        'Label20
        '
        Me.Label20.AutoSize = True
        Me.Label20.Location = New System.Drawing.Point(6, 126)
        Me.Label20.Name = "Label20"
        Me.Label20.Size = New System.Drawing.Size(37, 13)
        Me.Label20.TabIndex = 1
        Me.Label20.Text = "Scale:"
        '
        'Label19
        '
        Me.Label19.AutoSize = True
        Me.Label19.Location = New System.Drawing.Point(6, 100)
        Me.Label19.Name = "Label19"
        Me.Label19.Size = New System.Drawing.Size(103, 13)
        Me.Label19.TabIndex = 1
        Me.Label19.Text = "Push Grad Strength:"
        '
        'Label18
        '
        Me.Label18.AutoSize = True
        Me.Label18.Location = New System.Drawing.Point(6, 74)
        Me.Label18.Name = "Label18"
        Me.Label18.Size = New System.Drawing.Size(104, 13)
        Me.Label18.TabIndex = 1
        Me.Label18.Text = "Push Color Strength:"
        '
        'Label17
        '
        Me.Label17.AutoSize = True
        Me.Label17.Location = New System.Drawing.Point(6, 48)
        Me.Label17.Name = "Label17"
        Me.Label17.Size = New System.Drawing.Size(66, 13)
        Me.Label17.TabIndex = 1
        Me.Label17.Text = "Push Colors:"
        '
        'Label16
        '
        Me.Label16.AutoSize = True
        Me.Label16.Location = New System.Drawing.Point(6, 22)
        Me.Label16.Name = "Label16"
        Me.Label16.Size = New System.Drawing.Size(44, 13)
        Me.Label16.TabIndex = 1
        Me.Label16.Text = "Passes:"
        '
        'Label15
        '
        Me.Label15.AutoSize = True
        Me.Label15.Location = New System.Drawing.Point(180, 22)
        Me.Label15.Name = "Label15"
        Me.Label15.Size = New System.Drawing.Size(62, 13)
        Me.Label15.TabIndex = 1
        Me.Label15.Text = "Filter Mode:"
        '
        'PyGroup
        '
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
        'PyArguements
        '
        Me.PyArguements.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells
        Me.PyArguements.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.PyArguements.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.PyFlag, Me.PyValue})
        Me.PyArguements.Location = New System.Drawing.Point(9, 72)
        Me.PyArguements.Name = "PyArguements"
        Me.PyArguements.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.PyArguements.Size = New System.Drawing.Size(339, 122)
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
        Me.PyModel.Location = New System.Drawing.Point(234, 19)
        Me.PyModel.Name = "PyModel"
        Me.PyModel.Size = New System.Drawing.Size(114, 21)
        Me.PyModel.TabIndex = 2
        '
        'PyInputFlag
        '
        Me.PyInputFlag.Location = New System.Drawing.Point(72, 46)
        Me.PyInputFlag.Name = "PyInputFlag"
        Me.PyInputFlag.Size = New System.Drawing.Size(102, 20)
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
        Me.PyOutputFlag.Location = New System.Drawing.Point(251, 46)
        Me.PyOutputFlag.Name = "PyOutputFlag"
        Me.PyOutputFlag.Size = New System.Drawing.Size(97, 20)
        Me.PyOutputFlag.TabIndex = 1
        Me.PyOutputFlag.Text = "--output"
        '
        'Label26
        '
        Me.Label26.AutoSize = True
        Me.Label26.Location = New System.Drawing.Point(180, 22)
        Me.Label26.Name = "Label26"
        Me.Label26.Size = New System.Drawing.Size(39, 13)
        Me.Label26.TabIndex = 1
        Me.Label26.Text = "Model:"
        '
        'Label28
        '
        Me.Label28.AutoSize = True
        Me.Label28.Location = New System.Drawing.Point(6, 49)
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
        Me.Label27.Location = New System.Drawing.Point(180, 49)
        Me.Label27.Name = "Label27"
        Me.Label27.Size = New System.Drawing.Size(65, 13)
        Me.Label27.TabIndex = 0
        Me.Label27.Text = "Output Flag:"
        '
        'WorkHorse
        '
        Me.WorkHorse.WorkerReportsProgress = True
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1456, 718)
        Me.Controls.Add(Me.PyGroup)
        Me.Controls.Add(Me.RunOnceButton)
        Me.Controls.Add(Me.AnimeCPPGroup)
        Me.Controls.Add(Me.WaifuCPPGroup)
        Me.Controls.Add(Me.VulkanGroup)
        Me.Controls.Add(Me.SettingsGroup)
        Me.Controls.Add(Me.WatchDogButton)
        Me.Controls.Add(Me.CaffeGroup)
        Me.Controls.Add(Me.PathGroup)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "Form1"
        Me.Text = "AutoCrispy"
        Me.PathGroup.ResumeLayout(False)
        Me.PathGroup.PerformLayout()
        Me.CaffeGroup.ResumeLayout(False)
        Me.CaffeGroup.PerformLayout()
        CType(Me.CaffeScale, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.CaffeNoise, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SettingsGroup.ResumeLayout(False)
        Me.SettingsGroup.PerformLayout()
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
        CType(Me.AnimeCPPGradStrength, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.AnimeCPPColorStrength, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.AnimeCPPPushColors, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.AnimeCPPPasses, System.ComponentModel.ISupportInitialize).EndInit()
        Me.PyGroup.ResumeLayout(False)
        Me.PyGroup.PerformLayout()
        CType(Me.PyArguements, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents PathGroup As GroupBox
    Friend WithEvents OutputBrowse As Button
    Friend WithEvents InputBrowse As Button
    Friend WithEvents OutputTextBox As TextBox
    Friend WithEvents InputTextBox As TextBox
    Friend WithEvents Label2 As Label
    Friend WithEvents Label1 As Label
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
    Friend WithEvents WaifuCPPTTA As CheckBox
    Friend WithEvents WaifuCPPMode As ComboBox
    Friend WithEvents WaifuCPPFormat As ComboBox
    Friend WithEvents Label10 As Label
    Friend WithEvents Label14 As Label
    Friend WithEvents WaifuCPPScale As NumericUpDown
    Friend WithEvents Label11 As Label
    Friend WithEvents WaifuCPPNoise As NumericUpDown
    Friend WithEvents Label13 As Label
    Friend WithEvents WaifuCPPNoGPU As CheckBox
    Friend WithEvents WaifuCPPOpenCL As CheckBox
    Friend WithEvents AnimeCPPFilters As CheckedListBox
    Friend WithEvents AnimeCPPFilterType As ComboBox
    Friend WithEvents Label15 As Label
    Friend WithEvents AnimeCPPScale As NumericUpDown
    Friend WithEvents AnimeCPPGradStrength As NumericUpDown
    Friend WithEvents AnimeCPPColorStrength As NumericUpDown
    Friend WithEvents AnimeCPPPushColors As NumericUpDown
    Friend WithEvents AnimeCPPPasses As NumericUpDown
    Friend WithEvents Label20 As Label
    Friend WithEvents Label19 As Label
    Friend WithEvents Label18 As Label
    Friend WithEvents Label17 As Label
    Friend WithEvents Label16 As Label
    Friend WithEvents AnimeCPPCnn As CheckBox
    Friend WithEvents AnimeCPPGpu As CheckBox
    Friend WithEvents AnimeCPPPost As CheckBox
    Friend WithEvents AnimeCPPPre As CheckBox
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
End Class
