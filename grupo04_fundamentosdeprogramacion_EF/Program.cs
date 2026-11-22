using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
namespace Essalud_SJM
{
    namespace grupo04_fundamentosdeprogramacion_EF
    {
        public class Atencion
        {
            public string Dni { get; set; }
            public string Hc { get; set; }
            public string Nombres { get; set; }
            public string Especialidad { get; set; }
            public string Fecha { get; set; }
            public string FechaCita { get; set; }
            public Atencion(string dni, string hc, string nombres, string especialidad, string fecha, string fechaCita)
            {
                Dni = dni;
                Hc = hc;
                Nombres = nombres;
                Especialidad = especialidad;
                Fecha = fecha;
                FechaCita = fechaCita;
            }
        }
        public static class LogicaAtenciones
        {
            public static Tuple<bool, string> ValidarDatos(string dni, string hc)
            {
                if (string.IsNullOrWhiteSpace(dni) || dni.Length != 8 || !EsSoloDigitos(dni))
                {
                    return Tuple.Create(false, "El DNI debe tener exactamente 8 dígitos numéricos.");
                }
                if (string.IsNullOrWhiteSpace(hc))
                {
                    return Tuple.Create(false, "El código de Historia Clínica (HC) es obligatorio.");
                }
                return Tuple.Create(true, "Datos válidos.");
            }
            public static bool ExisteDuplicado(List<Atencion> lista, string hc)
            {
                if (lista == null || string.IsNullOrWhiteSpace(hc)) return false;
                string hcBuscado = hc.Trim().ToUpper();
                foreach (var reg in lista)
                {
                    if (reg != null && !string.IsNullOrEmpty(reg.Hc) && reg.Hc.Trim().ToUpper() == hcBuscado)
                    {
                        return true;
                    }
                }
                return false;
            }
            public static Tuple<bool, string> ValidarFechaCita(string fechaCita)
            {
                DateTime dt;
                if (!DateTime.TryParseExact(fechaCita, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                {
                    return Tuple.Create(false, "Fecha de cita inválida. Use el formato AAAA-MM-DD.");
                }
                return Tuple.Create(true, "Fecha de cita válida.");
            }
            public static Atencion BuscarPorDniOHc(List<Atencion> lista, string criterio)
            {
                if (lista == null || string.IsNullOrWhiteSpace(criterio)) return null;
                string buscado = criterio.Trim().ToUpper();
                foreach (var reg in lista)
                {
                    if (reg == null) continue;
                    bool coincideDni = !string.IsNullOrEmpty(reg.Dni) && reg.Dni.Trim().ToUpper() == buscado;
                    bool coincideHc = !string.IsNullOrEmpty(reg.Hc) && reg.Hc.Trim().ToUpper() == buscado;
                    if (coincideDni || coincideHc)
                    {
                        return reg;
                    }
                }
                return null;
            }
            public static Tuple<bool, string> ReprogramarCita(Atencion atencion, string nuevaFecha)
            {
                if (atencion == null)
                {
                    return Tuple.Create(false, "No se encontró la atención.");
                }
                var validacion = ValidarFechaCita(nuevaFecha);
                if (!validacion.Item1)
                {
                    return validacion;
                }
                DateTime dtAtencion;
                DateTime dtNueva;
                if (DateTime.TryParseExact(atencion.Fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dtAtencion) &&
                    DateTime.TryParseExact(nuevaFecha, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dtNueva) &&
                    dtNueva < dtAtencion)
                {
                    return Tuple.Create(false, "La nueva fecha de cita no puede ser anterior a la fecha de atención.");
                }
                atencion.FechaCita = nuevaFecha;
                return Tuple.Create(true, "Cita reprogramada.");
            }
            public static List<Atencion> OrdenarPorBurbuja(List<Atencion> lista, string criterio)
            {
                List<Atencion> copia = new List<Atencion>();
                if (lista != null)
                {
                    copia.AddRange(lista);
                }
                int n = copia.Count;
                for (int i = 0; i < n - 1; i++)
                {
                    for (int j = 0; j < n - i - 1; j++)
                    {
                        string valA = ObtenerValorCriterio(copia[j], criterio);
                        string valB = ObtenerValorCriterio(copia[j + 1], criterio);

                        if (string.Compare(valA, valB, StringComparison.OrdinalIgnoreCase) > 0)
                        {
                            Atencion temp = copia[j];
                            copia[j] = copia[j + 1];
                            copia[j + 1] = temp;
                        }
                    }
                }
                return copia;
            }
            private static string ObtenerValorCriterio(Atencion reg, string criterio)
            {
                if (reg == null) return "";
                switch ((criterio ?? "").ToLower())
                {
                    case "hc":
                        return reg.Hc ?? "";
                    case "cita":
                        return reg.FechaCita ?? "";
                    case "fecha":
                    default:
                        return reg.Fecha ?? "";
                }
            }
            private static bool EsSoloDigitos(string texto)
            {
                for (int i = 0; i < texto.Length; i++)
                {
                    if (!char.IsDigit(texto[i])) return false;
                }
                return true;
            }
        }
        public static class ArchivoManager
        {
            public static List<Atencion> CargarArchivo(string nombreArchivo)
            {
                List<Atencion> lista = new List<Atencion>();
                if (string.IsNullOrWhiteSpace(nombreArchivo) || !File.Exists(nombreArchivo))
                {
                    return lista;
                }
                string[] lineas = File.ReadAllLines(nombreArchivo, Encoding.UTF8);
                foreach (string linea in lineas)
                {
                    if (string.IsNullOrWhiteSpace(linea)) continue;
                    string[] partes = linea.Split('|');
                    if (partes.Length >= 6)
                    {
                        lista.Add(new Atencion(
                            partes[0].Trim(),
                            partes[1].Trim(),
                            partes[2].Trim(),
                            partes[3].Trim(),
                            partes[4].Trim(),
                            partes[5].Trim()
                        ));
                    }
                }
                return lista;
            }
            public static void GuardarArchivo(List<Atencion> lista, string nombreArchivo)
            {
                List<string> lineas = new List<string>();
                if (lista != null)
                {
                    foreach (var reg in lista)
                    {
                        if (reg == null) continue;
                        lineas.Add(string.Format("{0}|{1}|{2}|{3}|{4}|{5}",
                            reg.Dni, reg.Hc, reg.Nombres, reg.Especialidad, reg.Fecha, reg.FechaCita));
                    }
                }
                File.WriteAllLines(nombreArchivo, lineas, Encoding.UTF8);
                Console.WriteLine("\n¡Datos guardados correctamente en '" + nombreArchivo + "'!");
            }
        }
        class Program
        {
            private const string NOMBRE_ARCHIVO = "atenciones_salud.txt";
            private static void RegistrarAtencion(List<Atencion> lista)
            {
                Console.WriteLine("\n--- REGISTRO DE NUEVA ATENCIÓN MÉDICA ---");
                string dni = "";
                string hc = "";
                while (true)
                {
                    Console.Write("Ingrese DNI del paciente (8 dígitos): ");
                    string entradaDni = Console.ReadLine();
                    dni = entradaDni != null ? entradaDni.Trim() : "";
                    Console.Write("Ingrese Código HC (ej. HC-2026-001): ");
                    string entradaHc = Console.ReadLine();
                    hc = entradaHc != null ? entradaHc.Trim().ToUpper() : "";
                    var resultadoValidacion = LogicaAtenciones.ValidarDatos(dni, hc);
                    if (resultadoValidacion.Item1) break;
                    Console.WriteLine("[!] " + resultadoValidacion.Item2);
                }
                if (LogicaAtenciones.ExisteDuplicado(lista, hc))
                {
                    Console.WriteLine("\n[!] ERROR: El código de Historia Clínica ya se encuentra registrado.");
                    return;
                }
                Console.Write("Ingrese Nombres y Apellidos: ");
                string entradaNombres = Console.ReadLine();
                string nombres = (entradaNombres != null && entradaNombres.Trim().Length > 0)
                    ? entradaNombres.Trim().ToUpper()
                    : "NO REGISTRADO";
                Console.Write("Ingrese Especialidad Médica: ");
                string entradaEsp = Console.ReadLine();
                string especialidad = (entradaEsp != null && entradaEsp.Trim().Length > 0)
                    ? entradaEsp.Trim()
                    : "General";
                string fecha;
                while (true)
                {
                    Console.Write("Ingrese Fecha de atención (AAAA-MM-DD) [ENTER = 2026-01-01]: ");
                    string entradaFecha = Console.ReadLine();
                    string fechaTrim = entradaFecha != null ? entradaFecha.Trim() : "";
                    if (string.IsNullOrWhiteSpace(fechaTrim))
                    {
                        fecha = "2026-01-01";
                        break;
                    }
                    if (EsFechaValida(fechaTrim))
                    {
                        fecha = fechaTrim;
                        break;
                    }
                    Console.WriteLine("[!] Fecha inválida. Use el formato AAAA-MM-DD.");
                }
                string fechaCita;
                DateTime dtAtencion;
                DateTime.TryParseExact(fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dtAtencion);
                while (true)
                {
                    Console.Write("Ingrese Fecha de la cita (AAAA-MM-DD): ");
                    string entradaCita = Console.ReadLine();
                    fechaCita = entradaCita != null ? entradaCita.Trim() : "";
                    var resultadoCita = LogicaAtenciones.ValidarFechaCita(fechaCita);
                    if (!resultadoCita.Item1)
                    {
                        Console.WriteLine("[!] " + resultadoCita.Item2);
                        continue;
                    }
                    DateTime dtCita;
                    if (DateTime.TryParseExact(fechaCita, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dtCita) &&
                        dtCita < dtAtencion)
                    {
                        Console.WriteLine("[!] La fecha de la cita no puede ser anterior a la fecha de atención.");
                        continue;
                    }
                    break;
                }
                Atencion nuevaAtencion = new Atencion(dni, hc, nombres, especialidad, fecha, fechaCita);
                lista.Add(nuevaAtencion);
                Console.WriteLine("\n¡Registro guardado correctamente en memoria!");
            }
            private static void ReprogramarCita(List<Atencion> lista)
            {
                Console.WriteLine("\n--- REPROGRAMAR CITA ---");
                if (lista.Count == 0)
                {
                    Console.WriteLine("La lista está vacía.");
                    return;
                }
                Console.Write("Ingrese Código HC o DNI del paciente: ");
                string entradaCriterio = Console.ReadLine();
                string criterio = entradaCriterio != null ? entradaCriterio.Trim() : "";
                if (string.IsNullOrWhiteSpace(criterio))
                {
                    Console.WriteLine("\n[!] Debe ingresar un criterio de búsqueda.");
                    return;
                }
                Atencion atencion = LogicaAtenciones.BuscarPorDniOHc(lista, criterio);
                if (atencion == null)
                {
                    Console.WriteLine("\n[!] No se encontró ningún registro con el criterio ingresado.");
                    return;
                }
                Console.WriteLine("\nPaciente:             " + atencion.Nombres);
                Console.WriteLine("Especialidad:         " + atencion.Especialidad);
                Console.WriteLine("Fecha de cita actual: " + atencion.FechaCita);
                while (true)
                {
                    Console.Write("Ingrese la nueva fecha de la cita (AAAA-MM-DD) o ENTER para cancelar: ");
                    string entradaNueva = Console.ReadLine();
                    string nuevaFecha = entradaNueva != null ? entradaNueva.Trim() : "";
                    if (string.IsNullOrEmpty(nuevaFecha))
                    {
                        Console.WriteLine("Reprogramación cancelada.");
                        return;
                    }
                    string anterior = atencion.FechaCita;
                    var resultadoReprog = LogicaAtenciones.ReprogramarCita(atencion, nuevaFecha);
                    if (resultadoReprog.Item1)
                    {
                        Console.WriteLine("\n¡Cita reprogramada correctamente! " + anterior + " -> " + atencion.FechaCita);
                        Console.WriteLine("(Recuerde guardar los cambios con la opción 5.)");
                        return;
                    }
                    Console.WriteLine("[!] " + resultadoReprog.Item2);
                }
            }
            private static void BuscarAtencion(List<Atencion> lista)
            {
                Console.WriteLine("\n--- BÚSQUEDA DE ATENCIÓN MÉDICA ---");
                if (lista.Count == 0)
                {
                    Console.WriteLine("La lista está vacía.");
                    return;
                }
                Console.Write("Ingrese Código HC o DNI a consultar: ");
                string entradaCriterio = Console.ReadLine();
                string criterio = entradaCriterio != null ? entradaCriterio.Trim() : "";
                if (string.IsNullOrWhiteSpace(criterio))
                {
                    Console.WriteLine("\n[!] Debe ingresar un DNI o Código HC.");
                    return;
                }
                Atencion reg = LogicaAtenciones.BuscarPorDniOHc(lista, criterio);
                if (reg == null)
                {
                    Console.WriteLine("\n[!] No se encontró ningún registro con el criterio ingresado.");
                    return;
                }
                Console.WriteLine("\n========================================");
                Console.WriteLine("        FICHA CLINICA ENCONTRADA");
                Console.WriteLine("========================================");
                Console.WriteLine("DNI:                 " + reg.Dni);
                Console.WriteLine("Historia Clínica:    " + reg.Hc);
                Console.WriteLine("Paciente:            " + reg.Nombres);
                Console.WriteLine("Especialidad:        " + reg.Especialidad);
                Console.WriteLine("Fecha de Atención:   " + reg.Fecha);
                Console.WriteLine("Fecha de la Cita:    " + reg.FechaCita);
                Console.WriteLine("========================================");
            }
            private static void MenuOrdenarBurbuja(List<Atencion> lista)
            {
                Console.WriteLine("\n--- ORDENAR ATENCIONES (MÉTODO BURBUJA) ---");
                if (lista.Count == 0)
                {
                    Console.WriteLine("La lista de atenciones está vacía.");
                    return;
                }
                Console.WriteLine("1. Ordenar por Fecha de Atención");
                Console.WriteLine("2. Ordenar por Código de Historia Clínica (HC)");
                Console.WriteLine("3. Ordenar por Fecha de la Cita");
                Console.Write("Seleccione la opción: ");
                string entradaOp = Console.ReadLine();
                string op = entradaOp != null ? entradaOp.Trim() : "";
                string criterio = null;
                switch (op)
                {
                    case "1":
                        criterio = "fecha";
                        break;
                    case "2":
                        criterio = "hc";
                        break;
                    case "3":
                        criterio = "cita";
                        break;
                }
                if (criterio == null)
                {
                    Console.WriteLine("Opción no válida.");
                    return;
                }
                List<Atencion> resultadoOrdenado = LogicaAtenciones.OrdenarPorBurbuja(lista, criterio);
                Console.WriteLine("\n¡Lista ordenada por '" + criterio.ToUpper() + "' mediante Burbuja exitosamente!");
                MostrarTabla(resultadoOrdenado);
            }
            private static void MostrarTabla(List<Atencion> lista)
            {
                Console.WriteLine("\n" + new string('-', 98));
                Console.WriteLine("{0,-10} | {1,-14} | {2,-25} | {3,-15} | {4,-10} | {5,-10}",
                    "DNI", "HISTORIA C.", "PACIENTE", "ESPECIALIDAD", "FECHA", "FECHA CITA");
                Console.WriteLine(new string('-', 98));
                foreach (var reg in lista)
                {
                    string nom = FormatearColumna(reg.Nombres, 24);
                    string esp = FormatearColumna(reg.Especialidad, 14);
                    Console.WriteLine("{0,-10} | {1,-14} | {2,-25} | {3,-15} | {4,-10} | {5,-10}",
                        reg.Dni, reg.Hc, nom, esp, reg.Fecha, reg.FechaCita);
                }
                Console.WriteLine(new string('-', 98));
            }
            private static string FormatearColumna(string valor, int maxLen)
            {
                string texto = valor ?? "";
                return texto.Length > maxLen ? texto.Substring(0, maxLen) : texto;
            }
            private static bool EsFechaValida(string fecha)
            {
                DateTime dt;
                return DateTime.TryParseExact(
                    fecha,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out dt
                );
            }
            static void Main(string[] args)
            {
                List<Atencion> listaAtenciones = ArchivoManager.CargarArchivo(NOMBRE_ARCHIVO);
                if (listaAtenciones == null)
                {
                    listaAtenciones = new List<Atencion>();
                }
                bool salir = false;
                while (!salir)
                {
                    Console.WriteLine("\n==============================================");
                    Console.WriteLine("  SISTEMA DE ATENCIONES - CENTRO DE SALUD (C#)");
                    Console.WriteLine("==============================================");
                    Console.WriteLine("1. Registrar nueva atención médica");
                    Console.WriteLine("2. Buscar atención por DNI o Historia Clínica");
                    Console.WriteLine("3. Reprogramar cita");
                    Console.WriteLine("4. Ordenar por Método Burbuja (retorna lista)");
                    Console.WriteLine("5. Guardar datos en el archivo (.txt)");
                    Console.WriteLine("6. Salir");
                    Console.WriteLine("==============================================");
                    Console.Write("Seleccione una opción (1-6): ");
                    string entradaOpcion = Console.ReadLine();
                    string opcion = entradaOpcion != null ? entradaOpcion.Trim() : "";
                    switch (opcion)
                    {
                        case "1":
                            RegistrarAtencion(listaAtenciones);
                            break;
                        case "2":
                            BuscarAtencion(listaAtenciones);
                            break;
                        case "3":
                            ReprogramarCita(listaAtenciones);
                            break;
                        case "4":
                            MenuOrdenarBurbuja(listaAtenciones);
                            break;
                        case "5":
                            ArchivoManager.GuardarArchivo(listaAtenciones, NOMBRE_ARCHIVO);
                            break;
                        case "6":
                            Console.Write("¿Desea guardar los cambios antes de salir? (s/n): ");
                            string entradaResp = Console.ReadLine();
                            string resp = entradaResp != null ? entradaResp.Trim().ToLower() : "";
                            if (resp == "s")
                            {
                                ArchivoManager.GuardarArchivo(listaAtenciones, NOMBRE_ARCHIVO);
                            }
                            Console.WriteLine("\n¡Gracias por utilizar el sistema! Hasta luego.");
                            salir = true;
                            break;
                        default:
                            Console.WriteLine("\n[!] Opción no válida. Intente nuevamente.");
                            break;
                    }
                }
            }
        }
    }
}