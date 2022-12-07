using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using DevExpress.Web;
using DevExpress.XtraPrinting;
using DevExpress.Data.Filtering;

namespace WebApplication211
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void ASPxButton1_Click(object sender, EventArgs e)
        {
            if (ASPxGridView1.GroupCount == 0)
            {
                ASPxGridViewExporter1.WritePdfToResponse();
            }
            else 
            {
                string groupField = ASPxGridView1.GetGroupedColumns()[0].FieldName;
                object[] groupValues = FindGroupValues(ASPxGridView1);
                PrintingSystem ps = new PrintingSystem();
                Link clink = new Link(ps);
                clink.CreateDetailArea += new CreateAreaEventHandler(delegate(object sender2, CreateAreaEventArgs e2)
                {
                    Link self = (Link)sender2;
                    for (int i = 0; i < groupValues.Length; i++)
                    {
                        DevExpress.Web.Export.GridViewLink linkdata = new DevExpress.Web.Export.GridViewLink(ASPxGridViewExporter1);
                        linkdata.PrintingSystemBase = self.PrintingSystem;
                        if (i > 0)
                        {
                            self.PrintingSystem.InsertPageBreak(0);
                        }
                        string filter = ASPxGridView1.FilterExpression;
                        ASPxGridView1.FilterExpression = new GroupOperator( GroupOperatorType.And, 
                            CriteriaOperator.Parse(filter), new BinaryOperator(groupField, groupValues[i])).ToString();
                        ASPxGridView1.ExpandAll();
                        ASPxGridViewExporter1.DataBind();

                        BrickModifier skipArea = linkdata.SkipArea;
                        linkdata.SkipArea = self.SkipArea;
                        linkdata.AddSubreport(System.Drawing.PointF.Empty);
                        linkdata.SkipArea = skipArea;

                        ASPxGridView1.FilterExpression = filter;
                    }
                });

                clink.CreateDocument();
                ps.PageSettings.Landscape = true;
                System.IO.MemoryStream stream = new System.IO.MemoryStream();
                ps.ExportToPdf(stream);
                WriteToResponse("export", true, "pdf", stream);
                stream.Close();
            }
        }

        protected void WriteToResponse(string fileName, bool saveAsFile, string fileFormat, System.IO.MemoryStream stream)
        {
            if (Page == null || Page.Response == null) return;
            string disposition = saveAsFile ? "attachment" : "inline";
            Page.Response.Clear();
            Page.Response.Buffer = false;
            Page.Response.AppendHeader("Content-Type", string.Format("application/{0}", fileFormat));
            Page.Response.AppendHeader("Content-Transfer-Encoding", "binary");
            Page.Response.AppendHeader("Content-Disposition", string.Format("{0}; filename={1}.{2}", disposition, fileName, fileFormat));
            Page.Response.BinaryWrite(stream.GetBuffer());
            Page.Response.End();
        }

        private object[] FindGroupValues(ASPxGridView grid)
        {
            grid.DataBind();
            string column = grid.GetGroupedColumns()[0].FieldName;
            ArrayList list = new ArrayList();
            for (int i = 0; i < grid.VisibleRowCount; i++)
            {
                if (grid.GetRowLevel(i) == 0)
                {
                    list.Add(grid.GetRowValues(i, column));
                }
            }
            return list.ToArray();
        }
    }
}