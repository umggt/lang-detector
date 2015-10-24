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
        public static async Task<long> Insertar(Documento documento)
        {
            const string insert = @"INSERT INTO DOCUMENTOS (LETRAS) VALUES (@letras)";
            const string selectId = @"SELECT last_insert_rowid() FROM DOCUMENTOS";

            using (var conexion = await BaseDeDatos.AbrirConexionAsync())
            {
                await conexion.ExecuteAsync(insert, documento);

                var id = await conexion.QueryAsync<long>(selectId);
                return id.FirstOrDefault();
            }
        }

        public static async Task Insertar(IEnumerable<Letra> letra)
        {
            const string insert = @"INSERT INTO DOCUMENTOS_LETRAS (DOCUMENTO_ID, LETRA_ID, CANTIDAD, PORCENTAJE) VALUES (@documentoId, @id, @cantidad, @porcentaje)";
            using (var conexion = await BaseDeDatos.AbrirConexionAsync())
            {
                await conexion.ExecuteAsync(insert, letra);
            }
        }
    }
}
