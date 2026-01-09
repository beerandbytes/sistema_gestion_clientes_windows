using System.Globalization;
using System.IO.Compression;
using System.Xml;
using GestorClientes.Data;
using GestorClientes.Models;

namespace GestorClientes.Services;

public class ClienteImportService
{
    private readonly ClienteRepository _clienteRepository;

    public ClienteImportService()
    {
        _clienteRepository = new ClienteRepository();
    }

    public ImportResult ImportarDesdeOds(string filePath, bool limpiarAntes = false)
    {
        var result = new ImportResult();
        
        // Si se solicita limpiar antes, eliminar todos los clientes y pagos
        if (limpiarAntes)
        {
            try
            {
                _clienteRepository.DeleteAll();
                result.Errores.Add("Base de datos limpiada antes de la importación.");
            }
            catch (Exception ex)
            {
                result.Errores.Add($"Error al limpiar la base de datos: {ex.Message}");
                return result;
            }
        }

        if (!File.Exists(filePath))
        {
            result.Errores.Add($"El archivo no existe: {filePath}");
            return result;
        }

        try
        {
            // Los archivos ODS son archivos ZIP que contienen XML
            // Necesitamos extraer content.xml y parsearlo
            using var zipArchive = ZipFile.OpenRead(filePath);
            var contentEntry = zipArchive.GetEntry("content.xml");
            
            if (contentEntry == null)
            {
                // Intentar buscar en otras ubicaciones posibles
                var allEntries = zipArchive.Entries.Select(e => e.FullName).ToList();
                result.Errores.Add($"No se encontró content.xml en el archivo ODS. Entradas encontradas: {string.Join(", ", allEntries.Take(10))}");
                return result;
            }

            using var contentStream = contentEntry.Open();
            var xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(contentStream);
            }
            catch (XmlException ex)
            {
                result.Errores.Add($"Error al parsear el XML del archivo ODS: {ex.Message}");
                return result;
            }

            // Namespace de OpenDocument
            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("table", "urn:oasis:names:tc:opendocument:xmlns:table:1.0");
            nsManager.AddNamespace("text", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
            nsManager.AddNamespace("office", "urn:oasis:names:tc:opendocument:xmlns:office:1.0");

            // Obtener la primera hoja
            var spreadsheet = xmlDoc.SelectSingleNode("//office:spreadsheet", nsManager);
            if (spreadsheet == null)
            {
                result.Errores.Add("No se encontró ninguna hoja en el archivo.");
                return result;
            }

            var firstTable = spreadsheet.SelectSingleNode(".//table:table", nsManager);
            if (firstTable == null)
            {
                result.Errores.Add("No se encontró ninguna tabla en la hoja.");
                return result;
            }

            // Leer todas las filas
            var rows = firstTable.SelectNodes(".//table:table-row", nsManager);
            if (rows == null || rows.Count == 0)
            {
                result.Errores.Add("El archivo está vacío.");
                return result;
            }

            // Buscar la fila de encabezados (puede no ser la primera)
            XmlNode? headerRow = null;
            int headerRowIndex = -1;
            
            for (int i = 0; i < Math.Min(10, rows.Count); i++)
            {
                var testRow = rows[i];
                if (testRow == null) continue;
                
                var testMap = MapearColumnas(testRow, nsManager);
                if (testMap.ContainsKey("Nombre") || testMap.ContainsKey("Telefono"))
                {
                    headerRow = testRow;
                    headerRowIndex = i;
                    break;
                }
            }

            if (headerRow == null)
            {
                result.Errores.Add("No se encontró la fila de encabezados. Buscando columnas: Nombre, Telefono, etc.");
                return result;
            }
            
            var columnMap = MapearColumnas(headerRow, nsManager);

            // Validar que tenemos las columnas mínimas requeridas
            if (!columnMap.ContainsKey("Nombre"))
            {
                result.Errores.Add("No se encontró la columna 'Nombre' en el archivo.");
                return result;
            }

            // Procesar filas de datos (empezar desde después de la fila de encabezados)
            int startRowIndex = headerRowIndex + 1;
            for (int i = startRowIndex; i < rows.Count; i++)
            {
                var row = rows[i];
                if (row == null) continue;
                
                try
                {
                    var cliente = MapearFilaACliente(row, columnMap, nsManager, i + 1);
                    
                    if (cliente == null)
                    {
                        result.FilasOmitidas++;
                        continue;
                    }

                    // Verificar duplicados
                    if (_clienteRepository.ExistsByNombreAndTelefono(cliente.Nombre, cliente.Telefono))
                    {
                        result.DuplicadosOmitidos++;
                        continue;
                    }

                    // Insertar cliente
                    _clienteRepository.Insert(cliente);
                    result.ClientesImportados++;
                }
                catch (Exception ex)
                {
                    result.Errores.Add($"Fila {i + 1}: {ex.Message}");
                    result.FilasConError++;
                }
            }
        }
        catch (Exception ex)
        {
            result.Errores.Add($"Error general al leer el archivo: {ex.Message}");
        }

        return result;
    }

