using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlightLib;

namespace SimulatorConsole
{
    public class Program
    {
        
        static void Main(string[] args)
        {

            FlightPlanList lista = new FlightPlanList();

            //DATOS DEL PRIMER VUELO
            try
            {
                Console.WriteLine("Escribe el identificador del primer vuelo:");
                //   string nombre = Console.ReadLine();
                string identificador = Console.ReadLine(); ;

                Console.WriteLine("Escribe la velocidad del primer vuelo:");
                double velocidad = Convert.ToDouble(Console.ReadLine());

                Console.WriteLine("Escribe las coordenadas de la posición inicial del primer vuelo, separadas por un blanco");
                string linea = Console.ReadLine();
                string[] trozos = linea.Split(' ');
                double ix = Convert.ToDouble(trozos[0]);
                double iy = Convert.ToDouble(trozos[1]);

                Console.WriteLine("Escribe las coordenadas de la posición final, separadas por un blanco");
                linea = Console.ReadLine();
                trozos = linea.Split(' ');
                double fx = Convert.ToDouble(trozos[0]);
                double fy = Convert.ToDouble(trozos[1]);

                FlightPlan plan_a = new FlightPlan(identificador, ix, iy, fx, fy, velocidad);

                Console.WriteLine("Escribe el identificador del segundo vuelo:");
                //   string nombre = Console.ReadLine();
                identificador = Console.ReadLine(); ;

                Console.WriteLine("Escribe la velocidad del segundo vuelo:");
                velocidad = Convert.ToDouble(Console.ReadLine());

                Console.WriteLine("Escribe las coordenadas de la posición inicial del segundo, separadas por un blanco");
                linea = Console.ReadLine();
                trozos = linea.Split(' ');
                ix = Convert.ToDouble(trozos[0]);
                iy = Convert.ToDouble(trozos[1]);

                Console.WriteLine("Escribe las coordenadas de la posición final, separadas por un blanco");
                linea = Console.ReadLine();
                trozos = linea.Split(' ');
                fx = Convert.ToDouble(trozos[0]);
                fy = Convert.ToDouble(trozos[1]);

                FlightPlan plan_b = new FlightPlan(identificador, ix, iy, fx, fy, velocidad);

                lista.AddFlightPlan(plan_a);
                lista.AddFlightPlan(plan_b);

                int i = 0;
                int ciclos = 100;
                int tiempoCiclo = 10;
                double distanciaSeguridad = 10;
                bool fin = false;
                while (i < ciclos)
                {
                    lista.GetFlightPlan(0).Mover(tiempoCiclo);
                    lista.GetFlightPlan(1).Mover(tiempoCiclo);
                    lista.GetFlightPlan(0).EscribeConsola();
                    lista.GetFlightPlan(1).EscribeConsola();
                    if (lista.GetFlightPlan(0).Conflicto(plan_b, distanciaSeguridad))
                    {
                        Console.WriteLine("Conflicto!!");
                    }
                    i = i + 1;
                }
                    
                Console.ReadLine();


            } 
            catch (FormatException)
            {
                Console.WriteLine("Error de formato.");
                Console.ReadLine();
            }

            

            
        }
    }
}
