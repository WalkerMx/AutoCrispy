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
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.ComputeComboBox = New System.Windows.Forms.ComboBox()
        Me.ModeComboBox = New System.Windows.Forms.ComboBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.TAAComboBox = New System.Windows.Forms.ComboBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.WatchDog = New System.Windows.Forms.Timer(Me.components)
        Me.Button3 = New System.Windows.Forms.Button()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.ExeComboBox = New System.Windows.Forms.ComboBox()
        Me.DebugCheckBox = New System.Windows.Forms.CheckBox()
        Me.CheckBox1 = New System.Windows.Forms.CheckBox()
        Me.NumericNoise = New System.Windows.Forms.NumericUpDown()
        Me.NumericScale = New System.Windows.Forms.NumericUpDown()
        Me.NumericGPU = New System.Windows.Forms.NumericUpDown()
        Me.OutputTextBox = New System.Windows.Forms.TextBox()
        Me.InputTextBox = New System.Windows.Forms.TextBox()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        CType(Me.NumericNoise, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.NumericScale, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.NumericGPU, System.ComponentModel.ISupportInitialize).BeginInit()
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
        Me.GroupBox2.Controls.Add(Me.Label8)
        Me.GroupBox2.Controls.Add(Me.NumericNoise)
        Me.GroupBox2.Controls.Add(Me.NumericScale)
        Me.GroupBox2.Controls.Add(Me.ComputeComboBox)
        Me.GroupBox2.Controls.Add(Me.NumericGPU)
        Me.GroupBox2.Controls.Add(Me.ModeComboBox)
        Me.GroupBox2.Controls.Add(Me.Label7)
        Me.GroupBox2.Controls.Add(Me.Label6)
        Me.GroupBox2.Controls.Add(Me.Label5)
        Me.GroupBox2.Controls.Add(Me.TAAComboBox)
        Me.GroupBox2.Controls.Add(Me.Label4)
        Me.GroupBox2.Controls.Add(Me.Label3)
        Me.GroupBox2.Location = New System.Drawing.Point(12, 98)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(396, 106)
        Me.GroupBox2.TabIndex = 1
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Waifu2x Settings"
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
        'ComputeComboBox
        '
        Me.ComputeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ComputeComboBox.FormattingEnabled = True
        Me.ComputeComboBox.Items.AddRange(New Object() {"CPU", "GPU", "cuDNN"})
        Me.ComputeComboBox.Location = New System.Drawing.Point(295, 19)
        Me.ComputeComboBox.Name = "ComputeComboBox"
        Me.ComputeComboBox.Size = New System.Drawing.Size(81, 21)
        Me.ComputeComboBox.TabIndex = 3
        '
        'ModeComboBox
        '
        Me.ModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ModeComboBox.FormattingEnabled = True
        Me.ModeComboBox.Items.AddRange(New Object() {"Noise", "Scale", "Both", "Auto"})
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
        Me.TAAComboBox.Location = New System.Drawing.Point(295, 46)
        Me.TAAComboBox.Name = "TAAComboBox"
        Me.TAAComboBox.Size = New System.Drawing.Size(81, 21)
        Me.TAAComboBox.TabIndex = 3
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
        Me.Button3.Location = New System.Drawing.Point(486, 181)
        Me.Button3.Name = "Button3"
        Me.Button3.Size = New System.Drawing.Size(150, 23)
        Me.Button3.TabIndex = 2
        Me.Button3.Text = "Running: False"
        Me.Button3.UseVisualStyleBackColor = True
        '
        'GroupBox3
        '
        Me.GroupBox3.Controls.Add(Me.DebugCheckBox)
        Me.GroupBox3.Controls.Add(Me.ExeComboBox)
        Me.GroupBox3.Controls.Add(Me.CheckBox1)
        Me.GroupBox3.Location = New System.Drawing.Point(414, 98)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Size = New System.Drawing.Size(222, 77)
        Me.GroupBox3.TabIndex = 3
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "Program Settings"
        '
        'ExeComboBox
        '
        Me.ExeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ExeComboBox.Enabled = False
        Me.ExeComboBox.FormattingEnabled = True
        Me.ExeComboBox.Items.AddRange(New Object() {"Waifu2x Caffe", "Waifu2x Vulkan"})
        Me.ExeComboBox.Location = New System.Drawing.Point(6, 42)
        Me.ExeComboBox.Name = "ExeComboBox"
        Me.ExeComboBox.Size = New System.Drawing.Size(109, 21)
        Me.ExeComboBox.TabIndex = 1
        '
        'DebugCheckBox
        '
        Me.DebugCheckBox.AutoSize = True
        Me.DebugCheckBox.Checked = Global.AutoCrispy.My.MySettings.Default.DebugOn
        Me.DebugCheckBox.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Global.AutoCrispy.My.MySettings.Default, "DebugOn", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
        Me.DebugCheckBox.Location = New System.Drawing.Point(93, 19)
        Me.DebugCheckBox.Name = "DebugCheckBox"
        Me.DebugCheckBox.Size = New System.Drawing.Size(93, 17)
        Me.DebugCheckBox.TabIndex = 2
        Me.DebugCheckBox.Text = "Debug Output"
        Me.DebugCheckBox.UseVisualStyleBackColor = True
        '
        'CheckBox1
        '
        Me.CheckBox1.AutoSize = True
        Me.CheckBox1.Checked = Global.AutoCrispy.My.MySettings.Default.DeleteFiles
        Me.CheckBox1.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Global.AutoCrispy.My.MySettings.Default, "DeleteFiles", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
        Me.CheckBox1.Location = New System.Drawing.Point(6, 19)
        Me.CheckBox1.Name = "CheckBox1"
        Me.CheckBox1.Size = New System.Drawing.Size(81, 17)
        Me.CheckBox1.TabIndex = 0
        Me.CheckBox1.Text = "Delete Files"
        Me.CheckBox1.UseVisualStyleBackColor = True
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
        'NumericGPU
        '
        Me.NumericGPU.DataBindings.Add(New System.Windows.Forms.Binding("Value", Global.AutoCrispy.My.MySettings.Default, "GPUIndex", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
        Me.NumericGPU.Location = New System.Drawing.Point(295, 73)
        Me.NumericGPU.Maximum = New Decimal(New Integer() {9, 0, 0, 0})
        Me.NumericGPU.Name = "NumericGPU"
        Me.NumericGPU.Size = New System.Drawing.Size(81, 20)
        Me.NumericGPU.TabIndex = 1
        Me.NumericGPU.Value = Global.AutoCrispy.My.MySettings.Default.GPUIndex
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
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(650, 214)
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
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox3.PerformLayout()
        CType(Me.NumericNoise, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.NumericScale, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.NumericGPU, System.ComponentModel.ISupportInitialize).EndInit()
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
End Class
