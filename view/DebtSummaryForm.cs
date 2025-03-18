using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace tsenacsharp.Views
{
    public partial class DebtSummaryForm : Form
    {
        private Dictionary<string, double> _dettes;

        public DebtSummaryForm(Dictionary<string, double> dettes)
        {
            _dettes = dettes;
            InitializeComponent();
            LoadDebtSummary();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Configuration de la fenêtre
            this.Text = "Résumé des dettes";
            this.Size = new Size(400, 300);
            this.BackColor = ColorTranslator.FromHtml("#44474D");
            this.Padding = new Padding(20);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Ajouter un titre
            Label titleLabel = new Label
            {
                Text = "Dettes des locataires",
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 20)
            };
            this.Controls.Add(titleLabel);

            this.ResumeLayout(false);
        }

        private void LoadDebtSummary()
        {
            FlowLayoutPanel debtListPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(0, 35, 0, 20)
            };

            foreach (var kvp in _dettes)
            {
                var debtLabel = new Label
                {
                    Text = $"{kvp.Key}: {kvp.Value}",
                    ForeColor = Color.White,
                    AutoSize = true,
                    Margin = new Padding(0, 5, 0, 0) // Marge entre les éléments
                };
                debtListPanel.Controls.Add(debtLabel);
            }

            this.Controls.Add(debtListPanel);
        }
    }
}