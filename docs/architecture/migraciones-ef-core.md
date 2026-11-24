# Migraciones EF Core y evolución del esquema

Versión 0.1 · Propuesta de arranque · 6 de noviembre de 2026.

EF Core Migrations será el propietario del esquema del piloto. Una base, un contexto propietario L330DbContext y una línea de migraciones evitan que dos mecanismos intenten modificar las mismas tablas.

## Creación de la primera línea

En E1 se fijarán proveedor, versión compatible de EF y herramientas locales; las migraciones vivirán en `L330.Infrastructure/Persistence/Migrations`. Las opciones del contexto se resolverán sin arrancar la aplicación ni enviar correos. Las pruebas comprobarán construcción desde cero y actualización de la versión anterior.

No se creará una línea host y otra tenant como en GTS.ERP. Añadir otra base o contexto en el futuro exigirá una ADR que identifique propietario, esquema e historial para evitar gobernar dos veces las mismas tablas.

## Procedimiento de cambio

1. Partir de la base integrada del repositorio y conocer las migraciones aplicadas en el entorno de prueba.
2. Modificar modelo y configuraciones del módulo; generar la migración con EF Core.
3. Revisar renombres, nulabilidad, índices, FKs de organización, referencias compartidas y SQL generado.
4. Probar una base vacía y una copia sintética de la versión anterior, incluidos datos históricos y ficheros vinculados.
5. Generar el artefacto de migración para la candidata; registrar identificador, commit y checksum.
6. Aplicarlo mediante identidad de despliegue con capacidad de cambiar esquema y conservar el resultado.

Una migración aplicada no se edita retrospectivamente. Dos cambios de esquema se integran en serie y la segunda migración se regenera sobre el modelo ya integrado. Cada rama usará una base de prueba propia cuando puedan interferir sus migraciones.

## Comandos previstos

Se utilizarán después de crear los proyectos y fijar la herramienta local `dotnet-ef` en E1:

```powershell
dotnet tool restore
dotnet ef migrations add NombreDelCambio --project src/L330.Infrastructure --startup-project src/L330.Web --context L330DbContext --output-dir Persistence/Migrations
dotnet ef migrations script --idempotent --project src/L330.Infrastructure --startup-project src/L330.Web --context L330DbContext --output work/migrations.sql
```

El script SQL se revisa, pero no se interpreta como aplicado. Un bundle puede ser el artefacto ejecutable de despliegue si ADR09 confirma esa opción. La [documentación oficial de EF Core](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying) describe ambas estrategias. El SQL del script es un artefacto de esquema y no una puerta para hacer consultas de negocio desde Web.

## Arranque y datos iniciales

El arranque web comprobará compatibilidad y fallará de forma diagnóstica si falta una migración requerida; no ejecutará cambios de esquema de producción en cada login. Los seeds serán explícitos, idempotentes y separados del esquema. Crear una cuenta inicial necesitará un mecanismo de invitación o secreto externo, nunca una contraseña demo fijada en el repositorio.

## Reversión

La estrategia preferente de evolución es expandir, migrar datos y contraer en otra candidata. Un Down que elimina columnas o versiones no constituye una recuperación segura. Se probará la restauración coordinada de base, objetos y claves y se determinará la compatibilidad del código anterior con el esquema nuevo. Tras escrituras en la nueva versión, la reversión requiere conciliar esas operaciones.
