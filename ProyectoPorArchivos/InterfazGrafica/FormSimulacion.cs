using FlightLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InterfazGrafica
{
    public partial class FormSimulacion : Form
    {
        private bool conflictoActivo = false;
        private FlightPlanList miLista;
        int tiempoCiclo;
        FlightPlan Vuelo1;
        FlightPlan Vuelo2;

        // VARIABLES PARA DIBUJAR LÍNEAS Y ELIPSES
        private Point origen1, destino1, origen2, destino2;

        private double distanciaSeguridad;
        private int diametroSeguridad;

        public void SetPlanes(FlightPlan Vuelo1, FlightPlan Vuelo2)
        {
            Vuelo1 = Vuelo1;
            Vuelo2 = Vuelo2;
        }

        public FormSimulacion(FlightPlanList lista, int tiempo, double distSeguridad)
        {
            InitializeComponent();
            miLista = lista;
            tiempoCiclo = tiempo;

            // USAR LA DISTANCIA CONFIGURADA POR EL USUARIO
            distanciaSeguridad = distSeguridad;
            diametroSeguridad = Convert.ToInt32((distanciaSeguridad * 2));

            // Configurar eventos de dibujo
            miPanel.Paint += new PaintEventHandler(DibujarLineasRuta);
            miPanel.Paint += new PaintEventHandler(DibujarElementos);
        }

        // MÉTODO NUEVO PARA DIBUJAR LÍNEAS
        private void DibujarLineasRuta(object sender, PaintEventArgs e)
        {
            // Dibujar línea para el vuelo 1 (color que coincida con Avion1)
            using (Pen pen1 = new Pen(Color.FromArgb(255, 128, 128), 2))
            {
                pen1.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                e.Graphics.DrawLine(pen1, origen1, destino1);
            }

            // Dibujar línea para el vuelo 2 (color que coincida con Avion2)
            using (Pen pen2 = new Pen(Color.FromArgb(128, 255, 128), 2))
            {
                pen2.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                e.Graphics.DrawLine(pen2, origen2, destino2);
            }
        }

        private void btnMoverCiclo_Click(object sender, EventArgs e)
        {
            miLista.Mover(tiempoCiclo);

            // Actualizar posiciones
            Avion1.Location = new Point(
                (int)miLista.GetFlightPlan(0).GetCurrentPosition().GetX(),
                (int)miLista.GetFlightPlan(0).GetCurrentPosition().GetY()
            );

            Avion2.Location = new Point(
                (int)miLista.GetFlightPlan(1).GetCurrentPosition().GetX(),
                (int)miLista.GetFlightPlan(1).GetCurrentPosition().GetY()
            );

            // VERIFICAR CONFLICTO (FASE 10)
            VerificarConflictoTiempoReal();

            miPanel.Invalidate();
        }

        // MÉTODO NUEVO PARA DIBUJAR LÍNEAS Y ELIPSES
        private void DibujarElementos(object sender, PaintEventArgs e)
        {
            // Dibujar línea para el vuelo 1
            using (Pen pen1 = new Pen(Color.FromArgb(255, 128, 128), 2))
            {
                pen1.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                e.Graphics.DrawLine(pen1, origen1, destino1);
            }


            using (Pen pen2 = new Pen(Color.FromArgb(128, 255, 128), 2))
            {
                pen2.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                e.Graphics.DrawLine(pen2, origen2, destino2);
            }


            using (Brush brush1 = new SolidBrush(Color.FromArgb(50, 255, 0, 0)))
            {
                Rectangle elipse1 = new Rectangle(
                    Avion1.Location.X - (int)distanciaSeguridad,
                    Avion1.Location.Y - (int)distanciaSeguridad,
                    diametroSeguridad,
                    diametroSeguridad
                );
                e.Graphics.FillEllipse(brush1, elipse1);

                // Contorno de la elipse
                using (Pen penElipse1 = new Pen(Color.Red, 1))
                {
                    e.Graphics.DrawEllipse(penElipse1, elipse1);
                }
            }

            // Elipse para Avion2 (verde transparente)
            using (Brush brush2 = new SolidBrush(Color.FromArgb(50, 0, 255, 0)))
            {
                Rectangle elipse2 = new Rectangle(
                    Avion2.Location.X - (int)distanciaSeguridad,
                    Avion2.Location.Y - (int)distanciaSeguridad,
                    diametroSeguridad,
                    diametroSeguridad
                );
                e.Graphics.FillEllipse(brush2, elipse2);

                // Contorno de la elipse
                using (Pen penElipse2 = new Pen(Color.Green, 1))
                {
                    e.Graphics.DrawEllipse(penElipse2, elipse2);
                }
            }
        }

        // MÉTODO NUEVO PARA ACTUALIZAR DISTANCIA DE SEGURIDAD
        public void SetDistanciaSeguridad(double distancia)
        {
            distanciaSeguridad = distancia;
            diametroSeguridad = (int)(distancia * 2);
            miPanel.Invalidate(); // Redibujar con la nueva distancia
        }


        private void FormSimulacion_Load(object sender, EventArgs e)
        {
            Avion1.Location = new Point((int)miLista.GetFlightPlan(0).GetCurrentPosition().GetX(), (int)miLista.GetFlightPlan(0).GetCurrentPosition().GetY());
            Avion2.Location = new Point((int)miLista.GetFlightPlan(1).GetCurrentPosition().GetX(), (int)miLista.GetFlightPlan(1).GetCurrentPosition().GetY());

            // INICIALIZAR PUNTOS DE LÍNEAS (NUEVO)
            origen1 = new Point(
                (int)miLista.GetFlightPlan(0).GetInitialPosition().GetX(),
                (int)miLista.GetFlightPlan(0).GetInitialPosition().GetY()
            );
            destino1 = new Point(
                (int)miLista.GetFlightPlan(0).GetFinalPosition().GetX(),
                (int)miLista.GetFlightPlan(0).GetFinalPosition().GetY()
            );

            origen2 = new Point(
                (int)miLista.GetFlightPlan(1).GetInitialPosition().GetX(),
                (int)miLista.GetFlightPlan(1).GetInitialPosition().GetY()
            );
            destino2 = new Point(
                (int)miLista.GetFlightPlan(1).GetFinalPosition().GetX(),
                (int)miLista.GetFlightPlan(1).GetFinalPosition().GetY()
            );

            // FORZAR DIBUJADO INICIAL (NUEVO)
            miPanel.Invalidate();
        }


        private void Avion1_Click_1(object sender, EventArgs e)
        {
            InfoAvion formInfo = new InfoAvion(miLista.GetFlightPlan(0));
            formInfo.Show();
        }

        private void Avion2_Click(object sender, EventArgs e)
        {
            InfoAvion formInfo = new InfoAvion(miLista.GetFlightPlan(1));
            formInfo.Show();
        }

        int i = 0;
        bool inicio = false;
        bool final = false;

        private void Inicio_Click(object sender, EventArgs e)
        {
            inicio = true;
            final = false;
            i = 0;

            timer1.Enabled = true;
        }


        private void Final_Click(object sender, EventArgs e)
        {
            inicio = false;
            final = true;

            timer1.Enabled = false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!inicio || final)
            {
                timer1.Enabled = false;
                return;
            }

            miLista.Mover(tiempoCiclo);
            Avion1.Location = new Point(
                (int)miLista.GetFlightPlan(0).GetCurrentPosition().GetX(),
                (int)miLista.GetFlightPlan(0).GetCurrentPosition().GetY()
            );

            Avion2.Location = new Point(
                (int)miLista.GetFlightPlan(1).GetCurrentPosition().GetX(),
                (int)miLista.GetFlightPlan(1).GetCurrentPosition().GetY()
            );

            // VERIFICAR CONFLICTO (FASE 10)
            VerificarConflictoTiempoReal();

            miPanel.Invalidate();

            if (miLista.hanLlegadoTodos())
            {
                inicio = false;
                final = true;
                timer1.Enabled = false;
            }
        }

        private void MostrarDatosActuales_Click(object sender, EventArgs e)
        {

            timer1.Enabled = false;

            DatosActuales datosActuales = new DatosActuales(miLista);
            datosActuales.PonerDatos();
            datosActuales.ShowDialog();

            timer1.Enabled = true;

        }
        private void VerificarConflictoTiempoReal()
        {
            if (miLista.GetNum() < 2) return;

            FlightPlan vuelo1 = miLista.GetFlightPlan(0);
            FlightPlan vuelo2 = miLista.GetFlightPlan(1);

            // Calcular distancia entre centros
            double distanciaCentros = vuelo1.Distancia(
                vuelo2.GetCurrentPosition().GetX(),
                vuelo2.GetCurrentPosition().GetY()
            );

            // CONFLICTO cuando distancia entre centros < 2 * distanciaSeguridad
            // (porque cada elipse tiene radio = distanciaSeguridad)
            //TENEMOS LA DUDA DE SI LA DISTANCIA DE SEGURIDAD ES DOBLE POR LAS ELIPSES O SOLO CUANDO ENTRE UN AVIÓN
            //DENTRO DE LA ELIPSE?????
            bool enConflicto = distanciaCentros < (2 * distanciaSeguridad);

            if (enConflicto && !conflictoActivo)
            {
                conflictoActivo = true;

                MessageBox.Show($"¡ALERTA! Las zonas de seguridad se están tocando.\n" +
                               $"Distancia entre aviones: {distanciaCentros:F2}\n" +
                               $"Umbral de seguridad: {2 * distanciaSeguridad:F2}",
                               "Conflicto Detectado - Fase 10",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Warning);

                // Cambiar color de los aviones
                Avion1.BackColor = Color.Red;
                Avion2.BackColor = Color.Red;
            }
            else if (!enConflicto && conflictoActivo)
            {
                conflictoActivo = false;
                //Color rojo a los aviones
                Avion1.BackColor = Color.FromArgb(255, 192, 192);
                Avion2.BackColor = Color.FromArgb(192, 255, 192);
            }
        }
        private void btnVerificarConflicto_Click(object sender, EventArgs e)
        {
            if (miLista.GetNum() < 2)
            {
                MessageBox.Show("No hay suficientes vuelos para verificar conflicto.");
                return;
            }

            FlightPlan vuelo1 = miLista.GetFlightPlan(0);
            FlightPlan vuelo2 = miLista.GetFlightPlan(1);

            double distanciaCentros = vuelo1.Distancia(
                vuelo2.GetCurrentPosition().GetX(),
                vuelo2.GetCurrentPosition().GetY()
            );

            double umbralSeguridad = 2 * distanciaSeguridad;
            bool enConflicto = distanciaCentros < umbralSeguridad;

            string mensaje = enConflicto
                ? $"CONFLICTO DETECTADO\nDistancia: {distanciaCentros:F2}"
                : $"SIN CONFLICTO\nDistancia: {distanciaCentros:F2}";

            MessageBox.Show(mensaje, "Verificación de Conflicto",
                           MessageBoxButtons.OK,
                           enConflicto ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }
        public double vel1;
        public double vel2;
        public void SetVelocidad(double v1, double v2)
        {
            vel1 = v1;
            vel2 = v2;
        }
        private void btnCambiarVelocidadesDeLosVuelos_Click(object sender, EventArgs e)
        {
            CambiarVelocidades Cambiar = new CambiarVelocidades();
            Cambiar.SetVelocidad(vel1, vel2);
            Cambiar.ShowDialog();
            Vuelo1.SetVelocidad(Cambiar.velvuelo1);
            Vuelo2.SetVelocidad(Cambiar.velvuelo2);

        }

        private void btnReiniciar_Click(object sender, EventArgs e)
        {


        }
    }
}
