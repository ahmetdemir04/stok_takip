using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StokTakipUygulamasi
{
    public partial class FrmSalesDetail : Form
    {
        public FrmSalesDetail()
        {
            InitializeComponent();
        }
        SqlConnection connection = new SqlConnection(SqlConnectionCls.ConString);
        public string SalesID;

        private void FrmSalesDetail_Load(object sender, EventArgs e)
        {

            CultureInfo trculture = new CultureInfo("tr-TR");

            Thread.CurrentThread.CurrentCulture = trculture;
            Thread.CurrentThread.CurrentUICulture = trculture;

            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            try
            {
                DataTable table = new DataTable();

                string ListSell = "select SalesDetailID,tpr.[Product],Qty,Unitprice,Total from TblSalesDetail tsd \r\nINNER JOIN TblProducts tpr\r\non tsd.[Product] = tpr.ProductID WHERE tsd.SalesID = @id";
                using (SqlDataAdapter ListSellDetailQuery = new SqlDataAdapter(ListSell, connection))
                {
                    ListSellDetailQuery.SelectCommand.Parameters.AddWithValue("@id", SalesID);
                    ListSellDetailQuery.Fill(table);
                
                }


                foreach (DataRow row in table.Rows)
                {
                    int index = gridSales.Rows.Add();
                    gridSales.Rows[index].Cells["SalesDetailID"].Value = row["SalesDetailID"].ToString();
                    gridSales.Rows[index].Cells["Product"].Value = row["Product"].ToString();
                    gridSales.Rows[index].Cells["UnitPrice"].Value = decimal.Parse(row["UnitPrice"].ToString());
                    gridSales.Rows[index].Cells["Qty"].Value = row["Qty"].ToString();
                    gridSales.Rows[index].Cells["Total"].Value = decimal.Parse(row["Total"].ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Listeleme esnasında bir hatayla karşılaşıldı! Hata detayı:! \n" + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { connection.Close(); }
        }
    }
}
