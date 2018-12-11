﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ControleEstoque.Web.Models
{
    public class GrupoProdutoModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Preencha o nome.")]

        public string Nome { get; set; }
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
                    comando.CommandText = "select count(*) from grupo_produto";
                    ret = (int)comando.ExecuteScalar();

                }
            }
            return ret;
        }


        public static List<GrupoProdutoModel> RecuperarLista(int pagina, int tamPagina, string filtro = "")
        {
            var ret = new List<GrupoProdutoModel>();
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

                    //este comando (offset) nao esta funcionando no sql 2008, somente no 2012 em diante
                    //comando.CommandText = string.Format(
                    //    "select * from grupo_produto order by nome offset {0} rows fetch next {1} rows only",
                    //    pos>0?pos-1:0, tamPagina);

                    //gambiarra de um aluna
                    comando.CommandText = string.Format(
                        "WITH Linha AS  (SELECT *, ROW_NUMBER() OVER(ORDER BY nome) AS LinhaNumero FROM grupo_produto) " +
                        "SELECT * FROM Linha WHERE LinhaNumero BETWEEN {0} AND {1} " + filtroWhere ,
                        pos > 0 ? pos + 1 : 0, pos > 0 ? (tamPagina * pagina) : tamPagina);


                    var reader = comando.ExecuteReader();
                    while (reader.Read())
                    {
                        ret.Add(new GrupoProdutoModel
                        {
                            Id = (int)reader["id"],
                            Nome = (string)reader["nome"],
                            Ativo = (bool)reader["ativo"]
                        });
                    }

                }
            }
            return ret;
        }

        public static GrupoProdutoModel RecuperarPeloId(int id)
        {
            GrupoProdutoModel ret = null;
            using (var conexao = new SqlConnection())
            {
                conexao.ConnectionString = ConfigurationManager.ConnectionStrings["principal"].ConnectionString;
                conexao.Open();
                using (var comando = new SqlCommand())
                {
                    comando.Connection = conexao;
                    comando.CommandText = "select * from grupo_produto where (id = @id)";
                    comando.Parameters.Add("@id", SqlDbType.VarChar).Value = id;

                    var reader = comando.ExecuteReader();
                    if (reader.Read())
                    {
                        ret = new GrupoProdutoModel
                        {
                            Id = (int)reader["id"],
                            Nome = (string)reader["nome"],
                            Ativo = (bool)reader["ativo"]
                        };
                    }

                }
            }
            return ret;
        }

        //internal static object RecuperarLista(object pagina, int v)
        //{
        //    throw new NotImplementedException();
        //}

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
                        comando.CommandText = "delete from grupo_produto where(id = @id)";
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
                        comando.CommandText = "insert into grupo_produto (nome, ativo) values (@nome,@ativo); select convert(int, scope_identity())";
                        comando.Parameters.Add("@nome", SqlDbType.VarChar).Value = this.Nome;
                        comando.Parameters.Add("@ativo", SqlDbType.VarChar).Value = (this.Ativo ? 1 : 0);

                        ret = ((int)comando.ExecuteScalar());
                    }
                    else
                    {
                        comando.CommandText = "update grupo_produto set nome=@nome, ativo=@ativo where id = @id";
                        comando.Parameters.Add("@nome", SqlDbType.VarChar).Value = this.Nome;
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