// El catálogo de textos (Textos.Idioma) es estado estático del proceso: las clases que lo
// cambian (TextosTests) y las que leen la salida formateada (FichaTextoTests,
// ComparadorTextoTests) no pueden ejecutarse en paralelo o compiten por el idioma activo.
//
// xunit v2 no lee la propiedad MSBuild `ParallelizeTestCollections` (eso es de v3): el
// mecanismo válido es este atributo de ensamblado. Sin él, la suite pasaba en Windows por
// suerte de "timing" y fallaba en el runner de Linux.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
