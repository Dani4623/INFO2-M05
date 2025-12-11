using FlightLib;
using System;
using System.Windows.Forms;

namespace InterfazGrafica
{
    public partial class Principal : Form
    {
        private FlightPlan Vuelo1;
        private FlightPlan Vuelo2;
        private double distanciaSeguridad;
        private int tiempoCiclo;
        public Principal()
        {
            InitializeComponent();
        }

        private void cargarListaDeVuelosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DatosVuelos formDatos = new DatosVuelos();
            if (formDatos.ShowDialog() == DialogResult.OK)
            {
                Vuelo1 = formDatos.Vuelo1;
                Vuelo2 = formDatos.Vuelo2;

                MessageBox.Show("Vuelos cargados correctamente");
            }
        }

        private void distanciaDeSeguridadYTiempoDeCicloToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Configuracion formConfig = new Configuracion();
            if (formConfig.ShowDialog() == DialogResult.OK)
            {
                // Usar los nuevos nombres de propiedades
                distanciaSeguridad = formConfig.DistanciaSeguridadValor;
                tiempoCiclo = (int)formConfig.TiempoCicloValor;
                MessageBox.Show("Configuración guardada");
            }
        }


        private void simulaciónToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Vuelo1 == null || Vuelo2 == null)
            {
                MessageBox.Show("Carga los datos de vuelos");
                return;
            }

            // Crear lista
            FlightPlanList lista = new FlightPlanList();
            lista.AddFlightPlan(Vuelo1);
            lista.AddFlightPlan(Vuelo2);

            // CÁLCULO INSTANTÁNEO - FASE 11
            
            double tiempoMaximo = 120; // 2 horas
            bool conflictoFuturo = Vuelo1.EntraraEnConflicto(Vuelo2, distanciaSeguridad, tiempoMaximo);
            double? tiempoConflicto = Vuelo1.TiempoHastaConflicto(Vuelo2, distanciaSeguridad, tiempoMaximo);

            // Verificar también conflicto inmediato
            double distanciaActual = Vuelo1.Distancia(
                Vuelo2.GetCurrentPosition().GetX(),
                Vuelo2.GetCurrentPosition().GetY()
            );

            bool conflictoInmediato = distanciaActual < (2 * distanciaSeguridad);

            if (conflictoInmediato)
            {
                DialogResult resultado = MessageBox.Show(
                    $"CONFLICTO INMEDIATO\n\n" +
                    $"Los aviones ya están en conflicto.\n" +
                    $"Distancia actual: {distanciaActual:F0}\n" +
                    $"Umbral seguridad: {2 * distanciaSeguridad:F0}\n\n" +
                    $"¿Resolver automáticamente?",
                    "Conflicto Detectado",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error
                );

                if (resultado == DialogResult.Yes)
                {
                    ResolverConflictoInstantaneo(Vuelo1, Vuelo2, distanciaSeguridad, tiempoMaximo);
                }
            }
            else if (conflictoFuturo && tiempoConflicto.HasValue)
            {
                DialogResult resultado = MessageBox.Show(
                    $"CONFLICTO FUTURO\n\n" +
                    $"Tiempo hasta conflicto: {tiempoConflicto.Value:F1} minutos\n" +
                    $"Distancia seguridad: {distanciaSeguridad}\n\n" +
                    $"¿Resolver automáticamente?",
                    "Conflicto Futuro Detectado",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (resultado == DialogResult.Yes)
                {
                    ResolverConflictoInstantaneo(Vuelo1, Vuelo2, distanciaSeguridad, tiempoMaximo);
                }
            }
            else
            {
                MessageBox.Show(
                    $"SIN CONFLICTOS\n\n" +
                    $"Distancia actual: {distanciaActual:F0}\n" +
                    $"No se detectaron conflictos futuros",
                    "Simulación Segura",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }

            // Iniciar simulación
            FormSimulacion formSimulacion = new FormSimulacion(lista, tiempoCiclo, distanciaSeguridad);
            formSimulacion.SetVelocidad(Vuelo1.GetVelocidad(), Vuelo2.GetVelocidad());
            formSimulacion.SetPlanes(Vuelo1,Vuelo2);
            formSimulacion.Show();
        }







        private void Principal_Load(object sender, EventArgs e)
        {

        }

        //ResolverConflictoInstantaneo
        private bool ResolverConflictoInstantaneo(FlightPlan vuelo1, FlightPlan vuelo2, double distanciaSeguridad, double tiempoMaximo)
        {
            double v1Original = vuelo1.GetVelocidad();
            double v2Original = vuelo2.GetVelocidad();



            // ESTRATEGIA 1: Reducir velocidad del vuelo 1 de unidad en unidad
            for (double v1Nueva = v1Original - 1; v1Nueva >= 1; v1Nueva -= 1)
            {
                vuelo1.SetVelocidad(v1Nueva);

                //Comprueba si sigue habiendo conflicto, si no lo hay , retornará true
                bool hayConflicto = vuelo1.EntraraEnConflicto(vuelo2, distanciaSeguridad, tiempoMaximo);
                if (hayConflicto == false)
                {
                    MessageBox.Show(
                        $"Conflicto resuelto reduciendo velocidad Vuelo 1\n" +
                        $"Nueva velocidad Vuelo 1: {v1Nueva} (original: {v1Original})\n" +
                        $"Velocidad Vuelo 2: {v2Original}");
                    return true;
                }
            }
            // Restaurar velocidad original del vuelo 1
            vuelo1.SetVelocidad(v1Original);



            // ESTRATEGIA 2: Reducir velocidad del vuelo 2 de unidad en unidad
            for (double v2Nueva = v2Original - 1; v2Nueva >= 1; v2Nueva -= 1)
            {
                vuelo2.SetVelocidad(v2Nueva);

                //Comprueba si sigue habiendo conflicto, si no lo hay , retornará true
                bool hayConflicto = vuelo1.EntraraEnConflicto(vuelo2, distanciaSeguridad, tiempoMaximo);
                if (hayConflicto == false)
                {
                    MessageBox.Show(
                        $"Conflicto resuelto reduciendo velocidad Vuelo 2\n" +
                        $"Velocidad Vuelo 1: {v1Original}\n" +
                        $"Nueva velocidad Vuelo 2: {v2Nueva} (original: {v2Original})");
                    return true;
                }
            }
            // Restaurar velocidad original del vuelo 2
            vuelo2.SetVelocidad(v2Original);



            // ESTRATEGIA 3: Combinación - reducir vuelo 1 y aumentar vuelo 2
            for (double v1Nueva = v1Original - 1; v1Nueva >= 1; v1Nueva -= 1)
            {
                for (double v2Nueva = v2Original + 1; v2Nueva <= v2Original + 20; v2Nueva += 1)
                {
                    vuelo1.SetVelocidad(v1Nueva);
                    vuelo2.SetVelocidad(v2Nueva);

                    bool hayConflicto = vuelo1.EntraraEnConflicto(vuelo2, distanciaSeguridad, tiempoMaximo);
                    if (hayConflicto == false)
                    {
                        MessageBox.Show(
                            $"Conflicto resuelto con combinación\n" +
                            $"Nueva velocidad Vuelo 1: {v1Nueva:F1} (original: {v1Original:F1})\n" +
                            $"Nueva velocidad Vuelo 2: {v2Nueva:F1} (original: {v2Original:F1})");
                        return true;
                    }
                }
            }
            // Si no se ha resuelto, establece las velocidades originales
            vuelo1.SetVelocidad(v1Original);
            vuelo2.SetVelocidad(v2Original);

            MessageBox.Show("Conflicto no resuelto");



            //Como es un bool y no se ha resuelto, retorna fase
            return false;
        }


    }
}









    private void ExportarPDF_Click(object sender, EventArgs e)
    {
        if (listavuelos == null || listavuelos.GetNum() == 0)
        {
            MessageBox.Show("No hay vuelos cargados para exportar.", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        SaveFileDialog saveDialog = new SaveFileDialog();
        saveDialog.Filter = "Archivos PDF (*.pdf)|*.pdf";
        saveDialog.Title = "Exportar informe a PDF";
        saveDialog.FileName = $"informe_vuelos_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

        if (saveDialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                // Usar ImagePdfExporter que no tiene problemas con fuentes
                ImagePdfExporter.ExportToPDF(listavuelos, companyManager, saveDialog.FileName);

                MessageBox.Show($"Informe exportado exitosamente a:\n{saveDialog.FileName}",
                    "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Opcional: abrir el PDF automáticamente
                try
                {
                    System.Diagnostics.Process.Start(saveDialog.FileName);
                }
                catch
                {
                    // Si no se puede abrir, no hacer nada
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar PDF: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
}
