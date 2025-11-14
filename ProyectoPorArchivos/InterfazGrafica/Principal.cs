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
                MessageBox.Show("Primero carga los datos de vuelos");
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
        // Método para resolver conflictos automáticamente ajustando velocidades
        private bool ResolverConflictoAutomatico(FlightPlan vuelo1, FlightPlan vuelo2, double distanciaSeguridad, double tiempoMaximo)
        {
            double velocidadOriginalVuelo1 = vuelo1.GetVelocidad();
            double velocidadOriginalVuelo2 = vuelo2.GetVelocidad();

            Console.WriteLine($"Velocidad original V1: {velocidadOriginalVuelo1}");
            Console.WriteLine($"Velocidad original V2: {velocidadOriginalVuelo2}");

            // Más factores de velocidad para mayor probabilidad de éxito
            double[] factoresVelocidad = { 0.5, 0.6, 0.7, 0.8, 0.9, 1.1, 1.2, 1.3, 1.5, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0};

            // ESTRATEGIA 1: Vuelo 1 más lento
            foreach (double factor in factoresVelocidad)
            {
                if (factor < 1.0) // Solo hacer más lento
                {
                    vuelo1.SetVelocidad(velocidadOriginalVuelo1 * factor);
                    if (!vuelo1.EntraraEnConflicto(vuelo2, distanciaSeguridad, tiempoMaximo))
                    {
                        return true;
                    }
                }
            }

            // ESTRATEGIA 2: Vuelo 1 más rápido
            foreach (double factor in factoresVelocidad)
            {
                if (factor > 1.0) // Solo hacer más rápido
                {
                    vuelo1.SetVelocidad(velocidadOriginalVuelo1 * factor);
                    if (!vuelo1.EntraraEnConflicto(vuelo2, distanciaSeguridad, tiempoMaximo))
                    {
                        return true;
                    }
                }
            }

            // ESTRATEGIA 3: Vuelo 2 más lento
            vuelo1.SetVelocidad(velocidadOriginalVuelo1);
            foreach (double factor in factoresVelocidad)
            {
                if (factor < 1.0)
                {
                    vuelo2.SetVelocidad(velocidadOriginalVuelo2 * factor);
                    if (!vuelo1.EntraraEnConflicto(vuelo2, distanciaSeguridad, tiempoMaximo))
                    {
                        return true;
                    }
                }
            }

            // ESTRATEGIA 4: Vuelo 2 más rápido
            foreach (double factor in factoresVelocidad)
            {
                if (factor > 1.0)
                {
                    vuelo2.SetVelocidad(velocidadOriginalVuelo2 * factor);
                    if (!vuelo1.EntraraEnConflicto(vuelo2, distanciaSeguridad, tiempoMaximo))
                    {
                        return true;
                    }
                }
            }

            // Restaurar si no se pudo resolver
            vuelo1.SetVelocidad(velocidadOriginalVuelo1);
            vuelo2.SetVelocidad(velocidadOriginalVuelo2);

            Console.WriteLine("No se pudo resolver automáticamente");
            return false;
        }
        private bool ResolverConflictoInstantaneo(FlightPlan vuelo1, FlightPlan vuelo2, double distanciaSeguridad, double tiempoMaximo)
        {
            double v1Original = vuelo1.GetVelocidad();
            double v2Original = vuelo2.GetVelocidad();


            // Estrategias matemáticas instantáneas
            (double, double)[] estrategias = {
        (v1Original * 0.3, v2Original),      // V1 muy lento
        (v1Original * 0.5, v2Original),      // V1 lento
        (v1Original * 0.7, v2Original),      // V1 moderado
        (v1Original * 1.5, v2Original),      // V1 rápido
        (v1Original * 2.0, v2Original),      // V1 muy rápido
        (v1Original, v2Original * 0.3),      // V2 muy lento
        (v1Original, v2Original * 0.5),      // V2 lento
        (v1Original, v2Original * 0.7),      // V2 moderado
        (v1Original, v2Original * 1.5),      // V2 rápido
        (v1Original, v2Original * 2.0),      // V2 muy rápido
        (v1Original * 0.8, v2Original * 1.2), // Combinación 1
        (v1Original * 1.2, v2Original * 0.8), // Combinación 2
        (v1Original * 0.6, v2Original * 0.6), // Ambos lentos
        (v1Original * 1.4, v2Original * 1.4)  // Ambos rápidos
    };

            for (int i = 0; i < estrategias.Length; i++)
            {
                vuelo1.SetVelocidad(estrategias[i].Item1);
                vuelo2.SetVelocidad(estrategias[i].Item2);

                // Verificación MATEMÁTICA instantánea
                if (!vuelo1.EntraraEnConflicto(vuelo2, distanciaSeguridad, tiempoMaximo))
                {
                    MessageBox.Show(
                        $"Nuevas velocidades:\n" +
                        $"Vuelo 1: {estrategias[i].Item1:F1} (original: {v1Original:F1})\n" +
                        $"Vuelo 2: {estrategias[i].Item2:F1} (original: {v2Original:F1})",
                        "Resolución Exitosa",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    return true;
                }
            }
            

            // Restaurar si no se pudo resolver
            vuelo1.SetVelocidad(v1Original);
            vuelo2.SetVelocidad(v2Original);

            MessageBox.Show(
                "No se pudo resolver automáticamente\n" +
                "Ajuste manualmente las rutas o velocidades",
                "No Se Pudo Resolver",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
            return false;
        }
    }
}
