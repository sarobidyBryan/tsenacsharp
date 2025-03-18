using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Drawing;
using System.Windows.Forms;
using tsenacsharp.Models;
using tsenacsharp.Services;

namespace tsenacsharp.Views
{
    public class MarketDisplay : UserControl
    {
        private MarketBox _marketBox;
        private int? _mois;
        private int? _annee;
        private TableLayoutPanel mainContainer;
        private OleDbConnection _conn;

        public MarketDisplay(MarketBox marketBox, int? mois, int? annee, OleDbConnection conn)
        {
            _marketBox = marketBox;
            _mois = mois;
            _annee = annee;
            _conn = conn;

            InitializeComponent();
            LoadPanels();
        }

        private void InitializeComponent()
        {
            mainContainer = new TableLayoutPanel();
            SuspendLayout();

            mainContainer.AutoSize = true;
            mainContainer.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            mainContainer.BackColor = Color.Transparent; // Supprimer la couleur de fond
            mainContainer.ColumnCount = 2; // Deux colonnes : info (20%) et box (70%)
            mainContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            mainContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            mainContainer.Dock = DockStyle.Fill;
            mainContainer.Location = new Point(0, 0);
            mainContainer.Name = "mainContainer";
            mainContainer.RowCount = 1;
            mainContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            this.AutoSize = false; // Désactiver AutoSize pour forcer la hauteur
            this.Height = 500; // Hauteur fixe de 500
            this.Margin = new Padding(0, 0, 0, 10); // Marge en bas de 10 pixels
            this.Controls.Add(mainContainer);
            this.ResumeLayout(false);

            // Attacher l'événement Resize après que le contrôle ait été ajouté à son parent
            this.ParentChanged += (sender, e) =>
            {
                if (this.Parent != null)
                {
                    this.Resize += (sender, e) =>
                    {
                        this.Width = this.Parent.ClientSize.Width - SystemInformation.VerticalScrollBarWidth;
                    };
                }
            };
        }

        private void LoadPanels()
        {
            // Créer et ajouter les panels
            Panel infoPanel = CreateMarketInfoPanel();
            Panel boxPanel = CreateBoxPanel();

            mainContainer.Controls.Add(infoPanel, 0, 0); // Colonne 0 (20%)
            mainContainer.Controls.Add(boxPanel, 1, 0);  // Colonne 1 (70%)
        }

        private Panel CreateMarketInfoPanel()
        {
            var infoPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                AutoSize = true,
                BorderStyle = BorderStyle.FixedSingle, // Ajouter une bordure
                BackColor = ColorTranslator.FromHtml("#55585E")
            };

            // Ajouter une bordure jaune
            infoPanel.Paint += (sender, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, infoPanel.ClientRectangle, Color.Yellow, ButtonBorderStyle.Solid);
            };

            var titleLabel = new Label
            {
                Text = $"Nom: {_marketBox.Market.Nom}",
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                AutoSize = true
            };
            infoPanel.Controls.Add(titleLabel);

            var sizeLabel = new Label
            {
                Text = $"Dimensions: {_marketBox.Market.Longueur}m x {_marketBox.Market.Largeur}m",
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(0, 10, 0, 0)
            };
            infoPanel.Controls.Add(sizeLabel);

            var countLabel = new Label
            {
                Text = $"Nombre de boxes: {_marketBox.Boxes.Count}",
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(0, 10, 0, 0)
            };
            infoPanel.Controls.Add(countLabel);

            return infoPanel;
        }

        private Panel CreateBoxPanel()
        {
            var boxPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                AutoScroll = true,
                BorderStyle = BorderStyle.FixedSingle, // Ajouter une bordure
                
            };

            // Ajouter une bordure jaune
            boxPanel.Paint += (sender, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, boxPanel.ClientRectangle, Color.Yellow, ButtonBorderStyle.Solid);
            };

            BoxDisplay boxDisplay = new BoxDisplay(_marketBox.Boxes, _marketBox.Market, 500, 400, _mois, _annee, _conn)
            {
                Dock = DockStyle.Fill
            };

            boxPanel.Controls.Add(boxDisplay);
            return boxPanel;
        }
    }
}