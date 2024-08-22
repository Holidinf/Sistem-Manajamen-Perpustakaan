using Mission5Lib.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Mission5
{
    public partial class frmLogin : Form
    {
        public frmLogin()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string empNo = txtEmpNo.Text;
            string password = txtPassword.Text;

            using (var db = new DataContext())
            {
                // Cari pengguna di database berdasarkan LoginId dan Password
                var librarian = db.Librarians
                    .FirstOrDefault(l => l.LoginId == empNo && l.Password == password);

                if (librarian != null)
                {
                    // Jika ditemukan, sembunyikan form login dan buka form utama
                    this.Hide();
                    frmMain mainForm = new frmMain();
                    mainForm.Show();
                }
                else
                {
                    // Jika tidak ditemukan, tampilkan pesan error
                    MessageBox.Show("ID / Password salah.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}



