Imports DevScope.Ocr.Tesseract
Imports DevScope.Ocr.Tesseract.Windows
Imports System.Drawing
Imports System.Windows.Forms
Imports pbs.Helper

Partial Public Class ScanDialog

    Private _ocrEngine As TesseractOcrEngine = Nothing

    Private _tempAutoDeskew As Boolean = True
    Private _tempUseLocalAdaptiveThresholding As Boolean = False
    Private _CharsToRecognize As String = String.Empty
    Private _CharsNotToRecognize As String = String.Empty ' "¢¿¬ö?ø§ñfi?üšäƒëﬁﬂ"

    Private _ocrJobCancel As Boolean = False
    Private _ocrJobRunning As Boolean = False

    Private lang As String = "eng"

    Private _currentOCRPage As Integer

    Private Sub RunOCR()

        UpdateOCRInterface(True)

        If Not String.IsNullOrEmpty(Me.OCRResult.Text.Trim) Then
            If MsgBox.Confirm(ResStr("Clear current text ? Click No to append to the end of current text")) = System.Windows.Forms.DialogResult.OK Then
                Me.OCRResult.Document.Text = String.Empty
            Else

                Me.OCRResult.Document.AppendSection()
                Dim cmd = New DevExpress.XtraRichEdit.Commands.InsertPageBreakCommand(OCRResult)
                cmd.Execute()
            End If
        End If
        'Dim selectedImages = DynamicDotNetTwain1.CurrentSelectedImageIndicesInBuffer.Count

        'If selectedImages > 0 Then
        '    For Each idx In DynamicDotNetTwain1.CurrentSelectedImageIndicesInBuffer
        '        RunOCR(idx)
        '    Next
        'Else
        For idx = 0 To DynamicDotNetTwain1.HowManyImagesInBuffer - 1
            RunOCR(idx)
        Next
        'End If

    End Sub

    Private Sub RunOCR(PageIdx As Integer)
        _currentOCRPage = PageIdx

        Dim pImg = Me.DynamicDotNetTwain1.GetImage(PageIdx)

        If _ocrJobRunning Then
            _ocrJobCancel = True
        Else

            Me.Cursor = Cursors.WaitCursor

            Try
                StatusLine.Caption = ResStr("Cancel OCR Job...")
                StatusLine.Tag = "1"

                PrepareTesseractEngine(lang)

                Dim segmode = GetSelectedPageSegmentationMode()

                _ocrJobRunning = True
                Dim jobResp = DoImageOcr(pImg, segmode, "OCR_Page")

                _ocrJobRunning = False

                'DynamicDotNetTwain1.SetImage(PageIdx, jobResp.ProcessedImage)

                If Not String.IsNullOrWhiteSpace(jobResp.ResultPages(0).Document.Text) Then
                    OCRResult.Document.Paragraphs.Append()
                    OCRResult.Document.AppendText(jobResp.ResultPages(0).Document.Text)
                    OCRResult.ScrollToCaret()
                End If

            Catch ex As Exception
                _ocrJobRunning = False
                MessageBox.Show(ex.Message, "An exception occurred", MessageBoxButtons.OK, MessageBoxIcon.[Error])
            Finally
                _ocrJobRunning = False
                _ocrJobCancel = False
                StatusLine.Caption = "Run OCR on Selected Image"
            End Try

            Me.Cursor = System.Windows.Forms.Cursors.Arrow
        End If

    End Sub

    Private Sub PrepareTesseractEngine(language As String)
        Dim t As DateTime = DateTime.Now

        Dim dictionariesPath As String = AppDomain.CurrentDomain.BaseDirectory

        If _ocrEngine IsNot Nothing Then
            ' just for safety has we already clear the results after the ocr
            _ocrEngine.ClearResults()
        Else
            _ocrEngine = New TesseractOcrEngine()
            AddHandler _ocrEngine.JobProgress, AddressOf ocr_OcrJobProgress
            AddHandler _ocrEngine.AutoDeskewCompleted, AddressOf ocr_AutoDeskewCompleted
            AddHandler _ocrEngine.AutoOrientationDetected, AddressOf ocr_AutoOrientationDetected
            AddHandler _ocrEngine.BeforeRecognition, AddressOf ocrEngine_BeforeRecognize
        End If
        Dim initok As Boolean
        Try

            Dim initParams As New TesseractOcrInitParams()
            initParams.TessdataRootPath = dictionariesPath
            initParams.Language = language
            initParams.DisableDictionaries = False 'Not useLanguageDictionaries.Checked
            initok = _ocrEngine.InitializeEngine(initParams)
        Catch
            Throw
        End Try


    End Sub

    ''' <summary>
    ''' Gets the selected page segmentation mode.
    ''' </summary>
    ''' <returns></returns>
    Private Function GetSelectedPageSegmentationMode() As TesseractOcrPageSegmentationMode
        Dim segModes = New TesseractOcrPageSegmentationMode() {TesseractOcrPageSegmentationMode.AutomaticPageSegmentationNoOCR, _
                                                               TesseractOcrPageSegmentationMode.AutomaticPageSegmentation, _
                                                              TesseractOcrPageSegmentationMode.SingleColumnTextVarSizes, _
                                                             TesseractOcrPageSegmentationMode.SingleBlockVerticalText, _
                                                               TesseractOcrPageSegmentationMode.SingleUniformBlockText, _
                                                               TesseractOcrPageSegmentationMode.SingleTextLine, _
                                                               TesseractOcrPageSegmentationMode.SingleWord, _
                                                               TesseractOcrPageSegmentationMode.SingleWordInCircle, _
                                                               TesseractOcrPageSegmentationMode.SingleCharacter}

        Return segModes(1)
    End Function

    ''' <summary>
    ''' Do ocr on the image.
    ''' </summary>
    ''' <param name="image">The image.</param>
    ''' <param name="segmentationMode">The segmentation mode.</param>
    ''' <param name="jobname">The jobname.</param>
    ''' <returns></returns>
    Private Function DoImageOcr(image As Bitmap, segmentationMode As TesseractOcrPageSegmentationMode, jobname As String) As TesseractOcrJobResponse
        Dim orientationMode As TesseractOcrOrientationMode = TesseractOcrOrientationMode.None

        'If autoDetectOrientationToolStripMenuItem.Checked Then
        orientationMode = TesseractOcrOrientationMode.AutoDetect
        'End If

        ' tipical chars to ignore when using latin1 languages
        ' ¢¿¬ö?ø§ñfi?üšäƒëﬁﬂ

        Dim request = New TesseractOcrJobRequest() With { _
             .OrientationMode = orientationMode, _
             .PageSegmentationMode = segmentationMode, _
             .EnableOCRAdaptation = True, _
             .CharsToRecognize = _CharsToRecognize, _
             .CharsNotToRecognize = _CharsNotToRecognize, _
             .JobName = jobname}

        Dim resp = _ocrEngine.DoOcr(request, image)

        _ocrEngine.ClearResults()

        StatusLine.Caption = String.Format("{0} : 100%.", jobname)
        'ocrPhaseLabel.Caption = String.Format(ResStr("Done in {0} msecs"), resp)

        Return resp
    End Function

#Region "OCR set pre-Run parameters"
    ''' <summary>
    ''' Event handler that is triggered just before the engine starts recognizing text
    ''' </summary>
    ''' <remarks>This is the event where you should set the engine settings/variables if you need to</remarks>
    Private Sub ocrEngine_BeforeRecognize(sender As TesseractOcrEngine, e As TesseractOcrBeforeRecognitionEventArgs)

        'e.OcrContext.SetTesseractVariable("textord_tabfind_find_tables", "1");
        'e.OcrContext.SetTesseractVariable("textord_tablefind_recognize_tables", "1");
        'e.OcrContext.SetTesseractVariable("textord_dump_table_images", "1");
        'e.OcrContext.SetTesseractVariable("chop_enable", "0");
    End Sub
#End Region

#Region "Status update"
    ''' <summary>
    ''' Event handler for the ocr job progress.
    ''' </summary>
    ''' <param name="e">The instance containing the event data.</param>
    Private Sub ocr_OcrJobProgress(sender As TesseractOcrEngine, e As TesseractOcrJobProgressEventArgs)

        ProgressBar1.Caption = Convert.ToString(e.JobName) & " : " & Convert.ToString(e.Percent) & "%"
        ocrPhaseLabel.Caption = e.OcrPhase
        ProgressBar1.EditValue = e.Percent
        Application.DoEvents()

        e.Cancel = _ocrJobCancel
    End Sub

#End Region

#Region "Deskew"
    ''' <summary>
    ''' Event handler for the Auto Deskew
    ''' </summary>
    Private Sub ocr_AutoDeskewCompleted(sender As TesseractOcrEngine, e As TesseractOcrAutoDeskewCompletedEventArgs)

        'Dim image = DynamicDotNetTwain1.GetImage(_currentOCRPage)
        'If Math.Abs(e.DeskewAngle) > 0.05 Then
        '    DynamicDotNetTwain1.SetImage(_currentOCRPage, RotateImage(image, e.DeskewAngle))
        'End If
        StatusLine.Caption = String.Format(ResStr("{0} : {1:-000.00} angle Deskew Completed"), e.JobName, e.DeskewAngle)

    End Sub

    ''' <summary>
    ''' Rotates the image.
    ''' </summary>
    ''' <param name="image">The image.</param>
    ''' <param name="angle">The angle.</param>
    ''' <returns></returns>
    Private Shared Function RotateImage(image As Bitmap, angle As Single) As Bitmap
        'create a new empty bitmap to hold rotated image
        Dim returnBitmap As New Bitmap(image.Width, image.Height)
        'make a graphics object from the empty bitmap
        Dim g As Graphics = Graphics.FromImage(returnBitmap)

        ' fill image with white color
        g.FillRectangle(Brushes.White, 0, 0, image.Width, image.Height)

        'move rotation point to center of image
        g.TranslateTransform(CSng(image.Width) / 2, CSng(image.Height) / 2)
        'rotate
        g.RotateTransform(angle)
        'move image back
        g.TranslateTransform(-CSng(image.Width) / 2, -CSng(image.Height) / 2)
        'draw passed in image onto graphics object
        g.DrawImage(image, New Point(0, 0))
        Return returnBitmap
    End Function
