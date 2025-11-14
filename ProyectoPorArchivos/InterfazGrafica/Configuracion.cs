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
    public partial class Configuracion : Form
    {
        public double DistanciaSeguridadValor;
        public double TiempoCicloValor;
        public Configuracion()
        {
            InitializeComponent();
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            try
            {
                DistanciaSeguridadValor = Convert.ToDouble(DistanciaSeguridad.Text);
                TiempoCicloValor = Convert.ToDouble(TiempoCiclo.Text);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en los datos");
            }
        }

        private void Configuracion_Load(object sender, EventArgs e)
        {

        }

        private void DatosEjemploBtn_Click(object sender, EventArgs e)
        {
            DistanciaSeguridad.Text = "50";
            TiempoCiclo.Text = "10";
        }
    }
}
