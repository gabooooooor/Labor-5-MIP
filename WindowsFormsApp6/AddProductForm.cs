using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace WindowsFormsApp6
{
    public class AddProductForm : Form
    {
        private TextBox tbDenumire;
        private TextBox tbDescriere;
        private DateTimePicker dtIntrare;
        private DateTimePicker dtExpirare;
        private NumericUpDown nudCantitate;
        private Button btnSave;
        private Button btnCancel;

        private Product newProduct;
        private string errorMessage;

        public AddProductForm()
        {
            this.Text = "Produs nou";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new Size(400, 340);

            Label l1 = new Label();
            l1.Text = "Denumire:";
            l1.Left = 15; l1.Top = 20; l1.Width = 100;

            tbDenumire = new TextBox();
            tbDenumire.Left = 130; tbDenumire.Top = 17; tbDenumire.Width = 250;
            tbDenumire.MaxLength = 30;

            Label l2 = new Label();
            l2.Text = "Descriere:";
            l2.Left = 15; l2.Top = 55; l2.Width = 100;

            tbDescriere = new TextBox();
            tbDescriere.Left = 130; tbDescriere.Top = 52;
            tbDescriere.Width = 250; tbDescriere.Height = 80;
            tbDescriere.Multiline = true;
            tbDescriere.MaxLength = 500;

            Label l3 = new Label();
            l3.Text = "Data intrării:";
            l3.Left = 15; l3.Top = 150; l3.Width = 100;

            dtIntrare = new DateTimePicker();
            dtIntrare.Left = 130; dtIntrare.Top = 147; dtIntrare.Width = 250;
            dtIntrare.Format = DateTimePickerFormat.Short;
            dtIntrare.Value = DateTime.Today;

            Label l4 = new Label();
            l4.Text = "Data expirării:";
            l4.Left = 15; l4.Top = 188; l4.Width = 110;

            dtExpirare = new DateTimePicker();
            dtExpirare.Left = 130; dtExpirare.Top = 185; dtExpirare.Width = 250;
            dtExpirare.Format = DateTimePickerFormat.Short;
            dtExpirare.Value = DateTime.Today.AddDays(30);

            Label l5 = new Label();
            l5.Text = "Cantitate:";
            l5.Left = 15; l5.Top = 226; l5.Width = 100;

            nudCantitate = new NumericUpDown();
            nudCantitate.Left = 130; nudCantitate.Top = 223; nudCantitate.Width = 100;
            nudCantitate.Minimum = 1;
            nudCantitate.Maximum = 1000000;
            nudCantitate.Value = 1;

            btnSave = new Button();
            btnSave.Text = "Salvează";
            btnSave.Left = 130; btnSave.Top = 280; btnSave.Width = 100;
            btnSave.Click += new EventHandler(btnSave_Click);

            btnCancel = new Button();
            btnCancel.Text = "Anulează";
            btnCancel.Left = 245; btnCancel.Top = 280; btnCancel.Width = 100;
            btnCancel.DialogResult = DialogResult.Cancel;

            this.Controls.Add(l1);
            this.Controls.Add(tbDenumire);
            this.Controls.Add(l2);
            this.Controls.Add(tbDescriere);
            this.Controls.Add(l3);
            this.Controls.Add(dtIntrare);
            this.Controls.Add(l4);
            this.Controls.Add(dtExpirare);
            this.Controls.Add(l5);
            this.Controls.Add(nudCantitate);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
            this.CancelButton = btnCancel;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (tbDenumire.Text.Trim().Length == 0)
            {
                MessageBox.Show("Introduceți denumirea produsului.");
                return;
            }
            if (dtExpirare.Value.Date < dtIntrare.Value.Date)
            {
                MessageBox.Show("Data expirării nu poate fi înaintea datei intrării.");
                return;
            }

            newProduct = new Product();
            newProduct.Denumire = tbDenumire.Text.Trim();
            newProduct.Descriere = tbDescriere.Text.Trim();
            newProduct.DataIntrarii = dtIntrare.Value.Date;
            newProduct.DataExpirarii = dtExpirare.Value.Date;
            newProduct.Cantitate = (int)nudCantitate.Value;

            btnSave.Enabled = false;
            ThreadPool.QueueUserWorkItem(new WaitCallback(SaveWorker));
        }

        // thread din Thread Pool
        private void SaveWorker(object state)
        {
            errorMessage = null;
            try
            {
                using (StoreDbContext db = new StoreDbContext())
                {
                    db.Products.Add(newProduct);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                errorMessage = ErrorHelper.GetMessage(ex);
            }

            if (errorMessage == null)
                this.Invoke(new MethodInvoker(SaveDone));
            else
                this.Invoke(new MethodInvoker(SaveError));
        }

        // thread GUI
        private void SaveDone()
        {
            MessageBox.Show("Produs adăugat cu ID: " + newProduct.Id);
            this.DialogResult = DialogResult.OK;
        }

        private void SaveError()
        {
            btnSave.Enabled = true;
            MessageBox.Show("Eroare: " + errorMessage);
        }
    }
}