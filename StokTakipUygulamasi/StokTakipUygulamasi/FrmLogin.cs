using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StokTakipUygulamasi
{
    public partial class FrmLogin : Form
    {
        public FrmLogin()
        {
            InitializeComponent();
        }
        SqlConnection connection = new SqlConnection(SqlConnectionCls.ConString);
        private bool CheckTextbox()
        {
            if (!string.IsNullOrEmpty(txtPassword.Text) && !string.IsNullOrEmpty(txtUsername.Text))
            {
                return true;
            }
            return false;
        }
        private void ChcPassword_CheckedChanged(object sender, EventArgs e)
        {

            txtPassword.PasswordChar = ChcPassword.Checked ? '\0' : '*';
        }

        private void bLogin_Click(object sender, EventArgs e)
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                if (CheckTextbox())
                {

                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }

                    string query = "SELECT * FROM TblTeamMembers WHERE UserName = @UserName AND Password = @Password";
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@UserName", txtUsername.Text.Trim());
                        cmd.Parameters.AddWithValue("@Password", txtPassword.Text.Trim());

                        
                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            FrmSatis mainForm = new FrmSatis();

                            while (reader.Read())
                            {
                                mainForm.SellerUserName = (string)reader["username"];
                                mainForm.SellerRole = (string)reader["role"];
                            }
                            mainForm.Show();
                            this.Hide();

                        }
                        else
                        {
                            MessageBox.Show("Kullanıcı adı veya şifre yanlış.", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }

                    }
                }
                else
                {
                    MessageBox.Show("Lütfen kullanıcı adı ve şifre giriniz.", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            catch (SqlException ex)
            {
                MessageBox.Show("Veritabanı hatası: " + ex.Message, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {

                MessageBox.Show("Bir hatayla karşılaşıldı! Hata detayı: " + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                connection.Close();
            }
        }

 
    }
}
