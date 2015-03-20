Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Public Class FrmMain
    'Tabela do codec de áudio VAG
    Dim f(,) As Double = {{0.0, 0.0}, _
        {60.0 / 64.0, 0.0}, _
        {115.0 / 64.0, -52.0 / 64.0}, _
        {98.0 / 64.0, -55.0 / 64.0}, _
        {122.0 / 64.0, -60.0 / 64.0}}

    Dim Selected_Index As Integer
    Private Enum SDT
        Unknow
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
    Dim VAG1_Section() As Byte

    Dim Current_Opened_File As String
    Private Sub DialogList_SelectedIndexChanged(sender As Object, e As EventArgs) Handles DialogList.SelectedIndexChanged
        Apply()
        If DialogList.SelectedIndices.Count > 0 Then
            Select Case Format
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
        Current_Opened_File = File_Name
        Dim Input As New FileStream(File_Name, FileMode.Open)
        Dim Reader As New BinaryReader(Input)

        Dim Out As String = Nothing
        Header = Nothing
        VAG1_Section = Nothing
        Dialogs = New List(Of String)
        Vox_Dialogs = New List(Of Vox_Dialog)
        Clear()

        If Utils.Read_String(Input, 0, 4) = "LCGB" Then
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
                Format = SDT.Unknow
                Exit Sub
            End If

            Dim Temp As UInt32 = Input.Position
            Input.Seek(PACB_Offset - &HC, SeekOrigin.Begin)
            Dim VAG1_Offset As UInt32 = Reader.ReadUInt32() + (PACB_Offset - &H10)
            Input.Seek(Temp, SeekOrigin.Begin)

            Dim Section_Length As UInt32 = Reader.ReadInt32() + PACB_Offset + 8
            While Input.Position < Section_Length
                Dim Offset As Integer
                Dim Dialog As Vox_Dialog
                With Dialog
                    .Offset_1 = Reader.ReadUInt32()
                    .Offset_2 = Reader.ReadUInt32()
                    .Offset_3 = Reader.ReadUInt32()
                    Dim Dialog_Length As UInt16 = Reader.ReadUInt16()
                    .Language_ID = Reader.ReadUInt16()
                    Offset = Input.Position
                    .Text = Utils.Read_String(Input, Input.Position)
                End With
                Vox_Dialogs.Add(Dialog)
                If Not Silent_Mode Then DialogList.Items.Add("0x" & Hex(Offset).PadLeft(8, "0"c) & " [" & Crop_Text(Dialog.Text) & "]")
            End While

            'Lê dados adicionais do arquivo (áudio)
            Input.Seek(0, SeekOrigin.Begin)
            ReDim Header(PACB_Offset - 1)
            Input.Read(Header, 0, Header.Length)

            Input.Seek(VAG1_Offset, SeekOrigin.Begin)
            ReDim VAG1_Section(Input.Length - VAG1_Offset - 1)
            Input.Read(VAG1_Section, 0, VAG1_Section.Length)

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
        Save(Current_Opened_File)
        MessageBox.Show("O arquivo foi alterado com sucesso!", "Feito", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub
    Private Sub Save(File_Name As String)
        Dim Output As New FileStream(File_Name, FileMode.Create)
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
                        Dim Buffer() As Byte = Adapt_Text(Dialog)
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
                Dim PACB_Length As Integer = 8
                For Each Dialog As Vox_Dialog In Vox_Dialogs
                    If Dialog.Text IsNot Nothing Then
                        Dim Text() As Byte = Adapt_Text(Dialog.Text)
                        PACB_Length += Text.Length + 1 + &H10
                    Else
                        PACB_Length += &H11
                    End If
                Next
                Dim VAG1_Offset As Integer = Header.Length + PACB_Length
                Do
                    If (VAG1_Offset And &HF) = 0 Then Exit Do
                    VAG1_Offset += 1
                Loop
                For Each Dialog As Vox_Dialog In Vox_Dialogs
                    Writer.Write(Convert.ToUInt32(Dialog.Offset_1))
                    Writer.Write(Convert.ToUInt32(Dialog.Offset_2))
                    Writer.Write(Convert.ToUInt32(Dialog.Offset_3))
                    If Dialog.Text IsNot Nothing Then
                        Dim Text() As Byte = Adapt_Text(Dialog.Text)
                        Writer.Write(Convert.ToUInt16(Text.Length + 1 + &H10))
                        Writer.Write(Dialog.Language_ID)
                        Writer.Write(Text)
                        Writer.Write(Convert.ToByte(0))
                    Else
                        Writer.Write(Convert.ToUInt16(&H11))
                        Writer.Write(Dialog.Language_ID)
                        Writer.Write(Convert.ToByte(0))
                    End If
                Next

                Dim Temp_1 As UInt32 = Output.Position - Header.Length - 8

                Output.Seek(VAG1_Offset, SeekOrigin.Begin)
                Writer.Write(VAG1_Section)

                Output.Seek(Header.Length - &HC, SeekOrigin.Begin)
                Writer.Write(VAG1_Offset - (Header.Length - &H10))

                Output.Seek(Header.Length + 4, SeekOrigin.Begin)
                Writer.Write(Temp_1)
        End Select

        Output.Close()
    End Sub

    Private Sub BtnExport_Click(sender As Object, e As EventArgs) Handles BtnExport.Click
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

        Dim SaveDlg As New SaveFileDialog
        SaveDlg.Filter = "Texto|*.txt"
        If SaveDlg.ShowDialog = DialogResult.OK Then
            File.WriteAllText(SaveDlg.FileName, Out.ToString())
            Export_Wave(SaveDlg.FileName & ".wav")
        End If
    End Sub
    Private Sub Export_Wave(Wave_File As String)
        If VAG1_Section Is Nothing Then Exit Sub

        Dim Header_Offset As Integer
        Dim Magic As String = Nothing
        For Offset As Integer = 0 To VAG1_Section.Length - 1 Step 4
            Magic = Utils.Read_String(VAG1_Section, Offset, 4)
            If Magic = "VAG1" Or Magic = "VAG2" Then
                Header_Offset = Offset
                Exit For
            ElseIf Offset = VAG1_Section.Length - 1 Then
                Exit Sub
            End If
        Next

        Dim Channels As Integer = 1
        Dim Bits_Per_Sample As UInt32 = 16
        Dim Sample_Rate As UInt32 = Read_32_BE(VAG1_Section, Header_Offset + &H10)

        Dim Audio_Data_Offset As UInt32 = Header_Offset + &H60
        Dim Wave As New FileStream(Wave_File, FileMode.Create)
        Dim Writer As New BinaryWriter(Wave)
        Writer.Write(Encoding.ASCII.GetBytes("RIFF"), 0, 4)
        Writer.Write(Convert.ToUInt32(0)) 'Place Holder
        Writer.Write(Encoding.ASCII.GetBytes("WAVE"), 0, 4)
        Writer.Write(Encoding.ASCII.GetBytes("fmt "), 0, 4)
        Writer.Write(Convert.ToUInt32(16)) '16 bytes de dados do formato
        Writer.Write(Convert.ToUInt16(1)) 'PCM
        Writer.Write(Convert.ToUInt16(Channels))
        Writer.Write(Sample_Rate)
        Writer.Write(Convert.ToUInt32((Sample_Rate * Bits_Per_Sample * Channels) / 8)) 'Bytes por segundo
        Writer.Write(Convert.ToUInt16((Bits_Per_Sample * Channels) / 8))
        Writer.Write(Convert.ToUInt16(Bits_Per_Sample))
        Writer.Write(Encoding.ASCII.GetBytes("data"), 0, 4)
        Writer.Write(Convert.ToUInt32(0)) 'Place Holder

        Dim Interleave As Integer = &H800
        Dim Factors(((Bits_Per_Sample * Channels) / 8) - 1) As Double
        Dim Wave_Samples((Channels * 28) - 1) As Integer
        Dim _Offset As Integer

        Do
            Array.Clear(Wave_Samples, 0, Wave_Samples.Length)

            For i As Integer = 0 To Channels - 1
                If Audio_Data_Offset + _Offset + 16 > VAG1_Section.Length - 1 Then Exit Do
                Dim Predict_Shift As Byte = VAG1_Section(Audio_Data_Offset + _Offset)
                Dim Flags As Byte = VAG1_Section(Audio_Data_Offset + _Offset + 1)
                Dim s(13) As Byte
                Buffer.BlockCopy(VAG1_Section, Audio_Data_Offset + _Offset + 2, s, 0, 14)
                _Offset += 16
                'Nota para gdkchan: Lembrar de ser preguiçoso e deixar o áudio Stereo!
                If Magic = "VAG2" Then If _Offset > 0 And (_Offset Mod Interleave) = 0 Then _Offset += Interleave

                If Flags And 2 Then 'Isso está certo?
                    Dim Shift As Byte = Predict_Shift And &HF
                    Dim Predict As Byte = (Predict_Shift And &HF0) >> 4
                    Dim Samples(28 - 1) As Integer

                    For j As Integer = 0 To 13
                        Samples(j * 2) = s(j) And &HF
                        Samples(j * 2 + 1) = (s(j) And &HF0) >> 4
                    Next

                    For j As Integer = 0 To 27
                        Dim _s As Integer = Samples(j) << 12
                        If _s And &H8000 Then _s = _s Or &HFFFF0000

                        Dim Sample As Double = (_s >> Shift) + Factors(i * 2) * f(Predict, 0) + Factors(i * 2 + 1) * f(Predict, 1)
                        Factors(i * 2 + 1) = Factors(i * 2)
                        Factors(i * 2) = Sample
                        Wave_Samples(j * Channels + i) = Convert.ToInt32(Sample)
                    Next
                End If
            Next

            For i As Integer = 0 To Wave_Samples.Length - 1
                Dim Value As Integer = Wave_Samples(i)
                If Value < 0 Then Value += &H10000
                Wave.WriteByte(Value And &HFF)
                Wave.WriteByte((Value >> 8) And &HFF)
            Next
        Loop

        Wave.Seek(4, SeekOrigin.Begin)
        Writer.Write(Convert.ToUInt32(Wave.Length - 8))
        Wave.Seek(40, SeekOrigin.Begin)
        Writer.Write(Convert.ToUInt32(Wave.Length - 36))

        Wave.Close()
    End Sub

    Private Sub BtnImport_Click(sender As Object, e As EventArgs) Handles BtnImport.Click
        Dim OpenDlg As New OpenFileDialog
        OpenDlg.Filter = "Texto|*.txt"
        If OpenDlg.ShowDialog = DialogResult.OK AndAlso File.Exists(OpenDlg.FileName) Then
            Import(OpenDlg.FileName)
        End If
    End Sub
    Private Sub Import(File_Name As String, Optional Silent_Mode As Boolean = False)
        Dim Text As String = File.ReadAllText(File_Name)
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
                    If Not Silent_Mode Then DialogList.Items.Add("0x???????? [" & Crop_Text(Data) & "]")
                Case SDT.Vox
                    Dim Temp() As Vox_Dialog = Vox_Dialogs.ToArray()
                    Temp(Index).Text = Data
                    Vox_Dialogs = Temp.ToList()
            End Select

            Index += 1
        Next
    End Sub

    Private Sub FrmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim Dumped, Inserted As Boolean
        Dim Insertion_Count As Integer

        If Environment.GetCommandLineArgs().Length > 1 Then
            Dim Dump_Dir As String = Path.Combine(Application.StartupPath, "Dump")
            Directory.CreateDirectory(Dump_Dir)
            Select Case Environment.GetCommandLineArgs()(1)
                Case "-e", "-esnd"
                    Dim Extract_Audio As Boolean
                    If Environment.GetCommandLineArgs()(1) = "-esnd" Then Extract_Audio = True
                    If Directory.Exists(Environment.GetCommandLineArgs()(2)) Then
                        Dim Files() As String = Directory.GetFiles(Environment.GetCommandLineArgs()(2), "*.sdt")
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
                            If Extract_Audio Then Export_Wave(Path.Combine(Dump_Dir, Path.GetFileNameWithoutExtension(Arquivo) & ".wav"))
                        Next
                        Dumped = True
                    End If

                Case "-i"
                    If Directory.Exists(Environment.GetCommandLineArgs()(2)) And Directory.Exists(Environment.GetCommandLineArgs()(3)) Then
                        Dim Files() As String = Directory.GetFiles(Environment.GetCommandLineArgs()(2), "*.sdt")
                        For Each Arquivo As String In Files
                            Open(Arquivo, True)
                            Dim Dump As String = Path.Combine(Environment.GetCommandLineArgs()(3), Path.GetFileNameWithoutExtension(Arquivo)) & ".txt"
                            If File.Exists(Dump) Then
                                Import(Dump, True)
                                Save(Current_Opened_File)
                                Insertion_Count += 1
                            End If
                        Next
                        Inserted = True
                    End If
            End Select

        End If

        If Dumped Then MessageBox.Show("Arquivos dumpados com sucesso!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information)
        If Inserted Then MessageBox.Show("Foram inseridos " & Insertion_Count & " arquivo(s)!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Function Adapt_Text(Text As String) As Byte()
        Dim Out As New List(Of Byte)
        For i As Integer = 0 To Text.Length - 1
            Dim Value As Byte = Asc(Text.Substring(i, 1))
            If Value = &HD Then
                Out.Add(&HA)
                i += 1
            ElseIf Value = &HA Then
                Out.Add(&HA)
            ElseIf Text.Substring(i, 1) = "\" Then
                Value = Convert.ToByte(Text.Substring(i + 3, 2), 16)
                Out.Add(Value)
                i += 4
            Else
                Out.AddRange(Encoding.UTF8.GetBytes(Text.Substring(i, 1)).ToList())
            End If
        Next
        Return Out.ToArray()
    End Function

    Private Function Read_32_BE(Data() As Byte, Address As UInt32) As UInt32
        Return (Data(Address) * &H1000000) Or _
                (Data(Address + 1) * &H10000) Or _
                (Data(Address + 2) * &H100) Or
                Data(Address + 3)
    End Function
End Class
