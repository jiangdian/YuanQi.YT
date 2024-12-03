using System;
using FreeSql.DataAnnotations;

namespace YuanQi.YT.InventoryWorker
{
    /// <summary>
    /// 分拣破拆结果记录
    /// </summary>
	public class ProcessingResult
    {
        [Column(IsIdentity = true, IsPrimary = true)] //自增主键
        public int Id { get; set; }
        #region 分选
        /// <summary>
        /// 分选时间--分选专机
        /// </summary>
        public DateTime SortingTime { get; set; }

        /// <summary>
        /// 物资 处理方式
        /// </summary>
        [Column(MapType = typeof(int))]
        public int HandlingMethod { get; set; }

        /// <summary>
        /// 处理之前结果照片
        /// </summary>
        public string? SortingImagePath { get; set; }
        #endregion

        #region 分拣装箱
        /// <summary>
        /// 分拣时间--分类装箱时间
        /// </summary>
        public DateTime? ClassificationAndPacking { get; set; }

        /// <summary>
        /// 装箱， 箱码--绑定物资的位置
        /// </summary>
        public string? PackingCode { get; set; }
        #endregion

        #region 破拆
        /// <summary>
        /// 破拆时间
        /// </summary>
        public DateTime? DemolitionTime { get; set; }

        /// <summary>
        /// 破拆位置
        /// </summary>
        public string? DemolitionPosition { get; set; }

        /// <summary>
        /// 破拆状态
        /// </summary>
        public int? DemolitionState { get; set; }

        /// <summary>
        /// 破拆之后结果照片
        /// </summary>
        public string? DemolitionImagePath { get; set; }
        #endregion

        #region 物资

        /// <summary>
        /// 物资 外键
        /// </summary>
        public int AssetArchiveId { get; set; }


        /// <summary>
        /// 物资信息来源
        /// </summary>
        [Column(MapType = typeof(int))]
        public int? AssetSource { get; set; }

        #endregion

        /// <summary>
        /// 线体id
        /// </summary>
        public int? LineBodyID { get; set; }
        /// <summary>
        /// 完成结果
        /// </summary>
        public bool IsFinish { get; set; } = false;
        /// <summary>
        /// 笼子编号
        /// </summary>
        public string? CageNo { get; set; }
        /// <summary>
        /// 工单名称
        /// </summary>
        public string? TaskName { get; set; }
        /// <summary>
        ///是否加入工单
        /// </summary>
        public bool HaveTask { get; set; } = false;
        /// <summary>
        /// 归属于哪个库位
        /// </summary>
        public string? StorageName { get; set; }
        /// <summary>
        /// 电表在笼子中的位置
        /// </summary>
        public string? CagePosition { get; set; }
        public string MeterType { get; set; }
        [Column(IsIgnore = true)]
        public string MeterModel { get; set; }
        [Column(IsIgnore = true)]
        public string MeterBarCode { get; set; }

    }
}
