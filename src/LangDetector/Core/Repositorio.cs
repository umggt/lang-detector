using Dapper;
using LangDetector.Core.Modelos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;

namespace LangDetector.Core
{
    class Repositorio : IDisposable
    {
        private readonly IDbConnection conexion;

        public Repositorio()
        {
            conexion = BaseDeDatos.AbrirConexion();
        }

        public async Task<long> Insertar(Idioma documento)
        {
            const string insert = @"INSERT INTO IDIOMAS (NOMBRE, LETRAS, SIGNOS, SIMBOLOS, PALABRAS, LETRAS_DISTINTAS, SIGNOS_DISTINTOS, SIMBOLOS_DISTINTOS, PALABRAS_DISTINTAS) VALUES (@nombre, @letras, @signos, @simbolos, @palabras, @letrasDistintas, @signosDistintos, @simbolosDistintos, @palabrasDistintas)";
            const string selectId = @"SELECT last_insert_rowid() FROM IDIOMAS";


            await conexion.ExecuteAsync(insert, documento);

            var id = await conexion.QueryAsync<long>(selectId);
            return id.FirstOrDefault();
        }

        public async Task<long> Insertar(Documento documento)
        {
            const string insert = @"
            INSERT INTO DOCUMENTOS (
                HASH, 
                LETRAS,
                LETRAS_DISTINTAS,
                SIGNOS, 
                SIGNOS_DISTINTOS,
                SIMBOLOS, 
                SIMBOLOS_DISTINTOS,
                PALABRAS,
                PALABRAS_DISTINTAS
            ) 
            VALUES (
                @hash, 
                @letras, 
                @letrasDistintas,
                @signos, 
                @signosDistintos,
                @simbolos,
                @simbolosDistintos,
                @palabras,
                @palabrasDistintas
            )";

            const string selectId = @"SELECT last_insert_rowid() FROM DOCUMENTOS";


            await conexion.ExecuteAsync(insert, documento);

            var id = await conexion.QueryAsync<long>(selectId);
            return id.FirstOrDefault();
        }

        internal IEnumerable<Idioma> ObtenerIdiomas()
        {
            const string select = @"SELECT ID, NOMBRE, LETRAS, LETRAS_DISTINTAS LetrasDistintas, SIGNOS, SIGNOS_DISTINTOS SignosDistintos, SIMBOLOS, SIMBOLOS_DISTINTOS SimbolosDistintos FROM IDIOMAS ORDER BY NOMBRE";
            
            var resultado = conexion.Query<Idioma>(select);
            return resultado.ToArray();
        }

        public async Task Insertar(IEnumerable<DocumentoLetra> letras)
        {
            const string insert = @"INSERT INTO DOCUMENTOS_LETRAS (DOCUMENTO_ID, LETRA_ID, CANTIDAD, PORCENTAJE, TIPO) VALUES (@documentoId, @id, @cantidad, @porcentaje, @tipo)";
            await conexion.ExecuteAsync(insert, letras);
        }

        public async Task Insertar(DocumentoLetra letra)
        {
            const string insert = @"INSERT INTO DOCUMENTOS_LETRAS (DOCUMENTO_ID, LETRA_ID, CANTIDAD, PORCENTAJE, TIPO) VALUES (@documentoId, @id, @cantidad, @porcentaje, @tipo)";

            await conexion.ExecuteAsync(insert, letra);
        }

        public async Task<long> ContarIdiomasConocidos()
        {
            const string select = @"SELECT COUNT(*) FROM IDIOMAS WHERE LETRAS > 0 OR SIMBOLOS > 0 OR SIGNOS > 0";


            var cantidad = await conexion.QueryAsync<long>(select);
            return cantidad.FirstOrDefault();
        }

        public async Task CopiarLetrasNuevas(Documento documento, Idioma idioma)
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


            await conexion.ExecuteAsync(insertSelect, new { documentoId = documento.Id, idiomaId = idioma.Id });
        }

