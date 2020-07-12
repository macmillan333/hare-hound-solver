using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HareHoundSolver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private List<State> states;

        private void solve_button_Click(object sender, RoutedEventArgs e)
        {
            // Initialize all states.
            states = new List<State>();
            for (int id = 0; id <= State.max_id; id++)
            {
                states.Add(new State(id));
            }

            // Display.
            game_tree.Items.Clear();
            // Root state:
            //    D  1  2
            // D  4  5  6  R
            //    D  9 10
            // board: 3*121+8*11+7 = 458
            // next player: 0
            // state ID: 458*2 = 916
            game_tree.Items.Add(states[916]);
            // A winning state for dogs:
            //    0  1  D
            // 3  4  5  D  R
            //    8  9  D
            // board: 2*1331+6*121+10*11+7 = 3505
            // next player: 0
            // state ID: 3505*2 = 7010
            game_tree.Items.Add(states[7010]);
            // A winning state for rabbit:
            //    0  1  D
            // 3  4  5  R  D
            //    8  9  D
            // board: 2*1331+7*121+10*11+6 = 3625
            // next player: 1
            // state ID: 3625*2+1 = 7251
            game_tree.Items.Add(states[7251]);
        }
    }
}
