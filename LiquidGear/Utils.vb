Imports System.IO
Imports System.Text
Public Class Utils
    Public Shared Function Read_String(Data As FileStream, Offset As Integer, Length As Integer) As String
        Data.Seek(Offset, SeekOrigin.Begin)
        Dim Text As String = Nothing
        For i As Integer = 0 To Length - 1
            Text &= Chr(Data.ReadByte())
        Next
        Return Text
    End Function
    Public Shared Function Read_String(Data As FileStream, Offset As Integer) As String
        Data.Seek(Offset, SeekOrigin.Begin)
        Dim Text As String = Nothing
        Dim CurrByte As Byte = Data.ReadByte()
        While CurrByte <> 0
            If CurrByte = &HA Then '0x0A (LF)
                Text &= Environment.NewLine
            ElseIf CurrByte = &H1F Then 'Espanhol
                CurrByte = Data.ReadByte()
                Text &= Chr((CurrByte + &HDA) And &HFF)
            ElseIf CurrByte >= &HC2 And CurrByte <= &HC5 Then 'UTF8
                Dim Buffer(1) As Byte
                Buffer(0) = CurrByte
                Buffer(1) = Data.ReadByte()
                Text &= Encoding.UTF8.GetString(Buffer)
            Else 'ANSI
                If CurrByte > &H7F Then
                    Text &= "\0x" & Hex(CurrByte).PadLeft(2, "0"c)
                Else
                    Text &= Chr(CurrByte)
                End If
            End If
            CurrByte = Data.ReadByte()
        End While
        Return Text
    End Function
End Class
