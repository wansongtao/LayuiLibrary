using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Models
{
    public class BookType
    {
        public int ID { get; set; }

        /// <summary>
        /// 图书类别名
        /// </summary>
        public string BookTypeName { get; set; }

        /// <summary>
        /// 路径
        /// </summary>
        public string BooktypeUrl { get; set; }

        /// <summary>
        /// 添加人ID
        /// </summary>
        public int AddId { get; set; }

        /// <summary>
        /// 添加时间
        /// </summary>
        public DateTime AddDate { get; set; }

        /// <summary>
        /// 修改人ID
        /// </summary>
        public int UpdateId { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime UpdateDate { get; set; }

        /// <summary>
        /// 删除人ID
        /// </summary>
        public int DeleteId { get; set; }

        /// <summary>
        /// 删除时间
        /// </summary>
        public DateTime DeleteDate { get; set; }

        /// <summary>
        /// 0未删除, 1已删除
        /// </summary>
        public int Isdelete { get; set; }
    }
}
