namespace Elin.Plugin.Main.Models
{
    /// <summary>
    /// <see cref="ParticleSystem"/> フレーム毎クリア処理請負人。
    /// </summary>
    public struct ParticleCleaner
    {
        public ParticleCleaner()
        {
            IsCleaned = false;
        }

        #region property

        /// <summary>
        /// クリア済みか。
        /// </summary>
        private bool IsCleaned { get; set; }

        #endregion

        #region function

        /// <summary>
        /// クリアされていなければクリアする。
        /// </summary>
        /// <param name="particleSystem"></param>
        public void ClearIfNotCleaned(ParticleSystem particleSystem)
        {
            if (IsCleaned)
            {
                return;
            }

            particleSystem.Clear();
            IsCleaned = true;
        }

        #endregion
    }
}
