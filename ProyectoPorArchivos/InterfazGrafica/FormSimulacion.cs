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






        //CARGA LA SIMULACION
        private void FormSimulacion_Load(object sender, EventArgs e)
        {
            Avion1.Location = new Point(
                Convert.ToInt32(miLista.GetFlightPlan(0).GetCurrentPosition().GetX()),
                Convert.ToInt32(miLista.GetFlightPlan(0).GetCurrentPosition().GetY())
            );
            Avion2.Location = new Point(
                Convert.ToInt32(miLista.GetFlightPlan(1).GetCurrentPosition().GetX()),
                Convert.ToInt32(miLista.GetFlightPlan(1).GetCurrentPosition().GetY())
            );

            // INICIALIZAR PUNTOS DE LÍNEAS
            origen1 = new Point(
                Convert.ToInt32(miLista.GetFlightPlan(0).GetInitialPosition().GetX()),
                Convert.ToInt32(miLista.GetFlightPlan(0).GetInitialPosition().GetY())
            );
            destino1 = new Point(
                Convert.ToInt32(miLista.GetFlightPlan(0).GetFinalPosition().GetX()),
                Convert.ToInt32(miLista.GetFlightPlan(0).GetFinalPosition().GetY())
            );

            origen2 = new Point(
                Convert.ToInt32(miLista.GetFlightPlan(1).GetInitialPosition().GetX()),
                Convert.ToInt32(miLista.GetFlightPlan(1).GetInitialPosition().GetY())
            );
            destino2 = new Point(
                Convert.ToInt32(miLista.GetFlightPlan(1).GetFinalPosition().GetX()),
                Convert.ToInt32(miLista.GetFlightPlan(1).GetFinalPosition().GetY())
            );

            // FORZAR DIBUJADO INICIAL
            miPanel.Invalidate();
        }






        //Muestra info del avion 1
        private void Avion1_Click_1(object sender, EventArgs e)
        {
            InfoAvion formInfo = new InfoAvion(miLista.GetFlightPlan(0));
            formInfo.Show();
        }






        //Muesta info del vuelo 2
        private void Avion2_Click(object sender, EventArgs e)
        {
            InfoAvion formInfo = new InfoAvion(miLista.GetFlightPlan(1));
            formInfo.Show();
        }

        int i = 0;
        bool inicio = false;
        bool final = false;
        






        //INICIO DE LA SIMULACION
        private void Inicio_Click(object sender, EventArgs e)
        {
            inicio = true;
            final = false;
            i = 0;

            timer1.Enabled = true;
        }






        //FINAL DE LA SIMULACION
        private void Final_Click(object sender, EventArgs e)
        {
            inicio = false;
            final = true;

            timer1.Enabled = false;
        }







        //MOTOR DE LA SIMULACION
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (inicio == false || final)
            {
                timer1.Enabled = false;
            }
            else
            {
                miLista.Mover(tiempoCiclo);

                Avion1.Location = new Point(
                    Convert.ToInt32(miLista.GetFlightPlan(0).GetCurrentPosition().GetX()),
                    Convert.ToInt32(miLista.GetFlightPlan(0).GetCurrentPosition().GetY())
                );

                Avion2.Location = new Point(
                    Convert.ToInt32(miLista.GetFlightPlan(1).GetCurrentPosition().GetX()),
                    Convert.ToInt32(miLista.GetFlightPlan(1).GetCurrentPosition().GetY())
                );

                VerificarConflictoTiempoReal();
                miPanel.Invalidate();

                if (miLista.hanLlegadoTodos())
                {
                    inicio = false;
                    final = true;
                    timer1.Enabled = false;
                }
            }
        }






        //Mostrar datos actuales de los vuelos
        private void MostrarDatosActuales_Click(object sender, EventArgs e)
        {

            timer1.Enabled = false;

            DatosActuales datosActuales = new DatosActuales(miLista);
            datosActuales.PonerDatos();
            datosActuales.ShowDialog();

            timer1.Enabled = true;

        }








        //Cada vez que se mueve un ciclo, se verifica si hay conflicto en tiempo real
        private void VerificarConflictoTiempoReal()
        {
            FlightPlan vuelo1 = miLista.GetFlightPlan(0);
            FlightPlan vuelo2 = miLista.GetFlightPlan(1);

            double distanciaCentros = vuelo1.Distancia(
                vuelo2.GetCurrentPosition().GetX(),
                vuelo2.GetCurrentPosition().GetY()
            );

            double umbralSeguridad = 2 * distanciaSeguridad;
            bool enConflicto = distanciaCentros < umbralSeguridad;

            if (enConflicto && conflictoActivo == false)
            {
                conflictoActivo = true;
                MessageBox.Show($"Conflicto detectado. Distancia: {distanciaCentros}");
                Avion1.BackColor = Color.Red;
                Avion2.BackColor = Color.Red;
            }
            else if (enConflicto == false && conflictoActivo)
            {
                conflictoActivo = false;
                Avion1.BackColor = Color.LightCoral;
                Avion2.BackColor = Color.LightGreen;
            }
        }









        //Boton de verificacion de conflicto
        private void btnVerificarConflicto_Click(object sender, EventArgs e)
        {
            FlightPlan vuelo1 = miLista.GetFlightPlan(0);
            FlightPlan vuelo2 = miLista.GetFlightPlan(1);

            double distanciaCentros = vuelo1.Distancia(
                vuelo2.GetCurrentPosition().GetX(),
                vuelo2.GetCurrentPosition().GetY()
            );

            double umbralSeguridad = 2 * distanciaSeguridad;
            bool enConflicto = distanciaCentros < umbralSeguridad;

            if (enConflicto)
            {
                MessageBox.Show($"CONFLICTO - Distancia: {distanciaCentros:F2}", "Conflicto", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show($"Sin conflicto - Distancia: {distanciaCentros:F2}", "Sin Problemas", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }





        public double vel1;
        public double vel2;
        public void SetVelocidad(double v1, double v2)
        {
            vel1 = v1;
            vel2 = v2;
        }


        //Modificar velocidades de los vuelos
        private void btnCambiarVelocidadesDeLosVuelos_Click(object sender, EventArgs e)
        {
            CambiarVelocidades Cambiar = new CambiarVelocidades();
            Cambiar.SetVelocidad(vel1, vel2);
            Cambiar.ShowDialog();
            miLista.GetFlightPlan(0).SetVelocidad(Cambiar.velvuelo1);
            miLista.GetFlightPlan(1).SetVelocidad(Cambiar.velvuelo2);

        }

        //BOTON DE REINICIO DE LA SIMULACION
        private void btnReiniciar_Click(object sender, EventArgs e)
        {
            // Avion 1 a posicion inicial
            miLista.GetFlightPlan(0).SetPosition(
                miLista.GetFlightPlan(0).GetInitialPosition().GetX(),
                miLista.GetFlightPlan(0).GetInitialPosition().GetY()
            );

            // Avion 2 a posicion inicial
            miLista.GetFlightPlan(1).SetPosition(
                miLista.GetFlightPlan(1).GetInitialPosition().GetX(),
                miLista.GetFlightPlan(1).GetInitialPosition().GetY()
            );

            // Actualiza posiciones en pantalla
            Avion1.Location = new Point(
                Convert.ToInt32(miLista.GetFlightPlan(0).GetCurrentPosition().GetX()),
                Convert.ToInt32(miLista.GetFlightPlan(0).GetCurrentPosition().GetY())
            );
            Avion2.Location = new Point(
                Convert.ToInt32(miLista.GetFlightPlan(1).GetCurrentPosition().GetX()),
                Convert.ToInt32(miLista.GetFlightPlan(1).GetCurrentPosition().GetY())
            );

            // Reinicia la simulación
            inicio = false;
            final = false;
            conflictoActivo = false;

            // Restaura colores
            Avion1.BackColor = Color.LightCoral;
            Avion2.BackColor = Color.LightGreen;

            // Redibuja
            miPanel.Invalidate();
        }
    }
}














private void deshacerBtn_Click(object sender, EventArgs e)
{
    if (miLista == null) return;

    // Verificar si estamos en posición inicial
    bool enInicial = true;
    for (int i = 0; i < miLista.GetNum(); i++)
    {
        double xActual = miLista.GetFlightPlan(i).GetCurrentPosition().GetX();
        double yActual = miLista.GetFlightPlan(i).GetCurrentPosition().GetY();
        double xInicial = miLista.GetFlightPlan(i).GetInitialPosition().GetX();
        double yInicial = miLista.GetFlightPlan(i).GetInitialPosition().GetY();

        if (xActual != xInicial || yActual != yInicial)
        {
            enInicial = false;
            break;
        }
    }

    if (enInicial)
    {
        MessageBox.Show("Ya estás en la posición inicial");
        return;
    }

    else
    {
        miLista.Mover(-tiempoCiclo);
        GuardarEstadoActual();
        VerificarConflictoTiempoReal();
        miPanel.Invalidate();   
        ActualizarIconos();

    }
}






MessageBox.Show($"Información de los datos en el fichero de vuelos:\n\n - Los datos deben estar ordenados en ID, compañia, x inicial, y inicial,        x final, y final, velocidad.\n" +
    $" - Cada linea de la lista tiene que corresponder a un vuelo. \n - Los datos deben estar separados por comas (,)");





















private void exportarListaBtn_Click(object sender, EventArgs e)
{
    // Ruta dentro de la carpeta del proyecto
    string proyectoPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\"));

    SaveFileDialog saveDialog = new SaveFileDialog();
    saveDialog.Filter = "Archivos de texto (*.txt)|*.txt";
    saveDialog.Title = "Guardar vuelos";
    saveDialog.FileName = $"vuelos_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

    // Establecer la ruta inicial en la carpeta del proyecto
    saveDialog.InitialDirectory = proyectoPath;

    if (saveDialog.ShowDialog() == DialogResult.OK)
    {
        try
        {
            using (StreamWriter archivo = new StreamWriter(saveDialog.FileName))
            {
                // Usar CultureInfo.InvariantCulture para asegurar punto como separador decimal
                System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;
                
                for (int i = 0; i < miLista.GetNum(); i++)
                {
                    FlightPlan vuelo = miLista.GetFlightPlan(i);
                    
                    // Formatear números con punto decimal usando CultureInfo.InvariantCulture
                    string linea = string.Format(culture,
                        "{0},{1},{2:F2},{3:F2},{4:F2},{5:F2},{6:F2}",
                        vuelo.GetId(),
                        vuelo.GetCompany(),
                        vuelo.GetCurrentPosition().GetX(),
                        vuelo.GetCurrentPosition().GetY(),
                        vuelo.GetFinalPosition().GetX(),
                        vuelo.GetFinalPosition().GetY(),
                        vuelo.GetVelocidad());
                    
                    archivo.WriteLine(linea);
                }
            }

            // Mostrar ruta relativa al proyecto
            string rutaRelativa = Path.GetRelativePath(proyectoPath, saveDialog.FileName);
            MessageBox.Show($"Archivo guardado en:\n{rutaRelativa}", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            numeroArchivosGuardado++;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al guardar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
