using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FlightLib;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Font = iTextSharp.text.Font; // ALIAS para evitar conflicto
using Paragraph = iTextSharp.text.Paragraph;
using PdfPTable = iTextSharp.text.pdf.PdfPTable;
using PdfPCell = iTextSharp.text.pdf.PdfPCell;
using Phrase = iTextSharp.text.Phrase;

namespace InterfazGrafica
{
    public class ImagePdfExporter
    {
        public static void ExportToPDF(FlightPlanList vuelos, CompanyManager companyManager, string filePath)
        {
            if (vuelos == null || vuelos.GetNum() == 0)
                throw new Exception("No hay vuelos para exportar");

            // 1. Crear documento con márgenes grandes
            Document document = new Document(PageSize.A4, 50, 50, 50, 50);

            try
            {
                // 2. Crear writer
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));

                document.Open();

                // 3. FUENTES (MUY GRANDES)
                BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);

                // TÍTULO - ENORME
                Font fontTituloGrande = new Font(baseFont, 28, Font.BOLD, BaseColor.BLUE);

                // SUBTÍTULO - GRANDE
                Font fontSubtitulo = new Font(baseFont, 20, Font.BOLD, BaseColor.DARK_GRAY);

                // TEXTO NORMAL - GRANDE
                Font fontNormal = new Font(baseFont, 14, Font.NORMAL, BaseColor.BLACK);

                // TEXTO PEQUEÑO
                Font fontPequeño = new Font(baseFont, 12, Font.NORMAL, BaseColor.GRAY);

                // TABLA - ENCABEZADO
                Font fontTablaHeader = new Font(baseFont, 13, Font.BOLD, BaseColor.WHITE);

                // TABLA - DATOS
                Font fontTablaDatos = new Font(baseFont, 12, Font.NORMAL, BaseColor.BLACK);

                // 4. TÍTULO PRINCIPAL (CENTRADO)
                Paragraph titulo = new Paragraph("INFORME DE VUELOS POR AEROLÍNEA", fontTituloGrande);
                titulo.Alignment = Element.ALIGN_CENTER;
                titulo.SpacingAfter = 30;
                document.Add(titulo);

                // 5. INFORMACIÓN GENERAL
                Paragraph fecha = new Paragraph($"FECHA: {DateTime.Now:dd/MM/yyyy HH:mm}", fontNormal);
                fecha.SpacingAfter = 10;
                document.Add(fecha);

                Paragraph total = new Paragraph($"TOTAL VUELOS: {vuelos.GetNum()}", fontNormal);
                total.SpacingAfter = 30;
                document.Add(total);

                // 6. LÍNEA SEPARADORA GRUESA
                AddSeparator(document, 3, BaseColor.GRAY);
                document.Add(new Paragraph(" ") { SpacingAfter = 30 });

                // 7. AGRUPAR VUELOS
                var aerolineas = ObtenerAerolineas(vuelos);

                foreach (var aerolinea in aerolineas)
                {
                    string nombre = aerolinea.Key;
                    var vuelosAerolinea = aerolinea.Value;

                    // 7.1 NOMBRE AEROLÍNEA (GRANDE)
                    Paragraph nombreAerolinea = new Paragraph($"AEROLÍNEA: {nombre}", fontSubtitulo);
                    nombreAerolinea.SpacingAfter = 15;
                    document.Add(nombreAerolinea);

                    // 7.2 CANTIDAD DE VUELOS
                    Paragraph cantidad = new Paragraph($"VUELOS REGISTRADOS: {vuelosAerolinea.Count}", fontNormal);
                    cantidad.SpacingAfter = 10;
                    document.Add(cantidad);

                    // 7.3 INFORMACIÓN DE CONTACTO
                    if (companyManager != null)
                    {
                        try
                        {
                            Compañias info = companyManager.GetCompany(nombre);
                            if (info != null)
                            {
                                Paragraph contacto = new Paragraph(
                                    $"CONTACTO: Tel {info.GetTelefono()} - Email: {info.GetEmail()}",
                                    fontPequeño);
                                contacto.SpacingAfter = 15;
                                document.Add(contacto);
                            }
                        }
                        catch
                        {
                            // Ignorar si hay error
                        }
                    }

                    // 7.4 TABLA DE VUELOS (SI HAY)
                    if (vuelosAerolinea.Count > 0)
                    {
                        CrearTablaVuelos(document, vuelosAerolinea, fontTablaHeader, fontTablaDatos);
                    }
                    else
                    {
                        Paragraph sinVuelos = new Paragraph("NO HAY VUELOS PARA ESTA AEROLÍNEA", fontPequeño);
                        sinVuelos.SpacingAfter = 20;
                        document.Add(sinVuelos);
                    }

                    // 7.5 ESPACIO ENTRE AEROLÍNEAS
                    document.Add(new Paragraph(" ") { SpacingAfter = 40 });

                    // 7.6 LÍNEA SEPARADORA FINA
                    AddSeparator(document, 1, new BaseColor(200, 200, 200));
                    document.Add(new Paragraph(" ") { SpacingAfter = 20 });
                }

                // 8. PIE DE PÁGINA
                Paragraph pie = new Paragraph(
                    "SISTEMA DE GESTIÓN DE VUELOS - DOCUMENTO GENERADO AUTOMÁTICAMENTE",
                    fontPequeño);
                pie.Alignment = Element.ALIGN_CENTER;
                pie.SpacingBefore = 30;
                document.Add(pie);

