Imports System.Windows.Forms
Imports pbs.Helper
Imports Dynamsoft.DotNet.TWAIN
Imports DevExpress.XtraEditors.Repository
Imports DevExpress.XtraEditors.Controls
Imports DevExpress.XtraBars
'Imports pbs.UI.Extensions
Imports System.Text.RegularExpressions
Imports DevExpress.XtraBars.Ribbon
Imports DevScope.Ocr.Tesseract
Imports DevScope.Ocr.Tesseract.Windows

'Imports pbs.BO.DM

Public Class ScanDialog

#Region "OCR"
    Private Shared Sub RegisterOCRSDK()

        Try
            Dim status = DevScope.Ocr.Tesseract.Windows.TesseractOcrEngine.SetLicense("Ngo Thanh", "ngo-thanh.tung@spc-technology.com", "BJABCADFgHs4mbWDTa5h7K9msFYq+rX7dlR0I+fwEZZe/o/5uimHOSeaaZHDTzGS4YJ8JRltneXJoacc3ZSG7IJsRVItW3pymtZFf190eoXwWmkGzgTct7fQ808OWYh2aXASy7zagp1gnI8cubTbgUdpmGjiNsM5/4ufS1M+z8HjnTcmTK8=")

            Dim licStatusErrorMessage = ""
            Select Case status
                Case TesseractOcrLicenseStatus.LICENSE_ERROR
                    licStatusErrorMessage = "License Error"

                Case TesseractOcrLicenseStatus.LICENSE_EXPIRED
                    licStatusErrorMessage = "License Expired"

                Case TesseractOcrLicenseStatus.LICENSE_INVALID
                    licStatusErrorMessage = "License is invalid"

            End Select
            If status <> TesseractOcrLicenseStatus.LICENSE_OK Then
                TextLogger.Log(licStatusErrorMessage + ". Please get a valid license from spc technology", "TesseractOcrEngine License failed")
            End If
        Catch ex As Exception
            TextLogger.Log(ex)
        End Try

    End Sub

#End Region

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        RegisterOCRSDK()

        ' Add any initialization after the InitializeComponent() call.
        DynamicDotNetTwain1.LicenseKeys = "05C90D4CC0AA0DE8E65601A2C20612DE"

    End Sub

    Private _arg As pbsCmdArgs = Nothing
    Public Sub New(arg As pbsCmdArgs)

        ' This call is required by the designer.
        InitializeComponent()

        RegisterOCRSDK()

        ' Add any initialization after the InitializeComponent() call.
        DynamicDotNetTwain1.LicenseKeys = "05C90D4CC0AA0DE8E65601A2C20612DE"

        _arg = arg
        If _arg Is Nothing Then _arg = New pbsCmdArgs

        If _arg.BO IsNot Nothing Then
            'Dim DQParams = TryCast(_arg.BO, DQ_ParamsBag)
            'If DQParams IsNot Nothing Then
            '    _transType = DQParams.TransType
            '    _reference = DQParams.Reference
            'End If
        ElseIf _arg.GetDefaultParameter.Contains("#") Then

            Dim _refset = _arg.GetDefaultParameter

            _transType = System.Text.RegularExpressions.Regex.Match(_refset, "^[^\#]+").Value
            _reference = System.Text.RegularExpressions.Regex.Match(_refset, "[^\#]+$").Value

        Else
            _transType = _arg.GetValueByKey("transtype", String.Empty)
            _reference = _arg.GetValueByKey("reference", String.Empty)
        End If

    End Sub

