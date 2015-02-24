Imports System.Runtime.InteropServices
Public Class gdkForm

#Region "API Mover"
    Public Const WM_NCLBUTTONDOWN As Integer = &HA1
    Public Const WM_NCHITTEST As Integer = &H84
    Public Const HT_CAPTION As Integer = &H2

    <DllImportAttribute("user32.dll")> _
    Public Shared Function SendMessage(hWnd As IntPtr, Msg As Integer, wParam As Integer, lParam As Integer) As Integer
    End Function
    <DllImportAttribute("user32.dll")> _
    Public Shared Function ReleaseCapture() As Boolean
    End Function
#End Region

#Region "API Sombra"
    <DllImport("dwmapi")> _
    Public Shared Function DwmExtendFrameIntoClientArea(ByVal hWnd As IntPtr, ByRef pMarInset As Margins) As Integer
    End Function
    <DllImport("dwmapi")> _
    Public Shared Function DwmSetWindowAttribute(ByVal hWnd As IntPtr, ByVal Attr As Integer, ByRef AttrValue As Integer, ByVal AttrSize As Integer) As Integer
    End Function
    Public Structure Margins
        Dim TopHeight As Integer
        Dim BottomHeight As Integer
        Dim LeftWidth As Integer
        Dim RightWidth As Integer
    End Structure
#End Region

#Region "WndProc Sombra"
    Protected Overrides Sub WndProc(ByRef m As Message)
        Select Case m.Msg
            Case &H85 'Cria sombra (com Aero)
                Dim val = 2
                DwmSetWindowAttribute(Handle, 2, val, 4)
                Dim Margins As New Margins()
                With Margins
                    .TopHeight = 1
                    .BottomHeight = 1
                    .LeftWidth = 1
                    .RightWidth = 1
                End With
                DwmExtendFrameIntoClientArea(Handle, Margins)
        End Select

        MyBase.WndProc(m)
    End Sub
#End Region

#Region "Inicialização/Controles de janela"
    Public Sub New()
        InitializeComponent()
        Me.MinimumSize = New Size(128, 74)
    End Sub

    Private Sub OForm_MouseDown(sender As Object, e As MouseEventArgs) Handles Me.MouseDown, TitleBar.MouseDown, LblTitle.MouseDown
        If e.Button = MouseButtons.Left And e.Y < 28 Then
            ReleaseCapture()
            SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0)
        End If
    End Sub
    Private Sub OForm_MouseMove(sender As Object, e As MouseEventArgs) Handles Me.MouseMove, TitleBar.MouseMove, LblTitle.MouseMove
        SendMessage(Handle, WM_NCHITTEST, 0, CInt(e.X Or (e.Y << 16)))
    End Sub

    Private Sub BtnClose_MouseEnter(sender As Object, e As EventArgs) Handles BtnClose.MouseEnter
        BtnClose.BackgroundImage = My.Resources.hover_red
    End Sub
    Private Sub BtnMinimize_MouseEnter(sender As Object, e As EventArgs) Handles BtnMinimize.MouseEnter
        BtnMinimize.BackgroundImage = My.Resources.hover_minimize
    End Sub
    Private Sub Btn_MouseLeave(sender As Object, e As EventArgs) Handles BtnClose.MouseLeave, BtnMinimize.MouseLeave
        Dim Btn As PictureBox = DirectCast(sender, PictureBox)
        Btn.BackgroundImage = Nothing
    End Sub

    Private Sub BtnClose_Click(sender As Object, e As EventArgs) Handles BtnClose.Click
        Me.Close()
    End Sub
    Private Sub BtnMinimize_Click(sender As Object, e As EventArgs) Handles BtnMinimize.Click
        Me.WindowState = FormWindowState.Minimized
    End Sub
#End Region

End Class