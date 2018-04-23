Imports Microsoft.VisualBasic
Imports System
Imports System.Data
Imports System.Configuration
Imports System.Collections
Imports System.Web
Imports System.Web.Security
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.UI.WebControls.WebParts
Imports System.Web.UI.HtmlControls
Imports DevExpress.Web.ASPxGridView
Imports DevExpress.XtraPrinting
Imports DevExpress.Data.Filtering

Namespace WebApplication211
	Partial Public Class _Default
		Inherits System.Web.UI.Page
		Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)

		End Sub

		Protected Sub ASPxButton1_Click(ByVal sender As Object, ByVal e As EventArgs)
			If ASPxGridView1.GroupCount = 0 Then
				ASPxGridViewExporter1.WritePdfToResponse()
			Else
				groupField = ASPxGridView1.GetGroupedColumns()(0).FieldName
				groupValues = FindGroupValues(ASPxGridView1)
				Dim ps As New PrintingSystem()
				Dim clink As New Link(ps)
				AddHandler clink.CreateDetailArea, AddressOf clink_CreateDetailArea
				clink.CreateDocument()
				ps.PageSettings.Landscape = True
				Dim stream As New System.IO.MemoryStream()
				ps.ExportToPdf(stream)
				WriteToResponse("export", True, "pdf", stream)
				stream.Close()
			End If
		End Sub

		Private groupField As String
		Private groupValues() As Object

		Private Sub clink_CreateDetailArea(ByVal sender2 As Object, ByVal e2 As CreateAreaEventArgs)
			Dim self As Link = CType(sender2, Link)
			For i As Integer = 0 To groupValues.Length - 1
				Dim linkdata As New DevExpress.Web.ASPxGridView.Export.Helper.GridViewLink(ASPxGridViewExporter1)
				linkdata.PrintingSystem = self.PrintingSystem
				If i > 0 Then
					self.PrintingSystem.InsertPageBreak(0)
				End If
				Dim filter As String = ASPxGridView1.FilterExpression
				ASPxGridView1.FilterExpression = New GroupOperator(GroupOperatorType.And, CriteriaOperator.Parse(filter), New BinaryOperator(groupField, groupValues(i))).ToString()
				ASPxGridView1.ExpandAll()
				ASPxGridViewExporter1.DataBind()

				Dim skipArea As BrickModifier = linkdata.SkipArea
				linkdata.SkipArea = self.SkipArea
				linkdata.AddSubreport(System.Drawing.PointF.Empty)
				linkdata.SkipArea = skipArea

				ASPxGridView1.FilterExpression = filter
			Next i
		End Sub

		Protected Sub WriteToResponse(ByVal fileName As String, ByVal saveAsFile As Boolean, ByVal fileFormat As String, ByVal stream As System.IO.MemoryStream)
			If Page Is Nothing OrElse Page.Response Is Nothing Then
				Return
			End If
			Dim disposition As String
			If saveAsFile Then
				disposition = "attachment"
			Else
				disposition = "inline"
			End If
			Page.Response.Clear()
			Page.Response.Buffer = False
			Page.Response.AppendHeader("Content-Type", String.Format("application/{0}", fileFormat))
			Page.Response.AppendHeader("Content-Transfer-Encoding", "binary")
			Page.Response.AppendHeader("Content-Disposition", String.Format("{0}; filename={1}.{2}", disposition, fileName, fileFormat))
			Page.Response.BinaryWrite(stream.GetBuffer())
			Page.Response.End()
		End Sub

		Private Function FindGroupValues(ByVal grid As ASPxGridView) As Object()
			grid.DataBind()
			Dim column As String = grid.GetGroupedColumns()(0).FieldName
			Dim list As New ArrayList()
			For i As Integer = 0 To grid.VisibleRowCount - 1
				If grid.GetRowLevel(i) = 0 Then
					list.Add(grid.GetRowValues(i, column))
				End If
			Next i
			Return list.ToArray()
		End Function
	End Class
End Namespace
