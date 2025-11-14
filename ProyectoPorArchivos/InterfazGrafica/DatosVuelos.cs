using System;
using System.Windows.Forms;
using FlightLib;

namespace InterfazGrafica
{
    public partial class DatosVuelos : Form
    {
        public FlightPlan Vuelo1;
        public FlightPlan Vuelo2;

        public DatosVuelos()
        {
            InitializeComponent();
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            try
            {
                // Vuelo 1 - separar coordenadas
                string[] pi1 = PIVuelo1.Text.Split(',');
                string[] pf1 = PFVuelo1.Text.Split(',');

                // Vuelo 2 - separar coordenadas  
                string[] pi2 = PIVuelo2.Text.Split(',');
                string[] pf2 = PFVuelo2.Text.Split(',');

                // Crear Vuelo 1
                Vuelo1 = new FlightPlan(
                    IDVuelo1.Text,
                    Convert.ToDouble(pi1[0]), Convert.ToDouble(pi1[1]),
                    Convert.ToDouble(pf1[0]), Convert.ToDouble(pf1[1]),
                    Convert.ToDouble(VelocidadVuelo1.Text)
                );

                // Crear Vuelo 2
                Vuelo2 = new FlightPlan(
                    IDVuelo2.Text,
                    Convert.ToDouble(pi2[0]), Convert.ToDouble(pi2[1]),
                    Convert.ToDouble(pf2[0]), Convert.ToDouble(pf2[1]),
                    Convert.ToDouble(VelocidadVuelo2.Text)
                );

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en los datos. Formato correcto: x,y");
            }
        }

        private void DatosVuelos_Load(object sender, EventArgs e)
        {
        }

        private void DatosEjemploBtn_Click(object sender, EventArgs e)
        {
            IDVuelo1.Text = "IB123";
            IDVuelo2.Text = "RYN321";
            VelocidadVuelo1.Text = "100";
            VelocidadVuelo2.Text = "100";
            PIVuelo1.Text = "100,100";
            PFVuelo1.Text = "300,300";
            PIVuelo2.Text = "100,300";
            PFVuelo2.Text = "300,100";
        }
    }
}
