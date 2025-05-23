using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace StokTakipUygulamasi
{
    public partial class FrmBrands : Form
    {
        public FrmBrands()
        {
            InitializeComponent();
        }
        SqlConnection connection = new SqlConnection(SqlConnectionCls.ConString);
        bool status;
        private void BringBrands()
        {
            gridBrandsList.DataSource = null;
            gridBrandsList.Rows.Clear();
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    DataTable dt = new DataTable();
                    string query = $"SELECT  tcb.CBID, tc.Category,tb.Brands FROM TblCategoryBrands tcb \r\nINNER JOIN   TblCategory tc ON tcb.Category = tc.CategoryID\r\nINNER JOIN   TblBrands tb ON tcb.Brands = tb.BrandsId";
                    using (SqlDataAdapter sda = new SqlDataAdapter(query, connection))
                    {
                        sda.Fill(dt);

                        foreach (DataRow row in dt.Rows)
                        {
                            int ID = (int)row["CBID"];
                            string brand = row["Brands"].ToString();
                            string category = row["Category"].ToString();

                            int index = gridBrandsList.Rows.Add();
                            gridBrandsList.Rows[index].Cells["CBID"].Value = ID;
                            gridBrandsList.Rows[index].Cells["Category"].Value = category;
                            gridBrandsList.Rows[index].Cells["Brands"].Value = brand;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Marka verisi datagride eklenirken bir hatayla karşılaşıldı! Hata Detayı: \n" + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                connection.Close();
            }
        }
        private void BringCategory()
        {
            gridBrandsList.DataSource = null;
            TxtBrands.Clear();
            TxtBrands.Text = "";

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                DataTable dtb = new DataTable();
                string BringBrands = $"SELECT * FROM TblCategory";
                using (SqlDataAdapter BringCategoryQuery = new SqlDataAdapter(BringBrands, connection))
                {
                    BringCategoryQuery.Fill(dtb);
                    CmbCategory.DataSource = dtb;
                    CmbCategory.DisplayMember = "Category";
                    CmbCategory.ValueMember = "CategoryID";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kategori getirilirken bir hatayla karşılaşıldı! Hata Detayı: \n " + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { connection.Close(); }

        }
        private void CheckBrands(int c_id)
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                SqlTransaction transaction = connection.BeginTransaction();


                int brandId = -1;
                string brandName = TxtBrands.Text.Trim();

                // Marka var mı?
                using (SqlCommand cmd = new SqlCommand("SELECT BrandsId FROM TblBrands WHERE Brands = @brand", connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@brand", brandName);
                    object result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        // Marka zaten var
                        brandId = Convert.ToInt32(result);
                    }
                    else
                    {
                        // Marka yoksa ekle
                        SqlCommand insertBrand = new SqlCommand("INSERT INTO TblBrands (Brands) OUTPUT INSERTED.BrandId VALUES (@brand)", connection, transaction);
                        insertBrand.Parameters.AddWithValue("@brand", brandName);
                        brandId = (int)insertBrand.ExecuteScalar();
                    }
                }


                SqlCommand checkRelation = new SqlCommand("SELECT COUNT(*) FROM TblCategoryBrands WHERE Brands = @brandId AND Category = @categoryId", connection, transaction);
                checkRelation.Parameters.AddWithValue("@brandId", brandId);
                checkRelation.Parameters.AddWithValue("@categoryId", c_id);

                int relationCount = (int)checkRelation.ExecuteScalar();

                if (relationCount == 0)
                {
                    // İlişki yoksa ekle
                    SqlCommand insertRelation = new SqlCommand("INSERT INTO TblCategoryBrands (Brands, Category) VALUES (@brandId, @categoryId)", connection, transaction);
                    insertRelation.Parameters.AddWithValue("@brandId", brandId);
                    insertRelation.Parameters.AddWithValue("@categoryId", c_id);
                    insertRelation.ExecuteNonQuery();
                    MessageBox.Show("Marka ve kategori ilişkisi başarıyla eklendi.");
                }
                else
                {
                    MessageBox.Show("Bu marka zaten bu kategoriye eklenmiş!");
                }


                transaction.Commit();
            }
            catch (Exception ex)
            {

                if (connection.State == ConnectionState.Open)
                {
                    try
                    {
                        connection.BeginTransaction().Rollback();
                    }
                    catch
                    {
                        // Rollback sırasında başka bir hata olursa sessizce geç
                    }
                }

                MessageBox.Show("Bir hata oluştu: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }
            //try
            //{
            //    status = true;
            //    string Brands = TxtBrands.Text.Trim();

            //    if (connection.State != ConnectionState.Open)
            //    {
            //        connection.Open();
            //    }
            //    using (SqlCommand cmd = new SqlCommand("SELECT Brands FROM TblCategoryBrands WHERE Category = @cat", connection))
            //    {
            //        cmd.Parameters.AddWithValue("@cat", c_id);
            //        SqlDataReader reader = cmd.ExecuteReader();

            //        while (reader.Read())
            //        {
            //            if (reader["Brands"].ToString() == Brands)
            //            {
            //                status = false;
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("Marka kontrol edilirken bi hatayla karşılaşıldı! Hata Detayı:\n" + ex, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
            //finally
            //{
            //    connection.Close();
            //}

        }

        private int GiveBrandsId(string brands)
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                string brndquery = "Select BrandsId FROM TblBrands WHERE Brands = @brndName";
                using (SqlCommand cmd = new SqlCommand(brndquery, connection))
                {
                    cmd.Parameters.AddWithValue("@brndName", brands);
                    object BrndID = cmd.ExecuteScalar();
                    return (int)BrndID;
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show("Marka id'si alınırken bi hatayla karşılaşıldı! Hata Detayı:: \n" + ex, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { connection.Close(); }
            return 0;
        }
        private void Btn_add_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(TxtBrands.Text))
            {
                try
                {

                    if (connection.State != ConnectionState.Open)
                        connection.Open();

                    SqlTransaction trans = connection.BeginTransaction();

                    DataRowView selectedRow = (DataRowView)CmbCategory.SelectedItem;
                    int category_id = Convert.ToInt32(selectedRow["CategoryID"]); 

                    string brands = TxtBrands.Text.Trim();

                    string query1 = "Select BrandsID FROM TblBrands WHERE Brands = @brn";
                    using (SqlCommand CmdBrandsID = new SqlCommand(query1, connection, trans))
                    {
                        CmdBrandsID.Parameters.AddWithValue("@brn", brands);
                        object BrndId = CmdBrandsID.ExecuteScalar();

                        if (BrndId != null)
                        {
                            // brndid geldi
                            string query2 = "SELECT * FROM TblCategoryBrands WHERE Brands = @brn AND Category = @cat";
                            using (SqlCommand CmdRelationCheck = new SqlCommand(query2, connection, trans))
                            {
                                CmdRelationCheck.Parameters.AddWithValue("@brn", BrndId);
                                CmdRelationCheck.Parameters.AddWithValue("@cat", category_id);

                                object relation = CmdRelationCheck.ExecuteScalar();

                                if (relation == null)
                                {
                                    string query3 = "INSERT INTO TblCategoryBrands (Brands, Category) VALUES (@brn2, @cat2)";
                                    using (SqlCommand AddRelation = new SqlCommand(query3, connection, trans))
                                    {
                                        AddRelation.Parameters.AddWithValue("@brn2", BrndId);
                                        AddRelation.Parameters.AddWithValue("@cat2", category_id);

                                        AddRelation.ExecuteNonQuery();
                                        MessageBox.Show("Marka eklendi!", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Böyle bi kayıt halihazırda zaten var!", "Warning Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }

                        }
                        else
                        {

                            // Marka yoksa ekle
                            string query4 = "INSERT INTO TblBrands (Brands) OUTPUT INSERTED.BrandsId VALUES (@brand)";
                            SqlCommand insertBrand = new SqlCommand(query4, connection, trans);
                            insertBrand.Parameters.AddWithValue("@brand", brands);

                            int BrnNewId = (int)insertBrand.ExecuteScalar();

                            string query5 = "INSERT INTO TblCategoryBrands (Brands, Category) VALUES (@brn2, @cat2)";
                            using (SqlCommand AddRelation = new SqlCommand(query5, connection, trans))
                            {
                                AddRelation.Parameters.AddWithValue("@brn2", BrnNewId);
                                AddRelation.Parameters.AddWithValue("@cat2", category_id);

                                AddRelation.ExecuteNonQuery();
                                MessageBox.Show("Marka eklendi!", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }

                        trans.Commit();
                    }
                }
                catch (Exception ex)
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        try
                        {
                            connection.BeginTransaction().Rollback();
                            MessageBox.Show("Bi hatayla karşılaşıldı! \n Hata detayı: \n" + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        catch
                        {
                            // Rollback sırasında başka bir hata olursa sessizce geç
                        }
                    }
                }
                finally
                {
                    connection.Close();
                        BringBrands();
                }
            }
            else
            {
                MessageBox.Show("Marka alanı boş geçilemez!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FrmMarka_Load(object sender, EventArgs e)
        {
            BringCategory(); BringBrands();
        }
        object brndID;
        private void gridBrandsList_DoubleClick(object sender, EventArgs e)
        {
            CmbUpdateCategory.Text = gridBrandsList.CurrentRow.Cells["Category"].Value.ToString();
            TxtUpdateBrands.Text = gridBrandsList.CurrentRow.Cells["Brands"].Value.ToString();

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                string qry = "Select BrandsID FROM TblBrands WHERE Brands = @brnd";
                using (SqlCommand cmdTakeBrndId = new SqlCommand(qry, connection))
                {
                    cmdTakeBrndId.Parameters.AddWithValue("@brnd", TxtUpdateBrands.Text.Trim());
                    cmdTakeBrndId.ExecuteNonQuery();
                    brndID = cmdTakeBrndId.ExecuteScalar();
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        private void BtnUpdateBrands_Click(object sender, EventArgs e)
        {
            try
            {
                if (brndID != null && !string.IsNullOrEmpty(TxtUpdateBrands.Text))
                {
                    int brandID = (int)brndID;

                    string query = "UPDATE TblBrands SET Brands = @brnd WHERE BrandsId = @brndID";
                    using (SqlCommand cmdBrandsNameUpdt = new SqlCommand(query, connection))
                    {
                        cmdBrandsNameUpdt.Parameters.AddWithValue("@brnd", TxtUpdateBrands.Text.Trim());
                        cmdBrandsNameUpdt.Parameters.AddWithValue("@brndID", brandID);

                        cmdBrandsNameUpdt.ExecuteNonQuery();
                        MessageBox.Show("Marka ismi güncellendi!", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Güncellemek istediğiniz markayı lütfen liste kutusundan seçiniz!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Marka güncellenirken bir hatayla karşılaşıldı! Hata Detayı: \n" + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                connection.Close();
                BringBrands();
                CmbUpdateCategory.Text = "";
                TxtUpdateBrands.Text = "";
            }
        }


        #region TxtFormatControl

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