        public async Task CopiarPalabrasNuevas(Documento documento, Idioma idioma)
        {
            const string insertSelect = @"
            INSERT INTO IDIOMAS_PALABRAS (IDIOMA_ID, PALABRA, CANTIDAD, PORCENTAJE)
            SELECT 
                @idiomaId IDIOMA_ID, PALABRA, CANTIDAD, PORCENTAJE
            FROM 
                DOCUMENTOS_PALABRAS
            WHERE 
                DOCUMENTO_ID = @documentoId
                AND NOT PALABRA IN (SELECT PALABRA FROM IDIOMAS_PALABRAS WHERE IDIOMA_ID = @idiomaId)
            ";


            await conexion.ExecuteAsync(insertSelect, new { documentoId = documento.Id, idiomaId = idioma.Id });
        }

        internal async Task EliminarPalabras(Documento documento)
        {
            const string delete = @"DELETE FROM DOCUMENTOS_PALABRAS WHERE DOCUMENTO_ID = @documentoId";
            await conexion.ExecuteAsync(delete, new { documentoId = documento.Id });
        }

        internal async Task<Documento> ObtenerDocumento(string hash, int palabrasDistintas)
        {
            const string select = @"
            SELECT 
                ID, 
                HASH, 
                LETRAS,
                LETRAS_DISTINTAS LetrasDistintas,
                SIGNOS, 
                SIGNOS_DISTINTOS SignosDistintos,
                SIMBOLOS, 
                SIMBOLOS_DISTINTOS SimbolosDistintos,
                PALABRAS, 
                PALABRAS_DISTINTAS PalabrasDistintas,
                IDIOMA_ID IdiomaId, 
                CONFIANZA 
            FROM 
                DOCUMENTOS 
            WHERE 
                HASH = @hash 
                AND PALABRAS_DISTINTAS = @palabrasDistintas";

            var result = await conexion.QueryAsync<Documento>(select, new { hash, palabrasDistintas });
            return result.FirstOrDefault();
        }

        internal async Task ActualizarPorcentajeEnLetras(Idioma idioma)
        {
            const string update = @"
            UPDATE IDIOMAS_LETRAS 
                SET PORCENTAJE = CAST(CANTIDAD AS REAL) / CAST((SELECT SUM(CASE IDIOMAS_LETRAS.TIPO WHEN 1 THEN LETRAS WHEN 2 THEN SIGNOS ELSE SIMBOLOS END) FROM IDIOMAS WHERE IDIOMAS.ID = IDIOMAS_LETRAS.IDIOMA_ID) AS REAL) * 100
            WHERE 
                IDIOMA_ID = @idiomaId";

            await conexion.ExecuteAsync(update, new { idiomaId = idioma.Id });
        }

        internal async Task ActualizarPorcentajeEnPalabras(Idioma idioma)
        {
            const string update = @"
            UPDATE IDIOMAS_PALABRAS
                SET PORCENTAJE = CAST(CANTIDAD AS REAL) / CAST((SELECT SUM(PALABRAS) FROM IDIOMAS WHERE IDIOMAS.ID = IDIOMAS_PALABRAS.IDIOMA_ID) AS REAL) * 100
            WHERE 
                IDIOMA_ID = @idiomaId";


            await conexion.ExecuteAsync(update, new { idiomaId = idioma.Id });
        }


        internal async Task Actualizar(Documento documento)
        {
            const string update = "UPDATE DOCUMENTOS SET LETRAS = @letras, PALABRAS = @palabras, SIGNOS = @signos, SIMBOLOS = @simbolos, HASH = @hash, IDIOMA_ID = @idiomaId, CONFIANZA = @confianza WHERE ID = @id";


            await conexion.ExecuteAsync(update, documento);
        }

        internal async Task Insertar(IEnumerable<DocumentoPalabra> palabras)
        {
            const string update = "INSERT INTO DOCUMENTOS_PALABRAS (PALABRA, DOCUMENTO_ID, CANTIDAD, PORCENTAJE) VALUES (@palabra, @documentoId, @cantidad, @porcentaje)";


            await conexion.ExecuteAsync(update, palabras);
        }