#End Region

#Region "Auto detect orientation"
    ''' <summary>
    ''' Event handler for the Auto Orientation Detection
    ''' </summary>
    Private Sub ocr_AutoOrientationDetected(sender As TesseractOcrEngine, e As TesseractOcrAutoOrientationDetectedEventArgs)
        StatusLine.Caption = e.JobName & " : Auto Orientation Detected"
    End Sub
#End Region

End Class


'Imports CSRichTextBoxSyntaxHighlighting
'Imports System.Collections.Generic
'Imports System.Collections.Specialized
'Imports System.Drawing
'Imports System.Globalization
'Imports System.IO
'Imports System.Linq
'Imports System.Text
'Imports System.Threading
'Imports System.Windows.Forms
'Imports System.Xml

'Namespace DevScope.Ocr.Tesseract.UsageSample
'    Partial Public Class MainForm


'        ''' <summary>
'        ''' Prints the XML.
'        ''' </summary>
'        ''' <param name="xmldata">The XML.</param>
'        ''' <returns></returns>
'        Public Shared Function PrintXML(xmldata As [String]) As String
'            If String.IsNullOrWhiteSpace(xmldata) Then
'                Return ""
'            End If

'            Dim Result As String = ""

'            Dim mStream As New MemoryStream()
'            Dim writer As New XmlTextWriter(mStream, Encoding.Unicode)
'            Dim document As New XmlDocument()

