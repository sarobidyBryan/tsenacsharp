using System;
using System.Data.OleDb;
using System.Windows.Forms;
using tsenacsharp.DB;
using tsenacsharp.Views;

namespace tsenacsharp
{
    public partial class MainForm : Form
    {
        private OleDbConnection _dbConnection;
        private Panel _mainFrame;

        public MainForm(OleDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
            this.FormClosing += MainForm_FormClosing;
            InitializeComponent();
            InitializeLayout();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_dbConnection != null && _dbConnection.State == System.Data.ConnectionState.Open)
            {
                _dbConnection.Close();
                _dbConnection.Dispose();
                _dbConnection = null;
                Console.WriteLine("Connexion fermée proprement.");
            }
        }

        private void InitializeLayout()
        {
            this.Width = 1000;
            this.Height = 700;

            // Création du menu en haut à droite
            var menuStrip = new MenuStrip { Dock = DockStyle.Top, RightToLeft = RightToLeft.Yes };
            var menu = new ToolStripMenuItem("Menu");
            var payementItem = new ToolStripMenuItem("Payer", null, OnPayerClicked);
            var listePayementsItem = new ToolStripMenuItem("Liste Payements", null, OnListePayementsClicked);

            menu.DropDownItems.Add(payementItem);
            menu.DropDownItems.Add(listePayementsItem);
            menuStrip.Items.Add(menu);
            this.Controls.Add(menuStrip);

            // Création du panneau principal contenant les vues (scrollable)
            _mainFrame = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Height = 600
            };

            this.Controls.Add(_mainFrame);

            ShowMarketView();
        }

        public void ShowMarketView()
        {
            var marketView = new MarketView(_dbConnection);
            LoadView(marketView);
        }

        public void ShowPaiementView(int tenantId)
        {
            Console.WriteLine(tenantId);
            var paiementView = new PaiementView(tenantId, _dbConnection);
            LoadView(paiementView);
        }

        public void ShowListePayementsView()
        {
            var listePayementsView = new ListePayements(_dbConnection);
            LoadView(listePayementsView);
        }

        public void ShowListePersonneView()
        {
            var listePersonneView = new ListePersonne(_dbConnection, ShowPaiementView);
            LoadView(listePersonneView);
        }

        private void LoadView(UserControl view)
        {
            _mainFrame.Controls.Clear();
            if (!(view is MarketView))
            {
                Panel retourPanel = CreateRetourButtonPanel();
                _mainFrame.Controls.Add(retourPanel);
            }


            view.Dock = DockStyle.Fill;
            _mainFrame.Controls.Add(view);
        }

        private void OnPayerClicked(object sender, EventArgs e)
        {
            ShowListePersonneView();
        }

        private void OnListePayementsClicked(object sender, EventArgs e)
        {
            ShowListePayementsView();
        }

        private Panel CreateRetourButtonPanel()
        {
            var retourPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                Location = new Point(0, 100),
                Margin = new Padding(0, 50, 0, 0) // Ajoute un margin-top de 20
            };

            Button retourButton = new Button
            {
                Text = "<- Retour",
                BackColor = ColorTranslator.FromHtml("#5A5D63"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                AutoSize = true
            };
            retourButton.Click += (sender, e) => ShowMarketView();

            retourPanel.Controls.Add(retourButton);
            return retourPanel;
        }
    }
}