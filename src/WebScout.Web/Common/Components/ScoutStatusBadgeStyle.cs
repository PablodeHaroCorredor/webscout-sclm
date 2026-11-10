using WebScout.Web.Features.Activities.Domain;
using WebScout.Web.Features.Admissions.Domain;
using WebScout.Web.Features.Inscriptions.Domain;
using WebScout.Web.Features.Orders.Domain;
using WebScout.Web.Infrastructure.Outbox.Domain;

namespace WebScout.Web.Common.Components;

public record ScoutStatusBadgeStyle(
    string Text,
    string BackgroundColorHex,
    string TextColorHex,
    string BorderColorHex,
    string DotColorHex
);

public static class ScoutStatusBadgeStyleResolver
{
    public static readonly ScoutStatusBadgeStyle EstiloExito = new(
        Text: "Completado",
        BackgroundColorHex: "#F0FDF4",
        TextColorHex: "#166534",
        BorderColorHex: "#BBF7D0",
        DotColorHex: "#16A34A"
    );

    public static readonly ScoutStatusBadgeStyle EstiloTramite = new(
        Text: "En Trámite",
        BackgroundColorHex: "#EFF6FF",
        TextColorHex: "#1D4ED8",
        BorderColorHex: "#BFDBFE",
        DotColorHex: "#2563EB"
    );

    public static readonly ScoutStatusBadgeStyle EstiloAtencion = new(
        Text: "Requiere Atención",
        BackgroundColorHex: "#FFFBEB",
        TextColorHex: "#92400E",
        BorderColorHex: "#FDE68A",
        DotColorHex: "#D97706"
    );

    public static readonly ScoutStatusBadgeStyle EstiloGestionExterna = new(
        Text: "Gestión Externa",
        BackgroundColorHex: "#FAF5FF",
        TextColorHex: "#6D28D9",
        BorderColorHex: "#DDD6FE",
        DotColorHex: "#7C3AED"
    );

    public static readonly ScoutStatusBadgeStyle EstiloNegativo = new(
        Text: "Cancelado",
        BackgroundColorHex: "#FEF2F2",
        TextColorHex: "#991B1B",
        BorderColorHex: "#FECACA",
        DotColorHex: "#DC2626"
    );

    public static readonly ScoutStatusBadgeStyle EstiloNeutral = new(
        Text: "Inactivo",
        BackgroundColorHex: "#F8FAFC",
        TextColorHex: "#334155",
        BorderColorHex: "#E2E8F0",
        DotColorHex: "#64748B"
    );

    public static ScoutStatusBadgeStyle Resolver(object? estado, string? textoSobrescrito = null)
    {
        var estiloBase = estado switch
        {
            EstadoInscripcion inscripcion => ResolverInscripcion(inscripcion),
            EstadoPedidoMaterial pedido => ResolverPedido(pedido),
            EstadoSolicitudAdmision admision => ResolverAdmision(admision),
            EstadoActividad actividad => ResolverActividad(actividad),
            EstadoMensajeOutbox outbox => ResolverOutbox(outbox),
            _ => EstiloNeutral with { Text = estado?.ToString() ?? "Sin estado" }
        };

        return string.IsNullOrWhiteSpace(textoSobrescrito)
            ? estiloBase
            : estiloBase with { Text = textoSobrescrito };
    }

    private static ScoutStatusBadgeStyle ResolverInscripcion(EstadoInscripcion estado) => estado switch
    {
        EstadoInscripcion.PendienteJustificante => EstiloAtencion with { Text = "Pendiente de Justificante" },
        EstadoInscripcion.EnRevision => EstiloAtencion with { Text = "En Revisión" },
        EstadoInscripcion.Confirmada => EstiloExito with { Text = "Confirmada" },
        EstadoInscripcion.RequiereSubsanacion => EstiloNegativo with { Text = "Requiere Subsanación" },
        EstadoInscripcion.Cancelada => EstiloNegativo with { Text = "Cancelada" },
        _ => EstiloNeutral with { Text = estado.ToString() }
    };

    private static ScoutStatusBadgeStyle ResolverPedido(EstadoPedidoMaterial estado) => estado switch
    {
        EstadoPedidoMaterial.Solicitado => EstiloTramite with { Text = "Solicitado" },
        EstadoPedidoMaterial.Revisado => EstiloAtencion with { Text = "Revisado" },
        EstadoPedidoMaterial.PedidoAlProveedor => EstiloGestionExterna with { Text = "En Proveedor" },
        EstadoPedidoMaterial.Entregado => EstiloExito with { Text = "Entregado" },
        EstadoPedidoMaterial.Rechazado => EstiloNegativo with { Text = "Rechazado" },
        EstadoPedidoMaterial.Cancelado => EstiloNegativo with { Text = "Cancelado" },
        _ => EstiloNeutral with { Text = estado.ToString() }
    };

    private static ScoutStatusBadgeStyle ResolverAdmision(EstadoSolicitudAdmision estado) => estado switch
    {
        EstadoSolicitudAdmision.Pendiente => EstiloTramite with { Text = "Pendiente" },
        EstadoSolicitudAdmision.EnListaEspera => EstiloGestionExterna with { Text = "En Lista de Espera" },
        EstadoSolicitudAdmision.Admitida => EstiloExito with { Text = "Admitida" },
        EstadoSolicitudAdmision.Rechazada => EstiloNegativo with { Text = "Rechazada" },
        EstadoSolicitudAdmision.Archivada => EstiloNeutral with { Text = "Archivada" },
        _ => EstiloNeutral with { Text = estado.ToString() }
    };

    private static ScoutStatusBadgeStyle ResolverActividad(EstadoActividad estado) => estado switch
    {
        EstadoActividad.Borrador => EstiloNeutral with { Text = "Borrador" },
        EstadoActividad.Publicada => EstiloTramite with { Text = "Publicada" },
        EstadoActividad.InscripcionesCerradas => EstiloAtencion with { Text = "Inscripciones Cerradas" },
        EstadoActividad.EnCurso => EstiloTramite with { Text = "En Curso" },
        EstadoActividad.Finalizada => EstiloExito with { Text = "Finalizada" },
        EstadoActividad.Cancelada => EstiloNegativo with { Text = "Cancelada" },
        _ => EstiloNeutral with { Text = estado.ToString() }
    };

    private static ScoutStatusBadgeStyle ResolverOutbox(EstadoMensajeOutbox estado) => estado switch
    {
        EstadoMensajeOutbox.Pendiente => EstiloTramite with { Text = "Pendiente" },
        EstadoMensajeOutbox.EnProceso => EstiloTramite with { Text = "En Proceso" },
        EstadoMensajeOutbox.Enviado => EstiloExito with { Text = "Enviado" },
        EstadoMensajeOutbox.Fallido => EstiloNegativo with { Text = "Fallido" },
        EstadoMensajeOutbox.Descartado => EstiloNeutral with { Text = "Descartado" },
        _ => EstiloNeutral with { Text = estado.ToString() }
    };

    public static IEnumerable<ScoutStatusBadgeStyle> ObtenerTodosLosEstilos() => new[]
    {
        EstiloExito,
        EstiloTramite,
        EstiloAtencion,
        EstiloGestionExterna,
        EstiloNegativo,
        EstiloNeutral
    };
}
