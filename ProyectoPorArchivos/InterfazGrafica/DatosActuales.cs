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
    public partial class DatosActuales : Form
    {

        private FlightPlanList miLista;
        public DatosActuales(FlightPlanList lista)
        {
            InitializeComponent();
            this.miLista = lista;
        }

        public void PonerDatos()
        {
            dataGridView1.Rows.Clear();
            dataGridView1.ColumnCount = 4;
            dataGridView1.Columns[0].Name = "ID";
            dataGridView1.Columns[1].Name = "Velocidad";
            dataGridView1.Columns[2].Name = "Posición X actual";
            dataGridView1.Columns[3].Name = "Posición Y actual";

            for (int i = 0; i < miLista.GetNum(); i++)
            {
                FlightPlan plan = miLista.GetFlightPlan(i);
                string id = plan.GetId();
                double velocidad = plan.GetVelocidad();
                double px = plan.GetCurrentPosition().GetX();
                double py = plan.GetCurrentPosition().GetY();

                dataGridView1.Rows.Add(id, velocidad, px, py);
            }

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            string idSeleccionado = Convert.ToString(dataGridView1.Rows[e.RowIndex].Cells[0].Value);


            FlightPlan planSeleccionado = null;
            for (int i = 0; i < miLista.GetNum(); i++)
            {
                if (miLista.GetFlightPlan(i).GetId() == idSeleccionado)
                {
                    planSeleccionado = miLista.GetFlightPlan(i);
                    break;
                }
            }

            if (planSeleccionado != null)
            {
                Distancia ventana = new Distancia(planSeleccionado, miLista);
                ventana.ShowDialog();
            }
        }

        private void DatosActuales_Load(object sender, EventArgs e)
        {

        }
    }
}