'            Try
'                ' Load the XmlDocument with the XML.
'                document.LoadXml(xmldata)

'                writer.Formatting = Formatting.Indented

'                ' Write the XML into a formatting XmlTextWriter
'                document.WriteContentTo(writer)
'                writer.Flush()
'                mStream.Flush()

'                ' Have to rewind the MemoryStream in order to read
'                ' its contents.
'                mStream.Position = 0

'                ' Read MemoryStream contents into a StreamReader.
'                Dim sReader As New StreamReader(mStream)

'                ' Extract the text from the StreamReader.
'                Dim FormattedXML As [String] = sReader.ReadToEnd()

'                Result = FormattedXML
'            Catch xe As XmlException
'                MessageBox.Show(xe.Message, "PrintXML Failed")
'            End Try

'            mStream.Close()
'            writer.Close()

'            Return Result
'        End Function

'   

'        ''' <summary>
'        ''' Formats the last document text.
'        ''' </summary>
'        Private Sub FormatLastDocumentText()
'            txtOcrText.Text = ""

'            If _lastOcrDocument Is Nothing Then
'                Return
'            End If

'            Dim textSize = txtOcrText.Font.SizeInPoints

'            Using g = txtOcrText.CreateGraphics()
'                Dim sb As New StringBuilder()

