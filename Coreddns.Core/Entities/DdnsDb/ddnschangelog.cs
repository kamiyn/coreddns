using System;
using System.ComponentModel.DataAnnotations;

namespace Coreddns.Core.Entities.DdnsDb
{
    public class ddnschangelog
    {
        [Key]
        public long id { get; set; }

        /// <summary>
        /// ホスト名
        /// </summary>
        [MaxLength(64)]
        public string name { get; set; }

        /// <summary>
        /// IPv6 アドレス
        /// </summary>
        [MaxLength(40)]
        public string ip { get; set; }

        /// <summary>
        /// アドレスファミリー番号 IPv4 = 2, IPv6 = 23
        /// </summary>
        public int addrfamily { get; set; }

        /// <summary>
        /// 初期生成日時
        /// </summary>
        public DateTimeOffset createtime { get; set; }
    }
}
