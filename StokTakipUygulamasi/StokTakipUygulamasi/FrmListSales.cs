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
    public partial class FrmListSales : Form
    {
        public FrmListSales()
        {
            InitializeComponent();
        }
        SqlConnection connection = new SqlConnection(SqlConnectionCls.ConString);
        private void FrmSatisListele_Load(object sender, EventArgs e)
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

                string ListSell = "Select * From TblSales";
                using (SqlDataAdapter ListSellQuery = new SqlDataAdapter(ListSell, connection))
                {
                    ListSellQuery.Fill(table);   
                }

                foreach (DataRow row in table.Rows)
                {
                    int index = gridSales.Rows.Add();
                    gridSales.Rows[index].Cells["SalesID"].Value = row["SalesID"].ToString();
                    gridSales.Rows[index].Cells["CustomerTC"].Value = row["CustomerTC"].ToString();
                    gridSales.Rows[index].Cells["PhoneNumber"].Value = row["PhoneNumber"].ToString();
                    gridSales.Rows[index].Cells["CustomerNameSurname"].Value = row["CustomerName"] + " " + row["CustomerSurname"];
                    gridSales.Rows[index].Cells["TotalCost"].Value = decimal.Parse(row["TotalCost"].ToString());
                    gridSales.Rows[index].Cells["Date"].Value = row["Date"].ToString();
                    gridSales.Rows[index].Cells["PaymentType"].Value = row["PaymentType"].ToString();
                    gridSales.Rows[index].Cells["SalesRepresentative"].Value = row["SalesRepresentative"].ToString();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Listeleme esnasında bir hatayla karşılaşıldı! Hata detayı:! \n" + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { connection.Close(); }
        }

        private void gridSales_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            FrmSalesDetail fSaleDetail = new FrmSalesDetail();
            fSaleDetail.SalesID = gridSales.CurrentRow.Cells["SalesID"].Value.ToString();
            fSaleDetail.ShowDialog();
        }
    }
}