'                Dim lineHeight As Double = Double.Parse(cboxHeightPerLine.Text)
'                Dim spaceWidth As Double = Double.Parse(cboxWidthPerSpace.Text)

'                Dim prevLineY As Integer = -1
'                For Each l As var In _lastOcrDocument.AllTextLines
'                    Dim textWidth = g.MeasureString(l.Text, txtOcrText.Font)
'                    Dim targetTextSize As Double = (l.Bounds.Width * textSize) / textWidth.Width

'                    Dim linesToAppend As Integer = CInt(Math.Truncate(targetTextSize))
'                    linesToAppend = 0
'                    If prevLineY <> -1 Then
'                        linesToAppend = CInt(Math.Truncate(Math.Floor((l.Bounds.Y - prevLineY) / lineHeight)))
'                    Else
'                        linesToAppend = CInt(Math.Truncate(Math.Ceiling(l.Bounds.Y / lineHeight)))
'                    End If

'                    prevLineY = l.Bounds.Y + l.Bounds.Height

'                    For i As Integer = 0 To linesToAppend - 1
'                        sb.AppendLine()
'                    Next

'                    Dim prevWordX As Integer = -1
'                    For Each w As TesseractOcrWord In l.Words
'                        Dim spacesToAppend As Integer = 0
'                        If prevWordX <> -1 Then
'                            spacesToAppend = CInt(Math.Truncate(Math.Floor((w.Bounds.X - prevWordX) / spaceWidth)))
'                        Else
'                            spacesToAppend = CInt(Math.Truncate(Math.Ceiling(w.Bounds.X / spaceWidth)))
'                        End If

'                        prevWordX = w.Bounds.X + w.Bounds.Width
'                        For i As Integer = 0 To spacesToAppend - 1
'                            sb.Append(" ")
'                        Next

'                        sb.Append(" " + w.Text)
'                    Next
'                    sb.AppendLine()
'                Next
'                txtOcrText.Text = sb.ToString()
'            End Using



'            'webBrowser1.DocumentText = sb.ToString();
'        End Sub

'        ''' <summary>
'        ''' Renders the document elements of the ocr result.
'        ''' </summary>
'        ''' <param name="resp">The resp.</param>
'        Private Sub RenderDocumentElements(resp As TesseractOcrJobResponse)
'            treeView1.Nodes.Clear()

'            Dim tnDocument As TreeNode = treeView1.Nodes.Add("Document")

'            Dim index As Integer = 0

'            Dim sb As New StringBuilder()


'            For Each b As var In resp.Document.Blocks
'                Dim tnSection = tnDocument.Nodes.Add(String.Format("Block {0} ({1} Parag.)", (System.Math.Max(System.Threading.Interlocked.Increment(index), index - 1)), b.Paragraphs.Count))
'                tnSection.Tag = b

'                For Each p As var In b.Paragraphs
'                    Dim tnPara = tnSection.Nodes.Add(String.Format("Paragraph ({0} Lines)", p.TextLines.Count))
'                    tnPara.Tag = p

'                    For Each l As var In p.TextLines

'                        Dim tnTextLine = tnPara.Nodes.Add(String.Format("{0} ({1} Words)", l.Text, l.Words.Count))
'                        tnTextLine.Tag = l

'                        For Each w As TesseractOcrWord In l.Words
'                            Dim tn = tnTextLine.Nodes.Add(String.Format("{0} ({1} Chars) - {2}%", w.Text, w.Chars.Count, w.Confidence))
'                            tn.Tag = w

