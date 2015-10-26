using LangDetector.Core.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace LangDetector.Core
{
    class Repositorio
    {
        public static async Task<long> Insertar(Idioma documento)
        {
            const string insert = @"INSERT INTO IDIOMAS (NOMBRE, LETRAS, SIGNOS, SIMBOLOS, PALABRAS) VALUES (@nombre, @letras, @signos, @simbolos, @palabras)";
            const string selectId = @"SELECT last_insert_rowid() FROM IDIOMAS";

            using (var conexion = await BaseDeDatos.AbrirConexionAsync())
            {
                await conexion.ExecuteAsync(insert, documento);

                var id = await conexion.QueryAsync<long>(selectId);
                return id.FirstOrDefault();
            }
        }

        public static async Task<long> Insertar(Documento documento)
        {
            const string insert = @"INSERT INTO DOCUMENTOS (HASH, LETRAS, SIGNOS, SIMBOLOS, PALABRAS) VALUES (@hash, @letras, @signos, @simbolos, @palabras)";
            const string selectId = @"SELECT last_insert_rowid() FROM DOCUMENTOS";

            using (var conexion = await BaseDeDatos.AbrirConexionAsync())
            {
                await conexion.ExecuteAsync(insert, documento);

                var id = await conexion.QueryAsync<long>(selectId);
                return id.FirstOrDefault();
            }
        }

        public static async Task Insertar(IEnumerable<DocumentoLetra> letras)
        {
            const string insert = @"INSERT INTO DOCUMENTOS_LETRAS (DOCUMENTO_ID, LETRA_ID, CANTIDAD, PORCENTAJE, TIPO) VALUES (@documentoId, @id, @cantidad, @porcentaje, @tipo)";
            using (var conexion = await BaseDeDatos.AbrirConexionAsync())
            {
                await conexion.ExecuteAsync(insert, letras);
            }
        }

        public static async Task<long> ContarIdiomasConocidos()
        {
            const string select = @"SELECT COUNT(*) FROM IDIOMAS WHERE LETRAS > 0 OR SIMBOLOS > 0 OR SIGNOS > 0";

            using (var conexion = await BaseDeDatos.AbrirConexionAsync())
            {
                var cantidad = await conexion.QueryAsync<long>(select);
                return cantidad.FirstOrDefault();
            }
        }

        internal static async Task<Idioma> ObtenerPrimerIdioma()
        {
            const string select = "SELECT IDIOMA_ID, NOMBRE, LETRAS, SIGNOS, SIMBOLOS, PALABRAS FROM IDIOMAS";

            using (var conexion = await BaseDeDatos.AbrirConexionAsync())
            {
                var result = await conexion.QueryAsync<Idioma>(select);
                return result.FirstOrDefault();
            }
        }

        public static async Task CopiarLetrasNuevas(Documento documento, Idioma idioma)
        {
            const string insertSelect = @"
            INSERT INTO IDIOMAS_LETRAS (IDIOMA_ID, LETRA_ID, CANTIDAD, PORCENTAJE, TIPO)
            SELECT 
                @idiomaId IDIOMA_ID, LETRA_ID, CANTIDAD, PORCENTAJE, TIPO
            FROM 
                DOCUMENTOS_LETRAS 
            WHERE 
                DOCUMENTO_ID = @documentoId
                AND NOT LETRA_ID IN (SELECT LETRA_ID FROM IDIOMAS_LETRAS WHERE IDIOMA_ID = @idiomaId)
            ";

            using (var conexion = await BaseDeDatos.AbrirConexionAsync())
            {
                await conexion.ExecuteAsync(insertSelect, new { documentoId = documento.Id, idiomaId = idioma.Id });
            }
        }

        internal static async Task Asociar(Documento documento, Idioma idioma)
        {
            documento.IdiomaId = idioma.Id;

            const string update = @"UPDATE DOCUMENTOS SET IDIOMA_ID = @idiomaId, CONFIANZA = @confianza WHERE DOCUMENTO_ID = @id";

            using (var conexion = await BaseDeDatos.AbrirConexionAsync())
            {
                await conexion.ExecuteAsync(update, documento);
            }

        }

        internal static async Task Actualizar(DocumentoLetra letra)
        {
            const string update = "UPDATE DOCUMENTOS_LETRAS SET CANTIDAD = @cantidad, PORCENTAJE = @porcentaje WHERE DOCUMENTO_ID = @documentoId AND LETRA_ID = @id";

            using (var conexion = await BaseDeDatos.AbrirConexionAsync())
            {
                await conexion.ExecuteAsync(update, letra);
            }
        }

        internal static async Task<DocumentoPalabra> ObtenerPalabra(string palabra, Documento documento)
        {
            const string select = "SELECT PALABRA, DOCUMENTO_ID DocumentoId, CANTIDAD, PORCENTAJE FROM DOCUMENTOS_PALABRAS WHERE DOCUMENTO_ID = @documentoId AND PALABRA = @palabra";
            using (var conexion = await BaseDeDatos.AbrirConexionAsync())
            {
                var result = await conexion.QueryAsync<DocumentoPalabra>(select, new { palabra, documentoId = documento.Id });
                return result.FirstOrDefault();
            }
        }

        internal async static Task<DocumentoLetra> ObtenerLetra(char letraId, Documento documento)
        {
            const string select = "SELECT LETRA_ID Id, DOCUMENTO_ID DocumentoId, CANTIDAD, PORCENTAJE, TIPO FROM DOCUMENTOS_LETRAS WHERE DOCUMENTO_ID = @documentoId AND LETRA_ID = @letraId";
            using (var conexion = await BaseDeDatos.AbrirConexionAsync())
            {
                var result = await conexion.QueryAsync<DocumentoLetra>(select, new { letraId = (int)letraId, documentoId = documento.Id });
                return result.FirstOrDefault();
            }
        }

        internal static async Task Actualizar(DocumentoPalabra palabra)
        {
            const string update = "UPDATE DOCUMENTOS_PALABRAS SET CANTIDAD = @Cantidad, PORCENTAJE = @Porcentaje WHERE DOCUMENTO_ID = @DocumentoId AND PALABRA = @Palabra";

            using (var conexion = await BaseDeDatos.AbrirConexionAsync())
            {
                var result = await conexion.ExecuteAsync(update, new { palabra.Cantidad, palabra.Porcentaje, palabra.DocumentoId, Palabra = new DbString { Value = palabra.Palabra, IsAnsi = true } });
                System.Diagnostics.Debug.WriteLine("Update " + result.ToString());
            }
        }

        internal static async Task ActualizarPorcentajeEnLetras(Idioma idioma)
        {
            const string update = @"
            UPDATE IDIOMAS_LETRAS 
                SET PORCENTAJE = CANTIDAD / 
                    CASE TIPO 
                    WHEN 1
                        CASE IDIOMAS.LETRAS WHEN 0 THEN 0 ELSE CANTIDAD / IDIOMAS.LETRAS END
                    WHEN 2
                        CASE IDIOMAS.SIGNOS WHEN 0 THEN 0 ELSE CANTIDAD / IDIOMAS.SIGNOS END
                    WHEN 3
                        CASE IDIOMAS.SIMBOLOS WHEN 0 THEN 0 ELSE CANTIDAD / IDIOMAS.SIMBOLOS END
                    END
            FROM 
                IDIOMAS 
            WHERE 
                IDIOMA_ID = @idiomaId";

            using (var conexion = await BaseDeDatos.AbrirConexionAsync())
            {
                await conexion.ExecuteAsync(update, new { idiomaId = idioma.Id });
            }
        }

        internal static async Task ActualizarPorcentajeEnPalabras(Idioma idioma)
        {
            const string update = @"
            UPDATE IDIOMAS_PALABRAS
                SET PORCENTAJE = CASE IDIOMAS.PALABRAS WHEN 0 THEN 0 ELSE CANDIDAD / IDIOMAS.PALABRAS END
            FROM 
                IDIOMAS 
            WHERE 
                IDIOMA_ID = @idiomaId";

            using (var conexion = await BaseDeDatos.AbrirConexionAsync())
            {
                await conexion.ExecuteAsync(update, new { idiomaId = idioma.Id });
            }
        }


        internal static async Task Actualizar(Documento documento)
        {
            const string update = "UPDATE DOCUMENTOS SET LETRAS = @letras, PALABRAS = @palabras, SIGNOS = @signos, SIMBOLOS = @simbolos, HASH = @hash WHERE DOCUMENTO_ID = @id";

            using (var conexion = await BaseDeDatos.AbrirConexionAsync())
            {
                await conexion.ExecuteAsync(update, documento);
            }
        }

        internal static async Task ActualizarPorcentajeEnPalabras(Documento documento)
        {
            const string update = @"
            UPDATE DOCUMENTOS_PALABRAS
                SET PORCENTAJE = CANTIDAD / @palabras
            WHERE 
                DOCUMENTO_ID = @id";

            using (var conexion = await BaseDeDatos.AbrirConexionAsync())
            {
                await conexion.ExecuteAsync(update, documento);
            }
        }

        internal static async Task ActualizarPorcentajeEnLetras(Documento documento)
        {
            const string update = @"
            UPDATE DOCUMENTOS_LETRAS 
                SET PORCENTAJE = 
                    CASE TIPO 
                    WHEN 1 THEN
                        CASE @letras WHEN 0 THEN 0 ELSE CANTIDAD / @letras END
                    WHEN 2 THEN
                        CASE @signos WHEN 0 THEN 0 ELSE CANTIDAD / @signos END
                    ELSE
                        CASE @simbolos WHEN 0 THEN 0 ELSE CANTIDAD / @simbolos END
                    END
            WHERE 
                DOCUMENTO_ID = @id";

            using (var conexion = await BaseDeDatos.AbrirConexionAsync())
            {
                await conexion.ExecuteAsync(update, documento);
            }
        }

        internal static async Task Insertar(IEnumerable<DocumentoPalabra> palabras)
        {
            const string update = "INSERT INTO DOCUMENTOS_PALABRAS (PALABRA, DOCUMENTO_ID, CANTIDAD, PORCENTAJE) VALUES (@palabra, @documentoId, @cantidad, @porcentaje)";

            using (var conexion = await BaseDeDatos.AbrirConexionAsync())
            {
                await conexion.ExecuteAsync(update, palabras);
            }
        }

        internal static async Task ActualizarTotales(Idioma idioma)
        {
            const string update = @"
            UPDATE IDIOMAS SET 
                LETRAS = (SELECT SUM(CANTIDAD) FROM IDIOMAS_LETRAS WHERE IDIOMA_ID = @idiomaId AND TIPO = 1),
                SIGNOS = (SELECT SUM(CANTIDAD) FROM IDIOMAS_LETRAS WHERE IDIOMA_ID = @idiomaId AND TIPO = 2),
                SIMBOLOS = (SELECT SUM(CANTIDAD) FROM IDIOMAS_LETRAS WHERE IDIOMA_ID = @idiomaId AND TIPO = 3),
                PALABRAS = (SELECT SUM(CANTIDAD) FROM IDIOMAS_PALABRAS WHERE IDIOMA_ID = @idiomaId)";

            using (var conexion = await BaseDeDatos.AbrirConexionAsync())
            {
                await conexion.ExecuteAsync(update, new { idiomaId = idioma.Id });
            }
        }
        
        internal async static Task ActualizarLetrasExistentes(Documento documento, Idioma idioma)
        {
            const string insertSelect = @"
            UPDATE IDIOMAS_LETRAS IL
                SET IL.CANTIDAD = IL.CANTIDAD + DL.CANTIDAD
            FROM
                DOCUMENTOS_LETRAS DL
            WHERE 
                IL.LETRA_ID = DL.LETRA_ID
                AND IL.IDIOMA_ID = @idiomaId
                AND DL.DOCUMENTO_ID = @documentoId
            ";

            using (var conexion = await BaseDeDatos.AbrirConexionAsync())
            {
                await conexion.ExecuteAsync(insertSelect, new { documentoId = documento.Id, idiomaId = idioma.Id });
            }
        }

        public static async Task EliminarLetras(Documento documento)
        {
            const string delete = @"DELETE DOCUMENTOS_LETRAS WHERE DOCUMENTO_ID = @documentoId";

            using (var conexion = await BaseDeDatos.AbrirConexionAsync())
            {
                await conexion.ExecuteAsync(delete, new { documentoId = documento.Id });
            }
        }
    }
}