#Region "Loading"
    'Private Sub ScanDialog_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
    '    'Me.SaveMyTerritory(Me.Name)

    '    If Not String.IsNullOrEmpty(_currentScanProfile) Then
    '        Dim repository = New pbs.Helper.RegRepository
    '        repository.SaveToRepository("DefaultScanProfile", _currentScanProfile)
    '    End If

    'End Sub

    Private Sub ScanDialog_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Me.Icon = ResIco("miniScanner")
        Me.Text = ResStr("Scan and upload document")
        'Me.RestoreMyTerritory(Me.Name)

        InitDefaultValueForTWAIN()

        'Dim repository = New pbs.Helper.RegRepository
        '_currentScanProfile = repository.LoadFromRepository("DefaultScanProfile")

        UpdateOCRInterface(False)

        RefreshUI()

    End Sub

    Private Sub UpdateOCRInterface(show As Boolean)
        If show Then
            Me.SplitContainerControl1.PanelVisibility = DevExpress.XtraEditors.SplitPanelVisibility.Both
            Me.HomeRibbonPage1.Visible = True
            Me.ViewRibbonPage1.Visible = True
            Me.PageLayoutRibbonPage1.Visible = True
        Else
            Me.HomeRibbonPage1.Visible = False
            Me.ViewRibbonPage1.Visible = False
            Me.PageLayoutRibbonPage1.Visible = False
            Me.SplitContainerControl1.PanelVisibility = DevExpress.XtraEditors.SplitPanelVisibility.Panel1

        End If

    End Sub

    Private Sub InitDefaultValueForTWAIN()
        Try
            Me.DynamicDotNetTwain1.Resolution = 300

            ' dynamicDotNetTwain1.IfThrowException = true
            DynamicDotNetTwain1.ScanInNewProcess = True
            DynamicDotNetTwain1.SupportedDeviceType = Enums.EnumSupportedDeviceType.SDT_TWAIN
            DynamicDotNetTwain1.IfFitWindow = False
            DynamicDotNetTwain1.MouseShape = False

            DynamicDotNetTwain1.SetViewMode(1, 1)

            ' Init the sources for TWAIN scanning, show in the cbxSources controls
            If DynamicDotNetTwain1.SourceCount > 0 Then
                Dim hasTwainSource As Boolean : hasTwainSource = False
                Dim hasWebcamSource As Boolean : hasWebcamSource = False
                Dim i As Integer

                RepositoryItemComboBox1.Items.Clear()
                For i = 0 To DynamicDotNetTwain1.SourceCount - 1
                    RepositoryItemComboBox1.Items.Add(DynamicDotNetTwain1.SourceNameItems(Convert.ToInt16(i)))
                    Dim enumDeviceType As Dynamsoft.DotNet.TWAIN.Enums.EnumDeviceType = DynamicDotNetTwain1.GetSourceType(Convert.ToInt16(i))
                    If (enumDeviceType = Dynamsoft.DotNet.TWAIN.Enums.EnumDeviceType.SDT_TWAIN) Then
                        hasTwainSource = True
                    ElseIf (enumDeviceType = Dynamsoft.DotNet.TWAIN.Enums.EnumDeviceType.SDT_WEBCAM) Then
                        hasWebcamSource = True
                    End If
                Next

                If hasTwainSource Then
                    cbxSource.Enabled = True
                    chkShowUI.Enabled = True
                    chkADF.Enabled = True
                    chkDuplex.Enabled = True
                    cbxResolution.Enabled = True
                    rdbtnGray.Checked = True
                    '  cbxResolution.SelectedIndex = 0
                    '  EnableControls(Me.picboxScan)
                End If

                'If hasWebcamSource Then
                '    chkShowUIForWebcam.Enabled = True
                '    cbxMediaType.Enabled = True
                '    cbxResolutionForWebcam.Enabled = True
                '    EnableControls(Me.picboxGrab)
                'End If

                cbxSource.EditValue = RepositoryItemComboBox1.Items(0)
                _scannerIdx = 0
                'dynamicDotNetTwain1.SelectSourceByIndex(cbxSource.SelectedIndex)
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try

    End Sub

    Private Sub RefreshUI()
        CheckIfRibbonCommandIsExecutable(Me.Ribbon, AddressOf CanExcecute)
        'LoadProfile()

        If Not String.IsNullOrEmpty(_reference) Then
            Me.Text = String.Format(ResStr("Scan and link documents to {0}#{1}"), _transType, _reference)
        End If

    End Sub

#End Region

#Region "Interactive Events"
    Private Sub cbxSource_EditValueChanged(sender As Object, e As EventArgs) Handles cbxSource.EditValueChanged
        Dim combo = TryCast(cbxSource.Edit, RepositoryItemComboBox)
        If combo IsNot Nothing Then
            For Each itm As String In combo.Items
                If itm = cbxSource.EditValue.ToString Then
                    _scannerIdx = combo.Items.IndexOf(itm)
                End If
            Next
        End If
    End Sub

    Private Sub RibbonControl1_ItemClick(sender As Object, e As DevExpress.XtraBars.ItemClickEventArgs) Handles RibbonControl1.ItemClick
        If TypeOf (e.Item) Is BarButtonItem Then
            RouteCommand(e.Item.Name)
        End If
    End Sub
