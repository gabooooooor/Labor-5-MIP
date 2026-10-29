using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace WindowsFormsApp6
{
    public class MainForm : Form
    {
        private DataGridView dgv;
        private MenuStrip menu;
        private Panel panelSearch;
        private Panel panelBottom;
        private TextBox tbSearch;
        private Button btnSearch;
        private Button btnRefresh;
        private Label lblStatus;

        private ToolStripMenuItem miAdd;
        private ToolStripMenuItem miAddNew;
        private ToolStripMenuItem miAddQuantity;
        private ToolStripMenuItem miSell;
        private ToolStripMenuItem miSearch;

        // date folosite intre thread-uri
        private string filter = "";
        private List<Product> loadedProducts;
        private string errorMessage;

        public MainForm()
        {
            this.Text = "Administrare magazin";
            this.Size = new Size(950, 550);
            this.StartPosition = FormStartPosition.CenterScreen;

            // DataGridView
            dgv = new DataGridView();
            dgv.Dock = DockStyle.Fill;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.CellDoubleClick += new DataGridViewCellEventHandler(dgv_CellDoubleClick);

            // panou jos cu butonul Refresh
            panelBottom = new Panel();
            panelBottom.Dock = DockStyle.Bottom;
            panelBottom.Height = 40;

            btnRefresh = new Button();
            btnRefresh.Text = "Refresh";
            btnRefresh.Left = 10;
            btnRefresh.Top = 8;
            btnRefresh.Width = 90;
            btnRefresh.Click += new EventHandler(btnRefresh_Click);

            lblStatus = new Label();
            lblStatus.Left = 120;
            lblStatus.Top = 13;
            lblStatus.Width = 400;

            panelBottom.Controls.Add(btnRefresh);
            panelBottom.Controls.Add(lblStatus);

            // panou cautare (ascuns la inceput)
            panelSearch = new Panel();
            panelSearch.Dock = DockStyle.Top;
            panelSearch.Height = 40;
            panelSearch.Visible = false;

            Label lblDenumire = new Label();
            lblDenumire.Text = "Denumire:";
            lblDenumire.Left = 10;
            lblDenumire.Top = 13;
            lblDenumire.Width = 65;

            tbSearch = new TextBox();
            tbSearch.Left = 80;
            tbSearch.Top = 10;
            tbSearch.Width = 250;

            btnSearch = new Button();
            btnSearch.Text = "Caută";
            btnSearch.Left = 340;
            btnSearch.Top = 8;
            btnSearch.Width = 80;
            btnSearch.Click += new EventHandler(btnSearch_Click);

            panelSearch.Controls.Add(lblDenumire);
            panelSearch.Controls.Add(tbSearch);
            panelSearch.Controls.Add(btnSearch);

            // meniu
            menu = new MenuStrip();

            miAdd = new ToolStripMenuItem("Adăugare produse");
            miAddNew = new ToolStripMenuItem("Produs nou");
            miAddQuantity = new ToolStripMenuItem("Cantitate la produs existent");
            miAddNew.Click += new EventHandler(miAddNew_Click);
            miAddQuantity.Click += new EventHandler(miAddQuantity_Click);
            miAdd.DropDownItems.Add(miAddNew);
            miAdd.DropDownItems.Add(miAddQuantity);

            miSell = new ToolStripMenuItem("Vânzare produse");
            miSell.Click += new EventHandler(miSell_Click);

            miSearch = new ToolStripMenuItem("Căutare produse");
            miSearch.Click += new EventHandler(miSearch_Click);

            menu.Items.Add(miAdd);
            menu.Items.Add(miSell);
            menu.Items.Add(miSearch);
            this.MainMenuStrip = menu;

            // ordinea conteaza: Fill primul, meniul ultimul
            this.Controls.Add(dgv);
            this.Controls.Add(panelBottom);
            this.Controls.Add(panelSearch);
            this.Controls.Add(menu);

            this.Load += new EventHandler(MainForm_Load);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadProducts();
        }

        // ---------- incarcare produse pe Thread Pool ----------

        private void LoadProducts()
        {
            if (panelSearch.Visible)
                filter = tbSearch.Text.Trim();
            else
                filter = "";

            lblStatus.Text = "Se încarcă...";
            btnRefresh.Enabled = false;

            ThreadPool.QueueUserWorkItem(new WaitCallback(LoadWorker));
        }

        // ruleaza pe thread-ul din Thread Pool
        private void LoadWorker(object state)
        {
            errorMessage = null;
            try
            {
                using (StoreDbContext db = new StoreDbContext())
                {
                    if (filter.Length > 0)
                    {
                        loadedProducts = db.Products
                            .Where(p => p.Denumire.Contains(filter))
                            .OrderBy(p => p.Id)
                            .ToList();
                    }
                    else
                    {
                        loadedProducts = db.Products
                            .OrderBy(p => p.Id)
                            .ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ErrorHelper.GetMessage(ex);
            }

            // notificam thread-ul GUI
            if (errorMessage == null)
                this.Invoke(new MethodInvoker(ShowProducts));
            else
                this.Invoke(new MethodInvoker(ShowLoadError));
        }

        // ruleaza pe thread-ul GUI
        private void ShowProducts()
        {
            dgv.DataSource = loadedProducts;
            FormatGrid();
            lblStatus.Text = loadedProducts.Count + " produse";
            btnRefresh.Enabled = true;
        }

        private void ShowLoadError()
        {
            lblStatus.Text = "Eroare la încărcare";
            btnRefresh.Enabled = true;
            MessageBox.Show("Eroare: " + errorMessage);
        }

        private void FormatGrid()
        {
            if (dgv.Columns.Count == 0) return;
            dgv.Columns["Id"].Width = 50;
            dgv.Columns["Descriere"].Width = 300;
            dgv.Columns["DataIntrarii"].HeaderText = "Data intrării";
            dgv.Columns["DataIntrarii"].DefaultCellStyle.Format = "dd.MM.yyyy";
            dgv.Columns["DataExpirarii"].HeaderText = "Data expirării";
            dgv.Columns["DataExpirarii"].DefaultCellStyle.Format = "dd.MM.yyyy";
        }

        // ---------- evenimente ----------

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadProducts();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadProducts();
        }

        private void miSearch_Click(object sender, EventArgs e)
        {
            panelSearch.Visible = !panelSearch.Visible;
            if (!panelSearch.Visible)
                tbSearch.Clear();
            LoadProducts();
        }

        private void miAddNew_Click(object sender, EventArgs e)
        {
            AddProductForm f = new AddProductForm();
            if (f.ShowDialog() == DialogResult.OK)
                LoadProducts();
        }

        private void miAddQuantity_Click(object sender, EventArgs e)
        {
            Product p = GetSelectedProduct();
            if (p == null) return;

            AddQuantityForm f = new AddQuantityForm(p.Id, p.Denumire);
            if (f.ShowDialog() == DialogResult.OK)
                LoadProducts();
        }

        private void miSell_Click(object sender, EventArgs e)
        {
            OpenSellForm();
        }

        private void dgv_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
                OpenSellForm();
        }

        private void OpenSellForm()
        {
            Product p = GetSelectedProduct();
            if (p == null) return;

            SellForm f = new SellForm(p);
            if (f.ShowDialog() == DialogResult.OK)
                LoadProducts();
        }

        private Product GetSelectedProduct()
        {
            if (dgv.CurrentRow == null)
            {
                MessageBox.Show("Selectați un produs din tabel.");
                return null;
            }
            Product p = dgv.CurrentRow.DataBoundItem as Product;
            if (p == null)
                MessageBox.Show("Selectați un produs din tabel.");
            return p;
        }
    }
}