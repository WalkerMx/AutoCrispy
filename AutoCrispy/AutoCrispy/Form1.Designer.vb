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
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.OutputTextBox = New System.Windows.Forms.TextBox()
        Me.InputTextBox = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.FormatComboBox = New System.Windows.Forms.ComboBox()
        Me.Label11 = New System.Windows.Forms.Label()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.NumericNoise = New System.Windows.Forms.NumericUpDown()
        Me.NumericScale = New System.Windows.Forms.NumericUpDown()
        Me.ComputeComboBox = New System.Windows.Forms.ComboBox()
        Me.NumericJPWP = New System.Windows.Forms.NumericUpDown()
        Me.NumericPNG = New System.Windows.Forms.NumericUpDown()
        Me.NumericGPU = New System.Windows.Forms.NumericUpDown()
        Me.ModeComboBox = New System.Windows.Forms.ComboBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.TAAComboBox = New System.Windows.Forms.ComboBox()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.WatchDog = New System.Windows.Forms.Timer(Me.components)
        Me.Button3 = New System.Windows.Forms.Button()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.CheckBox2 = New System.Windows.Forms.CheckBox()
        Me.DebugCheckBox = New System.Windows.Forms.CheckBox()
        Me.ExeComboBox = New System.Windows.Forms.ComboBox()
        Me.CheckBox1 = New System.Windows.Forms.CheckBox()
        Me.ThreadComboBox = New System.Windows.Forms.ComboBox()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        CType(Me.NumericNoise, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.NumericScale, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.NumericJPWP, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.NumericPNG, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.NumericGPU, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox3.SuspendLayout()
        Me.SuspendLayout()
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.Button2)
        Me.GroupBox1.Controls.Add(Me.Button1)
        Me.GroupBox1.Controls.Add(Me.OutputTextBox)
        Me.GroupBox1.Controls.Add(Me.InputTextBox)
        Me.GroupBox1.Controls.Add(Me.Label2)
        Me.GroupBox1.Controls.Add(Me.Label1)
        Me.GroupBox1.Location = New System.Drawing.Point(12, 12)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(624, 80)
        Me.GroupBox1.TabIndex = 0
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Paths"
        '
        'Button2
        '
        Me.Button2.Location = New System.Drawing.Point(578, 45)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(30, 20)
        Me.Button2.TabIndex = 4
        Me.Button2.Text = "..."
        Me.Button2.UseVisualStyleBackColor = True
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(578, 19)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(30, 20)
        Me.Button1.TabIndex = 3
        Me.Button1.Text = "..."
        Me.Button1.UseVisualStyleBackColor = True
        '
        'OutputTextBox
        '
        Me.OutputTextBox.DataBindings.Add(New System.Windows.Forms.Binding("Text", Global.AutoCrispy.My.MySettings.Default, "Output", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
        Me.OutputTextBox.Location = New System.Drawing.Point(86, 45)
        Me.OutputTextBox.Name = "OutputTextBox"
        Me.OutputTextBox.Size = New System.Drawing.Size(486, 20)
        Me.OutputTextBox.TabIndex = 3
        Me.OutputTextBox.Text = Global.AutoCrispy.My.MySettings.Default.Output
        '
        'InputTextBox
        '
        Me.InputTextBox.DataBindings.Add(New System.Windows.Forms.Binding("Text", Global.AutoCrispy.My.MySettings.Default, "Input", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
        Me.InputTextBox.Location = New System.Drawing.Point(86, 19)
        Me.InputTextBox.Name = "InputTextBox"
        Me.InputTextBox.Size = New System.Drawing.Size(486, 20)
        Me.InputTextBox.TabIndex = 3
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
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.FormatComboBox)
        Me.GroupBox2.Controls.Add(Me.Label11)
        Me.GroupBox2.Controls.Add(Me.Label10)
        Me.GroupBox2.Controls.Add(Me.Label8)
        Me.GroupBox2.Controls.Add(Me.NumericNoise)
        Me.GroupBox2.Controls.Add(Me.NumericScale)
        Me.GroupBox2.Controls.Add(Me.ComputeComboBox)
        Me.GroupBox2.Controls.Add(Me.NumericJPWP)
        Me.GroupBox2.Controls.Add(Me.NumericPNG)
        Me.GroupBox2.Controls.Add(Me.NumericGPU)
        Me.GroupBox2.Controls.Add(Me.ModeComboBox)
        Me.GroupBox2.Controls.Add(Me.Label7)
        Me.GroupBox2.Controls.Add(Me.Label6)
        Me.GroupBox2.Controls.Add(Me.Label5)
        Me.GroupBox2.Controls.Add(Me.TAAComboBox)
        Me.GroupBox2.Controls.Add(Me.Label9)
        Me.GroupBox2.Controls.Add(Me.Label4)
        Me.GroupBox2.Controls.Add(Me.Label3)
        Me.GroupBox2.Location = New System.Drawing.Point(12, 98)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(396, 154)
        Me.GroupBox2.TabIndex = 1
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Waifu2x Settings"
        '
        'FormatComboBox
        '
        Me.FormatComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.FormatComboBox.FormattingEnabled = True
        Me.FormatComboBox.Items.AddRange(New Object() {"PNG", "WEBP", "JPG"})
        Me.FormatComboBox.Location = New System.Drawing.Point(107, 98)
        Me.FormatComboBox.Name = "FormatComboBox"
        Me.FormatComboBox.Size = New System.Drawing.Size(81, 21)
        Me.FormatComboBox.TabIndex = 4
        '
        'Label11
        '
        Me.Label11.AutoSize = True
        Me.Label11.Location = New System.Drawing.Point(194, 128)
        Me.Label11.Name = "Label11"
        Me.Label11.Size = New System.Drawing.Size(102, 13)
        Me.Label11.TabIndex = 2
        Me.Label11.Text = "JPG/WEBP Quality:"
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Location = New System.Drawing.Point(194, 102)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(96, 13)
        Me.Label10.TabIndex = 2
        Me.Label10.Text = "PNG Compression:"
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(194, 75)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(62, 13)
        Me.Label8.TabIndex = 2
        Me.Label8.Text = "GPU Index:"
        '
        'NumericNoise
        '
        Me.NumericNoise.DataBindings.Add(New System.Windows.Forms.Binding("Value", Global.AutoCrispy.My.MySettings.Default, "Noise", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
        Me.NumericNoise.Location = New System.Drawing.Point(107, 46)
        Me.NumericNoise.Maximum = New Decimal(New Integer() {3, 0, 0, 0})
        Me.NumericNoise.Name = "NumericNoise"
        Me.NumericNoise.Size = New System.Drawing.Size(81, 20)
        Me.NumericNoise.TabIndex = 3
        Me.NumericNoise.Value = Global.AutoCrispy.My.MySettings.Default.Noise
        '
        'NumericScale
        '
        Me.NumericScale.DataBindings.Add(New System.Windows.Forms.Binding("Value", Global.AutoCrispy.My.MySettings.Default, "Scale", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
        Me.NumericScale.DecimalPlaces = 1
        Me.NumericScale.Increment = New Decimal(New Integer() {1, 0, 0, 65536})
        Me.NumericScale.Location = New System.Drawing.Point(107, 72)
        Me.NumericScale.Maximum = New Decimal(New Integer() {40, 0, 0, 65536})
        Me.NumericScale.Minimum = New Decimal(New Integer() {10, 0, 0, 65536})
        Me.NumericScale.Name = "NumericScale"
        Me.NumericScale.Size = New System.Drawing.Size(81, 20)
        Me.NumericScale.TabIndex = 3
        Me.NumericScale.Value = Global.AutoCrispy.My.MySettings.Default.Scale
        '
        'ComputeComboBox
        '
        Me.ComputeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ComputeComboBox.FormattingEnabled = True
        Me.ComputeComboBox.Items.AddRange(New Object() {"CPU", "GPU", "cuDNN"})
        Me.ComputeComboBox.Location = New System.Drawing.Point(302, 20)
        Me.ComputeComboBox.Name = "ComputeComboBox"
        Me.ComputeComboBox.Size = New System.Drawing.Size(81, 21)
        Me.ComputeComboBox.TabIndex = 3
        '
        'NumericJPWP
        '
        Me.NumericJPWP.DataBindings.Add(New System.Windows.Forms.Binding("Value", Global.AutoCrispy.My.MySettings.Default, "JPWPc", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
        Me.NumericJPWP.Location = New System.Drawing.Point(302, 126)
        Me.NumericJPWP.Maximum = New Decimal(New Integer() {101, 0, 0, 0})
        Me.NumericJPWP.Name = "NumericJPWP"
        Me.NumericJPWP.Size = New System.Drawing.Size(81, 20)
        Me.NumericJPWP.TabIndex = 1
        Me.NumericJPWP.Value = Global.AutoCrispy.My.MySettings.Default.JPWPc
        '
        'NumericPNG
        '
        Me.NumericPNG.DataBindings.Add(New System.Windows.Forms.Binding("Value", Global.AutoCrispy.My.MySettings.Default, "PNGc", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
        Me.NumericPNG.Location = New System.Drawing.Point(302, 100)
        Me.NumericPNG.Maximum = New Decimal(New Integer() {9, 0, 0, 0})
        Me.NumericPNG.Name = "NumericPNG"
        Me.NumericPNG.Size = New System.Drawing.Size(81, 20)
        Me.NumericPNG.TabIndex = 1
        Me.NumericPNG.Value = Global.AutoCrispy.My.MySettings.Default.PNGc
        '
        'NumericGPU
        '
        Me.NumericGPU.DataBindings.Add(New System.Windows.Forms.Binding("Value", Global.AutoCrispy.My.MySettings.Default, "GPUIndex", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
        Me.NumericGPU.Location = New System.Drawing.Point(302, 74)
        Me.NumericGPU.Maximum = New Decimal(New Integer() {9, 0, 0, 0})
        Me.NumericGPU.Name = "NumericGPU"
        Me.NumericGPU.Size = New System.Drawing.Size(81, 20)
        Me.NumericGPU.TabIndex = 1
        Me.NumericGPU.Value = Global.AutoCrispy.My.MySettings.Default.GPUIndex
        '
        'ModeComboBox
        '
        Me.ModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ModeComboBox.FormattingEnabled = True
        Me.ModeComboBox.Items.AddRange(New Object() {"Noise", "Scale", "Both"})
        Me.ModeComboBox.Location = New System.Drawing.Point(107, 19)
        Me.ModeComboBox.Name = "ModeComboBox"
        Me.ModeComboBox.Size = New System.Drawing.Size(81, 21)
        Me.ModeComboBox.TabIndex = 3
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(194, 49)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(31, 13)
        Me.Label7.TabIndex = 0
        Me.Label7.Text = "TAA:"
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(194, 22)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(91, 13)
        Me.Label6.TabIndex = 0
        Me.Label6.Text = "Compute Method:"
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(6, 22)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(37, 13)
        Me.Label5.TabIndex = 0
        Me.Label5.Text = "Mode:"
        '
        'TAAComboBox
        '
        Me.TAAComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.TAAComboBox.FormattingEnabled = True
        Me.TAAComboBox.Items.AddRange(New Object() {"OFF", "ON"})
        Me.TAAComboBox.Location = New System.Drawing.Point(302, 47)
        Me.TAAComboBox.Name = "TAAComboBox"
        Me.TAAComboBox.Size = New System.Drawing.Size(81, 21)
        Me.TAAComboBox.TabIndex = 3
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Location = New System.Drawing.Point(6, 101)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(77, 13)
        Me.Label9.TabIndex = 0
        Me.Label9.Text = "Output Format:"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(6, 74)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(37, 13)
        Me.Label4.TabIndex = 0
        Me.Label4.Text = "Scale:"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(6, 48)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(37, 13)
        Me.Label3.TabIndex = 0
        Me.Label3.Text = "Noise:"
        '
        'WatchDog
        '
        Me.WatchDog.Interval = 1000
        '
        'Button3
        '
        Me.Button3.Location = New System.Drawing.Point(486, 258)
        Me.Button3.Name = "Button3"
        Me.Button3.Size = New System.Drawing.Size(150, 23)
        Me.Button3.TabIndex = 2
        Me.Button3.Text = "Running: False"
        Me.Button3.UseVisualStyleBackColor = True
        '
        'GroupBox3
        '
        Me.GroupBox3.Controls.Add(Me.ThreadComboBox)
        Me.GroupBox3.Controls.Add(Me.CheckBox2)
        Me.GroupBox3.Controls.Add(Me.DebugCheckBox)
        Me.GroupBox3.Controls.Add(Me.ExeComboBox)
        Me.GroupBox3.Controls.Add(Me.CheckBox1)
        Me.GroupBox3.Location = New System.Drawing.Point(414, 98)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Size = New System.Drawing.Size(222, 154)
        Me.GroupBox3.TabIndex = 3
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "Program Settings"
        '
        'CheckBox2
        '
        Me.CheckBox2.AutoSize = True
        Me.CheckBox2.Location = New System.Drawing.Point(6, 95)
        Me.CheckBox2.Name = "CheckBox2"
        Me.CheckBox2.Size = New System.Drawing.Size(95, 17)
        Me.CheckBox2.TabIndex = 3
        Me.CheckBox2.Text = "Force OpenCL"
        Me.CheckBox2.UseVisualStyleBackColor = True
        '
        'DebugCheckBox
        '
        Me.DebugCheckBox.AutoSize = True
        Me.DebugCheckBox.Checked = Global.AutoCrispy.My.MySettings.Default.DebugOn
        Me.DebugCheckBox.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Global.AutoCrispy.My.MySettings.Default, "DebugOn", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
        Me.DebugCheckBox.Location = New System.Drawing.Point(6, 72)
        Me.DebugCheckBox.Name = "DebugCheckBox"
        Me.DebugCheckBox.Size = New System.Drawing.Size(93, 17)
        Me.DebugCheckBox.TabIndex = 2
        Me.DebugCheckBox.Text = "Debug Output"
        Me.DebugCheckBox.UseVisualStyleBackColor = True
        '
        'ExeComboBox
        '
        Me.ExeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ExeComboBox.FormattingEnabled = True
        Me.ExeComboBox.Location = New System.Drawing.Point(6, 19)
        Me.ExeComboBox.Name = "ExeComboBox"
        Me.ExeComboBox.Size = New System.Drawing.Size(109, 21)
        Me.ExeComboBox.TabIndex = 1
        '
        'CheckBox1
        '
        Me.CheckBox1.AutoSize = True
        Me.CheckBox1.Checked = Global.AutoCrispy.My.MySettings.Default.DeleteFiles
        Me.CheckBox1.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Global.AutoCrispy.My.MySettings.Default, "DeleteFiles", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
        Me.CheckBox1.Location = New System.Drawing.Point(6, 49)
        Me.CheckBox1.Name = "CheckBox1"
        Me.CheckBox1.Size = New System.Drawing.Size(108, 17)
        Me.CheckBox1.TabIndex = 0
        Me.CheckBox1.Text = "Delete Input Files"
        Me.CheckBox1.UseVisualStyleBackColor = True
        '
        'ThreadComboBox
        '
        Me.ThreadComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ThreadComboBox.FormattingEnabled = True
        Me.ThreadComboBox.Items.AddRange(New Object() {"Serial", "Half Threads", "All Threads", "Unlimited (!!)"})
        Me.ThreadComboBox.Location = New System.Drawing.Point(6, 118)
        Me.ThreadComboBox.Name = "ThreadComboBox"
        Me.ThreadComboBox.Size = New System.Drawing.Size(109, 21)
        Me.ThreadComboBox.TabIndex = 4
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(652, 292)
        Me.Controls.Add(Me.GroupBox3)
        Me.Controls.Add(Me.Button3)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.GroupBox1)
        Me.Name = "Form1"
        Me.Text = "AutoCrispy"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        CType(Me.NumericNoise, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.NumericScale, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.NumericJPWP, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.NumericPNG, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.NumericGPU, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox3.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents Button2 As Button
    Friend WithEvents Button1 As Button
    Friend WithEvents OutputTextBox As TextBox
    Friend WithEvents InputTextBox As TextBox
    Friend WithEvents Label2 As Label
    Friend WithEvents Label1 As Label
    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents NumericScale As NumericUpDown
    Friend WithEvents ModeComboBox As ComboBox
    Friend WithEvents Label5 As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents NumericNoise As NumericUpDown
    Friend WithEvents ComputeComboBox As ComboBox
    Friend WithEvents Label6 As Label
    Friend WithEvents TAAComboBox As ComboBox
    Friend WithEvents Label7 As Label
    Friend WithEvents WatchDog As Timer
    Friend WithEvents Button3 As Button
    Friend WithEvents GroupBox3 As GroupBox
    Friend WithEvents CheckBox1 As CheckBox
    Friend WithEvents Label8 As Label
    Friend WithEvents NumericGPU As NumericUpDown
    Friend WithEvents ExeComboBox As ComboBox
    Friend WithEvents DebugCheckBox As CheckBox
    Friend WithEvents FormatComboBox As ComboBox
    Friend WithEvents Label9 As Label
    Friend WithEvents Label10 As Label
    Friend WithEvents NumericPNG As NumericUpDown
    Friend WithEvents Label11 As Label
    Friend WithEvents NumericJPWP As NumericUpDown
    Friend WithEvents CheckBox2 As CheckBox
    Friend WithEvents ThreadComboBox As ComboBox
End Class
