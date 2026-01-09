==========================================
  SISTEMA DE GESTIÓN - VERSIÓN PORTÁTIL
  (SELF-CONTAINED - COMPLETAMENTE PORTABLE)
==========================================

Esta es la versión portátil SELF-CONTAINED de la aplicación de gestión.
Incluye .NET Runtime y todas las dependencias necesarias. NO requiere instalación.

ARCHIVOS INCLUIDOS:
-------------------
- GestorClientes.exe       : Ejecutable principal
- GestorClientes.dll       : Librería principal
- gestor.db                : Base de datos SQLite (se crea automáticamente si no existe)
- BCrypt.Net-Next.dll      : Encriptación de contraseñas
- System.Data.SQLite.dll   : Driver de SQLite
- SQLite.Interop.dll       : Interoperabilidad de SQLite
- .NET 8.0 Runtime         : Runtime completo incluido (NO requiere instalación)
- Todas las dependencias   : DLLs y librerías necesarias
- runtimes/                : Librerías nativas para SQLite y otros componentes

REQUISITOS:
-----------
- Windows 10 o superior (x64)
- NINGUNA otra dependencia necesaria
- NO requiere .NET Runtime instalado en el sistema
- NO requiere instalación de ningún componente

INSTRUCCIONES DE USO:
---------------------
1. Copia esta carpeta completa a cualquier ubicación (USB, disco duro, etc.)
2. Ejecuta GestorClientes.exe directamente desde esta carpeta
3. La base de datos se creará automáticamente al iniciar si no existe
4. Si es la primera vez, se creará un usuario administrador al iniciar
5. NO necesitas instalar nada - todo está incluido
6. Funciona en cualquier Windows 10+ (x64) sin dependencias adicionales
7. Todos tus clientes, pagos y datos están preservados

TAMAÑO APROXIMADO:
------------------
- ~100-150 MB (incluye .NET Runtime completo)
- Más grande que la versión framework-dependent, pero completamente autónoma

CARACTERÍSTICAS:
---------------
✓ Gestión completa de clientes
✓ Registro de pagos
✓ Sistema de recordatorios de vencimientos
✓ Dashboard con métricas clickables
✓ Reportes y resúmenes
✓ Sistema de backups
✓ Interfaz responsive
✓ Fecha de alta editable
✓ Seguimiento de último pago
✓ Cálculo automático de vencimientos

NOTAS IMPORTANTES:
------------------
- La base de datos (gestor.db) se guarda en la misma carpeta donde está el ejecutable
- Realiza backups regularmente usando la función de backups integrada
- NO elimines ningún archivo de esta carpeta - todos son necesarios para el funcionamiento
- La carpeta "Backups" se creará automáticamente cuando uses la función de backup
- Esta versión es completamente portable: copia la carpeta y funciona en cualquier PC
- No modifiques ni elimines archivos .dll, .exe o carpetas runtimes/
- El .NET Runtime está incluido, por lo que no necesitas instalarlo en el sistema

VERSIÓN:
--------
- Tipo: Self-Contained (incluye .NET Runtime)
- Plataforma: Windows x64
- Compilada con todos los cambios recientes:
  - FechaAlta editable
  - FechaUltimoPago implementada
  - UI responsive
  - Dashboard clickable con métricas interactivas
  - Dashboard optimizado para pantalla completa
  - Solución de overlap en búsqueda
  - Diseño responsivo mejorado con mejor uso del espacio
  - Corrección del texto del contador "Próximos a Vencer" (ya no se corta)

VENTAJAS DE ESTA VERSIÓN:
--------------------------
✓ Completamente portable - copia y ejecuta
✓ No requiere instalación de .NET Runtime
✓ Funciona en cualquier Windows 10+ sin dependencias
✓ Versión aislada del sistema
✓ Ideal para usar desde USB o carpeta compartida

==========================================
¡Listo para usar!
==========================================

