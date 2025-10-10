using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightLib
{
    public class Position
    {
        // Atributos
        double x; // coordenada X (2D)
        double y; // coordenada Y (2D)

        // Constructores

        public Position(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        // Metodos

        public double GetX()
        // getter del atributo x
        { return x; }

        public double GetY()
        // getter del atributo y
        { return y; }

        public double Distancia(Position b)
        // retorna la distancia entre los dos Postion
        {
            double resultado = Math.Sqrt((x - b.x) * (x - b.x) + (y - b.y) * (y - b.y));
            return resultado;
        }
    }
}
