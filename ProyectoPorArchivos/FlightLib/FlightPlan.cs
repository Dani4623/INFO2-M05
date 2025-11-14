using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace FlightLib
{
    public class FlightPlan
    {
        // Atributos
        string id; // identificador
        public Position initialPosition; // posición inicial
        public Position currentPosition; // posicion actual
        public Position finalPosition; // posicion final
        double velocidad;

        // Constructur flight plan
        public FlightPlan(string id, double cpx, double cpy, double fpx, double fpy, double velocidad)
        {
            this.id = id;
            this.currentPosition = new Position(cpx, cpy);
            this.finalPosition = new Position(fpx, fpy);
            this.velocidad = velocidad;
            this.initialPosition = new Position(cpx, cpy);
        }
        public void SetPosition (double cpx, double cpy)
        {
            this.currentPosition = new Position (cpx, cpy);
        }

        // Metodos

        public void SetVelocidad(double velocidad)
        { this.velocidad = velocidad; }

        public void Mover(double tiempo)
        {
            //Calculamos la distancia recorrida en el tiempo dado
            double distancia = tiempo * this.velocidad / 60;

            //Calculamos las razones trigonométricas
            double hipotenusa = Math.Sqrt((finalPosition.GetX() - currentPosition.GetX()) * (finalPosition.GetX() - currentPosition.GetX()) + (finalPosition.GetY() - currentPosition.GetY()) * (finalPosition.GetY() - currentPosition.GetY()));
            double coseno = (finalPosition.GetX() - currentPosition.GetX()) / hipotenusa;
            double seno = (finalPosition.GetY() - currentPosition.GetY()) / hipotenusa;

            //Caculamos la nueva posición del vuelo
            double x = currentPosition.GetX() + distancia * coseno;
            double y = currentPosition.GetY() + distancia * seno;

            Position nextPosition = new Position(x, y);

            if (currentPosition.Distancia(nextPosition) < hipotenusa)
            {
                currentPosition = nextPosition;
            }
            else
            {
                currentPosition = finalPosition;
            }
        }

        public bool HasArrived()
        {
            // Comparar coordenadas 
            double tolerancia = 0.1; // Tolerancia para errores de precisión
            bool llegadoX = Math.Abs(currentPosition.GetX() - finalPosition.GetX()) < tolerancia;
            bool llegadoY = Math.Abs(currentPosition.GetY() - finalPosition.GetY()) < tolerancia;
            return llegadoX && llegadoY;
        }

        public void Restart(double cpx, double cpy)
        {
            currentPosition = new Position(initialPosition.GetX(), initialPosition.GetY());
        }

        public double Distancia(double fpx, double fpy) 
        {
            double hipotenusa = Math.Sqrt((fpx - currentPosition.GetX()) * (fpx - currentPosition.GetX()) + (fpy - currentPosition.GetY()) * (fpy - currentPosition.GetY()));
            return hipotenusa;
        }

        public bool Conflicto(FlightPlan b, double distanciaSeguridad)
        {
            bool conflicto = false;
            if (this.currentPosition.Distancia(b.currentPosition) < distanciaSeguridad)
            {
                conflicto = true;
            }
            return conflicto;
        }

        public void EscribeConsola()
        // escribe en consola los datos del plan de vuelo
        {
            Console.WriteLine("******************************");
            Console.WriteLine("Datos del vuelo: ");
            Console.WriteLine("Identificador: {0}", id);
            Console.WriteLine("Velocidad: {0:F2}", velocidad);
            Console.WriteLine("Posición actual: ({0:F2},{1:F2})", currentPosition.GetX(), currentPosition.GetY());
            if (this.HasArrived())
            {
                Console.WriteLine("Ha llegado al destino.");
            }
            Console.WriteLine("******************************");
        }

        public Position GetInitialPosition()
        {
            return initialPosition;
        }
        public Position GetCurrentPosition()
        {
            return currentPosition;
        }
        public Position GetFinalPosition()
        {
            return finalPosition;
        }
        //Más metodos
        public string GetId()
        {
            return id;
        }
        public double GetVelocidad()
        {
            return velocidad;
        }
        
        //Constructor 2
        public bool EntraraEnConflicto(FlightPlan otroVuelo, double distanciaSeguridad, double tiempoMaximo)
        {
            // Obtener datos iniciales
            double x1 = this.GetCurrentPosition().GetX();
            double y1 = this.GetCurrentPosition().GetY();
            double x2 = otroVuelo.GetCurrentPosition().GetX();
            double y2 = otroVuelo.GetCurrentPosition().GetY();

            double v1 = this.velocidad;
            double v2 = otroVuelo.GetVelocidad();

            // Calcular vectores de dirección
            double dx1 = this.GetFinalPosition().GetX() - x1;
            double dy1 = this.GetFinalPosition().GetY() - y1;
            double dx2 = otroVuelo.GetFinalPosition().GetX() - x2;
            double dy2 = otroVuelo.GetFinalPosition().GetY() - y2;

            // Normalizar vectores (convertir a vectores unitarios)
            double dist1 = Math.Sqrt(dx1 * dx1 + dy1 * dy1);
            double dist2 = Math.Sqrt(dx2 * dx2 + dy2 * dy2);

            if (dist1 == 0 || dist2 == 0) return false; // Ya han llegado

            double ux1 = dx1 / dist1;
            double uy1 = dy1 / dist1;
            double ux2 = dx2 / dist2;
            double uy2 = dy2 / dist2;

            // Pasar de segundos a minutos la velocidad
            double v1_sec = v1 / 60.0;
            double v2_sec = v2 / 60.0;

            // ECUACIONES DE MOVIMIENTO:
            // Pos1(t) = (x1 + ux1 * v1_sec * t, y1 + uy1 * v1_sec * t)
            // Pos2(t) = (x2 + ux2 * v2_sec * t, y2 + uy2 * v2_sec * t)

            // Distancia al cuadrado entre aviones en función del tiempo
            // d²(t) = [deltax + (ux1*v1_sec - ux2*v2_sec)*t]² + [deltay + (uy1*v1_sec - uy2*v2_sec)*t]²

            double deltaX = x1 - x2;
            double deltaY = y1 - y2;
            double velX = ux1 * v1_sec - ux2 * v2_sec;
            double velY = uy1 * v1_sec - uy2 * v2_sec;

            // Ecuación cuadrática: a*t² + b*t + c = 0
            double a = velX * velX + velY * velY;
            double b = 2 * (deltaX * velX + deltaY * velY);
            double c = deltaX * deltaX + deltaY * deltaY;

            double umbral = 2 * distanciaSeguridad;
            double umbralCuadrado = umbral * umbral;

            // Resolver para el momento en que d²(t) = umbral²
            c -= umbralCuadrado;

            double discriminante = b * b - 4 * a * c;

            if (discriminante < 0)
            {
                // No hay solución real - nunca se acercan al umbral
                return false;
            }

            double t1 = (-b - Math.Sqrt(discriminante)) / (2 * a);
            double t2 = (-b + Math.Sqrt(discriminante)) / (2 * a);

            // Buscar el primer tiempo positivo donde ocurre el conflicto
            double tiempoConflicto = -1;

            if (t1 >= 0 && t1 <= tiempoMaximo * 60)
            {
                tiempoConflicto = t1;
            }
            else if (t2 >= 0 && t2 <= tiempoMaximo * 60)
            {
                tiempoConflicto = t2;
            }
            if (tiempoConflicto >= 0)
            {
                // Verificar que no hayan llegado antes del conflicto (1 dia entero para pensar esta solución)
                double tiempoLlegada1 = dist1 / v1_sec;
                double tiempoLlegada2 = dist2 / v2_sec;

                if (tiempoConflicto <= tiempoLlegada1 && tiempoConflicto <= tiempoLlegada2)
                {
                    Console.WriteLine($" CONFLICTO MATEMÁTICO: {tiempoConflicto / 60:F2} minutos");
                    return true;
                }
            }

            return false;
        }

        // Método para obtener el tiempo exacto del conflicto
        public double? TiempoHastaConflicto(FlightPlan otroVuelo, double distanciaSeguridad, double tiempoMaximo)
        {
            // Mismo cálculo matemático que arriba, pero devolviendo el tiempo(ctrl c ctrl v)

            double x1 = this.GetCurrentPosition().GetX();
            double y1 = this.GetCurrentPosition().GetY();
            double x2 = otroVuelo.GetCurrentPosition().GetX();
            double y2 = otroVuelo.GetCurrentPosition().GetY();

            double v1 = this.velocidad;
            double v2 = otroVuelo.GetVelocidad();

            double dx1 = this.GetFinalPosition().GetX() - x1;
            double dy1 = this.GetFinalPosition().GetY() - y1;
            double dx2 = otroVuelo.GetFinalPosition().GetX() - x2;
            double dy2 = otroVuelo.GetFinalPosition().GetY() - y2;

            double dist1 = Math.Sqrt(dx1 * dx1 + dy1 * dy1);
            double dist2 = Math.Sqrt(dx2 * dx2 + dy2 * dy2);

            if (dist1 == 0 || dist2 == 0)
            {
                return null;
            }

            double ux1 = dx1 / dist1;
            double uy1 = dy1 / dist1;
            double ux2 = dx2 / dist2;
            double uy2 = dy2 / dist2;

            double v1_sec = v1 / 60.0;
            double v2_sec = v2 / 60.0;

            double deltaX = x1 - x2;
            double deltaY = y1 - y2;
            double velX = ux1 * v1_sec - ux2 * v2_sec;
            double velY = uy1 * v1_sec - uy2 * v2_sec;

            double a = velX * velX + velY * velY;
            double b = 2 * (deltaX * velX + deltaY * velY);
            double c = deltaX * deltaX + deltaY * deltaY;

            double umbral = 2 * distanciaSeguridad;
            double umbralCuadrado = umbral * umbral;

            c -= umbralCuadrado;

            double discriminante = b * b - 4 * a * c;

            if (discriminante < 0)
            {
                return null;
            }
            double t1 = (-b - Math.Sqrt(discriminante)) / (2 * a);
            double t2 = (-b + Math.Sqrt(discriminante)) / (2 * a);

            double tiempoConflicto = -1;

            if (t1 >= 0 && t1 <= tiempoMaximo * 60)
            {
                tiempoConflicto = t1;
            }
            else if (t2 >= 0 && t2 <= tiempoMaximo * 60)
            {
                tiempoConflicto = t2;
            }
            if (tiempoConflicto >= 0)
            {
                double tiempoLlegada1 = dist1 / v1_sec;
                double tiempoLlegada2 = dist2 / v2_sec;

                if (tiempoConflicto <= tiempoLlegada1 && tiempoConflicto <= tiempoLlegada2)
                {
                    return tiempoConflicto / 60.0; // Convertir a minutos
                }
            }
            return null;
        }

    }
}
