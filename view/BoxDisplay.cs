using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Drawing;
using System.Windows.Forms;
using tsenacsharp.Models;
using tsenacsharp.Services;

namespace tsenacsharp.Views
{
    public class BoxDisplay : UserControl
    {
        private List<Box> _boxes;
        private Market _market;
        private int _displayHeight;
        private int _displayWidth;
        private int? _mois;
        private int? _annee;
        private OleDbConnection _conn;

        public BoxDisplay(List<Box> boxes, Market market, int displayHeight, int displayWidth, int? mois, int? annee, OleDbConnection conn)
        {
            _boxes = boxes;
            _market = market;
            _displayHeight = displayHeight;
            _displayWidth = displayWidth;
            _mois = mois;
            _annee = annee;
            _conn = conn;

            InitializeComponent();
            CreateBoxes();
        }

        private void InitializeComponent()
        {
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.Padding = new Padding(10); // Ajouter un padding pour l'espacement
        }

        private void CreateBoxes()
        {
            double scaleX = _displayWidth / _market.Largeur; // Échelle horizontale
            double scaleY = _displayHeight / _market.Longueur; // Échelle verticale

            foreach (var box in _boxes)
            {
                // Calcul des coordonnées et dimensions en fonction de l'échelle
                int x = (int)(box.X * scaleX)*2;
                int y = (int)(box.Y * scaleY);
                int width = (int)(box.Longueur * scaleX*3);
                int height = (int)(box.Largeur * scaleY);

                // Couleurs par défaut
                string color1 = "gray";
                string color2 = "gray";
                double redPerc = 100;
                double greenPerc = 0;
                string locataire = "";

                // Vérifier si la boîte est occupée et calculer les couleurs en fonction du paiement
                if (_mois.HasValue && _annee.HasValue && !box.EstLibre(_conn, _mois.Value, _annee.Value))
                {
                    double prix = box.GetPrix(_mois.Value, _annee.Value, _conn);
                    DateTime dateVerif = new DateTime(_annee.Value, _mois.Value, 28);
                    double valeurDejaPaye = box.GetValueAtDate(_conn, _mois.Value, _annee.Value, dateVerif);
                    valeurDejaPaye = box.GetValue(_conn, _mois.Value, _annee.Value);
                    Tenant tenant = box.GetLocataire(_conn, _mois.Value, _annee.Value);
                    locataire = tenant != null ? $"- {tenant.Nom}" : "";

                    if (prix > 0)
                    {
                        greenPerc = (valeurDejaPaye * 100) / prix;
                        redPerc = 100 - greenPerc;
                    }

                    color1 = "green";
                    color2 = "red";
                }

                // Créer un panel pour représenter la boîte
                Panel boxPanel = new Panel
                {
                    Width = width,
                    Height = height,
                    Location = new Point(x, y),
                    BackColor = Color.Transparent // Fond transparent pour voir la bordure
                };

                // Dessiner le rectangle avec les couleurs calculées
                DrawRectangle(boxPanel, width, height, color1, color2, greenPerc, redPerc);

                // Ajouter un label pour afficher la référence et le locataire
                Label label = new Label
                {
                    Text = $"{box.Reference}",
                    ForeColor = Color.White,
                    Font = new Font("Arial", 10, FontStyle.Bold),
                    AutoSize = true,
                    Location = new Point(1, 1) // Centrer le texte
                };

                boxPanel.Controls.Add(label);
                this.Controls.Add(boxPanel);
            }
        }

        private void DrawRectangle(Panel panel, int width, int height, string color1, string color2, double greenPerc, double redPerc)
        {
            int width1 = (int)((greenPerc / 100) * width);
            int width2 = (int)((redPerc / 100) * width);

            // Utiliser un Bitmap pour dessiner les rectangles
            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.FillRectangle(new SolidBrush(ColorTranslator.FromHtml(color1)), 0, 0, width1, height);
                g.FillRectangle(new SolidBrush(ColorTranslator.FromHtml(color2)), width1, 0, width2, height);
            }

            // Afficher le Bitmap dans le panel
            panel.BackgroundImage = bmp;
        }
    }
}