'                            For Each c As TesseractOcrChar In w.Chars
'                                Dim twc As TreeNode = tn.Nodes.Add(String.Format("{0} - {1}%", If(c.Character = 0, " "c, c.Character), c.Confidence))
'                                twc.Tag = c
'                            Next
'                        Next
'                    Next
'                Next



'                tnSection.Expand()
'            Next

'            tnDocument.Expand()
'        End Sub

'

'        ''' <summary>
'        ''' Initializes the main ocr engine instance.
'        ''' </summary>
'  











'        ''' <summary>
'        ''' Handles the Click event of the btZoomIn control.
'        ''' </summary>
'        ''' <param name="sender">The source of the event.</param>
'        ''' <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
'        Private Sub btZoomIn_Click(sender As Object, e As EventArgs)
'            pictureViewer1.Zoom += 0.1F
'        End Sub

'        ''' <summary>
'        ''' Handles the Click event of the btZoomOut control.
'        ''' </summary>
'        ''' <param name="sender">The source of the event.</param>
'        ''' <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
'        Private Sub btZoomOut_Click(sender As Object, e As EventArgs)
'            pictureViewer1.Zoom -= 0.1F
'        End Sub

'        ''' <summary>
'        ''' Handles the Click event of the btZoomWidth control.
'        ''' </summary>
'        ''' <param name="sender">The source of the event.</param>
'        ''' <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
'        Private Sub btZoomWidth_Click(sender As Object, e As EventArgs)
'            pictureViewer1.ZoomImage(Utils.ImageViewerControl.ZoomImageType.Width)
'        End Sub

'        ''' <summary>
'        ''' Handles the Click event of the btZoomAll control.
'        ''' </summary>
'        ''' <param name="sender">The source of the event.</param>
'        ''' <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
'        Private Sub btZoomAll_Click(sender As Object, e As EventArgs)
'            pictureViewer1.ZoomImage(Utils.ImageViewerControl.ZoomImageType.Full)
'        End Sub

'        ''' <summary>
'        ''' Handles the AfterSelect event of the treeView1 control.
'        ''' </summary>
'        ''' <param name="sender">The source of the event.</param>
'        ''' <param name="e">The <see cref="TreeViewEventArgs"/> instance containing the event data.</param>
'        Private Sub treeView1_AfterSelect(sender As Object, e As TreeViewEventArgs)
'            pictureViewer1.HighlightRegions.Clear()
'            Dim node = e.Node
'            If node Is Nothing OrElse node.Tag Is Nothing Then

'                Return
'            End If

'            Dim elem = TryCast(node.Tag, ITesseractOcrDocumentElement)

'			pictureViewer1.HighlightRegions.Add(New Utils.ImageViewerControl.HighlightRegion() With { _
'				Key .Bounds = elem.Bounds _
'			})

'            pictureViewer1.ScrollToRegion(elem.Bounds, btAutoZoomToSelection.Checked, True)

'            propertyGrid1.SelectedObject = elem
'        End Sub

'        ''' <summary>
'        ''' Handles the Click event of the openToolStripMenuItem control.
'        ''' </summary>
'        ''' <param name="sender">The source of the event.</param>
'        ''' <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
'        Private Sub openToolStripMenuItem_Click(sender As Object, e As EventArgs)
'            Dim fd As New OpenFileDialog()
'            fd.Filter = "Image files|*.bmp;*.gif;*.jpg;*.jpeg;*.tif;*.tiff;*.png"
'            If fd.ShowDialog() <> System.Windows.Forms.DialogResult.OK Then
'                Return
'            End If
'            'txtImageLocation.Text = fd.FileName;

'            ClearContext()

'            Try
'                Using bmp = New Bitmap(fd.FileName)
'                    _basicImage = New Bitmap(bmp)

'                End Using
'            Catch
'                MessageBox.Show("Please select a BMP,TIFF,JPG,PNG or GIF image", "Invalid Image Format", MessageBoxButtons.OK, MessageBoxIcon.Warning)
'                Return
'            End Try

