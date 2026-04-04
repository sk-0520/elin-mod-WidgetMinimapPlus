namespace Elin.Plugin.Main.Models
{
    public class Marker
    {
        #region property

        /// <summary>
        /// パーティクルシステム
        /// </summary>
        /// <remarks>こいつの比較は ==/!= で行うこと。</remarks>
        public ParticleSystem? ParticleSystem { get; set; } = null;
        public MarkerShape MarkerShape { get; set; }

        #endregion

        #region function

        #endregion
    }
}
