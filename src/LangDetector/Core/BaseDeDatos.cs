using LangDetector.Tools;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;

namespace LangDetector.Core
{
    class BaseDeDatos
    {
        private const string dbName = "datos.db";
        private const string connectionString = "Data Source=" + dbName;

        public static void CrearSiNoExiste()
        {
            if (!File.Exists(dbName))
            {
                using (var conexion = AbrirConexion())
                {
                    var comando = conexion.CreateCommand();
                    comando.CommandText = Resources.ObtenerTexto("LangDetector.Scripts.201510211259-Initial-Create.sql");
                    comando.ExecuteNonQuery();
                }
            }
        }

        public static IDbConnection AbrirConexion()
        {
            var conexion = new SQLiteConnection(connectionString);
            conexion.Open();
            return conexion;
        }

        public static async Task<IDbConnection> AbrirConexionAsync()
        {
            var conexion = new SQLiteConnection(connectionString);
            await conexion.OpenAsync();
            return conexion;
        }
    }
}
