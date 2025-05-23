using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StokTakipUygulamasi
{
    public partial class FrmSatis : Form
    {
        public static FrmSatis Instance;
        public FrmSatis()
        {
            InitializeComponent();
            Instance = this;
        }
        SqlConnection connection = new SqlConnection(SqlConnectionCls.ConString);
        FunctionCls function = new FunctionCls();
        public int personal = 1;
        #region Button_Direction
        private void Btn_AddProducts_Click(object sender, EventArgs e)
        {
            FrmProductsAdd urunekle = new FrmProductsAdd();
            urunekle.ShowDialog();
        }

        private void Add_Brand_Click(object sender, EventArgs e)
        {
            FrmBrands marka = new FrmBrands();
            marka.ShowDialog();
        }

        private void Add_Category_Click(object sender, EventArgs e)
        {
            FrmCategory kategori = new FrmCategory();
            kategori.ShowDialog();
        }

        private void Btn_ListProducts_Click(object sender, EventArgs e)
        {
            FrmProductControl urunlistele = new FrmProductControl();
            urunlistele.ShowDialog();
        }
        private void Btn_ListSell_Click(object sender, EventArgs e)
        {
            FrmListSales satislistele = new FrmListSales();
            satislistele.ShowDialog();
        }
        #endregion
  
     
        public string SellerUserName;
        public string SellerRole;
        private void FrmSatis_Load(object sender, EventArgs e)
        {
            if (SellerRole != "admin")
            {
                Btn_AddProducts.Enabled = false;
                Btn_ListProducts.Enabled = false;
                Add_Category.Enabled = false; 
                Add_Brand.Enabled = false;
            }
        }
        private void CalculateTotalCost()
        {
            if (connection.State != ConnectionState.Open)
            {
                try
                {
                    connection.Open();
                    string calculate = "Select sum(toplamfiyat) from sepet WHERE tc=@tc ";
                    SqlCommand CalculateQuery = new SqlCommand(calculate, connection);
                    CalculateQuery.Parameters.AddWithValue("@tc", TxtTC.Text);
                    LblGenelToplam.Text = CalculateQuery.ExecuteScalar() + "TL";
                }
                catch (Exception ex)
                {

                    MessageBox.Show("Genel toplam hesaplanamadı! \n " + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
                finally { connection.Close(); }
            }
        }
        bool status ;
        private void CheckBarcodeControl()
        {
            connection.Open();
            status = true;
            using (SqlCommand cmd = new SqlCommand("Select * from sepet WHERE tc=@tc", connection))
            {
                cmd.Parameters.AddWithValue("@tc", TxtTC.Text);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (TxtBarkodNo.Text == reader["barkodno"].ToString())
                    {
                        status = false;
                    }
                }
            }
            connection.Close();
        }



        private void Clear()
        {
            foreach (Control item in groupBox1.Controls)
            {
                if (item is TextBox)
                {
                    item.Text = "";
                }
            }
            TxtBarkodNo.Text = "";
            TxtQty.Text = "1";
            TxtProductName.Text = "";
            pictureBox1.Image = Image.FromFile(FunctionCls.DefaultImgPath);



            mskPhone.Text = "";
            cmbPaymentMethod.SelectedIndex = -1;

            LblGenelToplam.Text = "00";
            
        }
      
        private bool CheckCustomerInformation()
        {
            if (
                !string.IsNullOrEmpty(TxtTC.Text) &&
                !string.IsNullOrEmpty(TxtName.Text) &&
                !string.IsNullOrEmpty(TxtSurname.Text) &&
                !string.IsNullOrEmpty(mskPhone.Text) &&
                !string.IsNullOrEmpty(cmbPaymentMethod.Text)
                )
            {
                return true;
            }
            else
            {
                MessageBox.Show("Lütfen müşteri bilgilerini eksiksiz doldurunuz!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        private void StockControl(DataGridView dgv, SqlTransaction trans)
        {

            foreach (DataGridViewRow row in dgv.Rows)
            {
                string query = "UPDATE TblProducts SET Stock = Stock - @stc WHERE BarkodNo=@brk AND Stock >= @stc";

                using (SqlCommand stockcommand = new SqlCommand(query, connection, trans))
                {
                    stockcommand.Parameters.AddWithValue("@brk", row.Cells["ProductBarcode"].Value.ToString());
                    stockcommand.Parameters.AddWithValue("@stc", Convert.ToInt32(row.Cells["qty"].Value));

                    int affectedrow = stockcommand.ExecuteNonQuery();
                    if (affectedrow == 0)
                    {
                        MessageBox.Show("Stokta yeterli ürün yok!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void Btn_Satis_Yap_Click(object sender, EventArgs e)
        {
            if (gridSales.Rows.Count > 0)
            {
                if (CheckCustomerInformation())
                {
                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }
                    SqlTransaction transaction = connection.BeginTransaction();
                    try
                    {
                        StockControl(gridSales, transaction);

                        string add_bill_query = "INSERT INTO TblSales (CustomerTC, PhoneNumber, CustomerName, CustomerSurname, PaymentType,TotalCost,Date,SalesRepresentative) OUTPUT INSERTED.SalesID VALUES (@ctc, @pn, @name, @surname, @ptype,@tcost,@date,@salerepresent)";



                        SqlCommand billcommand = new SqlCommand(add_bill_query, connection, transaction);

                        billcommand.Parameters.AddWithValue("@ctc", TxtTC.Text);
                        billcommand.Parameters.AddWithValue("@pn", mskPhone.Text);
                        billcommand.Parameters.AddWithValue("@name", TxtName.Text.Trim());
                        billcommand.Parameters.AddWithValue("@surname", TxtSurname.Text.Trim());
                        billcommand.Parameters.AddWithValue("@ptype", cmbPaymentMethod.Text);
                        billcommand.Parameters.AddWithValue("@tcost", function.ConvertDecimal(LblGenelToplam.Text));
                        billcommand.Parameters.AddWithValue("@date", DateTime.Now);
                        billcommand.Parameters.AddWithValue("@salerepresent", SellerUserName);

                        int salesID = (int)billcommand.ExecuteScalar();


                        string sales_detail_query = "INSERT INTO TblSalesDetail (SalesID, Product, UnitPrice, Qty, Total) VALUES (@sid, @pr, @unitprice, @qty, @total)";

                        foreach (DataGridViewRow row in gridSales.Rows)
                        {
                            using (SqlCommand salesdetailcommand = new SqlCommand(sales_detail_query, connection, transaction))
                            {
                                salesdetailcommand.Parameters.AddWithValue("@sid", salesID);
                                salesdetailcommand.Parameters.AddWithValue("@pr", row.Cells["ProductId"].Value.ToString());
                                salesdetailcommand.Parameters.AddWithValue("@unitprice", function.ConvertDecimal(row.Cells["UnitPrice"].Value.ToString()));
                                salesdetailcommand.Parameters.AddWithValue("@qty", Convert.ToInt32(row.Cells["Qty"].Value));
                                salesdetailcommand.Parameters.AddWithValue("@total", function.ConvertDecimal(row.Cells["Total"].Value.ToString()));
                                salesdetailcommand.ExecuteNonQuery();
                            }
                        }
                        transaction.Commit();
                        MessageBox.Show("Satış işlemi başarıyla gerçekleştirildi!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        gridSales.Rows.Clear();


                    }
                    catch (Exception ex)
                    {

                        MessageBox.Show("Satış işlemi sırasında bi hata ile karşılaşıldı! Error Detaıl: \n " + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        connection.Close();
                        Clear();
                      
                    }
                }

            }
            else
            {
                MessageBox.Show("Sepetinizde ürün yok!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TxtTC_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }


        private void BtnFindProducts_Click(object sender, EventArgs e)
        {
            FrmFindProducts f_findprd = new FrmFindProducts();
            f_findprd.ShowDialog();
        }
        private bool BarkcodeCheck(string barcode)
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                string query = "SELECT * FROM TblProducts WHERE BarkodNo=@barkod";
                using (SqlCommand check = new SqlCommand(query, connection))
                {
                    check.Parameters.AddWithValue("@barkod", barcode);
                    using (SqlDataReader rd = check.ExecuteReader())
                    {
                        if (rd.Read())
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show("Ürün kontrol edilirken bi hatayla karşılaşıldı! Error Detayı: \n" + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { connection.Close(); }
            return false;
        }

        private bool CheckForAdd()
        {
            if (TxtBarkodNo.Text != null &&
                TxtBarkodNo.Text != "" &&
                Convert.ToInt32(TxtQty.Text) != 0)

            {
                return true;
            }
            return false;
        }
        private bool doesProductExist(string barcode)
        {
            foreach (DataGridViewRow row in gridSales.Rows)
            {
                string relevant_row = row.Cells["ProductBarcode"].Value.ToString();
                if (relevant_row == barcode)
                {
                    return false;
                }
            }
            return true;
        }
        private void CalculateFunction()
        {
            decimal total = 0;
            foreach (DataGridViewRow row in gridSales.Rows)
            {
                if (row.Cells["Total"].Value != null)
                {
                    total += Convert.ToDecimal(row.Cells["Total"].Value);
                }
            }
            LblGenelToplam.Text = function.DecimalFormatter(total) + " ₺";
        }
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (CheckForAdd())
            {
                try
                {

                    string Barcode = TxtBarkodNo.Text.Trim();
                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }
                    SqlTransaction trans = connection.BeginTransaction();
                    if (doesProductExist(Barcode))
                    {
                        string query = "SELECT * FROM TBLPRODUCTS WHERE BarkodNO = @barcode";

                        using (SqlCommand cmd = new SqlCommand(query, connection, trans))
                        {
                            cmd.Parameters.AddWithValue("@barcode", TxtBarkodNo.Text.Trim());

                            SqlDataReader reader = cmd.ExecuteReader();

                            while (reader.Read())
                            {
                                int index = gridSales.Rows.Add();

                                decimal sale_price = (decimal)reader["SalePrice"];
                                int qty = Convert.ToInt32(TxtQty.Text.Trim());

                                gridSales.Rows[index].Cells["ProductBarcode"].Value = reader["BarkodNO"];
                                gridSales.Rows[index].Cells["ProductId"].Value = reader["ProductID"];
                                gridSales.Rows[index].Cells["Product"].Value = reader["Product"];
                                gridSales.Rows[index].Cells["UnitPrice"].Value = sale_price;
                                gridSales.Rows[index].Cells["Qty"].Value = qty;


                                gridSales.Rows[index].Cells["Total"].Value = (sale_price * qty);
                            }
                        }
                    }
                    else
                    {
                        int qty = int.Parse(TxtQty.Text.Trim());
                        foreach (DataGridViewRow row in gridSales.Rows)
                        {
                            if (row.Cells["ProductBarcode"].Value != null &&
                                row.Cells["ProductBarcode"].Value.ToString() == Barcode)
                            {
                                if (row.Cells["Qty"].Value != null && row.Cells["UnitPrice"].Value != null)
                                {
                                    int currentQty = Convert.ToInt32(row.Cells["Qty"].Value);
                                    decimal unitPrice = Convert.ToDecimal(row.Cells["UnitPrice"].Value);
                                    int updatedQty = currentQty + qty;

                                    row.Cells["Qty"].Value = updatedQty;
                                    row.Cells["Total"].Value = updatedQty * unitPrice;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ürünü sepete eklerken hatayla karşılaşıldı! Hata detayı: \n " + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally { connection.Close(); CalculateFunction(); }
            }
            else
            {
                MessageBox.Show("Lütfen barkod ve adet alanını doldurunuz!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TxtBarkodNo_TextChanged(object sender, EventArgs e)
        {
            if (TxtBarkodNo.Text.Length >= 8)
            {
                if (!string.IsNullOrEmpty(TxtBarkodNo.Text))
                {
                    string Barcode = TxtBarkodNo.Text.Trim();
                    if (BarkcodeCheck(Barcode))
                    {
                        try
                        {
                            if (connection.State != ConnectionState.Open)
                            {
                                connection.Open();
                            }
                            string img;
                            string query = "SELECT * FROM TblProducts WHERE BarkodNo=@barkod";
                            using (SqlCommand relevantprd = new SqlCommand(query, connection))
                            {
                                relevantprd.Parameters.AddWithValue("@barkod", TxtBarkodNo.Text.Trim());

                                using (SqlDataReader reader = relevantprd.ExecuteReader())
                                {
                                    reader.Read();


                                    TxtProductName.Text = reader["Product"].ToString();
                                    img = reader["Image"].ToString();
                                }

                            }
                            if (img != null && img != "")
                            {
                                string img_path = Path.Combine(Application.StartupPath, "Images", "Products", img);
                                pictureBox1.Image = Image.FromFile(img_path);
                            }

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Connection Error! Error Detail: \n " + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally { connection.Close(); }
                    }
                    else
                    {
                        MessageBox.Show("Barkod numarasına ait ürün bulunamadı!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

            }


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
    }
}
