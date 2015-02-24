<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmMain
    Inherits LiquidGear.gdkForm

    'Descartar substituições de formulário para limpar a lista de componentes.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Exigido pelo Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'OBSERVAÇÃO: O procedimento a seguir é exigido pelo Windows Form Designer
    'Ele pode ser modificado usando o Windows Form Designer.  
    'Não o modifique usando o editor de códigos.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.TxtDialogs = New System.Windows.Forms.TextBox()
        Me.DialogList = New System.Windows.Forms.ListView()
        Me.CDiags = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.BtnOpen = New System.Windows.Forms.Button()
        Me.BtnSave = New System.Windows.Forms.Button()
        Me.BtnExport = New System.Windows.Forms.Button()
        Me.BtnImport = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'LblTitle
        '
        Me.LblTitle.Location = New System.Drawing.Point(229, 4)
        Me.LblTitle.Size = New System.Drawing.Size(183, 19)
        Me.LblTitle.Text = "LiquidGear v0.1 by gdkchan"
        '
        'TxtDialogs
        '
        Me.TxtDialogs.Location = New System.Drawing.Point(214, 182)
        Me.TxtDialogs.Multiline = True
        Me.TxtDialogs.Name = "TxtDialogs"
        Me.TxtDialogs.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.TxtDialogs.Size = New System.Drawing.Size(418, 164)
        Me.TxtDialogs.TabIndex = 11
        '
        'DialogList
        '
        Me.DialogList.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.CDiags})
        Me.DialogList.FullRowSelect = True
        Me.DialogList.Location = New System.Drawing.Point(8, 36)
        Me.DialogList.MultiSelect = False
        Me.DialogList.Name = "DialogList"
        Me.DialogList.Size = New System.Drawing.Size(200, 340)
        Me.DialogList.TabIndex = 12
        Me.DialogList.UseCompatibleStateImageBehavior = False
        Me.DialogList.View = System.Windows.Forms.View.Details
        '
        'CDiags
        '
        Me.CDiags.Text = "Diálogos"
        Me.CDiags.Width = 175
        '
        'BtnOpen
        '
        Me.BtnOpen.Location = New System.Drawing.Point(552, 352)
        Me.BtnOpen.Name = "BtnOpen"
        Me.BtnOpen.Size = New System.Drawing.Size(80, 24)
        Me.BtnOpen.TabIndex = 13
        Me.BtnOpen.Text = "&Abrir"
        Me.BtnOpen.UseVisualStyleBackColor = True
        '
        'BtnSave
        '
        Me.BtnSave.Enabled = False
        Me.BtnSave.Location = New System.Drawing.Point(466, 352)
        Me.BtnSave.Name = "BtnSave"
        Me.BtnSave.Size = New System.Drawing.Size(80, 24)
        Me.BtnSave.TabIndex = 14
        Me.BtnSave.Text = "&Salvar"
        Me.BtnSave.UseVisualStyleBackColor = True
        '
        'BtnExport
        '
        Me.BtnExport.Enabled = False
        Me.BtnExport.Location = New System.Drawing.Point(372, 352)
        Me.BtnExport.Name = "BtnExport"
        Me.BtnExport.Size = New System.Drawing.Size(80, 24)
        Me.BtnExport.TabIndex = 15
        Me.BtnExport.Text = "&Exportar"
        Me.BtnExport.UseVisualStyleBackColor = True
        '
        'BtnImport
        '
        Me.BtnImport.Enabled = False
        Me.BtnImport.Location = New System.Drawing.Point(286, 352)
        Me.BtnImport.Name = "BtnImport"
        Me.BtnImport.Size = New System.Drawing.Size(80, 24)
        Me.BtnImport.TabIndex = 16
        Me.BtnImport.Text = "&Importar"
        Me.BtnImport.UseVisualStyleBackColor = True
        '
        'FrmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.ClientSize = New System.Drawing.Size(640, 384)
        Me.Controls.Add(Me.BtnImport)
        Me.Controls.Add(Me.BtnExport)
        Me.Controls.Add(Me.BtnSave)
        Me.Controls.Add(Me.BtnOpen)
        Me.Controls.Add(Me.DialogList)
        Me.Controls.Add(Me.TxtDialogs)
        Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Name = "FrmMain"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Controls.SetChildIndex(Me.TxtDialogs, 0)
        Me.Controls.SetChildIndex(Me.DialogList, 0)
        Me.Controls.SetChildIndex(Me.BtnOpen, 0)
        Me.Controls.SetChildIndex(Me.BtnSave, 0)
        Me.Controls.SetChildIndex(Me.BtnExport, 0)
        Me.Controls.SetChildIndex(Me.BtnImport, 0)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents TxtDialogs As System.Windows.Forms.TextBox
    Friend WithEvents DialogList As System.Windows.Forms.ListView
    Friend WithEvents BtnOpen As System.Windows.Forms.Button
    Friend WithEvents CDiags As System.Windows.Forms.ColumnHeader
    Friend WithEvents BtnSave As System.Windows.Forms.Button
    Friend WithEvents BtnExport As System.Windows.Forms.Button
    Friend WithEvents BtnImport As System.Windows.Forms.Button

End Class
