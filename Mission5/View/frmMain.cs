using Mission5Lib.Model;
using System;
using System.Windows.Forms;

namespace Mission5
{
    public partial class frmMain : Form
    {
        public Librarian Librarian { get; set; }
        private Member searchedMember;
        private BookCopy searchedBookCopy;

        public frmMain()
        {
            InitializeComponent();
        }

        private void btnSearchMember_Click(object sender, EventArgs e)
        {
            RefreshMemberInfo();
        }

        private void RefreshMemberInfo()
        {
            var member = MemberDAO.GetInstance().Get(txtMemberId.Text);

            if (member == null)
            {
                ClearMemberFields();
                member = null;

                MessageBox.Show("Tidak ada anggota yang cocok dengan ID yang dimasukkan.", "Kesalahan dalam pencarian anggota.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                return;
            }

            searchedMember = member;

            var memberCheckOutDAO = MemberCheckOutDAO.GetInstance();
            var checkoutInfo = memberCheckOutDAO.GetCheckOutSummary(member.Id);

            lblMemberName.Text = member.Name;
            lblPhoneNo.Text = member.PhoneNo;
            lblDaysOfOverdue.Text = checkoutInfo.DaysOfOverdue.ToString();
            lblNumOfAvailable.Text = checkoutInfo.NumOfBookAvailable.ToString();
            lblNumOfAvailable.Text = checkoutInfo.NumOfBookCheckOut.ToString();
            lblOverdueFee.Text = checkoutInfo.OverdueFee.ToString();
            lblNumOfOverdue.Text = checkoutInfo.NumOfBookOverdue.ToString();

            var bookCopyList = memberCheckOutDAO.GetCheckOutBookInfoList(member.Id);
            dgvBookList.AutoGenerateColumns = false;
            dgvBookList.DataSource = bookCopyList;

            btnReturn.Enabled = false;

            if (IsMemberCheckOutAvailable())
            {
                if (searchedMember != null)
                    btnCheckOut.Enabled = true;
            }

            if (bookCopyList.Count == 0)
                btnReturn.Enabled = false;
            else
                btnReturn.Enabled = true;
        }

        private bool IsMemberCheckOutAvailable()
        {
            if (searchedMember == null)
                return false;

            var memberCheckOutDAO = MemberCheckOutDAO.GetInstance();
            var checkoutInfo = memberCheckOutDAO.GetCheckOutSummary(searchedMember.Id);

            // Check if the member has any overdue books
            if (checkoutInfo.NumOfBookOverdue > 1)
                return false;

            // Check if the member has reached the borrowing limit
            if (checkoutInfo.NumOfBookAvailable <= 0)
                return false;

            return true;
        }

        private void ClearMemberFields()
        {
            lblMemberName.Text = "";
            lblPhoneNo.Text = "";
            lblDaysOfOverdue.Text = "";
            lblNumOfAvailable.Text = "";
            lblNumOfAvailable.Text = "";
            lblOverdueFee.Text = "";
            lblNumOfOverdue.Text = "";

            dgvBookList.DataSource = null;

            btnCheckOut.Enabled = false;
            btnReturn.Enabled = false;
        }

        private void btnSearchBook_Click(object sender, EventArgs e)
        {
            var bookCopy = BookCopyDAO.GetInstance().Get(txtBookCopyCode.Text);

            if (bookCopy == null)
            {
                ClearBookCopyInfo();
                MessageBox.Show("Tidak ada buku yang cocok dengan nomor registrasi buku yang dimasukkan.", "Kesalahan dalam pencarian buku", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            var book = BookDAO.GetInstance().Get(bookCopy.BookId);
            lblTitle.Text = book.Title;
            lblPublisher.Text = book.Publisher;

            searchedBookCopy = bookCopy;

            btnCheckOut.Enabled = false;

            if (bookCopy.BookStatus == (int)BookCopyStatusEnum.CheckedOut)
            {
                MessageBox.Show("Buku sedang dipinjam sehingga tidak dapat dipinjam lagi.", "Kesalahan dalam peminjaman buku.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            else if (bookCopy.BookStatus == (int)BookCopyStatusEnum.Lost)
            {
                MessageBox.Show("Buku ini telah hilang dan tidak dapat dipinjam.", "Kesalahan dalam peminjaman buku.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (searchedMember != null && searchedMember != null)
                btnCheckOut.Enabled = true;
            else
                btnCheckOut.Enabled = false;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            ClearMemberFields();
        }

        private void btnCheckOut_Click(object sender, EventArgs e)
        {
            if (searchedMember != null && searchedBookCopy != null)
            {
                var checkOut = new CheckOut
                {
                    MemberId = searchedMember.Id,
                    BookCopyId = searchedBookCopy.Id,
                    CheckOutDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(14) // Example: 2 weeks due date
                };

                var checkOutDAO = CheckOutDAO.GetInstance();
                checkOutDAO.Add(checkOut);

                searchedBookCopy.BookStatus = (int)BookCopyStatusEnum.CheckedOut;
                BookCopyDAO.GetInstance().Update(searchedBookCopy);

                ClearBookCopyInfo();
                RefreshMemberInfo();
            }
        }

        private void ClearBookCopyInfo()
        {
            txtBookCopyCode.Text = "";
            lblTitle.Text = "";
            lblPublisher.Text = "";
            searchedBookCopy = null;
            btnCheckOut.Enabled = false;
        }

        private void btnReturn_Click(object sender, EventArgs e)
        {
            if (dgvBookList.CurrentRow == null)
            {
                return;
            }

            int checkOutId = Convert.ToInt32(dgvBookList.CurrentRow.Cells[0].Value);
            int overdueDays = Convert.ToInt32(dgvBookList.CurrentRow.Cells[5].Value);
            int overdueFee = Convert.ToInt32(dgvBookList.CurrentRow.Cells[6].Value);

            var checkOutDAO = CheckOutDAO.GetInstance();
            var checkOut = checkOutDAO.Get(checkOutId);

            if (checkOut != null)
            {
                checkOut.ReturnDate = DateTime.Now;
                checkOut.OverdueCharge = overdueFee;
                checkOut.OverdueDays = overdueDays;

                checkOutDAO.Update(checkOut);

                var bookCopy = BookCopyDAO.GetInstance().Get(checkOut.BookCopyId);
                bookCopy.BookStatus = (int)BookCopyStatusEnum.Available;

                BookCopyDAO.GetInstance().Update(bookCopy);

                RefreshMemberInfo();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dgvBookList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btnCari_Click(object sender, EventArgs e)
        {
            var member = MemberDAO.GetInstance().Get(txtMemberId.Text);

            if (member == null)
            {
                ClearMemberFields();
                MessageBox.Show("Tidak ada anggota yang cocok dengan ID yang dimasukkan.", "Kesalahan dalam pencarian anggota.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            searchedMember = member;

            var memberCheckOutDAO = MemberCheckOutDAO.GetInstance();
            var checkoutInfo = memberCheckOutDAO.GetCheckOutSummary(member.Id);

            lblMemberName.Text = member.Name;
            lblPhoneNo.Text = member.PhoneNo;
            lblDaysOfOverdue.Text = checkoutInfo.DaysOfOverdue.ToString();
            lblNumOfAvailable.Text = checkoutInfo.NumOfBookAvailable.ToString();
            lblNumOfAvailable.Text = checkoutInfo.NumOfBookCheckOut.ToString();
            lblOverdueFee.Text = checkoutInfo.OverdueFee.ToString();
            lblNumOfOverdue.Text = checkoutInfo.NumOfBookOverdue.ToString();

            var bookCopyList = memberCheckOutDAO.GetCheckOutBookInfoList(member.Id);
            dgvBookList.AutoGenerateColumns = false;
            dgvBookList.DataSource = bookCopyList;

            btnReturn.Enabled = bookCopyList.Count > 0;

            if (IsMemberCheckOutAvailable())
            {
                btnCheckOut.Enabled = true;
            }
            else
            {
                btnCheckOut.Enabled = false;
            }
        }
    }
}
