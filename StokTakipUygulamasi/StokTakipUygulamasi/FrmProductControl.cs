using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace StokTakipUygulamasi
{
    public partial class FrmProductControl : Form
    {
        public FrmProductControl()
        {
            InitializeComponent();
        }


        SqlConnection connection = new SqlConnection(SqlConnectionCls.ConString);
        FunctionCls Fcls = new FunctionCls();
        string DefaultImgPath = Path.Combine(Application.StartupPath, "Images", "noproductimage.jpg");


        #region Listing_Operations 
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

                    MessageBox.Show("Listeleme yaparken bi hatayla karşılaşıldı! Hata Detayı:" + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally { connection.Close(); }

            }
        }

        private void ListCategory()
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            try
            {
                DataTable cdt = new DataTable();
                string query = "SELECT * FROM TblCategory";
                using (SqlDataAdapter adp = new SqlDataAdapter(query, connection))
                {
                    adp.Fill(cdt);
                    CmbCategory.DataSource = cdt;
                    CmbCategory.DisplayMember = "Category";
                    CmbCategory.ValueMember = "CategoryID";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kategori listelemesi yaparken bir hatayla karşılaşıldı! Hata detayı: \n" + ex, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                connection.Close();
            }
        }
        private void ListBrands()
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            try
            {
                DataTable bdt = new DataTable();
                string query = "SELECT * FROM TblBrands";
                using (SqlDataAdapter adp = new SqlDataAdapter(query, connection))
                {
                    adp.Fill(bdt);
                    CmbBrands.DataSource = bdt;
                    CmbBrands.DisplayMember = "Brands";
                    CmbBrands.ValueMember = "BrandsId";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Markaları listelerken bi hatayla karşılaşıldı! Hata Detayı:\n" + ex, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                connection.Close();
            }
        }
        #endregion

        private void Clear()
        {
            foreach (Control item in this.Controls)
            {
                if (item is TextBox)
                {
                    item.Text = "";
                }
            }
            CmbBrands.SelectedIndex = -1;
            CmbCategory.SelectedIndex = -1;
        }

        private void FrmUrunListele_Load(object sender, EventArgs e)
        {
            CultureInfo turkish = new CultureInfo("tr-TR");
            Thread.CurrentThread.CurrentCulture = turkish;
            Thread.CurrentThread.CurrentUICulture = turkish;
            ListData(); ListCategory(); ListBrands();
            Clear();
        }

        private void Btn_refresh_Click(object sender, EventArgs e)
        {
            ListData();
        }
        #region Search Item
        private void TxtSearch_TextChanged(object sender, EventArgs e)
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
                adapter.SelectCommand.Parameters.AddWithValue("@p1", "%" + TxtSearch.Text.Trim() + "%"); // Use parameterized query to prevent SQL injection
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

        #endregion
        private string CopyImage(string barcode)
        {
            // PictureBox'ın resmi bir dosyadan yüklendiyse
            if (UploadedImg != null)
            {
                string image_source = UploadedImg;

                string projectRoot = Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\.."));

                // 4) Hedef klasör: projeKök\Images\Products
                string ImgPathInProject = Path.Combine(projectRoot, "Images", "Products", barcode + ".jpg");

                string ImgPathInDebug = Path.Combine(Application.StartupPath, "Images", "Products", barcode + ".jpg");
                string everyapth = Path.Combine("Images", "Products", barcode + ".jpg");
                try
                {
                    if (pictureBox1.Image != null)
                    {
                        pictureBox1.Image.Dispose();
                        pictureBox1.Image = null;
                        GC.Collect(); // Garbage Collector çalıştır (güvenlik amaçlı)
                        GC.WaitForPendingFinalizers(); // Bekleyen işlemleri tamamla
                    }

                    if (File.Exists(ImgPathInProject))
                    {
                        File.Delete(ImgPathInProject);
                    }

                    if (File.Exists(ImgPathInDebug))
                    {
                        File.Delete(ImgPathInDebug);
                    }

                    File.Copy(image_source, ImgPathInProject, true);

                    File.Copy(image_source, ImgPathInDebug, true);

                    imgStatus = false;

                    return everyapth;
                }
                catch (Exception ex)
                {

                    MessageBox.Show($"Resim kopyalanırken hata oldu: \n {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Resimi kopyalarken bi hatayla karşılaşıldı!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return null;
        }
        private int[] CategoryAndBrandsFind(string _category, string _brands)
        {
            int[] willreturn = new int[2];
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            try
            {
                object brandid, categoryid;

                string query_find_brand = "SELECT BrandsID FROM TblBrands WHERE Brands = @brn";
                using (SqlCommand brandsql = new SqlCommand(query_find_brand, connection))
                {
                    brandsql.Parameters.AddWithValue("@brn", _brands);
                    brandid = brandsql.ExecuteScalar();
                }
                string query_find_category = "SELECT CategoryID FROM TblCategory WHERE Category = @cat ";
                using (SqlCommand categorysql = new SqlCommand(query_find_category, connection))
                {
                    categorysql.Parameters.AddWithValue("@cat", _category);
                    categoryid = categorysql.ExecuteScalar();
                }

                willreturn[0] = (int)categoryid;
                willreturn[1] = (int)brandid;
            }
            catch (Exception)
            {
                throw;
            }
            return willreturn;
        }
        private void Btn_update_Click(object sender, EventArgs e)
        {
            string barcode = TxtBarkod.Text.Trim();
            string ImgLocation = "";
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            try
            {
                if (TxtBarkod.Text != "")
                {
                    string update = "UPDATE TblProducts SET Product=@prd,Category=@ctg,Brands=@brnd,Stock=@stc, PurchasePrice = @prchprice, SalePrice=@saleprice, KDVRatio = @kdvrt, Image=@img WHERE BarkodNo=@brc";
                    using (SqlCommand UpdateQuery = new SqlCommand(update, connection))
                    {
                        int[] getCatBrnValue = CategoryAndBrandsFind(CmbCategory.Text, CmbBrands.Text);

                        UpdateQuery.Parameters.AddWithValue("@prd", TxtProductName.Text.Trim());
                        UpdateQuery.Parameters.AddWithValue("@ctg", getCatBrnValue[0]);
                        UpdateQuery.Parameters.AddWithValue("@brnd", getCatBrnValue[1]);
                        UpdateQuery.Parameters.AddWithValue("@stc", int.Parse(TxtQty.Text.Trim()));
                        UpdateQuery.Parameters.AddWithValue("@prchprice", Fcls.ConvertDecimal(TxtPurchasePrice.Text));
                        UpdateQuery.Parameters.AddWithValue("@saleprice", Fcls.ConvertDecimal(TxtSellPrice.Text));
                        UpdateQuery.Parameters.AddWithValue("@kdvrt", int.Parse(txtKDVratio.Text.Trim()));

                        UpdateQuery.Parameters.AddWithValue("@brc", TxtBarkod.Text.Trim());


                        if (imgStatus == true)
                        {
                            CopyImage(barcode);
                            UpdateQuery.Parameters.AddWithValue("@img", TxtBarkod.Text.Trim() + ".jpg");
                        }
                        else
                        {
                            string currentImg = Path.Combine(Application.StartupPath, "Images", "Products", barcode + ".jpg");
                            if (File.Exists(currentImg))
                            {
                                UpdateQuery.Parameters.AddWithValue("@img", barcode + ".jpg");
                            }
                            else
                            {
                                ImgLocation = DefaultImgPath;
                                UpdateQuery.Parameters.AddWithValue("@img", ImgLocation);
                            }

                        }
                        UpdateQuery.ExecuteNonQuery();

                        MessageBox.Show("Ürün Güncellendi!", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        pictureBox1.Image = Image.FromFile(Path.Combine(Application.StartupPath, "Images", "Products", barcode + ".jpg"));
                        Clear();
                        ListData();
                    }
                }
                else
                {
                    MessageBox.Show("Barkot numarası seçili değil!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Güncelleme işlemi yapılırken bi hatayla karşılaşıldı! Hata detayı:" + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                connection.Close();
            }
        }
        string UploadedImg = "";
        bool imgStatus = false;
        private void btnImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Title = "Select an image";
            fd.Filter = "Image Files|*.jpg;*.jpeg;*.png;";

            try
            {
                if (fd.ShowDialog() == DialogResult.OK)
                {

                    byte[] ImgData = File.ReadAllBytes(fd.FileName);
                    using (MemoryStream ms = new MemoryStream(ImgData))
                    {
                        Image tempImage = Image.FromStream(ms);
                        pictureBox1.Image = new Bitmap(tempImage);
                    }

                    UploadedImg = fd.FileName;
                    imgStatus = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Resim içeriye aktarılırken bi hatayla karşılaşıldı! Hata detayı: \n" + ex, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    
        private void gridProductList_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            string ImgName = gridProductList.CurrentRow.Cells["GridImage"].Value.ToString();
            string BringImg = Path.Combine(Application.StartupPath, "Images", "Products", ImgName);

            try
            {
                if (ImgName != "" && ImgName != null)
                {
                    pictureBox1.Image = Image.FromFile(BringImg);
                }
                else
                {
                    pictureBox1.Image = Image.FromFile(DefaultImgPath);
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show("Program was faced an error during adding image! Error Detail: \n" + ex, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            txtID.Text = gridProductList.CurrentRow.Cells["ProductID"].Value.ToString();
            TxtBarkod.Text = gridProductList.CurrentRow.Cells["BarkodNo"].Value.ToString();
            CmbCategory.Text = gridProductList.CurrentRow.Cells["Category"].Value.ToString();
            CmbBrands.Text = gridProductList.CurrentRow.Cells["Brands"].Value.ToString();
            TxtProductName.Text = gridProductList.CurrentRow.Cells["Product"].Value.ToString();
            TxtQty.Text = gridProductList.CurrentRow.Cells["Stock"].Value.ToString();
            TxtPurchasePrice.Text = gridProductList.CurrentRow.Cells["PurchasePrice"].Value.ToString();
            TxtSellPrice.Text = gridProductList.CurrentRow.Cells["SalePrice"].Value.ToString();
            txtKDVratio.Text = gridProductList.CurrentRow.Cells["KDVRatio"].Value.ToString();
        }
        #region Delete Item
        private void silToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int id = (int)gridProductList.CurrentRow.Cells["ProductID"].Value;
            string barkod = gridProductList.CurrentRow.Cells["BarkodNo"].Value.ToString();

            DialogResult result = MessageBox.Show($"{barkod} numaralı kaydı silmek istediğinize emin misiniz?", "Silme Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                string delete = "DELETE FROM TblProducts WHERE ProductID=@id";
                using (SqlCommand cmd = new SqlCommand(delete, connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    cmd.ExecuteNonQuery();
                    connection.Close();
                    ListData();
                }
            }

        }


        #endregion

        private void btnAddProduct_Click(object sender, EventArgs e)
        {
            FrmProductsAdd fr = new FrmProductsAdd();
            fr.ShowDialog();
        }

        private void TxtSellPrice_Leave(object sender, EventArgs e)
        {
            Fcls.Textboxformatter(TxtSellPrice);
        }

        private void TxtPurchasePrice_Leave(object sender, EventArgs e)
        {
            Fcls.Textboxformatter(TxtPurchasePrice);
        }

        #region TxtFormatControl
        public void OnlyNumericCharacter(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void OnlyAlphabaticCharacter(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetter(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }
        #endregion

        private void txtKDVratio_TextChanged(object sender, EventArgs e)
        {
            int vergi = Convert.ToInt16(txtKDVratio.Text);


            if (vergi >= 40)
            {
                MessageBox.Show("Vergi oranı %40'dan fazla olamaz!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtKDVratio.Text = "0";
            }
        }
    }
}
