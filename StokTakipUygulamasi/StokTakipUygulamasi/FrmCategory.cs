using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Security.Cryptography;
namespace StokTakipUygulamasi
{
    public partial class FrmCategory : Form
    {
        public FrmCategory()
        {
            InitializeComponent();
        }
        SqlConnection connection = new SqlConnection(SqlConnectionCls.ConString);
        bool status;
        private void BringCategoryList()
        {
            gridCategoryList.DataSource = null;
            gridCategoryList.Rows.Clear();

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                DataTable dtc = new DataTable();
                using (SqlDataAdapter adp = new SqlDataAdapter("SELECT * FROM TblCategory", connection))
                {
                    dtc.Clear();
                    adp.Fill(dtc);


                    foreach (DataRow row in dtc.Rows)
                    {
                        int ctID = (int)row["CategoryID"];
                        string cName = row["Category"].ToString();

                        int index = gridCategoryList.Rows.Add();
                        gridCategoryList.Rows[index].Cells[0].Value = ctID;
                        gridCategoryList.Rows[index].Cells[1].Value = cName;
                    }

                }

            }


            catch (Exception ex)
            {

                MessageBox.Show("Kategoriler listelenirken bir hatayla karşılaşıldı! Hata detayı: \n" + ex, "Erorr", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                connection.Close();
            }

        }
        private void checkcataegory()
        {
            try
            {
                status = true;
                string InputCat = TxtCategory.Text.Trim();
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                using (SqlCommand cmd = new SqlCommand("Select Category from TblCategory", connection))
                {
                    cmd.Parameters.AddWithValue("@cat", InputCat);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        if ((string)reader[0] == InputCat)
                        {
                            status = false;
                        }
                    }

                }
            }
            catch (Exception ex)
            {

                MessageBox.Show("Kategori kontrol edilirken bi hatayla karşılaşıldı! Hata detayı: \n " + ex, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { connection.Close(); }
        }
        private void Btn_add_Click(object sender, EventArgs e)
        {
            checkcataegory();
            if (status == true)
            {
                if (TxtCategory.Text != "")
                {
                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }

                    try
                    {
                        string insertbrand = "INSERT INTO TblCategory (Category) VALUES (@cat)";
                        using (SqlCommand insertbrandquery = new SqlCommand(insertbrand, connection))
                        {
                            insertbrandquery.Parameters.AddWithValue("@cat", TxtCategory.Text.Trim());
                            insertbrandquery.ExecuteNonQuery();

                            TxtCategory.Text = "";
                            BringCategoryList();
                            MessageBox.Show("Yeni Kategori Eklendi!", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {

                        MessageBox.Show("Yeni kategori eklenirken hatayla karşılaşıldı! Hata detayı: \n" + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    }
                    finally
                    {
                        connection.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Kategori alanı boş geçilemez", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Böyle bir hali hazırda mevcut!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }

        private void FrmKategori_Load(object sender, EventArgs e)
        {
            BringCategoryList();
        }
        int RowCategoryID;
        private void gridCategoryList_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            RowCategoryID = int.Parse(gridCategoryList.CurrentRow.Cells["CategoryID"].Value.ToString());
            TxtCategoryUpdate.Text = gridCategoryList.CurrentRow.Cells["Category"].Value.ToString();
        }

        private void BtnCatUpdt_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(RowCategoryID.ToString()))
            {
                try
                {

                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }
                    DialogResult result = MessageBox.Show("Kategori güncellenecek, onaylıyor musunuz?", "Confirmation Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        string UpdateQuery = "UPDATE TblCategory SET Category=@category WHERE CategoryID=@ctgID";
                        using (SqlCommand upd = new SqlCommand(UpdateQuery, connection))
                        {
                            upd.Parameters.AddWithValue("@ctgID", RowCategoryID);
                            upd.Parameters.AddWithValue("@category", TxtCategoryUpdate.Text.Trim());

                            upd.ExecuteNonQuery();
                            BringCategoryList();
                            MessageBox.Show("Kategori güncellendi!", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Kategori güncellenirken bi hatayla karşılaşıldı! Hata detayı: \n" + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally { connection.Close(); }
            }
        }


        private void silToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(gridCategoryList.CurrentRow.Cells["CategoryID"].Value);

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                DialogResult result = MessageBox.Show("Kategori silinecek, onaylıyor musunuz?", "Confirmation Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    string DeleteQuery = "DELETE FROM TblCategory WHERE CategoryID=@ctgID";
                    using (SqlCommand cmd = new SqlCommand(DeleteQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("ctgID", id);
                        cmd.ExecuteNonQuery();
                        BringCategoryList();
                        MessageBox.Show("Kategori silindi!", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

            }
            catch (Exception ex)
            {

                MessageBox.Show("Kategori silinirken bi hatayla karşılaşıldı! Hata detayı: \n" + ex, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                connection.Close();
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
