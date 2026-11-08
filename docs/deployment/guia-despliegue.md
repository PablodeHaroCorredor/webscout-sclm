# Guía de despliegue y promoción

Versión 0.1 · Propuesta de arranque · 6 de noviembre de 2026.

No hay infraestructura creada ni despliegue ejecutado. Esta guía establece el contrato operativo de E1 a E5; DC03 y DC08 deben fijar proveedor, coste, nombres y responsables antes de concretar comandos.

## Entornos e identidades

Desarrollo usa datos sintéticos, almacén aislado y correo capturado. Validación reproduce versiones y servicios de producción con datos sintéticos o una copia anonimizada autorizada. Producción se habilita tras aceptación. Bases, objetos, claves, buzones y secretos quedan separados por entorno.

La identidad de ejecución tiene acceso mínimo a datos y prefijos privados y no necesita alterar esquema. La identidad de migración es distinta, acotada al despliegue. Los operadores usan cuentas nominativas y acceso protegido. La aplicación conserva sus claves de protección de datos en almacenamiento durable restringido y compartido solo por sus réplicas del mismo entorno.

## Artefacto

Construir desde commit identificado, con SDK y paquetes fijados. Conservar resultado de pruebas, versión, inventario y artefacto publicado; promover ese mismo artefacto entre entornos. Los secretos se inyectan fuera del paquete. El esquema se aplica por el procedimiento de [migraciones](../architecture/migraciones-ef-core.md), sin que varias instancias lo alteren al arrancar.

## Secuencia de una promoción

1. Registrar candidato, alcance, ventanas, configuración versionada sin secretos y responsable.
2. Comprobar copia coordinada y recuperación disponible, compatibilidad del esquema y de trabajadores.
3. Aplicar migración revisada con identidad de despliegue y guardar resultado.
4. Instalar la aplicación y worker; mantener la escritura cerrada durante comprobaciones incompatibles.
5. Verificar salud de proceso, base, objetos y cola; login y archivo sintético permitido/denegado.
6. Ejecutar humo de actividades, pedidos y fotos del alcance; comprobar que correo usa proveedor correcto.
7. Habilitar tráfico/escritura y observar errores, latencia y trabajos. Registrar aceptación o retorno.

La reversión de código solo procede si es compatible con el esquema actual. Un Down destructivo no es un plan de recuperación. Si se restaura una copia, se contabilizan operaciones posteriores y se aplica la política de conciliación, especialmente durante el corte desde WordPress.

## Condiciones técnicas de la instalación

TLS, host permitido, cabeceras de proxy confiables y cookies seguras se fijan según topología real. No confiar cabeceras de cualquier origen. Los recursos estáticos públicos se separan de objetos privados; no se expone listado del almacén ni URL permanente anónima. Derivados, cachés y CDN deben respetar el alcance de lectura y la retirada.

Los tamaños, tiempos y límites de subida se coordinan entre proxy, aplicación y almacenamiento. Deben aceptar los archivos del contrato funcional sin habilitar cargas ilimitadas. Las miniaturas usan la misma autorización que el original. El worker usa el ámbito de organización del trabajo y límites de reintento/concurrencia.

Los endpoints de salud no revelan secretos ni contenido de datos. Monitorizar proceso vivo no prueba capacidad de descargar o tramitar; añadir sondas sintéticas acotadas. El proveedor de correo, DNS del remitente y alertas se validan en entorno de prueba antes del uso autorizado real.

## Evidencia de salida

Commit, hash de artefacto, versión de esquema, entorno, fecha, operador, pruebas, resultado, incidencias y decisión. La checklist ejecutada enlaza esa evidencia en [Estado](../estado-proyecto.md). El manual de [operación](operacion-y-recuperacion.md) identifica cómo mantener y recuperar el servicio.
