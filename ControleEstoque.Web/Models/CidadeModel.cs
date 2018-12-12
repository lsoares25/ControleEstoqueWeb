using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ControleEstoque.Web.Models
{
    public class CidadeModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Preencha o nome.")]
        [MaxLength(30, ErrorMessage = "O nome da cidade pode ter no máximo 30 caracteres.")]
        public string Nome { get; set; }

        public int Id_estado { get; set; }

        public bool Ativo { get; set; }

        public static int RecuperarQuantidade()
        {
            var ret = 0;
            using (var conexao = new SqlConnection())
            {
                conexao.ConnectionString = ConfigurationManager.ConnectionStrings["principal"].ConnectionString;
                conexao.Open();
                using (var comando = new SqlCommand())
                {
                    comando.Connection = conexao;
                    comando.CommandText = "select count(*) from cidade";
                    ret = (int)comando.ExecuteScalar();

                }
            }
            return ret;
        }


        public static List<CidadeModel> RecuperarLista(int pagina, int tamPagina, string filtro = "")
        {
            var ret = new List<CidadeModel>();
            using (var conexao = new SqlConnection())
            {
                conexao.ConnectionString = ConfigurationManager.ConnectionStrings["principal"].ConnectionString;
                conexao.Open();
                using (var comando = new SqlCommand())
                {
                    var pos = (pagina - 1) * tamPagina;
                    var filtroWhere = "";
                    if (!string.IsNullOrEmpty(filtro))
                    {
                        filtroWhere = string.IsNullOrEmpty(filtro) ? "" : string.Format(" and lower(nome) like '%{0}%'", filtro.ToLower());
                    }

                    comando.Connection = conexao;
                    comando.CommandText = string.Format(
                        "WITH Linha AS  (SELECT *, ROW_NUMBER() OVER(ORDER BY nome) AS LinhaNumero FROM cidade) " +
                        "SELECT * FROM Linha WHERE LinhaNumero BETWEEN {0} AND {1} " + filtroWhere,
                        pos > 0 ? pos + 1 : 0, pos > 0 ? (tamPagina * pagina) : tamPagina);


                    var reader = comando.ExecuteReader();
                    while (reader.Read())
                    {
                        ret.Add(new CidadeModel
                        {
                            Id = (int)reader["id"],
                            Nome = (string)reader["nome"],                            
                            Ativo = (bool)reader["ativo"],
                            Id_estado = (int)reader["id_estado"]
                        });
                    }

                }
            }
            return ret;
        }

        public static CidadeModel RecuperarPeloId(int id)
        {
            CidadeModel ret = null;
            using (var conexao = new SqlConnection())
            {
                conexao.ConnectionString = ConfigurationManager.ConnectionStrings["principal"].ConnectionString;
                conexao.Open();
                using (var comando = new SqlCommand())
                {
                    comando.Connection = conexao;
                    comando.CommandText = "select * from cidade where (id = @id)";
                    comando.Parameters.Add("@id", SqlDbType.VarChar).Value = id;

                    var reader = comando.ExecuteReader();
                    if (reader.Read())
                    {
                        ret = new CidadeModel
                        {
                            Id = (int)reader["id"],
                            Nome = (string)reader["nome"],                            
                            Ativo = (bool)reader["ativo"],
                            Id_estado = (int)reader["id_estado"]
                        };
                    }

                }
            }
            return ret;
        }


        public static bool ExcluirPeloId(int id)
        {
            var ret = false;

            if (RecuperarPeloId(id) != null)
            {
                using (var conexao = new SqlConnection())
                {
                    conexao.ConnectionString = ConfigurationManager.ConnectionStrings["principal"].ConnectionString;
                    conexao.Open();
                    using (var comando = new SqlCommand())
                    {
                        comando.Connection = conexao;
                        comando.CommandText = "delete from cidade where(id = @id)";
                        comando.Parameters.Add("@id", SqlDbType.VarChar).Value = id;

                        ret = (comando.ExecuteNonQuery() > 0);


                    }
                }
            }

            return ret;
        }



        public int Salvar()
        {
            var ret = 0;
            var model = RecuperarPeloId(this.Id);


            using (var conexao = new SqlConnection())
            {
                conexao.ConnectionString = ConfigurationManager.ConnectionStrings["principal"].ConnectionString;
                conexao.Open();
                using (var comando = new SqlCommand())
                {
                    comando.Connection = conexao;
                    if (model == null)
                    {
                        comando.CommandText = "insert into cidade (nome, ativo, id_estado) values (@nome,@ativo,@id_estado); select convert(int, scope_identity())";
                        comando.Parameters.Add("@nome", SqlDbType.VarChar).Value = this.Nome;
                        comando.Parameters.Add("@id_estado", SqlDbType.Int).Value = this.Id_estado;
                        comando.Parameters.Add("@ativo", SqlDbType.VarChar).Value = (this.Ativo ? 1 : 0);

                        ret = ((int)comando.ExecuteScalar());
                    }
                    else
                    {
                        comando.CommandText = "update cidade set nome=@nome, ativo=@ativo, id_estado=@id_estado where id = @id";
                        comando.Parameters.Add("@nome", SqlDbType.VarChar).Value = this.Nome;
                        comando.Parameters.Add("@id_estado", SqlDbType.Int).Value = this.Id_estado;
                        comando.Parameters.Add("@ativo", SqlDbType.VarChar).Value = (this.Ativo ? 1 : 0);
                        comando.Parameters.Add("@id", SqlDbType.VarChar).Value = this.Id;

                        if (comando.ExecuteNonQuery() > 0)
                        {
                            ret = this.Id;
                        }
                    }

                }
            }


            return ret;
        }
    }
}