#End Region

#Region "Commands"

    Private Shared Sub CheckIfRibbonCommandIsExecutable(theRibbon As Ribbon.RibbonControl, fCheck As pbs.Helper.CommandExecutableCheck)
        For Each itm As BarItem In theRibbon.Items
            If Not fCheck.Invoke(itm.Name) Then

                itm.Visibility = BarItemVisibility.Never

            ElseIf itm.Visibility = BarItemVisibility.Never Then

                itm.Visibility = BarItemVisibility.Always

            End If
        Next
        For Each page As RibbonPage In theRibbon.Pages
            For Each rpg As RibbonPageGroup In page.Groups
                Dim visibleItems = (From itm As BarItemLink In rpg.ItemLinks Where itm.CanVisible).Count
                rpg.Visible = visibleItems > 0
            Next
        Next
    End Sub

    Private Function SecurityKey() As String
        Return True
        ' Return GetType(pbs.BO.DM.Scanner).ToString
    End Function

    Protected Overridable Function CanExcecute(cmd As String) As Boolean
        If String.IsNullOrWhiteSpace(cmd) Then Return True
        Select Case cmd
            'Case btnLinkDocument.Name
            '    Return Not String.IsNullOrEmpty(_reference) AndAlso Me.DynamicDotNetTwain1.HowManyImagesInBuffer > 0
            'Case btnOpenProfile.Name
            '    Return False
            '    ' Return pbs.BO.DM.ScannerInfoList.GetScannerInfoList.Count > 0
            'Case btnSaveProfile.Name
            '    Return False
            '    'Return pbs.UsrMan.Permission.isPermited(String.Format("{0}.{1}", SecurityKey, Action._Create))

            Case btnSaveToPDF.Name, btnSaveToPNG.Name, btnOCR.Name, btnOCR.Name, btnOCREnglish.Name, btnOCRVietnamese.Name, btnOCRFrench.Name, btnOCRRussian.Name, btnDeleteSelectedPages.Name
                Return Me.DynamicDotNetTwain1.HowManyImagesInBuffer > 0
            Case Else
                Return True
                'Return pbs.UsrMan.Permission.isPermited(String.Format("{0}.{1}", SecurityKey, Regex.Replace(cmd, "^btn|^chk|^cbx", String.Empty)))
        End Select
    End Function

    Private Sub RouteCommand(cmd As String)
        Try
            Select Case cmd
                Case "DeleteCurrentImage", btnDeleteSelectedPages.Name

                    Dim del = DynamicDotNetTwain1.CurrentSelectedImageIndicesInBuffer
                    If del.Count > 0 Then DynamicDotNetTwain1.RemoveImages(del)

                    RefreshUI()

                Case btnOCREnglish.Name
                    lang = "eng"
                    RunOCR()

                Case btnOCRVietnamese.Name
                    lang = "vie"
                    RunOCR()
                Case btnOCRFrench.Name
                    lang = "fra"
                    RunOCR()
                Case btnOCRRussian.Name
                    lang = "rus"
                    RunOCR()
                Case btnOCR.Name
                    lang = "eng"
                    RunOCR()
                    'Case btnLinkDocument.Name

                    'UploadAndLinkFile()

                    'Case btnOpenProfile.Name
                    '    Dim theProfileId = ValueSelector.SelectValue("Scanner", ResStr("Select Scanner Profile"))

                    '    If _currentScanProfile.Equals(theProfileId) Then Exit Select

                    '    If String.IsNullOrEmpty(theProfileId) Then
                    '        If MsgBox.Confirm(ResStr("Reset Scanner Profile ? ")) = Windows.Forms.DialogResult.OK Then
                    '            _currentScanProfile = String.Empty
                    '        End If
                    '    Else
                    '        _currentScanProfile = theProfileId
                    '    End If

                    '    RefreshUI()

                    'Case btnSaveProfile.Name
                    '    If Not String.IsNullOrWhiteSpace(_currentScanProfile) Then

                    '        Dim _scannerProfile = Scanner.GetScanner(_currentScanProfile)
                    '        SaveProfile(_scannerProfile)
                    '    Else

                    '        Dim _scannerProfile = Scanner.NewScanner("?")
                    '        SaveProfile(_scannerProfile)

                    '    End If

                Case btnOpen.Name
                    Dim filename = AskImportFileName("Pdf file|*.pdf|Png image|*.png|Bitmap image|*.bmp|Tiff Image|*.tif;*tiff|Jpeg image", "*.jpg", "Select pdf or image file")
                    If Not String.IsNullOrWhiteSpace(filename) AndAlso My.Computer.FileSystem.FileExists(filename) Then
                        Me.DynamicDotNetTwain1.RemoveAllImages()

                        Me.DynamicDotNetTwain1.LoadImage(filename)

                        RefreshUI()
                    End If

                Case btnScan.Name
                    DoScan()

                Case btnSaveToPDF.Name
                    _defaultSaveAs = "pdf"
                    SaveOutput(True)

                    Me.OCRResult.Text = String.Empty
                    UpdateOCRInterface(False)

                    RefreshUI()

                Case btnSaveToPNG.Name
                    _defaultSaveAs = "png"
                    SaveOutput(True)

                    Me.OCRResult.Text = String.Empty
                    UpdateOCRInterface(False)

                    RefreshUI()

                Case btnSavetoTiff.Name
                    _defaultSaveAs = "tiff"
                    SaveOutput(True)

                    Me.OCRResult.Text = String.Empty
                    UpdateOCRInterface(False)

                    RefreshUI()

            End Select
        Catch ex As Exception
            MsgBox.ShowError(ex)
        End Try
    End Sub