                document.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al generar PDF: " + ex.Message);
            }
        }

        private static Dictionary<string, List<FlightPlan>> ObtenerAerolineas(FlightPlanList vuelos)
        {
            Dictionary<string, List<FlightPlan>> grupos = new Dictionary<string, List<FlightPlan>>();

            for (int i = 0; i < vuelos.GetNum(); i++)
            {
                FlightPlan vuelo = vuelos.GetFlightPlan(i);
                string compania = vuelo.GetCompany();

                if (string.IsNullOrWhiteSpace(compania))
                    compania = "SIN AEROLÍNEA";

                if (!grupos.ContainsKey(compania))
                    grupos[compania] = new List<FlightPlan>();

                grupos[compania].Add(vuelo);
            }

            // Ordenar alfabéticamente
            return grupos.OrderBy(g => g.Key)
                         .ToDictionary(g => g.Key, g => g.Value);
        }

        private static void CrearTablaVuelos(Document document, List<FlightPlan> vuelos,
            Font fontHeader, Font fontDatos)
        {
            // Crear tabla con 5 columnas
            PdfPTable tabla = new PdfPTable(5);
            tabla.WidthPercentage = 100;

            // ANCHOS DE COLUMNAS (GRANDES)
            float[] anchos = { 2.5f, 2.5f, 2.5f, 2f, 2f };
            tabla.SetWidths(anchos);
            tabla.SpacingBefore = 10;
            tabla.SpacingAfter = 20;

            // ENCABEZADOS DE TABLA
            string[] headers = {
                "ID VUELO",
                "POSICIÓN ACTUAL",
                "DESTINO",
                "VELOCIDAD",
                "ESTADO"
            };

            foreach (string header in headers)
            {
                PdfPCell celda = new PdfPCell(new Phrase(header, fontHeader));
                celda.BackgroundColor = new BaseColor(60, 60, 150); // Azul oscuro
                celda.HorizontalAlignment = Element.ALIGN_CENTER;
                celda.Padding = 10;
                celda.PaddingTop = 12;
                celda.PaddingBottom = 12;
                tabla.AddCell(celda);
            }

            // DATOS DE LOS VUELOS
            for (int i = 0; i < vuelos.Count; i++)
            {
                FlightPlan vuelo = vuelos[i];

                // Fondo alternado para mejor legibilidad
                BaseColor fondoFila = (i % 2 == 0) ?
                    BaseColor.WHITE :
                    new BaseColor(240, 240, 240);

                // 1. ID VUELO
                PdfPCell celdaId = new PdfPCell(new Phrase(vuelo.GetId(), fontDatos));
                celdaId.BackgroundColor = fondoFila;
                celdaId.Padding = 8;
                celdaId.PaddingLeft = 10;
                tabla.AddCell(celdaId);

                // 2. POSICIÓN ACTUAL
                string posicion = $"({vuelo.GetCurrentPosition().GetX():F0}, {vuelo.GetCurrentPosition().GetY():F0})";
                PdfPCell celdaPos = new PdfPCell(new Phrase(posicion, fontDatos));
                celdaPos.BackgroundColor = fondoFila;
                celdaPos.Padding = 8;
                tabla.AddCell(celdaPos);

                // 3. DESTINO
                string destino = $"({vuelo.GetFinalPosition().GetX():F0}, {vuelo.GetFinalPosition().GetY():F0})";
                PdfPCell celdaDest = new PdfPCell(new Phrase(destino, fontDatos));
                celdaDest.BackgroundColor = fondoFila;
                celdaDest.Padding = 8;
                tabla.AddCell(celdaDest);

                // 4. VELOCIDAD
                PdfPCell celdaVel = new PdfPCell(new Phrase($"{vuelo.GetVelocidad():F1} km/h", fontDatos));
                celdaVel.BackgroundColor = fondoFila;
                celdaVel.Padding = 8;
                celdaVel.HorizontalAlignment = Element.ALIGN_CENTER;
                tabla.AddCell(celdaVel);

                // 5. ESTADO (CON COLOR)
                string estado = vuelo.HasArrived() ? "✓ LLEGADO" : "→ EN VUELO";
                Font fontEstado = new Font(BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED),
                    12,
                    Font.NORMAL,
                    vuelo.HasArrived() ? BaseColor.GREEN : BaseColor.BLUE);

                PdfPCell celdaEst = new PdfPCell(new Phrase(estado, fontEstado));
                celdaEst.BackgroundColor = fondoFila;
                celdaEst.Padding = 8;
                celdaEst.HorizontalAlignment = Element.ALIGN_CENTER;
                tabla.AddCell(celdaEst);
            }

            document.Add(tabla);
        }

        private static void AddSeparator(Document document, float grosor, BaseColor color)
        {
            PdfPTable linea = new PdfPTable(1);
            linea.WidthPercentage = 100;

            PdfPCell celda = new PdfPCell(new Phrase(" "));
            celda.Border = PdfPCell.NO_BORDER;
            celda.FixedHeight = grosor;
            celda.BackgroundColor = color;

            linea.AddCell(celda);
            document.Add(linea);
        }
    }
}