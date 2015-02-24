<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class gdkForm
    Inherits System.Windows.Forms.Form

    'Descartar substituições de formulário para limpar a lista de componentes.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Exigido pelo Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'OBSERVAÇÃO: O procedimento a seguir é exigido pelo Windows Form Designer
    'Ele pode ser modificado usando o Windows Form Designer.  
    'Não o modifique usando o editor de códigos.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(gdkForm))
        Me.TitleBar = New System.Windows.Forms.Panel()
        Me.BtnMinimize = New System.Windows.Forms.PictureBox()
        Me.BtnClose = New System.Windows.Forms.PictureBox()
        Me.LblTitle = New System.Windows.Forms.Label()
        Me.TitleBar.SuspendLayout()
        CType(Me.BtnMinimize, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.BtnClose, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'TitleBar
        '
        Me.TitleBar.BackColor = System.Drawing.Color.Transparent
        Me.TitleBar.Controls.Add(Me.BtnMinimize)
        Me.TitleBar.Controls.Add(Me.BtnClose)
        Me.TitleBar.Controls.Add(Me.LblTitle)
        Me.TitleBar.Dock = System.Windows.Forms.DockStyle.Top
        Me.TitleBar.Location = New System.Drawing.Point(0, 0)
        Me.TitleBar.Name = "TitleBar"
        Me.TitleBar.Size = New System.Drawing.Size(640, 28)
        Me.TitleBar.TabIndex = 10
        '
        'BtnMinimize
        '
        Me.BtnMinimize.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BtnMinimize.BackColor = System.Drawing.Color.Transparent
        Me.BtnMinimize.Image = CType(resources.GetObject("BtnMinimize.Image"), System.Drawing.Image)
        Me.BtnMinimize.Location = New System.Drawing.Point(596, 4)
        Me.BtnMinimize.Name = "BtnMinimize"
        Me.BtnMinimize.Size = New System.Drawing.Size(20, 20)
        Me.BtnMinimize.TabIndex = 3
        Me.BtnMinimize.TabStop = False
        '
        'BtnClose
        '
        Me.BtnClose.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BtnClose.BackColor = System.Drawing.Color.Transparent
        Me.BtnClose.Image = CType(resources.GetObject("BtnClose.Image"), System.Drawing.Image)
        Me.BtnClose.Location = New System.Drawing.Point(616, 4)
        Me.BtnClose.Name = "BtnClose"
        Me.BtnClose.Size = New System.Drawing.Size(20, 20)
        Me.BtnClose.TabIndex = 1
        Me.BtnClose.TabStop = False
        '
        'LblTitle
        '
        Me.LblTitle.Anchor = System.Windows.Forms.AnchorStyles.Top
        Me.LblTitle.AutoSize = True
        Me.LblTitle.BackColor = System.Drawing.Color.Transparent
        Me.LblTitle.Font = New System.Drawing.Font("Segoe UI Semibold", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblTitle.ForeColor = System.Drawing.Color.Black
        Me.LblTitle.Location = New System.Drawing.Point(271, 4)
        Me.LblTitle.Name = "LblTitle"
        Me.LblTitle.Size = New System.Drawing.Size(98, 19)
        Me.LblTitle.TabIndex = 12
        Me.LblTitle.Text = "[Place Holder]"
        Me.LblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'gdkForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackgroundImage = CType(resources.GetObject("$this.BackgroundImage"), System.Drawing.Image)
        Me.ClientSize = New System.Drawing.Size(640, 384)
        Me.Controls.Add(Me.TitleBar)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Name = "gdkForm"
        Me.Text = "gdkForm"
        Me.TitleBar.ResumeLayout(False)
        Me.TitleBar.PerformLayout()
        CType(Me.BtnMinimize, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.BtnClose, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents TitleBar As System.Windows.Forms.Panel
    Private WithEvents BtnMinimize As System.Windows.Forms.PictureBox
    Private WithEvents BtnClose As System.Windows.Forms.PictureBox
    Protected WithEvents LblTitle As System.Windows.Forms.Label
End Class
