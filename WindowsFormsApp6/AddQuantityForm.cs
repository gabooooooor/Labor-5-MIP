using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace WindowsFormsApp6
{
    public class AddQuantityForm : Form
    {
        private int productId;
        private int quantityToAdd;
        private int newTotal;
        private string errorMessage;

        private NumericUpDown nudCantitate;
        private Button btnSave;
        private Button btnCancel;

        public AddQuantityForm(int id, string denumire)
        {
            productId = id;

            this.Text = "Adăugare cantitate";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new Size(360, 170);

            Label l1 = new Label();
            l1.Text = "ID produs:";
            l1.Left = 15; l1.Top = 20; l1.Width = 100;

            Label lId = new Label();
            lId.Text = id.ToString();
            lId.Left = 130; lId.Top = 20; lId.Width = 200;

            Label l2 = new Label();
            l2.Text = "Denumire:";
            l2.Left = 15; l2.Top = 50; l2.Width = 100;

            Label lDen = new Label();
            lDen.Text = denumire;
            lDen.Left = 130; lDen.Top = 50; lDen.Width = 210;

            Label l3 = new Label();
            l3.Text = "Cantitate de adăugat:";
            l3.Left = 15; l3.Top = 82; l3.Width = 115;

            nudCantitate = new NumericUpDown();
            nudCantitate.Left = 130; nudCantitate.Top = 80; nudCantitate.Width = 100;
            nudCantitate.Minimum = 1;
            nudCantitate.Maximum = 1000000;
            nudCantitate.Value = 1;

            btnSave = new Button();
            btnSave.Text = "Adaugă";
            btnSave.Left = 70; btnSave.Top = 120; btnSave.Width = 100;
            btnSave.Click += new EventHandler(btnSave_Click);

            btnCancel = new Button();
            btnCancel.Text = "Anulează";
            btnCancel.Left = 185; btnCancel.Top = 120; btnCancel.Width = 100;
            btnCancel.DialogResult = DialogResult.Cancel;

            this.Controls.Add(l1);
            this.Controls.Add(lId);
            this.Controls.Add(l2);
            this.Controls.Add(lDen);
            this.Controls.Add(l3);
            this.Controls.Add(nudCantitate);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
            this.CancelButton = btnCancel;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            quantityToAdd = (int)nudCantitate.Value;
            btnSave.Enabled = false;
            ThreadPool.QueueUserWorkItem(new WaitCallback(UpdateWorker));
        }

        // thread din Thread Pool
        private void UpdateWorker(object state)
        {
            errorMessage = null;
            newTotal = -1;
            try
            {
                using (StoreDbContext db = new StoreDbContext())
                {
                    Product p = db.Products.Find(productId);
                    if (p != null)
                    {
                        p.Cantitate = p.Cantitate + quantityToAdd;
                        db.SaveChanges();
                        newTotal = p.Cantitate;
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ErrorHelper.GetMessage(ex);
            }

            if (errorMessage == null)
                this.Invoke(new MethodInvoker(UpdateDone));
            else
                this.Invoke(new MethodInvoker(UpdateError));
        }

        // thread GUI
        private void UpdateDone()
        {
            if (newTotal == -1)
            {
                MessageBox.Show("Produsul nu mai există în baza de date.");
                btnSave.Enabled = true;
            }
            else
            {
                MessageBox.Show("Cantitate nouă: " + newTotal);
                this.DialogResult = DialogResult.OK;
            }
        }

        private void UpdateError()
        {
            btnSave.Enabled = true;
            MessageBox.Show("Eroare: " + errorMessage);
        }
    }
}