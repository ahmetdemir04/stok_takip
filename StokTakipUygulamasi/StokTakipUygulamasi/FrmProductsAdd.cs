using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StokTakipUygulamasi
{
    public partial class FrmProductsAdd : Form
    {
        public FrmProductsAdd()
        {
            InitializeComponent();
        }
        SqlConnection connection = new SqlConnection(SqlConnectionCls.ConString);
        FunctionCls Fcls = new FunctionCls();
        private bool EmptyFieldCheck()
        {
            if (TxtBarcode.Text != "" && CmbCategory.SelectedIndex != -1 && CmbBrand.SelectedIndex != -1 && TxtProductName.Text != "" && TxtKdvRatio.Text != "" && TxtPurchasePrice.Text != "" && TxtSellPrice.Text != "")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void ClearFields()
        {
            TxtProductName.Clear();
            CmbCategory.SelectedIndex = -1;
            CmbBrand.SelectedIndex = -1;
            CmbBrand.Text = "";
            TxtQty.Text = 0.ToString();
            TxtPurchasePrice.Clear();
            TxtSellPrice.Clear();
            TxtKdvRatio.Text = 0.ToString();
            
        }


        private bool checkproduct()
        {
            bool status = true;
            connection.Open();
            SqlCommand cmd = new SqlCommand("Select BarkodNo from TblProducts ", connection);
            SqlDataReader rd = cmd.ExecuteReader();

            while (rd.Read())
            {
                if (rd[0].ToString() == TxtBarcode.Text)
                {
                    status = false;
                    break;
                }
            }
            connection.Close();
            return status;
        }
        private int CreatenQrCode()
        {
            Random rnd = new Random();
            int qrCode = 0;
            bool check = true;

            while (check)
            {
                qrCode = rnd.Next(10000000, 99999999);
                check = false;
                try
                {
                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }

                    string checkquery = "SELECT BarkodNo FROM TblProducts WHERE Barkodno=@qcode";
                    using (SqlCommand cmd = new SqlCommand(checkquery, connection))
                    {
                        cmd.Parameters.AddWithValue("@qcode", qrCode);
                        SqlDataReader rd = cmd.ExecuteReader();
                        if (rd.Read())
                        {
                            if ((int)rd[0] == qrCode)
                            {
                                check = true;
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("QR kod tanımlaması yapılırken bi hatayla karşılaşıldı! Hata detayı: \n" + ex, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally { connection.Close(); }
            }
            return qrCode;
        }
        string DefaultImgPath = Path.Combine(Application.StartupPath, "Images", "noproductimage.jpg");
        private void Btn_add_Click(object sender, EventArgs e)
        {
            if (checkproduct())
            {
                if (EmptyFieldCheck())
                {

                    try
                    {
                        if (connection.State != ConnectionState.Open)
                        {
                            connection.Open();
                        }
                        if (is_there_image == true)
                        {
                            string debugpath = Path.Combine(Application.StartupPath, "Images", "Products", TxtBarcode.Text.Trim() + ".jpg");

                            string projectRoot = Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..", "Images", "Products", TxtBarcode.Text + ".jpg"));

                            File.Copy(img_source, debugpath);
                            File.Copy(img_source, projectRoot);
                        }
                      

                        string AddProduct = "INSERT INTO TblProducts (BarkodNo,Product,Category,Brands,Stock,PurchasePrice,SalePrice,KDVRatio,Image) VALUES (@brkdno,@prd,@cat,@brd,@stck,@purprc,@saleprc,@kdv,@img)";

                        using (SqlCommand AddProductQuery = new SqlCommand(AddProduct, connection))
                        {
                            AddProductQuery.Parameters.AddWithValue("@brkdno", TxtBarcode.Text.Trim());
                            AddProductQuery.Parameters.AddWithValue("@prd", TxtProductName.Text.Trim());
                            AddProductQuery.Parameters.AddWithValue("@cat", Convert.ToInt32(CmbCategory.SelectedValue));
                            AddProductQuery.Parameters.AddWithValue("@brd", Convert.ToInt32(CmbBrand.SelectedValue));
                            AddProductQuery.Parameters.AddWithValue("@stck", Convert.ToInt32(TxtQty.Text.Trim()));
                            AddProductQuery.Parameters.AddWithValue("@purprc", Fcls.ConvertDecimal(TxtPurchasePrice.Text));
                            AddProductQuery.Parameters.AddWithValue("@saleprc", Fcls.ConvertDecimal(TxtSellPrice.Text));
                            AddProductQuery.Parameters.AddWithValue("@kdv", Convert.ToInt32(TxtKdvRatio.Text.Trim()));

                            if (is_there_image == true)
                            {
                                AddProductQuery.Parameters.AddWithValue("@img", TxtBarcode.Text.Trim() + ".jpg");
                            }
                            else
                            {
                                AddProductQuery.Parameters.AddWithValue("@img",DefaultImgPath) ;
                            }

                            AddProductQuery.ExecuteNonQuery();


                            MessageBox.Show("Yeni ürün eklendi!", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearFields();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Yeni ürün eklenirken bi hatayla karşılaşıldı! Hata detayı: \n" + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally { connection.Close(); }
                    
                }
                else
                {
                    MessageBox.Show("Yeni ürün tanımlaması yapamk için bütün alanları doldurmak zorundasınız!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Böyle bir ürün mevcut!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private void BtnCreateQr_Click(object sender, EventArgs e)
        {
            int Qr = CreatenQrCode();
            TxtBarcode.Text = Qr.ToString();
        }

        string img_source;
        bool is_there_image = false;
        private void btnImport_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog fd = new OpenFileDialog();
                fd.Title = "Resim Seç";
                fd.Filter = "Image File|*.jpg;*.jpeg;*.png";
                if (fd.ShowDialog() == DialogResult.OK)
                {
                    pctProduct.Image = Image.FromFile(fd.FileName);
                    img_source = fd.FileName;
                    is_there_image = true;
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show("Resim içeri aktarılırken bi hatayla karşılaşıldı! \n  Hata detayı: \n " + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
   
        }
        private void BringCategory()
        {
            if (connection.State != ConnectionState.Open)
            {
                try
                {
                    connection.Open();

                    DataTable dtc = new DataTable();
                    string BringCategory = "SELECT * FROM TblCategory";
                    using (SqlDataAdapter BringCategoryQuery = new SqlDataAdapter(BringCategory, connection))
                    {
                        BringCategoryQuery.Fill(dtc);
                        CmbCategory.DataSource = dtc;
                        CmbCategory.DisplayMember = "Category";
                        CmbCategory.ValueMember = "CategoryID";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Combobox'a kategori getirilirken bi hatayla karşılaşıldı! Hata detayı: \n" +ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally { connection.Close(); }

            }
        }
        private void BringBrand()
        {
            CmbBrand.DataSource = null;
            CmbBrand.Items.Clear();
            CmbBrand.Text = "";
            if (connection.State != ConnectionState.Open)
            {
                try
                {
                    connection.Open();
                    DataTable dtb = new DataTable();
                    string BringBrands = $"SELECT \r\n    tb.Brands,\r\n    tc.Category,\r\n\ttb.BrandsId\r\nFROM \r\n    TblCategoryBrands tcb\r\nINNER JOIN \r\n    TblCategory tc ON tcb.Category = tc.CategoryID\r\nINNER JOIN \r\n    TblBrands tb ON tcb.Brands = tb.BrandsId\r\nWHERE \r\n    tc.CategoryID = @cat";
                    using (SqlDataAdapter BringBrandsQuery = new SqlDataAdapter(BringBrands, connection))
                    {
                        BringBrandsQuery.SelectCommand.Parameters.AddWithValue("@cat", CmbCategory.SelectedValue);
                        BringBrandsQuery.Fill(dtb);
                        CmbBrand.DataSource = dtb;
                        CmbBrand.DisplayMember = "Brands";
                        CmbBrand.ValueMember = "BrandsId";
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show("Combobox'a marka getirilirken bi hatayla karşılaşıldı! Hata detayı: ex" + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally { connection.Close(); }

            }
        }
        private void FrmUrunEkle_Load(object sender, EventArgs e)
        {
            BringCategory();
        }

        private void CmbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            BringBrand();
        }

        private void TxtBarcodeNo_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void TxtBarcodeNo2_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void TxtPurchasePrice_Leave(object sender, EventArgs e)
        {
            Fcls.Textboxformatter(TxtPurchasePrice);
        }

        private void TxtSellPrice_Leave(object sender, EventArgs e)
        {
            Fcls.Textboxformatter(TxtSellPrice);
        }


        private void TxtKdvRatio_TextChanged(object sender, EventArgs e)
        {
            int vergi = Convert.ToInt16(TxtKdvRatio.Text);


            if (vergi >= 40)
            {
                MessageBox.Show("Vergi oranı %40'dan fazla olamaz!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                TxtKdvRatio.Text = "0";
            }
        }

        public void OnlyNumericCharacter(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }
    }
}
