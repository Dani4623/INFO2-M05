using FlightLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;


namespace InterfazGrafica
{
    public partial class FormSimulacion : Form
    {
        private bool conflictoActivo = false;
        private FlightPlanList miLista;
        private List<int> vuelosEnConflicto = new List<int>();
        int tiempoCiclo;
        private List<PictureBox> iconosAviones = new List<PictureBox>();
        private Stack<Point[]> posiciones = new Stack<Point[]>();
        bool tienenEstadoAnterior = false;
        private bool simulacionTerminada = false;
        private bool mensajeListaVaciaMostrado = false;

        private double distanciaSeguridad;
        private int diametroSeguridad;
        private CompanyManager companyManager;
        private List<string> cambiosRealizados = new List<string>();

        public void SetPlanes(FlightPlanList lista)
        {
            miLista = lista;
        }

        public FormSimulacion(int tiempo, double distSeguridad)
        {
            InitializeComponent();

            tiempoCiclo = tiempo;
            distanciaSeguridad = distSeguridad;
            diametroSeguridad = Convert.ToInt32((distanciaSeguridad * 2));

            // Inicializar CompanyManager
            companyManager = new CompanyManager();

            miPanel.Paint += new PaintEventHandler(DibujarLineasRuta);
            miPanel.Paint += new PaintEventHandler(DibujarElementos);
            miPanel.Paint += new PaintEventHandler(DibujarCuadriculaRadar);

            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.miPanel.BackColor = System.Drawing.Color.Black;
            this.miPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.miPanel.ForeColor = System.Drawing.Color.FromArgb(0, 192, 192);
            System.Drawing.Color accentColor = System.Drawing.Color.FromArgb(0, 192, 192);

            this.Inicio.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(0, 192, 192);
            this.Inicio.FlatAppearance.BorderSize = 1;

            System.Drawing.Color buttonBackColor = System.Drawing.Color.FromArgb(45, 50, 60);
            System.Drawing.Color buttonForeColor = System.Drawing.Color.WhiteSmoke;

            System.Windows.Forms.Button[] allButtons = new System.Windows.Forms.Button[]
            {
                this.btnMoverCiclo,
                this.Inicio,
                this.Final,
                this.MostrarDatosActuales,
                this.btnCambiarVelocidadesDeLosVuelos,
                this.btnReiniciar,
                this.deshacerBtn,
                this.btnVerificarConflicto,
                this.mostrarDistanciasTxt,
                this.mostrarVuelosBtn,
                this.exportarListaBtn,
                this.informesBtn
            };

            foreach (System.Windows.Forms.Button btn in allButtons)
            {
                btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                btn.BackColor = buttonBackColor;
                btn.ForeColor = buttonForeColor;
                btn.FlatAppearance.BorderColor = accentColor;
                btn.FlatAppearance.BorderSize = 1;
            }

            this.BackColor = System.Drawing.Color.WhiteSmoke;
        }

        private void DibujarCuadriculaRadar(object sender, PaintEventArgs e)
        {
            Color gridColor = Color.FromArgb(20, 0, 192, 192);
            using (Pen gridPen = new Pen(gridColor, 1))
            {
                int panelWidth = miPanel.Width;
                int panelHeight = miPanel.Height;
                const int step = 50;

                for (int x = 0; x < panelWidth; x += step)
                {
                    e.Graphics.DrawLine(gridPen, x, 0, x, panelHeight);
                }

                for (int y = 0; y < panelHeight; y += step)
                {
                    e.Graphics.DrawLine(gridPen, 0, y, panelWidth, y);
                }
            }
        }

        private void FormSimulacion_Load(object sender, EventArgs e)
        {
            if (!mensajeListaVaciaMostrado && (miLista == null || miLista.GetNum() == 0))
            {
                MessageBox.Show("No hay vuelos cargados en la simulación.", "Lista vacía", MessageBoxButtons.OK, MessageBoxIcon.Information);
                mensajeListaVaciaMostrado = true;
                return;
            }

            InicializarIconos();
            RegistrarInicioSimulacion();
        }

