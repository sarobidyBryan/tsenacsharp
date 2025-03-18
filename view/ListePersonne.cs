using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Drawing;
using System.Windows.Forms;
using tsenacsharp.Models;

namespace tsenacsharp.Views
{
    public class ListePersonne : UserControl
    {
        private OleDbConnection _conn;
        private Action<int> _onValidate;
        private int _selectedPersonId = -1;

        public ListePersonne(OleDbConnection conn, Action<int> onValidate)
        {
            _conn = conn;
            _onValidate = onValidate;

            InitializeComponent();
            LoadPersonnes();
        }

        private void InitializeComponent()
        {
            this.AutoScroll = true;
            this.BackColor = ColorTranslator.FromHtml("#33363D");
            this.Padding = new Padding(10);
        }

        private void LoadPersonnes()
        {
            List<Tenant> personnes = Tenant.GetTenants(_conn);

            // Conteneur pour centrer les éléments
            Panel centerPanel = new Panel
            {
                Dock = DockStyle.Fill
            };

            FlowLayoutPanel flowPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(20),
                Anchor = AnchorStyles.None // Permet de rester centré
            };

            foreach (var personne in personnes)
            {
                RadioButton radioButton = new RadioButton
                {
                    Text = personne.Nom,
                    Tag = personne.Id,
                    ForeColor = Color.White,
                    AutoSize = true,
                    Margin = new Padding(5)
                };
                radioButton.CheckedChanged += (sender, e) =>
                {
                    if (radioButton.Checked)
                    {
                        _selectedPersonId = (int)radioButton.Tag;
                    }
                };

                flowPanel.Controls.Add(radioButton);
            }

            // Bouton Valider
            Button validateButton = new Button
            {
                Text = "Valider",
                BackColor = ColorTranslator.FromHtml("#5A5D63"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                AutoSize = true,
                Padding = new Padding(5)
            };
            validateButton.Click += ValidateButton_Click;

            flowPanel.Controls.Add(validateButton);

            // Ajout des éléments dans le Panel principal
            centerPanel.Controls.Add(flowPanel);
            this.Controls.Add(centerPanel);
        }



        private void ValidateButton_Click(object sender, EventArgs e)  
        {
            if (_selectedPersonId != -1)
            {
                _onValidate(_selectedPersonId); // Appeler la méthode de validation avec l'ID sélectionné
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner une personne.", "Avertissement", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}