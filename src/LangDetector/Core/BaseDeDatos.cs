﻿using LangDetector.Tools;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;

namespace LangDetector.Core
{
    /// <summary>
    /// Clase en la que se agregan las funcionalidades para administrar la base de datos.
    /// </summary>
    class BaseDeDatos
    {
        private const string dbName = "datos.db";
        private const string connectionString = "Data Source=" + dbName;

        /// <summary>
        /// Crea la base de datos si no existe aún en la misma carpeta que la aplicación.
        /// </summary>
        public static void CrearSiNoExiste()
        {
            if (!File.Exists(dbName))
            {
                // Si no existe el archivo de base de datos, se crea
                // ejecutando el script de base de datos ubicado en la carpeta Scripts\Creacion-de-base-de-datos.sql
                using (var conexion = AbrirConexion())
                {
                    var comando = conexion.CreateCommand();
                    comando.CommandText = Resources.ObtenerTexto("LangDetector.Scripts.Creacion-de-base-de-datos.sql");
                    comando.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Método que abre una conexión hacia la base de datos.
        /// </summary>
        /// <returns>Retorna el objeto de conexión que se puede usar para ejecutar consultas hacia la base de datos.</returns>
        public static IDbConnection AbrirConexion()
        {
            var conexion = new SQLiteConnection(connectionString);
            conexion.Open();
            return conexion;
        }

        /// <summary>
        /// Método que abre una conexión hacia la base de datos de forma asincrona (mas eficiente que el método AbrirConexion para procesos asincronos).
        /// </summary>
        /// <returns>Retorna el objeto de conexión que se puede usar para ejecutar consultas hacia la base de datos.</returns>
        public static async Task<IDbConnection> AbrirConexionAsync()
        {
            var conexion = new SQLiteConnection(connectionString);
            await conexion.OpenAsync();
            return conexion;
        }
    }
}