        private void InicializarIconos()
        {
            iconosAviones.Clear();
            miPanel.Controls.Clear();

            if (miLista == null || miLista.GetNum() == 0)
            {
                MessageBox.Show("No hay vuelos para mostrar", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            for (int i = 0; i < miLista.GetNum(); i++)
            {
                try
                {
                    FlightPlan vuelo = miLista.GetFlightPlan(i);
                    if (vuelo == null) continue;

                    PictureBox avion = new PictureBox();
                    avion.Width = 20;
                    avion.Height = 20;
                    avion.BackColor = Color.LightGreen;
                    avion.BorderStyle = BorderStyle.FixedSingle;
                    avion.Tag = i;

                    int x = Convert.ToInt32(vuelo.GetCurrentPosition().GetX());
                    int y = Convert.ToInt32(vuelo.GetCurrentPosition().GetY());

                    // Validar coordenadas
                    x = Math.Max(0, Math.Min(x, miPanel.Width - avion.Width));
                    y = Math.Max(0, Math.Min(y, miPanel.Height - avion.Height));

                    avion.Location = new Point(x, y);

                    System.Windows.Forms.Label lbl = new System.Windows.Forms.Label();
                    lbl.Text = vuelo.GetId() ?? $"Vuelo {i + 1}";
                    lbl.AutoSize = true;
                    lbl.BackColor = Color.Transparent;
                    lbl.ForeColor = Color.White;
                    lbl.Font = new Font("Arial", 8);
                    lbl.Location = new Point(x + 15, y - 15);

                    miPanel.Controls.Add(avion);
                    miPanel.Controls.Add(lbl);
                    iconosAviones.Add(avion);
                    avion.Click += Avion_Click;
                }
                catch (Exception)
                {
                    // Continuar con el siguiente avión si hay error
                }
            }

            GuardarEstadoInicial();
            miPanel.Invalidate();
        }

        private void GuardarEstadoInicial()
        {
            if (miLista == null || miLista.GetNum() == 0) return;

            Point[] estadoInicial = new Point[miLista.GetNum()];
            for (int i = 0; i < miLista.GetNum(); i++)
            {
                FlightPlan vuelo = miLista.GetFlightPlan(i);
                if (vuelo != null)
                {
                    estadoInicial[i] = new Point(
                        Convert.ToInt32(vuelo.GetCurrentPosition().GetX()),
                        Convert.ToInt32(vuelo.GetCurrentPosition().GetY())
                    );
                }
            }

            posiciones.Clear();
            posiciones.Push(estadoInicial);
        }

        private void DibujarLineasRuta(object sender, PaintEventArgs e)
        {
            if (!mensajeListaVaciaMostrado && (miLista == null || miLista.GetNum() == 0))
            {
                return;
            }

            for (int i = 0; i < miLista.GetNum(); i++)
            {
                Point origen = new Point(
                    Convert.ToInt32(miLista.GetFlightPlan(i).GetInitialPosition().GetX()),
                    Convert.ToInt32(miLista.GetFlightPlan(i).GetInitialPosition().GetY())
                );
                Point destino = new Point(
                    Convert.ToInt32(miLista.GetFlightPlan(i).GetFinalPosition().GetX()),
                    Convert.ToInt32(miLista.GetFlightPlan(i).GetFinalPosition().GetY())
                );

                Color colorLinea = Color.FromArgb(0, 192, 192);
                bool enConflicto = vuelosEnConflicto.Contains(i);

                if (enConflicto)
                {
                    colorLinea = Color.Red;
                }

                using (Pen pen = new Pen(colorLinea, 2))
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    e.Graphics.DrawLine(pen, origen, destino);
                }
            }
        }

        private void DibujarElementos(object sender, PaintEventArgs e)
        {
            if (!mensajeListaVaciaMostrado && (miLista == null || miLista.GetNum() == 0))
            {
                return;
            }

            for (int i = 0; i < miLista.GetNum(); i++)
            {
                int x = Convert.ToInt32(miLista.GetFlightPlan(i).GetCurrentPosition().GetX());
                int y = Convert.ToInt32(miLista.GetFlightPlan(i).GetCurrentPosition().GetY());

                Rectangle elipse = new Rectangle(
                    x - Convert.ToInt32(distanciaSeguridad),
                    y - Convert.ToInt32(distanciaSeguridad),
                    diametroSeguridad,
                    diametroSeguridad
                );

                bool enConflicto = vuelosEnConflicto.Contains(i);

                if (enConflicto)
                {
                    using (Brush brush = new SolidBrush(Color.FromArgb(100, 255, 0, 0)))
                    {
                        e.Graphics.FillEllipse(brush, elipse);
                    }

                    using (Pen pen = new Pen(Color.Red, 1))
                    {
                        e.Graphics.DrawEllipse(pen, elipse);
                    }
                }
                else
                {
                    using (Brush brush = new SolidBrush(Color.FromArgb(50, 0, 192, 192)))
                    {
                        e.Graphics.FillEllipse(brush, elipse);
                    }

                    using (Pen pen = new Pen(Color.FromArgb(0, 192, 192), 1))
                    {
                        e.Graphics.DrawEllipse(pen, elipse);
                    }
                }
            }
        }

        private void GuardarEstadoActual()
        {
            if (miLista == null || miLista.GetNum() == 0) return;

            Point[] listaPosiciones = new Point[miLista.GetNum()];
            for (int i = 0; i < miLista.GetNum(); i++)
            {
                FlightPlan vuelo = miLista.GetFlightPlan(i);
                if (vuelo != null)
                {
                    listaPosiciones[i] = new Point(
                        Convert.ToInt32(vuelo.GetCurrentPosition().GetX()),
                        Convert.ToInt32(vuelo.GetCurrentPosition().GetY())
                    );
                }
            }
            posiciones.Push(listaPosiciones);
            tienenEstadoAnterior = true;
        }

        private void btnMoverCiclo_Click(object sender, EventArgs e)
        {
            if (miLista == null || miLista.GetNum() == 0)
            {
                MessageBox.Show("No hay vuelos para mover", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (simulacionTerminada)
            {
                MessageBox.Show("La simulación ya terminó. Usa 'Deshacer' o 'Reiniciar'.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                GuardarEstadoActual();
                miLista.Mover(tiempoCiclo);
                ActualizarIconos();
                VerificarConflictoTiempoReal();
                miPanel.Invalidate();
                RegistrarMovimientoCiclo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al mover aviones: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void SetDistanciaSeguridad(double distancia)
        {
            distanciaSeguridad = distancia;
            diametroSeguridad = Convert.ToInt32((distancia * 2));
            miPanel.Invalidate();
        }

        private void Avion_Click(object sender, EventArgs e)
        {
            if (!mensajeListaVaciaMostrado && (miLista == null || miLista.GetNum() == 0))
            {
                MessageBox.Show("No hay vuelos cargados en la simulación.", "Lista vacía", MessageBoxButtons.OK, MessageBoxIcon.Information);
                mensajeListaVaciaMostrado = true;
                return;
            }

            PictureBox pb = (PictureBox)sender;
            int index = Convert.ToInt32(pb.Tag);

            InfoAvion info = new InfoAvion(miLista.GetFlightPlan(index));
            info.Show();
        }

        int i = 0;
        bool inicio = false;
        bool final = false;

        private void Inicio_Click(object sender, EventArgs e)
        {
            if (simulacionTerminada)
            {
                MessageBox.Show("La simulación ya terminó. Usa 'Deshacer' o 'Reiniciar'.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            inicio = true;
            final = false;
            simulacionTerminada = false;
            timer1.Enabled = true;
            RegistrarInicioAutomatico();
        }

        private void Final_Click(object sender, EventArgs e)
        {
            if (miLista == null || miLista.GetNum() == 0) return;

            inicio = false;
            final = true;
            timer1.Enabled = false;

            // Preguntar confirmación antes de finalizar
            DialogResult resultado = MessageBox.Show(
                "¿Está seguro de que desea finalizar la simulación?\n\n" +
                "Todos los aviones se moverán a sus destinos finales.",
                "Confirmar finalización",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (resultado != DialogResult.Yes)
                return;

            // Registrar cambios de posición final
            for (int i = 0; i < miLista.GetNum(); i++)
            {
                FlightPlan vuelo = miLista.GetFlightPlan(i);
                if (!vuelo.HasArrived())
                {
                    double xAnterior = vuelo.GetCurrentPosition().GetX();
                    double yAnterior = vuelo.GetCurrentPosition().GetY();
                    double xDestino = vuelo.GetFinalPosition().GetX();
                    double yDestino = vuelo.GetFinalPosition().GetY();

                    if (Math.Abs(xAnterior - xDestino) > 0.01 || Math.Abs(yAnterior - yDestino) > 0.01)
                    {
                        var contacto = ObtenerContactoCompania(vuelo.GetCompany());
                        string cambio = $"El avión {vuelo.GetId()} de {vuelo.GetCompany()} movido al destino: " +
                                       $"({xAnterior:F2}, {yAnterior:F2}) → ({xDestino:F2}, {yDestino:F2}). " +
                                       $"Contacto: Tel: {contacto.telefono}, Email: {contacto.email}";
                        AgregarCambio(cambio);
                    }

                    vuelo.SetPosition(xDestino, yDestino);
                }
            }

            // Guardar estado actual antes de mover al final
            GuardarEstadoActual();

            simulacionTerminada = true;
            ActualizarIconos();
            miPanel.Invalidate();

            // Registrar finalización
            AgregarCambio($"Simulación finalizada manualmente. Todos los aviones en destino.");

            MessageBox.Show("Simulación finalizada. Usa 'Deshacer' para volver atrás.",
                          "Finalizado",
                          MessageBoxButtons.OK,
                          MessageBoxIcon.Information);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!inicio || final)
            {
                timer1.Enabled = false;
                return;
            }
            else
            {
                timer1.Interval = 1000;

                try
                {
                    miLista.Mover(tiempoCiclo);
                    GuardarEstadoActual();
                    ActualizarIconos();
                    VerificarConflictoTiempoReal();
                    miPanel.Invalidate();

                    if (miLista.hanLlegadoTodos())
                    {
                        inicio = false;
                        final = true;
                        timer1.Enabled = false;
                        simulacionTerminada = true;

                        // Registrar llegada de todos los aviones
                        AgregarCambio($"Simulación automática completada. Todos los aviones han llegado a destino.");

                        MessageBox.Show("Todos los aviones han llegado a su destino. Usa 'Deshacer' para volver atrás.",
                                      "Simulación completada",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Information);
                    }
                }
                catch (Exception)
                {
                    timer1.Enabled = false;
                    MessageBox.Show("Error durante la simulación automática.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ActualizarIconos()
        {
            if (miLista == null) return;

            int numVuelos = miLista.GetNum();
            if (numVuelos == 0) return;

            if (iconosAviones.Count != numVuelos)
            {
                InicializarIconos();
                return;
            }

            for (int i = 0; i < numVuelos; i++)
            {
                if (i < iconosAviones.Count)
                {
                    try
                    {
                        FlightPlan vuelo = miLista.GetFlightPlan(i);
                        if (vuelo != null)
                        {
                            // Cambiar color si llegó a destino
                            if (vuelo.HasArrived())
                            {
                                iconosAviones[i].BackColor = Color.Gray;
                            }
                            else
                            {
                                int x = Convert.ToInt32(vuelo.GetCurrentPosition().GetX());
                                int y = Convert.ToInt32(vuelo.GetCurrentPosition().GetY());
                                iconosAviones[i].Location = new Point(x, y);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Si hay error, no mover este icono
                    }
                }
            }
        }

        private void MostrarDatosActuales_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            DatosActuales datosActuales = new DatosActuales(miLista);
            datosActuales.PonerDatos();
            datosActuales.ShowDialog();

            if (inicio && !final)
            {
                timer1.Enabled = true;
            }
        }

        private void VerificarConflictoTiempoReal()
        {
            int n = miLista.GetNum();
            if (n < 2)
            {
                vuelosEnConflicto.Clear();
                RestaurarColoresNormales();
                return;
            }

            conflictoActivo = false;
            vuelosEnConflicto.Clear();
            RestaurarColoresNormales();

            for (int i = 0; i < n; i++)
            {
                FlightPlan vueloA = miLista.GetFlightPlan(i);

                for (int j = i + 1; j < n; j++)
                {
                    FlightPlan vueloB = miLista.GetFlightPlan(j);
                    double distanciaCentros = vueloA.Distancia(
                        vueloB.GetCurrentPosition().GetX(),
                        vueloB.GetCurrentPosition().GetY()
                    );

                    if (distanciaCentros < 2 * distanciaSeguridad)
                    {
                        conflictoActivo = true;

                        // Registrar el conflicto
                        RegistrarConflictoDetectado(vueloA, vueloB, distanciaCentros);

                        if (!vuelosEnConflicto.Contains(i))
                            vuelosEnConflicto.Add(i);
                        if (!vuelosEnConflicto.Contains(j))
                            vuelosEnConflicto.Add(j);

                        if (i < iconosAviones.Count)
                            iconosAviones[i].BackColor = Color.Red;
                        if (j < iconosAviones.Count)
                            iconosAviones[j].BackColor = Color.Red;
                    }
                }
            }
        }

        private void RestaurarColoresNormales()
        {
            for (int i = 0; i < iconosAviones.Count; i++)
            {
                FlightPlan vuelo = miLista.GetFlightPlan(i);
                if (vuelo != null && vuelo.HasArrived())
                {
                    iconosAviones[i].BackColor = Color.Gray;
                }
                else
                {
                    iconosAviones[i].BackColor = Color.LightGreen;
                }
            }
        }

        public void SetVelocidad(FlightPlan plan, double v)
        {
            for (int i = 0; i < miLista.GetNum(); i++)
            {
                if (plan.GetId() == miLista.GetFlightPlan(i).GetId())
                {
                    double velocidadAnterior = miLista.GetFlightPlan(i).GetVelocidad();
                    miLista.GetFlightPlan(i).SetVelocidad(v);

                    // Registrar cambio de velocidad
                    if (Math.Abs(velocidadAnterior - v) > 0.01)
                    {
                        var contacto = ObtenerContactoCompania(plan.GetCompany());
                        string cambio = $"Velocidad cambiada para {plan.GetId()} ({plan.GetCompany()}): " +
                                       $"{velocidadAnterior:F2} → {v:F2}. " +
                                       $"Contacto: Tel: {contacto.telefono}, Email: {contacto.email}";
                        AgregarCambio(cambio);
                    }

                    break;
                }
            }
        }

        private void btnCambiarVelocidadesDeLosVuelos_Click(object sender, EventArgs e)
        {
            CambiarVelocidades Cambiar = new CambiarVelocidades(miLista);
            Cambiar.ShowDialog();

            FlightPlanList velocidadescambiadas = Cambiar.cambioVelocidad;
            for (int i = 0; i < velocidadescambiadas.GetNum(); i++)
            {
                var vueloOriginal = miLista.GetFlightPlan(i);
                var vueloModificado = velocidadescambiadas.GetFlightPlan(i);

                if (vueloModificado.GetId() == vueloOriginal.GetId())
                {
                    double velocidadAnterior = vueloOriginal.GetVelocidad();
                    double velocidadNueva = vueloModificado.GetVelocidad();

                    if (Math.Abs(velocidadAnterior - velocidadNueva) > 0.01)
                    {
                        miLista.GetFlightPlan(i).SetVelocidad(velocidadNueva);

                        // Registrar el cambio
                        var contacto = ObtenerContactoCompania(vueloOriginal.GetCompany());
                        string cambio = $"El avión {vueloOriginal.GetId()} de {vueloOriginal.GetCompany()} " +
                                      $"cambió velocidad de {velocidadAnterior:F2} a {velocidadNueva:F2}. " +
                                      $"Contacto: Tel: {contacto.telefono}, Email: {contacto.email}";
                        AgregarCambio(cambio);
                    }
                }
            }
        }

        private void btnReiniciar_Click(object sender, EventArgs e)
        {
            if (miLista == null || miLista.GetNum() == 0) return;

            // Preguntar confirmación antes de reiniciar
            DialogResult resultado = MessageBox.Show(
                "¿Está seguro de que desea reiniciar la simulación?\n\n" +
                "Todos los aviones volverán a sus posiciones iniciales y se perderá el progreso actual.",
                "Confirmar reinicio",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (resultado != DialogResult.Yes)
                return;

            // Registrar reinicio
            AgregarCambio("Simulación reiniciada a posiciones iniciales.");

            for (int i = 0; i < miLista.GetNum(); i++)
            {
                var vuelo = miLista.GetFlightPlan(i);
                double xAnterior = vuelo.GetCurrentPosition().GetX();
                double yAnterior = vuelo.GetCurrentPosition().GetY();
                double xInicial = vuelo.GetInitialPosition().GetX();
                double yInicial = vuelo.GetInitialPosition().GetY();

                if (Math.Abs(xAnterior - xInicial) > 0.01 || Math.Abs(yAnterior - yInicial) > 0.01)
                {
                    var contacto = ObtenerContactoCompania(vuelo.GetCompany());
                    string cambio = $"Avión {vuelo.GetId()} ({vuelo.GetCompany()}) reiniciado: " +
                                   $"({xAnterior:F2}, {yAnterior:F2}) → ({xInicial:F2}, {yInicial:F2}). " +
                                   $"Contacto: Tel: {contacto.telefono}, Email: {contacto.email}";
                    AgregarCambio(cambio);
                }

                miLista.GetFlightPlan(i).SetPosition(xInicial, yInicial);
            }

            inicio = false;
            final = false;
            simulacionTerminada = false;
            conflictoActivo = false;
            vuelosEnConflicto.Clear();

            // Limpiar el stack y guardar estado inicial
            posiciones.Clear();
            GuardarEstadoInicial();

            RestaurarColoresNormales();
            miPanel.Invalidate();

            MessageBox.Show("Simulación reiniciada exitosamente.",
                          "Reinicio completado",
                          MessageBoxButtons.OK,
                          MessageBoxIcon.Information);
        }

        private void deshacerBtn_Click(object sender, EventArgs e)
        {
            if (miLista == null || miLista.GetNum() == 0) return;

            if (posiciones.Count == 0)
            {
                MessageBox.Show("Ya estás en la posición inicial. No se puede deshacer más.");
                return;
            }

            // Preguntar confirmación antes de deshacer
            DialogResult resultado = MessageBox.Show(
                "¿Está seguro de que desea deshacer la última acción?\n\n" +
                "Los aviones volverán a su estado anterior.",
                "Confirmar deshacer",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (resultado != DialogResult.Yes)
                return;

            Point[] estadoAnterior = posiciones.Pop();

            // Registrar los cambios de posición
            for (int i = 0; i < miLista.GetNum(); i++)
            {
                var vuelo = miLista.GetFlightPlan(i);
                double xAnterior = vuelo.GetCurrentPosition().GetX();
                double yAnterior = vuelo.GetCurrentPosition().GetY();

                miLista.GetFlightPlan(i).SetPosition(estadoAnterior[i].X, estadoAnterior[i].Y);

                double xNuevo = vuelo.GetCurrentPosition().GetX();
                double yNuevo = vuelo.GetCurrentPosition().GetY();

                if (Math.Abs(xAnterior - xNuevo) > 0.01 || Math.Abs(yAnterior - yNuevo) > 0.01)
                {
                    var contacto = ObtenerContactoCompania(vuelo.GetCompany());
                    string cambio = $"Acción deshecha para {vuelo.GetId()} ({vuelo.GetCompany()}): " +
                                   $"({xAnterior:F2}, {yAnterior:F2}) → ({xNuevo:F2}, {yNuevo:F2}). " +
                                   $"Contacto: Tel: {contacto.telefono}, Email: {contacto.email}";
                    AgregarCambio(cambio);
                }
            }

            // Si se deshace desde el estado final, reactivar la simulación
            if (simulacionTerminada)
            {
                simulacionTerminada = false;
                inicio = false;
                final = false;
                RestaurarColoresNormales();
            }

            ActualizarIconos();
            VerificarConflictoTiempoReal();
            miPanel.Invalidate();

            if (posiciones.Count == 0)
            {
                MessageBox.Show("Estado inicial restaurado exitosamente.",
                              "Restauración completada",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Acción deshecha exitosamente.",
                              "Deshacer completado",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Information);
            }
        }

        private void mostrarDistanciasTxt_Click(object sender, EventArgs e)
        {
            DistanciasCompletas formDistancias = new DistanciasCompletas(miLista);
            formDistancias.ShowDialog();
        }

        private void mostrarVuelosBtn_Click(object sender, EventArgs e)
        {
            ListaVuelos listaVuelos = new ListaVuelos(miLista);
            listaVuelos.ShowDialog();
        }

        public int numeroArchivosGuardado = 0;

        private void exportarListaBtn_Click(object sender, EventArgs e)
        {
            if (miLista == null || miLista.GetNum() == 0)
            {
                MessageBox.Show("No hay vuelos para exportar.", "Lista vacía", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Ruta dentro de la carpeta del proyecto
            string proyectoPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\"));

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Archivos de texto (*.txt)|*.txt";
            saveDialog.Title = "Guardar lista de vuelos";
            saveDialog.FileName = $"vuelos_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

            // Establecer la ruta inicial en la carpeta del proyecto
            saveDialog.InitialDirectory = proyectoPath;

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                // Preguntar confirmación antes de guardar
                DialogResult confirmacion = MessageBox.Show(
                    $"¿Está seguro de que desea guardar la lista de vuelos en:\n{saveDialog.FileName}?\n\n" +
                    $"Se guardarán {miLista.GetNum()} vuelos.",
                    "Confirmar guardado",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmacion != DialogResult.Yes)
                    return;

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

                    // Mostrar mensaje de confirmación con detalles
                    string mensajeConfirmacion = $"✅ Archivo guardado exitosamente\n\n" +
                                                $"📁 Ubicación: {rutaRelativa}\n" +
                                                $"📊 Vuelos guardados: {miLista.GetNum()}\n" +
                                                $"📄 Tipo: Archivo de texto (.txt)\n" +
                                                $"⏰ Fecha: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";

                    MessageBox.Show(mensajeConfirmacion,
                                  "✅ Guardado exitoso",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Information);

                    numeroArchivosGuardado++;

                    // Registrar exportación
                    AgregarCambio($"Lista de vuelos exportada exitosamente a: {rutaRelativa} ({miLista.GetNum()} vuelos)");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Error al guardar el archivo:\n{ex.Message}",
                                  "Error de guardado",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error);
                }
            }
        }

        // ============================ MÉTODOS PARA INFORMES ============================

        private void AgregarCambio(string cambio)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            cambiosRealizados.Add($"[{timestamp}] {cambio}");
        }

        private (string telefono, string email) ObtenerContactoCompania(string nombreCompania)
        {
            try
            {
                Compañias compania = companyManager.GetCompany(nombreCompania);
                if (compania != null)
                {
                    return (compania.GetTelefono(), compania.GetEmail());
                }
            }
            catch (Exception)
            {
                // Si hay error, devolver valores por defecto
            }

            return ("No disponible", "No disponible");
        }

        private void RegistrarInicioSimulacion()
        {
            if (miLista == null || miLista.GetNum() == 0) return;

            AgregarCambio("=== INICIO DE SIMULACIÓN ===");
            AgregarCambio($"Parámetros: Distancia seguridad={distanciaSeguridad:F2}, Tiempo ciclo={tiempoCiclo}min");
            AgregarCambio($"Número de vuelos: {miLista.GetNum()}");

            // Registrar información de cada vuelo
            for (int i = 0; i < miLista.GetNum(); i++)
            {
                var vuelo = miLista.GetFlightPlan(i);
                if (vuelo != null)
                {
                    var contacto = ObtenerContactoCompania(vuelo.GetCompany());
                    AgregarCambio($"Vuelo {i + 1}: {vuelo.GetId()} ({vuelo.GetCompany()}) - " +
                                 $"Origen: ({vuelo.GetInitialPosition().GetX():F2}, {vuelo.GetInitialPosition().GetY():F2}), " +
                                 $"Destino: ({vuelo.GetFinalPosition().GetX():F2}, {vuelo.GetFinalPosition().GetY():F2}), " +
                                 $"Velocidad: {vuelo.GetVelocidad():F2}, " +
                                 $"Contacto: {contacto.telefono}/{contacto.email}");
                }
            }
        }

        private void RegistrarMovimientoCiclo()
        {
            AgregarCambio($"Movimiento de ciclo ejecutado ({tiempoCiclo} minutos)");
        }

        private void RegistrarInicioAutomatico()
        {
            AgregarCambio("Simulación automática iniciada");
        }

        private void RegistrarConflictoDetectado(FlightPlan vueloA, FlightPlan vueloB, double distancia)
        {
            var contactoA = ObtenerContactoCompania(vueloA.GetCompany());
            var contactoB = ObtenerContactoCompania(vueloB.GetCompany());

            string conflicto = $"CONFLICTO DETECTADO: {vueloA.GetId()} ({vueloA.GetCompany()}) y " +
                             $"{vueloB.GetId()} ({vueloB.GetCompany()}) a distancia {distancia:F2}. " +
                             $"Contactos: {contactoA.telefono}/{contactoA.email} y {contactoB.telefono}/{contactoB.email}";

            AgregarCambio(conflicto);
        }

        // ============================ EVENTO DEL BOTÓN INFORMES ============================

        private void informesBtn_Click(object sender, EventArgs e)
        {
            // Este método ya está implementado, pero hay que corregir el nombre del método
            // El método informesBtn_Click_1 debería ser informesBtn_Click
            // Pero ya existe el método correcto arriba
            // Eliminamos el método duplicado y dejamos solo uno
        }

        private void informesBtn_Click_1(object sender, EventArgs e)
        {
            // ESTE ES EL MÉTODO QUE SE EJECUTA AL DAR CLICK EN EL BOTÓN INFORMES
            if (cambiosRealizados.Count == 0)
            {
                MessageBox.Show("No se han realizado cambios en la simulación.", "Sin cambios",
                               MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Ruta dentro de la carpeta del proyecto
            string proyectoPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\"));

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Archivos de texto (*.txt)|*.txt|Archivos PDF (*.pdf)|*.pdf";
            saveDialog.Title = "Guardar informe de cambios";
            saveDialog.FileName = $"informe_cambios_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            saveDialog.InitialDirectory = proyectoPath;

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                // Preguntar confirmación antes de guardar
                DialogResult confirmacion = MessageBox.Show(
                    $"¿Está seguro de que desea guardar el informe de cambios en:\n{saveDialog.FileName}?\n\n" +
                    $"📊 Cambios registrados: {cambiosRealizados.Count}\n" +
                    $"✈️ Vuelos en simulación: {miLista.GetNum()}\n" +
                    $"📄 Formato: {Path.GetExtension(saveDialog.FileName).ToUpper()}",
                    "Confirmar guardado de informe",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmacion != DialogResult.Yes)
                    return;

                try
                {
                    using (StreamWriter archivo = new StreamWriter(saveDialog.FileName, false, Encoding.UTF8))
                    {
                        // Encabezado del informe
                        archivo.WriteLine("=".PadRight(80, '='));
                        archivo.WriteLine($"INFORME DE CAMBIOS - SIMULACIÓN DE VUELOS");
                        archivo.WriteLine($"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                        archivo.WriteLine($"Simulación ID: {DateTime.Now.Ticks}");
                        archivo.WriteLine($"Total de cambios registrados: {cambiosRealizados.Count}");
                        archivo.WriteLine("=".PadRight(80, '='));
                        archivo.WriteLine();

                        // Información de la simulación
                        archivo.WriteLine("INFORMACIÓN DE LA SIMULACIÓN:");
                        archivo.WriteLine("-".PadRight(50, '-'));
                        archivo.WriteLine($"Distancia de seguridad: {distanciaSeguridad:F2}");
                        archivo.WriteLine($"Tiempo por ciclo: {tiempoCiclo} minutos");
                        archivo.WriteLine($"Número de vuelos: {miLista.GetNum()}");
                        archivo.WriteLine($"Estado actual: {(simulacionTerminada ? "Terminada" : "En curso")}");
                        archivo.WriteLine();

                        // Lista de vuelos con información de contacto
                        archivo.WriteLine("LISTA DE VUELOS EN SIMULACIÓN:");
                        archivo.WriteLine("-".PadRight(50, '-'));
                        for (int i = 0; i < miLista.GetNum(); i++)
                        {
                            var vuelo = miLista.GetFlightPlan(i);
                            if (vuelo != null)
                            {
                                var contacto = ObtenerContactoCompania(vuelo.GetCompany());
                                string estado = vuelo.HasArrived() ? "LLEGADO" : "EN RUTA";

                                archivo.WriteLine($"Vuelo {i + 1}: {vuelo.GetId()}");
                                archivo.WriteLine($"  Compañía: {vuelo.GetCompany()}");
                                archivo.WriteLine($"  Estado: {estado}");
                                archivo.WriteLine($"  Posición actual: ({vuelo.GetCurrentPosition().GetX():F2}, {vuelo.GetCurrentPosition().GetY():F2})");
                                archivo.WriteLine($"  Destino: ({vuelo.GetFinalPosition().GetX():F2}, {vuelo.GetFinalPosition().GetY():F2})");
                                archivo.WriteLine($"  Velocidad actual: {vuelo.GetVelocidad():F2}");
                                archivo.WriteLine($"  Teléfono compañía: {contacto.telefono}");
                                archivo.WriteLine($"  Email compañía: {contacto.email}");
                                archivo.WriteLine();
                            }
                        }

                        // Historial de cambios
                        archivo.WriteLine("HISTORIAL DETALLADO DE CAMBIOS:");
                        archivo.WriteLine("-".PadRight(50, '-'));
                        for (int i = 0; i < cambiosRealizados.Count; i++)
                        {
                            archivo.WriteLine($"{i + 1:000}. {cambiosRealizados[i]}");
                        }
                        archivo.WriteLine();

                        // Resumen estadístico
                        archivo.WriteLine("RESUMEN ESTADÍSTICO:");
                        archivo.WriteLine("-".PadRight(50, '-'));
                        archivo.WriteLine($"• Total de cambios registrados: {cambiosRealizados.Count}");

                        // Contar tipos de cambios
                        int cambiosVelocidad = cambiosRealizados.Count(c =>
                            c.Contains("velocidad", StringComparison.OrdinalIgnoreCase) ||
                            c.Contains("cambió velocidad", StringComparison.OrdinalIgnoreCase));

                        int cambiosPosicion = cambiosRealizados.Count(c =>
                            c.Contains("movido", StringComparison.OrdinalIgnoreCase) ||
                            c.Contains("reiniciado", StringComparison.OrdinalIgnoreCase) ||
                            c.Contains("deshecha", StringComparison.OrdinalIgnoreCase) ||
                            (c.Contains("→") && c.Contains("Contacto:")));

                        int conflictos = cambiosRealizados.Count(c =>
                            c.Contains("CONFLICTO", StringComparison.OrdinalIgnoreCase));

                        int otros = cambiosRealizados.Count - cambiosVelocidad - cambiosPosicion - conflictos;

                        archivo.WriteLine($"• Cambios de velocidad: {cambiosVelocidad}");
                        archivo.WriteLine($"• Cambios de posición: {cambiosPosicion}");
                        archivo.WriteLine($"• Conflictos detectados: {conflictos}");
                        archivo.WriteLine($"• Otros eventos: {otros}");
                        archivo.WriteLine();

                        // Vuelos que han llegado
                        int vuelosLlegados = 0;
                        for (int i = 0; i < miLista.GetNum(); i++)
                        {
                            if (miLista.GetFlightPlan(i).HasArrived())
                                vuelosLlegados++;
                        }
                        archivo.WriteLine($"• Vuelos que han llegado a destino: {vuelosLlegados}/{miLista.GetNum()}");
                        archivo.WriteLine($"• Vuelos en ruta: {miLista.GetNum() - vuelosLlegados}/{miLista.GetNum()}");
                        archivo.WriteLine();

                        // Contactos de todas las compañías involucradas
                        archivo.WriteLine("CONTACTOS DE COMPAÑÍAS INVOLUCRADAS:");
                        archivo.WriteLine("-".PadRight(50, '-'));
                        var companiasUnicas = new HashSet<string>();
                        for (int i = 0; i < miLista.GetNum(); i++)
                        {
                            companiasUnicas.Add(miLista.GetFlightPlan(i).GetCompany());
                        }

                        foreach (var compania in companiasUnicas)
                        {
                            var contacto = ObtenerContactoCompania(compania);
                            archivo.WriteLine($"{compania}:");
                            archivo.WriteLine($"  Teléfono: {contacto.telefono}");
                            archivo.WriteLine($"  Email: {contacto.email}");
                        }
                        archivo.WriteLine();

                        // Pie del informe
                        archivo.WriteLine("=".PadRight(80, '='));
                        archivo.WriteLine("FIN DEL INFORME");
                        archivo.WriteLine("Este informe fue generado automáticamente por el sistema de simulación de vuelos.");
                        archivo.WriteLine($"Archivo generado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                        archivo.WriteLine("=".PadRight(80, '='));
                    }

                    // Mostrar ruta relativa al proyecto
                    string rutaRelativa = Path.GetRelativePath(proyectoPath, saveDialog.FileName);

                    // Mostrar mensaje de confirmación con detalles
                    string extension = Path.GetExtension(saveDialog.FileName).ToUpper();
                    long fileSize = 0;
                    try
                    {
                        fileSize = new FileInfo(saveDialog.FileName).Length / 1024;
                    }
                    catch { }

                    string mensajeConfirmacion = $"✅ INFORME GUARDADO EXITOSAMENTE\n\n" +
                                                $"📁 Ubicación: {rutaRelativa}\n" +
                                                $"📊 Cambios registrados: {cambiosRealizados.Count}\n" +
                                                $"✈️ Vuelos en simulación: {miLista.GetNum()}\n" +
                                                $"📄 Formato: {extension}\n" +
                                                $"📏 Tamaño: {fileSize} KB\n" +
                                                $"⏰ Fecha: {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n\n" +
                                                $"El informe contiene todos los cambios realizados durante la simulación, incluyendo información de contacto de las compañías.";

                    MessageBox.Show(mensajeConfirmacion,
                                  "✅ Informe guardado exitosamente",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Information);

                    // Registrar la generación del informe
                    AgregarCambio($"Informe de cambios generado y guardado exitosamente en: {rutaRelativa} ({cambiosRealizados.Count} cambios)");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Error al guardar el informe:\n{ex.Message}",
                                  "Error de guardado",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error);
                }
            }
        }

        private void btnVerificarConflicto_Click(object sender, EventArgs e)
        {
            // Verificar conflicto manualmente
            VerificarConflictoTiempoReal();

            if (conflictoActivo)
            {
                MessageBox.Show($"Se detectaron {vuelosEnConflicto.Count} aviones en conflicto.",
                              "Conflicto detectado",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show("No se detectaron conflictos entre los aviones.",
                              "Sin conflictos",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Information);
            }
        }

        private void FormSimulacion_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Preguntar confirmación antes de cerrar si hay simulación en curso
            if (inicio || !simulacionTerminada)
            {
                DialogResult resultado = MessageBox.Show(
                    "¿Está seguro de que desea cerrar la simulación?\n\n" +
                    "Se perderá el progreso no guardado.",
                    "Confirmar cierre",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (resultado != DialogResult.Yes)
                {
                    e.Cancel = true;
                }
                else
                {
                    // Registrar cierre de simulación
                    AgregarCambio("Simulación cerrada por el usuario.");
                }
            }
        }
    }
}