        internal async Task Insertar(DocumentoPalabra palabra)
        {
            const string update = "INSERT INTO DOCUMENTOS_PALABRAS (PALABRA, DOCUMENTO_ID, CANTIDAD, PORCENTAJE) VALUES (@palabra, @documentoId, @cantidad, @porcentaje)";


            await conexion.ExecuteAsync(update, palabra);
        }

        internal async Task ActualizarTotales(Idioma idioma)
        {
            const string update = @"
            UPDATE IDIOMAS SET 
                LETRAS = IFNULL((SELECT SUM(CANTIDAD) FROM IDIOMAS_LETRAS WHERE IDIOMA_ID = @idiomaId AND TIPO = 1), 0),
                LETRAS_DISTINTAS = IFNULL((SELECT COUNT(*) FROM IDIOMAS_LETRAS WHERE IDIOMA_ID = @idiomaId AND TIPO = 1), 0),
                SIGNOS = IFNULL((SELECT SUM(CANTIDAD) FROM IDIOMAS_LETRAS WHERE IDIOMA_ID = @idiomaId AND TIPO = 2), 0),
                SIGNOS_DISTINTOS = IFNULL((SELECT COUNT(*) FROM IDIOMAS_LETRAS WHERE IDIOMA_ID = @idiomaId AND TIPO = 2), 0),
                SIMBOLOS = IFNULL((SELECT SUM(CANTIDAD) FROM IDIOMAS_LETRAS WHERE IDIOMA_ID = @idiomaId AND TIPO = 3), 0),
                SIMBOLOS_DISTINTOS = IFNULL((SELECT COUNT(*) FROM IDIOMAS_LETRAS WHERE IDIOMA_ID = @idiomaId AND TIPO = 3), 0),
                PALABRAS = IFNULL((SELECT SUM(CANTIDAD) FROM IDIOMAS_PALABRAS WHERE IDIOMA_ID = @idiomaId), 0),
                PALABRAS_DISTINTAS = IFNULL((SELECT COUNT(*) FROM IDIOMAS_PALABRAS WHERE IDIOMA_ID = @idiomaId), 0)";


            await conexion.ExecuteAsync(update, new { idiomaId = idioma.Id });
        }

        internal async Task ActualizarLetrasExistentes(Documento documento, Idioma idioma)
        {
            const string insertSelect = @"
            UPDATE IDIOMAS_LETRAS
                SET CANTIDAD = CANTIDAD + IFNULL((SELECT SUM(CANTIDAD) FROM DOCUMENTOS_LETRAS WHERE DOCUMENTO_ID = @documentoId AND DOCUMENTOS_LETRAS.LETRA_ID = IDIOMAS_LETRAS.LETRA_ID), 0)
            WHERE 
                IDIOMAS_LETRAS.IDIOMA_ID = @idiomaId
                AND LETRA_ID IN (SELECT LETRA_ID FROM DOCUMENTOS_LETRAS WHERE DOCUMENTO_ID = @documentoId)
            ";

            await conexion.ExecuteAsync(insertSelect, new { documentoId = documento.Id, idiomaId = idioma.Id });
        }

        internal async Task ActualizarPalabrasExistentes(Documento documento, Idioma idioma)
        {
            const string insertSelect = @"
            UPDATE IDIOMAS_PALABRAS
                SET CANTIDAD = CANTIDAD + IFNULL((
                    SELECT 
                        SUM(CANTIDAD) 
                    FROM 
                        DOCUMENTOS_PALABRAS 
                    WHERE 
                        DOCUMENTOS_PALABRAS.PALABRA = IDIOMAS_PALABRAS.PALABRA 
                        AND DOCUMENTOS_PALABRAS.DOCUMENTO_ID = @documentoId), 0)
            WHERE 
                IDIOMA_ID = @idiomaId
            ";


            await conexion.ExecuteAsync(insertSelect, new { documentoId = documento.Id, idiomaId = idioma.Id });
        }

        public async Task EliminarLetras(Documento documento)
        {
            const string delete = @"DELETE FROM DOCUMENTOS_LETRAS WHERE DOCUMENTO_ID = @documentoId";
            await conexion.ExecuteAsync(delete, new { documentoId = documento.Id });
        }

        public void Dispose()
        {
            conexion.Dispose();
        }
    }
}
