using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace Projet.Net
{
    public partial class Home : Form
    {
        private readonly HttpClient httpClient;
        private readonly Random random;

        public Home()
        {
            InitializeComponent();
            httpClient = new HttpClient();
            random = new Random();
            // Définir le style du formulaire pour occuper tout l'écran
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen; // Centrer le formulaire sur l'écran
        }

        private async void Home_Load(object sender, EventArgs e)
        {
            try
            {
                // Récupérer la liste des catégories de recettes
                List<string> categories = await GetCategories();

                // Sélectionner aléatoirement cinq catégories
                List<string> selectedCategories = SelectRandomCategories(categories);

                // Créer un Panel pour contenir le FlowLayoutPanel avec la barre de défilement
                Panel scrollPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    AutoScroll = true // Activer la barre de défilement automatique
                };

                // Ajouter le FlowLayoutPanel au Panel de défilement
                scrollPanel.Controls.Add(flowLayoutPanel1);
                Controls.Add(scrollPanel); // Ajouter le Panel de défilement au formulaire

                // Activer le défilement automatique du FlowLayoutPanel
                flowLayoutPanel1.AutoScroll = true;

                // Fixer la largeur et la hauteur du FlowLayoutPanel
                flowLayoutPanel1.Width = 1500; // Largeur fixe de 800 pixels
                flowLayoutPanel1.Height = 800; // Hauteur fixe de 600 pixels

                // Récupérer les repas pour chaque catégorie sélectionnée et les afficher
                foreach (string category in selectedCategories)
                {
                    await DisplayMealsForCategory(category);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }

        private async Task<List<string>> GetCategories()
        {
            HttpResponseMessage response = await httpClient.GetAsync("https://www.themealdb.com/api/json/v1/1/list.php?c=list");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<CategoryListResponse>(responseBody);
            List<string> categories = new List<string>();
            foreach (var category in result.meals)
            {
                categories.Add(category.strCategory);
            }
            return categories;
        }

        private List<string> SelectRandomCategories(List<string> categories)
        {
            List<string> selectedCategories = new List<string>();
            while (selectedCategories.Count < 5)
            {
                int index = random.Next(categories.Count);
                string category = categories[index];
                if (!selectedCategories.Contains(category))
                {
                    selectedCategories.Add(category);
                }
            }
            return selectedCategories;
        }

        private async Task DisplayMealsForCategory(string category)
        {
            HttpResponseMessage response = await httpClient.GetAsync($"https://www.themealdb.com/api/json/v1/1/filter.php?c={category}");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RootObject>(responseBody);

            if (result.meals != null && result.meals.Length > 0)
            {
                foreach (var meal in result.meals)
                {
                    // Créer et afficher les contrôles pour chaque repas
                    createCardMeal(meal);
                }
            }
        }

        private void createCardMeal(Meal meal)
        {
            // Créer un Panel pour contenir uniquement l'image
            Panel imagePanel = new Panel
            {
                Width = 260,
                Height = 260,
                Padding = new Padding(5), // Ajouter un remplissage pour l'apparence de la carte
                BackColor = Color.White // Couleur de fond de la carte
            };

            // Créer un PictureBox pour afficher l'image
            PictureBox pictureBox = new PictureBox
            {
                ImageLocation = meal.strMealThumb,
                SizeMode = PictureBoxSizeMode.Zoom,
                Dock = DockStyle.Fill // Remplir le PictureBox dans le Panel
            };

            // Ajouter le PictureBox au Panel de l'image
            imagePanel.Controls.Add(pictureBox);

            // Créer un Button pour afficher les détails du repas
            Button detailsButton = new Button
            {
                Text = "Détails",
                Width = 70,
                Location = new Point(180, 290), // Positionner le bouton sous l'image à droite
                BackColor = Color.Orange, // Définir la couleur d'arrière-plan du bouton en orange
                FlatStyle = FlatStyle.Flat, // Définir le style du bouton sur Flat pour désactiver la bordure
                FlatAppearance = { BorderSize = 0 }, // Définir la taille de la bordure sur 0 pour la rendre invisible
            };

            // Gérer l'événement de clic du bouton pour afficher les détails du repas
            detailsButton.Click += (_, args) =>
            {
                // Récupérer l'ID du repas en utilisant la méthode GetMealId()
                string mealId = GetMealId(meal);

                // Créer un nouveau formulaire pour afficher les détails du repas
                MealDetailsForm mealDetailsForm = new MealDetailsForm();

                // Afficher le nouveau formulaire
                mealDetailsForm.Show();
            };

            // Créer un Button pour afficher le nom du repas
            Button mealButton = new Button
            {
                Text = meal.strMeal,
                AutoSize = false, // Désactiver l'ajustement automatique de la taille en fonction du contenu
                Width = 250, // Définir la largeur du bouton
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 12, FontStyle.Bold), // Définir la police avec la taille et le style
                Location = new Point(2, 260), // Positionner le bouton sous l'image à gauche
                Margin = new Padding(0, 0, 0, 10), // Ajouter une marge de 10 pixels au-dessus du bouton
                FlatStyle = FlatStyle.Flat, // Définir le style du bouton sur Flat pour désactiver la bordure
                FlatAppearance = { BorderSize = 0 }, // Définir la taille de la bordure sur 0 pour la rendre invisible
            };

            // Créer un Panel pour contenir à la fois l'image, le nom du repas et les boutons
            Panel panel = new Panel
            {
                Width = 260,
                Height = 320, // Ajuster la hauteur pour inclure les boutons
                Padding = new Padding(5), // Ajouter un remplissage pour l'apparence du panneau
                Margin = new Padding(30), // Ajouter une marge entre les cartes
                BorderStyle = BorderStyle.FixedSingle, // Ajouter une bordure
                BackColor = Color.White, // Couleur de fond du panneau
            };

            // Ajouter les contrôles au panneau
            panel.Controls.Add(imagePanel);
            panel.Controls.Add(mealButton);
            panel.Controls.Add(detailsButton);

            // Ajouter le panneau au FlowLayoutPanel
            flowLayoutPanel1.Controls.Add(panel);
        }

        private string GetMealId(Meal meal)
        {
            return meal.IdMeal;
        }
    }

    // Définition des classes de données pour désérialiser les réponses JSON
    public class Meal
    {
        public string strMealThumb { get; set; }
        public string strMeal { get; set; }
        public string IdMeal { get; set; }
    }

    public class RootObject
    {
        public Meal[] meals { get; set; }
    }

    public class Category
    {
        public string strCategory { get; set; }
    }

    public class CategoryListResponse
    {
        public List<Category> meals { get; set; }
    }
}
