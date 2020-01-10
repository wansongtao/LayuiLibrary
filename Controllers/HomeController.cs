using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Library.Models;
using Oracle.ManagedDataAccess.Client;
using Dapper;
using Newtonsoft.Json;

namespace Library.Controllers
{
    public class HomeController : Controller
    {
        #region 读取数据库连接字符串
        /// <summary>
        /// Oracle数据库连接字符串
        /// </summary>
        private readonly string OracleConnectionString = Startup.MyConnectionString;
        #endregion

        #region 定义返回信息
        /// <summary>
        /// 0失败，1成功, 100查询数据成功（赋值给表格）
        /// </summary>
        public int state;

        /// <summary>
        /// 提示文本
        /// </summary>
        public string msg;

        /// <summary>
        /// 返回json格式字符串 
        /// </summary>
        public string json;
        #endregion


        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Login()
        {
            return View();
        }


        public IActionResult Literature()
        {
            return View();
        }

        public IActionResult Popular_science()
        {
            return View();
        }

        public IActionResult Addbook()
        {
            return View();
        }

        public IActionResult AlterBook()
        {
            return View();
        }

        //登录
        public string BookLogin(string userAcc, string passwd)
        {
            try
            {
                using(OracleConnection conn = new OracleConnection(OracleConnectionString))
                {
                    string sql = "select ID from BookUser where UserName = :userAcc and UserPwd = :passwd";

                    int Loginlist = conn.Query(sql, new { userAcc, passwd }).Count();

                    if(Loginlist == 1)
                    {
                        state = 1;
                        msg = "登录成功";
                    }
                    else
                    {
                        state = 0;
                        msg = "用户名或密码错误";
                    }
                    json = "{\"state\": " + state + ", \"msg\": \"" + msg + "\"}";
                }
                return json;
            }
            catch(Exception ex)
            {
                state = 0;
                msg = ex.Message;
                json = "{\"state\": " + state + ", \"msg\": \"" + msg + "\"}";
                return json;
            }
        }

        //表格分页显示
        public string Bookview(int page, int limit, string BookType)
        {
            try
            {
                using(OracleConnection conn = new OracleConnection(OracleConnectionString))
                {
                    string sql = "select BookId from Book where Isdelete = :Isdelete and BookType = :BookType";

                    int records = conn.Query(sql, new { Isdelete = 0, BookType }).Count();

                    if(records > 0)
                    {
                        string sqlone = "select BookId, ISBN, BookName, BookType, Author, Price, Publishing " +
                            " from book where Isdelete = :Isdelete and BookType = :BookType and rownum <= :limit";

                        if(page == 1)
                        {
                            var booklist = conn.Query<Book>(sqlone, new { Isdelete = 0, BookType, limit }).ToList();

                            if(booklist.Count > 0)
                            {
                                state = 100;
                                msg = "查找成功";
                                json = JsonConvert.SerializeObject(booklist);
                            }
                            else
                            {
                                state = 0;
                                msg = "数据获取失败";
                                json = "[]";
                            }
                        }
                        else if(page > 1)
                        {
                            int PageLimit = (page - 1) * limit;
                            sqlone = sqlone +  " and BookId not in (select BookId from Book " +
                                " where Isdelete = :Isdelete and BookType = :BookType and rownum <= :PageLimit)";

                            var booklist = conn.Query<Book>(sqlone, new { Isdelete = 0, BookType, limit, PageLimit }).ToList();

                            if (booklist.Count > 0)
                            {
                                state = 100;
                                msg = "查找成功";
                                json = JsonConvert.SerializeObject(booklist);
                            }
                            else
                            {
                                state = 0;
                                msg = "数据获取失败";
                                json = "[]";
                            }
                        }
                        else
                        {
                            state = 0;
                            msg = "页码获取错误";
                            json = "[]";
                        }
                    }
                    else
                    {
                        state = 0;
                        msg = "未查找到数据";
                        json = "[]";
                    }

                    json = "{\"state\": " + state + ", \"msg\":\"" + msg + "\", \"count\": "+ records +", \"data\": "+ json +"}";
                }

                return json;
            }
            catch(Exception ex)
            {
                state = 0;
                msg = ex.Message;
                json = "{\"state\": " + state + ", \"msg\":\"" + msg + "\", \"count\": 0, \"data\": []}";
                return json;
            }
        }

        //删除多条数据
        public string BookDelete(int[] numberId)
        {
            try
            {
                using(OracleConnection conn = new OracleConnection(OracleConnectionString))
                {
                    string ID = null, sql = null;
                    for(int i = 0; i < numberId.Length; i++)
                    {
                        if(i == 0)
                        {
                            ID = numberId[0].ToString();
                        }
                        else if(i > 0)
                        {
                            ID += "," + numberId[i];
                        }
                    }

                    sql = "update Book set Isdelete = :Isdelete where BookId in (" + ID + ")";
                    int delbook = conn.Execute(sql, new { Isdelete = 1 });

                    if(delbook > 0)
                    {
                        state = 1;
                        msg = "删除成功";
                    }
                    else
                    {
                        state = 0;
                        msg = "删除失败";
                    }

                    json = "{\"state\": " + state + ", \"msg\": \"" + msg + "\", \"count\": " + numberId.Length + "}";
                }
                return json;
            }
            catch(Exception ex)
            {
                state = 0;
                msg = ex.Message;
                json = "{\"state\": " + state + ", \"msg\": \"" + msg + "\", \"count\": 0, \"data\": []}";
                return json;
            }
        }

