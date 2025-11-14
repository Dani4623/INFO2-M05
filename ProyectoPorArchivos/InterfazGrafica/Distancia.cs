using FlightLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InterfazGrafica
{
    public partial class Distancia : Form
    {
        private FlightPlanList milista;
        public Distancia(FlightPlan plan, FlightPlanList lista)
        {
            InitializeComponent();
            this.milista = lista;
            if (milista.GetFlightPlan(0).GetId() == plan.GetId())
            {
                Vuelo1.Text = milista.GetFlightPlan(0).GetId();
                Vuelo2.Text = milista.GetFlightPlan(1).GetId();
                textBox3.Text = Convert.ToString(milista.GetFlightPlan(0).Distancia(milista.GetFlightPlan(1).GetCurrentPosition().GetX(), milista.GetFlightPlan(1).GetCurrentPosition().GetY()));
            }
            else
            {
                Vuelo1.Text = milista.GetFlightPlan(1).GetId();
                Vuelo2.Text = milista.GetFlightPlan(0).GetId();
                textBox3.Text = Convert.ToString(milista.GetFlightPlan(1).Distancia(milista.GetFlightPlan(0).GetCurrentPosition().GetX(), milista.GetFlightPlan(0).GetCurrentPosition().GetY()));
            }
        }

        


        private void Distancia_Load(object sender, EventArgs e)
        {

        }
    }
}