    private Dictionary<string, int> MapearColumnas(XmlNode headerRow, XmlNamespaceManager nsManager)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var cells = ExpandirCeldas(headerRow, nsManager);

        for (int i = 0; i < cells.Count; i++)
        {
            var cell = cells[i];
            var textNodes = cell.SelectNodes(".//text:p", nsManager);
            
            if (textNodes == null || textNodes.Count == 0) continue;

            var headerValue = string.Join(" ", textNodes.Cast<XmlNode>()
                .Select(n => n.InnerText?.Trim() ?? string.Empty))
                .Trim();

            if (string.IsNullOrEmpty(headerValue)) continue;

            // Mapeos comunes
            if ((headerValue.Contains("Nombre", StringComparison.OrdinalIgnoreCase) && 
                 !headerValue.Contains("Apellido", StringComparison.OrdinalIgnoreCase)) ||
                headerValue.Contains("Nombre del alumno", StringComparison.OrdinalIgnoreCase) ||
                headerValue.Contains("Alumno", StringComparison.OrdinalIgnoreCase))
            {
                map["Nombre"] = i;
            }
            else if (headerValue.Contains("Apellido", StringComparison.OrdinalIgnoreCase))
            {
                map["Apellidos"] = i;
            }
            else if (headerValue.Contains("Tel", StringComparison.OrdinalIgnoreCase) ||
                     headerValue.Contains("Phone", StringComparison.OrdinalIgnoreCase))
            {
                map["Telefono"] = i;
            }
            else if (headerValue.Contains("Edad", StringComparison.OrdinalIgnoreCase) ||
                     headerValue.Contains("Age", StringComparison.OrdinalIgnoreCase))
            {
                map["Edad"] = i;
            }
            else if (headerValue.Contains("Peso", StringComparison.OrdinalIgnoreCase) ||
                     headerValue.Contains("Weight", StringComparison.OrdinalIgnoreCase))
            {
                map["Peso"] = i;
            }
            else if (headerValue.Contains("Fecha Alta", StringComparison.OrdinalIgnoreCase) ||
                     headerValue.Contains("FechaAlta", StringComparison.OrdinalIgnoreCase) ||
                     headerValue.Contains("Alta", StringComparison.OrdinalIgnoreCase))
            {
                map["FechaAlta"] = i;
            }
            else if (headerValue.Contains("Fecha Vencimiento", StringComparison.OrdinalIgnoreCase) ||
                     headerValue.Contains("FechaVencimiento", StringComparison.OrdinalIgnoreCase) ||
                     headerValue.Contains("Vencimiento", StringComparison.OrdinalIgnoreCase))
            {
                map["FechaVencimiento"] = i;
            }
        }

