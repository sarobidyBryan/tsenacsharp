using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using tsenacsharp.Models;
using tsenacsharp.Services;

namespace tsenacsharp.Views
{
    public class PaiementView : UserControl
    {
        private int _idTenant;
        private Tenant _tenant;
        private OleDbConnection _conn;

        private ComboBox _boxDropdown;
        private ComboBox _moisDropdown;
        private ComboBox _anneeDropdown;
        private TextBox _amountEntry;
        private TextBox _dateEntry;

        private Dictionary<string, int> _boxDict = new Dictionary<string, int>();
        private Dictionary<string, int> _months = new Dictionary<string, int>
        {
            { "Janvier", 1 }, { "Février", 2 }, { "Mars", 3 }, { "Avril", 4 },
            { "Mai", 5 }, { "Juin", 6 }, { "Juillet", 7 }, { "Août", 8 },
            { "Septembre", 9 }, { "Octobre", 10 }, { "Novembre", 11 }, { "Décembre", 12 }
        };

        public PaiementView(int idTenant, OleDbConnection conn)
        {
            _idTenant = idTenant;
            _conn = conn;
            _tenant = Tenant.GetTenantById(conn, idTenant);

            InitializeComponent();
            LoadForm();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // PaiementView
            // 
            AutoScroll = true;
            BackColor = Color.FromArgb(42, 45, 50);
            Name = "PaiementView";
            Size = new Size(1078, 508);
            ResumeLayout(false);
        }

        private void LoadForm()
        {
            Panel formPanel = new Panel
            {
                BackColor = ColorTranslator.FromHtml("#33363D"),
                Size = new Size(400, 500),
                Padding = new Padding(20),
                AutoSize = false
            };

            this.Controls.Add(formPanel);
            this.Resize += (s, e) => CenterForm(formPanel);
            CenterForm(formPanel);

            // Ajouter le label "Payer box pour _tenant.Nom"
            Label titleLabel = new Label
            {
                Text = $"Payer box pour {_tenant.Nom}", // Utilisez le nom du locataire
                ForeColor = Color.White,
                Font = new Font("Arial", 12, FontStyle.Bold), // Personnalisez la police
                AutoSize = true,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 0, 20) // Ajoutez une marge en bas
            };
            formPanel.Controls.Add(titleLabel);

            Button submitButton = new Button
            {
                Text = "Valider Paiement",
                BackColor = ColorTranslator.FromHtml("#005A9E"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Width = 300,
                Dock = DockStyle.Top
            };
            submitButton.Click += (sender, e) => EnregistrerPaiement();
            formPanel.Controls.Add(submitButton);

            AddLabeledTextBox(formPanel, "Date de paiement:", out _dateEntry, "AAAA-MM-JJ");
            AddLabeledTextBox(formPanel, "Montant:", out _amountEntry, "Montant");
            AddLabeledDropdown(formPanel, "Sélectionner une Année:", out _anneeDropdown, Enumerable.Range(2021, 10).Select(y => y.ToString()).ToArray());
            AddLabeledDropdown(formPanel, "Sélectionner un Mois:", out _moisDropdown, _months.Keys.ToArray());

            // Remplir le ComboBox des boxes
            List<Box> boxes = _tenant.GetBoxes(_conn);
            foreach (var box in boxes)
            {
                _boxDict.Add($"{box.Reference}_{box.Id}", box.Id);
            }

            // Ajouter les boxes au ComboBox seulement si _boxDict contient des éléments
            if (_boxDict.Count > 0)
            {
                AddLabeledDropdown(formPanel, "Sélectionner un Box:", out _boxDropdown, _boxDict.Keys.ToArray());
            }
            else
            {
                MessageBox.Show("Aucun box disponible pour ce locataire.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void CenterForm(Control formPanel)
        {
            formPanel.Location = new Point((this.ClientSize.Width - formPanel.Width) / 2, (this.ClientSize.Height - formPanel.Height) / 2);
        }

        private void AddLabeledDropdown(Control parent, string labelText, out ComboBox comboBox, string[] items)
        {
            comboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 300,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Top
            };

            comboBox.Items.AddRange(items);

            // Vérifiez si le ComboBox contient des éléments avant de définir SelectedIndex
            if (comboBox.Items.Count > 0)
            {
                comboBox.SelectedIndex = 0;
            }

            Label label = new Label
            {
                Text = labelText,
                ForeColor = Color.White,
                AutoSize = true,
                Dock = DockStyle.Top
            };

            parent.Controls.Add(comboBox);
            parent.Controls.Add(label);
        }

        private void AddLabeledTextBox(Control parent, string labelText, out TextBox textBox, string placeholder)
        {
            textBox = new TextBox
            {
                PlaceholderText = placeholder,
                Width = 300,
                Dock = DockStyle.Top
            };

            Label label = new Label
            {
                Text = labelText,
                ForeColor = Color.White,
                AutoSize = true,
                Dock = DockStyle.Top
            };

            parent.Controls.Add(textBox);
            parent.Controls.Add(label);
        }

        private void EnregistrerPaiement()
        {
            try
            {
                int idBox = _boxDict[_boxDropdown.SelectedItem.ToString()];
                int mois = _months[_moisDropdown.SelectedItem.ToString()];
                int annee = int.Parse(_anneeDropdown.SelectedItem.ToString());
                double montant = double.Parse(_amountEntry.Text);
                DateTime date = DateTime.Parse(_dateEntry.Text);

                PayementService.PayerLocations(idBox, _tenant.Id, montant, mois, annee, date, _conn);
                MessageBox.Show("Paiement enregistré avec succès !", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Réinitialiser les ComboBox seulement s'ils contiennent des éléments
                if (_boxDropdown.Items.Count > 0)
                {
                    _boxDropdown.SelectedIndex = 0;
                }
                if (_moisDropdown.Items.Count > 0)
                {
                    _moisDropdown.SelectedIndex = 0;
                }
                if (_anneeDropdown.Items.Count > 0)
                {
                    _anneeDropdown.SelectedIndex = 0;
                }

                _amountEntry.Clear();
                _dateEntry.Clear();
            }
            catch (FormatException)
            {
                MessageBox.Show("Veuillez entrer un montant et une date valides.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Une erreur est survenue : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