'            pictureViewer1.Image = _basicImage
'            pictureViewer1.ZoomImage(Utils.ImageViewerControl.ZoomImageType.Full)

'        End Sub

'        ''' <summary>
'        ''' Handles the Click event of the autoZoomImageViewerToolStripMenuItem control.
'        ''' </summary>
'        ''' <param name="sender">The source of the event.</param>
'        ''' <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
'        Private Sub autoZoomImageViewerToolStripMenuItem_Click(sender As Object, e As EventArgs)
'        End Sub

'        ''' <summary>
'        ''' Handles the Load event of the MainForm control.
'        ''' </summary>
'        ''' <param name="sender">The source of the event.</param>
'        ''' <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
'        Private Sub MainForm_Load(sender As Object, e As EventArgs)
'            cboxSegmentationMode.SelectedIndex = 1
'            cboxLanguage.SelectedIndex = 0
'            cboxHeightPerLine.SelectedIndex = cboxHeightPerLine.Items.Count - 1
'            cboxWidthPerSpace.SelectedIndex = cboxWidthPerSpace.Items.Count - 1
'            ddlMarkType.SelectedIndex = 0

'            ' Initialize the XMLViewerSettings. 
'			Dim viewerSetting As New XMLViewerSettings() With { _
'				Key .AttributeKey = Color.Red, _
'				Key .AttributeValue = Color.Blue, _
'				Key .Tag = Color.Blue, _
'				Key .Element = Color.DarkRed, _
'				Key .Value = Color.Black _
'			}

'            txtHOCRText.Settings = viewerSetting

'        End Sub

'        ''' <summary>
'        ''' Handles the Click event of the pasteImageToolStripMenuItem control.
'        ''' </summary>
'        ''' <param name="sender">The source of the event.</param>
'        ''' <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
'        Private Sub pasteImageToolStripMenuItem_Click(sender As Object, e As EventArgs)
'            If Clipboard.ContainsImage() Then
'                ClearContext()
'                If _basicImage IsNot Nothing Then
'                    _basicImage.Dispose()
'                End If
'                _basicImage = New Bitmap(Clipboard.GetImage())
'                pictureViewer1.Image = _basicImage
'                pictureViewer1.ZoomImage(Utils.ImageViewerControl.ZoomImageType.Full)
'            Else

'                MessageBox.Show("There is no image in the clipboard", "Image not available", MessageBoxButtons.OK, MessageBoxIcon.Information)
'            End If

'        End Sub

'        ''' <summary>
'        ''' Clears the context of the ocr.
'        ''' </summary>
'        Private Sub ClearContext()
'            If _basicImage IsNot Nothing Then
'                _basicImage.Dispose()
'            End If

'            treeView1.Nodes.Clear()
'            pictureViewer1.HighlightRegions.Clear()
'            pictureViewer1.Refresh()
'            pictureViewer1.Image = Nothing

'            txtHOCRText.Text = ""
'            txtOcrText.Text = ""
'            txtRawOcrText.Text = ""
'        End Sub

'        ''' <summary>
'        ''' Handles the SelectedIndexChanged event of the cboxHeightPerLine control.
'        ''' </summary>
'        ''' <param name="sender">The source of the event.</param>
'        ''' <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
'        Private Sub cboxHeightPerLine_SelectedIndexChanged(sender As Object, e As EventArgs)
'            FormatLastDocumentText()
'        End Sub

'        ''' <summary>
'        ''' Handles the SelectedIndexChanged event of the cboxWidthPerSpace control.
'        ''' </summary>
'        ''' <param name="sender">The source of the event.</param>
'        ''' <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
'        Private Sub cboxWidthPerSpace_SelectedIndexChanged(sender As Object, e As EventArgs)
'            FormatLastDocumentText()
'        End Sub


'        ''' <summary>
'        ''' Handles the Click event of the btBrowseFolder control.
'        ''' </summary>
'        ''' <param name="sender">The source of the event.</param>
'        ''' <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
'        Private Sub btBrowseFolder_Click(sender As Object, e As EventArgs)
'            Dim fdDialog As New FolderBrowserDialog()
'            If fdDialog.ShowDialog() <> System.Windows.Forms.DialogResult.OK Then
'                Return
'            End If

