using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Drawing;
using System.Windows.Forms;
using tsenacsharp.Models;
using tsenacsharp.Services;

namespace tsenacsharp.Views
{
    public partial class MarketView : UserControl
    {
        private OleDbConnection _dbConnection;
        private ComboBox _monthComboBox;
        private ComboBox _yearComboBox;
        private Button _filterButton;
        private Panel _marketContainerPanel; // Nouveau panel pour contenir les MarketDisplay
        private Panel _mainContainer;

        public MarketView(OleDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
            Console.WriteLine("MarketView");

            InitializeUI();
            LoadMarketData(_dbConnection, null, null);
        }

        private void InitializeUI()
        {
            // Disposer le contrôle pour qu'il occupe tout l'espace disponible
            this.Dock = DockStyle.Fill;
            this.AutoScroll = true;

            // Créer un panel pour les filtres
            Panel filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80, // Hauteur du filtre
                BackColor = ColorTranslator.FromHtml("#44474D"),
                Padding = new Padding(10, 20, 10, 10) // Marge en haut de 20 pixels
            };

            // Créer et ajouter les filtres
            CreateFilterControls(filterPanel);
            this.Controls.Add(filterPanel);

            // Créer le panel principal pour les marchés
            _mainContainer = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            // Créer un Panel pour contenir les MarketDisplay
            _marketContainerPanel = new Panel
            {
                Location = new Point(0, 150), // Décaler de 150 pixels vers le bas
                AutoSize = true,
                Width = this.ClientSize.Width // Prendre toute la largeur disponible
            };

            _mainContainer.Controls.Add(_marketContainerPanel);
            this.Controls.Add(_mainContainer);

            // Redimensionner le _marketContainerPanel lorsque la fenêtre est redimensionnée
            this.Resize += (sender, e) =>
            {
                _marketContainerPanel.Width = this.ClientSize.Width;
                foreach (Control control in _marketContainerPanel.Controls)
                {
                    control.Width = _marketContainerPanel.ClientSize.Width - SystemInformation.VerticalScrollBarWidth;
                }
            };
        }

        private void CreateFilterControls(Panel filterPanel)
        {
            FlowLayoutPanel controlsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            // Mois
            Label monthLabel = new Label
            {
                Text = "Mois:",
                ForeColor = Color.White,
                AutoSize = true,
                Margin = new Padding(0, 5, 5, 0)
            };
            controlsPanel.Controls.Add(monthLabel);

            string[] months = new string[]
            {
                "Janvier", "Février", "Mars", "Avril", "Mai", "Juin",
                "Juillet", "Août", "Septembre", "Octobre", "Novembre", "Décembre"
            };

            _monthComboBox = new ComboBox
            {
                DataSource = months,
                Width = 120,
                Margin = new Padding(0, 0, 10, 0)
            };
            controlsPanel.Controls.Add(_monthComboBox);

            // Année
            Label yearLabel = new Label
            {
                Text = "Année:",
                ForeColor = Color.White,
                AutoSize = true,
                Margin = new Padding(10, 5, 5, 0)
            };
            controlsPanel.Controls.Add(yearLabel);

            var years = new string[] { "2021", "2022", "2023", "2024", "2025" };
            _yearComboBox = new ComboBox
            {
                DataSource = years,
                Width = 100,
                Margin = new Padding(0, 0, 10, 0)
            };
            _yearComboBox.SelectedItem = DateTime.Now.Year.ToString();
            controlsPanel.Controls.Add(_yearComboBox);

            // Bouton de validation
            _filterButton = new Button
            {
                Text = "Valider",
                Width = 100,
                BackColor = ColorTranslator.FromHtml("#5A5D63"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _filterButton.Click += FilterButton_Click;
            controlsPanel.Controls.Add(_filterButton);

            filterPanel.Controls.Add(controlsPanel);
        }

        private void FilterButton_Click(object sender, EventArgs e)
        {
            string selectedMonth = _monthComboBox.SelectedItem.ToString();
            string selectedYear = _yearComboBox.SelectedItem.ToString();
            int month = Array.IndexOf(new string[]
            {
                "Janvier", "Février", "Mars", "Avril", "Mai", "Juin",
                "Juillet", "Août", "Septembre", "Octobre", "Novembre", "Décembre"
            }, selectedMonth) + 1;

            int year = int.Parse(selectedYear);

            UpdateMarketDisplay(month, year);
        }

        private void LoadMarketData(OleDbConnection conn, int? mois, int? annee)
        {
            _marketContainerPanel.Controls.Clear();
            _marketContainerPanel.AutoSize = true;

            List<MarketBox> marketboxes = SituationService.GetSituation(conn);

            foreach (var marketBox in marketboxes)
            {
                Console.WriteLine(marketBox.Market.Nom);

                MarketDisplay marketDisplay = new MarketDisplay(marketBox, mois, annee, conn)
                {
                    Location = new Point(0, _marketContainerPanel.Controls.Count * 510), // Positionner chaque MarketDisplay
                    Width = _marketContainerPanel.ClientSize.Width - SystemInformation.VerticalScrollBarWidth,
                    Height = 500 // Hauteur fixe de 500 pixels
                };

                _marketContainerPanel.Controls.Add(marketDisplay);
            }

            // Ouvrir la fenêtre des dettes après l'affichage des marchés
            OpenDebtSummaryWindow(mois, annee);
        }

        private void OpenDebtSummaryWindow(int? mois, int? annee)
        {
            if (mois.HasValue && annee.HasValue)
            {
                List<Tenant> tenants = Tenant.GetTenants(_dbConnection);
                Dictionary<string, double> dettes = new Dictionary<string, double>();

                foreach (var tenant in tenants)
                {
                    try
                    {
                        double dette = PayementService.CalculerDette(tenant.Id, mois.Value, annee.Value, _dbConnection);
                        dettes.Add(tenant.Nom, dette);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Erreur lors du calcul de la dette pour {tenant.Nom} : {e.Message}");
                    }
                }

                // Ouvrir la fenêtre des dettes
                DebtSummaryForm debtSummaryForm = new DebtSummaryForm(dettes);
                debtSummaryForm.Show();
            }
        }

        private void UpdateMarketDisplay(int month, int year)
        {
            Console.WriteLine($"Mise à jour des marchés : {month}/{year}");
            LoadMarketData(_dbConnection, month, year);
        }
    }
}