#End Region

#Region "Scan"

    Private _scannerIdx As Integer = 0

    Private Sub DoScan()
        Try

            If _scannerIdx < 0 Then
                DynamicDotNetTwain1.SelectSource()
            Else
                DynamicDotNetTwain1.SelectSourceByIndex(_scannerIdx)
            End If

            If DynamicDotNetTwain1.OpenSource() Then
                ApplyScannerSettings()
                DynamicDotNetTwain1.AcquireImage()

                'if this is the first page. change ribbons
                If DynamicDotNetTwain1.HowManyImagesInBuffer = 1 Then RefreshUI()

            End If

        Catch ex As Exception
            DynamicDotNetTwain1.CloseSource()
            MsgBox.ShowError(ex)
        End Try
    End Sub

    Private Sub ApplyScannerSettings()
        DynamicDotNetTwain1.IfAutoWhiteBalance = True

        DynamicDotNetTwain1.IfShowUI = chkShowUI.Checked
        DynamicDotNetTwain1.IfDuplexEnabled = chkDuplex.Checked
        DynamicDotNetTwain1.IfAutoFeed = chkADF.Checked

        If cbxColor.EditValue.ToString = "B/W" Then
            DynamicDotNetTwain1.PixelType = Enums.TWICapPixelType.TWPT_BW
            DynamicDotNetTwain1.BitDepth = 1
        ElseIf cbxColor.EditValue.ToString = "Gray" Then
            DynamicDotNetTwain1.PixelType = Enums.TWICapPixelType.TWPT_GRAY
            DynamicDotNetTwain1.BitDepth = 8
        ElseIf cbxColor.EditValue.ToString = "Color" Then
            DynamicDotNetTwain1.PixelType = Enums.TWICapPixelType.TWPT_RGB
            DynamicDotNetTwain1.BitDepth = 16
        End If

        DynamicDotNetTwain1.Resolution = cbxResolution.EditValue.ToString.ToInteger

    End Sub

#End Region