'            txtFolder.Text = fdDialog.SelectedPath

'            Dim files = Directory.GetFiles(txtFolder.Text)


'            Dim pageSegMode = GetSelectedPageSegmentationMode()
'            Dim lang = cboxLanguage.Text.Substring(0, 3).ToLower()

'            lvFolderFiles.Items.Clear()
'            For Each file As var In files
'                If file.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) OrElse file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) OrElse file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) OrElse file.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) OrElse file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) OrElse file.EndsWith(".tif", StringComparison.OrdinalIgnoreCase) OrElse file.EndsWith(".tiff", StringComparison.OrdinalIgnoreCase) Then
'                    Dim jobKey = Guid.NewGuid().ToString()
'                    Dim lvi = New ListViewItem(Path.GetFileName(file), jobKey)
'                    lvi.SubItems.Add(New ListViewItem.ListViewSubItem(lvi, "0%"))
'                    lvi.SubItems.Add(New ListViewItem.ListViewSubItem(lvi, ""))
'                    lvFolderFiles.Items.Add(lvi)


'					lvi.Tag = New JobInfo() With { _
'						Key .Index = lvi.Index, _
'						Key .FileName = file, _
'						Key .Name = jobKey, _
'						Key .Language = lang, _
'						Key .PageSegmentationMode = pageSegMode _
'					}



'                    Dim pb As ProgressBar() = New ProgressBar(9) {}
'                End If
'            Next


'        End Sub

'        ''' <summary>
'        ''' Handles the Click event of the btRunOcrAllFiles control.
'        ''' </summary>
'        ''' <param name="sender">The source of the event.</param>
'        ''' <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
'        Private Sub btRunOcrAllFiles_Click(sender As Object, e As EventArgs)
'            Dim jobs = New List(Of JobInfo)()
'            For i As Integer = 0 To lvFolderFiles.Items.Count - 1
'                Dim job = DirectCast(lvFolderFiles.Items(i).Tag, JobInfo)
'                ThreadPool.QueueUserWorkItem(New WaitCallback(AddressOf RunJob), job)
'            Next
'        End Sub

'        ''' <summary>
'        ''' Runs a OCR job.
'        ''' </summary>
'        ''' <param name="b">The b.</param>
'        Private Sub RunJob(b As Object)

'            Dim jobInfo As JobInfo = DirectCast(b, JobInfo)

'            Dim dictionariesPath = AppDomain.CurrentDomain.BaseDirectory

'            Using ocrEngine As New TesseractOcrEngine()
'                Dim initok As Boolean = ocrEngine.InitializeEngine(dictionariesPath, jobInfo.Language)
'                Dim fnProgress = New TesseractOcrJobProgressEventHandler(AddressOf ocr_RunJobOcrProgress)
'                ocrEngine.JobProgress += fnProgress

'                Dim t As DateTime = DateTime.Now

'                Using bmpSrc As New Bitmap(jobInfo.FileName)
'                    Dim orientationMode = TesseractOcrJobRequest.TesseractOcrOrientationMode.None
'                    If autoDetectOrientationToolStripMenuItem.Checked Then
'                        orientationMode = TesseractOcrJobRequest.TesseractOcrOrientationMode.AutoDetect
'                    End If

'					Using response = ocrEngine.DoOcr(New TesseractOcrJobRequest() With { _
'						Key .Image = bmpSrc, _
'						Key .AutoDeskew = autoDeskewToolStripMenuItem.Checked, _
'						Key .OrientationMode = orientationMode, _
'						Key .PageSegmentationMode = jobInfo.PageSegmentationMode, _
'						Key .JobName = "" & jobInfo.Index _
'					})
'                        Me.SafeInvoke(Function()
'                                          lvFolderFiles.Items(jobInfo.Index).SubItems(2).Text = response.Text
'                                          Application.DoEvents()

'                                      End Function)

'                    End Using
'                End Using

'                ocrEngine.JobProgress -= fnProgress
'            End Using

'        End Sub

