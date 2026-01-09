# Gestor de Clientes - Sistema de Gestión

Sistema de gestión de clientes desarrollado con **Avalonia UI** y **.NET 8.0** para administrar clientes, pagos, membresías y recordatorios.

## 📋 Tabla de Contenidos

- [Instalación](#instalación)
- [Primera Configuración](#primera-configuración)
- [Funcionalidades](#funcionalidades)
- [Flujo de Trabajo](#flujo-de-trabajo)
- [Línea de Comandos](#línea-de-comandos)
- [Requisitos del Sistema](#requisitos-del-sistema)
- [Tecnologías Utilizadas](#tecnologías-utilizadas)
- [Desarrollo](#desarrollo)

---

## 🚀 Instalación

### Requisitos Previos

- **.NET 8.0 Runtime** o superior
- **Windows 10/11** (la aplicación está optimizada para Windows)
- Espacio en disco: ~100 MB

### Pasos de Instalación

1. **Descargar la aplicación**
   - Obtener el ejecutable desde la carpeta `bin/Release/net8.0/` o `publish/portable/`
   - El archivo principal es `GestorClientes.exe`

2. **Ejecutar la aplicación**
   - Hacer doble clic en el ejecutable
   - La base de datos SQLite se creará automáticamente en la primera ejecución

3. **Inicializar la base de datos (opcional)**
   - Si necesitas ejecutar solo la migración de la base de datos:
     ```bash
     GestorClientes.exe --migrate
     # O usando la forma corta:
     GestorClientes.exe --migrar
     # O usando el alias:
     GestorClientes.exe -m
     ```

### Versión Portátil (Self-Contained)

La aplicación también está disponible en versión **portátil** (self-contained) que incluye todas las dependencias:

- **Ubicación:** `publish/portable/`
- **Ventajas:**
  - No requiere .NET Runtime instalado en el sistema
  - Funciona en cualquier Windows 10+ (x64) sin dependencias adicionales
  - Completamente portable (puede ejecutarse desde USB)
- **Tamaño:** ~100-150 MB (incluye .NET Runtime completo)
- **Uso:** Ejecutar `GestorClientes.exe` directamente desde la carpeta

---

## ⚙️ Primera Configuración

### 1. Inicio de Sesión

Al ejecutar la aplicación por primera vez, se creará automáticamente un usuario administrador:

- **Usuario:** `admin`
- **Contraseña:** `admin`

**⚠️ IMPORTANTE:** Cambia la contraseña después del primer inicio de sesión por seguridad.

### 2. Base de Datos

La base de datos SQLite (`gestor.db`) se crea automáticamente en:
- Misma carpeta donde está el ejecutable
- O en `bin/Debug/net8.0/` si ejecutas desde Visual Studio

### 3. Importar Clientes Existentes (Opcional)

Si tienes un archivo `CLIENTES.ods` (formato OpenDocument Spreadsheet), puedes importar clientes:

```bash
# Importar sin limpiar datos existentes
GestorClientes.exe --importar
# O usando la forma corta:
GestorClientes.exe -i

# Importar limpiando todos los datos existentes
GestorClientes.exe --importar --limpiar
# O usando formas cortas:
GestorClientes.exe -i -l
GestorClientes.exe --importar -l

# Importar desde una ruta específica
GestorClientes.exe --importar "C:\ruta\al\archivo.ods"
GestorClientes.exe -i "C:\ruta\al\archivo.ods"

# Importar sin confirmación (útil para scripts)
GestorClientes.exe --importar --limpiar --sin-confirmar
GestorClientes.exe -i -l -y
```

**Nota:** El archivo `CLIENTES.ods` debe estar en el mismo directorio que el ejecutable, o puedes especificar la ruta completa.

### 4. Poblar con Datos de Prueba (Opcional)

Para probar la aplicación con datos ficticios:

```bash
# Poblar sin limpiar datos existentes
GestorClientes.exe --poblar
# O usando formas cortas:
GestorClientes.exe --poblar-datos
GestorClientes.exe -p

# Poblar limpiando todos los datos existentes
GestorClientes.exe --poblar --limpiar
GestorClientes.exe -p -l

# Poblar sin confirmación (útil para scripts)
GestorClientes.exe --poblar --limpiar --sin-confirmar
GestorClientes.exe -p -l -y
```

---

## 🎯 Funcionalidades

### 1. **Dashboard Principal**

Vista inicial que muestra métricas clave:
- **Total de Clientes:** Número total de clientes registrados
- **Clientes Activos:** Clientes con membresía vigente
- **Clientes Vencidos:** Clientes con membresía expirada
- **Ingresos del Mes:** Total de pagos recibidos en el mes actual
- **Clientes Próximos a Vencer:** Clientes que vencen en los próximos 7 días

**Características:**
- Clic en cualquier métrica para ver detalles
- Notificaciones automáticas de recordatorios
- Actualización automática al volver a la ventana principal

### 2. **Gestión de Clientes** 👥

#### Funcionalidades principales:
- **Agregar Cliente:** Registrar nuevos clientes con información completa
- **Editar Cliente:** Modificar datos de clientes existentes
- **Eliminar Cliente:** Eliminar clientes (con confirmación)
- **Buscar Clientes:** Búsqueda en tiempo real por nombre, apellidos o teléfono
- **Filtrar por Estado:** Ver todos, solo activos o solo vencidos
- **Selección Múltiple:** Seleccionar varios clientes para operaciones en lote

#### Información del Cliente:
- Nombre y apellidos
- Edad y peso (opcionales)
- Teléfono
- Fecha de alta
- Fecha de vencimiento de membresía
- Fecha del último pago
- Estado (Activo/Vencido/Pendiente)

#### Acciones Adicionales:
- **Ver Historial de Pagos:** Ver todos los pagos de un cliente (doble clic en la fila)
- **Registrar Pago:** Registrar un nuevo pago y renovar automáticamente la membresía
- **Cambiar Estado:** Cambiar el estado de uno o varios clientes seleccionados

#### Atajos de Teclado:
- `F5`: Recargar lista de clientes
- `Ctrl + N`: Agregar nuevo cliente
- `Escape`: Limpiar búsqueda

### 3. **Gestión de Pagos** 💰

#### Funcionalidades:
- **Registrar Pago:** Registrar pagos de clientes con fecha y cantidad
- **Ver Historial Completo:** Ver todos los pagos registrados
- **Filtrar por Fecha:** Filtrar pagos entre dos fechas específicas
- **Filtrar por Mes:** Filtrar pagos de un mes y año específicos
- **Exportar a Excel:** Exportar pagos filtrados a archivo Excel (.xlsx)

#### Características:
- Al registrar un pago, la membresía se renueva automáticamente por 30 días
- Cálculo automático del total de pagos mostrados
- Contador de registros visibles

### 4. **Recordatorios** ⚠️

Sistema de alertas para gestionar vencimientos:

- **Clientes Vencidos:** Lista de clientes con membresía expirada
- **Clientes Próximos a Vencer:** Clientes que vencen en los próximos 7 días
- **Información Mostrada:**
  - Nombre completo
  - Teléfono
  - Fecha de vencimiento
  - Días vencidos o días restantes

**Nota:** Los recordatorios aparecen automáticamente en el dashboard principal.

### 5. **Resumen y Métricas** 📊

Vista detallada de métricas del negocio:

- Total de clientes
- Clientes activos
- Clientas vencidos
- Ingresos del mes actual

**Características:**
- Botón de actualización manual
- Métricas en tiempo real

### 6. **Reportes** 📄

Sistema completo de reportes con exportación a Excel:

#### Tipos de Reportes:

1. **Pagos por Fecha:**
   - Filtrar pagos entre dos fechas
   - Ver total de ingresos en el rango
   - Exportar a Excel

2. **Pagos por Mes:**
   - Filtrar pagos de un mes específico
   - Ver total del mes
   - Exportar a Excel

3. **Historial por Cliente:**
   - Seleccionar un cliente
   - Ver su historial completo de pagos
   - Ver total histórico del cliente
   - Exportar a Excel

**Formato de Exportación:**
- Archivos Excel (.xlsx)
- Incluye encabezados formateados
- Total calculado automáticamente
- Columnas ajustadas automáticamente

### 7. **Sistema de Backup** 💾

Gestión completa de copias de seguridad:

#### Funcionalidades:
- **Crear Backup:** Crear una copia de seguridad de la base de datos
- **Restaurar Backup:** Restaurar la base de datos desde un backup anterior
- **Eliminar Backup:** Eliminar backups antiguos
- **Listar Backups:** Ver todos los backups disponibles con fecha y tamaño

#### Características:
- Los backups se guardan automáticamente con fecha y hora
- Al restaurar, se crea un backup de emergencia automáticamente
- La aplicación se cierra después de restaurar (requiere reinicio)

**Ubicación de Backups:**
- Carpeta `Backups` en el directorio de la aplicación

---

## 💻 Línea de Comandos

La aplicación soporta varios argumentos de línea de comandos para operaciones sin interfaz gráfica:

### Argumentos Disponibles

| Argumento | Forma Corta | Descripción |
|-----------|-------------|-------------|
| `--migrate` | `-m` o `--migrar` | Ejecuta solo la migración de la base de datos y sale |
| `--importar` | `-i` | Importa clientes desde archivo ODS |
| `--poblar` | `-p` o `--poblar-datos` | Pobla la base de datos con datos ficticios |
| `--limpiar` | `-l` | Limpia todos los datos antes de importar/poblar |
| `--sin-confirmar` | `-y` | Omite la confirmación (útil para scripts) |

### Ejemplos de Uso

```bash
# Migrar base de datos
GestorClientes.exe --migrate

# Importar clientes con confirmación
GestorClientes.exe --importar --limpiar

# Importar sin confirmación (para automatización)
GestorClientes.exe -i -l -y

# Poblar datos de prueba
GestorClientes.exe --poblar

# Importar desde ruta específica
GestorClientes.exe --importar "C:\datos\clientes.ods"
```

### Logs

Las operaciones de línea de comandos generan logs:
- **Importación:** `importacion_log.txt` en el directorio de la aplicación
- **Errores:** `error_log.txt` en el directorio de la aplicación

---

## 🔄 Flujo de Trabajo

### Flujo Diario Típico

1. **Iniciar Sesión**
   - Abrir la aplicación
   - Ingresar usuario y contraseña

2. **Revisar Dashboard**
   - Ver métricas del día
   - Revisar recordatorios de clientes vencidos o próximos a vencer
   - Hacer clic en métricas para ver detalles

3. **Gestionar Clientes**
   - **Agregar Nuevos Clientes:**
     - Clic en "Clientes" → "Agregar"
     - Completar información del cliente
     - Guardar
   
   - **Registrar Pagos:**
     - Seleccionar cliente en la lista
     - Clic en "Registrar Pago"
     - Ingresar cantidad y fecha
     - La membresía se renueva automáticamente por 30 días

4. **Revisar Recordatorios**
   - Ir a "Recordatorios"
   - Contactar clientes vencidos o próximos a vencer
   - Actualizar estados después de contactar

5. **Generar Reportes (Fin de Mes)**
   - Ir a "Reportes"
   - Filtrar pagos del mes
   - Exportar a Excel para contabilidad

6. **Crear Backup (Recomendado Diario)**
   - Ir a "Backup"
   - Clic en "Crear Backup"
   - Guardar backup en ubicación segura

### Flujo para Nuevo Cliente

1. Nuevo cliente se registra
2. **Agregar Cliente:**
   - Nombre, apellidos, teléfono
   - Fecha de alta (automática)
   - Fecha de vencimiento (30 días desde hoy)
3. **Registrar Primer Pago:**
   - Ir a "Pagos" o desde la vista de clientes
   - Seleccionar cliente
   - Registrar pago con cantidad
   - La membresía se activa automáticamente

### Flujo para Renovación de Membresía

1. Cliente viene a renovar
2. **Buscar Cliente:**
   - Usar búsqueda en vista de clientes
   - O ver en "Recordatorios" si está próximo a vencer
3. **Registrar Pago:**
   - Seleccionar cliente
   - Registrar nuevo pago
   - La fecha de vencimiento se actualiza automáticamente (+30 días)
   - El estado cambia a "Activo"

### Flujo para Reportes Mensuales

1. Al final del mes, ir a "Reportes"
2. **Pagos por Mes:**
   - Seleccionar mes y año
   - Clic en "Filtrar"
   - Revisar total del mes
   - Clic en "Descargar Excel"
3. **Guardar archivo Excel** para contabilidad

### Flujo de Backup y Restauración

#### Crear Backup:
1. Ir a "Backup"
2. Clic en "Crear Backup"
3. Confirmar creación
4. El backup se guarda automáticamente

#### Restaurar Backup:
1. Ir a "Backup"
2. Seleccionar backup de la lista
3. Clic en "Restaurar"
4. Confirmar restauración
5. La aplicación se cierra automáticamente
6. Reiniciar la aplicación para usar datos restaurados

---

## 💡 Consejos y Mejores Prácticas

### Seguridad
- **Cambiar contraseña por defecto** inmediatamente después de la primera instalación
- **Crear backups regularmente** (diario o semanal según volumen)
- **Guardar backups en ubicación externa** (USB, nube, etc.)

### Gestión de Clientes
- **Mantener información actualizada:** Teléfonos y datos de contacto
- **Revisar recordatorios diariamente** para contactar clientes a tiempo
- **Usar búsqueda rápida** para encontrar clientes rápidamente

### Pagos
- **Registrar pagos el mismo día** que se reciben
- **Verificar fechas** al registrar pagos retroactivos
- **Exportar reportes mensuales** para contabilidad

### Backups
- **Crear backup antes de importar datos masivos**
- **Verificar backups periódicamente** para asegurar que funcionan
- **Mantener múltiples copias** en diferentes ubicaciones

---

## 🛠️ Requisitos del Sistema

### Mínimos
- **Sistema Operativo:** Windows 10 (64-bit) o superior
- **RAM:** 4 GB
- **Espacio en Disco:** 100 MB
- **.NET Runtime:** .NET 8.0 o superior

### Recomendados
- **Sistema Operativo:** Windows 11 (64-bit)
- **RAM:** 8 GB
- **Espacio en Disco:** 500 MB (para backups)
- **Resolución:** 1280x720 o superior

---

## 🛠️ Tecnologías Utilizadas

### Framework y UI
- **.NET 8.0** - Framework de desarrollo
- **Avalonia UI 11.3.10** - Framework de interfaz gráfica multiplataforma
- **Huskui.Avalonia 0.10.1** - Tema de UI moderno

### Base de Datos
- **SQLite** (System.Data.SQLite.Core 1.0.119) - Base de datos local embebida

### Seguridad
- **BCrypt.Net-Next 4.0.3** - Encriptación de contraseñas

### Exportación
- **EPPlus 7.0.0** - Generación de archivos Excel (.xlsx)

### Características
- ✅ Interfaz moderna con tema Fluent
- ✅ Base de datos local (sin servidor requerido)
- ✅ Exportación a Excel
- ✅ Sistema de autenticación seguro
- ✅ Backups automáticos
- ✅ Logs de errores y operaciones

---

## 📝 Notas Adicionales

### Base de Datos
- La base de datos es **SQLite** y se guarda localmente
- El archivo `gestor.db` contiene todos los datos
- **No requiere servidor de base de datos** adicional

### Importación de Datos
- Formato soportado: **ODS** (OpenDocument Spreadsheet)
- El archivo debe tener columnas: Nombre, Apellidos, Teléfono, etc.
- Consultar documentación de importación para formato exacto

### Exportación
- Todos los reportes se exportan en formato **Excel (.xlsx)**
- Compatible con Microsoft Excel y LibreOffice Calc
- Los archivos incluyen formato y totales calculados

---

## 🆘 Solución de Problemas

### La aplicación no inicia
- Verificar que .NET 8.0 Runtime esté instalado
- Verificar permisos de escritura en la carpeta de la aplicación

### Error de base de datos
- Verificar que el archivo `gestor.db` no esté bloqueado
- Restaurar desde un backup si es necesario

### Problemas con importación
- Verificar formato del archivo ODS
- Revisar el archivo `importacion_log.txt` para detalles

---

## 📞 Soporte

Para problemas o consultas, revisar:
- Archivos de log en la carpeta de la aplicación
- `error_log.txt` para errores de la aplicación
- `importacion_log.txt` para problemas de importación

---

## 👨‍💻 Desarrollo

### Requisitos para Desarrollo

- **.NET 8.0 SDK** (no solo Runtime)
- **Visual Studio 2022** o **JetBrains Rider** o **VS Code**
- **Windows 10/11** (64-bit)

### Compilar el Proyecto

```bash
# Restaurar dependencias
dotnet restore

# Compilar en modo Debug
dotnet build

# Compilar en modo Release
dotnet build -c Release

# Ejecutar la aplicación
dotnet run

# Publicar versión portable (self-contained)
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false
```

### Estructura del Proyecto

```
sistema_gestion_clientes_windows/
├── Data/              # Repositorios y contexto de base de datos
├── Models/            # Modelos de datos (Cliente, Pago, Usuario)
├── Services/          # Servicios de negocio
├── Views/             # Vistas de Avalonia (UI)
├── Helpers/           # Utilidades y ayudantes
├── Resources/         # Recursos (logos, iconos)
├── Program.cs         # Punto de entrada
└── GestorClientes.csproj  # Archivo de proyecto
```

### Base de Datos

- **Tipo:** SQLite
- **Archivo:** `gestor.db` (se crea automáticamente)
- **Migración:** Automática al iniciar la aplicación
- **Ubicación:** Directorio de la aplicación

### Contribuir

1. Fork el repositorio
2. Crea una rama para tu feature (`git checkout -b feature/nueva-funcionalidad`)
3. Commit tus cambios (`git commit -am 'Agrega nueva funcionalidad'`)
4. Push a la rama (`git push origin feature/nueva-funcionalidad`)
5. Abre un Pull Request

---

**Versión:** 1.0  
<<<<<<< Updated upstream
**Última actualización:** 09-01-2026
=======
**Última actualización:** Enero 2026  
**Licencia:** Ver archivo LICENSE (si aplica)
>>>>>>> Stashed changes