#Region "Save output"

    Private _transType As String = String.Empty
    Private _reference As String = String.Empty

    Private Shared _defaultSaveAs As String = String.Empty

    Friend Sub SaveOutput(Optional _saveAsMultiPageDocument As Boolean = True)
        Try
            Dim TwainControl = Me.DynamicDotNetTwain1

            TwainControl.CloseSource()

            Dim theFilePath = AskTheOutputFileName()

            If Not String.IsNullOrEmpty(theFilePath) Then
                Dim filename = theFilePath.ShortFileName(False)
                Dim foldername = theFilePath.FolderName
                Dim numbersOfImages = TwainControl.HowManyImagesInBuffer
                Dim FileExtension = theFilePath.Leaf.ToLower

                Dim theOCRFilePath = My.Computer.FileSystem.CombinePath(foldername, String.Format("OCR_{0}.docx", filename))

                Select Case FileExtension

                    Case "pdf"

                        If _saveAsMultiPageDocument OrElse numbersOfImages = 1 Then

                            TwainControl.SaveAllAsPDF(theFilePath)

                            If Not String.IsNullOrEmpty(Me.OCRResult.Text) Then
                                OCRResult.Document.SaveDocument(theOCRFilePath, DevExpress.XtraRichEdit.DocumentFormat.OpenXml)
                            End If

                        ElseIf numbersOfImages > 1 Then

                            For idx = 0 To numbersOfImages - 1
                                Dim imagename = String.Format("{0}_{1:000}.{2}", filename, idx, FileExtension)
                                Dim imagefilename = My.Computer.FileSystem.CombinePath(foldername, imagename)
                                TwainControl.SaveAsPDF(imagefilename, idx)
                            Next

                        End If

                    Case "tiff", "tif"
                        If _saveAsMultiPageDocument OrElse numbersOfImages = 1 Then

                            TwainControl.SaveAllAsMultiPageTIFF(theFilePath)

                        ElseIf numbersOfImages > 1 Then

                            For idx = 0 To numbersOfImages - 1
                                Dim imagename = String.Format("{0}_{1:000}.{2}", filename, idx, FileExtension)
                                Dim imagefilename = My.Computer.FileSystem.CombinePath(foldername, imagename)
                                TwainControl.SaveAsTIFF(theFilePath, idx)
                            Next

                        End If

                    Case "bmp"
                        If numbersOfImages = 1 Then
                            TwainControl.SaveAsBMP(theFilePath, 0)
                        ElseIf numbersOfImages > 1 Then
                            For idx = 0 To numbersOfImages - 1
                                Dim imagename = String.Format("{0}_{1:000}.{2}", filename, idx, FileExtension)
                                Dim imagefilename = My.Computer.FileSystem.CombinePath(foldername, imagename)
                                TwainControl.SaveAsBMP(imagefilename, idx)
                            Next
                        End If

                    Case "png"
                        If numbersOfImages = 1 Then
                            TwainControl.SaveAsPNG(theFilePath, 0)
                        ElseIf numbersOfImages > 1 Then
                            For idx = 0 To numbersOfImages - 1
                                Dim imagename = String.Format("{0}_{1:000}.{2}", filename, idx, FileExtension)
                                Dim imagefilename = My.Computer.FileSystem.CombinePath(foldername, imagename)
                                TwainControl.SaveAsPNG(imagefilename, idx)
                            Next
                        End If

                    Case "JPG".ToLower, "JPEG".ToLower, "JPE".ToLower, "JFIF".ToLower
                        If numbersOfImages = 1 Then
                            TwainControl.SaveAsJPEG(theFilePath, 0)
                        ElseIf numbersOfImages > 1 Then
                            For idx = 0 To numbersOfImages - 1
                                Dim imagename = String.Format("{0}_{1:000}.{2}", filename, idx, FileExtension)
                                Dim imagefilename = My.Computer.FileSystem.CombinePath(foldername, imagename)
                                TwainControl.SaveAsJPEG(imagefilename, idx)
                            Next
                        End If


                End Select
                TwainControl.RemoveAllImages()
            End If



        Catch ex As Exception
            MsgBox.ShowError(ex)
        End Try

    End Sub

    Private Shared Function AskTheOutputFileName() As String
        Dim thefilters As New List(Of String)

        Select Case _defaultSaveAs
            Case "png"
                thefilters.Add("PNG image file|*.PNG")
            Case "pdf"
                thefilters.Add("Pdf file|*.PDF")
            Case Else
                thefilters.Add("Pdf file|*.PDF")
                thefilters.Add("JPEG image file|*.JPG;*.JPEG;*.JPE;*.JFIF")
                thefilters.Add("Bitmap file|*.BMP")
                thefilters.Add("PNG image file|*.PNG")
                thefilters.Add("TIFF image file|*.TIF;*.TIFF")
        End Select

        Return AskSaveAsFile(String.Join("|", thefilters.ToArray))
    End Function

    'Private Function UploadAndLinkOCRFile() As String
    '    Try
    '        Dim theFilePath = My.Computer.FileSystem.CombinePath(pbs.BO.DM.DMD.GetDMDInfo.MonitoringLoc, String.Format("OCR_{0}#{1}.docx", _transType, _reference))

    '        If Not String.IsNullOrEmpty(Me.OCRResult.Text) Then
    '            Me.OCRResult.SaveDocument(theFilePath, DevExpress.XtraRichEdit.DocumentFormat.OpenXml)
    '        End If

    '        Return theFilePath
    '    Catch ex As Exception
    '        TextLogger.Log(ex)
    '    End Try

    '    Return String.Empty

    'End Function

    'Private Sub UploadAndLinkFile()
    '    Dim theFilePath = My.Computer.FileSystem.CombinePath(pbs.BO.DM.DMD.GetDMDInfo.MonitoringLoc, String.Format("{0}#{1}.pdf", _transType, _reference))

    '    Me.DynamicDotNetTwain1.SaveAllAsPDF(theFilePath)

    '    DynamicDotNetTwain1.RemoveAllImages()

    '    Dim thedic = _arg.GetNonEmptyDictionary
    '    If thedic.ContainsKey("reference") Then thedic.Remove("reference")
    '    If thedic.ContainsKey("transtype") Then thedic.Remove("transtype")

    '    thedic.Add("reference", _reference)
    '    thedic.Add("transtype", _transType)

    '    Dim files = New List(Of String)
    '    files.Add(theFilePath)

    '    Dim theOCRFile = UploadAndLinkOCRFile()
    '    If Not String.IsNullOrEmpty(theOCRFile) Then files.Add(theOCRFile)

    '    Dim ret = pbs.BO.DM.DOC.SubmitFile(thedic, files)


    '    RefreshUI()

    '    If ret.Count > 0 Then
    '        MsgBox.Confirm("File '{0}' has been uploaded and link to {1}#{2}", theFilePath.FileName, _transType, _reference)
    '    End If
    'End Sub

