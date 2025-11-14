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
    public partial class InfoAvion : Form
    {
        public InfoAvion(FlightPlan vuelo)
        {
            InitializeComponent();

            lblId.Text = "ID: " + vuelo.GetId;
            lblVelocidad.Text = "Velocidad: " + vuelo.GetVelocidad().ToString();
            lblPosActual.Text = "Pos Actual: (" + vuelo.GetCurrentPosition().GetX() + ", " + vuelo.GetCurrentPosition().GetY() + ")";
            lblPosDestino.Text = "Pos Destino: (" + vuelo.GetFinalPosition().GetX() + ", " + vuelo.GetFinalPosition().GetY() + ")";
            lblEstado.Text = "Estado: " + (vuelo.HasArrived() ? "Llegado" : "En camino");
        }

        private void InfoAvion_Load(object sender, EventArgs e)
        {

        }
    }
}
