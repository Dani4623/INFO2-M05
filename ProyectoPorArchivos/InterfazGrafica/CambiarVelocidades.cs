using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FlightLib;


namespace InterfazGrafica
{
    public partial class CambiarVelocidades : Form
    {
        public double velvuelo1;
        public double velvuelo2;

        public CambiarVelocidades()
        {
            InitializeComponent();
        }
        public void SetVelocidad(double v1, double v2)
        {
            velvuelo1 = v1;
            velvuelo2 = v2;
        }
        private void CambiarVelocidades_Load(object sender, EventArgs e)
        {
            Velocidad1.Text = Convert.ToString(velvuelo1);
            Velocidad2.Text = Convert.ToString(velvuelo2);
        }
        private void btnAceptar_Click(object sender, EventArgs e)
        {
            velvuelo1 = Convert.ToDouble(Velocidad1.Text);
            velvuelo2 = Convert.ToDouble(Velocidad2.Text);
            Close();
        }

    }
}