#End Region

    '#Region "Save profile"

    Private _currentScanProfile As String = String.Empty

    '    Private Sub SaveProfile(_scannerProfile As Scanner)

    '        _scannerProfile.Name = Me.cbxSource.EditValue.ToString
    '        _scannerProfile.ShowUi = Me.chkShowUI.Checked
    '        _scannerProfile.UseAdf = Me.chkADF.Checked
    '        _scannerProfile.UseDuplex = Me.chkDuplex.Checked

    '        _scannerProfile.Resolution = Me.cbxResolution.EditValue.ToString.ToInteger

    '        If cbxColor.EditValue.ToString = "B/W" Then
    '            _scannerProfile.PixelType = Enums.TWICapPixelType.TWPT_BW
    '            _scannerProfile.BitDepth = 1
    '        ElseIf cbxColor.EditValue.ToString = "Gray" Then
    '            _scannerProfile.PixelType = Enums.TWICapPixelType.TWPT_GRAY
    '            _scannerProfile.BitDepth = 8
    '        ElseIf cbxColor.EditValue.ToString = "Color" Then
    '            _scannerProfile.PixelType = Enums.TWICapPixelType.TWPT_RGB
    '            _scannerProfile.BitDepth = 16
    '        End If

    '        _scannerProfile.OutputFormat = _defaultSaveAs

    '        If _scannerProfile.IsSavable Then
    '            _scannerProfile.Save()

    '            btnOpenProfile.Visibility = BarItemVisibility.Always

    '        Else
    '            MsgBox.Confirm(_scannerProfile.BrokenRulesCollection.Dump)
    '        End If
    '    End Sub

    '    Private Sub LoadProfile()
    '        Dim theProfile As Scanner = Nothing
    '        If String.IsNullOrEmpty(_currentScanProfile) Then
    '            theProfile = Scanner.NewScanner("")
    '        Else
    '            theProfile = Scanner.GetScanner(_currentScanProfile)
    '        End If

    '        chkShowUI.Checked = theProfile.ShowUi
    '        chkADF.Checked = theProfile.UseAdf
    '        chkDuplex.Checked = theProfile.UseDuplex

    '        cbxResolution.EditValue = theProfile.Resolution

    '        If theProfile.PixelType = PixelType.TWPT_BW Then
    '            cbxColor.EditValue = "B/W"
    '        ElseIf theProfile.PixelType = PixelType.TWPT_GRAY Then
    '            cbxColor.EditValue = "Gray"
    '        Else
    '            cbxColor.EditValue = "Color"
    '        End If
    '    End Sub

    '#End Region

    Private Sub BarButtonItem1_ItemClick(sender As Object, e As ItemClickEventArgs) Handles BarButtonItem1.ItemClick
        Process.Start("http://www.spc-technology.com")
    End Sub

   
   
End Class