        return map;
    }

    private List<XmlNode> ExpandirCeldas(XmlNode row, XmlNamespaceManager nsManager)
    {
        var expandedCells = new List<XmlNode>();
        var cells = row.SelectNodes(".//table:table-cell", nsManager);

        if (cells == null) return expandedCells;

        foreach (XmlNode cell in cells)
        {
            var repeatedAttr = cell.Attributes?["table:number-columns-repeated"];
            int repeatCount = 1;
            
            if (repeatedAttr != null && int.TryParse(repeatedAttr.Value, out int count))
            {
                repeatCount = count;
            }

            for (int i = 0; i < repeatCount; i++)
            {
                expandedCells.Add(cell);
            }
        }

        return expandedCells;
    }

    private Cliente? MapearFilaACliente(XmlNode row, Dictionary<string, int> columnMap, XmlNamespaceManager nsManager, int rowNumber)
    {
        try
        {
            var cliente = new Cliente();
            var cells = ExpandirCeldas(row, nsManager);

            if (cells == null || cells.Count == 0)
            {
                return null; // Fila vacía
            }

            // Nombre (requerido)
            if (!columnMap.ContainsKey("Nombre"))
            {
                return null;
            }

            var nombreIndex = columnMap["Nombre"];
            if (nombreIndex >= cells.Count)
            {
                return null;
            }

            var nombre = ObtenerValorCelda(cells[nombreIndex], nsManager)?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return null; // Fila vacía
            }
            cliente.Nombre = nombre;

            // Apellidos (opcional)
            if (columnMap.ContainsKey("Apellidos") && columnMap["Apellidos"] < cells.Count)
            {
                cliente.Apellidos = ObtenerValorCelda(cells[columnMap["Apellidos"]], nsManager)?.Trim() ?? string.Empty;
            }

            // Teléfono (opcional)
            if (columnMap.ContainsKey("Telefono") && columnMap["Telefono"] < cells.Count)
            {
                cliente.Telefono = ObtenerValorCelda(cells[columnMap["Telefono"]], nsManager)?.Trim() ?? string.Empty;
            }

            // Edad (opcional)
            if (columnMap.ContainsKey("Edad") && columnMap["Edad"] < cells.Count)
            {
                var edadStr = ObtenerValorCelda(cells[columnMap["Edad"]], nsManager);
                if (!string.IsNullOrWhiteSpace(edadStr) && int.TryParse(edadStr, out int edad))
                {
                    cliente.Edad = edad;
                }
            }

            // Peso (opcional)
            if (columnMap.ContainsKey("Peso") && columnMap["Peso"] < cells.Count)
            {
                var pesoStr = ObtenerValorCelda(cells[columnMap["Peso"]], nsManager);
                if (!string.IsNullOrWhiteSpace(pesoStr))
                {
                    pesoStr = pesoStr.Replace(",", ".");
                    if (decimal.TryParse(pesoStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal peso))
                    {
                        cliente.Peso = peso;
                    }
                }
            }

            // FechaAlta (requerido)
            if (columnMap.ContainsKey("FechaAlta") && columnMap["FechaAlta"] < cells.Count)
            {
                var fechaAltaStr = ObtenerValorCelda(cells[columnMap["FechaAlta"]], nsManager);
                if (!string.IsNullOrWhiteSpace(fechaAltaStr))
                {
                    if (DateTime.TryParse(fechaAltaStr, out DateTime fechaAlta))
                    {
                        cliente.FechaAlta = fechaAlta.Date;
                    }
                    else
                    {
                        throw new Exception($"Formato de fecha inválido para FechaAlta: {fechaAltaStr}");
                    }
                }
                else
                {
                    cliente.FechaAlta = DateTime.Today; // Default si no se proporciona
                }
            }
            else
            {
                cliente.FechaAlta = DateTime.Today; // Default
            }

            // FechaVencimiento (requerido)
            if (columnMap.ContainsKey("FechaVencimiento") && columnMap["FechaVencimiento"] < cells.Count)
            {
                var fechaVencStr = ObtenerValorCelda(cells[columnMap["FechaVencimiento"]], nsManager);
                if (!string.IsNullOrWhiteSpace(fechaVencStr))
                {
                    if (DateTime.TryParse(fechaVencStr, out DateTime fechaVenc))
                    {
                        cliente.FechaVencimiento = fechaVenc.Date;
                    }
                    else
                    {
                        throw new Exception($"Formato de fecha inválido para FechaVencimiento: {fechaVencStr}");
                    }
                }
                else
                {
                    // Si no hay fecha de vencimiento, usar 30 días desde FechaAlta
                    cliente.FechaVencimiento = cliente.FechaAlta.AddDays(30);
                }
            }
            else
            {
                // Si no hay columna de vencimiento, usar 30 días desde FechaAlta
                cliente.FechaVencimiento = cliente.FechaAlta.AddDays(30);
            }

            // Validar que FechaVencimiento >= FechaAlta
            if (cliente.FechaVencimiento < cliente.FechaAlta)
            {
                throw new Exception($"La fecha de vencimiento ({cliente.FechaVencimiento:yyyy-MM-dd}) no puede ser anterior a la fecha de alta ({cliente.FechaAlta:yyyy-MM-dd})");
            }

            // Activo: true si FechaVencimiento >= hoy
            cliente.Activo = cliente.FechaVencimiento >= DateTime.Today;

            return cliente;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al procesar fila {rowNumber}: {ex.Message}", ex);
        }
    }

    private string? ObtenerValorCelda(XmlNode cell, XmlNamespaceManager nsManager)
    {
        // Intentar obtener el valor del atributo office:value-type y office:value
        var valueType = cell.Attributes?["office:value-type"]?.Value;
        var value = cell.Attributes?["office:value"]?.Value;

        if (!string.IsNullOrEmpty(value))
        {
            // Si es una fecha, convertir desde el formato ODS (días desde 1899-12-30)
            if (valueType == "date" && double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double days))
            {
                var baseDate = new DateTime(1899, 12, 30);
                var date = baseDate.AddDays(days);
                return date.ToString("yyyy-MM-dd");
            }
            
            // Si es un número, devolverlo como string
            if (valueType == "float" || valueType == "percentage")
            {
                return value;
            }
        }

        // Si no hay valor numérico, obtener el texto
        var textNodes = cell.SelectNodes(".//text:p", nsManager);
        if (textNodes != null && textNodes.Count > 0)
        {
            return string.Join(" ", textNodes.Cast<XmlNode>()
                .Select(n => n.InnerText?.Trim() ?? string.Empty))
                .Trim();
        }

        return null;
    }
}

public class ImportResult
{
    public int ClientesImportados { get; set; }
    public int DuplicadosOmitidos { get; set; }
    public int FilasOmitidas { get; set; }
    public int FilasConError { get; set; }
    public List<string> Errores { get; set; } = new();

    public bool TieneErrores => Errores.Count > 0;
}

