using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TechMartProductManager
{
    public partial class Form1 : Form
    {
        private BindingList<Product> productList = new BindingList<Product>();
        private BindingSource bindingSource = new BindingSource();
        private string selectedImagePath = "";

        public Form1()
        {
            InitializeComponent();
            SetupForm();
        }

        private void SetupForm()
        {
            exportCSVToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.E;
            exitToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.X;

            var categories = new List<CategoryItem>
            {
                new CategoryItem { Id = "Điện thoại", Name = "Điện thoại" },
                new CategoryItem { Id = "Laptop", Name = "Laptop" },
                new CategoryItem { Id = "Phụ kiện", Name = "Phụ kiện" }
            };

            cboCategory.DataSource = categories;
            cboCategory.DisplayMember = "Name";
            cboCategory.ValueMember = "Id";

            bindingSource.DataSource = productList;
            dgvProducts.DataSource = bindingSource;

            dgvProducts.AutoGenerateColumns = false;
            dgvProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            dgvProducts.Columns.Clear();
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductId", HeaderText = "Mã SP" });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductName", HeaderText = "Tên SP" });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Category", HeaderText = "Danh Mục" });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "UnitPrice", HeaderText = "Đơn Giá", DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" } });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Quantity", HeaderText = "Số Lượng" });

            dgvProducts.CellClick += DgvProducts_CellClick;
            txtSearch.TextChanged += TxtSearch_TextChanged;

            UpdateStatus();
        }

        private void UpdateStatus()
        {
            lblStatus.Text = $"Tổng số sản phẩm: {dgvProducts.Rows.Count}";
        }

        private bool ValidateInput()
        {
            errorProvider1.Clear();
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                errorProvider1.SetError(txtProductName, "Tên SP không được để trống");
                isValid = false;
            }

            if (!decimal.TryParse(txtUnitPrice.Text, out decimal price) || price <= 0)
            {
                errorProvider1.SetError(txtUnitPrice, "Đơn giá phải > 0");
                isValid = false;
            }

            if (!int.TryParse(txtQuantity.Text, out int qty) || qty < 0)
            {
                errorProvider1.SetError(txtQuantity, "Số lượng phải >= 0");
                isValid = false;
            }

            return isValid;
        }

        private void ClearInput()
        {
            txtProductId.Clear();
            txtProductName.Clear();
            txtUnitPrice.Clear();
            txtQuantity.Clear();
            if (cboCategory.Items.Count > 0) cboCategory.SelectedIndex = 0;
            picAvatar.Image = null;
            selectedImagePath = "";
            txtProductId.Focus();
        }

        private void btnChooseImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    selectedImagePath = dialog.FileName;
                    picAvatar.Image = Image.FromFile(selectedImagePath);
                    picAvatar.SizeMode = PictureBoxSizeMode.Zoom;
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            string id = txtProductId.Text.Trim();
            if (productList.Any(p => p.ProductId.Equals(id, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Mã sản phẩm đã tồn tại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Product product = new Product
            {
                ProductId = id,
                ProductName = txtProductName.Text.Trim(),
                Category = cboCategory.SelectedValue?.ToString() ?? "",
                UnitPrice = decimal.Parse(txtUnitPrice.Text),
                Quantity = int.Parse(txtQuantity.Text),
                ImagePath = selectedImagePath
            };

            productList.Add(product);
            UpdateStatus();
            ClearInput();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null) return;
            if (!ValidateInput()) return;

            string currentId = txtProductId.Text.Trim();
            Product product = productList.FirstOrDefault(p => p.ProductId.Equals(currentId, StringComparison.OrdinalIgnoreCase));

            if (product != null)
            {
                product.ProductName = txtProductName.Text.Trim();
                product.Category = cboCategory.SelectedValue?.ToString() ?? "";
                product.UnitPrice = decimal.Parse(txtUnitPrice.Text);
                product.Quantity = int.Parse(txtQuantity.Text);
                if (!string.IsNullOrEmpty(selectedImagePath))
                {
                    product.ImagePath = selectedImagePath;
                }

                bindingSource.ResetBindings(false);
                ClearInput();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null) return;

            string id = dgvProducts.CurrentRow.Cells[0].Value.ToString();
            Product product = productList.FirstOrDefault(p => p.ProductId.Equals(id, StringComparison.OrdinalIgnoreCase));

            if (product != null)
            {
                if (MessageBox.Show("Bạn có chắc chắn muốn xóa sản phẩm này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    productList.Remove(product);
                    UpdateStatus();
                    ClearInput();
                }
            }
        }

        private void DgvProducts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvProducts.Rows.Count)
            {
                DataGridViewRow row = dgvProducts.Rows[e.RowIndex];
                Product product = row.DataBoundItem as Product;

                if (product != null)
                {
                    txtProductId.Text = product.ProductId;
                    txtProductName.Text = product.ProductName;
                    cboCategory.SelectedValue = product.Category;
                    txtUnitPrice.Text = product.UnitPrice.ToString();
                    txtQuantity.Text = product.Quantity.ToString();
                    selectedImagePath = product.ImagePath;

                    if (!string.IsNullOrEmpty(product.ImagePath) && File.Exists(product.ImagePath))
                    {
                        picAvatar.Image = Image.FromFile(product.ImagePath);
                        picAvatar.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                    else
                    {
                        picAvatar.Image = null;
                    }
                }
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(keyword))
            {
                bindingSource.DataSource = productList;
            }
            else
            {
                var filtered = productList.Where(p => p.ProductName.ToLower().Contains(keyword)).ToList();
                bindingSource.DataSource = new BindingList<Product>(filtered);
            }
            UpdateStatus();
        }

        private void exportCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV File|*.csv";
                dialog.FileName = "Products.csv";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Mã SP,Tên SP,Danh Mục,Đơn Giá,Số Lượng");

                    foreach (DataGridViewRow row in dgvProducts.Rows)
                    {
                        Product item = row.DataBoundItem as Product;
                        if (item != null)
                        {
                            sb.AppendLine($"\"{item.ProductId}\",\"{item.ProductName}\",\"{item.Category}\",{item.UnitPrice},{item.Quantity}");
                        }
                    }

                    File.WriteAllText(dialog.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Xuất file CSV thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }

    public class Product
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string Category { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string ImagePath { get; set; }
    }

    public class CategoryItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}