        //添加一条数据
        public string BookAddOne(string ISBN, string BookName, string BookType
            , string Author, decimal Price, string Publishing)
        {
            try
            {
                using(OracleConnection conn = new OracleConnection(OracleConnectionString))
                {
                    string seachsql = "select BookId from Book where ISBN = :ISBN";
                    int seachlist = conn.Query(seachsql, new { ISBN }).Count();

                    if(seachlist > 0)
                    {
                        state = 0;
                        msg = "ISBN码重复";
                    }
                    else
                    {
                        string sql = "insert into Book (BookId, ISBN, BookName, BookType, Author, Price, Publishing, AddDate) " +
                        " Values(BOOKID_SEQUENCE.Nextval, :ISBN, :BookName, :BookType, :Author, :Price, :Publishing, :AddDate)";

                        int addbook = conn.Execute(sql, new
                        {
                            ISBN,
                            BookName,
                            BookType,
                            Author,
                            Price,
                            Publishing,
                            AddDate = DateTime.Now
                        });

                        if(addbook > 0)
                        {
                            state = 1;
                            msg = "添加成功";
                        }
                        else
                        {
                            state = 0;
                            msg = "添加失败";
                        }
                    }

                    json = "{\"state\": " + state + ", \"msg\": \"" + msg + "\"}";
                }
                return json;
            }
            catch(Exception ex)
            {
                state = 0;
                msg = ex.Message;
                json = "{\"state\": " + state + ", \"msg\": \"" + msg + "\"}";
                return json;
            }
        }


        //删除一条数据
        public string DelBookOne(int ID)
        {
            try
            {
                using(OracleConnection conn = new OracleConnection(OracleConnectionString))
                {
                    string sql = "update Book set Isdelete = :Isdelete where BookId = :ID";

                    int delbook = conn.Execute(sql, new { ID, Isdelete = 1 });

                    if(delbook > 0)
                    {
                        state = 1;
                        msg = "删除成功";
                    }
                    else
                    {
                        state = 0;
                        msg = "删除失败";
                    }
                }
                json = "{\"state\": " + state + ", \"msg\": \"" + msg + "\"}";
                return json;
            }
            catch(Exception ex)
            {
                state = 0;
                msg = ex.Message;
                json = "{\"state\": " + state + ", \"msg\": \"" + msg + "\"}";
                return json;
            }
        }

        //根据ID查询一条数据和图书类别
        public string BookOne(int ID)
        {
            try
            {
                using(OracleConnection conn = new OracleConnection(OracleConnectionString))
                {
                    string sql = "select ISBN, BookName, BookType, Author, Price, Publishing from Book " +
                        " where BookId = :ID";
                    string type = "[]";

                    var bookone = conn.Query<Book>(sql, new { ID }).ToList();

                    if (bookone.Count > 0)
                    {
                        string sqltype = "select BookTypeName from BookType where Isdelete = :Isdelete";

                        var booktwo = conn.Query<BookType>(sqltype, new { Isdelete = 0 }).Select(it => it.BookTypeName).ToList();

                        if(booktwo.Count > 0)
                        {
                            state = 100;
                            msg = "查询成功";
                            json = JsonConvert.SerializeObject(bookone);
                            type = JsonConvert.SerializeObject(booktwo);
                        }
                        else
                        {
                            state = 0;
                            msg = "获取图书类别失败";
                            json = "[]";
                        }
                    }
                    else
                    {
                        state = 0;
                        msg = "获取数据失败";
                        json = "[]";
                    }
                    json = "{\"state\": " + state + ", \"msg\": \"" + msg + "\", \"data\": "+ json +", \"type\": "+ type +"}";
                }
                return json;
            }
            catch(Exception ex)
            {
                state = 0;
                msg = ex.Message;
                json = "{\"state\": " + state + ", \"msg\": \"" + msg + "\", \"data\": []}";
                return json;
            }
        }

        //修改
        public string AlterBooklist(int ID, string ISBN, string BookName, string BookType,
            string Author, decimal Price, string Publishing)
        {
            try
            {
                using(OracleConnection conn = new OracleConnection(OracleConnectionString))
                {
                    string sql = "update Book set ISBN = :ISBN, BookName = :BookName, BookType = :BookType," +
                        " Author = :Author, Price = :Price, Publishing = :Publishing where BookId = :ID";

                    int alterbook = conn.Execute(sql, new { ID, ISBN, BookName, BookType, Author, Price, Publishing });

                    if(alterbook > 0)
                    {
                        state = 1;
                        msg = "修改成功";
                    }
                    else
                    {
                        state = 0;
                        msg = "修改失败";
                    }
                }

                json = "{\"state\": " + state + ", \"msg\": \"" + msg + "\"}";
                return json;
            }
            catch(Exception ex)
            {
                state = 0;
                msg = ex.Message;
                json = "{\"state\": " + state + ", \"msg\": \"" + msg + "\"}";
                return json;
            }
        }

        //获取图书类别
        public string SelectBooktype()
        {
            try
            {
                using(OracleConnection conn = new OracleConnection(OracleConnectionString))
                {
                    string sql = "select BookTypeName, BookTypeUrl from BookType where Isdelete = :Isdelete";

                    var booktype = conn.Query(sql, new { Isdelete = 0 }).ToList();

                    if(booktype.Count > 0)
                    {
                        json = JsonConvert.SerializeObject(booktype);
                        state = 1;
                        msg = "操作成功";
                    }
                    else
                    {
                        state = 0;
                        msg = "获取图书类别列表失败";
                    }
                }
                json = "{\"state\": " + state + ", \"msg\": \"" + msg + "\", \"data\": " + json + "}";
                return json;
            }
            catch(Exception ex)
            {
                state = 0;
                msg = ex.Message;
                json = "{\"state\": " + state + ", \"msg\": \"" + msg + "\"}";
                return json;
            }
        }

    }
}
