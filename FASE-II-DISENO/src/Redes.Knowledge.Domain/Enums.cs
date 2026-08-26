namespace Redes.Knowledge.Domain;

/// <summary>Estados de ciclo de vida (F0 — Ejes de clasificación, §3).</summary>
public enum LifecycleState
{
    Vigente,
    Actualizado,
    Obsoleto,
    Sustituido,
    Experimental,
    Propietario,
    Restringido,
    Historico,
    Desconocido
}

/// <summary>Jerarquía de evidencia (F0 — Política de fuentes, §1).</summary>
public enum NivelAutoridad
{
    PrimariaNormativa = 1,
    PrimariaImplementacion = 2,
    SecundariaEspecializada = 3,
    Terciaria = 4
}

/// <summary>Grados de confianza (F0 — Política de incertidumbre, §2).</summary>
public enum Confianza
{
    Alto,
    Medio,
    Bajo,
    Desconocido
}

/// <summary>Relaciones tipadas entre entidades (plan, sección 6.1).</summary>
public enum RelacionTipo
{
    Encapsula,
    CorreSobre,
    DependeDe,
    EsVersionDe,
    SustituyeA,
    Implementa,
    Documenta
}

/// <summary>Las 13 familias funcionales de protocolos (F0 — Ejes, §2).</summary>
public enum FamiliaProtocolo
{
    ACEL,   // Acceso y enlace
    ADCONF, // Direccionamiento, descubrimiento y configuración
    ROUT,   // Routing y forwarding
    MOV,    // Movilidad
    TRAN,   // Transporte y sesión
    APP,    // Aplicación
    GEST,   // Gestión, monitorización y operaciones
    SYNC,   // Sincronización temporal
    STOR,   // Almacenamiento/red y automatización
    SEG,    // Seguridad
    IOT,    // IoT/OT y tiempo real
    RAD,    // Radio/móvil y satélite
    HIST    // Históricos y de transición
}