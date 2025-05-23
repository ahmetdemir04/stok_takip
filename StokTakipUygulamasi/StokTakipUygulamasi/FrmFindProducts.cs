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
    public partial class FrmFindProducts : Form
    {
        public FrmFindProducts()
        {
            InitializeComponent();
        }
        SqlConnection connection = new SqlConnection(SqlConnectionCls.ConString);
        private void ListData()
        {
            if (connection.State != ConnectionState.Open)
            {
                gridProductList.Rows.Clear(); // Clear previous data
                try
                {
                    connection.Open();
                    string ListProduct = "exec ListProduct";
                    using (SqlDataAdapter adapter = new SqlDataAdapter(ListProduct, connection))
                    {
                        DataTable dt = new DataTable();
                        dt.Clear();
                        adapter.Fill(dt);


                        foreach (DataRow row in dt.Rows)
                        {
                            int prdID = (int)row["ProductID"];
                            string prdName = row["Product"].ToString();
                            string prdBarkod = row["BarkodNo"].ToString();
                            string prdCategory = row["Category"].ToString();
                            string prdBrands = row["Brands"].ToString();
                            int prdStock = (int)row["Stock"];
                            decimal prdPurchasePrice = (decimal)row["PurchasePrice"];
                            decimal prdSalePrice = (decimal)row["SalePrice"];
                            short prdKDVratio = (short)row["KDVRatio"];
                            string prdImg = row["Image"].ToString();


                            int index = gridProductList.Rows.Add();
                            gridProductList.Rows[index].Cells["ProductID"].Value = prdID;
                            gridProductList.Rows[index].Cells["Product"].Value = prdName;
                            gridProductList.Rows[index].Cells["BarkodNo"].Value = prdBarkod;
                            gridProductList.Rows[index].Cells["Category"].Value = prdCategory;
                            gridProductList.Rows[index].Cells["Brands"].Value = prdBrands;
                            gridProductList.Rows[index].Cells["Stock"].Value = prdStock;
                            gridProductList.Rows[index].Cells["PurchasePrice"].Value = prdPurchasePrice;
                            gridProductList.Rows[index].Cells["SalePrice"].Value = prdSalePrice;
                            gridProductList.Rows[index].Cells["KDVRatio"].Value = prdKDVratio;
                            gridProductList.Rows[index].Cells["GridImage"].Value = prdImg;
                        }
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show("Listeleme yaparken hatayla karşılaşıldı! Hata detayı:" + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally { connection.Close(); }

            }
        }

        private void FrmFindProducts_Load(object sender, EventArgs e)
        {
            CultureInfo trculture = new CultureInfo("tr-TR");
            Thread.CurrentThread.CurrentCulture = trculture;
            Thread.CurrentThread.CurrentUICulture = trculture;
            ListData();
        }

        private void TxtProductName_TextChanged(object sender, EventArgs e)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            string search = @"SELECT ProductID, BarkodNo, [Product], ctg.Category, brd.Brands,Stock,PurchasePrice,SalePrice,KDVRatio,[Image] FROM TblProducts pr
INNER JOIN TblCategory ctg on pr.Category = ctg.CategoryID 
INNER JOIN TblBrands brd on pr.Brands = brd.BrandsId
WHERE pr.[Product] LIKE @p1";
            using (SqlDataAdapter adapter = new SqlDataAdapter(search, connection))
            {
                adapter.SelectCommand.Parameters.AddWithValue("@p1", "%" + TxtSearchProduct.Text.Trim() + "%"); // Use parameterized query to prevent SQL injection
                DataTable srtb = new DataTable();
                adapter.Fill(srtb);

                gridProductList.AutoGenerateColumns = false; // You will control it YOURSELF
                gridProductList.Rows.Clear(); // Clear previous data

                foreach (DataRow row in srtb.Rows)
                {
                    int rowIndex = gridProductList.Rows.Add(); // Add a new line
                    DataGridViewRow newRow = gridProductList.Rows[rowIndex];

                    // Assign data based on existing columns
                    newRow.Cells["ProductId"].Value = row["ProductId"];
                    newRow.Cells["Product"].Value = row["Product"];
                    newRow.Cells["BarkodNo"].Value = row["BarkodNo"];
                    newRow.Cells["Category"].Value = row["Category"];
                    newRow.Cells["Brands"].Value = row["Brands"];
                    newRow.Cells["Stock"].Value = row["Stock"];
                    newRow.Cells["PurchasePrice"].Value = row["PurchasePrice"];
                    newRow.Cells["SalePrice"].Value = row["SalePrice"];
                    newRow.Cells["KDVRatio"].Value = row["KDVRatio"];
                    newRow.Cells["GridImage"].Value = row["Image"];

                }
            }
        }

        private void gridProductList_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {

            // MainForm üzerinden erişim:

            FrmSatis.Instance.TxtBarkodNo.Text = gridProductList.CurrentRow.Cells["BarkodNo"].Value.ToString();
            this.Close();
        }
    }
}
