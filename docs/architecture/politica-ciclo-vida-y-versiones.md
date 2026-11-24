# Política de ciclo de vida y versiones

Versión 0.1 · Propuesta de arranque · 6 de noviembre de 2026.

Las entidades se gobiernan con estados de negocio y versiones explícitas. Archivar contenido, desactivar una cuenta, cancelar un pedido y suprimir datos personales son operaciones diferentes. Un campo genérico IsDeleted no sustituye esas reglas.

| Recurso | Retirada ordinaria | Evidencia que se conserva | Eliminación física |
| --- | --- | --- | --- |
| Cuenta o membresía | Desactivar o revocar | Autoría y referencias permitidas | Mediante proceso de privacidad y reglas de integridad |
| Producto | Archivar o despublicar | Instantáneas de pedidos | Solo si no hay uso o al aplicar una política de conservación válida |
| Comunicación o página | Archivar, retirar o papelera | Historial y relaciones | Revisión de referencias y conservación |
| Modelo de autorización | Crear otra versión vigente | Todas las versiones utilizadas | No se elimina por sustituir el modelo |
| Respuesta | Revisar, pedir corrección, validar o rechazar | Versiones y documentos de cada entrega | Proceso de fin de conservación |
| Pedido | Uno de sus seis estados | Precio informativo, adjuntos e historial | Proceso de fin de conservación |
| Archivo preparado sin vínculo | Expirar preparación | Evento técnico mínimo | Limpieza controlada tras plazo técnico acordado |
| Evento de auditoría | Consulta restringida | Evento mínimo | Tarea de conservación con identidad específica |

## Correcciones de autorizaciones

El propietario solo corrige una respuesta marcada Necesita corrección, dentro del plazo de la actividad. El participante del expediente se conserva, se añade una versión y la respuesta vuelve a Recibida. Los demás participantes no cambian. El modelo utilizado se registra cuando se conoce y se conserva desconocido en históricos sin evidencia.

Sustituir el modelo PDF requiere confirmación y crea una versión inmutable. No invalida entregas existentes ni inicia automáticamente una nueva firma. Si el cambio exige una nueva entrega, el responsable solicita corrección de los expedientes afectados con su motivo.

## Pedidos 1 29

El gestor puede elegir cualquiera de los estados Solicitado, Revisado, Pedido al proveedor, Entregado, Rechazado y Cancelado, incluso para rectificar o reabrir. Cada cambio deja historial y usa control de concurrencia. No se impone una secuencia económica.

El titular cancela solo desde Solicitado o Revisado y con motivo. Puede subir o sustituir justificante salvo en Rechazado o Cancelado. El documento es independiente del estado y la entrega no exige adjunto. No existen acciones de aceptación de importe ni de verificación de pago.

## Referencias compartidas y borrado

Un justificante compartido de actividad mantiene varios vínculos a un objeto inmutable. Retirar una referencia no elimina el archivo mientras quede otra referencia que deba conservarse, incluida una histórica. La eliminación comprueba relaciones dentro de una operación protegida y no decide por un contador de memoria no transaccional.

El fin de conservación puede requerir bloqueo, supresión o anonimización conforme a una decisión documentada. Se revisan también objetos versionados, derivados, réplicas y copias. La política jurídica y los plazos se concretan en [privacidad y conservación](../requisitos/privacidad-y-conservacion.md).