'        ''' <summary>
'        ''' OCR job progress.
'        ''' </summary>
'        ''' <param name="sender">The sender.</param>
'        ''' <param name="e">The <see cref="TesseractOcrProgressEventArgs" /> instance containing the event data.</param>
'        Private Sub ocr_RunJobOcrProgress(sender As TesseractOcrEngine, e As TesseractOcrJobProgressEventArgs)
'            Me.SafeInvoke(Function()
'                              Dim index = Integer.Parse(e.JobName)
'                              lvFolderFiles.Items(index).SubItems(1).Text = Convert.ToString(e.Percent) & "%"
'                              Application.DoEvents()

'                          End Function)
'        End Sub

'        ''' <summary>
'        ''' Handles the Click event of the btMarkAllWords control.
'        ''' </summary>
'        ''' <param name="sender">The source of the event.</param>
'        ''' <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
'        Private Sub btMarkAllWords_Click(sender As Object, e As EventArgs)
'            pictureViewer1.HighlightRegions.Clear()

'            If _lastOcrDocument Is Nothing Then
'                Return
'            End If

'            Select Case ddlMarkType.SelectedIndex
'                Case 0
'                    If True Then
'                        For Each word As var In _lastOcrDocument.AllWords
'							pictureViewer1.HighlightRegions.Add(New Utils.ImageViewerControl.HighlightRegion() With { _
'								Key .Bounds = word.Bounds _
'							})
'                        Next
'                        Exit Select
'                    End If
'                Case 1
'                    If True Then
'                        For Each ch As var In _lastOcrDocument.AllChars
'							pictureViewer1.HighlightRegions.Add(New Utils.ImageViewerControl.HighlightRegion() With { _
'								Key .Bounds = ch.Bounds _
'							})
'                        Next
'                        Exit Select
'                    End If
'                Case 2
'                    If True Then
'                        For Each tl As var In _lastOcrDocument.AllTextLines
'							pictureViewer1.HighlightRegions.Add(New Utils.ImageViewerControl.HighlightRegion() With { _
'								Key .Bounds = tl.Bounds _
'							})
'                        Next
'                        Exit Select
'                    End If
'            End Select


'            pictureViewer1.Refresh()
'        End Sub

'        ''' <summary>
'        ''' Handles the Click event of the copyImageToolStripMenuItem control.
'        ''' </summary>
'        ''' <param name="sender">The source of the event.</param>
'        ''' <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
'        Private Sub copyImageToolStripMenuItem_Click(sender As Object, e As EventArgs)
'            If pictureViewer1.Image IsNot Nothing Then
'                Clipboard.SetImage(pictureViewer1.Image)
'                MessageBox.Show(Me, "Image copied to clipboard.", "Success")
'            End If
'        End Sub

'        Private Sub menuStrip1_ItemClicked(sender As Object, e As ToolStripItemClickedEventArgs)

'        End Sub


'    End Class

'    Class JobInfo
'        Public Property Name() As String
'            Get
'                Return m_Name
'            End Get
'            Set(value As String)
'                m_Name = value
'            End Set
'        End Property
'        Private m_Name As String
'        Public Property FileName() As String
'            Get
'                Return m_FileName
'            End Get
'            Set(value As String)
'                m_FileName = value
'            End Set
'        End Property
'        Private m_FileName As String
'        Public Property Language() As String
'            Get
'                Return m_Language
'            End Get
'            Set(value As String)
'                m_Language = value
'            End Set
'        End Property
'        Private m_Language As String
'        Public Property PageSegmentationMode() As TesseractOcrJobRequest.TesseractOcrPageSegmentationMode
'            Get
'                Return m_PageSegmentationMode
'            End Get
'            Set(value As TesseractOcrJobRequest.TesseractOcrPageSegmentationMode)
'                m_PageSegmentationMode = value
'            End Set
'        End Property
'        Private m_PageSegmentationMode As TesseractOcrJobRequest.TesseractOcrPageSegmentationMode
'        Public Property Index() As Integer
'            Get
'                Return m_Index
'            End Get
'            Set(value As Integer)
'                m_Index = value
'            End Set
'        End Property
'        Private m_Index As Integer

'    End Class
'End Namespace
