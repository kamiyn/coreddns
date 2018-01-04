using System;
using System.ComponentModel.DataAnnotations;

namespace Coreddns.Core.Entities.DdnsDb
{
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
        /// IPv4 アドレス
        /// </summary>
        [MaxLength(16)]
        public string ipv4 { get; set; }

        /// <summary>
        /// IPv6 アドレス
        /// </summary>
        [MaxLength(40)]
        public string ipv6 { get; set; }

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

        /// <summary>
        /// 更新日時
        /// </summary>
        public DateTimeOffset updatetimev4 { get; set; }

        /// <summary>
        /// 更新日時
        /// </summary>
        public DateTimeOffset updatetimev6 { get; set; }
    }
}
