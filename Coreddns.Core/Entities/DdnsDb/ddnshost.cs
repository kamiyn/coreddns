using System;
using System.ComponentModel.DataAnnotations;

namespace Coreddns.Core.Entities.DdnsDb
{
    public interface Iddnshost
    {
        string name { get; set; }
        string ipv4 { get; set; }
        string ipv6 { get; set; }
    }

    public class ddnshost
    {
        [Key]
        public long id { get; set; }

        /// <summary>
        /// ホスト名
        /// </summary>
        [MaxLength(64)]
        public string name { get; set; }

        /// <summary>
        /// ホストハッシュ
        /// </summary>
        [MaxLength(64)]
        public string hash { get; set; }

        /// <summary>
        /// レコードが有効かどうか
        /// </summary>
        public bool isvalid { get; set; }

        /// <summary>
        /// メモ
        /// </summary>
        [MaxLength(2048)]
        public string note { get; set; }

        /// <summary>
        /// 初期生成日時
        /// </summary>
        public DateTimeOffset createtime { get; set; }
    }
}
