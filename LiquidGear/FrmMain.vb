Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Public Class FrmMain
    Dim Selected_Index As Integer
    Private Enum SDT
        Codec
        Vox
    End Enum
    Dim Format As SDT

    'SDT Codec
    Dim Dialogs As List(Of String)

    Dim Script_Header() As Byte
    Dim Script_Data() As Byte
    Dim Block_2_Table() As Byte

    'SDT Vox
    Private Structure Vox_Dialog
        Dim Offset_1 As UInt32
        Dim Offset_2 As UInt32
        Dim Offset_3 As UInt32
        Dim Language_ID As UInt16
        Dim Text As String
    End Structure
    Dim Vox_Dialogs As List(Of Vox_Dialog)

    Dim Header() As Byte
    Dim PACB_Appendix() As Byte
    Dim VAG1_Section() As Byte
    Private Sub DialogList_SelectedIndexChanged(sender As Object, e As EventArgs) Handles DialogList.SelectedIndexChanged
        Apply()
        If DialogList.SelectedIndices.Count > 0 Then
            Select Format
                Case SDT.Codec : TxtDialogs.Text = Dialogs(DialogList.SelectedIndices(0))
                Case SDT.Vox : TxtDialogs.Text = Vox_Dialogs(DialogList.SelectedIndices(0)).Text
            End Select
            Selected_Index = DialogList.SelectedIndices(0)
        Else
            TxtDialogs.Text = Nothing
            Selected_Index = -1
        End If
    End Sub

    Private Sub Apply()
        If Selected_Index > -1 Then
            Select Case Format
                Case SDT.Codec
                    Dim Temp() As String = Dialogs.ToArray()
                    Temp(Selected_Index) = TxtDialogs.Text
                    Dialogs = Temp.ToList()
                Case SDT.Vox
                    Dim Temp() As Vox_Dialog = Vox_Dialogs.ToArray()
                    Temp(Selected_Index).Text = TxtDialogs.Text
                    Vox_Dialogs = Temp.ToList()
            End Select
        End If
    End Sub
    Private Function Crop_Text(Text As String) As String
        If Text IsNot Nothing Then
            If Text.Length < 20 Then
                Return Text
            Else
                Return Text.Substring(0, 10) & "..."
            End If
        End If

        Return Nothing
    End Function
    Private Sub Clear()
        DialogList.Items.Clear()
        Selected_Index = -1
        TxtDialogs.Text = Nothing
    End Sub

    Private Sub BtnOpen_Click(sender As Object, e As EventArgs) Handles BtnOpen.Click
        Dim OpenDlg As New OpenFileDialog
        OpenDlg.Filter = "Script do MGS|*.sdt"
        If OpenDlg.ShowDialog = DialogResult.OK AndAlso File.Exists(OpenDlg.FileName) Then
            Open(OpenDlg.FileName, False)
        End If
    End Sub
    Private Sub Open(File_Name As String, Silent_Mode As Boolean)
        Dim Input As New FileStream(File_Name, FileMode.Open)
        Dim Reader As New BinaryReader(Input)

        Dim Out As String = Nothing

        If Utils.Read_String(Input, 0, 4) = "LCGB" Then
            'Limpa
            Dialogs = New List(Of String)
            Clear()

            'Carrega
            Dim Language As UInt16 = Reader.ReadUInt16()
            Reader.ReadUInt16()
            While Reader.ReadInt32() <> &HFFFFFFFF
                If Input.Position = Input.Length Then Exit Sub
            End While

            Dim Header_Offset As UInt32 = Input.Position
            Dim Script_Offset As UInt32 = Reader.ReadUInt32() + Header_Offset
            Dim Block_2_Table_Offset As UInt32 = Reader.ReadUInt32() + Header_Offset
            Dim Block_2_Text_Offset As UInt32 = Reader.ReadUInt32() + Header_Offset
            Dim Section_Length As UInt32 = Reader.ReadUInt32()
            Reader.ReadUInt32() '0x0
            Dim Text_Offset As UInt32 = Reader.ReadUInt32()
            Dim Text_Entries As UInt32 = Reader.ReadUInt32()

            Dim Position As UInt32 = Input.Position
            For Entry As Integer = 0 To Text_Entries - 1
                Input.Seek(Position + Entry * 4, SeekOrigin.Begin)
                Dim Offset As UInt32 = Header_Offset + Text_Offset + Reader.ReadUInt32()
                Dim Text As String = Utils.Read_String(Input, Offset)
                Dialogs.Add(Text)
                If Not Silent_Mode Then DialogList.Items.Add("0x" & Hex(Offset).PadLeft(8, "0"c) & " [" & Crop_Text(Text) & "]")
            Next

            'Lê dados adicionais do arquivo (relacionados ao script?)
            Input.Seek(0, SeekOrigin.Begin)
            ReDim Script_Header(Header_Offset - 1)
            Input.Read(Script_Header, 0, Script_Header.Length)

            Input.Seek(Section_Length + Header_Offset, SeekOrigin.Begin)
            ReDim Script_Data(Input.Length - Input.Position - 1)
            Input.Read(Script_Data, 0, Script_Data.Length)

            Input.Seek(Block_2_Table_Offset, SeekOrigin.Begin)
            ReDim Block_2_Table(Block_2_Text_Offset - Block_2_Table_Offset - 1)
            Input.Read(Block_2_Table, 0, Block_2_Table.Length)

            '---

            Format = SDT.Codec
        Else
            'Limpa
            Vox_Dialogs = New List(Of Vox_Dialog)
            Clear()

            'Carrega
            Dim PACB_Offset As UInt32
            For Offset As Integer = 0 To Input.Length - 1 Step 4
                If Utils.Read_String(Input, Offset, 4) = "PACB" Then
                    PACB_Offset = Offset
                    Exit For
                End If
            Next
            If PACB_Offset = 0 Then
                If Not Silent_Mode Then MessageBox.Show("Formato não suportado!" & Environment.NewLine & File_Name, "Erro!", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Input.Close()
                Exit Sub
            End If

            Dim Section_Length As UInt32 = Reader.ReadInt32() + PACB_Offset + 8
            While Input.Position < Section_Length
                Dim Dialog As Vox_Dialog
                Dialog.Offset_1 = Reader.ReadUInt32()
                Dialog.Offset_2 = Reader.ReadUInt32()
                Dialog.Offset_3 = Reader.ReadUInt32()
                Dim Dialog_Length As UInt16 = Reader.ReadUInt16()
                Dialog.Language_ID = Reader.ReadUInt16()
                Dim Offset As Integer = Input.Position
                Dialog.Text = Utils.Read_String(Input, Input.Position)
                Vox_Dialogs.Add(Dialog)
                If Not Silent_Mode Then DialogList.Items.Add("0x" & Hex(Offset).PadLeft(8, "0"c) & " [" & Crop_Text(Dialog.Text) & "]")
            End While

            Input.Seek(PACB_Offset - &HC, SeekOrigin.Begin)
            Dim VAG1_Offset As UInt32 = Reader.ReadUInt32() + PACB_Offset

            'Lê dados adicionais do arquivo (texturas?)
            Input.Seek(0, SeekOrigin.Begin)
            ReDim Header(PACB_Offset - 1)
            Input.Read(Header, 0, Header.Length)

            Input.Seek(Section_Length, SeekOrigin.Begin)
            Dim Length As UInt32 = VAG1_Offset - Section_Length
            ReDim PACB_Appendix(Length - 1)
            Input.Read(PACB_Appendix, 0, Length)

            Input.Seek(VAG1_Offset, SeekOrigin.Begin)
            Length = Input.Length - VAG1_Offset
            ReDim VAG1_Section(Length - 1)
            Input.Read(VAG1_Section, 0, Length)

            '---

            Format = SDT.Vox
        End If

        BtnSave.Enabled = True
        BtnExport.Enabled = True
        BtnImport.Enabled = True

        Input.Close()
    End Sub
    Private Sub BtnSave_Click(sender As Object, e As EventArgs) Handles BtnSave.Click
        Apply()

        Dim SaveDlg As New SaveFileDialog
        SaveDlg.Filter = "Script do MGS|*.sdt"
        If SaveDlg.ShowDialog = DialogResult.OK Then
            Dim Output As New FileStream(SaveDlg.FileName, FileMode.Create)
            Dim Writer As New BinaryWriter(Output)

            Select Case Format
                Case SDT.Codec
                    Writer.Write(Script_Header)
                    Dim Header_Offset As UInt32 = Output.Position
                    Writer.Write(Convert.ToUInt32(0)) 'Script Offset (Place Holder)
                    Writer.Write(Convert.ToUInt32(0)) 'Block 2 Table Offset (Place Holder)
                    Writer.Write(Convert.ToUInt32(0)) 'Block 2 Text Offset (Place Holder)
                    Writer.Write(Convert.ToUInt32(0)) 'Section Length (Place Holder)
                    Writer.Write(Convert.ToUInt32(0)) '0x0
                    Dim Text_Offset As UInt32 = &H18 'Sempre 0x18?
                    Writer.Write(Text_Offset)
                    Writer.Write(Convert.ToUInt32(Dialogs.Count))
                    Dim Str_Header_Offset As UInt32 = Output.Position
                    Dim Str_Offset As UInt32 = Output.Position + Dialogs.Count * 4

                    Dim Temp_Buffer As New MemoryStream()
                    For Each Dialog As String In Dialogs
                        Output.Seek(Str_Header_Offset, SeekOrigin.Begin)
                        Writer.Write(Str_Offset - Header_Offset - Text_Offset)
                        Str_Header_Offset += 4

                        Output.Seek(Str_Offset, SeekOrigin.Begin)
                        If Dialog IsNot Nothing Then
                            Dim Buffer() As Byte = Encoding.UTF8.GetBytes(Dialog.Replace(Environment.NewLine, Convert.ToChar(&HA)))
                            Writer.Write(Buffer)
                            Temp_Buffer.Write(Buffer, 0, Buffer.Length)
                            Str_Offset += Buffer.Length
                        End If
                        Writer.Write(Convert.ToByte(0))
                        Temp_Buffer.WriteByte(0)
                        Str_Offset += 1
                    Next

                    Dim Temp As UInt32 = Output.Position
                    Output.Seek(Header_Offset + 4, SeekOrigin.Begin)
                    Writer.Write(Temp - Header_Offset)
                    Writer.Write(Convert.ToUInt32((Temp + Block_2_Table.Length) - Header_Offset))
                    Output.Seek(Temp, SeekOrigin.Begin)

                    Writer.Write(Block_2_Table)
                    Writer.Write(Temp_Buffer.ToArray())
                    Writer.Write(Encoding.ASCII.GetBytes("FON"))
                    Dim Section_Length As UInt32 = Output.Position
                    Output.Seek(Header_Offset + 12, SeekOrigin.Begin)
                    Writer.Write(Section_Length - Header_Offset)
                    Output.Seek(Section_Length, SeekOrigin.Begin)

                    Dim Script_Offset As UInt32 = Output.Position
                    Writer.Write(Script_Data)
                    Output.Seek(Header_Offset, SeekOrigin.Begin)
                    Writer.Write(Convert.ToUInt32((Script_Offset - Header_Offset) + 4))
                Case SDT.Vox
                    Writer.Write(Header)
                    Writer.Write(Encoding.ASCII.GetBytes("PACB"))
                    Writer.Write(Convert.ToUInt32(0)) 'Section Length (Place Holder)
                    For Each Dialog As Vox_Dialog In Vox_Dialogs
                        Writer.Write(Dialog.Offset_1)
                        Writer.Write(Dialog.Offset_2)
                        Writer.Write(Dialog.Offset_3)
                        Dim Text() As Byte = Encoding.UTF8.GetBytes(Dialog.Text.Replace(Environment.NewLine, Convert.ToChar(&HA)))
                        Writer.Write(Convert.ToUInt16(Text.Length + 1 + &H10))
                        Writer.Write(Dialog.Language_ID)
                        Writer.Write(Text)
                        Writer.Write(Convert.ToByte(0))
                    Next

                    Dim Temp_1 As UInt32 = Output.Position - Header.Length - 8

                    Writer.Write(PACB_Appendix)
                    Dim Temp_2 As UInt32 = Output.Position - Header.Length
                    Writer.Write(VAG1_Section)

                    Output.Seek(Header.Length - &HC, SeekOrigin.Begin)
                    Writer.Write(Temp_2)

                    Output.Seek(Header.Length + 4, SeekOrigin.Begin)
                    Writer.Write(Temp_1)
            End Select

            Output.Close()
        End If
    End Sub

    Private Sub BtnExport_Click(sender As Object, e As EventArgs) Handles BtnExport.Click
        Dim Out As New StringBuilder
        Select Format
            Case SDT.Codec
                For Each Dialog As String In Dialogs
                    Out.AppendLine("[texto]" & Environment.NewLine & Dialog & Environment.NewLine & "[/texto]")
                Next
            Case SDT.Vox
                For Each Dialog As Vox_Dialog In Vox_Dialogs
                    Out.AppendLine("[texto]" & Environment.NewLine & Dialog.Text & Environment.NewLine & "[/texto]")
                Next
        End Select

        Dim SaveDlg As New SaveFileDialog
        SaveDlg.Filter = "Texto|*.txt"
        If SaveDlg.ShowDialog = DialogResult.OK Then File.WriteAllText(SaveDlg.FileName, Out.ToString())
    End Sub
    Private Sub BtnImport_Click(sender As Object, e As EventArgs) Handles BtnImport.Click
        Dim OpenDlg As New OpenFileDialog
        OpenDlg.Filter = "Texto|*.txt"
        If OpenDlg.ShowDialog = DialogResult.OK AndAlso File.Exists(OpenDlg.FileName) Then
            Dim Text As String = File.ReadAllText(OpenDlg.FileName)
            Dim Dlgs As MatchCollection = Regex.Matches(Text, "\[texto\]\r\n(.+?)(\r\n)?\[/texto\]", RegexOptions.Singleline)
            Dim Index As Integer

            If Format = SDT.Codec Then
                DialogList.Items.Clear()
                Dialogs = New List(Of String)
            End If

            For Each Dlg As Match In Dlgs
                Dim Data As String = Dlg.Groups(1).Value
                If Data = Environment.NewLine Then Data = Nothing

                Select Case Format
                    Case SDT.Codec
                        Dialogs.Add(Data)
                        DialogList.Items.Add("0x???????? [" & Crop_Text(Data) & "]")
                    Case SDT.Vox
                        Dim Temp() As Vox_Dialog = Vox_Dialogs.ToArray()
                        Temp(Index).Text = Data
                        Vox_Dialogs = Temp.ToList()
                End Select

                Index += 1
            Next
        End If
    End Sub

    Private Sub FrmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim Dumped As Boolean
        If Environment.GetCommandLineArgs().Length > 1 Then
            Dim Dump_Dir As String = Path.Combine(Application.StartupPath, "Dump")
            Directory.CreateDirectory(Dump_Dir)
            If Directory.Exists(Environment.GetCommandLineArgs()(1)) Then
                Dim Files() As String = Directory.GetFiles(Environment.GetCommandLineArgs()(1), "*.sdt")
                For Each Arquivo As String In Files
                    Open(Arquivo, True)

                    Dim Out As New StringBuilder
                    Select Case Format
                        Case SDT.Codec
                            For Each Dialog As String In Dialogs
                                Out.AppendLine("[texto]" & Environment.NewLine & Dialog & Environment.NewLine & "[/texto]")
                            Next
                        Case SDT.Vox
                            For Each Dialog As Vox_Dialog In Vox_Dialogs
                                Out.AppendLine("[texto]" & Environment.NewLine & Dialog.Text & Environment.NewLine & "[/texto]")
                            Next
                    End Select
                    File.WriteAllText(Path.Combine(Dump_Dir, Path.GetFileNameWithoutExtension(Arquivo) & ".txt"), Out.ToString())
                Next
                Dumped = True
            End If
        End If

        If Dumped Then MessageBox.Show("Arquivos dumpados com sucesso!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub
End Class
