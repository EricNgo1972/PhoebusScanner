Imports pbs.Helper

Public Class MsgBox

#Region " Dialog Sub for use "
    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.TopMost = True
        Me.Caption.Text = String.Empty
    End Sub

    Private _originalText As String = String.Empty

    Public Shared Function Confirm(ByVal format As String, ByVal ParamArray params As Object()) As System.Windows.Forms.DialogResult
        Dim dlg As New MsgBox
        dlg.Text = ResStr(".:| Information |:.")

        dlg._originalText = format
        dlg.Caption.Text = String.Format(format, params)

        Dim ret = dlg.ShowDialog
        dlg.Dispose()
        Return ret

    End Function

    Public Shared Function Confirm(ByVal question As String) As System.Windows.Forms.DialogResult
        Dim dlg As New MsgBox
        dlg.Text = ResStr(".:| Information |:.")

        dlg._originalText = question

        dlg.Caption.Text = question

        Dim ret = dlg.ShowDialog
        dlg.Dispose()
        Return ret

    End Function
    Public Shared Function ShowError(ByVal ErrorMsg As String) As System.Windows.Forms.DialogResult
        Dim dlg As New MsgBox
        dlg.Text = ResStr(".:| Error |:.")

        dlg._originalText = ErrorMsg

        dlg.Caption.Text = ErrorMsg

        Dim ret = dlg.ShowDialog
        dlg.Dispose()
        Return ret


    End Function

    Public Shared Function ShowError(ByVal Ex As Exception) As System.Windows.Forms.DialogResult
        Dim dlg As New MsgBox

        dlg.Text = ResStr(".:| Error |:.")

        dlg.Caption.Text = Ex.ToString

        Dim ret = dlg.ShowDialog
        dlg.Dispose()
        Return ret


    End Function

#End Region

    Private Sub btnTranslate_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnTranslate.Click
        Try
            Me.TopMost = False
            If Not String.IsNullOrEmpty(Me._originalText) Then
                Dim _key = Me._originalText
                Dim translation As String = _key

                Dim translated = InputBox(String.Format("Translate '{0}'", _key), "Help transalate Phoebus to your language", _
                                        translation)

                If Not String.IsNullOrEmpty(translated) AndAlso translated <> translation Then
                    If translated = "!" Then translated = String.Empty

                    If pbs.Helper.UserVocabulary.ContainsKey(_key) Then
                        pbs.Helper.UserVocabulary.Remove(_key)
                    End If
                    pbs.Helper.UserVocabulary.Add(_key, translated)

                End If
                'Clipboard.SetText(Me._originalText)
                'ConfirmDialog.Confirm("Use command VOCA to translate the original text, which is copied to the clipboard:" & Environment.NewLine & Me._originalText)
            End If
        Catch ex As Exception

        End Try
        Me.TopMost = True
    End Sub
End Class