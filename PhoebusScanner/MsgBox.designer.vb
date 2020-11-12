Imports System.Windows.Forms
Imports DevExpress.XtraEditors

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class MsgBox
    Inherits DevExpress.XtraEditors.XtraForm

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.Caption = New System.Windows.Forms.RichTextBox()
        Me.Cancel_Button = New SimpleButton
        Me.OK_Button = New SimpleButton
        Me.PanelControl1 = New DevExpress.XtraEditors.PanelControl()
        Me.ConfirmPanel = New DevExpress.XtraEditors.PanelControl()
        Me.btnTranslate = New SimpleButton
        CType(Me.PanelControl1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.PanelControl1.SuspendLayout()
        CType(Me.ConfirmPanel, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ConfirmPanel.SuspendLayout()
        Me.SuspendLayout()
        '
        'Caption
        '
        Me.Caption.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.Caption.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Caption.Location = New System.Drawing.Point(10, 10)
        Me.Caption.Name = "Caption"
        Me.Caption.ShowSelectionMargin = True
        Me.Caption.Size = New System.Drawing.Size(674, 201)
        Me.Caption.TabIndex = 0
        Me.Caption.Text = "Caption"
        '
        'Cancel_Button
        '
        Me.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Cancel_Button.Location = New System.Drawing.Point(570, 6)
        Me.Cancel_Button.Name = "Cancel_Button"
        Me.Cancel_Button.Size = New System.Drawing.Size(112, 27)
        Me.Cancel_Button.TabIndex = 1
        Me.Cancel_Button.Text = "Cancel"
        '
        'OK_Button
        '
        Me.OK_Button.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.OK_Button.Location = New System.Drawing.Point(445, 6)
        Me.OK_Button.Name = "OK_Button"
        Me.OK_Button.Size = New System.Drawing.Size(119, 27)
        Me.OK_Button.TabIndex = 0
        Me.OK_Button.Text = "OK"
        '
        'PanelControl1
        '
        Me.PanelControl1.Controls.Add(Me.Caption)
        Me.PanelControl1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.PanelControl1.Location = New System.Drawing.Point(0, 0)
        Me.PanelControl1.Name = "PanelControl1"
        Me.PanelControl1.Padding = New System.Windows.Forms.Padding(8)
        Me.PanelControl1.Size = New System.Drawing.Size(694, 221)
        Me.PanelControl1.TabIndex = 2
        '
        'ConfirmPanel
        '
        Me.ConfirmPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
        Me.ConfirmPanel.Controls.Add(Me.OK_Button)
        Me.ConfirmPanel.Controls.Add(Me.btnTranslate)
        Me.ConfirmPanel.Controls.Add(Me.Cancel_Button)
        Me.ConfirmPanel.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.ConfirmPanel.Location = New System.Drawing.Point(0, 221)
        Me.ConfirmPanel.Name = "ConfirmPanel"
        Me.ConfirmPanel.Size = New System.Drawing.Size(694, 40)
        Me.ConfirmPanel.TabIndex = 2
        '
        'btnTranslate
        '
        Me.btnTranslate.Location = New System.Drawing.Point(10, 6)
        Me.btnTranslate.Name = "btnTranslate"
        Me.btnTranslate.Size = New System.Drawing.Size(112, 27)
        Me.btnTranslate.TabIndex = 1
        Me.btnTranslate.Text = "Translate"
        '
        'MsgBox
        '
        Me.AcceptButton = Me.OK_Button
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.Cancel_Button
        Me.ClientSize = New System.Drawing.Size(694, 261)
        Me.Controls.Add(Me.PanelControl1)
        Me.Controls.Add(Me.ConfirmPanel)
        Me.FormBorderEffect = DevExpress.XtraEditors.FormBorderEffect.Shadow
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow
        Me.Name = "MsgBox"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "pbsDialog"
        CType(Me.PanelControl1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.PanelControl1.ResumeLayout(False)
        CType(Me.ConfirmPanel, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ConfirmPanel.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents Caption As RichTextBox
    Friend WithEvents Cancel_Button As SimpleButton
    Friend WithEvents OK_Button As SimpleButton
    Friend WithEvents PanelControl1 As DevExpress.XtraEditors.PanelControl
    Friend WithEvents ConfirmPanel As DevExpress.XtraEditors.PanelControl
    Friend WithEvents btnTranslate As SimpleButton

End Class
